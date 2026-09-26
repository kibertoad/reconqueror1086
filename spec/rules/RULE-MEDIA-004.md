---
id: RULE-MEDIA-004
title: Loading a skirmish screen from SKIRMISH.RES
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-MEDIA-007, FND-MEDIA-005, FND-RES-006]
conflicting: []
split_with: []
related: [FMT-MEDIA-004, FMT-MEDIA-005, FMT-RES-002, RULE-RES-001, RULE-MEDIA-003]
---

## Summary

A skirmish screen is read as raw pixels from `SKIRMISH.RES`, with its palette from the `.PAL` entry
of the same name. When the entry is missing the game decodes the PCX file of that name from disk and
adds the pixels and palette to the archive.

## When it runs

When the castle skirmish shows `EXIT.PCX`, `STAIRS.PCX`, `BRAWL.PCX`, `SKIRMCUR.PCX`,
`SKIRMISH.PCX` or `HELP.PCX`.

## Parameters

- `name`: the picture name.
- `a`, `b`: passed to `fn_00040CE4` only.
- `buffer`: where the pixels go.
- `palette`: where the colours go, or 0.

## Inputs

`cd_path`.

## Procedure

```text
define load_skirmish_picture(name: char[], a: INT32, b: INT32, buffer: UINT8[], palette: UINT8[]):
    let path = sprintf("%sSKIRMISH.RES", cd_path)
    if open_archive(path, 0) == -1:
        return
    let e = find_entry(name)
    if read_entry(e, buffer) == 0:
        close_archive(0)
        fn_00040CE4(name, a, b, buffer, palette)
        if count(buffer) == 0:
            # the game stops with "Can't read file <name>! (PCX)"
            return
        if open_archive(path, 0) < 0:
            return
        add_entry(name, STORAGE_BLOCKS, 0, count(buffer), buffer)
        if palette != 0:
            add_entry(pal_name(name), STORAGE_BLOCKS, 0, 768, palette)
    else if palette != 0:
        if read_entry(find_entry(pal_name(name)), palette) == 0:
            # the game stops with "Can't find <name>.PAL in RES file.!"
            return
    close_archive(0)

define pal_name(name: char[]) -> char[]:
    # the name up to its first "." with ".PAL" appended
    return sprintf("%s.PAL", name)
```

## Outputs

The pixels, 64,000 bytes for a 320 by 200 screen (FMT-MEDIA-005), and the 768-byte palette
(FMT-MEDIA-004). A missing entry adds entries to `SKIRMISH.RES`.

## Edge cases

- The archive is opened read-only first and for update when adding, modes `rb` and `r+b`.
- The shipped archive holds every screen the game asks for, so the PCX path does not run with the
  released files.

## What the sources say

The executable (FND-MEDIA-007).

## Differences between builds

None known.

## Open questions

- What `a` and `b` mean to `fn_00040CE4`.
- What loads `MELEE2.PCX`, which no caller of this routine names.
