---
id: FND-RES-062
title: Verified analyzer body extents and a named return boundary used to convert location bounds
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1066:0008..1067:0007
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1183:0009..1186:0005
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 152C:0004..1543:0003
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1543:0003..1548:0002
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 189A:0007..189F:0002
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B20:0000..1B7C:0003
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B94:0007..1B98:0002
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1EED:000A..1EF9:0003
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 20B2:000B..20F3:000E
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 213C:0007..2141:0009
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2146:0001..2152:0004
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 229D:0002..22A4:000F
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2DA2:000D..2DBC:0003
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2DE5:000D..2DEA:000B
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2EC7:0008..2ED3:000E
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2F0C:000C..2F2C:0000
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 3039:000D..3044:0000
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001074C..0x000109AB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000110E8..0x00011280
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00012A4C..0x00012AEA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000130E4..0x00013166
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015920..0x00015AA0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015EAC..0x00015EBB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015EF0..0x00015F0A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015F0C..0x00015F87
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00018334..0x00018392
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00018430..0x0001869C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001869C..0x0001884B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00018850..0x00018B0D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001BB5C..0x0001BC0D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00021E88..0x00021EEB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00022564..0x0002258E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00024C20..0x00024C36
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00024C38..0x00024C4C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00024C4C..0x00024C9D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00024D14..0x00024D54
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00028408..0x000289CA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000296B4..0x000297A0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002B060..0x0002B1EC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002DC08..0x0002DD3C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002FC70..0x0002FCAC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002FCB0..0x0002FCED
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00035924..0x00035C5A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00038678..0x00038681
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00038A8C..0x00038B3D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00039428..0x000398FD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003A6A0..0x0003A737
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003C6E0..0x0003C76A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003E660..0x0003E6CC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003E6CC..0x0003E708
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000408CC..0x00040AEE
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00040ED0..0x00041106
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00042F64..0x00043001
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043100..0x0004310C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000431D0..0x00043248
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043558..0x0004358F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043670..0x000436E0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004377C..0x00043793
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000437CC..0x000437EC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000437EC..0x0004381E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043868..0x0004389C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043B8C..0x00043CD5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000445B4..0x000445C2
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000470A8..0x0004758A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000476C0..0x00047738
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00047EA8..0x0004808E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00048090..0x00048148
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00048148..0x0004819C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000491D4..0x000491FF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00049200..0x00049460
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00049478..0x00049574
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00049574..0x0004957A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004957C..0x000495C2
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000495C4..0x000495E1
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000495F4..0x000497AB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000497BC..0x00049AB7
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00049BA8..0x00049C79
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00049ED0..0x0004A06D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004A61C..0x0004A956
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004B198..0x0004B279
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004B290..0x0004B327
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004C414..0x0004C6DE
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004C7F4..0x0004C8FD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004CB20..0x0004CCB1
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004CE4C..0x0004CE74
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00050524..0x00050579
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00052DE0..0x00052F12
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00055524..0x00055A59
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000595C0..0x00059660
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000596C0..0x0005975F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059BD4..0x00059C10
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059C4C..0x00059C6E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059D34..0x00059D4D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059D70..0x00059E76
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059FA0..0x0005A180
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005A2A4..0x0005A414
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B3B0..0x0005B552
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B584..0x0005B717
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C380..0x0005C3DE
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C550..0x0005C5CB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C5CC..0x0005C614
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C930..0x0005C999
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00062610..0x00062810
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00062828..0x00062892
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000628AC..0x00062916
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000629B0..0x00062B7D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00062B80..0x00062D93
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00062D94..0x00062E9D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00062EA0..0x00062ECF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00062ED0..0x00062FC0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00063098..0x00063102
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000636D0..0x00063748
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00063EC0..0x00063EDD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00063EE0..0x00063F48
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00064030..0x0006409E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00064164..0x0006419C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000642CC..0x00064301
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000644D4..0x00064524
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000682C0..0x00068325
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006B413..0x0006B423
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006DF1C..0x0006DF90
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E0D0..0x0006E0E2
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E1D0..0x0006E318
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E500..0x0006E519
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E524..0x0006E545
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E558..0x0006E571
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E57C..0x0006E59D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006FF10..0x0006FF4D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006FF50..0x00070084
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000700C0..0x0007010E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000703E0..0x0007047E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00070480..0x0007054B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00070550..0x000705C4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000705D0..0x00070610
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000715D0..0x00071838
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0007234C..0x00072375
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0007331C..0x00073355
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000733BF..0x00073400
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0007CB24..0x0007CB96
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0007CEB0..0x0007CF14
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0007DC00..0x0007DD15
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008A564..0x0008A5BD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008A794..0x0008A7E5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008A7E8..0x0008A86D
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 102C:0002..102D:000A
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1043:000D..1047:0005
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 108E:0006..1090:0000
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 10C5:0003..10C6:000B
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 11A6:0002..11A7:000A
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1411:0003..1414:000A
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1629:0005..162D:0002
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1632:000B..1637:0000
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 163F:0007..1641:0006
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 165E:000A..166B:0001
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1BAB:0007..1BAE:000E
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1BAE:000E..1BBE:000D
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C31:000E..1C57:0001
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C57:0001..1C67:0004
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C8B:0005..1C98:0008
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1CA9:0008..1CCA:0005
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1E01:0008..1E08:000B
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1E08:000B..1E0A:000B
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1E21:0003..1E2D:0004
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 20F3:000E..210E:0004
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2162:0007..2177:0002
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2177:0002..217C:0001
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 219A:0007..21A0:0004
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 21A0:0004..21F1:0004
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2387:0009..238A:0005
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 238A:0005..23B9:000A
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 23C9:0001..23CD:0006
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 27DB:000B..27F1:0007
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2811:0000..2813:0004
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2856:000E..285D:000D
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 285D:000D..289A:000E
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 289A:000E..28A9:0008
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 28A9:0008..28FA:000B
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 28FA:000B..2909:0001
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2909:0001..2914:000D
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2925:0003..292E:000B
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2936:0009..2943:0000
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2943:0000..2959:0000
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2CBA:0000..2CBB:0004
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2CCB:000C..2CCD:000A
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2DDA:000A..2E04:0008
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2FA4:0005..2FA6:0006
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2FB5:0008..2FB7:0007
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001B250..0x0001B282
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001B284..0x0001B294
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00016008..0x00016078
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00016078..0x00016124
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00016124..0x000162A0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006B3EB..0x0006B3F1
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006B3F1..0x0006B413
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2DBF:0005..2DC7:000B
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2DC7:000B..2DD6:000B
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2DD6:000B..2DD9:000D
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2DD9:000D..2DDB:0005
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 226F:0009..2271:0006
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2271:0006..2273:0009
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 3044:0000..304F:0002
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 304F:0002..3055:000A
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 3055:000A..305A:000D
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 15FF:0006..1602:0006
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1602:0006..1604:000F
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1604:000F..1609:0004
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1609:0004..160D:0005
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B80:0007..1B8A:0005
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B8A:0005..1B94:0007
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2643:000D..2652:0003
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2652:0003..265D:000E
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2CC8:0006..2CCB:000C
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2CCD:000A..2CD1:0003
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029E58..0x00029E7D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029E80..0x00029EA9
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029EF0..0x00029F10
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029F5C..0x00029F6E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029F70..0x00029F86
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B554..0x0005B583
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004310C..0x00043121
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043124..0x00043132
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002C20C..0x0002C215
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002C218..0x0002C236
tool: Ghidra 12.1.3 function-body metadata export
environment: null
---

