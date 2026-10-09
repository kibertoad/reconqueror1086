# Original function inventories

The committed inventories use the Standard's native addresses and `start`,
`size`, `ranges` columns. `size` is the sum of the separate half-open body
ranges, not a contiguous span. Paths follow the manifest; `CD:` becomes `@CD/`.
No original names, strings, instructions, operands or bytes are exported.

Each inventory has adjacent `.provenance.tsv` and `.regions.tsv` files. The
provenance records the build's source XXH3, source SHA-256, analyzer version and
language, frozen project digest, exporter digest, and explicit exclusions.
The regions partition source-backed executable blocks into decoded instruction,
defined data and undefined bytes, with separate counts outside the union of
function bodies. Shared tails count once in that union. Analyzer metadata does
not establish native reachability, exhaustive discovery or complete readings.

## Reproduction and verification

Run `tools/ghidra/ExportCoverageSnapshot.java` against a verified frozen project
with `-readOnly -noanalysis`, a new ignored output directory and the expected
source SHA-256. Require its COVERAGE_EXPORT_COMPLETE marker, successful script
completion, identical repeated exports and an unchanged project digest. Use
`node tools/evidence/coverage-snapshot.mjs snapshot <project.rep>` to compute the
ordered directory digest; symbolic links are rejected.

Adopt with `node tools/evidence/coverage-snapshot.mjs adopt <local-contract.json>`.
The contract names the build, manifest, source, XXH3/SHA-256, frozen project,
snapshot digest, raw export, completion log, mapping and destination. It stays
local because it identifies the owned installation and analysis database.
Adoption validates all data before replacing the inventory and its sidecars.

Run `node tools/Check-Coverage.mjs` to parse native bodies with the pinned shared
Standard checker and validate source identities, region partitions, body
intersections, unique unions and exclusions. Run the documentation checker to
validate citations and range endpoints. Neither check proves behavior.

## Source mappings

CONQUER.EXE uses the LE mapping recorded in FND-RES-009 and
`x86:LE:32:default`. Ordinary MZ import sees its outer loader and cannot supply
game-code coverage. The retained snapshot preserves raw fixup operands, so
indirect targets still need separate evidence. FND-RES-062 records the metadata
extents used for the evidence-boundary conversion.

AUTOPLAY.EXE uses PE preferred virtual addresses with `x86:LE:32:default`.
SETUP.EXE uses `x86:LE:16:Protected Mode`; analyzer selectors 1000 and 1008 map
to native NE table segments 0001 and 0002. FND-RES-024 records the container
structure. Auxiliary analyzer definitions without a source-backed segment are
excluded explicitly in the provenance, rather than counted as native code.

CONFIG.EXE and the unpacked INST.EXE use the Old-style DOS Executable loader,
`x86:LE:16:Real Mode`, load segment 0x1000. Segmented aliases map to one physical
address and are written as canonical paragraph:low-nibble addresses. INST.EXE's
source identity is the unpacked identity in BLD-GOG-EN; obtain it using the
procedure in FND-RES-066. A whole rebuilt MZ project was frozen and exported
twice after its repository bookkeeping stabilized.

The older size-only exports remain historical validation records. They cannot
recover body extents by adding size to start and are not inputs to the current
coverage checker. Raw exports, databases, logs and source files remain ignored.
The HMI data-container analysis in FND-RES-013 supplies no function inventory.

## Progress reports

`node tools/Report-Coverage.mjs` reads only committed spec and coverage metadata.
Keep its JSON output in ignored artifacts. It checks the sidecars before joining
locations to actual body ranges. Shared tails count once; a location in a gap
does not cite either adjoining body. For packed files, loaded addresses use the
manifest's unpacked format and verified inventory identity; file-data offsets
remain separate.

The report distinguishes all citations from research citations after removing
the metadata-only findings named in `baseline.json`. For each it gives both the
unique bytes of cited bodies and the actual bytes intersected by locations.
Neither is a complete-reading count. Complete-reading coverage is unavailable
until a function-level audit checks the findings in `complete_reading`; the
number of established entries is not a substitute.

The generated report names manifest code files without inventories. The baseline
also records opaque driver payloads, imported/generated code and unresolved area
assignments. These limits keep Survey open; an absent denominator is unavailable,
not zero. `compareInventories` in the progress helper distinguishes additions,
removals and changed body boundaries when comparing frozen revisions. It does
not infer why a definition disappeared or credit denominator changes as research.
