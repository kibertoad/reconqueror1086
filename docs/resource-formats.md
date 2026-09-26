# Original resource format specification

This document is the clean-room, byte-level specification for formats observed in the legally owned GOG release identified in [`original-findings.md`](original-findings.md). It contains structural facts and independently derived descriptions only—never extracted dialogue, images, audio, video, or binary payloads.

Confidence terms have the same meaning as the evidence register: **Confirmed** is directly measured across the named sample, **Corroborated** combines local evidence with independent documentation, **Provisional** remains a working hypothesis, and **Disproved** records a rejected interpretation.

## Container reader

`DynamixArchive` reads the container of FMT-RES-001. It rejects a directory before byte 8, a directory extending past end of file, more than one million records, blank names, and any stored extent outside the data area. Allocation uses checked conversions.

`DynamixCompression` decodes kinds 1 and 2 (FMT-RES-003, FMT-RES-004). The kind-1 decoder rejects truncated control words and tokens, history references before the beginning of the block's output, output overflow, block-count disagreement, and any final expanded-size mismatch. The kind-2 decoder requires an end code, rejects truncated input, undefined codes and cyclic/out-of-range dictionary chains, bounds the dictionary at 16,384 entries, enforces the declared expanded length, and applies the common 256 MiB expansion ceiling before allocation. The separate LSB-first LZW decoder is kept for inner chunk streams and is never selected from the outer kind value.

## Raw scene textures

`DynamixSceneTextureDecoder` reads the `TEX` resources of FMT-VIEW-008. It validates the three name fields, limits either dimension to 4,096, checks the product without integer overflow, requires exact payload consumption, and returns an owned index buffer. `scene-texture-report.txt` records only counts and distinct dimensions per archive.

## First-person scene structure

The combat maps hold an unreachable gallery of sample blocks (FND-VIEW-020). Runtime conversion therefore floods through passable and interactable cells from the Viewer, retains only that connected play area plus one enclosing wall cell, and translates the cropped coordinates back to the source map for texture lookup.

The renderer regenerates the active colour-map family with RULE-VIEW-006, selects maps with RULE-VIEW-007, and keeps each encountered texture/map result. Transparency follows the source index before remapping.

Actor sprites use the texture runs of FND-VIEW-021. Runtime enemies keep a cardinal facing, advance the three-pose walk cycle only while moving, and stay visible through the imported death-state completion gate after leaving collision and combat.

The raycaster walks the wrapped 128-cell source map (RULE-VIEW-003), not only the cropped `SiegeLayout`, so the rebuild's acquisition alpha testing must retain decoded sources for all structural face selectors and every sector reachable from kind-4 `Surface0` plus the heading-sector formula, including mirrored sectors. `RaycastTextureReferences` supplies this complete source-only dependency set; `ActivateSiegeVisuals` keeps it separate from the smaller GPU/render set. Omitting the distinction caused practice melee acquisition to request an unloaded but valid texture 11.

The dependency closure is over placed map blocks and their executable-eligible state-target chains, not every definition in the 0x80-block table. Unplaced templates in `MELEE0.RES` reference texture numbers such as 3 that the archive intentionally does not carry; they cannot enter the ray traversal. `SiegeTextureDependencies.AcquisitionTextures` starts from the 128x128 map population and follows a state target only for the behavior/kind combinations accepted by the ray traversal. This fixes the false startup requirement for textures 3/4/6 while retaining every source that a live ray can alpha-test.

The owned-resource census further shows that individual `MELEE*`/`DEFEND*` scene archives are sparse overlays and need not contain all three normalized walk families. `CONQUER/DEFEND2.RES` is the supported high-resolution archive containing the complete actor texture span 64-138. Runtime texture resolution therefore uses that archive as the common combat actor atlas and replaces matching slots with the active scene archive. The normalized slot identities follow RULE-ASSAULT-025; the complete-atlas identity and sparse-overlay relationship are **Corroborated** by the owned archive census. `LoadCombatTextureSources` validates the resulting closure before graphics use. Actor, object, wall, backdrop, and combat-shell procedural fallbacks have been removed: imported siege rendering now has one supported, verified original-art path and reports a precise missing dependency instead of drawing rectangles.

Runtime upload preserves source index zero as transparent and emits premultiplied `(0,0,0,0)` for MonoGame's default alpha blend; CSF alpha planes are premultiplied by the same rule. Retaining palette RGB under alpha zero caused the combat atlas's transparent border to blend as a white rectangle. Actor/object horizontal placement now inverts the confirmed viewer basis directly as `screen = center + lateral / forward`; the earlier conventional 60-degree FOV approximation is **Disproved**. Actor width is one projected 8.8 cell and vertical bounds use block `LowerElevation`/`UpperElevation`, viewer elevation `0x80`, viewport width, and live actor depth, matching RULE-VIEW-003. Render sector selection now uses the same confirmed `actor heading - viewer heading` byte-turn input as acquisition instead of the former actor-to-viewer bearing approximation. Keyboard attack compatibility targets the resulting billboard center; pointer attacks retain the exact centered cursor contact.

## Smacker movie container

`SmackerMovieDecoder` reads the header and tables of FMT-MEDIA-006 and validates the optional ring frame, the frame-size and frame-flag tables, the tree extent, and every aligned frame extent through the exact end of file. It also accepts `SMK4` framing so the playback boundary is explicit.

