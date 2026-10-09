"""Synthetic queue memory; no original addresses or content in these fixtures."""
import hashlib
import struct
from types import SimpleNamespace
import unittest
from unittest.mock import patch
from supported_pointer_input import queue_primary_click
from rng_recording import RecordingError


LAYOUT = {'pointer_events': 0, 'pointer_count': 40, 'pointer_clock': 44,
          'primary_release_time': 48, 'long_press_limit': 52, 'double_click_limit': 56}


def fixture(clock=10, release=0, count=0, long_limit=4, double_limit=4):
    memory = bytearray(64)
    memory[16:20], memory[36:40] = b'abcd', b'efgh'
    for name, value in [('pointer_count', count), ('pointer_clock', clock),
                        ('primary_release_time', release), ('long_press_limit', long_limit),
                        ('double_click_limit', double_limit)]:
        struct.pack_into('<I', memory, LAYOUT[name], value)
    writes = []
    def write(session, address, data, expected_sha256=None):
        if hashlib.sha256(memory[address:address + len(data)]).hexdigest() != expected_sha256:
            raise RuntimeError('Changed expected memory')
        writes.append((address, data))
        memory[address:address + len(data)] = data
    runtime = SimpleNamespace(
        session=SimpleNamespace(id='owned', state='stopped'),
        registers=lambda: SimpleNamespace(cpu_mode='protected', segments={'ds': '2', 'ss': '2'}),
        MemoryAddress=SimpleNamespace(segmented=lambda selector, offset: offset),
        read=lambda address, length: bytes(memory[address:address + length]),
        agent=SimpleNamespace(write_memory=write))
    mapping = SimpleNamespace(data=SimpleNamespace(selector=2), data_address=lambda address, length: (2, address))
    return runtime, mapping, memory, writes


class SupportedPointerInputTests(unittest.TestCase):
    def invoke(self, runtime, mapping):
        with patch('supported_pointer_input.FIELDS', LAYOUT):
            return queue_primary_click(runtime, mapping, 12, 34)

    def test_only_supported_event_fields_change_and_count_is_published_last(self):
        runtime, mapping, memory, writes = fixture()
        result = self.invoke(runtime, mapping)
        self.assertEqual([address for address, data in writes], [0, 20, 40])
        self.assertEqual([len(data) for address, data in writes], [16, 16, 4])
        self.assertEqual((memory[16:20], memory[36:40]), (b'abcd', b'efgh'))
        self.assertEqual(struct.unpack_from('<Iiii', memory, 0), (10, 12, 34, 0))
        self.assertEqual(struct.unpack_from('<Iiii', memory, 20), (10, 12, 34, 1))
        self.assertEqual(result['event_count'], 2)

    def test_nonempty_queue_is_rejected_before_any_write(self):
        runtime, mapping, memory, writes = fixture(count=1)
        with self.assertRaisesRegex(RecordingError, 'queue must be empty'):
            self.invoke(runtime, mapping)
        self.assertEqual(writes, [])

    def test_double_click_and_zero_long_press_threshold_are_rejected(self):
        for options in ({'release': 9}, {'long_limit': 0}):
            runtime, mapping, memory, writes = fixture(**options)
            with self.subTest(options=options), self.assertRaisesRegex(RecordingError, 'timing'):
                self.invoke(runtime, mapping)
            self.assertEqual(writes, [])

    def test_unsigned_release_gap_wrap_and_disabled_double_clicks_are_supported(self):
        for options in ({'clock': 2, 'release': 4294967289}, {'clock': 0, 'double_limit': 0}):
            runtime, mapping, memory, writes = fixture(**options)
            self.assertEqual(self.invoke(runtime, mapping)['time'], options['clock'])

    def test_running_guest_is_rejected_without_writes(self):
        runtime, mapping, memory, writes = fixture()
        runtime.session.state = 'running'
        with self.assertRaisesRegex(RecordingError, 'stopped owned guest'):
            self.invoke(runtime, mapping)
        self.assertEqual(writes, [])

    def test_readback_divergence_stops_the_input_operation(self):
        runtime, mapping, memory, writes = fixture()
        original = runtime.read
        runtime.read = lambda address, length: (b'x' + original(address, length)[1:]
                                               if address == 0 and length == 40 and len(writes) == 3
                                               else original(address, length))
        with self.assertRaisesRegex(RecordingError, 'readback differs'):
            self.invoke(runtime, mapping)


if __name__ == '__main__':
    unittest.main()
