---
id: RULE-SOUND-003
title: Starting the sound systems and playing MIDI music
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-SOUND-004, FND-SOUND-005, FND-RES-006, FND-SOUND-006, FND-UI-012, FND-SOUND-003]
conflicting: []
split_with: []
related: [RULE-RES-001, RULE-RES-004, RULE-SOUND-001, RULE-CONFIG-002]
---

## Summary

At startup `CONQUER.INI` decides which of digital sound, MIDI music and CD music start. A screen then
plays one HMP song at a time from the open archive and stops it when it leaves.

## When it runs

`read_sound_options`, `init_digital_sound` and `init_midi` once at startup; `play_music` and
`stop_music` when a screen starts or ends (28 and 38 calls); `pause_music` and `resume_music` once
each.

## Parameters

- `index`: the directory index of the song.
- `from_open`: 1 to read from the open archive; every caller passes 1.

## Inputs

`ini_value` (RULE-CONFIG-002), the value of a `CONQUER.INI` key.

## Procedure

```text
define read_sound_options():
    # absent keys leave the flags at 1
    if ini_value("CDMUSIC") != 0 and ini_value("CDMUSIC") != "ON":
        cd_music_on = 0
    if ini_value("SOUND_EFFECTS") != 0 and ini_value("SOUND_EFFECTS") != "ON":
        sound_effects_on = 0
    if ini_value("MIDIMUSIC") != 0 and ini_value("MIDIMUSIC") != "ON":
        midi_music_on = 0
    # MOVIE, ANIMATIONS, CREDITS and DIG_SPEECH are read the same way into their own flags
    if sound_effects_on != 0:
        init_digital_sound()
    if midi_music_on != 0:
        init_midi()

define init_digital_sound():
    if digital_ready != 0:
        return
    for key in ["SOUND_DEVICE", "SOUND_PORT", "SOUND_DMA", "SOUND_IRQ"]:
        if ini_value(key) == 0:
            # the game stops with "Error..Sound System not configured...Aborting."
            return
    digital_ready = 1
    # the driver for SOUND_DEVICE is loaded from AUD_DRV (default ".\"), opened with
    # the hexadecimal port, DMA and IRQ and an output rate of 22050; any driver error
    # stops the game with "Error : " and the driver's message

define init_midi():
    if midi_ready != 0:
        return
    for key in ["MIDI_DEVICE", "MIDI_PORT"]:
        if ini_value(key) == 0:
            # the game stops with "Error..Midi System not configured...Aborting."
            return
    # the MIDI driver is loaded from AUD_DRV, a 32 KB song buffer is taken from DOS memory,
    # and entries 0x17E (melodic.bnk) and 0x17F (drum.bnk) are read and handed to the driver
    midi_ready = 1

define play_music(index: UINT32, from_open: INT32):
    if midi_music_on == 0 or midi_ready == 0:
        return
    if from_open != 1:
        read_from_data_archive()
    let e = entry_at(index)
    if e == 0:
        # the game stops with "Midi File not found in Resource file. Aborting."
        return
    if e.expanded_size > 0x8000:
        # the game exits with 1
        return
    read_entry(e, song_buffer)
    # the driver prepares the song in song_buffer and starts it

define stop_music():
    # the driver stops the song and releases it

define pause_music():
    # the driver pauses the song; music_paused records it

define resume_music():
    # the driver resumes the song; music_paused records it
```

## Outputs

Sound from the drivers; the flags `digital_ready` and `midi_ready`.

## Edge cases

- Turning `SOUND_EFFECTS` or `MIDIMUSIC` on in the options screen after starting with it off
  sets the flag but starts no driver, so nothing plays until the game is restarted.
- A missing `SOUND_*` or `MIDI_*` key ends the game at startup, but only when its system is on.

## What the sources say

The executable (FND-SOUND-004, FND-SOUND-005); the installed `CONQUER.INI` and the songs.

## Differences between builds

None known.

## Open questions

- The HMI driver calls inside `init_digital_sound`, `init_midi`, `play_music` and `stop_music`.
