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
Current Survey claims: Q-RES-017 / FMT-RES-015, using the current entry title
and file list rather than a copied filename-family label; and the Survey screen
reconciliation, which compares the screens SRC-MANUAL mentions with the SCR
entries and adds `unknown` screen entries (areas UI and SAVE) for any missing.
Batches: research and its tooling only. Select concrete Survey research areas
and record their claims here before changing their entries or queue items.
Only this goal runs locally while publication is not authorized.

## Must not touch

Gameplay implementation under `src/`. Proprietary content and generated analysis
artifacts in Git. Immutable rules snapshots. Remote configuration.

## Dead ends

None known.

## Handover

- Stage: Survey. Latest research on this branch: the INSTALL.SCR language and
  commands (FND-RES-055 and FND-RES-047 to FND-RES-052), INST.EXE's text dictionary and its lookups (FND-RES-053,
  FND-RES-056, FMT-RES-013), and the moves of the disc's `DEMOS/`, `INN/`,
  `VESA/`, readme, version and placeholder files to Other files (FND-RES-057,
  FND-RES-058). Latest tooling: MZ function inventories for CD:CONFIG.EXE and
  the unpacked CD:INST.EXE, verified by Check-Coverage.
- Last checks: documentation check passed at the last batch commit; coverage
  check and evidence tests passed at the tooling commit. Fast gate last
  passed on 2026-10-07 before these batches; they changed no code or tests.
- Unfinished: the screen reconciliation claimed above has started; a manual
  read is under way and nothing is written yet. No original program ran.
  Ghidra ran headless twice and left no process. Nothing is pushed; pushing
  needs the owner's word.
- Blockers: none known.
- Upstream reporting: GAP-016 to GAP-021 are filed and linked in gaps.md;
  GAP-021 is toolkit #328. Future concerns still need duplicate checks first.
- Next: finish the screen reconciliation; then Q-RES-162, Q-RES-163,
  Q-RES-206, Q-RES-207 (FMT-RES-013); Q-RES-199, Q-RES-202, Q-RES-203
  (FMT-RES-016); Q-RES-180 to Q-RES-182 and Q-RES-185 to Q-RES-191
  (FMT-RES-120); Q-RES-176 to Q-RES-178. Q-RES-017 waits for a new lead, as its Tried note says.
