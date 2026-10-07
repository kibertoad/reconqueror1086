---
id: FMT-RES-015
title: Unidentified .INF data candidates in CD root
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

None known. This is a provisional inventory group by directory and filename
suffix, not a finding that its files share one format. No field layout, parsing
syntax or runtime use is established. `CD:AUTORUN.INF` was listed here until
FND-RES-026 located its reader; it is FMT-RES-117. `CD:LANGUAGE.INF` was listed
here until FND-RES-032 located its readers; it is FMT-RES-119. `CD:SIERRA.INF`
was listed here until FND-RES-033 read its loader; it is FMT-RES-120.

## Enumerations and flags

None known.

## Differences between builds

None known.

## Coverage

The listed paths occur in BLD-GOG-EN's manifest. No layout definition has been
checked against them. An enclosing archive's listing does not establish the
formats of its members.

## Open questions

- Do these files share a permissive line grammar or require independent formats?
  FND-RES-021 records distinct complete line shapes: a bracket/key grammar does
  not account for the plain CONQUER regions or SIERRA's other class. One broad
  consumer and independent consumers both remain possible. FND-RES-022 adds
  distinct token families and APPEND-shaped equals-bearing left portions.
  FND-RES-023 identifies AUTOPLAY and SETUP filename-occurrence leads, without
  proving file opens. FND-RES-024 supplies distinct PE/NE data mappings and
  profile-import leads. FND-RES-025 rules out direct SETUP filename fixups within
  its declared relocation table, while leaving selector/offset construction open.
  FND-RES-026 shows that AUTOPLAY reads a file named LANGUAGE.INF through the
  Windows profile API, but the copy in an installed game directory, not the
  disc's, and only its `Ident` section's `Title`; AUTOPLAY names neither
  CONQUER.INF nor SIERRA.INF. That the installer copies the disc's LANGUAGE.INF
  there rests only on equal values. FND-RES-029 shows that SETUP reads only
  `SetupSize` and `ForceLanguage` of SIERRA.INF's `Setup` section, through the
  Windows profile routines, and then starts `_SETUP.EXE`, a second installer
  whose names sit beside `.SOL` in SETUP's data; the other SIERRA sections
  are not read by SETUP. FND-RES-032 shows that `_SETUP.EXE` loads
  SIERRA.INF as a whole file divided by bracketed section markers, and reads
  LANGUAGE.INF (now FMT-RES-119) through the profile routines, so the two
  have different readers. FND-RES-033 reads SIERRA.INF's loader (now
  FMT-RES-120). CONQUER.INF, the one file left here, has no located reader;
  INST.EXE is the remaining lead. (Q-RES-017)
