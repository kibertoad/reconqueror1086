---
id: RULE-SAVE-002
title: Writing a saved game
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-SAVE-002, FND-SAVE-004, FND-PERSON-003, FND-TOURNEY-001]
conflicting: []
split_with: []
related: [RULE-RES-001, FMT-SAVE-001, FMT-SAVE-002, FMT-SAVE-003, FMT-SAVE-004, FMT-SAVE-005, FMT-SAVE-006, FMT-SAVE-007, FMT-TALK-008, FMT-PERSON-001]
---

## Summary

Saving writes each part of the game state to a loose file in the current directory, packs the
files with the title and the version `2.1` into `SAVEGAME\CONQn.SAV`, and deletes the loose files.
The random number generator's state is not saved.

## When it runs

When the player confirms a title on SCR-SAVE-002.

## Parameters

- `name`: the slot's file name, `CONQn.SAV`.
- `title`: the title, up to 40 characters.

## Inputs

The game state the component formats list.

## Procedure

```text
define save_game(name: char[], title: char[]):
    # the loose files TROOPS.SAV, PROPERTY.SAV, ~~1.SAV to ~~5.SAV and vtsave.vtb are deleted
    # troops.sav is written as FMT-SAVE-002; property.sav as FMT-SAVE-003, after which the
    # conversation variables are written to vtsave.vtb as FMT-TALK-008, loaded from ALL.VTB first
    # when no table is loaded
    # ~~1.SAV is the character table as FMT-PERSON-001
    # ~~2.SAV (FMT-SAVE-004), ~~3.SAV (FMT-SAVE-005) and ~~5.SAV (FMT-SAVE-007) are written only when
    # their blocks exist; ~~4.SAV is FMT-SAVE-006
    open_archive(sprintf("SAVEGAME\\%s", name), 1)
    add_entry(sprintf("TITLE"), 2, 0, 40, title)
    add_entry(sprintf("VERSION"), 2, 0, 4, sprintf("2.1"))
    # the files PROPERTY.SAV, vtsave.vtb, TROOPS.SAV and ~~1.SAV to ~~5.SAV are added in that
    # order, each as an entry named after the file with kind 2; a file that cannot be read stops
    # the game with fatal error 0
    # ictempmc.jp, ictempmf.jp, ictempmm.jp, ictempmt.jp and temp.jap are added when they exist
    close_archive(0)
    # the eight loose files deleted at the start are deleted again
```

## Outputs

`SAVEGAME\CONQn.SAV` in the layout of FMT-SAVE-001.

## Edge cases

- Saving while the calendar, estate or tournament block does not exist leaves out its file, and
  adding the missing file then stops the game.
- The `ictemp*.jp` files and `temp.jap` are left in the current directory.

## What the sources say

None of the sources describe the file layout.

## Differences between builds

None known.

## Open questions

- Whether the calendar, estate and tournament blocks always exist once a campaign runs.
