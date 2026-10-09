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
| Send game input: keyboard | person | SRC-MANUAL describes keyboard input; Windows accepted a posted Escape message, but game consumption was not verified | Verify keyboard input through an observable game-state transition |
| Send game input: mouse | person | SRC-MANUAL describes mouse input; no mouse message has been tried on its own, so this answer is carried over from the earlier single input answer and names no attempt | Try a mouse message and verify it through an observable game-state transition |
| Read guest memory and control breakpoints | agent | DOSBox-X 2026.10.01 native TCP debugger: synthetic memory roundtrip and breakpoint hit verified; original relocated DOS entry and protected-mode guest memory read verified on 2026-10-09 | Map live LE game addresses before using spec fields or game-function breakpoints |
| Map live guest memory to spec fields | none | The capability probe stops in the protected-mode loader; its selectors are not an audited mapping of the running LE game | Verify code/data selectors, relocation bases and independent positive controls |
| Load a patched save | none | No original-save patch/load workflow is verified; FMT-SAVE-001 through FMT-SAVE-005 retain gaps | Complete the needed structure and verify a reversible patch/load experiment |
| Capture client frames | agent | Direct PrintWindow capture verified against an offscreen synthetic renderer and owned DOSBox client; blank/unsupported results fail | Recheck each different renderer and interpret each frame before using it as evidence |
| Capture sound | person | Bundled DOSBox documentation specifies Ctrl-F6 WAV capture; no unattended audio adapter is verified | Verify a repeatable audio recording case |
| Replay a recording with game draws | none | No original draw recorder or replay fixture workflow exists | Implement RNG instrumentation and a draw-by-draw replay case |
| Call a function in an emulator harness | agent | Unicorn 2.1.4; fingerprinted BLD-GOG-EN LE loader and RULE-RNG-001 function smoke comparisons verified on 2026-10-09 | Add explicit service models only for functions that need them; unsupported accesses stop |

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

There is no unattended gameplay-state probe or full-game RNG recorder yet.
Debugger transport is verified, but the live LE game mapping and rule-tagged
draw capture remain unverified. Research must not assume those capabilities.
An emulated call starts no game process and requires no run lock.

## DOSBox-X debugger installation and probe

The official portable Windows x64 release 2026.10.01 is installed locally at
`artifacts/runtime-tools/dosbox-x-2026.10.01/portable/bin/x64/Release/dosbox-x.exe`.
The archive SHA-256 is
`3c50915e63aefe35777d65ebb4391a06b4a2c2342e4c75cbee6bbda4c6916e33`.
It leaves the owned bundled DOSBox installation intact. The upstream debugger
[control protocol](https://github.com/joncampbell123/dosbox-x/blob/dosbox-x-v2026.10.01/README.debugger)
connects to a loopback listener; no MCP server is required by this Python adapter.

Run `python tools/Probe-DosBoxDebugger.py --emulator <local-exe> --output
artifacts/runtime-tools/probe-synthetic` for the synthetic test. It creates a
synthetic DOS program, takes the exclusive machine run lock, starts its own
hidden emulator, checks memory and breakpoint effects, then stops that process
and releases only its own lock. A genuine hidden native console is required;
`CREATE_NO_WINDOW`, `-noconsole` and redirected console handles failed debugger
entry. Use the non-SDL2 Release executable verified here. Numeric debugger
arguments and an explicit dump filename passed the memory comparison.

With `--original`, set GAME_DIR for that command to the owned installation
containing game.ins. The disc is read-only; C: uses the existing isolated drive
under analysis/documentation-audit/runtime. The probe verifies the original's
relocated DOS entry and a protected-mode memory read, not a gameplay state.
Its current source-identity check also requires the local extracted executable
under analysis/original/disc-root. Dumps and reports stay under ignored artifacts.

## Unicorn function harness

Install the hashed dependency with `python -m pip install --require-hashes -r
tools/emu/requirements.txt` after the main evidence dependencies. The harness
uses Unicorn 2.1.4 and the existing pinned Capstone decoder. It supports only
the fingerprinted BLD-GOG-EN bound LE image, its verified object/page layout and
internal 32-bit offset relocation forms. Unsupported formats, page flags,
relocations, interrupts and hardware/service instructions stop with errors.
There are no import, interrupt, port, timer or video models. Loaded state is
fresh for every call, code is protected, and an instruction budget bounds it.

Run `python tools/emu/test_harness.py` without original files. To smoke-check
RULE-RNG-001's functions, set GAME_DIR for that command to the local original
store containing disc-root/CONQUER.EXE and run `python tools/emu/verify_rng.py
--output artifacts/runtime-tools/unicorn-rng`. Missing GAME_DIR skips. Named
parameters/results are separate from local instruction traces. These smoke
comparisons do not establish branch coverage, complete callers/inputs, or raise
any spec or parity status. A research batch must still record its experiment
and evidence before those results can validate a parity row.
