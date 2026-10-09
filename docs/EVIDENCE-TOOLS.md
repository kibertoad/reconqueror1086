# Bounded evidence tools

The adopted tools require Node.js 22 or later. Their synthetic tests run in the
canonical validation gate. Reports, input configurations and extracted original
bytes remain in the ignored local evidence store, never in Git.

## Conqueror executable boundary

BLD-GOG-EN identifies a DOS/16M-bound LE executable. The generic MZ/FBOV operand
and incoming-call resolver does **not** support this container. Do not
pass it a stripped stub or pretend its addresses are MZ addresses. Use the
Conqueror inspector's explicit LE object mapping and the mapped Ghidra project
described in [ghidra.md](ghidra.md). A function inventory is analyzer metadata,
not proof of a complete reading, all callers or instruction identity.

## Reusable reports

`tools/evidence/report.mjs` implements bounded flow/table reports, native inventory
adoption and checking, and the MZ/FBOV identity resolver. `@scientific-method/executable-reader`
is the separately locked toolkit package instruction-derived reporter. Its supported
formats and query contracts are in
[BOUNDED-EVIDENCE-REPORTERS.md](BOUNDED-EVIDENCE-REPORTERS.md). Source-derived reports
name their source by the `xxh3` its build entry gives and refuse another file.
Keep unsupported formats and unresolved calls explicit; synthetic acceptance of
a tool is never original-game evidence.

For each game request record the request ID, case, source fingerprint, tool
revision, input assumptions, expected observation and outcome in the researcher's
words. Distinguish operands reached only after an unresolved call from traced
effects, name every stop they depend on, and state any assumption that a callee
returns or preserves state. Keep original-derived configurations and output local;
shared reporter regressions use synthetic instructions and state.

## Packed executables

The compatibility `tools/evidence/unlzexe.mjs` command delegates decoding to the
pinned executable-reader `unpack` API and retains exclusive output creation.
Its report includes the reader version and layout rule alongside the unpacked
identity. Use `report.mjs x86-unpack` with a hash-guarded local configuration
when using the shared command contract. BLD-GOG-EN records the installer's
unpacked identity and the package/layout that reproduces it. Historical findings
retain the revision-specific procedures they originally recorded.

## Native function inventories

Use `ExportCoverageSnapshot.java` against a verified frozen project with
`-readOnly -noanalysis`. It exports starts, body-byte counts, separate body
ranges and executable-region partitions. Compare repeated exports and unchanged
project digests before adoption. `coverage-snapshot.mjs adopt` converts the
native coordinates and writes the inventory with provenance and region
sidecars. The procedure and mapping qualifications are in
[coverage/README.md](../coverage/README.md). Do not export names, instructions,
strings or bytes; a body byte count does not define an end address.

## Committed inventory verification

Use `node tools/Check-Coverage.mjs` for all committed inventories. It loads the
spec and native body notation with the pinned Standard checker, then validates
the sidecars, source identities, executable partitions and unique body unions.
Starts have no manifest prefix: LE/PE use virtual addresses; MZ uses native
segment:offset addresses; NE uses table segment:offset addresses. The manifest
is identified by the inventory path. `report.mjs inventory` uses the same frozen
snapshot contract as `coverage-snapshot.mjs adopt`. `report.mjs inventory-check`
takes a local JSON object with `repositoryRoot` and optional `build` and
`manifest` selectors, and checks committed metadata without the original files.
`x86-inventory-check` is the shared source-derived call-target report.
Historical size-only contracts are refused: byte counts cannot supply missing
body ranges or establish a complete reading.

Use `x86-target` before citing a far call's target: it keeps the raw operand,
the relocation or FBOV fixup, the stored descriptor word and decoded index, the
trampoline and the canonical target, and compares an analyzer's address with
each instead of replacing them. Use `x86-bounds` and `x86-owner` before joining a
call to its caller or bounding a reading by an analyzer's size: an analyzer's
size is a body-byte count and is never added to a start to make an end. Give
`formatControls` the build's known relocation, descriptor, overlay, fixup and
trampoline counts so a misread table fails the query. Declare a resident
segment's bounds from the build's code ranges in `segments` so an incoming
search over part of it is reported as partial.

The latest toolkit also supplies `x86-pointer-inventory` and instruction-derived
dispatch recovery. Use the bounded reporter guide for supported source formats
and contracts; these reporters do not support Conqueror LE code analysis.
