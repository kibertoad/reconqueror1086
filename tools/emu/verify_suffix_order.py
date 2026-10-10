"""Authored nonzero-selector controls for FND-RNG-014."""
import argparse
import json
import os
import struct
from pathlib import Path
from function_call import call
from le_image import load_image


def verify(path):
    original,_=load_image(path)
    base=0x5000000
    direct=[(1,0,0xffffffff),(1,-2147483648,0x7fffffff),
            (1,2147483647,0x7ffffffe),(-1,0,0),(-1,-2147483648,0x80000000),
            (2,365,365),(-2,400,400)]
    comparisons=[((1,1),(-1,0),0),((-1,0),(1,0),1),
                 ((1,-2147483648),(-1,2147483647),0),
                 ((-1,-2147483648),(-1,1),0),((-1,5),(2,4),1)]
    cases=[('ordinal',[(selector,ordinal)],expected) for selector,ordinal,expected in direct]
    cases += [('order',[first,second],expected) for first,second,expected in comparisons]
    reports=[]
    for mode,records,expected in cases:
        for year in (0,124):
            memory=bytearray([0x5a]*4096)
            for i,(selector,ordinal) in enumerate(records):
                struct.pack_into('<ii',memory,64*i+28,ordinal,selector)
            objects=original+[dict(base=base,size=4096,image=memory,flags=3)]
            entry=0x8ac7e if mode=='ordinal' else 0x8ad6f
            arguments=(base,year) if mode=='ordinal' else (base,base+64,year)
            result=call(objects,entry,arguments,instruction_limit=1000)
            if result['result']!=expected or result['read'](base,4096)!=bytes(memory):
                raise RuntimeError(f'{mode}/{records}/{year}: result or full RAM differs')
            if any(not 0x2000000<=address<0x2010000 for address in result['writes']):
                raise RuntimeError('Ordinal/order path wrote outside stack')
            if any(not (0x8ac7e<=pc<0x8ac94 or 0x8ad58<=pc<0x8adab)
                   for pc in result['trace']):
                raise RuntimeError('Unexpected month/weekday or other helper path')
            reports.append(dict(mode=mode,records=[dict(selector=s,ordinal=o) for s,o in records],
                                year_since_1900=year,return_u32=expected))
    return dict(experiment='EXP-RNG-007',cases=reports,stubs=[],ports=[],video_ram=False,draws=[],
                segment_model='authored flat 32-bit CS/DS/ES/SS')


if __name__=='__main__':
    parser=argparse.ArgumentParser()
    parser.add_argument('--output',type=Path,required=True)
    args=parser.parse_args()
    game_dir=os.environ.get('GAME_DIR')
    if not game_dir:
        print('Skipped: GAME_DIR is absent')
    else:
        # // needs: GAME_DIR
        report=verify(Path(game_dir)/'disc-root'/'CONQUER.EXE')
        args.output.parent.mkdir(parents=True,exist_ok=True)
        args.output.write_text(json.dumps(report,indent=2)+'\n')
        print('Isolated nonzero-selector suffix-order controls passed')
