---
id: BUG-RES-001
title: The install script's third menu choice names no label exactly and ends the script without its closing message
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: presentation
intent: unintended
player_reliance: not-relied-on
evidence: [FND-RES-046, FND-RES-050]
conflicting: []
split_with: []
related: [FMT-RES-016]
---

## Symptom

Choosing the third option of the installer script's menu ends the script
at once. The message and pause that the other two choices reach on line 53
of `INSTALL.SCR` are not shown.

## Trigger conditions

`INST.EXE` runs `INSTALL.SCR` (FND-RES-050) and the player presses the third
key of the `pick` command on line 16.

## Mechanism

`pick` jumps through `goto`, which looks forward for a label equal to the
word byte for byte (FND-RES-046). The word on line 16 differs in letter case
from the label on line 53 and is one character short of the label on line
81, so no label matches, every later line is skipped and the run ends at the
end of the script (FND-RES-050).

## Frequency

Every time the third choice is made.

## Player reliance

None known; the effect is limited to the installer's screen.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Which of the two labels the script's author meant. Both lead to `end`,
  one after a message and a pause. (No item: no evidence beyond the script
  can settle it.)
