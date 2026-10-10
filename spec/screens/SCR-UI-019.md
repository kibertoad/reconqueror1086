---
id: SCR-UI-019
title: Dubbing
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-PERSON-004, FND-PERSON-006, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010, FND-UI-013, FND-UI-024]
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

None known.

## Other input

None known.

## Sounds

| Sound | Resource | Played when | Evidence |
|---|---|---|---|
| Full-screen click sample | The value at g_0009A708; sample identity untraced | The full-screen click requests playback when this value is nonzero | FND-UI-024 |

## States

None known.

## Timing

None known.

## Differences between builds

None known.

## Open questions

- What the entry routine `0x00019A8C` plays or shows. (Q-UI-026)
- Which sample the full-screen click requests through g_0009A708. (Q-UI-044)
