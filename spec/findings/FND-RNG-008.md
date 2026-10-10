---
id: FND-RNG-008
title: Environment suffix parsing writes selector-dependent record fields and default clock components
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008B38C..0x0008B492
tool: Capstone 5.0.9, bounded x86-32 decoding of the fingerprinted relocated LE image
environment: null
---

## Observation

The wrapper read in FND-RNG-006 advances past a comma before each call to
this routine, passing a source string and one of its two record destinations.
The routine saves two registers and reserves sixteen local bytes; its source
and destination arguments are then at stack offsets 28 and 32. It calls only
the decimal helper read in FND-RNG-007. Each nested call supplies a pointer to
a local word and the current source, then restores eight argument bytes.

The initial selector is -1. Uppercase `J` sets it to one and advances the
source. The following test for uppercase `M` is independent: it sets the
selector to zero and advances the source, even after a consumed `J`.
The selector is written to the destination's 32-bit field at offset 32 before
the decimal helper runs. No missing-number or invalid-prefix error prevents
this record write.

For a nonzero selector, the first decimal result is stored at offset 28.
For selector zero, that result minus one is stored at offset 16; one dot
permits a second decimal result at offset 12, and another permits a third
at offset 24. The field at offset 28 is set to zero on this path.
Missing dots leave their corresponding fields unchanged. A dot followed by
no digits stores the helper's zero. A missing first number on the zero-selector
path therefore stores -1 at offset 16. No component range check follows.

Three local clock components start at hours two, minutes zero and seconds
zero. A slash permits an hours parse, followed by optional colon-separated
minutes and seconds. These use the same unsigned-digit, wrapping decimal
helper, without sign handling. A slash with no digits replaces the default
hours with zero. The routine always stores seconds at offset zero, minutes
at offset four and hours at offset eight. It returns the pointer following
the last parsed digit sequence or delimiter it consumed. A nondigit suffix
can remain unconsumed; there is no separate failure flag.

The nine-word destination is not cleared as a whole. Offset 20 is never
written here; the fields belonging to unselected or absent components also
retain their incoming values. EXP-RNG-003 uses distinct initial values to
check these retained fields as well as the prescribed writes.

## Interpretation

This routine parses two selector forms and an optional clock into a partially
updated record. A caller must supply meaningful initial values for fields
that a chosen form does not write. The wrapping digit helper and absent
validation permit incomplete and oversized components. The record offsets
remain neutral here: their complete calendar/classification meanings depend
on the consumers still under Q-RNG-001.

## Alternatives

A zero-filled output record is ruled out by the selected stores and the
unchanged-field controls. Requiring exactly one of the two prefix letters
is ruled out by the sequential independent tests, corroborated by the `JM`
case. Treating a missing slash component as the original default hour is
ruled out by the numeric helper's zero store after a consumed slash.
General time-zone validity and the full seed-conversion meaning remain open.

## How to reproduce

Decode the listed range in thirty-two-bit mode and follow the two arguments,
selector, four local words and numeric helper return through all branches.
Read its decimal callee in FND-RNG-007 and its comma-skipping caller in
FND-RNG-006. Compare all destination words, source bytes and returned positions
with EXP-RNG-003 before interpreting the downstream record consumers.
