---
id: RULE-ESTATE-002
title: Monthly estate pass and upkeep
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ESTATE-003, FND-ESTATE-002, FND-PERSON-001, FND-STRATEGY-001, FND-STRATEGY-014, SRC-MANUAL]
conflicting: []
split_with: []
related: [RULE-ESTATE-001, RULE-ESTATE-003, RULE-ESTATE-004, RULE-ESTATE-005, RULE-ESTATE-006, RULE-PERSON-001]
---

## Summary

Once a month the game settles the home fief: it copies WEALTH into the fief, runs its revenue,
expense and population steps, runs the July harvest steps once a year, collects a loan in July, and
copies the wealth back. The expenses are the running costs of the fief's four lists and the monthly
cost of every company in every army.

## When it runs

`monthly_estate_pass` from the calendar routine.

## Parameters

None.

## Inputs

`fiefs`, `army_records`, attribute 17 of row 0, `current_month`.

## Procedure

```text
define fief_population(f):
    if fiefs[f].list_2C[1] < 0:
        fiefs[f].list_2C[1] = 0
    return fiefs[f].list_2C[1]

define set_fief_population(f, n):
    fiefs[f].list_2C[1] = max(n, 0)

define fief_serfs(f):
    if fiefs[f].serfs < 0:
        fiefs[f].serfs = 0
    return fiefs[f].serfs

define set_fief_serfs(f, n):
    fiefs[f].serfs = max(n, 0)

define monthly_expenses(f):
    let r = fiefs[f]
    let spent = 0
    for i in 0..19:
        if r.list_28[8 * i + 7] != 0:
            spent = spent + r.list_28[8 * i + 7] * r.list_28[8 * i + 10]
    for i in 0..15:
        if r.list_2C[11 * i + 9] != 0:
            spent = spent + r.list_2C[11 * i + 9] * r.list_2C[11 * i + 11]
    for i in 0..10:
        if r.list_34[10 * i + 6] != 0:
            spent = spent + r.list_34[10 * i + 6] * r.list_34[10 * i + 8]
    for i in 0..9:
        if r.list_30[8 * i + 4] != 0:
            spent = spent + r.list_30[8 * i + 4] * r.list_30[8 * i + 6]
    for a in 0..5:
        for t in 0..6:
            spent = spent + army_records[a].units[t].count * army_records[0].units[t].upkeep
    if spent > r.wealth:
        r.wealth = 0
    else:
        r.wealth = r.wealth - spent

define monthly_estate_pass():
    let month = current_month()
    let last = fiefs[0].last_month
    # attribute 17 is WEALTH
    fiefs[0].wealth = attr(0, 17)
    if month <= last and not (month == 0 and last == 11):
        return
    fn_0002DE60(0)
    fn_0002D9E8(0)
    fn_0002DA48(0)
    fn_0002E588(0)
    monthly_expenses(0)
    fn_0002E628(0)
    fiefs[0].last_month = month
    fn_0002E150(0)
    if month == 6:
        if fiefs[0].july_done == 0:
            fn_0002E3B8(0)
            fn_0002DEAC(0)
            fn_0002DDFC(0)
            fn_0002DD3C(0)
            fn_0002DD94(0)
            fiefs[0].july_done = 1
        if collect_debt() == 1:
            return
    else if fiefs[0].july_done != 0:
        fiefs[0].july_done = 0
    if month == 2:
        # suggests planting crops
    if fiefs[0].wealth == 0:
        fiefs[0].months_broke = fiefs[0].months_broke + 1
        if fiefs[0].months_broke > 2:
            fn_0002CCD4(0)
    else:
        fiefs[0].months_broke = 0
    set_attr(0, 17, fiefs[0].wealth)
    fn_0005C550()
    fn_0005C5CC()
```

## Outputs

WEALTH, the fief's lists, serfs and population through the unnamed steps, and the months without
money. After a third month in a row with no money, `fn_0002CCD4` may show that the castle is
deteriorating.

## Edge cases

Only fief 0 is settled. Upkeep is charged for every army, raised or not and wherever it is, since
the loop visits all five records. When the player dies fighting Drogo the pass returns before it
writes WEALTH back, so the July steps' changes to the fief's wealth are not copied to the player.
A month passed over entirely, such as a jump of two months, is settled once.

## What the sources say

SRC-MANUAL (pp. 41 to 42) says that costs are subtracted monthly and that taxes fall due on July 1.

## Differences between builds

None known.

## Open questions

- What `fn_0002DE60`, `fn_0002D9E8`, `fn_0002DA48`, `fn_0002E588`, `fn_0002E628` and `fn_0002E150`
  do each month, and `fn_0002E3B8`, `fn_0002DEAC`, `fn_0002DDFC`, `fn_0002DD3C` and `fn_0002DD94` in
  July; they are expected to hold the revenue, tax, population and harvest rules.
- The layout of the rows of the four lists. In the `list_28` and `list_2C` sums the second word lies
  in the next row, and the last row's reaches past the list; what the original reads there is not
  known.
- What `fn_0002CCD4` does beyond the message, and what `fn_0005C550` and `fn_0005C5CC` do.
- What `fiefs[0]` field `+0x14`, which `0x0002D5D4` reads capped at 100, holds.
