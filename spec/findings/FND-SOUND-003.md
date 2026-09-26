---
id: FND-SOUND-003
title: Samples play through 0x0005B3B0 on ten voices, with rate factors for 11,025, 22,050 and 44,100 Hz only
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B3B0..0x0005B551
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009D628
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009DB80
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0005B3B0(bank, offset, a, b)` returns 0 at once unless both `0x0009DBAC` (digital sound
started) and `0x0009ADBC` (`SOUND_EFFECTS`) are non-zero. `bank` is a record of `0x0005B584`; the
routine reads the `UINT32` length at `data + offset` and the `UINT32` rate after it, and the sample
bytes start at `data + offset + 8`. It checks nothing else, not even the `0x004A5031` tag.

It then looks for a voice among the ten handles at `0x0009DB80`: the first whose handle is 0 or for
which `0x0006E0F3(driver, handle)` returns non-zero, `driver` being the dword at `0x0009DAB0`. When
all ten are busy it takes `0x0006B3F1() % 10`, the game's random generator (FND-RNG-003), and stops
that voice with `0x00064BCF(driver, handle)`; a non-zero result prints `Error : ` with the driver's
message and exits with 1.

The voice's request is the 116-byte record at `0x0009D628 + 116 * voice`: `+0x00` the data address
and `+0x04` the data segment, `+0x08` the length, `+0x0C` `a`, `+0x14` `b`, and `+0x38` a rate
factor that it sets to `0x8000` for 11,025 Hz, `0x10000` for 22,050 Hz and `0x20000` for 44,100 Hz.
Any other rate leaves `+0x38` as the voice's last request left it. It starts the request with
`0x000645C0(driver, request, ds)` and stores the returned handle at `0x0009DB80 + 4 * voice`.

A byte scan of object 1 finds 192 calls. 141 pass `offset` 4, the first sample of the bank. The
others pass 0x87B four times and 0xA9FB six times, and 26 more offsets once to four times; every
one lands on the length field of a sample in the census of FND-SOUND-001: `0x87B` is sample 1 of
five banks, 18 offsets fit only `SKirmsnd.666`, 3 only `war.666`, 2 only `joust.666`, 1 only
`utility.666` and 3 only `fiefmgmt.666` (its 11,050 Hz samples 3, 4 and 5). The bank records at `0x0009A8A8` and `0x0009AD04`, both loaded from index `0x16A`
(`fiefmgmt`), are played at offset `0xA9FB` by the calls at `0x0001E5F6` and `0x00034461`, `0x72F8`
at `0x0001E7F1` and `0xEB9A` at `0x00034679`. `b` is `0x7FFF` in 177 calls, `0x3FFF`
twice, `0x1FFF` once and a register in seven. `a` is 0 in 172 calls, 10 twice, 100 three times and
a register in the rest.

## Interpretation

The game mixes up to ten samples at once. The factor at `+0x38` is the step through the sample at a
22,050 Hz output rate (`0x5622` is stored at `0x0009DAC4` when the driver starts, FND-SOUND-004), so a
sample at 11,050 Hz plays with whatever step the voice last used (BUG-SOUND-001). `b` is most likely
a volume with `0x7FFF` as full. Offset 4 is the shared sample of FND-SOUND-001 in the 16 banks that
start with it, so most calls play that one click.

## Alternatives

`a` could be a loop count or a pan; the 116-byte request is laid out by the linked sound library,
whose field names were not recovered. When every voice is busy the sound consumes one value of the
game's generator, so a game with sound on can draw a different sequence than one with sound off.

## How to reproduce

Disassemble `0x0005B3B0..0x0005B551` in `CD:CONQUER.EXE`; scan object 1 for calls to `0x0005B3B0`
and match their offsets against the sample starts of each bank.
