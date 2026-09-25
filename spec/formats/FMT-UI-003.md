---
id: FMT-UI-003
title: Exterior catalogs, VILLAGE.DAT and TVILLAGE.DAT
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-UI-004, FND-UI-006]
conflicting: []
split_with: []
related: [RULE-UI-001, RULE-UI-002]
---

## Layout

The `village.dat` and `tvillage.dat` entries of `C1086.GOB`, opened with mode `rt`, so CRLF reads as
LF. The game reads records 0 to the wanted index in order; each record is seven lines read with
`fgets` into an 80-byte buffer and cut at the first `;` and the first LF, so a `;` starts a comment.
There are no markers or blank lines. The numbers of a row are read with `"%d,%d,%d,%d,%d"`: matching
stops at the first character that does not fit, and the fields after it keep the values the same
row of the previous record left. The six rows are, in order, the tournament, the map, the inn, the
forge, the lender and the church (RULE-UI-002). Case does not matter in the background name, which the game looks
up in the archive. A missing record is not detected.

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| line 0 | `char[15]` | `background` | The background picture; the game copies 15 bytes. | supported | FND-UI-004, FND-UI-006 |
| lines 1 to 6, field 0 | `INT32` | `rows[i].enabled` | 1 to give region `i` of the exterior this row's rectangle, 0 to turn region `i` off. | supported | FND-UI-004, FND-UI-006 |
| lines 1 to 6, field 1 | `INT32` | `rows[i].x` | Left edge. | supported | FND-UI-004, FND-UI-006 |
| lines 1 to 6, field 2 | `INT32` | `rows[i].y` | Top edge. | supported | FND-UI-004, FND-UI-006 |
| lines 1 to 6, field 3 | `INT32` | `rows[i].width` | Width. | supported | FND-UI-004, FND-UI-006 |
| lines 1 to 6, field 4 | `INT32` | `rows[i].height` | Height. | supported | FND-UI-004, FND-UI-006 |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The GOG archive: 67 records in `village.dat` and 12 in `tvillage.dat`. Three rows of `village.dat`
differ from the form: record 16 row 1 and record 37 row 4 stop after two numbers, and record 18 row
3 has a trailing comma (BUG-UI-001). Thirteen `village.dat` backgrounds and one `tvillage.dat`
background are absent from the archive; no person record selects those records [FND-UI-006].

## Open questions

- What the four digits after `_` in a background name mean.
