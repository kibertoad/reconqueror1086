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
| Campaign briefing | After character selection, a full-screen parchment explains the conquest and tournament/dragon campaign routes before normal estate play. | Decoded `FLUFF.PCX` reproduces the supplied 640×480 screen, including the complete parchment, decoration, and text; normalized screenshot RMSE is 0.07360 despite capture scaling/color variation. `MESSAGE.PCX` is visibly a different blank writing screen. | Sequence, presentation, and resource association Confirmed from controlled observation and direct image comparison. |
| Village exterior | A village is a full-screen location scene. Named destinations are written on the bottom scroll, and Travel is an in-world sign hotspot. The supplied example is Sabine's Keep and includes inn and blacksmith signs around a church. | A normalized pixel comparison over all decoded village images selected `V66_1111.PCX` with RMSE 0.05645, far ahead of the next candidate at 0.16899; direct inspection confirms the scene match. The destination caption is a runtime overlay. | Scene/resource association Confirmed; the wider village-to-location mapping remains Provisional. |
| Village variation | Scott's Keep uses a different street composition while retaining the bottom name scroll, in-world business signs, sword cursor, and Travel sign. | This confirms location-dependent village backgrounds and rules out reusing the Sabine's Keep bitmap globally. Its exact decoded PCX identifier still needs population comparison. | Visual variation Confirmed; resource identity Provisional. |
| Inn interior | Inns are populated full-screen rooms with multiple character hotspots, an exit area, sword cursor, and the bottom hover-label scroll used by other visual locations. | Decoded `INNPEOPL.PCX` reproduces the supplied populated inn exactly. `VINN.HAT` screen 12 names that image and defines ten occupant rectangles plus overlapping bottom regions 10 and 11. A contiguous executable catalog supplies Frederick, Gerard, Barkeep, Otto, Hugh, Gilbert, Nellie, Richard, Ivo, Albert, and Exit in action-region order; each patron has a matching named PCC portrait. `INN.CSF` contains two small face patches, but their timing and placement semantics are not yet established. | Background, patron identities/portraits, ten action regions, Exit, and footer Confirmed; dialogue and CSF animation bindings require tracing. |
| Blacksmith dialogue | Selecting the smith opens a framed portrait/conversation screen: portrait and speaker label at upper left, response at upper right, and player choices across the lower panel. The separate Buy/Sell hotspot opens inventory. | `COMSCRN1.PCX` is the exact empty frame; `BLACKSMI.PCC` is the named portrait candidate. `VSMITH.666` is a two-sample sound bank, not dialogue, so the text/choice source remains unidentified. | Frame and observed navigation split Confirmed; portrait binding Corroborated; dialogue source unknown. |

## Implementation follow-ups

### Youth dilemma

- **Implemented:** render each selected dilemma's CSF with the `MORALITY.PCX` palette into HAT regions 0–2.
- Validate the apparent frame grouping: frames 0–3 for choice I, 4–7 for choice II, and 8–11 for choice III.
- **Implemented:** place and wrap prompt/outcome prose inside the central scroll rather than over the upper stat panel.
- **Implemented, pending visual tuning:** draw current strength, dexterity, piety, stamina, honor, wealth, and age in the original summary positions; intelligence remains an internal scored attribute unless further evidence shows a visible field.
- **Implemented:** bind region 6 and keyboard Enter/Space to Continue. Recover Reroll semantics and region 5 behavior before enabling it.
- Add a 640×480 reference-layout render test using synthetic art and frames; also verify integer-scaled 4:3 output.

### Estate/travel view

- **Implemented:** use the exact `ICONTEMP.PCX` estate shell; startup now requires a verified official-asset extraction instead of offering a no-media presentation.
- Identify the background, terrain tiles, road pieces, crop/forest/building sprites, player shield, sword cursor, and inset England-map source by decoded-image comparison.
- **Implemented:** render the decoded seasonal `ICA.CSF`/`ICS.CSF`/`ICW.CSF` 80x80 tiles with the `ICONTEMP.PCX` palette and switch their typed atlas role by campaign month. Missing atlases or required frames now fail explicitly rather than reverting to constructed terrain. Terrain frame assignments remain provisional.
- **Implemented:** replace the system pointer with the six classified `FFMOUSE.CSF` frames when imported media is present; travel, dialogue, targeting, and held-pointer contexts select the corresponding decoded frames. Exact executable timing and hourglass dispatch remain to be traced.
- **Implemented:** bind Map, Orders, Help, date, speed, wealth, census, location, Home, Village, and inset-map selection through the typed `ICONMAP.HAT` layout.
- Replace the provisional campaign-driven terrain composition with the original tile atlas and exact road/crop/building placement.

### Options/start hub

