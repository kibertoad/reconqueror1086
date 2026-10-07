---
id: FMT-RES-017
title: SETUP.SOL, the first-stage installer's member archive
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:SETUP.SOL"]
byte_order: little
size: null
text: false
definition: fmt_res_017.ksy
evidence: [FND-RES-030]
conflicting: []
split_with: []
related: [RULE-RES-005]
---

## Layout

A whole archive file. `SETUP.EXE` reads the header and the directory once,
then walks the directory in order, skipping the stored bytes of each member
it does not want and expanding the ones it does (FND-RES-030).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 6 | `BYTE[6]` | `header` | Read and never tested. ASCII `DISK01` in the shipped file. | supported | FND-RES-030 |
| `0x06` | `directory_size` | `FMT-RES-118[]` | `directory` | Records up to and including the first whose `name` is `THE_END`. A record whose `tag` is not `DH9` ends the reading, and SETUP then copies the whole file through LZEXPAND instead. | supported | FND-RES-030 |
| | sum of `stored_size` | `BYTE[]` | `members` | Each member's stored stream, packed in directory order from the end of the directory, with no gaps; its position is the sum of the `stored_size` values before it. Expanded as RULE-RES-005 gives. | supported | FND-RES-030 |
| | | | | Total size `6 + directory_size` plus the sum of `stored_size` | | |

`directory_size` is 26 bytes for each record of kind `1` or `8` and 22 for
any other, `THE_END` included.

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The shipped `CD:SETUP.SOL` of BLD-GOG-EN: 16 records, all kind `1`, ending
with `THE_END` at offset 396, and the stored sizes of the 15 members sum to
exactly the 800,418 bytes after the directory (FND-RES-030).

## Open questions

- What are the members' streams? RULE-RES-005 lists the readings.
  (Q-RES-175)
