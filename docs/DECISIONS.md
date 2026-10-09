# Decisions

## 2026-10-10: Preinstalled game excludes installer recreation

The owner confirmed that the restoration operates on the preinstalled game.
Setup and installation executables are outside further analysis and
reimplementation scope, rather than deferred requirements. Retain existing
evidence and media accounting. Installer coverage gaps and installer-only
questions do not block completion; configuration and resources consumed by
CONQUER.EXE remain in scope. See AGENTS.md and implementation-plan.md.

## 2026-10-09: Standing DOSBox-X instrumentation authorization

The owner authorized future programmatic DOSBox-X installation/builds and
isolated debugger probes for CONQUER.EXE live mapping and RNG recording without
another permission question. [AGENTS.md](../AGENTS.md) records the scope and
preserves run locking, supported-state writes, local-only captures and process
ownership requirements. Execution-environment sandbox approvals remain separate.

## 2026-10-09: Focus mapping on CONQUER.EXE

The owner deferred analysis of every other runtime to Later as non-essential.
Current research covers CONQUER.EXE and its consumed resources. Preserve
auxiliary-runtime evidence, queues and baselines; report focused coverage
separately from full-build scope. See implementation-plan.md for the priority.

## 2026-09-30: Infrastructure migration and planning authorization

The owner approved the scope in [template-migration-plan.md](template-migration-plan.md)
and requested removal of separate explicit plan approval requirements from
Reconqueror and the template. Maintain implementation plans and proceed within
authorized scope. Missing owner decisions, releases, pushes and source-version
evidence requirements remain separate.
