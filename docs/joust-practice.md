# Practice Joust executable map

All addresses below refer to the owned GOG `CONQUER.EXE` with SHA-256
`5d7231758766204ad061e6b82cf2f0e0cbe28899b35d095f13e4aad75c8b79d6`.
The supporting disassembly and resource reports are generated locally under
ignored `analysis/original/joust-practice-xrefs/`; no original bytes are tracked.

## Movie and foreground

The printable `jousprac.SMK` string is at object-2 `+0x61DC` (file `0xCF430`).
Routine `0x41164` pushes that address at `0x41227`, constructs its path with
`0x63014`, and opens the movie through `0x6C287`. The owned movie decodes to
88 frames of 640-by-300 pixels at 71 ms. Its decoded picture contains the lane
and opponent but no foreground lance. These identities are **Confirmed** by
the executable string relocation and decoded movie frames.

Caller `0x41CEC` passes 1 to `0x41120` at `0x41D17`. The callee formats the
object-2 `+0x61B0` string `lance%1d.csf`, loads it through `0x18430`, and stores
the handle at `+0x1C170`. The caller then invokes `0x41164` at `0x41D46` and
releases the lance handle through `0x41154` at `0x41D69`. Worker
`0x4169E-0x4174F` reads that handle, crops the selected frame, and paints it
at `(lanceX, lanceY + 90)`. The `lance1.csf` foreground identity and 90-pixel
paint offset are **Confirmed**. The replacement currently uses its imported
`Dragon.Lance` texture role for the same source file; exact active palette
association during the practice movie remains **Provisional**.

## Frame selection and motion

Initializer `0x4116D-0x411CF` sets the source bounds to x `50..400` and y
`20..280`, centered at `(225,150)`. At `0x41387-0x4139C`, the five horizontal
columns have width `floor((400 - 50 + 2) / 5) = 70`. The object-2 dword table
at `+0xB838` begins `150,98,46,24,2`. The scan at `0x41632-0x41646` takes the
first threshold no greater than y; `0x41646-0x4169E` then selects
`clamp(5 * band - 1 - floor((x - 50) / 70), 0, 24)` where `band` is 1..5.
`OriginalPracticeJoustLance.FrameFor` and focused tests preserve these
**Confirmed** boundaries. This is distinct from the dragon worker's table,
whose first four thresholds are 90 pixels higher.

Worker `0x41562-0x41632` adds the current signed 8.8 velocities to the lance
position and clamps both axes, resetting an out-of-range fixed coordinate to
the bound. At `0x4177F-0x41817`, it damps each velocity to 80 percent and adds
`floor_toward_zero((rawPointer - clampedLance) / 10) * 0x3200 / parameter`.
The fifth argument passed at `0x41D2E` is `0x78 = 120`, so this practice path
uses divisor 120. The input globals `+0x207E8/+0x207EC` are the raw 640-by-480
mouse coordinates, as established by the input callback `0x849C1-0x849C7`;
there is no subtraction of the movie's 90-pixel paint offset from those
coordinates. At `0x41372-0x4139C`, the worker derives a frame interval
`floor(500 / movieFrameMilliseconds) = 7`. At `0x413D5-0x413F1`, the vertical
impulse is `(150 - 120) * 50 = 1500` in 8.8 units. `0x41819-0x4183F`
subtracts it when the movie frame counter is divisible by seven.
`OriginalPracticeJoustLance.Advance` implements this **Confirmed** arithmetic.

The original worker runs in an unrestricted loop, so its number of motion
iterations per displayed movie frame depends on processor speed. The host
advances once per decoded movie frame, catching up elapsed frames in order;
this is an explicit stable timing policy. The exact foreground palette remains
**Provisional**.

## Contact and result

Object-2 `+0xB864` is the first contact frame, 80. The worker's strict gate at
`0x4184D-0x41871` admits only movie frame counters 80, 81, and 82. The x
targets at `+0xB84C` are `191,161,122`; the y targets at `+0xB858` are
`191,203,211`. At `0x41877-0x41922`, it accumulates the absolute x and y
distance from each target independently, while also retaining signed deltas
for later direction-specific text. The 88-frame movie completes at frame 87.
These table entries and the three-frame window are **Confirmed** by direct
executable data and control flow.

Caller `0x41CEC-0x41D46` initializes local player points to 20, opponent
points to 50, and passes a result pointer to `0x41164`. At `0x41A97-0x41AD9`,
the worker succeeds only when `90 - playerPoints` is strictly greater than
**both** accumulated errors. This practice invocation therefore requires each
axis error to be below 70. A hit sets result 1 and adds two player points.
On a miss, `0x41ADB-0x41B34` draws `random(100)` through `0x41110` and
compares it strictly below `opponentPoints - playerPoints + 50 = 80`: a lower
roll sets result 0 and adds two opponent points; otherwise result 2 records
both riders missing. The random draw is skipped entirely on a player hit.
`OriginalPracticeJoustTrial` and its threshold, draw, and incomplete-frame
tests preserve this **Confirmed** branch structure.

`0x41B59-0x41C93` picks result and direction-dependent source message
pointers, then waits for an input event. The host shows a short authored result
overlay on the Practice screen and requires dismissal before another choice.
Its wording, the original text catalog, and exact physical dialog events
remain **Provisional**. The caller's practice points are local stack values;
this isolated training run does not mutate campaign tournament scoring.