## Observation

The hash-verified analysis snapshots define the following function bodies. Each
row records the entry address, every separate body range and their total byte
count. A location above is the bounding range of the complete body, including
any gaps between its separate ranges. Range ends name the byte after the body.
The old whole-function location bounds matched the first body byte and the
last body byte of these definitions. Their conversion changes the endpoint
notation while keeping the complete function body in scope.

| File | Source XXH3-128 | Source SHA-256 | Database snapshot SHA-256 | Export script SHA-256 |
|---|---|---|---|---|
| CD:CONFIG.EXE | 1a8dc11fdd903a1c1e3994d6cba341e4 | f779912f559e82163b177b3b71b1e7f62d206dffa7f38c44626eaa1cfaaa7e70 | d3e334b8b5cafa168a3d8ce91ee3db59e467dc968cbbb76e89db23fd33cc95b7 | 01ea48f91c448ef5b35fdcbb67927a1e28e553dc5294e3de32204c4d20e169a7 |
| CD:CONQUER.EXE | 5106f53f8201761cb5112034f6c594d4 | 5d7231758766204ad061e6b82cf2f0e0cbe28899b35d095f13e4aad75c8b79d6 | c372f3b14028601943281c0e4ddfbe02509ee0fa013cc5b0e81b514d7d6dfe8d | 01ea48f91c448ef5b35fdcbb67927a1e28e553dc5294e3de32204c4d20e169a7 |
| CD:INST.EXE | bf7404f9532b9075dbe7004337db4119 | 44d4250461ba542d4f7d63b04e0e1958410bf2067e355a1d7534a9c620882423 | d3e334b8b5cafa168a3d8ce91ee3db59e467dc968cbbb76e89db23fd33cc95b7 | 01ea48f91c448ef5b35fdcbb67927a1e28e553dc5294e3de32204c4d20e169a7 |

