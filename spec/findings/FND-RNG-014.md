---
id: FND-RNG-014
title: Nonzero suffix selectors give ordinal values for signed transition ordering
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008AC7E..0x0008AC94
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008AD58..0x0008ADAB
tool: Capstone 5.0.9, bounded x86-32 decoding of the fingerprinted relocated LE image
environment: null
---

## Observation

FND-RNG-008 describes suffix records: selector at offset 32 and ordinal at
offset 28 for the nonzero selectors. The first listed helper saves four
registers and reserves thirty-six local stack bytes. It reads its first
argument, a record pointer, at current ESP plus fifty-six, corresponding to
entry ESP plus four. It reads the selector as a four-byte value through DS.

For a nonzero selector it branches to the second listed range. Selector one
returns the offset-28 dword minus one, with thirty-two-bit wrapping. Every
other nonzero selector returns that dword unchanged. It restores its local
stack and saved registers and performs a near return. On this path it makes
no calls, writes no record field, and does not read its second argument, the
year used by the zero-selector path. No leap adjustment or ordinal range
validation occurs on this path. This does not describe the zero-selector
month/weekday calculation or the classifier's later use of the ordinal.

The order helper at `0x0008AD6F` takes first-record, second-record and year
arguments. After its four saved registers, it reads year at current ESP plus
twenty-eight and pushes it; the resulting stack-relative read of the first
record is current ESP plus twenty-four. It pushes that record and calls
`0x0008AC7E`, removing both four-byte arguments afterward. It similarly pushes
year and second record for another call, keeping the first result in ESI.
The called helper saves and restores ESI on its nonzero-selector path.

It compares the two returned thirty-two-bit values as signed integers. It
returns one only when the first is strictly greater than the second, otherwise
zero. Its own saved registers are restored and it performs a near return.
Equal values therefore return zero. The first result's register provenance
and second call's preservation matter; it does not compare raw record words
before the selector-one adjustment.

## Interpretation

For two nonzero selectors, ordering is a signed comparison of the adjusted
ordinal quantities and ignores year. The wrapping selector-one subtraction
can change their signed order at integer edges. This does not establish valid
native input limits, the zero-selector branch, or complete nonempty-name
classification; those dependencies remain under Q-RNG-001.

## Alternatives

Always subtracting one is ruled out by the selector comparison. Comparing
unsigned ordinals or treating equality as first-greater is ruled out by the
signed less-or-equal branch. Inferring leap adjustment from the syntax's
selector name is ruled out on this branch by its direct dword return path.

## How to reproduce

Verify BLD-GOG-EN and decode the listed entry and tail/order ranges. Track
stack offsets after each push and cleanup, the width of selector and ordinal,
the first return's ESI storage, ESI preservation in the second call, and the
signed comparison. Read the separate month/weekday path and classifier
consumers before extending these results to complete transition behavior.
