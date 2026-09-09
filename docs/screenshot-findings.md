# Original-screen observations and follow-ups

This document records presentation facts visible in original-game screenshots supplied by the owner on 2026-09-08. Local copies are preserved under the Git-ignored `analysis/original/screenshots/` directory for later layout and visual comparison. They are behavioral references and must not be committed or redistributed with the project.

Local reference filenames:

- `dilemma-screen.png`
- `estate-travel-screen.png`
- `options-hub-screen.png`
- `village-sabines-keep.png`
- `blacksmith-screen.png`
- `blacksmith-dialogue-screen.png`
- `blacksmith-store-screen.png`
- `home-office-screen.png`
- `farm-management-screen.png`
- `campaign-briefing-screen.png`
- `village-scotts-keep.png`
- `inn-screen.png`

## Observed screens

| Screen | Direct observations | Resource correlation | Confidence |
| --- | --- | --- | --- |
| Youth dilemma | A stone-framed 4:3 screen has the character summary at upper left, Reroll and Continue at upper right, an orange scroll across the center, and three animated choice panels along the bottom labelled I, II, and III. | `CHARGEN.HAT` names `MORALITY.PCX`; its regions 0–2 align with the three bottom panels and region 6 aligns with Continue. Each `D*.CSF` frame is 99×149 versus the 100×150 choice regions. Rendering `D121.CSF` with the `MORALITY.PCX` palette reproduces the screenshot's sepia figures. | Confirmed layout and palette association; four-frame-per-choice grouping is Corroborated pending code-path confirmation. |
| Campaign briefing | After character selection, a full-screen parchment explains the conquest and tournament/dragon campaign routes before normal estate play. | The exact backing resource and transition handler remain to be identified; it is distinct from character options and youth dilemmas. | Sequence and presentation Confirmed from controlled observation; asset assignment Provisional. |
| Estate/travel view | The main area is a tile-based fief landscape, not a full-screen England map. A smaller England map occupies the upper-right panel; Map, Orders, and Help tabs sit below it. The lower-right panel shows location, date, time multiplier, wealth, and census. Home and Village are persistent bottom navigation targets. | `ICONTEMP.PCX` is the exact stone shell. `ICONMAP.HAT` screen 23 defines inset map region 0, information panel 9, main viewport 10, Home/Village/footer 11–13, and Map/Orders/Help 15–17. The original terrain atlas, roads, shield, and cursor still require mapping. | Shell and geometry Confirmed; current procedural terrain is explicitly Provisional. |
| Options/start hub | The interactive hub includes New Game, Load, Save, Practice, Resume, Exit DOS, CD music, MIDI music, sound effects, digitized speech, animation, credits, and movie controls. It follows the introductory title rather than being the title bitmap itself. | `GAMEOPTS.HAT` declares screen 1 with `OPTFIN.PCX`; `OPTION.CSF` contains five settings-widget frames. Exact region-to-action IDs and enabled/disabled states still need executable or controlled-input confirmation. | Direct visual controls Confirmed; resource/action mapping Corroborated. |
| Village exterior | A village is a full-screen location scene. Named destinations are written on the bottom scroll, and Travel is an in-world sign hotspot. The supplied example is Sabine's Keep and includes inn and blacksmith signs around a church. | A normalized pixel comparison over all decoded village images selected `V66_1111.PCX` with RMSE 0.05645, far ahead of the next candidate at 0.16899; direct inspection confirms the scene match. The destination caption is a runtime overlay. | Scene/resource association Confirmed; the wider village-to-location mapping remains Provisional. |
| Village variation | Scott's Keep uses a different street composition while retaining the bottom name scroll, in-world business signs, sword cursor, and Travel sign. | This confirms location-dependent village backgrounds and rules out reusing the Sabine's Keep bitmap globally. Its exact decoded PCX identifier still needs population comparison. | Visual variation Confirmed; resource identity Provisional. |
| Inn interior | Inns are populated full-screen rooms with multiple character hotspots, an exit area, sword cursor, and the bottom hover-label scroll used by other visual locations. | `VINN.HAT` screen 12 names `INNPEOPL.PCX` and defines ten character/object rectangles plus overlapping bottom regions 10 and 11. | Background/layout association Corroborated; individual character/action bindings require tracing. |
| Castle office/Home | Home is a full-screen castle office with desk, map, orders, model keep, and exits embedded in the room. | Decoded `TACTICAL.PCX` matches the supplied screenshot exactly. `FOPTS.HAT` is screen 16, names `TACTICAL.PCX`, and defines ten object/exit regions. Regions 0-5 and 8 align with the overview book, castle model, Farm/Village/Forest ledgers, Orders scroll, and wall map. | Background/descriptor Confirmed; visible-object action bindings Corroborated. |
| Farm management | Farm management has a parchment accounting table on the left, a terrain grid on the right, OK/Cancel in the footer, and a Full Screen target at upper right. It is distinct from Home and travel. | Decoded `FIEFMGMT.PCX` is the exact static shell. HAT screens 19–22 reuse it with different visible row counts and section-specific footer/terrain region identifiers. Contiguous executable strings recover the Castle, Village, Farm, and Forest row-label catalogs. | Background, screen-family geometry, fullscreen target, and label order Confirmed; unsupported row behavior and tile editing remain under investigation. |
| Blacksmith workshop | Entering the blacksmith first shows a full-screen forge with the smith standing behind the anvil and a blank bottom scroll. | Decoded `FORGESMI.PCX` matches the screenshot exactly. `VSMITH.HAT`/screen 13 identifies the same background and region 0 covers the smith. | Background and smith hotspot Confirmed; conversation trigger semantics Corroborated. |
| Blacksmith dialogue | Selecting the smith opens a framed portrait/conversation screen: portrait and speaker label at upper left, response at upper right, and player choices across the lower panel. The separate Buy/Sell hotspot opens inventory. | `COMSCRN1.PCX` is the exact empty frame; `BLACKSMI.PCC` is the named portrait candidate. `VSMITH.666` is a two-sample sound bank, not dialogue, so the text/choice source remains unidentified. | Frame and observed navigation split Confirmed; portrait binding Corroborated; dialogue source unknown. |
| Blacksmith store | The store is a separate stone-and-parchment inventory screen with selected item art, description, wealth/price, previous/next, View, Purchase, and Exit controls. | `SWDTEMP.PCX` is the exact empty shell. `WEAPONS.DAT` is a 40-record table whose explicit image index selects one of 39 `SWORDS.CSF` frames; record 2 plus frame 2 reproduces the screenshot's Battle Sword and 500-shilling price with the `SWDTEMP.PCX` palette. With that palette, `BUYSELL.CSF` frames 0/1 visibly contain the blank/labelled View stone and frames 2/3 contain Sell/Purchase labels. | Shell, palette, tables, item/control frames, labels, Battle Sword binding, and price Confirmed; dynamic overlay selection Corroborated. |

