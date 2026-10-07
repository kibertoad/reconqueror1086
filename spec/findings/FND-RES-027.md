---
id: FND-RES-027
title: AUTOPLAY's name and title comparisons fold only ASCII capital letters
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x004112B1..0x004112EA
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x00410A81..0x00410A95
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x00410AB1..0x00410AC5
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x00410B88..0x00410B9C
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x004113D6..0x004113F1
tool: capstone 5.0.7 bounded disassembly of the listed AUTOPLAY ranges; Python whole-section scan for direct calls
environment: null
---

## Observation

AUTOPLAY's length and canonical fingerprint matched BLD-GOG-EN, and the
section mapping is FND-RES-024's.

`fn_004112B1` takes two string addresses in EAX and EDX and has one loop. Each
pass loads one byte from each string. A byte from 0x41 to 0x5A inclusive gets
0x20 added; no other byte changes, so bytes above 0x7F and all other values
are compared as stored. When the two bytes differ, or both are 0, the loop
ends; otherwise both addresses advance by one. On exit it zero-extends the
first string's byte and the second string's byte to 32 bits and returns
their difference in EAX. The result is 0 exactly when the strings are equal
up to and including the terminating 0 after that folding. There is no length
limit and no other exit.

A scan of every byte of BEGTEXT for an `E8` byte whose 32-bit relative
operand reaches 0x004112B1 found four sites, at 0x00410A8C, 0x00410ABC,
0x00410B93 and 0x004113E8, each of which decodes as a call; the decoded
listing shows the same four. As a positive control, the same scan for
0x004107DC finds the call at 0x004105A2 that FND-RES-026 reads. No 32-bit
absolute copy of 0x004112B1 occurs in the file. The scan does not cover
calls through a register or a stored pointer.

The callers:

- 0x00410A8C, in `fn_004109B8`: the found directory entry's name against the
  `DirName` value. The caller tests all of EAX for zero; zero leads to the
  LANGUAGE.INF check that FND-RES-026 describes.
- 0x00410ABC, in `fn_004109B8`: the entry's name against `SIERRA`. The
  caller turns a zero EAX into a flag of 1 and anything else into 0.
- 0x00410B93, in `fn_00410B20`: the `Title` read from LANGUAGE.INF against
  the `Title` read from AUTORUN.INF. A nonzero EAX makes the routine return
  0.
- 0x004113E8, in the library routine at 0x00411328 that the routine at
  0x00411304 calls: when the global at 0x004205C8 is nonzero, the name being
  opened against `con`. That routine is not read further here.

## Interpretation

AUTOPLAY matches the `DirName` directory, the `SIERRA` directory and the
`Title` of LANGUAGE.INF without regard to the case of ASCII letters, and byte
for byte otherwise. A title or directory name with an accented letter matches
only with the same byte in both places.

## Alternatives

- A comparison that uses the C library's locale or Windows' `lstrcmpi`:
  ruled out, since the routine calls nothing and folds only 0x41 to 0x5A.
- A comparison of a fixed number of bytes: ruled out, since the only exits
  are a difference and a shared 0.

## How to reproduce

Verify AUTOPLAY's manifest size and XXH3-128 and map BEGTEXT from file offset
0x400 to 0x00410000. Disassemble 0x004112B1 to its return and each listed
caller range. Scan every BEGTEXT offset for `E8` with a relative target of
0x004112B1, and of 0x004107DC as the control, and search the whole file for
the little-endian dword 0x004112B1. Keep listings in ignored local storage.
