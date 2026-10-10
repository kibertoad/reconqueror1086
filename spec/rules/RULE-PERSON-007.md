---
id: RULE-PERSON-007
title: March aging and milestone skill increases
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-PERSON-001, FND-PERSON-002, FND-PERSON-003, FND-PERSON-011, FND-PERSON-012, FND-STRATEGY-001, FND-STRATEGY-014, EXP-PERSON-001, EXP-PERSON-002]
conflicting: []
split_with: []
related: [RULE-PERSON-001, RULE-PERSON-003, RULE-PERSON-004, RULE-PERSON-006]
---

## Summary

In month 2, March, a zero first-character flag lets the pass add one AGE year
to every visited character and mark the whole table. Computed ages 20, 23,
26 and 29 also increase STR, DEX, STAM and INT. Outside March a marked first
character causes all visited flags to be cleared. A newly loaded table has
zero flags, so the pass can age it in the starting March before any youth answer.

## When it runs

The observed main-loop path calls it after calendar update and before input
dispatch when the character-table pointer is nonnull. This is a table-wide
pass, including character-generation preparation, rather than an AGE modifier
inside a youth answer.

## Parameters

None.

## Inputs

`current_month`, `character_count`, `character_attributes` and `march_age_flags`.

## Procedure

```text
define march_age_pass():
    if current_month() == 2:
        if march_age_flags[0] != 0:
            return
        let row = 0
        while row < character_count:
            # Each addition wraps and is interpreted as signed INT32.
            let computed_age = attr(row, 18) + 1
            set_attr(row, 18, computed_age)
            if computed_age in [20, 23, 26, 29]:
                for field in [0, 1, 3, 4]:
                    set_attr(row, field, attr(row, field) + 1)
            march_age_flags[row] = 1
            row = row + 1
    else:
        if march_age_flags[0] == 0:
            return
        let row = 0
        while row < character_count:
            march_age_flags[row] = 0
            row = row + 1
```

## Outputs

The visited AGE and milestone skills, with RULE-PERSON-001's setter limits,
and `march_age_flags`. The character and calendar root pointers stay unchanged.

## Edge cases

Only the first flag gates the pass; a second row already marked is still aged
when the first row is unmarked. Other months clear nothing when the first flag
is zero. The milestone test uses the computed AGE before the setter's lower
bound, not the value read back afterward. Addition wraps at 32 bits; a wrapped
negative AGE is stored as zero. AGE has no upper bound in this setter.

The first flag is read before a count check, so a nonnull table pointer alone
does not establish that row zero is accessible. With a fabricated accessible
row zero, signed nonpositive counts skip both loops. These controls do not
establish that such a table can be loaded from a valid source file.

## What the sources say

FND-PERSON-011 gives loaded player AGE 12. The observed starting calendar
constructor uses month 2, day 16 and year 1086, and FND-PERSON-003 initializes
the flags to zero. EXP-PERSON-002 records AGE 13 before the first youth answer;
the isolated pass of EXP-PERSON-001 supplies that extra year from this state.

## Differences between builds

None known.

## Open questions

- Other producers of the calendar and March flags, including reroll timing and
  the full lifecycle of the calendar's update control. (Q-PERSON-022)
