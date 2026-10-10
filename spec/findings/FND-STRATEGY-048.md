---
id: FND-STRATEGY-048
title: Home selection links the selected person twice, with the second call refusing a duplicate
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
    address: 0x00043248..0x000432B6
tool: Capstone 5.0.7 over the fingerprint-verified relocated LE code object
environment: null
---

## Observation

After the initialization call distinguished in FND-STRATEGY-047, the home
selector retains the selected person in ESI. It computes that person's
18-byte record address, reads the two word coordinates and writes them as
zero-extended dwords through its two caller-supplied pointers. It clears the
person's assignment byte, then pushes ESI and calls `0x00043248`. It removes
that argument, pushes ESI again, stores ESI in `place_person`, and calls the
same function again. After cleanup it returns ESI in EAX. Both callee return
values are ignored; neither call replaces ESI.

The link function reads its argument as a dword and loads `person_list_head`.
If the head equals 255, it sets the head to the argument, stores 255 in that
person's byte `next`, and returns 1. Otherwise it searches the existing chain,
comparing each person index with the argument. A match returns zero before
any write. It follows each record's one-byte `next` after multiplying the
current index by 18, zero-extending the next index; 255 ends the search.
If the argument was absent, it writes the old head's low byte into the new
person's `next`, replaces the dword head with the argument, and returns 1.

The search's first equality jump after copying the nonempty head inherits
the earlier head-versus-255 flags: the copy does not change flags, so that
jump cannot bypass the first comparison on this path. Subsequent iterations
return directly to the argument comparison or reach insertion after testing
the terminator.

For drawn home index 7, FND-STRATEGY-044 identifies ESI as person 28.
Both link calls therefore receive 28, unlike the earlier initialization
call's index argument. With a well-formed list, the first call inserts 28
or finds it already present; the immediately repeated call returns zero
without changing that list. It makes no call or random draw. The selected
person, rather than the drawn home index, is returned to new-game setup.

## Interpretation

The endpoint remains person 28 through coordinate, assignment, place and
person-link handling. The duplicate call is not a second list entry and does
not clamp the eighth home. FND-STRATEGY-045 describes the setup's subsequent
stores; this observation adds the local link behavior to Q-STRATEGY-043.

## Alternatives

The link function has no index bound or malformed-chain guard. A cyclic chain
that does not contain the argument need not terminate, and an invalid index
may address storage outside the person records. This reading does not establish
that all callers supply well-formed chains, nor that every later consumer
accepts the selected person's group. No complete reading is claimed.

## How to reproduce

Verify BLD-GOG-EN and its LE relocation mapping. Decode both listed exclusive
ranges, track ESI and every argument push and cleanup across the home selector,
and follow the empty, duplicate and nonempty-absent link paths. Compare record
stride and link/head fields with FND-STRATEGY-040 and their glossary entries.
