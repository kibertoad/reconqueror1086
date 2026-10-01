import { existsSync, readdirSync, readFileSync } from 'node:fs';
import { resolve, dirname, relative } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const projectRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..');
// These are immutable external rules or generic workflow examples, not claims
// about this game's spec. The snapshots are checked by upstream.mjs instead.
const examples = new Set(['docs/goals/README.md', 'docs/live-sessions/README.md',
  'docs/reports/README.md', 'docs/SPEC-ENTRY-TEMPLATES.md', 'docs/CUSTOMIZATION.md', 'docs/BOUNDED-EVIDENCE-REPORTERS.md']);
function files(path) {
  if (!existsSync(path)) return [];
  return readdirSync(path, { withFileTypes: true }).flatMap(entry => {
    const full = resolve(path, entry.name);
    if (entry.isSymbolicLink()) throw new Error(`Refusing linked documentation path: ${full}`);
    return entry.isDirectory() ? files(full) : entry.name.endsWith('.md') ? [full] : [];
  });
}
export function checkNarrative(root = projectRoot) {
  const entries = new Map();
  for (const file of [...files(resolve(root, 'spec')), ...files(resolve(root, 'deviations'))]) {
    const text = readFileSync(file, 'utf8');
    const header = /^---\r?\n([\s\S]*?)\r?\n---/.exec(text)?.[1];
    if (!header) {
      // Standard v1 deviation entries have a heading and bullet fields rather
      // than YAML frontmatter.
      const deviation = /^#\s+(DEV-[A-Z0-9]+-\d+)\s*$/m.exec(text)?.[1];
      if (deviation) entries.set(deviation, /^- Dropped:\s*yes\s*$/mi.test(text));
      continue;
    }
    const id = /^id:\s*(\S+)/m.exec(header)?.[1];
    if (id) entries.set(id, /^status:\s*superseded\s*$/m.test(header) ||
      /^superseded_by:\s*\[[^\]\s][^\]]*\]/m.test(header));
  }
  const problems = [];
  for (const file of files(resolve(root, 'docs'))) {
    const path = relative(root, file).replaceAll('\\', '/');
    if (path.startsWith('docs/upstream/') || examples.has(path)) continue;
    const lines = readFileSync(file, 'utf8').split(/\r?\n/);
    lines.forEach((line, index) => {
      for (const id of new Set(line.match(/\b(?:BLD|SRC|FND|EXP|RULE|FMT|SCR|BUG|DEV)-[A-Z0-9]+(?:-[A-Z0-9]+)*\b/g) ?? [])) {
        if (/^(BLD|SRC)-/.test(id) && !entries.has(id)) continue;
        if (!entries.has(id)) problems.push(`${path}:${index + 1}: cites missing entry ${id}`);
        else if (entries.get(id)) problems.push(`${path}:${index + 1}: cites superseded entry ${id}`);
      }
    });
  }
  return problems;
}
if (process.argv[1] && import.meta.url === pathToFileURL(resolve(process.argv[1])).href) {
  try {
    const problems = checkNarrative();
    if (problems.length) { console.error(problems.join('\n')); process.exitCode = 1; }
    else console.log('Narrative references resolve to active project spec entries.');
  } catch (error) { console.error(error.message); process.exitCode = 1; }
}
