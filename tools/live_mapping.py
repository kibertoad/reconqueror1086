"""Fail-closed per-run LE map validation. Evidence: FND-RNG-004.

No guest bytes are retained by the returned map. Original identity verification
belongs to emu.le_image.load_image before this pure comparison is called.
"""
from collections import Counter
from dataclasses import dataclass
import re
import struct


class MappingError(ValueError):
    pass


@dataclass(frozen=True)
class Descriptor:
    selector: int
    base: int
    limit: int
    access: int
    big: bool

    def resolve(self, offset, length=1):
        if not isinstance(offset, int) or not isinstance(length, int) or length < 1:
            raise MappingError('Invalid segmented access extent')
        if not self.big or offset < 0 or offset + length - 1 > self.limit:
            raise MappingError('Segment access exceeds verified 32-bit descriptor')
        address = self.base + offset
        if address + length > 1 << 32:
            raise MappingError('Segment access wraps the linear address space')
        return address


def descriptor(table, selector):
    if selector & 7 or selector < 8 or selector + 8 > len(table):
        raise MappingError('Unsupported selector or missing descriptor')
    row = table[selector:selector + 8]
    access = row[5]
    if access & 0xf0 != 0x90:
        raise MappingError('Expected a present ring-zero code/data descriptor')
    base = int.from_bytes(row[2:4], 'little') | row[4] << 16 | row[7] << 24
    limit = int.from_bytes(row[:2], 'little') | (row[6] & 15) << 16
    if row[6] & 128:
        limit = (limit << 12) | 4095
    return Descriptor(selector, base, limit, access, bool(row[6] & 64))


def tables_from_diagnostic(diagnostic):
    cr0 = re.search(r'cr0:([0-9a-fA-F]{8})', diagnostic)
    table = re.search(r'GDT\s+base=([0-9a-fA-F]+)\s+limit=([0-9a-fA-F]+)', diagnostic)
    if not cr0 or not table:
        raise MappingError('Pinned CPU diagnostic omitted control/table fields')
    control = int(cr0.group(1), 16)
    if control & 0x80000001 != 1:
        raise MappingError('Only unpaged protected mode has been verified')
    base, limit = (int(value, 16) for value in table.groups())
    if limit > 65535 or base + limit + 1 > 16 * 1024 * 1024:
        raise MappingError('Descriptor table exceeds the bounded snapshot')
    return base, limit + 1


@dataclass(frozen=True)
class LiveMap:
    code_base: int
    data_base: int
    code_size: int
    data_size: int
    code: Descriptor
    data: Descriptor

    def code_address(self, canonical, length=1):
        return self._address(canonical, length, 0x10000, self.code_base, self.code_size, self.code)

    def data_address(self, canonical, length=1):
        return self._address(canonical, length, 0x90000, self.data_base, self.data_size, self.data)

    @staticmethod
    def _address(canonical, length, original, loaded, size, segment):
        offset = canonical - original
        if length < 1 or offset < 0 or offset + length > size:
            raise MappingError('Canonical access lies outside its LE object')
        linear = loaded + offset
        segmented = linear - segment.base
        if segment.resolve(segmented, length) != linear:
            raise MappingError('Descriptor and LE object mapping disagree')
        return segment.selector, segmented


def validate_snapshot(memory, objects, diagnostic):
    """Validate the two observed objects and selectors against a coherent stop."""
    if len(memory) != 16 * 1024 * 1024 or len(objects) != 2:
        raise MappingError('Expected two LE objects and a complete 16 MiB snapshot')
    table_base, table_length = tables_from_diagnostic(diagnostic)
    table = memory[table_base:table_base + table_length]
    probe = 0x6b3f1 - objects[0]['base']
    signature = bytes(objects[0]['image'][probe:probe + 34])
    if len(signature) != 34:
        raise MappingError('RNG identity control exceeds code image')
    found = memory.find(signature)
    if found < 0 or memory.find(signature, found + 1) >= 0:
        raise MappingError('RNG identity control is absent or ambiguous')
    code_base = found - probe
    if code_base < 0 or code_base + objects[0]['size'] > len(memory):
        raise MappingError('Candidate code object exceeds guest memory')
    bases = {}
    for target in (1, 2):
        candidates = Counter((struct.unpack_from('<I', memory, code_base + row['position'])[0]
                              - row['target_offset']) & 0xffffffff
                             for row in objects[0]['relocations'] if row['target'] == target)
        if len(candidates) != 1:
            raise MappingError('Code relocation targets do not agree on one object base')
        bases[target] = next(iter(candidates))
    if bases[1] != code_base:
        raise MappingError('Code identity and relocation controls disagree')
    for number, obj in enumerate(objects, 1):
        if bases[number] + obj['size'] > len(memory):
            raise MappingError('Loaded object exceeds guest memory')
    if not (bases[1] + objects[0]['size'] <= bases[2] or bases[2] + objects[1]['size'] <= bases[1]):
        raise MappingError('Loaded objects overlap')
    selector_offset = 0x71139 - objects[0]['base']
    data_selector = struct.unpack_from('<H', memory, code_base + selector_offset)[0]
    data = descriptor(table, data_selector)
    if data.base != 0 or data.limit != 0xffffffff or not data.big or data.access & 0xe != 2:
        raise MappingError('Unsupported data descriptor; only observed flat writable data is verified')
    expected = bytearray(objects[0]['image'])
    for row in objects[0]['relocations']:
        struct.pack_into('<I', expected, row['position'],
                         (bases[row['target']] + row['target_offset']) & 0xffffffff)
    # FND-RNG-004: exactly this selector word differs at the observed live stop.
    struct.pack_into('<H', expected, selector_offset, data_selector)
    if memory[code_base:code_base + len(expected)] != expected:
        raise MappingError('Unexpected code mutation or incomplete relocation')
    codes = []
    for selector in range(8, len(table) - 7, 8):
        try:
            row = descriptor(table, selector)
        except MappingError:
            continue
        if row.big and row.base == 0 and row.limit == 0xffffffff and row.access & 0xe == 0xa:
            codes.append(row)
    if len(codes) != 1:
        raise MappingError('Expected one observed flat readable code descriptor')
    state_pointer = struct.unpack_from('<I', memory, code_base + 0x6b3ec - objects[0]['base'])[0]
    if state_pointer != bases[2] + 0x9e044 - objects[1]['base']:
        raise MappingError('Independent relocated RNG state pointer disagrees with data base')
    data.resolve(state_pointer, 4)
    codes[0].resolve(code_base, objects[0]['size'])
    data.resolve(bases[2], objects[1]['size'])
    return LiveMap(code_base, bases[2], objects[0]['size'], objects[1]['size'], codes[0], data)
