"""Synthetic positive and refusal controls for FND-UI-026 entry boundaries."""
from types import SimpleNamespace
import unittest
from unittest.mock import Mock, patch
from dubbing_entry_input import DubbingEntryInput
from rng_recording import RecordingError


class EntryInputTests(unittest.TestCase):
    def setUp(self):
        self.driver = DubbingEntryInput()
        self.runtime = SimpleNamespace(read=Mock(return_value=bytes(4)),
                                       MemoryAddress=SimpleNamespace(segmented=lambda segment, offset: (segment, offset)))
        self.mapping = SimpleNamespace(data_address=lambda address, length: (0x188, address))
        self.rng = Mock(return_value=123)
        self.request = {'screen': 6, 'draw': 0, 'mode': 1}

    def observe(self, pc, stack=0xff4, eax=1):
        return self.driver.observe(self.runtime, self.mapping, pc, (0x188, stack), eax, self.request, self.rng)

    def begin(self):
        self.observe(0x19a8c, 0x1000)

    def test_two_waits_acceptance_and_restored_return(self):
        with patch('dubbing_entry_input.primary_click_ready', side_effect=[False, True, True]) as ready, \
             patch('dubbing_entry_input.queue_primary_click', return_value={'action': 'primary-short-click'}) as click:
            self.begin()
            self.assertIsNone(self.observe(0x19bbf))
            self.assertEqual(self.observe(0x19bbf)['stage'], 'text-wait')
            self.assertIsNone(self.observe(0x19bbf))  # down event can repeat the loop
            self.observe(0x19bcc)
            self.assertEqual(self.observe(0x19c51)['stage'], 'briefing-wait')
            self.observe(0x19c5e)
            self.observe(0x19c61, 0x1000)
            self.assertTrue(self.driver.completed)
            self.assertIsNone(self.driver.key)
            self.assertEqual(ready.call_count, 3)
            self.assertEqual(click.call_count, 2)

    def test_wrong_request_and_animations_refuse_before_writes(self):
        self.request['screen'] = 3
        with self.assertRaises(RecordingError):
            self.begin()
        self.request['screen'] = 6
        self.runtime.read.return_value = (1).to_bytes(4, 'little')
        with self.assertRaises(RecordingError):
            self.begin()

    def test_wrong_stack_order_and_premature_return(self):
        self.begin()
        for pc, stack in ((0x19bbf, 0xff0), (0x19c51, 0xff4), (0x19c61, 0x1000), (0x19a8c, 0x1000)):
            with self.subTest(pc=pc), self.assertRaises(RecordingError):
                self.observe(pc, stack)

    def test_acceptance_requires_prescribed_input_result_and_empty_queue(self):
        for eax, queue in ((0, bytes(4)), (1, (1).to_bytes(4, 'little'))):
            with self.subTest(eax=eax, queue=queue):
                self.setUp()
                self.begin()
                with patch('dubbing_entry_input.primary_click_ready', return_value=True), \
                     patch('dubbing_entry_input.queue_primary_click', return_value={}):
                    self.observe(0x19bbf)
                self.runtime.read.return_value = queue
                with self.assertRaises(RecordingError):
                    self.observe(0x19bcc, eax=eax)
        self.setUp()
        self.begin()
        with self.assertRaises(RecordingError):
            self.observe(0x19bcc)

    def test_rng_change_refuses_completion(self):
        self.begin()
        self.rng.side_effect = [123, 124]
        with patch('dubbing_entry_input.primary_click_ready', return_value=True), \
             patch('dubbing_entry_input.queue_primary_click', return_value={}), self.assertRaises(RecordingError):
            self.observe(0x19bbf)


if __name__ == '__main__':
    unittest.main()
