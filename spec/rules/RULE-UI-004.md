---
id: RULE-UI-004
title: Store purchase and sale
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-UI-014, FND-UI-007, FND-PERSON-001, FND-PERSON-007]
conflicting: []
split_with: []
related: [RULE-UI-003, RULE-PERSON-001, SCR-UI-007]
---

## Summary

The store's transaction control buys the shown item when the player lacks it and has the price, and
sells it back for its price less a quarter when the player holds it.

## When it runs

When the player clicks region 3 of the forge while the store is open.

## Parameters

None.

## Inputs

`store_offers`, `store_index`, `item_counts` and WEALTH, field 17 of row 0.

## Procedure

```text
define store_transaction():
    let e = store_offers[store_index]
    if item_counts[e.item] == 0:
        if attr(0, 17) < e.price:
            # shows the message that the player lacks the money and redraws the entry
            return
        set_attr(0, 17, attr(0, 17) - e.price)
        item_counts[e.item] = item_counts[e.item] + 1
    else:
        item_counts[e.item] = 0
        set_attr(0, 17, attr(0, 17) + e.price - e.price / 4)
    # redraws the entry: the sell or buy price, the View control and WEALTH
```

## Outputs

WEALTH and the item's count. The entry stays in the store after either transaction.

## Edge cases

`e.price / 4` truncates toward zero, so an item of price 30 sells for 23. A purchase can spend the
player's last shilling. A sale clears the whole count, so a player holding two of an item gets
the price of one. The buy test is on the item's count, so an item offered as unowned becomes
sellable at once.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

None known.
