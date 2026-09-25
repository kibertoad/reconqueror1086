---
id: FND-JOUST-005
title: A practice pass hits when 90 less the player points beats both error totals; otherwise random(100) decides whether the opponent scores
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041CEC..0x00041D46
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041A97..0x00041B34
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041110
tool: Ghidra 12.1.3
environment: null
---

## Observation

Caller `0x00041CEC`..`0x00041D46` sets local player points to 20 and opponent points to 50 and
passes pointers to them and to a result to the worker. At `0x00041A97`..`0x00041AD9`, after the
movie, the worker compares `90 - player_points` with both error totals; when it is greater than
each, it sets the result to 1 and adds 2 to the player points. Otherwise, at
`0x00041ADB`..`0x00041B34`, it calls `0x00041110` with 100 and compares the value with
`opponent_points - player_points + 50`: below it sets the result to 0 and adds 2 to the opponent
points, otherwise it sets the result to 2. The caller does not store the points anywhere else.

## Interpretation

With the practice values a pass hits when both error totals are below 70. A miss leaves an 80
in 100 chance that the opponent unseats the player (result 0) and otherwise both miss
(result 2). Practice does not change any tournament score.

## Alternatives

None known.

## How to reproduce

Open `0x00041A97`; the constant `5Ah` and the call with `64h` are there.
