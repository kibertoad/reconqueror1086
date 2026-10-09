---
id: FND-SOUND-008
title: Busy-voice replacement reduces one generator result by the verified ten-voice loop count
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B418..0x0005B470
tool: Capstone 5.0.7, bounded 32-bit decoding of the fingerprinted LE code object
environment: null
---

## Observation

Within the sample-playing routine of FND-SOUND-003, the voice scan starts
both the index and byte offset at zero. It exits for a zero handle or a
nonzero result from the driver's completion test. Otherwise it adds four
to the offset and one to the index while the offset is below 40. The
subsequent comparison permits the random replacement path only when the
index in ESI equals ten.

That path calls the generator at `0x0005B448`, returning to
`0x0005B44D`. It sign-extends EAX into EDX and performs signed division by
ESI. At `0x0005B454`, before reading the selected handle, EDX is the
remainder and therefore the selected voice index. The generator's
nonnegative result of FND-RNG-003 makes this a value from zero through
nine. The path copies EDX into ESI before asking the driver to stop the
selected handle.

## Interpretation

This draw belongs to RULE-SOUND-002. Its native reduction can be checked
at the boundary before the selected handle is read: the divisor is ten,
the raw return is EAX, and the reduced result is EDX. The read does not
establish driver timing, voice completion, stop success or the whole
sample-playing procedure.

## Alternatives

The replacement path does not use an arbitrary voice count supplied by
a caller: the preceding scan and equality comparison determine ten.
The handle returned by the driver is not the reduced RNG result.

## How to reproduce

Verify BLD-GOG-EN identity and decode the listed range. Follow both scan
exits and the continuing edge into the equality test. Trace ESI through
the generator call and EDX through signed division to the handle lookup
and index store. FND-SOUND-003 records the surrounding service calls;
FND-RNG-003 records the generator's result range.
