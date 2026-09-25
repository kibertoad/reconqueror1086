---
id: RULE-TALK-002
title: Action-tree interpreter
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-TALK-004, FND-TALK-005]
conflicting: []
split_with: []
related: [FMT-TALK-003, FMT-TALK-004, FMT-TALK-005, FMT-TALK-006, FMT-TALK-007, RULE-TALK-003]
---

## Summary

An action number found in a conversation node names a group in `ALL.TMI`. The group's actions run in
order; each evaluates an expression and, by its kind, runs one branch, chooses between two or does
nothing more. An expression folds its values left to right with eight comparison and logic
operators, and a value is a literal, a nested expression or a call of a script function of
RULE-TALK-003. A failure anywhere in an action abandons that action and the group moves on.

## When it runs

For each action slot of a node that RULE-TALK-001 runs.

## Parameters

`id`, the action number.

## Inputs

`ALL.TMI` and `ALL.TMB`.

## Procedure

```text
define run_action_group(id):
    # gives 0 when the group cannot be found or read
    if id < 1:
        return 0
    let tmi = read_file("ALL.TMI", FMT-TALK-003)
    let offset = -1
    for each r in tmi.records:
        if r.action == id and offset == -1:
            offset = r.offset
    if offset < 0:
        return 0
    let group = read_file("ALL.TMB", FMT-TALK-004, offset)
    for i in 0..group.count:
        run_action(group.actions[i])
    return 1

define run_action(offset):
    # gives 0 when the action fails
    let a = read_file("ALL.TMB", FMT-TALK-005, offset)
    let x = evaluate_expression(a.expression)
    if x.failed:
        return 0
    if a.kind == ACTION_WHEN:
        if a.branch_count != 1:
            return 0
        if x.value != 0:
            return run_action(a.branches[0])
        return 1
    if a.kind == ACTION_IF_ELSE:
        if a.branch_count != 2:
            return 0
        if x.value != 0:
            return run_action(a.branches[0])
        return run_action(a.branches[1])
    if a.kind == ACTION_EVALUATE:
        return 1
    return 0

define evaluate_expression(offset):
    # gives a result with the fields failed and value
    let e = read_file("ALL.TMB", FMT-TALK-006, offset)
    if e.operator_count + 1 != e.value_count:
        return failure()
    let first = evaluate_value(e.values[0])
    if first.failed:
        return first
    let acc = first.value
    for i in 0..e.operator_count:
        let v = evaluate_value(e.values[i + 1])
        if v.failed:
            return v
        let op = e.operators[i]
        if op == OP_NE:
            acc = acc != v.value
        else if op == OP_LE:
            acc = acc <= v.value
        else if op == OP_GE:
            acc = acc >= v.value
        else if op == OP_AND:
            acc = acc != 0 and v.value != 0
        else if op == OP_OR:
            acc = acc != 0 or v.value != 0
        else if op == OP_GT:
            acc = acc > v.value
        else if op == OP_LT:
            acc = acc < v.value
        else if op == OP_EQ:
            acc = acc == v.value
        else:
            return failure()
    return success(acc)

define evaluate_value(offset):
    let v = read_file("ALL.TMB", FMT-TALK-007, offset)
    let result = 0
    if v.kind == VALUE_EXPRESSION:
        let x = evaluate_expression(v.value)
        if x.failed:
            return x
        result = x.value
    else if v.kind == VALUE_LITERAL:
        result = v.value
    else if v.kind == VALUE_FUNCTION:
        let args: INT32[] = []
        for i in 0..v.argument_count:
            let x = evaluate_expression(v.arguments[i])
            if x.failed:
                return x
            append(args, x.value)
        let c = call_function(v.value, args)
        if c.failed:
            return c
        result = c.value
    else:
        return failure()
    if v.flag <= -1:
        return success(result)
    if v.flag == 1:
        return success(result == 0)
    return failure()
```

## Outputs

The effects of the script functions the actions call.

## Edge cases

Every operand is evaluated, so AND and OR run the functions on both sides. A comparison gives 1 or 0.
An action of kind 3 is not checked for its branch count. The group lookup reads the index from the
start each time and takes the first matching record.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- `failure()` and `success(x)` build the result that `evaluate_expression`, `evaluate_value` and
  `call_function` give; how the original passes it (a return code and an out-parameter) has no
  effect on the outcome.
