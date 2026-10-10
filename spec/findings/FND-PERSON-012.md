---
id: FND-PERSON-012
title: The March age pass runs before input and increments freshly loaded characters
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002B1EC..0x0002B2EF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015F98..0x00015FAA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015FAC..0x00015FC2
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015FDC..0x00015FE5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015FF8..0x00016008
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029FF4..0x0002A015
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000384E0..0x00038595
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003866C..0x00038675
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00038708..0x0003871A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00038853..0x00038857
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002A8B5..0x0002A8EB
tool: Bounded Capstone 5.0.9 reading of the fingerprint-verified relocated LE image
environment: null
---

## Observation

The age pass at `0x0002B1EC` calls `0x0003866C`, which reads the dword at
`+0x18` of the calendar block reached through `0x0009AE00`. FMT-SAVE-004
and FND-STRATEGY-014 identify that value as zero-based `current_month`.
When it is 2, the pass reads row zero's record dword at `+0x54` through
`0x00015F98(0)`. A nonzero value returns without changing any character.

With a zero first-row flag it visits rows from zero while the signed row
index is below the character count returned by `0x00015FDC`. For each row,
it gets field 18 with `0x00015EF0`, computes the 32-bit value AGE plus one,
and passes it to `0x00015F0C(row, 18, value)` at `0x0002B22F`.
FND-PERSON-001 describes this setter's lower bound and its absence of an
upper bound for AGE. If the computed value equals 20, 23, 26 or 29, the
pass also increments fields 0, 1, 3 and 4, through the same getter/setter
pair. FND-PERSON-002 identifies those as STR, DEX, STAM and INT. These
increments therefore keep the setter's skill limits. The milestone tests
use the computed value before any setter clipping, including overflow.

It then stores 1 in that row's `+0x54` through `0x00015FAC(row, 1)` and
continues with the next row, rereading the count. For any month other than
2, it returns unchanged when row zero's flag is zero; otherwise it stores
zero in every row's flag without modifying AGE or skills. The first row's
flag gates the entire pass: other rows' incoming flags are not tested.
The two flag helpers follow the same character-header and row-pointer
chain as the attribute helpers. The count getter reads header `+0x0C`.
FND-PERSON-003 records that the character loader initializes each flag to
zero. These flag accesses do not use the adjacent attribute-list pointer
at record `+0x50` as the flag itself.

The startup sequence at `0x00029FF4` seeds the RNG, calls the calendar
constructor with arguments `(2, 16, 1086, 0, 0, 0)`, then calls the
character loader. On the constructor's successful allocation path, its
first three arguments are stored at calendar `+0x18`, `+0x1C` and `+0x20`,
and it stores 1 at `+0x30`. The observed entry branch of the update at
`0x00038708` returns without advancing the block when `+0x30` is nonzero.
This describes those stores and that branch only, not allocation failure,
all calendar producers or the units of its clock-related fields.

The main-loop segment calls the calendar update at `0x0002A8BC`, checks
character-table presence with `0x00015FF8`, and calls the age pass at
`0x0002A8CA` when present. Table presence is the nonnull header pointer,
not a row-count check. The input dispatch call at `0x0002A8E6` follows this
pass and the intervening conditional estate work. Thus freshly loaded
AGE 12 with zero flags in month 2 can become AGE 13 before the first
generation answer, independently of a dilemma modifier.

## Interpretation

The code supplies a specific producer for the extra initial AGE year:
the March pass treats a freshly loaded table as not yet aged that March.
FND-PERSON-011's AGE 12 describes the file value, not a guarantee that the
first youth answer sees 12. The same row-zero flag prevents another
increment on subsequent calls in that month, until a reset or reload.

## Alternatives

This is not a complete reading of the main loop, calendar lifecycle or
every flag/AGE writer. The observed startup stores and ordering identify
a candidate initial producer; a controlled AGE observation and isolated
pass must check it. No claim about real elapsed time, every reroll route,
all later birthdays or another edition follows from this reading.

## How to reproduce

Verify the BLD-GOG-EN executable identity and apply its recorded LE
relocations. Read the located pass and its attribute, flag, count and
month helpers, tracing each pushed argument and each row's last flag
write. Compare all four milestone branches and both non-March outcomes.
Read the startup constructor call, its successful stores, the update's
nonzero control branch and the main-loop call ordering separately. Use
FND-PERSON-003/011 for the loader's initial flag and AGE rather than
assuming the manual's starting-age description is the first answer's state.
