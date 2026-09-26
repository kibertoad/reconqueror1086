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

| Artifact | Size | XXH3-128 | Confidence |
| --- | ---: | --- | --- |
| `game.gog` | 667,316,496 bytes | `b915491c5bdce934ca216d2ceebec0ce` | Confirmed |
| Track 1 ISO payload | 244,445 sectors | derived from track 2 index `54:19:20` in `game.ins` | Confirmed |
| `CONQUER.EXE` | 919,107 bytes | `5106f53f8201761cb5112034f6c594d4` | Confirmed |
| `C1086.GOB` | 35,361,714 bytes | `741826412e307510f394d5e774519320` | Confirmed |

These hashes identify the evidence source and are not claims that every retail or localized release is byte-identical.

## Other executable findings

| Finding | Evidence | Confidence |
| --- | --- | --- |
| The configured original supports a `WAR_MODE` setting and the shipped CD configuration uses `640`. | Extracted `CONQUER.INI` in the hashed disc image. | Confirmed for this disc configuration |

## Resource archive findings

| Finding | Evidence | Confidence |
| --- | --- | --- |
| All 26 `.666` resources are rate-tagged unsigned 8-bit mono PCM banks: magic `0x004A5031`, then repeated `UINT32LE` byte length, `UINT32LE` sample rate, and payload. | A bounded decoder consumes all 26 GOB/scene banks exactly; waveform centering distinguishes unsigned from signed PCM. The same 2,159-byte/11,025-Hz sample occurs identically in 17 interface banks. | Framing/rates/PCM Confirmed; shared activation role Corroborated; other event bindings Provisional |
| The ten populated-inn patrons enter through selector roots `1900,1100,3200,3500,1600,1400,3000,1698,3300,3600` in screen order. Their continuations resolve respectively to named Frederick, Gerard, Bartender, Otto, Hugh, Gilbert, Nellie, Richard, Ivo, and Albert portrait nodes. | Metadata-only probe of each root/target pair against decoded `ALL.CIF`/`ALL.CBF`; portraits and speakers match the executable-ordered `VINN.HAT` hotspot catalog. | Confirmed |
| Conversation node `3201` is the blacksmith prompt with `BLACKSMI.PCC`; root `3200` is instead the populated-inn bartender selector whose normal continuation is bartender node `3298`. | Exact decoded node speakers, portraits, prompts, continuations, and the `VINN.HAT` Barkeep association. | Confirmed |
| Generic parish selector root `3100` invokes action group `3102`, whose sole operation redirects to priest node `3101`. That node offers an offering, joust blessing, fief blessing, information, or exit. Selector `3149` instead runs group `3101` and enters Father Hyacinth's dragon-proof-armor chain, whose own text places the armor and Duchess Valletta in Cambridgeshire; Cambridge therefore uses `3149`. Six structurally parallel regional priest roots `5000`-`5500` use `PRIEST2.PCC` through `PRIEST7.PCC`. The executable's object-2 selector pool at `+0xA998` contains those six roots consecutively, followed by known selector roots including `3149` and the ten inn patrons; it establishes that the regional roots are registered conversation selectors, not an ordinal extension of the 18 reimplementation travel locations. The reproducible `Conqueror.Inspect --conversation-selector-pool` report validates this fixed 17-entry prefix without retaining original bytes. The decoded 176 person records expose groups and map coordinates, but no field or executable reference connects any one of the six entries to a regional root. | Complete decoded conversation graph and action groups `3101`/`3102`; the `3149` branch text repeatedly identifies Cambridgeshire; matching repeated prompt/response structures at each regional root; LE data-object census at `+0xA998` through the bounded `--conversation-selector-pool` report; person-table census at `+0xBA50`. | Generic and Cambridge roots Confirmed; regional root registration Confirmed; remaining regional location association incomplete |
| All declared conversation responses are rendered in five possible 558x35 rows beginning at `(41,302)`, with mouse activation and number keys 1-5. | Interpreter paths `0x17E74`-`0x17FBA` and `0x17E3C`-`0x17E5B`. | Confirmed |
| Produced/consumed item selectors `0`-`23` cover the Holy Chalice, tournament gifts, dragon quest objects, the Orchid, armor title, Book of Hours, and Gilbert's note; multiple generic dagger, sword, and hammer selectors intentionally share visible names while retaining distinct counters. A lone `161` possession test has no producer and occurs only in Victoria's final shield-offer gate. | Complete metadata action trace; selected owner-local dialogue correlation; contiguous executable item-name catalog at file offsets `0xD02B4`-`0xD03AF`. | Item IDs `0`-`23` Confirmed; `161` identity remains Provisional and is preserved raw |
| Anna Lisa's response that leads directly to node `24261`, where she discloses the lair in northwestern Wales, invokes action group `2447`; its sole operation increments global variable `2`. Action group `3202` separately tests variable `2 == 1` and redirects, while group `3203` provides the only other writer by assigning it to 1. Tournament selector roots are `2200` Adela, `2900` Jane, `2400` Anna Lisa, `2100` Victoria, `2000` Wendessa, and `2500` Valletta. The lady scripts treat global variable `42` as the current lady colors: 1 Wendessa, 2 Victoria, 3 Anna Lisa, 4 Valletta, or 5 Jane. | Exact conversation targets/text plus complete action-tree writer/reader audit in `conversation-text-report.txt` and `action-group-report.txt`; each root resolves to the matching named portrait. | Lair discovery variable, lady roots and colors selector Confirmed for the hashed release; exact external call sites of groups `3202`/`3203` remain Provisional |
| Courtship prizes are dialogue actions, not an unconditional joust reward. The decoded lady action groups include Jane's item-9/item-10 reward paths, Valletta's item-20 armor path, and other lady-specific item or wealth actions. The selected lady's root evaluates the joust result, the current colors in global `42` and quest state before applying a prize. | Action-tree report groups `2948`/`2954`/`2957`/`2959` (Jane), `2453` (Anna Lisa), `2154`/`2156`/`2158`/`2159` (Victoria), `2051`/`2060`/`2063` (Wendessa), and `2551` (Valletta); selector roots and global slots above. | Dialogue-owned prize mutation Confirmed; exact paths and eligibility predicates are retained in the imported action graph |
| Sir Frederick's repeated raids on the lair while the dragon is away are backstory; no conversation action awards the player lair treasure. | Anna Lisa and Frederick's decoded branches describe only Frederick taking treasure. | Narrative-only absent-dragon raids Corroborated |
| `DROGO.PCC` is his 195x203 portrait. The high-resolution archive places one template-0 player base (block 114, combat row 9) and four hostile `thug` bases: block 97/template 9/row 16, block 101/template 8/row 17, and two block-134/template-3/row-17 placements. The desktop runtime binds this decoded map, actor metadata, collision, backdrop, color maps, texture source, and template stats for that refusal fight; its layout-required campaign API leaves no production one-enemy fallback. | Six contiguous outcome/prompt strings at executable file offsets `0xC9293`-`0xC948C`; strict decoded portrait dimensions and index hash in `stored-image-report.txt`; bounded scene-archive and actor-block census in `scene-res-report.txt` and `scene-block-report.txt`; template table at `0x542F8`; executable control flow `0x2B060 -> 0x1074C`, with object-2 `+0xA4` relocations at `0x10829` and `0x108D2`; runtime binding in `ImportedSiegeLayouts.ForDrogo`, `Campaign.CreateDrogoBattle(SiegeLayout)`, and `ConquerorGame.UpdateDrogoDemand`. | Archive validity and placed actor metadata Confirmed. |
| In placed `MELEE*`/`DEFEND*` map records, behaviour value 83 marks the observed `exit`/`gate` boundaries. | Bounded population report over the owned scene archives, including placement counts, block kinds, behavior masks and names. | Confirmed for the placed population |

