---
id: RULE-STRATEGY-019
title: Map events from conversation variables
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-STRATEGY-001, FND-STRATEGY-019, FND-STRATEGY-036, FND-STRATEGY-037, FND-TOURNEY-001, FND-TOURNEY-004, FND-TOURNEY-005, FND-TALK-006, FND-RES-002, FND-RES-006]
conflicting: []
split_with: []
related: [RULE-TALK-004, RULE-RES-001, RULE-RES-004]
---

## Summary

After a conversation the action hook turns conversation variables into map events: two bar scenes,
a melee in `MELEE0.RES`, and the Scottish and Welsh raids. Accepting the tasks of conversation nodes
2439 and 2593 sets variables 43 and 93, which start the raids on the next strategic pass.

## When it runs

From the hook of the dialogue screen, after the part RULE-TALK-004 gives.

## Parameters

`name` a scene resource.

## Inputs

The conversation state `g_0009A928` through `fn_00062828`.

## Procedure

```text
define strategic_actions():
    let g = g_0009A928
    if fn_00062828(g, 0x1D) != 0:
        fn_000628AC(g, 0x1E, 1)
        fn_000628AC(g, 0x1D, 0)
        fn_00021E28("BAR0.RES")
        fn_0005B0D4(0x18C, 1)
    if fn_00062828(g, 0x4B) != 0:
        fn_000628AC(g, 0x4C, 1)
        fn_000628AC(g, 0x4B, 0)
        fn_00021E28("BAR2.RES")
    if fn_00062828(g, 0x80) != 0:
        fn_000628AC(g, 0x81, 1)
        fn_000628AC(g, 0x80, 0)
        melee_from_map("MELEE0.RES")
        fn_0005B0D4(0x18B, 1)
    if fn_00062828(g, 0x2B) != 0:
        scotland_raid_pending = 1
    if fn_00062828(g, 0x5D) != 0:
        wales_raid_pending = 1
    fn_00024CA0()
    fn_00059760(1, 1)

define melee_from_map(name):
    fn_0005B2C0()
    fn_0005B0D4(0x17C, 1)
    close_archive(0)
    let r = fn_0005877C(name, 1, 1, 2)
    open_archive(gob_path, 0)
    return r
```

## Outputs

The conversation variables, the two raid requests and the scenes run. `melee_from_map` returns 1 when
the player wins the melee.

## Edge cases

Variables 43 and 93 are not cleared here, so the raid requests are raised again after every
conversation while they stay set; a raid whose slot is busy is dropped by RULE-STRATEGY-016.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_00021E28`, `fn_0005B0D4`, `fn_00059760` and `fn_00062828` do.
- What makes conversation nodes 2439 and 2593 reachable.
- What `fn_00024CA0` does.
- What `fn_0005877C` does.
- What `fn_0005B2C0` does.
- What `fn_000628AC` does.
- What `g_0009A928` holds.
