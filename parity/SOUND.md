# SOUND

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-SOUND-001` | Sound bank, a .666 entry | supported | complete | None | None | implemented | `DynamixSoundBankDecoder` requires the tag the original ignores, rejects rates outside 1,000 to 192,000 Hz and a trailing partial sample, and converts each sample once to signed 16-bit. Every shipped bank passes. |
| `RULE-SOUND-001` | Loading and freeing a sound bank | supported | partial | None | None | supported | The importer decodes every bank at install time and the game keeps the samples it binds for the session; there is no per-screen loading, and `SOUND_EFFECTS` off still loads them. |
| `RULE-SOUND-002` | Playing a sample | supported | partial | None | None | supported | Only the shared click (sample 0 of `gameopts.666`) is bound, played on every click while effects are on. The host mixer has no ten-voice limit, draws nothing from the game's generator, uses the effects volume, and plays the 11,050 Hz samples at their own rate (BUG-SOUND-001 is not reproduced). |
| `RULE-SOUND-003` | Starting the sound systems and playing MIDI music | supported | partial | None | None | supported | The settings file carries the on and off states and applies them at once; there are no drivers or `CONQUER.INI` keys to check. No HMP song is played. |
| `RULE-SOUND-004` | Playing CD music | supported | partial | None | None | supported | The importer converts the five audio tracks to WAV; the game loops track 2 from the end of the opening movie on every screen and pauses it around event movies. Tracks 3 to 6 are not bound to their screens. |
