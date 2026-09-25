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

Renderer `0x28AD8` now confirms the ordinary-frame index: lane + category + five heading frames + phase remainder + state. The six lane/category groups, eight headings, five phase frames, and three state blocks occupy frames 0-719; frames 720, 721, and 722 are the selected-index overlay, armed-first-control overlay, and initial control-strip image respectively. Its prior pointer-array sort invokes comparator `0x289CC`: non-positive-strength records first, then record `+0x08`, then `+0x04`, both ascending; exact comparator ties have no source-defined ordering. It centers each 90x90 ordinary sprite at the live record coordinate after scroll translation, then `0x28B5C` draws one frame-720 overlay for every selected-list byte in list order at live x/y minus scroll and `(5,7)`. Setup draws frame 722 at first control-rectangle origin plus `(20,6)`; the armed branch draws frame 721 at that origin plus `(15,6)`. Semantic labels remain **Provisional**.

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

`DilemmaTextDecoder` bounds input size, accepts ASCII only, validates identifiers, counts, integer rows, unique choices, and the complete outcome set. `ImportedDialogueRepository` resolves a locally imported definition by stable number and maps its named attributes into the typed core interpreter. The inspector records only structural counts and text lengths in `dilemma-text-report.txt` and compact breakpoint/modifier metadata in `dilemma-rules-report.txt`; original prose remains confined to ignored `UserContent`.

## Weapon-store text resource

`WeaponStoreDecoder` rejects oversized input, non-printable/non-ASCII bytes, bare line endings, incomplete records, excessive record counts or text fields, and negative/non-decimal numeric fields. Runtime equipment definitions store the original record number explicitly; the UI uses the parsed record's frame, price, and locally held description without copying original prose into tracked files or matching on display names. `weapon-store-report.txt` emits only numeric fields, movie presence, and description lengths.

For owner-local catalog reconciliation, the inspector accepts `--weapon-text=0,1,...` (or `all`) and writes the explicitly selected descriptions to the ignored `weapon-store-text.txt`. That report may be inspected locally but must not be committed or redistributed.

`BUYSELL.CSF` is a four-frame indexed overlay sequence associated with the `SWDTEMP.PCX` palette. Frames 0 and 1 are 81x71 versions of the View stone without and with its label; frames 2 and 3 are 100x32 stones labelled Sell and Purchase. Those dimensions, pixels, labels, order, and palette association are **Confirmed** by bounded decoding and palette-assisted rendering. Selecting the View variant from the store record's movie marker and selecting Sell/Purchase from current ownership are **Corroborated** by the table population and visible store behavior; exact input timing remains untraced.

## Conversation database

The bounded recursive decoder validates all 689 groups, 2,267 unique actions, 10,373 unique expressions, and 13,352 unique values, rejecting invalid counts, offsets, kinds, arities, flags, operators, excessive depth, and cycles. Every distinct conversation action ID resolves except `5011`, which is retained and reported as an anomaly in the original data. The adapter maps produced or consumed item selectors `0` through `23` to named campaign inventory. The only other item operand is a producer-less `161` check in Victoria's final shield-offer gate; it is retained as raw original state rather than assigned a speculative identity.

The executable maps those conversation item selectors to physical possession slots through a little-endian 16-bit table at object 2 `+0xA938`: helper `0x2250C` reads the high word of the dword at `+0xA936 + 2 * selector`, then passes that slot to increment helper `0x43100`. `0x2252C` and `0x2254C` use the same lookup for clearing and testing. The 24 supported selector-to-slot values, in selector order, are **Confirmed** from the hashed executable:

| Selectors | Physical possession slots |
| --- | --- |
| 0–7 | 65, 17, 10, 19, 21, 49, 18, 9 |
| 8–15 | 20, 53, 54, 55, 15, 6, 58, 59 |
| 16–23 | 14, 61, 6, 63, 64, 67, 68, 69 |

The possession array uses eight bytes per slot at object 2 `+0xC6C4`: the dword at `+0x04` is the live count. `0x43100` increments it, `0x4310C` tests nonzero, and `0x43124` clears it. Enumerator `0x430B0` bounds the array at `0x230` bytes, or 70 slots. Selector 10 is Dragon Slaying Lance and maps to slot `0x36`; selector 15 is Shield of St. George and maps to `0x3B`; selector 20 is Dragon Slaying Armor and maps to `0x40`. These are exactly the three slots tested by dragon worker `0x1B5DA`–`0x1B614`. Name identities come from the existing executable item-name catalog and decoded conversation rewards; the slot relation comes directly from the table and helper control flow. `OriginalConversationBindings.Items`, `OriginalDragonRunScore`, and `Campaign.BeginDragonBattle` retain the mapping. The anomalous selector 161 is outside this 24-entry mapping and remains raw.

`DynamixConversationDecoder` bounds the node population and every record extent, validates IDs, offsets, markers, ASCII strings, response counts, and graph targets, and exposes typed prompt variants and responses without copying original prose into the repository. `conversation-report.txt` contains only aggregate structural counts.

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

## Open questions

1. Recover the semantic meaning of directory field `0x24` and test whether data extents may alias or overlap.
2. Specify nested chunk headers and the exact compression selector used inside decoded resources.
3. Associate CSF sequences and scene textures with their palettes, confirm `.666` event bindings, and specify the remaining PCC, scene `Viewer`/`Scenario`, non-property-route RAT, and FNT semantics as each decoder is validated.

Presentation binding (2026-09-23): `OriginalUiFontDefinition.MaskPixel` expands each decoded `CONFONT.CSF` mask intensity into premultiplied RGBA, matching MonoGame `SpriteBatch` alpha blending and preventing solid glyph rectangles. Generic replacement-screen text is drawn by `PixelFont` for a legible 5x7 grid on the current layouts. Its host-authored punctuation set covers the date, names, counters, and control hints; typographic quotes and dashes normalize to their grid counterparts, while unsupported printable symbols appear as question marks rather than disappearing. The imported source font is retained and validated for screens whose original typesetting can be mapped. The map notice is drawn only in its right information panel.
