"""Isolated selector controls for Q-STRATEGY-043.

Locations and field paths: FND-STRATEGY-043..048, FND-STRATEGY-040,
FND-RNG-002/003. No original code or data is embedded in this tool.
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
    data = objects[1]
    def initial_word(address, width):
        return int.from_bytes(data['image'][address-data['base']:address-data['base']+width], 'little')
    # FND-STRATEGY-045: the caller supplies these two destination fields.
    row_out, col_out = 0xab0d4, 0xab0d0
    seeds = {}
    for seed in range(65536):
        state = (seed * 0x41c64e6d + 0x3039) & 0xffffffff  # RULE-RNG-001
        index = ((state >> 16) & 0x7fff) % 8
        seeds.setdefault(index, seed)
        if len(seeds) == 8:
            break
    if len(seeds) != 8:
        raise RuntimeError('Seed controls did not cover the selector range')
    cases = []
    for index, seed in sorted(seeds.items()):
        person = initial_word(0x9b8c8 + 4 * index, 4)
        record = 0x9ba50 + 18 * person
        coordinates = [initial_word(record + offset, 2) for offset in (8, 10)]
        actual = call(objects, 0x43670, arguments=(row_out, col_out), writes=(
            (0x9e044, struct.pack('<I', seed)), (0x9b8c4, struct.pack('<I', 255)),
            (0x9abec, bytes(4)), (record + 7, bytes([9])), (record + 13, bytes([10]))),
            instruction_limit=1000)
        ranges = ((0x43670, 0x436e0), (0x2c3a4, 0x2c428), (0x2cccc, 0x2ccd3),
                  (0x43248, 0x432b6), (0x24c38, 0x24c4c), (0x6b3eb, 0x6b413))
        if any(not any(a <= pc < b for a, b in ranges) for pc in actual['trace']):
            raise RuntimeError('Execution outside the recorded selector and callee paths')
        def word(address):
            return int.from_bytes(actual['read'](address, 4), 'little')
        final_rng = (seed * 0x41c64e6d + 0x3039) & 0xffffffff
        if actual['trace'].count(0x6b3f1) != 1 or word(0x9e044) != final_rng:
            raise RuntimeError('Unexpected selector draw count/state')
        if actual['result'] != person or word(0x9c9d0) != index or word(0xac180) != person:
            raise RuntimeError('Selected home/person identity changed')
        if word(0x9b8c4) != person or actual['read'](record + 13, 1) != bytes([255]):
            raise RuntimeError('Person list is not a single terminated selected entry')
        if actual['read'](record + 7, 1) != bytes(1):
            raise RuntimeError('Selected assignment was not cleared')
        if [word(row_out), word(col_out)] != coordinates or word(0x9abec) != 0:
            raise RuntimeError('Coordinate/root result differs')
        permitted = {record + 7, record + 13}
        for address, length in ((0x9e044, 4), (0x9c9d0, 4), (0x9abec, 4),
                                (0x9b8c4, 4), (0xac180, 4), (row_out, 4), (col_out, 4)):
            permitted.update(range(address, address + length))
        if any(address not in permitted and not 0x2000000 <= address < 0x2010000
               for address in actual['writes']):
            raise RuntimeError('Write outside prescribed state and synthetic call stack')
        cases.append(dict(seed=seed, home_index=index, person=person, coordinates=coordinates,
                          rng_state=final_rng, assignment=0, next=255))
    return dict(cases=cases, stubs=[], ports=[], video_ram=False)


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
        print('All eight isolated home-selection controls passed')
