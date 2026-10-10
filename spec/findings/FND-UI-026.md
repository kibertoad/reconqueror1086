---
id: FND-UI-026
title: Dubbing entry selects its sound bank and waits for input before returning
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00019A8C..0x00019C80
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    kind: file-data
    offset: 0x000CACDC..0x000CACE6
tool: Capstone 5.0.7, bounded x86-32 decoding of the fingerprinted relocated LE image
environment: null
---

## Observation

The entry saves ESI, EDI and EBP, then passes open archive 1 and directory
index 361 to the bank loader of FND-SOUND-002. Its result is stored through
DS at `0x0009A708`. The fingerprinted C1086.GOB directory of FMT-RES-001
names that entry `dking.666`. FND-SOUND-001 describes its one sample;
FND-UI-024's later callback requests the first sample at bank offset 4.
The bank loader returns zero when sound effects are disabled.

The entry performs the age, item and wealth preparation described by
FND-PERSON-007. It then tests the animation flag at `0x0009ADB8`
(FND-SOUND-004). With animations disabled it loads archive entry 476,
`dubb3.666`, draws the current object, and requests four text draws at
x 20 and y 365, 379, 393 and 407. Their text arguments are the data
addresses `0x000919E8`, `0x00091A2C`, `0x00091A70` and `0x00091A84`.
After the presentation request it conditionally starts this bank's first
sample and keeps the returned handle in EDI.

That branch repeatedly calls the input poll of FND-UI-019 with argument
zero: the argument push is at `0x00019BBF`, call at `0x00019BC0`, cleanup
at `0x00019BC5`, and zero result repeats the loop. A nonzero result reaches
`0x00019BCC`. If the bank exists it requests playback stop using the saved
handle and frees the bank, then joins the common path at `0x00019C32`.

With animations enabled the alternative branch requests the movie whose
identifier at the listed file-data location is `dubb3.smk`, through
`0x00030100` at `0x00019C11`. FND-MEDIA-009 describes that movie helper.
This branch also joins the common path at `0x00019C32`.

The common path requests picture index 482, the archive directory's
`FLUFF.PCX`, then presentation. It repeatedly calls the same input poll
with argument zero: push at `0x00019C51`, call at `0x00019C52`, cleanup
at `0x00019C57`, and zero result repeats. A nonzero result reaches
`0x00019C5E`, restores the three saved registers and returns at
`0x00019C61`. At either loop's argument-push boundary ESP is 12 bytes
below entry ESP; the final return restores entry ESP.

The cleanup callback at `0x00019C64` tests the stored bank at
`0x0009A708`, conditionally frees it and joins `0x00024CA0`. The
full-screen click of FND-UI-024 clears that stored value before requesting
screen replacement.

## Interpretation

Disabling animations does not remove all dubbing entry interaction. That
path requires accepted input once for the initial presentation and again
for the common briefing picture before the entry can return. The input
poll accepts the short primary or secondary releases and keyboard
availability described by FND-UI-019. Waiting for entry return before
providing either input cannot advance these loops.

The later full-screen click's stored bank is the `dking.666` bank, distinct
from the temporary `dubb3.666` bank used by the animations-disabled entry.

## Alternatives

These are presentation and playback requests, not observations of pixels,
audible output, driver progress or wall time. This bounded reading does
not establish every drawing helper, all cleanup callers or all allocation
failure behavior. It does not show that a particular pending recorded run
has reached either input loop.

## How to reproduce

Verify BLD-GOG-EN identities, relocate the LE image and decode the listed
code range. Follow both animation branches, saved registers, argument
cleanup and both input-loop exits. Compare the three directory indices
against the fingerprinted archive using FMT-RES-001, the sound banks
against FND-SOUND-001 and FND-SOUND-002, and the callback against
FND-UI-024. Locate the movie identifier in the executable's data object
at offset 0x1A88 and compare its raw file location above.
