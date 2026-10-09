"""Owned-process lifecycle for the pinned structured DOSBox-X debugger.

The upstream Python client supplies protocol/models. This wrapper adds the
restoration machine lock, isolated paths, bounded waits and guaranteed cleanup.
"""
import datetime
import json
import os
from pathlib import Path
import subprocess
import sys
import time
import uuid

SOURCE_REVISION = 'b6abbd5980a885f5f310a4088c59a8688d1b116c'


class SessionError(RuntimeError):
    pass


class ObservationTimeout(TimeoutError):
    """The owned operation is still pending, rather than failed."""


def import_client(source):
    source = Path(source).resolve()
    revision = subprocess.check_output(['git', '-C', str(source), 'rev-parse', 'HEAD'], text=True).strip()
    if revision != SOURCE_REVISION:
        raise SessionError('DOSBox-X source/client revision does not match the pinned build')
    sys.path.insert(0, str(source / 'client' / 'python'))
    from dosbox_agent import AgentClient, MemoryAddress
    return AgentClient, MemoryAddress


class AgentRuntime:
    def __init__(self, source, emulator, output, workdir, target, disc=None, cycles=10000, sound_investigation=False):
        self.source = Path(source).resolve()
        self.emulator = Path(emulator).resolve()
        self.output = Path(output).resolve()
        self.workdir = Path(workdir).resolve()
        self.target = target
        if not isinstance(cycles, int) or not 1000 <= cycles <= 100000:
            raise ValueError('Diagnostic CPU cycles must be between 1000 and 100000')
        self.cycles = cycles
        self.sound_investigation = sound_investigation
        self.disc = Path(disc).resolve() if disc else None
        self.process = self.agent = self.session = self.lock_handle = None
        self.lock_path = Path(os.environ.get('REFURBISHED_DINOSAURS_RUN_LOCK',
                                            'C:/ProgramData/refurbished-dinosaurs/run.lock'))
        self.AgentClient, self.MemoryAddress = import_client(self.source)

    def __enter__(self):
        self.output.mkdir(parents=True, exist_ok=True)
        self.lock_handle = self.lock_path.open('x')
        self.lock_handle.write(json.dumps({'repository': str(Path.cwd()), 'session': 'live-rng-agent',
                                          'time': datetime.datetime.now().astimezone().isoformat(),
                                          'pids': [os.getpid()]}))
        self.lock_handle.flush()
        try:
            config = self.output / 'agent.env'
            endpoint = '\\\\.\\pipe\\reconqueror-' + uuid.uuid4().hex
            config.write_text(f'transport=named_pipe\nendpoint={endpoint}\n'
                              f'dosbox_executable={self.emulator}\ndosbox_workdir={self.workdir}\n'
                              'profile=production\nrequest_timeout_ms=15000\n'
                              'max_message_bytes=1048576\nmax_memory_read_bytes=65536\n'
                              'max_trace_events=100000\n')
            dosbox_config = self.output / 'dosbox.conf'
            ready = self.workdir / 'AGENT.RDY'
            if ready.exists():
                raise SessionError('Private drive already has a startup readiness marker')
            autoexec = f'mount c "{self.workdir}"\n'
            if not self.sound_investigation:
                autoexec = 'mixer master 0:0 /noshow\n' + autoexec
            if self.disc:
                autoexec += f'imgmount d "{self.disc}" -t iso\n'
            autoexec += 'echo RECONQUEROR-READY > C:\\AGENT.RDY\n'
            dosbox_config.write_text('[sdl]\nfullscreen=false\noutput=surface\nautolock=false\n'
                                     '[dosbox]\nmachine=svga_s3\nmemsize=16\n'
                                     f'[cpu]\ncore=normal\ncycles=fixed {self.cycles}\n'
                                     f'[midi]\nmididevice={"default" if self.sound_investigation else "none"}\n'
                                     '[autoexec]\n' + autoexec)
            startup = subprocess.STARTUPINFO()
            startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW
            startup.wShowWindow = 0
            self.process = subprocess.Popen([str(self.emulator), '-conf', str(dosbox_config),
                                             '--agent-config', str(config)], cwd=self.output,
                                            startupinfo=startup, creationflags=subprocess.CREATE_NEW_CONSOLE)
            self.lock_handle.write('\n' + json.dumps({'pid': self.process.pid}))
            self.lock_handle.flush()
            self.agent = self.AgentClient.from_config(config)
            # The RPC server can accept requests before AUTOEXEC has finished.
            # Wait on its guest-written marker, not elapsed startup time, before
            # launching a target that needs the mounted disc.
            deadline = time.monotonic() + 20
            while True:
                if self.process.poll() is not None:
                    raise SessionError('Emulator exited before drive setup completed')
                try:
                    initialized = ready.read_text().strip() == 'RECONQUEROR-READY'
                except (FileNotFoundError, PermissionError):
                    initialized = False
                if initialized:
                    break
                if time.monotonic() >= deadline:
                    raise SessionError('Guest drive setup readiness marker was not observed')
                time.sleep(0.02)
            capabilities = self.agent.capabilities()
            if not capabilities.get('debugger'):
                raise SessionError('Structured debugger capability is unavailable')
            self.capabilities = capabilities
            (self.output / 'capabilities.json').write_text(json.dumps(capabilities, indent=2))
            self.session = self.agent.start(self.target, mount_path=self.workdir)
            if self.session.state != 'stopped' or self.session.stop_reason.kind != 'startup':
                raise SessionError('Target did not stop at its verified startup boundary')
            (self.output / 'session-identity.json').write_text(json.dumps({
                'session_id': self.session.id, 'owned_emulator_pid': self.process.pid,
                'source_revision': SOURCE_REVISION
            }, indent=2))
            return self
        except BaseException:
            self.close()
            raise

    def wait(self, operation, seconds=10):
        deadline = time.monotonic() + seconds
        while time.monotonic() < deadline:
            if self.process.poll() is not None:
                raise SessionError('Owned emulator exited while operation was pending')
            result = self.agent.wait(self.session.id, operation.id, timeout_ms=100)
            if not result.running:
                self.session = result.session
                return self.session
        # A timeout is not a terminal state and never starts another operation.
        raise ObservationTimeout(f'Operation {operation.id} remains pending')

    def wait_until_stopped(self, operation, seconds=30):
        """Observe one operation until terminal; each poll verifies process life.

        Transport failures propagate. Only our bounded observation expiry is
        retried, without creating another continuation or restarting the guest.
        """
        while True:
            try:
                return self.wait(operation, seconds=seconds)
            except ObservationTimeout:
                print(f'Owned debugger operation {operation.id} still pending; observing same operation', flush=True)

    def registers(self):
        return self.agent.get_registers(self.session.id)

    def read(self, address, length):
        return self.agent.read_memory(self.session.id, address, length).data

    def close(self):
        cleanup_errors = []
        if self.agent is not None:
            try:
                if self.session is not None and self.process.poll() is None:
                    state = self.agent.status(self.session.id)
                    if state.state not in ('exited', 'failed'):
                        self.wait(self.agent.stop(self.session.id), seconds=5)
            except Exception as error:
                cleanup_errors.append(str(error))
            finally:
                try:
                    self.agent.close()
                except Exception as error:
                    cleanup_errors.append(str(error))
                self.agent = None
        try:
            if self.process is not None and self.process.poll() is None:
                self.process.terminate()
                self.process.wait(timeout=10)
        except Exception as error:
            cleanup_errors.append(str(error))
        stopped = self.process is None or self.process.poll() is not None
        if self.lock_handle is not None and stopped:
            self.lock_handle.close()
            self.lock_path.unlink()
            self.lock_handle = None
        if cleanup_errors:
            (self.output / 'cleanup-diagnostic.txt').write_text('\n'.join(cleanup_errors))
        if not stopped:
            raise SessionError('Owned emulator remains live; retaining its run lock')

    def __exit__(self, *_):
        self.close()
