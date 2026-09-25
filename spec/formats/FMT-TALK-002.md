---
id: FMT-TALK-002
title: Conversation node, one record of ALL.CBF
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: null
text: false
definition: fmt_talk_002.ksy
evidence: [FND-TALK-001, FND-TALK-002, FND-TALK-003]
conflicting: []
split_with: []
related: [RULE-TALK-001]
---

## Layout

A record of the `ALL.CBF` entry of `C1086.GOB`, at the offset its FMT-TALK-001 record gives: a fixed header, then its strings, each `length + 1` bytes with the lengths taken from the header. The strings are the portrait name, the speaker, the prompt variants and the responses, in that order. A run of lengths ends at its first length of 0 or less, so the strings are read up to that point; the loop reads the prompt and response counts from their own bytes.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x000` | 4 | `INT32LE` | `portrait_length` | Length of the portrait string. | supported | FND-TALK-001 |
| `0x004` | 4 | `INT32LE` | `speaker_length` | Length of the speaker string. | supported | FND-TALK-001 |
| `0x008` | 1 | `UINT8` | `prompt_count` | Number of prompt variants, 0 for a node with no text. | supported | FND-TALK-001, FND-TALK-002, FND-TALK-003 |
| `0x009` | 3 | `BYTE[3]` | `unk_009` | Purpose unknown; not copied. | supported | FND-TALK-001 |
| `0x00C` | 40 | `INT32LE[10]` | `prompt_lengths` | Lengths of the prompt variants. | supported | FND-TALK-001 |
| `0x034` | 20 | `INT32LE[5]` | `response_lengths` | Lengths of the responses. | supported | FND-TALK-001 |
| `0x048` | 1 | `UINT8` | `response_count` | Number of responses, 0 to 5; 0 makes the node a statement. | supported | FND-TALK-001, FND-TALK-002 |
| `0x049` | 3 | `BYTE[3]` | `unk_049` | Not copied; every record in the GOG archive holds `65 3A 5C`. | supported | FND-TALK-001 |
| `0x04C` | 20 | `INT32LE[5]` | `targets` | The node each response leads to, or for a statement `targets[0]` is the next node; 0 ends the conversation. | supported | FND-TALK-001, FND-TALK-002 |
| `0x060` | 600 | `INT32LE[150]` | `response_actions` | 30 action numbers per response, response `r` at `r * 30`; -1 marks an empty slot. | supported | FND-TALK-001, FND-TALK-002 |
| `0x2B8` | 120 | `INT32LE[30]` | `node_actions` | Action numbers run for every pass through the node; -1 marks an empty slot. | supported | FND-TALK-001, FND-TALK-002 |
| `0x330` | 20 | `INT32LE[5]` | `unk_330` | Copied into the node; no reader was found. | supported | FND-TALK-001 |
| `0x344` | 4 | `INT32LE` | `unk_344` | Copied into the node; no reader was found. | supported | FND-TALK-001 |
| `0x348` | | `char[]` | `strings` | The strings, each followed by a NUL, as described above. | supported | FND-TALK-001 |
| | | | | Total size `0x348` plus the strings | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read against the node reader `0x000163B8` and the copy `0x00016684`; the GOG archive holds 1,311 records [FND-TALK-001].

## Open questions

- The purpose of `unk_009`, `unk_049`, `unk_330` and `unk_344`.
