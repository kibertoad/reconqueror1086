"""Consumer delegation and failure controls for the shared event log."""
from types import SimpleNamespace
import unittest
from unittest.mock import Mock
from native_event_log import EventLog, PENDING_FIELDS, outcome


class ConsumerLogTests(unittest.TestCase):
    def report(self):
        return {'schema': 'conquer-native-rng-journal-v1', 'status': 'bounded-limit-reached',
                'end_rng_state': 1, 'full_game_complete': False, 'accepted_callers_complete': False,
                **dict.fromkeys(PENDING_FIELDS, False)}

    def test_completed_rule_event_and_outcome_delegate(self):
        session = Mock(run_failure=None)
        log = EventLog(SimpleNamespace(owned=session))
        log.append({'kind': 'seed', 'rule': 'RULE-RNG-001', 'seed': 1})
        session.log_event.assert_called_once_with('seed', {'rule': 'RULE-RNG-001', 'seed': 1})
        report = self.report()
        log.finish(report)
        session.finish_log.assert_called_once_with(outcome(report))
        session.fail_log.assert_not_called()

    def test_consumer_failure_is_recorded_without_finishing_success(self):
        session = Mock(run_failure=None)
        report = dict(self.report(), status='incomplete', failure='Synthetic frame mismatch')
        EventLog(SimpleNamespace(owned=session)).finish(report)
        session.fail_log.assert_called_once_with('Synthetic frame mismatch', outcome(report))
        session.finish_log.assert_not_called()

    def test_package_failure_is_not_overwritten(self):
        session = Mock(run_failure='Synthetic write refused')
        EventLog(SimpleNamespace(owned=session)).finish(dict(self.report(), failure='Write refused'))
        session.fail_log.assert_not_called()
        session.finish_log.assert_not_called()

    def test_presentation_contract_never_defaults_missing_flags(self):
        for version in (2, 3):
            with self.subTest(version=version), self.assertRaises(KeyError):
                outcome(dict(self.report(), schema=f'conquer-native-rng-journal-v{version}'))


if __name__ == '__main__':
    unittest.main()
