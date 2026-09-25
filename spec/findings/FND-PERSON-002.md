---
id: FND-PERSON-002
title: CHARACTR.DAT lists 15 characters and 30 attributes
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B2
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The `CHARACTR.DAT` entry of `C1086.GOB`, decoded, is a text file. Its header gives 15 characters
and 30 attributes, then lists the attributes in this order, counted from 0: STRENGTH, DEXTERITY,
PIETY, STAMINA, INTELLIGENCE, HONOR, FAME, ARMOR, HEALTH, HEAD_HEALTH, TORSO_HEALTH,
LEFT_ARM_HEALTH, RIGHT_ARM_HEALTH, LEFT_LEG_HEALTH, RIGHT_LEG_HEALTH, EXPERIENCE_WITH_SWORD,
EXPERIENCE_WITH_LANCE, WEALTH, AGE, COLOR, JOUST_WON, JOUST_LOST, MELEE_WON, MELEE_LOST,
BATTLE_WON, BATTLE_LOST, MONEY_BORROWED, PAY_BACK_MONTH, MARRIED, ARROWS. Each of the 15 character
blocks gives a name and one value for each attribute. Row 0 is the player's; row 8 is the king.

## Interpretation

The field numbers the executable passes to `0x00015EF0` are indexes into this list: 5 is HONOR, 6
FAME, 16 EXPERIENCE_WITH_LANCE, 17 WEALTH and 19 COLOR.

## Alternatives

None known.

## How to reproduce

Extract and decode `CHARACTR.DAT` from `C1086.GOB` and read its header.