| File | Function entry | Body ranges (half-open) | Body bytes |
|---|---|---|---:|
| CD:CONFIG.EXE | `1066:0008` | `1066:0008..1067:0007` | 15 |
| CD:CONFIG.EXE | `1183:0009` | `1183:0009..1186:0005` | 44 |
| CD:CONFIG.EXE | `152C:0004` | `152C:0004..1543:0003` | 367 |
| CD:CONFIG.EXE | `1543:0003` | `1543:0003..1548:0002` | 79 |
| CD:CONFIG.EXE | `189A:0007` | `189A:0007..189F:0002` | 75 |
| CD:CONFIG.EXE | `1B20:0000` | `1B20:0000..1B23:0003 1B24:000F..1B25:0005 1B77:0008..1B7C:0003` | 132 |
| CD:CONFIG.EXE | `1B94:0007` | `1B94:0007..1B95:000F 1B97:000E..1B98:0002` | 28 |
| CD:CONFIG.EXE | `1EED:000A` | `1EED:000A..1EF9:0003` | 185 |
| CD:CONFIG.EXE | `20B2:000B` | `20B2:000B..20F3:000E` | 1043 |
| CD:CONFIG.EXE | `213C:0007` | `213C:0007..2141:0009` | 82 |
| CD:CONFIG.EXE | `2146:0001` | `2146:0001..2152:0004` | 195 |
| CD:CONFIG.EXE | `229D:0002` | `229D:0002..22A4:000F` | 125 |
| CD:CONFIG.EXE | `2DA2:000D` | `2DA2:000D..2DBC:0003` | 406 |
| CD:CONFIG.EXE | `2DE5:000D` | `2DE5:000D..2DEA:000B` | 78 |
| CD:CONFIG.EXE | `2EC7:0008` | `2EC7:0008..2ED3:000E` | 198 |
| CD:CONFIG.EXE | `2F0C:000C` | `2F0C:000C..2F2C:0000` | 500 |
| CD:CONFIG.EXE | `3039:000D` | `3039:000D..3044:0000` | 163 |
| CD:CONQUER.EXE | `0x0001074C` | `0x0001074C..0x000109AB` | 607 |
| CD:CONQUER.EXE | `0x000110E8` | `0x000110E8..0x00011280` | 408 |
| CD:CONQUER.EXE | `0x00012A4C` | `0x00012A4C..0x00012AEA` | 158 |
| CD:CONQUER.EXE | `0x000130E4` | `0x000130E4..0x00013166` | 130 |
| CD:CONQUER.EXE | `0x00015920` | `0x00015920..0x00015AA0` | 384 |
| CD:CONQUER.EXE | `0x00015EAC` | `0x00015EAC..0x00015EBB` | 15 |
| CD:CONQUER.EXE | `0x00015EF0` | `0x00015EF0..0x00015F0A` | 26 |
| CD:CONQUER.EXE | `0x00015F0C` | `0x00015F0C..0x00015F87` | 123 |
| CD:CONQUER.EXE | `0x00018334` | `0x00018334..0x00018392` | 94 |
| CD:CONQUER.EXE | `0x00018430` | `0x00018430..0x0001869C` | 620 |
| CD:CONQUER.EXE | `0x0001869C` | `0x0001869C..0x0001884B` | 431 |
| CD:CONQUER.EXE | `0x00018850` | `0x00018850..0x00018B0D` | 701 |
| CD:CONQUER.EXE | `0x0001BB5C` | `0x0001BB5C..0x0001BC0D` | 177 |
| CD:CONQUER.EXE | `0x00021E88` | `0x00021E88..0x00021EEB` | 99 |
| CD:CONQUER.EXE | `0x00022564` | `0x00022564..0x0002258E` | 42 |
| CD:CONQUER.EXE | `0x00024C20` | `0x00024C20..0x00024C36` | 22 |
| CD:CONQUER.EXE | `0x00024C38` | `0x00024C38..0x00024C4C` | 20 |
| CD:CONQUER.EXE | `0x00024C4C` | `0x00024C4C..0x00024C9D` | 81 |
| CD:CONQUER.EXE | `0x00024D14` | `0x00024D14..0x00024D54` | 64 |
| CD:CONQUER.EXE | `0x00028408` | `0x00028408..0x000289CA` | 1474 |
| CD:CONQUER.EXE | `0x000296B4` | `0x000296B4..0x000297A0` | 236 |
| CD:CONQUER.EXE | `0x0002B060` | `0x0002B060..0x0002B1EC` | 396 |
| CD:CONQUER.EXE | `0x0002DC08` | `0x0002DC08..0x0002DD3C` | 308 |
| CD:CONQUER.EXE | `0x0002FC70` | `0x0002FC70..0x0002FCAC` | 60 |
| CD:CONQUER.EXE | `0x0002FCB0` | `0x0002FCB0..0x0002FCED` | 61 |
| CD:CONQUER.EXE | `0x00035924` | `0x00035924..0x00035C5A` | 822 |
| CD:CONQUER.EXE | `0x00038678` | `0x00038678..0x00038681` | 9 |
| CD:CONQUER.EXE | `0x00038A8C` | `0x00038A8C..0x00038B3D` | 177 |
| CD:CONQUER.EXE | `0x00039428` | `0x00039428..0x000395E1 0x00039662..0x000397AA 0x00039866..0x000398FD` | 920 |
| CD:CONQUER.EXE | `0x0003A6A0` | `0x0003A6A0..0x0003A737` | 151 |
| CD:CONQUER.EXE | `0x0003C6E0` | `0x0003C6E0..0x0003C76A` | 138 |
| CD:CONQUER.EXE | `0x0003E660` | `0x0003E660..0x0003E6CC` | 108 |
| CD:CONQUER.EXE | `0x0003E6CC` | `0x0003E6CC..0x0003E708` | 60 |
| CD:CONQUER.EXE | `0x000408CC` | `0x000408CC..0x00040AEE` | 546 |
| CD:CONQUER.EXE | `0x00040ED0` | `0x00040ED0..0x00041106` | 566 |
| CD:CONQUER.EXE | `0x00042F64` | `0x00042F64..0x00043001` | 157 |
| CD:CONQUER.EXE | `0x00043100` | `0x00043100..0x0004310C` | 12 |
| CD:CONQUER.EXE | `0x000431D0` | `0x000431D0..0x00043248` | 120 |
| CD:CONQUER.EXE | `0x00043558` | `0x00043558..0x0004358F` | 55 |
| CD:CONQUER.EXE | `0x00043670` | `0x00043670..0x000436E0` | 112 |
| CD:CONQUER.EXE | `0x0004377C` | `0x0004377C..0x00043793` | 23 |
| CD:CONQUER.EXE | `0x000437CC` | `0x000437CC..0x000437EC` | 32 |
| CD:CONQUER.EXE | `0x000437EC` | `0x000437EC..0x0004381E` | 50 |
| CD:CONQUER.EXE | `0x00043868` | `0x00043868..0x0004389C` | 52 |
| CD:CONQUER.EXE | `0x00043B8C` | `0x00043B8C..0x00043CD5` | 329 |
| CD:CONQUER.EXE | `0x000445B4` | `0x000445B4..0x000445C2` | 14 |
| CD:CONQUER.EXE | `0x000470A8` | `0x000470A8..0x0004758A` | 1250 |
| CD:CONQUER.EXE | `0x000476C0` | `0x000476C0..0x00047738` | 120 |
| CD:CONQUER.EXE | `0x00047EA8` | `0x00047EA8..0x0004808E` | 486 |
| CD:CONQUER.EXE | `0x00048090` | `0x00048090..0x00048148` | 184 |
| CD:CONQUER.EXE | `0x00048148` | `0x00048148..0x0004819C` | 84 |
| CD:CONQUER.EXE | `0x000491D4` | `0x000491D4..0x000491FF` | 43 |
| CD:CONQUER.EXE | `0x00049200` | `0x00049200..0x00049460` | 608 |
| CD:CONQUER.EXE | `0x00049478` | `0x00049478..0x00049574` | 252 |
| CD:CONQUER.EXE | `0x00049574` | `0x00049574..0x0004957A` | 6 |
| CD:CONQUER.EXE | `0x0004957C` | `0x0004957C..0x000495C2` | 70 |
| CD:CONQUER.EXE | `0x000495C4` | `0x000495C4..0x000495E1` | 29 |
| CD:CONQUER.EXE | `0x000495F4` | `0x000495F4..0x0004964B 0x00049776..0x0004977F 0x000497A4..0x000497AB` | 103 |
| CD:CONQUER.EXE | `0x000497BC` | `0x000497BC..0x000498CD 0x00049A7F..0x00049A88 0x00049AB0..0x00049AB7` | 289 |
| CD:CONQUER.EXE | `0x00049BA8` | `0x00049BA8..0x00049C79` | 209 |
| CD:CONQUER.EXE | `0x00049ED0` | `0x00049ED0..0x0004A06D` | 413 |
| CD:CONQUER.EXE | `0x0004A61C` | `0x0004A61C..0x0004A956` | 826 |
| CD:CONQUER.EXE | `0x0004B198` | `0x0004B198..0x0004B279` | 225 |
| CD:CONQUER.EXE | `0x0004B290` | `0x0004B290..0x0004B2C5 0x0004B318..0x0004B327` | 68 |
| CD:CONQUER.EXE | `0x0004C414` | `0x0004C414..0x0004C6DE` | 714 |
| CD:CONQUER.EXE | `0x0004C7F4` | `0x0004C7F4..0x0004C8FD` | 265 |
| CD:CONQUER.EXE | `0x0004CB20` | `0x0004CB20..0x0004CCB1` | 401 |
| CD:CONQUER.EXE | `0x0004CE4C` | `0x0004CE4C..0x0004CE74` | 40 |
| CD:CONQUER.EXE | `0x00050524` | `0x00050524..0x00050579` | 85 |
| CD:CONQUER.EXE | `0x00052DE0` | `0x00052DE0..0x00052F12` | 306 |
| CD:CONQUER.EXE | `0x00055524` | `0x00055524..0x00055A59` | 1333 |
| CD:CONQUER.EXE | `0x000595C0` | `0x000595C0..0x00059660` | 160 |
| CD:CONQUER.EXE | `0x000596C0` | `0x000596C0..0x0005975F` | 159 |
| CD:CONQUER.EXE | `0x00059BD4` | `0x00059BD4..0x00059C10` | 60 |
| CD:CONQUER.EXE | `0x00059C4C` | `0x00059C4C..0x00059C6E` | 34 |
| CD:CONQUER.EXE | `0x00059D34` | `0x00059D34..0x00059D4D` | 25 |
| CD:CONQUER.EXE | `0x00059D70` | `0x00059D70..0x00059E76` | 262 |
| CD:CONQUER.EXE | `0x00059FA0` | `0x00059FA0..0x0005A180` | 480 |
| CD:CONQUER.EXE | `0x0005A2A4` | `0x0005A2A4..0x0005A414` | 368 |
| CD:CONQUER.EXE | `0x0005B3B0` | `0x0005B3B0..0x0005B552` | 418 |
| CD:CONQUER.EXE | `0x0005B584` | `0x0005B584..0x0005B717` | 403 |
| CD:CONQUER.EXE | `0x0005C380` | `0x0005C380..0x0005C3DE` | 94 |
| CD:CONQUER.EXE | `0x0005C550` | `0x0005C550..0x0005C5CB` | 123 |
| CD:CONQUER.EXE | `0x0005C5CC` | `0x0005C5CC..0x0005C614` | 72 |
| CD:CONQUER.EXE | `0x0005C930` | `0x0005C930..0x0005C999` | 105 |
| CD:CONQUER.EXE | `0x00062610` | `0x00062610..0x00062810` | 512 |
| CD:CONQUER.EXE | `0x00062828` | `0x00062828..0x00062875 0x00062887..0x00062892` | 88 |
| CD:CONQUER.EXE | `0x000628AC` | `0x000628AC..0x000628F9 0x0006290B..0x00062916` | 88 |
| CD:CONQUER.EXE | `0x000629B0` | `0x000629B0..0x00062B7D` | 461 |
| CD:CONQUER.EXE | `0x00062B80` | `0x00062B80..0x00062D93` | 531 |
| CD:CONQUER.EXE | `0x00062D94` | `0x00062D94..0x00062E9D` | 265 |
| CD:CONQUER.EXE | `0x00062EA0` | `0x00062EA0..0x00062ECF` | 47 |
| CD:CONQUER.EXE | `0x00062ED0` | `0x00062ED0..0x00062FC0` | 240 |
| CD:CONQUER.EXE | `0x00063098` | `0x00063098..0x00063102` | 106 |
| CD:CONQUER.EXE | `0x000636D0` | `0x000636D0..0x00063748` | 120 |
| CD:CONQUER.EXE | `0x00063EC0` | `0x00063EC0..0x00063EDD` | 29 |
| CD:CONQUER.EXE | `0x00063EE0` | `0x00063EE0..0x00063F48` | 104 |
| CD:CONQUER.EXE | `0x00064030` | `0x00064030..0x0006409E` | 110 |
| CD:CONQUER.EXE | `0x00064164` | `0x00064164..0x0006419C` | 56 |
| CD:CONQUER.EXE | `0x000642CC` | `0x000642CC..0x00064301` | 53 |
| CD:CONQUER.EXE | `0x000644D4` | `0x000644D4..0x00064524` | 80 |
| CD:CONQUER.EXE | `0x000682C0` | `0x000682C0..0x00068325` | 101 |
| CD:CONQUER.EXE | `0x0006B413` | `0x0006B413..0x0006B423` | 16 |
| CD:CONQUER.EXE | `0x0006DF1C` | `0x0006DF1C..0x0006DF90` | 116 |
| CD:CONQUER.EXE | `0x0006E0D0` | `0x0006E0D0..0x0006E0E2` | 18 |
| CD:CONQUER.EXE | `0x0006E1D0` | `0x0006E1D0..0x0006E318` | 328 |
| CD:CONQUER.EXE | `0x0006E500` | `0x0006E500..0x0006E519` | 25 |
| CD:CONQUER.EXE | `0x0006E524` | `0x0006E524..0x0006E545` | 33 |
| CD:CONQUER.EXE | `0x0006E558` | `0x0006E558..0x0006E571` | 25 |
| CD:CONQUER.EXE | `0x0006E57C` | `0x0006E57C..0x0006E59D` | 33 |
| CD:CONQUER.EXE | `0x0006FF10` | `0x0006FF10..0x0006FF4D` | 61 |
| CD:CONQUER.EXE | `0x0006FF50` | `0x0006FF50..0x00070084` | 308 |
| CD:CONQUER.EXE | `0x000700C0` | `0x000700C0..0x0007010E` | 78 |
| CD:CONQUER.EXE | `0x000703E0` | `0x000703E0..0x0007047E` | 158 |
| CD:CONQUER.EXE | `0x00070480` | `0x00070480..0x0007054B` | 203 |
| CD:CONQUER.EXE | `0x00070550` | `0x00070550..0x000705C4` | 116 |
| CD:CONQUER.EXE | `0x000705D0` | `0x000705D0..0x00070610` | 64 |
| CD:CONQUER.EXE | `0x000715D0` | `0x000715D0..0x00071838` | 616 |
| CD:CONQUER.EXE | `0x0007234C` | `0x0007234C..0x00072375` | 41 |
| CD:CONQUER.EXE | `0x0007331C` | `0x0007331C..0x00073355` | 57 |
| CD:CONQUER.EXE | `0x000733BF` | `0x000733BF..0x00073400` | 65 |
| CD:CONQUER.EXE | `0x0007CB24` | `0x0007CB24..0x0007CB96` | 114 |
| CD:CONQUER.EXE | `0x0007CEB0` | `0x0007CEB0..0x0007CF14` | 100 |
| CD:CONQUER.EXE | `0x0007DC00` | `0x0007DC00..0x0007DD15` | 277 |
| CD:CONQUER.EXE | `0x0008A564` | `0x0008A564..0x0008A5BD` | 89 |
| CD:CONQUER.EXE | `0x0008A794` | `0x0008A794..0x0008A7E5` | 81 |
| CD:CONQUER.EXE | `0x0008A7E8` | `0x0008A7E8..0x0008A86D` | 133 |
| CD:INST.EXE | `102C:0002` | `102C:0002..102D:000A` | 24 |
| CD:INST.EXE | `1043:000D` | `1043:000D..1047:0005` | 56 |
| CD:INST.EXE | `108E:0006` | `108E:0006..1090:0000` | 26 |
| CD:INST.EXE | `10C5:0003` | `10C5:0003..10C6:000B` | 24 |
| CD:INST.EXE | `11A6:0002` | `11A6:0002..11A7:000A` | 24 |
| CD:INST.EXE | `1411:0003` | `1411:0003..1414:000A` | 55 |
| CD:INST.EXE | `1629:0005` | `1629:0005..162D:0002` | 61 |
| CD:INST.EXE | `1632:000B` | `1632:000B..1637:0000` | 69 |
| CD:INST.EXE | `163F:0007` | `163F:0007..1641:0006` | 31 |
| CD:INST.EXE | `165E:000A` | `165E:000A..166B:0001` | 199 |
| CD:INST.EXE | `1BAB:0007` | `1BAB:0007..1BAE:000E` | 55 |
| CD:INST.EXE | `1BAE:000E` | `1BAE:000E..1BBE:000D` | 255 |
| CD:INST.EXE | `1C31:000E` | `1C31:000E..1C57:0001` | 595 |
| CD:INST.EXE | `1C57:0001` | `1C57:0001..1C67:0004` | 259 |
| CD:INST.EXE | `1C8B:0005` | `1C8B:0005..1C98:0008` | 211 |
| CD:INST.EXE | `1CA9:0008` | `1CA9:0008..1CCA:0005` | 525 |
| CD:INST.EXE | `1E01:0008` | `1E01:0008..1E08:000B` | 115 |
| CD:INST.EXE | `1E08:000B` | `1E08:000B..1E0A:000B` | 32 |
| CD:INST.EXE | `1E21:0003` | `1E21:0003..1E2D:0004` | 193 |
| CD:INST.EXE | `20F3:000E` | `20F3:000E..210E:0004` | 422 |
| CD:INST.EXE | `2162:0007` | `2162:0007..2177:0002` | 331 |
| CD:INST.EXE | `2177:0002` | `2177:0002..217C:0001` | 79 |
| CD:INST.EXE | `219A:0007` | `219A:0007..21A0:0004` | 93 |
| CD:INST.EXE | `21A0:0004` | `21A0:0004..21F1:0004` | 1296 |
| CD:INST.EXE | `2387:0009` | `2387:0009..238A:0005` | 44 |
| CD:INST.EXE | `238A:0005` | `238A:0005..23B9:000A` | 757 |
| CD:INST.EXE | `23C9:0001` | `23C9:0001..23CD:0006` | 69 |
| CD:INST.EXE | `27DB:000B` | `27DB:000B..27F1:0007` | 348 |
| CD:INST.EXE | `2811:0000` | `2811:0000..2813:0004` | 36 |
| CD:INST.EXE | `2856:000E` | `2856:000E..285D:000D` | 111 |
| CD:INST.EXE | `285D:000D` | `285D:000D..289A:000E` | 977 |
| CD:INST.EXE | `289A:000E` | `289A:000E..28A9:0008` | 234 |
| CD:INST.EXE | `28A9:0008` | `28A9:0008..28FA:000B` | 1299 |
| CD:INST.EXE | `28FA:000B` | `28FA:000B..2909:0001` | 230 |
| CD:INST.EXE | `2909:0001` | `2909:0001..2914:000D` | 188 |
| CD:INST.EXE | `2925:0003` | `2925:0003..292E:000B` | 152 |
| CD:INST.EXE | `2936:0009` | `2936:0009..2943:0000` | 199 |
| CD:INST.EXE | `2943:0000` | `2943:0000..2959:0000` | 352 |
| CD:INST.EXE | `2CBA:0000` | `2CBA:0000..2CBB:0004` | 20 |
| CD:INST.EXE | `2CCB:000C` | `2CCB:000C..2CCD:0002 2CCD:0004..2CCD:000A` | 28 |
| CD:INST.EXE | `2DDA:000A` | `2DDA:000A..2E04:0008` | 670 |
| CD:INST.EXE | `2FA4:0005` | `2FA4:0005..2FA6:0006` | 33 |
| CD:INST.EXE | `2FB5:0008` | `2FB5:0008..2FB7:0007` | 31 |

