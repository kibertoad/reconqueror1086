---
id: FMT-RES-015
title: Unidentified .INF data candidates in CD root
status: unknown
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:AUTORUN.INF", "CD:CONQUER.INF", "CD:LANGUAGE.INF", "CD:SIERRA.INF"]
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

- Do these files share a permissive line grammar or require independent formats?
  FND-RES-021 records distinct complete line shapes: a bracket/key grammar does
  not account for the plain CONQUER regions or SIERRA's other class. One broad
  consumer and independent consumers both remain possible. Trace the readers
  and split the entry before asserting incompatible field layouts. (Q-RES-017)
- Which encoding interprets AUTORUN's four non-ASCII bytes? Multiple single-byte
  code pages remain possible; inspect the consuming decoder or a relevant format
  source rather than normalizing them to ASCII. This is an independently
  answerable dependency of layout reconciliation. (Q-RES-168)