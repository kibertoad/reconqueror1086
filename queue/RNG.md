# RNG

Next ID: Q-RNG-002

## Static

- Q-RNG-001. RULE-RNG-001: What `fn_0006B3B4` returns? Settles it: read the relevant branch and
  its callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.
  Tried: FND-RNG-005 reads the DOS calendar/time reader, rounding and converter
  wrapper; the calendar tables, adjustment initialization and record-offset-32
  write remain for the next static reading. No complete timestamp claim yet.
  FND-RNG-006 follows the environment lookup and parser wrapper, including
  shipped adjustment values. The parser syntax, environment initialization,
  record normalization and offset-32 classification still require reading.
  FND-RNG-007 and EXP-RNG-002 cover the name/adjustment helper, missing-number
  preservation, clipped names and wrapping numeric components. Suffix syntax,
  environment initialization, calendar tables and record normalization remain.
  FND-RNG-008 and EXP-RNG-003 cover the suffix parser's selectors, partial
  record writes, default clock components and remaining input. Its calendar
  classification consumers and their initialization still need completion.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

None.

## Blocked

None.
