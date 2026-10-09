#!/usr/bin/env node
// The installed checker owns canonical address/body parsing; this project checks its sidecars.
import { readFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { resolve } from 'node:path';
import { verifyCoverageBundle } from './evidence/coverage-bundle.mjs';
const require = createRequire(import.meta.url);
const packageModule = path => import(pathToFileURL(require.resolve(`@scientific-method/standard-checker/dist/${path}.js`)));
const [{loadSpec}, {parseOptions}, {readInventories}] = await Promise.all([
  packageModule('load/spec'), packageModule('options'), packageModule('inventory')]);
const root = fileURLToPath(new URL('..', import.meta.url));
const problems = [];
const spec = loadSpec({config:parseOptions(['--root',root]),problem:(file,message)=>problems.push(`${file}: ${message}`)});
const read = readInventories(root,spec);
problems.push(...read.problems);
if (!read.inventories.length) problems.push('No committed coverage inventory');
for (const inventory of read.inventories) {
  try {
    const source = spec.buildFiles.get(inventory.build).find(f=>f.path===inventory.file);
    const summary = verifyCoverageBundle(inventory,
      readFileSync(resolve(root,inventory.path.replace(/\.tsv$/,'.regions.tsv')),'utf8'),
      readFileSync(resolve(root,inventory.path.replace(/\.tsv$/,'.provenance.tsv')),'utf8'), source);
    console.log(`${inventory.path}: validated canonical bodies, provenance and region partition (${summary.functions} functions).`);
  } catch(error) { problems.push(`${inventory.path}: ${error.message}`); }
}
if (problems.length) { console.error(problems.join('\n')); process.exitCode=1; }
