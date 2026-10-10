# march_age_flags

The per-character dwords at row-record offset `+0x54`, reached through the
character-table header and row-pointer list. The March age pass tests only row
zero's flag to gate the whole table, sets every visited flag to 1 after aging,
and clears the visited flags outside March when row zero is nonzero
[FND-PERSON-012]. Loading a character initializes its flag to zero
[FND-PERSON-003]. This name describes those observed producers and consumers,
without claiming that every other access has been read.
