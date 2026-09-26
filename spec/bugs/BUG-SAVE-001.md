---
id: BUG-SAVE-001
title: A saved game without temp.jap stops the program when loaded
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: crash
intent: unintended
player_reliance: not-relied-on
evidence: [FND-SAVE-002, FND-SAVE-003]
conflicting: []
split_with: []
related: [RULE-SAVE-002, RULE-SAVE-003]
---

## Symptom

Loading the slot ends the program with `Error in Restoring game data. Aborting.` and exit code 100.

## Trigger conditions

The game was saved at a time when `temp.jap` did not exist in the current directory.

## Mechanism

`save_game` adds `temp.jap` to the container only when the file exists, but `load_game` treats a
missing `temp.jap` entry as a fatal error, as it does for the entries every save has.

## Frequency

Every load of such a save. Startup deletes `temp.jap`, so the window is from the start of a session until the strategic map first writes it.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Whether normal play can reach the save screen before `temp.jap` is written.
