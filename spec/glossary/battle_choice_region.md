# battle_choice_region

A value from outside the game: the region of the battle choice screen the player clicks, 1 to 5, or 0. The original reads it through the gate `0x000256FC`, which tests the pointer against five rectangles with `0x0006FF10` [FND-BATTLE-003]. It is read once for each battle.
