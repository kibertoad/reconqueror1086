---
id: FND-RES-056
title: INST.EXE looks up 78 fixed INSTALL.TXT keys, help and confirmation texts by file name with hlp and inf extensions, and stops with a fatal error on a missing key everywhere but two calls
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2852:0E49..2852:0F10
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2852:0F10..2852:1070
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2852:1070..2852:1160
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1BA0:00B7..1BA0:00EE
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1BA0:00EE..1BA0:01ED
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2E08:000E..2E08:0115
tool: capstone 5.0.7 bounded 16-bit disassembly of the LZEXE-unpacked file with relocation targets marked; Python byte searches of its load image
environment: null
---

## Observation

The unpacked file, its notation and the lookup 2852:0B71 (result, dictionary,
key, flag) are as FND-RES-053 gives them.

Calls. A search of the load image for the far call bytes 0x9A 0x71 0x0B 0x52
0x18 (2852:0B71 with its segment word before relocation), and for `push cs`
followed by a near call whose target is 2852:0B71, finds 94 far and 4 near
calls, 98 in all. The same search for 2852:00BD finds its one call, in main
at 20F3:2D83, located before this search by reading main (FND-RES-066).
Calls through pointers, jumps and far pointers stored as data were not
searched. At each call the caller pushes the flag word, reserves 4 bytes for
the key string, builds the key there, pushes DS:53AC and the result, and
removes 0xE bytes after the call.

Flags. 96 calls push 1. Two push 0: 1E86:027F and 2852:0EE2.

Keys. 90 calls build the key from a fixed string in DS. They name 78
different keys, and each is a key of `CD:INSTALL.TXT` (FND-RES-053). The
other 60 keys of INSTALL.TXT reach no such call. Eight calls build the key
from data:

| Call | Flag | Key |
|---|---|---|
| 1BA0:02A2 | 1 | the string at +0x10B of the object passed |
| 1BA0:047B | 1 | the string at +0x10B of the object passed |
| 1E86:027F | 0 | a string argument |
| 2012:0101 | 1 | the string at +0x22 of the object passed |
| 23F1:092D | 1 | a string argument |
| 27C0:0139 | 1 | the string at +0xC of the object passed |
| 27C0:018C | 1 | the string at +0x10 of the object passed |
| 2852:0EE2 | 0 | a file name with its extension replaced, below |

2852:0E49 (result, name, extension) cuts the name at its first `.` (found
through 1000:6295), appends `.` and the extension through the format `.%s`
(DS:3018), and looks the result up with the flag 0.

2852:0F10 (name) looks up the name with the extension `hlp` (DS:301C). When
the text is empty, it takes the text of `noHelp` (DS:3020) instead. It passes
the text and the text of `pressKeyToContinue` (DS:3027) to 263F:02BF (not
read), the routine FND-RES-055's message box calls.

2852:1070 (name) looks up the name with the extension `inf` (DS:303A) and
returns 1 when the text is empty. Otherwise it passes the text, the text of
`enterConfirm` (DS:303E) and the keys CR and Esc (DS:304B) to 263F:02BF.

1BA0:00B7 calls 2852:0F10 with the string whose far pointer is at +0xA of its
object, and 1BA0:00EE calls 2852:1070 with the same string when the check
before it gives 0.

Missing keys. With the flag set, 2852:0B71 calls 2E08:0034, which passes
`\n%s : %s(%u)` (DS:3AE8) with the message (or `Fatal error`, DS:3AF5, when
it is empty), the file name and the line to 2E08:000E. That calls the far
pointer at DS:3AE4, whose stored value is 2E08:0087 with a relocation entry on
its segment word; a byte search for 0xE4 0x3A finds only the indirect call
itself, so nothing in the image stores another value there. 2E08:0087 calls
2CB5:000A (not read), formats the arguments into a 0xCA-byte stack buffer,
prints them with `%s%s` and the rest of the format at DS:3B01 to the stream
at DS:4E36, waits until 1000:3290 returns 0x1B, writes LF to the stream and
calls 1000:027A, which ends the program with "Abnormal program termination".

## Interpretation

Every fixed text the installer shows comes from INSTALL.TXT, and a missing
one stops the installer with a message and a wait for Esc. INSTALL.HLP's keys
are file names: the help for a file named `x.drv` is the entry `x.hlp`, with
`noHelp`'s text when there is none, and a file's `x.inf` entry, when present,
is a notice the player confirms with Enter or leaves with Esc. Both lookups
pass the flag 0, so a missing entry is allowed. The objects of the 1BA0 class
carry the file names, at +0xA for these two lookups; the other strings they
look up with the flag 1, at +0x10B, are probably the same kind of name, and
the `.drv` keys of INSTALL.HLP fit them, but which strings those objects hold
was not traced. The 60 INSTALL.TXT keys no fixed call names may reach the
calls that build their key from data.

## Alternatives

- INSTALL.HLP holds one help text per installer screen, keyed by screen:
  ruled out for the two lookups read; they key by file name with the
  extension replaced.
- A missing key gives an empty text everywhere: ruled out; only the two calls
  that pass 0 do that.

## How to reproduce

Unpack `CD:INST.EXE` as FND-RES-066 gives. Search the load image for the
bytes 0x9A 0x71 0x0B 0x52 0x18, and for 0x0E 0xE8 followed by a 16-bit
displacement that reaches 2852:0B71, and the same for 2852:00BD as the
control. At each hit, disassemble back to the `push` before `sub sp, 4` for
the flag, and to the last call of 2FD0:009B for the key, reading the DS
string it is given. Compare the keys with the trimmed, lower-cased keys of
`CD:INSTALL.TXT` and `CD:INSTALL.HLP`. Read the strings at DS:3018 to DS:304B
and DS:3AE8 to DS:3B1C and the far pointer at DS:3AE4. Disassemble the ranges
in Locations as 16-bit code with the load image at segment 0x1000 and
relocation targets marked. Keep listings in ignored local storage.

Whole-function exclusive bounds follow the body extents recorded in FND-RES-062.

Grouped whole-function exclusive bounds follow the extents recorded in FND-RES-063.
