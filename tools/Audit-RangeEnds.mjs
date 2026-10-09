#!/usr/bin/env node
// Review candidates only. A last-byte coincidence never authorizes an evidence edit.
import { createRequire } from 'node:module';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { writeFileSync } from 'node:fs';
import { resolve, sep } from 'node:path';
const root = fileURLToPath(new URL('..',import.meta.url));
const require = createRequire(import.meta.url);
const module = path=>import(pathToFileURL(require.resolve(`@scientific-method/standard-checker/dist/${path}.js`)));
const [{loadSpec},{parseOptions},{readInventories,linear},{checkRangeEnds}] = await Promise.all([
  module('load/spec'),module('options'),module('inventory'),module('checks/range-ends')]);
const config = parseOptions(['--root',root]), loadProblems = [];
const spec = loadSpec({config,problem:(file,message)=>loadProblems.push({file,message})});
const read = readInventories(root,spec);
if (loadProblems.length || read.problems.length) throw new Error('Fix spec-loading and inventory problems before the range audit');
const diagnostics = [];
checkRangeEnds({config,spec,problem:(file,message)=>diagnostics.push({file,message})});
const candidates = new Map();
for (const diagnostic of diagnostics) {
  const match = /^(?:location (?:address|offset)|the body's range) (\S+)\.\.(\S+) ends/.exec(diagnostic.message);
  if (!match) throw new Error('Unsupported checker range diagnostic');
  const entry = [...spec.entries.values()].find(e=>e.file===diagnostic.file);
  if (!entry) throw new Error('Diagnostic names no spec entry');
  const key = `${diagnostic.file}\0${match[1]}..${match[2]}`;
  if (candidates.has(key)) continue;
  const places = new Set(entry.meta.locations.map(loc=>`${loc.build}\0${loc.file}`));
  const spans = [], groupedSpans = [];
  for (const inv of read.inventories.filter(inv=>places.has(`${inv.build}\0${inv.file}`))) {
    const start = linear(match[1],inv.format), end = linear(match[2],inv.format);
    if (start===null || end===null) continue;
    for (const fn of inv.functions) {
      if (fn.body[0].start===start && fn.body.at(-1).end===end+1n)
        spans.push({build:inv.build,file:inv.file,functionStart:fn.start,
          body:fn.body.map(r=>({start:r.start.toString(),end:r.end.toString()}))});
    }
    // A grouped extent remains a review candidate: holes and omitted callees
    // do not prove which bytes the historical author intended to include.
    const contained = inv.functions.filter(fn=>fn.body.every(r=>r.start>=start && r.end<=end+1n));
    if (contained.length>1 && contained.some(fn=>fn.body[0].start===start) &&
        contained.some(fn=>fn.body.at(-1).end===end+1n))
      groupedSpans.push({build:inv.build,file:inv.file,functions:contained.map(fn=>({
        functionStart:fn.start,body:fn.body.map(r=>({start:r.start.toString(),end:r.end.toString()}))}))});
  }
  candidates.set(key,{entry:entry.meta.id,file:diagnostic.file,oldRange:`${match[1]}..${match[2]}`,
    fullFunctionSpans:spans,groupedFunctionSpans:groupedSpans,intendedEndReviewed:false});
}
const result = {diagnostics:diagnostics.length,candidates:[...candidates.values()],
  fullFunctionCandidates:[...candidates.values()].filter(c=>c.fullFunctionSpans.length).length,
  requiresIntendedEndReview:[...candidates.values()].filter(c=>!c.fullFunctionSpans.length).length};
const output = resolve(process.argv[2]??'artifacts/coverage-migration/range-end-audit.json');
if (!output.startsWith(resolve(root,'artifacts')+sep)) throw new Error('Range audit output must remain in local artifacts');
writeFileSync(output,JSON.stringify(result,null,2)+'\n');
console.log(`Range audit: ${result.diagnostics} diagnostics, ${result.candidates.length} distinct candidates; ${result.fullFunctionCandidates} span complete inventoried bodies, ${result.requiresIntendedEndReview} require other intended-end evidence. Report: ${output}`);
