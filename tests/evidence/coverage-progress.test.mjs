import test from 'node:test';
import assert from 'node:assert/strict';
import { measureInventory, compareInventories } from '../../tools/evidence/coverage-progress.mjs';
const fn = (start, ranges, outOfScope = '') => ({start,space:'address',size:ranges.reduce((n,[a,b]) => n+b-a,0),
  body:ranges.map(([start,end]) => ({start:BigInt(start),end:BigInt(end)})),outOfScope});
const inventory = functions => ({path:'synthetic.tsv',build:'BLD-SYNTHETIC',file:'GAME.EXE',format:'MZ',functions});
const entry = (id,address,extra = {}) => ({meta:{id,status:'recorded',locations:[
  {build:'BLD-SYNTHETIC',file:'GAME.EXE',address,...extra}]}});

test('coverage keeps holes, shared tails, metadata and exact cited bytes distinct', () => {
  const inv = inventory([fn('1000:0000',[[65536,65540],[65544,65548]]),
    fn('1000:0008',[[65544,65550]]),fn('1001:0000',[[65552,65556]],'Synthetic exclusion')]);
  const result = measureInventory(inv,[entry('mapping','1000:0000..1000:000E'),
    entry('rule','1000:0009'),entry('gap','1000:0005'),
    entry('data','1000:0000',{kind:'file-data'})],{metadataOnly:['mapping']});
  assert.equal(result.bodyBytes,10);
  assert.deepEqual(result.researchCitations,{functions:2,bodyBytes:10,locationBytes:1});
  assert.deepEqual(result.metadataCitations,{functions:2,bodyBytes:10,locationBytes:10});
  assert.equal(result.outOfScope,1);
  assert.ok(result.functionDetails.every(f => !f.citedBy.includes('gap') && !f.citedBy.includes('data')));
  assert.equal(result.completeReading.status,'unavailable');
});

test('packed file-data stays separate from loaded addresses and real-mode aliases agree', () => {
  const inv = inventory([fn('1001:0000',[[65552,65556]])]);
  const entries = [entry('packed-data','1001:0000',{kind:'file-data',unpacked:true}),entry('loaded-code','1000:0010')];
  assert.deepEqual(measureInventory(inv,entries).functionDetails[0].citedBy,['loaded-code']);
});

test('inventory changes remain separate from unchanged research evidence', () => {
  const before = inventory([fn('1000:0000',[[65536,65540]]),fn('1000:0008',[[65544,65548]])]);
  const after = inventory([fn('1000:0000',[[65536,65542]]),fn('1001:0000',[[65552,65556]])]);
  assert.deepEqual(compareInventories(before,after),{added:['1001:0000'],removed:['1000:0008'],changed:['1000:0000']});
  const evidence = [entry('rule','1000:0001')];
  assert.equal(measureInventory(before,evidence).citations.locationBytes,1);
  assert.equal(measureInventory(after,evidence).citations.locationBytes,1);
  assert.notEqual(measureInventory(before,evidence).bodyBytes,measureInventory(after,evidence).bodyBytes);
});
