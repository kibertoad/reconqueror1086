---
id: RULE-RNG-001
title: The game's random number routine
status: unknown
builds: [BLD-GOG-EN]
superseded_by: []
evidence: []
conflicting: []
split_with: []
related: []
---

## Summary

Routine `0x0004CE4C` gives a random integer. The ASSAULT rules use it as `random(n)`, a value
from 0 to `n - 1` (FND-ASSAULT-031, FND-ASSAULT-036).

## When it runs

Wherever a rule calls `random`.

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

- The generator, its state, how it is seeded and how it reduces a draw to a range.
