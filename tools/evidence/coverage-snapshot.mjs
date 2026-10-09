// Convert address/count-only Ghidra exports to the Standard's inventory notation.
import { readFileSync, readdirSync, statSync, lstatSync, writeFileSync, renameSync, unlinkSync } from 'node:fs';
import { resolve, relative, join, dirname } from 'node:path';
import { createHash } from 'node:crypto';
import { pathToFileURL } from 'node:url';
import { sourceXxh3 } from '@scientific-method/executable-reader';
import { inventoryPath } from './inventory.mjs';

const hash = value => createHash('sha256').update(value).digest('hex');
function boundedFile(path, limit = 32*1024*1024) {
  const stat = statSync(path);
  if (!stat.isFile() || stat.size > limit) throw new Error('Input exceeds file bound');
  return readFileSync(path);
}
export function snapshotDigest(directory) {
  const digest = createHash('sha256');
  function visit(path) {
    for (const name of readdirSync(path).sort()) {
      const file = join(path, name), stat = lstatSync(file);
      if (stat.isSymbolicLink()) throw new Error('Snapshot must not follow links');
      if (stat.isDirectory()) visit(file);
      else if (stat.isFile()) {
        digest.update(relative(directory, file).split('\\').join('/') + '\0');
        digest.update(readFileSync(file));
      } else throw new Error('Unsupported snapshot item');
    }
  }
  visit(directory);
  return digest.digest('hex');
}
function table(text, columns) {
  const lines = text.trimEnd().split(/\r?\n/);
  if (lines.shift() !== columns.join('\t')) throw new Error('Unexpected export columns');
  return lines.map(line => {
    const cells = line.split('\t');
    if (cells.length !== columns.length) throw new Error('Invalid export row');
    return Object.fromEntries(columns.map((column, i) => [column, cells[i]]));
  });
}
function integer(value, allowZero = true) {
  if (!/^\d+$/.test(String(value)) || !Number.isSafeInteger(Number(value)) || (!allowZero && Number(value) === 0))
    throw new Error('Invalid count');
  return Number(value);
}
export function unionRanges(ranges) {
  const merged = [];
  for (const range of [...ranges].sort((a,b) => a.start-b.start)) {
    const last = merged.at(-1);
    if (last && range.start <= last.end) last.end = Math.max(last.end, range.end);
    else merged.push({...range});
  }
  return merged;
}
const intersectBytes = (ranges, start, end) => ranges.reduce((n,r) => n + Math.max(0, Math.min(end,r.end)-Math.max(start,r.start)),0);
export function addressMapper(format, selectors = {}) {
  if (!['LE','PE','MZ','NE'].includes(format)) throw new Error('Unsupported address format');
  if (format === 'NE' && (Object.entries(selectors).some(([key,value]) =>
    !/^[0-9A-F]{4}$/.test(key) || !Number.isInteger(value) || value < 1 || value > 65535) ||
    new Set(Object.values(selectors)).size !== Object.keys(selectors).length)) throw new Error('Invalid NE selector mapping');
  const hex = (n, width) => n.toString(16).toUpperCase().padStart(width, '0');
  const raw = value => {
    if (['LE', 'PE'].includes(format) && /^[0-9a-f]{8}$/i.test(value)) return parseInt(value, 16);
    const match = /^([0-9a-f]{4}):([0-9a-f]{4})$/i.exec(value);
    if (!match) throw new Error('Invalid analyzer address');
    const segment = parseInt(match[1], 16), offset = parseInt(match[2], 16);
    if (format === 'MZ') return segment * 16 + offset;
    if (format !== 'NE') throw new Error('Unsupported address format');
    const canonicalSegment = selectors[match[1].toUpperCase()];
    return canonicalSegment === undefined ? null : canonicalSegment * 65536 + offset;
  };
  const written = n => {
    if (!Number.isSafeInteger(n) || n < 0) throw new Error('Invalid mapped address');
    if (['LE', 'PE'].includes(format)) {
      if (n > 0xffffffff) throw new Error('Flat address overflow');
      return '0x' + hex(n, 8);
    }
    if (format === 'NE') {
      if (n > 0xffffffff) throw new Error('NE address overflow');
      return `${hex(Math.floor(n / 65536), 4)}:${hex(n % 65536, 4)}`;
    }
    if (n > 0xfffff) throw new Error('Real-mode address overflow');
    return `${hex(Math.floor(n / 16), 4)}:${hex(n % 16, 4)}`;
  };
  return { raw, written };
}
export function convertSnapshot(input, contract) {
  const map = addressMapper(contract.format, contract.selectors);
  const rows = table(input.functions, ['start', 'size', 'ranges']);
  const functions = [], excluded = [];
  for (const row of rows) {
    const start = map.raw(row.start), size = integer(row.size, false);
    if (start === null) {
      if (row.ranges.split(';').some(span => map.raw(span.slice(0,span.lastIndexOf(':'))) !== null))
        throw new Error('Unmapped entry owns mapped source bytes');
      excluded.push(row); continue;
    }
    const body = row.ranges.split(';').map(span => {
      const match = /^(.*):(\d+)$/.exec(span);
      if (!match) throw new Error('Invalid raw body range');
      const begin = map.raw(match[1]), length = integer(match[2], false);
      if (begin === null) throw new Error('Body crosses unmapped source segment');
      const end = begin + length;
      if (contract.format === 'NE' && Math.floor((end - 1) / 65536) !== Math.floor(begin / 65536))
        throw new Error('Body range crosses NE segment');
      return { start: begin, end };
    }).sort((a,b) => a.start - b.start);
    if (body.some((r, i) => i && r.start < body[i-1].end) ||
        body.reduce((sum,r) => sum + r.end-r.start, 0) !== size ||
        !body.some(r => start >= r.start && start < r.end)) throw new Error('Invalid function body');
    functions.push({ start, size, body });
  }
  functions.sort((a,b) => a.start-b.start);
  if (!functions.length || functions.some((f,i) => i && f.start === functions[i-1].start)) throw new Error('Duplicate or empty mapped inventory');
  const inventory = 'start\tsize\tranges\n' + functions.map(f =>
    `${map.written(f.start)}\t${f.size}\t${f.body.map(r => `${map.written(r.start)}..${map.written(r.end)}`).join(' ')}\n`).join('');
  const columns = ['start','size','initialized','instructions','data','undefined','instructions_outside','data_outside','undefined_outside'];
  const bodies = unionRanges(functions.flatMap(f => f.body));
  const regions = table(input.regions, columns).map(row => {
    const start = map.raw(row.start);
    if (start === null) throw new Error('Executable region has no source mapping');
    const counts = Object.fromEntries(columns.filter(c => !['start','initialized'].includes(c)).map(c => [c, integer(row[c])]));
    if (!['true','false'].includes(row.initialized) || !counts.size ||
        counts.instructions + counts.data + counts.undefined !== counts.size ||
        ['instructions','data','undefined'].some(c => counts[c+'_outside'] > counts[c])) throw new Error('Invalid denominator partition');
    const covered = counts.size - counts.instructions_outside - counts.data_outside - counts.undefined_outside;
    if (covered !== intersectBytes(bodies, start, start+counts.size)) throw new Error('Denominator body intersection disagrees with inventory');
    return { ...row, ...counts, start: map.written(start) };
  });
  if (!regions.length) throw new Error('No measured executable regions');
  return { inventory, regions: columns.join('\t') + '\n' + regions.map(row => columns.map(c => row[c]).join('\t')+'\n').join(''),
    functions: functions.length, excludedFunctions: excluded.length,
    uniqueBodyBytes: bodies.reduce((n,r) => n+r.end-r.start,0) };
}
export function adoptSnapshot(configPath) {
  const configFile = resolve(configPath), base = dirname(configFile);
  const config = JSON.parse(boundedFile(configFile,1024*1024).toString('utf8'));
  const local = path => resolve(base, path);
  const source = boundedFile(local(config.source),256*1024*1024);
  if (sourceXxh3(source) !== config.xxh3 || hash(source) !== config.sourceSha256) throw new Error('Source identity mismatch');
  const snapshot = snapshotDigest(local(config.snapshot));
  if (snapshot !== config.snapshotSha256) throw new Error('Database snapshot changed');
  const exported = local(config.export);
  const metadataText = boundedFile(join(exported,'metadata.tsv')).toString('utf8');
  const metadata = Object.fromEntries(metadataText.trimEnd().split(/\r?\n/).map(line => line.split('\t')));
  if (metadata.source_sha256 !== config.sourceSha256) throw new Error('Database source identity mismatch');
  const log = boundedFile(local(config.log)).toString('utf8');
  if (!log.includes(`COVERAGE_EXPORT_COMPLETE functions=${metadata.functions} uniqueBodyBytes=${metadata.body_bytes_unique}`))
    throw new Error('No exporter completion marker');
  const result = convertSnapshot({ functions: boundedFile(join(exported,'functions.tsv')).toString('utf8'),
    regions: boundedFile(join(exported,'regions.tsv')).toString('utf8') }, config);
  if (result.functions + result.excludedFunctions !== integer(metadata.functions)) throw new Error('Export count mismatch');
  if (result.excludedFunctions && !config.exclusionReason) throw new Error('Unmapped definitions require a scope reason');
  const revision = hash(readFileSync(new URL('../ghidra/ExportCoverageSnapshot.java', import.meta.url)));
  const provenance = { xxh3: config.xxh3, source_sha256: config.sourceSha256, analysis_tool: metadata.analysis_tool,
    language: metadata.language, database_snapshot: `sha256:${snapshot}`, export_revision: `sha256:${revision}`,
    exported_functions: metadata.functions, unmapped_functions: result.excludedFunctions,
    unmapped_reason: config.exclusionReason ?? 'None', analyzer_body_bytes_unique: metadata.body_bytes_unique,
    analyzer_body_bytes_outside_executable: metadata.body_bytes_outside_executable };
  if (Object.values(provenance).some(value=>/[\t\r\n]/.test(String(value)))) throw new Error('Invalid provenance field');
  const path = local(config.writeRoot + '/' + inventoryPath(config.build, config.manifest));
  // All validation is complete before any destination is replaced.
  const outputs = [[path,result.inventory],[path.replace(/\.tsv$/,'.regions.tsv'),result.regions],
    [path.replace(/\.tsv$/,'.provenance.tsv'),Object.entries(provenance).map(([k,v]) => `${k}\t${v}\n`).join('')]];
  const partials = [];
  try {
    for (const [destination, text] of outputs) {
      const temporary = destination + `.partial-${process.pid}`;
      writeFileSync(temporary,text,{flag:'wx'}); partials.push([temporary,destination]);
    }
    for (const [temporary,destination] of partials) renameSync(temporary,destination);
  } finally {
    for (const [temporary] of partials) { try { unlinkSync(temporary); } catch(error) {if(error.code!=='ENOENT') throw error;} }
  }
  return { ...result, inventory: undefined, regions: undefined, path };
}
if (process.argv[1] && import.meta.url === pathToFileURL(resolve(process.argv[1])).href) {
  try {
    const [command, path, extra] = process.argv.slice(2);
    if (!path || extra) throw new Error('Usage: coverage-snapshot.mjs <snapshot|adopt> <path>');
    if (command === 'snapshot') console.log(snapshotDigest(resolve(path)));
    else if (command === 'adopt') console.log(JSON.stringify(adoptSnapshot(path)));
    else throw new Error('Unknown snapshot command');
  } catch (error) { console.error(error.message); process.exitCode = 1; }
}
