---
id: FMT-VIEW-004
title: Scene settings, the Scenario resource
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/*.RES", "CD:CONQUER/*.LOW"]
byte_order: little
size: 568
text: false
definition: fmt_view_004.ksy
evidence: [FND-VIEW-013, FND-VIEW-011, FND-VIEW-017]
conflicting: []
split_with: []
related: [RULE-VIEW-006, RULE-VIEW-007]
---

## Layout

The `Scenario` resource of a scene archive.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 20 | `BYTE[20]` | `unk_00` | Purpose unknown. | supported | FND-VIEW-013 |
| `0x14` | 4 | `INT32LE` | `texture_count` | Number of `TEX` resources (FND-VIEW-018). | supported | FND-VIEW-013 |
| `0x18` | 4 | `INT32LE` | `block_count` | Number of block records (FMT-VIEW-001). | supported | FND-VIEW-013 |
| `0x1C` | 4 | `INT32LE` | `effect_count` | Number of effect descriptors (FMT-ASSAULT-003). | supported | FND-VIEW-013 |
| `0x20` | 8 | `BYTE[8]` | `unk_20` | Purpose unknown. | supported | FND-VIEW-013 |
| `0x28` | 4 | `INT32LE` | `color_maps_on` | Non-zero: surfaces are shaded by distance. 1 in every archive. | supported | FND-VIEW-011, FND-VIEW-013 |
| `0x2C` | 4 | `INT32LE` | `color_map_count` | Number of maps in a family; 32 in every archive. | supported | FND-VIEW-011, FND-VIEW-013 |
| `0x30` | 4 | `INT32LE` | `distance_shift` | Depth shift of the shading (RULE-VIEW-007). | supported | FND-VIEW-011, FND-VIEW-013 |
| `0x34` | 4 | `INT32LE` | `fade_color` | Palette index the maps fade towards (RULE-VIEW-006). | supported | FND-VIEW-011, FND-VIEW-013, FND-VIEW-017 |
| `0x38` | 512 | `BYTE[512]` | `unk_38` | Purpose unknown. | supported | FND-VIEW-013 |
| `0x238` | | | | Total size 568 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Checked against the `Scenario` resource of all 90 archives that have one: size, the three counts, and the colour-map fields.

## Open questions

- The purpose of `unk_00`, `unk_20` and `unk_38` is unknown.
