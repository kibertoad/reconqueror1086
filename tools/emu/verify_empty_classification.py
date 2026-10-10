"""Authored empty-name calendar controls for FND-RNG-013."""
import argparse
import json
import os
import struct
from pathlib import Path
from function_call import call
from le_image import load_image


def verify(path):
    original,_ = load_image(path)
    base = 0x5000000
    reports = []
    for mode,classification in [('direct',v) for v in (-2,-1,0,1)] + [('conversion',v) for v in (-1,0,1)]:
        words = [0,0,0,1,0,70,-100,-101,classification]
        memory = bytearray([0x5a]*4096)
        struct.pack_into('<9i',memory,0,*words)
        memory[256] = 0
        prescribed = ((0xa70ca,struct.pack('<I',base+256)),
                      (0xa70d0,bytes(4)),(0xa707c,struct.pack('<i',18000)),
                      (0xa7080,struct.pack('<i',3600)))
        objects = original + [dict(base=base,size=4096,image=memory,flags=3)]
        result = call(objects,0x8adab if mode=='direct' else 0x80ab3,
                      (base,),writes=prescribed,instruction_limit=6000)
        wanted = bytearray(memory)
        normalized = words.copy()
        if mode=='direct':
            normalized[8] = 0
            expected = 0
        else:
            normalized[6:8] = [4,0]
            normalized[8] = max(classification,0)
            expected = 14400 if classification>0 else 18000
        struct.pack_into('<9i',wanted,0,*normalized)
        if result['result'] != expected or result['read'](base,4096) != bytes(wanted):
            raise RuntimeError(f'{mode}/{classification}: result or full RAM differs')
        record_limit = 36 if mode=='direct' or classification<0 else 32
        if any(not base<=address<base+record_limit and not 0x2000000<=address<0x2010000
               for address in result['writes']):
            raise RuntimeError(f'{mode}/{classification}: unexpected write')
        if mode=='direct' and any(base<=address<base+32 for address in result['writes']):
            raise RuntimeError('Direct classification changed another record field')
        ranges = [(0x8adab,0x8adc7),(0x8b076,0x8b083)]
        if mode=='conversion':
            ranges += [(0x80ab3,0x80c0b),(0x8b0b9,0x8b1f9),(0x8ac45,0x8ac7e),
                       (0x8b230,0x8b24b),(0x84620,0x8467b)]
        if any(not any(start<=pc<end for start,end in ranges) for pc in result['trace']):
            raise RuntimeError(f'{mode}/{classification}: unlisted original path')
        for address,value in prescribed:
            if result['read'](address,len(value)) != value:
                raise RuntimeError(f'{mode}/{classification}: prescribed global changed')
        reports.append(dict(mode=mode,initial_words=words,final_words=normalized,
                            return_u32=expected,first_adjustment=18000,secondary_adjustment=3600))
    return dict(experiment='EXP-RNG-006',cases=reports,classification_name_empty=True,
                environment_array=None,stubs=[],ports=[],video_ram=False,draws=[],
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
        print('Isolated empty-name classification controls passed')
