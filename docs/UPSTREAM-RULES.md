# Pinned rules and toolkit packages

`tools/upstream-lock.json` pins the unmodified Standard v1, Methodology, Protocol
and their MIT license by full upstream commit and SHA-256. Their snapshots stay
in `docs/upstream/`, with EOL conversion disabled. Configuration never edits them.
Read that local copy during ordinary work as AGENTS.md requires.

The toolkit checker, executable reader and Python engine are installed packages,
not copied source. Their exact npm versions and distribution integrity are in
`package-lock.json`; Python versions and distribution hashes are in
`requirements-evidence.txt`. `tools/toolkit-packages.json` records the reviewed
upstream commit and release versions. The MIT notice is retained in
`docs/licenses/scientific-method-MIT.txt`.

## Install and verify

Use Node.js 22+, Python 3.12+ and the .NET SDK required by `global.json`:

```sh
npm ci --ignore-scripts
python -m pip install --require-hashes -r requirements-evidence.txt
node tools/upstream.mjs verify
node tools/Verify-ToolkitPackages.mjs
node tools/upstream.mjs docs --check
node tools/upstream.mjs links
```

Rule verification is offline and checks the four snapshot digests. Package
verification checks exact manifest/lock/installed versions, npm integrity fields,
the Python engine requirement and the CI action revision. The CI checker action
is pinned to a commit carrying the installed checker's version. Package
installation verifies distribution hashes; the local checker runs the installed
package and inherits the same checker inputs as CI.

`docs` without `--check` regenerates indexes and parity totals. `links --write`
updates section-line ranges after a rules refresh. The canonical gate runs these
checks. Kaitai compilation still requires the compiler; installing toolkit
packages does not install it. The pre-commit hook checks the staged tree after
restoring its dependency lock from the local npm cache with installation scripts
disabled; run npm ci first to populate that cache.

## Explicit refresh

Only when the owner asks for an upstream update in the current task:

```sh
node tools/upstream.mjs check-upstream
node tools/upstream.mjs refresh --rules <full-40-character-commit>
```

Freshness compares all four pinned rule files with website main. Exit 0 means
unchanged bytes, 2 means changed bytes, and 1 means verification or network
failure. A failed fetch never proves freshness. Refresh downloads everything
before writing, requires the Standard's v1 declaration, replaces files atomically
and writes the lock last. An interrupted update fails digest verification.
Review the rules diff, update affected guidance, regenerate section links and run
the canonical gate. Do not edit the snapshots or promote evidence claims as a
side effect of migration.

Toolkit updates are separate: review upstream's package migration guide and
release versions, update package.json/package-lock.json and the hash-pinned
Python requirements, then update tools/toolkit-packages.json and the CI action
pin together. Remove superseded copies and migrate every active caller. Test the
project wrappers and configuration; package-only tests run upstream before release.

## Configuration-test prerequisites

Configuration tests require a Git checkout, Node.js 22+ and PowerShell. Their
scratch copy includes Git-visible files, skips deleted paths and excludes ignored
dependencies, original content and local outputs. Tests make installed packages
available through a scratch dependency link; project configuration excludes
node_modules. Initialize a ZIP checkout with git init before validation.

## Current migration target

The 2026-10-02 goal targets template 79d18a20cb4d97c7153e74e5695cbb80e7ebf73e
and toolkit 0b4694df7edb621c19171dd79718458378c811a2. Rules main remains
ca39d0750e67c8c3900e8554e66a84083fe67452. The package tooling batch installs
reader 0.2.0, checker 0.1.0 and engine 0.4.0. Shared .NET readers 0.2.0 and the full template delta are adopted and locally
verified. Cross-platform CI, all installer checks and workflow security checks
passed; [VALIDATION.md](VALIDATION.md) records acceptance and
[template-migration-plan.md](template-migration-plan.md) records the completed audit.
