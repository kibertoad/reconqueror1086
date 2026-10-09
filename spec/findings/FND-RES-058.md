---
id: FND-RES-058
title: The disc's two readme files, EMPTY.TXT, VERSION.TXT and the VESA directory are documentation, a placeholder and a third-party driver that no Conqueror program reads
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 20F3:2975..20F3:2C6A
  - build: BLD-GOG-EN
    file: CD:SIERRA.INF
    offset: 0x00000656..0x0000069D
  - build: BLD-GOG-EN
    file: CD:SIERRA.INF
    offset: 0x00000939..0x0000094B
  - build: BLD-GOG-EN
    file: CD:SIERRA.INF
    offset: 0x00000D16..0x00000D46
  - build: BLD-GOG-EN
    file: CD:README.BAT
    offset: 0x00000000..0x0000001C
tool: Python 3.14 case-insensitive byte searches of the files FND-RES-057 extracted and checked; capstone 5.0.7 bounded 16-bit disassembly of the LZEXE-unpacked INST.EXE with relocation targets marked
environment: null
---

## Observation

The files searched are those of FND-RES-057: every disc file outside `DEMOS/`
and `INN/`, checked against the manifest's sizes and XXH3-128 values, the
installed `CONQUER.INI` and `game.ins`, and the unpacked `CD:INST.EXE`. Each
was searched without case, in ASCII and UTF-16LE, for `readme`, `.wri`,
`.doc`, `empty.txt`, `version.txt`, `univesa`, `vesa\` and `vesa/`.

| Name | Files that hold it, besides the named file itself |
|---|---|
| `readme.wri` | `CD:SIERRA.INF`: a line that runs `write.exe` on `*sourcedir\readme.wri` with the title key `readmetitle`, and the setting `readme=readme.wri` |
| `readme.txt` | `CD:README.BAT`, whose one command after `@echo off` is `edit readme.txt` |
| `empty.txt` | `CD:SIERRA.INF`: a `[Files]`-style line that copies it to `*destdir\savegame\empty.txt` |
| `version.txt` | none |
| `univesa` | `CD:README.TXT` and `CD:README.WRI`, which tell the player to change to the `VESA` directory and type `UNIVESA` before running the game |
| `vesa\`, `vesa/` | none |
| `.doc` | none that names `UNIVESA.DOC` |

Other `readme` hits: two keys and one text line of `CD:INSTALL.TXT`
(FND-RES-053), the `readmetitle` setting of `CD:LANGUAGE.INF`, and five
strings in the unpacked INST.EXE. Two of those are the keys `viewReadMe`
(DS:16CD) and `readMePrompt` (DS:1799). The others are the file names
`readme` (DS:1755, DS:176A, DS:177F) and `read.me` (DS:175C, DS:1774,
DS:1786), used by 20F3:2975 (this):

- It forms DS:5557 followed by `readme`, and DS:5557 followed by `read.me`,
  and opens the first, or else the second, through 1000:48EF with `rt`.
- When one opens, it opens the bare name `readme`, or else `read.me`, and
  keeps that handle, then reads the file through 1000:4543 in lines of up to
  99 bytes, cuts each at its LF, copies it through 1000:6370 and appends it to
  a list it makes at +0x20F.
- When neither opens, or the bare names do not, it looks for the members
  `readme`, then `read.me`, in the archive at +0x23A through 30C9:0394.

No file on the disc or in the installation is named `README` or `READ.ME`
without an extension, and the build ships no `.SIP` archive (FND-RES-049).

The files themselves: `CD:EMPTY.TXT` is 0 bytes. `CD:VERSION.TXT` is one CRLF
line naming the product, version 1.0, its publisher and 1995, followed by
0x1A. `CD:README.TXT` and `CD:README.WRI` are the player's notes, in plain
text and in Windows Write form. `CD:VESA/UNIVESA.EXE` is a DOS program and
`CD:VESA/UNIVESA.DOC` its text manual, whose copyright line names an
individual author.

## Interpretation

No program of the game reads any of these files' contents. The two readme
files are documentation that the Windows setup (FMT-RES-120) opens in the
Windows Write program and `README.BAT` opens in the DOS editor. `EMPTY.TXT`
is an empty placeholder that the setup copies so that the save directory
exists. `VERSION.TXT` is named by nothing. The `VESA` directory holds a
third-party VESA driver that the readme asks the player to run by hand. The
same kinds of reference as FND-RES-057 were not covered.

## Alternatives

- The game reads `VERSION.TXT` to show its version: not found; no file names
  it.
- The installer reads the readme to show it itself: ruled out for the
  Windows setup, which hands `readme.wri` to `write.exe`, and for INST.EXE,
  whose readme loader opens only files named `readme` or `read.me`.

## How to reproduce

Extract and check the files as FND-RES-057 gives. Search each for the names
above, case-insensitively, as ASCII and as UTF-16LE, and read each hit's
line. Unpack `CD:INST.EXE` as FND-RES-066 gives and disassemble 20F3:2975
with the load image at segment 0x1000. Keep the extracted files and listings
in ignored local storage.

Whole-function exclusive bounds follow the body extents recorded in FND-RES-062.
