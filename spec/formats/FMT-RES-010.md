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
evidence: [FND-RES-016]
conflicting: []
split_with: []
related: []
---

## Layout

The owned file stores ASCII key/value lines terminated by CRLF. Keys begin at
line offset 0, are padded with spaces up to offset 10, and are followed by an
equals sign and one space. Values start at offset 12 and end before CRLF.
Every line, including the last, has that delimiter. These are observed-file
constraints, not the full language accepted by a reader [FND-RES-016].

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

No semantic enumeration or boolean mapping is established. The stored letter
and digit tokens above are not a list of all accepted values.

## Differences between builds

None known.

## Coverage

FND-RES-016 checks the whole listed file, every line and exact byte reconstruction.
This entry does not cover similarly named files in media subdirectories or other
configuration files. No shipped reader or consumer behavior is established.

## Open questions

- Which shipped reader consumes this CD-root file, and is the path reachable?
  (Q-RES-138)
- Does the reader require the observed fixed alignment or accept other spacing?
  Both fit this file; inspect its token-boundary logic. (Q-RES-139)
- Does the reader match key casing exactly or without case distinctions?
  Both fit this file; inspect its key comparison. (Q-RES-140)
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
