---
id: RULE-RES-005
title: Expanding a SETUP.SOL member's stream
status: unknown
builds: [BLD-GOG-EN]
superseded_by: []
evidence: []
conflicting: []
split_with: []
related: []
---

## Summary

How SETUP turns a member's stored stream in SETUP.SOL (FMT-RES-017) into the
file it writes. SETUP does it with a routine at 0001:41AB that no finding has
read.

## When it runs

When SETUP extracts a marked member from SETUP.SOL.

## Parameters

None.

## Inputs

None known.

## Procedure

None known.

## Outputs

None known.

## Edge cases

None known.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Is the stream PKWARE Data Compression Library "implode" output, expanded
  by its "explode"? For: every shipped member starts with 0x00 and 0x06,
  which is that format's header for binary mode and a 4,096-byte dictionary,
  and SETUP allocates 12,574 bytes, the documented size of explode's work
  buffer (FND-RES-030). Both are circumstantial. Against: nothing yet.
  Reading 0001:41AB would settle it; expanding every member with a known
  explode would add circumstantial support only. (Q-RES-175)
