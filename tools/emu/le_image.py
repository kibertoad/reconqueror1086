"""Bounded BLD-GOG-EN loader. Mapping: BLD-GOG-EN and FND-RES-009.

No original bytes are embedded. Unsupported relocation forms fail closed.
"""
import hashlib
import struct
from pathlib import Path

SOURCE_SHA256 = '5d7231758766204ad061e6b82cf2f0e0cbe28899b35d095f13e4aad75c8b79d6'


class LoaderError(ValueError):
    pass


def load_image(source):
    if Path(source).stat().st_size != 919107:
        raise LoaderError('Wrong BLD-GOG-EN executable size')
    data = Path(source).read_bytes()
    if len(data) != 919107 or hashlib.sha256(data).hexdigest() != SOURCE_SHA256:
        raise LoaderError('Wrong BLD-GOG-EN executable identity')
    return decode_image(data)


def decode_image(data):
    """Parser entry point for independently authored synthetic tests."""
    def read(offset, width=4, signed=False):
        if offset < 0 or offset + width > len(data):
            raise LoaderError('Truncated LE field')
        return int.from_bytes(data[offset:offset + width], 'little', signed=signed)

    module = 0x26654
    header = module + read(module + 0x3c)
    if read(module, 2) != 0x5a4d or read(header, 2) != 0x454c:
        raise LoaderError('Expected DOS/16M-bound LE image')
    if read(header + 2, 2) != 0 or read(header + 0x28) != 4096:
        raise LoaderError('Unsupported byte order or page size')
    count = read(header + 0x14)
    if count != 149 or read(header + 0x44) != 2:
        raise LoaderError('Unexpected object/page count')
    objects = []
    table = header + read(header + 0x40)
    pages = module + read(header + 0x80)
    last = read(header + 0x2c)
    if not 0 < last <= 4096:
        raise LoaderError('Invalid final page size')
    for index in range(2):
        row = table + index * 24
        size, base, flags, first, number = [read(row + k * 4) for k in range(5)]
        expected = [(0x7cb9e, 0x10000, 1, 125), (0x23670, 0x90000, 126, 24)][index]
        if (size, base, first, number) != expected:
            raise LoaderError('Unexpected object mapping')
        page_map = header + read(header + 0x48)
        image = bytearray(size)
        for ordinal in range(number):
            logical = first + ordinal
            entry = page_map + (logical - 1) * 4
            if read(entry + 3, 1) != 0:
                raise LoaderError('Unsupported LE page flags')
            physical = int.from_bytes(data[entry:entry + 3], 'big')
            if not 1 <= physical <= count:
                raise LoaderError('Invalid physical page')
            length = min(4096, size - ordinal * 4096)
            copied = min(length, last if physical == count else 4096)
            offset = pages + (physical - 1) * 4096
            if offset < 0 or offset + copied > len(data):
                raise LoaderError('Truncated LE page')
            image[ordinal * 4096:ordinal * 4096 + copied] = data[offset:offset + copied]
        objects.append(dict(base=base, size=size, flags=flags, first=first,
                            count=number, image=image))
    page_table = header + read(header + 0x68)
    records = header + read(header + 0x6c)
    extent = read(page_table + count * 4)
    if records + extent > len(data):
        raise LoaderError('Truncated fixup records')
    fixups = 0
    for page in range(count):
        start = read(page_table + page * 4)
        end = read(page_table + (page + 1) * 4)
        if not 0 <= start <= end <= extent:
            raise LoaderError('Invalid fixup page range')
        cursor, limit = records + start, records + end
        obj = next(o for o in objects if o['first'] <= page + 1 < o['first'] + o['count'])
        while cursor < limit:
            sf, tf = read(cursor, 1), read(cursor + 1, 1)
            if sf != 7 or tf not in (0, 16):
                raise LoaderError('Unsupported relocation form')
            width = 4 if tf == 16 else 2
            if cursor + 5 + width > limit:
                raise LoaderError('Fixup crosses page record boundary')
            offset = read(cursor + 2, 2, True)
            target = read(cursor + 4, 1)
            target_offset = read(cursor + 5, width)
            cursor += 5 + width
            if target not in (1, 2):
                raise LoaderError('Invalid fixup target object')
            # Relocations may form boundary pointers; dereference validity is
            # enforced by the emulator rather than by the pointer's creation.
            position = (page + 1 - obj['first']) * 4096 + offset
            if position < 0 or position + 4 > obj['size']:
                raise LoaderError('Fixup source outside object')
            value = (objects[target - 1]['base'] + target_offset) & 0xffffffff
            struct.pack_into('<I', obj['image'], position, value)
            fixups += 1
    return objects, fixups
