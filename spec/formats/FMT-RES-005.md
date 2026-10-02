---
id: FMT-RES-005
title: Raw disc carrier with cue-delimited data and audio spans
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["game.gog"]
byte_order: little
size: null
text: false
definition: fmt_res_005.ksy
evidence: [FND-RES-011, FND-RES-012]
conflicting: []
split_with: []
related: []
---

## Layout

The carrier has no global header that distinguishes its spans. Its paired cue
sheet FMT-RES-006 supplies the first audio start; the leading span consists of
that many 2,352-byte records. The owned value of `num_disc_data_records` is
244445. This count is an external format parameter, not a field stored in the
carrier. The remaining bytes form the cue-labelled audio span. Audio samples
and the embedded ISO structures need separate format descriptions.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0` | `num_disc_data_records * 2352` | `FMT-RES-116[num_disc_data_records]` | `disc_data_records` | Leading raw records up to the first audio start from the paired cue; includes the observed padding exceptions. | supported | FND-RES-011, FND-RES-012 |
| `num_disc_data_records * 2352` | `file_size - num_disc_data_records * 2352` | `BYTE[file_size - num_disc_data_records * 2352]` | `disc_audio_bytes` | Cue-labelled audio bytes through the file end; the cue divides this span into five tracks. No sample encoding is asserted here. | supported | FND-RES-011, FND-RES-012 |
| | | | | Total size variable | | |

The owned cue boundary is raw offset `0x2244CE70`; the carrier ends at
`0x27C67110`. Its leading span extends beyond the ISO descriptor's declared
volume. Neither span identification nor physical indexing requires every
leading record's stored address to be unique or every prefix to be nonzero
[FND-RES-012]. The definition takes `num_disc_data_records` from the paired
cue and describes storage only; it makes no claim about a wrapper's validations.

## Enumerations and flags

None. Record mode values belong to FMT-RES-116.

## Differences between builds

None known.

## Coverage

The owned carrier's complete leading span was inspected, including the duplicate
and complete-zero runs. Its cue boundary and total byte size agree with the
record cardinalities [FND-RES-011, FND-RES-012]. The ISO directory extents fit
inside the smaller declared volume. The wrapper consumer has not been completely
read, and audio sample or ISO member layouts are outside this entry's coverage.

## Open questions

- Does the shipped wrapper reject entirely-zero records before the cue's first
  audio start, or keep them as part of the stored data span? (Q-RES-120)
- What is the audio sample representation in the cue-labelled audio span?
  (Q-RES-121)
