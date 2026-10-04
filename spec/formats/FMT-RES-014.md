---
id: FMT-RES-014
title: Unidentified .ICO data candidates in CD root
status: unknown
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:AUTOPLAY.ICO", "CD:CONQCFG.ICO", "CD:CONQUER.ICO"]
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

- What layout, if any, is shared by these candidates? (Q-RES-016) The required
  binary/little metadata values are provisional hypotheses. Text syntax,
  big-endian fields or multiple unrelated layouts remain possible; the filenames
  do not settle them. Inspect file signatures and the relevant readers, and split
  this group before asserting incompatible layouts. FND-RES-020 records matching fingerprints and a shared counted-directory
  partition, but bitmap field roles and the differing payload size values remain
  unconfirmed by a consumer. No parsing decision follows
  from this unknown entry.
