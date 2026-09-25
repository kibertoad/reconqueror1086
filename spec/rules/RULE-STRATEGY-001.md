---
id: RULE-STRATEGY-001
title: Strategic pass and the yearly orders
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-STRATEGY-001, FND-STRATEGY-002, FND-STRATEGY-004, FND-STRATEGY-014, FND-STRATEGY-015, FND-STRATEGY-019, FND-STRATEGY-023, FND-STRATEGY-027, FND-STRATEGY-036]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-STRATEGY-002, RULE-STRATEGY-010, RULE-STRATEGY-015, RULE-STRATEGY-016, RULE-STRATEGY-017, RULE-STRATEGY-018]
---

## Summary

One pass of the strategic map advances the brigands, the spy report, the player records and, unless
the camera is being panned, the hostile forces. It then draws the map, starts any raid a
conversation asked for, shows the planting notice once, and issues the yearly brigand order and the
yearly order from the king.

## When it runs

Once for each frame of the strategic map.

## Parameters

None.

## Inputs

The game date through `current_month` and `current_year`, and the state the called rules read.

## Procedure

```text
define strategic_pass():
    fn_0003AFB4()
    brigand_pass()
    spy_report()
    player_pass()
    if map_busy == 0:
        hostile_pass()
        if map_session_over == 1:
            return
    draw_route_preview()
    draw_player_markers()
    draw_hostile_markers()
    if scotland_raid_pending == 1:
        scotland_raid_pending = 0
        raid_scotland()
    if wales_raid_pending == 1:
        wales_raid_pending = 0
        raid_wales()
    if planting_notice == 1:
        # shows the planting notice
        planting_notice = 0
    let o = fallback_origin
    if current_year() >= next_brigand_year and properties[o].state != 0 and properties[7].alerted == 0:
        issue_order(1, random_inclusive(3) + 7, next_brigand_year, o, 0)
        next_brigand_year = next_brigand_year + 1
    if current_year() >= next_king_order_year and current_month() >= 5 and properties[7].alerted == 0:
        let r = random_inclusive(13)
        while properties[r].state == 0 or properties[r].state == 7 or r == fallback_origin:
            r = random_inclusive(13)
        issue_order(2, random_inclusive(4), next_king_order_year + 1, 7, r)
        next_king_order_year = next_king_order_year + 1
```

## Outputs

The pass changes the state its callees change. The planting notice is a message box. A timed brigand
order ends in month 7 to 10 of the year it is issued; an order from the king falls due in month 0 to
4 of the next year.

## Edge cases

A pass that meets a battle ending the map session returns before drawing and before the yearly
orders. Once London (property 7) is alerted, neither yearly order is issued again, and the year
counters stop advancing. The king's draw loops for ever when no property other than the fallback
origin has a state other than 0 and 7.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_0003AFB4` does.
