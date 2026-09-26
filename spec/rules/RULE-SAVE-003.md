---
id: RULE-SAVE-003
title: Reading a saved game
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-SAVE-003, FND-SAVE-004, FND-PERSON-003]
conflicting: []
split_with: []
related: [RULE-RES-001, FMT-SAVE-001, FMT-SAVE-002, FMT-SAVE-003, FMT-SAVE-004, FMT-SAVE-005, FMT-SAVE-006, FMT-SAVE-007, FMT-TALK-008, FMT-PERSON-001]
---

## Summary

Loading refuses a file whose `VERSION` is not `2.1`, unpacks the entries to loose files, reads
each part back in the order the save wrote them and deletes the loose files. A missing or unreadable
part ends the program.

## When it runs

When the player picks a slot on SCR-SAVE-001.

## Parameters

- `name`: the slot's file name, `CONQn.SAV`.

## Inputs

`SAVEGAME\CONQn.SAV` and the files it unpacks.

## Procedure

```text
define load_game(name: char[]) -> INT32:
    # the eight loose files are deleted; a file that cannot be opened gives 0
    let archive = read_file(sprintf("SAVEGAME\\%s", name), FMT-SAVE-001)
    if archive.VERSION != sprintf("2.1"):
        # C1086.GOB is opened again and the revision message shown
        return 0
    # TROOPS.SAV, vtsave.vtb, PROPERTY.SAV, ~~1.SAV to ~~5.SAV and temp.jap are written back as
    # loose files, and the four ictemp*.jp files when present; a missing entry among the others
    # stops the game with fatal error 0
    # the parts are read in the save's order: FMT-SAVE-002, FMT-SAVE-003 with vtsave.vtb, the
    # character table, FMT-SAVE-004, FMT-SAVE-005 (every chain comes back in reverse order),
    # FMT-SAVE-006 and FMT-SAVE-007
    # the eight loose files are deleted
    return 1
```

## Outputs

1 when the game was loaded, 0 when the file is missing or of another revision.

## Edge cases

- A save without `temp.jap` ends the program (BUG-SAVE-001).
- `rng_state`, `tournament_previous_site` and `tournament_here` keep the values the session had.
- Each load reverses the chains of the home fief's lists.
- A failed read of `VERSION` gives 0 with the save still open as the archive.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

None.
