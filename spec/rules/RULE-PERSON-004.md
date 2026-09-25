---
id: RULE-PERSON-004
title: Youth dilemmas
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-PERSON-001, FND-PERSON-004, FND-PERSON-005, FND-PERSON-010, FND-RNG-002, FND-STRATEGY-019, SRC-MANUAL, FND-STRATEGY-036, FND-PERSON-006, FND-PERSON-007, FND-PERSON-003]
conflicting: []
split_with: []
related: [RULE-PERSON-001, RULE-PERSON-003, RULE-RNG-001, FMT-PERSON-002]
---

## Summary

The generated knight answers one dilemma for each age from 12 to 17, drawn from the five of that
age. Each answer scores one attribute against two breakpoints for a win, draw or loss, and the
outcome changes attributes, including AGE, and may give an item.

## When it runs

When the generation screen opens or rerolls (RULE-PERSON-003), when a choice is clicked, and on Continue.

## Parameters

`number`, `INT32`, a dilemma number; `choice`, `INT32`, 0 to 2; `score`, `INT32`.

## Inputs

The character table and `current_dilemma`.

## Procedure

```text
define load_dilemma(number):
    current_dilemma = read_file(sprintf("DILEM%d.DAT", number), FMT-PERSON-002)

define continue_youth():
    let age = attr(0, 18)
    if age == 18:
        fn_0005B2C0()
        g_0009AAC4 = -1
        fn_000596C0(6, 0, 1)
    else:
        load_dilemma((age - 12) * 5 + random_inclusive(4))

define resolve_dilemma(score, choice):
    let points = current_dilemma.choices[choice].breakpoints
    let i = 0
    while i < count(points):
        if score >= points[i]:
            return i
        i = i + 1
    return i

define modifier_delta(choice, outcome, field):
    for each m in current_dilemma.choices[choice].outcomes[outcome].modifiers:
        if field_index(m.attribute) == field:
            return m.delta
    return 0

define answer_dilemma(choice):
    let c = current_dilemma.choices[choice]
    let outcome = resolve_dilemma(attr(0, field_index(c.scoring_attribute)), choice)
    for each m in c.outcomes[outcome].modifiers:
        let field = field_index(m.attribute)
        set_attr(0, field, attr(0, field) + modifier_delta(choice, outcome, field))
    for each item in dilemma_item_grants(current_dilemma.number, choice, outcome):
        item_counts[item] = item_counts[item] + 1
    # shows the outcome's text and pictures
    return outcome

define field_index(name):
    for i in 0..attribute_count:
        if attribute_names[i] == name:
            return i
    return unlisted_field
```

## Outputs

`answer_dilemma` returns the outcome, 0 win, 1 draw or 2 lose, and changes row 0 and `item_counts`.

## Edge cases

A score equal to a breakpoint reaches it. A field named twice in one outcome would get its first
change twice; none of the files does this. Every outcome in the files adds 1 to AGE, so six answers
reach 18; without that, Continue would draw from the same age again.

## What the sources say

SRC-MANUAL (p. 19) says each answer shapes strength, dexterity, piety, stamina, honour and starting
wealth, adds a year of age, and may give items, and that the six dilemmas start at age twelve.

## Differences between builds

None known.

## Open questions

- The field `unlisted_field` gives for `NONE`.
- Which dilemma numbers, choices and outcomes `dilemma_item_grants` covers in each of the three
  handlers.
- What `fn_000596C0(6, ...)` opens, and what `fn_0005B2C0` and `g_0009AAC4` do here.
