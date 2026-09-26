# Repository agent instructions

## Original technical-design documentation

The final reimplementation deliverable includes a documented technical map of
the original game, not only working replacement code. It lives in `spec/` and
follows the [documentation standard](https://dinorefurb.com/documentation-standard/);
`spec/README.md` lists the areas. As implementation work progresses:

- record each observation of the original (executable control flow at an
  address, decoded resource data, the owned manual, a controlled observation)
  as a finding in `spec/findings/`, and the formats, rules, screens and bugs it
  supports in `spec/formats/`, `spec/rules/`, `spec/screens/` and `spec/bugs/`;
- give each entry the status its evidence supports, as the standard's Status
  section defines, and put the uncertain part in its Open questions;
- write addresses as the standard's Notation section requires, and name
  globals, record fields and modes in `spec/glossary/`;
- never name the rebuild's types, methods, files or tests in `spec/`. Trace
  original behaviour into the rebuild through the parity rows in `parity/`,
  spec IDs cited in code comments and tests, and `PLACEHOLDER: <ID>` comments
  wherever the code guesses;
- record every deliberate departure from the original in `deviations/`;
- keep narrative docs in `docs/` about the rebuild and the research process.
  Where they need a claim about the original, they cite its spec ID instead of
  restating it.

Run `tools/Check-Documentation.ps1 -Write` after changing `spec/`, `parity/` or
`deviations/`, and commit the indexes and `PARITY.md` it writes
(`docs/VALIDATION.md` describes the check).

Keep proprietary bytes and generated analysis artifacts out of Git. Commit only
independently authored technical descriptions, compact facts, hashes, and legal
fixtures. Never commit disassembly or decompiler output. A gameplay batch is not
complete until its new findings, rules and parity rows have been written
alongside the implementation.

## Git push destination

The authorized canonical repository is
`https://github.com/kibertoad/reconqueror1086.git`.
Before every push, inspect the repository's configured push destination with
`git remote get-url --push origin` (and `git remote -v` when additional context
is useful), and verify that it resolves to this canonical repository. Push
through the configured remote name and an explicit refspec, for example
`git push origin HEAD:main`.

Never rewrite, replace, or temporarily override a remote URL in order to push.
This prohibition includes `git remote set-url`, changing `remote.*.url` or
`remote.*.pushurl`, and command-scoped configuration such as
`git -c remote.origin.pushurl=...`. If the configured destination is missing or
does not match the repository the user authorized, stop and ask the user to
correct or approve the remote configuration instead of modifying it.

## Post-commit orphan-process audit

After every commit in this repository, inspect running processes for orphaned
work created by this repository's tasks. Check at least PowerShell
(`powershell` and `pwsh`), Ghidra/Java, .NET (`dotnet` and `testhost`), the
Conqueror game, and any other process families that the agent launched while
building, testing, validating, importing, or analyzing this repository.

Reusable MSBuild nodes (`dotnet` running `MSBuild.dll` with `/nodeReuse:true`)
are expected background workers, not orphans. Do not stop or log them merely
because their spawning build process exited, their start time matches a
repository validation, or they remain idle after a build. They are exempt from
cleanup unless there is separate evidence that the process is malfunctioning
and must be stopped to complete this repository's work. Do not disable MSBuild
node reuse in routine build or test commands; keeping these workers available
makes subsequent builds faster.

Multiple reimplementation agents normally work in parallel, but they may work
on other games. A process may be treated as belonging to this work when its
command line, parent, task/session, or source paths show that it targets this
repository or the read-only reference installation/assets at
`C:\GOG Games\Conqueror AD1086`. Do not terminate a process merely because its
executable name matches. Preserve processes for other games, unrelated
user/IDE/system processes, and validation that is intentionally still running.
If repository/source ownership is uncertain, leave the process running.

Stop confirmed orphaned repository processes. Whenever any process is stopped,
append an entry to `orphanCleanupLog.md` containing:

- the local timestamp including UTC offset;
- each stopped PID and process name;
- its start time or task/session association when known;
- why it was identified as orphaned;
- any related process deliberately left running and why.

If the audit finds nothing to stop, no log entry is required.
