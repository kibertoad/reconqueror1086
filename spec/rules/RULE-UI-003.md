---
id: RULE-UI-003
title: Store stock
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-UI-007, FND-TOURNEY-005, FND-PERSON-007, FND-RNG-002, FND-UI-004]
conflicting: []
split_with: []
related: [RULE-RNG-001, FMT-UI-004, SCR-UI-007]
---

## Summary

Each visit to the store draws its stock from the records of `WEAPONS.DAT` whose number has the
place's residue modulo 4, each with its own chance, leaving out items the player owns. The owned
items follow, all but record 16's.

## When it runs

When the player opens the store from the forge.

## Parameters

None.

## Inputs

The person of the place, `place_person`, and `item_counts`.

## Procedure

```text
define stock_store():
    let s = place_person & 3
    let offers = []
    for r in 0..38:
        let rec = store_records[r]
        if r % 4 == s and item_counts[rec.item] == 0 and random_inclusive(20) < rec.chance:
            append(offers, rec)
    store_offer_count = count(offers)
    for r in 0..38:
        let rec = store_records[r]
        if item_counts[rec.item] != 0 and r != 16:
            append(offers, rec)
    store_offers = offers
```

## Outputs

`store_offers` and `store_offer_count`. The generator advances once for each record of the place's
residue whose item the player lacks.

## Edge cases

A record with chance 20 appears with probability 20 in 21. The item of record 16 never appears among
the owned items. Record 39 is never read.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- How the store screen uses the owned entries after `store_offer_count`.
