---
id: RULE-RES-004
title: Archive paths and which archive is open
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-RES-006, FND-RES-002]
conflicting: []
split_with: []
related: [RULE-RES-001]
---

## Summary

At startup the game builds the paths of `C1086.GOB`, of an optional `C1086ad.GOB` and of the CD
directory from `CONQUER.INI`, and opens `C1086.GOB`. Any other archive is opened in its place and
`C1086.GOB` is opened again afterwards.

## When it runs

`set_archive_paths` and `open_master_archive` run once at startup; `read_from_data_archive` runs
when a picture, HAT layout, song or sound is loaded with mode 0.

## Parameters

None.

## Inputs

`fn_00062EA0`, which returns the value `CONQUER.INI` gives a key, or 0.

## Procedure

```text
define set_archive_paths():
    let data = fn_00062EA0(sprintf("DATA"))
    if data == 0:
        data = sprintf(".\\DATA\\")
    data_gob_path = sprintf("%sC1086ad.GOB", data)
    let cd = fn_00062EA0(sprintf("CD_PATH"))
    if cd == 0:
        cd = sprintf(".\\CD_PATH\\")
    cd_path = sprintf("%s", cd)
    let gob = fn_00062EA0(sprintf("GOB"))
    if gob == 0:
        gob = sprintf(".\\GOB\\")
    gob_path = sprintf("%sC1086.GOB", gob)

define open_master_archive():
    if open_archive(gob_path, 0) == -1:
        # the game stops with an error message
        return

define read_from_data_archive():
    close_archive(0)
    if open_archive(data_gob_path, 0) == -1:
        # the game stops with an error message
        return
    # the resource is read from the open archive
    close_archive(0)
    if open_archive(gob_path, 0) == -1:
        # the game stops with an error message
        return
```

## Outputs

`gob_path`, `data_gob_path` and `cd_path`, and the open archive.

## Edge cases

- The release has no `C1086ad.GOB`, so `read_from_data_archive` would stop the game.
- A scene archive is opened from `cd_path` in the same way, closing `C1086.GOB` first and opening it
  again after the scene. The scene loader takes the `.LOW` file in place of the `.RES` file only when
  the dword at `0x0009CB7C` is at least 1024, which only the scene writer sets (FND-RES-006).

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_00062EA0` returns for each key; `CONQUER.INI` is the CONFIG area's.
- Whether any call loads a picture, layout, song or sound with mode 0.
- Whether the shipped game ever reads a `.LOW` file.
