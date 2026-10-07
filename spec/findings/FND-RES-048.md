---
id: FND-RES-048
title: INSTALL.SCR's if command in INST.EXE tests errorlevel or exist, with an optional not, and resumes the raw line at the expanded command's offset
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:1C63..1C17:1E4A
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1000:65EA..1000:66B0
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 3583:116A..3583:11ED
tool: capstone 5.0.7 bounded 16-bit disassembly of the LZEXE-unpacked file with relocation targets marked
environment: null
---

## Observation

The unpacked file, its notation, the run loop, the dispatch, the message box
1C17:0745 and the program runner's store of a result word into DS:5340 are as
FND-RES-055 gives them; 1C17:1F1B is as FND-RES-047 gives it.
A byte search of the load image for 0x40 0x53 finds the store at 1C17:0731
in the program runner and the read at 1C17:1D2B below, and nothing else.

1000:65EA (string, delimiters) keeps its position in the far pointer
DS:5A72, which a non-null string replaces. It skips bytes found in the
delimiters, returns null when it reaches a NUL, and otherwise returns the
position, stores a NUL over the next delimiter and keeps the byte after it.
1C17:1E09 (string, delimiters) calls it, keeps the result in DS:5346, and,
when the result is null, calls 1C17:0745 with `Missing parameter in command:
%s` (DS:11CD) and the line at DS:5342, then returns.

`if`, 1C17:1C63 (text):

- It sets the result si to 0 and the wanted value di to 1, copies the text
  through 1000:6302 into a stack buffer at [bp-0x106] with no bound, and
  takes the first token through 1C17:1E09 with space, tab and LF (DS:116A).
- When the token equals `not` (DS:116E) through 1000:63B6, di becomes 0
  and the next token is taken.
- When the token equals `errorlevel` (DS:1172), the next token is taken and
  its number n read through 1C17:1F1B. When n is 0, 1C17:0745 shows
  `Invalid IF comparison value: '%s'` (DS:117D) with the token. si becomes
  1 when the word DS:5340 is not less than n, compared signed, and 0
  otherwise.
- Otherwise, when the token equals `exist` (DS:119F), the next token is
  taken and si becomes 1 when 1000:45DF finds it with attribute 0, and 0
  otherwise.
- Otherwise 1C17:0745 shows `Unsupported IF parameter in command: %s`
  (DS:11A5) with the line at DS:5342, and si stays 0.
- It then takes one more token. When si equals di, it sets DS:533C to
  DS:5338 plus the token's offset in the copy plus the text's offset in the
  line at DS:5342, and moves DS:533C past spaces and tabs through
  1C17:08F9. When si differs from di, it leaves DS:533C as it was.

Each keyword comparison goes through 1000:63B6 even when the token before
it was null.

## Interpretation

`if [not] errorlevel n command` runs the command when the last program the
script ran returned n or more (or, with `not`, less than n). `if [not]
exist pattern command` runs it when a file matches (or, with `not`, when
none does). The command is not run inside `if`: the loop's next pass starts
in the script's own line at the command's position, so the command is
expanded and dispatched like any other line. That position is measured in
the expanded line and applied to the unexpanded one, so a parameter before
the command whose value is not exactly as long as its two-character
reference moves the restart point. Any other test word shows an error box
and skips the command.

## Alternatives

- `if` compares strings with `==` as DOS batch files do: ruled out; only
  `errorlevel` and `exist` are accepted, and any other word shows the
  unsupported-parameter box.
- `if` runs its command itself through the dispatch: ruled out; it only moves
  the next-line pointer.

## How to reproduce

Unpack `CD:INST.EXE` as FND-RES-054 gives. Disassemble the ranges in
Locations as 16-bit code with the load image at segment 0x1000 and relocation
targets marked, read the strings from DS:116A to DS:11ED, and search the
load image for 0x40 0x53. Keep listings
in ignored local storage.
