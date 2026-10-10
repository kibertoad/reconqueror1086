---
id: FND-RNG-012
title: Startup publishes the environment source and dispatches its constructor through an initializer table
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00070FF1..0x00071013
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000710E2..0x000710F3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00083FB4..0x00083FFF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    kind: file-data
    offset: 0x000E0608..0x000E0638
tool: Capstone 5.0.9, bounded x86-32 decoding and independent LE relocation and byte-pattern searches
environment: null
---

## Observation

FND-RNG-011 identifies the selector/offset source consumed by the environment
constructor. In the first code range, startup stores ESI as a four-byte offset
at `0x0009F505`, followed by the two-byte CX selector at `0x0009F509`.
These accesses use DS. This bounded reading identifies the stores, not the
validity or origin of those incoming register values.

The second range places 255 in EAX and calls `0x00083FB4`. On return it clears
EBP and calls `0x00083F5A`. Thus the initializer dispatch precedes that latter
call on this continuation. Neither callback completion nor the latter callee's
complete effects are inferred from this call sequence alone.

The dispatcher's table spans `0x000A73B4..0x000A73E4`, with eight six-byte
rows. The listed file-data span holds those rows; LE internal fixups produce
the following target addresses. All markers are initially zero.

| Row address | Priority byte | Relocated target |
| --- | --- | --- |
| `0x000A73B4` | 32 | `0x0007CF14` |
| `0x000A73BA` | 1 | `0x00064FF2` |
| `0x000A73C0` | 2 | `0x0007CF77` |
| `0x000A73C6` | 32 | `0x0007E546` |
| `0x000A73CC` | 32 | `0x0008A101` |
| `0x000A73D2` | 32 | `0x0008B641` |
| `0x000A73D8` | 32 | `0x0008A1F4` |
| `0x000A73DE` | 32 | `0x0008C4A2` |

Each scan resets the candidate to the end address and its priority threshold
to the low byte of the dispatch input. It skips a row whose marker equals
two, or whose priority is unsigned-greater than the current threshold.
Otherwise it selects the row and replaces the threshold with that priority.
It advances by six until the end. Consequently the scan selects the smallest
eligible priority, choosing the last row in a tie. At input 255 every priority
byte is initially within the threshold.

If no candidate remains, the routine restores its saved registers and ES and
returns. For a selected row it reads the four-byte target at offset two.
A zero target skips the call. A nonzero target first loads ES from DS and
makes a near indirect call, preserving the dispatch input in a stack slot.
After the call returns, or for a zero target, it writes marker two into the
selected row and starts another scan.

The environment constructor from FND-RNG-011 is therefore a shipped initializer
target, not a direct call encoded at the startup dispatch site. On unchanged
rows and returning callbacks that preserve the selected-row pointer, priority
one and two run before the constructor; it is then the first priority-32
selection because its row is last. The constructor itself saves and restores
EBX, which carries that selected-row pointer in the dispatcher. Other callbacks'
effects, register preservation, table mutations and failures remain unread;
this conditional order is not a proof of every actual startup invocation.

Two searches over both loaded objects found direct offset references at operand
positions `0x00071008` and `0x0008C4C4`, and direct selector references at
`0x0007100F`, `0x00071052` and `0x0008C4B8`. One search used LE relocation
targets, the other all occurrences of the canonical four-byte address values,
without function boundaries. Decoding distinguishes the startup stores from
the later reads. The constructor accesses in FND-RNG-011 were the independently
located positive controls. Indirect aliases, partial-address construction and
runtime-generated accesses are outside these searches; no unique-writer claim
is made.

The table's page mapping was checked separately against the environment key
from FND-RNG-006: address `0x00099FC4` maps to file offset `0x000D3218`, where
the three bytes are the already recorded key and terminator. The table begins
at file offset `0x000E0608`; its constructor row begins at `0x000E0632`.

## Interpretation

Startup has a concrete producer for both parts of the environment source and
a concrete initializer-table route to the constructor. The incoming source
values, earlier startup branches, operating-system selector behavior and other
callbacks must still be read before deriving the environment seen by seed
conversion. Those dependencies remain under Q-RNG-001.

## Alternatives

Finding no direct relative call to the constructor would not show that startup
omits it: the relocated table target and indirect call explain this route.
Ascending table order alone would predict the wrong tie order; the scan replaces
the candidate on an equal priority. The source selector is not assumed to be
DS merely because DS holds the reference's two stored parts.

## How to reproduce

Verify BLD-GOG-EN and apply its LE internal fixups. Decode the three code ranges,
follow the byte-width unsigned comparisons, six-byte scan, near call and marker
write. Read all eight rows from the bounded file span, applying their recorded
fixups, and verify the LE page map with the independent environment-key control.
Repeat both direct-reference searches over both loaded objects and decode each
hit. Follow the incoming source values and other initializer effects before
claiming complete startup environment provenance.