Frame flags bind optional palette data followed by audio packets for tracks 0 to 6; the remainder is the video packet. Palette packets copy the previous 256-entry RGB table, then apply bounded skip, old-palette copy, and new 6-bit RGB commands; six-bit components expand with `value * 4 + value / 16`. Packed audio uses one LSB-first Huffman tree of 8-bit deltas for the mono profile: each packet declares its decoded byte count, defines a bounded tree of at most 256 leaves and depth 27, seeds an unsigned 8-bit predictor, and reconstructs later samples with byte-wrapping delta addition. The playback adapter converts these samples once to signed PCM16 with the same mapping as `.666` banks.

Video uses four shared LSB-first adaptive Huffman trees for monochrome maps, monochrome colors, full-color pairs, and block types. Each frame resets the three recency slots in each tree, then reconstructs 4x4 blocks as two-color bitmap, full-color, previous-frame skip, or solid fill runs. The stateful decoder writes into the caller's retained index buffer so predicted frames need no full-frame intermediate allocation.

Because every shipped movie is `SMK2` (FND-MEDIA-008), the rebuild decodes them directly; installation-time transcoding is unnecessary. The MonoGame adapter drives the title and credits sequences plus the item previews named by `WEAPONS.DAT`: it keeps one indexed frame buffer, one RGBA upload buffer, and one `Texture2D`, advances frames at the container duration, submits decoded PCM16 packets through `DynamicSoundEffectInstance`, and supports skip/end/return transitions. Its seekable reader keeps only the bounded header/table/tree prefix and one reusable maximum-compressed-frame buffer. Broader scene-event bindings and seeking remain adapter work.

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

`PcxDecoder` reads FMT-MEDIA-003. It requires encoding 1, 8 bits and one plane, decodes `bytesPerLine x height` bytes from byte 128, discards per-row padding beyond the header width, requires the pixel stream to end exactly at the `0x0C` palette marker, and bounds the pixel allocation. It can expand indices to RGBA8 with alpha 255. Extensions are hints only: `.PCC` entries are decoded by header.

## Indexed RGB palettes

`RawIndexedImageDecoder` reads the headerless skirmish screens of FMT-MEDIA-005. It requires caller-supplied dimensions, an exact `width * height` payload, a valid 256-color palette, and bounded pixel counts. `IndexedPaletteDecoder` reads FMT-MEDIA-004; the importer assigns the `palette` kind only after exact-length validation, and `stored-palette-report.txt` records provenance, component range, and a stable XXH3-128 without exporting palette bytes.

## CSF chunk sequences

`CsfSequence` reads FMT-MEDIA-001 and FMT-MEDIA-002. The container parser rejects oversized counts, truncated tables, chunks beyond the resource, and any trailing bytes not consumed by the size table. The frame decoder requires every row to total exactly the width and every chunk to end exactly after its final row; it bounds pixel allocation, rejects unknown operations and truncated commands, and returns separate index and alpha arrays. `csf-report.txt` records per-resource counts and stable XXH3-128 values over decoded dimensions, indices, and alpha masks for stored and kind-1 CSFs.

## Dilemma text resources

`DilemmaTextDecoder` bounds input size, accepts ASCII only, validates identifiers, counts, integer rows, unique choices, and the complete outcome set. `ImportedDialogueRepository` resolves a locally imported definition by stable number and maps its named attributes into the typed core interpreter. The inspector records only structural counts and text lengths in `dilemma-text-report.txt` and compact breakpoint/modifier metadata in `dilemma-rules-report.txt`; original prose remains confined to ignored `UserContent`.

## Weapon-store text resource

`WeaponStoreDecoder` rejects oversized input, non-printable/non-ASCII bytes, bare line endings, incomplete records, excessive record counts or text fields, and negative/non-decimal numeric fields. Runtime equipment definitions store the original record number explicitly; the UI uses the parsed record's frame, price, and locally held description without copying original prose into tracked files or matching on display names. `weapon-store-report.txt` emits only numeric fields, movie presence, and description lengths.

For owner-local catalog reconciliation, the inspector accepts `--weapon-text=0,1,...` (or `all`) and writes the explicitly selected descriptions to the ignored `weapon-store-text.txt`. That report may be inspected locally but must not be committed or redistributed.

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

`ImportedFonts` requires the `CONFONT.CSF` asset of RULE-MEDIA-002 and `OriginalUiFont` rebuilds its alpha masks as tintable textures, keeping the proportional width advance and caller color control while fitting it into the existing host canvas. Its two-thirds host-canvas policy converts a cumulative source advance, never each narrow glyph independently, so rounding cannot distort the relative source spacing.

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

1. Specify nested chunk headers and the exact compression selector used inside decoded resources.
2. Associate CSF sequences and scene textures with their palettes, confirm `.666` event bindings, and specify the remaining scene `Viewer`/`Scenario`, non-property-route RAT, and FNT semantics as each decoder is validated.

Presentation binding (2026-09-23): `OriginalUiFontDefinition.MaskPixel` expands each decoded `CONFONT.CSF` mask intensity into premultiplied RGBA, matching MonoGame `SpriteBatch` alpha blending and preventing solid glyph rectangles. Generic replacement-screen text is drawn by `PixelFont` for a legible 5x7 grid on the current layouts. Its host-authored punctuation set covers the date, names, counters, and control hints; typographic quotes and dashes normalize to their grid counterparts, while unsupported printable symbols appear as question marks rather than disappearing. The imported source font is retained and validated for screens whose original typesetting can be mapped. The map notice is drawn only in its right information panel.
