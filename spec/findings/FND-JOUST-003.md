---
id: FND-JOUST-003
title: The practice lance frame comes from the first row threshold at or above the lance and a 70-pixel column
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041379..0x000413A9
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041632..0x0004169E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009B838..0x0009B84B
tool: Ghidra 12.1.3
environment: null
---

## Observation

At `0x00041379`..`0x000413A9` the worker sets the column width to `(400 - 50 + 2) / 5`, 70.
At `0x00041632`..`0x0004169E` it scans the dwords from `0x0009B838`, which are 150, 98, 46, 24
and 2, counting until it reaches one that is not greater than the lance y, with no limit on the
count. It then takes `5 * count - 1 - (x - 50) / 70`, raised to 0 when negative and lowered to
24 when above.

## Interpretation

The 25 frames of `lance1.csf` are five rows of five, and the row is picked by height and the
frame within it by the lance's horizontal position, right to left. The last threshold, 2, is
below the lowest possible lance y, so the scan always ends inside the table.

## Alternatives

None known.

## How to reproduce

Read five dwords at `0x0009B838` and open `0x00041632`.
