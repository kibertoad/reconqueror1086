# Ghidra setup for Codex analysis

This machine has a portable, user-wide Ghidra installation for clean-room analysis of the legally owned *Conqueror: A.D. 1086* executable. It does not require administrator access and must not place Ghidra projects, disassembly dumps, or proprietary binaries in Git.

## Installed tools

| Tool | Version | Location |
| --- | --- | --- |
| Ghidra | 12.1.3 | `C:\Users\kiber\AppData\Local\Programs\Ghidra\ghidra_12.1.3_PUBLIC` |
| Eclipse Temurin JDK | 21.0.12.1 | `C:\Users\kiber\AppData\Local\Programs\Java\jdk-21.0.12.1+1` |

The Ghidra archive was downloaded from the official NSA release and verified before extraction. Its SHA-256 is `93A5D11A9AD510622ACAAF908C556A7B9B764D338E78A7567F3689BF5081FD54` and its size is 569,445,154 bytes. Use the [official Ghidra releases page](https://github.com/NationalSecurityAgency/ghidra/releases) if the installation must be recreated; Ghidra 12.1.x requires a supported JDK 21 installation.

## Check before installing

A new Codex process must run this read-only preflight before attempting any download or installation:

```powershell
$knownGhidraHome = 'C:\Users\kiber\AppData\Local\Programs\Ghidra\ghidra_12.1.3_PUBLIC'
$knownJavaHome = 'C:\Users\kiber\AppData\Local\Programs\Java\jdk-21.0.12.1+1'
$ghidraLauncher = Join-Path $knownGhidraHome 'support\analyzeHeadless.bat'
$javaLauncher = Join-Path $knownJavaHome 'bin\java.exe'

$ghidraReady = Test-Path -LiteralPath $ghidraLauncher -PathType Leaf
$javaReady = Test-Path -LiteralPath $javaLauncher -PathType Leaf
Write-Output "Ghidra installed: $ghidraReady"
Write-Output "JDK installed: $javaReady"
```

When both results are `True`, Ghidra is already installed: reuse these directories, set the process environment as shown below, and skip all download, extraction, package-manager, and environment-persistence steps. If only one result is `False`, install only the missing component. Do not reinstall merely because an isolated Codex process cannot see the user's persisted environment variables.

The user environment contains `GHIDRA_HOME`, `JAVA_HOME`, and corresponding `PATH` entries. A Codex process may run under an isolated Windows identity and therefore may not inherit those user variables. Set the process environment explicitly before invoking Ghidra:

```powershell
$ghidraHome = 'C:\Users\kiber\AppData\Local\Programs\Ghidra\ghidra_12.1.3_PUBLIC'
$javaHome = 'C:\Users\kiber\AppData\Local\Programs\Java\jdk-21.0.12.1+1'
$env:GHIDRA_HOME = $ghidraHome
$env:JAVA_HOME = $javaHome
$env:Path = "$javaHome\bin;$ghidraHome;$ghidraHome\support;$env:Path"
```

Verify both installations without creating a project:

```powershell
& "$env:JAVA_HOME\bin\java.exe" -version
& "$env:GHIDRA_HOME\support\analyzeHeadless.bat"
```

The second command prints headless-analyzer usage and exits with code 1 when no project arguments are supplied; that is the expected smoke-test result.

## Original executable

The owned reference executable is generated locally by the inspector at:

```text
C:\GOG Games\reimp\analysis\original\artifacts\CONQUER.EXE
```

Reference facts:

- source installation: `C:\GOG Games\Conqueror AD1086`
- byte length: 919,107
- SHA-256: `5D7231758766204AD061E6B82CF2F0E0CBE28899B35D095F13E4AAD75C8B79D6`
- format: 32-bit Linear Executable embedded behind a DOS16M MZ stub
- embedded MZ module file offset: `0x26654`
- LE header offset within that module: `0x2AA8`
- LE header file offset for this build: `0x290FC`
- LE enumerated-data-page offset: module-relative `0x25C00`, hence raw file offset `0x4C254`

If the artifact is absent, regenerate it without modifying the original installation:

```powershell
$env:DOTNET_CLI_HOME = 'C:\GOG Games\reimp\.dotnet-home'
$artifactPath = Join-Path $env:TEMP ('reconqueror-inspect-' + [guid]::NewGuid().ToString('N'))
dotnet run --project tools\Conqueror.Inspect --artifacts-path $artifactPath -- `
  'C:\GOG Games\Conqueror AD1086' 'analysis\original'
```

Never add `analysis/original/artifacts`, a Ghidra project, exported bytes, or full disassembly output to Git.

## Headless-project workflow

Always create analysis projects under a unique temporary directory:

```powershell
$projectRoot = Join-Path $env:TEMP ('reconqueror-ghidra-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $projectRoot | Out-Null
& "$env:GHIDRA_HOME\support\analyzeHeadless.bat" `
  $projectRoot ConquerorAnalysis `
  -import 'C:\GOG Games\reimp\analysis\original\artifacts\CONQUER.EXE' `
  -overwrite
```

Ghidra's automatic importer sees the outer DOS MZ executable and does not automatically map the embedded LE objects correctly. Do not treat that default disassembly as authoritative. For reproducible address-level work, use the repository inspector's LE mapper and Iced decoder:

```powershell
$env:DOTNET_CLI_HOME = 'C:\GOG Games\reimp\.dotnet-home'
$env:NUGET_PACKAGES = 'C:\Users\kiber\.nuget\packages'
$artifactPath = Join-Path $env:TEMP ('reconqueror-disassembly-' + [guid]::NewGuid().ToString('N'))
dotnet run --project tools\Conqueror.Inspect --artifacts-path $artifactPath -- `
  'C:\GOG Games\Conqueror AD1086' 'analysis\original' `
  '--disassemble=0x45C6E,0x456F4,0x4576C' '--executable-only'
```

This writes the ignored metadata-only `analysis/original/executable-disassembly-report.txt`. Addresses are LE virtual addresses, not raw file offsets. The inspector locates the nested MZ module that owns the LE header, applies module-relative LE page offsets, and then maps virtual addresses through the executable's object and page tables before decoding 32-bit x86 instructions. For this build, adding the page offset to the LE header would be wrong by `0x2AA8`; the enumerated pages begin at raw file offset `0x4C254`, not `0x4ECFC`.

For data references that are absent from raw instruction operands, `--xref-data=` also decodes the executable's fixup page and record tables according to the IBM LE/LX field definitions and Open Watcom's `exeflat.h` constants. Pass comma-separated target-object offsets; the ignored report includes matching internal relocations and a small source-address neighborhood without exporting original bytes:

```powershell
dotnet run --project tools\Conqueror.Inspect -- `
  'C:\GOG Games\Conqueror AD1086' 'analysis\original' `
  '--xref-data=0x17C0,0x17CC,0x1810' '--executable-only'
```

`--executable-only` returns after artifact extraction and requested executable string, disassembly, or relocation reports. Use it for iterative static analysis so unrelated GOB, scene-container, and movie population scans are not repeated.

`--scene-blocks` writes the ignored `scene-block-report.txt`. Its actor rows distinguish the visual-state effect selector at block `+0x2E` from the movement selector at `+0x46`; expose initial signed 8.8 block offsets `+0x24/+0x28`; and include the selected movement descriptor's tick count, interval, flags, coordinate/block/surface/heading deltas, loop block, and terminal surface. Use this census to corroborate handler traces against every owned scene variant; never commit the generated report.

For mode-12 routing, disassemble `0x445C4`, `0x4FF53`, and `0x530D0`. The first is the piecewise integer heading helper, the second applies cardinal quantization and rewrites descriptor flags `0x142` to live flags `0x112`, and the scheduler span through `0x53D5F` contains the strict movement bands, behavior-bit-2 neighbor test, flag-`0x10` early effect termination, and return to actor thinking. Compare `0x4FFD1`-`0x4FFDA` with `0x4FE27`-`0x4FE30`: the latter installs `0x40` for modes 5/6/17 and reaches the left-turn branch, while mode 12 does not. Corroborate the blocker predicate by grouping `Behavior & 2` and `Placed` from `scene-block-report.txt`, then join actor `Field40` state targets back to rows in the same archive to identify the underlying block restored after movement. Generated disassembly and census rows remain ignored owner-local evidence.

For pointer-contact projection, begin at viewer initialization `0x5421F`-`0x542E4`
and pointer dispatcher `0x55524`, then decode all of `0x470A8` and traversal
`0x45158` rather than labeling the former a generic floor-plane routine. Record
the initialized elevation `0x80`, integer horizon `viewportHeight / 2`, and
viewport-width inputs. `0x470A8` constructs forward basis `0x4000` and lateral
basis `((0x400000 / viewportWidth) * (pointerX - viewportWidth / 2)) >> 8`
before rotation through `0x447C4`. `0x45158` normalizes the signed major axis to
`0x100`, derives the minor increment by signed division, performs original map
lookup with `& 0x7F`, and permits only `0x40` iterations. Intersection switch
table `0x34A0C` maps kind 0 to no candidate, kinds 1/4 to an inclusive cell
box, kinds 2/3 to horizontal/vertical center planes, and kinds 5/6 to the two
diagonals. Helper `0x44F7C` stores contact-x, contact-y, packed-face, and
block-pointer entries at globals `1C220`, `1C1A0`, `1C2A0`, and `1C320`.
Those arrays have 32 slots, but `0x45061` stops when the post-increment count
reaches `0x1F`, so only 31 candidates are usable. Traversal branches
`0x4527D`-`0x4566D` probe a changed corner as `(newX, oldY)`, `(oldX, newY)`,
then `(newX, newY)`; a single-axis change probes the entered cell followed by
its two perpendicular neighbors. Candidate behavior bit 0 controls whether
the walk stops or continues. Eligible kind-4/behavior-bit-3 blocks follow
their `+0x40` state target in place while that target has nonzero kind.
`0x470A8` selects surfaces `+0x2C..+0x38`, flips north/east texture
coordinates, projects `top = horizon - (upper - cameraElevation) *
viewportWidth / depth` and `bottom = horizon + (cameraElevation - lower) *
viewportWidth / depth` from block `+0x20/+0x1C`, and calls alpha sampler
`0x444E8`; raw palette index zero is transparent. Primary flag-0 dispatch at
`0x555AB` gives selected friendlies mode 12 at the contact cell before
actor/object-specific actions. These mappings Disprove the former empty-floor
plane. `OriginalSiegeProjection` implements the fixed step, wrapped source-map
lookup, exact probe order, pass/stop behavior, state-target chaining, 31-entry
sentinel, center/diagonal shapes, and insertion-ordered alpha candidates for
pointer picking. For kind 4, `0x47308`-`0x4735A` forms the live center as
`cell * 0x100 + 0x80 + offset`, subtracts the viewer, and rotates by negative
view heading through `0x447C4`. `0x4737B`-`0x473C3` uses the rotated forward
coordinate as depth and computes horizontal coordinate
`((rayLateral * depth) >> 14) - centerLateral + 0x80`, rejecting values outside
`0..0xFF` and shifting by `8 - block.TextureWidthShift`. `0x473CE`-`0x47419`
selects sector `(((block.Surface3 - viewHeading + 0x80 / divisions) & 0xFF) *
divisions) >> 8`; behavior bit 2 (`0x04`) folds the far half and mirrors the
texture coordinate. Pointer picking implements this transform, vertical bounds,
alpha, and live actor/object identity in candidate insertion order. General
Actor acquisition now uses that same rotation/table path. Routine `0x4F98D`
walks 68-byte actor records in authored order, skips inactive, self, and
same-side records, subtracts live fixed positions at `+0x0C/+0x10`, derives a
byte heading with `0x445C4`, and casts from the source through the centered
viewport column with `0x470A8`. The returned block is resolved through
`0x4CEA0`, and a candidate is accepted only when its actor identity matches.
Nearest depth begins at `0x7FFF`, only strictly smaller depths replace it, and
an accepted depth below `0x154` ends the scan. `0x445C4` folds quadrants around
`floor(0x20 * minor / major)`; local `(dx,dy)` maps to original-coordinate
`(-dy,dx)`. Sine `0x446BC`, cosine `0x4472C`, and rotation `0x447C4` address the
64 signed 1.15 quarter-wave samples at object-2 `+0xC904`; each multiplied term
is rounded with `(product + 0x3FFF) >> 15`. `OriginalSiegeProjection` and
`OriginalSiegeActorAcquisition` implement the heading, ray, ordered identity,
vertical-bound, and palette-index-zero alpha path for imported scenes.
GameFAQs FAQ 66730 has no low-level ray mathematics and must not be cited as
corroboration for this trace.

For public Retreat, trace the command handler `0x5744B` through transition helper `0x4E5F0`, acquisition `0x4F98D`, kind-0 transition `0x4E76B`, kind-1 transition `0x4E805`, and handlers `0x4FDCD`/`0x4FDAB`; a handler-table label alone is insufficient. Kind 0 selects mode 5 after visible-opponent acquisition and preserves heading with flags `0x142`; the scheduler's flag-`0x40` collision branch clears the blocked remainder and applies `(((heading + 0x20) & 0xC0) - 0x40) & 0xFF`. Kind 1 selects mode 4, whose acquisition-table entry repeats `0x4F98D`; decode mode 4 of the kind-1 table at object-1 `0x3E480` to transition `0x4E6D3`, which selects ranged mode 11 or fallback mode 1. Handler `0x5010B` aims at the acquired actor, uses a close branch at Manhattan fixed distance `<= 0x154`, otherwise raycasts, and requires returned distance to be strictly below object-2 raw contact column `0xCE24 + 28 * combatRow`. Its row-23/24 effect construction doubles descriptor field `+0x14`. The mode-9/10 handler at `0x50020` is a separate later-state route, not the immediate public Retreat path: relocation source `0x5009E` resolves object-2 offset `0x7CEA`, whose supported executable value is the double `1.5`. Use `0x446BC`, `0x4472C`, and `0x44740` when closing its non-cardinal motion; the quarter-wave table matches `round(sin(i * pi / 126) * 32767)` for `i=0..63`, and rotation rounds with `(product + 0x3FFF) >> 15`.

Continue kind-0 mode 5 through its own acquisition entry `0x4FC34` and transition `0x4E70C` in the runtime table at `0x4E43C`. The predicate scans all 68-byte actor records in authored order, skips inactive/self/opposite-side records, computes live-center heading through `0x445C4`, casts the centered `0x470A8` ray, and requires the identity returned through `0x4CEA0` to match the candidate. Keep only strictly nearer depth from `0x7FFF`; `< 0x200`, not `<=`, is the early-exit gate. Success requests mode 7 while failure retains mode 5. Handler `0x4FE76` directly approaches the stored actor. Mode-7 acquisition `0x4F8B0` differs critically: it casts toward the stored target but accepts any living same-side actor returned below `0x200`, even when it is not that target; transition `0x4E732` then selects mode 1 or returns to mode 5. The first authored friendly promoted by loader `0x51560` supplies player actor metadata and ordering, while the live player center comes from the session/`Viewer`; do not assume the authored actor cell equals the viewer cell. Treat mode 1 onward as a separate open trace. GameFAQs FAQ 66730 supplies no low-level corroboration for this state route.

For public Attack's no-target path, start with requested mode 6 and acquisition-table entry `0x4F98D`. Decode kind-0 transition `0x4E71F` and kind-1 transition `0x4E745`: failure leaves both kinds in mode 6, whereas success selects modes 8 and 11. The mode-6 current-handler table entry is `0x4FDCD`; verify that it preserves heading and rewrites low descriptor flags with `(flags & 0xA7) | 0x40`. Follow that live flag into scheduler `0x53C13`-`0x53C6C`, where collision clears the blocked axis and applies `(((heading + 0x20) & 0xC0) - 0x40) & 0xFF`. Record the failed acquisition edge as part of the route; inspecting only successful Attack movement incorrectly hides mode-6 wandering.

To close mode-11 attack cadence, do not stop at handler `0x5010B` or its doubled descriptor interval. In scheduler `0x530D0`, strict deadline check `0x5310D` advances a live effect only when `now - deadline > interval` and carries the overrun into the next deadline. At `0x53365`, require `effect counter == effect count`, actor mode `+0x18 == 0x0B`, and living target health `+0x40 > 0` before call `0x4F070`. Completion then reaches `0x53D47`, clears actor effect handle `+0x08` to `-1`, and calls thinker `0x4F49C` for that same actor. This proves damage belongs to effect completion and that the next shot may be constructed immediately afterward; a handler-entry damage model is incorrect.

For actor transition tables, distinguish an object-relative offset from a runtime virtual address. In this LE image object 1 loads at `0x10000`, and its initialized bytes begin at raw data-page offset `0x4C254`. The actor-kind dispatcher is object-1 offset `0x3E41C`; its transition tables begin at `0x3E43C`, `0x3E480`, `0x3E4C4`, `0x3E500`, `0x3E53C`, `0x3E578`, and `0x3E5B4`. Decode each relocated dword as an object-1 target, then add the load base only when comparing with disassembly VAs. `--fixup-source` at sources `0x4E6A7`, `0x4E6BC`, `0x4E801`, `0x4E888`, `0x4E90F`, `0x4E97F`, `0x4E9A3`, and `0x4E9C7` verifies those table references. The same method resolves acquisition and current-mode tables at runtime `0x4F414/0x4F458` from sources `0x4F644/0x4F8AC`. This avoids treating a file offset, object-relative offset, and loaded VA as interchangeable.

For bounded conversation entry-point analysis, pass decimal or hexadecimal node identifiers with `--conversation-nodes=1100,0x44C`. The ignored `conversation-node-report.txt` records only structural metadata plus portrait and speaker identifiers; it does not copy prompts or response text into the repository.

For numeric action-tree inspection, `--action-groups=1101,2011` writes an ignored expression/branch trace without dialogue prose, while `--resource-integers=all.vtb` emits a bounded dword view. Pass `all` to `--action-groups` or `--conversation-nodes` to emit the complete metadata-only population rather than a comma-separated selection. For evidence correlation against the owner-local dialogue, `--conversation-text=1101,1152` writes only the explicitly selected decoded text to an ignored, redistribution-prohibited report; never commit that output. `--fixup-source=0x6ABBC` reports LE relocations near a source address and is useful for resolving switch tables whose unrelocated operands are misleading.

To close the mode-9/10 route question, decode slots 9 and 10 in every kind
table, not only the friendly public-command tables. Object-1 table bases are
`0x3E43C`, `0x3E480`, `0x3E4C4`, `0x3E500`, `0x3E53C`, `0x3E578`, and
`0x3E5B4`; kind 7 shares kind 2. Their resolved transition routines are among
`0x4E6C0`, `0x4E758`, `0x4E76B`, `0x4E805`, `0x4E818`, `0x4E8D8`,
`0x4E926`, `0x4E95F`, and `0x4E9A7`. Cross-check those columns against all ten
current/requested/previous triples written by initializer `0x542F8`, then
against the complete official placed-template census. That combined evidence,
not absence of a public button alone, proves `0x50020` unreachable for the
supported population. Preserve the handler's 1.5 non-cardinal formula as a
dormant technical mapping rather than inventing a runtime entry.

For shared-destination behavior, correlate scheduler `0x53425`-`0x53694`
with the ignored `--scene-blocks` census. The neighbor test reads behavior bit
`0x02`; every placed actor base and each adjacent attack/hit/death state is
`0x87`. On a successful cell change, follow actor record `+0x28` as the saved
underlying block: it is restored into the old map cell, replaced with the new
cell's block, copied into live block state target `+0x40`, and followed by
actor block `+0x24` being installed in the new cell. Because this occurs before
the next effect record, the trace proves single-cell occupancy and blocked
retry without relying on a visual observation. Exact simultaneous precedence
is not a stable original rule. Input dispatch at `0x55D0F`-`0x561DB` installs
orders before the loop calls scheduler `0x530D0` and thinker `0x50524`;
`0x50524` starts from persistent actor cursor `D5C4`, while constructor
`0x4C7F4` scans live effects from slot zero and takes the first free slot.
The cursor advances once per unrestricted main-loop pass, whose `0x584DF`
jump has no wait, so the actor encountered first after input depends on
processor/input timing. Record stable list-order precedence as an adaptation,
not as recovered executable behavior.

## Evidence discipline

- Hash the executable before analysis and compare it with the reference hash above.
- Record virtual addresses and independently described behavior, not copied machine-code bytes.
- Confirm recovered algorithms against original resource populations and synthetic malformed inputs.
- Keep uncertain interpretations marked Provisional until static evidence or controlled observation resolves them.
- Put stable format facts in `resource-formats.md`, gameplay findings in `original-findings.md`, and status changes in `implementation-plan.md`.
