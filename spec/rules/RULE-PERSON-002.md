---
id: RULE-PERSON-002
title: Loading and saving the character table
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-PERSON-001, FND-PERSON-002, FND-PERSON-003, FND-RNG-002]
conflicting: []
split_with: []
related: [RULE-PERSON-001, RULE-RNG-001, FMT-PERSON-001]
---

## Summary

The game builds its 15 characters from `CHARACTR.DAT` at startup and on a reroll, and each time
moves the player's first 15 attributes by a random -8 to 8 without holding them to 0 to 20. A saved
game keeps the table in a file of the same layout and reloads it as it was.

## When it runs

At startup, on a reroll (RULE-PERSON-003), and when a game is saved or loaded.

## Parameters

`path`, `char[]`, and for `load_characters` `shift`, true or false.

## Inputs

The file.

## Procedure

```text
define load_characters(path, shift):
    # any failure to read the layout ends the program with exit code 1
    let t = read_file(path, FMT-PERSON-001)
    character_count = t.character_count
    attribute_count = t.attribute_count
    for i in 0..attribute_count:
        attribute_names[i] = t.attribute_names[i]
    for r in 0..character_count:
        character_names[r] = t.characters[r].name
        for field in 0..attribute_count:
            character_attributes[r][field] = t.characters[r].values[field]
        if shift and r == 0:
            for field in 0..15:
                let negative = random_inclusive(1)
                let amount = random_inclusive(8)
                if negative != 0:
                    amount = -amount
                character_attributes[0][field] = character_attributes[0][field] + amount

define start_game_characters():
    load_characters("CHARACTR.DAT", true)

define save_characters(path):
    write_file(path, character_table)

define restore_characters(path):
    load_characters(path, false)
```

## Outputs

The character table.

## Edge cases

The shift writes the field directly, so a value can end below 0 or above 20 until the next
`set_attr` on that field. The generator is drawn 30 times for each shifted load.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The name of the file `save_characters` writes, and what else the save and load do.
