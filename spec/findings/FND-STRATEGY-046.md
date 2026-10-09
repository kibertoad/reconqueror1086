---
id: FND-STRATEGY-046
title: The home-route branch uses the selected route without a seven-home clamp
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004A070..0x0004A2FD
tool: Capstone 5.0.7 over the fingerprint-verified relocated LE code object
environment: null
---

## Observation

The route constructor saves three registers and reserves 92 stack bytes.
Its record, origin, target and route-kind arguments are then at stack offsets
108, 112, 116 and 120 respectively. Only route-kind equal to 1 enters the
property-pair branch; every other value enters the home-route branch.

At `0x0004A173` the home branch reads the dword `start_home`, then reads the
route-name pointer at `start_routes + 4 * start_home`. No range check, clamp
or subtraction intervenes. FND-STRATEGY-043 bounds a newly drawn index to
0 through 7, and FND-STRATEGY-044 identifies the corresponding route names.
Thus a newly drawn index 7 selects `sc_7.rat` on this branch. It does not
select the ninth slot, wrap to slot 0 or become a property-pair index.

The branch formats that name into its stack buffer, requests a temporary
buffer through `0x00063D20`, looks up the formatted name through
`0x0004957C`, and passes the lookup result and temporary buffer to
`0x000495F4`. It sets its local pair index and the record's dword `+0x10`
to zero before joining the shared route-copy path. Unlike the property-pair
branch, it does not test the lookup result before passing it onward.

The shared path reads the first temporary-buffer dword as the count,
requests a route buffer using the count shifted left three in a 32-bit
register, and stores its pointer through the record's `+0x6C`. Starting at
temporary-buffer dword 1, it copies two dwords per iteration to successive
eight-byte positions. The home branch's zero `+0x10` skips the reverse-copy
path. It finally writes zero to `+0x30` and `+0x0C`, the count to `+0x18`,
frees the temporary buffer and returns 1.

## Interpretation

For the home branch, selector 7 remains the eighth route throughout name
selection and enters the forward-copy path. This adds a direct consumer
reading to FND-STRATEGY-044; it does not settle every consumer named by
Q-STRATEGY-043.

## Alternatives

No successful resource read or allocation is inferred from the caller's
unconditional progress. The allocator's units, headers and writable capacity,
the resource reader's failure behavior, and the count and loop arithmetic's
behavior on malformed input remain outside this bounded observation.
The equality test also means that FND-STRATEGY-007's description of flag 0
names one home-branch input, rather than proving other non-1 inputs unreachable.
No complete reading is claimed.

## How to reproduce

Verify the shipped executable hash from BLD-GOG-EN, decode its LE objects and
fixups, and inspect the stated code interval. Follow the non-1 branch from
`0x0004A08C` through `0x0004A173`, the zero pair-index and direction writes,
and the shared copy and return path. Resolve route slot 7 using the independent
shipped-table and name verification in FND-STRATEGY-044.
