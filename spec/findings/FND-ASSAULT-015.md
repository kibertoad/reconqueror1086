---
id: FND-ASSAULT-015
title: The actor thinker visits about a sixteenth of the combatants per call from a persistent cursor, and the state routine runs one test and one handler
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00050524..0x00050578
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004F49C
tool: Ghidra 12.1.3
environment: null
---

## Observation

Thinker `0x00050524`..`0x00050578` computes `(record_count - 10) >> 4`, where `record_count`
counts the combatant records including the ten templates, and visits that many records starting
at the index stored at `0x0009D5C4` (object 2 offset `0xD5C4`). It passes each live record to
state routine `0x0004F49C`, advances the index, wraps it past the last record, and stores it
back.

State routine `0x0004F49C` calls the current mode's test from the table at `0x0004F414`, calls
the actor kind's transition body through the dispatcher at `0x0004E41C`, calls helper
`0x0004E5F0` with the test's result, and then calls the handler of the resulting mode from the
table at `0x0004F458`. It does not call the new mode's test in the same call. For a combatant
whose death effect has finished, it removes the combatant and calls `0x0004D8C0`
(FND-ASSAULT-026).

## Interpretation

Each state decision is one test followed by the new mode's handler. A mode whose handler starts
no effect is tested again only on a later thinker call.

## Alternatives

None known.

## How to reproduce

Open `0x00050524`; the shift by 4 and the store back to `0x0009D5C4` bracket the loop. Follow
its call to `0x0004F49C`.
