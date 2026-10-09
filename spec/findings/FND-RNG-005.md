---
id: FND-RNG-005
title: The seed source reads DOS calendar time and rounds its seconds before conversion
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006B3B4..0x0006B3EB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000809F2..0x00080AB3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00080AB3..0x00080C0B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008AC45..0x0008AC7E
tool: Capstone 5.0.7, bounded x86-32 decoding of the fingerprinted relocated LE image
environment: null
---

## Observation

The seed source at `0x0006B3B4` allocates a 36-byte temporary record on its
stack and passes its address to `0x000809F2`. If that call returns at least
500, the source increments the record's first 32-bit field. It then passes
the record to `0x00080AB3` and returns that routine's 32-bit result unchanged.
When its own pointer argument is nonzero it also stores the same result
through that pointer. FND-RNG-003 identifies the two seeding callers, both of
which pass a null pointer; one shifts the returned value before seeding.

The reader uses interrupt `0x21` with AH `0x2A`, AH `0x2C`, and AH `0x2A`
again, in that order. It forms the record from the returned calendar/time
registers. Relative to the record pointer, the 32-bit fields are seconds at
0, minutes at 4, hours at 8, day at 12, month minus one at 16, and the low
byte of year minus 1900, zero-extended, at 20. It sets the field at 32 to
minus one. The returned quantity is ten times the hundredths value from the
time read, so the seed source's increment rounds seconds upward at half a
second. This reader does not initialize the fields at 24 or 28.

The reader compares the day byte of its two date reads. If they differ and
the time read's hour is not 23, it replaces the day/month/year fields with
the second date. When the days agree or the hour is 23, it retains the first
date. This is a day-byte comparison, not a comparison of the whole date.

The converter at `0x00080AB3` reads those date/time fields. It normalizes the
month with signed division by twelve, rejects a negative normalized year,
selects one of two month-offset tables using `0x0008AC45`, forms a day count,
and combines hours, minutes and seconds. The leap predicate returns true for
years divisible by four except those divisible by one hundred but not four
hundred.

Conversion is not just multiplication of the DOS time of day: it calls
`0x0008B0B9` to normalize the record and `0x0008B230` before adding the
32-bit value stored at `0x000A707C`. With a negative field at record offset
32, it calls `0x0008ADAB`; when that field is subsequently positive it
subtracts the value stored at `0x000A7080`. After normalization and range
checks it returns a 32-bit day/seconds combination, or minus one on its
rejection paths. The origins and complete effects of those adjustment
helpers have not been read here.

## Interpretation

The seed source depends on DOS calendar time and rounds to seconds before
conversion. It is not a raw random draw or merely a counter of hundredths
since midnight. Its exact timestamp interpretation remains Q-RNG-001:
calendar tables, adjustment initialization and the helper's write to record
offset 32 still require their own complete provenance reading. This finding
does not assert complete reading of the converter or all its callees.

## Alternatives

A direct BIOS tick count or raw hundredths value is ruled out for this
function's return by the record construction, rounding and converter call.
A calendar-based epoch-seconds interpretation remains plausible, but is not
established by naming a familiar library function or by EXP-RNG-001's seeds.

## How to reproduce

Load the fingerprinted BLD-GOG-EN LE objects with internal relocations applied.
Decode each listed exclusive range in 32-bit mode. Follow the temporary record
pointer through the reader and converter, track the returned register through
the optional pointer store, and compare the two callers from FND-RNG-003.
To settle Q-RNG-001, continue through the converter's calendar tables and
adjustment helpers, including the fields they overwrite and error exits.
