# Original resource format specification

This document is the clean-room, byte-level specification for formats observed in the legally owned GOG release identified in [`original-findings.md`](original-findings.md). It contains structural facts and independently derived descriptions only—never extracted dialogue, images, audio, video, or binary payloads.

Confidence terms have the same meaning as the evidence register: **Confirmed** is directly measured across the named sample, **Corroborated** combines local evidence with independent documentation, **Provisional** remains a working hypothesis, and **Disproved** records a rejected interpretation.

## Indexed `.RES`/`.LOW` container

The loose `C1086.GOB`, 50 `.RES` scene files, and 49 `.LOW` low-detail scene files on the CD use the same outer indexed container and `.RES` signature. The `.LOW` suffix is a presentation tier, not an image format; the `.GOB` extension does not imply the unrelated LucasArts GOB format.

### Header

All integers are little-endian.

| Offset | Size | Type | Meaning | Confidence |
| ---: | ---: | --- | --- | --- |
| `0x00` | 4 | ASCII | Literal `.RES` signature | Confirmed |
| `0x04` | 4 | `UINT32LE` | Absolute directory offset | Confirmed |

Entry data occupies the range beginning at offset 8 and ending no later than the directory offset. Gaps have not yet been assigned semantics.

### Directory

| Offset from directory | Size | Type | Meaning | Confidence |
| ---: | ---: | --- | --- | --- |
| `0x00` | 4 | `UINT32LE` | Entry count | Confirmed |
| `0x04` | `count × 52` | records | Directory entries | Confirmed |

Each 52-byte record is:

| Record offset | Size | Type | Meaning | Confidence |
| ---: | ---: | --- | --- | --- |
| `0x00` | 32 | ASCII, NUL padded | Resource name | Confirmed |
| `0x20` | 4 | `UINT32LE` | Storage/compression kind: observed values 0, 1, 2 | Confirmed classification and kind-1/kind-2 codecs |
| `0x24` | 4 | `UINT32LE` | Reserved or unknown | Provisional |
| `0x28` | 4 | `UINT32LE` | Stored byte length | Confirmed |
| `0x2C` | 4 | `UINT32LE` | Expanded byte length | Confirmed |
| `0x30` | 4 | `UINT32LE` | Absolute data offset | Confirmed |

The parser rejects a directory before byte 8, a directory extending past end of file, more than one million records, blank names, and any stored extent outside the data area. Allocation uses checked conversions.

### Storage kinds

- Kind 0 entries have equal stored and expanded sizes in every observed example and are copied byte-for-byte. This behavior is **Corroborated**, because the bytes are structurally readable but the original decoding branch has not yet been disassembled.
- Kind 1 entries have unequal sizes and use the independently decoded block format below. Its framing and token grammar are **Confirmed for the hashed release**.
- Kind 2 occurs in five unequal-size GOB entries and uses the executable-confirmed adaptive LZW stream described below.
- Stored-versus-compressed behavior is determined from both the kind and size relationship; unknown combinations must be rejected rather than guessed.

### Kind-1 block framing

A kind-1 stored extent is a concatenation of blocks:

| Block offset | Size | Type | Meaning | Confidence |
| ---: | ---: | --- | --- | --- |
| `+0x00` | 2 | `UINT16LE` | Number of payload bytes following this field | Confirmed |
| `+0x02` | 1 | `BYTE` | Storage marker: `0x40` compressed, `0x80` verbatim | Confirmed for this release |
| `+0x03` | `length - 1` | bytes | Encoded data | Confirmed boundary and codec |

The next length begins immediately after the preceding payload. Zero lengths, truncated length fields, unknown storage markers, payloads beyond the entry extent, and trailing bytes that cannot form a complete block are invalid.

Each block expands to 16,384 bytes except the final block of an entry, which expands to the remaining bytes. Consequently, an entry has exactly `ceil(expanded length / 16384)` blocks. A `0x80` block contains those output bytes directly, so its framed payload length is the expected output length plus one marker byte. The decoder validates this relationship before copying it.

Across the hashed release, all 471 kind-1 GOB entries consume exactly as 2,963 blocks: 2,940 use `0x40` and 23 use `0x80`. The 99 CD `.RES`/`.LOW` scene containers add 29,126 blocks: 29,118 use `0x40` and eight use `0x80`. Every one of these 32,089 blocks decodes to its expected slice length. No scene entry uses kind 2. These population-wide results make the framing, marker meanings, output block size, and decoder **Confirmed for this release**, not necessarily for every game using a `.RES` signature.

### Kind-1 compressed token grammar

A `0x40` payload starts with the marker, one metadata byte that the original decoder does not consult, and a 16-bit big-endian control word. Control bits are consumed most-significant first. After 16 tokens, the next two bytes in the stream form another control word.

| Control bit | Token bytes | Meaning |
| ---: | --- | --- |
| `0` | one byte | Emit one literal byte. |
| `1`, nonzero distance | two bytes `a`, `b` | Copy `(b & 0x0F) + 3` bytes from the already decoded output at distance `(a << 4) | (b >> 4)`. Overlap is allowed. Distances range from 1 to 4095 and lengths from 3 to 18. |
| `1`, zero distance | four bytes `0`, `b`, `c`, `value` | Emit `value` repeatedly `(b << 8) + c + 16` times. Because the encoded distance is zero, `b` is at most `0x0F`; run lengths range from 16 to 4111. |

The implementation rejects truncated control words and tokens, history references before the beginning of output, output overflow, block-count disagreement, and any final expanded-size mismatch. The grammar was reconstructed from the owned executable's decoder and then independently checked by exact-size decoding of the full GOB/scene kind-1 population. It also yields 188 strict PCX-compatible images, including 640x480 `fftitle.pcx` and `engmap1.pcx`. The algorithm and those dimensions are **Confirmed for the hashed release**; assigning those two filenames to the title and England-map runtime roles is **Corroborated** by decoded visual inspection.

### Kind-2 adaptive LZW stream

Kind 2 is one continuous MSB-first LZW bitstream with no outer block framing. Codes `0` through `255` are literal bytes, `256` clears the dictionary, `257` ends the stream, and newly defined strings begin at code `258`. A clear resets the width to 9 bits and the next dictionary code to `258`. Each ordinary code after the first adds `previous string + first byte of current string`; the conventional `code == next code` special case emits `previous string + its first byte`.

The code width grows from 9 through 14 bits. After adding a dictionary entry, when the next code equals the current mask `(1 << width) - 1`, the decoder increments the width before reading the next code and recomputes the mask. At 14 bits the dictionary stops growing. This less common transition point and the 14-bit ceiling explain why the earlier 12-bit probe produced plausible PCX prefixes before losing bit alignment.

Static analysis of the owned LE executable identifies the MSB bit reader at virtual address `0x456F4`, dictionary helper at `0x4576C`, and kind-2 decoder at `0x45C6E`. Its setup at `0x45C90` writes width 9 and mask `0x1FF`; branches at `0x45CCC` and `0x45CFD` recognize end `0x101` and clear `0x100`; the growth path at `0x45D7F`–`0x45DF4` compares against the mask and caps the width at `0x0E`. The independently implemented bounded decoder expands all five kind-2 entries to their exact directory lengths. `prog.pcx`, `champion.pcx`, `crowning.pcx`, and `death.pcx` also pass strict PCX validation as 640x480 images. The codec is therefore **Confirmed for the hashed release**.

The implementation requires an end code, rejects truncated input, undefined codes and cyclic/out-of-range dictionary chains, bounds the dictionary at 16,384 entries, enforces the declared expanded length, and applies the common 256 MiB expansion ceiling before allocation.

### Resource population

The 486 GOB records and 13,147 scene records produce this extension-level inventory. `<none>` entries are predominantly internal scene resources; an absent extension is not evidence of a single payload type.

| Extension | Total | Stored | Kind 1 | Kind 2 | Scope |
| --- | ---: | ---: | ---: | ---: | --- |
| `<none>` | 13,129 | 1,770 | 11,359 | 0 | Scene |
| `.PCX` | 152 | 0 | 148 | 4 | GOB and scene |
| `.RAT` | 116 | 1 | 115 | 0 | GOB |
| `.CSF` | 66 | 5 | 61 | 0 | GOB and scene |
| `.PCC` | 49 | 1 | 48 | 0 | GOB |
| `.DAT` | 35 | 0 | 35 | 0 | GOB |
| `.HAT` | 27 | 0 | 27 | 0 | GOB |
| `.666` | 26 | 2 | 23 | 1 | GOB and scene |
| `.HMP` | 12 | 0 | 12 | 0 | GOB |
| `.PAL` | 5 | 5 | 0 | 0 | Scene |
| `.JP` | 5 | 0 | 5 | 0 | GOB |
| Other named extensions | 11 | 1 | 10 | 0 | GOB and scene |

The five kind-2 resources are four strictly validated 640x480 `.PCX` entries and one exactly expanded `.666` sound bank. This distribution, the image structure, and the sound-bank framing below are **Confirmed**; most other extensions remain unknown.

## Raw scene textures

`DynamixSceneTextureDecoder` reads the `TEX` resources of FMT-VIEW-008. It validates the three name fields, limits either dimension to 4,096, checks the product without integer overflow, requires exact payload consumption, and returns an owned index buffer. `scene-texture-report.txt` records only counts and distinct dimensions per archive.

## First-person scene structure

The combat maps hold an unreachable gallery of sample blocks (FND-VIEW-020). Runtime conversion therefore floods through passable and interactable cells from the Viewer, retains only that connected play area plus one enclosing wall cell, and translates the cropped coordinates back to the source map for texture lookup.

The 15 two-digit `MELEE00`–`MELEE24` archives belong to the tournament name builder, not to a one-per-castle campaign sequence. The practice callback selects inclusively among the three distinct unsuffixed `MELEE0`–`MELEE2` archives. Initialization `0x21DFC` registers dispatcher `0x21EEC` in engine callback slot `+0xB0` through `0x59C24`; the sole indirect call through that slot at `0x59BC7` reaches dispatcher slot 128 and its literal `MELEE0.RES` load at `0x22226`. The common campaign binding is therefore **Confirmed**.

The renderer regenerates the active colour-map family with RULE-VIEW-006, selects maps with RULE-VIEW-007, and keeps each encountered texture/map result. Transparency follows the source index before remapping.

Actor sprites use the texture runs of FND-VIEW-021. Runtime enemies keep a cardinal facing, advance the three-pose walk cycle only while moving, and stay visible through the imported death-state completion gate after leaving collision and combat.

The nested `SKIRMISH.RES/skirmish.csf` uses the same bounded CSF framing and contains exactly 53 frames. Frames 0-23 are 96x43 weapon-item views, frames 24-26 are 70x82 shields, frames 27-41 form five three-frame first-person weapon groups (axe, crossbow, hammer/pick, mace, and sword), frame 42 is a single dagger view, frames 43-47 are a large 70x82 blood-effect group, and frames 48-52 are a small 46x40 blood-effect group. Every frame renders coherently with the archive's raw 768-byte `SKIRMISH.PAL`. Framing, dimensions, grouping, palette coherence, and visible identities are **Confirmed**.

