---
id: FMT-PERSON-001
title: Character table, CHARACTR.DAT and saved copies
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-PERSON-002, FND-PERSON-003, FND-SAVE-002]
conflicting: []
split_with: []
related: [RULE-PERSON-002]
---

## Layout

The `CHARACTR.DAT` entry of `C1086.GOB`, and `~~1.SAV`, the character file a saved game writes
and stores in its container (FND-SAVE-002), are 8-bit text read a line at a time with `fgets` into a 256-byte buffer, so a longer line is read as more
than one. Some reads skip lines: they pass over any line whose first byte is a space, `#`, CR or
LF, so those lines are comments and blank lines. The other reads take the very next line. Numbers
are read with `atoi`, split on spaces, tabs and commas. Case matters in the markers. A missing
marker, a count that does not match, or the end of the file where a line is expected ends the
program with exit code 1. The game writes the same layout, with two empty lines before each
character block. The table below lists the lines in order; "skipping" marks the reads that skip
comments.

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| line 0, skipping | text | `counts_header` | Any line; its content is ignored. The game writes a heading naming the two counts. | supported | FND-PERSON-003 |
| line 1 | `INT32`, `INT32` | `character_count`, `attribute_count` | The number of characters and of attributes, read with `"%d %d"`. | supported | FND-PERSON-003 |
| line 2, skipping | marker `>` | `attributes_header` | Must start with `>`. | supported | FND-PERSON-003 |
| lines 3 on | `char[25]` each | `attribute_names` | One line per attribute: `!` and the name, read with `"%s"`. The list ends at the first line not starting with `!`, which is read and dropped, so it must not be the next marker. The count must equal `attribute_count`. | supported | FND-PERSON-002, FND-PERSON-003 |
| each character, skipping | marker `@` | `name_header` | Must start with `@`. | supported | FND-PERSON-003 |
| each character, next line | `char[80]` | `characters[r].name` | Up to 80 bytes of the line, copied with `strncpy`; an empty name when the line starts with `#`, a space, LF or CR. | supported | FND-PERSON-003 |
| each character, skipping | marker `>` | `values_header` | Must start with `>`. The game writes attribute abbreviations after it. | supported | FND-PERSON-003 |
| each character, next line | `INT32[attribute_count]` | `characters[r].values` | The attributes in the order of `attribute_names`. There must be at least `attribute_count` numbers; later ones are ignored. | supported | FND-PERSON-002, FND-PERSON-003 |

## Enumerations and flags

The attribute order is the field numbering of RULE-PERSON-001.

## Differences between builds

None known.

## Coverage

The `CHARACTR.DAT` entry of the GOG archive: 15 characters and 30 attributes [FND-PERSON-002].

## Open questions

- Whether anything removes the line end that `strncpy` copies into a name.
