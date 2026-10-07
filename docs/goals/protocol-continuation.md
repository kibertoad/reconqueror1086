# Protocol continuation

## Condition

Continue the owner's ongoing restoration work according to the local Protocol
and document results using the local Standard. Record suggestions and concerns
for template, process, Protocol, Standard and shared-tooling authors as GitHub
issues after duplicate checks, recording submission links in `gaps.md`.
Commit completed batches and handovers on this goal branch. Since 2026-10-07
the owner has not authorized pushing to main, so the goal runs on
`goal/protocol-continuation`. The owner supplied no finite completion condition
or turn limit, so this goal remains ongoing; completion of one batch does not
complete the objective.

## Scope

Areas: project planning, Survey reconciliation, BLD-GOG-EN build inventory, RES inventory findings, formats and queue planning, and shared research tooling.
Current Survey claim: Q-RES-017 / FMT-RES-015, using the current entry title
and file list rather than a copied filename-family label.
Batches: research and its tooling only. Select concrete Survey research areas
and record their claims here before changing their entries or queue items.
Only this goal runs locally while publication is not authorized.

## Must not touch

Gameplay implementation under `src/`. Proprietary content and generated analysis
artifacts in Git. Immutable rules snapshots. Remote configuration.

## Dead ends

None known.

## Handover

- Stage: Survey. Latest research on this branch: FND-RES-031 (RULE-RES-005
  supported), FND-RES-032 and FND-RES-033 (FMT-RES-119, FMT-RES-120),
  FND-RES-034 (FMT-RES-015 retitled, still unknown). Latest tooling: the
  LZEXE 0.91 unpacker in `tools/evidence/` with its test; BLD-GOG-EN's
  CD:INST.EXE item records its packer and unpacked identity.
- Last checks: documentation check passed at the last batch commit. Fast
  gate passed on 2026-10-07 after installing the hash-pinned evidence
  requirements (the engine had drifted to 8.1.0).
- Unfinished: none. No original program ran. Post-commit audits found no
  repository orphans; reusable MSBuild workers and other agents' processes
  were left running. Nothing is pushed; pushing needs the owner's word.
- Blockers: none known.
- Upstream reporting: GAP-016 (toolkit #324), GAP-017 (rules #81), GAP-018
  (rules #83) and GAP-019 (toolkit #326) are filed and linked in gaps.md.
  Future concerns still need duplicate checks first.
- Next: Q-RES-151 (FMT-RES-011, CONFIG.EXE's script open and the program
  that starts it, its Tried note names where to resume); Q-RES-179 to
  Q-RES-183 (FMT-RES-120 script interpreter and fields); Q-RES-176,
  Q-RES-177, Q-RES-178, Q-RES-172. Q-RES-017 waits for a new lead, as its
  Tried note says; do not repeat the whole-name search.
