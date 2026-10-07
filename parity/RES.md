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
| `FMT-RES-010` | ASCII key/value syntax in CD-root RESOURCE.CFG | supported | missing | None | None | supported | Stored syntax and INST.EXE's line syntax supported; reachability and value meanings remain open. |
| `FMT-RES-011` | ASCII line framing of CD-root INSTALL.DAT | supported | missing | None | None | supported | Stored framing supported; interpreter grammar and runtime behavior remain open. |
| `FMT-RES-013` | INST.EXE text dictionary syntax of CD-root INSTALL.TXT and INSTALL.HLP | supported | missing | None | None | supported | Dictionary syntax and key naming supported; 0x1A handling, tab display and some lookups remain open. |
| `FMT-RES-014` | Owned disc-root indexed icon container layout | supported | missing | None | None | supported | Storage partition recorded; consumer questions Q-RES-164 through Q-RES-167 remain open. |
| `FMT-RES-015` | CONQUER.INF, the CD marker the install script tests for | unknown | missing | None | None | unknown | Only located use is a presence test; contents read by nothing located. |
| `FMT-RES-016` | INST.EXE script syntax of CD-root INSTALL.SCR | supported | missing | None | None | supported | Line syntax, labels, parameters, every command, the program runner and the run's caller supported; parameter values and the installer's checks remain open. |
| `FMT-RES-017` | SETUP.SOL, the first-stage installer's member archive | supported | missing | None | None | supported | Read by the CD's installer only; the rebuild does not install from the disc. Member streams are expanded as RULE-RES-005 gives. |
| `FMT-RES-116` | Leading raw-track record in the owned disc carrier | supported | missing | None | None | supported | Owned padding exceptions included; trailer validation and record selection remain open under Q-RES-122 and Q-RES-123. |
| `FMT-RES-117` | AUTORUN.INF, the CD launcher's profile file | supported | missing | None | None | supported | Read by the CD launcher only; the rebuild has no launcher. Open under Q-RES-171 and Q-RES-172. |
| `FMT-RES-118` | SETUP.SOL directory record | supported | missing | None | None | supported | Read by the CD's installer only. Open under Q-RES-176. |
| `FMT-RES-119` | LANGUAGE.INF, the installer's profile file of titles and strings | supported | missing | None | None | supported | Read by the CD launcher and installer only; the rebuild has neither. Open under Q-RES-178. |
| `FMT-RES-120` | SIERRA.INF, the installer's script file | supported | missing | None | None | supported | Read by the CD's installers only; the rebuild has no installer. Open under Q-RES-180 to Q-RES-182 and Q-RES-185 to Q-RES-191. |
| `FMT-RES-121` | Pointer hot spots, an MVG entry | supported | missing | None | None | supported | Survey listing; open under Q-RES-208. |
| `FMT-RES-122` | Starting home fief, FIEF0.DAT | unknown | missing | None | None | unknown | Survey listing; open under Q-RES-209. |
| `FMT-RES-123` | MIDI song, an HMP entry | supported | missing | None | None | supported | Survey listing; open under Q-RES-210. |
| `FMT-RES-124` | MIDI instrument bank, a BNK entry | supported | missing | None | None | supported | Survey listing; open under Q-RES-211. |
| `FMT-RES-125` | Point-of-view settings, the POV entry of a scene file | unknown | missing | None | None | unknown | Survey listing; open under Q-RES-212. |
| `FMT-RES-126` | SVG entry of SKIRMISH.RES | unknown | missing | None | None | unknown | Survey listing; open under Q-RES-213. |
| `FMT-RES-127` | SFG entry of SKIRMISH.RES | unknown | missing | None | None | unknown | Survey listing; open under Q-RES-214. |
| `RULE-RES-001` | Opening, searching, reading and writing the open archive | supported | partial | None | None | supported | `DynamixArchive.ReadDecoded` decodes kinds 0 to 2 by kind and throws on kind 3. The importer reads every archive at install time, so there is no single open archive, no case-insensitive lookup through a directory, and no writer or encoders. |
| `RULE-RES-002` | Kind-1 decoding | supported | complete | None | None | implemented | `DynamixCompression.DecodeKind1`; see FMT-RES-003 for the extra checks. |
| `RULE-RES-003` | Kind-2 decoding | supported | complete | None | None | implemented | `DynamixCompression.DecodeKind2` rejects a code above the next free code, which the original expands as the next one, and a first code of 256 or above, which the original writes as a byte. No shipped stream has either. |
| `RULE-RES-004` | Archive paths and which archive is open | supported | missing | None | None | supported | The rebuild finds the installed files through its own path resolver and never reads `CONQUER.INI` paths, `C1086ad.GOB` or the `.LOW` scene files at run time. |
| `RULE-RES-005` | Expanding a SETUP.SOL member's stream | supported | missing | None | None | supported | Used by the CD's installer only; the rebuild has no installer. Open under Q-RES-177. |
