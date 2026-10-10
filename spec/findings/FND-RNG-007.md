---
id: FND-RNG-007
title: The environment adjustment parser consumes a bounded name and wrapping hours, minutes and seconds
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008B24B..0x0008B275
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008B275..0x0008B38C
tool: Capstone 5.0.9, bounded x86-32 decoding of the fingerprinted relocated LE image
environment: null
---

## Observation

FND-RNG-006 identifies the caller that passes a value string, a name
destination and an adjustment destination to the second listed routine.
The routine saves four registers and reserves twelve local bytes. Its three
arguments are then at stack offsets 32, 36 and 40. The temporary segment
save and copy-destination save are restored before those offsets are reused.

It skips one leading colon, then scans name bytes until a null byte, comma,
minus, plus or ASCII digit. A colon after the first byte is an ordinary name
byte. The scan consumes the full name, but limits the copied prefix to thirty
bytes and appends a null byte at the clipped end. It saves ES, loads ES from
DS and copies the prefix with a dword count and a remaining-byte count.
Both copy instructions carry an F2 prefix; the instruction decoder's printed
mnemonic alone does not expose the dword instruction's repeat prefix.
EXP-RNG-002 records the copied outputs in the explicit flat segment model.

The scan-ending minus sets a negation flag and advances the source pointer.
Plus advances it without setting that flag. If the next byte is outside
ASCII digits, the routine returns that pointer without writing the adjustment
destination. The name has already been copied and terminated in this path.
There is no separate error result.

Otherwise it zeroes three 32-bit local quantities and calls the first listed
routine for hours. That helper accumulates decimal digits in a 32-bit register
with multiplication by ten and addition, stores the result, and returns a
pointer to the first nondigit. Accumulation wraps at thirty-two bits; an
empty digit sequence stores zero. The name routine calls it for minutes
after one colon and for seconds after a second colon. An absent component
retains the initialized zero; a present colon with no digits also gives zero.

It forms the adjustment as `((hours * 60) + minutes) * 60 + seconds`, keeping
the low thirty-two bits at every arithmetic step. It stores this quantity,
and on the minus path stores its thirty-two-bit negation. It neither bounds
minutes and seconds nor rejects decimal overflow. Its return pointer is
the first byte the final numeric helper did not consume, or the earlier
scan/sign pointer on the no-number path.

## Interpretation

The helper accepts an adjustment expressed in hours with optional minute and
second components. Its bounded name output does not bound the amount of input
it scans. A name-only value leaves the caller's previous adjustment intact;
the wrapper's initialization and second-parse default therefore matter.
This reading covers the helper's syntax and outputs, not the calendar
conversion's full meaning or the validity of every environment value.

## Alternatives

A parser that always resets a missing adjustment to zero is ruled out by
the no-number path's return before the destination store. Rejecting oversized
minute/second fields or decimal overflow is ruled out by the arithmetic path
without those checks and by EXP-RNG-002's authored controls. Treating all
colons as name delimiters is ruled out by the initial-only skip and the scan
comparisons. Full epoch-seconds semantics remain open under Q-RNG-001.

## How to reproduce

Load the fingerprinted LE objects with relocations, and decode both ranges
in thirty-two-bit mode. Inspect each copy instruction's prefix bytes as well
as its mnemonic. Follow the local stack offsets through the nested numeric
calls and segment/register saves. Compare the output bytes, returned pointer
and unchanged memory against EXP-RNG-002. No claim about every caller,
environment initialization or the original operating system's segment table
is made here.
