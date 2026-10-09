#!/usr/bin/env node
// Reads committed metadata only; generated reports remain local.
import { readFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { checkCoverage } from './Check-Coverage.mjs';
import { measureInventory } from './evidence/coverage-progress.mjs';
const require = createRequire(import.meta.url);
const module = path => import(pathToFileURL(require.resolve(`@scientific-method/standard-checker/dist/${path}.js`)));
const [{loadSpec},{parseOptions},{readInventories}] = await Promise.all([
  module('load/spec'),module('options'),module('inventory')]);
export function reportCoverage(root) {
  root = resolve(root);
  const bundles = checkCoverage(root).inventories;
  const problems = [];
  const spec = loadSpec({config:parseOptions(['--root',root]),problem:(file,message) => problems.push(`${file}: ${message}`)});
  const read = readInventories(root,spec);
  problems.push(...read.problems);
  const baseline = JSON.parse(readFileSync(resolve(root,'coverage/baseline.json'),'utf8'));
  if (!Array.isArray(baseline.metadataOnlyFindings) || baseline.metadataOnlyFindings.some(id =>
    !spec.entries.has(id) || spec.entries.get(id).meta.status === 'superseded')) throw new Error('Invalid metadata-only coverage findings');
  if (problems.length) throw new Error(problems.join('\n'));
  const inventories = read.inventories.map(inv => {
    const {functionDetails,...summary} = measureInventory(inv,[...spec.entries.values()],
      {metadataOnly:baseline.metadataOnlyFindings});
    return {...summary,denominator:bundles.find(bundle => bundle.path === inv.path),
      provenance:Object.fromEntries(readFileSync(resolve(root,inv.path.replace(/\.tsv$/,'.provenance.tsv')),'utf8')
        .trimEnd().split(/\r?\n/).map(line => line.split('\t')))};
  });
  const inventoried = new Set(inventories.map(inv => `${inv.build}\0${inv.file}`));
  const missing = [...spec.buildFiles].flatMap(([build,files]) => files.filter(file =>
    ['MZ','COM','NE','LE','LX','PE'].includes(file.format) && !inventoried.has(`${build}\0${file.path}`))
    .map(file => ({build,file:file.path,format:file.format})));
  return {inventories,missingInventories:missing,limitations:baseline.limitations};
}
if (process.argv[1] && import.meta.url === pathToFileURL(resolve(process.argv[1])).href) {
  try {
    if (process.argv.length > 3) throw new Error('Usage: Report-Coverage.mjs [repository-root]');
    console.log(JSON.stringify(reportCoverage(process.argv[2] ?? fileURLToPath(new URL('..',import.meta.url))),null,2));
  } catch(error) {console.error(error.message);process.exitCode = 1;}
}
