"""Durable address-free event output; RULE-RNG-001 supplies the numeric control."""
import json
from pathlib import Path
import tempfile
import unittest
from native_rng_recorder import EventLog
from rng_journal import Journal, replay


class EventLogTests(unittest.TestCase):
    def test_completed_prefix_is_readable_before_writer_closes(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'events.jsonl'
            log = EventLog(path)
            journal = Journal()
            try:
                journal.begin_seed('RULE-RNG-001', 1, 0)
                journal.finish_seed(1)
                log.append(journal.events[-1])
                self.assertEqual([json.loads(line) for line in path.read_text().splitlines()], journal.events)
                journal.begin_draw('RULE-PERSON-002', 'inclusive', 8, 1)
                journal.finish_raw(1103527590, 16838)
                journal.finish_bound(1103527590, 8)
                log.append(journal.events[-1])
                recovered = [json.loads(line) for line in path.read_text().splitlines()]
                self.assertEqual(recovered, journal.events)
                self.assertEqual(replay(recovered), 1103527590)
            finally:
                log.close()

    def test_existing_capture_cannot_be_overwritten_or_appended(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'events.jsonl'
            path.write_text('prior capture\n')
            with self.assertRaises(FileExistsError):
                EventLog(path)
            self.assertEqual(path.read_text(), 'prior capture\n')


if __name__ == '__main__':
    unittest.main()
