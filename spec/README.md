# Conqueror A.D. 1086 specification

## Scope

This spec describes the original DOS game *Conqueror A.D. 1086*: its executable, its data
files and CD tracks, and the rules the executable applies to them. It covers the build listed
in `builds/`, the GOG English release, which is the only copy studied so far. It covers the
whole game: character creation, the estate, the strategic map, tournaments, jousts, field
battles, first-person castle assaults and melees, dialogue, the dragon quest, the screens, the
resource containers, media and sound formats, configuration and saves.

The spec describes the original game and nothing else. It never names a class, file or setting
from the rebuild in this repository. Values that a designer filled in (names, texts, images,
per-unit statistics) stay in the player's copy of the game; the spec gives their layouts.

## Standard version

This spec follows version 1 of the
[documentation standard](https://dinorefurb.com/documentation-standard/).

## Areas

Areas are added to this list and never removed or renamed. The Covers column says where the
boundary between two areas lies.

| Area | Covers |
|---|---|
| `ASSAULT` | First-person castle assaults and melees: actor records, modes, transitions, acquisition, retainer commands, hits, damage, death. Acquisition that calls the raycaster belongs here. |
| `VIEW` | The first-person raycaster, projection, pointer candidates, and the trigonometry helpers they share. The raycaster's own traversal and projection belong here. |
| `JOUST` | Jousting, including the practice joust and the dragon run. |
| `TOURNEY` | Tournament structure, opponents, wagers and melee selection, apart from the joust itself. |
| `STRATEGY` | The strategic map, movement, calendar, seasons, patrols, temporary forces and strategic encounters. |
| `BATTLE` | Field battles resolved from the strategic map, including the interactive encounter screen. |
| `ESTATE` | Fiefs, buildings, crops, forests, population, economy, loans and staff. |
| `PERSON` | Character creation, attributes, youth dilemmas, promotion, courtship and marriage. |
| `TALK` | Dialogue, conversation trees, rumours and the action-tree interpreter. |
| `DRAGON` | The dragon quest and the dragon battle, apart from the run in `JOUST`. |
| `UI` | Screens, HAT layouts, menus, the village and store screens, and input handling. |
| `RES` | The `.RES`/`.LOW`/`.GOB` containers and their compression. |
| `MEDIA` | Images, palettes, CSF sprites, fonts, Smacker movies. |
| `SOUND` | `.666` sound banks, MIDI music and CD audio. |
| `CONFIG` | `CONQUER.INI` and the setup program's settings. |
| `SAVE` | Saved games and the state they hold. |
| `RNG` | Random number generators and the functions that reduce their draws. |