| CD:CONQUER.EXE | `0x0001B250` | `0x0001B250..0x0001B282` | 50 |
| CD:CONQUER.EXE | `0x0001B284` | `0x0001B284..0x0001B294` | 16 |
| CD:CONQUER.EXE | `0x00016008` | `0x00016008..0x00016078` | 112 |
| CD:CONQUER.EXE | `0x00016078` | `0x00016078..0x00016124` | 172 |
| CD:CONQUER.EXE | `0x00016124` | `0x00016124..0x000162A0` | 380 |
| CD:CONQUER.EXE | `0x0006B3EB` | `0x0006B3EB..0x0006B3F1` | 6 |
| CD:CONQUER.EXE | `0x0006B3F1` | `0x0006B3F1..0x0006B413` | 34 |

The return site named by FND-DRAGON-002 at `0x0001BB5A` is a decoded
one-byte terminal instruction in the same CONQUER.EXE snapshot. Its instruction
start is `0x0001BB5A` and its exclusive end is `0x0001BB5B`. The finding's own
reproduction text names this instruction as the last item, so the corrected
location includes it without changing the address that identifies the return.

| CD:CONFIG.EXE | `2DBF:0005` | `2DBF:0005..2DC7:000B` | 134 |
| CD:CONFIG.EXE | `2DC7:000B` | `2DC7:000B..2DD6:000B` | 240 |
| CD:CONFIG.EXE | `2DD6:000B` | `2DD6:000B..2DD9:000D` | 50 |
| CD:CONFIG.EXE | `2DD9:000D` | `2DD9:000D..2DDB:0005` | 24 |
| CD:CONFIG.EXE | `226F:0009` | `226F:0009..2271:0006` | 29 |
| CD:CONFIG.EXE | `2271:0006` | `2271:0006..2273:0009` | 35 |
| CD:CONFIG.EXE | `3044:0000` | `3044:0000..304F:0002` | 178 |
| CD:CONFIG.EXE | `304F:0002` | `304F:0002..3055:000A` | 104 |
| CD:CONFIG.EXE | `3055:000A` | `3055:000A..305A:000D` | 83 |
| CD:CONFIG.EXE | `15FF:0006` | `15FF:0006..1602:0006` | 48 |
| CD:CONFIG.EXE | `1602:0006` | `1602:0006..1604:000F` | 41 |
| CD:CONFIG.EXE | `1604:000F` | `1604:000F..1609:0004` | 69 |
| CD:CONFIG.EXE | `1609:0004` | `1609:0004..160D:0005` | 65 |
| CD:CONFIG.EXE | `1B80:0007` | `1B80:0007..1B8A:0005` | 158 |
| CD:CONFIG.EXE | `1B8A:0005` | `1B8A:0005..1B94:0007` | 162 |
| CD:INST.EXE | `2643:000D` | `2643:000D..2652:0003` | 230 |
| CD:INST.EXE | `2652:0003` | `2652:0003..265D:000E` | 187 |
| CD:INST.EXE | `2CC8:0006` | `2CC8:0006..2CC9:000C 2CC9:000E..2CCB:0004 2CCB:0006..2CCB:000C` | 50 |
| CD:INST.EXE | `2CCD:000A` | `2CCD:000A..2CD1:0003` | 57 |

