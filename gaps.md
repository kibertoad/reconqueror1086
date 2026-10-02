# Upstream improvement suggestions

## GAP-001: Operational documentation can retain superseded adoption instructions

Recorded: 2026-10-02. Audience: template and shared-tooling authors.

After the package migration, the current implementation plan still named the
previous template/checker/reporter revisions, and the validation guide described
running the checker from `vendor/`. The committed adoption record and active
wrapper used the packaged checker instead. The offline snapshot verifier passed:
it checks immutable rules, rather than these operational claims.

Suggestion: migration acceptance should explicitly reconcile current-use
instructions with the machine-readable adoption record. Prefer linking that
record over repeating revisions in narrative docs. A narrow regression check
could reject obsolete vendored-checker instructions in current-use sections,
while allowing them in dated historical validation records.

Local resolution: corrected the current-use sections of
`docs/implementation-plan.md` and `docs/VALIDATION.md`. Historical migration
records remain dated evidence. No upstream submission has been made.
