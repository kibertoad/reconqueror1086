"""Authored no-environment, no-classification cases for FND-RNG-010."""
import argparse
from datetime import datetime
import json
import os
import struct
from pathlib import Path
from function_call import call
from le_image import load_image


def verify(path):
    original,_=load_image(path)
    # Calendar records: second, minute, hour, day, zero-based month,
    # year minus 1900, weekday, year-day, classification.
    cases = [
        ('before-epoch',[0,0,0,1,0,0,-100,-101,0],0,0xffffffff,[0,0,0,1,0,0,1,0,0]),
        ('epoch-edge-positive-adjustment',[59,59,23,31,11,69,-100,-101,0],3600,3599,[59,59,23,31,11,69,3,364,0]),
        ('epoch',[0,0,0,1,0,70,-100,-101,0],0,0,[0,0,0,1,0,70,4,0,0]),
        ('epoch-adjustment',[0,0,0,1,0,70,-100,-101,0],18000,18000,[0,0,0,1,0,70,4,0,0]),
        ('leap-2000',[56,34,12,29,1,100,-100,-101,0],0,951827696,[56,34,12,29,1,100,2,59,0]),
        ('common-2100',[0,0,0,1,2,200,-100,-101,0],0,4107542400,[0,0,0,1,2,200,1,59,0]),
        ('after-century',[0,0,0,1,0,201,-100,-101,0],0,4134067200,[0,0,0,2,0,201,0,1,0]),
    ]
    base=0x5000000
    reports=[]
    for name,words,adjustment,expected,normalized in cases:
        memory=bytearray(4096)
        struct.pack_into('<9i',memory,0,*words)
        objects=original+[dict(base=base,size=4096,image=memory,flags=3)]
        # FND-RNG-006 identifies the environment root and adjustment globals.
        initial=((0xa70d0,bytes(4)),(0xa707c,struct.pack('<i',adjustment)),(0xa7080,bytes(4)))
        result=call(objects,0x80ab3,(base,),writes=initial,instruction_limit=6000)
        wanted=bytearray(memory)
        struct.pack_into('<9i',wanted,0,*normalized)
        if result['result'] != expected or result['read'](base,4096) != bytes(wanted):
            raise RuntimeError(f'{name}: returned value or normalized record differs')
        if any(not base <= a < base+32 and not 0x2000000 <= a < 0x2010000
               for a in result['writes']):
            raise RuntimeError(f'{name}: write outside normalized record and stack')
        paths=((0x80ab3,0x80c0b),(0x8b0b9,0x8b1f9),(0x8ac45,0x8ac7e),
               (0x8b230,0x8b24b),(0x84620,0x8467b))
        if any(not any(a <= pc < b for a,b in paths) for pc in result['trace']):
            raise RuntimeError(f'{name}: unexpected helper path')
        for address,value in initial:
            if result['read'](address,len(value)) != value:
                raise RuntimeError(f'{name}: prescribed global changed')
        reference=int((datetime(words[5]+1900,words[4]+1,words[3],
                                words[2],words[1],words[0])-datetime(1970,1,1)).total_seconds())+adjustment
        difference=None if name == 'before-epoch' else expected-reference
        if difference is not None and difference != (86400 if name == 'after-century' else 0):
            raise RuntimeError(f'{name}: independent Gregorian comparison differs')
        reports.append(dict(case=name,initial_words=words,adjustment=adjustment,
                            return_u32=expected,normalized_words=normalized,
                            gregorian_seconds=reference,difference_seconds=difference))
    return dict(experiment='EXP-RNG-004',cases=reports,stubs=[],ports=[],video_ram=False,
                environment_array=None,classification=0,secondary_adjustment=0,
                segment_model='authored flat 32-bit CS/DS/ES/SS')


if __name__ == '__main__':
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
        print('Isolated no-environment seed-calendar controls passed')
