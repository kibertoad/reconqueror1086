---
id: FND-RES-010
title: Owned installation and raw disc directory listing include auxiliary media paths
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: game.gog
    offset: 0x00000000..0x27C67110
tool: Bounded raw-sector ISO directory traversal and recursive filesystem enumeration
environment: null
---

## Observation

The owned image is 667,316,496 bytes and has XXH3-128
`b915491c5bdce934ca216d2ceebec0ce`. Its data track has 244,445 raw sectors.
Following the primary volume descriptor's root directory and every child
directory produces 2,830 file paths. A second traversal, independently reading
the directory records, agrees with every path and declared size. The raw audio
tracks add five track paths, and recursive enumeration of the installation adds
38 file paths. The combined listing has 2,873 distinct paths.

Installation paths include the source image and files beneath `DOSBOX/`, not
just the installation root. The installation enumeration agrees path-for-path
and size-for-size with a separate recursive filesystem listing. No reparse
point was encountered in this owned installation; enumeration excludes such
points rather than following them.

The disc listing includes `DEMOS/`, `INN/`, `VESA/`, the disc copy of `C1086.GOB`
and root-level auxiliary files, in addition to `CONQUER/` and the executables
already identified in BLD-GOG-EN. Each file has a size and XXH3-128 fingerprint.
For newly listed files, executable-container classifications follow their MZ
header and, where present and bounded within the file, the new-header signature;
a file with no recognized executable header remains data. The `.COM` path is
classified as COM. This reads container metadata only, not instructions.

## Interpretation

A root-only installation listing misses compatibility-wrapper files. A listing
of only the known game resource directory also misses other shipped media paths.
The image is the carrier of the disc files and audio tracks, and is identified
alongside its contents. Accounting for these paths is separate from proving
which auxiliary files the game uses or identifying their internal formats.

## Alternatives

The additional disc directories may be used only by bundled demos, installers
or network clients. Their paths and headers alone do not settle every possible
game-native access. They remain in the manifest until a reading of the relevant
file loaders and callers establishes exclusions. No absence of runtime reads is
claimed, and the observed directory layout does not establish retail-disc
identity or archive-member format coverage.

## How to reproduce

Use the owned installation identified by BLD-GOG-EN. Verify the image hash and
size, then use the cue sheet to bound the data track and raw audio-track spans.
Read each data sector's 2,048-byte payload after its 16-byte prefix. Follow the
ISO primary volume descriptor's root directory and recursively follow child
directory records, excluding the current and parent directory records. Preserve
file paths and sizes, and remove ISO filename version suffixes. Hash file
payloads and each audio track's raw sectors. Independently traverse the directory
records and compare the path/size pairs.

Recursively list installation files without following reparse points, retain
the source image path, and compare with a second filesystem traversal. Inspect
bounded executable header fields to distinguish the executable containers.
Compare the combined paths against the build manifest and explicit Other files
list; each path must occur exactly once in their union. Do not execute original
programs or treat this comparison as proof of their runtime dependencies.