### Resource archive coverage

Across the GOB and 99 imported `.RES`/`.LOW` scene archives, the installer extracts every supported byte-stored, kind-1, and kind-2 resource through a centralized codec registry. A disposable end-to-end import of the hashed installation produces 29,226 manifest entries, including the five decoded kind-2 resources. Extensions remain hints rather than proof; known formats are validated before runtime decoding.

### Startup and character-screen findings

| Finding | Evidence | Confidence |
| --- | --- | --- |
| `LOADGAME.PCX` is a distinct five-slot Load Game screen with a Resume control. The executable names `SAVEGAME\\`, `~~1.SAV` through `~~5.SAV`, `CONQ`, and `.SAV`. | Exact decoded resource, local pixel inspection, and printable strings in the hashed executable. | Confirmed five-slot structure and filenames; exact dispatch/timing remains Corroborated |

### Runtime coverage

The runtime additionally activates validated startup and character art, installed HAT geometry, `OPTION.CSF` held/released setting widgets, the five-slot load screen, all six classified `FFMOUSE.CSF` pointers with contextual travel/talk/target/pressed-hand selection, and the `PRACTICE.PCX` menu with its five original regions and joust movie. War launches an isolated tactical practice battle; Melee and Castle Skirmish launch isolated first-person sessions, so their damage, loot, and survivors never mutate a campaign. The estate view uses `ICONTEMP.PCX`, typed `ICONMAP.HAT` panels, and locally imported seasonal `ICA`/`ICS`/`ICW` tiles; exact terrain data, frame assignments, cursor timing, and practice combat parameters remain provisional. Home/farm and blacksmith scenes use their distinct original layers, with the inventory driven by `WEAPONS.DAT`, `SWORDS.CSF`, and `BUYSELL.CSF`. Imported dilemmas use the verified selection and outcome rules while keeping original prose local. A verified official-resource extraction is required at startup; no no-media gameplay path is supported.

