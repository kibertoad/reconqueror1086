"""Authored callback-stub controls for FND-RNG-012's original dispatcher."""
import argparse
import json
import os
import struct
from pathlib import Path
from function_call import call
from le_image import load_image


def verify(path):
    original, _ = load_image(path)
    # Each row is marker, priority, callback present. Callbacks preserve all
    # registers, segments and memory and return immediately; they do no game work.
    cases = [
        ('shipped-priorities', 255, [(0, p, True) for p in (32,1,2,32,32,32,32,32)], [1,2,7,6,5,4,3,0]),
        ('no-eligible', 0, [(0,1,True)] * 8, []),
        ('all-completed', 255, [(2,0,True)] * 8, []),
        ('ties-and-other-markers', 7, [(m,7,True) for m in (0,1,2,3,0,1,2,3)], [7,5,4,3,1,0]),
        ('null-target', 255, [(0,1,False)] + [(2,0,True)] * 7, []),
        ('bounded-threshold', 2, [(0,p,True) for p in (0,1,2,3,255,2,1,0)], [7,0,6,1,5,2]),
    ]
    reports = []
    base, table = 0x5000000, 0xa73b4
    for name, threshold, rows, order in cases:
        code = bytearray(4096)
        # Independently authored wrapper: set EAX, call the declared original
        # entry through ECX, then return to the harness sentinel.
        code[:13] = b'\xb8'+struct.pack('<I',threshold)+b'\xb9'+struct.pack('<I',0x83fb4)+b'\xff\xd1\xc3'
        stub_addresses = [base+0x100+16*i for i in range(8)]
        for address in stub_addresses:
            code[address-base] = 0xc3
        initial = b''.join(struct.pack('<BBI',m,p,stub_addresses[i] if present else 0)
                           for i,(m,p,present) in enumerate(rows))
        objects = original + [dict(base=base,size=4096,image=code,flags=5)]
        result = call(objects,base,writes=((table,initial),),instruction_limit=4000)
        actual = [stub_addresses.index(pc) for pc in result['trace'] if pc in stub_addresses]
        expected = bytearray(initial)
        for i,(marker,priority,_) in enumerate(rows):
            if marker != 2 and priority <= threshold:
                expected[6*i] = 2
        if actual != order or result['read'](table,48) != bytes(expected):
            raise RuntimeError(f'{name}: callback order or full table differs')
        if result['read'](base,4096) != bytes(code):
            raise RuntimeError(f'{name}: authored code changed')
        allowed = set(range(table,table+48,6))
        if any(address not in allowed and not 0x2000000 <= address < 0x2010000
               for address in result['writes']):
            raise RuntimeError(f'{name}: unexpected memory write')
        if any(not (base <= pc < base+4096 or 0x83fb4 <= pc < 0x83fff)
               for pc in result['trace']):
            raise RuntimeError(f'{name}: undeclared original callback path')
        reports.append(dict(case=name,threshold=threshold,
                            initial_rows=[dict(marker=m,priority=p,callback_present=c) for m,p,c in rows],
                            callback_order=actual,final_markers=list(expected[::6])))
    return dict(experiment='EXP-RNG-005',cases=reports,
                stubs=['eight distinct register/segment/memory-preserving immediate-return callbacks'],
                segment_model='authored flat 32-bit CS/DS/ES/SS',ports=[],video_ram=False,draws=[])


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--output',type=Path,required=True)
    args = parser.parse_args()
    game_dir = os.environ.get('GAME_DIR')
    if not game_dir:
        print('Skipped: GAME_DIR is absent')
    else:
        # // needs: GAME_DIR
        report = verify(Path(game_dir)/'disc-root'/'CONQUER.EXE')
        args.output.parent.mkdir(parents=True,exist_ok=True)
        args.output.write_text(json.dumps(report,indent=2)+'\n')
        print('Isolated initializer-dispatch controls passed')
