---
id: FND-UI-025
title: Startup preparation has distinct service return boundaries before its animation test
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002AC08..0x0002AC86
tool: Capstone 5.0.7, bounded x86-32 decoding of the fingerprinted relocated LE image
environment: null
---

## Observation

FND-UI-018 identifies startup preparation. Its entry saves three four-byte
registers and reserves 80 bytes, leaving ESP 92 bytes below its entry value.
The following boundaries are immediately after service calls, before their
argument cleanup. Their ESP displacement includes the arguments still present.

| Service | Call | Return boundary | ESP below entry |
| --- | --- | --- | --- |
| `0x0005B584` | `0x0002AC15` | `0x0002AC1A` | 100 |
| `0x00024DB8` | `0x0002AC2A` | `0x0002AC2F` | 104 |
| `0x00063270` | `0x0002AC36` | `0x0002AC3B` | 100 |
| `0x0005B3B0` | `0x0002AC48` | `0x0002AC4D` | 108 |
| `0x0005B790` | `0x0002AC5C` | `0x0002AC61` | 100 |
| `0x00064BCF` | `0x0002AC70` | `0x0002AC75` | 100 |
| `0x0005B554` | `0x0002AC79` | `0x0002AC7E` | 96 |
| `0x00024CA0` | `0x0002AC81` | `0x0002AC86` | 92 |

Each cleanup drops exactly the arguments in its row before the next call.
The `0x00064BCF` call occurs only when ESI is nonzero; its two arguments
are EDI and the dword read through DS at `0x0009DAB0`. Both paths join at
`0x0002AC78`. The other listed boundaries occur on the straight-line path.
The first service's result is retained in ESI and EBP before the second call;
the sample service's result is retained in EDI before the wait call.

## Interpretation

These boundaries distinguish completion of the preparation services without
assuming that disabling animations bypasses them. Reaching one establishes
only that the corresponding call returned, not its hardware outcome.

## Alternatives

Treating every boundary as the same stack depth would overlook argument bytes
whose cleanup is deferred until after that boundary.

## How to reproduce

Verify the BLD-GOG-EN source identity, load its LE code object with relocations,
and decode only the listed location in x86-32 mode. Follow ESP from entry,
count each four-byte push and the reserved local space, and pair each call
with its following argument cleanup. Follow the ESI test's two paths to their
join. Compare the eight return boundaries independently of callee behavior.
