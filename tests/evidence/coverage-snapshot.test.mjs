import test from 'node:test';
import assert from 'node:assert/strict';
import { addressMapper, convertSnapshot } from '../../tools/evidence/coverage-snapshot.mjs';
import { verifyCoverageBundle } from '../../tools/evidence/coverage-bundle.mjs';
const regionHeader = 'start\tsize\tinitialized\tinstructions\tdata\tundefined\tinstructions_outside\tdata_outside\tundefined_outside\n';
const raw = {
  functions: 'start\tsize\tranges\n00001000\t6\t00001000:2;00001008:4\n',
  regions: regionHeader + '00001000\t16\ttrue\t10\t2\t4\t4\t2\t4\n'
};
test('coverage conversion preserves actual discontiguous bodies and denominator classes', () => {
  const result = convertSnapshot(raw, {format:'PE'});
  assert.equal(result.inventory, 'start\tsize\tranges\n0x00001000\t6\t0x00001000..0x00001002 0x00001008..0x0000100C\n');
  assert.equal(result.functions, 1);
  assert.match(result.regions, /0x00001000\t16\ttrue\t10\t2\t4\t4\t2\t4/);
});
test('real-mode aliases join at physical addresses and NE selectors map to table segments', () => {
  const real = addressMapper('MZ');
  assert.equal(real.raw('1000:0010'), real.raw('1001:0000'));
  assert.equal(real.written(real.raw('1000:0010')), '1001:0000');
  const ne = addressMapper('NE', {'1000':1});
  assert.equal(ne.written(ne.raw('1000:0010')), '0001:0010');
  assert.equal(ne.raw('1058:0000'), null);
  const input = {functions:'start\tsize\tranges\n1000:0000\t2\t1000:0000:2\n1058:0000\t2\t1058:0000:2\n',
    regions: regionHeader+'1000:0000\t16\ttrue\t2\t0\t14\t0\t0\t14\n'};
  const result = convertSnapshot(input, {format:'NE',selectors:{'1000':1}});
  assert.equal(result.excludedFunctions, 1);
  assert.match(result.inventory, /0001:0000\t2\t0001:0000\.\.0001:0002/);
});
test('invalid function extents and duplicate native aliases cannot become an inventory', () => {
  for (const functions of [raw.functions.replace('\t6\t', '\t7\t'),
    raw.functions.replace('00001008:4','00001001:4'),
    raw.functions.replace('00001000\t6','00001004\t6'),
    raw.functions + raw.functions.split('\n')[1]+'\n'])
    assert.throws(() => convertSnapshot({...raw,functions}, {format:'PE'}), /body|Duplicate/);
});
test('denominator partitions reject inconsistent totals and impossible outside counts', () => {
  for (const regions of [raw.regions.replace('\t16\t','\t17\t'),
    raw.regions.replace('\t4\t2\t4\n','\t11\t2\t4\n'),
    raw.regions.replace('\ttrue\t','\tunknown\t')])
    assert.throws(() => convertSnapshot({...raw,regions},{format:'PE'}), /partition/);
});
test('an NE body cannot silently enter the next segment', () => {
  assert.throws(() => convertSnapshot({functions:'start\tsize\tranges\n1000:FFFE\t4\t1000:FFFE:4\n',
    regions:regionHeader+'1000:0000\t65536\ttrue\t4\t0\t65532\t0\t0\t65532\n'},
    {format:'NE',selectors:{'1000':1}}), /crosses NE/);
});
test('sidecar validation joins the manifest identity and unique body union without counting shared tails twice', () => {
  const inventory = {format:'PE',functions:[{body:[{start:4096n,end:4098n},{start:4104n,end:4108n}]},
    {body:[{start:4104n,end:4108n}]}]};
  const source = {xxh3:'a'.repeat(32)};
  const metadata = Object.entries({xxh3:source.xxh3,source_sha256:'b'.repeat(64),analysis_tool:'Ghidra 12.1.3',
    database_snapshot:'sha256:'+'c'.repeat(64),export_revision:'sha256:'+'d'.repeat(64),language:'synthetic',
    exported_functions:2,unmapped_functions:0,unmapped_reason:'None'}).map(row=>row.join('\t')).join('\n')+'\n';
  const result = verifyCoverageBundle(inventory, raw.regions.replace('00001000','0x00001000'),metadata,source);
  assert.equal(result.uniqueBodyBytes,6);
  assert.equal(result.bodyInside,6);
  assert.throws(()=>verifyCoverageBundle(inventory, raw.regions.replace('00001000','0x00001000'),metadata,{xxh3:'f'.repeat(32)}),/identity/);
  assert.throws(()=>verifyCoverageBundle(inventory, raw.regions.replace('00001000','0x00001000').replace('\t4\t2\t4\n','\t5\t2\t4\n'),metadata,source),/intersection/);
});
