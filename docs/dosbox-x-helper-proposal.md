# Reusable DOSBox-X instrumentation helpers

Published after duplicate checking as
[toolkit issue #403](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/403).
No shared package has been extracted or adopted yet.

Reconqueror has verified structured debugger transport, owned runtime lifecycle,
live mapping and a guarded startup-input/screen-return probe. The commands and
limits are in [RUNTIME.md](RUNTIME.md), with dated checks in
[VALIDATION.md](VALIDATION.md). Full-game recording and rebuild replay remain
unfinished. This proposal exports infrastructure experience, not game evidence.

## Reproduce the runtime

Use the official `joncampbell123/dosbox-x` source at revision
`b6abbd5980a885f5f310a4088c59a8688d1b116c`, tag `dosbox-x-v2026.10.01`,
and its matching Python Agent client. The Windows source build uses x64,
MSVC v142 and Windows SDK 10.0.19041.0; RUNTIME.md gives the MSBuild command.
`Agent Debug SDL2` supports tracing; `Agent Debug No Heavy SDL2` lacks CPU
tracing and memory-change breakpoints. Inspect actual capabilities and reject
unsupported operations. The portable release does not supply this structured
Agent API. Keep source/build/capture directories local and ignored.

The verified Windows launch needs a hidden native console. Redirecting its
console, using `-noconsole`, or using `CREATE_NO_WINDOW` failed debugger entry.
Use a unique endpoint and retain the owned process/session identity and source
revision. Before launching the target, observe a guest-written readiness
marker after mounts and AUTOEXEC setup; RPC readiness alone is insufficient.

## Helpers worth sharing

- Pinned client loading, capability checks, session identity and unique request
  IDs, including for diagnostic clients sharing a session.
- Owned process startup with a hidden native console, explicit guest readiness,
  teardown, and cleanup that retains ownership information when shutdown fails.
- Exclusive machine run locking, private writable drives and read-only media,
  with cleanup restricted to task-owned processes and locks.
- Operation observation that treats timeouts as pending and continues observing
  the same operation. Transport errors must propagate rather than trigger a
  fresh run or duplicate continuation.
- Stopped-state register/memory APIs and writes guarded by expected hashes,
  with caller-supplied supported-field contracts and readback verification.
- Quiet host output through `MIXER MASTER 0:0 /NOSHOW` and MIDI `none`, with
  an explicit sound-investigation opt-in. Preserve emulated sound devices:
  `nosound=true` failed structured readiness in our checks.
- Durable completed-event logging with flush/fsync before continuation, strict
  schema validation, source fingerprints and explicit incomplete outcomes.
- Synthetic lifecycle/transport tests requiring no licensed game or original
  content, plus a separately invoked owner-local verification procedure.

The existing authored candidates are `tools/dosbox_session.py` and the event
logging/source-fingerprint portions of the probe controller. Extract interfaces
only after separating repository paths, source identity, drive setup and the
evidence review contract. Start with shared lifecycle and transport rather than
shipping an allegedly universal gameplay recorder.

## Keep in each restoration

Executable fingerprints, relocation readers, live address mapping, code and
descriptor controls, supported state layouts, input coordinates, screen
boundaries, RNG caller-to-rule ownership, reduction semantics and gameplay
replay adapters stay with the game. A shared helper cannot declare an arbitrary
write supported or assign semantic owners to draws. The Unicorn function
harness remains separate: guest debugger transport does not replace isolated
function emulation.

No proprietary executables, memory images, disassembly, screenshots, resource
files or game recordings accompany this proposal. Public examples should use
synthetic programs and authored fixtures. The current native verification is
a repeatable startup boundary; it does not prove every RNG caller is owned or
that full-game trace completeness has been established.
