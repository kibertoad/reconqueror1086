"""Compare a local guest snapshot with the fingerprinted LE relocation model.

This diagnostic emits compact mapping facts, never guest bytes. Residual changes
remain unverified; its output is not permission to write game state.
"""
import argparse
import dataclasses
from collections import Counter
import json
from pathlib import Path
import struct
from emu.le_image import load_image
from live_mapping import validate_snapshot


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('snapshot', type=Path)
    parser.add_argument('--source', type=Path, default=Path('analysis/original/disc-root/CONQUER.EXE'))
    parser.add_argument('--observations', type=Path,
                        help='Coherent CPU/table observations from the snapshot run; require full map validation')
    args = parser.parse_args()
    objects, _ = load_image(args.source)
    memory = args.snapshot.read_bytes()
    if len(memory) != 16 * 1024 * 1024:
        raise ValueError('Expected the complete bounded 16 MiB probe snapshot')
    if args.observations:
        records = json.loads(args.observations.read_text())
        if not isinstance(records, list) or not records or not isinstance(records[-1].get('cpu'), str):
            raise ValueError('Missing coherent CPU diagnostic')
        mapping = validate_snapshot(memory, objects, records[-1]['cpu'])
        print(json.dumps({'mapping_status': 'validated for this snapshot',
                          'mapping': dataclasses.asdict(mapping)}, indent=2))
        return
    # FND-RNG-003: relative calls and state arithmetic form a local identity
    # control. The full relocation comparison below independently checks it.
    offset, end = 0x6b3f1 - objects[0]['base'], 0x6b413 - objects[0]['base']
    signature = bytes(objects[0]['image'][offset:end])
    match = memory.find(signature)
    if match < 0 or memory.find(signature, match + 1) >= 0:
        raise ValueError('RNG control must have exactly one snapshot match')
    code_base = match - offset
    if code_base < 0 or code_base + objects[0]['size'] > len(memory):
        raise ValueError('Candidate code object exceeds the snapshot')
    inferred = {}
    for target in (1, 2):
        candidates = Counter((struct.unpack_from('<I', memory, code_base + row['position'])[0]
                              - row['target_offset']) & 0xffffffff
                             for row in objects[0]['relocations'] if row['target'] == target)
        if len(candidates) != 1:
            raise ValueError('Relocation targets do not agree on one object base')
        inferred[target] = next(iter(candidates))
    if inferred[1] != code_base:
        raise ValueError('Code fingerprint and relocation targets disagree')
    summary = []
    for ordinal, obj in enumerate(objects, 1):
        base = inferred[ordinal]
        if base + obj['size'] > len(memory):
            raise ValueError('Mapped object exceeds snapshot')
        expected = bytearray(obj['image'])
        for row in obj['relocations']:
            struct.pack_into('<I', expected, row['position'],
                             (inferred[row['target']] + row['target_offset']) & 0xffffffff)
        actual = memory[base:base + obj['size']]
        differences = [i for i, (left, right) in enumerate(zip(expected, actual)) if left != right]
        summary.append({'object': ordinal, 'candidate_base': hex(base),
                        'size': obj['size'], 'residual_changed_bytes': len(differences),
                        'first_changed_offsets': [hex(i) for i in differences[:16]]})
    print(json.dumps({'mapping_status': 'candidate; residual writes require an audit',
                      'objects': summary}, indent=2))


if __name__ == '__main__':
    main()
