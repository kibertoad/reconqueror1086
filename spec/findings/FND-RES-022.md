---
id: FND-RES-022
title: INF candidates retain different token families and equals signs inside command-shaped lines
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:AUTORUN.INF
    offset: 0x00000000..0x000002DE
  - build: BLD-GOG-EN
    file: CD:CONQUER.INF
    offset: 0x00000000..0x00000039
  - build: BLD-GOG-EN
    file: CD:LANGUAGE.INF
    offset: 0x00000000..0x00000787
  - build: BLD-GOG-EN
    file: CD:SIERRA.INF
    offset: 0x00000000..0x00000D6F
tool: Python complete token-shape traversal retaining original CRLF regions
environment: null
---

## Observation

The complete extents, canonical fingerprints and byte reconstruction agree with
FND-RES-021. This pass classifies tokens rather than treating an equals sign as
proof of an assignment grammar. No consumer was read or run.

AUTORUN has bracket remainders autorun, Sierra, english, french and german in
that order. Its equals-bearing lines include OPEN, ICON, Title, DirName,
NecFiles, ExeName and DefaultLang before repeated language-associated token
families runText1 and installText1 through installText3. The labels alone do not
establish shell launch, language selection or installation behavior.

CONQUER has four plain name-shaped records, each followed by CRLF. There is no
bracket marker, equals sign or comma in these records. Their original writing
is not reproduced and their presentation or purpose is not inferred.

LANGUAGE has bracket remainders Ident and Strings. Among its equals-bearing
regions, the exact left token InstallDoneTitle occurs twice. Duplicate counting
retains the spelling and does not collapse records. First-wins, last-wins and
multiple-use behavior are not distinguished by stored text.

SIERRA has bracket remainders Script, Setup, Requirements, Ident, Dialogs and
Files. Four equals-bearing regions have left portions beginning with APPEND or
a FLAG-prefixed APPEND shape, followed by a destination-path-shaped token and
another token. They are distinct from its shorter identifier-shaped left tokens.
This records token shape, not a command interpreter or the effects of a write.
No full command sequence, value prose or destination contents are retained.

Its other-line class includes regions beginning with digit bytes and regions
with one, three, four or five commas, as well as comma-free regions. No quote
byte occurs in these other regions. This pass retains all of them and makes no
claim that comma counts imply a particular record schema.

## Interpretation

Filename suffix and equals-sign presence do not establish a shared key/value
language. The retained plain, section-shaped, command-shaped and comma-bearing
regions require consumer-led reconciliation. A generic assignment splitter
would mislabel the four APPEND-shaped left portions and omit the other class.
Duplicate lookup is a separate question from text framing.

## Alternatives

- Every equals-bearing line having an identifier-only left side is ruled out
  by the four multi-token APPEND-shaped portions.
- Unique left tokens in LANGUAGE are ruled out by the repeated InstallDoneTitle.
- A permissive shared parser and independent parsers remain possible; only
  reader identification and token handling decide this distinction.
- Digit-led comma regions may be records, commands or data for another reader;
  their shape alone establishes none of those semantic roles.

## How to reproduce

Repeat FND-RES-021's bounded complete-file read and fingerprint checks. Preserve
every line and CRLF. Read bracket remainders and equals-bearing left portions
without assuming their roles; count complete repeated left tokens. Keep the
semicolon and other-line classes separate. Classify commas, quotes and digit
prefixes within the complete other class. Compare the exact reconstruction again.
Export compact token facts only, excluding resource prose and command sequences.
