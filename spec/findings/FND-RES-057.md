---
id: FND-RES-057
title: No Conqueror file outside the disc's DEMOS and INN directories names DEMOS or a demo, and only INN.BAT enters INN, to run that directory's own installer
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INN.BAT
    offset: 0x00000000..0x0000001D
tool: Python 3.14 ISO 9660 traversal of the raw 2352-byte data records with XXH3-128 checks against the manifest, and case-insensitive byte searches
environment: null
---

## Observation

Files. BLD-GOG-EN's manifest lists 452 paths under `CD:DEMOS/` and 111 under
`CD:INN/`. Every other file on the disc, 2267 of them (36 in the root and
`VESA/`, 2231 under `CONQUER/`), was read from the owned image by walking its
ISO 9660 directories through FMT-RES-005's record mapping. Each one's size and
XXH3-128 equal the manifest's. To them were added the installed `C1086.GOB`,
`CONQUER.INI` and `game.ins`, and the LZEXE-unpacked `CD:INST.EXE` of
FND-RES-054. The installed `C1086.GOB` is byte for byte the disc copy.

Search. Each file was searched, as stored, for these names in ASCII and in
UTF-16LE, without case: `DEMOS`, `MOVIES`, `SHIVERS`, `SWAT`, `THEXDER`,
`DIRECTX`, `FOOTBALL`, `TWINION`, `INN\`, `\INN`, `INN/`, `INN.BAT`,
`IE.EXE`, plus `VESA` and `UNIVESA`.

- No file holds any of the names from `DEMOS` to `TWINION`, `INN.BAT` or
  `IE.EXE`, in either encoding.
- `INN/` occurs once, at offset 0x226AC of `CD:CONQUER/DRJSTLSE.SMK`, and
  `\INN` once, at offset 0x10F4E6E of `C1086.GOB`, each between non-text
  bytes, inside media data.
- `VESA` and `UNIVESA` occur in `CD:VESA/`, the two readme files, `CONQUER.EXE`,
  `CONQUER/CONCFG.EXE`, `INSTALL.HLP` and INST.EXE; that directory is not
  part of this question.

Positive control. The search finds `install.scr` at offset 0x2A6F5 of the
unpacked INST.EXE, which is DS:14D5, the string FND-RES-054 locates through
the code that uses it. No reference in UTF-16LE was located in advance, so
that encoding has no control.

`CD:INN.BAT` holds three commands: turn echo off, change to the directory
`inn`, and run `install`, which from there is `CD:INN/INSTALL.BAT`.
`CD:AUTORUN.INF` names `AUTOPLAY.EXE` and `AUTOPLAY.ICO`; the printable
strings of `CD:AUTOPLAY.EXE` that name files are `autoplay.bmp`, `install`
and `setup.exe`.

## Interpretation

The demos under `DEMOS/` are other Sierra products that nothing of Conqueror
names: the game, its configuration programs, its installers, the autoplay
launcher and the batch files never start or read them. `INN/` holds the
ImagiNation Network software, another product, reached only when the player
runs `INN.BAT` by hand; no program of the game names it. Neither directory is
a file the game uses.

Kinds of reference not covered: names in UTF-16LE (searched, but with no
control), names a program builds from parts at run time, names inside data
stored compressed (the members of `C1086.GOB`, and any packed program other
than INST.EXE), and directory listings a program walks with wildcards, such as
INST.EXE's `*.txt` and `*.hlp` search (FND-RES-053), which reads only its own
directory.

## Alternatives

- The autoplay launcher offers the demos: not found; its file names are the
  bitmap, `install` and `setup.exe`, and no demo name occurs in it in either
  encoding.
- The game's setup installs the ImagiNation Network: not found; only
  `INN.BAT` names `inn`.

## How to reproduce

Walk the ISO 9660 directories of the owned image's data records (mode 1, 2048
bytes at offset 16 of each 2352-byte record), extract every file outside
`DEMOS/` and `INN/`, and check each against the manifest's size and XXH3-128.
Unpack `CD:INST.EXE` as FND-RES-054 gives. Search every file for the names
above, case-insensitively, as ASCII bytes and as UTF-16LE. Keep the extracted
files in ignored local storage.
