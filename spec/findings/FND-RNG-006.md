---
id: FND-RNG-006
title: Calendar seed conversion consults the DOS environment before applying adjustments
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00080B78..0x00080BAE
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00084620..0x0008467B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008B230..0x0008B24B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008B492..0x0008B534
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008B5DE..0x0008B641
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    kind: file-data
    offset: 0x000D3218..0x000D321B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    kind: file-data
    offset: 0x000E02D0..0x000E02DC
tool: Capstone 5.0.7, bounded x86-32 decoding of the fingerprinted relocated LE image
environment: null
---

## Observation

FND-RNG-005 follows the DOS calendar reader into its converter. After the
converter normalizes the temporary record through `0x0008B0B9`, it calls
`0x0008B230` with no arguments, then reads the adjustment at `0x000A707C`.
It adds that adjustment to its saved seconds quantity. If the record's field
at offset 32 is negative it calls `0x0008ADAB` with the record pointer. If
that field is subsequently positive, it subtracts `0x000A7080`. The
environment call therefore precedes both adjustment reads.

The no-argument helper passes a pointer to the null-terminated environment
key `TZ` to `0x00084620`. The key lies at the first listed file-data range,
mapped to `0x00099FC4`. The lookup reads the pointer at `0x000A70D0` as an
array of string pointers terminated by a null pointer. A null array or null
key returns zero. It compares each string's prefix with the key, using the
key's length and `0x0008B5DE`, and requires the following byte to be `=`.
On a match it returns a pointer to the byte after that separator; an empty
value still has a nonzero pointer. No match returns zero.

The comparison folds ASCII uppercase letters to lowercase on both sides.
It returns zero after the requested byte count, or an equal null byte,
and otherwise returns the difference of the folded unsigned byte values.
Thus this lookup accepts the key regardless of ASCII letter case, and does
not match a longer key that merely begins with the same letters.

If lookup returns zero, the helper returns without calling the parser or
writing the adjustment fields. Otherwise it passes the value pointer to
`0x0008B492`. That parser first clears `0x000A7084`, and passes the value,
a name destination at `0x000A7088`, and the address of `0x000A707C` to
`0x0008B275`. It examines the byte at that callee's returned pointer.
If that byte is null, it clears the byte at `0x000A70A7` and skips the
second parse. Otherwise it prepares a stack word equal to the newly read
`0x000A707C` minus 3600, sets `0x000A7084` to one, and passes the returned
pointer, name destination `0x000A70A7`, and that stack word's address to
`0x0008B275`. After the second call it writes `0x000A7080` as the first
adjustment minus the stack word. It then conditionally calls `0x0008B38C`
with destinations `0x000A7034` and `0x000A7058` for comma-separated suffixes.
The next suffix is tested at the first suffix callee's returned pointer.

The second listed file-data range holds initial 32-bit values 18000, 3600
and one for the fields mapped to `0x000A707C`, `0x000A7080` and
`0x000A7084`, respectively. These are shipped load-image values, not proof
that startup leaves them unchanged or that any recorded run used them.

## Interpretation

The seed converter has an environment-dependent adjustment path. Observing
the DOS date and time alone does not determine its returned seed source.
The lookup and parser wrapper explain which environment value reaches the
adjustment machinery, but do not establish all accepted value syntax, parser
errors, suffix semantics, environment-array initialization, or the record's
offset-32 classification. Those remain part of Q-RNG-001.

## Alternatives

A conversion independent of the DOS environment is ruled out by the lookup
and conditional parser call before the converter reads its adjustment.
A complete epoch-seconds interpretation remains open: recognizable key names
and shipped adjustment values do not substitute for reading the parser,
record normalization and classification, and initialization paths.

## How to reproduce

Load the fingerprinted BLD-GOG-EN LE objects with internal relocations applied.
Decode the listed code ranges in 32-bit mode. Track the converter's saved
seconds quantity, the lookup's key length and returned pointer, and the
parser wrapper's stack word through both calls. Locate the key and initial
adjustments through the LE page map, independently of instruction decoding.
Continue through the named parser and classification helpers before claiming
the seed source's complete timestamp semantics.
