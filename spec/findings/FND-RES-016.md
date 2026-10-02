---
id: FND-RES-016
title: CD-root RESOURCE.CFG stores aligned ASCII key/value lines
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:RESOURCE.CFG
    offset: 0x00000000..0x000000BB
tool: Python 3.14 bounded ISO read and complete line/token traversal
environment: null
---

## Observation

The file was selected from the owned ISO root directory and read through
FMT-RES-005's physical-sector payload mapping. Its 187-byte length agrees
with BLD-GOG-EN. Whole-file SHA-256 is
`2d397b04f6b564c00e61d82029e51bd02a4bcfb5eaf42d46d9d78558ed5ddca6`.

The whole file consists of ten nonempty ASCII lines, each ending CRLF, including
the last. Every line has exactly one equals sign at byte offset 10 within the
line. Its key starts at offset 0 and is followed by enough spaces to place that
sign at offset 10. A single space follows the sign; the value begins at offset
12 and ends immediately before CRLF, without trailing whitespace. Keys are
unique and use mixed letter casing. FMT-RES-010 records the key/value tokens as
compact data facts, not their runtime meanings.

Every byte was classified. Apart from CRLF, all bytes are printable ASCII.
There are no blank lines, comments, byte-order mark, tabs, NUL bytes, quoted
values or bytes above 127. No bare CR or bare LF occurs. Values include plain
letter tokens, digit tokens and a backslash-delimited path token. Backslashes
are stored bytes; no escape or path-resolution rule is established.

A complete line/token traversal retains each key, its spacing, the separator,
value and delimiter and reconstructs the whole file exactly. This is a stored
syntax check. No consumer, interpreter or launched target was read or run.

## Interpretation

The provisional binary/little-endian classification is replaced by a text
layout supported by complete file-data evidence. Fixed alignment is observed
in this file; it is not evidence that an interpreter requires that alignment.
Likewise key casing, option names and digit values do not prove case matching,
boolean semantics, numeric bases, version scales or resource requirements.

## Alternatives

- Binary field framing has no supporting evidence; the complete file instead
  partitions into the described ASCII lines and tokens.
- A missing final line delimiter is ruled out by the final CRLF.
- A mandatory column-10 separator and freely spaced separator are competing
  consumer readings; this one aligned file cannot distinguish them.
- Case-sensitive and case-insensitive key matching both fit the stored text;
  the reader must settle the distinction.

## How to reproduce

Verify BLD-GOG-EN and the complete fingerprint. Select CD:RESOURCE.CFG in the
ISO root directory, bound its declared extent and read exactly its length.
Retain CRLF on every line, check every line's separator position, key padding,
value start and lack of trailing value whitespace. Check unique keys and every
byte's character class. Concatenate the retained regions and compare them to
the complete source. Do not execute a consumer or infer option semantics.
