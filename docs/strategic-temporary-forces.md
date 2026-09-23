# Original strategic temporary forces

This map covers the owned GOG `CONQUER.EXE` with SHA-256
`5d7231758766204ad061e6b82cf2f0e0cbe28899b35d095f13e4aad75c8b79d6`.
Addresses below are loaded Linear Executable virtual addresses unless prefixed
`object-2 +`. The evidence is direct LE-aware disassembly and bounded decoded
`all.cbf/all.cif`, `all.tmb/all.tmi`, `scot.rat`, and `wales.rat` resources.
The analysis artifacts and original bytes stay outside Git.

## Record layout

| Record | Offset | Recovered field | Confidence and source |
| --- | ---: | --- | --- |
| Creator descriptor, `0x20` bytes at object-2 `+0x1A0F0` | `+0x04/+0x08` | Independent zero-based expiry month and year, `1/2000` for fixed slots | Confirmed, wrappers `0x3B97C/0x3B9B4` and expiry `0x3B841-0x3B865` |
| Creator descriptor | `+0x10/+0x14/+0x18/+0x1C` | Origin property 7, active flag, encounter result flag, physical slot 1 or 2 | Confirmed, `0x3B0FA-0x3B1E6`, `0x3B732-0x3B750`, `0x3B7A7-0x3B7BD` |
| Force, `0x118` bytes at object-2 `+0x1A150 + slot * 0x118` | `+0x00/+0x0C/+0x18/+0x30` | Active, route completion, route count, route cursor | Confirmed, loader `0x49ED0`, updater `0x4A61C`, lifecycle `0x3B0E4` |
| Force | `+0x1C/+0x20/+0x24` | Swordsmen, halberdiers, knights | Confirmed, constructor `0x3B1A2-0x3B1D6`, resolver caller `0x3B3ED-0x3B468` |
| Force | `+0x28/+0x2C/+0x34` | Origin property, lord, routed mode 2 | Confirmed, `0x3B177-0x3B199` |
| Force | `+0x3C..+0x48` | Target and projected grid coordinates | Confirmed, `0x49ED0-0x4A06C` |
| Force | `+0x5C..+0x68/+0x6C` | Float current position/direction and allocated route handle | Confirmed, loader, updater, expiry |
| Player, `0x118` bytes at object-2 `+0x1A4B8 + slot * 0x118` | `+0x00/+0x54/+0x5C/+0x60` | Active, avatar notice cooldown, float x/y | Confirmed, contact scan `0x3B2C9-0x3B360` |

The two fixed routes contain 44 and 42 signed point pairs respectively.
`OriginalStrategicTemporaryForceSlot`, `OriginalStrategicTemporaryForces`, and
`OriginalStrategicTemporaryForces.Runtime` implement the currently active
creation, route, expiry, and marker portions. `OriginalStrategicPlayerMovementSlot`
preserves the player contact fields.

## Creation and pass order

Albert conversation node 2439's accepted northern-campaign exit executes action
group 2423, which writes scope-zero variable 43 to one. Valletta node 2593's
accepted Welsh task executes group 2552, which writes variable 93 to one.
These are decoded resource facts. UI poll `0x22242-0x2228A` tests those variables
for nonzero and raises one-shot globals `A92C/A930`. Continuation
`0x3C2D6-0x3C301` clears those globals and calls fixed creator wrappers
`0x3B97C/0x3B9B4` in slot order. An already active descriptor returns at
`0x3B0FA-0x3B10B` before reloading or consuming random values.

For each strategic pass, `0x3C293-0x3C2A7` calls the no-descriptor
`0x3B0E4` lifecycle before player updater `0x13168`. The lifecycle visits
three descriptors in physical order. Each active descriptor scans all six
player records before its route update `0x4A61C` and independent expiry
comparisons. The source main loop runs without a stable processor-independent
frequency; `OriginalStrategicHostRuntime` uses one explicit 60 Hz fixed pass.
Its conversation-variable bridge, `ProcessTemporaryForceActions`, and
`AdvanceTemporaryForcePass` preserve the mapped creation and movement order.

## Contact and resolver

The contact predicate at `0x3B2C9-0x3B30A` is active player plus
`abs(playerX - forceX) < 30.0` **and** `abs(playerY - forceY) < 30.0`.
The double at object-2 `+0x5654` is exactly `30.0`; equality on either axis
fails. Army slots 0-4 precede avatar slot 5 in each scan. An avatar contact
with zero cooldown issues a distinct notice and writes `120` to player
record `+0x54`, then continues the scan and patrol route. An army contact
focuses that record, writes selected slot `AE64`, increments scope-zero
variable 5, and calls shared resolver `0x258FC` with the army and patrol's
six direct counters and two zero score modifiers.

After the resolver, `0x3B4BB-0x3B591` clamps negative counters to zero,
zeroes patrol counters for return value one, and treats an empty patrol as
success or an empty player army as failure. Success writes player survivors
through `0x29DE0` and patrol survivors back to `+0x1C/+0x20/+0x24`.
The terminal success branch awards `40` units to scope-zero variable 17 for
slot 1, otherwise `random(100) + 50`, increments variable 24, clears the
descriptor and force active flags, and releases the route. The failure branch
increments variable 25 and marks descriptor `+0x18`; it sends the engaged
player record `AE6C` to the loss modal or removes an ordinary field record
through `0x29D24`. These counter and branch relationships are Confirmed.
The modern campaign wallet is not inferred from variable 17.

## Runtime mapping and remaining evidence

`AdvanceTemporaryForceContactPass` scans active descriptors and player records
in physical order at their pre-route positions. It returns the first army
contact before route movement or the later strategic pass, while avatar
contact emits `OriginalStrategicTemporaryForceAvatarNotice` and sets the
existing 120-pass `CollisionCooldown`. Strict `< 30.0` comparisons and the
first-contact handoff are tested by
`ResourceAndDefinitionTests.StrategicTemporaryForceContact`.

`OriginalStrategicTemporaryForceEncounter.Capture` carries the six direct
force counters into `Campaign.OriginalStrategicPatrolEncounter`, where the
shared `0x258FC` automatic/tactical resolver receives zero score modifiers
and zero player reserves. `ConquerorGame.StrategicEncounter` presents that
separate caller through the existing tactical screen. Settlement clamps
counters, zeroes patrol survivors on resolver victory, writes army and patrol
survivors, increments scope-zero variables 5/24/25, awards scope-zero variable
17, and clears the active descriptor/record or removes an empty ordinary
field record. The distinguished record raises the existing loss-modal signal.
The runtime preserves variable 17 as a source conversation variable; no
modern wallet equivalence is asserted. These bindings and the slot-1 fixed
reward are covered by the patrol settlement test.

The source's descriptor `+0x18` terminal marker and heap route release have
no persisted replacement fields yet; clearing `Active` prevents further
route execution. The exact notice text selectors and loss-modal presentation
remain Provisional. The source battle UI and original conversation
availability predicates also remain Provisional.
