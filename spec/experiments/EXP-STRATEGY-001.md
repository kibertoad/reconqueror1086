---
id: EXP-STRATEGY-001
title: Drawn home indices leave fabricated fief lists unchanged while person 28 initializes them
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11, Unicorn 2.1.4 x86-32, Capstone 5.0.7
starting_state: emulated-call
recording: null
repetitions: 9
fixture: EXP-STRATEGY-001.json
---

## Question

Does the dispatch read as FND-STRATEGY-047 describes: do home indices 0 through
7 leave the fief lists unchanged, while argument 28 reaches the explicit
initialization writes? This distinguishes the drawn index from the selected
person identifier and supplies a positive control for the unchanged cases.

## Setup

Load the fingerprint-verified BLD-GOG-EN LE image with its relocations applied.
Call the function identified by FND-STRATEGY-047 with one argument, each of
0 through 7 and then 28. Each case gets a fresh stack and fresh fabricated
state. The current fief root points to a fabricated record whose `+0x08`
points to a one-element fief pointer list. Its first fief has separate zeroed
`list_34`, `list_30` and `list_2C` buffers, each large enough for every accessed
offset. FND-ESTATE-003 identifies those field paths; no original fief bytes
or saves supply the pointees.

Map that fabricated state as ordinary readable/writable RAM, independently
of the loaded objects. No video memory is mapped, no ports are modeled and
no service, interrupt or import stubs are installed: any such access fails.
There is no window, clock, input or sound device. These paths contain no draw;
RNG state is not varied or used as an input. Execution is confined to the
dispatch, explicit-28 branch and common-return ranges in FND-STRATEGY-047.
Each case must return within 1,000 instructions.

## Procedure

Run each isolated call from the same fabricated state. Compare every byte of
the fabricated region against its initial state for arguments 0 through 7.
For argument 28 compare it against only the dword changes tabulated in
FND-STRATEGY-047. In all cases verify that the current fief-root value is
unchanged and that the memory-write hook reports no writes outside that root
and, for argument 28, the prescribed list fields. A changed list, unexpected
write, instruction path, hardware access or failure to return fails the case.

Install the hash-pinned harness dependencies in a private Python environment.
Set GAME_DIR to the local evidence directory containing the owned
`disc-root/CONQUER.EXE`. Run `tools/emu/verify_home_init.py --output` with an
ignored local report path. The procedure refuses another executable identity,
skips without GAME_DIR, and keeps all loaded code and generated state local.

## Observations

All eight drawn-index cases returned with every fabricated fief byte unchanged.
The root was stored back unchanged. Argument 28 returned after exactly the
tabulated list writes and the unchanged root store. All cases stayed within
the prescribed paths; no stub or hardware access was needed.

## Results

The fixture compares the listed dwords of `fiefs` exactly, with the same zeroed
fabricated lists in each case. All listed values remain zero for arguments
0 through 7; argument 28 produces the values in FND-STRATEGY-047. The full
region and write-domain controls additionally passed locally. No random
distribution or timing result is inferred.

## Conclusion

The isolated dispatch corroborates FND-STRATEGY-047's distinction: substituting
the selected person for the caller's actual index changes the branch and its
writes. This does not establish a full new-game state, list allocation capacity,
every possible input, another caller or later initialization. It does not
promote RULE-STRATEGY-012 or resolve Q-STRATEGY-043.

