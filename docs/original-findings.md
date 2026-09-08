# Findings from the owned original

This register records compact, non-copyrightable facts derived from the user's installed GOG release. It deliberately does not contain extracted binaries, resource dumps, long dialogue, or media.

## Confidence scale

| Grade | Meaning | Can be treated as factual? |
| --- | --- | --- |
| Confirmed | Directly measured in the identified files, or unambiguous executable text/structure | Yes, for the hashed GOG build below |
| Corroborated | Independent documentation agrees with binary/resource evidence, but the exact executing code path has not been recovered | Usually; not necessarily an exact formula or table |
| Provisional | Plausible implementation chosen to make the game playable while exact original behavior remains unknown | No |

Confidence applies narrowly to the stated claim. Finding a name near joust resources, for example, proves that the identifier is present; it does not prove an opponent's odds or wager.

## Provenance

| Artifact | Size | SHA-256 | Confidence |
| --- | ---: | --- | --- |
| `game.gog` | 667,316,496 bytes | `8a584cc03a0a19f74851d2c1f4653804a58bfbf2c16e4fd34759f7a7f88cf015` | Confirmed |
| Track 1 ISO payload | 244,445 sectors | derived from track 2 index `54:19:20` in `game.ins` | Confirmed |
| `CONQUER.EXE` | 919,107 bytes | `5d7231758766204ad061e6b82cf2f0e0cbe28899b35d095f13e4aad75c8b79d6` | Confirmed |
| `C1086.GOB` | 35,361,714 bytes | installed loose resource archive; hash is intentionally left to each local report | Confirmed size for this build |

These hashes identify the evidence source and are not claims that every retail or localized release is byte-identical.

## Tournament findings

