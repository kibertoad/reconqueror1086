# Pinned rules and toolkit packages

`tools/upstream-lock.json` pins the unmodified Standard v1, Methodology, Protocol
and their MIT license by full upstream commit and SHA-256. Their snapshots stay
in `docs/upstream/`, with EOL conversion disabled. Configuration never edits them.
Read that local copy during ordinary work as AGENTS.md requires.

The toolkit checker, executable reader and Python engine are installed packages,
not copied source. Their exact npm versions are in `package.json` and their
distribution integrity in `pnpm-lock.yaml`; Python versions and distribution
hashes are in `requirements-evidence.txt`. CI runs the toolkit's
`check-documentation` action at a full commit SHA, which runs the checker source
at that commit. The lock's `checker` entry records that commit and the
`@scientific-method/standard-checker` version it carries, and `package.json` pins
exactly that version, so the offline run and CI apply the same rules.
`tools/toolkit-packages.json` records the reviewed toolkit commit and release
versions. The MIT notice is retained in `docs/licenses/scientific-method-MIT.txt`.

## Install and verify

Use Node.js 22+ with pnpm, Python 3.12+ and the .NET SDK required by `global.json`:

```sh
pnpm install --frozen-lockfile
python -m pip install --require-hashes -r requirements-evidence.txt
node tools/upstream.mjs verify
node tools/Verify-ToolkitPackages.mjs
node tools/upstream.mjs docs --check
node tools/upstream.mjs links
```

`verify` is offline: it checks the four snapshot digests, and that the CI action
pin, the lock's `checker` entry and `package.json` agree. Package verification
checks exact manifest, `pnpm-lock.yaml` and installed versions, the lock's sha512
integrity fields, the Python engine requirement, and that
`tools/toolkit-packages.json` names the checker commit and version the lock pins.
The documentation runner refuses an installed checker of another version.
Package installation verifies distribution hashes; the local checker runs the
installed package and inherits the same checker inputs as CI.

`pnpm-workspace.yaml` exempts the pinned checker and reader releases from pnpm's
minimum release age, so a release adopted as soon as it is tagged still installs.

`docs` without `--check` regenerates indexes and parity totals. `links --write`
updates section-line ranges after a rules refresh. The canonical gate runs these
checks. Kaitai compilation still requires the compiler; installing toolkit
packages does not install it. The pre-commit hook checks the staged tree after
installing its dependency lock from the local pnpm store, offline and with
installation scripts disabled; run `pnpm install` first to fill that store.

## Explicit refresh

Only when the owner asks for an upstream update in the current task:

```sh
node tools/upstream.mjs check-upstream
node tools/upstream.mjs refresh --rules <full-40-character-commit> --toolkit <full-40-character-commit>
```

`check-upstream` compares the four pinned rule files with the rules repository's
main commit, and the pinned checker version with the one on the toolkit's main
branch. Exit 0 means unchanged, 2 means changed, and 1 means verification or
network failure. A failed fetch never proves freshness.

Refresh downloads everything, and the checker version the toolkit commit
carries, before writing. The toolkit commit must be the one the toolkit tagged
`@scientific-method/standard-checker@<version>`: a later commit can carry the
same version with unreleased checker changes, which CI would run and the
published package would not. Refresh requires the Standard's v1 declaration,
updates the CI checker pin, the `package.json` pin and the checker's exemption in
`pnpm-workspace.yaml`, replaces files atomically and writes the lock last. An
interrupted update fails digest verification. Run `pnpm install` to update
`pnpm-lock.yaml`, set `tools/toolkit-packages.json` to the same checker commit
and version, review the rules diff, update affected guidance, regenerate section
links and run the canonical gate. Do not edit the snapshots, patch the installed
checker or promote evidence claims as a side effect of migration.

Reader and engine updates follow the toolkit's package migration guide and
release versions: update `package.json`, `pnpm-lock.yaml` and the hash-pinned
Python requirements together with `tools/toolkit-packages.json`. Remove
superseded copies and migrate every active caller. Test the project wrappers and
configuration; package-only tests run upstream before release.

## Configuration-test prerequisites

Configuration tests require a Git checkout, Node.js 22+ and PowerShell. Their
scratch copy includes Git-visible files, skips deleted paths and excludes ignored
dependencies, original content and local outputs. Tests make installed packages
available through a scratch dependency link; project configuration excludes
node_modules. Initialize a ZIP checkout with git init before validation.

## Current migration target

The template adoption is 25c5808bb497d863aded815e50838ee1e6d94256. Rules main is
efa138ba212260b23bb4eb599e61336a7126c973, still Standard v1, the same rules the
template pins, together with checker 2.2.0, reader 2.3.0 and engine 12.0.0. The checker commit is in `tools/upstream-lock.json`;
package versions and the toolkit revision are in `tools/toolkit-packages.json`.
Python integrity pins are in `requirements-evidence.txt`, and the validation gate
checks every installed locked distribution through
`tools/Verify-EvidenceEnvironment.py`. [VALIDATION.md](VALIDATION.md) records
local verification, and [template-migration-plan.md](template-migration-plan.md)
records each adoption's scope and acceptance.
