---
id: FND-RES-029
title: SETUP reads two Setup keys of a SIERRA.INF it finds beside itself and starts _SETUP.EXE
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:031A..0001:0402
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:0402..0001:0503
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:0562..0001:0583
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:08FC..0001:091D
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:09D3..0001:0A59
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:0D6A..0001:0E54
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    kind: file-data
    offset: 0x00004E93..0x00004EA8
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    kind: file-data
    offset: 0x00005100..0x0000510B
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    kind: file-data
    offset: 0x0000512F..0x00005155
  - build: BLD-GOG-EN
    file: CD:SIERRA.INF
    offset: 0x00..0xD6F
tool: capstone 5.0.7 bounded 16-bit disassembly of SETUP segment 1; Python bounded NE relocation-chain walk; Python bounded ISO 9660 root read
environment: null
---

## Observation

SETUP's complete length (28,096 bytes) and canonical fingerprint matched
BLD-GOG-EN. FND-RES-024 gives its NE segment table: segment 1 (code) from
file offset 0x5A0, 17,982 bytes, and segment 2 (data) from file offset
0x4E00, 4,634 bytes. The NE header names segment 2 as the automatic data
segment. Near data offsets below are read as offsets into segment 2, on the
reading that DS holds the automatic data segment, which this finding does
not trace instruction by instruction.

Imports. Each far call to an import is stored as `9A` with offset 0xFFFF and
segment 0 or as a link in a relocation chain. Segment 1's relocation table
holds 63 records (cap 4,096, not reached); walking each record's chain from
its offset until 0xFFFF, unless the record is additive, gives 150 sites. The
module reference table names KERNEL, GDI, USER, LZEXPAND and TOOLHELP. The
sites read here are imports by ordinal: 0001:033A KERNEL 58, 0001:0412
KERNEL 49, 0001:045B KERNEL 128, 0001:057B KERNEL 127, 0001:0919 KERNEL 129
and 0001:0A55 KERNEL 166. Wine's export table for the 16-bit KERNEL
(`krnl386.exe16.spec`) gives these ordinals as `GetProfileString`,
`GetModuleFileName`, `GetPrivateProfileString`, `GetPrivateProfileInt`,
`WritePrivateProfileString` and `WinExec`, and the number and order of the far and near
arguments each site pushes match those signatures under the Pascal
convention. The ordinal-to-name mapping is that outside source's, not read
from a KERNEL module of the period.

`fn_0001_0D6A` takes one near buffer address. It copies the buffer to a
local, writes `<copy>\SIERRA.INF` into the buffer with the format at segment
2 offset 0x032F, and passes that path, the attribute word 1 and a record to a
library routine at 0001:304F. A result of 0 returns 1. Otherwise it writes
`<copy>\*.*` (format at 0x033D), calls the same routine with attribute 0x10,
and moves on with a second library routine at 0001:3044 until one returns
nonzero. For each record whose byte at offset 0x15 has bit 0x10 set and whose
name at offset 0x1E does not start with `.`, it writes
`<copy>\<name>\SIERRA.INF` (format at 0x0344) into the buffer and calls the
first routine on it with attribute 0; a result of 0 returns 1. Neither
library routine is read here; the record layout matches a find-first record
of the C library. It returns 0
when the enumeration ends. The buffer keeps the last path written.

The caller in `fn_0001_024E`, from 0001:0402: `GetModuleFileName` of the
module handle at segment 2 offset 0x108C into a 260-byte stack buffer, cut at
its last `\`, then `fn_0001_0D6A` on that buffer. The return value is not
tested. It then calls `GetPrivateProfileString` with section `Setup`, key
`ForceLanguage`, an empty default, a 10-byte buffer and the buffer from
`fn_0001_0D6A` as the file name. When the result is greater than 0, it
compares the value with `GERMAN`, `SPANISH`, `FRENCH`, `ITALIAN` and
`ENGLISH` in that order through a library routine at 0001:33F2, and the
first match sets a language number of 2, 4, 1, 3 or 0. No match, or a
result of 0, keeps the number chosen before it from 0001:031A on: key
`sLanguage` of section `Intl` read with `GetProfileString` (default `ENU`,
4 bytes) and compared with `DEU`, `ESN`, `ESP`, `PTG`, `FRA`, `FRC` and `ITA`. The
number times 100, plus 5, is used from 0001:0503.

At 0001:0562 it calls `GetPrivateProfileInt` with section `Setup`, key
`SetupSize`, default 0 and the same file name, and stores the result. Its
later use is not read here.

At 0001:08FC it calls `WritePrivateProfileString` with section `Setup`, key
`Presetup`, value `1` and file name `SIERRA.INI`. At 0001:0A0C it formats a
command line `%s\_SETUP.EXE /U /o %s` or `%s\_SETUP.EXE /o %s` (the first
when the text at segment 2 offset 0x11BE, which `fn_0001_0010` fills at
0001:0059 from its arguments, holds `/U` or `/u`), the first `%s` being the
directory at segment 2 offset 0x1094 and the second a stack buffer built at
0001:08F6. It writes the result over the buffer at 0x1094 and, at 0001:0A54,
passes it to `WinExec` with show value 1. Segment 2 also holds the names
`.SOL`, `_SETUP.EXE`, `SETUP32.EXE` and `SOL_ENG.DLL` and its French, German,
Italian and Spanish counterparts, and the code calls LZEXPAND ordinals 1, 2,
6, 7, 8 and 9. This finding does not read where `_SETUP.EXE` comes from.

No 16-bit word equal to the offsets of the three formats (0x032F, 0x033D,
0x0344) occurs in segment 1 apart from the pushes at 0001:0D88, 0001:0DB7
and 0001:0E00; 0x0344 has three more raw hits at 0001:0744, 0001:07E1 and
0001:08D6, which are not read. The positive control is 0x032F itself, located
from the data segment's bytes without disassembly and found at the push the
disassembly shows. FND-RES-023's literal search found no `LANGUAGE.INF` or
`CONQUER.INF` in SETUP.

The shipped `CD:SIERRA.INF` (3,439 bytes) has a `Setup` section holding
`SetupSize` and no `ForceLanguage`.

## Interpretation

SETUP is a first stage. It reads SIERRA.INF through the Windows profile
routines, only `SetupSize` and `ForceLanguage` of section `Setup`, from the
copy in its own directory or one directory below it, and starts a second
installer program, `_SETUP.EXE`, with a path argument. With the shipped
file, `ForceLanguage` is missing, so the language follows the Windows locale.
The `Script`, `Files`, `Dialogs` and other sections are not read by SETUP;
`_SETUP.EXE` is the next candidate reader.

## Alternatives

- SETUP parses SIERRA.INF itself: ruled out for the reads found, which go
  through the profile imports. A read through a file routine elsewhere in
  segment 1 is not ruled out by this finding.
- The SIERRA.INF read is the CD root's: it is the one beside SETUP.EXE or one
  directory below, which is the CD root's only when SETUP runs from there.

## How to reproduce

Verify SETUP's manifest size and XXH3-128. Read the NE header, segment table,
module reference and imported-name tables, and segment 1's relocation table,
walking each chain with the stated cap. Disassemble segment 1 as 16-bit code
and read the listed ranges. Search segment 1's bytes for the three format
offsets as little-endian words. Compare the ordinals with Wine's
`dlls/krnl386.exe16/krnl386.exe16.spec`. Read `SIERRA.INF` from the disc
image's root as FND-RES-026 does and list its `Setup` keys. Keep listings in
ignored local storage.
