"""Isolated FND-PERSON-012 controls; FND-RES-062 bounds the attribute setter."""
import argparse
import json
import os
from pathlib import Path
import struct
from function_call import call
from le_image import load_image


def signed(value):
    value &= 0xffffffff
    return value - 0x100000000 if value & 0x80000000 else value


def verify(source):
    original, _ = load_image(source)
    base = 0x10000000
    header, pointers, calendar = base, base + 0x40, base + 0x80
    rows, values = (base + 0x100, base + 0x160), (base + 0x200, base + 0x300)
    cases = [
        ('fresh-march', 2, 2, (0, 1), (12, 19)),
        ('already-marked', 2, 2, (1, 0), (12, 19)),
        ('other-month-clear', 1, 2, (0, 1), (12, 19)),
        ('other-month-reset', 3, 2, (1, 0), (12, 19)),
        *[(f'milestone-{age + 1}', 2, 2, (0, 0), (age, 12)) for age in (19, 22, 25, 28)],
        ('age-overflow', 2, 2, (0, 0), (0x7fffffff, -1)),
        ('march-zero-count', 2, 0, (0, 1), (12, 19)),
        ('march-negative-count', 2, -1, (0, 1), (12, 19)),
        ('reset-zero-count', 1, 0, (1, 1), (12, 19)),
    ]
    reports = []
    for name, month, count, flags, ages in cases:
        memory = bytearray(4096)
        def put(at, value):
            struct.pack_into('<I', memory, at - base, value & 0xffffffff)
        put(header, pointers)
        put(header + 8, 30)
        put(header + 12, count)
        put(calendar + 0x18, month)
        skills = {0: 0, 1: 19, 3: 20, 4: -1}
        for i in range(2):
            put(pointers + 4 * i, rows[i])
            put(rows[i] + 0x50, values[i])
            put(rows[i] + 0x54, flags[i])
            for field, value in skills.items():
                put(values[i] + 4 * field, value)
            put(values[i] + 4 * 2, 17)
            put(values[i] + 4 * 18, ages[i])
        wanted = bytearray(memory)
        allowed = set(range(0x9a650, 0x9a654))
        def expect(at, value):
            struct.pack_into('<I', wanted, at - base, value & 0xffffffff)
            allowed.update(range(at, at + 4))
        active = count > 0 and ((month == 2 and flags[0] == 0) or (month != 2 and flags[0] != 0))
        if active:
            for i in range(count):
                if month == 2:
                    computed = signed(ages[i] + 1)
                    expect(values[i] + 4 * 18, max(0, computed))
                    if computed in (20, 23, 26, 29):
                        for field, value in skills.items():
                            expect(values[i] + 4 * field, max(0, min(20, signed(value + 1))))
                expect(rows[i] + 0x54, int(month == 2))
        objects = original + [dict(base=base, size=len(memory), flags=3, image=memory)]
        result = call(objects, 0x2b1ec, writes=((0x9a650, struct.pack('<I', header)),
                      (0x9ae00, struct.pack('<I', calendar))), instruction_limit=2000)
        paths = ((0x2b1ec, 0x2b2ef), (0x15ef0, 0x15f87), (0x15f98, 0x15faa),
                 (0x15fac, 0x15fc2), (0x15fdc, 0x15fe5), (0x3866c, 0x38675))
        unexpected = sorted({pc for pc in result['trace'] if not any(a <= pc < b for a, b in paths)})
        if unexpected:
            raise RuntimeError(f'{name}: instruction outside the recorded pass/helpers: {unexpected}')
        observed = result['read'](base, len(memory))
        outside = {address for address in result['writes'] if address not in allowed
                   and not 0x2000000 <= address < 0x2010000}
        if observed != bytes(wanted) or outside:
            differences = [(i, a, b) for i, (a, b) in enumerate(zip(observed, wanted)) if a != b]
            raise RuntimeError(f'{name}: fabricated state differences {differences[:20]}; '
                               f'outside writes {sorted(outside)}')
        for global_address, pointer in ((0x9a650, header), (0x9ae00, calendar)):
            if result['read'](global_address, 4) != struct.pack('<I', pointer):
                raise RuntimeError(f'{name}: state root changed')
        reports.append({'case': name, 'month': month, 'character_count': count,
                        'ages_before': ages, 'flags_before': flags,
                        'ages_after': [struct.unpack_from('<i', wanted, v + 72 - base)[0] for v in values],
                        'flags_after': [struct.unpack_from('<I', wanted, r + 84 - base)[0] for r in rows],
                        'skills_after': [{str(f): struct.unpack_from('<i', wanted, v + 4 * f - base)[0]
                                         for f in skills} for v in values],
                        'instructions': len(result['trace'])})
    return {'cases': reports, 'stubs': [], 'ports': [], 'video_ram': False}


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    game_dir = os.environ.get('GAME_DIR')
    if not game_dir:
        print('Skipped: GAME_DIR is absent')
    else:
        # // needs: GAME_DIR
        result = verify(Path(game_dir) / 'disc-root' / 'CONQUER.EXE')
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(json.dumps(result, indent=2))
        print('March age-pass controls passed')
