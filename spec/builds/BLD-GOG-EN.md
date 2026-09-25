---
id: BLD-GOG-EN
title: Conqueror A.D. 1086 1.0, English, GOG release
superseded_by: []
developer: Software Sorcery
publisher: Sierra On-Line
publisher_version: "1.0"
distribution: GOG
languages: [en]
int_width: 32
manifest: BLD-GOG-EN.files.yaml
---

## Obtaining

Buy *Conqueror A.D. 1086* from GOG and install it (GOG game ID `2110944433`, build ID
`50474999570497193`, both from `goggame-2110944433.info`). The installation holds a disc image,
`game.gog` with the cue sheet `game.ins`, XXH3-128 `b915491c5bdce934ca216d2ceebec0ce` for
`game.gog`. `CD:VERSION.TXT` on the disc names the game as version 1.0, Sierra On-Line, 1995.

The GOG DOSBox configuration mounts the install directory as `C:` and the disc image as `D:`.
`CONQUER.BAT` runs `D:\CONQUER.EXE`, so the executable is the disc's copy. The installed
`CONQUER.INI` sets `GOB=C:\`, `CD_PATH=D:\CONQUER\` and `AUD_DRV=D:\`: the game reads the
installed `C1086.GOB` and takes its Smacker movies, `.RES` and `.LOW` files from `CD:CONQUER/`.
The disc holds a byte-identical copy of `C1086.GOB` (same size and hash), which this
configuration does not read.

The disc's first track is data (`MODE1/2352`); tracks 2 to 6 are CD audio. The manifest hashes
each audio track over its raw 2,352-byte sectors, taken from `game.gog` at the positions
`game.ins` gives.

`CD:CONQUER.EXE` is a DOS/16M-bound LE executable. Its MZ stub embeds a second MZ module at file
offset `0x26654`, and the LE header sits at file offset `0x290FC`. The LE header's data-pages
offset counts from the start of that module, so the pages begin at file offset `0x4C254`. It is
not packed. Addresses in the spec place each LE object at the relocation base its object table gives:

| Object | Holds | Relocation base | Virtual size | Pages |
|---|---|---|---|---|
| 1 | code | `0x00010000` | `0x7CB9E` | 1 to 125 |
| 2 | data | `0x00090000` | `0x23670` | 126 to 149 |

The entry point is object 1 offset `0x60E64`, address `0x00070E64`. An offset into object 2,
such as `+0xC904`, is the address `0x00090000 + 0xC904 = 0x0009C904`.

## Compared with other builds

No other build has been studied. Whether this disc image is byte-identical to the 1995 retail
CD is not known.

## Other files

The installation adds GOG's DOSBox and its configuration files (`dosbox_ad1086*.conf`),
`CONQUER.BAT` and `CONFIG.BAT` launchers, `Manual.pdf` (see SRC-MANUAL), icons, the GOG
metadata files `goggame-2110944433.*`, `EULA.txt`, `webcache.zip` and the uninstaller
`unins000.*`. The install directory also holds the empty `SAVEGAME` directory where the game
writes its saves.

The disc also holds the Sierra installer and setup files (`INST.EXE`, `INSTALL.*`, `SETUP.EXE`,
`SETUP.SOL`, `SIERRA.INF`, `LANGUAGE.INF`, `RESOURCE.CFG`), the Windows autoplay program
(`AUTOPLAY.*`, `AUTORUN.INF`), `BOOTDISK.EXE`, the HMI sound drivers (`HMIDET.386`,
`HMIDRV.386`, `HMIMDRV.386`), `VESA/UNIVESA.*`, readme files, icons, `DEMOS/` with demos of other Sierra games, and `INN/`,
which appears to hold client files for Sierra's ImagiNation Network service. The manifest lists the
disc's setup program `CD:CONFIG.EXE` and its configuration program `CD:CONQUER/CONCFG.EXE`
because the CONFIG area describes the settings they write.
