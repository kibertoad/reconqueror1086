"""Bounded debugger transport smoke test; original data always stays local."""
import argparse
import datetime
import json
import os
from pathlib import Path
import subprocess
import time
from dosbox_control import DebuggerControl
import hashlib
import struct


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--emulator', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--original', action='store_true')
    args = parser.parse_args()
    root = args.output.resolve()
    root.mkdir(parents=True, exist_ok=True)
    if args.original:
        # // needs: GAME_DIR
        source = Path(os.environ['GAME_DIR']).resolve()
        drive = Path('analysis/documentation-audit/runtime').resolve()
        if not (drive / 'CONQUER.INI').is_file() or not (drive / 'C1086.GOB').is_file():
            raise RuntimeError('Verified writable runtime drive is unavailable')
        commands = f'mount c "{drive}"\nimgmount d "{source / "game.ins"}" -t iso\nc:\ndebugbox D:\\CONQUER.EXE'
        executable = Path('analysis/original/disc-root/CONQUER.EXE').read_bytes()
        if hashlib.sha256(executable).hexdigest() != '5d7231758766204ad061e6b82cf2f0e0cbe28899b35d095f13e4aad75c8b79d6':
            raise RuntimeError('Wrong BLD-GOG-EN executable identity')
        entry_ip, entry_cs = struct.unpack_from('<HH', executable, 0x14)
        image_offset = struct.unpack_from('<H', executable, 8)[0] * 16
        entry_bytes = executable[image_offset + entry_cs * 16 + entry_ip:
                                 image_offset + entry_cs * 16 + entry_ip + 16]
        relocation_count = struct.unpack_from('<H', executable, 6)[0]
        relocation_table = struct.unpack_from('<H', executable, 0x18)[0]
    else:
        # Independently authored code: set AX and loop. This is not game code.
        (root / 'PROBE.COM').write_bytes(bytes.fromhex('b83412ebfe'))
        commands = f'mount c "{root}"\nc:\ndebugbox PROBE.COM'
    lock = Path(os.environ.get('REFURBISHED_DINOSAURS_RUN_LOCK',
                               'C:/ProgramData/refurbished-dinosaurs/run.lock'))
    # Exclusive create; a held lock aborts immediately, without waiting.
    handle = lock.open('x')
    process = None
    result = {'original': args.original, 'started': datetime.datetime.now().astimezone().isoformat()}
    try:
        handle.write(json.dumps({'repository': str(Path.cwd()), 'session': 'debugger-capability-probe',
                                 'time': result['started'], 'pids': [os.getpid()]}))
        handle.flush()
        with DebuggerControl(timeout=15) as control:
            config = root / 'probe.conf'
            config.write_text(f'[sdl]\nfullscreen=false\noutput=surface\nautolock=false\n'
                              f'[dosbox]\nmachine=svga_s3\nmemsize=16\nmcp_server={control.port}\n'
                              f'[cpu]\ncore=normal\ncycles=fixed 10000\n'
                              f'[autoexec]\n{commands}\n')
            startup = subprocess.STARTUPINFO()
            startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW
            startup.wShowWindow = 0
            # A genuine hidden console is required by the debugger. Redirected
            # console handles or CREATE_NO_WINDOW cause debugger-entry failure.
            process = subprocess.Popen([str(args.emulator.resolve()), '-conf', str(config)],
                                       cwd=root, startupinfo=startup,
                                       creationflags=subprocess.CREATE_NEW_CONSOLE)
            handle.write('\n' + json.dumps({'pid': process.pid}))
            handle.flush()
            control.accept()
            result['ping'] = control.request('PING')
            (root / 'HELP.txt').write_text(control.execute('HELP'))
            deadline = time.monotonic() + 20
            # Wait on guest execution state, not a fixed startup sleep.
            while time.monotonic() < deadline:
                state = control.execute('EV CS IP')
                words = state.splitlines()[-1].split()
                if len(words) == 2 and all(all(c in '0123456789abcdefABCDEF' for c in w) for w in words):
                    segment, offset = [int(w, 16) for w in words]
                    if segment not in (0, 0xf000):
                        if not args.original and offset == 0x100:
                            break
                        if args.original and offset == entry_ip:
                            control.execute(f'MEMDUMPBIN {segment:04X}:{offset:04X} 10 ENTRY.BIN')
                            capture = root / 'ENTRY.BIN'
                            expected = bytearray(entry_bytes)
                            entry_start = entry_cs * 16 + entry_ip
                            load_segment = (segment - entry_cs) & 0xffff
                            for index in range(relocation_count):
                                rel_offset, rel_segment = struct.unpack_from('<HH', executable, relocation_table + index * 4)
                                position = rel_segment * 16 + rel_offset - entry_start
                                if 0 <= position <= len(expected) - 2:
                                    value = struct.unpack_from('<H', expected, position)[0]
                                    struct.pack_into('<H', expected, position, (value + load_segment) & 0xffff)
                            if capture.is_file() and capture.read_bytes() == expected:
                                result['stub_entry_verified'] = True
                                break
            else:
                result['last_state'] = state
                result['failure_cpu'] = control.execute('CPU')
                control.request('BREAK')
                result['text_capture'] = control.execute('MEMDUMPBIN B800:0000 FA0 TEXTMODE.BIN')
                raise RuntimeError('Did not observe guest program entry')
            result['entry'] = state
            result['break'] = control.request('BREAK')
            result['cpu'] = control.execute('CPU')
            result['registers'] = control.execute('EV AX CS IP')
            result['selector'] = control.execute('SELINFO CS')
            if args.original:
                result['run'] = control.execute('RUN')
                deadline = time.monotonic() + 30
                while time.monotonic() < deadline:
                    cr0 = control.execute('EV CR0').splitlines()[-1].strip()
                    if int(cr0, 16) & 1:
                        control.request('BREAK')
                        if int(control.execute('EV CR0').splitlines()[-1].strip(), 16) & 1:
                            result['protected_mode'] = True
                            break
                        control.execute('RUN')
                else:
                    raise RuntimeError('No verified protected-mode stop')
                result['protected_cpu'] = control.execute('CPU')
                regs = control.execute('EV CS DS EIP')
                result['protected_registers'] = regs
                cs, ds, ip = [int(w, 16) for w in regs.splitlines()[-1].split()]
                result['code_selector'] = control.execute(f'SELINFO {cs:04X}')
                result['data_selector'] = control.execute(f'SELINFO {ds:04X}')
                result['code_dump'] = control.execute(f'MEMDUMPBIN {cs:04X}:{ip:08X} 10 PROTECTED.BIN')
                result['protected_memory_read'] = (root / 'PROTECTED.BIN').stat().st_size == 16
                if not result['protected_memory_read']:
                    raise RuntimeError('Protected-mode read did not return expected length')
            if not args.original:
                result['write'] = control.execute(f'SM {segment:04X}:0200 78 56 34 12')
                result['dump'] = control.execute(f'MEMDUMPBIN {segment:04X}:0200 0004 ROUNDTRIP.BIN')
                dump = root / 'ROUNDTRIP.BIN'
                if not dump.is_file() or dump.read_bytes() != bytes.fromhex('78563412'):
                    raise RuntimeError('Guest memory write/read comparison failed')
                result['memory_roundtrip'] = True
                result['breakpoint'] = control.execute(f'BP {segment:04X}:0103')
                result['run'] = control.execute('RUN')
                deadline = time.monotonic() + 10
                while time.monotonic() < deadline:
                    registers = control.execute('EV AX IP')
                    if registers.splitlines()[-1].split() == ['1234', '103']:
                        result['breakpoint_hit'] = True
                        break
                else:
                    raise RuntimeError('Synthetic breakpoint did not reach expected state')
            result['passed'] = True
            print(json.dumps({key: value for key, value in result.items() if key not in ('selector',)}))
    finally:
        if process is not None and process.poll() is None:
            process.terminate()
            process.wait(timeout=10)
        handle.close()
        lock.unlink()
        (root / 'result.json').write_text(json.dumps(result, indent=2))


if __name__ == '__main__':
    main()
