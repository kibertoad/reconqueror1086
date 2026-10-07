---
id: FMT-RES-118
title: SETUP.SOL directory record
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:SETUP.SOL"]
byte_order: little
size: null
text: false
definition: fmt_res_118.ksy
evidence: [FND-RES-030]
conflicting: []
split_with: []
related: []
---

## Layout

One record of FMT-RES-017's directory. SETUP reads the first 22 bytes, then
the last 4 only for kinds `1` and `8`, and sets them to 0 for any other kind
(FND-RES-030).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 3 | `char[3]` | `tag` | ASCII `DH9`; any other value ends the directory reading. | supported | FND-RES-030 |
| `0x03` | 1 | `char[1]` | `kind` | ASCII digit. `1` and `8` add the two words at `0x16`. Every shipped record has `1`. | supported | FND-RES-030 |
| `0x04` | 14 | `char[14]` | `name` | Member name, ASCII, NUL-terminated. `THE_END` marks the end of the directory and has no member. A name beginning `SOL_` is written out as `SETUPL.DLL` when its bytes 8 to 10 are `DLL`, and as `_SETUP.HLP` otherwise. | supported | FND-RES-030 |
| `0x12` | 4 | `UINT32LE` | `stored_size` | Length of the member's stored stream in the archive. | supported | FND-RES-030 |
| `0x16` | 2 if `kind` is `1` or `8` | `UINT16LE` | `unk_16` | When nonzero, passed with `unk_18` and the output file to a library routine of SETUP after the member is written. 0x1D37 to 0x1F5A in the shipped file. | supported | FND-RES-030 |
| | 2 if `kind` is `1` or `8` | `UINT16LE` | `unk_18` | Passed with `unk_16`. | supported | FND-RES-030 |
| | | | | Total size 26 for kinds `1` and `8`, 22 otherwise | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 16 records of the shipped `CD:SETUP.SOL` (FND-RES-030).

## Open questions

- Are `unk_16` and `unk_18` a DOS date and time that SETUP gives the written
  file? The values fit dates in 1994 and 1995, which is circumstantial; read
  the library routine at 0001:311E. (Q-RES-176)
- What do kinds other than `1` and `8` mean? Only `1` occurs in the shipped
  file, and SETUP tests only for `1` and `8`. (No item: no shipped record
  and no other reader in this build)
