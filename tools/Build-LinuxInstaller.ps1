[CmdletBinding()]
param(
    [string] $Version = '0.1.0'
)

$ErrorActionPreference = 'Stop'
if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    throw "Invalid installer version '$Version'; expected x.y.z."
}

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$artifactsRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts'))
$portableRoot = Join-Path $artifactsRoot 'ReConqueror1086-linux-x64'
& (Join-Path $PSScriptRoot 'Publish-Portable.ps1') -Runtime linux-x64 `
    -OutputDirectory $portableRoot
if ($LASTEXITCODE -ne 0) { throw 'Linux package creation failed.' }

$stagingRoot = Join-Path $artifactsRoot ".linux-deb-$Version"
$resolvedStaging = [IO.Path]::GetFullPath($stagingRoot)
$artifactsPrefix = $artifactsRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) +
    [IO.Path]::DirectorySeparatorChar
if (-not $resolvedStaging.StartsWith($artifactsPrefix, [StringComparison]::Ordinal)) {
    throw "Staging output must remain below '$artifactsRoot'."
}
if (Test-Path -LiteralPath $resolvedStaging) {
    Remove-Item -LiteralPath $resolvedStaging -Recurse -Force
}

$installRoot = Join-Path $resolvedStaging 'opt/reconqueror-ad-1086'
$debianRoot = Join-Path $resolvedStaging 'DEBIAN'
$binaryRoot = Join-Path $resolvedStaging 'usr/bin'
$desktopRoot = Join-Path $resolvedStaging 'usr/share/applications'
New-Item -ItemType Directory -Path $installRoot, $debianRoot, $binaryRoot, $desktopRoot `
    -Force | Out-Null
Get-ChildItem -LiteralPath $portableRoot | Copy-Item -Destination $installRoot -Recurse

@"
Package: reconqueror-ad-1086
Version: $Version
Section: games
Priority: optional
Architecture: amd64
Maintainer: ReConqueror contributors
Depends: libc6, libgl1, libx11-6, libopenal1
Description: Clean-room recreation of Conqueror A.D. 1086
 Requires resources imported from a legally owned original copy for original media.
"@ | Set-Content -LiteralPath (Join-Path $debianRoot 'control') -Encoding utf8NoBOM

@'
#!/bin/sh
exec /opt/reconqueror-ad-1086/Game/Conqueror.Game "$@"
'@ | Set-Content -LiteralPath (Join-Path $binaryRoot 'reconqueror-ad-1086') `
    -Encoding utf8NoBOM

@'
#!/bin/sh
set -eu
if [ "$#" -ne 1 ]; then
  echo "Usage: reconqueror-ad-1086-import /path/to/Conqueror AD1086" >&2
  exit 2
fi
data_root="${XDG_DATA_HOME:-$HOME/.local/share}/ReConquerorAD1086/UserContent"
exec /opt/reconqueror-ad-1086/Tools/Conqueror.Import "$1" "$data_root"
'@ | Set-Content -LiteralPath (Join-Path $binaryRoot 'reconqueror-ad-1086-import') `
    -Encoding utf8NoBOM

@'
[Desktop Entry]
Type=Application
Name=ReConqueror A.D. 1086
Comment=Clean-room recreation of Conqueror A.D. 1086
Exec=reconqueror-ad-1086
Terminal=false
Categories=Game;StrategyGame;
'@ | Set-Content -LiteralPath (Join-Path $desktopRoot 'reconqueror-ad-1086.desktop') `
    -Encoding utf8NoBOM

& chmod 755 (Join-Path $binaryRoot 'reconqueror-ad-1086') `
    (Join-Path $binaryRoot 'reconqueror-ad-1086-import')
if ($LASTEXITCODE -ne 0) { throw 'Could not mark Linux launchers as executable.' }

$installer = Join-Path $artifactsRoot "ReConqueror1086-linux-x64-Setup-$Version.deb"
if (Test-Path -LiteralPath $installer) { Remove-Item -LiteralPath $installer -Force }
& dpkg-deb --build --root-owner-group $resolvedStaging $installer
if ($LASTEXITCODE -ne 0 -or -not [IO.File]::Exists($installer)) {
    throw 'Debian installer creation failed.'
}
Remove-Item -LiteralPath $resolvedStaging -Recurse -Force
Write-Host "Linux installer created at $installer"
