---
id: FND-RES-031
title: SETUP expands SETUP.SOL members with a DCL-style explode whose binary-mode path is read in full
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:1104..0001:1182
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:1182..0001:11A8
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:3529..0001:3531
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:407A..0001:40A1
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:4152..0001:42A4
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:42A5..0001:4370
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:4370..0001:4417
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:44A8..0001:4516
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:4516..0001:4599
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:4599..0001:45CE
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    kind: file-data
    offset: 0x00003AD1..0x00003BA1
  - build: BLD-GOG-EN
    file: CD:SETUP.SOL
    offset: 0x1A6..0xC3848
tool: capstone 5.0.7 bounded 16-bit disassembly of SETUP segment 1; Python bounded NE relocation-chain walk; Python decoder of the procedure below, written for this finding
environment: null
---

## Observation

SETUP's length and canonical fingerprint matched BLD-GOG-EN; FND-RES-029 and
FND-RES-030 give the mapping and the caller. Segment 1 starts at file offset
0x5A0, so its offsets 0x3531 to 0x3601 are file offsets 0x3AD1 to 0x3BA1.

Entry. The far call at 0001:1367 reaches 0001:41AB, which pushes a relocated
segment word and 0x4152, calls 0001:3529 (it stores the difference between
SP before and after one push at segment 2 offset 0x03B0 and returns), and
returns far, so control continues at 0001:4152 with the caller's frame. The
arguments, after the far return address, are the work block's far address,
then two far procedure addresses whose segment words are relocated to
segment 1: 1:1182 and then 1:1104. 0001:4152 returns far, removing 12 bytes.

Callbacks. 1:1104 receives a buffer and a far pointer to a requested count.
It lowers the count to the member's remaining stored length (segment 2
offsets 0x11B6 and 0x11B8), subtracts the count from that length, reads that
many bytes from the archive handle at segment 2 offset 0x120E through the
library routine at 0001:30EA, and returns the number read. 1:1182 writes a buffer of the count
it is given to the output handle at segment 2 offset 0x11BC through a library
routine at 0001:30F1. 1:1104 is not a start in the committed inventory.

Set-up, 0001:4152. It stores the work block's address at segment 2 offsets
0x03B6 and 0x03B8, stores 1:1182 at work offsets 0x16 and 0x18 and 1:1104 at
0x12 and 0x14, sets work offset 0x0E to 0x800 and calls the read procedure
for up to 0x800 bytes into work offset 0x221E. A count of 4 or less returns
3. Otherwise the first byte (offset 0x221E) is the mode, kept at work offset
2; the second (0x221F) is the dictionary size in bits, kept at offset 6; the
third (0x2220) starts the bit buffer at offset 0x0A, with 0 bits used
(offset 0x0C) and the input position at 3 (offset 0x0E). A dictionary size
below 4 or above 6 returns 1. The mask `0xFFFF >> (16 - size)` is kept at
offset 8. Mode 0 goes on; mode 1 first copies 256 bytes from CS:0x3601 and
calls 0001:45CF, which this finding does not read; any other mode returns 2.
It then copies the 16 bytes at CS:0x35E1 (length-code bit counts), the 16 at
CS:0x35B1 (extra-bit counts), the 32 at CS:0x35C1 (16 `UINT16LE` length
bases) and the 64 at CS:0x3531 (distance-code bit counts) into the work
block, and fills a 256-entry lookup for lengths from the 16 codes at
CS:0x35F1 and one for distances from the 64 codes at CS:0x3571, through
0001:4599.

0001:4599 takes a count, a table of codes and a table of bit counts. For
each index from the last down to the first, it writes the index into every
lookup entry whose low `bits[index]` bits equal `code[index]`, for all such
entries up to 0xFF.

Bits, 0001:4516. The bit buffer is a 16-bit word whose low bit is the next
bit. Taking `n` bits shifts the word right by `n`. When fewer than `n` bits
remain, it shifts out what remains, moves the next input byte into the high
byte of the word and shifts again so that the low bits are the requested
ones; when the input position reaches the count read, it first calls the read
procedure for up to 0x800 more bytes, and a count of 0 makes the routine
return 1.