The raycaster walks the wrapped 128-cell source map (RULE-VIEW-003), not only the cropped `SiegeLayout`, so the rebuild's acquisition alpha testing must retain decoded sources for all structural face selectors and every sector reachable from kind-4 `Surface0` plus the heading-sector formula, including mirrored sectors. `RaycastTextureReferences` supplies this complete source-only dependency set; `ActivateSiegeVisuals` keeps it separate from the smaller GPU/render set. Omitting the distinction caused practice melee acquisition to request an unloaded but valid texture 11.

The dependency closure is over placed map blocks and their executable-eligible state-target chains, not every definition in the 0x80-block table. Unplaced templates in `MELEE0.RES` reference texture numbers such as 3 that the archive intentionally does not carry; they cannot enter the ray traversal. `SiegeTextureDependencies.AcquisitionTextures` starts from the 128x128 map population and follows a state target only for the behavior/kind combinations accepted by the ray traversal. This fixes the false startup requirement for textures 3/4/6 while retaining every source that a live ray can alpha-test.

The owned-resource census further shows that individual `MELEE*`/`DEFEND*` scene archives are sparse overlays and need not contain all three normalized walk families. `CONQUER/DEFEND2.RES` is the supported high-resolution archive containing the complete actor texture span 64-138. Runtime texture resolution therefore uses that archive as the common combat actor atlas and replaces matching slots with the active scene archive. The normalized slot identities follow RULE-ASSAULT-025; the complete-atlas identity and sparse-overlay relationship are **Corroborated** by the owned archive census. `LoadCombatTextureSources` validates the resulting closure before graphics use. Actor, object, wall, backdrop, and combat-shell procedural fallbacks have been removed: imported siege rendering now has one supported, verified original-art path and reports a precise missing dependency instead of drawing rectangles.

Runtime upload preserves source index zero as transparent and emits premultiplied `(0,0,0,0)` for MonoGame's default alpha blend; CSF alpha planes are premultiplied by the same rule. Retaining palette RGB under alpha zero caused the combat atlas's transparent border to blend as a white rectangle. Actor/object horizontal placement now inverts the confirmed viewer basis directly as `screen = center + lateral / forward`; the earlier conventional 60-degree FOV approximation is **Disproved**. Actor width is one projected 8.8 cell and vertical bounds use block `LowerElevation`/`UpperElevation`, viewer elevation `0x80`, viewport width, and live actor depth, matching RULE-VIEW-003. Render sector selection now uses the same confirmed `actor heading - viewer heading` byte-turn input as acquisition instead of the former actor-to-viewer bearing approximation. Keyboard attack compatibility targets the resulting billboard center; pointer attacks retain the exact centered cursor contact.

## Smacker movie container

The CD contains 2,131 Smacker movies occupying 288,867,980 bytes and indexing 182,360 frames. Every movie uses the `SMK2` version. The bounded container parser validates the fixed header, optional ring-frame adjustment, frame-size table, frame-flags table, tree extent, and every aligned frame extent through exact end of file. It supports `SMK4` framing synthetically so the playback boundary is explicit, but no `SMK4` file occurs in this release.

| Offset | Size | Meaning |
| ---: | ---: | --- |
| `0x00` | 4 | ASCII `SMK2` or `SMK4` |
| `0x04` | 12 | width, height, and declared frame count as `UINT32LE` |
| `0x10` | 4 | signed frame-duration field |
| `0x14` | 4 | video flags; bit 0 adds a ring frame |
| `0x18` | 28 | maximum decoded byte count for each of seven audio tracks |
| `0x34` | 20 | combined tree length followed by four tree descriptors |
| `0x48` | 28 | seven packed sample-rate/audio-flag words |
| `0x64` | 4 | reserved padding |
| `0x68` | `frames * 4` | encoded frame lengths; low bits carry frame flags and the remaining bits are the aligned extent |
| next | `frames` | per-frame palette/audio-presence flags |
| next | declared tree length | shared Huffman tree data |
| next | sum of aligned frame lengths | frame payloads, ending exactly at EOF |

Of the 2,131 files, 2,095 declare one packed 8-bit mono audio track: 2,093 at 22,050 Hz and two at 11,025 Hz. The other 36 are silent. The dominant geometry is 196x204 (2,068 files); larger and special-purpose movies span eight other observed dimensions up to 640x480. Frame flags bind optional palette data followed by audio packets for tracks 0–6; the remainder is the video packet. Population-wide demultiplexing validates 2,135 palette changes and 161,884 audio packets while leaving a non-empty video packet in every frame. Palette packets copy the previous 256-entry RGB table, then apply bounded skip, old-palette copy, and new 6-bit RGB commands. Six-bit components expand with `value * 4 + value / 16`.

Packed audio uses one LSB-first Huffman tree of 8-bit deltas for this release's mono profile. Each packet declares its decoded byte count, defines a bounded tree of at most 256 leaves and depth 27, seeds an unsigned 8-bit predictor, and reconstructs subsequent samples with byte-wrapping delta addition. All 161,884 packets decode to their declared sizes, totaling 398,368,812 unsigned PCM bytes. The playback adapter can convert these samples once to signed PCM16 using the same range-preserving mapping as `.666` banks.

Video uses four shared LSB-first adaptive Huffman trees for monochrome maps, monochrome colors, full-color pairs, and block types. Each frame resets the three recency slots in each tree, then reconstructs 4x4 blocks as two-color bitmap, full-color, previous-frame skip, or solid fill runs. The stateful decoder writes into the caller's retained index buffer so predicted frames require no full-frame intermediate allocation. All 182,360 frames decode within their packet bounds. A locally rendered final frame of `TITLE.SMK` confirms coherent palette, orientation, and block composition.

This uniform `SMK2` population makes direct decoding the primary strategy; installation-time transcoding is unnecessary for the owned release. The MonoGame adapter now drives the original title and credits sequences plus the item previews named by `WEAPONS.DAT`: it retains one indexed frame buffer, one RGBA upload buffer, and one `Texture2D`, advances frames at the container duration, submits decoded PCM16 packets through `DynamicSoundEffectInstance`, and supports skip/end/return transitions. Its seekable reader retains only the bounded header/table/tree prefix and one reusable maximum-compressed-frame buffer; it does not load the complete active movie. Broader scene-event bindings and seeking remain adapter work rather than codec work.

## Dynamix `.666` sound banks

All 26 decoded `.666` resources use the same rate-tagged sample sequence:

| Offset | Size | Type | Meaning |
| ---: | ---: | --- | --- |
| `0x00` | 4 | `UINT32LE` | Magic `0x004A5031` |
| `0x04` | 4 | `UINT32LE` | First sample byte length |
| `0x08` | 4 | `UINT32LE` | First sample rate in hertz |
| `0x0C` | declared length | bytes | First sample payload |
| next | repeated | same three fields | Further samples until exact end of resource |

The GOB and scene population contains 26 banks and 102 sample payloads. Observed rates are 11,025, 11,050, and 22,050 Hz; every bank consumes its decoded resource exactly, including kind-2 `CONFIGIT.666`. `VSMITH.666` contains two samples (32,132 payload bytes total at 11,025 and 22,050 Hz), so the earlier hypothesis that it contains blacksmith dialogue nodes is **Disproved**.

The framing, lengths, sample counts, and rates are **Confirmed for the hashed release**. Payload analysis confirms unsigned 8-bit mono PCM: the shared 2,159-byte interface sample has mean 127.51, begins with quiet values around 127-128, spans 36-242, and contains no zero-valued silence. Interpreting those bytes as signed PCM would create a near-full-scale DC offset. At game startup the runtime decodes every installed bank once, converts every sample once to signed 16-bit little-endian PCM with `(value - 128) << 8`, and retains the ready buffers for the session. This preserves the full source range without clipping and keeps conversion out of the input/playback path.

That same 2,159-byte payload (XXH3-128 `7b1ce5e094e0526e7c8ecc16b35eeb1d`) occurs bit-for-bit in 17 screen-specific banks, always at 11,025 Hz: `ICONMAP`, `MONYLNDR`, `FOPTS`, `TOPTS`, `VINN`, `VOPTS`, `VSMITH`, `WAR`, `CGOPTS`, `CHARGEN`, `DKING`, `FIEFMGMT`, `FOVIEW`, `FWARPLAN`, `GAMEOPTS`, `TENTS`, and `UTILITY`. Its reuse and position as the sole `GAMEOPTS.666` sample identify it as a shared interface activation sound with **Corroborated** confidence. Exact event bindings for the other 101 samples remain **Provisional**.

`DynamixSoundBankDecoder` limits a bank to 64 MiB, a sample to 16 MiB, a bank to 4,096 samples, and rates to 1,000-192,000 Hz. It rejects bad magic, incomplete metadata, invalid rates or lengths, excessive counts, and trailing partial records before exposing sample bytes.

## Indexed PCX images

At least one `.PCC` resource is actually a standard single-plane, 8-bit PCX payload. Decoders must identify the content from its header rather than relying exclusively on its filename extension.

The currently supported subset requires:

| Header offset | Size | Required value or meaning |
| ---: | ---: | --- |
| `0x00` | 1 | Manufacturer `0x0A` |
| `0x01` | 1 | Version (observed `5`) |
| `0x02` | 1 | RLE encoding `1` |
| `0x03` | 1 | 8 bits per plane |
| `0x04`–`0x0B` | 8 | `xmin`, `ymin`, `xmax`, `ymax` as `UINT16LE` |
| `0x41` | 1 | One color plane |
| `0x42` | 2 | Bytes per scanline as `UINT16LE`, at least image width |

Image dimensions are inclusive: `width = xmax - xmin + 1` and `height = ymax - ymin + 1`. Beginning at byte 128, a byte with top bits other than `11` is one literal index. A byte with top bits `11` stores a run length in its low six bits and is followed by the repeated index. Decoding fills `bytesPerLine × height`; per-row padding beyond `width` is discarded.

The final 769 bytes are marker `0x0C` followed by 256 RGB triples. The implementation requires the decoded scanline stream to end exactly at that marker and bounds the total pixel allocation. It can expand indices to RGBA8 with alpha 255.

The stored `richard.pcc` entry validates as 195×203 pixels; its decoded index XXH3-128 is `fca967cbf7655d5ccff840ca193a26ee`. This structure and hash are **Confirmed for the hashed release**. It does not yet prove that all `.PCC` resources are PCX or that compressed `.PCX` entries contain identical payloads after outer decompression.

## Indexed RGB palettes

The stored `SKIRMISH.PCX` resource is not a conventional PCX stream despite its name. It is exactly 64,000 bytes and decodes as a headerless 320x200 indexed plane using the separate 768-byte `SKIRMISH.PAL`. `RawIndexedImageDecoder` therefore requires caller-supplied dimensions, an exact `width * height` payload, a valid 256-color palette, and bounded pixel counts. The dimensions, framing, palette association, and visible combat-shell geometry are **Confirmed for the hashed release**.

All five byte-stored `.PAL` entries are exactly 768 bytes: 256 consecutive red, green, and blue byte triples with no header or trailer. Observed channel values span nearly the complete byte range (maximum values 252–255), so they are already 8-bit color components and must not be multiplied from VGA 6-bit values. Exact length, byte interpretation, and component range are **Confirmed for these resources**. Association with particular images or CSF sequences remains **Provisional**.

