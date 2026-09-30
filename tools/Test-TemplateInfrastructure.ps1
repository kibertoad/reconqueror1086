[CmdletBinding()]
param([string] $RepositoryRoot)
$ErrorActionPreference = 'Stop'
if (-not $RepositoryRoot) { $RepositoryRoot = Split-Path -Parent $PSScriptRoot }
$root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$config = Get-Content (Join-Path $root 'tools/project-config.json') -Raw | ConvertFrom-Json
$failures = [Collections.Generic.List[string]]::new()
foreach ($path in @('tools/Bootstrap-Project.ps1', 'tools/Invoke-Validation.ps1', 'tools/Install-CodeSignTool.ps1',
    'tools/Invoke-ESigner.ps1', 'tools/Invoke-GpgSigner.ps1', $config.extractorProject, $config.solution,
    'spec/README.md', 'spec/LICENSE', 'PARITY.md', 'docs/RUNTIME.md', 'docs/DECISIONS.md',
    'docs/handover.md', 'queue/README.md', 'docs/goals/README.md', 'docs/live-sessions/README.md', 'docs/reports/README.md')) {
    if (-not (Test-Path -LiteralPath (Join-Path $root $path) -PathType Leaf)) { $failures.Add("Missing infrastructure: $path") }
}
foreach ($skill in @('runtime-access', 'start-session', 'research-item', 'implement-rows', 'triage-report', 'live-session', 'end-session', 'plan-work')) {
    if (-not (Test-Path (Join-Path $root ".claude/skills/$skill/SKILL.md"))) { $failures.Add("Missing skill: $skill") }
}
foreach ($name in @('ExportEditionAnalysis.java', 'ExportFunctionAddressCorrelations.java',
    'ExportVersionTrackingAddressContexts.java', 'ExportVersionTrackingMatches.java')) {
    $path = Join-Path $root "tools/ghidra/$name"
    if (-not (Test-Path $path) -or (Get-Content $path -Raw) -notmatch 'requireLocalOutput') { $failures.Add("Missing export guard: $name") }
}
foreach ($directory in @('spec', 'parity', 'deviations', 'queue', 'docs/goals', 'docs/reports', 'docs/live-sessions')) {
    foreach ($file in Get-ChildItem (Join-Path $root $directory) -Recurse -File -Filter '*.md') {
        if ([IO.File]::ReadAllLines($file.FullName).Length -gt 1000) { $failures.Add("Documentation exceeds 1,000 lines: $($file.FullName)") }
    }
}
if ([IO.File]::ReadAllLines((Join-Path $root 'docs/handover.md')).Length -gt 200) { $failures.Add('Handover exceeds 200 lines.') }
$installer = Get-Content (Join-Path $root 'packaging/windows/Conqueror1086.iss') -Raw
if (-not $installer.Contains($config.appId.TrimStart('{'))) { $failures.Add('Installer GUID differs from configured identity.') }
$release = Get-Content (Join-Path $root '.github/workflows/release.yml') -Raw
foreach ($required in @('signed_release:', 'release-signing', 'Invoke-ESigner.ps1', 'Invoke-GpgSigner.ps1', 'Get-AuthenticodeSignature')) {
    if (-not $release.Contains($required)) { $failures.Add("Release signing infrastructure missing: $required") }
}
if ($failures.Count) { throw ($failures -join "`n") }
Write-Host 'Template infrastructure and preserved game identity verified.'
