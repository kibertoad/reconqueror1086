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
    $resolvedPackage = (Resolve-Path -LiteralPath $packageRoot).Path
    if (-not $resolvedPackage.StartsWith($artifactsPrefix, [StringComparison]::OrdinalIgnoreCase)) { throw 'Package path escaped artifacts.' }
    Remove-Item -LiteralPath $resolvedPackage -Recurse -Force
}
$gameOutput = Join-Path $packageRoot 'Game'
$toolOutput = Join-Path $packageRoot 'Tools'
$buildRoot = Join-Path $packageRoot '.build'
New-Item -ItemType Directory -Path $gameOutput, $toolOutput -Force | Out-Null

$common = @(
    '--configuration', 'Release',
    '--runtime', 'win-x64',
    '--self-contained', 'true',
    '-p:DebugType=None',
    '-p:DebugSymbols=false',
    '-p:UseSharedCompilation=false',
    '-m:1',
    '--artifacts-path', $buildRoot,
    '--verbosity', 'minimal'
)
& dotnet publish (Join-Path $repositoryRoot 'src/Conqueror.Game/Conqueror.Game.csproj') @common --output $gameOutput
if ($LASTEXITCODE -ne 0) { throw 'Game publish failed.' }
& dotnet publish (Join-Path $repositoryRoot 'tools/Conqueror.Import/Conqueror.Import.csproj') @common `
    '-p:PublishSingleFile=true' --output $toolOutput
if ($LASTEXITCODE -ne 0) { throw 'Importer publish failed.' }
Remove-Item -LiteralPath $buildRoot -Recurse -Force

Copy-Item -LiteralPath (Join-Path $repositoryRoot 'packaging/windows/Start Conqueror 1086.bat') -Destination $packageRoot
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'packaging/windows/Install Original Resources.bat') -Destination $packageRoot
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'packaging/windows/Manage Original Resources.bat') -Destination $packageRoot
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'README.md') -Destination $packageRoot
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'NOTICE') -Destination $packageRoot
New-Item -ItemType Directory -Force -Path (Join-Path $packageRoot 'docs/licenses') | Out-Null
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'docs/licenses/scientific-method-MIT.txt') `
    -Destination (Join-Path $packageRoot 'docs/licenses/scientific-method-MIT.txt')

New-Item -ItemType Directory -Force -Path (Join-Path $packageRoot 'vendor/template') | Out-Null
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'vendor/template/LICENSE') `
    -Destination (Join-Path $packageRoot 'vendor/template/LICENSE')

if (Test-Path -LiteralPath (Join-Path $packageRoot 'UserContent')) { throw 'The package contains original content.' }

$softwareDrivers = @('opengl32.dll', 'libgallium_wgl.dll', 'libglapi.dll', 'osmesa.dll')
$leaked = @(Get-ChildItem -LiteralPath $packageRoot -Recurse -File |
    Where-Object { $softwareDrivers -contains $_.Name })
if ($leaked.Count) {
    throw ("The package contains an OpenGL driver, which would override the player's own: " +
        (($leaked | ForEach-Object { $_.FullName }) -join ', '))
}


function Invoke-PackagedGame([string] $executable, [string[]] $gameArguments, [string] $failure) {
    $stdout = [IO.Path]::GetTempFileName()
    $stderr = [IO.Path]::GetTempFileName()
    try {
        $process = Start-Process -FilePath $executable -ArgumentList $gameArguments -PassThru `
            -WindowStyle Hidden -RedirectStandardOutput $stdout -RedirectStandardError $stderr
        # Windows PowerShell otherwise may lose the exit code of a short-lived
        # GUI process. Retain its handle before waiting for it.
        $null = $process.Handle
        $exited = $process.WaitForExit(120000)
        if (-not $exited) {
            $process.Kill($true)
            throw "$failure The packaged game did not exit within 120 seconds."
        }
        # Always surface stderr: it is empty on an ordinary run and carries the software-renderer
        # banner otherwise, which is the only record of which renderer a passing run exercised.
        if ((Test-Path -LiteralPath $stderr) -and (Get-Item -LiteralPath $stderr).Length -gt 0) {
            Get-Content -LiteralPath $stderr | Write-Host
        }
        if ($process.ExitCode -ne 0) {
            if ((Test-Path -LiteralPath $stdout) -and (Get-Item -LiteralPath $stdout).Length -gt 0) {
                Get-Content -LiteralPath $stdout | Write-Host
            }
            throw "$failure It exited with $($process.ExitCode)."
        }
    }
    finally {
        Remove-Item -LiteralPath $stdout, $stderr -Force -ErrorAction SilentlyContinue
    }
}


$gameExecutable = Join-Path $gameOutput 'Conqueror.Game.exe'
Invoke-PackagedGame $gameExecutable @('--smoke-test') 'Packaged startup check failed.'
$platformArguments = @('--platform-smoke-test')
if ($env:PLATFORM_SMOKE_TEST_GL_DRIVER) { $platformArguments += '--software-renderer' }
Invoke-PackagedGame $gameExecutable $platformArguments 'Packaged native-platform check failed.'
foreach ($nativeLibrary in @('SDL2.dll', 'openal.dll')) {
    if (-not (Test-Path -LiteralPath (Join-Path $gameOutput $nativeLibrary))) {
        throw "Packaged game is missing native library '$nativeLibrary'."
    }
}
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