The importer assigns the `palette` kind only after exact-length validation. `stored-palette-report.txt` records provenance, component range, and a stable XXH3-128 without exporting palette bytes.

## CSF chunk sequences

All five byte-stored `.CSF` resources share this structure:

| Offset | Size | Type | Meaning | Confidence |
| ---: | ---: | --- | --- | --- |
| `0x00` | 2 | `UINT16LE` | Observed magic `0x4A32` | Confirmed |
| `0x02` | 4 | `UINT32LE` | Chunk count | Confirmed |
| `0x06` | `count × 4` | `UINT32LE[]` | Ordered chunk byte lengths | Confirmed |
| after table | sum of lengths | bytes | Concatenated chunk payloads | Confirmed boundaries; semantics unknown |

There are no offsets in the observed table: each chunk begins immediately after its predecessor. The bounded parser rejects oversized counts, truncated tables, chunks beyond the resource, and any trailing bytes not consumed by the size table.

| Stored resource | Chunks | Minimum chunk | Maximum chunk | Total payload |
| --- | ---: | ---: | ---: | ---: |
| `men8.CSF` | 723 | 71 | 3,412 | 1,247,405 |
| `credit.CSF` | 32 | 50,717 | 52,472 | 1,648,002 |
| `ica.CSF` | 337 | 599 | 5,878 | 827,986 |
| `ics.CSF` | 337 | 599 | 5,878 | 827,273 |
| `icw.CSF` | 337 | 599 | 5,864 | 824,883 |

The table and scanline decoder establish an indexed-frame sequence. Treating `.CSF` as dialogue based solely on its extension is **Disproved** for these five resources.

Every one of the 1,766 stored chunks begins with two positive `UINT16LE` values that fit conservative image bounds. Their observed distribution is:

- `credit.CSF`: 32 chunks at 275×190.
- `ica.CSF`, `ics.CSF`, and `icw.CSF`: 337 chunks each at 80×80.
- `men8.CSF`: 720 chunks at 90×90, plus one each at 3×9, 39×13, and 57×14.

The values, population counts, and width/height interpretation are **Confirmed** by exact scanline decoding across every chunk.

`MEN8.CSF` is the resolver-owned strategic-encounter unit sequence: setup `0x258FC` references it together with the 1024x728 indexed `BATTLE.PCX` background. Bounded previews show its ordinary frames as transparent foot and mounted units under the background palette. The resource association, dimensions, and broad visual population are **Confirmed**; frame-to-unit/state indexing and runtime placement remain **Provisional**.

Renderer `0x28AD8` now confirms the ordinary-frame index: lane + category + five heading frames + phase remainder + state. The six lane/category groups, eight headings, five phase frames, and three state blocks occupy frames 0-719; frames 720, 721, and 722 are the selected-index overlay, armed-first-control overlay, and initial control-strip image respectively. Its prior pointer-array sort invokes comparator `0x289CC`: non-positive-strength records first, then record `+0x08`, then `+0x04`, both ascending; exact comparator ties have no source-defined ordering. It centers each 90x90 ordinary sprite at the live record coordinate after scroll translation, then `0x28B5C` draws one frame-720 overlay for every selected-list byte in list order at live x/y minus scroll and `(5,7)`. Setup draws frame 722 at first control-rectangle origin plus `(20,6)`; the armed branch draws frame 721 at that origin plus `(15,6)`. Semantic labels remain **Provisional**.

The kind-1 `FFMOUSE.CSF` sequence uses the same container and scanline grammar: six frames, all 20x20. Startup `0x2A4DC` passes object-2 resource string `+0x3A70` to mouse initializer `0x72180`; that routine forwards the name to resource loader `0x18850`, proving that this sequence is the active mouse-cursor source. Rendering it with the `ICONTEMP.PCX` palette produces coherent sword, hourglass/wait, travel arrows, speaking mouth, targeting, and pointing-hand cursors in frames 0-5. The loader identity, dimensions, frame count, and visual identities are **Confirmed**; palette association and the runtime's contextual travel/talk/target/pressed-hand selection are **Corroborated**, while exact executable timing and hourglass dispatch remain **Provisional**.

Decoded-image comparison confirms that `ica.CSF`, `ics.CSF`, and `icw.CSF` are matching 337-frame isometric estate atlases for autumn, spring/summer, and winter respectively. Each frame index preserves the same broad visual role across the three files, and all three render coherently with the palette embedded in `ICONTEMP.PCX`. The runtime therefore selects the atlas by campaign month and uses that palette. The seasonal filename interpretation and palette association are **Corroborated** by the rendered populations and screen context; the precise month boundaries and individual frame-to-estate-data mapping remain **Provisional** pending executable table recovery.

### CSF frame payload

After the four-byte dimension header, each of `height` scanlines is encoded independently:

| Size | Type | Meaning |
| ---: | --- | --- |
| 1 | `BYTE` | Number of segments in this scanline |
| variable | segments | Consecutive commands whose lengths must sum to `width` |

Each segment begins with an operation byte and a `UINT16LE` pixel length. Lengths must be positive and may not extend beyond the declared width.

| Operation | Following bytes | Meaning |
| ---: | --- | --- |
| `0` | `length` palette indices | Copy literal opaque pixels |
| `1` | none | Advance by `length` transparent pixels |
| `2` | one palette index | Fill `length` opaque pixels with that index |

The decoder requires every row to total exactly the width and every chunk to end exactly after the final row. It bounds pixel allocation, rejects unknown operations and truncated commands, and returns separate index and alpha arrays. Palette association remains unresolved; CSF frames appear to rely on a palette supplied by their surrounding screen rather than embedding one.

All 1,766 stored chunks decode with this grammar. Across the five resources they contain 119,292 literal segments, 254,433 transparent-skip segments, and 19,573 fill segments. `csf-report.txt` records per-resource counts and stable XXH3-128 values over decoded dimensions, indices, and alpha masks, and now applies the same validation to kind-1 CSFs. These command meanings and stored-resource population results are **Confirmed for the hashed release**.

### Rejected codec identifications

Documented Dynamix inner chunks can use an LSB-first 9-to-12-bit LZW variant. Applying that bitstream directly to outer kind-1 blocks produces undefined initial dictionary codes and no valid output. The claim that outer kind 1 is raw inner-chunk LZW is therefore **Disproved**. The bounded inner LZW decoder remains separate and must not be selected from the outer kind value.

Classic LH1/LZHUF with a 4 KiB history window and independently reset 16 KiB output blocks was also tested against all 187 kind-1 `.PCX` and `.PCC` entries in the GOB. It produced no valid PCX headers under either tested bit order. That exact interpretation is **Disproved**; the similar compression ratio and block count are not sufficient evidence to label kind 1 as LH1.

Reading kind-2 data with a 12-bit ceiling produces the exact four-byte PCX prefix for all four image entries but diverges when the dictionary reaches that artificial ceiling. Byte-, word-, and doubleword-aligned reset hypotheses likewise fail. Those reset and 12-bit-ceiling variants are **Disproved**; executable analysis instead confirms the uninterrupted 9-to-14-bit stream specified above.

## Dilemma text resources

The 30 `DILEM0.DAT` through `DILEM29.DAT` resources are marker-delimited 7-bit ASCII text ending optionally in DOS EOF byte `0x1A`. Each contains a stable numeric identifier, declared age, title, and CSF reference, a multi-line prompt, and exactly three choice records. The declarations form six groups of five definitions for ages 12 through 17. Each choice declares a scoring attribute, high/low breakpoints, and win/draw/lose outcomes; each outcome contains text and a counted table of named integer attribute modifiers. The marker spelling contains two harmless inconsistencies in the original data, so the parser dispatches on the stable prefix character and validates the following typed row rather than matching commentary prose. This structure, including the age grouping, and all 30 instances are **Confirmed** for the hashed release. Static analysis also confirms selection number `(age - 12) * 5 + random(0..4)` and inclusive high/draw-low outcome bands; gameplay evidence is recorded in [`original-findings.md`](original-findings.md).

`DilemmaTextDecoder` bounds input size, accepts ASCII only, validates identifiers, counts, integer rows, unique choices, and the complete outcome set. `ImportedDialogueRepository` resolves a locally imported definition by stable number and maps its named attributes into the typed core interpreter. The inspector records only structural counts and text lengths in `dilemma-text-report.txt` and compact breakpoint/modifier metadata in `dilemma-rules-report.txt`; original prose remains confined to ignored `UserContent`.

## Weapon-store text resource

`WEAPONS.DAT` is a CRLF-delimited printable-ASCII table containing 40 records of exactly six lines each. For every record the lines are, in order: movie filename (or `#` when absent), one non-negative numeric field whose semantics remain unknown, `SWORDS.CSF` image-frame index, internal item identifier, purchase price in shillings, and display description. This framing, the 40-record population, prices, identifiers, and image indices are **Confirmed** for the hashed release. The first numeric field is deliberately exposed as unknown rather than assigned a speculative gameplay meaning.

Image frames and item identifiers normally advance together from 0 through 36. The final three records demonstrate that they are separate fields: record 37 maps frame 38 to item 37, record 38 maps frame 37 to item 38, and record 39 repeats frame/item pair 38/37 with a different unknown value. `SWORDS.CSF` contains exactly 39 frames numbered 0 through 38. Rendering it with the `SWDTEMP.PCX` palette reproduces the Battle Sword shown in the owner screenshot at the frame index declared by record 2, so that palette and table-to-frame binding are **Confirmed**. The purpose of duplicate record 39 remains **Provisional**.

`WeaponStoreDecoder` rejects oversized input, non-printable/non-ASCII bytes, bare line endings, incomplete records, excessive record counts or text fields, and negative/non-decimal numeric fields. Runtime equipment definitions store the original record number explicitly; the UI uses the parsed record's frame, price, and locally held description without copying original prose into tracked files or matching on display names. `weapon-store-report.txt` emits only numeric fields, movie presence, and description lengths.

For owner-local catalog reconciliation, the inspector accepts `--weapon-text=0,1,...` (or `all`) and writes the explicitly selected descriptions to the ignored `weapon-store-text.txt`. That report may be inspected locally but must not be committed or redistributed.

`BUYSELL.CSF` is a four-frame indexed overlay sequence associated with the `SWDTEMP.PCX` palette. Frames 0 and 1 are 81x71 versions of the View stone without and with its label; frames 2 and 3 are 100x32 stones labelled Sell and Purchase. Those dimensions, pixels, labels, order, and palette association are **Confirmed** by bounded decoding and palette-assisted rendering. Selecting the View variant from the store record's movie marker and selecting Sell/Purchase from current ownership are **Corroborated** by the table population and visible store behavior; exact input timing remains untraced.

## Conversation database

`ALL.CIF` is an index of 1,311 eight-byte records, each containing a unique non-negative node identifier and a unique 32-bit little-endian byte offset into `ALL.CBF`. Sorting those offsets partitions the 1,514,658-byte body into complete node records; the first begins at offset zero. Every node has a fixed `0x348`-byte binary header followed by zero or more null-terminated printable-ASCII strings. Header byte `0x08` is the prompt-variant count, byte `0x48` is a response count from zero through five, and bytes `0x49`-`0x4B` are the invariant marker `65 3A 5C`. Response target node identifiers begin at `0x4C`; when the response count is zero, the same first dword is the automatic continuation target. Target zero terminates a branch; every nonzero response and continuation target resolves to another CIF node.

