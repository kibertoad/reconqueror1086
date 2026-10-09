# Protocol continuation

## Condition

Update to the latest published Standard, work protocol and shared libraries,
and fully convert existing data and evidence to their requirements. Completion
requires verified upstream provenance, consistent installed dependency locks,
conversion of every affected active caller and evidence entry, updated workflow
guidance, regenerated documentation, and passing applicable validation gates.
Preserve evidence uncertainty and immutable historical findings; conversion
does not authorize promoting claims. Commit completed batches and handovers on
this goal branch. Publishing remains unauthorized.

## Scope

Areas: all existing evidence and tracking, workflow guidance, upstream snapshots,
dependency locks, shared research tooling and runtime-library integrations.
Batches: tooling migration and evidence conversion, kept separate where required.
Only this goal runs locally while publication is not authorized.

## Must not touch

Gameplay rule changes. Proprietary content and generated analysis artifacts in
Git. Manual edits to immutable upstream snapshots. Remote configuration.

## Dead ends

None known.

## Handover

- Active: owner requested latest rules/libraries and full evidence conversion.
- Baseline freshness check on 2026-10-09 identifies rules a9884ae2 and checker
  3.0.1 as newer than the installed pins. Migration acceptance is pending.
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
