"""Isolated new-game consumers for Q-STRATEGY-043; FND-STRATEGY-049.
No original code or resource content is embedded here.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import struct
from function_call import call
from le_image import load_image


def verify(source):
    if hashlib.sha256(Path(source).read_bytes()).hexdigest() != '5d7231758766204ad061e6b82cf2f0e0cbe28899b35d095f13e4aad75c8b79d6':
        raise RuntimeError('Source does not match BLD-GOG-EN')
    objects, _ = load_image(source)
    data = objects[1]
    seeds = {}
    for seed in range(65536):
        state = (seed * 0x41c64e6d + 0x3039) & 0xffffffff  # RULE-RNG-001
        seeds.setdefault(((state >> 16) & 0x7fff) % 8, seed)
        if len(seeds) == 8:
            break
    cases = []
    for index, seed in sorted(seeds.items()):
        slot = 0x9b8c8 + 4 * index  # FND-STRATEGY-044
        person = int.from_bytes(data['image'][slot-data['base']:slot-data['base']+4], 'little')
        record = 0x9ba50 + 18 * person  # FND-STRATEGY-040
        row, col, group = 4 + index, 3 + index, 1 + index
        initial = ((0x9e044, struct.pack('<I', seed)), (0x9b8c4, struct.pack('<I', 255)),
            (0x9abec, bytes(4)), (record+4, bytes([group])), (record+7, bytes([9])),
            (record+8, struct.pack('<HH', row, col)), (record+12, bytes([199, 10])),
            (0xab0f8, struct.pack('<I', 80)), (0xab100, struct.pack('<I', 80)))
        actual = call(objects, 0x110e8, writes=initial, instruction_limit=2000)
        ranges = ((0x110e8, 0x11280), (0x43670, 0x436e0), (0x2c3a4, 0x2c428),
            (0x2cccc, 0x2ccd3), (0x43248, 0x432b6), (0x24c38, 0x24c4c),
            (0x6b3eb, 0x6b413), (0x4389c, 0x438c5), (0x437b0, 0x437c9),
            (0x63e20, 0x63ebf))  # FND-STRATEGY-043..049 / FND-RNG-002/003
        if any(not any(a <= pc < b for a, b in ranges) for pc in actual['trace']):
            raise RuntimeError('Execution outside the read setup/callee paths')
        def word(address):
            return int.from_bytes(actual['read'](address, 4), 'little')
        final_rng = (seed * 0x41c64e6d + 0x3039) & 0xffffffff
        if actual['trace'].count(0x6b3f1) != 1 or word(0x9e044) != final_rng:
            raise RuntimeError('Setup changed the prescribed draw count/state')
        expected = {0x9c9d0:index, 0xac180:person, 0x9b8c4:person,
            0xab0c4:person, 0xab0cc:group, 0xab0d4:row, 0xab0d0:col,
            0xaaa6c:80*(row+1), 0xaaa70:20*(col+1), 0xaaa74:row, 0xaaa78:col,
            0x9ae6c:5, 0x9ae64:5, 0x9ae5c:0, 0x9a560:1,
            0xaa4b0:0, 0xaa4ac:0, 0x9ae4c:1086, 0x9ae50:1086, 0x9ae54:5000,
            0x9abec:0, 0x9e044:final_rng, 0xab0f8:80, 0xab100:80}
        # FND-STRATEGY-045: six player records, then the active sixth record.
        for i in range(6):
            base = 0xaa4b8 + 0x118*i
            expected.update({base+offset:value for offset,value in
                ((0,0),(4,0),(8,0),(12,1),(20,0),(84,0))})
        expected.update({0xaaa30:1, 0xaaa34:1, 0xaaa3c:1,
                         0xaaa60:0, 0xaaa48:0, 0xaaa44:0})
        for i in range(5):
            base = 0xaab48 + 0x118*i
            expected.update({base:0, base+0x6c:0})
        for i in range(3):
            base = 0xaa150 + 0x118*i
            expected.update({base+offset:0 for offset in (0,12,24,108)})
            order = 0xaa0f0 + 0x20*i
            expected.update({order+20:0, order+24:0})
        for address, value in expected.items():
            if word(address) != value:
                raise RuntimeError(f'Setup field mismatch at {address:#x}')
        anchor = (80*(row+1), 20*(col+1))
        floats = struct.unpack('<ff', actual['read'](0xaaa8c, 8))
        if floats != anchor or actual['read'](record+7, 1) != bytes(1) or \
                actual['read'](record+12, 2) != bytes([12,255]):
            raise RuntimeError('Selected record or floating anchor differs')
        permitted = {record+7, record+12, record+13}
        for address in expected:
            permitted.update(range(address,address+4))
        permitted.update(range(0xaaa8c,0xaaa94))
        if any(address not in permitted and not 0x2000000 <= address < 0x2010000
               for address in actual['writes']):
            raise RuntimeError('Setup writes outside prescribed fields: ' + ', '.join(hex(a) for a in actual['writes'] if a not in permitted and not 0x2000000 <= a < 0x2010000))
        cases.append(dict(seed=seed,home_index=index,person=person,group=group,
                          coordinates=[row,col],anchor=list(anchor),rating=12,rng_state=final_rng))
    return dict(cases=cases,stubs=[],ports=[],video_ram=False)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    game_dir = os.environ.get('GAME_DIR')
    if not game_dir:
        print('Skipped: GAME_DIR is absent')
    else:
        # // needs: GAME_DIR
        report = verify(Path(game_dir)/'disc-root'/'CONQUER.EXE')
        args.output.parent.mkdir(parents=True,exist_ok=True)
        args.output.write_text(json.dumps(report,indent=2)+'\n')
        print('All eight isolated full home-setup controls passed')



