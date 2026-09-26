---
id: RULE-SAVE-004
title: Starting values for a new game, default.dat
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-SAVE-005, FND-STRATEGY-008, FND-STRATEGY-015, FND-PERSON-007]
conflicting: []
split_with: []
related: [FMT-SAVE-008, RULE-TALK-001]
---

## Summary

At every start the game stores the persons' and properties' starting values in `default.dat`; a
new game restores them from it, so every new game in a session starts from the same values.

## When it runs

`write_default_file` once at startup; `reset_new_game` when a new game starts from SCR-UI-002.

## Parameters

None.

## Inputs

`persons`, `properties` and `item_counts`.

## Procedure

```text
define write_default_file():
    # default.dat is written as FMT-SAVE-008 from persons and properties;
    # a failure stops the game with fatal error 0
    for each person in persons:
        person.next = 0xFF

define reset_new_game():
    property_list_head = 0xFF
    person_list_head = 0xFF
    # the dwords at 0x0009ABE8 and 0x0009B718 are set to 0 and the conversation variables freed
    for i in 0..70:
        item_counts[i] = 0
    let defaults = read_file(sprintf("default.dat"), FMT-SAVE-008)
    # each person's bytes 6 to 10 and the 14 property records are taken from defaults
    for each person in persons:
        person.next = 0xFF
```

## Outputs

`default.dat` in the current directory; `persons`, `properties` and the list heads.

## Edge cases

- A missing `default.dat` at a new game stops the program; startup has always just written it.
- The file is rewritten at every start, so editing it has no lasting effect.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

None.
