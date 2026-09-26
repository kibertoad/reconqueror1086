---
id: FND-SOUND-001
title: The release holds 26 .666 sound banks with 102 unsigned 8-bit samples at 11,025, 11,050 and 22,050 Hz
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A457CB..0x1A4B69C
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A4B69D..0x1A64642
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A64643..0x1A64B6B
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A64B6C..0x1A65094
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A65095..0x1A655BD
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A655BE..0x1A65AE6
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A65AE7..0x1A6A9AA
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A6A9AB..0x1A77BF4
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A77BF5..0x1A7811D
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A7811E..0x1A78646
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A78647..0x1A78B6F
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A78B70..0x1A861DC
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A861DD..0x1A86705
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A86706..0x1A86C2E
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A86C2F..0x1A87157
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A87158..0x1A87680
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1A87681..0x1AAC3C7
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1AAC3C8..0x1AC2061
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1AC2062..0x1ACBE7C
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x2076CB4..0x20A81FB
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x20A81FC..0x20BDE07
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x20BDE08..0x20E84EA
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x20E84EB..0x2134998
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x2134999..0x2161407
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x21A4357..0x21B30F5
  - build: BLD-GOG-EN
    file: CD:CONQUER/SKIRMISH.RES
    offset: 0x08..0x4403F
tool: Container census written for this project
environment: null
---

## Observation

`C1086.GOB` holds 25 entries named `.666`, at directory indices `0x15F` to `0x171` (`iconmap`,
`monylndr`, `fopts`, `topts`, `vinn`, `vopts`, `vsmith`, `war`, `cgopts`, `chargen`, `dking`,
`fiefmgmt`, `foview`, `fwarplan`, `gameopts`, `tents`, `skirmsnd`, `utility`, `configit`), `0x1D8` to
`0x1DC` (`joust`, `avg_end`, `champl30`, `crownl30`, `dubb3`) and `0x1E5` (`intro`). `utility` and
`intro` are kind 0, `configit` is kind 2 and the rest kind 1. `SKIRMISH.RES` holds one more,
`SKirmsnd.666`, at index 0, kind 1. Every one of the 26 decoded banks:

- begins with the `UINT32LE` `0x004A5031`, the bytes `1PJ` and a zero;
- continues with samples, each a `UINT32LE` byte length, a `UINT32LE` rate in hertz and that many
  bytes, until the entry ends exactly at the end of a sample.

There are 102 samples: 10 at 22,050 Hz, 5 at 11,050 Hz, all in `fiefmgmt.666` (its samples 1 to 5),
and 87 at 11,025 Hz. Per bank, samples and total sample bytes:

| Bank | Samples | Rates | Sample bytes |
|---|---|---|---|
| `iconmap`, `monylndr`, `vsmith` | 2 each | 11,025 then 22,050 | 33,459; 137,793; 32,132 |
| `fopts`, `topts`, `vinn`, `vopts`, `cgopts`, `chargen`, `dking`, `foview`, `fwarplan`, `gameopts`, `tents` | 1 each | 11,025 | 2,159 each |
| `war` | 9 | 11,025 | 57,754 |
| `fiefmgmt` | 6 | 11,025, then five at 11,050 | 74,820 |
| `skirmsnd` (GOB) | 21 | 11,025 | 179,638 |
| `utility` | 4 | 11,025 | 89,206 |
| `configit` | 1 | 22,050 | 47,557 |
| `joust` | 3 | 22,050, 22,050, 11,025 | 209,555 |
| `avg_end`, `champl30`, `crownl30`, `dubb3` | 1 each | 22,050 | 148,217; 269,251; 458,733; 247,394 |
| `intro` | 1 | 11,025 | 60,819 |
| `SKirmsnd` (`SKIRMISH.RES`) | 36 | 11,025 | 306,382 |

The same 2,159 bytes at 11,025 Hz (XXH3-128 `7b1ce5e094e0526e7c8ecc16b35eeb1d`) are sample 0 of 16
banks, `iconmap`, `monylndr`, `fopts`, `topts`, `vinn`, `vopts`, `vsmith`, `cgopts`, `chargen`,
`dking`, `fiefmgmt`, `foview`, `fwarplan`, `gameopts`, `tents` and `utility`, and sample 8, the
last, of `war`. Eleven of the 16 hold nothing else; in the other five sample 1 starts at offset
`0x87B`. The two `skirmsnd` banks differ: the GOB one is 179,810 bytes and the `SKIRMISH.RES` one
306,674.

That shared sample has mean 127.51, starts with values of 127 and 128, spans 36 to 242 and holds no
zero byte.

## Interpretation

A bank is a tag and a packed list of rate-tagged mono samples (FMT-SOUND-001), unsigned with 128 as
silence: read as signed, the shared sample would sit near full scale. `VSMITH.666` is two samples of
audio; the blacksmith's conversation is elsewhere.

## Alternatives

The unsigned reading rests on the waveform; the game passes the bytes to its sound driver without
converting them (FND-SOUND-003), and the driver's sample format was not traced.

## How to reproduce

Decode each named entry (RULE-RES-001) and walk its samples from offset 4.
