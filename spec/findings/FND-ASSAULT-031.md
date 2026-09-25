---
id: FND-ASSAULT-031
title: The damage routine checks reach, rolls a hit chance from both skills, then rolls the combat row's dice against armour
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004F070
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004F10D..0x0004F125
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004F2AC..0x0004F2D6
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004F2DC..0x0004F30E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004CE4C
tool: Ghidra 12.1.3
environment: null
---

## Observation

Damage routine `0x0004F070` takes an attacker and a defender. At `0x0004F10D`..`0x0004F125` it
compares the attacker's combat-row column 4 (the dword at `0x0009CE24 + 28 * row`) plus `0x40`
with the Manhattan distance between their live positions. At `0x0004F2AC`..`0x0004F2D6` the
blow hits when `random(200) < 100 + attacker_skill - defender_skill / 2 + bonus`, where the
skills are field `+0x34`, and the bonus adds 30 when the attacker uses a crossbow and is within
one cell of the player, and 30 when attacker and defender face the same direction. At
`0x0004F2DC`..`0x0004F30E` it reads the dice count, die sides and armour penetration from the
attacker's combat row (dwords 0, 1 and 2), rolls each die as `random(sides) + 1` through
`0x0004CE4C`, and subtracts from the sum the defender's armour less the penetration, where a
negative remainder counts as 0. A negative result is raised to 0.

## Interpretation

Skill makes hits likelier and the defender's skill counts half as much. Striking from behind (the
two facing the same way) and shooting a crossbow at close range each add 30 in 200. Damage is
the row's dice less whatever armour the penetration does not cancel.

## Alternatives

None known.

## How to reproduce

Open `0x0004F070`. The call to the random routine with 200 and the two additions of 30 are at
`0x0004F2AC`; the dice loop follows.
