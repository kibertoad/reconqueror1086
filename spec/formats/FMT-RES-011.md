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
evidence: [FND-RES-017, FND-RES-036, FND-RES-037, FND-RES-064, FND-RES-042, FND-RES-043]
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
letters, digits or underscores. Other nonempty regions are retained unchanged,
not treated as invalid or silently omitted.

The reader's lexing of the two prefixes is supported by FND-RES-036. An at-sign
name is `@` followed by letters and digits, upper-cased as it is read; any
other byte, `_` and `:` included, ends it and is read again as the start of
the next token. Names are matched against the keyword table and then the
table of registered names with `a` to `z` and `A` to `Z` treated alike, so
the case variants in the file select the same keyword or name. `@` right
after a name, followed by `!`, is dropped as a separator; `@!` alone is
passed over; `@@` is a token of its own. Outside quoted strings, `//` drops
everything to the end of its line, quotes included, and `/*` everything to
the next `*/`; inside a quoted string neither starts a comment.

A quoted string, supported by FND-RES-037, runs from `"` to the next
unescaped `"`, across line breaks, and holds at most 1,499 bytes. Its
escapes are `\"`, `\@`, `\\`, `\a`, `\b`, `\n`, `\r`, `\t` and `\x` (or
`\X`) with hex digits; any other byte after a backslash, a string reaching
1,500 bytes, or the end of the file inside a string ends the run with an
error. An at-sign name inside a string is replaced by its value, `@@` gives
`@`, and keywords insert either their numeric value or their own name. The
file's strings use only `\\` and `\n`.

Outside quoted strings and comments the file holds at-sign commands,
labels and block text, supported by FND-RES-042. A label is a word of
letters, digits and `_` in the first column followed by `:`; it defines
`LABEL_<word>` for `@GOTO`. Text between `@Display` (or `@Welcome`) and
`@EndDisplay` is written to the screen byte by byte, line breaks included,
and the at-sign tokens in it are handled as they come. A bare word anywhere
else in the main run stops the program with a syntax error. The blocks of
`@GetOutDrive`, `@GetSubdir` and `@GetOption` write their text the same way
(FND-RES-043). The shipped file's bare text is all labels or block text.

The run ends at the end of the file or at `@FINISH`, supported by
FND-RES-064. `@FINISH` closes the file and marks the start of a finish
block, which runs just before the program exits: the file is reopened
there, every byte outside an at-sign command up to `@ENDFINISH` is written
to the screen, line breaks included, and the commands in the block run.
The end of the file inside the block, before an `@`, ends the run with an
error. The shipped file's final unterminated region is the block's
`@EndFinish`, which the name reader ends at the end of the file, so the
missing line end has no effect.

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
with exact reconstruction [FND-RES-017]. Parts of the interpreter are read in
FND-RES-036, FND-RES-037 and FND-RES-064, FND-RES-042 and FND-RES-043; nothing was run. CD:CONFIG.EXE opens the file read-only, by default beside its
own executable, and passes the handle to its script runner (FND-RES-034,
FND-RES-035), which reads it as a stream of tokens in which line ends
are white space, and runs the finish block at exit (FND-RES-064). Supported status
applies to stored text framing only.

## Open questions

None.
