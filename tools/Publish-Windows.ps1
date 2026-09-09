[CmdletBinding()]
param(
    [string] $OutputDirectory,
    [switch] $SkipArchive
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$artifactsRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts'))
if (-not $OutputDirectory) {
    $OutputDirectory = Join-Path $artifactsRoot 'Conqueror1086-win-x64'
}
$packageRoot = [IO.Path]::GetFullPath($OutputDirectory)
$artifactsPrefix = $artifactsRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
if (-not $packageRoot.StartsWith($artifactsPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Package output must remain below '$artifactsRoot'."
}

& (Join-Path $PSScriptRoot 'Verify-Repository.ps1') -RepositoryRoot $repositoryRoot
if ($LASTEXITCODE -ne 0) { throw 'Repository policy verification failed.' }

if (Test-Path -LiteralPath $packageRoot) {
    Remove-Item -LiteralPath $packageRoot -Recurse -Force
}
$gameOutput = Join-Path $packageRoot 'Game'
$toolOutput = Join-Path $packageRoot 'Tools'
$buildRoot = Join-Path $packageRoot '.build'
New-Item -ItemType Directory -Path $gameOutput, $toolOutput -Force | Out-Null

$common = @(
    '--configuration', 'Release',
    '--runtime', 'win-x64',
    '--self-contained', 'true',
    '-p:PublishSingleFile=true',
    '-p:IncludeNativeLibrariesForSelfExtract=true',
    '-p:DebugType=None',
    '-p:DebugSymbols=false',
    '-p:UseSharedCompilation=false',
    '-m:1',
    '--artifacts-path', $buildRoot,
    '--verbosity', 'minimal'
)
& dotnet publish (Join-Path $repositoryRoot 'src/Conqueror.Game/Conqueror.Game.csproj') @common --output $gameOutput
if ($LASTEXITCODE -ne 0) { throw 'Game publish failed.' }
& dotnet publish (Join-Path $repositoryRoot 'tools/Conqueror.Import/Conqueror.Import.csproj') @common --output $toolOutput
if ($LASTEXITCODE -ne 0) { throw 'Importer publish failed.' }
Remove-Item -LiteralPath $buildRoot -Recurse -Force

Copy-Item -LiteralPath (Join-Path $repositoryRoot 'packaging/windows/Start Conqueror 1086.bat') -Destination $packageRoot
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'packaging/windows/Install Original Resources.bat') -Destination $packageRoot
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'packaging/windows/Manage Original Resources.bat') -Destination $packageRoot
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'README.md') -Destination $packageRoot

& (Join-Path $gameOutput 'Conqueror.Game.exe') --smoke-test
if ($LASTEXITCODE -ne 0) { throw 'Packaged game smoke check failed.' }
& (Join-Path $toolOutput 'Conqueror.Import.exe') --verify (Join-Path $packageRoot 'UserContent')
if ($LASTEXITCODE -ne 2) { throw 'Packaged importer smoke check returned an unexpected result.' }

if (-not $SkipArchive) {
    $archivePath = "$packageRoot.zip"
    if (Test-Path -LiteralPath $archivePath) { Remove-Item -LiteralPath $archivePath -Force }
    Compress-Archive -LiteralPath $packageRoot -DestinationPath $archivePath -CompressionLevel Optimal
    Write-Host "Created $archivePath"
}
Write-Host "Self-contained Windows package verified at $packageRoot"
exit 0