“Installed” means the importer safely made owned resources available under ignored local storage. The runtime now uses validated `fftitle.pcx`, `char_ops.pcx`, `pregen.pcx`, `fluff.pcx`, `loadgame.pcx`, `engmap1.pcx`, and `richard.pcc` images for the title, character flow, one-time campaign briefing, five-slot load/resume flow, strategic map, and Richard's tournament-opponent screen. Existing users must rerun `Install Original Resources.bat` once so older manifests gain all decoded entries, including kind 2. Direct SMK title/credits/store-preview playback is active; remaining event bindings, palette associations, and dialogue containers remain incomplete.

## GameFAQs secondary-source consistency audit

The [Conqueror 1086 A.D. FAQ by mikel123456](https://gamefaqs.gamespot.com/pc/574792-conqueror-1086-ad/faqs/66730) is a valuable observation-based starting point, not primary proof. This audit records whether current provisional behavior agrees with it; a match remains Corroborated until the owned executable, resources, manual, or controlled observation independently establishes it.

| Area | FAQ comparison with the current implementation | Status and next corroboration |
| --- | --- | --- |
| Equipment and armor | Prices, 75% resale, armor values, castle-only crossbows, breakable weapons, and Gambeson's extra five armor while another body armor is equipped agree. The FAQ's weapon-length values were observational surrogates and are not treated as damage formulas; executable combat rows supersede them. | Consistent where independently table- or executable-backed. |

## Promotion rule

A provisional value is promoted only when we can cite one of: an unambiguous static table and its code reference, a decoded resource with known semantics, a controlled repeated in-game observation, or agreement between executable behavior and independent documentation. Every promotion should update this file, `docs/fidelity.md`, and the relevant executable specification together.

`FieldBattlePointerControls` supplies mouse selection and visible order buttons to the replacement overhead battle; no original mouse-order equivalence is claimed. Source UI resource decoding confirms `CONFONT.CSF` masks, while the replacement generic labels use `PixelFont` because the imported ornate glyphs are illegible in the current synthetic layout.