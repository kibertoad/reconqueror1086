---
id: EXP-PERSON-001
title: Isolated March aging follows the first-row flag, milestones and signed count
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11, Unicorn 2.1.4 x86-32, Capstone 5.0.9
starting_state: emulated-call
recording: null
repetitions: 12
fixture: EXP-PERSON-001.json
---

## Question

Does the pass read in FND-PERSON-012 increment freshly loaded AGE 12 to 13 in
month 2, and do its first-row gate, other-month reset, milestone and signed
count branches behave as read?

## Setup

Load the fingerprint-verified BLD-GOG-EN LE image and its relocations. Call
the pass with no arguments, using a fresh stack and fabricated RAM for each
case. The calendar has the chosen zero-based month; the character header has
30 attributes, the chosen signed character count and a two-element row-pointer
list. Each row record has a separate attribute array and its `march_age_flags`
value. These structures are fabricated, not original character bytes or saves.

Each row's STR, DEX, STAM and INT start at 0, 19, 20 and -1 respectively;
PIETY starts at 17 and all other attributes except AGE at zero. The negative
skill is deliberate: a milestone must use the setter's limits, whereas an
unvisited field remains unchanged. Cases vary the month, first and second flags,
AGE and count as the fixture lists. The count-zero and negative-count controls
still have an accessible row-zero record: they test the loop boundary after
the first flag read, not the validity of a loaded zero-row table.

All pointees are ordinary readable/writable RAM. No video RAM, ports, service,
interrupt or import stubs are provided; any such access fails. There is no
window, timer, input or sound device. The call is confined to the pass and
the actual month, count, flag and attribute helpers, including FND-RES-062's
exclusive setter bound. Each case must return within 2,000 instructions.
There is no RNG draw or seed input to vary.

## Procedure

Run tools/emu/verify_march_age.py with GAME_DIR naming the owned evidence
directory containing disc-root/CONQUER.EXE and an ignored local report path.
The command skips without GAME_DIR and refuses another executable identity.
Compare all fabricated RAM, not only the reported fields. Admit writes only
to the prescribed AGE, milestone skills and flags, the unchanged character
root store made by the setter, and the bounded synthetic call stack. Verify
that both original state-root pointers and all other fabricated bytes stay
unchanged. Reject an unexpected instruction path or a failure to return.

## Observations

All twelve cases passed. Fresh March rows aged from 12 to 13 and from 19 to
20 even though the second row's incoming flag was already 1. A nonzero first
flag skipped all aging, including a second row whose flag was zero. Outside
March, a zero first flag left the second flag unchanged, while a nonzero
first flag cleared both. The four computed ages 20, 23, 26 and 29 each
incremented exactly STR, DEX, STAM and INT, with the setter's limits.

The overflow and negative-AGE controls produced the setter's lower bound of
zero without a milestone. Nonpositive counts skipped the row loop after
reading the accessible first flag. Full-region comparisons, root checks,
instruction paths and write-domain controls passed without a stub.

## Results

The fixture gives exact field and flag outcomes for each case. The fresh-March
case independently corroborates the producer needed to turn file AGE 12 into
13; it does not establish when the main game invokes that producer.

## Conclusion

The isolated pass corroborates FND-PERSON-012's branch reading. It does not
establish calendar timing, every caller or flag writer, loaded-table validity,
all rerolls, a full new-game state or full-game recording/replay.
