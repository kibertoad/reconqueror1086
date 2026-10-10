---
id: FND-RNG-010
title: Seed conversion combines a four-year day-count term with a 1970 epoch and normalizes before adjustments
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00080AB3..0x00080C0B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008B0B9..0x0008B1F9
tool: Capstone 5.0.9, bounded x86-32 decoding of the fingerprinted relocated LE image
environment: null
---

## Observation

FND-RNG-005 identifies the seed converter's calendar fields and partial control
flow. FND-RNG-009 supplies its two cumulative month tables. For a nonnegative
normalized year `Y` measured from 1900, the converter's year term is
`365 * Y + ((Y + 3) >> 2)`. It adds the selected month boundary and day minus
one, then subtracts one more day when `Y` is nonzero. Its arithmetic is
thirty-two-bit; the shift is arithmetic. The year term has no century division
or subtraction even though the month-table selection uses the Gregorian leap
predicate. This description does not remove its earlier year rejection paths.

It combines hour, minute and second as `((hour * 60) + minute) * 60 + second`.
While that signed quantity is negative it adds 86400 and decrements the day
count. Before consulting the environment or applying either adjustment, it
calls the second listed routine with the day count, this seconds quantity,
a third argument zero and the calendar record pointer.

On the exercised paths, that helper fills the record's seconds, minutes and
hours from divisions by 86400, 3600 and 60. It derives the year, year-day,
month and day from the day count and the common/leap tables, and writes
weekday as `(day_count + 1) % 7`, with Sunday zero. The record's offset-32
classification is not written by this helper. Its output describes the
calendar before the converter applies the adjustment, not the adjusted
instant represented by the returned quantity.

The converter then makes the environment-dependent call read in FND-RNG-006,
adds the first adjustment, and optionally obtains the offset-32 classification
and subtracts the secondary adjustment. Negative adjusted seconds are handled
by adding 86400 and decrementing the day count again. The output path subtracts
25567 from the day count and multiplies that difference by 86400 before adding
the adjusted seconds. The multiplication is synthesized with shifts, additions
and subtraction and retains thirty-two bits. The day count 25567 is the
converter's 1970-01-01 value on the zero-adjustment control in EXP-RNG-004.

A signed day count below 25566 takes the minus-one rejection path. For exactly
25566, it subtracts 86400 from the adjusted seconds and accepts only when the
first adjustment is positive and the resulting signed quantity is nonnegative.
This permits the last pre-epoch local day to produce a nonnegative adjusted
result. Other accepted dates use the day-difference multiplication above.

EXP-RNG-004 fixes a null environment array and an offset-32 classification of
zero, so neither parsing nor the classification routine runs. With those
explicit inputs, 2000-02-29 and 2100-03-01 match Gregorian epoch arithmetic.
For 2101-01-01, however, the converter returns 4134067200 and normalizes the
record to 2101-01-02. Gregorian arithmetic for the input date gives 4133980800,
one day earlier. The converter's four-year year term accounts for this
discrepancy; Gregorian month-table selection alone does not correct it.

## Interpretation

For the controlled no-environment, no-classification cases, the result is
thirty-two-bit epoch arithmetic with a 1970 origin and an explicit first
adjustment. It is not equivalent to unrestricted Gregorian timestamp
conversion. Its normalized record can differ from the input date and remains
unadjusted even when the return crosses the epoch boundary.

This is a bounded reading, not complete provenance for every seed-source
input. The environment initialization, negative-classification path and full
normalizer input domain still belong to Q-RNG-001.

## Alternatives

A pure time-of-day result is ruled out by the day-difference multiplier and
the controlled dates. Always rejecting every local date before 1970 is ruled
out by the positive-adjustment edge case. Treating Gregorian leap selection
as proof of Gregorian conversion for every year is ruled out by the year term
and the 2101 control. No live-session date, adjustment or classification is
inferred from these fabricated calls.

## How to reproduce

Decode both listed ranges, following the saved day and seconds quantities
through the normalization call and subsequent adjustments. Read the month
tables in FND-RNG-009 and the Gregorian leap predicate in FND-RNG-005.
Reconstruct the shift/add day multiplier independently. Run EXP-RNG-004's
fabricated cases with the environment root and classification explicitly
fixed as stated, checking normalized records as well as returned values.
