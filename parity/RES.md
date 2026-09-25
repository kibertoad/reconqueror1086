# RES

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-RES-001` | Resource container, a GOB, RES or LOW file | supported | complete | None | None | implemented | `DynamixArchive` reads the header and directory of a whole file held in memory. It also rejects a directory before byte 8 or past the end, more than a million records, blank names and entries outside the data area, where the original checks only the magic and short reads. There is no writer. |
| `FMT-RES-002` | Container directory record | supported | complete | None | None | implemented | `DynamixEntry` keeps the kind as `Flags` and `unk_24` as `Reserved`. |
| `FMT-RES-003` | Kind-1 stream, the stored bytes of a kind-1 entry | supported | complete | None | None | implemented | `DynamixCompression.DecodeKind1` accepts only the markers `0x40` and `0x80`, blocks of 16,384 output bytes and copies inside their block, and rejects anything else; the original accepts any marker but `0x80` as compressed and has none of these limits. Every shipped stream passes. |
| `FMT-RES-004` | Kind-2 stream, the stored bytes of a kind-2 entry | supported | complete | None | None | implemented | `DynamixCompression.DecodeKind2`. |
| `RULE-RES-001` | Opening, searching, reading and writing the open archive | supported | partial | None | None | supported | `DynamixArchive.ReadDecoded` decodes kinds 0 to 2 by kind and throws on kind 3. The importer reads every archive at install time, so there is no single open archive, no case-insensitive lookup through a directory, and no writer or encoders. |
| `RULE-RES-002` | Kind-1 decoding | supported | complete | None | None | implemented | `DynamixCompression.DecodeKind1`; see FMT-RES-003 for the extra checks. |
| `RULE-RES-003` | Kind-2 decoding | supported | complete | None | None | implemented | `DynamixCompression.DecodeKind2` rejects a code above the next free code, which the original expands as the next one, and a first code of 256 or above, which the original writes as a byte. No shipped stream has either. |
| `RULE-RES-004` | Archive paths and which archive is open | supported | missing | None | None | supported | The rebuild finds the installed files through its own path resolver and never reads `CONQUER.INI` paths, `C1086ad.GOB` or the `.LOW` scene files at run time. |
