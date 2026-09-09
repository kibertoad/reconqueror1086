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

The 99 scene containers contain 12,982 headerless `TEX` resources. Their directory names carry the complete rectangular layout as three whitespace-separated fields: `TEXnnn width height`, where `nnn` is a three-digit decimal texture index and width and height are positive decimal integers. For every texture in the hashed release, the decoded payload length is exactly `width * height`, establishing one byte per pixel with no row padding or embedded header. The meaning of each byte and its palette association are not yet confirmed.

Paired scene tiers commonly use different dimensions: `.LOW` entries are generally 64 pixels wide while matching `.RES` entries are generally 128 pixels wide. `DynamixSceneTextureDecoder` validates the three name fields, limits either dimension to 4,096, checks the product without integer overflow, requires exact payload consumption, and returns an owned index buffer. `scene-texture-report.txt` records only counts and distinct dimensions per archive. The naming grammar, dimensions, payload relationship, and complete 12,982-resource population are **Confirmed for the hashed release**; pixel ordering, palette selection, and texture-to-surface mapping remain **Provisional**.

## First-person scene structure

The decoded `MELEE*.RES`, `DEFEND*.RES`, and `BAR*.RES` archives contain four structural resources used by the first-person scenes. `Viewer` is exactly 108 bytes; its first four signed little-endian integers are X, Y, elevation, and heading. X and Y are 8.8 fixed-point map coordinates, and heading spans one unsigned 16-bit turn. `Scenario` is exactly 568 bytes; the little-endian integers at offsets 20, 24, and 28 give texture, block, and sound-effect counts. The remainder is retained as unknown rather than interpreted as stable serialized pointers.

`Map` is exactly 32,768 bytes: 128 by 128 unsigned 16-bit block indices stored in column-major order (`x * 128 + y`). `Blocks` contains exactly the Scenario block count times 96 bytes. Each record ends in `CC CC` and contains a null-terminated printable ASCII label at offset 78 with a 16-byte field. For shape kinds 0, 1, 2, 3, 5, and 6, the four signed little-endian integers at offsets 44, 48, 52, and 56 are bounded texture-table references, using `-1` for no surface. Kind 4 is instead a camera-facing billboard: offset 44 is its primary texture reference, offset 48 is zero across the complete 4,468-record kind-4 population, and offsets 52/56 contain orientation or state values rather than texture references. For example, the `stone wall` record selects texture 12 on all four sides, `TEX012 128 256` visibly decodes as matching masonry under `SKIRMISH.PAL`, and placed knight/champion billboard records select coherent actor images through offset 44. Observed labels identify terrain and interactive roles including ground and floor materials, doors, portcullises, Secret Passage, stairs, Exit, food, treasure/equipment, and knight, footman, bowman, and champion occupants. The bounded decoder verifies all sizes, counts, record sentinels, type-appropriate texture references, map references, viewer coordinates, and heading before exposing a scene.

The full map also contains a disconnected resource-authoring gallery holding representative door and secret-passage state records. In `MELEE0.RES` it occupies the low-coordinate area around X 4–25/Y 2–29, while the Viewer-connected playable component occupies X 80–120/Y 27–60. Runtime conversion therefore floods through passable and interactable cells from Viewer, retains only that connected play area plus one enclosing wall cell, and translates the cropped coordinates back to the source map for texture lookup. This separation is **Confirmed** by connectivity and repeated state-record placement; the original editor/runtime reason for retaining the gallery is **Provisional**.

`Backdrop` is a 24-byte descriptor. All sampled melee and defense scenes declare kind 1, width 1,088, height 200, horizon 199, and mode 3 at signed little-endian offsets 0, 8, 12, 16, and 20; offset 4 is an unstable serialized address and is ignored. Its paired `BackImage` is exactly `width * height` palette indices. The bounded decoder validates kind, dimensions, horizon, and exact image consumption. The two distinct sampled images render as coherent wrapping landscapes with `SKIRMISH.PAL` and are used as the runtime facing panorama. Structure and dimensions are **Confirmed**; palette association and heading-to-panorama offset remain **Corroborated**.

The structure, sizes, column ordering, fixed-point viewer position, named block roles, shape texture slots, and distinct kind-4 primary-image slot are **Confirmed for the hashed release** by cross-checking all 12,784 block records and locating Viewer positions and coherent images in multiple scene variants. `SKIRMISH.PAL` and the contacted cardinal surface slot now drive runtime wall columns, while kind-4 actors use only their primary billboard texture. The palette association and north/east/south/west slot ordering are **Corroborated** by resource naming and coherent decoded images, but still need executable confirmation. The meanings of the remaining block fields, exact heading orientation, animation, collision variants, state-transition records, and both 64-entry families within `Pal0` through `Pal127` remain **Provisional**.

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

