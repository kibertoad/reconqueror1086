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

## Observed screens

| Screen | Direct observations | Resource correlation | Confidence |
| --- | --- | --- | --- |
| Youth dilemma | A stone-framed 4:3 screen has the character summary at upper left, Reroll and Continue at upper right, an orange scroll across the center, and three animated choice panels along the bottom labelled I, II, and III. | `CHARGEN.HAT` names `MORALITY.PCX`; its regions 0–2 align with the three bottom panels and region 6 aligns with Continue. Each `D*.CSF` frame is 99×149 versus the 100×150 choice regions. Rendering `D121.CSF` with the `MORALITY.PCX` palette reproduces the screenshot's sepia figures. | Confirmed layout and palette association; four-frame-per-choice grouping is Corroborated pending code-path confirmation. |
| Estate/travel view | The main area is a tile-based fief landscape, not a full-screen England map. A smaller England map occupies the upper-right panel; Map, Orders, and Help tabs sit below it. The lower-right panel shows location, date, time multiplier, wealth, and census. Home and Village are persistent bottom navigation targets. | The framing resembles the `FIEFMGMT.PCX` screen family and its HAT layouts, but the terrain, marker, inset-map role, and exact tab mapping require resource-to-screenshot comparison. The current runtime's full-screen `engmap1.pcx` assumption must not be treated as final. | Direct visual layout Confirmed; individual asset assignments Provisional. |
| Options/start hub | The interactive hub includes New Game, Load, Save, Practice, Resume, Exit DOS, CD music, MIDI music, sound effects, digitized speech, animation, credits, and movie controls. It follows the introductory title rather than being the title bitmap itself. | `GAMEOPTS.HAT` declares screen 1 with `OPTFIN.PCX`; `OPTION.CSF` contains five settings-widget frames. Exact region-to-action IDs and enabled/disabled states still need executable or controlled-input confirmation. | Direct visual controls Confirmed; resource/action mapping Corroborated. |
| Village exterior | A village is a full-screen location scene. Named destinations are written on the bottom scroll, and Travel is an in-world sign hotspot. The supplied example is Sabine's Keep and includes inn and blacksmith signs around a church. | A normalized pixel comparison over all decoded village images selected `V66_1111.PCX` with RMSE 0.05645, far ahead of the next candidate at 0.16899; direct inspection confirms the scene match. The destination caption is a runtime overlay. | Scene/resource association Confirmed; the wider village-to-location mapping remains Provisional. |
| Castle office/Home | Home is a full-screen castle office with desk, map, orders, model keep, and exits embedded in the room. | Decoded `TACTICAL.PCX` matches the supplied screenshot exactly. HAT screen 16 uses `TACTICAL.PCX` and defines ten object/exit regions. | Background Confirmed; object action dispatch Corroborated. |
| Farm management | Farm management has a parchment accounting table on the left, a terrain grid on the right, and OK/Cancel plus wealth, productivity, and date in the footer. It is distinct from Home and travel. | Decoded `FIEFMGMT.PCX` is the exact static shell. HAT screens 19–22 reuse it with different row counts and right-panel/footer regions. | Background and screen-family geometry Confirmed; row semantics and tile editing remain under investigation. |
| Blacksmith workshop | Entering the blacksmith first shows a full-screen forge with the smith standing behind the anvil and a blank bottom scroll. | Decoded `FORGESMI.PCX` matches the screenshot exactly. `VSMITH.HAT`/screen 13 identifies the same background and region 0 covers the smith. | Background and smith hotspot Confirmed; conversation trigger semantics Corroborated. |
| Blacksmith dialogue | Selecting the smith opens a framed portrait/conversation screen: portrait and speaker label at upper left, response at upper right, and player choices across the lower panel. | `COMSCRN1.PCX` is the exact empty frame; `BLACKSMI.PCC` is the named portrait candidate. The complete text/choice source still needs decoding from `VSMITH.666`. | Frame association Confirmed; portrait and dialogue-data bindings Corroborated. |
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

- Split the current strategic-map presentation into an estate/fief view and the appropriate England-map navigation view instead of stretching one asset into both roles.
- Identify the background, terrain tiles, road pieces, crop/forest/building sprites, player shield, sword cursor, and inset England-map source by decoded-image comparison.
- Recover the Map, Orders, and Help tab region IDs and their screen transitions from HAT data and executable handlers.
- Move date, speed, wealth, census, location, Home, and Village presentation into a typed layout definition populated from original coordinates.
- Preserve modern keyboard navigation while adding the original mouse targets.

### Options/start hub

- **Implemented:** insert the options hub after the title/intro and route New Game to character creation.
- **Implemented:** decode `GAMEOPTS.HAT` into typed actions rather than branching on raw region numbers in screen code.
- **Partially implemented:** New Game, Load, Save, Resume, and Exit have real state transitions. Practice, Credits, and Movie remain explicit unavailable actions pending their runtime systems.
- **Partially implemented:** render `OPTION.CSF` frames 0 and 2 as the ON/OFF state below each setting label and frame 4 as Resume when a campaign is active. The `OPTFIN.PCX` palette renders these frames coherently and their 23x22/80x40 dimensions match the screenshot/HAT roles. Frames 1 and 3 appear to be alternate pressed-state variants and remain disabled pending input-timing confirmation.
- **Partially implemented:** CD music and animation control active runtime behavior; sound-effects and speech state are represented; MIDI and unsupported presentations are explicitly labelled unavailable. Persisted settings, fullscreen/scaling, subtitles, and reduced motion remain follow-ups.
- Replace Exit DOS wording only in fallback presentation; imported original-media mode may preserve the original label while exiting the application safely.

### Home, farm, village, and blacksmith

- **Implemented:** split the previous overloaded Home handler into a castle office and separate farm-management screen; use `TACTICAL.PCX` and `FIEFMGMT.PCX` when installed.
- **Implemented:** insert the original `FORGESMI.PCX` workshop between Village and the store, with the smith hotspot sourced from `VSMITH.HAT`.
- **Implemented:** render `SWDTEMP.PCX` as the store shell; parse `WEAPONS.DAT`; bind each shop definition to its original record; render the selected `SWORDS.CSF` frame, local description, and exact price; and route previous, next, transaction, and exit through typed 640x480 controls.
- Map the village-image suffixes to world locations before activating `V66_1111.PCX` outside its confirmed Sabine's Keep role.
- Decode `VSMITH.666` dialogue nodes and bind `COMSCRN1.PCX`, `BLACKSMI.PCC`, scroll regions, and choices through the generic dialogue interpreter planned in Phase 1.2.
- **Implemented:** render `BUYSELL.CSF` through the store palette using data-driven overlay definitions: hide/show View from the record's movie marker and switch Purchase/Sell from current ownership. Exact mouse-down timing remains to be traced.
- Recover `TACTICAL.PCX` object actions and the four `FIEFMGMT.PCX` HAT variants; then replace the provisional farm table/terrain fill with exact definitions and editable tiles.

## Acceptance reference

Future render comparisons should target the original logical 640×480 canvas and then verify nearest-neighbor 4:3 scaling. Exact screenshot pixels are not golden-test fixtures because the source captures include emulator scaling and proprietary artwork; tests should use synthetic layouts and independently authored images while local comparison may use the owner's ignored imported assets.
