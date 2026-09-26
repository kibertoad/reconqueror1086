# DEV-UI-001

- Departs from: BUG-UI-001, FMT-UI-003
- Reason: Row 4 of `VILLAGE.DAT` record 37 separates x, y and width with spaces, and the original's `"%d,%d,%d,%d,%d"` read stops after x, so the lender's hot spot takes y, width and height from record 36 (BUG-UI-001). The rebuild reads the row as its text gives it: y 141, width 43 and height 52.
- Setting: None
- Default: mandatory
- Justification: In the eight places that use record 37, the original's hot spot for the lender is not over the lender's house, because of a typo in one line of a data file. The rebuild puts it where the line puts it, over the house, and the lender stays reachable in every place. The departure moves one click area and changes no game state, so the player loses only a hot spot that does not match the picture. The original's result also depends on how its runtime library reads a space, which is not confirmed, so a setting could not promise the original's behaviour.
- Dropped: no
