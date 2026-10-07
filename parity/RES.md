# RES

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-RES-001` | Resource container, a GOB, RES or LOW file | supported | complete | None | None | implemented | `DynamixArchive` reads the header and directory of a whole file held in memory. It also rejects a directory before byte 8 or past the end, more than a million records, blank names and entries outside the data area, where the original checks only the magic and short reads. There is no writer. |
| `FMT-RES-002` | Container directory record | supported | complete | None | None | implemented | `DynamixEntry` keeps the kind as `Flags` and `unk_24` as `Reserved`. |
| `FMT-RES-003` | Kind-1 stream, the stored bytes of a kind-1 entry | supported | complete | None | None | implemented | `DynamixCompression.DecodeKind1` accepts only the markers `0x40` and `0x80`, blocks of 16,384 output bytes and copies inside their block, and rejects anything else; the original accepts any marker but `0x80` as compressed and has none of these limits. Every shipped stream passes. |
| `FMT-RES-004` | Kind-2 stream, the stored bytes of a kind-2 entry | supported | complete | None | None | implemented | `DynamixCompression.DecodeKind2`. |
| `FMT-RES-005` | Raw disc carrier with cue-delimited data and audio spans | supported | missing | None | None | supported | Stored framing only; consumer behavior and audio representation remain open under Q-RES-120 and Q-RES-121. |
| `FMT-RES-006` | Installed disc-image cue sheet | supported | missing | None | None | supported | Observed shipped text syntax only; wrapper parsing remains open under Q-RES-118 and Q-RES-119. |
| `FMT-RES-007` | Counted record containers in the three CD-root .386 files | supported | missing | None | None | supported | Stored framing supported; consumers and payload syntax remain open. |
| `FMT-RES-008` | Empty and ASCII text framing of CD-root batch files | supported | missing | None | None | supported | Stored framing supported; interpreter and runtime use remain open. |
| `FMT-RES-009` | Owned autoplay indexed bitmap layout | supported | missing | None | None | supported | Complete stored layout supported; consumer behavior remains open. |
| `FMT-RES-010` | ASCII key/value syntax in CD-root RESOURCE.CFG | supported | missing | None | None | supported | Stored syntax supported; reader and value meanings remain open. |
| `FMT-RES-011` | ASCII line framing of CD-root INSTALL.DAT | supported | missing | None | None | supported | Stored framing supported; interpreter grammar and runtime behavior remain open. |
| `FMT-RES-013` | Marked ASCII text framing of CD-root INSTALL.HLP | supported | missing | None | None | supported | Stored framing supported; reader semantics and runtime use remain open. |
| `FMT-RES-014` | Owned disc-root indexed icon container layout | supported | missing | None | None | supported | Storage partition recorded; consumer questions Q-RES-164 through Q-RES-167 remain open. |
| `FMT-RES-015` | Unidentified .INF data candidates in CD root | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-016` | Unidentified .SCR data candidates in CD root | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-017` | SETUP.SOL, the first-stage installer's member archive | supported | missing | None | None | supported | Read by the CD's installer only; the rebuild does not install from the disc. Member streams are open under Q-RES-175. |
| `FMT-RES-018` | Unidentified .TXT data candidates in CD root | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-019` | Unidentified .WRI data candidates in CD root | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-020` | Unidentified .AVI data candidates in CD:DEMOS/MOVIES | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-021` | Unidentified .000 data candidates in CD:DEMOS/SHIVERS | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-022` | Unidentified .AUD data candidates in CD:DEMOS/SHIVERS | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-023` | Unidentified .ERR data candidates in CD:DEMOS/SHIVERS | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-024` | Unidentified .ICO data candidates in CD:DEMOS/SHIVERS | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-025` | Unidentified .INF data candidates in CD:DEMOS/SHIVERS | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-026` | Unidentified .SOL data candidates in CD:DEMOS/SHIVERS | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-027` | Unidentified .WAV data candidates in CD:DEMOS/SHIVERS | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-028` | Unidentified .WIN data candidates in CD:DEMOS/SHIVERS | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-029` | Unidentified .000 data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-030` | Unidentified .CFG data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-031` | Unidentified .DRV data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-032` | Unidentified .ERR data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-033` | Unidentified .HLP data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-034` | Unidentified .ICO data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-035` | Unidentified .INF data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-036` | Unidentified .MAP data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-037` | Unidentified .MDT data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-038` | Unidentified .MID data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-039` | Unidentified .SCR data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-040` | Unidentified .SFX data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-041` | Unidentified .SOL data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-042` | Unidentified .TXT data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-043` | Unidentified .WIN data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-044` | Unidentified without a suffix data candidates in CD:DEMOS/SWAT | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-045` | Unidentified .HEP data candidates in CD:DEMOS/SWAT/PATCHES | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-046` | Unidentified .SCR data candidates in CD:DEMOS/SWAT/PATCHES | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-047` | Unidentified .RBT data candidates in CD:DEMOS/SWAT/ROBOTS | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-048` | Unidentified .DIB data candidates in CD:DEMOS/THEXDER | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-049` | Unidentified .HLP data candidates in CD:DEMOS/THEXDER | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-050` | Unidentified .ICO data candidates in CD:DEMOS/THEXDER | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-051` | Unidentified .INF data candidates in CD:DEMOS/THEXDER | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-052` | Unidentified .SOL data candidates in CD:DEMOS/THEXDER | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-053` | Unidentified .THL data candidates in CD:DEMOS/THEXDER | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-054` | Unidentified .INF data candidates in CD:DEMOS/THEXDER/DIRECTX | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-055` | Unidentified .TXT data candidates in CD:DEMOS/THEXDER/DIRECTX | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-056` | Unidentified .INF data candidates in CD:DEMOS/THEXDER/DIRECTX/DRIVERS/AUDIO | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-057` | Unidentified .CNT data candidates in CD:DEMOS/THEXDER/DIRECTX/DRIVERS/AUDIO/BIN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-058` | Unidentified .CSP data candidates in CD:DEMOS/THEXDER/DIRECTX/DRIVERS/AUDIO/BIN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-059` | Unidentified .HLP data candidates in CD:DEMOS/THEXDER/DIRECTX/DRIVERS/AUDIO/BIN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-060` | Unidentified .SBK data candidates in CD:DEMOS/THEXDER/DIRECTX/DRIVERS/AUDIO/BIN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-061` | Unidentified .INF data candidates in CD:DEMOS/THEXDER/DIRECTX/DRIVERS/DISPLAY | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-062` | Unidentified .INI data candidates in CD:DEMOS/THEXDER/DIRECTX/DRIVERS/DISPLAY/BIN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-063` | Unidentified .INF data candidates in CD:DEMOS/THEXDER/ENGLISH | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-064` | Unidentified .TXT data candidates in CD:DEMOS/THEXDER/ENGLISH | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-065` | Unidentified .MDS data candidates in CD:DEMOS/THEXDER/MDS | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-066` | Unidentified .MID data candidates in CD:DEMOS/THEXDER/MDS | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-067` | Unidentified .WAV data candidates in CD:DEMOS/THEXDER/WAVE | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-068` | Unidentified .BAT data candidates in CD:INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-069` | Unidentified .000 data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-070` | Unidentified .001 data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-071` | Unidentified .002 data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-072` | Unidentified .BAT data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-073` | Unidentified .CFG data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-074` | Unidentified .DAT data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-075` | Unidentified .DOC data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-076` | Unidentified .DRV data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-077` | Unidentified .HLP data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-078` | Unidentified .PRG data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-079` | Unidentified .SCR data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-080` | Unidentified .SO data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-081` | Unidentified .TXT data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-082` | Unidentified .V56 data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-083` | Unidentified without a suffix data candidates in CD:INN/FOOTBALL | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-084` | Unidentified .1 data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-085` | Unidentified .BAT data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-086` | Unidentified .CFG data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-087` | Unidentified .DAT data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-088` | Unidentified .DLL data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-089` | Unidentified .DOC data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-090` | Unidentified .DRV data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-091` | Unidentified .HLP data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-092` | Unidentified .ICO data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-093` | Unidentified .ME data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-094` | Unidentified .NL data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-095` | Unidentified .PIF data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-096` | Unidentified .PRG data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-097` | Unidentified .SCR data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-098` | Unidentified .TXT data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-099` | Unidentified without a suffix data candidates in CD:INN/INN | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-100` | Unidentified .TO data candidates in CD:INN/INN/ARPATCH | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-101` | Unidentified .SO data candidates in CD:INN/INN/CCPATCH | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-102` | Unidentified .TO data candidates in CD:INN/INN/CCPATCH | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-103` | Unidentified .TO data candidates in CD:INN/INN/LLPATCH | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-104` | Unidentified .TO data candidates in CD:INN/INN/SLPATCH | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-105` | Unidentified .1 data candidates in CD:INN/TWINION | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-106` | Unidentified .2 data candidates in CD:INN/TWINION | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-107` | Unidentified .BAT data candidates in CD:INN/TWINION | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-108` | Unidentified .CFG data candidates in CD:INN/TWINION | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-109` | Unidentified .HLP data candidates in CD:INN/TWINION | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-110` | Unidentified .SCR data candidates in CD:INN/TWINION | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-111` | Unidentified .TXT data candidates in CD:INN/TWINION | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-112` | Unidentified .DAT data candidates in CD:INN/TWINION/TWPATCH | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-113` | Unidentified .DOC data candidates in CD:INN/TWINION/TWPATCH | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-114` | Unidentified .EXE data candidates in CD:INN/TWINION/TWPATCH | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-115` | Unidentified .DOC data candidates in CD:VESA | unknown | missing | None | None | unknown | Survey listing only; structural layout and runtime use remain unresolved. |
| `FMT-RES-116` | Leading raw-track record in the owned disc carrier | supported | missing | None | None | supported | Owned padding exceptions included; trailer validation and record selection remain open under Q-RES-122 and Q-RES-123. |
| `FMT-RES-117` | AUTORUN.INF, the CD launcher's profile file | supported | missing | None | None | supported | Read by the CD launcher only; the rebuild has no launcher. Open under Q-RES-171 and Q-RES-172. |
| `FMT-RES-118` | SETUP.SOL directory record | supported | missing | None | None | supported | Read by the CD's installer only. Open under Q-RES-176. |
| `RULE-RES-001` | Opening, searching, reading and writing the open archive | supported | partial | None | None | supported | `DynamixArchive.ReadDecoded` decodes kinds 0 to 2 by kind and throws on kind 3. The importer reads every archive at install time, so there is no single open archive, no case-insensitive lookup through a directory, and no writer or encoders. |
| `RULE-RES-002` | Kind-1 decoding | supported | complete | None | None | implemented | `DynamixCompression.DecodeKind1`; see FMT-RES-003 for the extra checks. |
| `RULE-RES-003` | Kind-2 decoding | supported | complete | None | None | implemented | `DynamixCompression.DecodeKind2` rejects a code above the next free code, which the original expands as the next one, and a first code of 256 or above, which the original writes as a byte. No shipped stream has either. |
| `RULE-RES-004` | Archive paths and which archive is open | supported | missing | None | None | supported | The rebuild finds the installed files through its own path resolver and never reads `CONQUER.INI` paths, `C1086ad.GOB` or the `.LOW` scene files at run time. |
| `RULE-RES-005` | Expanding a SETUP.SOL member's stream | unknown | missing | None | None | unknown | Open under Q-RES-175. |
