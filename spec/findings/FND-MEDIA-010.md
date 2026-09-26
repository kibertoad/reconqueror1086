---
id: FND-MEDIA-010
title: Owner screenshots match FLUFF.PCX, V66_1111.PCX, INNPEOPL.PCX and COMSCRN1.PCX, and the executable names the inn patrons
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x21805C1..0x21A0F57
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1D560F8..0x1D766DA
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x8D793E..0x901BBB
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x15CBB1A..0x15F90E1
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x141D04C..0x143C3DB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00098D54..0x00098DB7
tool: Screenshot comparison written for this project
environment: null
---

## Observation

Twelve screenshots of the running original, taken by the owner on 2026-09-08 and kept outside the
repository, were compared with the decoded pictures of `C1086.GOB`:

- The campaign briefing shown after character selection and before estate play is `FLUFF.PCX`,
  parchment, decoration and text included: normalized RMSE 0.07360 against the 640x480 capture.
  `MESSAGE.PCX` is a different, blank writing screen.
- The Sabine's Keep village is `V66_1111.PCX`: over every `V*_????.PCX` and `TOWN_[YN].PCX`, it
  scored RMSE 0.05645 and the next best 0.16899. The place name on the bottom scroll is drawn on
  top at run time.
- Scott's Keep shows a different street with the same bottom scroll, signs, sword pointer and
  Travel sign; which picture it is was not found.
- The inn is `INNPEOPL.PCX`, with no difference.
- The blacksmith's conversation frame is `COMSCRN1.PCX` with the portrait `BLACKSMI.PCC` at upper
  left.

The executable holds the inn labels `Frederick`, `Gerard`, `Barkeep`, `Otto`, `Hugh`, `Gilbert`,
`Nellie`, `Richard`, `Ivo`, `Albert` and `Exit` from `0x00098D54`, followed by `INN.CSF`. The
portraits of those names in `C1086.GOB` are 195x203 except `nellie.pcc` at 194x202. `INN.CSF` has
two frames, 32x38 and 54x42.

## Interpretation

The briefing, the Sabine's Keep village, the inn and the conversation frame are drawn from these
pictures. The inn labels are in the order of the regions of SCR-UI-017.

## Alternatives

The captures were scaled by an emulator, so pixel-exact comparison was not possible; the RMSE
values are from normalized images.

## How to reproduce

Decode the named pictures (RULE-RES-001) and compare them with captures of the same screens of the
running game; read the strings from `0x00098D54` in `CD:CONQUER.EXE`.
