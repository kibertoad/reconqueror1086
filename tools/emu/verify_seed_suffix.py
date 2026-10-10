"""Authored isolated suffix-record controls for FND-RNG-008."""
import argparse
import json
import os
import struct
from pathlib import Path
from function_call import call
from le_image import load_image


def verify(path):
    original, _ = load_image(path)
    # Neutral field offsets avoid assuming the uncompleted calendar reading.
    cases = [
        ('empty','',0,{28:0,32:-1}),
        ('numeric','60',2,{28:60,32:-1}),
        ('julian','J60',3,{28:60,32:1}),
        ('month','M3.2.0',6,{12:2,16:2,24:0,28:0,32:0}),
        ('month-only','M3',2,{16:2,28:0,32:0}),
        ('month-one-dot','M3.2',4,{12:2,16:2,28:0,32:0}),
        ('empty-month','M',1,{16:-1,28:0,32:0}),
        ('empty-dot-fields','M..',3,{12:0,16:-1,24:0,28:0,32:0}),
        ('prefix-overridden','JM3.2.0',7,{12:2,16:2,24:0,28:0,32:0}),
        ('clock','J60/1:2:3',9,{0:3,4:2,8:1,28:60,32:1}),
        ('hour-only','60/5',4,{8:5,28:60,32:-1}),
        ('empty-hour','60/',3,{8:0,28:60,32:-1}),
        ('empty-minute','60/5:',5,{8:5,28:60,32:-1}),
        ('empty-minute-seconds','60/5::7',7,{0:7,8:5,28:60,32:-1}),
        ('no-clock-sign','60/-1',3,{8:0,28:60,32:-1}),
        ('unbounded-clock','60/99:90:90',11,{0:90,4:90,8:99,28:60,32:-1}),
        ('unrecognized-prefix','q60',0,{28:0,32:-1}),
        ('remaining-comma','J60,tail',3,{28:60,32:1}),
        ('decimal-wrap','4294967296',10,{28:0,32:-1}),
    ]
    base, record = 0x5000000, 0x5000100
    initial = [101+i for i in range(9)]
    reports = []
    for name, text, consumed, fields in cases:
        memory = bytearray(4096)
        encoded = text.encode('ascii') + b'\0'
        memory[:len(encoded)] = encoded
        struct.pack_into('<9i',memory,256,*initial)
        wanted = bytearray(memory)
        for offset,value in {0:0,4:0,8:2,**fields}.items():
            struct.pack_into('<i',wanted,256+offset,value)
        result = call(original+[dict(base=base,size=4096,image=memory,flags=3)],
                      0x8b38c,(base,record),instruction_limit=3000)
        if result['read'](base,4096) != bytes(wanted) or result['result'] != base+consumed:
            raise RuntimeError(f'{name}: record or returned position differs')
        if any(not record <= a < record+36 and not 0x2000000 <= a < 0x2010000
               for a in result['writes']):
            raise RuntimeError(f'{name}: write outside record and stack')
        if any(not (0x8b24b <= pc < 0x8b275 or 0x8b38c <= pc < 0x8b492)
               for pc in result['trace']):
            raise RuntimeError(f'{name}: unexpected instruction path')
        reports.append(dict(case=name,input=text,initial_words=initial,
                            output_words=list(struct.unpack_from('<9i',wanted,256)),
                            consumed=consumed,remainder=text[consumed:]))
    return dict(experiment='EXP-RNG-003',cases=reports,stubs=[],ports=[],video_ram=False,
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
        print('Isolated seed-suffix record controls passed')
