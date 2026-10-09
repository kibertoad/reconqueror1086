---
id: FND-RES-050
title: INST.EXE runs INSTALL.SCR from vtable entry +0x5C after two installer checks, and one choice of the shipped script's menu misses its label
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 20F3:03D6..20F3:0590
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 20F3:2949..20F3:2975
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:0D8F..1C17:0DC9
tool: capstone 5.0.7 bounded 16-bit disassembly of the LZEXE-unpacked file with relocation targets marked; Python byte searches of its load image; a Python walk of the shipped INSTALL.SCR's commands and labels
environment: null
---

## Observation

The unpacked file, its notation, the object at DS:55D8 with its vtables
DS:1846 and DS:1A14, the routine 20F3:029C and the switch word +0x1F3 are
as FND-RES-066 and FND-RES-045 give them. The script commands are as
FND-RES-055, FND-RES-066, FND-RES-053, FND-RES-052, FND-RES-051, FND-RES-050 and FND-RES-049 give them.

Callers. Entry +0x30 of both vtables is 1C17:01AE and entry +0x5C is
20F3:2949. A search of the load image for the encodings of a far call
through [bx+0x30] (0xFF 0x5F 0x30) finds six; the one at 20F3:296D is in
20F3:2949, and the one at 20F3:1D7B calls +0x30 of the objects in a list
passed as an argument. 20F3:2949 (this) calls +0x30 of this when the byte
at +0x23E differs from the byte at +0x1E6, or when the word at +0x1F3 is
not 0, and otherwise returns. The same search for [bx+0x5C] finds 20 far
calls; the one at 20F3:057D is in 20F3:029C on this; the one at 20F3:31B2
passes four argument words, against the two 20F3:2949 takes, in a routine
that reads fields at +0xB0 to +0xB4.

20F3:029C after its read of `resource.cfg` (20F3:03D6):

- calls +0x0C, 2B44:0009 and 2A64:0002, then +0x60, and returns 1 when that
  returns 0;
- calls +0x38; when the doubleword at +0x1A2 and the word at +0x1F5 are
  both not 0, calls +0x64 and returns 1 when that returns 0;
- at 20F3:0457: when the word at +0x1F5 is not 0, calls +0x34 and returns 1
  when that returns 0; when it is 0, calls +0x04 with the far pointer at
  +0x1EB;
- calls +0x3C of the object at +0x1EB with the byte at +0x1E6, shows a window
  with the text for the key at DS:14F4, calls +0x48 with the flag 1 (the
  second `resource.cfg` read, with the directory) and closes the window;
- calls +0x10 and then +0x28, going back to 20F3:0457 when either returns 0;
- calls +0x6C and then +0x5C, and goes on at 20F3:0583.

`pick`, 1C17:0D8F, gives its stack buffers no value before 1000:620F
fills them; a buffer the format's later `%s` finds no word for keeps the
bytes it held.

The shipped script. Its 93 lines hold 12 labels. Every `goto`, `space`,
`godir` and `pick` word that names a label, except one, is byte for byte the
name of a label later in the file. The exception is the third label word
of the `pick` line, on line 16 (counting from 1): no label is that word
exactly. No label earlier than a command starts with the word it names, so
the backward search never matches. The label on line 53 equals it apart from letter case, and the
label on line 81 is that word followed by one more character. The `pick`
key list has three letters and the line names three labels. Following the
commands:

- The first key leads to lines 18 to 28 and the second to lines 30 to 38;
  each ends with a `goto` to the label on line 40. A `space` failure in them
  goes to an `alert` and `end` on lines 71 to 74 or 76 to 79, and a `godir`
  failure to lines 85 to 87.
- From line 40, a failure of the `godir` on line 48 goes to lines 89 to
  91, and of the one on line 49 to lines 85 to 87; otherwise the run reaches
  line 53, a `pause` and a `goto` to the label on line 81, followed by
  `end`.
- The third key's label is searched forward with an exact comparison. No
  label matches, so every later line is skipped and the run ends at the end
  of the buffer without an `end`.

## Interpretation

The installer runs `INSTALL.SCR` once its own checks (+0x10 and +0x28) have
passed, and only when two drive letters it holds differ or `-f` was given;
`INSTALL.BAT` passes `-f`, so a run from it always reaches the script once
those checks pass. Every route through the shipped script ends at `end` or at
an `alert` box the player can leave by, except the menu's third choice, which
ends the script with nothing shown, where the label on line 53 or line 81
was probably meant. The block from line 53, a message and a pause, is reached only
by the first two choices.

## Alternatives

- The third choice reaches line 53 because labels are compared without
  case: ruled out; the forward comparison 1000:62D2 keeps case.
- It reaches line 81 by a prefix match: ruled out; only the backward
  search, over labels already passed, compares a prefix, and line 81 is
  ahead.

## How to reproduce

Unpack `CD:INST.EXE` as FND-RES-066 gives. Read the far pointers at +0x30
and +0x5C of DS:1846 and DS:1A14. Search the load image for 0xFF 0x5F 0x30
and for 0xFF followed by a byte 0x50 to 0x5F and 0x5C, and decode each hit.
Disassemble the ranges in Locations as 16-bit code with the load image at
segment 0x1000 and relocation targets marked. Split the shipped
`INSTALL.SCR` at CR LF and list each label and each command word that names
one, comparing them byte for byte. Keep listings in ignored local storage.

Whole-function exclusive bounds follow the body extents recorded in FND-RES-062.
