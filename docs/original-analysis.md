# Original-data inspection utilities

`tools/Conqueror.Inspect` is the reproducible, read-only bridge from an owned GOG installation to local reverse-engineering evidence. It never changes the installation and never adds proprietary artifacts to source control.

## Run

From the repository root:

```powershell
dotnet run --project tools/Conqueror.Inspect -- "C:\GOG Games\Conqueror AD1086" "analysis\original" wager joust skirmish
```

CSF previews may name either a top-level GOB resource or one nested scene resource as
`ARCHIVE.RES/resource`. A raw 768-byte palette or a paletted PCX can supply colors; use
`--preview-only` to stop after the bounded render instead of repeating population scans:

```powershell
dotnet run --project tools/Conqueror.Inspect -- `
  "C:\GOG Games\Conqueror AD1086" "analysis\original" `
  --render-csf=SKIRMISH.RES/skirmish.csf `
  --palette-pcx=SKIRMISH.RES/SKIRMISH.PAL --preview-only
```

The first argument is the installed-game directory, the second is an output directory inside this repository, and any remaining arguments are case-insensitive printable-string search terms. With no arguments, the utility uses the paths shown above.

End users should use `Install Original Resources.bat` instead. It runs `tools/Conqueror.Import`, extracts owned SMK/RES/CSF/PCX/PCC/LOW/audio resources into ignored `UserContent`, copies the owned GOB archive, converts Red Book CD audio losslessly to runtime-loadable PCM WAV, and records every installed file in `manifest.json`. MonoGame's runtime stream loader requires PCM RIFF data; compressed Ogg/MP3 support normally goes through the build-time content pipeline, while FLAC would require an additional decoder.

The importer and runtime catalog are format-neutral: assets are addressed by original path and kind, and the game rejects manifest paths that escape `UserContent`. The current runtime plays imported CD audio and direct file-backed Smacker movies, renders validated PCX/PCC/CSF presentation art, and binds all 30 marker-delimited dilemma definitions to the executable-confirmed six-age selection and outcome interpreter. The complete `ALL.CIF`/`ALL.CBF` conversation graph and its referenced portraits are decoded once at startup; the populated inn now enters its ten confirmed selector roots, displays original prompts and responses, follows response/continuation edges, and returns on terminal target zero. Original prose remains in ignored local content; the presence of an imported asset must not be reported as equivalent to displaying or playing it. On the hashed GOG build, an end-to-end import produces 29,226 catalog entries with zero duplicate output paths. The installer decodes every supported byte-stored, kind-1, and kind-2 entry from the GOB and all 99 `.RES`/`.LOW` scene containers; source extensions remain in decoded directory names so paired scene tiers stay distinct.

Runtime conversation entry also covers all six named tournament-lady selector roots and blacksmith node `3201`; the latter must not be confused with inn bartender selector `3200`.

## What it does

1. Parses `game.ins` to find the end of CD data track 1.
2. Reads MODE1/2352 sectors directly, exposing only their 2,048-byte ISO payloads.
3. Walks the ISO-9660 directory tree without mounting or modifying the image.
4. Writes a complete CD manifest and SHA-256 provenance hashes.
5. Locally extracts small executable/configuration artifacts useful for inspection.
6. Searches printable strings in `CONQUER.EXE` and the installed `C1086.GOB`, recording byte offsets for follow-up analysis.
7. Parses the bounded top-level Dynamix archive directory and reports names, flags, sizes, and offsets. For kind-1 entries it validates every two-byte-length-prefixed block and expected 16 KiB output slice; for kind 2 it applies the executable-confirmed MSB-first 9-to-14-bit LZW stream. It validates `.666` resources as bounded rate-tagged sample banks and records metadata without exporting audio. The end-user importer writes every supported decoded entry through one centralized codec registry.

`gob-compression-report.txt` is a generated local report. On the hashed build, all 471 kind-1 GOB entries divide exactly into 2,963 expected 16 KiB output slices: 2,940 compressed-marker blocks and 23 verbatim blocks. The 99-container `.RES`/`.LOW` scene report adds 29,118 compressed-marker and eight verbatim blocks. All 32,089 blocks decode to their exact expected sizes with the bounded LZ/RLE decoder. The five unequal-size kind-2 entries also decode exactly with the separately bounded adaptive LZW decoder. The reports contain names and structural results, not extracted payloads. The shared resource library retains a distinct LSB-first LZW decoder for documented inner chunk streams; real-data probes disproved both that codec and classic LH1 as interpretations of outer kind 1.

`scene-res-report.txt` repeats the same bounded inventory directly from all 99 scene containers in the ISO without writing their payloads. `scene-texture-report.txt` validates all 12,982 `TEXnnn width height` entries as exact headerless `width * height` byte planes and records only counts and distinct dimensions. `scene-scenario-report.txt` validates all 90 Scenario-bearing containers and records only color-map parameters, block-offset populations, and whether each stored active family exactly matches executable-confirmed regeneration under `SKIRMISH.PAL`. `resource-extension-report.txt` aggregates storage kinds, and `stored-image-report.txt` validates stored, kind-1, and kind-2 PCX-compatible images using dimensions and pixel-index hashes. It identifies 192 strict PCX payloads, including `richard.pcc`, `fftitle.pcx`, `engmap1.pcx`, and all four kind-2 PCX entries. The runtime catalog decodes installed PCX-compatible assets into bounded indexed pixels and RGBA data without creating a graphics device.

`sound-bank-report.txt` validates all 26 `.666` banks across the GOB and scene archives and records only provenance, sample counts, rates, and aggregate byte sizes. The 102 framed samples consume every decoded bank exactly. `VSMITH.666` is therefore known to be audio rather than the blacksmith dialogue database. `ALL.CIF` and `ALL.CBF` are the actual indexed conversation-node family: all 1,311 nodes, 2,062 prompt variants, 2,696 response branches, and 63 zero-response continuation slots validate structurally. The ten inn patron selector roots and their first named nodes are confirmed; their decoded action conditions, mutations, redirects, typed character selectors, and named item bindings now run in the imported conversation session.

`smacker-report.txt` validates the fixed header, frame tables, tree extent, exact aligned frame extents, palette updates, audio packets, and video decoding of every `.SMK` without exporting movie content. All 2,131 files are `SMK2`: 2,095 have one packed 8-bit mono track and 36 are silent. Together they contain 182,360 indexed frames, 2,135 palette changes, and 161,884 audio packets in 288,867,980 source bytes. The bounded predictive-Huffman decoder expands every audio packet to its declared length, totaling 398,368,812 unsigned PCM bytes; the stateful four-tree video decoder reconstructs every frame and records stable final-frame hashes. A local `TITLE.SMK` render independently confirms coherent palette, orientation, and composition. Direct decoding therefore needs no installation-time transcoding dependency. The runtime activates `TITLE.SMK`, `CREDITZZ.SMK`, and the item previews named by `WEAPONS.DAT` through the MonoGame adapter, reusing its texture and image buffers and handing decoded audio packets to a dynamic mono sound instance. All 23 store records that name a movie have an exact installed disc-file match; the other 17 use `#` for no preview. A seekable reader keeps only the index/tree prefix and one compressed frame in memory. CD music pauses around event movies and starts only after the opening movie ends or is skipped.

