---
id: FND-RES-032
title: _SETUP.EXE, expanded from SETUP.SOL, loads SIERRA.INF with section markers and reads LANGUAGE.INF through the profile routines
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:SETUP.SOL
    offset: 0x1115F..0x47E42
tool: Python decoder of RULE-RES-005; Python bounded NE header and relocation-chain walk; capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

`_SETUP.EXE` ships only as a member of `CD:SETUP.SOL`, whose stored stream
is at the offsets above (FND-RES-030). Expanded as RULE-RES-005 gives, it is
454,016 bytes with XXH3-128 `5274aa318f8af23efc455b25f215cc23`, an MZ file
whose header points at an NE header at 0x80. The NE header lists 9 segments
and 12 imported modules: EREGLIB, KERNEL, GDI, USER, KEYBOARD, WIN87EM,
COMMDLG, DDEML, LZEXPAND, MMSYSTEM, SHELL and VER. Segments 1 to 5 are code
at file offsets 0x4C0, 0xD2C0, 0x19840, 0x28DE0 and 0x38100, of 52,057,
49,083, 61,371, 61,424 and 10,882 bytes; segment 9 is data at 0x3AC60, of
10,136 bytes; segments 6 to 8 have no file data. Addresses below are
`segment:offset` in this expanded file, which the build's file list does not
name (see Interpretation).

Imports are by ordinal, named as FND-RES-029 names KERNEL's. The relocation
chains of segments 1 to 5 hold 97 far calls to the profile routines: 42 to
ordinal 128 (GetPrivateProfileString), 19 to 127 (GetPrivateProfileInt), 34
to 129 (WritePrivateProfileString), one to 58 (GetProfileString) and one to
59 (WriteProfileString). For each, the arguments were read from the pushes
before the call; a pushed segment word that the relocation table fixes to a
code segment, followed by a pushed offset, is a constant string in that
segment.

- Constant file `SIERRA.INI`: sections `Setup` (keys `Purge`, `PurgePath`,
  `Presetup`, `Restart`, `Reboot`), `UserInfo` (names, address, phone,
  `Gender` and twelve `Male1` to `Female6` flags), `Misc` (`Language`,
  `ProductDir`, `DirectXSourceDir`, `CDCheck`), `Sierra` (`SierraDir`),
  `SierraDirs`, `Config` (`VideoSpeed`) and `Last Test`.
- Constant file `SYSTEM.INI`: `[boot]` `shell`. WIN.INI through ordinals 58
  and 59: `Intl` `sLanguage` (default `ENU`) and `WinG` `ProfileMessage`.
- A file passed in a variable, with constant section and key: `Setup` with
  `SkipReplaceWarning`, `BillboardSize`, `Win95Logo`, `Debug`,
  `AnimationDLL` and `InstallType`; `Ident` with `Title`, `ShortTitle`,
  `Version` and `ProductID`; `Requirements` with `SetupVer`; `Misc` with
  `SourceDir`.
- Section `Strings` with a key passed in a variable, at 0004:34DF and
  0004:366C (default empty, buffer 0x200) and at 0004:472C (default `Sierra`)
  and 0004:496B (default empty), both buffer 0x50; each passes the far
  pointer held at offset 0x148 of the object it was given as the file name.
  0004:6C72 and 0004:6CC2 read `Ident` `ShortTitle` from the same field.
  0003:CE87 and 0003:CFC3 read `Strings` from a file in a local variable.
- No call passes a constant section named `Script`, `Files`, `Dialogs`,
  `Archives` or `Billboards`. Calls whose section is a variable remain:
  0002:AA42, 0002:AA88, 0003:5994, 0004:103D, 0004:1091, 0004:10ED,
  0004:1185, 0004:4323, 0004:4394, 0004:69C5 and 0004:D031.

The object field at 0x148 holds a far pointer to text, pushed as the file
name above. Every instruction in segments 1 to 5 with an operand of 0x148 or
0x14A was listed (a sweep that skips undecodable bytes), and the raw bytes
`48 01` occur 11 times in segment 4 and once in segment 1, all accounted for
by those instructions or by unrelated bytes. Apart from the pushes above,
0004:2993 and 0004:2A0B pass the field's address to the library routines at
0002:09B8 and 0002:0A74, which set up and release it around the object, and
two places write it:

- 0004:46C6 to 0004:46F5: when the word at offset 0x12E of the object is not
  0, it joins the text at offset 0x12A with `\LANGUAGE.INF` (a constant in
  segment 3) through the library routine at 0002:0CD2 and stores the result
  at 0x148 through 0002:0B9C. 0004:44F7 writes the text at 0x12A to
  SIERRA.INI as `Misc` `ProductDir`.
- 0004:2CE9 passes the field to 0004:CDB6, with the text at offset 0x132 of
  the object.

0004:CDB6 (far, returns in AX) takes a path, an output field and a far
pointer. It cuts the path after its last `\` (0x5C), adds `LANGUAGE.INF`,
and tests the result with the C library's file test at 0001:4632, which
issues DOS function 0x4300 (through KERNEL ordinal 102 when the word at CS:0x10
has bit 0 set, otherwise INT 21h). When the file is there, it stores that
path in the output field and returns 1. Otherwise it joins a directory held
at offset 0xD6 of the object whose far pointer is at DS:0x0FB4 with
`\LANGUAGE.INF`, stores that in the output field and tests it the same way:
found, it returns 1; not found, it calls 0004:C70A with the third argument
(when that pointer is not null) and the values 0x63, 0xC8, 0 and 0x10, and
returns 0.

The call at 0004:2CE9 lies in the function that starts at 0004:2C84,
which first stores five far pointers to
constants of segment 4 in its frame, in this order: `[Archives]`, `[Files]`,
`[Dialogs]`, `[Script]` and `[Billboards]`, followed in the segment by the
four bytes `, \t\n`. It passes `\SIERRA.INF` (segment 3) and the object's
field at 0x132 to the library routine at 0002:59E0, then calls 0004:CDB6
with the field at 0x132 as the path, so the LANGUAGE.INF it finds is the one
beside that path or in the fallback directory. When 0004:CDB6 returns 0,
0004:2C84 jumps to 0004:2E8F. Segment 4 also holds the messages that a script
over 64,000 bytes cannot be read; the one at 0004:751E is pushed at
0004:388D.

## Interpretation

`_SETUP.EXE` is the second-stage installer. It reads SIERRA.INF as a whole
file divided by the five bracketed markers, which fits the shipped file's
`[Script]` lines without `=`, and reads the `Strings` and `Ident` sections of
a LANGUAGE.INF beside the script through the Windows profile routines, one
key at a time. Which occurrence of a repeated key such a call returns is
decided by Windows, not by this program. The parts of SIERRA.INF it reads
through the profile routines (`Setup`, `Requirements`, `Ident`) depend on
which files the variable file names hold, which this finding does not trace.

The standard gives addresses only for files in a build's file list, and
`_SETUP.EXE` exists only inside SETUP.SOL, so the location above is the
member's stored bytes and the addresses are given in the text.

## Alternatives

- SETUP reads the other SIERRA.INF sections itself: ruled out by FND-RES-029,
  which shows SETUP reads only two `Setup` keys.
- `_SETUP.EXE` reads the `Script` section through the profile routines: no
  call passes that section as a constant; the variable-section calls listed
  above are not yet read.

## How to reproduce

Verify SETUP.SOL's manifest size and XXH3-128, expand its `_SETUP.EXE`
member as RULE-RES-005 gives from the offsets above, and check the expanded
file's size and XXH3-128. Walk its NE segment table and the relocation
records after each code segment, following each chain (an additive record is
one site) with a cap of 4,096 sites, and keep the sites whose record names
KERNEL ordinal 58, 59, 127, 128 or 129. For each, disassemble as 16-bit code
from the first start between 0x21 and 0x70 bytes back that decodes to the
call, and read the pushes after the last call or jump before it. Search the
code segments for operands 0x148 and 0x14A and for the raw bytes `48 01`,
and read 0004:2C84 to 0004:2D03, 0004:46C6 to 0004:46F5 and 0004:CDB6 to
0004:CF48 with the relocation targets beside each instruction. Keep the
expanded file and listings in ignored local storage.
