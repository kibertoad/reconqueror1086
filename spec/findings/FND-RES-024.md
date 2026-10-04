---
id: FND-RES-024
title: AUTOPLAY uses PE32 data mapping while SETUP uses NE segment mapping
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
    offset: 0x00000080..0x00000240
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    kind: file-data
    offset: 0x00005800..0x00006000
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    kind: file-data
    offset: 0x00000400..0x00000450
tool: Python bounded PE section/import and NE segment metadata traversal
environment: null
---

## Observation

Complete source lengths and canonical fingerprints were checked against
BLD-GOG-EN before inspecting container metadata. FND-RES-023 records the literal
filename bytes. These probes read data tables only, not executable instructions.

AUTOPLAY's new header is PE at file offset 128, with machine value 332 and
optional-header magic 267. The preferred image base is 0x00400000. Its second
section has RVA 0x00020000, raw offset 19968 and raw length 2560. Consequently
LANGUAGE.INF's file offset 20353 maps to preferred address 0x00420181, and
AUTORUN.INF's file offset 21390 maps to 0x0042058E. These are preferred-image
mappings, not observations of a loaded process or code references.

The fourth section maps RVA 0x00040000 to raw offset 22528 and spans 2048 raw
bytes. Bounded import-descriptor and thunk traversal names KERNEL32.DLL imports
GetPrivateProfileSectionA and GetPrivateProfileStringA. Their IAT slot preferred
addresses are 0x0044016C and 0x00440170 respectively. Their presence establishes
neither a call nor which filename a call receives. No import implementation is
read or modeled. Descriptor and thunk traversal caps were 128 and 1024; neither
was reached before its zero terminator. Name reads were bounded to 256 bytes.

SETUP instead has an NE header at file offset 1024. The segment table begins at
1088, contains two records and uses alignment shift 4. Segment 2 has raw offset
19968, stored length 4634 and flags value 3153, whose data-segment bit is set.
The two SIERRA.INF occurrences lie at segment-relative offsets 818 and 842.
A runtime selector is not determined by these segment numbers. NE offsets must
not be interpreted through AUTOPLAY's PE image base or the game's LE mapping.

Every filename extent lies wholly inside its identified initialized data span.
A PE section with raw offset zero was excluded from filename ownership; a size
alone does not establish that it owns bytes at the start of the file. No original
program ran, no address was used to patch state, and no code range is established.

## Interpretation

The launcher candidates need different loaders. AUTOPLAY's profile imports are
specific leads for argument tracing, while SETUP's filenames need NE segment
and relocation provenance before their code references can be named.

## Alternatives

- A single NE mapping for both candidates is ruled out by AUTOPLAY's PE header.
- A single PE mapping is ruled out by SETUP's NE header.
- Imported profile APIs may be used for these INF paths, other configuration
  paths or unreachable code. The metadata cannot choose among these readings.
- A filename's data location alone is not proof of a reference or file open.

## How to reproduce

Repeat the manifest-guarded root-file reads of FND-RES-023. Bound the MZ new-header
pointer before checking the actual signature. For AUTOPLAY, validate the PE32
optional header and section table, map initialized file spans by section RVA and
preferred base, then traverse bounded terminated import descriptors and thunks.
For SETUP, bound the NE segment table and apply its alignment shift to stored
segment offsets. Locate each complete filename inside exactly one initialized
span. Keep file offsets, preferred addresses and NE segment-relative offsets
separate; export no instructions or broad import/string listing.
