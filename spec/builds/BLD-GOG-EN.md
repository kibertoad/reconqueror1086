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

The GOG DOSBox configuration mounts the install directory as `C:` and the disc image as `D:`,
and emulates an S3 SVGA card with 16 MB of memory. `dosbox_ad1086_single.conf` runs
`CONQUER.BAT`, which runs `D:\CONQUER.EXE` with up to two arguments, so the executable is the disc's
copy. `dosbox_ad1086_settings.conf` runs `CONFIG.BAT`, which runs the disc's setup program
`D:\config.exe`. The installed
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

FND-RES-010 records the complete owned installation/media listing and its
independent path/size comparisons. The installation was enumerated recursively,
without following reparse points and with inaccessible paths treated as errors.
The source image's ISO data track was traversed recursively from its root
directory; every file payload was hashed. The cue sheet bounded each raw audio
track for hashing. The source image itself is also in the manifest.

Every listed file or audio track occurs either in the manifest or in
`BLD-GOG-EN.other-files.yaml`, whose explicit paths each give an exclusion reason.
That list accounts for the installed DOSBox wrapper, host launch/configuration
files, distributor metadata, documentation and uninstaller. `Manual.pdf` is the
owned source SRC-MANUAL. The empty `SAVEGAME` directory has no file to list.

The disc's `DEMOS/` and `INN/` trees are listed in `BLD-GOG-EN.other-files.yaml`:
other Sierra products' demos, and the ImagiNation Network software that only
`INN.BAT` reaches, which no file of the game names (FND-RES-057). `VESA/`, the
root auxiliary files and the disc copy of `C1086.GOB` remain in the manifest
conservatively. This inventory does not prove that every such path is a
gameplay dependency; exclusion awaits a reading that establishes its lack of
game-native use.
The HMI drivers and both configuration executables remain accounted for.

Traversal entered the raw image's ISO filesystem and identified the raw audio
tracks. It did not enter the installation's `webcache.zip`, the DOSBox source
archive, or archives embedded in disc files. Resource archives in the manifest
still need member-format coverage; path accounting does not complete Survey.

## Code ranges

None.
