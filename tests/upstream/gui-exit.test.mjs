import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, writeFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { resolve, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';

test('packaging waits for synthetic GUI processes and rejects a real nonzero exit',
  { skip: process.platform !== 'win32' }, t => {
    const scratch = mkdtempSync(join(tmpdir(), 'conqueror-gui-exit-'));
    t.after(() => rmSync(scratch, { recursive: true, force: true }));
    const root = fileURLToPath(new URL('../../', import.meta.url));
    const script = join(scratch, 'check.ps1');
    writeFileSync(script, `param([string] $PublishScript, [string] $Scratch)
$ErrorActionPreference = 'Stop'
$tokens = $null; $errors = $null
$ast = [Management.Automation.Language.Parser]::ParseFile($PublishScript, [ref] $tokens, [ref] $errors)
$helper = $ast.Find({ param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Invoke-PackagedGame' }, $true)
if (-not $helper -or $errors.Count) { throw 'Packaging helper did not parse.' }
. ([scriptblock]::Create($helper.Extent.Text))
foreach ($exit in @(0, 7)) {
    $exe = Join-Path $Scratch "exit-$exit.exe"
    Add-Type -TypeDefinition "public static class Exit$exit { public static int Main(string[] args) { return $exit; } }" -OutputAssembly $exe -OutputType WindowsApplication
    $rejected = $false
    try { Invoke-PackagedGame $exe @('--synthetic') 'Synthetic GUI failure.' }
    catch {
        if ($exit -eq 0 -or $_.Exception.Message -notmatch 'exited with 7') { throw }
        $rejected = $true
    }
    if ($exit -eq 7 -and -not $rejected) { throw 'Nonzero GUI exit was incorrectly accepted.' }
}
`);
    const result = spawnSync('powershell.exe', ['-NoProfile', '-ExecutionPolicy', 'Bypass',
      '-File', script, '-PublishScript', resolve(root, 'tools/Publish-Windows.ps1'), '-Scratch', scratch],
    { encoding: 'utf8', timeout: 30000 });
    assert.equal(result.status, 0, result.error?.message ?? result.stdout + result.stderr);
  });
