"""Synthetic terminal acceptance and corrupted/incomplete diagnostic refusal."""
import copy
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

from rng_recording import RecordingError
from verify_native_recording import PENDING_FIELDS, verify
from native_event_log import CONTRACT, AGE_CONTRACT, SCHEMAS, LOG_FORMAT, outcome
from dinorefurb_dosbox_session import EventLogWriter


class NativeVerificationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.path = Path(self.temp.name)
        self.events = [
            {'kind': 'seed', 'rule': 'RULE-RNG-001', 'seed': 1},
            {'kind': 'draw', 'rule': 'RULE-PERSON-002', 'reduction': 'inclusive',
             'bound': 8, 'state_before': 1, 'state_after': 1103527590,
             'raw_result': 16838, 'result': 8},
        ]
        self.report = {
            'schema': 'conquer-native-rng-journal-v1',
            'status': 'youth-continue-return-reached', 'events': self.events,
            'end_rng_state': 1103527590, 'full_game_complete': False,
            'accepted_callers_complete': False,
            **dict.fromkeys(PENDING_FIELDS, False),
        }

    def write(self, report=None, events=None):
        (self.path / 'native-rng-journal.json').write_text(json.dumps(
            self.report if report is None else report), encoding='utf-8')
        (self.path / 'native-rng-events.jsonl').write_text(''.join(
            json.dumps(event) + '\n' for event in
            (self.events if events is None else events)), encoding='utf-8')

    def check(self):
        return verify(self.path, 'youth-continue-return-reached')

    def write_shared(self, events=None, failure=None):
        self.report['event_log_format'] = LOG_FORMAT
        (self.path / 'native-rng-journal.json').write_text(json.dumps(self.report), encoding='utf-8')
        folder = self.path / 'session'
        folder.mkdir(exist_ok=True)
        target = folder / 'events.jsonl'
        if target.exists():
            target.unlink()
        log = EventLogWriter.create(target, AGE_CONTRACT if self.report['schema'] == 'conquer-native-rng-journal-v4' else CONTRACT, SCHEMAS, package_version='0.3.0')
        try:
            for event in self.events if events is None else events:
                log.append(event['kind'], {key: value for key, value in event.items() if key != 'kind'})
            if failure is None:
                log.finish(outcome(self.report))
            else:
                log.fail(failure, outcome(self.report))
        finally:
            log.close()
        return target

    def test_shared_log_positive_and_journal_agreement(self):
        self.write_shared()
        self.assertEqual(self.check()['events'], 2)
        different = [self.events[0], dict(self.events[1], rule='RULE-RNG-001')]
        self.write_shared(different)
        with self.assertRaisesRegex(RecordingError, 'Durable events differ'):
            self.check()

    def test_shared_log_reordering_and_incomplete_refused(self):
        target = self.write_shared()
        lines = target.read_text().splitlines(keepends=True)
        target.write_text(''.join([lines[0], lines[2], lines[1], lines[3]]))
        with self.assertRaisesRegex(RecordingError, 'Shared event log refused'):
            self.check()
        target.write_text(''.join(lines[:-1]))
        with self.assertRaisesRegex(RecordingError, 'Shared event log refused'):
            self.check()

    def test_shared_failure_outcome_is_not_terminal_success(self):
        self.write_shared(failure='Synthetic interrupted capture')
        with self.assertRaisesRegex(RecordingError, 'Shared event log refused'):
            self.check()

    def test_shared_outcome_must_match_whole_journal(self):
        self.write_shared()
        self.report['completed_youth_cycles'] = 1
        (self.path / 'native-rng-journal.json').write_text(json.dumps(self.report))
        with self.assertRaisesRegex(RecordingError, 'Shared event log refused'):
            self.check()

    def test_shared_transport_does_not_replace_numeric_semantics(self):
        self.events[1]['result'] = 7
        self.write_shared()
        with self.assertRaises(RecordingError):
            self.check()

    def test_terminal_numeric_and_durable_agreement(self):
        self.write()
        result = self.check()
        self.assertEqual(result['events'], 2)
        self.assertEqual(result['end_rng_state'], 1103527590)
        self.assertIs(result['full_game_complete'], False)

    def test_incomplete_wrong_boundary_and_failure_rejected(self):
        for changes in ({'status': 'incomplete'}, {'status': 'youth-answer-return-reached'},
                        {'failure': ''}, {'diagnostic_failure': 'TimeoutError'}):
            with self.subTest(changes=changes):
                self.write({**self.report, **changes})
                with self.assertRaises(RecordingError):
                    self.check()

    def test_pending_missing_and_nonboolean_fields_rejected(self):
        for name in PENDING_FIELDS + ('full_game_complete', 'accepted_callers_complete'):
            for value in (None, True, 0):
                with self.subTest(name=name, value=value):
                    self.write({**self.report, name: value})
                    with self.assertRaises(RecordingError):
                        self.check()
            report = dict(self.report)
            del report[name]
            self.write(report)
            with self.assertRaises(RecordingError):
                self.check()

    def test_missing_reordered_and_extra_durable_events_rejected(self):
        for events in (self.events[:1], self.events[::-1], self.events + self.events[:1]):
            self.write(events=events)
            with self.assertRaisesRegex(RecordingError, 'Durable events'):
                self.check()

    def test_matching_but_corrupt_numeric_events_rejected(self):
        events = copy.deepcopy(self.events)
        events[1]['result'] = 7
        self.write({**self.report, 'events': events}, events)
        with self.assertRaisesRegex(RecordingError, 'diverged'):
            self.check()

    def test_final_state_type_and_value_rejected(self):
        for value in (None, True, '1103527590', 1):
            self.write({**self.report, 'end_rng_state': value})
            with self.assertRaisesRegex(RecordingError, 'final state'):
                self.check()

    def test_numeric_equality_cannot_hide_invalid_journal_event_types(self):
        for value in (True, 1.0):
            events = copy.deepcopy(self.events)
            events[0]['seed'] = value
            self.write({**self.report, 'events': events})
            with self.assertRaisesRegex(RecordingError, 'Invalid seed'):
                self.check()

    def test_unknown_schema_and_nonterminal_request_rejected(self):
        self.write({**self.report, 'schema': 'unknown'})
        with self.assertRaisesRegex(RecordingError, 'schema'):
            self.check()
        with self.assertRaisesRegex(RecordingError, 'terminal'):
            verify(self.path, 'incomplete')

    def test_truncated_and_oversized_files_rejected(self):
        self.write()
        with patch('verify_native_recording.MAX_BYTES', 16):
            with self.assertRaisesRegex(RecordingError, 'size limit'):
                self.check()
        (self.path / 'native-rng-events.jsonl').write_text('{', encoding='utf-8')
        with self.assertRaises(json.JSONDecodeError):
            self.check()

    def youth_report(self, cycles):
        screen = 6 if cycles == 6 else 3
        return {**self.report,
                'status': 'youth-continue-return-reached' if cycles == 1 else 'youth-sequence-return-reached',
                'completed_youth_cycles': cycles,
                'screen_observation': {'screen_id': screen, 'history': [screen, 2, 1, 0, -1]}}

    def test_prescribed_cycles_and_dubbing_endpoint(self):
        for cycles in range(1, 7):
            report = self.youth_report(cycles)
            self.write(report)
            result = verify(self.path, report['status'], cycles)
            self.assertEqual(result['completed_youth_cycles'], cycles)
            self.assertEqual(result['screen_id'], 6 if cycles == 6 else 3)

    def test_missing_mismatched_and_noninteger_cycle_count_rejected(self):
        for value in (None, 5, True, 6.0, '6'):
            report = {**self.youth_report(6), 'completed_youth_cycles': value}
            self.write(report)
            with self.assertRaisesRegex(RecordingError, 'requested youth cycles'):
                verify(self.path, report['status'], 6)

    def test_wrong_or_missing_endpoint_screen_and_history_rejected(self):
        for cycles in (1, 6):
            report = self.youth_report(cycles)
            for observation in (None, {}, {'screen_id': 3 if cycles == 6 else 6, 'history': [3] * 5},
                                {'screen_id': report['screen_observation']['screen_id'], 'history': []},
                                {'screen_id': report['screen_observation']['screen_id'], 'history': [2] * 5}):
                self.write({**report, 'screen_observation': observation})
                with self.assertRaisesRegex(RecordingError, 'screen'):
                    verify(self.path, report['status'], cycles)

    def test_cycle_request_requires_valid_count_and_matching_boundary(self):
        for value in (0, 7, True, 1.5):
            with self.assertRaisesRegex(RecordingError, 'integer'):
                verify(self.path, 'youth-sequence-return-reached', value)
        for status, cycles in (('youth-answer-return-reached', 1),
                               ('youth-continue-return-reached', 6),
                               ('youth-sequence-return-reached', 1)):
            with self.assertRaisesRegex(RecordingError, 'corresponding Continue boundary'):
                verify(self.path, status, cycles)

    def dubbing_report(self):
        return {**self.youth_report(6), 'schema': 'conquer-native-rng-journal-v2',
                'status': 'dubbing-return-reached', 'pending_dubbing': False,
                'screen_observation': {'screen_id': 11, 'history': [11, 6, 3, 2, 1]}}

    def march_report(self):
        rows = [{'boundary': 'youth-screen-return', 'completed_cycles': 0, 'age': 13, 'rng_state': 1}]
        for cycle in range(5):
            rows.extend({'boundary': boundary, 'completed_cycles': cycle, 'age': age,
                         'rng_state': 1103527590} for boundary, age in (
                ('answer-entry', 13+cycle), ('answer-return', 14+cycle),
                ('continue-entry', 14+cycle), ('continue-return', 14+cycle)))
        return {**self.dubbing_report(), 'schema': 'conquer-native-rng-journal-v4',
                'completed_youth_cycles': 5, 'pending_dubbing_entry': False,
                'dubbing_entry_complete': True, 'youth_ages': rows}

    def test_v4_accepts_five_age_checked_cycles_with_shared_log(self):
        report = self.march_report()
        self.report = report
        self.write_shared()
        self.assertEqual(verify(self.path, 'dubbing-return-reached', 5)['screen_id'], 11)

    def test_v4_shared_outcome_binds_age_observation_rng_states(self):
        self.report = self.march_report()
        self.write_shared()
        self.report['youth_ages'][3]['rng_state'] = 2
        self.write(self.report)
        with self.assertRaisesRegex(RecordingError, 'Shared event log refused'):
            verify(self.path, 'dubbing-return-reached', 5)

    def test_v4_rejects_missing_reordered_or_changed_age_boundaries(self):
        original = self.march_report()
        for index, field, value in ((0, 'age', 12), (2, 'age', 13), (3, 'age', 15),
                                   (20, 'age', 19), (4, 'completed_cycles', True),
                                   (8, 'boundary', 'answer-return')):
            report = copy.deepcopy(original)
            report['youth_ages'][index][field] = value
            self.write(report)
            with self.assertRaisesRegex(RecordingError, 'AGE boundary'):
                verify(self.path, 'dubbing-return-reached', 5)
        self.write({**original, 'youth_ages': original['youth_ages'][:-1]})
        with self.assertRaisesRegex(RecordingError, 'AGE boundary'):
            verify(self.path, 'dubbing-return-reached', 5)

    def test_v2_dubbing_requires_six_cycles_and_village_endpoint(self):
        self.write(self.dubbing_report())
        result = verify(self.path, 'dubbing-return-reached', 6)
        self.assertEqual(result['screen_id'], 11)
        self.assertEqual(result['completed_youth_cycles'], 6)
        with self.assertRaisesRegex(RecordingError, 'schema v2 and six'):
            verify(self.path, 'dubbing-return-reached')

    def test_v2_dubbing_pending_flag_must_be_explicit_and_false(self):
        for value in (True, None, 0):
            self.write({**self.dubbing_report(), 'pending_dubbing': value})
            with self.assertRaisesRegex(RecordingError, 'pending or unreported dubbing'):
                verify(self.path, 'dubbing-return-reached', 6)
        report = self.dubbing_report()
        del report['pending_dubbing']
        self.write(report)
        with self.assertRaisesRegex(RecordingError, 'pending or unreported dubbing'):
            verify(self.path, 'dubbing-return-reached', 6)

    def test_dubbing_rejects_v1_schema_or_youth_endpoint(self):
        self.write({**self.dubbing_report(), 'schema': 'conquer-native-rng-journal-v1'})
        with self.assertRaisesRegex(RecordingError, 'schema v2'):
            verify(self.path, 'dubbing-return-reached', 6)
        self.write({**self.dubbing_report(), 'screen_observation': self.youth_report(6)['screen_observation']})
        with self.assertRaisesRegex(RecordingError, 'endpoint screen'):
            verify(self.path, 'dubbing-return-reached', 6)


    def test_v3_requires_completed_and_explicit_entry_state(self):
        report = {**self.dubbing_report(), 'schema': 'conquer-native-rng-journal-v3',
                  'pending_dubbing_entry': False, 'dubbing_entry_complete': True}
        self.write(report)
        self.assertEqual(verify(self.path, 'dubbing-return-reached', 6)['screen_id'], 11)
        for field, values in (('pending_dubbing_entry', (True, None, 0)),
                              ('dubbing_entry_complete', (False, None, 1))):
            for value in values:
                with self.subTest(field=field, value=value):
                    self.write({**report, field: value})
                    with self.assertRaises(RecordingError):
                        verify(self.path, 'dubbing-return-reached', 6)
            missing = dict(report)
            del missing[field]
            self.write(missing)
            with self.assertRaises(RecordingError):
                verify(self.path, 'dubbing-return-reached', 6)

    def test_entry_fields_cannot_reinterpret_historical_v2(self):
        self.write({**self.dubbing_report(), 'dubbing_entry_complete': True})
        with self.assertRaisesRegex(RecordingError, 'schema v3'):
            verify(self.path, 'dubbing-return-reached', 6)


if __name__ == '__main__':
    unittest.main()
