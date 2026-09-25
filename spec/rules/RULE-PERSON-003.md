---
id: RULE-PERSON-003
title: Character generation, the pre-generated knights and dubbing
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-PERSON-001, FND-PERSON-003, FND-PERSON-004, FND-PERSON-006, FND-PERSON-007, FND-RNG-002, FND-STRATEGY-019, SRC-MANUAL, FND-STRATEGY-036]
conflicting: []
split_with: []
related: [RULE-PERSON-001, RULE-PERSON-002, RULE-PERSON-004, RULE-RNG-001]
---

## Summary

A new player picks a colour and either generates a knight through six youth dilemmas or takes one
of six pre-generated knights. Sir Chaunce Norman, the first, is random: each skill moves by up to 8
and his purse is 0 to 1,000 shillings. Dubbing then makes every character 18, gives the player five
starting items and adds 240 shillings.

## When it runs

On the character options, generation and pre-generated screens, and when either route ends.

## Parameters

`region`, `INT32`: the clicked region of the options screen (3 to 5) or of the pre-generated
screen (0 to 5), read through `0x00059D50`.

## Inputs

The character table, `attribute_names` and `item_counts`.

## Procedure

```text
define open_character_options():
    set_attr(0, 19, 0)

define choose_shield(region):
    if region == 3:
        set_attr(0, 19, 0)
    else if region == 4:
        set_attr(0, 19, 3)
    else if region == 5:
        set_attr(0, 19, 5)

define start_generation():
    load_dilemma(random_inclusive(4))

define reroll():
    let name = character_names[0]
    let color = attr(0, 19)
    load_characters("CHARACTR.DAT", true)
    character_names[0] = name
    set_attr(0, 19, color)
    load_dilemma(random_inclusive(4))

define choose_pregen(region):
    character_names[0] = pregen_name(region)
    if region == 0:
        for field in 0..17:
            let negative = random_inclusive(1)
            let amount = random_inclusive(8)
            if negative != 0:
                amount = -amount
            set_attr(0, field, attr(0, field) + amount)
            set_attr(0, 17, random_inclusive(10) * 100)
    else:
        for each field in [0, 1, 2, 3, 5, 6, 15, 16, 17]:
            set_attr(0, field, pregen_value(region, field))
    fn_0005B2C0()
    g_0009AAC4 = -1
    fn_000596C0(6, 1, 1)

define dub_knight():
    for r in 0..character_count:
        set_attr(r, 18, 18)
    for each item in [9, 23, 32, 28, 16]:
        item_counts[item] = item_counts[item] + 1
    set_attr(0, 17, attr(0, 17) + 240)
```

## Outputs

The character table and `item_counts`.

## Edge cases

Sir Chaunce Norman starts from row 0 as loaded, which already carries the shift of RULE-PERSON-002,
and moves fields 0 to 16 again, held to their limits by `set_attr`; only the last of his 17 wealth
draws counts. The generated knight's wealth before dubbing is row 0's WEALTH after the dilemmas.

## What the sources say

SRC-MANUAL (pp. 18 to 20) describes the screen: three shields, a name of up to 20 letters, six
moral dilemmas from age twelve that change strength, dexterity, piety, stamina, honour and wealth
and add a year each, a Reroll that rolls new statistics and dilemmas, and six pre-generated
knights of whom Sir Chaunce Norman has random attributes and purse. It lists the other five
knights' values. SRC-MANUAL (p. 1) gives the starting knight the knight's sword, fighter's dagger,
gambeson, footman's helm, tilting shield, a purse of 240 to 1,240 shillings and a small fief, which
matches five items and a wealth of 0 to 1,000 plus 240.

## Differences between builds

None known.

## Open questions

- What `fn_000596C0(6, ...)` opens, and whether screen 6 runs `dub_knight`, which is registered as
  a callback at `0x00019A51`.
- What `fn_0005B2C0` and `g_0009AAC4` do here.
- Which items 9, 23, 32, 28 and 16 are.
- What the Choose Character Name control writes, and the limit of 20 letters.
- Which region of the pre-generated screen each knight's shield is, beyond region 0.
