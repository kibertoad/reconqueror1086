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
