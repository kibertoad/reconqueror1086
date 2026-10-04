# Runtime access

## BLD-GOG-EN

The owned edition runs in GOG's bundled DOSBox 0.74, with its documented S3 SVGA
and memory configuration (BLD-GOG-EN). Official-version provenance and the owned
source fingerprint are verified in [SOURCE-EDITIONS.md](SOURCE-EDITIONS.md).

The 2026-09-30 capability assessment used a separate writable DOSBox C: drive
under ignored analysis/documentation-audit/runtime/, containing copies of the
installed data and configuration. The original disc was mounted read-only as
D:. The owner installation was not the writable drive. A bounded launch reached
DOSBox's Program: CONQUER state; direct-window capture recorded that launcher's
client pixels. This proves launch and capture transport, not a repeatable
prescribed gameplay state or a gameplay observation.

Here person means the documented interactive workflow requires a person; none
means no verified adapter in this repository/environment provides the capability.
It describes the current environment, not every possible instrumentation method.

| Capability | Who | Evidence or attempt | What would change it |
|---|---|---|---|
| Start and reach a prescribed gameplay state | person | Owned DOSBox launch works; no state-readable unattended probe exists | Implement and verify a build-specific state probe |
| Send game input | person | SRC-MANUAL describes keyboard/mouse input; Windows accepted a posted Escape message, but game consumption was not verified | Verify input through an observable game-state transition |
| Read game memory and set breakpoints | none | No DOS guest debugger adapter is configured; host addresses are not guest LE addresses | Verify a guest debugger and build-specific address mapping |
| Load a patched save | none | No original-save patch/load workflow is verified; FMT-SAVE-001 through FMT-SAVE-005 retain gaps | Complete the needed structure and verify a reversible patch/load experiment |
| Capture client frames | agent | Direct PrintWindow capture verified against an offscreen synthetic renderer and owned DOSBox client; blank/unsupported results fail | Recheck each different renderer and interpret each frame before using it as evidence |
| Capture sound | person | Bundled DOSBox documentation specifies Ctrl-F6 WAV capture; no unattended audio adapter is verified | Verify a repeatable audio recording case |
| Replay a recording with game draws | none | No original draw recorder or replay fixture workflow exists | Implement RNG instrumentation and a draw-by-draw replay case |
| Call a function in an emulator harness | none | tools/emu/ is absent; the adopted reporter has no LE loader | Verify a fingerprinted LE harness with explicit relocation, interrupt, import and port models |

The bundled DOSBOX/Documentation/dosbox_README.txt also specifies Ctrl-F5 PNG
capture and Ctrl-Alt-F5 AVI capture. Those recordings do not supply game RNG draws
or prove deterministic original/rebuild replay.

## Capture and run discipline

The old desktop-copy helper could sample an occluding application. Its invalid
assessment capture was discarded. [Capture-OriginalWindow.ps1](../tools/Capture-OriginalWindow.ps1)
now captures only the selected window's client through PrintWindow, with no
desktop fallback. A uniform frame is rejected because renderer support and state
cannot be established from it. Unsupported renderers need a verified emulator
capture path or a person; do not substitute desktop sampling. Its
`checkpoint.json` (schema version 3) names each frame by the `xxh3` the spec cites
it by, computed after the burst by `tools/evidence/xxh3.mjs`, so the script checks
for Node.js and the installed reader package before it captures anything.

Original runs take an exclusive machine lock at
C:\ProgramData\refurbished-dinosaurs\run.lock on Windows or
/var/tmp/refurbished-dinosaurs/run.lock elsewhere, overridden by
REFURBISHED_DINOSAURS_RUN_LOCK. The assessment took this lock and released it
when its owned DOSBox process ended. [orphanCleanupLog.md](../orphanCleanupLog.md)
records forced cleanup. Captures, configurations and probe logs remain local.

There is no gameplay probe, RNG recorder or verified emulator harness yet.
Research must not plan on those capabilities. An emulated call starts no game
process and requires no run lock once its loader and models are verified.
