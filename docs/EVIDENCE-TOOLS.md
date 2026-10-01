# Bounded evidence tools

The adopted tools require Node.js 22 or later. Their synthetic tests run in the
canonical validation gate. Reports, input configurations and extracted original
bytes remain in the ignored local evidence store, never in Git.

## Conqueror executable boundary

BLD-GOG-EN identifies a DOS/16M-bound LE executable. The generic MZ/FBOV operand,
incoming-call and inventory resolver does **not** support this container. Do not
pass it a stripped stub or pretend its addresses are MZ addresses. Use the
Conqueror inspector's explicit LE object mapping and the mapped Ghidra project
described in [ghidra.md](ghidra.md). A function inventory is analyzer metadata,
not proof of a complete reading, all callers or instruction identity.

## Reusable reports

`tools/evidence/report.mjs` implements bounded flow/table reports and the
MZ/FBOV identity and inventory resolver. `tools/evidence/x86-reporter/report.mjs`
is the separately pinned toolkit instruction-derived reporter. Its supported
formats and query contracts are in
[BOUNDED-EVIDENCE-REPORTERS.md](BOUNDED-EVIDENCE-REPORTERS.md).
Keep unsupported formats and unresolved calls explicit; synthetic acceptance of
a tool is never original-game evidence.

For each game request record the request ID, case, source fingerprint, tool
revision, input assumptions, expected observation and outcome in the researcher's
words. Distinguish operands reached only after an unresolved call from traced
effects, name every stop they depend on, and state any assumption that a callee
returns or preserves state. Keep original-derived configurations and output local;
shared reporter regressions use synthetic instructions and state.

## Function inventories

Run `ExportFunctionInventory.java` against a correctly mapped analysis project
with a new local TSV destination. It exports only starts and body byte counts.
Verify the project's source hash, loader, address map and analyzer version before
copying those two columns into `coverage/<build>/<manifest>.tsv`. `CD:` becomes
`@CD/`. Retain provenance and exclusions in `coverage/README.md`. Do not export
names, instructions, strings or bytes; a body byte count does not define an end
address, especially for noncontiguous or shared bodies.

## Committed inventory verification

The shared `inventory-check` command validates a hash-guarded MZ/FBOV source,
build and manifest. Its input path must end in the declared safe repository
path, which must match the portable coverage destination. Starts must be unique,
use the manifest prefix and eight-digit uppercase notation, and lie in mapped
source. Sizes are positive bounded body byte counts, never end addresses. Only
`start`, `size`, optional researcher-authored `name` and `out_of_scope` columns
are permitted; analyzer default names are rejected. A historical path requires
an explicit safe `legacyPath` and nonempty `legacyEvidence`.

Reconqueror uses `node tools/Check-Coverage.mjs` for its committed LE inventory.
That adapter supplies the documented LE code-object range and checks build,
manifest, destination and TSV metadata through the same verifier. Its starts
are LE virtual addresses, not MZ file offsets. It does not claim that the shared
MZ/FBOV loader supports LE or that an inventory establishes complete behavior.

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
