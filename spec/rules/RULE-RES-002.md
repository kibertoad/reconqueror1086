---
id: RULE-RES-002
title: Kind-1 decoding
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-RES-003]
conflicting: []
split_with: []
related: [RULE-RES-001]
---

## Summary

A kind-1 entry is decoded block by block. A block marked `0x80` is copied; any other block is read
as tokens under 16-bit control words: literals, back-references of 3 to 18 bytes into the block's
own output, and runs of 16 to 4,111 bytes of one value.

## When it runs

When `read_entry` reads an entry of kind 1 (RULE-RES-001).

## Parameters

`src`, the stored bytes, `stored`, their number, and `out`, the list the output is appended to.

## Inputs

None.

## Procedure

```text
define decode_kind1(src: UINT8[], stored: UINT32, out: UINT8[]) -> UINT32:
    let pos = 0
    let total = 0
    while pos < stored:
        let length = src[pos] | (src[pos + 1] << 8)
        total = total + decode_kind1_block(src, pos + 2, length, out)
        pos = pos + 2 + length
    return total

define decode_kind1_block(src: UINT8[], start: UINT32, length: UINT16, out: UINT8[]) -> UINT16:
    let base = count(out)
    if src[start] == 0x80:
        for i in 1..length:
            append(out, src[start + i])
        return length - 1
    # src[start + 1] is never read
    let control: UINT16 = (src[start + 2] << 8) | src[start + 3]
    let bits = 16
    let i: UINT16 = 4
    let written: UINT16 = 0
    while i < length:
        if bits == 0:
            control = (src[start + i] << 8) | src[start + i + 1]
            i = i + 2
            bits = 16
        if (control & 0x8000) == 0:
            append(out, src[start + i])
            i = i + 1
            written = written + 1
        else:
            let a = src[start + i]
            let b = src[start + i + 1]
            i = i + 2
            let distance = (a << 4) | (b >> 4)
            if distance != 0:
                let n = (b & 0x0F) + 3
                for k in 0..n:
                    append(out, out[base + written - distance + k])
                written = written + n
            else:
                let n = (b << 8) + src[start + i] + 16
                let v = src[start + i + 1]
                i = i + 2
                for k in 0..n:
                    append(out, v)
                written = written + n
        control = control << 1
        bits = bits - 1
    return written
```

## Outputs

`out` gains the decoded bytes; `decode_kind1` returns their number.

## Edge cases

- The block decoder stops when its input position reaches `length`, not after a number of output
  bytes; the shipped blocks end exactly after their last token.
- `written` and the input position are 16-bit counts. A back-reference is taken from `out` at
  `base + written - distance`, so a distance larger than `written` would read the end of the
  previous block, which is in `out` before `base`. No shipped block does this.
- The control word is refilled only when a token is about to be read, so a block that ends right
  after its 16th token has no further control word.
- Nothing checks that the decoded bytes fit the output buffer or add up to the expanded size.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

None known.
