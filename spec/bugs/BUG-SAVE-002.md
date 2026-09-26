---
id: BUG-SAVE-002
title: The title editor cannot be cancelled and types Esc and Backspace into the title
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: presentation
intent: unclear
player_reliance: unknown
evidence: [FND-SAVE-001]
conflicting: []
split_with: []
related: [RULE-SAVE-001]
---

## Symptom

After picking a slot on the save screen the game always saves; `Esc` and a Backspace on an empty title add control characters to the title.

## Trigger conditions

Pressing `Esc`, or Backspace with nothing typed, while editing a title.

## Mechanism

The editor handles only Enter and Backspace on a non-empty title; every other key below 256 is appended.

## Frequency

Every such key press.

## Player reliance

Not known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- How the font draws the control characters.
