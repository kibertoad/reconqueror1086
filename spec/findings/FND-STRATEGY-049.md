---
id: FND-STRATEGY-049
title: New-game setup retains the selected person's group, rating and coordinate outputs
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000110E8..0x00011280
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004389C..0x000438C5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000437B0..0x000437C9
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00063E20..0x00063EBF
tool: Capstone 5.0.9 over the fingerprint-verified relocated LE code object
environment: null
---

## Observation

The caller stores the selector's EAX in `fallback_person`, pushes that same
dword into the group getter, and stores its EAX in `fallback_origin`.
It then loads `fallback_person` again, passes it with dword 12 to the rating
setter, and removes both arguments. The setup introduces no seven-home test
or replacement of the selected person between these calls.

The group getter reads a dword index. Zero returns zero; a signed comparison
returns zero for indexes above 176. Otherwise it addresses the 18-byte person
record and returns its byte `group` zero-extended. The comparison does not
reject negative indexes. This describes the getter's branch, not safe access
for negative values or the validity of index 176. The rating setter forms the
same stride address without a bound test, takes the second argument's low
byte, and stores that byte in `rating`. It makes no call and does not return
a replacement person identifier. The setup ignores its return register.
For the selector's endpoint person 28, these calls use record 28, rather than
home index 7; the rating becomes 12 and its group becomes the fallback origin.

The setup passes `home_row`, `home_col` and two distinct stack-local output
pointers to the coordinate conversion. Four saved registers in the callee
place these arguments at stack offsets 20, 24, 28 and 32. The caller removes
16 bytes, reads both output dwords, stores them as the sixth player record's
integer destinations and converts each signed dword to a single-precision
floating position. It also copies the original home row and column into that
record. Its remaining loops and reset stores are the ones in FND-STRATEGY-045.

The conversion reads `g_000AB0F8` as its row multiplier and `g_000AB100` as
its vertical parameter, and restores the latter unchanged before returning.
Its two branches are selected by the column's signed remainder on division
by two. For fabricated nonnegative coordinates with both parameters 80, both
branches produce the anchor described by RULE-STRATEGY-014. No initial value
or complete producer search for these globals is established here.

## Interpretation

The selected endpoint survives the caller's immediate group, rating and
coordinate consumers. A home-index clamp cannot be inferred from those calls.
The selected person's group may matter to later hostile generation; reading
these immediate consumers does not settle every downstream route or property
consumer required by Q-STRATEGY-043.

## Alternatives

Sending drawn index 7 into the group or rating helper conflicts with the
caller's stored return and subsequent pushes. Reading the coordinate outputs
as the callee's register return conflicts with the output-pointer writes and
the caller's stack-local reads. This is not a complete reading of every caller,
all coordinate-parameter writers or later fallback-origin use.

## How to reproduce

Verify BLD-GOG-EN, apply the LE fixups and decode the listed exclusive ranges.
Track each push, cleanup and saved-register offset, the getter's signed bound,
the setter's one-byte store and the two coordinate output pointers. Follow the
returned selected person independently of the home-index argument distinguished
by FND-STRATEGY-047. Compare the fabricated whole-setup cases in EXP-STRATEGY-003.