Symbol, 0001:4370. When the low bit is 1, it takes that bit, looks up the low
8 bits in the length lookup to get an index `i`, takes `lenbits[i]` bits,
and, when `extrabits[i]` is not 0, takes that many more bits as `e` and
gives `0x100 + lenbase[i] + e`, otherwise `0x100 + i`. When the low bit is 0,
it takes that bit and, in mode 0, takes 8 bits and gives them as a literal.
Any failure of the bit routine gives 0x306. The mode 1 literal path, from
0001:4417 to 0001:44A8, is not read here.

Distance, 0001:44AE, with the length. It looks up the low 8 bits in the
distance lookup to get `j` and takes `distbits[j]` bits. For a length of 2 it
gives `((j << 2) | the next 2 bits) + 1`, taking 2 bits; otherwise `((j <<
size) | (the next bits & mask)) + 1`, taking `size` bits. A failure of the
bit routine gives 0.

Loop, 0001:42A5. The output window is at work offset 0x1A and the position
starts at 0x1000. Each symbol below 0x100 is stored at the position. A symbol
from 0x100 to 0x304 gives a length of the symbol minus 0xFE; a distance of 0
ends the loop with 0x306; otherwise the length bytes are copied one at a time
from the position minus the distance. When the position reaches 0x2000, the
4,096 bytes from work offset 0x101A are written through the write procedure,
the bytes from 0x101A up to the position are moved down to 0x1A, and the
position drops by 0x1000. A symbol of 0x305 or more ends the loop and writes
the bytes from 0x101A up to the position. 0001:4152 returns 4 when the loop
ended with 0x306 and 0 otherwise. The work block is zero-filled by its
allocation (FND-RES-030), so a distance reaching before the first output
byte copies zeros.

Constant tables, read from the file: the 16 length-code bit counts are
3, 2, 3, 3, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 7, 7; the 16 length codes are
0x05, 0x03, 0x01, 0x06, 0x0A, 0x02, 0x0C, 0x14, 0x04, 0x18, 0x08, 0x30, 0x10,
0x20, 0x40, 0x00; the extra-bit counts are eight 0s, then 1 to 8; the length
bases are 0 to 8, 10, 14, 22, 38, 70, 134, 262. The 64 distance-code bit
counts are one 2, two 4s, four 5s, fifteen 6s, twenty-six 7s and sixteen
8s, in that order; RULE-RES-005 gives the 64 distance codes.

Shipped data. A decoder written for this finding from the procedure above,
taking its tables from SETUP's bytes, expanded all 15 members of
`CD:SETUP.SOL` (FND-RES-030). Each has mode 0 and dictionary size 6, ends
with symbol 0x305 on its last stored byte, never reads before the first
output byte, and gives a file whose first bytes fit its name: `MZ` for the
eight `.EXE` and `.DLL` members, 0x3F 0x5F for the five `.HLP` members, and
`MThd` for the two `.MID` members. `_SETUP.EXE` expands to 454,016 bytes.
The expanded files are kept in ignored local storage.

## Interpretation

The stream format is the one PKWARE's Data Compression Library writes in
binary mode, and SETUP's routine follows that library's explode: the error
numbers 1 to 4, the 12,574-byte work block and the tables match its
published form. The reading above, not that match, is what RULE-RES-005
rests on.

## Alternatives

- Another LZ77 coding: ruled out for SETUP's routine by the reading.
- Expansion through LZEXPAND: ruled out for SOL members; SETUP uses LZEXPAND
  only to copy whole files (FND-RES-030).

## How to reproduce

Verify SETUP's manifest size and XXH3-128. Disassemble the listed ranges of
segment 1 as 16-bit code from their first addresses and resolve the far
pushes and calls through the relocation walk of FND-RES-029. Read the
constant tables at the listed file offsets. Implement the procedure, take the
tables from the file, and expand each member of `SETUP.SOL` at the offsets
FND-RES-030 gives, checking the end symbol, the bytes consumed and the first
bytes of each output. Keep listings and outputs in ignored local storage.
