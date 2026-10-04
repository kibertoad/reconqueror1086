---
id: FND-RES-021
title: Disc-root INF candidates have distinct line shapes and one contains non-ASCII bytes
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:AUTORUN.INF
    offset: 0x00000000..0x000002DE
  - build: BLD-GOG-EN
    file: CD:CONQUER.INF
    offset: 0x00000000..0x00000039
  - build: BLD-GOG-EN
    file: CD:LANGUAGE.INF
    offset: 0x00000000..0x00000787
  - build: BLD-GOG-EN
    file: CD:SIERRA.INF
    offset: 0x00000000..0x00000D6F
tool: Python bounded ISO root traversal and complete byte/line classification
environment: null
---

## Observation

Every file was read completely through FMT-RES-005's physical payload mapping.
Its length and canonical XXH3-128 agree with BLD-GOG-EN:

| File | Length | XXH3-128 |
|---|---|---|
| CD:AUTORUN.INF | 734 | c207fa939dec11b352676a50b73d9e36 |
| CD:CONQUER.INF | 57 | d1cb8388fff6c22f15c82852d40d3257 |
| CD:LANGUAGE.INF | 1927 | 9991d2d312d583c1ca554fe99876a297 |
| CD:SIERRA.INF | 3439 | a9f51bcff63b8c523a074202c5a3c7ef |

Splitting only at CRLF gives the following complete shape counts. A bracketed
region begins with a left bracket and ends with a right bracket. Classification
checks empty regions first, then bracket prefix, then semicolon prefix, then an
equals sign, then other. No whitespace normalization is used in reconstruction.

| File | CRLF pairs | Bracketed | Equals-bearing | Semicolon-prefixed | Other | Empty | Longest region |
|---|---|---|---|---|---|---|---|
| AUTORUN | 31 | 5 | 19 | 0 | 0 | 8 | 73 |
| CONQUER | 4 | 0 | 0 | 0 | 4 | 1 | 15 |
| LANGUAGE | 56 | 2 | 31 | 12 | 0 | 12 | 314 |
| SIERRA | 160 | 6 | 25 | 28 | 97 | 5 | 102 |

There is no bare CR or LF, tab, NUL, 0x1A or other byte below 32 outside CR/LF
in any file. No region ends in a space. CONQUER ends with CRLF, so its final
empty region is a split artifact after the last delimiter. The other three
files do not end with CRLF and retain a final unterminated region.

CONQUER, LANGUAGE and SIERRA contain only printable ASCII and CR/LF.
AUTORUN additionally has four bytes above 127, with distinct values 233, 234 and
246. Their code page and character interpretation are not established. No
stored prose, command values or complete line listing is reproduced here.

All bracket-prefixed regions close with a right bracket. This does not prove
that a reader treats them as sections. Likewise semicolon prefixes are stored
shapes, not established comments. The equals-bearing count excludes regions
already classified by an earlier prefix; no assignment interpreter is inferred.

Retaining every region and each original CRLF reconstructs every complete file
exactly. A 16 MiB logical-read bound was applied and never reached. No consumer,
installer, shell command or original program was read or run.

## Interpretation

The suffix-based inventory group does not justify a single section/key grammar.
CONQUER has no observed bracket or equals shape; SIERRA retains a large other
class that a key-only classifier would omit. The AUTORUN bytes also rule out
strict ASCII for that member. These distinctions must survive any later split
into formats and any decoding witness.

## Alternatives

- A grammar requiring every nonempty line to be a bracket marker or an equals
  assignment conflicts with the CONQUER and SIERRA stored shapes.
- One permissive line-oriented consumer and several independent consumers both
  fit the observations. Reader references and token handling must decide them.
- Several single-byte code pages can explain AUTORUN's high bytes; printable
  appearance or the filename alone cannot select one.
- Semicolon comment recognition and literal semicolon data remain competing
  readings until the consumer is read.

## How to reproduce

Verify the owned build. Traverse the bounded ISO root directory and select all
four manifest paths. Read complete extents and compare lengths and canonical
fingerprints. Classify every byte; retain CRLF delimiters, including whether the
last region has one. Count the mutually exclusive line shapes in the stated
order, retaining the other class. Check bracket closure and whitespace separately.
Rejoin every region and delimiter and compare the entire file exactly. Export
only compact shape facts, not file prose or command sequences.
