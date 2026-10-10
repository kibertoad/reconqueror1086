"""Downstream session-adapter controls, entirely synthetic."""
import tempfile
from pathlib import Path
from types import SimpleNamespace
import unittest
from unittest.mock import Mock, patch

from shared_dosbox_runtime import AgentRuntime, ObservationTimeout, SessionError, _GameClient


class AdapterTests(unittest.TestCase):
    def runtime(self):
        runtime = AgentRuntime.__new__(AgentRuntime)
        runtime.owned = Mock()
        runtime.session = SimpleNamespace(id='owned', state='stopped')
        return runtime

    def client(self):
        shared = Mock()
        shared.capabilities = {'debugger': True}
        shared.status.return_value = SimpleNamespace(state='stopped')
        shared.ids.next.side_effect = ['local.1', 'local.2']
        return _GameClient(shared), shared

    def test_pending_observation_keeps_same_operation(self):
        runtime = self.runtime()
        operation = SimpleNamespace(id='pending')
        stopped = SimpleNamespace(id='owned', state='stopped')
        runtime.owned.observe.side_effect = [SimpleNamespace(pending=True),
                                           SimpleNamespace(pending=False, session=stopped)]
        self.assertIs(runtime.wait_until_stopped(operation), stopped)
        for call in runtime.owned.observe.call_args_list:
            self.assertIs(call.args[0], operation)
        runtime.owned.continue_.assert_not_called()

    def test_transport_error_propagates(self):
        runtime = self.runtime()
        runtime.owned.observe.side_effect = TimeoutError('transport')
        with self.assertRaisesRegex(TimeoutError, 'transport'):
            runtime.wait_until_stopped(SimpleNamespace(id='pending'))
        self.assertEqual(runtime.owned.observe.call_count, 1)

    def test_cleanup_delegates_and_retains_failure(self):
        runtime = self.runtime()
        runtime.owned.close.side_effect = SessionError('retained lock')
        with self.assertRaisesRegex(SessionError, 'retained lock'):
            runtime.close()
        runtime.owned.close.assert_called_once()

    def test_local_cpu_diagnostic_is_exact_and_stopped(self):
        client, shared = self.client()
        client.execute_command('owned', 'CPU')
        shared.raw.execute_command.assert_called_once_with('owned', 'CPU', request_id='local.1')
        with self.assertRaises(SessionError):
            client.execute_command('owned', 'SET EAX=0')
        shared.status.return_value.state = 'running'
        with self.assertRaises(SessionError):
            client.execute_command('owned', 'CPU')
        self.assertEqual(shared.raw.execute_command.call_count, 1)

    def test_guarded_write_delegates_contract_and_retains_failure(self):
        runtime = self.runtime()
        contract = object()
        runtime.owned.write.side_effect = SessionError('failed write')
        with self.assertRaisesRegex(SessionError, 'failed write'):
            runtime.write(contract, 'press', b'ab', expected_sha256='a' * 64)
        runtime.owned.write.assert_called_once_with(contract, 'press', b'ab',
                                                   expected_sha256='a' * 64)
        client, shared = self.client()
        with self.assertRaises(AttributeError):
            client.write_memory
        shared.raw.write_memory.assert_not_called()

    def test_private_drive_and_cpu_profiles_are_session_settings(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            prepared = root / 'prepared'; prepared.mkdir()
            prepared.joinpath('synthetic.txt').write_text('fabricated')
            with patch('shared_dosbox_runtime.import_client', return_value=(Mock(), Mock())), \
                 patch('shared_dosbox_runtime.DosboxSession') as session:
                AgentRuntime(root, root / 'emulator', root / 'output', prepared, 'TEST.COM',
                             root / 'disc.iso', cpu_profile='gog')
                settings = session.call_args.args[0]
                drive = root / 'new-drive'; drive.mkdir()
                settings.prepare_drive(drive)
                drive.joinpath('synthetic.txt').write_text('changed')
                self.assertEqual(prepared.joinpath('synthetic.txt').read_text(), 'fabricated')
                self.assertEqual(settings.run_directory, root / 'output' / 'session')
                self.assertEqual(settings.emulator_config.sections['cpu'],
                                 {'core': 'normal', 'cycles': 'auto 30%'})
                self.assertFalse(settings.emulator_config.keep_host_sound)
                self.assertEqual(settings.emulator_config.media[0].kind, 'iso')


if __name__ == '__main__':
    unittest.main()
