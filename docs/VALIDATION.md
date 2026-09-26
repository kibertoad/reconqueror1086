# Validation

This page describes the checks a change has to pass before it is committed, locally and in CI.

## Local gate

`Run Tests.bat` is the canonical local gate. It runs, in order, and stops at the first failure:

1. `tools/Verify-Repository.ps1`, the repository policy described below.
2. `tools/Check-Documentation.ps1`, the documentation standard check described under
   [Spec checks](#spec-checks).
3. A build of `tests/Conqueror.Tests` into an isolated temporary directory, then the xUnit suite
   in it.
4. A build of `tests/Conqueror.Specs`, then its executable specifications. These are the
   rebuild's own behaviour checks, written as plain assertions in `Program.cs`. They have nothing
   to do with the documentation standard's `spec/` directory.

The builds use `-m:1` and no shared compiler. The gate needs no graphics device and no original
game files: the tests build their inputs from synthetic data.

The gate needs the .NET 10 SDK and Node.js 20 or newer on `PATH`. CI runs the same steps as
separate jobs in `.github/workflows/ci.yml` on every push to `main` and every pull request.

## Repository policy

`tools/Verify-Repository.ps1` applies `tools/repository-policy.json` to tracked files. It rejects
tracked files under `UserContent/` and `analysis/original/`, original-media extensions outside the
approved clean-room and synthetic fixture roots, and tracked files larger than 1 MiB. No original
game bytes, extracted resources or generated analysis reports are committed; `analysis/` holds
local research material only.

## Spec checks

The `Documentation standard` job in `.github/workflows/ci.yml` runs the `check-documentation`
action from
[refurbished-dinosaurs-toolkit](https://github.com/kibertoad/refurbished-dinosaurs-toolkit),
pinned to a full commit SHA, on every push to `main` and every pull request. It checks `spec/`,
`parity/` and `deviations/` against the standard's list of
[checks](https://dinorefurb.com/documentation-standard/#checks), compiles each `.ksy` file with
the Kaitai Struct compiler, checks that every spec and deviation ID cited in `src/`, `tests/`, `tools/`
and `docs/` exists and is not superseded, and fails when `spec/index/` or `PARITY.md` is stale. It
fetches the full history so it can fail a pull request that deletes a spec ID, area or deviation
that exists on `main`. The toolkit's
[setup guide](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/blob/main/docs/documentation-standard-check.md)
lists its inputs.

`tools/Check-Documentation.ps1` runs the same check locally. It reads the toolkit commit from the
workflow, downloads that commit's `tools/check-documentation.mjs` into the ignored `artifacts/`
directory once, and runs it with `--references docs` and `--check`. The check writes `spec/index/`
and `PARITY.md`, and nobody edits them by hand. After changing the spec, `parity/` or
`deviations/`, regenerate them and commit what the check writes:

```powershell
./tools/Check-Documentation.ps1 -Write
git add spec/index PARITY.md
```

The Kaitai Struct compiler is found through the `KSC` environment variable or
`kaitai-struct-compiler` on `PATH`; without it the local run skips compiling the definitions and
prints a warning. The script does not check some items on the standard's list, such as the
fixture schema and the hashes of saves and recordings; its guide lists them, and reviewers check
those by hand. A save-patch write is given as a byte offset and value in the experiment's Setup
section.
