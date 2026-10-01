# Documentation migration audit

The migration gaps are closed. This is a record of documentation and tooling
adoption, not a claim that the original game has been completely researched or
that every parity row is validated. The plan remains at Survey.

| Audit requirement | Result and authoritative record |
|---|---|
| Current rules and procedures | `tools/upstream-lock.json` pins reviewed Standard/protocol/checker commits; offline digest and section-link checks pass. Workflow skills and the plan use those procedures. |
| Actionable research tracking | Each area in `spec/README.md` has a queue. Active Open questions name stable items, which name their entries and the evidence that would settle them. `Check-ResearchTracking.mjs` verifies these links and allocator consistency. |
| Function inventory for studied code | `coverage/BLD-GOG-EN/@CD/CONQUER.EXE.tsv` contains compact address/body-size metadata from a fingerprinted LE-mapped Ghidra project. `coverage/README.md` records provenance and analysis limits; the outer MZ stub is excluded. `Check-Coverage.mjs` verifies identity, destination, mapped starts and metadata. |
| Official-version provenance | SRC-PATCH-CATALOG ties the public distributor build to the owned installation. SOURCE-EDITIONS records the executable fingerprint; `Verify-Configuration.ps1 -RequireAnalysisReady` passes. |
| Runtime access | RUNTIME records the bounded DOSBox assessment, verified client capture and the capabilities that still require a person or a new adapter. It does not treat queued input or a launcher frame as gameplay evidence. |
| Working documentation links | EVIDENCE-TOOLS exists; project narrative, workflow, queue and coverage file links resolve. Standard section links pass the pinned link checker. |
| Validation and legal boundary | The canonical fast gate passes with Kaitai compilation, synthetic reporter/tooling regressions, the solution build, xUnit and executable specifications. No gameplay code, evidence statuses or proprietary bytes were added by the migration. |

Shared defects found by the audit are now merged upstream:

- [Standard PR 28](https://github.com/kibertoad/refurbished-dinosaurs/pull/28)
  and [toolkit PR 20](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/pull/20)
  distinguish physical executable file-data locations from loaded code addresses.
  The merged revisions are pinned locally; see [UPSTREAM-RULES.md](UPSTREAM-RULES.md).
- [Template PR 35](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/35)
  captures the selected client and rejects blank or unsupported results without
  sampling an occluding application's desktop pixels.
- [Template PR 36](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/36)
  adds research-queue checks to the canonical gate and empty scaffold area queues.

Remaining research is explicit in the queues and plan: full media/manual Survey
reconciliation, complete readings, a verified game-state/input probe, an LE
emulator harness, and evidence-dependent parity work. Those are future research
and implementation tasks; the migration has supplied their tracking and honest
capability boundaries.
