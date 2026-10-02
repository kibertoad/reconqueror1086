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

## GAP-002: Survey inventory must include installation subdirectories and the image path

Recorded: 2026-10-02. Audience: template and shared-tooling authors.

The local disc inventory included ISO files and audio tracks but enumerated only
top-level installation files, excluding the image path. That omitted the DOSBox
wrapper directory from the listing used for Survey reconciliation. Documentation
checks accepted the build's broad Other files description without detecting this
incomplete input listing.

Suggestion: provide a reusable reconciliation gate that checks an explicitly
complete installation/media listing against manifest paths and Other files paths,
and makes recursion, symlink handling and archive traversal depth explicit.
Synthetic coverage should include a nested wrapper file and the source image.

Local resolution: the inspector now enumerates installation subdirectories,
skips reparse points, includes the source image path, and offers an inventory-only
mode. FND-RES-010 now records complete path accounting against the expanded
manifest and explicit Other files list; data-family Survey remains open. No upstream
submission has been made.

## GAP-003: Unknown format entries still require a parsing-model and byte-order choice

Recorded: 2026-10-02. Audience: Standard and shared-checker authors.

The Survey procedure requires unknown format entries for unstudied manifest
files. Standard v1 format metadata requires a text/binary choice and an endian
value for binary formats. Checker 0.1.0's binary branch requires little or big
without an exception for unknown entries; only its Kaitai-definition requirement
has that exception. Thus a truly unstudied file cannot represent both parsing
model and byte order as unresolved in typed metadata.

Suggestion: allow null parsing-model and byte-order metadata only at unknown,
and require concrete values before a layout is supported. This would preserve
explicit uncertainty without forcing a provisional metadata hypothesis.

Local handling: the new unknown inventory entries explicitly mark the required
binary/little values as provisional hypotheses in comments and Open questions.
They establish no field layout or parsing behavior, remain unknown, and have no
implementation. Directory/suffix grouping likewise does not assert a shared
format; research must split incompatible layouts. No upstream submission made.

## GAP-004: Raw-image validation must retain observed padding exceptions

Recorded: 2026-10-02. Audience: shared optical-tooling and template authors.

FND-RES-012 records a complete scan of the owned carrier: the cue-delimited
leading span contains a duplicate header-address run and wholly-zero records
outside the declared ISO volume. Requiring every leading raw record to carry a
Mode 1 prefix or to encode its physical index plus 150 would reject this source.

Suggestion: raw-reader validation fixtures should distinguish physical indexing,
stored address components, declared ISO bounds and cue track boundaries. Include
synthetic duplicate-address and zero-padding cases. Keep strict validation of
actual file extents while stating which padding/header checks are required.

Local resolution: FMT-RES-005 and FMT-RES-116 preserve the observed framing and
exceptions, without claiming the shipped wrapper's checks. No implementation
change or upstream submission is made by this research batch.
