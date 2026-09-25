---
id: FND-ESTATE-002
title: War Planning stages five armies, prices companies by fame and commits on OK
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00035C60..0x00035F95
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00035F96..0x00036174
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00036178..0x000361B8
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000362D0..0x000363B3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000363B4..0x0003663C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00036640..0x0003669A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000367D8..0x000368E3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000368E4..0x00036A19
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00036AFC..0x00036CF7
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00036CF8..0x00036EC3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00037300..0x0003735E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029E58..0x00029F85
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00011280..0x0001133A
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00035C60` registers the screen's routines through `0x00059C10` (open, `0x00035F98`), `0x00059C24`
(keys, `0x00036178`), `0x00059C38` (close, `0x000361BC`) and `0x00059C4C(region, event, routine)`.
For event 2, the left button, regions 0 to 4 call `0x000362D0`, region 6 `0x00036720`, regions 8 to
10 `0x000367D8`, region 11 `0x00036194`, region 12 `0x00036AFC` and region 13 `0x00036640`; regions 5
and 7 call routines that only return. For event 3, the right button, regions 0 to 4 call `0x000363B4`
and regions 8 to 10 `0x000368E4`. Events 0 and 1 draw hover labels. The key routine calls
`0x00036AFC` for key 13 and `0x00036194` for key 27. `0x00036194` plays a sound and calls
`0x00059760(1, 1)`.

The open routine calls `0x00063050`, then `0x000386A0` when `0x000386CC` returns 0, allocates a
buffer and calls `0x00036CF8`. `0x00036CF8` allocates the staging lists: a pointer at `0x0009AD74` to
five pointers to six dwords each, and a pointer at `0x0009AD70` to five dwords. For each army `a`
from 0 to 4 it stores `0x00029F5C(a)` in the second list, stores `a` in `0x0009AD78` when
`0x00011320(a)` returns 1, and for each `t` from 0 to 5 stores `0x00029E58(a, t)` both in the six
dwords of army `a` and at `0x000AA0AC + 12 * a + 4 * t`, then stores 0 at `0x0009AD7C + 4 * a`. It
copies each army's name from `0x00029F34(a)` to the buffer the pointer at `0x000AA094 + 4 * a`
points to, stores 0 in `0x000AA0EC`, `0x0002D674(0)` in `0x000AA0A8`, `0x0002D620(0)` in `0x000AA0E8`
and attribute 17 of row 0 in `0x000AA090`. Back in the open routine, when `0x00015EF0(0, 6)` is above
16 it stores, through `0x00029ECC(t, v)` and `0x00029F10(t, v)`, one set of three prices and three
monthly costs, and otherwise a second set whose every price is 8 higher and whose every monthly cost
is 3 to 6 higher. It then stores 0 in `0x0009AD6C` and draws the screen, the five army buttons in
the style of an empty army when all six of the army's staged counts are 0.

`0x00029E58(a, t)` and `0x00029E80(a, t, n)` read and write the dword at `+0xA4 + 92 * t` of the
record the pointer at `0x000A9CD0 + 4 * a` points to. `0x00029EAC(t)` and `0x00029ECC(t, v)` use
`+0xA8 + 92 * t`, and `0x00029EF0(t)` and `0x00029F10(t, v)` `+0xAC + 92 * t`, always of record 0.
`0x00029F34(a)` returns record `a`'s address, `0x00029F40(a, s)` copies up to 80 bytes of `s` to it,
and `0x00029F5C(a)` and `0x00029F70(a, v)` read and write its `+0x27C`.

`0x000362D0` plays a sound, redraws the button of the army in `0x0009AD6C` in its normal or empty
style, highlights the clicked army and stores it in `0x0009AD6C`.

`0x000363B4` plays a sound and, when any staged count of the clicked army `a` is not 0, does the
same. Then, when `a`'s dword in the list at `0x0009AD70` is 0, it shows the string at object-2
offset `0x4B60`, that the army has not been raised. Otherwise, when `0x00011280(a)` returns 1, it
shows `0x4AF8`, that the army is too far from home to disband. Otherwise it asks `0x4B38` through
`0x00025004` and, when that returns not 0, stores 0 in `a`'s dword of the `0x0009AD70` list, 1 at
`0x0009AD7C + 4 * a`, and for each of the six counts adds 100 times the count to `0x000AA0A8` and
to `0x000AA0E8` and stores 0 in the count. It stores `a` in `0x0009AD6C` and -1 in `0x0009AD78` when
that held `a`.

`0x000367D8` takes the unit kind `t` as the region minus 8. It plays a sound and shows `0x4C54`, that
there are not enough men, when `0x000AA0A8` is below 100; `0x4C1C`, that there is not enough money,
when `0x00029EAC(t)` is above `0x000AA090`; and `0x4BEC`, that the army is too far from home to
reinforce, when `0x00011280` returns 1 for the army in `0x0009AD6C`. Otherwise it adds 1 to the
army's staged count `t`, calls `0x00037300(100)`, `0x00037320(100)` and `0x00037340(0x00029EAC(t))`
and redraws. It has no other test. `0x00037300(n)` stores `0x000AA0A8 - n` there, or 0 when that is
negative. `0x00037320(n)` subtracts `n` from `0x000AA0E8` only when `0x000AA0E8 - (n + 100)` is 0
or more. `0x00037340(n)` stores `0x000AA090 - n` there, or 0 when that is negative.

`0x000368E4` plays a sound and shows `0x4CB4`, that the army has no warriors of the kind, when the
selected army's staged count `t` is 0, and `0x4C88`, that the army is too far from home to disband,
when `0x00011280` returns 1. Otherwise it subtracts 1 from the count, adds 100 to `0x000AA0E8` and
to `0x000AA0A8`, adds `0x00029EAC(t)` to `0x000AA090` when the army's dword at `0x0009AD7C` is 1 or
the dword at `0x000AA0AC + 12 * a + 4 * t` is 0 or less, and then subtracts 1 from that dword.

`0x00036640` edits the name the pointer at `0x000AA094 + 4 * a` points to through `0x00024FC4`, with
the title at `0x4B88` and a limit of 15.

`0x00036AFC` plays a sound. For each army `a` it adds the six staged counts. When the sum is not 0
and `a`'s dword in the `0x0009AD70` list is 0, it stores 1 there, calls `0x00012CAC(a)` when
`0x00029F5C(a)` is 0 and the dword at `0x0009AD7C + 4 * a` is 0, then calls `0x00012DD0(a)` and
`0x00012CAC(a)` when `0x00029F5C(a)` is not 0 and that dword is not 0, and redraws. When the sum is
0 and `0x00029F5C(a)` is not 0, it stores 0 in the list and calls `0x00012DD0(a)`. It then calls
`0x00029F70(a, list value)`, `0x00029F40(a, name)` and `0x00029E80(a, t, count)` for the six counts.
After the armies it calls `0x00038C68` once for each of the `0x000AA0EC` pending spies and shows
`0x4D04`, that spies are already out, each time it returns 0. Last it calls
`0x0002D654(0, 0x000AA0E8)`, `0x0002D6A0(0, 0x000AA0A8)`, `0x00015F0C(0, 17, 0x000AA090)` and
`0x00059760(1, 1)`.

`0x00011280(a)` returns 0 when the dword at `0x000AA4B8 + 0x118 * a` is 0. Otherwise it converts the
home cell at `0x000AB0D4` and `0x000AB0D0` through `0x00063E20`, subtracts the truncated floats at
`+0x5C` and `+0x60` of that record, and returns 1 when `0x0002FC70` of the sum of the squared
differences is 150 or more. `0x00011320(a)` returns 1 when the dword at `0x0009AE6C` is not 5 and
equals `a`.

## Interpretation

The screen works on staged copies: the unit counts of five armies, whether each army is raised, the
names, the free serfs, the population and the wealth. OK, Enter and buying a spy (RULE-STRATEGY-018)
write them back and leave; Cancel and Escape leave without writing, so the next opening copies the
committed values again. Each company is 100 serfs. The three unit kinds are swordsmen, halberdiers
and knights, in that order, by the hover labels at `0x4CE4`, `0x4CF0` and `0x4CFC`. The price and
monthly cost of each kind live in army record 0 and depend on FAME being above 16. An army counts as
away from home when it is on the map at a distance of 150 or more from the home anchor. The add
routine has no limit on the number of companies in an army. The dword `0x000AA0AC + 12 * a + 4 * t`
holds the committed count of kind `t` in army `a`, three to an army, but the opening copy stores six
per army, so it also writes the next army's first three entries, which the next pass overwrites,
and for army 4 the dwords `0x000AA0E8`, `0x000AA0EC` and `0x000AA0F0`.

## Alternatives

The unit order could be read from the prices alone; the hover labels and the order the outside guide
lists the kinds in agree with it.

## How to reproduce

Disassemble `0x00035C60` and follow each routine it registers, and `0x00029E58` to `0x00029F85`,
`0x00037300` to `0x0003735E` and `0x00011280` to `0x0001133A`. Read the strings at the object-2
offsets named above.
