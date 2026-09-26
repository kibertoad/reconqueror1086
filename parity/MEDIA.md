# MEDIA

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-MEDIA-001` | CSF sprite file | supported | complete | None | None | implemented | `CsfSequence` reads the tag, count and size table and rejects a table past the end or sizes that do not use the file exactly, where the original checks only the tag. |
| `FMT-MEDIA-002` | CSF frame | supported | complete | None | None | implemented | `CsfSequence.DecodeFrame` returns indices and an alpha mask. It rejects an unknown operation and rows that do not add up to the width; the original skips on any operation but 0 and 2 and checks no widths. Every shipped frame passes. |
| `FMT-MEDIA-003` | PCX picture as the game reads it | supported | partial | None | None | supported | `PcxDecoder` also requires encoding 1 and one plane, uses bytes per line and keeps the header width, so an odd-width portrait comes out one column narrower than the original draws it. |
| `FMT-MEDIA-004` | Stored palette, a PAL entry | supported | complete | None | None | implemented | `IndexedPaletteDecoder` requires exactly 768 bytes. |
| `FMT-MEDIA-005` | Cached skirmish screen, a raw picture entry | supported | complete | None | None | implemented | `RawIndexedImageDecoder` with the caller's 320 by 200 and palette. |
| `FMT-MEDIA-006` | Smacker 2 movie header | supported | complete | None | None | implemented | `SmackerMovieDecoder` and `SmackerMovieStream`; `SMK4` is accepted as well though no shipped file uses it. |
| `RULE-MEDIA-001` | Loading a CSF sprite file | supported | partial | None | None | supported | The importer decodes every CSF at install time into `ImportedContent`; there is no disk-file path and no row tables. |
| `RULE-MEDIA-002` | Drawing a sprite frame and a line of text | supported | partial | None | None | supported | `OriginalUiFont` draws `CONFONT.CSF` as masks in the caller colour and advances by frame widths, scaled by two thirds onto the host canvas. Sprites are uploaded as textures with skipped pixels transparent instead of drawn into an indexed buffer. |
| `RULE-MEDIA-003` | Drawing a PCX picture | supported | partial | None | None | supported | Pictures are decoded to textures and drawn with their own palette; odd widths differ as in FMT-MEDIA-003, and there is no shared palette buffer. |
| `RULE-MEDIA-004` | Loading a skirmish screen from SKIRMISH.RES | supported | partial | None | None | supported | The rebuild reads the cached entries and palettes from the imported archive and never builds them from PCX files or writes the archive. |
| `RULE-MEDIA-005` | Playing a Smacker movie | supported | partial | None | None | supported | `SmackerMoviePlayer` plays on the 640 by 480 canvas at the container frame rate with skip and end handling. Repeat counts, `DIG_SPEECH=OFF` and the 320 by 200 full-screen mode are not modelled. |
