# Ghidra setup for Codex analysis

This machine has a portable, user-wide Ghidra installation for clean-room analysis of the legally owned *Conqueror: A.D. 1086* executable. It does not require administrator access and must not place Ghidra projects, disassembly dumps, or proprietary binaries in Git.

## Installed tools

| Tool | Version | Location |
| --- | --- | --- |
| Ghidra | 12.1.3 | `C:\Users\kiber\AppData\Local\Programs\Ghidra\ghidra_12.1.3_PUBLIC` |
| Eclipse Temurin JDK | 21.0.12.1 | `C:\Users\kiber\AppData\Local\Programs\Java\jdk-21.0.12.1+1` |

The Ghidra archive was downloaded from the official NSA release and verified before extraction. Its SHA-256 is `93A5D11A9AD510622ACAAF908C556A7B9B764D338E78A7567F3689BF5081FD54` and its size is 569,445,154 bytes. Use the [official Ghidra releases page](https://github.com/NationalSecurityAgency/ghidra/releases) if the installation must be recreated; Ghidra 12.1.x requires a supported JDK 21 installation.

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
C:\GOG Games\reimp\analysis\original\artifacts\CONQUER.EXE
```

Reference facts:

- source installation: `C:\GOG Games\Conqueror AD1086`
- byte length: 919,107
- SHA-256: `5D7231758766204AD061E6B82CF2F0E0CBE28899B35D095F13E4AAD75C8B79D6`
- format: 32-bit Linear Executable embedded behind a DOS16M MZ stub
- embedded MZ module file offset: `0x26654`
- LE header offset within that module: `0x2AA8`
- LE header file offset for this build: `0x290FC`
- LE enumerated-data-page offset: module-relative `0x25C00`, hence raw file offset `0x4C254`

If the artifact is absent, regenerate it without modifying the original installation:

```powershell
$env:DOTNET_CLI_HOME = 'C:\GOG Games\reimp\.dotnet-home'
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
  -import 'C:\GOG Games\reimp\analysis\original\artifacts\CONQUER.EXE' `
  -overwrite
```

Ghidra's automatic importer sees the outer DOS MZ executable and does not automatically map the embedded LE objects correctly. Do not treat that default disassembly as authoritative. For reproducible address-level work, use the repository inspector's LE mapper and Iced decoder:

```powershell
$env:DOTNET_CLI_HOME = 'C:\GOG Games\reimp\.dotnet-home'
$env:NUGET_PACKAGES = 'C:\Users\kiber\.nuget\packages'
$artifactPath = Join-Path $env:TEMP ('reconqueror-disassembly-' + [guid]::NewGuid().ToString('N'))
dotnet run --project tools\Conqueror.Inspect --artifacts-path $artifactPath -- `
  'C:\GOG Games\Conqueror AD1086' 'analysis\original' `
  '--disassemble=0x45C6E,0x456F4,0x4576C' '--executable-only'
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

For bounded conversation entry-point analysis, pass decimal or hexadecimal node identifiers with `--conversation-nodes=1100,0x44C`. The ignored `conversation-node-report.txt` records only structural metadata plus portrait and speaker identifiers; it does not copy prompts or response text into the repository.

For numeric action-tree inspection, `--action-groups=1101,2011` writes an ignored expression/branch trace without dialogue prose, while `--resource-integers=all.vtb` emits a bounded dword view. Pass `all` to `--action-groups` or `--conversation-nodes` to emit the complete metadata-only population rather than a comma-separated selection. For evidence correlation against the owner-local dialogue, `--conversation-text=1101,1152` writes only the explicitly selected decoded text to an ignored, redistribution-prohibited report; never commit that output. `--fixup-source=0x6ABBC` reports LE relocations near a source address and is useful for resolving switch tables whose unrelocated operands are misleading.

## Evidence discipline

- Hash the executable before analysis and compare it with the reference hash above.
- Record virtual addresses and independently described behavior, not copied machine-code bytes.
- Confirm recovered algorithms against original resource populations and synthetic malformed inputs.
- Keep uncertain interpretations marked Provisional until static evidence or controlled observation resolves them.
- Put stable format facts in `resource-formats.md`, gameplay findings in `original-findings.md`, and status changes in `implementation-plan.md`.
