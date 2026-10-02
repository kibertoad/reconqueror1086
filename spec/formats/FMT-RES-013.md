---
id: FMT-RES-013
title: Marked ASCII text framing of CD-root INSTALL.HLP
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:INSTALL.HLP"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-RES-019]
conflicting: []
split_with: []
related: []
---

## Layout

The owned file stores ASCII text regions separated by CRLF, preserving tabs,
empty regions and trailing spaces. Some regions begin with two backslashes.
The sole 0x1A byte occupies its own region and is followed by the final CRLF.
These are stored shapes, not the language or acceptance policy of a reader
[FND-RES-019].

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| Between CRLF delimiters | BYTE[] | install_help_region | Retained printable ASCII or tab bytes, including empty regions; the single control-byte region is listed below. | supported | FND-RES-019 |
| At start of a marked region | BYTE[2] | install_help_prefix | Two stored backslashes; role unread. | supported | FND-RES-019 |
| After the prefix | BYTE[] | install_help_marker | Complete ASCII marker remainder, retaining trailing whitespace and duplicates. | supported | FND-RES-019 |
| Between regions | BYTE[2] | install_help_delimiter | CRLF, including after the control-byte region. | supported | FND-RES-019 |
| Penultimate region | BYTE[1] | install_help_control | Single 0x1A byte; reader interpretation unknown. | supported | FND-RES-019 |

Marked remainders include no-dot strings and drv, hlp and inf suffix shapes;
one hlp-shaped remainder has a trailing space. No region starts with exactly
one backslash. FND-RES-019 gives complete whitespace, marker and boundary
measurements. Those shapes do not establish links, sections or file loading.
No original help prose or complete marker list is retained here.

## Enumerations and flags

None established. The control byte and prefix shapes are not named as reader
operations without consumer evidence.

## Differences between builds

None known.

## Coverage

Every byte and region of the listed BLD-GOG-EN file was inspected and complete
byte reconstruction passed [FND-RES-019]. Similarly named help files in other
media directories are not covered. No consumer behavior was read or run.

## Open questions

- Which shipped reader consumes this file, and is the path reachable?
  (Q-RES-158)
- What roles do the backslash-prefixed regions have? Links, sections and file
  references remain competing readings, not established by suffixes. (Q-RES-159)
- Does marker lookup retain or normalize trailing whitespace? The spaced suffix
  alone cannot distinguish those readings. (Q-RES-160)
- How does the reader handle repeated complete marker remainders? Their stored
  presence does not establish first/last/all-match behavior. (Q-RES-161)
- How does the reader treat 0x1A and the following CRLF? End-of-input, ordinary
  data and another control role remain possible. (Q-RES-162)
- How does the reader handle tab bytes in text regions? Stored tabs do not
  establish display spacing or tokenization. (Q-RES-163)
