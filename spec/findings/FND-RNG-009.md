---
id: FND-RNG-009
title: Seed conversion uses common-year and leap-year cumulative month-day tables
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    kind: file-data
    offset: 0x000D35FC..0x000D3616
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    kind: file-data
    offset: 0x000D3616..0x000D3630
tool: Fingerprinted LE object/page mapping and independently decoded signed 16-bit values
environment: null
---

## Observation

The two listed file-data ranges each hold thirteen signed sixteen-bit values.
The LE page map places them at `0x0009A3A8` and `0x0009A3C2`, respectively.
FND-RNG-005 reads the converter's selection between those bases: the Gregorian
leap predicate is applied to the normalized year plus 1900, and a nonzero
result selects the second table. The normalized month selects a signed word.

| Month boundary | Common-year cumulative days | Leap-year cumulative days |
|---|---|---|
| January | 0 | 0 |
| February | 31 | 31 |
| March | 59 | 60 |
| April | 90 | 91 |
| May | 120 | 121 |
| June | 151 | 152 |
| July | 181 | 182 |
| August | 212 | 213 |
| September | 243 | 244 |
| October | 273 | 274 |
| November | 304 | 305 |
| December | 334 | 335 |
| Following January | 365 | 366 |

These are compact calendar quantities, not a retained byte dump. The boundary
names describe the cumulative values and their zero-based month selection in
the converter; they are independently authored labels.

The file locations were calculated from the data object's relative offsets,
logical page numbers and physical-page entries, independently of instruction
decoding. As a positive control, the same calculation mapped the `TZ` key
already recorded in FND-RNG-006 to its recorded file offset and matched its
two bytes. The source identity was checked before reading either table.

## Interpretation

The month-table quantities required by the partial converter reading are
accounted for. They do not by themselves establish how the full converter
handles distant years, adjustment initialization, classification or rejection
paths; Q-RNG-001 remains open.

## Alternatives

Treating the bases as byte-indexed or thirty-two-bit-element tables is ruled
out by the converter's doubled month index and signed-word load. A single
unchanging month table is ruled out by both the selected bases and the
different cumulative values after February. Full timestamp semantics still
require the converter and helper readings beyond this data mapping.

## How to reproduce

Verify the BLD-GOG-EN fingerprint, then map each listed file-data range through
the shipped LE object and page tables. Read thirteen little-endian signed words
at each location. Repeat the mapping for FND-RNG-006's key as the positive
control, and follow the normalized month and leap result in FND-RNG-005's
converter before using the quantities in a seed-source description.
