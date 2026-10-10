---
id: FND-RNG-013
title: An empty classification name clears calendar classification before conversion applies its secondary adjustment
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008ADAB..0x0008ADC7
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008B076..0x0008B083
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00080B8A..0x00080BAE
tool: Capstone 5.0.9, bounded x86-32 decoding of the fingerprinted relocated LE image
environment: null
---

## Observation

FND-RNG-006 follows the seed converter's conditional classification call.
The first listed range is the entry of that classifier. It saves four
thirty-two-bit registers and reserves twenty-eight stack bytes. Its record
argument is consequently read from entry ESP plus four, expressed as current
ESP plus forty-eight. It reads the four-byte pointer `g_000A70CA` through DS,
then one byte at the pointed-to DS address, and clears its local result to zero.

When that byte is zero it branches directly to the second listed range.
That exit returns the zero result in EAX, writes the same four-byte zero to
the record at offset 32, restores the reserved stack and saved registers,
and performs a near return. It does not read another record field or invoke
another routine on this empty-name path. This describes the empty first byte,
not the validity of the incoming pointer or other classification branches.

The converter has already normalized the record and consulted the environment
when it reaches the third range. It adds the first adjustment to its saved
seconds, tests the record's offset-32 field as a signed thirty-two-bit value,
and calls the classifier only when that field is negative. It then reads the
record field again, independently of the classifier's returned EAX. Only a
strictly positive field causes subtraction of the secondary adjustment.
Thus an empty-name classification of a negative input leaves no secondary
subtraction; a positive incoming classification skips the classifier and
still causes subtraction, even if the name would be empty.

## Interpretation

The empty-name branch is a bounded way to resolve an unknown negative
classification to zero. The converter uses the stored field rather than the
classifier's returned register. This does not establish the actual name or
classification in a native startup, nor the nonempty-name classification rules.
Those remaining dependencies belong to Q-RNG-001.

## Alternatives

Preserving a negative classification when the name is empty is ruled out by
the explicit zero store. Always applying the secondary adjustment merely
because a secondary-name pointer exists is ruled out by the post-call signed
field test. Using only the returned EAX as the converter's decision would
miss its explicit reread of the stored field.

## How to reproduce

Verify BLD-GOG-EN and decode the listed ranges at their instruction boundaries.
Track the saved-register and local-stack widths to the incoming argument.
Follow the empty first-byte branch through the field write, stack restoration
and near return; then follow the converter's independent signed field tests.
Do not assume pointer validity or extend this branch to nonempty names.
