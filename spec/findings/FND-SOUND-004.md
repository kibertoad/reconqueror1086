---
id: FND-SOUND-004
title: CONQUER.INI switches the sound systems and names an HMI SOS driver setup; the digital driver runs at 22,050 Hz
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002AAD0..0x0002AC01
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002A53C..0x0002A558
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005AAFC..0x0005ADA7
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005ADA8..0x0005B0D3
  - build: BLD-GOG-EN
    file: CONQUER.INI
    offset: 0x00..0x16F
  - build: BLD-GOG-EN
    file: CD:CONQUER.INI
    offset: 0x00..0x139
  - build: BLD-GOG-EN
    file: CD:HMIDET.386
    offset: 0x00..0x14023
  - build: BLD-GOG-EN
    file: CD:HMIDRV.386
    offset: 0x00..0x3FCB8
  - build: BLD-GOG-EN
    file: CD:HMIMDRV.386
    offset: 0x00..0x1B537
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The routine at `0x0002AAD0` reads seven `CONQUER.INI` keys with `0x00062EA0` and compares each with
`ON` through `0x00069890`: `MOVIE` into `0x0009ADC4`, `ANIMATIONS` into `0x0009ADB8`, `CDMUSIC`
through `0x0005B370` into `0x0009DBA8`, `CREDITS` into `0x0009ADC8`, `SOUND_EFFECTS` into
`0x0009ADBC`, `DIG_SPEECH` into `0x0009ADD4` and `MIDIMUSIC` into `0x0009ADCC`. A key that is present
and not `ON` stores 0; the flags start as 1.

Startup then calls `0x0005AAFC` at `0x0002A545` only when `0x0009ADBC` is non-zero, and `0x0005ADA8`
at `0x0002A553` only when `0x0009ADCC` is non-zero.

`0x0005AAFC` does nothing when `0x0009DBAC` is already set. It requires `SOUND_DEVICE`, `SOUND_PORT`,
`SOUND_DMA` and `SOUND_IRQ`, printing `Error..Sound System not configured...Aborting.` and exiting
with 1 when one is missing; `AUD_DRV` defaults to `.\`. It sets `0x0009DBAC`, loads the driver
from `AUD_DRV` through `0x00079C87` and `0x00079E6B`, parses the port, DMA and IRQ as hexadecimal
with `0x0007AAC2(text, 0, 16)` into `0x000AFE60`, `0x000AFE68` and `0x000AFE64`, stores 22,050
(`0x5622`) at `0x0009DAC4`, opens the driver through `0x00078151` into `0x0009DAB0`, starts a timer
with `0x00069AAC(30, ...)` and registers `0x0004AA88` to run at exit. Every driver failure prints
`Error : ` with the driver's message and exits with 1.

`0x0005ADA8` does nothing when `0x0009DBB4` is set. It requires `MIDI_DEVICE` and `MIDI_PORT`
(`Error..Midi System not configured...Aborting.`), reads `AUD_DRV`, loads the MIDI driver through
`0x000793C7` and `0x00079563`, allocates `0x800` paragraphs (32 KB) of DOS memory at `0x000AFF20`
for songs (`Not enough memory to alloc buffer for Midi Sound...Aborting.`), reads `melodic.bnk`
(index `0x17E`) and `drum.bnk` (`0x17F`) into DOS memory and hands each to `0x0007AB6C` with the
driver handle at `0x000AFF28`, and registers `0x0004AAB4` to run at exit.

The installed `CONQUER.INI` sets all seven keys `ON` and adds `SOUND_NAME=SB16`,
`SOUND_DEVICE=0xe015`, `SOUND_PORT=0x220`, `SOUND_DMA=0x1`, `SOUND_IRQ=0x7`, `MIDI_NAME=MIDI OPL3`,
`MIDI_DEVICE=0xa002`, `MIDI_PORT=0x220` and `AUD_DRV=D:\`. The disc root holds `HMIDET.386`,
`HMIDRV.386` and `HMIMDRV.386`. `melodic.bnk` and `drum.bnk` are 5,404 bytes each.

## Interpretation

Digital sound and MIDI music use the HMI Sound Operating System drivers from the disc, configured by
device numbers the setup program wrote. `SOUND_EFFECTS` and `MIDIMUSIC` decide at startup whether
each system starts at all: turning either on later in the options screen cannot start a system
that was off at startup, since the play routines test the started flags too. `SOUND_NAME` and
`MIDI_NAME` are labels for the setup program; the game reads neither. The two banks are the OPL
instrument sets the MIDI driver plays with.

## Alternatives

That the drivers are HMI's rests on the file names and the `HMIMIDIP013195` tag of the songs
(FND-SOUND-005); the driver entry points `0x00078151` and `0x000793C7` were not traced inside. What
the 30 passed to `0x00069AAC` means was not traced.

## How to reproduce

Disassemble the listed ranges in `CD:CONQUER.EXE`; read the strings at `0x0009850C` to `0x000987FC`
and compare the keys with `CONQUER.INI`.
