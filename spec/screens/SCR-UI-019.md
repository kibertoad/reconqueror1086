---
id: SCR-UI-019
title: Dubbing
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-PERSON-013, FND-PERSON-006, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010, FND-UI-013, FND-UI-019, FND-UI-024, FND-UI-026, FND-MEDIA-009, EXP-UI-001]
conflicting: []
split_with: []
related: [SCR-UI-006]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#DUBBING.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (0, 0, 640, 480) | Always | Optionally requests sample playback using g_0009A708, stores zero there, then switches to SCR-UI-006. | FND-UI-013, FND-UI-024 |

## Keyboard input

| Key | Enabled when | Effect | Evidence |
|---|---|---|---|
| Keyboard availability; individual keys untraced | During either entry presentation wait | Ends that wait | FND-UI-019, FND-UI-026 |

## Other input

None known.

## Sounds

| Sound | Resource | Played when | Evidence |
|---|---|---|---|
| Full-screen click sample | First sample of `C1086.GOB#dking.666`, through g_0009A708 | The full-screen click requests playback when this value is nonzero | FND-UI-024, FND-UI-026 |
| Entry sample | First sample of `C1086.GOB#dubb3.666` | Animations-disabled entry requests playback when its temporary bank is nonzero | FND-UI-026 |

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Text and temporary sample presentation | Entry with animations disabled; requests four text lines over the current object | Accepted input | FND-UI-026 |
| Movie presentation | Entry with animations enabled; requests `dubb3.smk` | Movie helper returns | FND-UI-026, FND-MEDIA-009 |
| Briefing picture | Either presentation path completes; requests `C1086.GOB#FLUFF.PCX` | Accepted input, followed by entry return | FND-UI-026 |

A short primary or secondary release can end each input wait (FND-UI-019,
FND-UI-026). These are static requests; their rendered appearance has not
been verified.

EXP-UI-001 records accepted controlled input at both waits and the restored
entry return on the animations-disabled path. It does not verify pixels or
the later full-screen callback.

## Timing

None known.

## Differences between builds

None known.

## Open questions

- What pixels and text layout the entry's drawing helpers produce. (Q-UI-026)
