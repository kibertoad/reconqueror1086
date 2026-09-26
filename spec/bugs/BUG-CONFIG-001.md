---
id: BUG-CONFIG-001
title: A CONQUER.INI without DELAYVGA makes the game compare memory at address 0 with ON
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: presentation
intent: unintended
player_reliance: not-relied-on
evidence: [FND-CONFIG-003, FND-CONFIG-002]
conflicting: []
split_with: []
related: [RULE-CONFIG-003, RULE-MEDIA-005]
---

## Symptom

None visible in normal play: the extra display reset `DELAYVGA` asks for is almost never made.

## Trigger conditions

The loaded `CONQUER.INI` has no `DELAYVGA` line, and a full-screen movie ends or the first-person view closes.

## Mechanism

`ini_value` returns 0 for a missing key. The two `DELAYVGA` readers pass that 0 straight to `strcmp`
with `ON`, so the comparison reads bytes from linear address 0, the real-mode interrupt table under
DOS/4GW. Every other reader of a switch tests for 0 first.

## Frequency

Every time either reader runs with the key missing. Both shipped files have the key.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Whether a DOS extender that does not map address 0 would stop the game here.
