---
id: EXP-STRATEGY-002
title: All eight isolated home selections preserve the chosen person through their actual callees
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11, Unicorn 2.1.4 x86-32, Capstone 5.0.7
starting_state: emulated-call
recording: null
repetitions: 8
fixture: EXP-STRATEGY-002.json
---

## Question

Does every output of the inclusive home draw, especially index 7, survive
the original selector's callees as its selected person and coordinates?
What value does that selector store in `place_person` for new-game setup?
The statically located calls and fields are FND-STRATEGY-043 through
FND-STRATEGY-048, with the RNG functions in FND-RNG-002 and FND-RNG-003.

## Setup

Load the verified BLD-GOG-EN LE image and apply its relocations. Call the home
selector with destination parameters pointing to `home_row` and `home_col`,
as the new-game caller does in FND-STRATEGY-045. Preserve the shipped person
and selection tables. Start with an empty `person_list_head` (255) and a null
current fief root. The default initialization path only reads and restores
that root; it does not dereference it, as EXP-STRATEGY-001 checks. Set the
selected person's `assignment` to 9 and `next` to 10 to make their subsequent
changes observable. Every case starts from a fresh copy of this memory.

Choose the first nonnegative seed below 65,536 producing each modulo-eight
result under RULE-RNG-001 and set `rng_state` to it. The fixture records those
seeds. Execute the actual inclusive helper, raw generator, fief dispatch and
both link calls; there are no substituted callees or service/import stubs.
No ports or video memory are modeled, and any hardware access fails. The
stack is fabricated ordinary RAM. There is no clock, window, sound or input.

## Procedure

Confine execution to the selector and callee ranges read in the cited findings.
Require return within 1,000 instructions. For each seed, require exactly one
raw draw and its expected final state; check `start_home`, the returned person,
`place_person`, `home_row`, `home_col`, cleared assignment and a single selected
person list entry ending in 255. Compare the coordinate destinations with the
selected shipped record's unsigned words. Refuse writes outside those fields,
the unchanged fief root and the fabricated call stack.

Set GAME_DIR to the evidence directory containing the owned
`disc-root/CONQUER.EXE`, then run `tools/emu/verify_home_selection.py --output`
with a local ignored report path and the private hash-pinned interpreter and
harness dependencies. Without GAME_DIR it skips. No native game is started.

## Observations

All eight cases returned. Their home/person pairs were 0/17, 1/18, 2/24, 3/29,
4/86, 5/144, 6/166 and 7/28. Each case made one draw. Each returned person
equaled `place_person` and `person_list_head`; assignment became zero and the
selected record's next link became 255. No write or execution-domain refusal
occurred. All source bytes and transient states remained local.

## Results

The fixture gives the seeds, bounded draw results, coordinates and named end
fields for exact comparison. Index 7 returned person 28 and wrote home row
133 and column 93. The earlier alternatives of clamping or silently changing
that endpoint are contradicted on these paths. These are branch controls,
not samples used to infer a random distribution.

## Conclusion

The isolated calls corroborate the selector's endpoint and place value through
its actual callees. Together with FND-STRATEGY-045 and FND-STRATEGY-048 they
settle Q-STRATEGY-023: new-game setup's home selector stores the selected person
in `place_person`. This does not establish a native full-game state, resource
loading, all later consumers, malformed lists or every caller. The remaining
endpoint review stays under Q-STRATEGY-043 and the rule remains disputed.
