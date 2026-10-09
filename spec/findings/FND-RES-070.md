---
id: FND-RES-070
title: Stored archive extents resolve resource-location last-byte bounds
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00000000..0x021B93B2
  - build: BLD-GOG-EN
    file: CD:CONQUER/SKIRMISH.RES
    offset: 0x00000000..0x000AFBF0
tool: Bounded archive-directory traversal and stored-extent comparison
environment: null
---

## Observation

The owned C1086.GOB matches the size and XXH3-128 in BLD-GOG-EN. Reading its
directory under FND-RES-001 gives the following stored extents. Each end is the
data offset plus the stored size, not the expanded size. Every listed extent
lies wholly before the directory and inside the owned file. These are framing
facts; neither decompression nor executable behavior is inferred from them.

| Resource | Stored start | Stored size | Exclusive end |
|---|---|---:|---|
| `v66_1111.pcx` | `0x008D793E` | 172670 | `0x00901BBC` |
| `men8.CSF` | `0x00A850D3` | 1250303 | `0x00BB64D2` |
| `fief0.dat` | `0x00C29B3F` | 777 | `0x00C29E48` |
| `credit.CSF` | `0x00C743E5` | 1648136 | `0x00E069ED` |
| `ica.CSF` | `0x0111B818` | 829340 | `0x011E5FB4` |
| `ics.CSF` | `0x011E5FB4` | 828627 | `0x012B0487` |
| `icw.CSF` | `0x012B0487` | 826237 | `0x0137A004` |
| `comscrn1.pcx` | `0x0141D04C` | 127888 | `0x0143C3DC` |
| `innpeopl.pcx` | `0x015CBB1A` | 185800 | `0x015F90E2` |
| `icontemp.pcx` | `0x016B7915` | 44032 | `0x016C2515` |
| `iconmap.666` | `0x01A457CB` | 24274 | `0x01A4B69D` |
| `monylndr.666` | `0x01A4B69D` | 102310 | `0x01A64643` |
| `fopts.666` | `0x01A64643` | 1321 | `0x01A64B6C` |
| `topts.666` | `0x01A64B6C` | 1321 | `0x01A65095` |
| `vinn.666` | `0x01A65095` | 1321 | `0x01A655BE` |
| `vopts.666` | `0x01A655BE` | 1321 | `0x01A65AE7` |
| `vsmith.666` | `0x01A65AE7` | 20164 | `0x01A6A9AB` |
| `war.666` | `0x01A6A9AB` | 53834 | `0x01A77BF5` |
| `cgopts.666` | `0x01A77BF5` | 1321 | `0x01A7811E` |
| `chargen.666` | `0x01A7811E` | 1321 | `0x01A78647` |
| `dking.666` | `0x01A78647` | 1321 | `0x01A78B70` |
| `fiefmgmt.666` | `0x01A78B70` | 54893 | `0x01A861DD` |
| `foview.666` | `0x01A861DD` | 1321 | `0x01A86706` |
| `fwarplan.666` | `0x01A86706` | 1321 | `0x01A86C2F` |
| `gameopts.666` | `0x01A86C2F` | 1321 | `0x01A87158` |
| `tents.666` | `0x01A87158` | 1321 | `0x01A87681` |
| `skirmsnd.666` | `0x01A87681` | 150855 | `0x01AAC3C8` |
| `utility.666` | `0x01AAC3C8` | 89242 | `0x01AC2062` |
| `configit.666` | `0x01AC2062` | 40475 | `0x01ACBE7D` |
| `castle2.hmp` | `0x01AFDDF7` | 7828 | `0x01AFFC8B` |
| `castle3.hmp` | `0x01AFFC8B` | 15750 | `0x01B03A11` |
| `church.hmp` | `0x01B03A11` | 970 | `0x01B03DDB` |
| `morals.hmp` | `0x01B03DDB` | 954 | `0x01B04195` |
| `fiefmgmt.hmp` | `0x01B04195` | 954 | `0x01B0454F` |
| `icast.hmp` | `0x01B0454F` | 2579 | `0x01B04F62` |
| `iconwrld.hmp` | `0x01B04F62` | 15492 | `0x01B08BE6` |
| `ocast.hmp` | `0x01B08BE6` | 5896 | `0x01B0A2EE` |
| `party.hmp` | `0x01B0A2EE` | 3116 | `0x01B0AF1A` |
| `melodic.bnk` | `0x01B0AF1A` | 3255 | `0x01B0BBD1` |
| `drum.bnk` | `0x01B0BBD1` | 1866 | `0x01B0C31B` |
| `font.CSF` | `0x01B0CFA6` | 8194 | `0x01B0EFA8` |
| `ffonta2.fnt` | `0x01B105B4` | 49 | `0x01B105E5` |
| `lady.hmp` | `0x01BCB224` | 1936 | `0x01BCB9B4` |
| `tavern.hmp` | `0x01BCB9B4` | 2423 | `0x01BCC32B` |
| `village.hmp` | `0x01BCC32B` | 2270 | `0x01BCCC09` |
| `nellie.pcc` | `0x01C42856` | 26203 | `0x01C48EB1` |
| `richard.pcc` | `0x01C7C310` | 25924 | `0x01C82854` |
| `engmap1.pcx` | `0x01CA9D9D` | 129522 | `0x01CC978F` |
| `message.pcx` | `0x01D560F8` | 132579 | `0x01D766DB` |
| `joust.666` | `0x02076CB4` | 202056 | `0x020A81FC` |
| `avg_end.666` | `0x020A81FC` | 89100 | `0x020BDE08` |
| `champl30.666` | `0x020BDE08` | 173795 | `0x020E84EB` |
| `crownl30.666` | `0x020E84EB` | 312494 | `0x02134999` |
| `dubb3.666` | `0x02134999` | 182895 | `0x02161408` |
| `FLUFF.PCX` | `0x021805C1` | 133527 | `0x021A0F58` |
| `CONFONT.CSF` | `0x021A0F58` | 7950 | `0x021A2E66` |
| `intro.666` | `0x021A4357` | 60831 | `0x021B30F6` |

The same directory traversal of CD:CONQUER/SKIRMISH.RES, verified against its
manifest size and hash, gives these additional stored extents:

| Resource | Stored start | Stored size | Exclusive end |
|---|---|---:|---|
| `SKirmsnd.666` | `0x00000008` | 278584 | `0x00044040` |
| `skirmish.csf` | `0x00044040` | 86521 | `0x00059239` |

## Interpretation

These extents identify the complete stored resources used by the resource-range
notation audit. A decoded resource can have a different length; its coordinates
must not be substituted for offsets in the shipped archive. Complete-file
sizes and identities are checked separately by FND-RES-069 and the build manifest.
The bounds do not establish a complete reading of any routine or resource.

## Alternatives

An end one byte below the exclusive end names the last stored byte. Where an
older entry describes the complete file or resource, changing its notation
preserves that scope. Where its own text does not settle the intended extent,
a replacement explicitly selects the complete stored resource; metadata alone
does not authorize editing that ambiguous observation in place.

## How to reproduce

Verify C1086.GOB and CD:CONQUER/SKIRMISH.RES against BLD-GOG-EN. Read each header's directory offset and count,
bound every 52-byte record, and read its data offset and stored size as
FND-RES-001 specifies. Check addition and file bounds before forming an end.
Compare the named resources with the table. Keep directory exports locally;
do not retain stored payloads in Git.
