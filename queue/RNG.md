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
  FND-RNG-009 records the common/leap cumulative month tables through their
  shipped file locations, with the previously recorded environment key as
  a positive control. Classification, normalization and initialization remain.
  FND-RNG-010 and EXP-RNG-004 cover bounded epoch arithmetic and normalization
  with a null environment array and classification zero. The negative-
  classification path, environment initialization and broader normalizer domain
  remain; a general Gregorian timestamp claim would contradict these controls.
  FND-RNG-011 reads the lazy environment constructor and two direct-reference
  searches with a positive control. The source selector/offset's writers,
  constructor startup ordering and allocator/segment effects remain unread.
  FND-RNG-012 identifies both startup source stores and the initializer-table
  dispatch route, including conditional priority and tie order. Incoming source
  values, OS selector behavior and other callback effects still prevent complete
  startup environment provenance; allocator and classification paths remain.
  EXP-RNG-005 corroborates the dispatcher with explicit preserving callback
  stubs. Actual callbacks and source provenance remain unread.
  FND-RNG-013 and EXP-RNG-006 cover the empty-name classification exit and
  converter's negative/zero/positive stored-field tests. Nonempty-name rules,
  actual startup inputs and the broader normalizer domain remain unresolved.
  FND-RNG-013 and EXP-RNG-006 cover the empty-name classification exit and
  converter's negative/zero/positive stored-field tests. Nonempty-name rules,
  actual startup inputs and the broader normalizer domain remain unresolved.

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
