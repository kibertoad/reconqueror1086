# SOUND

Next ID: Q-SOUND-009

## Static

- Q-SOUND-001. BUG-SOUND-001: How the driver treats a step of 0? Settles it: trace the cited
  path and its callers through the boundary case; distinguish executable behavior from runtime
  or driver assumptions. Blocks: none.

- Q-SOUND-003. RULE-SOUND-001: How does `fn_0007AB20` implement DOS-memory acquisition and
  failure? Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-SOUND-004. RULE-SOUND-002: How do the HMI voice-start, stop and completion routines
  implement their status and handle results? Settles it: read the relevant branch and its
  callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.

- Q-SOUND-005. RULE-SOUND-002: What `a` means? Settles it: read the relevant branch and its
  callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.

- Q-SOUND-006. RULE-SOUND-003: What do the HMI calls inside digital sound, MIDI initialization
  and music control do? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-SOUND-007. RULE-SOUND-004: How `cd_track_starts`, `cd_track_count` and `cd_adjust` are
  filled, and the address conversion the game does with `0x00083BB0` and `0x00083C10`? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-SOUND-008. RULE-SOUND-004: What sets `cd_paused`? Settles it: read the relevant branch and
  its callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

- Q-SOUND-002. BUG-SOUND-001: Is it true that the samples were meant to be 11,025 Hz? Settles
  it: a contemporary design note, erratum or author statement addressing the intent; executable
  behavior alone does not prove intent. Blocks: none.

## Blocked

None.
