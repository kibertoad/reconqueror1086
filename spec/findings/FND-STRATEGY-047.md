---
id: FND-STRATEGY-047
title: Home selection passes the drawn index rather than the selected person to fief initialization
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043670..0x000436E0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002C3A4..0x0002C428
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002CAE8..0x0002CBEE
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002CCCC..0x0002CCD3
tool: Capstone 5.0.7 over the fingerprint-verified relocated LE code object
environment: null
---

## Observation

FND-STRATEGY-044 identifies person 28 at home-selection index 7. The selector
loads the selected person into ESI and calls `0x0002C3A4` while the drawn home
index remains its stack argument. The callee reads that argument into EAX.
Consequently its input is the home index, not the person identifier: for
index 7 it takes the below-18 branch, fails the equality test against 17,
and reaches the common unchanged-state return at `0x0002CCCC`.

The same callee has an explicit input-28 branch. Its unsigned comparisons
place 28 below 29, above 18 and above 24, then equality against 28 enters
`0x0002CAE8`. That branch follows the current fief-list root through `+0x08`,
its first pointer and the fief's `+0x34`, `+0x30` or `+0x2C` list pointers
(the root/list interpretation is FND-ESTATE-003). It writes these dwords:

| Fief list pointer | Offset within list | Value |
| --- | --- | --- |
| +0x34 | +0x24 | 37 |
| +0x30 | +0x5C | 40 |
| +0x30 | +0x7C | 93 |
| +0x30 | +0x9C | 13 |
| +0x30 | +0xBC | 5 |
| +0x2C | +0x5C, +0x70 | 6 |
| +0x2C | +0x88, +0x9C | 6 |
| +0x2C | +0xB4, +0xC8 | 4 |
| +0x2C | +0x138, +0x14C | 10 |
| +0x2C | +0x164, +0x178 | 3 |

It stores the unchanged root pointer back to its global and returns, with
no calls, draws or index clamp on this branch. This explicit branch does
not run for home index 7 through the selector's call.

## Interpretation

The person lookup and the fief-initialization dispatch use different values.
The selected person survives in ESI for later coordinate and assignment
accesses, while the callee receives the drawn index still on the stack.
An explicit case for person 28 therefore does not prove that this selector
initializes that person's fief statistics. This adds an argument-provenance
constraint to Q-STRATEGY-043 without claiming a complete consumer reading.

## Alternatives

Reading ESI as the callee argument would send person 28 into its explicit
branch, but is contradicted by the push before the person lookup and the
callee's `[ESP + 4]` read. No argument replacement intervenes. Whether
another caller invokes the explicit person branches, and whether subsequent
initialization supplies the intended fief values, remains unread here.
No validity or capacity of the pointed-to lists is inferred from the writes.

## How to reproduce

Verify BLD-GOG-EN and apply its LE fixups before bounded 32-bit decoding.
Track the selector's push, person lookup, call and four-byte cleanup, then
the callee's first stack read. Follow input 7 and input 28 separately through
the unsigned comparisons, and read the complete explicit-28 write/return
path. Confirm the source ranges against the committed function inventory.
