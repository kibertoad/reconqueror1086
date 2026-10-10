"""Conqueror adapter for the released owned DOSBox-X session package.

Game mapping, input contracts and recording semantics stay in their callers.
The read-only CPU descriptor diagnostic remains a bounded local extension.
"""
import json
from pathlib import Path
import shutil
import sys

from dinorefurb_dosbox_session import (
    DosboxSession, EmulatorConfig, Media, PINNED_REVISION, SessionError,
    SessionSettings, Target, require, verify_checkout,
)

SOURCE_REVISION = PINNED_REVISION


class ObservationTimeout(TimeoutError):
    """The owned operation remains pending; this is not a transport failure."""


def import_client(source):
    checkout = verify_checkout(Path(source))
    sys.path.insert(0, str(checkout.path / 'client' / 'python'))
    from dosbox_agent import AgentClient, MemoryAddress
    return AgentClient, MemoryAddress


class _GameClient:
    """Shared guarded calls, plus explicitly bounded local extensions."""

    def __init__(self, client):
        self.client = client

    def __getattr__(self, name):
        if name.startswith('_') or name in ('raw', 'ids', 'write_memory'):
            raise AttributeError(name)
        return getattr(self.client, name)

    def _stopped(self, session_id):
        require(self.client.capabilities, 'debugger')
        if self.client.status(session_id).state != 'stopped':
            raise SessionError('Local diagnostic and supported writes require a stopped guest')

    def execute_command(self, session_id, command):
        # CPU is the read-only descriptor diagnostic consumed by live_mapping.
        if command != 'CPU':
            raise SessionError('Only the read-only CPU diagnostic is supported')
        self._stopped(session_id)
        return self.client.raw.execute_command(session_id, command, request_id=self.client.ids.next())


class AgentRuntime:
    def __init__(self, source, emulator, output, workdir, target, disc=None,
                 cycles=10000, sound_investigation=False, cpu_profile='fixed', prepare_drive=None,
                 cpu_core='normal', event_log=None):
        if type(cycles) is not int or not 1000 <= cycles <= 100000:
            raise ValueError('Diagnostic CPU cycles must be between 1000 and 100000')
        if cpu_profile not in ('fixed', 'gog'):
            raise ValueError('Unknown diagnostic CPU profile')
        if cpu_core not in ('normal', 'auto'):
            raise ValueError('Unknown diagnostic CPU core')
        self.source, self.emulator = Path(source).resolve(), Path(emulator).resolve()
        self.output, self.workdir = Path(output).resolve(), Path(workdir).resolve()
        self.AgentClient, self.MemoryAddress = import_client(self.source)
        self.session = self.agent = None
        # The package owns a new drive; the probe's prepared source is private too.
        def prepare(drive):
            shutil.copytree(self.workdir, drive, dirs_exist_ok=True)
        sections = {'sdl': {'fullscreen': 'false', 'output': 'surface', 'autolock': 'false'},
                    'dosbox': {'machine': 'svga_s3', 'memsize': '16'},
                    'cpu': {'core': cpu_core,
                            'cycles': 'auto 30%' if cpu_profile == 'gog' else f'fixed {cycles}'}}
        config = EmulatorConfig(
            media=(Media('D', Path(disc).resolve(), 'iso'),) if disc else (),
            sections=sections, keep_host_sound=sound_investigation)
        self.owned = DosboxSession(SessionSettings(
            checkout=self.source, emulator=self.emulator, run_directory=self.output / 'session',
            target=Target(target), client_factory=lambda endpoint: self.AgentClient.from_config(endpoint.agent_config),
            prepare_drive=prepare_drive or prepare, emulator_config=config, event_log=event_log))

    def __enter__(self):
        self.owned.start()
        try:
            self.session = self.owned.state
            self.agent = _GameClient(self.owned.client)
            self.capabilities = self.owned.capabilities
            self.output.joinpath('capabilities.json').write_text(json.dumps(self.capabilities, indent=2))
            self.output.joinpath('session-identity.json').write_text(json.dumps({
                'session_id': self.owned.session_id,
                'owned_emulator_pid': self.owned.emulator_process.pid,
                'source_revision': SOURCE_REVISION,
                'session_record': 'session/session.json',
                'emulator_sha256': self.owned.emulator_sha256,
            }, indent=2))
            return self
        except BaseException:
            self.close()
            raise

    def wait(self, operation, seconds=10):
        observed = self.owned.observe(operation, timeout=seconds)
        if observed.pending:
            raise ObservationTimeout(f'Operation {operation.id} remains pending')
        self.session = observed.session
        return self.session

    def wait_until_stopped(self, operation, seconds=30):
        while True:
            try:
                return self.wait(operation, seconds=seconds)
            except ObservationTimeout:
                print(f'Owned debugger operation {operation.id} still pending; observing same operation', flush=True)

    def registers(self):
        return self.agent.get_registers(self.session.id)

    def read(self, address, length):
        return self.agent.read_memory(self.session.id, address, length).data

    def write(self, contract, field, data, *, expected_sha256):
        return self.owned.write(contract, field, data, expected_sha256=expected_sha256)

    def close(self):
        self.owned.close()

    def __exit__(self, *_):
        self.close()
