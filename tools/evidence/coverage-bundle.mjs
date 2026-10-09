// Validate protocol sidecars against the installed checker's canonical body ranges.
import { unionRanges } from './coverage-snapshot.mjs';
export function verifyCoverageBundle(inventory, regionText, provenanceText, source) {
  const fields = provenanceText.trimEnd().split(/\r?\n/).map(line => line.split('\t'));
  if (fields.some(row => row.length !== 2) || new Set(fields.map(row=>row[0])).size !== fields.length)
    throw new Error('Invalid coverage provenance fields');
  const metadata = Object.fromEntries(fields);
  if (metadata.xxh3 !== (source.unpacked?.xxh3 ?? source.xxh3) || !/^[0-9a-f]{64}$/.test(metadata.source_sha256 ?? '') ||
      !/^Ghidra \d+(?:\.\d+)+$/.test(metadata.analysis_tool ?? '') ||
      !/^sha256:[0-9a-f]{64}$/.test(metadata.database_snapshot ?? '') ||
      !/^sha256:[0-9a-f]{64}$/.test(metadata.export_revision ?? '') || !metadata.language)
    throw new Error('Coverage provenance identity or revision is invalid');
  const regions = regionText.trimEnd().split(/\r?\n/);
  const columns = ['start','size','initialized','instructions','data','undefined','instructions_outside','data_outside','undefined_outside'];
  if (regions.shift() !== columns.join('\t') || !regions.length) throw new Error('Invalid coverage region columns');
  const value = text => {
    if (['PE','LE'].includes(inventory.format)) {
      if (!/^0x[0-9A-F]{8}$/.test(text)) throw new Error('Noncanonical region address');
      return Number(text);
    }
    const pair = /^([0-9A-F]{4}):([0-9A-F]{4})$/.exec(text);
    if (!pair) throw new Error('Noncanonical segmented region address');
    return parseInt(pair[1],16)*(inventory.format==='NE'?65536:16)+parseInt(pair[2],16);
  };
  const body = unionRanges(inventory.functions.flatMap(f => f.body.map(r => ({start:Number(r.start),end:Number(r.end)}))));
  let regionSize = 0, bodyInside = 0, instructionOutside = 0, undefinedOutside = 0;
  const ranges = [];
  for (const line of regions) {
    const cells = line.split('\t');
    if (cells.length !== columns.length || !['true','false'].includes(cells[2])) throw new Error('Invalid region row');
    const numbers = cells.slice(3).map(Number), size = Number(cells[1]), start = value(cells[0]), end = start+size;
    if (!/^[1-9][0-9]*$/.test(cells[1]) || !Number.isSafeInteger(size) ||
        cells.slice(3).some((c,i) => !/^\d+$/.test(c) || !Number.isSafeInteger(numbers[i]))) throw new Error('Invalid region counts');
    if (numbers[0]+numbers[1]+numbers[2] !== size || numbers.slice(3).some((n,i)=>n>numbers[i]))
      throw new Error('Region partition totals do not agree');
    if (inventory.format==='NE' && Math.floor(start/65536)!==Math.floor((end-1)/65536)) throw new Error('Region crosses NE segment');
    if (ranges.some(r=>start<r.end&&r.start<end)) throw new Error('Executable regions overlap');
    ranges.push({start,end});
    const inside = body.reduce((n,r)=>n+Math.max(0,Math.min(end,r.end)-Math.max(start,r.start)),0);
    if (inside !== size-numbers[3]-numbers[4]-numbers[5]) throw new Error('Region body intersection does not agree');
    regionSize += size; bodyInside += inside; instructionOutside += numbers[3]; undefinedOutside += numbers[5];
  }
  const uniqueBodyBytes = body.reduce((n,r)=>n+r.end-r.start,0);
  const unmapped = Number(metadata.unmapped_functions);
  if (!/^\d+$/.test(metadata.unmapped_functions ?? '') || !Number.isSafeInteger(unmapped) ||
      Number(metadata.exported_functions) !== inventory.functions.length+unmapped ||
      (unmapped && (!metadata.unmapped_reason || metadata.unmapped_reason==='None')))
    throw new Error('Function export count or exclusion provenance is invalid');
  return {functions:inventory.functions.length, regionSize, uniqueBodyBytes, bodyInside,
    bodyOutside:uniqueBodyBytes-bodyInside, instructionOutside, undefinedOutside};
}