| Finding | Evidence | Confidence | Implementation consequence |
| --- | --- | --- | --- |
| Jousts and castle melee both have wager-acceptance flows. | Distinct wager/refusal text adjacent to joust code around `CONQUER.EXE` offsets `0xCF1F7`-`0xCF2CE`, and melee text around `0xD1CF4`-`0xD1DF3`. | Confirmed | Both tournament event interpreters debit a stake and settle it on victory. |
| A participant with no money can be refused. | Separate zero-money refusal messages exist for joust and melee. | Confirmed | An event cannot begin when the selected wager is unaffordable. |
| The original exposes opponent choice and wagers commonly fall between 20 and 80 shillings. | [GameFAQs playthrough documentation](https://gamefaqs.gamespot.com/pc/574792-conqueror-1086-ad/faqs/66730); resource text also contains spelled-out shilling amounts. The full numeric table has not been recovered. | Corroborated | Definitions constrain current stakes to that range. Do not treat the individual stake assignments as original facts. |
| Joust resources identify Simon, Richard, Gerard, and Gilbert. | Printable identifiers `jsimonle.pcc`, `jrichard.pcc`, `jgerard.pcc`, and `jgilbert.pcc` in `CONQUER.EXE`. | Confirmed identifiers only | Those first names are preferable to invented full identities. It remains unproven that these are the complete opponent list. |
| Five selectable melee opponents accompany roughly eight soldiers per side. | [External FAQ description](https://gamefaqs.gamespot.com/pc/574792-conqueror-1086-ad/faqs/66730); executable has a distinct melee selection/acceptance flow. | Corroborated | Five data rows, each totaling eight enemy soldiers. |
| Current stakes `20/35/50/65/80`, hit tolerances, and per-opponent unit mixes reproduce the exact original table. | No direct evidence yet. | Provisional | Centralized in `TournamentOpponentDefinition`; expected to change after disassembly or controlled observation. |

## Other executable findings

| Finding | Evidence | Confidence |
| --- | --- | --- |
| The release has separate first, second, and third joust ordinal text and refuses further jousts when opponents are tired. | Adjacent printable strings in `CONQUER.EXE`; external documentation specifies three jousts. | Corroborated |
| Castle skirmish uses `SKIRMISH.RES`, `SKIRMISH.CSF`, and `SKIRMISH.PCX`. | Literal resource names in `CONQUER.EXE`. | Confirmed |
| The configured original supports a `WAR_MODE` setting and the shipped CD configuration uses `640`. | Extracted `CONQUER.INI` in the hashed disc image. | Confirmed for this disc configuration |

## Resource archive findings

| Finding | Evidence | Confidence |
| --- | --- | --- |
| `C1086.GOB` and the scene `.RES` files use the same indexed Dynamix container family. | Both begin with `.RES`, followed by a little-endian directory offset whose target begins with a plausible entry count. | Confirmed for the hashed build |
| Directory records are 52 bytes: a 32-byte null-padded name followed by two 32-bit fields, stored size, expanded size, and data offset. | All 486 GOB records and representative 222-entry scene archives fit exactly within their files; every tested extent remains before the directory. | Confirmed structure; semantics of the first two fields remain provisional |
| An entry with equal stored and expanded sizes is stored byte-for-byte. | Representative entries have equal lengths and readable resource payloads; synthetic parser tests verify bounded extraction. | Corroborated pending broader format sampling |
| The first field distinguishes stored (`0`), block LZ/RLE (`1`), and adaptive LZW (`2`) entries. | The hashed GOB contains 10 equal-size kind-0 entries, 471 unequal-size kind-1 entries, and five unequal-size kind-2 entries; the two decoder branches were recovered from the executable and validated against their complete populations. | Confirmed for this build |
| Every kind-1 payload is a sequence of `UINT16LE length` plus exactly that many bytes. | Bounded traversal of all 471 kind-1 entries consumes every stored byte exactly, producing 2,963 blocks with no zero or out-of-range lengths. | Confirmed framing for this build |
| Kind-1 blocks expand independently in 16,384-byte slices; marker `0x80` stores the slice verbatim and marker `0x40` selects an MSB-first controlled LZ/RLE token stream. | Every GOB and scene kind-1 entry has `ceil(expanded size / 16384)` blocks. All 20,381 blocks decode to their exact expected slice size; strict follow-on parsing identifies 188 PCX-compatible images. | Confirmed for the hashed release |
| Kind-1 LZ copies encode a 12-bit distance and a 3-18 byte length; distance zero instead encodes a 16-4111 byte repeated-value run. | The owned executable's decoder branches directly on these fields; an independently written bounded decoder accepts the complete GOB/scene kind-1 population. | Confirmed for the hashed release |
| Kind 2 is a continuous MSB-first LZW stream with clear `256`, end `257`, first dictionary code `258`, and adaptive widths from 9 through 14 bits. | Static code at LE virtual addresses `0x45C6E`–`0x45E04` establishes the controls and width ceiling; an independent bounded implementation expands all five entries exactly, and all four PCX results validate strictly as 640x480 images. | Confirmed for the hashed release |
| Kind-1 outer blocks use the same raw LZW stream as documented Dynamix inner chunks. | A bounded raw-LZW probe rejects the real streams at their first codes. | Disproved; do not apply the inner-chunk decoder to outer blocks |
| Kind-1 outer blocks are independently reset 16 KiB classic LH1/LZHUF streams. | A bounded probe under both bit orders produced no valid header among all 187 kind-1 `.PCX`/`.PCC` entries. | Disproved; block count and compression ratio alone do not identify the codec |
| `richard.pcc` contains a standard single-plane 8-bit RLE PCX image with a 256-color trailer palette. | The byte-stored entry passes strict header, scanline, RLE, palette, and exact-consumption validation and decodes to 195×203 pixels with index hash `bee27150…fcd36`. | Confirmed for this resource |
| `engmap1.pcx` is a kind-1 compressed 640x480 PCX and depicts the labeled parchment map of England used by the strategic screen. | Exact decoding, strict PCX validation, and local visual inspection of the owned image. | Confirmed payload and dimensions; runtime role Corroborated |
| `fftitle.pcx` is a kind-1 compressed 640x480 PCX depicting the Conqueror A.D. 1086 title composition. | Exact decoding, strict PCX validation, and local visual inspection of the owned image. | Confirmed payload and dimensions; runtime role Corroborated |
| Byte-stored `.CSF` resources begin with magic `0x4A32`, a 32-bit frame count, and one 32-bit size per frame; concatenated frames consume the remainder exactly. | All five stored CSFs validate: counts are 723, 32, 337, 337, and 337, with each size-table sum exactly matching its remaining bytes. | Confirmed for these resources |
| CSF frames contain width/height followed by per-row literal, transparent-skip, and repeated-color segments. | Every one of 1,766 frames decodes exactly to its declared dimensions and consumes its complete chunk; only operation values 0, 1, and 2 occur. | Confirmed for these resources |
| Stored CSFs contain 119,292 literal, 254,433 transparent-skip, and 19,573 fill segments. | Reproducible `csf-report.txt` totals emitted by the bounded decoder. | Confirmed for the hashed release |
| Five stored scene `.PAL` entries are raw 256-color RGB tables with 8-bit components. | Each is exactly 768 bytes; observed maxima are 252–255 and stable hashes are emitted by `stored-palette-report.txt`. | Confirmed for these resources; screen association remains Provisional |

The hashed `C1086.GOB` contains 486 directory entries, 10 of which have equal stored and expanded sizes. Across the GOB and 50 imported scene archives, the installer extracts every supported byte-stored, kind-1, and kind-2 resource through a centralized codec registry. A disposable end-to-end import of the hashed installation produces 16,097 manifest entries, including the five decoded kind-2 resources. Extensions remain hints rather than proof; known formats are validated before runtime decoding.

### Startup and character-screen findings

| Finding | Evidence | Confidence |
| --- | --- | --- |
| The title composition is the 640x480 `FFTITLE.PCX` named by `TITLE.HAT`. | `TITLE.HAT` directly names the image and declares 640x480; a `CONQUER.EXE` code reference to the HAT's data-object offset calls the common loader. | Confirmed for the hashed release |
| The title art itself contains no New/Load button overlay; input proceeds to a separate character-options screen. | Local pixel inspection of `FFTITLE.PCX` and `CHAR_OPS.PCX`, corroborated by the executable's separate screen-resource table and public gameplay footage. | Corroborated flow; exact input timing remains Provisional |
| `CHAR_OPS.PCX` presents character name, three shield colors, new generation, and pre-generated selection. | Exact decoded 640x480 resource plus `CGOPTS.HAT` region identifiers and coordinates. | Confirmed visible choices and geometry; action semantics Corroborated |
| `PREGEN.PCX` lays out six named profiles in a 3x2 grid matching the six current template names. | Exact decoded resource plus `PREGEN.HAT` region identifiers 0–5 and coordinates. | Confirmed visible names, layout, and geometry; selection semantics Corroborated |
| `LOADGAME.PCX` is a distinct five-slot Load Game screen with a Resume control. The executable names `SAVEGAME\\`, `~~1.SAV` through `~~5.SAV`, `CONQ`, and `.SAV`. | Exact decoded resource, local pixel inspection, and printable strings in the hashed executable. | Confirmed five-slot structure and filenames; exact dispatch/timing remains Corroborated |
| `OPTION.CSF` contains ON/OFF widget pairs (frames 0/1 and 2/3) plus an 80x40 Resume overlay (frame 4), not the primary title menu. The `OPTFIN.PCX` palette renders the widgets coherently; frames 0 and 2 match the owner screenshot's setting states, and frame 4 exactly matches disabled HAT region 11. | Exact CSF decoding, palette-assisted preview, HAT dimensions, and the owner-supplied runtime screenshot. | Palette and static ON/OFF/Resume roles Confirmed; alternate-frame input timing Corroborated |
| `CHARGEN.HAT` identifies `MORALITY.PCX` as screen 3 and defines three enabled 100×150 regions at `(62,310)`, `(268,310)`, and `(478,310)`. The dilemma sequences are 99×149, and rendering them with the morality-screen palette reproduces the screenshot's sepia choice art. | Bounded HAT/PCX/CSF decoding from the hashed archive plus the owner-supplied running-game screenshot. | Confirmed background, regions, dimensions, and palette association; four-frame choice grouping Corroborated |
| Home, farm, and blacksmith use distinct screen layers: `TACTICAL.PCX` is the castle office, `FIEFMGMT.PCX` is the farm-management shell, `FORGESMI.PCX` is the smith workshop, `COMSCRN1.PCX` is the conversation frame, and `SWDTEMP.PCX` is the store shell. | Exact decoded images, HAT background names/regions, and owner-supplied runtime screenshots. | Confirmed background roles; detailed dispatch remains Corroborated |
| `WEAPONS.DAT` contains 40 six-line store records with an optional movie, unknown numeric value, image-frame index, item identifier, price, and description. `SWORDS.CSF` has 39 corresponding frames and uses the `SWDTEMP.PCX` palette; record/frame 2 is the screenshot-confirmed Battle Sword priced at 500 shillings. `BUYSELL.CSF` frames are, in order, blank View, labelled View, Sell, and Purchase overlays using the same palette. | Bounded parsing of the complete resource, CSF/palette rendering, and the owner-supplied store screenshot. | Table structure, indices, prices, palettes, overlay pixels/labels, and Battle Sword binding Confirmed; overlay state selection Corroborated; unknown field and duplicate final record semantics Provisional |
| The supplied Sabine's Keep village exterior corresponds to `V66_1111.PCX`. | Visual inspection plus normalized 640x480 comparison against every decoded `V*_????.PCX` and `TOWN_[YN].PCX`; normalized RMSE 0.05645 versus 0.16899 for the next candidate. | Confirmed scene association; world-location index mapping Provisional |
| `DILEM0.DAT` through `DILEM29.DAT` are structured dilemma definitions with stable identifiers, declared ages, CSF references, three scored choices, win/draw/lose text, and counted attribute modifiers. They form five-definition groups for each age from 12 through 17. | All 30 decoded kind-1 resources pass the same bounded marker/row parser; metadata-only reports record structure and rules without prose. | Confirmed for the hashed release |
| At each age 12–17, the executable chooses one of the five contiguous definitions with `number = (age - 12) * 5 + random(0..4)`. | LE code at VAs `0x12616`–`0x12636` subtracts 12 from age, multiplies by five, calls the inclusive 0–4 range helper at `0x22190`, and adds the result. Equivalent first/later paths call the same helper at `0x11E49` and `0x127EC`. | Confirmed for the hashed release |
| A scored choice resolves to win when its attribute is at least the high breakpoint, draw when it is below high but at least low, and lose below low. | The choice handlers obtain the scoring attribute and call the resolver at VA `0x167DC`; its ordered comparison loop returns the first breakpoint whose value is less than or equal to the score. Resource ordering and handler tables bind indices 0/1/2 to win/draw/lose. | Confirmed for the hashed release |

Owner-supplied runtime screenshots additionally establish the visible dilemma composition, estate/travel panel hierarchy, post-title options hub, village exterior, castle office, farm manager, blacksmith workshop, dialogue, and inventory sequence. Resource correlations, confidence limits, and implementation follow-ups are maintained in [`screenshot-findings.md`](screenshot-findings.md); the images themselves remain untracked.

### Runtime coverage

The runtime additionally activates validated `optfin.pcx`, `char_ops.pcx`, `pregen.pcx`, and `morality.pcx` for the startup/character flow, reads their installed HAT descriptors for exact clickable geometry, renders the `OPTION.CSF` setting/Resume overlays with the options-screen palette, and presents `loadgame.pcx` as a five-slot load screen with bounded artwork-derived rows and Resume control. Home/farm use the distinct `TACTICAL.PCX`/`FIEFMGMT.PCX` layers. The blacksmith flow uses `FORGESMI.PCX`, while its `SWDTEMP.PCX` inventory shell displays `WEAPONS.DAT` descriptions/prices, table-selected `SWORDS.CSF` item art, and definition-selected `BUYSELL.CSF` View/Sell/Purchase controls. Imported mode selects the original age-specific dilemmas, resolves their verified breakpoint bands, applies typed attribute changes, displays local outcome prose, and animates the three choice groups using the morality-screen palette. The selected definition persists in modern saves so reloading cannot reroll it. Built-in summaries remain the no-media fallback. Some region-action and CSF timing semantics remain corroborated rather than executable-confirmed.

“Installed” means the importer safely made owned resources available under ignored local storage. The runtime now uses validated `fftitle.pcx`, `char_ops.pcx`, `pregen.pcx`, `loadgame.pcx`, `engmap1.pcx`, and `richard.pcc` images for the title, character flow, five-slot load/resume flow, strategic map, and Richard's tournament-opponent screen. Existing users must rerun `Install Original Resources.bat` once so older manifests gain all decoded entries, including kind 2. SMK playback, palette association, and remaining dialogue containers remain incomplete.

## Promotion rule

A provisional value is promoted only when we can cite one of: an unambiguous static table and its code reference, a decoded resource with known semantics, a controlled repeated in-game observation, or agreement between executable behavior and independent documentation. Every promotion should update this file, `docs/fidelity.md`, and the relevant executable specification together.
