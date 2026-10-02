---
id: FND-RES-014
title: CD-root batch candidates are empty files or short ASCII text with differing endings
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INN.BAT
    offset: 0x00000000..0x0000001E
  - build: BLD-GOG-EN
    file: CD:INSTALL.BAT
    offset: 0x00000000..0x00000017
  - build: BLD-GOG-EN
    file: CD:README.BAT
    offset: 0x00000000..0x0000001D
tool: Python 3.14 bounded ISO root-directory traversal and complete byte classification
environment: null
---

## Observation

All five candidates were located by name in the owned image's ISO root directory.
Each complete logical file was read using physical raw-sector indexing and the
2048-byte payload mapping of FMT-RES-005. The lengths agree with BLD-GOG-EN.

| File | Bytes | CRLF pairs | Final stored region | SHA-256 |
|---|---:|---:|---|---|
| CD:CONFIG.BAT | 0 | 0 | No content | e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855 |
| CD:CONQUER.BAT | 0 | 0 | No content | e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855 |
| CD:INN.BAT | 30 | 4 | CRLF after an empty line | 68347f5877b38a794dd19d4d5f165dfdc3081535ee9b4a38127d07fe04ef45d0 |
| CD:INSTALL.BAT | 23 | 1 | Printable text without a final CRLF | 411f49ac2715c2e6193d4f9667aa523244fcc7de700f8fe7cfe1a64265b9e059 |
| CD:README.BAT | 29 | 2 | Single byte 0x1A after the last CRLF | 216765eaf3d52b7a183063909c69db60f721f7ee0feea856a7523ac096e648ea |

The two empty files have zero directory-declared length and produce empty reads;
they are not truncated nonempty extents. The three nonempty files are positive
controls for the same directory selection and read mapping.

Every byte of every nonempty candidate is either printable ASCII (32 through
126), CR, LF or the final 0x1A noted above. Every CR is immediately followed by
LF and every LF is preceded by CR. No byte-order mark, NUL, tab or byte above
127 occurs. Only README.BAT contains 0x1A, and it occurs only once at file end.
These are complete-file absence checks, not sampled signatures.

Nonempty lines contain command-like ASCII tokens with spaces and filename
punctuation. Leading at-signs and hyphen-prefixed tokens occur; casing is not
uniform. No token is decoded as an integer field or binary record length here.
The scripts' writing and command sequences are not reproduced in this finding.
No interpreter or launched target was read or executed.

## Interpretation

The provisional binary/little-endian classification is replaced by a narrow
observed text-framing description. Empty content, an empty line, a final
unterminated text line and a terminal control byte are distinct stored cases.
A reader requiring every nonempty file to end with CRLF would reject INSTALL.BAT;
one allowing only printable bytes and CRLF would reject README.BAT.
Neither fact determines how a shipped interpreter handles those cases.

## Alternatives

- A binary field layout shared by these candidates has no supporting evidence;
  complete inspection instead shows the text framing described above.
- Uniform mandatory final CRLF is ruled out by INSTALL.BAT and README.BAT.
- A terminal 0x1A in every nonempty file is ruled out by INN.BAT and INSTALL.BAT.
- Empty CONFIG.BAT and CONQUER.BAT contain no encoding evidence of their own.
  Their listing in the same entry asserts empty stored content only, not that
  a filename establishes text syntax or runtime behavior.

## How to reproduce

Verify BLD-GOG-EN. Traverse the ISO root directory and select each full candidate
path, checking its extent and declared length against the logical volume. Read
exactly that length through FMT-RES-005's physical-sector payload mapping and
compare the complete SHA-256 values above. Inspect every byte, classify CR/LF
pairs, printable ASCII and terminal 0x1A separately, and retain whether the
last printable line has a delimiter. Reconstruct bytes from these regions and
compare them to the complete input. Do not run any batch file.
