---
id: FMT-RES-008
title: Empty and ASCII text framing of CD-root batch files
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONFIG.BAT", "CD:CONQUER.BAT", "CD:INN.BAT", "CD:INSTALL.BAT", "CD:README.BAT"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-RES-014]
conflicting: []
split_with: []
related: []
---

## Layout

This entry describes the complete stored framing of the listed owned files,
not the full language or acceptance policy of a command interpreter.
CONFIG.BAT and CONQUER.BAT are empty. They contain no encoding evidence.
Nonempty files contain printable ASCII lines separated by CRLF, with spaces
between command-like tokens and filename punctuation. Leading at-signs and
hyphen-prefixed tokens occur; letter casing is mixed. No byte-order mark, NUL,
tab or byte above 127 occurs [FND-RES-014].

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| Line content | BYTE[] | batch_text_line | Printable ASCII bytes before CRLF or file end; an empty line is allowed by the observed INN.BAT case. | supported | FND-RES-014 |
| Line delimiter | BYTE[2] | batch_text_separator | Stored CR followed by LF; the last text line need not have this delimiter. | supported | FND-RES-014 |
| Terminal region | BYTE[] | batch_text_terminal | Either absent or the single final byte 0x1A in README.BAT; runtime interpretation unread. | supported | FND-RES-014 |

INN.BAT has a final empty line followed by CRLF. INSTALL.BAT ends in printable
text without CRLF. README.BAT has a single 0x1A after its last CRLF. Those cases
are preserved separately; stripping a terminator or adding a newline changes
stored content. FND-RES-014 records their complete fingerprints. No command
sequence or script content is reproduced here.

## Enumerations and flags

None established. The observed leading punctuation is text syntax, not evidence
of a particular interpreter decision or option meaning.

## Differences between builds

None known.

## Coverage

Every byte of each listed BLD-GOG-EN file was inspected. The classification is
limited to these CD-root paths; similarly named installation files and batch
files below subdirectories are not covered. No interpreter or command target
has been completely read or run. Supported status applies to stored framing
only, not to an executable rule or acceptance policy.

## Open questions

- Which shipped interpreter or wrapper consumes these CD-root batch files, and
  which paths are reachable? Filenames and text alone do not prove runtime use.
  (Q-RES-132)
- How does that consumer treat the terminal 0x1A in README.BAT? Its position is
  observed, but stop/ignore/ordinary-byte behavior remains unresolved. (Q-RES-133)
- How does that consumer handle INSTALL.BAT's final unterminated line? Its stored
  existence does not establish execution or error behavior. (Q-RES-134)
