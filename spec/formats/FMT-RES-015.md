---
id: FMT-RES-015
title: CONQUER.INF, the CD marker the install script tests for
status: unknown
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER.INF"]
byte_order: little # provisional hypothesis; see Open questions
size: null
text: false # provisional hypothesis; see Open questions
definition: null
evidence: []
conflicting: []
split_with: []
related: []
---

## Layout

None known. No located code reads this file's contents. The install script
CD:INSTALL.DAT, run by CD:CONFIG.EXE, names it once, in an `@exists` test of
the startup drive and directory, which asks DOS whether a file of that name
is there without opening it (FND-RES-034). That test is the file's only
located use, so a file of any contents, including an empty one, would pass
it. `CD:AUTORUN.INF`, `CD:LANGUAGE.INF` and `CD:SIERRA.INF` were listed here
until FND-RES-026, FND-RES-032 and FND-RES-033 located their readers; they
are FMT-RES-117, FMT-RES-119 and FMT-RES-120.

## Enumerations and flags

None known.

## Differences between builds

None known.

## Coverage

The listed path occurs in BLD-GOG-EN's manifest. A search of every file on
the disc, the expanded SETUP.SOL members and the unpacked INST.EXE for the
name found it only in CD:INSTALL.DAT (FND-RES-034). No layout definition has
been checked against the file.

## Open questions

- Does any program read CONQUER.INF's contents? FND-RES-034 finds the name
  written whole only in INSTALL.DAT's `@exists` test, which reads nothing
  from the file, and neither INST.EXE nor CONQUER.EXE names it. A reader
  that builds the name at run time, or keeps it compressed inside another
  file, is not ruled out. FND-RES-021 and FND-RES-022 record the file's line shapes,
  which a reader would have to account for. (Q-RES-017)
