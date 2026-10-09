"""Synthetic journal tamper, ordering and content-boundary checks."""
import copy
import unittest
from rng_recording import RecordingError, replay_startup


def synthetic_events():
    state = 1
    events = [{'kind': 'seed', 'rule': 'RULE-RNG-001', 'seed': state}]
    for ordinal in range(30):
        before = state
        state = (1103515245 * state + 12345) % (1 << 32)
        raw = (state // 65536) % 32768
        parameter, bound = ('negative', 1) if ordinal % 2 == 0 else ('amount', 8)
        events.append({'kind': 'draw', 'rule': 'RULE-PERSON-002', 'parameter': parameter,
                       'field_index': ordinal // 2, 'range': 'inclusive', 'bound': bound,
                       'result': raw % (bound + 1), 'raw_result': raw,
                       'state_before': before, 'state_after': state})
    return events


class JournalTests(unittest.TestCase):
    def test_complete_case_and_known_initial_values(self):
        events = synthetic_events()
        self.assertEqual((1103527590, 16838, 0),
                         (events[1]['state_after'], events[1]['raw_result'], events[1]['result']))
        self.assertEqual((2524885223, 5758, 7),
                         (events[2]['state_after'], events[2]['raw_result'], events[2]['result']))
        self.assertEqual(events[-1]['state_after'], replay_startup(events))

    def test_missing_or_duplicate_draw_rejected(self):
        events = synthetic_events()
        with self.assertRaises(RecordingError):
            replay_startup(events[:-1])
        events[10] = copy.deepcopy(events[9])
        with self.assertRaises(RecordingError):
            replay_startup(events)

    def test_wrong_owner_bound_parameter_or_index_rejected(self):
        for field, value in [('rule', 'RULE-TALK-001'), ('bound', 8),
                             ('parameter', 'amount'), ('field_index', 1)]:
            with self.subTest(field=field):
                events = synthetic_events()
                events[1][field] = value
                with self.assertRaises(RecordingError):
                    replay_startup(events)

    def test_state_and_result_tampering_rejected_at_prefix(self):
        for field in ('state_before', 'state_after', 'raw_result', 'result'):
            with self.subTest(field=field):
                events = synthetic_events()[:2]
                events[1][field] ^= 1
                with self.assertRaises(RecordingError):
                    replay_startup(events, require_complete=False)

    def test_address_diagnostics_cannot_enter_event_schema(self):
        events = synthetic_events()
        events[1]['return_address'] = 123456
        with self.assertRaisesRegex(RecordingError, 'diagnostics'):
            replay_startup(events)

    def test_boolean_fields_and_out_of_width_seed_rejected(self):
        for field in ('bound', 'field_index', 'raw_result', 'result', 'state_before', 'state_after'):
            with self.subTest(field=field):
                events = synthetic_events()
                events[1][field] = False
                with self.assertRaises(RecordingError):
                    replay_startup(events)
        events = synthetic_events()
        events[0]['seed'] = 1 << 32
        with self.assertRaises(RecordingError):
            replay_startup(events)


if __name__ == '__main__':
    unittest.main()
