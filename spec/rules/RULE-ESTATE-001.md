---
id: RULE-ESTATE-001
title: War Planning armies, companies and prices
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ESTATE-002, FND-PERSON-001, FND-STRATEGY-011, FND-STRATEGY-018, FND-STRATEGY-019, FND-STRATEGY-023, FND-STRATEGY-024, FND-STRATEGY-025, FND-STRATEGY-032, FND-STRATEGY-035, FND-STRATEGY-036, SRC-MANUAL, SRC-GAMEFAQS-66730]
conflicting: []
split_with: []
related: [RULE-STRATEGY-018, RULE-STRATEGY-012, RULE-ESTATE-002, RULE-PERSON-001]
---

## Summary

The War Planning screen works on a staged copy of the five armies, the free serfs, the population
and the wealth. A left click on a unit row raises a company of 100 serfs of that kind in the selected
army, and a right click removes one. A right click on an army asks to disband it. Armies on the map
150 or more units from home cannot be changed. OK or Enter writes the copy back, and Cancel or Escape
drops it. Each kind's price and monthly cost is one of two sets, the cheaper when FAME is above 16.

## When it runs

`open_war_planning` when the screen opens. On `FWARPLAN.HAT`, a left click on regions 0 to 4 runs
`select_army`, on regions 8 to 10 `add_company` with kind 0 to 2, on region 11 `cancel_war_planning`,
on region 12 `commit_war_planning`, on region 13 `rename_army` and on region 6 `buy_spy`
(RULE-STRATEGY-018). A right click on regions 0 to 4 runs `disband_army` and on regions 8 to 10
`remove_company`. Key 13 runs `commit_war_planning` and key 27 `cancel_war_planning`.

## Parameters

None.

## Inputs

`army_records`, `fiefs`, attributes 6 and 17 of row 0, `player_confirms_disband`, `typed_army_name`.

## Procedure

```text
define set_unit_prices():
    let s = 0
    # attribute 6 is FAME
    if attr(0, 6) > 16:
        s = 1
    for t in 0..3:
        army_records[0].units[t].price = unit_price_sets[s][t]
        army_records[0].units[t].upkeep = unit_upkeep_sets[s][t]

define open_war_planning():
    fn_00063050()
    if fn_000386CC() == 0:
        fn_000386A0()
    for a in 0..5:
        planning_active[a] = army_records[a].on_map
        if rides_with(a) == 1:
            planning_joined = a
        for t in 0..6:
            let n = army_records[a].units[t].count
            planning_units[a][t] = n
            let k = 3 * a + t
            if k < 15:
                planning_committed[k] = n
            else if k == 15:
                planning_population = n
            else if k == 16:
                spies_pending = n
            else:
                brigand_orders[0].unk_00 = n
        planning_disbanded[a] = 0
    for a in 0..5:
        planning_names[a] = copy(army_records[a].name)
    spies_pending = 0
    planning_serfs = fief_serfs(0)
    planning_population = fief_population(0)
    # attribute 17 is WEALTH
    planning_wealth = attr(0, 17)
    set_unit_prices()
    planning_army = 0

define select_army(a):
    # plays a sound and moves the highlight
    planning_army = a

define disband_army(a):
    # plays a sound
    let total = 0
    for t in 0..6:
        total = total + planning_units[a][t]
    if total != 0:
        # moves the highlight
        planning_army = a
    if planning_active[a] == 0:
        # shows that the army has not been raised
        return
    if army_away(a) == 1:
        # shows that the army is too far from home to disband
        return
    if not player_confirms_disband:
        return
    planning_active[a] = 0
    planning_disbanded[a] = 1
    for t in 0..6:
        planning_serfs = planning_serfs + planning_units[a][t] * 100
        planning_population = planning_population + planning_units[a][t] * 100
        planning_units[a][t] = 0
    planning_army = a
    if planning_joined == a:
        planning_joined = -1

define add_company(t):
    # plays a sound
    let a = planning_army
    let price = army_records[0].units[t].price
    if planning_serfs < 100:
        # shows that there are not enough men
        return
    if price > planning_wealth:
        # shows that there is not enough money
        return
    if army_away(a) == 1:
        # shows that the army is too far from home to reinforce
        return
    planning_units[a][t] = planning_units[a][t] + 1
    planning_serfs = max(planning_serfs - 100, 0)
    if planning_population - 200 >= 0:
        planning_population = planning_population - 100
    planning_wealth = max(planning_wealth - price, 0)

define remove_company(t):
    # plays a sound
    let a = planning_army
    if planning_units[a][t] == 0:
        # shows that the army has no warriors of this kind
        return
    if army_away(a) == 1:
        # shows that the army is too far from home to disband
        return
    planning_units[a][t] = planning_units[a][t] - 1
    planning_population = planning_population + 100
    planning_serfs = planning_serfs + 100
    if planning_disbanded[a] == 1 or planning_committed[3 * a + t] <= 0:
        planning_wealth = planning_wealth + army_records[0].units[t].price
    planning_committed[3 * a + t] = planning_committed[3 * a + t] - 1

define rename_army():
    # shows the name box, up to 15 characters
    planning_names[planning_army] = typed_army_name

define send_spies():
    for k in 0..spies_pending:
        if dispatch_spy() == 0:
            # shows that spies are already out
            continue

define commit_war_planning():
    # plays a sound
    for a in 0..5:
        let total = 0
        for t in 0..6:
            total = total + planning_units[a][t]
        if total != 0:
            if planning_active[a] == 0:
                planning_active[a] = 1
                if army_records[a].on_map == 0 and planning_disbanded[a] == 0:
                    place_army(a)
                if army_records[a].on_map != 0 and planning_disbanded[a] != 0:
                    remove_army(a)
                    place_army(a)
        else if army_records[a].on_map != 0:
            planning_active[a] = 0
            remove_army(a)
        army_records[a].on_map = planning_active[a]
        army_records[a].name = copy(planning_names[a])
        for t in 0..6:
            army_records[a].units[t].count = planning_units[a][t]
    send_spies()
    set_fief_population(0, planning_population)
    set_fief_serfs(0, planning_serfs)
    set_attr(0, 17, planning_wealth)
    fn_00059760(1, 1)

define cancel_war_planning():
    # plays a sound
    fn_00059760(1, 1)
```