Installation and activation remain separate states. A successful import means the owned source files were inventoried and supported archive entries were decoded into ignored local storage; it does not imply support for every media format. The runtime now activates the strict PCX decodes of `fftitle.pcx`, `engmap1.pcx`, and `richard.pcc`. Their payload structure and dimensions are **Confirmed for the hashed release** and their runtime roles are **Corroborated** by local visual inspection. Users with an older manifest must rerun the installer once to receive these newly supported decoded files.

Runtime media selection is definition-driven. `ImportedArt.Definitions` maps stable presentation roles to an imported kind and original identifier suffix; loading and disposal are generic. Active art roles include the title, character-options, pre-generated-character, screenshot-confirmed `FLUFF.PCX` campaign briefing, load-game, estate/travel, England-map, Home/farm, populated inn and its ten patron portraits, the shared conversation frame, blacksmith workshop/portrait/store, and tournament portraits. `ImportedMovies.Definitions` similarly binds the opening and credits presentations to disc-file suffixes; item preview names come directly from the decoded store table. This distinction matters because archive-entry identifiers use `:name.ext`, while raw disc-file identifiers use `/name.ext`.

`csf-report.txt` validates the structural header, frame-size table, dimensions, scanline commands, and exact payload consumption of byte-stored and kind-1 CSF resources. It records storage kind, literal/transparent/fill segment totals, and stable hashes over decoded indices and alpha masks. The importer labels a CSF as `indexed-animation` only after this parser accepts it, and the runtime catalog can decode it without a graphics device. Display still requires associating each sequence with the correct surrounding-screen palette. For local visual study, pass both `--render-csf=<name>` and `--palette-pcx=<name>`; generated PPM frames stay under the ignored artifacts directory, and the palette association remains an inference until corroborated.

### Executable-guided screen tracing

