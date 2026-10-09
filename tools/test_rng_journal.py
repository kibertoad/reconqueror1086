"""Synthetic reseeding, state continuity, reduction and content-boundary checks."""
import copy
import unittest
from rng_journal import Journal, replay, reduced
from rng_recording import RecordingError


def draw(journal, reduction='inclusive', bound=8):
    before = journal.state
    journal.begin_draw('RULE-PERSON-002', reduction, bound, before)
    after = (before * 1103515245 + 12345) % 4294967296
    raw = (after // 65536) % 32768
    journal.finish_raw(after, raw)
    journal.finish_bound(after, reduced(raw, reduction, bound))


class GeneralJournalTests(unittest.TestCase):
    def test_initial_seed_draw_reseed_and_zero_seed_replay(self):
        journal = Journal()
        journal.begin_seed('RULE-RNG-001', 1, 123)
        journal.finish_seed(1)
        draw(journal)
        journal.begin_seed('RULE-TALK-001', 0, journal.state)
        journal.finish_seed(0)
        draw(journal, 'remainder', 3)
        self.assertEqual(journal.complete(), replay(journal.events))
        self.assertEqual(['seed', 'draw', 'seed', 'draw'], [e['kind'] for e in journal.events])

    def test_known_reductions_and_scaled_overflow(self):
        self.assertEqual(7, reduced(5758, 'inclusive', 8))
        self.assertEqual(35, reduced(5758, 'scaled', 200))
        self.assertEqual(1, reduced(5758, 'remainder', 3))
        self.assertEqual(5758, reduced(5758, 'raw', 32768))
        self.assertEqual(65535, reduced(32767, 'scaled', 2147483647))

    def test_missing_seed_and_pending_operations_rejected(self):
        journal = Journal()
        with self.assertRaises(RecordingError):
            journal.begin_draw('RULE-PERSON-002', 'inclusive', 8, 1)
        journal.begin_seed('RULE-RNG-001', 1, 0)
        with self.assertRaises(RecordingError):
            journal.complete()
        with self.assertRaisesRegex(RecordingError, 'Reentrant'):
            journal.begin_seed('RULE-RNG-001', 2, 0)

    def test_unrecorded_write_and_raw_or_bounded_divergence_rejected(self):
        for phase in ('entry', 'raw', 'bound'):
            with self.subTest(phase=phase):
                journal = Journal()
                journal.begin_seed('RULE-RNG-001', 1, 0)
                journal.finish_seed(1)
                with self.assertRaises(RecordingError):
                    if phase == 'entry':
                        journal.begin_draw('RULE-PERSON-002', 'inclusive', 1, 2)
                    else:
                        journal.begin_draw('RULE-PERSON-002', 'inclusive', 1, 1)
                        journal.finish_raw(1103527590, 16837 if phase == 'raw' else 16838)
                        journal.finish_bound(1103527590, 1)

    def test_original_address_fields_and_tampering_rejected(self):
        journal = Journal()
        journal.begin_seed('RULE-RNG-001', 1, 0)
        journal.finish_seed(1)
        draw(journal)
        for field, value in [('return_address', 123), ('state_before', 2)]:
            events = copy.deepcopy(journal.events)
            events[1][field] = value
            with self.assertRaises(RecordingError):
                replay(events)


if __name__ == '__main__':
    unittest.main()
