"""Tool smoke validation of RULE-RNG-001, not a complete-reading audit.

Original locations/calling conventions: FND-RNG-002 and FND-RNG-003.
Fixtures contain only rule parameters/results; instruction traces remain local.
"""
import argparse
import json
import os
from pathlib import Path
import random
import struct
from le_image import load_image
from function_call import call

ENTRIES = {'draw': 0x0006b3f1, 'seed_random': 0x0006b413,
           'random': 0x000445b4, 'random_inclusive': 0x00024c38, 'dice': 0x0004ce4c}
STATE = 0x0009e044


def verify(source):
    objects, relocations = load_image(source)
    chooser = random.Random(1086)
    seeds = [0, 1, 0x7fffffff, 0x80000000, 0xffffffff]
    seeds += [chooser.getrandbits(32) for _ in range(100)]
    fixtures, traces = [], []
    for seed in seeds:
        cases = [('draw', {}), ('seed_random', {'value': seed}),
                 ('random', {'n': 0}), ('random', {'n': 1}),
                 ('random', {'n': 131073}), ('random', {'n': 0x7fffffff}),
                 ('random_inclusive', {'n': 0}), ('random_inclusive', {'n': 100}),
                 ('dice', {'count': 0, 'sides': 6}),
                 ('dice', {'count': -1, 'sides': 6}),
                 ('dice', {'count': 3, 'sides': 6})]
        for name, parameters in cases:
            state, draws = seed, []

            def draw():
                nonlocal state
                state = (state * 0x41c64e6d + 0x3039) & 0xffffffff
                value = (state >> 16) & 0x7fff
                draws.append(value)
                return value

            if name == 'draw':
                expected = draw()
            elif name == 'seed_random':
                expected = None
                state = parameters['value']
            elif name == 'random':
                expected = ((draw() * parameters['n']) & 0xffffffff) >> 15
            elif name == 'random_inclusive':
                expected = draw() % (parameters['n'] + 1)
            else:
                expected = sum((((draw() * parameters['sides']) & 0xffffffff) >> 15) + 1
                               for _ in range(max(0, parameters['count']))) & 0xffffffff
            actual = call(objects, ENTRIES[name], arguments=tuple(parameters.values()),
                          writes=[(STATE, struct.pack('<I', seed))])
            actual_state = struct.unpack('<I', actual['read'](STATE, 4))[0]
            if actual_state != state or (expected is not None and actual['result'] != expected):
                raise RuntimeError(f'RNG mismatch: {name}, seed {seed}, parameters {parameters}')
            fixtures.append({'rule': 'RULE-RNG-001', 'operation': name, 'seed': seed,
                             'parameters': parameters, 'result': expected, 'rng_state': state})
            traces.append({'case': len(fixtures) - 1, 'instructions': actual['trace']})
    return {'build': 'BLD-GOG-EN', 'chooser_seed': 1086, 'relocations': relocations,
            'cases': fixtures}, traces


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    game_dir = os.environ.get('GAME_DIR')
    if not game_dir:
        print('Skipped: GAME_DIR is absent')
    else:
        # // needs: GAME_DIR
        source = Path(game_dir) / 'disc-root' / 'CONQUER.EXE'
        if not source.is_file():
            raise SystemExit('GAME_DIR must contain disc-root/CONQUER.EXE')
        report, trace = verify(source)
        args.output.mkdir(parents=True, exist_ok=True)
        (args.output / 'rng-results.json').write_text(json.dumps(report, indent=2))
        (args.output / 'rng-traces.json').write_text(json.dumps(trace))
        print(f'Passed {len(report["cases"])} function cases; no claim status changed')
