"""Synthetic mapping controls and rejection cases, with no original content."""
import struct
import unittest
from live_mapping import MappingError, validate_snapshot


def example():
    objects = [dict(base=0x10000, size=0x70000, image=bytearray(0x70000), relocations=[
        dict(position=0x1000, target=1, target_offset=0x100),
        dict(position=0x5b3ec, target=2, target_offset=0xe044)]),
        dict(base=0x90000, size=0x20000, image=bytearray(0x20000), relocations=[])]
    objects[0]['image'][0x5b3f1:0x5b413] = bytes(range(34))
    bases = {1: 0x1e0000, 2: 0x250000}
    memory = bytearray(16 * 1024 * 1024)
    image = bytearray(objects[0]['image'])
    for row in objects[0]['relocations']:
        struct.pack_into('<I', image, row['position'], bases[row['target']] + row['target_offset'])
    struct.pack_into('<H', image, 0x61139, 0x188)
    memory[bases[1]:bases[1] + len(image)] = image
    for selector, access in ((0x180, 0x9a), (0x188, 0x92)):
        memory[0x170010 + selector:0x170018 + selector] = bytes([255, 255, 0, 0, 0, access, 207, 0])
    return memory, objects, 'cr0:00000011\nGDT base=00170010 limit=00003FFF'


class MappingTests(unittest.TestCase):
    def test_separate_object_deltas_and_bounds(self):
        mapping = validate_snapshot(*example())
        self.assertEqual((0x180, 0x23b3f1), mapping.code_address(0x6b3f1))
        self.assertEqual((0x188, 0x25e044), mapping.data_address(0x9e044, 4))
        with self.assertRaises(MappingError):
            mapping.data_address(0x8ffff)
        with self.assertRaises(MappingError):
            mapping.data_address(0xaffff, 2)

    def test_unexpected_code_write_is_rejected(self):
        memory, objects, cpu = example()
        memory[0x1e0200] = 1
        with self.assertRaisesRegex(MappingError, 'code mutation'):
            validate_snapshot(memory, objects, cpu)

    def test_duplicate_control_is_rejected(self):
        memory, objects, cpu = example()
        memory[0x300000:0x300022] = bytes(range(34))
        with self.assertRaisesRegex(MappingError, 'ambiguous'):
            validate_snapshot(memory, objects, cpu)

    def test_inconsistent_relocation_base_is_rejected(self):
        memory, objects, cpu = example()
        objects[0]['relocations'].append(dict(position=0x1100, target=2, target_offset=0xe044))
        with self.assertRaisesRegex(MappingError, 'do not agree'):
            validate_snapshot(memory, objects, cpu)

    def test_data_descriptor_with_nonzero_base_is_rejected(self):
        memory, objects, cpu = example()
        memory[0x170010 + 0x188 + 2] = 1
        with self.assertRaisesRegex(MappingError, 'data descriptor'):
            validate_snapshot(memory, objects, cpu)

    def test_ambiguous_code_descriptor_is_rejected(self):
        memory, objects, cpu = example()
        memory[0x170010 + 0x190:0x170018 + 0x190] = bytes([255, 255, 0, 0, 0, 154, 207, 0])
        with self.assertRaisesRegex(MappingError, 'one observed'):
            validate_snapshot(memory, objects, cpu)

    def test_paging_and_unbounded_snapshots_are_rejected(self):
        memory, objects, cpu = example()
        with self.assertRaisesRegex(MappingError, 'unpaged'):
            validate_snapshot(memory, objects, cpu.replace('00000011', '80000011'))
        with self.assertRaisesRegex(MappingError, 'complete'):
            validate_snapshot(memory[:-1], objects, cpu)


if __name__ == '__main__':
    unittest.main()
