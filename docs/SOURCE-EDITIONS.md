# Source editions

BLD-GOG-EN is the supported owned edition. Its manifest identifies the executable
by size and XXH3. SRC-PATCH-CATALOG records the public GOG build and Sierra Help
patch catalogs checked on 2026-09-30: version 1.0 is the latest public official
edition found, and no official patch is listed. The owned installation identifies
the same public GOG build. Private distributor branches are not public patches.

The importer accepts an explicit installation path, then `CONQUEROR_SOURCE_PATH`,
then its existing GOG installation default. `GAME_DIR` remains the separate local
evidence root for tests against the original, as [VALIDATION.md](VALIDATION.md) describes.

The owned disc was read again through the bounded inspector. Its image fingerprint
matches BLD-GOG-EN, and the extracted `CD:CONQUER.EXE` is 919,107 bytes with xxh3
`5106f53f8201761cb5112034f6c594d4`, the hash its build manifest gives and the one
the evidence tools check a source against. This verification establishes the prerequisite without asserting byte
identity with unowned historical retail pressings.

`tools/project-config.json` now records the evidence and
`patchStatusEstablished: true`. Before executable analysis run
`tools/Verify-Configuration.ps1 -RequireAnalysisReady`; it passes for this edition.
Revisit patch provenance only if contradictory evidence or a newer official
edition appears.
