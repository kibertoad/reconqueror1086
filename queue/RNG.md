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
