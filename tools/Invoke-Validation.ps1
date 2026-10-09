[CmdletBinding()]
param(
    [ValidateRange(1, 16)][int] $MaxCpuCount = 2,
    [string] $TestFilter,
    [switch] $IncludeLongRunningTests,
    [switch] $LongRunningTestsOnly,
    [ValidateRange(1, 100000)][int] $MinimumExpectedTests = 98,
    [switch] $TraceTestOutput,
    [string] $ArtifactsPath,
    [switch] $NoRestore
)
$ErrorActionPreference = 'Stop'
if (($TestFilter -and ($IncludeLongRunningTests -or $LongRunningTestsOnly)) -or
    ($IncludeLongRunningTests -and $LongRunningTestsOnly)) {
    throw 'Choose only one test selection option.'
}
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$hash = [Security.Cryptography.SHA256]::Create()
try { $identity = [BitConverter]::ToString($hash.ComputeHash([Text.Encoding]::UTF8.GetBytes($root.ToUpperInvariant()))).Replace('-', '') }
finally { $hash.Dispose() }
$lockPath = Join-Path ([IO.Path]::GetTempPath()) "restoration-validation-$($identity.Substring(0, 16)).lock"
$lock = $null
if (-not $ArtifactsPath) { $ArtifactsPath = Join-Path $root 'artifacts/validation' }
$artifactRoot = [IO.Path]::GetFullPath((Join-Path $root 'artifacts'))
$ArtifactsPath = [IO.Path]::GetFullPath($ArtifactsPath)
if (-not $ArtifactsPath.StartsWith($artifactRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Validation artifacts must stay below the repository artifacts directory.'
}
function Invoke-Dotnet([string[]] $Arguments) {
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($Arguments[0]) failed ($LASTEXITCODE)." }
}
function Invoke-Node([string[]] $Arguments) {
    & node @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Node validation failed ($LASTEXITCODE)." }
}
Push-Location $root
try {
    try { $lock = [IO.File]::Open($lockPath, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None) }
    catch [IO.IOException] { throw "Another validation run is active for this checkout ($lockPath)." }
    # No game or unrelated build service is stopped as a side effect of a test run.
    & (Join-Path $PSScriptRoot 'Verify-Repository.ps1') -RepositoryRoot $root
    if ($LASTEXITCODE -ne 0) { throw 'Repository verification failed.' }
    & (Join-Path $PSScriptRoot 'Verify-Configuration.ps1') -RepositoryRoot $root
    if ($LASTEXITCODE -ne 0) { throw 'Configuration verification failed.' }
    & (Join-Path $PSScriptRoot 'Test-TemplateInfrastructure.ps1') -RepositoryRoot $root
    if ($LASTEXITCODE -ne 0) { throw 'Infrastructure verification failed.' }
    & (Join-Path $PSScriptRoot 'Check-Documentation.ps1') -RepositoryRoot $root
    Invoke-Node @('tools/Invoke-NodeChecks.mjs')
    Invoke-Node @('tools/upstream.mjs', 'links')
    Invoke-Node @('--test', 'tests/evidence/evidence.test.mjs', 'tests/evidence/packages.test.mjs', 'tests/evidence/xxh3.test.mjs',
        'tests/evidence/unlzexe.test.mjs',
        'tests/evidence/coverage-snapshot.test.mjs', 'tests/evidence/inventory-commands.test.mjs',
        'tests/evidence/coverage-progress.test.mjs',
        'tests/upstream/upstream.test.mjs', 'tests/upstream/narrative.test.mjs',
        'tests/upstream/gui-exit.test.mjs', 'tests/upstream/research-tracking.test.mjs',
        'tests/upstream/diagnostics.test.mjs', 'tests/upstream/memory-blocks.test.mjs',
        'tests/upstream/capture-window.test.mjs', 'tests/upstream/goal-run.test.mjs')
    # Give the command-double test the PowerShell running this gate.
    $previousPwsh = $env:PWSH
    if (-not $env:PWSH) { $env:PWSH = [Diagnostics.Process]::GetCurrentProcess().MainModule.FileName }
    try { Invoke-Node @('--test', 'tests/upstream/offline-validation.test.mjs', 'tests/upstream/release-signing.test.mjs') }
    finally { $env:PWSH = $previousPwsh }
    $python = if ($env:EVIDENCE_PYTHON) { $env:EVIDENCE_PYTHON } else { 'python' }
    & $python -B (Join-Path $root 'tools/Verify-EvidenceEnvironment.py')
    if ($LASTEXITCODE -ne 0) { throw 'Install the pinned engine with python -m pip install --require-hashes -r requirements-evidence.txt.' }
    $build = @('--artifacts-path', $ArtifactsPath, "-maxCpuCount:$MaxCpuCount", '-nodeReuse:true', '-p:UseSharedCompilation=false', '-v:minimal')
    if ($NoRestore) {
        Write-Host 'NoRestore: using existing restore state for this checkout; no restore fallback.'
        $build += '--no-restore'
    }
    Invoke-Dotnet (@('build', 'Conqueror1086.slnx') + $build)
    $test = @('test', '--project', 'tests/Conqueror.Tests/Conqueror.Tests.csproj', '--no-build', '--no-restore',
        '--artifacts-path', $ArtifactsPath, '--no-progress', '--minimum-expected-tests', "$MinimumExpectedTests", '-v:minimal')
    if ($TestFilter) { $test += @('--filter', $TestFilter) }
    elseif ($LongRunningTestsOnly) { $test += @('--filter', 'Category=LongRunning') }
    elseif (-not $IncludeLongRunningTests) { $test += @('--filter', 'Category!=LongRunning') }
    if ($TraceTestOutput) { $test += @('--output', 'Detailed', '--show-live-output', 'on', '--show-stdout', 'All') }
    Invoke-Dotnet $test
    $specs = Join-Path $ArtifactsPath 'bin/Conqueror.Specs/debug/Conqueror.Specs.dll'
    if (-not [IO.File]::Exists($specs)) { throw "Executable specifications not built: $specs" }
    Invoke-Dotnet @($specs)
    Write-Host "Validation passed. Build artifacts: $ArtifactsPath"
}
finally { if ($lock) { $lock.Dispose() }; Pop-Location }
