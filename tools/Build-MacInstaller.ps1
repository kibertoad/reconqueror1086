[CmdletBinding()]
param(
    [string] $Version = '0.1.0',
    [ValidateSet('osx-x64', 'osx-arm64')]
    [string] $Runtime = 'osx-arm64'
)

$ErrorActionPreference = 'Stop'
if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    throw "Invalid installer version '$Version'; expected x.y.z."
}

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$artifactsRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts'))
$portableRoot = Join-Path $artifactsRoot "ReConqueror1086-$Runtime"
& (Join-Path $PSScriptRoot 'Publish-Portable.ps1') -Runtime $Runtime `
    -OutputDirectory $portableRoot
if ($LASTEXITCODE -ne 0) { throw 'macOS package creation failed.' }

$stagingRoot = Join-Path $artifactsRoot ".macos-pkg-$Runtime-$Version"
$resolvedStaging = [IO.Path]::GetFullPath($stagingRoot)
$artifactsPrefix = $artifactsRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) +
    [IO.Path]::DirectorySeparatorChar
if (-not $resolvedStaging.StartsWith($artifactsPrefix, [StringComparison]::Ordinal)) {
    throw "Staging output must remain below '$artifactsRoot'."
}
if (Test-Path -LiteralPath $resolvedStaging) {
    Remove-Item -LiteralPath $resolvedStaging -Recurse -Force
}

$appRoot = Join-Path $resolvedStaging 'Applications/ReConqueror A.D. 1086.app'
$contentsRoot = Join-Path $appRoot 'Contents'
$macRoot = Join-Path $contentsRoot 'MacOS'
$resourcesRoot = Join-Path $contentsRoot 'Resources'
$toolsRoot = Join-Path $resourcesRoot 'Tools'
New-Item -ItemType Directory -Path $macRoot, $toolsRoot -Force | Out-Null
Get-ChildItem -LiteralPath (Join-Path $portableRoot 'Game') |
    Copy-Item -Destination $macRoot -Recurse
Get-ChildItem -LiteralPath (Join-Path $portableRoot 'Tools') |
    Copy-Item -Destination $toolsRoot -Recurse
Copy-Item -LiteralPath (Join-Path $portableRoot 'README.md') -Destination $resourcesRoot

@"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "https://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleDevelopmentRegion</key><string>en</string>
  <key>CFBundleDisplayName</key><string>ReConqueror A.D. 1086</string>
  <key>CFBundleExecutable</key><string>Conqueror.Game</string>
  <key>CFBundleIdentifier</key><string>io.github.kibertoad.reconqueror1086</string>
  <key>CFBundleInfoDictionaryVersion</key><string>6.0</string>
  <key>CFBundleName</key><string>ReConqueror A.D. 1086</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleShortVersionString</key><string>$Version</string>
  <key>CFBundleVersion</key><string>$Version</string>
  <key>LSMinimumSystemVersion</key><string>12.0</string>
  <key>NSHighResolutionCapable</key><true/>
</dict>
</plist>
"@ | Set-Content -LiteralPath (Join-Path $contentsRoot 'Info.plist') -Encoding utf8NoBOM

@'
#!/bin/sh
set -eu
if [ "$#" -ne 1 ]; then
  echo "Usage: Install Original Resources /path/to/Conqueror AD1086" >&2
  exit 2
fi
contents="$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"
output="$HOME/Library/Application Support/ReConquerorAD1086/UserContent"
exec "$contents/Resources/Tools/Conqueror.Import" "$1" "$output"
'@ | Set-Content -LiteralPath (Join-Path $macRoot 'Install Original Resources') `
    -Encoding utf8NoBOM

& chmod 755 (Join-Path $macRoot 'Conqueror.Game') `
    (Join-Path $toolsRoot 'Conqueror.Import') `
    (Join-Path $macRoot 'Install Original Resources')
if ($LASTEXITCODE -ne 0) { throw 'Could not mark macOS executables as executable.' }
& plutil -lint (Join-Path $contentsRoot 'Info.plist')
if ($LASTEXITCODE -ne 0) { throw 'Generated Info.plist is invalid.' }
& (Join-Path $macRoot 'Conqueror.Game') --smoke-test
if ($LASTEXITCODE -ne 0) { throw 'macOS app bundle smoke check failed.' }

$installer = Join-Path $artifactsRoot "ReConqueror1086-$Runtime-Setup-$Version.pkg"
if (Test-Path -LiteralPath $installer) { Remove-Item -LiteralPath $installer -Force }
& pkgbuild --root $resolvedStaging `
    --identifier 'io.github.kibertoad.reconqueror1086' `
    --version $Version `
    --install-location '/' `
    $installer
if ($LASTEXITCODE -ne 0 -or -not [IO.File]::Exists($installer)) {
    throw 'macOS installer creation failed.'
}
Remove-Item -LiteralPath $resolvedStaging -Recurse -Force
Write-Host "macOS installer created at $installer"