That same 2,159-byte payload (SHA-256 prefix `694de4a160413462`) occurs bit-for-bit in 17 screen-specific banks, always at 11,025 Hz: `ICONMAP`, `MONYLNDR`, `FOPTS`, `TOPTS`, `VINN`, `VOPTS`, `VSMITH`, `WAR`, `CGOPTS`, `CHARGEN`, `DKING`, `FIEFMGMT`, `FOVIEW`, `FWARPLAN`, `GAMEOPTS`, `TENTS`, and `UTILITY`. Its reuse and position as the sole `GAMEOPTS.666` sample identify it as a shared interface activation sound with **Corroborated** confidence. Exact event bindings for the other 101 samples remain **Provisional**.

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

The stored `richard.pcc` entry validates as 195×203 pixels; its decoded index SHA-256 is `bee27150d1271ec996cc29e63e196b3fe9ff0504faaec5486f35c685fcbfcd36`. This structure and hash are **Confirmed for the hashed release**. It does not yet prove that all `.PCC` resources are PCX or that compressed `.PCX` entries contain identical payloads after outer decompression.

## Indexed RGB palettes

All five byte-stored `.PAL` entries are exactly 768 bytes: 256 consecutive red, green, and blue byte triples with no header or trailer. Observed channel values span nearly the complete byte range (maximum values 252–255), so they are already 8-bit color components and must not be multiplied from VGA 6-bit values. Exact length, byte interpretation, and component range are **Confirmed for these resources**. Association with particular images or CSF sequences remains **Provisional**.

The importer assigns the `palette` kind only after exact-length validation. `stored-palette-report.txt` records provenance, component range, and a stable SHA-256 without exporting palette bytes.

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

The kind-1 `FFMOUSE.CSF` sequence uses the same container and scanline grammar: six frames, all 20x20. Rendering it with the `ICONTEMP.PCX` palette produces coherent sword, hourglass/wait, travel arrows, speaking mouth, targeting, and pointing-hand cursors in frames 0-5. The dimensions, frame count, and visual identities are **Confirmed**; palette association and the runtime's contextual travel/talk/target/pressed-hand selection are **Corroborated**, while exact executable timing and hourglass dispatch remain **Provisional**.

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

All 1,766 stored chunks decode with this grammar. Across the five resources they contain 119,292 literal segments, 254,433 transparent-skip segments, and 19,573 fill segments. `csf-report.txt` records per-resource counts and stable SHA-256 values over decoded dimensions, indices, and alpha masks, and now applies the same validation to kind-1 CSFs. These command meanings and stored-resource population results are **Confirmed for the hashed release**.

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

`BUYSELL.CSF` is a four-frame indexed overlay sequence associated with the `SWDTEMP.PCX` palette. Frames 0 and 1 are 81x71 versions of the View stone without and with its label; frames 2 and 3 are 100x32 stones labelled Sell and Purchase. Those dimensions, pixels, labels, order, and palette association are **Confirmed** by bounded decoding and palette-assisted rendering. Selecting the View variant from the store record's movie marker and selecting Sell/Purchase from current ownership are **Corroborated** by the table population and visible store behavior; exact input timing remains untraced.

## Conversation database

`ALL.CIF` is an index of 1,311 eight-byte records, each containing a unique non-negative node identifier and a unique 32-bit little-endian byte offset into `ALL.CBF`. Sorting those offsets partitions the 1,514,658-byte body into complete node records; the first begins at offset zero. Every node has a fixed `0x348`-byte binary header followed by zero or more null-terminated printable-ASCII strings. Header byte `0x08` is the prompt-variant count, byte `0x48` is a response count from zero through five, and bytes `0x49`-`0x4B` are the invariant marker `65 3A 5C`. Response target node identifiers begin at `0x4C`; when the response count is zero, the same first dword is the automatic continuation target. Target zero terminates a branch; every nonzero response and continuation target resolves to another CIF node.

For nonempty nodes, the first string is a PCC filename, the second is the displayed speaker, the final N strings are the N response labels, and the intervening strings are the exact number of prompt variants declared at `0x08`. Across the complete owned database, 29 nodes are empty sentinels, the remaining nodes contain 2,062 prompt variants, and 2,696 responses divide into 1,119 terminal and 1,577 linked branches. Sixty-three zero-response records carry continuation fields, of which 55 link onward and 8 terminate. The executable interpreter at `0x17498` selects between response and continuation handling and reads the zero-response target at `0x174A4`/`0x174C9`; `0x176BA`-`0x176E7` selects a prompt variant uniformly through the game's random source when more than one exists. `0x17E74` renders every declared response in one of five rows at `(41,302+n*35,558,35)`, and `0x17E3C`-`0x17E5B` accepts number keys 1-5 within the declared response count. These framing, string-role, count, prompt-selection, continuation, presentation, and link invariants are **Confirmed** for the hashed release. The command arrays applied after a node/response and their campaign-state bindings remain under analysis.

