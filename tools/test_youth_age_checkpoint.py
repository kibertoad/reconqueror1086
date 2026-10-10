"""Synthetic read-only AGE lookup and malformed-pointer refusal controls."""
from types import SimpleNamespace
import struct
import unittest
from unittest.mock import Mock
from youth_age_checkpoint import read_youth_age
from rng_recording import RecordingError


class AgeCheckpointTests(unittest.TestCase):
    def fixture(self, age=12, table=0x300000, attributes=30, characters=15, values=0x303000):
        blocks = {0x9a650: struct.pack('<I', table),
                  0x300000: struct.pack('<4I', 0x301000, 0, attributes, characters),
                  0x301000: struct.pack('<I', 0x302000),
                  0x302000: bytes(80) + struct.pack('<I', values),
                  0x303000: bytes(4), 0x303048: struct.pack('<i', age)}
        runtime = SimpleNamespace(MemoryAddress=SimpleNamespace(segmented=lambda s, o: o),
                                  read=Mock(side_effect=lambda address, length: blocks[address]))
        mapping = SimpleNamespace(data=SimpleNamespace(selector=0x188),
                                  data_address=lambda address, length: (0x188, address))
        return runtime, mapping

    def test_named_age_field_and_signed_width(self):
        for age in (12, 18, -1):
            with self.subTest(age=age):
                runtime, mapping = self.fixture(age)
                self.assertEqual(read_youth_age(runtime, mapping), age)

    def test_pointer_and_count_refusals_precede_outside_reads(self):
        for options in ({'table': 0}, {'table': 0x300001}, {'table': 0xfffffffc},
                        {'attributes': 18}, {'characters': 0}, {'values': 0xfffffffc}):
            with self.subTest(options=options):
                runtime, mapping = self.fixture(**options)
                with self.assertRaises(RecordingError):
                    read_youth_age(runtime, mapping)


if __name__ == '__main__':
    unittest.main()