## Outputs

The committed armies, the fief's population and free serfs, WEALTH, and the spy (RULE-STRATEGY-018)
after `commit_war_planning`. Each company then costs its kind's monthly cost every month
(RULE-ESTATE-002). The screen shows the staged counts, population, free serfs and wealth.

## Edge cases

Nothing limits the number of companies in an army. Raising a company takes 100 from the population
only while the population is 200 or more, but removing one always gives 100 back, so a small
population can be raised (BUG-ESTATE-002). Removing a company gives its price back only once the
army's committed count of that kind has run out, so a company raised and removed again in an army
that already had that kind costs its price (BUG-ESTATE-001). Disbanding gives the serfs back but no
money. The opening copy of the committed counts writes six per army into rows of three, so army 4's
last three kinds land in `planning_population`, `spies_pending` and `brigand_orders[0].unk_00`; the
first two are overwritten straight after. Changes to the name and the raised state of an army away
from home are still written back by `commit_war_planning`, since only the unit clicks and
disbanding test `army_away`.

## What the sources say

SRC-MANUAL (pp. 36 to 38) describes five armies named Army 1 to Army 5, companies of 100 serfs
raised and removed by left and right clicks, no changes to armies away from home, the spy, and OK
and Cancel. It advises against more than 60 companies in an army; the executable does not enforce
that. SRC-GAMEFAQS-66730 gives each kind's price and upkeep at 50% and at 100% productivity; the
two pairs it gives are the executable's two sets, which the executable picks by FAME, not by
productivity.

## Differences between builds

None known.

## Open questions

- What `fn_00063050`, `fn_000386CC` and `fn_000386A0` do when the screen opens, and what
  `fn_00059760` does when it closes.
- Whether `planning_joined` is set anywhere else, and what reads it.
- Whether any army record ever holds a count in unit rows 3 to 5, and whether anything reads
  `brigand_orders[0].unk_00` after the opening copy writes it.
- The values of `unit_price_sets` and `unit_upkeep_sets` are designer data and are left in the
  executable.
