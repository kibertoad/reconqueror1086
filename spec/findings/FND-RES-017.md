---
id: FND-RES-017
title: INSTALL.DAT stores ASCII directive-shaped lines with a final unterminated line
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INSTALL.DAT
    offset: 0x00000000..0x0000BDDB
tool: Python 3.14 complete byte classification and bounded line-prefix traversal
environment: null
---

## Observation

The ISO root-directory file was read completely through FMT-RES-005's physical
raw-sector payload mapping. Its 48603-byte length agrees with BLD-GOG-EN.
Whole-file SHA-256 is
`349504b7abe306216dd661d83a66207cd74cbe28797d3867bd2f554fe8dcbb04`.

Every byte is printable ASCII or CR/LF. There are 892 CRLF pairs and no bare
CR, bare LF, tab, NUL, byte-order mark, terminal control byte or non-ASCII byte.
Splitting at those pairs gives 893 stored line regions. The last is nonempty
and has no CRLF. The longest line region is 180 bytes.

Every line region was classified by its first bytes after removing leading
spaces for inspection, without altering the retained bytes. The classes are
497 at-sign-prefixed regions, 326 double-slash-prefixed regions, 45 empty or
space-only regions and 25 other nonempty regions. The other class is retained;
it is not discarded as invalid or assumed to be a particular continuation.
All at-sign-prefixed regions begin with an at-sign followed by a letter-led
identifier, with subsequent letters, digits or underscores. There are 69 distinct
first identifier spellings. Casing varies, including @If and @if, and @GOTO,
@Goto and @goto. This does not prove case-insensitive dispatch.

The complete file contains 1881 double-quote bytes and 547 non-overlapping pairs
of adjacent backslashes. There is no backslash immediately followed by a quote.
Those are byte observations, not an established string lexer or escape rule.
The odd quote count cannot establish malformed input: whether every quote acts
as a string delimiter, including on slash-prefixed lines, is unread.

Equals signs, parentheses, commas and brackets occur as text punctuation. The
file's script writing, quoted messages and directive sequences are not retained
here. No consumer was read and no script or original program ran.

A complete traversal retaining each original line region and each CRLF, plus
the final unterminated region, reconstructs every byte of the file exactly.

## Interpretation

The file is stored text, not a binary record family inferred from its suffix.
Its prefix classes provide leads to a script interpreter, while leaving its
comment rules, quoted-string grammar, control structures, directives and runtime
reachability unresolved. A parser that requires a final CRLF, silently drops the
other line class or validates by raw quote parity would make an unsupported
choice. Text framing alone proves none of the script's effects.

## Alternatives

- Binary length/record framing has no supporting evidence; complete inspection
  instead establishes the ASCII line partition above.
- A required stored final CRLF is ruled out by the complete final region.
- Case-insensitive directive dispatch and distinct case-sensitive tokens both
  fit the observed spellings; only the interpreter settles their relationship.
- Slash prefixes may introduce comments, but recognizing their lexical extent
  and their interaction with quotes requires the reader, not a prefix count.
- Other regions may be continuations or another statement form; neither reading
  follows from the line classification alone.

## How to reproduce

Verify BLD-GOG-EN and the complete fingerprint. Bound and read the ISO-declared
file length. Classify every byte; split only on CRLF while retaining every region
and delimiter. Count first-byte classes after leading-space inspection, retaining
all other lines unchanged. Count literal quote bytes and non-overlapping adjacent
backslash pairs without interpreting escapes. Concatenate retained regions and
delimiters and compare to the complete source. Do not run the script, normalize
case or assume comment/string/control syntax from those counts.
