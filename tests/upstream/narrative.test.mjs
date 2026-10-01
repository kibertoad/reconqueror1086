import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, mkdirSync, writeFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, dirname } from 'node:path';
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
