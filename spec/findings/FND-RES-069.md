---
id: FND-RES-069
title: Canonical primary-volume paths agree with the owned build accounting
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
tool: RefurbishedDinosaurs.LegacyFormats 11.2.0 ISO reader and independent path comparison
environment: null
---

## Observation

The image identity and size match BLD-GOG-EN. Reading its data track through the
primary-volume ISO reader gives names without version suffixes and without a
terminal dot on extensionless names; normalization collisions retain the full
names. The combined disc, raw audio-track and installation listing has the same
2,873 distinct paths, sizes and XXH3-128 hashes as the listing in FND-RES-010.
No path was added, removed or renamed, and no size or hash differed.

Comparing every path with the manifest and explicit Other files list gives no
unaccounted path, missing path or overlap. Every manifest file's size and hash
match the newly read values. No archive members were entered by this audit.

## Interpretation

The existing path accounting already agrees with canonical primary-volume name
normalization. This comparison establishes listing identity, not runtime use,
resource interpretation, complete executable discovery or retail-disc identity.

## Alternatives

FND-RES-010's procedure mentions version removal without spelling out terminal
dot and collision handling. The current reader comparison supplies that check
without changing its observations. An archive may contain additional members;
the comparison makes no claim about paths inside an archive.

## How to reproduce

Verify the owned image size and XXH3-128 against BLD-GOG-EN. Use the cue sheet to
bound the data track. Traverse the primary volume with the named reader and hash
every file payload. Add raw audio-track spans and recursively enumerated owned
installation files as FND-RES-010 describes, without following reparse points.
Compare path, size and hash tuples with the earlier listing, then compare their
paths with the disjoint union of the manifest and explicit Other files list.
Check each manifest identity separately. Retain generated listings locally.
