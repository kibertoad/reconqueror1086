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

- Stopped: owner asked to wrap up.
- Stage: Survey. Every manual screen has a screen entry (SCR-UI-021 to
  SCR-UI-036, `sourced`), and every archive member kind has a format entry
  (FMT-RES-121 to FMT-RES-127; FND-RES-061 covers the unread `.FNT` entry).
  Template main 25c5808b is adopted, with the RefurbishedDinosaurs 10.0.0
  packages.
- Last checks: documentation check passed at the last batch commit. The fast
  gate passed on 2026-10-07 at the template adoption; the later batch changed
  no code or tests.
- Unfinished: none. No original program ran.
- Blockers: none known.
- Upstream reporting: GAP-016 to GAP-021 are filed and linked in gaps.md;
  GAP-021 is toolkit #328. Future concerns still need duplicate checks first.
- Next: Q-RES-212 to Q-RES-214 (POV, SVG and SFG readers); Q-RES-209
  (FMT-RES-122 sections); Q-UI-028 to Q-UI-043 (screen code for the sourced
  screens); Q-RES-162, Q-RES-163, Q-RES-206, Q-RES-207 (FMT-RES-013);
  Q-RES-180 to Q-RES-182 and Q-RES-185 to Q-RES-191 (FMT-RES-120).
