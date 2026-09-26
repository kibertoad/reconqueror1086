---
id: RULE-SOUND-002
title: Playing a sample
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-SOUND-003, FND-SOUND-004, FND-SOUND-002, FND-RNG-003]
conflicting: []
split_with: []
related: [RULE-SOUND-001, RULE-RNG-001, FMT-SOUND-001]
---

## Summary

A sample of a loaded bank starts on the first free of ten voices; when all are busy a random voice
is cut off. Its rate becomes a step of the 22,050 Hz output for three known rates only.

## When it runs

On most clicks of hotspots and buttons (offset 4, 141 calls) and on scene events; 192 calls in all.

## Parameters

- `bank`: a `sound_bank`.
- `offset`: the offset of the sample's `length` field in the bank, FMT-SOUND-001.
- `a`: stored at `+0x0C` of the request; 0 in 172 calls.
- `b`: stored at `+0x14` of the request; `0x7FFF` in 177 calls, most likely the volume.

## Inputs

`digital_ready`, `sound_effects_on`, `voice_handles`, `voice_requests`, the game's random generator.

## Procedure

```text
define play_sample(bank: sound_bank, offset: UINT32, a: INT32, b: INT32) -> INT32:
    if digital_ready == 0 or sound_effects_on == 0:
        return 0
    let p = bank.data + offset
    let length = UINT32 at p
    let rate = UINT32 at p + 4
    let v = 0
    while v < 10:
        if voice_handles[v] == 0 or fn_0006E0F3(voice_handles[v]) != 0:
            break
        v = v + 1
    if v == 10:
        v = random_inclusive(9)
        if fn_00064BCF(voice_handles[v]) != 0:
            # the game stops with "Error : " and the driver's message
            return 0
    let r = voice_requests[v]
    r.data = p + 8
    r.length = length
    r.a = a
    r.b = b
    if rate == 11025:
        r.step = 0x8000
    else if rate == 22050:
        r.step = 0x10000
    else if rate == 44100:
        r.step = 0x20000
    # any other rate keeps the step of the voice's previous sample (BUG-SOUND-001)
    voice_handles[v] = fn_000645C0(r)
    return voice_handles[v]
```

## Outputs

The voice handle, or 0. A sound from the speakers.

## Edge cases

- No bounds check: an offset past the bank reads whatever follows it.
- The random voice uses the game's generator, so a busy mixer changes later random results.
- A bank of 0 is never read, since both flags are 0 whenever banks are not loaded.

## What the sources say

The executable (FND-SOUND-003); the shipped banks (FND-SOUND-001).

## Differences between builds

None known.

## Open questions

- `fn_0006E0F3` returns non-zero when the voice has finished; `fn_00064BCF` stops a voice and
  returns an error code; `fn_000645C0` starts a request and returns its handle. All three take the
  driver handle at `0x0009DAB0` first; their insides were not traced.
- What `a` means.
