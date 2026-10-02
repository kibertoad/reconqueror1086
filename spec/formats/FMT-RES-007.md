---
id: FMT-RES-007
title: Counted record containers in the three CD-root .386 files
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:HMIDET.386", "CD:HMIDRV.386", "CD:HMIMDRV.386"]
byte_order: little
size: null
text: false
definition: fmt_res_007.ksy
evidence: [FND-RES-013]
conflicting: []
split_with: []
related: []
---

## Layout

The owned files share a 44-byte prefix followed by the prefix's counted records.
The following is stored framing, not the shipped loader's acceptance policy.
Fields with unknown semantics remain neutral names; payloads are opaque.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| 0 | 4 | UINT32 | driver_unk_00 | Stored value 51; role unknown. | supported | FND-RES-013 |
| 4 | 28 | BYTE[28] | driver_reserved | Stored zero region. | supported | FND-RES-013 |
| 32 | 4 | UINT32 | driver_record_count | Number of following records. | supported | FND-RES-013 |
| 36 | 4 | UINT32 | driver_records_offset | Stored value 44, where the record sequence begins. | supported | FND-RES-013 |
| 40 | 4 | UINT32 | driver_unk_28 | Stored word with unknown role. | supported | FND-RES-013 |

Record offsets are relative to each record's start:

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| 0 | 32 | BYTE[32] | driver_name_region | Zero-terminated printable ASCII prefix; retain remaining bytes, which need not be zero. | supported | FND-RES-013 |
| 32 | 4 | UINT32 | driver_unk_record_20 | Stored value equals driver_payload_length plus 92; runtime role unknown. | supported | FND-RES-013 |
| 36 | 4 | UINT32 | driver_payload_length | Byte count of the following payload. | supported | FND-RES-013 |
| 40 | 4 | UINT32 | driver_unk_record_28 | Stored word with unknown role. | supported | FND-RES-013 |
| 44 | 4 | UINT32 | driver_unk_record_2c | Stored word with unknown role. | supported | FND-RES-013 |
| 48 | driver_payload_length | BYTE[] | driver_payload | Opaque stored content. | supported | FND-RES-013 |

Advance by 48 plus driver_payload_length for each record. The counted sequence
ends exactly at the file boundary for every listed file. Bounded parsing must
reject a count that cannot fit the minimum record headers and a payload whose
length exceeds the remaining file; these are parser bounds, not established
original rejection behavior.

## Enumerations and flags

No semantic enumeration is established. FND-RES-013 records observed values of
driver_unk_record_2c; they are not named as modes or flags.

## Differences between builds

None known.

## Coverage

Every record and payload boundary in all three BLD-GOG-EN files was checked;
FND-RES-013 gives fingerprints and complete traversal results. No payload syntax,
executable code mapping, loader read or runtime use is established. This file-data
evidence supports framing but is not a complete reading of the consumers.

## Open questions

- What loader reads this container, selects records and handles malformed counts
  or lengths? File framing does not settle its acceptance policy. (Q-RES-124)
- Does name lookup stop at the first zero or compare the full stored region?
  Nonzero bytes after terminators rule out required zero padding but not either
  lookup behavior. (Q-RES-125)
- What does driver_unk_00 mean to the header reader? (Q-RES-126)
- What does driver_unk_28 mean to the header reader? (Q-RES-127)
- What does driver_unk_record_20 mean beyond its observed length relation?
  (Q-RES-128)
- How is driver_unk_record_28 used? (Q-RES-129)
- How is driver_unk_record_2c used? (Q-RES-130)
- What layouts do the payloads use, and do they differ between containers or
  records? Executable mapping cannot be inferred from filenames. (Q-RES-131)