For nonempty nodes, the first string is a PCC filename, the second is the displayed speaker, the final N strings are the N response labels, and the intervening strings are the exact number of prompt variants declared at `0x08`. Across the complete owned database, 29 nodes are empty sentinels, the remaining nodes contain 2,062 prompt variants, and 2,696 responses divide into 1,119 terminal and 1,577 linked branches. Sixty-three zero-response records carry continuation fields, of which 55 link onward and 8 terminate. The executable interpreter at `0x17498` selects between response and continuation handling and reads the zero-response target at `0x174A4`/`0x174C9`; `0x176BA`-`0x176E7` selects a prompt variant uniformly through the game's random source when more than one exists. `0x17E74` renders every declared response in one of five rows at `(41,302+n*35,558,35)`, and `0x17E3C`-`0x17E5B` accepts number keys 1-5 within the declared response count. These framing, string-role, count, prompt-selection, continuation, presentation, and link invariants are **Confirmed** for the hashed release. The command arrays applied after a node/response and their campaign-state bindings remain under analysis.

The fixed header also contains 30 signed action-ID slots for each of five response positions beginning at `0x60` with a `0x78`-byte stride, plus 30 node-level slots beginning at `0x2B8`; unused slots are `-1`. The interpreter scans the selected response array at `0x1753B`-`0x17577` and the node array at `0x17507`-`0x17539`, invoking `0x687E4` for every populated identifier. Across the owned database, 423 node-level and 2,318 response-level action references are populated.

`ALL.TMI` is a 5,516-byte index: one leading dword (observed value 100, meaning not yet identified) followed by exactly 689 unique positive `(identifier, ALL.TMB offset)` pairs. Group records in the 427,492-byte `ALL.TMB` begin with an action count followed by that many action offsets. Action records have kinds 1 (`When`, one branch), 2 (`IfElse`, two branches), and 3 (`Evaluate`, no branches), plus an expression offset. Expressions contain N value offsets and N-1 operators. The relocated jump table at source `0x6ABBC` maps codes 1-8 to `!=`, `<=`, `>=`, boolean AND, boolean OR, `>`, `<`, and `==`, respectively. The reachable owned-data graph uses codes 3-8; codes 1 and 2 are supported by the executable but absent. Values are nested expressions, integer literals, or calls to function IDs 3 through 9 with expression arguments and an optional inversion flag.

The bounded recursive decoder validates all 689 groups, 2,267 unique actions, 10,373 unique expressions, and 13,352 unique values, rejecting invalid counts, offsets, kinds, arities, flags, operators, excessive depth, and cycles. Every distinct conversation action ID resolves except `5011`, which is retained and reported as an anomaly in the original data. Structural decoding and function roles are **Confirmed**. The scope-1 adapter maps selectors `0/2/3/5/6/7/8` to wealth/honor/fame/piety/strength/stamina/intelligence, and produced or consumed item selectors `0` through `23` to named campaign inventory. The only other item operand is a producer-less `161` check in Victoria's final shield-offer gate; it is retained as raw original state rather than assigned a speculative identity.

The executable maps those conversation item selectors to physical possession slots through a little-endian 16-bit table at object 2 `+0xA938`: helper `0x2250C` reads the high word of the dword at `+0xA936 + 2 * selector`, then passes that slot to increment helper `0x43100`. `0x2252C` and `0x2254C` use the same lookup for clearing and testing. The 24 supported selector-to-slot values, in selector order, are **Confirmed** from the hashed executable:

| Selectors | Physical possession slots |
| --- | --- |
| 0–7 | 65, 17, 10, 19, 21, 49, 18, 9 |
| 8–15 | 20, 53, 54, 55, 15, 6, 58, 59 |
| 16–23 | 14, 61, 6, 63, 64, 67, 68, 69 |

The possession array uses eight bytes per slot at object 2 `+0xC6C4`: the dword at `+0x04` is the live count. `0x43100` increments it, `0x4310C` tests nonzero, and `0x43124` clears it. Enumerator `0x430B0` bounds the array at `0x230` bytes, or 70 slots. Selector 10 is Dragon Slaying Lance and maps to slot `0x36`; selector 15 is Shield of St. George and maps to `0x3B`; selector 20 is Dragon Slaying Armor and maps to `0x40`. These are exactly the three slots tested by dragon worker `0x1B5DA`–`0x1B614`. Name identities come from the existing executable item-name catalog and decoded conversation rewards; the slot relation comes directly from the table and helper control flow. `OriginalConversationBindings.Items`, `OriginalDragonRunScore`, and `Campaign.BeginDragonBattle` retain the mapping. The anomalous selector 161 is outside this 24-entry mapping and remains raw.

`ALL.VTB` is a 12-byte signed-dword header followed by its variable values. The owned file declares 190 elements, an element size of 4, and element kind 5; exactly 190 signed 32-bit zero values consume the rest of its 772 bytes. The decoder requires exact length, the confirmed element size/kind, and a bounded population. These values form the persistent scope-zero state read and changed by action functions 4-6.

`DynamixConversationDecoder` bounds the node population and every record extent, validates IDs, offsets, markers, ASCII strings, response counts, and graph targets, and exposes typed prompt variants and responses without copying original prose into the repository. `conversation-report.txt` contains only aggregate structural counts.

## HAT screen layouts

The decoded `.HAT` population uses a compact fixed-header layout. Integer fields are signed 32-bit little-endian values. The following structure is **Confirmed for the hashed release**:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| `0x00` | 4 | Screen identifier |
| `0x04` | 4 | Screen origin X |
| `0x08` | 4 | Screen origin Y |
| `0x0C` | 4 | Screen width; observed as 640 |
| `0x10` | 4 | Screen height; observed as 480 |
| `0x14` | 4 | Region count |
| `0x18` | 13 | Null-terminated/padded background resource name |
| `0x25` | 3 | Unknown tag; observed as little-endian `0x0045C0` |
| `0x28` | `count × 24` | Ordered region records |

Each region record contains six 32-bit fields: identifier, X, Y, width, height, and enabled flag. Some rectangles deliberately extend beyond the 640x480 viewport and are clipped, so the bounded parser requires intersection rather than full containment. One observed file ends with the DOS text marker `0D 0A 1A`; no other trailing data is accepted. `hat-layout-report.txt` records every decoded descriptor without exporting its original bytes.

`CGOPTS.HAT` maps `CHAR_OPS.PCX` regions 0–2 to Generate New Character, Choose Pre-generated Character, and Choose Character Name, followed by red, green, and blue shield regions 3–5. `PREGEN.HAT` maps `PREGEN.PCX` region identifiers 0–5 to the six profile panels. `FOPTS.HAT` is screen 16's `TACTICAL.PCX` office descriptor; `FCASTLE.HAT` instead declares screen 19 and `FIEFMGMT.PCX`. Home setup `0x302A0`-`0x30492` binds all three callback classes to `FOPTS` regions 0-6 and 8-9 but never region 7. That enabled region still indexes label `JUMP!!`; it is intentionally label-only and has no action. `FCASTLE`, `FVILLAGE`, `FFARM`, and `FFOREST` reuse the management shell with different row counts. In all four, the final region at `(532,0,86,16)` overlays the shell's visible `Full Screen` text; it is a fullscreen control, not a wealth field. The geometry, numeric identifiers, filenames, backgrounds, Home callback membership, and fullscreen identity are **Confirmed**; remaining management action semantics are **Corroborated** or **Provisional** as recorded per screen.

`FWARPLAN.HAT` declares five 40×38 army buttons in regions 0–4, a 114×38 Field Army control in region 5, a 138×38 Send Out Spy control in region 6, a 198×38 membership control in region 7, three 300×14 unit rows in regions 8–10, OK/Cancel in regions 11–12, and a 100×20 army-name field in region 13. Decoded `WARPLAN.CSF` has 22 frames whose dimensions match those controls exactly: frames 0–14 are five selected/normal/disabled army triplets, frames 15–17 are Join/blank/Leave Selected Army, frames 18–19 are Field Army/blank, and frames 20–21 are two Send Out Spy states. Geometry, dimensions, palette association, visible frame identities, and the name field are **Confirmed**; independent army movement dispatch remains **Provisional**.

`ICONMAP.HAT` declares the map's three footer regions without overlap: region 11 Home is `(18,455,180,24)`, region 13 location status is `(200,455,200,24)`, and region 12 Village is `(400,455,200,24)` in the 640-by-480 source canvas. Region 10 is the map viewport `(19,8,370,433)`, region 0 the inset `(422,4,199,159)`, and region 9 the right information panel `(422,178,199,264)`. `EstatePresentationDefinitions.From` imports these numbered regions and `DrawEstateMap` draws each label inside its own scaled rectangle. These coordinates and non-overlap are **Confirmed** by the decoded owned HAT record; the former Home collision in the host came from a second global notice drawn over the footer, which the Map draw path now suppresses.

## Imported-content manifest

`UserContent/manifest.json` is a version-2 UTF-8 JSON document with the XXH3-128 of the source disc image and an asset array. Every asset records a stable source identifier, root-relative generated path, runtime kind, exact byte length, and lowercase XXH3-128. A manifest of any other version fails verification, so an import made before the move to XXH3-128 is imported again. Identifiers and paths must be nonempty and case-insensitively unique; paths must be relative and resolve beneath `UserContent`; sizes must be non-negative; both digests must contain exactly 32 hexadecimal characters.

The source-image digest `b915491c5bdce934ca216d2ceebec0ce` identifies the supported GOG English release (**Confirmed** from the owned installation). The importer announces that match before extraction and emits a warning for other hashes while continuing through the same bounded decoders rather than rejecting a potentially compatible legal copy.

`Conqueror.Import --verify` checks those structural invariants and then requires every listed file to exist with the declared size and digest. The install and repair paths run the same verification after writing the manifest. Generated files and the manifest use same-directory temporary files plus atomic replacement; byte-identical destinations are retained rather than rewritten. `--uninstall` prevalidates all manifest paths before removing only those files and the manifest, preserving everything unlisted. These manifest rules describe independently authored importer metadata, not an original-game format.

The desktop runtime supports only verified original assets. `ImportedContentCatalog.LoadRequired` requires the recognized GOG English source-image hash, verifies every manifest-owned path, size, and digest through `ImportManifestVerifier`, and checks every currently mapped art, layout, animation, sound, movie, weapon-store, conversation/action database, all 30 youth dilemmas, and first-person-scene dependency before constructing the game. Missing, incomplete, damaged, or unsupported imports fail before the graphics loop begins and are reported through the normal startup diagnostic path. Assetless and placeholder presentation are deliberately outside the supported design; `ResourceAndDefinitionTests.RequiredAssets.cs` locks the failure contract, while the platform smoke test validates the real installed-content success path.

