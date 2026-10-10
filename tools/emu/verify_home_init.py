"""Isolated FND-STRATEGY-047 dispatch controls; no native game state or assets.

The loaded code stays unchanged. FND-ESTATE-003 identifies the fief root.
All pointees are fabricated RAM, not an original save or live-state patch.
"""
import argparse
import json
import os
from pathlib import Path
import struct
from function_call import call
from le_image import load_image


def verify(source):
    objects, _ = load_image(source)
    synthetic_base = 0x10000000
    root, pointers, fief = (synthetic_base + n for n in (0, 0x100, 0x200))
    lists = {0x34: synthetic_base + 0x300, 0x30: synthetic_base + 0x600,
             0x2c: synthetic_base + 0x900}
    memory = bytearray(4096)
    def pointer(at, value):
        struct.pack_into('<I', memory, at - synthetic_base, value)
    pointer(root + 8, pointers)
    pointer(pointers, fief)
    for field, value in lists.items():
        pointer(fief + field, value)
    objects.append(dict(base=synthetic_base, size=len(memory), flags=3, image=memory))
    # These locations and values are recorded in FND-STRATEGY-047.
    expected = {0x34: {0x24: 37}, 0x30: {0x5c: 40, 0x7c: 93, 0x9c: 13, 0xbc: 5},
                0x2c: {0x5c: 6, 0x70: 6, 0x88: 6, 0x9c: 6, 0xb4: 4, 0xc8: 4,
                       0x138: 10, 0x14c: 10, 0x164: 3, 0x178: 3}}
    cases = []
    for argument in (*range(8), 28):
        actual = call(objects, 0x2c3a4, arguments=(argument,),
                      writes=((0x9abec, struct.pack('<I', root)),), instruction_limit=1000)
        if any(not (0x2c3a4 <= pc < 0x2c428 or 0x2cae8 <= pc < 0x2cbee or
                    0x2cccc <= pc < 0x2ccd3) for pc in actual['trace']):
            raise RuntimeError('Execution outside the recorded dispatch paths')
        wanted = bytearray(memory)
        if argument == 28:
            for field, values in expected.items():
                for offset, value in values.items():
                    struct.pack_into('<I', wanted, lists[field] + offset - synthetic_base, value)
        if actual['read'](synthetic_base, len(memory)) != bytes(wanted):
            raise RuntimeError(f'Unexpected fief writes for argument {argument}')
        if actual['read'](0x9abec, 4) != struct.pack('<I', root):
            raise RuntimeError('Fief root changed')
        allowed = set(range(0x9abec, 0x9abec + 4))
        if argument == 28:
            allowed.update(lists[field] + offset + byte
                           for field, values in expected.items() for offset in values for byte in range(4))
        if set(actual['writes']) - allowed:
            raise RuntimeError('Write outside the observed branch fields')
        cases.append({'argument': argument, 'fief_changed': argument == 28,
                      'instructions': len(actual['trace'])})
    return {'cases': cases, 'stubs': [], 'ports': [], 'video_ram': False}


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    game_dir = os.environ.get('GAME_DIR')
    if not game_dir:
        print('Skipped: GAME_DIR is absent')
    else:
        # // needs: GAME_DIR
        report = verify(Path(game_dir) / 'disc-root' / 'CONQUER.EXE')
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(json.dumps(report, indent=2))
        print('Home initialization dispatch controls passed')
