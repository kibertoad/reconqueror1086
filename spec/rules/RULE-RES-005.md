---
id: RULE-RES-005
title: Expanding a SETUP.SOL member's stream
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-RES-030, FND-RES-031]
conflicting: []
split_with: []
related: []
---

## Summary

How SETUP turns a member's stored stream in SETUP.SOL (FMT-RES-017) into the
file it writes: an LZ77 stream with fixed prefix codes, read a bit at a time
from the low end of each byte, in the form PKWARE's Data Compression Library
calls binary mode. Mode 1 is accepted but not described here.

## When it runs

When SETUP extracts a marked member from SETUP.SOL (FND-RES-030).

## Parameters

- `src`: the member's stored bytes, `stored_size` of them; reading stops
  there.
- `out`: the bytes produced, empty at the start; the procedure appends to it.

## Inputs

None.

## Procedure

`pos` counts the bits of `src` already taken, bit 0 of a byte coming first.
Taking `n` bits fails when `pos + n` passes the end of `src`; reading bits
beyond the end without taking them gives zero bits.

```text
define sol_bits(src: UINT8[], pos: UINT32, n: UINT8) -> UINT16:
    # the n bits from pos, the first in bit 0; bits past the end read as 0
    let value: UINT16 = 0
    for k in 0..n:
        let at: UINT32 = pos + k
        if at / 8 < count(src):
            value = value | (((src[at / 8] >> (at % 8)) & 1) << k)
    return value

define sol_lookup(codes: UINT8[], widths: UINT8[], low8: UINT16) -> UINT8:
    # the codes are prefix-free, so one index matches
    for i in 0..count(codes):
        if (low8 & ((1 << widths[i]) - 1)) == codes[i]:
            return i
    return 0

define expand_sol_member(src: UINT8[], out: UINT8[]) -> UINT16:
    let len_bits: UINT8[16] = [3, 2, 3, 3, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 7, 7]
    let len_code: UINT8[16] = [0x05, 0x03, 0x01, 0x06, 0x0A, 0x02, 0x0C, 0x14, 0x04, 0x18, 0x08, 0x30, 0x10, 0x20, 0x40, 0x00]
    let extra_bits: UINT8[16] = [0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8]
    let len_base: UINT16[16] = [0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 14, 22, 38, 70, 134, 262]
    let dist_bits: UINT8[64] = [
        2, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 6, 6,
        6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8
    ]
    let dist_code: UINT8[64] = [
        0x03, 0x0D, 0x05, 0x19, 0x09, 0x11, 0x01, 0x3E, 0x1E, 0x2E, 0x0E, 0x36, 0x16, 0x26, 0x06, 0x3A,
        0x1A, 0x2A, 0x0A, 0x32, 0x12, 0x22, 0x42, 0x02, 0x7C, 0x3C, 0x5C, 0x1C, 0x6C, 0x2C, 0x4C, 0x0C,
        0x74, 0x34, 0x54, 0x14, 0x64, 0x24, 0x44, 0x04, 0x78, 0x38, 0x58, 0x18, 0x68, 0x28, 0x48, 0x08,
        0xF0, 0x70, 0xB0, 0x30, 0xD0, 0x50, 0x90, 0x10, 0xE0, 0x60, 0xA0, 0x20, 0xC0, 0x40, 0x80, 0x00
    ]
    if count(src) <= 4:            # the first read is up to 2048 bytes
        return 3
    let mode = src[0]
    let size = src[1]
    if size < 4 or size > 6:
        return 1
    if mode == 1:
        return 5             # not described (see Open questions)
    if mode != 0:
        return 2
    let mask: UINT16 = 0xFFFF >> (16 - size)
    let limit: UINT32 = 8 * count(src)
    let pos: UINT32 = 16
    while true:
        if pos + 1 > limit:
            return 4
        let flag = sol_bits(src, pos, 1)
        pos = pos + 1
        if flag == 0:
            if pos + 8 > limit:
                return 4
            append(out, sol_bits(src, pos, 8))
            pos = pos + 8
            continue
        let i = sol_lookup(len_code, len_bits, sol_bits(src, pos, 8))
        if pos + len_bits[i] > limit:
            return 4
        pos = pos + len_bits[i]
        let symbol: UINT16 = 0x100 + i
        if extra_bits[i] != 0:
            if pos + extra_bits[i] > limit:
                return 4
            symbol = 0x100 + len_base[i] + sol_bits(src, pos, extra_bits[i])
            pos = pos + extra_bits[i]
        if symbol >= 0x305:
            return 0
        let length = symbol - 0xFE
        let j = sol_lookup(dist_code, dist_bits, sol_bits(src, pos, 8))
        if pos + dist_bits[j] > limit:
            return 4
        pos = pos + dist_bits[j]
        let low: UINT8 = size
        if length == 2:
            low = 2
        if pos + low > limit:
            return 4
        let distance = ((j << low) | (sol_bits(src, pos, low) & mask)) + 1
        pos = pos + low
        for k in 0..length:
            # a copy from before the first byte gives 0
            if count(out) >= distance:
                append(out, out[count(out) - distance])
            else:
                append(out, 0)
```

The result 5 marks where the mode 1 path begins; this entry does not
describe it, and SETUP itself never gives 5. For a length of 2 the mask
leaves the 2 bits whole, since `size` is at least 4.
Any failure to take bits inside a symbol or a distance ends the stream with
4, keeping the bytes already produced.

## Outputs

The bytes appended to `out`, and a result number: 0 at the end symbol, 1 for
a bad dictionary size, 2 for an unknown mode, 3 for a stream of 4 bytes or
fewer, 4 when the input runs out inside a symbol or a distance. SETUP writes the bytes
to the member's file in 4,096-byte pieces as they are produced, and the rest
at the end; it counts a member as extracted only for result 0, and stops the
extraction at the first other result (FND-RES-030).

## Edge cases

- A distance larger than the output so far copies zero bytes from the
  zero-filled window, with no error.
- `stored_size` bounds the input, so a stream with no end symbol ends with 4.
- The largest symbol the length codes can give is 0x305, the end: index 15
  with all 8 extra bits set.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- How does mode 1 read literals? SETUP builds further tables for it from
  CS:0x3601 through 0001:45CF and reads them from 0001:4417 to 0001:44A8;
  no shipped member uses mode 1. (Q-RES-177)
