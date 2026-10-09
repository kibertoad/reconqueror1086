---
id: FND-UI-020
title: Title callbacks replace the current screen and the replacement loader returns after its history update
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B7C0..0x0005B80A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005BC28..0x0005BC53
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005BC54..0x0005BC7F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000596C0..0x0005975F
tool: Capstone 5.0.7, bounded x86-32 decoding of the fingerprinted relocated LE image
environment: null
---

## Observation

The title setup in FND-UI-003 binds region zero's slot 2 to `0x0005BC28`
and slot 3 to `0x0005BC54`. Each callback reads `0x0009DBBC`. If that value
is nonzero, it passes that value and the additional arguments 4, 0 and 32767
to `0x0005B3B0`. Whether or not that call is made, it then passes screen 1,
draw 1 and mode 1 to `0x000596C0`. It drops the three arguments and returns
after the replacement call. The setup bindings and both callback paths agree
with SCR-UI-001's transition to game options; this finding does not read the
sound service's effects or establish physical input dispatch.

The replacement loader at `0x000596C0` saves three registers. It reaches the
current object through the record at `0x000AFE50`, calls that object's callback
at offset 180, then passes the current object pointer to `0x0005A180`.
It stores that callee's return at record offset zero without testing it. It
then passes the requested screen's registered filename and its mode argument
to the HAT loader of FND-UI-002, stores that returned object pointer at record
offset zero, and calls the requested screen's registered setup routine.

After the three saved registers, the requested screen, draw and mode are at
stack offsets 16, 20 and 24. When draw is nonzero it calls `0x00024DB8` with
the mode, minus one and the current object pointer. Both draw paths then reach
the current object through the record again and call its callback at offset
172. The loader passes its original requested screen argument to the history
routine `0x00059CE4` described in FND-UI-017. If draw is nonzero it next calls
`0x00063270` with 1 and 1. Both paths join at `0x0005975B`, restore the three
registers and return at `0x0005975E`.

Unlike the initial loader of FND-UI-017, this path does not allocate or clear
the five-slot history record before replacing its object. It does not make
the history-push call before the requested setup and entry callback have run.
The old object's callback, object cleanup, HAT loader, requested setup, entry
callback and optional drawing services are not completely read here.

## Interpretation

The replacement return is a distinct observation boundary from the initial
loading return. Its request can be read at entry and associated with the
same caller stack position at return. The returned record, current object
identifier and history head can then be checked against the requested screen,
using the layouts of FND-UI-002 and FND-UI-017. Reaching the title callback or
replacement entry alone does not prove that the requested screen is loaded.

## Alternatives

An unconditional replacement before either callback's optional sound call is
ruled out by their call order. Reusing the initial loader's return address for
replacement is ruled out by the two separate functions and epilogues.
Callbacks may alter the record, object or history, and nested loading may occur;
the direct stores alone do not prove their returned identities or rule out
recursion. Controlled observations must verify those identities and frames.

## How to reproduce

Load the fingerprinted BLD-GOG-EN LE objects with internal relocations applied.
Decode the listed exclusive ranges in 32-bit mode. Track argument order through
the callback calls and all replacement-loader cleanups, and enumerate the two
draw branches through their shared return. Read FND-UI-001 for registration,
FND-UI-002 for object layout and FND-UI-017 for the shared history update.
