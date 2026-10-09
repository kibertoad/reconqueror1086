---
id: FND-STRATEGY-043
title: The home selector calls the inclusive helper with seven, contradicting the rule's bound of six
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
    address: 0x00024C38..0x00024C4C
tool: Capstone 5.0.7, bounded x86-32 decoding of the fingerprinted relocated LE image
environment: null
---

## Observation

The function at `0x00043670` saves ESI, places the literal 7 on the stack,
and calls `0x00024C38` at `0x00043673`. It removes that four-byte argument
after the call returns to `0x00043678`. There is no intervening write to
the argument before the helper reads it.

The helper's reading in FND-RNG-002 is confirmed here: it calls the raw
generator, reads its own first four-byte argument, adds one to the divisor,
and returns the signed division's remainder in EAX. For argument 7 and the
nonnegative raw result of FND-RNG-003, that is the raw result modulo eight,
with an output range of 0 through 7 inclusive.

The home selector stores the returned EAX into `0x0009C9D0` at
`0x0004367C`, and reads a 32-bit person index from
`0x0009B8C8 + 4 * EAX` at `0x00043681`. It puts that person index in ESI
before another call. Nothing clamps, rejects or subtracts from the helper's
result before either the global store or the indexed read.

This reading does not establish the table's independent extent, what an
index of 7 selects, or every effect of the later callees. It does not read
the adjacent shipped data as a table merely because the code can access it.

## Interpretation

RULE-STRATEGY-012's `random_inclusive(6)` contradicts the direct argument
provenance. FND-STRATEGY-023 already records argument 7, but its seven-home
interpretation did not resolve this discrepancy. The rule is disputed until
Q-STRATEGY-043 independently reads the selection tables and follows index 7
through its consumers. A recorder must not substitute bound 6 for the native
argument or silently discard a result of 7.

## Alternatives

A count-style, upper-exclusive helper would explain a seven-home range, but
is ruled out by the helper's increment and remainder. An intervening clamp
before the person lookup is ruled out by the caller's instruction order.
Whether the tables include an eighth intended entry or the endpoint accesses
adjacent data remains open; neither follows just from the call argument.

## How to reproduce

Decode both listed exclusive ranges from the fingerprinted BLD-GOG-EN LE
objects in 32-bit mode. Track ESP through the argument push, call, helper's
argument read and caller cleanup. Follow the returned register to its first
global store and indexed read. Separately locate the shipped table data and
its readers before claiming its length or an endpoint's meaning.