| CD:CONQUER.EXE | `0x00029E58` | `0x00029E58..0x00029E7D` | 37 |
| CD:CONQUER.EXE | `0x00029E80` | `0x00029E80..0x00029EA9` | 41 |
| CD:CONQUER.EXE | `0x00029EF0` | `0x00029EF0..0x00029F10` | 32 |
| CD:CONQUER.EXE | `0x00029F5C` | `0x00029F5C..0x00029F6E` | 18 |
| CD:CONQUER.EXE | `0x00029F70` | `0x00029F70..0x00029F86` | 22 |
| CD:CONQUER.EXE | `0x0005B554` | `0x0005B554..0x0005B583` | 47 |

| CD:CONQUER.EXE | `0x0004310C` | `0x0004310C..0x00043121` | 21 |
| CD:CONQUER.EXE | `0x00043124` | `0x00043124..0x00043132` | 14 |
| CD:CONQUER.EXE | `0x0002C20C` | `0x0002C20C..0x0002C215` | 9 |
| CD:CONQUER.EXE | `0x0002C218` | `0x0002C218..0x0002C236` | 30 |

## Interpretation

These extents support converting whole-function last-byte endpoints to exclusive
endpoints. They describe the validated analyzer snapshots. Function discovery,
ownership, native reachability and complete readings remain provisional; a body
extent does not establish the behavior of its instructions.

