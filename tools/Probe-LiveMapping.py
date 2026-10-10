"""Bounded live mapping survey. Original-derived observations remain local."""
import argparse
import dataclasses
import hashlib
import json
import os
import re
from pathlib import Path
import shutil
from dosbox_session import AgentRuntime, ObservationTimeout
from emu.le_image import load_image
from live_mapping import validate_snapshot
from native_rng_recorder import record


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', required=True, type=Path)
    parser.add_argument('--samples', type=int, default=8)
    parser.add_argument('--observation-ms', type=int, default=100)
    parser.add_argument('--rng-break', action='store_true')
    parser.add_argument('--cycles', type=int, default=10000)
    parser.add_argument('--debugger-build', choices=('heavy', 'no-heavy'), default='heavy')
    parser.add_argument('--trace-loader', action='store_true')
    parser.add_argument('--seed-check', action='store_true')
    parser.add_argument('--draw-check', action='store_true')
    parser.add_argument('--record-startup-shifts', action='store_true')
    parser.add_argument('--record-native', action='store_true')
    parser.add_argument('--stop-at-screen', action='store_true')
    parser.add_argument('--stop-after-screen', action='store_true')
    parser.add_argument('--continue-after-screen', action='store_true',
                        help='Verify the first loaded screen, then continue recording RNG operations')
    parser.add_argument('--title-click', action='store_true',
                        help='Queue a supported-state click after the guarded initial title load')
    parser.add_argument('--screen-checkpoints', action='store_true',
                        help='Verify initial and replacement screen returns while recording')
    parser.add_argument('--new-game-click', action='store_true',
                        help='Queue one supported-state new-game click after verified game-options loading')
    parser.add_argument('--generation-click', action='store_true',
                        help='Queue one supported-state youth-generation click after verified character options')
    parser.add_argument('--youth-answer', action='store_true',
                        help='Queue the first youth answer and verify its callback return')
    parser.add_argument('--stop-after-screen-id', type=int,
                        help='Stop after a verified loaded screen identifier, requiring screen checkpoints')
    parser.add_argument('--startup-checkpoints', action='store_true',
                        help='Read and continue through verified preparation boundaries before screen loading')
    parser.add_argument('--startup-click', action='store_true',
                        help='Queue one supported-state short click at the verified preparation wait')
    parser.add_argument('--record-draw-limit', type=int, default=30)
    parser.add_argument('--animations-off', action='store_true',
                        help='Set the supported ANIMATIONS switch OFF in the private INI')
    parser.add_argument('--sound-investigation', action='store_true',
                        help='Enable host audio only for a probe investigating sound')
    args = parser.parse_args()
    if (args.stop_at_screen or args.stop_after_screen) and not args.record_native:
        raise ValueError('Screen diagnostic boundary requires native recording')
    if args.stop_at_screen and args.stop_after_screen:
        raise ValueError('Choose one screen diagnostic boundary')
    if args.startup_checkpoints and not args.stop_after_screen:
        raise ValueError('Startup checkpoints require --stop-after-screen')
    if args.startup_click and not args.startup_checkpoints:
        raise ValueError('Startup click requires --startup-checkpoints')
    if args.continue_after_screen and not args.stop_after_screen:
        raise ValueError('Continuation requires --stop-after-screen')
    if args.title_click and not args.continue_after_screen:
        raise ValueError('Title input requires --continue-after-screen')
    if args.screen_checkpoints and not args.continue_after_screen:
        raise ValueError('Screen checkpoints require --continue-after-screen')
    if args.new_game_click and not args.screen_checkpoints:
        raise ValueError('New-game input requires --screen-checkpoints')
    if args.generation_click and not args.new_game_click:
        raise ValueError('Generation input requires --new-game-click')
    if args.youth_answer and (not args.generation_click or args.stop_after_screen_id == 3):
        raise ValueError('Youth answer requires --generation-click and continuation beyond screen three')
    if args.stop_after_screen_id is not None and (not args.screen_checkpoints or
            not 0 <= args.stop_after_screen_id <= 24):
        raise ValueError('Screen target requires --screen-checkpoints and a registered identifier')
    if args.trace_loader and args.debugger_build != 'heavy':
        raise ValueError('CPU tracing requires the heavy debugger build')
    if args.draw_check and not args.seed_check:
        raise ValueError('Draw identity check requires the seed identity check')
    if args.seed_check and not args.rng_break:
        raise ValueError('Seed identity check requires RNG entry breakpoints')
    if args.record_startup_shifts and not args.seed_check:
        raise ValueError('Startup recording requires the verified initial seed check')
    if args.record_native and (not args.rng_break or args.record_startup_shifts or args.draw_check):
        raise ValueError('Native recording needs RNG breakpoints and its own recording mode')
    if not 1 <= args.record_draw_limit <= 100000:
        raise ValueError('Recording draw limit exceeds the bounded diagnostic')
    if not 1 <= args.samples <= 100:
        raise ValueError('Samples must be between 1 and 100')
    if not 1 <= args.observation_ms <= 1000:
        raise ValueError('Observation interval must be between 1 and 1000 ms')
    # // needs: GAME_DIR
    installation = Path(os.environ['GAME_DIR']).resolve()
    root = args.output.resolve()
    drive = root / 'drive'
    if drive.exists():
        raise RuntimeError('Use a fresh isolated drive for each survey')
    executable = Path('analysis/original/disc-root/CONQUER.EXE')
    if hashlib.sha256(executable.read_bytes()).hexdigest() != '5d7231758766204ad061e6b82cf2f0e0cbe28899b35d095f13e4aad75c8b79d6':
        raise RuntimeError('Wrong BLD-GOG-EN source identity')
    drive.mkdir(parents=True)
    tool_names = ('Probe-LiveMapping.py', 'dosbox_session.py', 'live_mapping.py',
                  'native_rng_recorder.py', 'rng_journal.py', 'rng_recording.py',
                  'supported_pointer_input.py', 'emu/le_image.py')
    (root / 'probe-source.json').write_text(json.dumps({
        'tool_sha256': {name: hashlib.sha256((Path(__file__).parent / name).read_bytes()).hexdigest()
                        for name in tool_names}
    }, indent=2))
    for name in ('C1086.GOB', 'CONQUER.INI'):
        shutil.copyfile(installation / name, drive / name)
    shutil.copyfile(executable, drive / 'CONQUER.EXE')
    (drive / 'SAVEGAME').mkdir()
    # FMT-CONFIG-001: supported switches; modify only the private copy.
    ini = (drive / 'CONQUER.INI').read_text()
    ini = ini.replace('MOVIE=ON', 'MOVIE=OFF').replace('CREDITS=ON', 'CREDITS=OFF')
    if args.animations_off:
        # FMT-CONFIG-001: explicit controlled configuration, not a runtime patch.
        lines = ini.splitlines()
        if sum(line.split('=', 1)[0].strip() == 'ANIMATIONS' for line in lines) != 1:
            raise RuntimeError('Expected exactly one private ANIMATIONS setting')
        ini = '\n'.join('ANIMATIONS=OFF' if line.split('=', 1)[0].strip() == 'ANIMATIONS'
                        else line for line in lines) + '\n'
    (drive / 'CONQUER.INI').write_text(ini)
    (root / 'probe-configuration.json').write_text(json.dumps({
        'movie': 'OFF', 'credits': 'OFF', 'animations_off': args.animations_off,
        'sound_investigation': args.sound_investigation,
        'startup_checkpoints': args.startup_checkpoints, 'startup_click': args.startup_click,
        'continue_after_screen': args.continue_after_screen, 'title_click': args.title_click,
        'screen_checkpoints': args.screen_checkpoints, 'stop_after_screen_id': args.stop_after_screen_id,
        'new_game_click': args.new_game_click, 'generation_click': args.generation_click,
        'youth_answer': args.youth_answer
    }, indent=2))
    source = Path('artifacts/runtime-tools/dosbox-x-agent-source')
    configuration = 'Agent Debug SDL2' if args.debugger_build == 'heavy' else 'Agent Debug No Heavy SDL2'
    emulator = source / 'bin/x64' / configuration / 'dosbox-x.exe'
    (root / 'debugger-build.json').write_text(json.dumps({
        'configuration': configuration, 'sha256': hashlib.sha256(emulator.read_bytes()).hexdigest()
    }, indent=2))
    records = []
    images, _ = load_image(executable)
    probe_offset = 0x6b3f1 - images[0]['base']  # FND-RNG-003
    signature = bytes(images[0]['image'][probe_offset:probe_offset + 34])
    breakpoints_installed = False
    native_recording_started = False
    try:
        with AgentRuntime(source, emulator, root, drive, 'CONQUER.EXE', installation / 'game.ins',
                          args.cycles, sound_investigation=args.sound_investigation) as runtime:
            print('startup', runtime.registers())
            for attempt in range(args.samples):
                operation = runtime.agent.continue_(runtime.session.id)
                if breakpoints_installed and args.record_native:
                    runtime.wait_until_stopped(operation)
                    observed = runtime.agent.wait(runtime.session.id, operation.id, timeout_ms=100)
                elif breakpoints_installed:
                    try:
                        runtime.wait(operation, seconds=30)
                        observed = runtime.agent.wait(runtime.session.id, operation.id, timeout_ms=100)
                    except ObservationTimeout:
                        # The same continuation remains live until the explicit
                        # pause below; allow uninterrupted loader progress.
                        observed = runtime.agent.wait(runtime.session.id, operation.id, timeout_ms=100)
                else:
                    observed = runtime.agent.wait(runtime.session.id, operation.id, timeout_ms=args.observation_ms)
                if observed.running:
                    # Observation timeout leaves that operation live. Pause it
                    # explicitly to obtain a coherent snapshot, never restart it.
                    runtime.wait(runtime.agent.pause(runtime.session.id))
                else:
                    runtime.session = observed.session
                if runtime.session.state != 'stopped':
                    print('terminal', runtime.session)
                    break
                registers = runtime.registers()
                diagnostic = runtime.agent.execute_command(runtime.session.id, 'CPU')
                print('sample', attempt, registers.cpu_mode, registers.segments['cs'],
                      registers.instruction_pointer, diagnostic.raw_output[:400])
                records.append({'registers': dataclasses.asdict(registers), 'cpu': diagnostic.raw_output})
                records[-1]['stop_reason'] = dataclasses.asdict(runtime.session.stop_reason)
                if breakpoints_installed and runtime.session.stop_reason.kind == 'breakpoint':
                    snapshot = b''.join(runtime.read(runtime.MemoryAddress.physical(base), 65536)
                                        for base in range(0, 16 * 1024 * 1024, 65536))
                    live_map = validate_snapshot(snapshot, images, diagnostic.raw_output)
                    (root / f'entry-memory-{attempt}.bin').write_bytes(snapshot)
                    records[-1]['validated_map'] = dataclasses.asdict(live_map)
                    expected_code, expected_offset = live_map.code_address(
                        0x6b413 if runtime.session.stop_reason.breakpoint_id == ids[1] else
                        0x6b3f1 if runtime.session.stop_reason.breakpoint_id == ids[0] else
                        0x636d0 if runtime.session.stop_reason.breakpoint_id == ids[2] else 0x64ff5)
                    if (int(registers.segments['cs'], 16), int(registers.instruction_pointer, 16)) != (expected_code, expected_offset):
                        raise RuntimeError('Native breakpoint and validated code map disagree')
                    if any(int(registers.segments[name], 16) != live_map.data.selector for name in ('ds', 'ss')):
                        raise RuntimeError('Native data/stack selectors differ from the verified mapping')
                    stack = runtime.read(runtime.MemoryAddress.segmented(
                        int(registers.segments['ss'], 16), int(registers.general['esp'], 16)), 64)
                    (root / 'breakpoint-stack.bin').write_bytes(stack)
                    print('instrumented entry breakpoint hit', runtime.session.stop_reason)
                    if args.record_native:
                        native_recording_started = True
                        journal = record(runtime, live_map, ids, root, args.record_draw_limit,
                                 args.stop_at_screen, args.stop_after_screen, args.startup_checkpoints,
                                 args.startup_click, args.continue_after_screen, args.title_click,
                                 args.screen_checkpoints, args.stop_after_screen_id, args.new_game_click,
                                 args.generation_click, args.youth_answer)
                        print('native recording', journal['status'], 'events', len(journal['events']))
                        break
                    if args.record_startup_shifts and runtime.session.stop_reason.breakpoint_id != ids[1]:
                        raise RuntimeError('Startup recording did not reach its initial seed first')
                    if args.seed_check and runtime.session.stop_reason.breakpoint_id == ids[1]:
                        # FND-RNG-003: the pointer-return function carries the
                        # relocated state address, independently of an assumed
                        # delta between the two loaded objects.
                        pointer = int.from_bytes(runtime.read(runtime.MemoryAddress.segmented(
                            int(registers.segments['cs'], 16), code_base + 0x6b3ec - images[0]['base']
                            - code_segment['base']), 4), 'little')
                        state_address = runtime.MemoryAddress.segmented(int(registers.segments['ds'], 16), pointer)
                        initial = int.from_bytes(runtime.read(state_address, 4), 'little')
                        return_address, seed = (int.from_bytes(stack[i:i + 4], 'little') for i in (0, 4))
                        if args.record_startup_shifts and return_address != live_map.code_address(0x24c32)[1]:
                            raise RuntimeError('Startup recording reached an unowned seed caller')
                        completed = False
                        for step_index in range(16):
                            runtime.session, after = runtime.agent.step(runtime.session.id)
                            if int(after.instruction_pointer, 16) == return_address:
                                completed = True
                                break
                        if not completed:
                            raise RuntimeError('Seed routine did not return within its instruction bound')
                        final = int.from_bytes(runtime.read(state_address, 4), 'little')
                        if final != seed:
                            raise RuntimeError('Native seed/state identity check failed')
                        records[-1]['seed_identity_check'] = {'before': initial, 'seed': seed,
                            'after': final, 'steps': step_index + 1,
                            'state_pointer': hex(pointer), 'return_address': hex(return_address),
                            'return_registers': dataclasses.asdict(after)}
                        print('native seed write verified', 'steps', step_index + 1)
                        if args.record_startup_shifts:
                            from rng_recording import record_startup_shifts
                            journal = record_startup_shifts(runtime, live_map, ids, seed, root)
                            print('startup shift recording', journal['case_status'], 'draws', len(journal['events']) - 1)
                            break
                        if args.draw_check:
                            continue
                    elif args.draw_check and runtime.session.stop_reason.breakpoint_id == ids[0]:
                        pointer = int.from_bytes(runtime.read(runtime.MemoryAddress.segmented(
                            int(registers.segments['cs'], 16), code_base + 0x6b3ec - images[0]['base']
                            - code_segment['base']), 4), 'little')
                        state_address = runtime.MemoryAddress.segmented(int(registers.segments['ds'], 16), pointer)
                        initial = int.from_bytes(runtime.read(state_address, 4), 'little')
                        return_address = int.from_bytes(stack[:4], 'little')
                        completed = False
                        for step_index in range(32):
                            runtime.session, after = runtime.agent.step(runtime.session.id)
                            if int(after.instruction_pointer, 16) == return_address:
                                completed = True
                                break
                        if not completed:
                            raise RuntimeError('Draw routine did not return within its instruction bound')
                        final = int.from_bytes(runtime.read(state_address, 4), 'little')
                        expected = (initial * 0x41c64e6d + 0x3039) & 0xffffffff
                        result = int(after.general['eax'], 16)
                        if final != expected or result != (expected >> 16) & 0x7fff:
                            raise RuntimeError('Native draw state/result identity check failed')
                        records[-1]['draw_identity_check'] = {'before': initial, 'after': final,
                            'result': result, 'steps': step_index + 1, 'state_pointer': hex(pointer),
                            'return_address': hex(return_address), 'return_registers': dataclasses.asdict(after)}
                        print('native draw state/result verified', 'steps', step_index + 1)
                    break
                # Preserve diagnostics locally even if the loader returns to
                # real mode; no guest text or bytes enter committed reports.
                # Linear access goes through the VGA memory handler. Raw
                # physical backing reads do not represent video device RAM.
                text_memory = runtime.read(runtime.MemoryAddress.linear(0xb8000), 4000)
                (root / f'text-video-{attempt}.bin').write_bytes(text_memory)
                if registers.cpu_mode == 'real' and int(registers.segments['cs'], 16) == 0:
                    (root / f'zero-state-{attempt}.bin').write_bytes(
                        runtime.read(runtime.MemoryAddress.physical(0), 65536))
                    print('invalid startup execution location; diagnostic captured')
                if registers.cpu_mode == 'protected':
                    memory = runtime.read(runtime.MemoryAddress.physical(0), 16 * 4096)
                    (root / f'low-memory-{attempt}.bin').write_bytes(memory)
                    match = re.search(r'GDT\s+base=([0-9a-fA-F]+)\s+limit=([0-9a-fA-F]+)', diagnostic.raw_output)
                    if not match:
                        raise RuntimeError('Protected-mode diagnostic omitted GDT bounds')
                    base, limit = (int(value, 16) for value in match.groups())
                    if limit > 65535:
                        raise RuntimeError('Descriptor table exceeds architectural bound')
                    table = runtime.read(runtime.MemoryAddress.physical(base), limit + 1)
                    (root / f'gdt-{attempt}.bin').write_bytes(table)
                    descriptors = []
                    for offset in range(0, len(table) - 7, 8):
                        row = table[offset:offset + 8]
                        if not row[5] & 128:
                            continue
                        descriptor_base = int.from_bytes(row[2:4], 'little') | row[4] << 16 | row[7] << 24
                        descriptor_limit = int.from_bytes(row[:2], 'little') | (row[6] & 15) << 16
                        if row[6] & 128:
                            descriptor_limit = (descriptor_limit << 12) | 4095
                        descriptors.append({'selector': offset, 'base': descriptor_base,
                                            'limit': descriptor_limit, 'access': row[5],
                                            'big': bool(row[6] & 64)})
                    records[-1]['descriptors'] = descriptors
                    print('large descriptors', [row for row in descriptors if row['big'] and row['limit'] > 65535])
                    if args.rng_break and not breakpoints_installed:
                        search = b''.join(runtime.read(runtime.MemoryAddress.physical(base), 65536)
                                          for base in range(0, 4 * 1024 * 1024, 65536))
                        position = search.find(signature)
                        if position >= 0:
                            if search.find(signature, position + 1) >= 0:
                                raise RuntimeError('Ambiguous RNG identity control')
                            code_base = position - probe_offset
                            code_segments = [row for row in descriptors if row['big'] and row['access'] & 8
                                             and row['base'] <= position <= row['base'] + row['limit']]
                            if len(code_segments) != 1:
                                raise RuntimeError('RNG candidate has ambiguous code selectors')
                            code_segment = code_segments[0]
                            ids = []
                            # FND-RNG-003 and FND-CONFIG-001: stop on RNG
                            # entries, fatal diagnostics or process exit.
                            for address in (0x6b3f1, 0x6b413, 0x636d0, 0x64ff5):
                                bp = runtime.agent.create_execution_breakpoint(runtime.session.id,
                                    code_segment['selector'], code_base + address - images[0]['base'] - code_segment['base'])
                                ids.append(bp.id)
                            records[-1]['candidate_rng_breakpoints'] = ids
                            breakpoints_installed = True
                            print('candidate RNG/fatal/exit breakpoints installed')
                if attempt == args.samples - 1 or (registers.cpu_mode == 'real' and int(registers.segments['cs'], 16) == 0):
                    # A bounded physical snapshot allows an independent local
                    # image search, including objects not selected by CS yet.
                    with (root / 'physical-memory.bin').open('wb') as snapshot:
                        for base in range(0, 16 * 1024 * 1024, 65536):
                            snapshot.write(runtime.read(runtime.MemoryAddress.physical(base), 65536))
                    if args.trace_loader:
                        runtime.agent.start_trace(runtime.session.id, 'normal', 256)
                        runtime.wait(runtime.agent.continue_(runtime.session.id), seconds=10)
                        trace = runtime.agent.read_trace(runtime.session.id, None, 256)
                        (root / 'startup-trace.json').write_text(json.dumps(dataclasses.asdict(trace), indent=2))
                        print('bounded startup trace events', len(trace.events), 'active', trace.active)
                if registers.cpu_mode == 'real' and int(registers.segments['cs'], 16) == 0:
                    break
            if args.record_native and not native_recording_started:
                raise RuntimeError('Native recording entry was not reached within the mapping survey')
    finally:
        (root / 'mapping-observations.json').write_text(json.dumps(records, indent=2))


if __name__ == '__main__':
    main()
