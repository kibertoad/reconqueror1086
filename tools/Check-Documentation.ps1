[CmdletBinding()]
param(
    [string] $RepositoryRoot,
    # Rewrite spec/index/ and PARITY.md instead of failing when they are stale.
    [switch] $Write,
    # Write VALIDATION.md for the marked tests of the validated rows, naming the builds the local
    # run used. Only after every test in those files passed against the original's files.
    [string[]] $RecordValidation
)

$ErrorActionPreference = 'Stop'

if (-not $RepositoryRoot) {
    $RepositoryRoot = Split-Path -Parent $PSScriptRoot
}

$root = (Resolve-Path -LiteralPath $RepositoryRoot).Path

# The local run uses the toolkit commit the CI workflow pins, so both apply the same checks.
$workflow = Get-Content -LiteralPath (Join-Path $root '.github/workflows/ci.yml') -Raw
$pin = [regex]::Match($workflow, '(?m)^\s*(?:-\s+)?uses:\s*kibertoad/refurbished-dinosaurs-toolkit/actions/check-documentation@([0-9a-f]{40})\b')
if (-not $pin.Success) {
    throw 'The CI workflow does not pin the documentation standard check to a full commit SHA.'
}
$sha = $pin.Groups[1].Value

$node = Get-Command node -ErrorAction SilentlyContinue
if (-not $node) {
    throw 'The documentation standard check needs Node.js 20 or newer on PATH.'
}

$script = Join-Path $root "artifacts/check-documentation-$sha.mjs"
if (-not (Test-Path -LiteralPath $script)) {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $script) | Out-Null
    $url = "https://raw.githubusercontent.com/kibertoad/refurbished-dinosaurs-toolkit/$sha/tools/check-documentation.mjs"
    $partial = "$script.partial"
    Invoke-WebRequest -Uri $url -OutFile $partial -UseBasicParsing
    Move-Item -LiteralPath $partial -Destination $script -Force
}

$arguments = @($script, '--root', $root, '--references', 'docs')
if ($RecordValidation) {
    $arguments += @('--record-validation', ($RecordValidation -join ','))
}
elseif (-not $Write) {
    $arguments += '--check'
}

& $node.Source @arguments
if ($LASTEXITCODE -ne 0) {
    throw "The documentation standard check failed with exit code $LASTEXITCODE."
}
