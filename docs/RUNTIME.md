# Runtime access

## BLD-GOG-EN

The existing owned-edition launch configuration is documented in the build entry.
This infrastructure migration does not reassess interactive runtime access, memory
instrumentation or original captures. Latest official patch provenance remains
unestablished, so executable analysis and new original runs are blocked pending
the documented version gate.

| Capability | Who | Tried | What would change it |
|---|---|---|---|
| Start it and reach a state without a person | none | Not assessed in migration | Establish patch provenance, then evaluate runtime access |
| Send it input | none | Not assessed in migration | Evaluate the supported runtime |
| Read memory and set breakpoints | none | Not assessed in migration | Evaluate instrumentation |
| Load a patched save | none | Not assessed in migration | Establish a supported save workflow |
| Capture frames and sound | none | Not assessed in migration | Evaluate capture tooling |
| Play back a recording | none | Not assessed in migration | Establish recorded-run support |
| Call a function in an emulator harness | none | No game-specific LE harness established | Build and verify an appropriate harness |

Original runs use an exclusive machine run lock at
`C:\ProgramData\refurbished-dinosaurs\run.lock` on Windows or
`/var/tmp/refurbished-dinosaurs/run.lock` elsewhere, overridden by
`REFURBISHED_DINOSAURS_RUN_LOCK`. No run lock was acquired by this migration.
Probe: none established. Emulated calls require no run lock once a supported
harness and the source-version gate are established.
