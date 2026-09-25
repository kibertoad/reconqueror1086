---
id: FND-VIEW-003
title: The heading helper folds the vector into octants around floor(0x20 * minor / major), and callers pass (-dy, dx)
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000445B4..0x000446AA
tool: Ghidra 12.1.3
environment: null
---

## Observation

Routine `0x000445C4` takes a vector `(a, b)` and returns a byte heading (256 steps a turn). It
returns 0 for `(0, 0)`. Otherwise it compares `abs(a)` and `abs(b)`, divides the smaller by the
larger after multiplying it by `0x20`, and adds or subtracts that from 0, `0x40`, `0x80`, `0xC0`
or `0x100` according to the signs and which component is larger:

| `b` | `a` | Larger | Result |
|---|---|---|---|
| `>= 0` | `>= 0` | `abs(a) > abs(b)` | `abs(b) * 0x20 / abs(a)` |
| `>= 0` | `>= 0` | otherwise | `0x40 - abs(a) * 0x20 / abs(b)` |
| `>= 0` | `< 0` | `abs(a) >= abs(b)` | `0x80 - abs(b) * 0x20 / abs(a)` |
| `>= 0` | `< 0` | otherwise | `0x40 + abs(a) * 0x20 / abs(b)` |
| `< 0` | `< 0` | `abs(a) > abs(b)` | `0x80 + abs(b) * 0x20 / abs(a)` |
| `< 0` | `< 0` | otherwise | `0xC0 - abs(a) * 0x20 / abs(b)` |
| `< 0` | `>= 0` | `abs(a) < abs(b)` | `0xC0 + abs(a) * 0x20 / abs(b)` |
| `< 0` | `>= 0` | otherwise | `(0x100 - abs(b) * 0x20 / abs(a)) & 0xFF` |

Its callers in the actor code pass the map difference `(dx, dy)` as `(a, b) = (-dy, dx)`.

## Interpretation

Heading 0 points to decreasing y (north on the map), and headings grow clockwise. The heading
is an approximation of the angle, linear in the ratio within each octant.

## Alternatives

None known.

## How to reproduce

Open `0x000445C4`; the multiplication by `0x20` and the division appear in each branch. The
callers at `0x0004F98D` and `0x0004FE76` negate the y difference before the call.
