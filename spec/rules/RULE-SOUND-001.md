---
id: RULE-SOUND-001
title: Loading and freeing a sound bank
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-SOUND-002, FND-SOUND-004, FND-RES-006]
conflicting: []
split_with: []
related: [RULE-RES-001, RULE-RES-004, RULE-SOUND-002]
---

## Summary

A screen loads the `.666` banks it plays from the open archive by directory index into DOS memory,
or ordinary memory when that runs out. Nothing is loaded while `SOUND_EFFECTS` is off.

## When it runs

When a screen or scene starts: 33 places, 22 banks of `C1086.GOB` by constant index and entry 0 of
`SKIRMISH.RES`. The screen frees them when it ends.

## Parameters

- `index`: the directory index of the bank.
- `from_open`: 1 to read from the open archive; every caller passes 1.

## Inputs

`sound_effects_on`; the open archive.

## Procedure

```text
define load_sound_bank(index: UINT32, from_open: INT32) -> sound_bank:
    if sound_effects_on == 0:
        return 0
    if from_open != 1:
        read_from_data_archive()
    let e = entry_at(index)
    if e == 0:
        # the game stops with "Sound File not found in Resource file. Aborting."
        return 0
    let b: sound_bank
    b.data = fn_0007AB20((e.expanded_size + 15) / 16)
    if b.data != 0:
        b.dos = 1
    else:
        # e.expanded_size bytes of ordinary memory are allocated into b.data
        b.dos = 0
    read_entry(e, b.data)
    return b

define free_sound_bank(b: sound_bank):
    if b == 0:
        return
    # the data is freed from DOS memory when b.dos is 1, otherwise from ordinary memory
```

## Outputs

A `sound_bank`, or 0.

## Edge cases

- With `SOUND_EFFECTS` off, callers keep 0 and `play_sample` never reads it.
- A failed read stops the game with `Unable to read MouseSoundOpen sounds`.

## What the sources say

The executable (FND-SOUND-002).

## Differences between builds

None known.

## Open questions

- `fn_0007AB20` allocates DOS memory in 16-byte paragraphs and returns 0 when there is none; its
  inside was not traced.