`CONFONT.CSF` is the executable-loaded text face. Object-2 string `+0x3A78` names it; application initialization `0x2A758` passes that string to `0x644B0`, which calls the CSF loader `0x18430` and stores the returned face at `+0xE03C`. Generic renderer `0x644D4` consumes null-terminated byte text, uses each byte as a four-byte frame-table index at face `+0x0C`, draws that frame, then calls `0x6E0D0`; the helper returns the frame's initial signed word, its width, as the next x advance. The owned `CONFONT.CSF` has 256 frames, every 13 pixels high and 2 through 10 pixels wide, so the source face is proportional with direct byte/ASCII indexing. Its CSF scanlines are masks: renderer `0x644D4` forwards its caller's palette-index argument through frame blitter `0x7CEB0` to `0x8A564`, whose opaque runs fill the destination with that supplied byte. Direct call sites supply, among others, `0xF7`, `0xF9`, and `0xB2`; glyph pixels therefore do not select their own visible colors. `font.CSF` is a separate 256-frame fixed 11-by-13 sibling with no direct executable load established. `ImportedFonts` requires the confirmed `CONFONT.CSF` asset and `OriginalUiFont` rebuilds its alpha masks as tintable textures, retaining the proportional width advance and caller color control while fitting it into the existing host canvas. Its two-thirds host-canvas policy converts a cumulative source advance, never each narrow glyph independently, so policy rounding cannot distort the confirmed relative source spacing. The active screen-palette association and screen-specific vertical/layout rules remain **Provisional**.

Before any generated file is written, the importer inventories every selected raw disc file, every decodable archive entry's declared expanded size, the loose GOB and its decodable entries, and each CDDA WAV header plus PCM extent. The preflight sums missing destinations and reserves scratch space for the largest possible atomic replacement, rejects duplicate or escaping planned paths, and compares that requirement with the output volume's available space.

## Reimplementation save format

Campaign slots are UTF-8 JSON representations of `CampaignState`. Schema 1 adds the required non-negative `SchemaVersion` field; a missing field identifies schema 0, the unversioned format written by every earlier prototype. Loading migrates schema 0 in memory before campaign-state repair, accepts schema 1, and rejects negative or future versions rather than silently discarding unknown state. The former `campaign.json` filename remains a schema-independent slot-1 fallback.

Each numbered slot uses `campaign-N.json`, for N from 1 through 5. A save is written and flushed to a uniquely named file in the same directory, then moved over the destination so a partial JSON document is never exposed as the current slot. Before replacing a readable current slot, the slot manager atomically refreshes `campaign-N.json.bak`; it deliberately preserves an existing valid backup when the current primary is already corrupt. Inspection and loading try the primary, then its backup, and finally the old slot-1 filename. These rules are independently authored reimplementation behavior rather than claims about the original save format.

`autosave.json` and `autosave.json.bak` use the same schema, atomic replacement, and recovery ordering as manual slots but form a separate stream. Stable campaign entry, travel/time advancement, and resolved battle transitions update it; loading it never changes or consumes the selected manual slot.

`settings.json` is a separate version-2 reimplementation document containing CD music, effects, speech, animation, fullscreen, integer-scaling, three normalized channel volumes, and reduced-motion state. Version 1 migrates additively with default volumes and motion behavior. The file uses the same atomic-write and previous-valid-generation recovery policy as campaign slots; malformed, missing, and future-version settings fall back safely to defaults. Settings never alter campaign simulation state.

## Disc image and audio

`game.ins` is a cue sheet. Track 1 is MODE1/2352 data; each raw sector exposes its 2,048-byte ISO-9660 payload beginning 16 bytes into the sector. The first audio-track index determines the data-track sector count. Later tracks are Red Book CDDA: 44,100 Hz, stereo, signed 16-bit little-endian PCM, with 2,352 bytes per sector. The importer wraps those samples losslessly in a RIFF/WAVE header.

The raw-sector bounds, ISO directory traversal, cue timestamps, and WAV sample preservation are covered by synthetic executable specifications.

## Reproducible reports

`tools/Conqueror.Inspect` generates these ignored reports under `analysis/original`:

- `cd-manifest.txt`: ISO paths and byte sizes.
- `gob-directory.txt`: outer GOB directory fields.
- `gob-compression-report.txt`: kind-1 block counts plus complete kind-1 and kind-2 decoding validation.
- `scene-res-report.txt`: per-scene entry, storage-kind, and block totals.
- `scene-texture-report.txt`: per-scene raw-texture counts and distinct dimensions.
- `smacker-report.txt`: version, dimensions, timing, frame counts, audio profiles, packet totals, decoded-audio totals, and final-frame hashes for every movie.
- `resource-extension-report.txt`: aggregate extension and storage-kind inventory.
- `stored-image-report.txt`: dimensions and decoded pixel-index hashes for stored, kind-1, and kind-2 PCX-compatible payloads.
- `csf-report.txt`: storage kind, chunk sizes, dimensions, decoded segment totals, and stable frame-sequence hashes for byte-stored and kind-1 CSF containers.
- `stored-palette-report.txt`: provenance, component ranges, and hashes for validated 256-color RGB palettes.
- `hat-layout-report.txt`: resource filenames, decoded screen identifiers, background names, and region records for HAT layout descriptors.
- `executable-data-xrefs.txt`: instruction addresses that reference requested object-relative LE data offsets.
- `dilemma-text-report.txt`: stable dilemma/scene identifiers plus choice, outcome, modifier, and text-length counts without original prose.
- `dilemma-rules-report.txt`: choice scoring attributes, low/high breakpoints, and outcome modifiers without original prose.
- `weapon-store-report.txt`: store record indices, movie presence, unknown numeric values, image/item indices, prices, and description lengths without original prose.
- `conversation-report.txt`: aggregate node, empty-sentinel, prompt-variant, response, and continuation counts without original prose.
- `conversation-node-report.txt`: requested node identifiers with offsets, continuation targets, structural counts, portraits, and speaker identifiers; prompts and response prose are omitted.
- `sound-bank-report.txt`: bank provenance, sample counts, distinct rates, and aggregate payload sizes without exporting audio.
- `artifact-hashes.txt`, `string-hits.txt`, and `executable-disassembly-report.txt`: provenance and targeted executable evidence.

The reports are regenerated from the user's installation and must never be committed. Stable conclusions belong here and confidence-scoped gameplay conclusions belong in [`original-findings.md`](original-findings.md).

## Strategic movement records

War Planning spy state is separate from persistent intelligence locations. Action handler `0x36720`-`0x367D4` requires temporary wealth at object-2 `+0x1A090` to be at least `0x50`, subtracts exactly 80 through helper `0x37340`, and increments the screen-local pending count at `+0x1A0EC`. Initializer `0x36E87` clears that pending count whenever War Planning opens. OK handler `0x36AFC` applies the transactional army edits, then calls `0x38C68` once per pending assignment; that helper admits only the first by setting single live-spy flag object-2 `+0xAE68`, and rejects later attempts while it remains set. The manual's description of a spy traveling until he observes troop movement is corroborated by strategic update `0x3C290`, which calls report routine `0x38C84` immediately before the movement update at `0x3C088`. The report scans five movement records from object-2 `+0x1AB48` at stride `0x118` in slot order; active flag `+0x00`, three unit counts `+0x1C/+0x20/+0x24`, and location key `+0x28` feed the `Spy Report` dialog. If no record is active, the live-spy flag remains set. Considering the first active record consumes it; a resolvable location produces the report. Movement update `0x3C088` dispatches state 1 to `0x38D78`, state 3 to `0x3908C`, and other states through `0x4A300`. Generator `0x3BE58` enforces the five-record cap and contains a strict random result `> 0x60` gate after its `0x1388` accumulator boundary. Cost, capacity, lifetime, scan population/order, and the pre-movement trigger are **Confirmed**. `Campaign.AssignSpy`, `StrategicEnemyMovement`, and `StrategicSpyReport` now preserve that report boundary; the former monthly unknown-garrison proxy is **Disproved** and removed. The runtime's processor-independent daily generation checkpoint, source/destination choice, equal three-way detachment, and arrival reinforcement remain **Provisional** pending exact strategic-clock, route, and lord-record recovery.

Further tracing maps these **Confirmed** movement-record fields; unnamed bytes remain deliberately uninterpreted:

| Record offset | Size | Meaning |
| --- | ---: | --- |
| `+0x00` | 4 | active flag (`1` means live) |
| `+0x0C` | 4 | routed-mode path-complete flag |
| `+0x14` | 4 | target movement-slot index used by mode 3 |
| `+0x18` | 4 | routed-mode waypoint count |
| `+0x1C/+0x20/+0x24` | 4 each | swordsmen, halberdiers, knights |
| `+0x28` | 4 | origin/current strategic location key reported by the spy |
| `+0x2C` | 4 | lord/person key returned by location helper `0x4377C` |
| `+0x30` | 4 | routed-mode current waypoint index |
| `+0x34` | 4 | movement mode; 1 is direct destination movement, 2 is waypoint-buffer movement, and 3 pursues another live strategic record by slot index |
| `+0x3C/+0x40` | 4 each | integer destination coordinates |
| `+0x44/+0x48` | 4 each | current map-grid coordinates |
| `+0x5C/+0x60` | 4 each | current single-precision x/y position |
| `+0x64/+0x68` | 4 each | normalized single-precision x/y direction |
| `+0x6C` | 4 | routed-mode heap pointer to packed x/y waypoint pairs |

Mode-1 constructor `0x3AC5C` resolves the location's lord, initializes current and destination positions through the map-coordinate helpers, and normalizes `(destination - current)` into `+0x64/+0x68`. Corrected instruction-level tracing at `0x38AF3`-`0x38B03` **Disproves** the earlier force formula: initializer `0x38A8C` counts that lord's active resolvable household records through `0x43004`, computes `floor(floor(unsigned lord byte / 4) / 3)`, adds that quotient to the household count, and writes the result equally to all three troop fields. A zero result leaves swordsmen/halberdiers at zero and writes one knight. Location 7 uses the separate `176 - 0x43558()` household source. The corrected ordinary-location formula is **Confirmed**; the semantic name of the lord byte and the special location-7 population are **Provisional**.

The supporting property table is 14 records of 15 bytes at object-2 `+0xB8EC`. Helpers `0x438C8` and `0x438F8` return unsigned coordinate pairs from `+0x01/+0x03` and `+0x05/+0x07`; `0x4377C` returns the person/lord byte at `+0x09`; and `0x43640`/`0x43658` get/set the garrison byte at `+0x0C`. `+0x0A` is the 16-bit property-list link manipulated by `0x43164/0x431D0` and initially equals `0x00FF`. The initial numeric rows are:

