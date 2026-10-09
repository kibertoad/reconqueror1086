// Citation coverage is separate from analyzer mapping and a complete reading.
import { createRequire } from 'node:module';
import { pathToFileURL } from 'node:url';
import { unionRanges } from './coverage-snapshot.mjs';
const require = createRequire(import.meta.url);
const { locationRange } = await import(pathToFileURL(require.resolve('@scientific-method/standard-checker/dist/inventory.js')));

function countBytes(ranges) {
  return unionRanges(ranges).reduce((sum, range) => sum + range.end - range.start, 0);
}
function numeric(range) {
  const start = Number(range.start), end = Number(range.end);
  if (!Number.isSafeInteger(start) || !Number.isSafeInteger(end)) throw new Error('Coverage address exceeds exact numeric bounds');
  return {start,end};
}
export function measureInventory(inventory, entries, {metadataOnly = []} = {}) {
  const metadata = new Set(metadataOnly), locations = [], completeReadingDeclarations = [];
  for (const entry of entries) {
    const meta = entry.meta;
    if (meta.status === 'superseded') continue;
    if (meta.complete_reading?.length) completeReadingDeclarations.push(meta.id);
    for (const loc of meta.locations ?? []) {
      if (loc.build !== inventory.build || loc.file !== inventory.file || loc.kind === 'file-data' ||
          loc.unpacked === true) continue;
      const range = locationRange(loc, inventory.format);
      if (!range || range.start >= range.end) throw new Error(`Invalid coverage location in ${meta.id}`);
      locations.push({...numeric(range),space:range.space,id:meta.id,metadata:metadata.has(meta.id)});
    }
  }
  const functions = inventory.functions.map(fn => {
    const body = fn.body.map(numeric);
    const cites = locations.filter(loc => loc.space === fn.space && body.some(r => loc.start < r.end && r.start < loc.end));
    return {...fn,body,cites};
  });
  const inScope = functions.filter(fn => !fn.outOfScope);
  // Keep address and overlay-offset spaces separate even if their numeric coordinates coincide.
  const bytes = selected => [...new Set(selected.map(fn => fn.space))].reduce((sum, space) =>
    sum + countBytes(selected.filter(fn => fn.space === space).flatMap(fn => fn.body)),0);
  const summary = include => {
    const selected = inScope.filter(fn => fn.cites.some(include));
    const intersections = selected.flatMap(fn => fn.cites.filter(include).flatMap(loc => fn.body
      .filter(r => loc.start < r.end && r.start < loc.end)
      .map(r => ({space:fn.space,start:Math.max(r.start,loc.start),end:Math.min(r.end,loc.end)}))));
    const locationBytes = [...new Set(intersections.map(r => r.space))].reduce((sum,space) =>
      sum + countBytes(intersections.filter(r => r.space === space)),0);
    return {functions:selected.length,bodyBytes:bytes(selected),locationBytes};
  };
  return {path:inventory.path,build:inventory.build,file:inventory.file,
    functions:functions.length,outOfScope:functions.length-inScope.length,bodyBytes:bytes(inScope),
    citations:summary(() => true),researchCitations:summary(loc => !loc.metadata),
    metadataCitations:summary(loc => loc.metadata),
    completeReading:{status:'unavailable',reason:'No audited function-level complete-reading baseline',
      declarations:completeReadingDeclarations.sort()},
    functionDetails:functions.map(fn => ({start:fn.start,size:fn.size,outOfScope:fn.outOfScope || null,
      citedBy:[...new Set(fn.cites.map(loc => loc.id))].sort(),
      researchCitedBy:[...new Set(fn.cites.filter(loc => !loc.metadata).map(loc => loc.id))].sort()}))};
}

export function compareInventories(previous, current) {
  if (previous.build !== current.build || previous.file !== current.file || previous.format !== current.format)
    throw new Error('Coverage comparison requires the same build, file and address format');
  const signature = fn => JSON.stringify({size:fn.size,space:fn.space,
    body:fn.body.map(r => [String(r.start),String(r.end)])});
  const before = new Map(previous.functions.map(fn => [fn.start,fn]));
  const after = new Map(current.functions.map(fn => [fn.start,fn]));
  return {added:[...after.keys()].filter(start => !before.has(start)).sort(),
    removed:[...before.keys()].filter(start => !after.has(start)).sort(),
    changed:[...after.keys()].filter(start => before.has(start) && signature(before.get(start)) !== signature(after.get(start))).sort()};
}
