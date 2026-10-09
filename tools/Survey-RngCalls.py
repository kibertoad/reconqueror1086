"""Local-only direct-call leads for RNG ownership; not a complete caller audit.

FND-RNG-002/003 identify the target functions. The inventory bounds one decoder
search; a boundary-independent E8 scan provides a separate comparison. None of these searches establishes absence of computed, far or interrupt-driven callers.
"""
import argparse
from collections import Counter
import csv
import json
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_32
from emu.le_image import load_image
from call_survey import walk_calls


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args()
    destination = args.output.resolve()
    roots = [Path('analysis/original').resolve(), Path('artifacts').resolve()]
    if not any(destination.is_relative_to(root) for root in roots) or destination.exists():
        raise ValueError('Use a fresh local-only output under artifacts or analysis/original')
    objects, _ = load_image('analysis/original/disc-root/CONQUER.EXE')
    code, base = bytes(objects[0]['image']), objects[0]['base']
    targets = {0x6b3f1: 'draw', 0x6b413: 'seed', 0x445b4: 'scaled',
               0x41110: 'scaled-copy', 0x24c38: 'inclusive', 0x4ce4c: 'dice'}
    raw = {}
    for position in range(len(code) - 4):
        if code[position] != 232:
            continue
        target = (base + position + 5 + int.from_bytes(code[position + 1:position + 5], 'little', signed=True)) & 0xffffffff
        if target in targets:
            raw[base + position] = targets[target]
    decoded, flowed, unresolved, body_spans = {}, {}, [], []
    decoder = Cs(CS_ARCH_X86, CS_MODE_32)
    with Path('coverage/BLD-GOG-EN/@CD/CONQUER.EXE.tsv').open() as stream:
        for row in csv.DictReader(stream, delimiter='\t'):
            spans = [tuple(int(value, 16) for value in span.split('..'))
                     for span in row['ranges'].split()]
            body_spans.extend(spans)
            calls, issues = walk_calls(code, base, int(row['start'], 16), spans, targets)
            for address, target in calls.items():
                found = flowed.setdefault(address, {'target': target, 'functions': []})
                found['functions'].append(row['start'])
            unresolved.extend({'function': row['start'], 'address': hex(issue['address']),
                               'reason': issue['reason']} for issue in issues)
            for span in row['ranges'].split():
                start, end = (int(value, 16) for value in span.split('..'))
                if not base <= start < end <= base + len(code):
                    raise ValueError('Inventory body range exceeds code object')
                for instruction in decoder.disasm(code[start - base:end - base], start):
                    if instruction.mnemonic != 'call' or not instruction.op_str.startswith('0x'):
                        continue
                    target = int(instruction.op_str, 16)
                    if target in targets:
                        previous = decoded.setdefault(instruction.address, {'target': targets[target], 'functions': []})
                        if row['start'] not in previous['functions']:
                            previous['functions'].append(row['start'])
    control = raw.get(0x24c38) == 'draw' and decoded.get(0x24c38, {}).get('target') == 'draw' and flowed.get(0x24c38, {}).get('target') == 'draw'
    if not control:
        raise ValueError('Known helper-to-draw call control was not found by all three searches')
    report = {'status': 'ownership leads; no absence or completeness claim',
              'searched': ['all code-object byte offsets for E8 rel32', 'linear instruction starts from inventory body ranges',
                           'direct control-flow traversal from function entries within body ranges'],
              'not_searched': ['indirect calls', 'far calls', 'register-held or table-held targets', 'interrupt dispatch'],
              'positive_control': 'inclusive helper direct call to draw',
              'raw_calls': [{'address': hex(address), 'target': target,
                             'in_inventory_body': any(a <= address < b for a, b in body_spans),
                             'flow_reached': address in flowed}
                            for address, target in sorted(raw.items())],
              'raw_counts': dict(Counter(raw.values())),
              'decoded_counts': dict(Counter(row['target'] for row in decoded.values())),
              'flow_counts': dict(Counter(row['target'] for row in flowed.values())),
              'flow_calls': [{'address': hex(address), **row} for address, row in sorted(flowed.items())],
              'unresolved_flow': unresolved,
              'unreached_raw_leads': [{'address': hex(address), 'target': target}
                                      for address, target in sorted(raw.items()) if address not in flowed],
              'calls': [{'address': hex(address), **row} for address, row in sorted(decoded.items())],
              'undecoded_raw_leads': [{'address': hex(address), 'target': target}
                                      for address, target in sorted(raw.items()) if address not in decoded]}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open('x') as stream:
        json.dump(report, stream, indent=2)
    print(json.dumps({key: report[key] for key in ('status', 'raw_counts', 'decoded_counts', 'flow_counts')}))


if __name__ == '__main__':
    main()
