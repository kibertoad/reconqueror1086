# Bounded evidence reporters

Use the installed toolkit packages through Conqueror's wrapper:

```sh
pnpm install --frozen-lockfile
python -m pip install --require-hashes -r requirements-evidence.txt
node tools/evidence/report.mjs x86-returns <local-config.json>
```

The toolkit's [reporter guide](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/blob/92a5592055559013373ea243a458f4ffc4fa05c4/docs/bounded-evidence-reporters.md)
defines the report commands, controls, caps and limitations. The packaged reader
prepares MZ/FBOV queries and the engine reads instruction paths; the existing
Conqueror wrapper keeps its own inventory and review contracts.
`EVIDENCE_PYTHON` selects the Python interpreter with the locked engine. Without
it, the wrapper and the evidence tests use the first of `python` and `python3`
that starts as Python 3.12 or later (`tools/evidence/python.mjs`). The reader
and the engine speak one prepared-config protocol (3 at the pinned releases), so
they are upgraded together.
The direct Python entry point is `python -m scientific_method_engine`.

A config names its source by `xxh3`, the XXH3-128 hash the build entry gives, as
32 lower-case hex digits. Every command refuses a source with another hash and a
config that still names a `sha256`, and the report's `sourceIdentity` repeats the
`xxh3` it checked. `node tools/evidence/xxh3.mjs <file>...` prints that hash.

`node tools/evidence/unlzexe.mjs <packed.exe> <unpacked.exe>` unpacks a DOS
executable packed by LZEXE 0.91 (INST.EXE) and prints the unpacked file's size
and `xxh3`, which the build's `unpacked` item records with this tool's commit.
The unpacked header is the tool's own, described at the top of the script, so
another unpacker gives other bytes. It refuses LZEXE 0.90 and writes no file
that already exists. The executable reader has no LZEXE support.

`x86-imports` and `x86-table` forward to the reader's `imports` and `table`
commands, which run in Node without the engine. `imports` maps each import
address table slot of a PE file (AUTOPLAY.EXE) to the import its tables name,
under a positive control slot, as STATUS-42 of the standard requires of a
finding that names an imported function. `table` reads one pointer table's
entries from an `mz` or `pe32` source and compares an analyzer's listing with
them, as STATUS-43 describes. Neither reads NE or LE files.

Conqueror's DOS/16M-bound LE image is outside the generic MZ/FBOV/PE loader set.
Keep its existing LE mapping and committed-inventory adapter. This dependency
migration does not make generic loader reports evidence for LE behavior.
Original query inputs and reports stay local under `GAME_DIR`; use synthetic
state for CI. No evidence status changes follow from package adoption.
