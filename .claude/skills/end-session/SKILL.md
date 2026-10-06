---
name: end-session
description: Close a work session on this restoration - audit processes, release a run lock the session owns, rewrite the handover, commit completed work, and report. Push only when explicitly requested by the owner.
---

# End a session

The rules are in the [work protocol](../../../docs/upstream/work-protocol.md#sessions) (lines 321-329).
Open a linked section only when a step leaves a question it answers, read
only the lines the link gives, and never a section already read this session.

1. **Processes**: stop every process this session started (Ghidra and Java,
   the original game, test hosts, servers), and leave anything whose owner is
   uncertain. Reusable MSBuild nodes are not orphans. Delete the run lock
   (path in `docs/RUNTIME.md`) if this session created it, and never
   otherwise. Follow any process-audit rule in `AGENTS.md`.
2. **Goal**: if the goal's condition holds, or the goal is dropped, the
   handover commit (step 4) deletes its file in `docs/goals/` instead of
   rewriting it, says which in the commit message, and moves anything in its
   Handover still worth handing on (a blocker, a `wip/` branch) to
   `docs/handover.md`, leaving the rest of that file as it was. The batch that
   met the condition never deletes it. If the goal continues, add any new
   dead end to its file.
3. **Working tree**: every finished batch is already committed. For anything
   half done, finish it, discard it, or leave it out of the batch commits and
   describe it under Unfinished in the handover. Where the working tree does
   not outlive the session (a cloud container), commit the half-done work to
   `wip/<working branch>` and push it there instead. Never commit to the
   working branch anything that fails the documentation check or the fast
   gate, or mixes two kinds of batch.
4. **Handover**: under a goal, rewrite the Handover section of its goal file;
   otherwise rewrite `docs/handover.md` from its section headings, after
   merging any goal-deletion commit on main that added to it and keeping what
   that commit added unless this session dealt with it. State what
   is true now: stage, the last gate result with its date, unfinished work
   (with its `wip/` branch), blockers, and at
   most five next items naming queue items by ID, parity rows or a slice.
   Get branch, commit and remote sync state from Git when needed; do not copy
   them into the handover. Never write what research found or tried. Delete
   what is no longer true instead of adding below it. Stay under 200 lines.
   Commit the handover on its own, with no trailers: it is not a batch.
5. **Push** the branch only when the owner explicitly requests it. Check the
   canonical configured push destination as `AGENTS.md` requires. Check Git directly for the branch's remote sync
   state when reporting it; do not copy a count into the handover.
6. **Report** the final status block from `research-item`, followed by one
   line on anything the owner has to decide or do, such as a live session
   request waiting for an answer.
