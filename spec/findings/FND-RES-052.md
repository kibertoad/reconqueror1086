---
id: FND-RES-052
title: INST.EXE's bytes behind INSTALL.SCR's %1 and %2 are the destination drive letter and the drive the installer started on
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 20F3:0A77..20F3:0AD4
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 20F3:0AD4..20F3:0FE4
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1000:08E6..1000:0900
tool: capstone 5.0.7 bounded 16-bit disassembly of the LZEXE-unpacked file with relocation targets marked; Python byte searches of its load image
environment: null
---

## Observation

The unpacked file, its notation, the object at DS:55D8 and its vtables are as
FND-RES-066 gives them, the flow of 20F3:029C as FND-RES-050 gives it, and
the parameters `%1` and `%2` as FND-RES-055 gives them: the bytes at +0x1E6
and +0x23E.

Writes. A byte search of the load image for 0xE6 0x01 and 0x3E 0x02, each
hit decoded with the instruction it ends, finds stores to these bytes at
three places: 20F3:0A84 (+0x23E), 20F3:0ABF (+0x1E6) and 20F3:0F8C (+0x1E6).
The other hits read them or are other instructions.

20F3:0A77, vtable entry +0x38 (this):

- It stores the result of 1000:08E6 plus 0x61 at +0x23E. 1000:08E6 passes a
  stack word to 1000:0674 (not read) and returns that word.
- It calls 1000:08E6 again. When the result is 1 or less it returns 0.
- Otherwise, when vtable entry +0x40 returns 0 and the words at +0x1F3 and
  +0x1D6 are both 0, it copies +0x23E to +0x1E6, sets the word at +0x1F5 to
  0 and returns 1. In every other case it returns 0.

20F3:0AD4, vtable entry +0x34 (this), builds a list at the far pointer
+0x1EF when that is null, runs a selection on it, and near its end stores
the byte at +2 of the entry 2D0F:0070 (not read) returns from the list,
plus 0x61, at +0x1E6. It returns 1 when the selection result is not -1.

20F3:029C calls +0x38 before 20F3:0457 and, from 20F3:0457, calls +0x34 when
the word at +0x1F5 is not 0 (FND-RES-050). The word at +0x1D6 is the flag
`floppy=yes` sets in `RESOURCE.CFG` (FND-RES-066), and +0x1F3 the flag the
`F` switch sets (FND-RES-045).

## Interpretation

Reading 1000:08E6 as the run-time library's current-drive call (0 for A),
`%2` is the lower-case letter of the drive the installer was started on and
`%1` the lower-case letter of the drive it installs to. When the installer
starts on a drive past B, `+0x40` approves, and neither `-f` nor
`floppy=yes` is given, it installs to that same drive without asking, and
the script does not run (FND-RES-050). Otherwise the player picks the drive
from a list, and the script runs when it differs from the starting drive.
From `INSTALL.BAT`, which passes `-f`, the player always picks.

## Alternatives

- `%1` is the CD drive: ruled out for the pick path, which stores the chosen
  list entry's drive.

## How to reproduce

Unpack `CD:INST.EXE` as FND-RES-066 gives. Search the load image for 0xE6
0x01 and 0x3E 0x02 and decode the instruction each hit ends. Disassemble the
ranges in Locations as 16-bit code with the load image at segment 0x1000 and
relocation targets marked. Keep listings in ignored local storage.

Whole-function exclusive bounds follow the body extents recorded in FND-RES-062.
