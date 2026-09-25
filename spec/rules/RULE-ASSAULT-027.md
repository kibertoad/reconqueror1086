---
id: RULE-ASSAULT-027
title: Combatant and effect values whose layout or computation is not recorded
status: unknown
builds: [BLD-GOG-EN]
superseded_by: []
evidence: []
conflicting: []
split_with: []
related: []
---

## Summary

The rules reach these through named functions until their offsets or procedures are found:

- a combatant's actor kind (`kind_of`, `set_kind`), combat row (`row_of`, `set_row`), heading (`heading_of`, `set_heading`), whether it is in play (`is_live`,
  `set_live`), its own live block (`block_of`), its base block (`base_block_of`), the cell it
  occupies (`cell_x_of`, `cell_y_of`, `set_cell`), and the side the loader derives from a block
  (`side_of_block`);
- the combatant a block belongs to (`combatant_of_block`, which gives -1 for a block that is no
  combatant's);
- whether two combatants are within one cell (`within_one_cell`);
- an effect's owner, tick counter, tick count, interval, flags and steps (`effect_owner`,
  `effect_ticks`, `effect_tick_count`, `effect_interval`, `effect_flags`, `effect_step_x`,
  `effect_step_y`), setting it up (`init_effect`), changing it (`set_effect_steps`,
  `set_effect_ticks_left`), advancing its visual steps by one tick (`advance_effect`), and
  whether it moves an actor (`effect_is_movement`) or has finished (`effect_finished`).

## When it runs

Wherever another ASSAULT rule calls one of the functions.

## Parameters

None.

## Inputs

None known.

## Procedure

None known.

## Outputs

None known.

## Edge cases

None known.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The offsets of kind, template, row, armour, heading, the in-play mark and the block in the
  combatant record (FMT-ASSAULT-001).
- The offsets of the effect record's fields apart from `active`, `deadline`, `moving_block` and
  `covered_block` (FMT-ASSAULT-004).
- How `0x0004CEA0` resolves a block to a combatant.
