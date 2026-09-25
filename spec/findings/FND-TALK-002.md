---
id: FND-TALK-002
title: The conversation loop runs node actions, then the chosen response actions, then follows a redirect
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00019CC0..0x0001A0E5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001A140
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00019CC0(root, name, flag)` opens the action scripts of `name` through `0x0006B0B8` and the node
files, and returns at once when `root` is 0. Each pass stores -1 in `0x000A96D0`, sets a local
response index to 6 and loads node `id` (at first `root`) through `0x000163B8`; a failed load ends
the conversation. When the node's response count (`+0x48`) is 0 it shows the prompt through
`0x0001A14C` and waits through `0x0001ACEC(2000, 0)` if the prompt count (`+0x08`) is not 0, and
takes the next node from `+0x4C`. Otherwise it shows the prompt, then calls `0x0001A3B8` until the
result is at least 0 and below the response count, and takes the next node from `+0x4C + 4 *
response`. Then it runs the 30 dwords at `+0x2B8` that are not -1 through `0x0006B28C(scripts, id)`
in order, and, when the response index is below 5, the 30 dwords at `+0x60 + 0x78 * response` the
same way. When `0x000A96D0` is not -1 afterwards, it replaces the next node. It calls `0x0001ABCC`
and frees the node through `0x00017CA8`, and repeats while the next node is not 0, then prints
"ENDING CONVERSATION...". `0x0001A140(x)` stores `x` in `0x000A96D0`.

## Interpretation

A node with no responses is a statement that continues on its own after two seconds; the response
index stays 6, so only the node actions run. Actions run after the choice is made, node actions
first, and a redirect set by either overrides the target the node gives. Node 0 ends the talk.

## Alternatives

What `0x0001ACEC`, `0x0001ABCC` and `0x0001A3B8` do beyond the waits and the input were not traced.

## How to reproduce

Run `tools/Conqueror.Inspect` against the installation with `--disassemble=ADDR --executable-only` for each address listed, and read the report.