## Implementation follow-ups

### Youth dilemma

- **Implemented:** render each selected dilemma's CSF with the `MORALITY.PCX` palette into HAT regions 0–2.
- Validate the apparent frame grouping: frames 0–3 for choice I, 4–7 for choice II, and 8–11 for choice III.
- **Implemented:** place and wrap prompt/outcome prose inside the central scroll rather than over the upper stat panel.
- **Implemented, pending visual tuning:** draw current strength, dexterity, piety, stamina, honor, wealth, and age in the original summary positions; intelligence remains an internal scored attribute unless further evidence shows a visible field.
- **Implemented:** bind region 6 and keyboard Enter/Space to Continue. Recover Reroll semantics and region 5 behavior before enabling it.
- Add a 640×480 reference-layout render test using synthetic art and frames; also verify integer-scaled 4:3 output.

### Estate/travel view

- **Implemented:** split the strategic-map presentation into the exact `ICONTEMP.PCX` estate shell and a retained no-media map fallback.
- Identify the background, terrain tiles, road pieces, crop/forest/building sprites, player shield, sword cursor, and inset England-map source by decoded-image comparison.
- **Implemented:** render the decoded seasonal `ICA.CSF`/`ICS.CSF`/`ICW.CSF` 80x80 tiles with the `ICONTEMP.PCX` palette and switch their typed atlas role by campaign month. Terrain frame assignments remain provisional.
- **Implemented:** replace the system pointer with confirmed default sword frame 0 from `FFMOUSE.CSF` when imported media is present. Alternate wait/scroll/target/hand dispatch remains to be traced.
- **Implemented:** bind Map, Orders, Help, date, speed, wealth, census, location, Home, Village, and inset-map selection through the typed `ICONMAP.HAT` layout.
- Replace the provisional campaign-driven terrain composition with the original tile atlas and exact road/crop/building placement.

