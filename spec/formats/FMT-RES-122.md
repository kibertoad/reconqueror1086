---
id: FMT-RES-122
title: Starting home fief, FIEF0.DAT
status: unknown
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-RES-060]
conflicting: []
split_with: []
related: []
---

## Layout

Text with CRLF line ends. A line whose first byte is a space, `#`, CR or LF
is skipped. The rest of the file is six sections in a fixed order. Each
section is one line that is not skipped (the shipped file starts each with
`@` and the column names), which is read and discarded, followed by one line
of up to 256 bytes holding decimal integers separated by spaces.

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| section 1 | `char[]` | `fief` | 3 values, read by `0x00029968` into the home fief record's dwords `+0x00`, `+0x10` and `+0x14`. Fewer values stop the game. | supported | FND-RES-060 |
| section 2 | `char[]` | `section_2` | 12 values; meaning not traced. | unknown | FND-RES-060 |
| section 3 | `char[]` | `section_3` | 7 values; meaning not traced. | unknown | FND-RES-060 |
| section 4 | `char[]` | `section_4` | 13 values; meaning not traced. | unknown | FND-RES-060 |
| section 5 | `char[]` | `section_5` | 10 values; meaning not traced. | unknown | FND-RES-060 |
| section 6 | `char[]` | `section_6` | 9 values; meaning not traced. | unknown | FND-RES-060 |

Sections 2 and 3 appear to be read by `0x00013520`, section 4 by `0x0005F480`,
section 5 by `0x0001C410` and section 6 by `0x000309D0`, going by the order
of the calls and the value counts.

The game writes the entry to a loose file named `FIEF0.DAT`, reads it, and
deletes it.

## Enumerations and flags

None known.

## Differences between builds

None known.

## Coverage

The one entry, `fief0.dat` in C1086.GOB, decodes to 1,752 bytes in 53 lines
with six sections of 3, 12, 7, 13, 10 and 9 values [FND-RES-060].

## Open questions

- Which values sections 2 to 6 hold, and which reader reads which section:
  the assignment above follows the order of the calls and the value counts
  only, and the format strings of two readers were not traced. (Q-RES-209)
