---
id: FMT-PERSON-002
title: Youth dilemma, DILEM0.DAT to DILEM29.DAT
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-PERSON-005, FND-PERSON-010]
conflicting: []
split_with: []
related: [RULE-PERSON-004]
---

## Layout

The `DILEM0.DAT` to `DILEM29.DAT` entries of `C1086.GOB` are 7-bit ASCII text with an optional
closing `0x1A`, read a line at a time with `fgets` into a 256-byte buffer. The marker reads skip
lines whose first byte is a space, `#`, CR or LF, so `#` lines are comments. Each section starts
with a line whose first byte is its marker; the text after the marker on that line is commentary.
Text lines start with `^`, and the text after it is shown. A missing marker ends the program with
exit code 1. Attribute names are those of `attribute_names` in FMT-PERSON-001, plus `NONE`.

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| `#` lines | comment | none | Comments; the files give the age and a title in them. | supported | FND-PERSON-010 |
| `!` | marker | `identity_header` | Starts the identity. | supported | FND-PERSON-005, FND-PERSON-010 |
| after `!` | `INT32`, `char[]` | `number`, `scene` | The dilemma number, equal to the one in the file name, and the `.CSF` file of its pictures. | supported | FND-PERSON-010 |
| `&` | marker | `prompt_header` | Starts the prompt. | supported | FND-PERSON-005, FND-PERSON-010 |
| `^` lines after `&` | `char[]` | `prompt` | The question, one `^` line per line of text. | supported | FND-PERSON-010 |
| `@`, three times | marker | `choice_header` | Starts `choices[c]`, `c` from 0 to 2 in file order. | supported | FND-PERSON-010 |
| `~` line | `char[]` | `choices[c].scoring_attribute` | The attribute the choice scores, or `NONE`. | supported | FND-PERSON-010 |
| `%` | marker | `breakpoints_header` | Starts the breakpoints. | supported | FND-PERSON-005, FND-PERSON-010 |
| after `%` | `INT32`, `INT32` | `choices[c].breakpoints` | Two values, the higher first: a score at least the first wins, at least the second draws. | supported | FND-PERSON-005, FND-PERSON-010 |
| `?`, three times per choice | marker | `outcome_header` | Starts `choices[c].outcomes[o]`, `o` 0 win, 1 draw, 2 lose, in that order; the commentary names the choice and outcome. | supported | FND-PERSON-005, FND-PERSON-010 |
| `^` lines after `?` | `char[]` | `choices[c].outcomes[o].text` | The outcome's text. | supported | FND-PERSON-010 |
| `*` | marker | `modifier_count_header` | Starts the count. | supported | FND-PERSON-005, FND-PERSON-010 |
| after `*` | `INT32` | `choices[c].outcomes[o].modifier_count` | The number of modifier lines. | supported | FND-PERSON-010 |
| `$` | marker | `modifiers_header` | Starts the modifiers. | supported | FND-PERSON-010 |
| after `$`, `modifier_count` lines | `char[]`, `INT32` | `choices[c].outcomes[o].modifiers[i]` | An attribute name and a signed change. | supported | FND-PERSON-005, FND-PERSON-010 |

## Enumerations and flags

Outcome indexes: 0 win, 1 draw, 2 lose.

## Differences between builds

None known.

## Coverage

All 30 dilemma entries of the GOG archive [FND-PERSON-010].

## Open questions

- The order in which the executable's parser `0x00019302` reads the sections: it checks `!`, `&`,
  `%`, `*` and `?` in that order of calls, which may mean it reads each kind of section for all
  three choices in turn.
- Which field the loader gives `NONE`, and how it turns names into fields; it receives
  `attribute_names` and `attribute_count`.
- What the arguments 3, 3, 2 and 5 of `0x00018B60` limit.
