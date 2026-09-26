---
id: FMT-SAVE-001
title: Saved game, SAVEGAME\CONQn.SAV
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: null
text: false
definition: fmt_save_001.ksy
evidence: [FND-SAVE-001, FND-SAVE-002, FND-SAVE-003, FND-SAVE-004]
conflicting: []
split_with: []
related: [RULE-SAVE-002, RULE-SAVE-003]
---

## Layout

A resource container in the layout of FMT-RES-001, written as a new file. Every entry has kind 2
(the kind-2 coder, stored plain when that does not shrink it) and 0 in field `+0x24`. The entries are
added in the order below; the directory is written unsorted. Each file entry holds the whole file of
that name, and the load writes it back to the current directory before reading it.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 8 | `BYTE[8]` | `header` | The container header of FMT-RES-001. | supported | FND-SAVE-002 |
| | 40 | `char[40]` | `TITLE` | Entry: the slot title in CP437, NUL-padded after a shorter title and not terminated after 40 characters. | supported | FND-SAVE-001, FND-SAVE-002 |
| | 4 | `char[4]` | `VERSION` | Entry: `2.1` and NUL; any other value is refused. | supported | FND-SAVE-002, FND-SAVE-003 |
| | 2638 | `FMT-SAVE-003` | `PROPERTY.SAV` | Entry. | supported | FND-SAVE-002, FND-SAVE-004 |
| | `12 + count * element_size` | `FMT-TALK-008` | `vtsave.vtb` | Entry: the conversation variables. | supported | FND-SAVE-002, FND-SAVE-004 |
| | variable | `FMT-SAVE-002` | `TROOPS.SAV` | Entry. | supported | FND-SAVE-002, FND-SAVE-004 |
| | variable | `FMT-PERSON-001` | `~~1.SAV` | Entry: the character table. | supported | FND-SAVE-002 |
| | 64 | `FMT-SAVE-004` | `~~2.SAV` | Entry. | supported | FND-SAVE-002, FND-SAVE-004 |
| | variable | `FMT-SAVE-005` | `~~3.SAV` | Entry. | supported | FND-SAVE-002, FND-SAVE-004 |
| | 3200 | `FMT-SAVE-006` | `~~4.SAV` | Entry. | supported | FND-SAVE-002, FND-SAVE-004 |
| | 24 | `FMT-SAVE-007` | `~~5.SAV` | Entry. | supported | FND-SAVE-002, FND-SAVE-004 |
| | variable | `BYTE[]` | `ictempmc.jp`, `ictempmf.jp`, `ictempmm.jp`, `ictempmt.jp` | Entries present only when the file existed when saving. | supported | FND-SAVE-002 |
| | variable | `BYTE[]` | `temp.jap` | Entry present only when the file existed when saving; the load requires it (BUG-SAVE-001). | supported | FND-SAVE-002, FND-SAVE-003 |
| | | | | Total size variable | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Not checked against a save from the original; the layout comes from the code that writes and reads it.

## Open questions

- What the `ictemp*.jp` files and `temp.jap` hold.
