---
id: RULE-TALK-001
title: Conversation walk
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-TALK-001, FND-TALK-002, FND-TALK-003, FND-RNG-003]
conflicting: []
split_with: []
related: [FMT-TALK-001, FMT-TALK-002, RULE-TALK-002, RULE-RNG-001]
---

## Summary

A conversation starts at a root node and walks from node to node until a target of 0. A node with
responses shows one of its prompt variants and waits for the player's choice; a node without them
is a statement that shows its prompt for two seconds and moves on. After the choice the node's own
actions run, then the chosen response's actions, and an action can redirect the walk to another
node. The prompt variant is drawn after reseeding the generator from the clock.

## When it runs

When the dialogue screen opens (RULE-TALK-004).

## Parameters

`root`, the first node; `name`, the base name of the files, `"all"` for every caller found.

## Inputs

`ALL.CIF`, `ALL.CBF`, the action files of RULE-TALK-002, `fn_0006B3B4`, and the player's choice through `fn_0001A3B8`.

## Procedure

```text
define find_node(cif, id):
    # binary search over the FMT-TALK-001 records; the file length comes from the file
    let low = 0
    let high = file_length(cif) / 8 - 1
    while low <= high:
        let mid = (low + high) / 2
        let record = read_file(cif, FMT-TALK-001, mid * 8)
        if id < record.node:
            high = mid - 1
        else:
            low = mid + 1
        if id == record.node:
            return record.offset
    return -1

define show_prompt(node, id):
    let t = fn_0006B3B4(0)
    let variant = 0
    if node.prompt_count > 1:
        seed_random(t)
        variant = draw() % node.prompt_count
        # plays sprintf("%s%d%c.smk", CD_PATH or "", id, 'a' + variant)
    else:
        # plays sprintf("%s%d.smk", CD_PATH or "", id)
    fn_0001A91C(node, variant)

define converse(root, name, flag):
    if root == 0:
        return
    let cbf = sprintf("%s%s", name, ".CBF")
    let cif = sprintf("%s%s", name, ".CIF")
    let id = root
    while id != 0:
        conversation_redirect = -1
        let response = 6
        let offset = find_node(cif, id)
        if offset == -1:
            return
        let node = read_file(cbf, FMT-TALK-002, offset)
        let next = 0
        if node.response_count == 0:
            if node.prompt_count != 0:
                show_prompt(node, id)
                fn_0001ACEC(2000, 0)
            next = node.targets[0]
        else:
            show_prompt(node, id)
            response = fn_0001A3B8(node)
            while response < 0 or response >= node.response_count:
                response = fn_0001A3B8(node)
            next = node.targets[response]
        for slot in 0..30:
            if node.node_actions[slot] != -1:
                run_action_group(node.node_actions[slot])
        if response < 5:
            for slot in 0..30:
                if node.response_actions[response * 30 + slot] != -1:
                    run_action_group(node.response_actions[response * 30 + slot])
        if conversation_redirect != -1:
            next = conversation_redirect
        id = next
```

## Outputs

Node and response actions have run; the generator is reseeded for every node with more than one prompt variant.

## Edge cases

A node the index lacks ends the conversation. A statement with no prompt variants shows nothing and
does not wait. `conversation_redirect` is reset at the start of each node, so only a redirect set by
this node's actions counts. Because each multi-variant prompt reseeds the generator from the clock,
every draw after a conversation depends on when it was held.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_0001A91C`, `fn_0001ACEC` and `fn_0001A3B8` do beyond showing the prompt, waiting and
  reading the choice, and what `flag` changes.
- What `fn_0006B3B4` returns; it is likely the C library's `time`.
- What the dwords at `0x330` and `0x344` of a node are for (FMT-TALK-002).
