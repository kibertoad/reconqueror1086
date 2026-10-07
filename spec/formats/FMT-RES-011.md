---
id: FMT-RES-011
title: ASCII line framing of CD-root INSTALL.DAT
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:INSTALL.DAT"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-RES-017]
conflicting: []
split_with: []
related: []
---

## Layout

The owned file stores printable ASCII line regions separated by CRLF, with a
nonempty final unterminated region. There are no tabs, NUL, non-ASCII bytes,
byte-order mark or other control bytes [FND-RES-017]. This entry describes stored
framing and prefix shapes, not the complete script grammar or reader policy.

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| Between CRLF pairs or at file end | char[] | install_dat_line | Retained ASCII region, including empty or space-only lines. | supported | FND-RES-017 |
| Between line regions | BYTE[2] | install_dat_delimiter | CRLF; absent after the final nonempty region. | supported | FND-RES-017 |
| At start of a region | BYTE[] | install_dat_indent | Leading spaces, retained literally; tabs do not occur. | supported | FND-RES-017 |
| After leading spaces | char[] | install_dat_body | Retained text, including at-sign prefixes, double-slash prefixes and other nonempty forms. | supported | FND-RES-017 |

At-sign-prefixed regions begin with letter-led identifier spellings containing
letters, digits or underscores. Case variants occur, but their dispatch relation
is unread. Double-slash-prefixed regions are comment-shaped; their lexical role
and extent are not established. Other nonempty regions are retained unchanged,
not treated as invalid or silently omitted.

Quotes, adjacent backslashes and punctuation occur inside the stored text.
FND-RES-017 records complete character/prefix measurements. This entry assigns
no string escape, comment or block semantics to them and reproduces no script
sequence or quoted message. A complete framing read preserves every line and
its delimiter instead of normalizing the file.

## Enumerations and flags

None established. Prefix spellings are not an exhaustive list of interpreter
operations or proof of option meanings.

## Differences between builds

None known.

## Coverage

Every byte and every line region in the identified BLD-GOG-EN file was inspected,
with exact reconstruction [FND-RES-017]. No interpreter or directive behavior was
read or run. A reader of similarly named media files is not inferred from this
one file. Supported status applies to stored text framing only.

## Open questions

- Which shipped interpreter consumes this file, and is that path reachable?
  FND-RES-034 locates CD:CONFIG.EXE building INSTALL.DAT's path from a drive
  and directory and reporting a failure to reopen its script file by that
  name, and reads its `@exists` handler, so CONFIG.EXE is the lead; where it
  first opens the file, and which shipped batch file or program starts it,
  are not read. (Q-RES-151)
- How does the interpreter tokenize at-sign identifiers and match their casing?
  The observed variants fit either distinct or case-insensitive dispatch.
  (Q-RES-152)
- How are double-slash regions recognized, and do quotes within them affect
  lexing? Prefix counts alone do not establish comment extent. (Q-RES-153)
- What quoted-string and backslash rules does the interpreter use? Raw quote
  parity and backslash counts are not string syntax evidence. (Q-RES-154)
- What grammar handles nonempty regions without at-sign or double-slash prefixes?
  Continuations and another statement form remain possible. (Q-RES-155)
- How does the interpreter handle the final unterminated region? Its stored
  presence does not establish execution or error behavior. (Q-RES-156)
