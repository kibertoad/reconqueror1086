---
id: RULE-RES-003
title: Kind-2 decoding
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-RES-004]
conflicting: []
split_with: []
related: [RULE-RES-001]
---

## Summary

A kind-2 entry is one LZW stream. Codes start at 9 bits and grow by one bit, up to 14, when the
next free code reaches the current mask. 256 clears the dictionary and 257 ends the stream.

## When it runs

When `read_entry` reads an entry of kind 2 (RULE-RES-001).

## Parameters

`src`, the stored bytes, `stored`, their number, and `out`, the list the output is appended to.

## Inputs

None.

## Procedure

```text
define decode_kind2(src: UINT8[], stored: UINT32, out: UINT8[]):
    # the two tables are allocated and not cleared; a valid stream reads only entries it wrote
    let prefix: UINT16[] = []
    let suffix: UINT8[] = []
    for k in 0..16384:
        append(prefix, 0)
        append(suffix, 0)
    let bitpos = 0
    let width = 9
    let mask = 0x1FF
    let next = 258
    let first_code = 1
    let previous = 0
    let previous_first = 0
    while true:
        # the stream also ends when bitpos / 8 passes stored
        let code = 0
        for k in 0..width:
            code = (code << 1) | ((src[(bitpos + k) / 8] >> (7 - (bitpos + k) % 8)) & 1)
        bitpos = bitpos + width
        if code == 257:
            return
        if first_code != 0:
            append(out, UINT8(code))
            previous = code
            previous_first = UINT8(code)
            first_code = 0
            continue
        if code == 256:
            width = 9
            mask = 0x1FF
            next = 258
            first_code = 1
            continue
        let stack: UINT8[] = []
        let c = code
        if code >= next:
            append(stack, previous_first)
            c = previous
        while c >= 256:
            append(stack, suffix[c])
            c = prefix[c]
        append(stack, UINT8(c))
        let first = UINT8(c)
        let s = count(stack)
        while s > 0:
            s = s - 1
            append(out, stack[s])
        if next <= mask:
            prefix[next] = previous
            suffix[next] = first
            next = next + 1
            if next == mask and width < 14:
                width = width + 1
                mask = (1 << width) - 1
        previous = code
        previous_first = first
```

## Outputs

`out` gains the decoded bytes. The routine returns nothing; `read_entry` returns the expanded size.

## Edge cases

- A code at or above `next` is taken as the previous string followed by the previous string's first
  byte, even when it is above `next`, where the dictionary holds nothing for it yet.
- The first code after the start or a clear is written as a byte with no dictionary entry made, even
  when it is 256 or above.
- The width grows when `next` reaches the mask, one code before it reaches the power of two. At 14
  bits the mask is 16,383, so the last code added is 16,383 and the dictionary then stops growing.
- The bit reader loads whole bytes ahead of the codes it returns and can read up to three bytes past
  `stored`; the loop stops at the end code before those bytes matter in every shipped entry.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

None known.
