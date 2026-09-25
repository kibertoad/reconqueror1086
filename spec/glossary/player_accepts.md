# player_accepts

A value from outside the game: whether the player accepts the wager a tournament offer shows, true or false. The original reads it from the dialog routine `0x00025004`, which returns 0 when the player declines [FND-TOURNEY-004, FND-TOURNEY-005]. It is read once for each offer.