| Row/name | `+00` | grid x/y | map x/y (8.8) | lord | link | garrison | `+0D/+0E` |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 0 York | 1 | 134 / 59 | `2A80` / `049C` | 13 | `00FF` | 18 | 0 / 0 |
| 1 Lincoln | 2 | 143 / 100 | `2D00` / `07E4` | 20 | `00FF` | 22 | 0 / 0 |
| 2 Chester | 3 | 90 / 115 | `1C70` / `0910` | 35 | `00FF` | 24 | 0 / 0 |
| 3 Gloucester | 4 | 112 / 187 | `2300` / `0EB0` | 59 | `00FF` | 28 | 0 / 0 |
| 4 Warwick | 5 | 118 / 159 | `2580` / `0C6C` | 73 | `00FF` | 18 | 0 / 0 |
| 5 Oxford | 6 | 126 / 187 | `27B0` / `0ED8` | 88 | `00FF` | 25 | 0 / 0 |
| 6 Cambridge | 7 | 161 / 167 | `32A0` / `0D20` | 96 | `00FF` | 18 | 0 / 0 |
| 7 London | 8 | 157 / 213 | `3160` / `10A4` | 100 | `00FF` | 90 | 0 / 0 |
| 8 Norwich | 9 | 181 / 137 | `38E0` / `0AF0` | 1 | `00FF` | 24 | 0 / 0 |
| 9 Colchester | 10 | 177 / 183 | `37A0` / `0E60` | 113 | `00FF` | 22 | 0 / 0 |
| 10 Arundel | 11 | 146 / 252 | `2DA0` / `13D8` | 119 | `00FF` | 18 | 0 / 0 |
| 11 Windsor | 12 | 146 / 216 | `2DA0` / `1108` | 143 | `00FF` | 24 | 0 / 0 |
| 12 Dunster | 13 | 75 / 239 | `17C0` / `12C0` | 155 | `00FF` | 15 | 0 / 0 |
| 13 Okehampton | 14 | 68 / 266 | `1590` / `14DC` | 171 | `00FF` | 20 | 0 / 0 |

The person table at object-2 `+0xBA50` contains 176 records of 18 bytes. Its exact numeric layout is `<name-address:dword, group:byte, state5:byte, flags:byte, assignment:byte, x:word, y:word, lord-rating:byte, list-next:byte, state14:byte, state15:byte, state16:byte, state17:byte>`. Household counter `0x43004` scans indices 0-175 and calls predicate `0x437EC`; index 0 is rejected, while indices 1-175 are eligible only when flags `+0x06` has bit `0x01` set. The counter additionally requires the requested group at `+0x04` and a nonzero assignment at `+0x07`. The initialized eligible-and-assigned counts for groups 0-13 are `4, 6, 7, 5, 2, 2, 4, 3, 3, 1, 9, 3, 3, 6`. Following each property's `+0x09` person index to that record's `+0x00` string pointer yields the table names shown above; the same records supply groups 0-13, initial assignments 1-14, and lord-rating bytes `30,24,16,20,18,20,16,70,24,28,33,32,28,24`. Save/load `0x42E4C`-`0x43000` resets the two list heads and every byte-sized person `+0x0D` link to `0xFF`, transfers overlapping four-byte person slices rooted at `+0x06` and `+0x07`, and transfers every complete property row; property `+0x0A` is a distinct 16-bit link. The inspector emits every field numerically without committing executable bytes. The newline-joined 176-row census has XXH3-128 `3ee3a20a2dcf7a7e8c5155e6eb4bad55`. `OriginalStrategicPersonDefinition` and `OriginalStrategicMovement.Persons` preserve it, and regression tests independently derive property-lord, starting-route, and household relationships. Field offsets, all numeric rows, property identities, filters, counts, and save/reset widths are **Confirmed**. The meanings of property `+0x00/+0x0D/+0x0E`, the broader semantics of person `state5/state14..17`, and migration of the runtime's incompatible location indices remain **Provisional**.

Mode-2 constructor `0x3AC5C` obtains a heap-backed route through `0x4A070`; generic updater `0x4A300` advances packed x/y waypoint pairs. The constructor arguments are target person, origin property, mode, and route flag. It rejects an origin whose `+0x0D` is zero, a full five-record roster, or a mode other than 1/2; free-slot helper `0x3A738` chooses the first inactive slot. Mode 1 derives the destination from target-person coordinates and current position from the origin property. Mode 2 delegates to the route builder. In both cases it stores origin `+0x28`, origin lord `+0x2C`, initializes forces through `0x38A8C`, and increments the live count only after successful construction. Route flag 1 indexes a 14-by-14 byte matrix at object-2 `+0xC9D4` by source property and destination person group. A zero rejects the route; these are the 14 diagonal entries plus both Cambridge/Dunster directions (one-based groups 7 and 13). Value 1 loads `rt_<source+1>_<destination+1>.rat` in stored order, while value 2 loads the same lower/higher-numbered file and reverses all waypoint pairs. Route flag 0 is the new-game starting-home family. New-game caller `0x1110A` invokes `0x43670`, which chooses one of seven person indices at `+0xB8C8`, stores that selector at `+0xC9D0`, resolves the selected person's coordinates, and clears its assignment. `0x4A070` uses the same selector at `+0xCA98` to load `sc_0.rat` through `sc_6.rat` without reversal. In selector order the target persons are 17 Scott's Keep, 18 Anne's Castle, 24 Stonetree Castle, 29 MacGibbon on the Hill, 86 Leecastle, 144 Damron Castle, and 166 Sabine's Keep. Their person groups/origin properties are `0,0,1,1,4,12,13`; initial assignments are `1,1,2,2,6,13,14`; and grid coordinates are `(94,40),(129,23),(142,82),(120,100),(125,152),(110,264),(27,306)`. Decoded endpoints corroborate that each path begins near its origin property seat and ends at the selected home castle. Thus 180 directed property routes share 90 canonical `rt_*.rat` files, alongside seven starting-home `sc_*.rat` files. Each file is exactly `4 + 8 * count` bytes: a little-endian signed 32-bit count followed by `count` signed 32-bit `(x, y)` pairs, with no footer or padding. This layout and the two route-selection families are **Confirmed** by `0x1110A/0x43670/0x3AC5C/0x4A070`, the initialized tables, and a structural census of the owned route files: `rt_7_13.rat` is absent while `rt_1_13.rat` is present. The archive contains 93 `rt_*` names because three unused self-route files also exist; all 90 matrix-selected files satisfy the layout and contain 13-197 points. The seven `sc_*` files contain 9-53 points. A waypoint changes when both absolute coordinate differences are strictly below 6 or the current direction has overshot it. Exhausting the route frees `+0x6C`, sets path-complete `+0x0C`, and returns completion signal `0xFFFF`. Zero-length direction, a rounded prior displacement component outside inclusive `-50..50`, or terrain kind 9 instead frees the route, marks the record inactive, and signals completion; updater `0x3C088` then deliberately skips contact resolution because active is no longer 1. `StrategicRouteDecoder` enforces a defensive 4,096-point maximum and exact length without treating that implementation ceiling as an original rule. `PropertyRouteResources` exposes the 90 pair-route identities, `StartingRoutes` exposes the seven home-route bindings, and supported startup requires all 97; `TryGetPropertyRoute` preserves direction and the unsupported pair. Live runtime movement does not yet consume the decoded points. This proves mode 2 is an authored waypoint-resource state rather than the runtime's dated arrival abstraction.

Generator `0x3BE58` checks its accumulator against `0x1388` (5,000 strategic units) before adding object-2 `+0xAEEC`, so a crossing is acted on only by a later call. `+0xAEEC` is not elapsed wall-clock time: new-game reset `0x110D7` initializes it to 1, and handlers `0x3D781`/`0x3D7BD` and `0x3E41E`/`0x3E44B` adjust it within inclusive 1-15 before calling display helper `0x38690`. Both the generator and movement updater read the same speed multiplier once per eligible strategic main-loop pass. At the boundary the generator resets the accumulator, requires fewer than five live records, and accepts only a random result strictly greater than `0x60` from a `0x64`-sized roll. When property-list head `0xB8C0` is absent, it tries mode 2 then mode 1 from global origin `+0x1B0CC`, passing route flag 0. When the list exists, it selects through `0x3BDC0`, tries mode 2 then mode 1 from that property, and passes route flag 1. Before that ordinary construction, a list-present branch whose previously detected property has nonzero `+0x0D` scans live movement slots in order; for each it independently selects an eligible property and attempts a mode-3 pursuit through `0x3A764`, stopping at the first success. Helper `0x3BDC0` scans property indices 0-13 in authored order. It appends an index only when property `+0x00` is nonzero and property `+0x0D` equals 1; no candidates returns `-1`, otherwise the inclusive random helper receives `candidateCount - 1` and its result indexes the compact list. These bytes remain structurally named. `AdvanceGenerationClock`, `ActiveMovementSlots`, `TimedConstructionFallback`, `GenerationPropertyCandidates`, and `SelectGenerationProperty` preserve the exact pre-add clock boundary, bounded speed multiplier, roll consumption, slot order, fallback order, route flag, filter, empty result, and ordinal selection. The original wall-clock rate remains processor-dependent; the reimplementation will apply these units on its explicit fixed update rather than reproduce unrestricted-loop throughput.

The same generator first calls reactive finder `0x3BB4C`, which returns a movement slot and property. Property 7 is delayed while counter `+0xAEA8 < 1000` unless the movement is global player slot `+0xAE6C`. Helper `0x3BE24` detects any live mode-3 record already targeting that slot; with such a pursuer, only the player slot may retry and only after the counter is strictly greater than 50. Successful `0x3A764` construction uses the first free slot, records mode 3 and the target slot, and resets the counter. It then rolls from six values; results 0/1 with remaining capacity try a route-flag-0 mode-2 movement from the detected property, falling back to mode 1. `CanAttemptReactivePursuit`, `HasLivePursuer`, `ShouldAttemptReactiveSpawn`, and `FirstFreeSlot` preserve those **Confirmed** gates without assigning a speculative gameplay name to the finder.

Movement update `0x3C088` advances live records in slot order. After a still-active handler returns completion signal `0xFFFF`, it searches for a person at current `(x,y)`, `(x,y-2)`, `(x+1,y)`, `(x-1,y-1)`, then `(x,y+1)`, accepting the first nonzero identity. A person whose assignment `+0x07` is zero and whose flags pass `0x437EC` enters encounter handler `0x39900` before transfer logic. Otherwise, only a nonzero contacted person whose assignment equals the origin property's `+0x00` state causes all three troop fields to be added to the origin garrison and destroys the movement through `0x3A6A0`; no person, a mismatched assignment, or an ineligible unassigned person calls retargeter `0x3A9BC`. `OriginalStrategicMovement.ContactProbes` and `ResolveCompletedContact` preserve this **Confirmed** order and precedence. The former generic destination-reinforcement interpretation is **Disproved**: transfer is origin-property/state matched, not simply arrival at a selected destination. The current runtime's daily generation checkpoint, incompatible named-location indices, aggregate-garrison source/destination choice, and dated arrival remain **Provisional** and require an explicit migration before replacement.

The generator-first scheduler shell now consumes these formats directly. `IOriginalStrategicResources.TryGridCell` exposes row/column cells so bits 16-23 (`Auxiliary`) supply the person identity used by the five completion probes, with saved full-dword mutations taking precedence. `AdvanceSchedulerPass` runs `0x3BE58` semantics before scanning slots 0-4, constructs into the first free slot, and resolves each handler's completion before visiting the next slot. Destroy path `0x3A6A0`, encounter handoff, matched-origin byte reinforcement, and `0x3A9BC` retargeting are represented explicitly. Reactive initializer `0x38B40` detaches `min(garrison, target troop total + 3)`, combines it with household support capped at 30 (or one when detached is zero), emits exactly three swordsmen when the combined value is at most three, otherwise writes `floor(combined / 3)` to each troop type, and subtracts only the detached component from the byte garrison. These rules and ordering are **Confirmed** by `0x38B40`, `0x3A6A0`, `0x3A764`, `0x3AC5C`, and `0x3C088`. The original timed branch at `0x3BFBD` can read property output after `0x3BB4C` returned false without initializing it; the clean runtime deliberately requires an explicit current-pass detection rather than reproduce undefined stale stack state.

