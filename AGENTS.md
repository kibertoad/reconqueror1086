# Repository agent instructions

## Original technical-design documentation

The final reimplementation deliverable includes a documented technical map of
the original game, not only working replacement code. As implementation work
progresses, update the relevant files under `docs/` with:

- original executable/resource addresses, fields, table layouts, state
  transitions, and subsystem relationships discovered during analysis;
- recovered formulas and values, labeled Confirmed, Corroborated, Provisional,
  or Disproved;
- the source of each original-game claim, such as executable control flow,
  decoded resource data, the owned manual, or controlled observation;
- the corresponding reimplementation types, methods, definitions, and tests so
  readers can trace original behavior into the new architecture.

Keep proprietary bytes and generated analysis artifacts out of Git. Commit only
independently authored technical descriptions, compact facts, hashes, and legal
fixtures. A gameplay batch is not complete until its new technical mappings and
evidence have been documented alongside the implementation.

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
