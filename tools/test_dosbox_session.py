"""Synthetic lifecycle checks; no DOSBox build or original files required."""
from pathlib import Path
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import Mock, patch
from dosbox_session import AgentRuntime, ObservationTimeout, SessionError


def runtime():
    value = AgentRuntime.__new__(AgentRuntime)
    value.session = SimpleNamespace(id='session', state='stopped')
    value.process = Mock()
    value.process.poll.return_value = None
    value.agent = Mock()
    value.lock_handle = None
    return value


class LifecycleTests(unittest.TestCase):
    def test_repeated_observation_expiry_keeps_same_operation(self):
        value = runtime()
        operation = SimpleNamespace(id='operation')
        stopped = SimpleNamespace(state='stopped')
        value.wait = Mock(side_effect=[ObservationTimeout(), ObservationTimeout(), stopped])
        self.assertIs(stopped, value.wait_until_stopped(operation))
        self.assertEqual(value.wait.call_count, 3)
        for call in value.wait.call_args_list:
            self.assertIs(call.args[0], operation)
        value.agent.continue_.assert_not_called()
        value.agent.start.assert_not_called()

    def test_transport_timeout_is_not_observation_expiry(self):
        value = runtime()
        value.wait = Mock(side_effect=TimeoutError('transport failed'))
        with self.assertRaisesRegex(TimeoutError, 'transport failed'):
            value.wait_until_stopped(SimpleNamespace(id='operation'))
        value.wait.assert_called_once()

    def test_observation_timeout_preserves_pending_operation(self):
        value = runtime()
        value.agent.wait.return_value = SimpleNamespace(running=True)
        with patch('dosbox_session.time.monotonic', side_effect=[0, 0, 2]):
            with self.assertRaises(TimeoutError):
                value.wait(SimpleNamespace(id='operation'), seconds=1)
        value.agent.wait.assert_called_once_with('session', 'operation', timeout_ms=100)
        value.agent.continue_.assert_not_called()
        value.agent.start.assert_not_called()

    def test_terminal_observation_updates_same_session(self):
        value = runtime()
        terminal = SimpleNamespace(id='session', state='exited')
        value.agent.wait.return_value = SimpleNamespace(running=False, session=terminal)
        self.assertIs(terminal, value.wait(SimpleNamespace(id='operation')))

    def test_transport_close_failure_still_cleans_owned_process(self):
        with tempfile.TemporaryDirectory() as directory:
            value = runtime()
            value.output = Path(directory)
            value.agent.status.return_value = SimpleNamespace(state='exited')
            value.agent.close.side_effect = RuntimeError('transport failure')
            value.process.poll.side_effect = [None, None, 0]
            value.lock_path = value.output / 'lock'
            value.lock_handle = value.lock_path.open('x')
            value.close()
            value.process.terminate.assert_called_once()
            self.assertFalse(value.lock_path.exists())

    def test_failed_process_cleanup_retains_run_lock(self):
        with tempfile.TemporaryDirectory() as directory:
            value = runtime()
            value.output = Path(directory)
            value.agent.status.return_value = SimpleNamespace(state='exited')
            value.process.terminate.side_effect = RuntimeError('cannot terminate')
            value.lock_path = value.output / 'lock'
            value.lock_handle = value.lock_path.open('x')
            try:
                with self.assertRaises(SessionError):
                    value.close()
                self.assertTrue(value.lock_path.exists())
                self.assertFalse(value.lock_handle.closed)
            finally:
                value.lock_handle.close()


if __name__ == '__main__':
    unittest.main()