## Alternatives

Treating body size as a contiguous range would include gaps and omit separate
tails. The separate ranges above preserve the actual address sets. Partial
procedure ranges need their own intended-end evidence and are outside this
finding. The LE mapping retains the raw fixup operands as FND-RES-009 records.

## How to reproduce

First unpack CD:INST.EXE as FND-RES-066 specifies and verify its unpacked
identity against BLD-GOG-EN. The table names that analyzed form for INST.EXE.
Use the source files and identities above from BLD-GOG-EN. Import the LE source
using the mapping in FND-RES-009, and CONFIG.EXE with the Old-style DOS Executable
loader at load segment 0x1000. Use Ghidra 12.1.3 with x86:LE:32:default for the
LE image and x86:LE:16:Real Mode for CONFIG.EXE and INST.EXE. Freeze the analyzed projects.

Export the selected snapshot with `-readOnly -noanalysis` and
`ExportCoverageSnapshot.java <new local directory> <source SHA-256>` from the
script under tools/ghidra at revision 283e4d4. It verifies the source fingerprint
and writes only address/count metadata. Require its COVERAGE_EXPORT_COMPLETE
marker and compare its body ranges and byte counts with each table row.

Use tools/evidence/coverage-snapshot.mjs at the same revision to compute the
ordered project-directory SHA-256 and to map the exported coordinates into
the file notation. Verify each source XXH3 against the build and each snapshot
digest against the table. Export the same stabilized snapshot twice and require
identical output files and an unchanged snapshot digest. Keep raw exports and
analysis databases local. No original function or game is executed.

For the named return boundary, use the metadata-only ExportBoundaryMetadata.java
script under tools/ghidra with `-readOnly -noanalysis`, a new local output file,
the CONQUER.EXE SHA-256 above, and address `0001bb5a`. Require the
BOUNDARY_EXPORT_COMPLETE marker. The output must identify instruction start
`0001bb5a`, size 1, and flow TERMINATOR. It exports no instruction bytes or
operands. This checks the last-item extent, not the worker's behavior.
