# Ghidra setup for Codex analysis

This machine has a portable, user-wide Ghidra installation for clean-room analysis of the legally owned *Conqueror: A.D. 1086* executable. It does not require administrator access and must not place Ghidra projects, disassembly dumps, or proprietary binaries in Git.

## Installed tools

| Tool | Version | Location |
| --- | --- | --- |
| Ghidra | 12.1.3 | `C:\Users\kiber\AppData\Local\Programs\Ghidra\ghidra_12.1.3_PUBLIC` |
| Eclipse Temurin JDK | 21.0.12.1 | `C:\Users\kiber\AppData\Local\Programs\Java\jdk-21.0.12.1+1` |

The Ghidra archive was downloaded from the official NSA release and verified against the checksum published with that release before extraction. Its size is 569,445,154 bytes. Use the [official Ghidra releases page](https://github.com/NationalSecurityAgency/ghidra/releases) if the installation must be recreated; Ghidra 12.1.x requires a supported JDK 21 installation.

## Check before installing

A new Codex process must run this read-only preflight before attempting any download or installation:

```powershell
$knownGhidraHome = 'C:\Users\kiber\AppData\Local\Programs\Ghidra\ghidra_12.1.3_PUBLIC'
$knownJavaHome = 'C:\Users\kiber\AppData\Local\Programs\Java\jdk-21.0.12.1+1'
$ghidraLauncher = Join-Path $knownGhidraHome 'support\analyzeHeadless.bat'
$javaLauncher = Join-Path $knownJavaHome 'bin\java.exe'

$ghidraReady = Test-Path -LiteralPath $ghidraLauncher -PathType Leaf
$javaReady = Test-Path -LiteralPath $javaLauncher -PathType Leaf
Write-Output "Ghidra installed: $ghidraReady"
Write-Output "JDK installed: $javaReady"
```

When both results are `True`, Ghidra is already installed: reuse these directories, set the process environment as shown below, and skip all download, extraction, package-manager, and environment-persistence steps. If only one result is `False`, install only the missing component. Do not reinstall merely because an isolated Codex process cannot see the user's persisted environment variables.

The user environment contains `GHIDRA_HOME`, `JAVA_HOME`, and corresponding `PATH` entries. A Codex process may run under an isolated Windows identity and therefore may not inherit those user variables. Set the process environment explicitly before invoking Ghidra:

```powershell
$ghidraHome = 'C:\Users\kiber\AppData\Local\Programs\Ghidra\ghidra_12.1.3_PUBLIC'
$javaHome = 'C:\Users\kiber\AppData\Local\Programs\Java\jdk-21.0.12.1+1'
$env:GHIDRA_HOME = $ghidraHome
$env:JAVA_HOME = $javaHome
$env:Path = "$javaHome\bin;$ghidraHome;$ghidraHome\support;$env:Path"
```

Verify both installations without creating a project:

```powershell
& "$env:JAVA_HOME\bin\java.exe" -version
& "$env:GHIDRA_HOME\support\analyzeHeadless.bat"
```

The second command prints headless-analyzer usage and exits with code 1 when no project arguments are supplied; that is the expected smoke-test result.

## Original executable

The owned reference executable is generated locally by the inspector at:

```text
C:\sources\reconqueror\analysis\original\artifacts\CONQUER.EXE
```

Source identity for BLD-GOG-EN:

- source installation: `C:\GOG Games\Conqueror AD1086`
- byte length: 919,107
- XXH3-128: `5106f53f8201761cb5112034f6c594d4`
- format: 32-bit Linear Executable embedded behind a DOS16M MZ stub
- embedded MZ module file offset: `0x26654`
- LE header offset within that module: `0x2AA8`
- LE header file offset for this build: `0x290FC`
- LE enumerated-data-page offset: module-relative `0x25C00`, hence raw file offset `0x4C254`

If the artifact is absent, regenerate it without modifying the original installation:

```powershell
$env:DOTNET_CLI_HOME = Join-Path $PWD '.dotnet-home'
$artifactPath = Join-Path $env:TEMP ('reconqueror-inspect-' + [guid]::NewGuid().ToString('N'))
dotnet run --project tools\Conqueror.Inspect --artifacts-path $artifactPath -- `
  'C:\GOG Games\Conqueror AD1086' 'analysis\original'
```

Never add `analysis/original/artifacts`, a Ghidra project, exported bytes, or full disassembly output to Git.

## Headless-project workflow

Always create analysis projects under a unique temporary directory:

```powershell
$projectRoot = Join-Path $env:TEMP ('reconqueror-ghidra-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $projectRoot | Out-Null
& "$env:GHIDRA_HOME\support\analyzeHeadless.bat" `
  $projectRoot ConquerorAnalysis `
  -import (Join-Path $PWD 'analysis/original/artifacts/CONQUER.EXE') `
  -overwrite
```

Ghidra's automatic importer sees the outer DOS MZ executable and does not automatically map the embedded LE objects correctly. Do not treat that default disassembly as authoritative. For reproducible address-level work, use the repository inspector's LE mapper and Iced decoder:

```powershell
$env:DOTNET_CLI_HOME = Join-Path $PWD '.dotnet-home'
$env:NUGET_PACKAGES = 'C:\Users\kiber\.nuget\packages'
$artifactPath = Join-Path $env:TEMP ('reconqueror-disassembly-' + [guid]::NewGuid().ToString('N'))
dotnet run --project tools\Conqueror.Inspect --artifacts-path $artifactPath -- `
  'C:\GOG Games\Conqueror AD1086' 'analysis\original' `
  '--disassemble=0x486B8,0x4819C,0x48658' '--executable-only'
```

This writes the ignored metadata-only `analysis/original/executable-disassembly-report.txt`. Addresses are LE virtual addresses, not raw file offsets. The inspector locates the nested MZ module that owns the LE header, applies module-relative LE page offsets, and then maps virtual addresses through the executable's object and page tables before decoding 32-bit x86 instructions. For this build, adding the page offset to the LE header would be wrong by `0x2AA8`; the enumerated pages begin at raw file offset `0x4C254`, not `0x4ECFC`.

For data references that are absent from raw instruction operands, `--xref-data=` also decodes the executable's fixup page and record tables according to the IBM LE/LX field definitions and Open Watcom's `exeflat.h` constants. Pass comma-separated target-object offsets; the ignored report includes matching internal relocations and a small source-address neighborhood without exporting original bytes:

```powershell
dotnet run --project tools\Conqueror.Inspect -- `
  'C:\GOG Games\Conqueror AD1086' 'analysis\original' `
  '--xref-data=0x17C0,0x17CC,0x1810' '--executable-only'
```

`--executable-only` returns after artifact extraction and requested executable string, disassembly, or relocation reports. Use it for iterative static analysis so unrelated GOB, scene-container, and movie population scans are not repeated.

`--scene-blocks` writes the ignored `scene-block-report.txt`. Its actor rows distinguish the visual-state effect selector at block `+0x2E` from the movement selector at `+0x46`; expose initial signed 8.8 block offsets `+0x24/+0x28`; and include the selected movement descriptor's tick count, interval, flags, coordinate/block/surface/heading deltas, loop block, and terminal surface. Use this census to corroborate handler traces against every owned scene variant; never commit the generated report.

For relocated tables, distinguish an object-relative offset from a runtime virtual address. In this LE image object 1 loads at `0x10000`, and its initialized bytes begin at raw data-page offset `0x4C254`. Decode each relocated dword as an object-1 target, then add the load base only when comparing with disassembly VAs, and use `--fixup-source` on the instructions that reference a table to confirm where it starts. This avoids treating a file offset, object-relative offset, and loaded VA as interchangeable.

For bounded conversation entry-point analysis, pass decimal or hexadecimal node identifiers with `--conversation-nodes=1100,0x44C`. The ignored `conversation-node-report.txt` records only structural metadata plus portrait and speaker identifiers; it does not copy prompts or response text into the repository.

For numeric action-tree inspection, `--action-groups=1101,2011` writes an ignored expression/branch trace without dialogue prose, while `--resource-integers=all.vtb` emits a bounded dword view. Pass `all` to `--action-groups` or `--conversation-nodes` to emit the complete metadata-only population rather than a comma-separated selection. For evidence correlation against the owner-local dialogue, `--conversation-text=1101,1152` writes only the explicitly selected decoded text to an ignored, redistribution-prohibited report; never commit that output. `--fixup-source=0x6ABBC` reports LE relocations near a source address and is useful for resolving switch tables whose unrelocated operands are misleading.

## Evidence discipline

- Hash the executable before analysis and compare it with the reference hash above.
- Record virtual addresses and independently described behavior, not copied machine-code bytes.
- Confirm recovered algorithms against original resource populations and synthetic malformed inputs.
- Record each observation as a finding in `spec/findings/`, and the formats, rules and screens it supports in `spec/`, with the status the evidence allows and the uncertain part in Open questions.
- Put status changes of the rebuild in `implementation-plan.md` and the parity rows in `parity/`.

## Template infrastructure migration

Existing procedure and local installation facts remain in [original-analysis.md](original-analysis.md).
Build identity is BLD-GOG-EN; latest official patch provenance is established by SRC-PATCH-CATALOG, as [SOURCE-EDITIONS.md](SOURCE-EDITIONS.md) records. Before executable analysis,
run `tools/Verify-Configuration.ps1 -RequireAnalysisReady`.

Template helpers under `tools/ghidra/` guard broad exports to local-only output.
Do not commit decompiler output, disassembly or analysis databases. The bounded
evidence reporter has its own pinned license and synthetic regression suite;
[BOUNDED-EVIDENCE-REPORTERS.md](BOUNDED-EVIDENCE-REPORTERS.md) describes its
supported image models. Its MZ/raw-image support does not establish a loader
for this game's protected-mode LE image. The documentation audit independently verified the source fingerprint and mapped LE object extents (FND-RES-009); it did not change gameplay evidence statuses.

## Verified LE inventory mapping

The ordinary executable import can analyze only the outer MZ loader. Do not use
that project's function list as protected-mode coverage. Import the identified
executable into a disposable BinaryLoader project with `x86:LE:32:default` and
run `MapConquerorLe.java` before auto-analysis. Its fingerprint guard and mapping
follow BLD-GOG-EN and FND-RES-009. The script replaces blocks in that disposable
project; never run it over an existing research project. Its required seed file may be empty, or contain documented entry addresses
within the code object, one hexadecimal number per line without `0x` or `fn_`.
Run `DescribeInventorySource.java` and `ExportFunctionInventory.java` afterward,
with exports in an ignored local analysis directory. The provenance and limits
of the committed metadata are in [coverage/README.md](../coverage/README.md).
Raw fixup operands remain unrelocated, so indirect targets remain uncertain.

For large memory maps use `ReportMemoryBlocks.java` with an explicit offset and
limit, or an exact block name, instead of exporting a whole map. Reports state
their selected scope and cap output. Before headless analysis, set the child
JVM's error, replay and heap-dump destinations to the ignored analysis directory:

```powershell
$diagnosticRoot = Join-Path $PWD 'analysis/ghidra-diagnostics'
New-Item -ItemType Directory -Force -Path $diagnosticRoot | Out-Null
$env:JAVA_TOOL_OPTIONS = '-XX:ErrorFile="' + $diagnosticRoot + '/hs_err_pid%p.log" ' +
    '-XX:ReplayDataFile="' + $diagnosticRoot + '/replay_pid%p.log" ' +
    '-XX:HeapDumpPath="' + $diagnosticRoot + '"'
```

Restore the prior `JAVA_TOOL_OPTIONS` after the task. JVM diagnostics may contain
original memory and local environment data; ignore and repository-policy checks
keep them local even if force-staged.

## Shared script package

Install the locked engine with
`python -m pip install --require-hashes -r requirements-evidence.txt`.
`python -m scientific_method_engine ghidra-scripts` prints the directory of
shared headless scripts, including function inventory and memory-block reports.
Pass that directory and the local `tools/ghidra` directory to Ghidra's
`-scriptPath`, separated by `;`. The local directory retains the LE mapper,
source description, jump-table helper and guarded edition/version-tracking
exports. Common helper copies have been removed; their packaged counterparts
provide the same inventory safety and bounded memory-map selection.
