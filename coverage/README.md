# Original function inventories

Inventories contain only `start` and `size`: the Standard address and Ghidra body
byte count. No names, original strings, code, byte signatures or instruction text
are included. Paths follow the build manifest; `CD:` becomes `@CD/`.

## BLD-GOG-EN / CD:CONQUER.EXE

Exported on 2026-09-30 with Ghidra 12.1.3 and JDK 21.0.12.1 using
`tools/ghidra/ExportFunctionInventory.java`. The source was freshly extracted
from the owned disc and matched BLD-GOG-EN; SHA-256 is
`5d7231758766204ad061e6b82cf2f0e0cbe28899b35d095f13e4aad75c8b79d6`.
`MapConquerorLe.java` rejects any other source fingerprint. It imports code and
initialized data at the LE bases documented in BLD-GOG-EN, preserves uninitialized
data as an uninitialized region, and selects `x86:LE:32:default`.

The analysis is a new disposable project. The older MZ-imported project maps the
DOS stub rather than the game code and is excluded. Entry seeds are the LE entry
and neutral `fn_XXXXXXXX` names already documented in `spec/`; Ghidra's ordinary
analysis discovers additional functions. Original fixup operands remain raw.
Consequently indirect targets, shared tails, overlapping entry streams and missed
functions require further review. These are recognized analyzer functions, not a
claim that every function has been found or completely read. Body sizes are not
contiguous address spans and cannot be added to starts to infer end addresses.

The source project, logs and entry seed list remain in ignored
`analysis/documentation-audit/`. Regenerate into a new local output, reject any
script error or analysis timeout, check source identity and both object ranges,
then copy only the two exported columns. The inventory is refreshed whenever
analysis discovers missed or merged functions.

The findings study code in `CD:CONQUER.EXE`. The HMI `.386` files are examined as
data containers by FND-SOUND-001, not as function bodies; they have no inferred
function inventory. The manifest's setup executables are not silently counted as
studied code. A future analysis into one of those files must add its own inventory
with its actual loader and mapping. The DOSBox host and third-party demo programs
are outside the original game's analyzed code scope.

## Launcher inventories

AUTOPLAY.EXE and SETUP.EXE were freshly extracted with manifest size/XXH3 guards
on 2026-10-05. `launcher-inventories.json` records the SHA-256 identities, selected
code spans and analyzer mapping metadata. Ghidra 12.1.3/JDK 21.0.12.1 imported
AUTOPLAY as PE with x86:LE:32:default, and SETUP as NE with
x86:LE:16:Protected Mode. The metadata reports agree with the independent
container reads in FND-RES-024. Source and projects stay in ignored artifacts.

The pinned toolkit a260e391 `ExportFunctionInventory.java` exported only starts
and body-byte counts. The PE inventory retains preferred virtual addresses. NE
selector 1000 identifies table segment 1; the committed addresses use 0001 as
the Standard requires. Data/resource blocks and imported external functions are
excluded from the committed inventory. The NE raw export also included functions
in non-executable block 1058; canonicalization excluded that entire block using
the inspected memory map. Canonicalization checked all
starts against the executable memory block and rejected duplicate mappings.

Both imports and post-analysis completed without script errors or timeouts.
Analyzer warnings remain qualifications: PE export-directory analysis reported
an invalid/missing function and no-return name normalization; NE decompilation
reported an unread address for one function. These do not establish complete
function discovery or validate any reading. Body sizes are never function ends.
`Check-Coverage.mjs` validates both inventories, including NE table segment
numbers; it does not prove code behavior or native reachability.
