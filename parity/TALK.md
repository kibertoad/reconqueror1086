# TALK

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-TALK-001` | Conversation index record, ALL.CIF | supported | complete | None | None | implemented | `DynamixConversationDecoder` reads the index into a dictionary and also checks that ids are unique and offsets bounded. |
| `FMT-TALK-002` | Conversation node, one record of ALL.CBF | supported | complete | None | None | implemented | `DynamixConversationDecoder` splits the strings at NULs and drops empty ones, where the original reads them by the header lengths; it also rejects any record whose marker bytes differ. |
| `FMT-TALK-003` | Action index, ALL.TMI | supported | complete | None | None | implemented | `DynamixActionTreeDecoder` keeps the skipped dword as `IndexHeaderValue`. |
| `FMT-TALK-004` | Action group, a record of ALL.TMB | supported | complete | None | None | implemented | `DynamixActionTreeDecoder`. |
| `FMT-TALK-005` | Action, a record of ALL.TMB | supported | complete | None | None | implemented | `DynamixActionTreeDecoder` rejects a branch count that does not match the kind when it loads, where the original fails the action when it runs. |
| `FMT-TALK-006` | Expression, a record of ALL.TMB | supported | complete | None | None | implemented | `DynamixActionTreeDecoder`. |
| `FMT-TALK-007` | Value, a record of ALL.TMB | supported | complete | None | None | implemented | `DynamixActionTreeDecoder` accepts only the flags -1 and 1. |
| `FMT-TALK-008` | Conversation variables, ALL.VTB | supported | partial | None | None | supported | `DynamixVariableTableDecoder` accepts only 4-byte variables of kind 5. |
| `RULE-TALK-001` | Conversation walk | supported | partial | None | None | supported | `ImportedConversationSession` runs node actions on arrival and follows a redirect before showing the node; prompt variants come from `Random.Shared`, without the reseed. |
| `RULE-TALK-002` | Action-tree interpreter | supported | partial | None | None | supported | `DynamixActionInterpreter` evaluates as the original does but never fails an action; an unknown function reads 0. |
| `RULE-TALK-003` | Script functions and conversation variables | supported | partial | None | `DEV-TALK-001` | supported | The seven selectors match; unmapped selectors keep values of their own, attribute writes clamp to 0 to 20 (wealth to 0 or more), an INT_MIN assignment to an attribute is refused, and an add is not checked for INT_MIN. |
| `RULE-TALK-004` | Conversation entry and its aftermath | supported | partial | None | None | supported | Roots come from per-place definitions instead of the partner table; the inn gate, the debt copy and the marriage variables are not applied. |
| `RULE-TALK-005` | Joust result for the conversations | supported | partial | None | None | supported | Variable 3 is written only while the player wears a lady's colours, and variable 42 is read as those colours. |
