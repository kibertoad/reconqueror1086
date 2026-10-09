---
id: FMT-RES-010
title: ASCII key/value syntax in CD-root RESOURCE.CFG
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:RESOURCE.CFG"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-RES-016, FND-RES-066]
conflicting: []
split_with: []
related: []
---

## Layout

The owned file stores ASCII key/value lines terminated by CRLF. Keys begin at
line offset 0, are padded with spaces up to offset 10, and are followed by an
equals sign and one space. Values start at offset 12 and end before CRLF.
Every line, including the last, has that delimiter. These are observed-file
constraints [FND-RES-016].

The installer `INST.EXE` reads the file as `resource.cfg` from the current
directory, with no drive or directory added. Its reader accepts a wider
language than the shipped file uses, supported by FND-RES-066:

- Each line is read into a buffer of 0x8D bytes and split at its first `=`.
  A line with no `=` has an empty key and value.
- Space, tab, CR and LF are trimmed from both ends of the line, then of the
  key and of the value, so no column alignment is needed.
- Keys match without regard to case: the key is lower-cased and every
  comparison folds `a` to `z` to upper case. The values `yes` and `none` are
  compared the same way.
- A line that starts with `default` after trimming, in that case only, is
  marked and loses those 7 bytes. When the reader is given a directory to
  read from, it skips marked lines.
- The keys the reader takes itself are `minems`, `mincpu`, `mode`,
  `directory`, `mindos`, `polyspace` (two `,`-separated numbers), `space`,
  `cmd`, `cd`, `smartdrv` and `floppy`. Any other key goes to a list of
  handler objects, which decide whether to take it.

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| Before key padding | char[] | resource_cfg_key | Case-preserved key token, unique within the owned file. | supported | FND-RES-016 |
| Between key and equals sign | BYTE[] | resource_cfg_padding | Stored spaces aligning the equals sign at line offset 10. | supported | FND-RES-016 |
| At line offset 10 | BYTE[2] | resource_cfg_separator | Equals sign and a following space. | supported | FND-RES-016 |
| From line offset 12 | char[] | resource_cfg_value | Stored value token, without trailing whitespace; consumer interpretation unread. | supported | FND-RES-016 |
| After value | BYTE[2] | resource_cfg_delimiter | CRLF, including after the final value. | supported | FND-RES-016 |

The following compact key/value facts occur in this order. A key's name does
not establish its runtime effect; digit tokens are retained as text.

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| `directory` | char[] | resource_cfg_value | Stored token `\SIERRA\CONQUER`. | supported | FND-RES-016 |
| `videoDrv` | char[] | resource_cfg_value | Stored token `none`. | supported | FND-RES-016 |
| `cd` | char[] | resource_cfg_value | Stored token `yes`. | supported | FND-RES-016 |
| `joyDrv` | char[] | resource_cfg_value | Stored token `none`. | supported | FND-RES-016 |
| `memoryDrv` | char[] | resource_cfg_value | Stored token `none`. | supported | FND-RES-016 |
| `minCPU` | char[] | resource_cfg_value | Stored token `486`. | supported | FND-RES-016 |
| `minDOS` | char[] | resource_cfg_value | Stored token `300`. | supported | FND-RES-016 |
| `mode` | char[] | resource_cfg_value | Stored token `real`. | supported | FND-RES-016 |
| `mouseDrv` | char[] | resource_cfg_value | Stored token `none`. | supported | FND-RES-016 |
| `smartDrv` | char[] | resource_cfg_value | Stored token `yes`. | supported | FND-RES-016 |

No blank lines, comments, quotes, tabs, NUL, byte-order mark or non-ASCII bytes
occur. Backslashes in the path token are retained literally; no escaping or
path-resolution behavior is asserted.

## Enumerations and flags

For `cd`, `smartDrv` and `floppy` (the last only when no directory is
given), the reader sets a flag only for the value `yes`, compared without
case; any other value leaves the flag as it was [FND-RES-066]. The meaning of each flag, and the values other keys accept,
are not established. The stored letter
and digit tokens above are not a list of all accepted values.

## Differences between builds

None known.

## Coverage

FND-RES-016 checks the whole listed file, every line and exact byte reconstruction.
This entry does not cover similarly named files in media subdirectories or other
configuration files. FND-RES-066 reads INST.EXE's reader and its line splitter;
the run-time library routines it calls for opening, reading lines and number
conversion, the handler objects and the uses of the stored values are not
read.

## Open questions

- Does INST.EXE reach its read of `resource.cfg` when `INSTALL.BAT` starts it
  as `inst.exe -f`? Main reaches it unless one of the calls the object makes
  first ends the run (FND-RES-066); along direct calls those end it only on
  failures, and eleven indirect calls are not resolved (FND-RES-045).
  (Q-RES-192)
- How does the consumer interpret the `directory` value token? Its stored spelling
  alone does not establish its effect. (Q-RES-141)
- How does the consumer interpret the `videoDrv` value token? Its stored spelling
  alone does not establish its effect. (Q-RES-142)
- How does the consumer interpret the `cd` value token? Its stored spelling
  alone does not establish its effect. (Q-RES-143)
- How does the consumer interpret the `joyDrv` value token? Its stored spelling
  alone does not establish its effect. (Q-RES-144)
- How does the consumer interpret the `memoryDrv` value token? Its stored spelling
  alone does not establish its effect. (Q-RES-145)
- How does the consumer interpret the `minCPU` value token? Its stored spelling
  alone does not establish its effect. (Q-RES-146)
- How does the consumer interpret the `minDOS` value token? Its stored spelling
  alone does not establish its effect. (Q-RES-147)
- How does the consumer interpret the `mode` value token? Its stored spelling
  alone does not establish its effect. (Q-RES-148)
- How does the consumer interpret the `mouseDrv` value token? Its stored spelling
  alone does not establish its effect. (Q-RES-149)
- How does the consumer interpret the `smartDrv` value token? Its stored spelling
  alone does not establish its effect. (Q-RES-150)
