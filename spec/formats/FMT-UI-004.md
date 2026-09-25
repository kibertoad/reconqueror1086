---
id: FMT-UI-004
title: Store catalog, WEAPONS.DAT
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-UI-007]
conflicting: []
split_with: []
related: [RULE-UI-003]
---

## Layout

The `weapons.dat` entry of `C1086.GOB`, opened with mode `rt`. Each record is six lines: two read
with `fgets` into a 600-byte buffer and cut at the first LF, and four numbers read with
`fscanf(file, "%d\n")`, which also skips the line end. The game reads records 0 to 38 and ignores
anything after them. A malformed number is not detected.

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| line 0 | `char[]` | `movie` | The movie shown for the item, or `#` for none. | supported | FND-UI-007 |
| line 1 | `INT32` | `chance` | The chance, out of 21, that the record appears in a visit's stock. | supported | FND-UI-007 |
| line 2 | `INT32` | `frame` | The frame of `SWORDS.CSF` drawn for the item. | supported | FND-UI-007 |
| line 3 | `INT32` | `item` | The item, an index into `item_counts`. | supported | FND-UI-007 |
| line 4 | `INT32` | `price` | The price in shillings. | supported | FND-UI-007 |
| line 5 | `char[]` | `description` | The description shown for the item. | supported | FND-UI-007 |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The GOG archive: 40 records and an empty last line. Record 39 repeats record 37's frame and item and
is never read [FND-UI-007].

## Open questions

- Whether the game reads the descriptions of the owned items for anything besides display.
