---
id: FMT-CONFIG-001
title: CONQUER.INI, the settings file
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CONQUER.INI", "CD:CONQUER.INI"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-CONFIG-001, FND-CONFIG-002, FND-CONFIG-003, FND-CONFIG-004, FND-CONFIG-005, FND-SOUND-004, FND-RES-006, FND-MEDIA-009]
conflicting: []
split_with: []
related: [RULE-CONFIG-001, RULE-CONFIG-002]
---

## Layout

8-bit text read in text mode a line at a time, up to 1,023 bytes per read, so a longer line is read
as more than one. White space is what the C library's `isspace` accepts. Keys and values are
compared byte for byte, so case matters. The game writes the file back as one `key=value` line per
stored key, ended by CR and LF, in the order it read them.

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| each line | `char[]` | `lines` | A line that is empty or white space, or whose first byte after white space is `#` or `;`, is ignored. Any other line must hold `=`: the key is the text before the first `=`, and the value the text after it, each without the white space around it. A value keeps inner spaces and later `=`. A line without `=` ends the program with code 115. | supported | FND-CONFIG-001, FND-CONFIG-002 |
| `MOVIE` | `char[]` | `movie` | Values: switch. Opening movie. Written by the options screen. | supported | FND-SOUND-004, FND-CONFIG-002 |
| `CREDITS` | `char[]` | `credits` | Values: switch. Credits movie. Written by the options screen. | supported | FND-SOUND-004, FND-CONFIG-002 |
| `ANIMATIONS` | `char[]` | `animations` | Values: switch. Animations between screens. Written by the options screen. | supported | FND-SOUND-004, FND-CONFIG-002 |
| `DIG_SPEECH` | `char[]` | `dig_speech` | Values: switch; also compared with `OFF`. Spoken dialogue in movies. Written by the options screen. | supported | FND-SOUND-004, FND-MEDIA-009, FND-CONFIG-003 |
| `CDMUSIC` | `char[]` | `cdmusic` | Values: switch. CD audio music. Written by the options screen. | supported | FND-SOUND-004 |
| `SOUND_EFFECTS` | `char[]` | `sound_effects` | Values: switch. Starts the digital sound driver at startup. Written by the options screen. | supported | FND-SOUND-004 |
| `MIDIMUSIC` | `char[]` | `midimusic` | Values: switch. Starts the MIDI driver at startup. Written by the options screen. | supported | FND-SOUND-004 |
| `USE_CYBERMAN` | `char[]` | `use_cyberman` | Values: `ON` or other. CyberMan controller in the first-person view, when one is detected. | supported | FND-CONFIG-003 |
| `DELAYVGA` | `char[]` | `delayvga` | Values: `ON` or other. An extra display reset after full-screen movies and the view. Must be present (BUG-CONFIG-001). | supported | FND-CONFIG-003, FND-MEDIA-009 |
| `WAR_MODE` | `char[]` | `war_mode` | Values: decimal number. 640 keeps the field battle at 640 by 480, 800 allows 800 by 600, anything else or no key tries 1024 by 768. | supported | FND-CONFIG-004 |
| `SOUND_DEVICE`, `SOUND_PORT`, `SOUND_DMA`, `SOUND_IRQ` | `char[]` | `sound_settings` | Values: hexadecimal. Digital sound driver settings; required when `SOUND_EFFECTS` is on. | supported | FND-SOUND-004 |
| `MIDI_DEVICE`, `MIDI_PORT` | `char[]` | `midi_settings` | Values: hexadecimal. MIDI driver settings; required when `MIDIMUSIC` is on. | supported | FND-SOUND-004 |
| `AUD_DRV` | `char[]` | `aud_drv` | Values: directory. Where the HMI drivers are; default `.\`. | supported | FND-SOUND-004 |
| `POVWINSIZE` | `char[]` | `povwinsize` | Values: `NORMAL`, `FULLSCREEN`, `SMALL`, `SMALLER`, `SMALLEST`. Size of the first-person view. Written back by the game. | supported | FND-CONFIG-002, FND-CONFIG-003 |
| `SLOWMACHINE` | `char[]` | `slowmachine` | Values: `TRUE` or other. Skips part of the view's drawing. | supported | FND-CONFIG-003 |
| `GOB` | `char[]` | `gob` | Values: directory. Where `C1086.GOB` is; default `.\GOB\`. | supported | FND-RES-006 |
| `DATA` | `char[]` | `data` | Values: directory. Where `C1086ad.GOB` would be; default `.\DATA\`. | supported | FND-RES-006 |
| `CD_PATH` | `char[]` | `cd_path` | Values: directory. Where scene archives and movies are; the default depends on the reader. | supported | FND-RES-006, FND-CONFIG-003 |
| `FULL_MOVIE` | `char[]` | `full_movie` | Values: any. When present, the opening movie is `movie3f.smk`. | supported | FND-CONFIG-003 |
| `GRAPHICS` | `char[]` | `graphics` | Values: any. Read and ignored. | supported | FND-CONFIG-003 |

"Switch" means `ON` turns it on, any other value turns it off, and a missing key leaves it on.
`SOUND_NAME` and `MIDI_NAME` are in both shipped files and are read by no code.

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Both shipped files: `CD:CONQUER.INI` (20 lines) and the installed `CONQUER.INI` (23 lines)
[FND-CONFIG-005].

## Open questions

- Which setup program writes `GOB`, `AUD_DRV`, `CD_PATH` and `WAR_MODE`.
