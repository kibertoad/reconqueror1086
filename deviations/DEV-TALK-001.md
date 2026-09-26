# DEV-TALK-001

- Departs from: BUG-TALK-001, RULE-TALK-003
- Reason: The original's conversation variable read and write accept a number equal to the count in `ALL.VTB`, 190 in the GOG archive, and reach the four bytes after the table (BUG-TALK-001). The rebuild refuses that number the way it refuses any other number outside the table.
- Setting: None
- Default: mandatory
- Justification: The four bytes after the table do not belong to the variable table, so what a script reads there, and what a write there overwrites, depends on the original's memory layout and is not game state. The rebuild keeps every variable the table holds and refuses only the number past its end. Whether any shipped script names that number is not known. If none does, no player sees a difference; if one does, the original's result cannot be reproduced without its memory layout, so a setting has nothing to offer.
- Dropped: no
