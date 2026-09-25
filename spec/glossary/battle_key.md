# battle_key

A value from outside the game: the code of the waiting key, the character or 0x100 plus the scan code for an Alt key. The original reads it from `0x00065388`, which removes the key [FND-BATTLE-016]. It is read once when `battle_key_ready` is true.
