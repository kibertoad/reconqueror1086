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
records remain dated evidence.

Upstream submission (2026-10-05): duplicate checks found no matching tracker;
[toolkit issue #233](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/233)
records this suggestion and synthetic acceptance criteria.

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
manifest and explicit Other files list; data-family Survey remains open.

Upstream submission (2026-10-05): duplicate checks found no matching tracker;
[toolkit issue #232](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/232)
records this suggestion and synthetic acceptance criteria.

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
format; research must split incompatible layouts.

Upstream submission (2026-10-05): duplicate searches found no matching tracker;
[toolkit issue #223](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/223)
records the schema suggestion, the original checker-version qualification and
synthetic acceptance criteria. Submitted through the GitHub connector.

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
exceptions, without claiming the shipped wrapper's checks. No implementation change is made by this research batch.

Upstream submission (2026-10-05): duplicate checks found no matching tracker;
[toolkit issue #230](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/230)
records this suggestion and synthetic acceptance criteria.

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
packages, non-exact requirements and transitive version drift.

Upstream submission (2026-10-05): duplicate checks found no matching tracker;
[toolkit issue #229](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/229)
records the downstream case and synthetic acceptance criteria.

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
questions.

Follow-up (2026-10-02): FND-RES-019 extends these fixture cases to a control
byte followed by a stored CRLF, rather than occupying the physical last byte.
Include that distinction and whitespace-bearing duplicate markers in synthetic
reader fixtures. The local complete-file witness preserves both; consumer
interpretation remains Q-RES-158 through Q-RES-163.

Upstream submission (2026-10-05): duplicate checks found no matching tracker;
[toolkit issue #231](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/231)
records this suggestion and synthetic acceptance criteria.

Follow-up (2026-10-05): FND-RES-021 adds unresolved non-ASCII encoding and
heterogeneous line shapes to the fixture request. After checking the existing
issue and its comments, added [issue #231 details](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/231#issuecomment-5985687083)
instead of opening a duplicate. The request preserves high bytes and the other
line class without inferring a code page or grammar from the suffix.

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
label.

Upstream submission (2026-10-05): duplicate checks found no matching tracker;
[toolkit issue #234](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/234)
records this suggestion and synthetic acceptance criteria.

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
file without parsing strings or executing directives.

Upstream submission (2026-10-05): duplicate checks found no matching tracker;
[toolkit issue #231](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/231)
records this suggestion and synthetic acceptance criteria.

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
FMT-RES-001, backed by FND-RES-018. No independent layout is introduced.

Upstream submission (2026-10-05): duplicate checks found no matching tracker;
[toolkit issue #227](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/227)
records the downstream case and synthetic acceptance criteria.

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
replacement by an active entry.

Upstream submission (2026-10-05): duplicate checks found no matching tracker;
[toolkit issue #228](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/228)
records the downstream case and synthetic acceptance criteria.


## GAP-011: Offscreen PrintWindow controls can observe cached frames

Recorded: 2026-10-05. Audience: shared capture-tooling and template authors.

A synthetic window created entirely offscreen repeatedly yielded a uniform
PrintWindow result. Rendering before moving offscreen produced its expected
pixels; another attempt to repaint the offscreen window uniformly returned the
previous colored frame. This is a fixture observation, not an original-game
finding or a universal claim about Windows renderers.

Local handling: initialize and render separate positive and uniform controls
before moving each offscreen. Preserve exact pixel assertions, invalid-window
rejection, uniform rejection and the prohibition on saved rejected frames.
This acceptance check does not establish freshness after offscreen repainting.

Suggestion: test cache/freshness behavior separately and document that successful,
nonuniform PrintWindow output alone does not prove a newly requested repaint.

Upstream submission: duplicate searches for PrintWindow and capture/offscreen
found no matching tracker. [Toolkit issue #235](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/235)
records the synthetic observations and suggested acceptance boundaries.

## GAP-012: Goal claims with push authorization given per task

Recorded: 2026-10-06. Audience: work protocol authors.

The protocol has a claim form for sessions that push to main and one for
sessions that cannot (`goal/<name>` branches). This repository pushes only when
the owner authorizes it in a task, so a goal file can reach main by an authorized
push while a later session without that authorization follows the branch rule.
The page does not say whether such a file on main still claims its areas.

Local handling: `docs/goals/README.md` and the session skills say to use main
when the session is authorized to push and the `goal/` branch otherwise.

Upstream submission (2026-10-06): after reading rules issues #42 and #43,
[rules issue #76](https://github.com/kibertoad/refurbished-dinosaurs/issues/76)
records the per-task case and an acceptance example.

## GAP-013: Engine upgrade notes carry no versions

Recorded: 2026-10-06. Audience: toolkit authors.

The engine has no changelog, and most "Engine upgrades" entries in the toolkit's
migration guide name no version, so the 1.0.1 to 12.0.0 upgrade needed GitHub
release notes matched to headings by hand.

Upstream submission (2026-10-06):
[toolkit issue #318](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/318).

## GAP-014: Checker argument-count skip message

Recorded: 2026-10-06. Audience: checker authors.

Checker 2.2.0 reports an uncountable Parameters section as "is not None. or a
list of parameters", which reads as "is not None", and does not name the item
that stops the count. Three rules here had items naming two parameters.

Local resolution: those items were split into one item per parameter with the
same meaning; the check runs with no skipped steps.

Upstream submission (2026-10-06):
[toolkit issue #319](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/319).

## GAP-015: Template behind the rules and toolkit releases

Recorded: 2026-10-06. Audience: template authors.

Template main pins rules 11dbbc53, checker 2.1.0, reader 1.0.0 and engine
1.0.1, and its skills do not summarize the rules added since, so this adoption
took the releases directly and adapted the guidance locally.

Upstream submission (2026-10-06): after reading template PRs #77 and #78,
[template issue #80](https://github.com/kibertoad/refurbished-dinosaurs-template/issues/80)
lists the remaining parts.

## GAP-016: PE readers refuse a section with a zero raw pointer and a nonzero raw size

Recorded: 2026-10-07. Audience: toolkit authors (executable reader and engine).

AUTOPLAY.EXE, a Watcom-linked PE32, has a `.bss` section with a raw-data
pointer of 0 and a raw size of 0xE00. Executable-reader 2.3.0 `imports` and
every engine 12.0.0 `pe32` report stop with "PE section raw bytes overlap
headers" before running a query, although the loader treats such a section
as having no file bytes.

Local handling: FND-RES-026 names the import slots with its own bounded walk
of the import tables, using FND-RES-024's two slots as positive controls, and
reads the code with a bounded disassembly kept in ignored storage.

Upstream submission (2026-10-07): duplicate searches for PointerToRawData,
overlap headers, bss, uninitialized section, Watcom and PE section found no
match; issue #313 concerns import descriptors only.
[Toolkit issue #324](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/324)
gives a synthetic reproduction and acceptance cases.

## GAP-017: No rule or reporter for naming NE imports

Recorded: 2026-10-07. Audience: Standard and toolkit authors.

STATUS-42 says how to name a PE import by slot; NE executables have no
slots. Each far call to an import is a relocation-chain site, the record
gives a module and an ordinal, and the ordinal's name needs the exporting
module's table, which the restoration does not have.

Local handling: FND-RES-029 walks SETUP's relocation chains with a cap,
names the ordinals from Wine's 16-bit KERNEL export table as an outside
source, and checks each call's arguments against it.

Upstream submission (2026-10-07): duplicate searches for NE import,
ordinal, NE relocation, Win16 and 16-bit Windows on the rules and toolkit
trackers found only rules issue #47, which covers PE.
[Rules issue #81](https://github.com/kibertoad/refurbished-dinosaurs/issues/81)
asks for a rule and an NE mode of the import report, with a synthetic
chain case.

## GAP-018: No location form for a file that ships only inside an archive

Recorded: 2026-10-07. Audience: Standard and checker authors.

A location names a file from the build's file list. `_SETUP.EXE`, the
second-stage installer that reads SIERRA.INF and LANGUAGE.INF, exists only
as a member of `CD:SETUP.SOL`, and the packed-file keys describe one file
packed in place, so its `segment:offset` addresses have no location form.

Local handling: FND-RES-032 locates the member's stored stream in
`CD:SETUP.SOL` with an `offset`, gives the expanded member's size and
XXH3-128, and writes its addresses in the text.

Upstream submission (2026-10-07): duplicate searches for archive member,
member executable, installer archive, nested executable, member location,
extracted file and embedded executable on the rules and toolkit trackers
found rules issue #77 (listing records) and the InstallShield reader issues,
none about locations.
[Rules issue #83](https://github.com/kibertoad/refurbished-dinosaurs/issues/83)
proposes a `members` list on an archive's file item and a `member` key on
locations, with acceptance cases.

## GAP-019: No shared unpacker for packed DOS executables

Recorded: 2026-10-07. Audience: toolkit authors.

The standard's packed-file keys need an unpacker named in `unpacked.tool`,
and the executable reader has none. `CD:INST.EXE`, the DOS installer, is
packed with LZEXE 0.91. (CD:CONFIG.EXE, not INST.EXE, runs INSTALL.DAT:
FND-RES-034.)

Local handling: `tools/evidence/unlzexe.mjs` unpacks LZEXE 0.91 with a
documented header rule and a synthetic test; the build entry names it with
its commit.

Upstream submission (2026-10-07): duplicate searches for LZEXE, unpacker,
PKLITE, packed executable and LZ91 on the toolkit tracker found only #297,
which mentions another project's LZEXE manifest item.
[Toolkit issue #326](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/326)
asks for an `unpack` command with a fixed header rule and acceptance cases.

## GAP-020: Goals stop after one session, and there is no wrap-up procedure

Recorded: 2026-10-07. Audience: template and protocol authors.

The owner runs this goal until the game is restored. Agents under it, here
and on other restorations, stopped after committing a session's handover
because nothing said the end of a session is not the end of the goal. Nor
did anything say what to do when the owner asks to wrap up with work left.

Local handling: none yet. The project syncs with the template once the
change below is reviewed and merged.

Upstream submission (2026-10-07): searches of both trackers for goal,
session, stop and handover found no duplicate.
[Template PR #84](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/84)
adds standing goals, the stop reasons, a wrap-up procedure, skill changes
and a Claude Code Stop hook.
[Rules issue #84](https://github.com/kibertoad/refurbished-dinosaurs/issues/84)
asks for the matching protocol wording.
