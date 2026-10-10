"""Isolated authored cases for FND-RNG-007; no DOS environment is modeled."""
import argparse
import json
import os
import struct
from pathlib import Path
from function_call import call
from le_image import load_image


def verify(path):
    original, _ = load_image(path)
    # Authored ASCII strings, expected consumed positions and wrapping values.
    # FND-RNG-007 locates the parser and its only called numeric helper.
    cases = [
        ('empty', '', '', None, 0),
        ('leading-colon', ':ABC8', 'ABC', 28800, 5),
        ('colon-only', ':', '', None, 1),
        ('comma', ',tail', '', None, 0),
        ('name-only', 'ABC', 'ABC', None, 3),
        ('minus-without-number', 'ABC-', 'ABC', None, 4),
        ('plus-without-number', 'ABC+q', 'ABC', None, 4),
        ('hours', 'ABC8', 'ABC', 28800, 4),
        ('positive-sign', 'ABC+8', 'ABC', 28800, 5),
        ('negative-hms', ':ABC-5:30:15', 'ABC', -19815, 12),
        ('empty-minutes', 'ABC8:', 'ABC', 28800, 5),
        ('empty-minutes-with-seconds', 'ABC8::7', 'ABC', 28807, 7),
        ('unbounded-minute-second', 'ABC8:90:90', 'ABC', 34290, 10),
        ('colon-in-name', 'ABC:8', 'ABC:', 28800, 5),
        ('remaining-comma', 'ABC8,DEF', 'ABC', 28800, 4),
        ('short-name', 'X9', 'X', 32400, 2),
        ('word-plus-tail', 'ABCDE9', 'ABCDE', 32400, 6),
        ('low-name-bytes', '\t!A8', '\t!A', 28800, 4),
        ('high-name-bytes', 'z_~8', 'z_~', 28800, 4),
        ('name-at-bound', 'X'*30+'9', 'X'*30, 32400, 31),
        ('name-past-bound', 'X'*31+'9', 'X'*30, 32400, 32),
        ('long-name', 'X'*40+'9', 'X'*30, 32400, 41),
        ('decimal-wrap', 'ABC4294967296', 'ABC', 0, 13),
        ('arithmetic-wrap', 'ABC'+'9'*27, 'ABC', 2147480048, 30),
    ]
    base, source, destination, adjustment = 0x5000000, 0x5000000, 0x5000100, 0x5000200
    reports = []
    for name, text, copied, expected, consumed in cases:
        memory = bytearray(4096)
        encoded = text.encode('ascii') + b'\0'
        memory[:len(encoded)] = encoded
        memory[256:288] = b'?'*32
        initial = 123456
        struct.pack_into('<i', memory, 512, initial)
        objects = original + [dict(base=base,size=4096,image=memory,flags=3)]
        result = call(objects,0x8b275,(source,destination,adjustment),instruction_limit=3000)
        wanted = bytearray(memory)
        output = copied.encode('ascii') + b'\0'
        wanted[256:256+len(output)] = output
        observed_adjustment = initial if expected is None else expected
        struct.pack_into('<i', wanted,512,observed_adjustment)
        if result['read'](base,4096) != bytes(wanted) or result['result'] != source+consumed:
            raise RuntimeError(f'{name}: output or returned remainder differs')
        allowed = set(range(destination,destination+len(output))) | set(range(adjustment,adjustment+4))
        if any(a not in allowed and not 0x2000000 <= a < 0x2010000 for a in result['writes']):
            raise RuntimeError(f'{name}: write outside prescribed outputs and stack')
        if any(not 0x8b24b <= pc < 0x8b38c for pc in result['trace']):
            raise RuntimeError(f'{name}: instruction outside parser and numeric helper')
        reports.append(dict(case=name,input=text,name=copied,initial_adjustment=initial,
                            adjustment=observed_adjustment,consumed=consumed,
                            remainder=text[consumed:]))
    return dict(experiment='EXP-RNG-002',cases=reports,stubs=[],ports=[],video_ram=False,
                segment_model='authored flat 32-bit CS/DS/ES/SS')


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
        print('Isolated seed-adjustment parser controls passed')
