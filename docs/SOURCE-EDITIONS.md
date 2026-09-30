# Source editions

BLD-GOG-EN is the supported owned edition. Its manifest identifies the executable
by size and XXH3; this migration adds no new original observations or hashes.

The importer accepts an explicit installation path, then `CONQUEROR_SOURCE_PATH`,
then its existing GOG installation default. `GAME_DIR` remains the separate local
evidence root for tests against the original, as [VALIDATION.md](VALIDATION.md) describes.

The latest official patch provenance is not established by the existing build
identity. `tools/project-config.json` preserves `patchStatusEstablished: false`;
neither a build nor a tooling migration establishes that fact. Executable analysis
requires `tools/Verify-Configuration.ps1 -RequireAnalysisReady` and remains blocked
until provenance, version, executable length and SHA-256 are recorded conclusively.