- **Implemented:** insert the options hub after the title/intro and route New Game to character creation.
- **Implemented:** decode `GAMEOPTS.HAT` into typed actions rather than branching on raw region numbers in screen code.
- **Implemented:** New Game, Load, Save, Resume, Exit, Credits, Movie, and Practice have real state transitions. Credits plays `CREDITZZ.SMK`; Movie replays `TITLE.SMK`; Practice uses its original screen and five regions, plays `JOUSPRAC.SMK`, and returns from isolated practice combat without campaign mutation.
- **Implemented, timing corroborated:** render `OPTION.CSF` frames 0/1 and 2/3 as enabled/disabled released/held states below each setting label and frame 4 as Resume when a campaign is active. Activation occurs only when the pointer is released inside its originally pressed HAT region; exact executable branch confirmation remains outstanding.
- **Partially implemented:** CD music, sound effects, speech state, animation, per-channel volumes, and reduced motion persist through an atomic recoverable settings file; `F11` switches windowed/borderless-fullscreen mode and `F10` selects aspect-fit/integer scaling. A centered 1024×768 virtual canvas preserves layout, aspect ratio, and hotspot alignment, while Pause freezes simulation and Smacker video/audio. MIDI remains explicitly unavailable; subtitles, controller support, and remapping remain follow-ups.
- Preserve the original Exit DOS label while exiting the application safely on modern systems.

### Campaign briefing

- **Implemented:** register screenshot-confirmed `FLUFF.PCX` as the campaign briefing and show it once for newly selected pregenerated characters or after a custom character completes all youth dilemmas.
- **Implemented:** continue on keyboard or click to the estate/travel view; loaded campaigns resume directly and do not replay the one-time briefing.
- **Superseded:** startup now rejects missing official media, so no clean-room briefing substitute is supported.

### Home, farm, village, and blacksmith

- **Implemented:** split the previous overloaded Home handler into a castle office and section-aware Castle, Village, Farm, and Forest management variants; use `TACTICAL.PCX` and the shared `FIEFMGMT.PCX` shell when installed.
- **Implemented:** source each management variant's OK/Cancel, terrain, and visible Full Screen regions from its own HAT descriptor. Management mutations are staged as a bounded session: OK/Enter commits, while Cancel/Escape restores wealth, fief development, army counts, and the associated journal entries.
- **Implemented:** route Village to the screenshot-confirmed populated inn; source ten patron actions, Exit, and the label scroll from `VINN.HAT`; and bind the executable-ordered names and matching PCC portraits. Patron selection enters its confirmed `ALL.CIF`/`ALL.CBF` selector root, opens the original generic conversation frame, renders the node's portrait/speaker/prompt and all declared responses, follows response and timed continuation edges, and executes redirects plus typed character and named-item mutations.
- **Implemented:** render the executable-recovered Castle (17), Village (17), Farm (4), and Forest (7) label catalogs in HAT row geometry; expose supported construction/development rows as transactional mouse targets and allow the 17-entry Village catalog to scroll through its 14 visible rows. Unsupported labels remain display-only.
- **Implemented:** insert the original `FORGESMI.PCX` workshop between Village and the store, with the smith hotspot sourced from `VSMITH.HAT`.
- **Implemented:** the ten Home regions follow SCR-UI-009; `JUMP!!` stays visible but inactive, and the original `F_OVER.PCX` and `WARPLAN.PCX` backgrounds are active for the corresponding destinations.
- **Implemented:** decode and render all 22 `WARPLAN.CSF` controls through the exact `FWARPLAN.HAT` rectangles. The manual-confirmed five-division roster persists named forces, left/right row clicks add/remove 100-serf companies, the 60-company and away-from-home restrictions are enforced, Join/Leave and Field Army state persist, and OK/Cancel commit or restore the complete plan. Spy assignment uses the executable-confirmed 80-shilling cost, one-live-spy limit, and one-report lifetime; it now waits for the first active slot in the persisted five-record enemy-movement roster and reports before that movement advances. The former monthly proxy is removed.
- **Implemented:** keep the smith and Buy/Sell hotspots distinct. The smith opens the conversation frame; the weapon-rack region opens inventory.
- **Implemented:** render `SWDTEMP.PCX` as the store shell; parse `WEAPONS.DAT`; bind each shop definition to its original record; render the selected `SWORDS.CSF` frame, local description, and exact price; and route previous, next, transaction, and exit through typed 640x480 controls.
- Map the village-image suffixes to world locations before activating `V66_1111.PCX` outside its confirmed Sabine's Keep role.
- Locate the actual blacksmith dialogue-node source and bind it to `COMSCRN1.PCX`, `BLACKSMI.PCC`, scroll regions, and choices through the generic dialogue interpreter planned in Phase 1.2. Do not repeat the disproved `VSMITH.666` text hypothesis.
- **Implemented:** render `BUYSELL.CSF` through the store palette using data-driven overlay definitions: hide/show View from the record's movie marker and switch Purchase/Sell from current ownership. Exact mouse-down timing remains to be traced.
- Recover exact path interaction, original lord-record scheduling, enemy detachment/arrival behavior, and captain battle rules for the now-active independently fielded divisions and autonomous enemy movements. Recover behavior/cost semantics for currently display-only management rows and replace the provisional terrain fill with editable tiles. The former `JUMP!!` effect gap is closed: executable callback registration proves the region has no action.

## Acceptance reference

Future render comparisons should target the original logical 640×480 canvas and then verify nearest-neighbor 4:3 scaling. Exact screenshot pixels are not golden-test fixtures because the source captures include emulator scaling and proprietary artwork; tests should use synthetic layouts and independently authored images while local comparison may use the owner's ignored imported assets.
