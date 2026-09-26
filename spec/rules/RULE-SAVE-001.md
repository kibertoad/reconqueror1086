---
id: RULE-SAVE-001
title: Load and save screens
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-SAVE-001]
conflicting: []
split_with: []
related: [RULE-SAVE-002, RULE-SAVE-003, RULE-RES-001, FMT-SAVE-001, SCR-SAVE-001, SCR-SAVE-002]
---

## Summary

Both screens list the titles of the five slots and take a slot by click or by the keys `1` to `5`.
Saving asks for a title of up to 40 characters, starting from the slot's old title, and cannot be
cancelled once a slot is picked. Loading stays on the screen until a load succeeds or the player
leaves.

## When it runs

When the player picks Load or Save on SCR-UI-002.

## Parameters

- `skip`: the slot whose title is not drawn, or 0.
- `slot`: 1 to 5.
- `x`, `y`: where the title editor draws.
- `title`: the text being edited.

## Inputs

`key_waiting` and `typed_key`, values from outside the game; the files in `SAVEGAME\`.

## Procedure

```text
define slot_file_name(slot: INT32) -> char[]:
    return sprintf("CONQ%d.SAV", slot)

define read_slot_title(slot: INT32) -> char[]:
    let path = sprintf("SAVEGAME\\%s", slot_file_name(slot))
    # a missing file gives an empty title
    let archive = read_file(path, FMT-SAVE-001)
    # a file without a readable TITLE entry stops the game with fatal error 0
    return archive.TITLE

define draw_slot_titles(skip: INT32):
    for slot in 1..6:
        if slot != skip:
            # read_slot_title(slot) is drawn at (100, 138 + 45 * (slot - 1)) in colour 255
            read_slot_title(slot)

define edit_title(x: INT32, y: INT32, title: char[]):
    # the text is drawn at (x, y) followed by "-" after every change
    while true:
        if key_waiting:
            let key = typed_key
            if key == 13:
                return
            if key < 256:
                if key == 8 and count(title) > 0:
                    remove_at(title, count(title) - 1)
                else if count(title) < 40:
                    append(title, key)
                else:
                    # the PC speaker sounds 300 Hz for 100 ms

define save_game_screen():
    # savegame.pcx is shown, the open archive closed and draw_slot_titles(0) run
    # a slot is picked by the pointer or the keys 1 to 5; back or Esc returns without saving
    let slot = 1
    draw_slot_titles(slot)
    let title = read_slot_title(slot)
    edit_title(100, 138 + 45 * (slot - 1), title)
    call RULE-SAVE-002(slot_file_name(slot), title)
    # C1086.GOB is opened again

define load_game_screen() -> INT32:
    # loadgame.pcx is shown, the open archive closed and draw_slot_titles(0) run
    # each slot picked by the pointer or the keys 1 to 5 runs the line below until it gives 1;
    # back or Esc leaves and gives 0
    let loaded = call RULE-SAVE-003(slot_file_name(1))
    # C1086.GOB is opened again
    return loaded
```

## Outputs

The load screen gives 1 when a game was loaded and 0 when the player left.

## Edge cases

- An empty slot shows no title; loading it fails quietly and the screen stays.
- A 40-character title is stored without its NUL.
- `Esc` and a Backspace on an empty title are typed into the title (BUG-SAVE-002).
- `SAVEGAME\` must exist under the current directory, or saving stops the game.

## What the sources say

The manual describes loading and saving games from the options screen (SRC-MANUAL); it gives no file names.

## Differences between builds

None known.

## Open questions

- How the slot picked by the pointer is read; the procedure writes slot 1 for it.
