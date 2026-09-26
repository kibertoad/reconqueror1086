---
id: RULE-TALK-003
title: Script functions and conversation variables
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-TALK-005, FND-TALK-006, FND-PERSON-001, FND-PERSON-002, FND-PERSON-007, FND-TALK-002, FND-TALK-010]
conflicting: []
split_with: []
related: [FMT-TALK-008, RULE-PERSON-001]
---

## Summary

The scripts call seven functions: 3 redirects the conversation, 4 assigns, 5 adds, 6 reads, and 7,
8 and 9 give, take away and test an item. Assign, add and read take a scope and a number: scope 0
is a conversation variable of `ALL.VTB` and scope 1 a selector for one of seven player attributes.

## When it runs

When RULE-TALK-002 evaluates a value of kind 3.

## Parameters

`fn`, the function number, and `args`, its evaluated arguments.

## Inputs

`conversation_variables`, the player's attributes, `item_counts` and `script_item_slots`.

## Procedure

```text
define variable_in_range(v):
    # v equal to the count passes, one element past the table (BUG-TALK-001)
    return v >= 0 and v <= conversation_variables.count

define get_variable(v):
    return conversation_variables.values[v]

define set_variable(v, value):
    conversation_variables.values[v] = value

define selector_field(n):
    # -1 for a selector with no attribute; negative selectors count as too large
    let fields = [17, -1, 5, 6, -1, 2, 0, 4, 3]
    if n < 0 or n > 8:
        return -1
    return fields[n]

define script_assign(scope, n, value):
    if scope == 0:
        if value == -2147483648 or not variable_in_range(n):
            return 0
        set_variable(n, value)
        return 1
    if scope == 1 and selector_field(n) != -1:
        set_attr(0, selector_field(n), value)
    return 0

define script_add(scope, n, delta):
    if scope == 0:
        if not variable_in_range(n):
            return 0
        let sum = INT32(get_variable(n) + delta)
        if sum == -2147483648:
            return 0
        set_variable(n, sum)
        return 1
    if scope == 1 and selector_field(n) != -1:
        set_attr(0, selector_field(n), attr(0, selector_field(n)) + delta)
        return 1
    return 0

define script_read(scope, n):
    if scope == 0:
        if not variable_in_range(n):
            return 0
        return get_variable(n)
    if scope == 1 and selector_field(n) != -1:
        return attr(0, selector_field(n))
    return 0

define call_function(fn, args):
    if fn == 3:
        conversation_redirect = args[0]
        return success(1)
    if fn == 4:
        return success(script_assign(args[0], args[1], args[2]))
    if fn == 5:
        return success(script_add(args[0], args[1], args[2]))
    if fn == 6:
        return success(script_read(args[0], args[1]))
    if fn == 7:
        let item = script_item_slots[args[1]]
        item_counts[item] = item_counts[item] + 1
        return success(1)
    if fn == 8:
        item_counts[script_item_slots[args[1]]] = 0
        return success(1)
    if fn == 9:
        return success(item_counts[script_item_slots[args[1]]] != 0)
    return failure()
```

## Outputs

The conversation variables, the player's attributes and item counts, and `conversation_redirect`.

## Edge cases

Assigning to an attribute gives 0 even though it writes, so an action that tests the result of an
attribute assignment takes the false branch. Selectors 1 and 4 read 0 and write nothing. Adding to
an attribute goes through `set_attr`, whose limits RULE-PERSON-001 gives. A function number other
than 3 to 9 fails the value, and so the action.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Which item each entry of `script_item_slots` names; the table is content, and how many entries
  it has is not known. The scripts use items 0 to 23 and, in one test, 161 (FND-TALK-010).
- What a redirect to a node missing from the index does; one script names node 5011, which is
  missing (FND-TALK-010).
- What `args[0]` of functions 7 to 9 holds; the handlers ignore it.
