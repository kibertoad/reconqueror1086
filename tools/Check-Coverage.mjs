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
export function checkCoverage(root, {build,manifest}={}) {
root=resolve(root);
const problems = [], summaries=[];
const spec = loadSpec({config:parseOptions(['--root',root]),problem:(file,message)=>problems.push(`${file}: ${message}`)});
const read = readInventories(root,spec);
problems.push(...read.problems);
if (!read.inventories.length) problems.push('No committed coverage inventory');
const selected=read.inventories.filter(inventory=>(!build || inventory.build===build) && (!manifest || inventory.file===manifest));
if ((build || manifest) && !selected.length) problems.push('No inventory matches the selected build/manifest');
for (const inventory of selected) {
  try {
    const source = spec.buildFiles.get(inventory.build).find(f=>f.path===inventory.file);
    const summary = verifyCoverageBundle(inventory,
      readFileSync(resolve(root,inventory.path.replace(/\.tsv$/,'.regions.tsv')),'utf8'),
      readFileSync(resolve(root,inventory.path.replace(/\.tsv$/,'.provenance.tsv')),'utf8'), source);
    summaries.push({path:inventory.path,...summary});
  } catch(error) { problems.push(`${inventory.path}: ${error.message}`); }
}
if (problems.length) throw new Error(problems.join('\n'));
return {inventories:summaries};
}
if (process.argv[1] && import.meta.url===pathToFileURL(resolve(process.argv[1])).href) {
  try {
    const result=checkCoverage(fileURLToPath(new URL('..', import.meta.url)));
    for (const summary of result.inventories)
      console.log(`${summary.path}: validated canonical bodies, provenance and region partition (${summary.functions} functions).`);
  } catch(error) {console.error(error.message);process.exitCode=1;}
}
