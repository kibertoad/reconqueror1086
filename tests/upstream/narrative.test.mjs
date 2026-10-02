import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, mkdirSync, writeFileSync, rmSync, copyFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, dirname, resolve } from 'node:path';
import { spawnSync } from 'node:child_process';
import { checkNarrative } from '../../tools/Check-NarrativeReferences.mjs';

test('narrative claims are checked without treating pinned examples as game claims', t => {
  const root = mkdtempSync(join(tmpdir(), 'conqueror-narrative-'));
  t.after(() => rmSync(root, { recursive: true, force: true }));
  const put = (path, text) => {
    const full = join(root, path); mkdirSync(dirname(full), { recursive: true }); writeFileSync(full, text);
  };
  const active = ['RULE', 'DATA', '001'].join('-');
  const old = ['RULE', 'DATA', '002'].join('-');
  const missing = ['RULE', 'DATA', '003'].join('-');
  put('spec/rules/active.md', `---\nid: ${active}\nstatus: supported\nsuperseded_by: []\n---\n`);
  put('spec/rules/old.md', `---\nid: ${old}\nstatus: superseded\nsuperseded_by: [${active}]\n---\n`);
  put('docs/upstream/documentation-standard.md', missing);
  put('docs/live-sessions/README.md', missing);
  put('docs/SPEC-ENTRY-TEMPLATES.md', missing);
  put('docs/current.md', active);
  assert.deepEqual(checkNarrative(root), []);
  const deviation = ['DEV', 'DATA', '001'].join('-');
  put('deviations/example.md', `# ${deviation}\n\n- Dropped: no\n`);
  put('docs/current.md', `${active}\n${deviation}`);
  assert.deepEqual(checkNarrative(root), []);
  put('deviations/example.md', `# ${deviation}\n\n- Dropped: yes\n`);
  assert.match(checkNarrative(root)[0], /superseded/);
  put('docs/current.md', `${active}\n${old}\n${missing}\n`);
  const problems = checkNarrative(root);
  assert.equal(problems.length, 2);
  assert.match(problems[0], /current.md:2: cites superseded/);
  assert.match(problems[1], /current.md:3: cites missing/);
});

test('shared pre-commit checks reject a superseded handover citation', t => {
  const root = mkdtempSync(join(tmpdir(), 'conqueror-handover-gate-'));
  t.after(() => rmSync(root, { recursive: true, force: true }));
  const put = (path, text) => {
    const full = join(root, path); mkdirSync(dirname(full), { recursive: true }); writeFileSync(full, text);
  };
  const repository = resolve(import.meta.dirname, '../..');
  mkdirSync(join(root, 'tools'));
  for (const script of ['Invoke-NodeChecks.mjs', 'Check-NarrativeReferences.mjs'])
    copyFileSync(join(repository, 'tools', script), join(root, 'tools', script));
  // Isolate the active narrative check; other shared checks have their own coverage.
  for (const script of ['upstream.mjs', 'Check-ResearchTracking.mjs', 'Verify-ToolkitPackages.mjs'])
    put('tools/' + script, 'process.exit(0);');
  const active = ['FMT', 'DATA', '001'].join('-'), retired = ['FMT', 'DATA', '002'].join('-');
  put('spec/formats/active.md', `---\nid: ${active}\nstatus: supported\nsuperseded_by: []\n---\n`);
  put('spec/formats/retired.md', `---\nid: ${retired}\nstatus: superseded\nsuperseded_by: [${active}]\n---\n`);
  put('docs/goals/current.md', `Handover: ${retired}`);
  const run = () => spawnSync(process.execPath, [join(root, 'tools/Invoke-NodeChecks.mjs'), '--no-ksy'], {encoding: 'utf8'});
  const rejected = run();
  assert.equal(rejected.status, 1, rejected.error?.message || rejected.stdout + rejected.stderr);
  assert.match(rejected.stderr, /current.md:1: cites superseded/);
  assert.match(rejected.stderr, /Narrative references failed/);
  put('docs/goals/current.md', `Handover: ${active}`);
  const accepted = run();
  assert.equal(accepted.status, 0, accepted.error?.message || accepted.stdout + accepted.stderr);
});