Retargeter `0x3A9BC` resets the movement to direct mode 1 and scans the five player field-army records in slot order. It chooses the first active army at Euclidean distance strictly below the object-2 `+0x537D` single `300.0`; it is not a nearest-army search, and equality is rejected. When none qualifies, it compares distance to the live player map position with distance to the origin property's `0x438F8` coordinate and chooses the nearer heading, with an exact tie selecting the player. The selected point is captured as a heading rather than a dynamically tracked mode-3 target. `SelectRetarget` preserves the first-slot, strict-range, fallback, and tie rules with integer squared-distance comparisons.

Routed updater `0x4A300` maps a one-rounded-direction lookahead through `0x629B0`/`0x3E72C`, then reads its unsigned terrain kind through the 331-byte tile lookup at object-2 `+0xAF78`. Kind 9 is the deactivation case. Calendar helper `0x3866C` supplies a zero-based month to profile setter `0x3E894`. Its 12-dword table at `+0xB720` is `2,2,3,3,3,0,0,0,1,1,1,2`, mapping June-August to profile 0 Summer, September-November to profile 1 Autumn, December-February to profile 2 Winter, and March-May to profile 3 Spring. Transition pointers at `+0xB750` name `tran4.smk/tran2.smk/tran3.smk/tran1.smk`; atlas pointers at `+0xB760` name `ics.csf/ica.csf/icw.csf/ics.csf`, independently identifying the profile order. The current profile at `+0xB610` indexes four speed pointers at `+0xB704`: Summer, Autumn, and Spring all point to the 30-float table at `+0xB614`; Winter points to the distinct table at `+0xB68C`. The primary values are `1,.2,1,.5,1,.5,.8,.8,1,5,.3,.6,.8,.8,2,2,2,1,.2,.5,.5,.5,.5,3,1,1,.5,.5,.5,.2`; the Winter values are `.8,.2,.8,.5,.8,.5,.6,.6,.8,5,.2,.4,.5,.5,1.5,1.5,1.5,.8,.1,.4,.4,.4,.4,2,1,1,.5,.5,.5,.6`. Each displacement is normalized direction times the selected float times the 1-15 strategic speed multiplier at `+0xAEEC`, stored at movement `+0x64/+0x68`. Only the absolute unrounded horizontal product is compared with object-2 double `+0x7389 = 50.0`; strict `< 50.0` applies both components, while equality or greater holds position without deactivating. The prior stored components are independently rounded and validated against inclusive `-50..50` on the next call. Profile identity/month/resource bindings, tables, tile-to-kind mapping, pointer sharing, kind-9 outcome, arithmetic order, and asymmetric strict application gate are **Confirmed** by `0x3866C/0x3E894/0x4A528`-`0x4A607` and the derived `--strategic-terrain-table` report. `StrategicTerrainProfile`, `TerrainProfileForMonth`, `TerrainKindForTile`, `TerrainSpeed`, and `CalculateRoutedStep` expose them with focused tests. Terrain-kind names, live decoded-path consumption, and save migration remain **Provisional**.

The strategic terrain source is GOB resource index `0x124` (292), `icon.jp`. New-game setup `0x3E660` allocates a 200-by-400 dword grid through `0x6E1D0`, then calls resource loader `0x3EE50` with index `0x124`; the owned resource is exactly 320,016 bytes and has XXH3-128 `5ea1f000eeb71430ba7827e6c71edae0`. Its four signed little-endian header dwords are cell width 80, cell height 80, row count 200, and column count 400, followed by 80,000 dwords serialized column-major (columns outside rows). The live allocation is row-pointer first: `grid[row][column]`. Cell low 16 bits are the tile identifier consumed by `0x3E72C` and the `+0xAF78` terrain-kind lookup; bits 16-23 are independently returned by `0x3E708`; the upper byte is preserved but remains semantically unnamed. Exit path `0x3E6CC` writes the complete header/grid through `0x6E980` to `temp.jap` and frees it through `0x6E928`; reload path `0x3E928` reallocates the same dimensions and `0x6E320` restores that scratch file. `StrategicWorldGridDecoder` reverses the file traversal into row-first addressing, `ImportedContentCatalog` decodes it, supported startup now requires `icon.jp`, and `strategic-world-grid-report.txt` reproduces its bounded metadata without committing proprietary bytes. Source identity, dimensions, layout, masks, and lifecycle are **Confirmed**; runtime mutation persistence remains **Provisional**.

Marker initialization `0x12A4C` passes object-2 string addresses `0x4AC` and `0x4BC` to CSF loader `0x18430`, not numeric resource IDs. Static decode yields `icon_men.CSF` and `marker.CSF` respectively; the loader's `0x6A190` preflight is a DOS file-attribute probe. The confirmed marker resources have 128 27-by-35 and four 9-by-8 frames. The dynamically loaded `CHARACTR.DAT` table has 15 character rows and 30 attributes; its zero-based field 19 is `COLOR`. Initializer `0x12A77-0x12AA5` reads row 0's field 19 with `0x15EF0` and stores `COLOR * 8` as every player marker's frame base. Character-options initializer `0x14230` sets that field to 0, and its HAT-region 3/4/5 handlers at `0x145A4-0x1464A` set Red/Green/Blue to 0/3/5. The runtime persists this numeric source field separately from the heraldic name, draws those recovered `icon_men.CSF` frames in physical source order after source terrain, and clips them through the recovered pre-blit projection against the logical map viewport. The campaign-shell palette association remains **Corroborated**; `marker.CSF` overlay semantics and any broader screen sequencing remain **Provisional**.

The movement-marker pass is separate from player-frame selection. Dispatcher `0x3C2CC` calls player pass `0x12F28` and then movement pass `0x3A63C`. The latter scans five `0x118`-byte records at object-2 `+0x1AB48`, skips inactive `+0x00`, truncates live `+0x5C/+0x60`, and sends direct record frame `+0x38` with the `+0x110` image handle to `0x3F0A0`. `0x12A4C` initializes that handle to `icon_men.CSF` and clears the frame. Constructors `0x3A764` and `0x3AC5C` write `(word object2[0xAE88 + 2 * originProperty]) << 3` to `+0x38`; across reachable property indices 0--13 this gives frame indices `88, 8, 16, 72, 32, 56, 48, 56, 64, 72, 80, 88, 96, 64`. Thus `marker.CSF` is not the source of this second icon pass. `OriginalStrategicMovementSlot.MarkerFrame`, construction paths, `BuildMovementDraws`, and `BuildMovementBlits` preserve this **Confirmed** record layout, source ordering, frame selection, projection, and clipping. Its palette association remains **Corroborated**.

Temporary-force lifecycle `0x3B0E4` owns a third `icon_men.CSF` pass. It scans the three active `0x118`-byte records at `+0x1A150` in physical order, converts each `+0x5C/+0x60` coordinate toward zero, and passes global handle `AE80` with fixed frame 3 to `0x3F0A0`. Initializer `0x12A4C` assigns that global handle from `icon_men.CSF`, proving the temporary force uses the ordinary icon sequence rather than `marker.CSF`. `OriginalStrategicMapMarkerPresentation.BuildTemporaryForceDraws`, `OriginalStrategicMarkerPresentation.BuildTemporaryForceBlits`, and the MonoGame map host preserve this **Confirmed** active filter, order, frame, and shared viewport clipping. Caller `0x3C290` runs the lifecycle before spy reporting, player movement, and the modal-gated hostile scheduler; `Campaign.AdvanceOriginalStrategicPass` preserves that sequence on its fixed cadence. Palette association remains **Corroborated**.

The paired `marker.CSF` sequence is the player route-input overlay, not a unit marker. With route-input `AE58` set and modal `AEF8` clear, caller `0x11EE3` gives the selected record's truncated live position and its inline waypoint pairs to `0x11CC0`. That helper subtracts `0x12` from each segment destination x, walks the line in major-axis order with an integer Bresenham error accumulator, and draws only when either axis is more than `0x14` from the prior dot. Each dot selects `frameCounter % 4` from global handle `AE84`, which initializer `0x12A4C` loads from `marker.CSF`; the original advances its starting phase every third unrestricted display pass. `OriginalStrategicRoutePreview`, `BuildRoutePreviewBlits`, and the host's third-fixed-update phase preserve the **Confirmed** geometry and frame sequence while replacing the original processor-dependent wall-clock rate with the fixed host cadence.

Temporary-force creator descriptors select two additional standard route resources. Wrapper `0x3B97C` sends descriptor index 1 through `0x3B0E4` to route loader `0x49ED0`, which copies executable string `scot.rat`; `0x3B9B4` passes index 2 and selects `wales.rat`. They contain 44 and 42 signed point pairs after the ordinary four-byte count. Loader `0x49ED0` stores the decoded heap route at temporary record `+0x6C`, count at `+0x18`, cursor at `+0x30`, current first point at `+0x5C/+0x60`, and projected grid at `+0x44/+0x48`. Each fixed wrapper passes origin property 7; lifecycle writes its resolved lord and mode 2, then random `(0..1)` swordsmen, random `(0..1)+1` halberdiers, and zero knights. Updater `0x4A61C` applies the same recovered terrain/speed math as strategic routed movement, resets the cursor to zero after the final point, and on zero-length, out-of-range-prior-direction, or kind-9 terrain failure marks completion and clears cursor/count while retaining lifecycle ownership. The follow-on lifecycle call compares zero-based calendar month `+0x18` and year `+0x20` independently with descriptor `+0x04=1` and `+0x08=2000`; only month `>=1` and year `>=2000` release route `+0x6C` and clear active state. `AdvanceTemporaryForcePass` preserves this non-lexicographic post-update boundary using the campaign date. The raw action ids `0x2B`/`0x5D` set one-shot flags that `0x3C2D6` consumes in fixed action order only after the ordinary strategic pass; the runtime represents this by `TemporaryForceActionIds` and `ProcessTemporaryForceActions`. Exact visible event labels/availability and descriptor-zero route/payload remain **Provisional**.

World/grid projection is also **Confirmed**. Helper `0x63E20` maps `(row,column)` to route-space anchor `(80 * (row + 1), 20 * (column + 1))`. Because even-column diamonds are centered at `x = 80 * row + 40`, this anchor is their shared horizontal vertex; on odd columns it is the center. Adapter `0x629B0` starts from live camera row `1B104` and column `1B108`, constructs virtual origin `(80 * row + 40, 20 * column + 20)`, and shifts that camera until the route point lies within the inclusive `383 x 434` viewport span. It then translates into viewport `left/top = 20/7`, temporarily installs the adjusted camera, and calls picker `0x6E5A0` before restoring the original globals.

