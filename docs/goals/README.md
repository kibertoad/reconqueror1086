# Goals

One file per long-running goal while it runs, named after it:
`docs/goals/combat-static.md`. The
[work protocol](../upstream/work-protocol.md#coding-agents-and-long-running-goals) (lines 446-497)
says how to write the condition. The batch whose work meets the goal leaves
the file in place, and a later commit deletes it: the session's handover commit
for its own goal, or a commit of its own for a goal dropped between sessions
or met by an earlier batch. That commit moves what is still worth handing on
to `docs/handover.md`. Commits that create a goal file, change its claim or
delete it are not batches: they change only `docs/goals/` and
`docs/handover.md`, keep the documentation check and fast gate passing, and
carry no trailers. The files here are the list of goals running.

A goal file:

```markdown
# combat-static

## Condition

Every item under Static in queue/COMBAT.md is closed or moved to Blocked with
what was tried, the documentation check passes on the last commit, and each
batch ended with a status block; or stop after 40 turns.

## Scope

Areas: COMBAT. Batches: research only. Queue sections: Static.

## Must not touch

Other areas' entries, queue files and parity rows. `src/`.

## Dead ends

None known.

## Handover

- Stage: Slices.
- Last gate: 2026-09-25, documentation check passed, fast gate passed.
- Unfinished: none.
- Blockers: none known.
- Next: Q-COMBAT-015, Q-COMBAT-017.
```

`Scope` names the areas the goal claims. A goal takes up a queue item only if
every entry it names is in one of those areas, and adds an area to its scope
only while no other goal file claims it. Where the session is authorized to
push to main, the file, and every change to its scope, reaches the main branch
before the first batch that relies on it, so every session sees the claim.

Where it is not (this repository's sessions push only when the owner asks for
it in the task), only one goal runs at a time and the claim lives in the shared
clone. The goal works on a branch `goal/<file name without .md>` whose first
commit creates the goal file. At the start of every session, and again before
starting or resuming a goal, run `git branch --list 'goal/*'` and look in
`docs/goals/` at each listed branch's tip: a branch whose tip still has its
goal file is the running goal, and no other goal starts while it is there. A
session that starts a goal lists the branches again after that first commit
and, if another listed branch has its goal file at the tip, deletes its own
branch and starts no goal. Deleting the goal file on the branch ends the
claim. A copy of a goal file that reaches main through a merge claims nothing
there. A session outside a worktree of the shared clone starts no goal unless
the person running it says none is running. The
[protocol](../upstream/work-protocol.md#coding-agents-and-long-running-goals) (lines 446-497)
gives the details. `Dead ends` records tools and approaches that failed across the
whole goal, in a line or two each, so a resumed session does not repeat them;
what a research attempt tried on a question goes under its queue item's
`Tried:`. `Handover` holds what `docs/handover.md` holds, for this goal only,
and is rewritten at the end of every session under the goal. Progress is not
written here: the queue and the commits show it.
