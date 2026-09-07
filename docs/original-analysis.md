# Original-data inspection utilities

`tools/Conqueror.Inspect` is the reproducible, read-only bridge from an owned GOG installation to local reverse-engineering evidence. It never changes the installation and never adds proprietary artifacts to source control.

## Run

From the repository root:

```powershell
dotnet run --project tools/Conqueror.Inspect -- "C:\GOG Games\Conqueror AD1086" "analysis\original" wager joust skirmish
```

The first argument is the installed-game directory, the second is an output directory inside this repository, and any remaining arguments are case-insensitive printable-string search terms. With no arguments, the utility uses the paths shown above.

End users should use `Install Original Resources.bat` instead. It runs `tools/Conqueror.Import`, extracts owned SMK/RES/CSF/PCX/PCC/LOW/audio resources into ignored `UserContent`, copies the owned GOB archive, converts Red Book CD audio losslessly to runtime-loadable PCM WAV, and records every installed file in `manifest.json`. MonoGame's runtime stream loader requires PCM RIFF data; compressed Ogg/MP3 support normally goes through the build-time content pipeline, while FLAC would require an additional decoder.

The importer and runtime catalog are format-neutral: assets are addressed by original path and kind, and the game rejects manifest paths that escape `UserContent`. The current runtime plays imported CD audio and can open any installed asset stream. SMK playback and decoding the long CSF/PCC/RES/GOB dialogue and image formats are still adapter work; the presence of a raw imported asset must not be reported as equivalent to displaying or playing it. On the hashed GOG build, an end-to-end import produced 2,464 catalog entries: 2,131 movies, 233 audio entries (including five CDDA WAVs), 99 resource containers, and one GOB archive.

## What it does

1. Parses `game.ins` to find the end of CD data track 1.
2. Reads MODE1/2352 sectors directly, exposing only their 2,048-byte ISO payloads.
3. Walks the ISO-9660 directory tree without mounting or modifying the image.
4. Writes a complete CD manifest and SHA-256 provenance hashes.
5. Locally extracts small executable/configuration artifacts useful for inspection.
6. Searches printable strings in `CONQUER.EXE` and the installed `C1086.GOB`, recording byte offsets for follow-up analysis.

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