The fixed header also contains 30 signed action-ID slots for each of five response positions beginning at `0x60` with a `0x78`-byte stride, plus 30 node-level slots beginning at `0x2B8`; unused slots are `-1`. The interpreter scans the selected response array at `0x1753B`-`0x17577` and the node array at `0x17507`-`0x17539`, invoking `0x687E4` for every populated identifier. Across the owned database, 423 node-level and 2,318 response-level action references are populated.

`ALL.TMI` is a 5,516-byte index: one leading dword (observed value 100, meaning not yet identified) followed by exactly 689 unique positive `(identifier, ALL.TMB offset)` pairs. Group records in the 427,492-byte `ALL.TMB` begin with an action count followed by that many action offsets. Action records have kinds 1 (`When`, one branch), 2 (`IfElse`, two branches), and 3 (`Evaluate`, no branches), plus an expression offset. Expressions contain N value offsets and N-1 operators. The relocated jump table at source `0x6ABBC` maps codes 1-8 to `!=`, `<=`, `>=`, boolean AND, boolean OR, `>`, `<`, and `==`, respectively. The reachable owned-data graph uses codes 3-8; codes 1 and 2 are supported by the executable but absent. Values are nested expressions, integer literals, or calls to function IDs 3 through 9 with expression arguments and an optional inversion flag.

The bounded recursive decoder validates all 689 groups, 2,267 unique actions, 10,373 unique expressions, and 13,352 unique values, rejecting invalid counts, offsets, kinds, arities, flags, operators, excessive depth, and cycles. Every distinct conversation action ID resolves except `5011`, which is retained and reported as an anomaly in the original data. Structural decoding and function roles are **Confirmed**. The scope-1 adapter maps selectors `0/2/3/5/6/7/8` to wealth/honor/fame/piety/strength/stamina/intelligence, and produced or consumed item selectors `0` through `23` to named campaign inventory. The only other item operand is a producer-less `161` check in Victoria's final shield-offer gate; it is retained as raw original state rather than assigned a speculative identity.

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

`CGOPTS.HAT` maps `CHAR_OPS.PCX` regions 0–2 to Generate New Character, Choose Pre-generated Character, and Choose Character Name, followed by red, green, and blue shield regions 3–5. `PREGEN.HAT` maps `PREGEN.PCX` region identifiers 0–5 to the six profile panels. `FOPTS.HAT` is screen 16's `TACTICAL.PCX` office descriptor; `FCASTLE.HAT` instead declares screen 19 and `FIEFMGMT.PCX`. `FCASTLE`, `FVILLAGE`, `FFARM`, and `FFOREST` reuse that shell with different row counts. In all four, the final region at `(532,0,86,16)` overlays the shell's visible `Full Screen` text; it is a fullscreen control, not a wealth field. The geometry, numeric identifiers, filenames, backgrounds, and this control identity are **Confirmed**; remaining action semantics are **Corroborated** or **Provisional** as recorded per screen.

`FWARPLAN.HAT` declares five 40×38 army buttons in regions 0–4, a 114×38 Field Army control in region 5, a 138×38 Send Out Spy control in region 6, a 198×38 membership control in region 7, three 300×14 unit rows in regions 8–10, OK/Cancel in regions 11–12, and a 100×20 army-name field in region 13. Decoded `WARPLAN.CSF` has 22 frames whose dimensions match those controls exactly: frames 0–14 are five selected/normal/disabled army triplets, frames 15–17 are Join/blank/Leave Selected Army, frames 18–19 are Field Army/blank, and frames 20–21 are two Send Out Spy states. Geometry, dimensions, palette association, visible frame identities, and the name field are **Confirmed**; independent army movement dispatch remains **Provisional**.

## Imported-content manifest

`UserContent/manifest.json` is a version-1 UTF-8 JSON document with the SHA-256 of the source disc image and an asset array. Every asset records a stable source identifier, root-relative generated path, runtime kind, exact byte length, and lowercase SHA-256. Identifiers and paths must be nonempty and case-insensitively unique; paths must be relative and resolve beneath `UserContent`; sizes must be non-negative; both digests must contain exactly 64 hexadecimal characters.

The source-image digest `8a584cc03a0a19f74851d2c1f4653804a58bfbf2c16e4fd34759f7a7f88cf015` identifies the supported GOG English release (**Confirmed** from the owned installation). The importer announces that match before extraction and emits a warning for other hashes while continuing through the same bounded decoders rather than rejecting a potentially compatible legal copy.

`Conqueror.Import --verify` checks those structural invariants and then requires every listed file to exist with the declared size and digest. The install and repair paths run the same verification after writing the manifest. Generated files and the manifest use same-directory temporary files plus atomic replacement; byte-identical destinations are retained rather than rewritten. `--uninstall` prevalidates all manifest paths before removing only those files and the manifest, preserving everything unlisted. These manifest rules describe independently authored importer metadata, not an original-game format.

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
3. Associate CSF sequences and scene textures with their palettes, confirm `.666` event bindings, and specify the remaining PCC, scene `Viewer`/`Scenario`, RAT, and FNT semantics as each decoder is validated.
