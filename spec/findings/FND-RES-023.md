---
id: FND-RES-023
title: Root executable filename occurrences identify AUTOPLAY and SETUP as INF consumer leads
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    kind: file-data
    offset: 0x00004F81..0x00004F8D
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    kind: file-data
    offset: 0x0000538E..0x00005399
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    kind: file-data
    offset: 0x00005132..0x0000513C
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    kind: file-data
    offset: 0x0000514A..0x00005154
tool: Python complete root executable literal scan with manifest identity guards
environment: null
---

## Observation

A bounded ISO root traversal selected every root file whose name ends EXE or
DLL. The selected files were AUTOPLAY.EXE, BOOTDISK.EXE, CONFIG.EXE, CONQUER.EXE,
INST.EXE and SETUP.EXE; no root DLL was selected. Each complete file's length
and canonical XXH3-128 matched BLD-GOG-EN before searching. A 16 MiB read bound
was applied; none of these reads reached it.

The scan searched all bytes of those files for each of AUTORUN.INF, CONQUER.INF,
LANGUAGE.INF and SIERRA.INF. It searched ASCII and UTF-16LE spellings with ASCII
letter case folding, retained every occurrence and imposed no match-count cap.
No executable instructions were decoded or run.

| File | Filename bytes | File offset | Encoding shape |
|---|---|---|---|
| CD:AUTOPLAY.EXE | LANGUAGE.INF | 0x00004F81 | ASCII |
| CD:AUTOPLAY.EXE | AUTORUN.INF | 0x0000538E | ASCII |
| CD:SETUP.EXE | SIERRA.INF | 0x00005132 | ASCII |
| CD:SETUP.EXE | SIERRA.INF | 0x0000514A | ASCII |

These are byte occurrences, not proven terminated strings or file-open inputs.
No other requested occurrence was found in this search domain, including no
UTF-16LE occurrence. The observed ASCII hits are positive controls for the
literal search. No claim is made about subdirectories, installation-side files,
compressed/unpacked data, archives, constructed filenames or other encodings.

## Interpretation

AUTOPLAY and SETUP are concrete candidates for consumer analysis. The two
SIERRA occurrences must be traced separately rather than assuming they share a
caller or role. The absence of a CONQUER literal in the stated domain does not
establish that its file is unused.

## Alternatives

- A filename occurrence may be a diagnostic, resource label or file-open input;
  code references and the path to an opening routine decide which.
- The two SIERRA occurrences may support distinct open paths or repeated data
  for one path; no executable reading distinguishes them yet.
- CONQUER may be opened through a constructed name, another executable or data
  outside this scan; all remain possible. The literal absence settles none.

## How to reproduce

Verify the owned build and bounded root-directory traversal. Select all root
EXE/DLL files and require each complete extent to match its manifest size and
fingerprint. Search every byte for the four filename spellings in ASCII and
UTF-16LE under ASCII case folding. Retain all offsets without a match cap, and
compare the positive occurrences above. Report the exact selected-file domain;
do not reinterpret negative literal results as absence of runtime use. Export
only compact filename/offset facts, not executable bytes or instruction listings.
