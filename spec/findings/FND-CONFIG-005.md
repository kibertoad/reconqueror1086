---
id: FND-CONFIG-005
title: The disc and the installation ship different CONQUER.INI files, written by two setup programs
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations: 
  - build: BLD-GOG-EN
    file: CONQUER.INI
    offset: 0x00..0x16F
  - build: BLD-GOG-EN
    file: CD:CONQUER.INI
    offset: 0x00..0x139
tool: Conqueror.Inspect (tools/Conqueror.Inspect)
environment: null
---

## Observation

`CD:CONQUER.INI` has 20 lines, each ended by CR and LF: `MOVIE=ON`, `CREDITS=ON`, `ANIMATIONS=ON`,
`DIG_SPEECH=OFF`, `CDMUSIC=ON`, `SOUND_EFFECTS=OFF`, `MIDIMUSIC=OFF`, `USE_CYBERMAN=OFF`,
`DELAYVGA=OFF`, `WAR_MODE=640`, then `SOUND_NAME`, `SOUND_DEVICE`, `SOUND_PORT`, `SOUND_DMA`,
`SOUND_IRQ`, `MIDI_NAME`, `MIDI_DEVICE` and `MIDI_PORT` with question marks as values, then
`POVWINSIZE=NORMAL` and `SLOWMACHINE=FALSE`.

The installed `CONQUER.INI` has the same keys in the same order with every switch `ON`,
`WAR_MODE=1024`, the sound values of FND-SOUND-004, and three more lines at the end: `GOB=C:\`,
`AUD_DRV=D:\` and `CD_PATH=D:\CONQUER\`. It has no `DATA`, `FULL_MOVIE` or `GRAPHICS` key.

`CD:CONFIG.EXE` is a 16-bit Sierra install program whose script language has `@SETINI` and `@GETINI`
commands, the messages `Did not specify the ini file` and `the ini file does not exist`, and the
pattern `%s=%s`. `CD:CONQUER/CONCFG.EXE` is a DOS/4GW program holding `SOUND_NAME`, `SOUND_DEVICE`,
`SOUND_PORT`, `SOUND_DMA`, `SOUND_IRQ`, `MIDI_NAME`, `MIDI_DEVICE`, `MIDI_PORT`, `AUD_DRV`, `GOB`,
`hmidet.386`, `hmidrv.386`, `hmimdrv.386` and `%s=%s`.

## Interpretation

The disc's file is a template: the Sierra installer copies it, and the sound setup program detects
the card with HMI's detection driver and fills in the sound and MIDI lines. The GOG installation
ships a file already filled in for DOSBox's Sound Blaster 16 and OPL3, with all switches on, the
largest battle display, and the paths of its DOSBox drives. The game itself only rewrites the
seven switches and `POVWINSIZE` (FND-CONFIG-002).

## Alternatives

The programs were identified by their strings only; neither was run or disassembled. Which of them
writes `GOB`, `AUD_DRV` and `CD_PATH`, and whether either writes `WAR_MODE`, `POVWINSIZE` or
`SLOWMACHINE`, is not known.

## How to reproduce

Extract the two INI files and compare them; list the printable strings of `CD:CONFIG.EXE` and
`CD:CONQUER/CONCFG.EXE` around the listed offsets.
