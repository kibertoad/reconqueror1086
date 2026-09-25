---
id: RULE-DRAGON-001
title: Dragon encounter
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-DRAGON-001, FND-DRAGON-002, FND-DRAGON-003, FND-STRATEGY-001, FND-STRATEGY-019, SRC-GAMEFAQS-66730]
conflicting: []
split_with: []
related: [RULE-PERSON-001, RULE-JOUST-003]
---

## Summary

Reaching the lair starts the dragon run of RULE-JOUST-003 with the player's lance experience held
to 0 to 20. A win adds 2 to it. Win or lose, the map session ends; a win ends the campaign with the
champion's investiture and a loss with the player's death.

## When it runs

When the record the player rides with reaches the lair (RULE-STRATEGY-010).

## Parameters

None.

## Inputs

Row 0 of `character_attributes`.

## Procedure

```text
define dragon_encounter():
    let lance = min(max(attr(0, 16), 0), 20)
    let result = fn_0001B584()
    if result == 0:
        lance = lance + 2
    set_attr(0, 16, lance)
    map_session_over = 1
    if result == 2 or result == 1:
        return 0
    return 1
```

## Outputs

`dragon_encounter` returns 1 for a win and 0 for a loss. The run plays `DRJSTRUN.SMK` from the CD
path, waits after its first pass for a click or a key and then plays the sound resource `0x1D8`, and
afterwards plays `DRJSTWIN.SMK` on a win or `DRJSTLSE.SMK` on a loss and waits for a click or a key.
After a win the lair shows the victory text and, when movies are enabled, plays `champl30.smk`;
otherwise it shows a still picture with text.

## Edge cases

Lance experience can reach 22 after a win, since `set_attr` does not limit field 16; the next read
of it for a run is held to 20. The test for result 1 never matches, since the worker returns only 0
or 2. There is no withdrawal: the run always ends in a win or a loss.

## What the sources say

SRC-GAMEFAQS-66730 gives the dragon-slaying lance, the shield and the armour as the way to kill the
dragon, which matches the items RULE-JOUST-003 counts. It says nothing of the lance experience
gained.

## Differences between builds

None known.

## Open questions

- What `fn_0001B584` does beyond RULE-JOUST-003; it returns 0 when `dragon_succeeds()` is true and 2
  otherwise.
- Whether anything outside the five-cell test keeps the player from the lair before the lair is
  disclosed in conversation.
