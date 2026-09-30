[CmdletBinding()]
param([string] $RepositoryRoot, [switch] $Write, [string[]] $RecordValidation)
$ErrorActionPreference = 'Stop'
if (-not $RepositoryRoot) { $RepositoryRoot = Split-Path -Parent $PSScriptRoot }
$root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$arguments = @((Join-Path $root 'tools/upstream.mjs'), 'docs')
if ($RecordValidation) { $arguments += @('--record-validation', ($RecordValidation -join ',')) }
elseif (-not $Write) { $arguments += '--check' }
Push-Location $root
try {
    & node @arguments
    if ($LASTEXITCODE -ne 0) { throw "Documentation check failed with exit code $LASTEXITCODE." }
    & node (Join-Path $root 'tools/Check-NarrativeReferences.mjs')
    if ($LASTEXITCODE -ne 0) { throw 'Narrative documentation cites missing or superseded entries.' }
    & node (Join-Path $root 'tools/Check-ResearchTracking.mjs')
    if ($LASTEXITCODE -ne 0) { throw 'Research tracking is inconsistent.' }
    & node (Join-Path $root 'tools/Check-Coverage.mjs')
    if ($LASTEXITCODE -ne 0) { throw 'Committed function inventory is inconsistent.' }
}
finally { Pop-Location }
