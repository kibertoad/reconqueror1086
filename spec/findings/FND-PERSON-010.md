---
id: FND-PERSON-010
title: DILEM0.DAT to DILEM29.DAT are marker-line text files with three choices of three outcomes
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B2
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The 30 `DILEM*.DAT` entries of `C1086.GOB`, decoded, are 7-bit ASCII text with an optional
closing `0x1A`. Each has `#` comment lines, among them an age and a title; a line
starting `!` followed by a line holding the dilemma number and a `.CSF` name; a line starting `&`
followed by `^` lines of prompt; then three choice blocks. A choice block starts with a line
starting `@`, then a line of `~` and an attribute name, a line starting `%` followed by a line of
two integers, the higher breakpoint first, then three outcome blocks, WIN, DRAW and LOSE, each a
line starting `?`, `^` lines of text, a line starting `*` followed by a count line, and a line
starting `$` followed by that many lines of an attribute name and a signed integer. The numbers
match the file names, and the declared ages form six groups of five, 12 to 17, in number order.
The attribute names are those of `CHARACTR.DAT` and `NONE`; every choice scored on `NONE` has
breakpoints 0 and 0, and every outcome of every file includes `AGE +1`.

## Interpretation

The layout agrees with the markers the executable's parser checks (FND-PERSON-005). The age group
of a number is `number / 5 + 12`, which is how the selection finds it.

## Alternatives

None known.

## How to reproduce

Extract and decode the `DILEM*.DAT` entries of `C1086.GOB` and list their marker lines.
