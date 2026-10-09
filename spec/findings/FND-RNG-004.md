---
id: FND-RNG-004
title: Live LE relocations and native seed and draw stops identify the loaded RNG state
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006B3EB..0x0006B423
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00024C20..0x00024C43
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00071139..0x0007113B
tool: DOSBox-X 2026.10.01 Agent Debug SDL2, source b6abbd5980a885f5f310a4088c59a8688d1b116c
environment: Windows 11 Pro 10.0.26200, normal CPU core, S3 SVGA, 16 MiB RAM, read-only owned disc, private C drive
---

## Observation

The object layout is the one recorded in FND-RES-009. In a paused live snapshot,
the code object starts at linear address `0x001EB000` and the data object at
`0x00269000`. Their deltas from the declared object bases differ: `0x001DB000`
for code and `0x001D9000` for data. A single delta must not be applied to both.

The control at `0x0006B3F1..0x0006B413` has one exact match in the 16 MiB
snapshot. Every code-object relocation record independently produces the same
target base for its target object: 1,957 target code and 15,214 target data,
including records repeated across page boundaries. Applying those bases to the
source object reproduces all 510,878 code-object bytes except the two bytes at
`0x00071139..0x0007113B`. That word is `0x0188` in the running image. The finding
does not establish every writer to that word. The initialized data comparison
has 904 changed bytes at this later startup stop; those changes are not an audit
of all initialization behavior.

The protected-mode table supplies a present 32-bit code descriptor at selector
`0x0180` and a present 32-bit writable data descriptor at `0x0188`, both with
base zero and limit `0xFFFFFFFF`. Paging is disabled. A native execution stop
at `0180:00246413` identifies the seed entry `0x0006B413`, rather than an
extender instruction. At that stop CS is `0x0180`, DS, ES and SS are `0x0188`,
and the caller's return address is `0180:001FFC32`, corresponding to
`0x00024C32` in FND-RNG-003.

The relocated pointer returned by `0x0006B3EB` is `0x00277044`, identifying
data-object offset `0xE044`, the RNG state in FND-RNG-003. In one startup run
the state was 1, the seed argument was 895800179, and eight native instruction
steps returned to the caller with the state equal to that argument. Another
startup run had argument 895800217, also written in eight steps.

Continuing the second run reached a native stop at `0180:002463F1`, the draw
entry. Twelve instruction steps returned to `0180:001FFC3D`, corresponding to
`0x00024C3D`. State changed from 895800217 to 4122797662 and EAX was 30140.
Those values agree with the recurrence and extraction in FND-RNG-003. The return
site is inside the inclusive helper in FND-RNG-002. No semantic owner or bound
of the outer caller is established by this observation.

## Interpretation

The relocation targets, complete code comparison, native entry stops and actual
state changes are independent controls for the observed code/data mapping.
At these stops DS and SS select the same zero-based descriptor, so the seed
stack argument and RNG state can both be read without inventing a segment alias.
The selectors and bases describe these observed runs, not constants for every
future launch. They must be derived and checked again.

## Alternatives

Applying the code delta to data puts the RNG state elsewhere. The unanimous
data relocation targets and native write distinguish the two object mappings.
The sampled extender selectors do not identify the game code. Native stops at
the relocated seed/draw entries and the changed state distinguish them.
The residual code word is consistent with the observed data selector, but that
agreement alone does not establish its full initialization or mutation history.

## How to reproduce

Use the fingerprinted BLD-GOG-EN executable and object/page layout in
FND-RES-009. The local source build uses the revision above, x64
`Agent Debug SDL2`, MSVC v142 and Windows SDK 10.0.19041.0. Use a hidden native
console, normal CPU core, `svga_s3`, 16 MiB RAM, and a private writable C drive
with the owned GOB, INI and executable. Keep the disc read-only at D:. Change
only MOVIE and CREDITS to OFF in the private INI, as FMT-CONFIG-001 permits.
Observe drive-setup completion before launch. Hold the exclusive machine run
lock throughout, and retain memory and trace captures locally.

`tools/Probe-LiveMapping.py --samples 2 --observation-ms 1000 --rng-break
--trace-loader` at 10,000 cycles produced the compared snapshot. Search the
complete 16 MiB snapshot for the control range above without a result cap;
infer each target base from the loaded 32-bit pointer minus the relocation's
target offset, then compare the complete relocated code object.
`tools/Verify-LiveSnapshot.py` performs that comparison. The physical snapshot
SHA-256 is `9046d81e0e0ba622de519516fe3153855a7e62799b70c816138ef5c12b6fe5d4`.

Use `tools/Probe-LiveMapping.py --samples 12 --observation-ms 1000 --rng-break
--cycles 1000 --seed-check --draw-check` with a fresh private drive to stop at
the seed, read its argument and state, step to its saved return address, then
continue to the first draw and step to its saved return. No guest writes are
needed. Stop if identity is ambiguous, descriptor access fails, the instruction
bound expires, or the state/result comparison fails. The two observation files
have SHA-256 `85c748a3c8ffbe6c834eb26235b2d3310a62552cac308d38c22a0f8c45fb29a5`
and `3939922e1f1080b74e4f7fb25d70ad039f96b7cf40fd24beda5ed2d67fab1860`.
This is not a complete caller/branch reading, a draw distribution, or a full-game
recording, and it promotes no rule's status.
The local original store retains these under
`captures/live-rng-map-2026-10-09/`: `map-m-physical.bin`, `map-m-trace.json`,
`seed-o-observations.json` and `seed-draw-p-observations.json`. None is committed.