The owned `CONQUER.EXE` is a Linear Executable embedded behind its DOS stub; this release's LE header begins at file offset `0x290FC`. The first object contains code and the second contains data addressed through `DS`, so references to resource strings are often 32-bit object-relative immediates rather than absolute file offsets. The reproducible workflow is: locate an exact resource string, translate its file position to the data-object offset, search code for that little-endian immediate, then disassemble and follow the surrounding loader call. This is clean-room behavioral evidence; extracted executable bytes and disassemblies remain ignored.

The inspector's `--disassemble=0xADDRESS,...` option maps LE objects/pages and uses the Iced 32-bit x86 decoder to emit a metadata-only local report. It exposed the kind-2 decoder at virtual address `0x45C6E`: MSB-first bit reading, clear/end codes `0x100`/`0x101`, first dictionary code `0x102`, and adaptive widths from 9 through 14 bits. Exact-size decoding of all five entries and strict PCX validation of the four images independently confirm the recovered behavior.

That workflow found a reference to data offset `0x8020` (`TITLE.HAT`) in code at virtual address `0x4FF4F`. The surrounding routine copies the name and calls the common resource loader. The 64-byte `TITLE.HAT` record identifies `FFTITLE.PCX` and a 640x480 presentation. Resource inspection then distinguishes the static title from the first interactive `CHAR_OPS.PCX` screen. `CGOPTS.HAT` and `PREGEN.HAT` now confirm the clickable geometry; exact navigation timing and region-action dispatch still require further executable tracing.

`hat-layout-report.txt` reproducibly decodes every supported HAT descriptor into its resource filename, screen identifier, origin, dimensions, background name, unknown three-byte tag, and ordered region records. Preserving the filename exposed an earlier false association: `FOPTS.HAT`, not `FCASTLE.HAT`, describes the `TACTICAL.PCX` Home office. The inspector's `--xref-data=` option now combines mapped-instruction operand scanning with a bounded parser for the LE fixup page/record tables and writes address-only derived metadata plus nearby relocation context for follow-up disassembly. The parser resolves 17,908 internal fixups in the hashed executable and rejects malformed table bounds, page ranges, source types, objects, and truncated records.

`stored-palette-report.txt` validates the five stored palettes as exact 256-entry RGB tables and records their scene provenance, component range, and hashes. The importer similarly assigns the `palette` kind from content validation rather than filename alone. All five currently belong to `SKIRMISH.RES`; no evidence yet associates them with the GOB-level menu and icon CSFs.

Generated manifests, reports, and artifacts under `analysis/original` are ignored by Git. Do not commit or redistribute them. Only derived facts, implementation code, and short evidence notes belong in the repository.

## Evidence workflow

- Record a discovered fact in `docs/fidelity.md`, stating whether it is confirmed, externally documented, or inferred.
- Put tunable values in a typed definition in `Balance.cs`; do not hard-code them in event or UI branches.
- Add a specification that exercises the definition through the campaign interpreter.
- Keep uncertain values labeled provisional until a table, code path, save-state experiment, or repeated controlled observation confirms them.

The inspector currently establishes that the disc contains a 919,107-byte DOS `CONQUER.EXE`, that the game has separate joust and melee wager dialogue, and that joust opponent resources include Simon, Richard, Gerard, and Gilbert identifiers. It does not by itself prove the exact wager/difficulty table.

Tracked conclusions, evidence offsets, and confidence grades are maintained in [`original-findings.md`](original-findings.md). Generated reports are evidence inputs; that reviewed register is the source of truth for what the project considers factual.

## Windows sandbox and Git ownership

If Codex shell commands fail before PowerShell starts with `setup refresh had errors`, inspect `(Get-Acl -LiteralPath '<workspace>').Owner` and the sandbox log under `%USERPROFILE%\.codex\.sandbox`. A child created as `CodexSandboxOffline` may prevent capability-ACE refresh even when the workspace root is correctly owned.

Repair only the affected subtree when its scope is known: audit it for reparse points, use an elevated PowerShell to run `icacls <path> /setowner <interactive-user> /T`, then `icacls <path> /reset /T` so it inherits the already-correct root ACL. Do not reset the root itself. For this repository, the affected subtree was newly initialized `.git`; after repair it is owned by `Potato\kiber` and normal sandboxed filesystem and read-only Git commands work. The managed permission profile may still deliberately require approval for writes under `.git`; that policy restriction is distinct from broken ownership and should not be removed with ACL edits.

Git may still report dubious ownership because sandboxed commands execute under a different account. Use the narrow per-command form `git -c safe.directory="C:/GOG Games/reimp" ...`; do not add a broad global wildcard exception.
