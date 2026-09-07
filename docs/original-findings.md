# Findings from the owned original

This register records compact, non-copyrightable facts derived from the user's installed GOG release. It deliberately does not contain extracted binaries, resource dumps, long dialogue, or media.

## Confidence scale

| Grade | Meaning | Can be treated as factual? |
| --- | --- | --- |
| Confirmed | Directly measured in the identified files, or unambiguous executable text/structure | Yes, for the hashed GOG build below |
| Corroborated | Independent documentation agrees with binary/resource evidence, but the exact executing code path has not been recovered | Usually; not necessarily an exact formula or table |
| Provisional | Plausible implementation chosen to make the game playable while exact original behavior remains unknown | No |

Confidence applies narrowly to the stated claim. Finding a name near joust resources, for example, proves that the identifier is present; it does not prove an opponent's odds or wager.

## Provenance

| Artifact | Size | SHA-256 | Confidence |
| --- | ---: | --- | --- |
| `game.gog` | 667,316,496 bytes | `8a584cc03a0a19f74851d2c1f4653804a58bfbf2c16e4fd34759f7a7f88cf015` | Confirmed |
| Track 1 ISO payload | 244,445 sectors | derived from track 2 index `54:19:20` in `game.ins` | Confirmed |
| `CONQUER.EXE` | 919,107 bytes | `5d7231758766204ad061e6b82cf2f0e0cbe28899b35d095f13e4aad75c8b79d6` | Confirmed |
| `C1086.GOB` | 35,361,714 bytes | installed loose resource archive; hash is intentionally left to each local report | Confirmed size for this build |

These hashes identify the evidence source and are not claims that every retail or localized release is byte-identical.

## Tournament findings

| Finding | Evidence | Confidence | Implementation consequence |
| --- | --- | --- | --- |
| Jousts and castle melee both have wager-acceptance flows. | Distinct wager/refusal text adjacent to joust code around `CONQUER.EXE` offsets `0xCF1F7`-`0xCF2CE`, and melee text around `0xD1CF4`-`0xD1DF3`. | Confirmed | Both tournament event interpreters debit a stake and settle it on victory. |
| A participant with no money can be refused. | Separate zero-money refusal messages exist for joust and melee. | Confirmed | An event cannot begin when the selected wager is unaffordable. |
| The original exposes opponent choice and wagers commonly fall between 20 and 80 shillings. | [GameFAQs playthrough documentation](https://gamefaqs.gamespot.com/pc/574792-conqueror-1086-ad/faqs/66730); resource text also contains spelled-out shilling amounts. The full numeric table has not been recovered. | Corroborated | Definitions constrain current stakes to that range. Do not treat the individual stake assignments as original facts. |
| Joust resources identify Simon, Richard, Gerard, and Gilbert. | Printable identifiers `jsimonle.pcc`, `jrichard.pcc`, `jgerard.pcc`, and `jgilbert.pcc` in `CONQUER.EXE`. | Confirmed identifiers only | Those first names are preferable to invented full identities. It remains unproven that these are the complete opponent list. |
| Five selectable melee opponents accompany roughly eight soldiers per side. | [External FAQ description](https://gamefaqs.gamespot.com/pc/574792-conqueror-1086-ad/faqs/66730); executable has a distinct melee selection/acceptance flow. | Corroborated | Five data rows, each totaling eight enemy soldiers. |
| Current stakes `20/35/50/65/80`, hit tolerances, and per-opponent unit mixes reproduce the exact original table. | No direct evidence yet. | Provisional | Centralized in `TournamentOpponentDefinition`; expected to change after disassembly or controlled observation. |

## Other executable findings

| Finding | Evidence | Confidence |
| --- | --- | --- |
| The release has separate first, second, and third joust ordinal text and refuses further jousts when opponents are tired. | Adjacent printable strings in `CONQUER.EXE`; external documentation specifies three jousts. | Corroborated |
| Castle skirmish uses `SKIRMISH.RES`, `SKIRMISH.CSF`, and `SKIRMISH.PCX`. | Literal resource names in `CONQUER.EXE`. | Confirmed |
| The configured original supports a `WAR_MODE` setting and the shipped CD configuration uses `640`. | Extracted `CONQUER.INI` in the hashed disc image. | Confirmed for this disc configuration |

## Resource archive findings

| Finding | Evidence | Confidence |
| --- | --- | --- |
| `C1086.GOB` and the scene `.RES` files use the same indexed Dynamix container family. | Both begin with `.RES`, followed by a little-endian directory offset whose target begins with a plausible entry count. | Confirmed for the hashed build |
| Directory records are 52 bytes: a 32-byte null-padded name followed by two 32-bit fields, stored size, expanded size, and data offset. | All 486 GOB records and representative 222-entry scene archives fit exactly within their files; every tested extent remains before the directory. | Confirmed structure; semantics of the first two fields remain provisional |
| An entry with equal stored and expanded sizes is stored byte-for-byte. | Representative entries have equal lengths and readable resource payloads; synthetic parser tests verify bounded extraction. | Corroborated pending broader format sampling |
| A first field value of `1` proves a specific LZW variant. | Size inequality and external Dynamix format research suggest compression, but no decoded-output comparison exists yet. | Provisional; the implementation does not decode or label the algorithm |

The hashed `C1086.GOB` contains 486 directory entries, 10 of which have equal stored and expanded sizes. Across the GOB and 99 imported scene archives, the installer currently extracts 1,785 equal-size entries byte-for-byte. Extensions such as `.CSF` are retained as unknown resources until their semantics are demonstrated; filename extensions alone are not treated as proof that an entry is dialogue, audio, or animation.

## Promotion rule

A provisional value is promoted only when we can cite one of: an unambiguous static table and its code reference, a decoded resource with known semantics, a controlled repeated in-game observation, or agreement between executable behavior and independent documentation. Every promotion should update this file, `docs/fidelity.md`, and the relevant executable specification together.
