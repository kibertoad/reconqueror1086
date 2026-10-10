---
id: FND-RNG-011
title: The environment array is lazily constructed from a selector-offset source
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008C4A2..0x0008C57F
tool: Capstone 5.0.9, bounded x86-32 decoding and independent LE-relocation/byte-pattern reference searches
environment: null
---

## Observation

FND-RNG-006 reads the environment lookup's array root at `0x000A70D0`.
The listed routine first tests that root and skips construction when it is
already nonzero. It saves EBX, ESI, EDI, ES, FS and EBP, and restores them on
every normal exit.

On the zero-root path it loads FS from the word at `0x0009F509`, loads the
source offset from the dword at `0x0009F505` into EBP, and loads ES from FS.
Those adjacent offset/selector parts are read together as `g_0009F505`.
Source scanning uses ES and the offset; the copied destination bytes later
use DS. This reading does not equate the source segment with DS or SS.

It scans null-terminated strings until the next string begins with a null
byte. The scan counts strings separately from the source distance. The distance
includes each scanned string's terminator and excludes the final empty-string
terminator. An empty source changes a zero distance to one. It passes that
quantity to `0x00073D78`, retaining the returned value as the prospective
string-buffer pointer. A zero return exits without publishing the array root.

For a nonzero first result, it forms a second request quantity as
`5 * scan_count + 4` using thirty-two-bit shifts and additions, and calls
the same helper. A zero second result passes the first buffer pointer to
`0x00073E7B` and then exits. A nonzero second result is published at
`0x000A70D0` before the copying loop begins.

It resets its copy counter, reloads ES from FS, and scans again from the saved
source offset. For each string it stores the current DS destination pointer
in the next four-byte array slot, then copies the source bytes through and
including their null terminator. At the end it stores a zero pointer after
the copied strings. It sets `g_000A70D4` to the address immediately after
that null pointer and calls `0x00065350` with this destination, fill value
zero and the copy count. Thus the last call receives one tail byte per copied
string, following `4 * (copy_count + 1)` bytes of pointer slots.

The scan count and copy count are distinct passes. Their equality requires
the source and selector to remain suitable across the intervening calls;
that is not established by counting the first pass alone. The allocation
helper's units, internal size rounding and failure effects, FS preservation
across those calls, and the fill/free helpers' complete behavior are outside
this bounded reading. The quantity passed to an allocation helper is not
claimed here as its actual allocated payload size.

Two searches independently located direct references to the array root:
LE relocation operands targeting that data address, and every occurrence of
its canonical thirty-two-bit value in both loaded objects without depending
on function boundaries. Both gave code operand positions `0x0008462A`,
`0x0008C4AB`, `0x0008C51A`, `0x0008C530` and `0x0008C54A`. The lookup in
FND-RNG-006 supplied the positive control independently of these searches.
These are operand positions, not function starts. The search does not cover
indirect aliases or addresses constructed from separate values, and does
not establish that no other writer can reach the root.

## Interpretation

The environment lookup's pointer array has a lazy construction path with
separate string and pointer/tail requests. Its source uses a selector/offset
pair, not an assumed flat data pointer. The root's initial zero value alone
does not establish whether it is still zero when the seed converter runs.
Startup ordering and the source pointer's writers remain part of Q-RNG-001.

## Alternatives

Always replacing an existing array is ruled out by the initial nonzero-root
exit. Publishing the root only after copying is ruled out by the earlier
store. Treating the source and destination as the same storage merely because
their offsets are comparable is ruled out by the separate ES source and DS
destination accesses. No complete allocation or initialization guarantee is
inferred from the two helper calls.

## How to reproduce

Verify the source identity and decode the listed range. Track both parts of
the source reference, ES/FS saves and reloads, each request's register width,
the published root, and both counters through every exit. Independently repeat
the relocation and raw-value searches with FND-RNG-006's lookup as control.
Follow the source pointer's producers, construction callers and allocator/
fill/free effects before claiming complete startup environment provenance.
