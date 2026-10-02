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

## GAP-005: Installed engine validation duplicated a mutable version pin

Recorded: 2026-10-02. Audience: template and shared-tooling authors.

The canonical gate asserted a literal engine version separately from the adoption
record and requirements lock. An authorized package update therefore failed even
when installation matched both updated records. Transitive pinned distributions
were not all checked by the assertion.

Suggestion: derive installed-version checks from the exact requirements lock,
cross-check adoption metadata, and cover changed pins and transitive drift with
synthetic distributions.

Local resolution: Verify-EvidenceEnvironment.py checks every locked distribution
and the adoption engine version. Synthetic regressions cover future pins, missing
packages, non-exact requirements and transitive version drift. No upstream
submission has been made.

## GAP-006: Survey text fixtures should preserve distinct end-of-file cases

Recorded: 2026-10-02. Audience: shared-tooling and template authors.

FND-RES-014 supplies direct file-data evidence for empty content and multiple
text-ending cases in one provisional inventory family. A text classifier or
round-trip witness that normalizes endings before reporting cannot distinguish
those cases. This is a fixture suggestion, not a reported defect in an adopted
reader or a claim about an interpreter's acceptance policy.

Suggestion: reusable Survey fixtures should include empty files, a blank final
line with CRLF, an unterminated final printable line and a separate terminal
control byte. Compare complete reconstruction and fingerprints, and preserve
consumer behavior as a separate research question.

Local handling: FMT-RES-008 distinguishes the stored cases and the local witness
reconstructed each complete file. Q-RES-132 through Q-RES-134 retain consumer
questions. No upstream submission has been made.

## GAP-007: Valid queue IDs can carry stale human-readable area labels

Recorded: 2026-10-02. Audience: template and shared-documentation-tooling authors.

The plan and goal handover described Q-RES-011 / FMT-RES-009 as configuration
candidates, while the authoritative entry named the bitmap candidate. The clean
start-session documentation checks passed: the references existed, but their
human-readable labels were wrong. This did not invalidate file evidence, but
could misdirect the next research session.

Suggestion: derive displayed item/entry labels from authoritative metadata when
possible, or add a focused handover review that compares the named ID's title
with its prose label. An ID resolving is distinct from its description agreeing.
Avoid a general semantic checker that would reject legitimate paraphrases.

Local resolution: the plan and active goal label are corrected after inspecting
the current entry. Research used FMT-RES-009's file list rather than the stale
label. No upstream submission has been made.

## GAP-008: Text Survey must not validate strings using raw quote parity

Recorded: 2026-10-02. Audience: shared-tooling and template authors.

FND-RES-017 records an odd raw quote count alongside multiple line-prefix
classes. This is not evidence of a malformed string: whether comment-shaped
regions and other line forms affect lexing is unresolved. A raw quote counter
or prefix-only classifier cannot establish the interpreter's grammar.

Suggestion: reusable text-Survey fixtures should include quotes in comment-shaped
regions, case variants, non-directive lines, adjacent backslashes and a final
unterminated region. Preserve all bytes and report unknown lexical state rather
than repairing quote balance, stripping lines or normalizing casing. This is a
fixture suggestion, not a reported defect in an adopted reader.

Local handling: FMT-RES-011 retains stored framing only and Q-RES-151 through
Q-RES-156 keep interpreter questions open. The complete witness reconstructs the
file without parsing strings or executing directives. No upstream submission made.

## GAP-009: Superseded unknown inventory entries still require a layout table

Recorded: 2026-10-02. Audience: Standard and shared-checker authors.

Superseding the unknown duplicate FMT-RES-012 with its evidence-backed active
format caused the checker to reject the former unknown entry for having no
layout table. Its unknown listing never had a table; the complete layout already
lives in the replacement entry. Requiring a new layout table on retirement can
encourage redundant or invented descriptions.

Suggestion: exempt superseded unknown inventory listings from the table
requirement when they identify their active replacement and cite the reconciliation
evidence. Preserve the old uncertainty rather than forcing a second layout.

Local handling: a single whole-file table redirects the former listing to
FMT-RES-001, backed by FND-RES-018. No independent layout is introduced and no
upstream submission has been made.

## GAP-010: Pre-commit omitted active narrative-reference validation

Recorded: 2026-10-02. Audience: shared-tooling and template authors.

The end-session handover named a superseded entry and passed the pre-commit
checks. The next clean start-session check rejected it. The shared Node check
list included spec, queue and package checks, but not the repository's active
narrative checker; the full documentation command ran that additional check.

Suggestion: include every active documentation-reference invariant in the shared
staged-tree check list, not only tests of the checker. Exercise an end-session
handover that names a superseded entry and verify the commit gate rejects it.

Local resolution: remove the retired citation from the current handover and add
Check-NarrativeReferences.mjs to the shared checks used by pre-commit and the
canonical gate. A synthetic handover regression proves rejection and successful
replacement by an active entry. No upstream submission has been made.