### Options/start hub

- **Implemented:** insert the options hub after the title/intro and route New Game to character creation.
- **Implemented:** decode `GAMEOPTS.HAT` into typed actions rather than branching on raw region numbers in screen code.
- **Partially implemented:** New Game, Load, Save, Resume, and Exit have real state transitions. Practice, Credits, and Movie remain explicit unavailable actions pending their runtime systems.
- **Partially implemented:** render `OPTION.CSF` frames 0 and 2 as the ON/OFF state below each setting label and frame 4 as Resume when a campaign is active. The `OPTFIN.PCX` palette renders these frames coherently and their 23x22/80x40 dimensions match the screenshot/HAT roles. Frames 1 and 3 appear to be alternate pressed-state variants and remain disabled pending input-timing confirmation.
- **Partially implemented:** CD music and animation control active runtime behavior; sound-effects and speech state are represented; MIDI and unsupported presentations are explicitly labelled unavailable. Persisted settings, fullscreen/scaling, subtitles, and reduced motion remain follow-ups.
- Replace Exit DOS wording only in fallback presentation; imported original-media mode may preserve the original label while exiting the application safely.

### Home, farm, village, and blacksmith

- **Implemented:** split the previous overloaded Home handler into a castle office and section-aware Castle, Village, Farm, and Forest management variants; use `TACTICAL.PCX` and the shared `FIEFMGMT.PCX` shell when installed.
- **Implemented:** source each management variant's OK/Cancel, terrain, and visible Full Screen regions from its own HAT descriptor. Management mutations are staged as a bounded session: OK/Enter commits, while Cancel/Escape restores wealth, fief development, army counts, and the associated journal entries.
- **Implemented:** render the executable-recovered Castle (17), Village (17), Farm (4), and Forest (7) label catalogs in HAT row geometry; expose supported construction/development rows as transactional mouse targets and allow the 17-entry Village catalog to scroll through its 14 visible rows. Unsupported labels remain display-only.
- **Implemented:** insert the original `FORGESMI.PCX` workshop between Village and the store, with the smith hotspot sourced from `VSMITH.HAT`.
- **Corrected:** `FOPTS.HAT`, not `FCASTLE.HAT`, supplies the ten `TACTICAL.PCX` Home rectangles. The old generated report omitted resource names, concealing that `FCASTLE.HAT` actually describes screen 19's `FIEFMGMT.PCX` table.
- **Implemented:** Home regions 0-5 and 8 expose Overview, Castle, Farm, Village, Forest, Orders, and Map. The Farm/Village/Forest trio follows the executable's consecutive label block and the left-to-right ledger rectangles. The two exits and small desk object remain disabled until their War Planning/Exit/JUMP dispatch is traced.
- **Implemented:** keep the smith and Buy/Sell hotspots distinct. The smith opens the conversation frame; the weapon-rack region opens inventory.
- **Implemented:** render `SWDTEMP.PCX` as the store shell; parse `WEAPONS.DAT`; bind each shop definition to its original record; render the selected `SWORDS.CSF` frame, local description, and exact price; and route previous, next, transaction, and exit through typed 640x480 controls.
- Map the village-image suffixes to world locations before activating `V66_1111.PCX` outside its confirmed Sabine's Keep role.
- Locate the actual blacksmith dialogue-node source and bind it to `COMSCRN1.PCX`, `BLACKSMI.PCC`, scroll regions, and choices through the generic dialogue interpreter planned in Phase 1.2. Do not repeat the disproved `VSMITH.666` text hypothesis.
- **Implemented:** render `BUYSELL.CSF` through the store palette using data-driven overlay definitions: hide/show View from the record's movie marker and switch Purchase/Sell from current ownership. Exact mouse-down timing remains to be traced.
- Recover the executable dispatch for the unresolved Home exits/small object (`War Planning`, `Exit`, and `JUMP!!`), then recover behavior/cost semantics for currently display-only management rows and replace the provisional terrain fill with editable tiles.

## Acceptance reference

Future render comparisons should target the original logical 640×480 canvas and then verify nearest-neighbor 4:3 scaling. Exact screenshot pixels are not golden-test fixtures because the source captures include emulator scaling and proprietary artwork; tests should use synthetic layouts and independently authored images while local comparison may use the owner's ignored imported assets.
