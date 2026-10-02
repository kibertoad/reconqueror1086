---
id: FMT-RES-076
title: Unidentified .DRV data candidates in CD:INN/FOOTBALL
status: unknown
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:INN/FOOTBALL/MIDNONE.DRV", "CD:INN/FOOTBALL/PROAUDIO.DRV", "CD:INN/FOOTBALL/SBPRO.DRV", "CD:INN/FOOTBALL/SNDBLAST.DRV"]
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
syntax or runtime use is established.

## Enumerations and flags

None known.

## Differences between builds

None known.

## Coverage

The listed paths occur in BLD-GOG-EN's manifest. No layout definition has been
checked against them. An enclosing archive's listing does not establish the
formats of its members.

## Open questions

- What layout, if any, is shared by these candidates? (Q-RES-078) The required
  binary/little metadata values are provisional hypotheses. Text syntax,
  big-endian fields or multiple unrelated layouts remain possible; the filenames
  do not settle them. Inspect file signatures and the relevant readers, and split
  this group before asserting incompatible layouts. No parsing decision follows
  from this unknown entry.