Picker `0x6E5A0` accepts viewport x `20..403` and y `7..441`, begins at y `-53`, and scans columns in pairs at 20-pixel vertical intervals. Even columns begin at x `-20`, odd columns at x `20`; rows advance by 80 pixels and wrap modulo 200, columns wrap modulo 400. Each cell is the exact integer raster `abs(x - centerX) + 2 * abs(y - centerY) <= 40`, including every edge pixel. Shared edges therefore belong to the first cell encountered from the current wrapped camera order rather than to a camera-independent floor formula. The complete owned `rt_*/sc_*` population has 102 resources and 8,304 point occurrences: every point projects, 210 occurrences lie on an inclusive edge, and 34 occurrences at six distinct coordinates select different cells from camera `(0,0)` versus `(199,399)`. `StrategicWorldProjection`, focused center/anchor/edge tests, and the derived route census retain this behavior. Runtime schema 2 must persist/use strategic camera row and column when sampling terrain; canonicalizing an edge independently of camera would change the original.

`OriginalStrategicCampaignState` is the serialized runtime counterpart of these mapped structures. Its property/person collections are mutable copies of the exact definition rows; its five `OriginalStrategicMovementSlot` records retain every confirmed field needed across a save plus route resource, reversal, waypoint cursor, target slot, and floating remainder. Camera bounds are 200 by 400, list heads initialize to `0xFF`, profile derives from the zero-based month table, and terrain changes are unique `(row,column,uint32)` records so all `icon.jp` bits survive. `StrategicSchemaTwoMigration.Prepare` processes dated schema-1 columns in slot order, returns complete troop totals only to valid unconquered Castle/London origins, journals return or dispersal, clears the incompatible roster, deep-copies 14/176 definitions, clamps speed to 1-15, and uses selector `-1` to mean that schema 1 had no recoverable starting-home choice. Tests mutate definitions independently, serialize route/camera/counter/terrain/remainder state, reject lossy shapes, and round-trip the complete replacement.

`IOriginalStrategicResources` carries the decoded evidence into Core without coupling it to `Conqueror.Resources`: `Route(name,reverse)` returns immutable signed coordinates and `TryTerrainCell(world,camera)` returns row, column, and the complete source dword. `ImportedOriginalStrategicResources` eagerly resolves all 90 pair routes and seven starting routes required by `ImportedContentCatalog.LoadRequired`, decodes each once, caches both orders, decodes resource 292 `icon.jp`, and delegates inversion to the Confirmed camera-ordered `StrategicWorldProjection`. Missing, malformed, or unknown route data fails explicitly; binding a prepared save also rejects a persisted waypoint count that differs from the decoded resource. Synthetic legal fixtures cover the whole required-name population, route reversal, raw cell fields, projection, malformed input, and campaign binding. Schema 1 remains active; only the fixed-update scheduler remains as the route/grid activation dependency.

Strategic motion update (2026-09-17): `OriginalStrategicMovement.AdvanceMovementPass` now consumes that provider in physical slot order and implements all three dispatcher branches from `0x3C088`. Direct mode 1 uses normalized direction, object-2 double `+0x4F97 = 0.9`, terrain speed, and strategic speed; exact destination equality remains live until the next crossing, while terrain kind 9 returns the total troop count to the origin property's byte garrison with modulo-256 storage and signals completion. Routed mode 2 consumes decoded points, advances at most one waypoint using the strict `< 6`/directional-crossing rules, validates the previous truncated direction against inclusive `-50..50`, and applies both components only when the unrounded horizontal magnitude is strictly below object-2 double `+0x7389 = 50.0`. Pursuit mode 3 tracks the live movement slot stored at `+0x14`, signals completion without destroying the pursuer when that target is inactive, and on terrain kind 9 changes to mode 1, aims at the origin property's map coordinates, and immediately takes one unscaled normalized step. All successful displacement paths refresh the projected grid coordinates through the same camera-sensitive provider. Six focused regressions cover slot order, direct scaling and crossing, byte-garrison wrap, routed tolerance/completion and direction rejection, live pursuit, impassable fallback, and inactive-target signaling. These findings supersede the earlier statements above that live decoded-path consumption remained Provisional. The dispatcher's generation call, completion/contact resolver, explicit fixed-update cadence, schema-2 activation, and dated-adapter retirement remain separate open integration work.

Strategic player-record checkpoint (2026-09-18): object-2 `+0x1A4B8` is an array of six `0x118`-byte player movement records. Records 0-4 are army records; record 5 is the player-avatar record. Enemy pursuit construction accepts only targets 0-4. Global `+0xAE64` selects any of the six records; distinct `+0xAE6C` identifies the distinguished player record and ranges across all six: 5 is the unjoined avatar, while `0x1133C/0x113A4` exchange it with an army index. New-game initializer `0x110E8` clears every record, marks all six complete, writes both globals to 5, activates/selects only the avatar, clears the chosen starting person's assignment, retains that person's grid, and uses `0x63E20` route anchor `(80*(x+1),20*(y+1))` for destination and current position. Joining activates/selects the army and deactivates the avatar. Leaving preserves the active army but copies its current/grid coordinates to a newly active avatar, clears only the avatar command fields, and resets `AE58/AE64/AE6C` to route-off/avatar/avatar. `OriginalStrategicCampaignState` persists these fields and `Campaign.ToggleArmyMembership` invokes the exact state exchange when replacement state is present.

Field-record constructor `0x12CAC` reads home grid globals `1B0D4/1B0D0`, converts them through `0x63E20`, and adds slot-indexed signed offsets from `A564`: `(-20,-30)`, `(20,-30)`, `(-30,0)`, `(30,0)`, `(0,30)`, `(0,0)`. Counter `AE5C` starts at zero even though avatar 5 is active; the constructor rejects only when its prior value is greater than five, so six successful constructions are possible. It initializes active, state `+0x08`, completion, destination, grid, cooldown, and float current coordinates, but deliberately clears target/count/cursor on the formerly selected record and leaves stale command fields on a reused constructed record. Removal `0x12DD0` rejects only a zero counter, chooses the first other active physical record when removing the selection, restores avatar 5 at the removed record's exact current/grid position when removing `AE6C`, then clears removed cooldown/active/selected/state and marks it complete before decrementing `AE5C`. These fields and transitions are **Confirmed** and mapped by `ConstructPlayerMovementRecord`/`RemovePlayerMovementRecord` with persisted home, count, and state-8 values.

Player fields mapped by handler `0x11554` are active `+0x00`, completed `+0x0C`, tagged target `+0x14`, waypoint count `+0x18`, cursor `+0x30`, destination `+0x3C/+0x40`, grid `+0x44/+0x48`, collision cooldown `+0x54`, terrain kind `+0x58`, float position/direction `+0x5C..+0x68`, and 21 inline x/y pairs beginning `+0x70`. Target bit `0x1000` selects one of five enemy records; `0x10000` selects one of three temporary division-force records; the low byte is the index. Input dispatcher `0x122AC` tests player, enemy, then division hits and consumes a declined target confirmation. Empty clicks use global `AE58` as route-input state, clear a prior target when beginning, and accept at most 20 points despite the 21-pair physical capacity. First-point helper `0x120E4` primes destination, normalized direction, projected grid, and terrain; undo `0x12044` retains stale inline pairs. Completion likewise clears count/cursor/target but not the fixed bytes. Direction components have an inclusive `[-40,40]` guard, and a current kind-9 cell uses an exact 50-unit second probe before stopping. `OriginalStrategicPlayerCommands` and `AdvancePlayerMovementPass` preserve the constructor and execution paths with typed enemy/division identities.

### Village and tournament exterior catalogs

`VILLAGE.DAT` and `TVILLAGE.DAT` are CRLF-delimited ASCII catalogs. Each source record has exactly seven lines: an uppercase PCX background identifier of the form `V<decimal>_<four binary availability digits>.PCX` or `T<decimal>_<four binary availability digits>.PCX`, then six action rows. An action row is `enabled,x,y,width,height ; label`: `enabled` is decimal zero or one, the rectangle is generally bounded by the 640-by-480 source screen, subject to the verified source defects below, and the semicolon introduces a short action label. Disabled rows may have a zero rectangle. The owned release contains 67 `V*` records in `VILLAGE.DAT` and 12 `T*` records in `TVILLAGE.DAT`; every record has its six rows and 54 of 67 VILLAGE backgrounds and 11 of 12 TVILLAGE backgrounds are present in the imported image population. This framing, counts, and partial image population are **Confirmed** by direct bounded decoding of both resources. `0x60854` chooses `TVILLAGE.DAT` only when object-2 globals `+0xDED4` is nonzero and `+0x1B0C4` differs from `+0x1C180`; otherwise it loads `VILLAGE.DAT`. Before that selection, `0x60812` receives a current person identity from `1C180` and calls `0x5C614`, which compares it exactly with `object-2:+0xDC30+4`; equality sets `DED4`. `0x5C648` writes the calendar field returned by `0x38678` to `object-2:+0xDC30+8`; the first Tournament-row handler `0x60BE0` calls `0x5C62C`, which tests the current `0x38678` field against that saved value. Thus the source eligibility gate is person-identity plus a later calendar-field change check, not merely the catalog row's enabled flag. `0x43080` reads the zero-based catalog index from original person record `+0x0F`, rejects values above 66, and returns the value unchanged. The identities of the scheduler fields, calendar component, tournament record selection, and the broader location route remain **Provisional**, but this gate/control-flow relationship and the non-tournament person-to-VILLAGE index are **Confirmed**. `VillageSceneCatalogDecoder` rejects non-ASCII bytes, non-CRLF/LF endings, incomplete records, duplicate/invalid PCX identifiers, invalid flags, numeric fields outside the three verified exceptions, labels over 64 characters, and rectangles beyond the verified right edge of 643 or lower edge of 480. `ImportedContentCatalog.LoadRequired` now requires, decodes, and checks image availability for the seven mapped starting homes before startup.

The verified `VILLAGE.DAT` also has three authored separator defects: one-based line 115 has a doubled comma before x, line 131 has a trailing comma after the fifth number, and line 265 separates x/y/width with spaces. Six rectangles reach x=641 or x=643, up to three pixels beyond the 640-pixel viewport. `VillageSceneCatalogDecoder` repairs only those exact separator spellings and admits a right edge through 643; `VillagePresentationDefinitions.Hit` retains the authored rectangles while the displayed viewport clips the excess. The synthetic `VillageSceneCatalogAcceptsOnlyTheVerifiedSeparatorAndRightEdgeQuirks` regression pins the repaired fields and rejects a larger overrun. Source: direct byte/line inspection of the verified GOG import's `C1086.GOB` entries 450/451; these are source-data defects, not inferred game rules.

## Open questions

1. Recover the semantic meaning of directory field `0x24` and test whether data extents may alias or overlap.
2. Specify nested chunk headers and the exact compression selector used inside decoded resources.
3. Associate CSF sequences and scene textures with their palettes, confirm `.666` event bindings, and specify the remaining PCC, scene `Viewer`/`Scenario`, non-property-route RAT, and FNT semantics as each decoder is validated.

Presentation binding (2026-09-23): `OriginalUiFontDefinition.MaskPixel` expands each decoded `CONFONT.CSF` mask intensity into premultiplied RGBA, matching MonoGame `SpriteBatch` alpha blending and preventing solid glyph rectangles. Generic replacement-screen text is drawn by `PixelFont` for a legible 5x7 grid on the current layouts. Its host-authored punctuation set covers the date, names, counters, and control hints; typographic quotes and dashes normalize to their grid counterparts, while unsupported printable symbols appear as question marks rather than disappearing. The imported source font is retained and validated for screens whose original typesetting can be mapped. The map notice is drawn only in its right information panel.
