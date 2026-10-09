import test from 'node:test';
import assert from 'node:assert/strict';
import {mkdtempSync,mkdirSync,writeFileSync,readFileSync,rmSync} from 'node:fs';
import {tmpdir} from 'node:os';
import {join,resolve} from 'node:path';
import {createHash} from 'node:crypto';
import {sourceXxh3} from '@scientific-method/executable-reader';
import {snapshotDigest} from '../../tools/evidence/coverage-snapshot.mjs';
import {run} from '../../tools/evidence/report.mjs';

test('native inventory command preserves separate bodies and guards the frozen snapshot',t=>{
  const dir=mkdtempSync(join(tmpdir(),'native-inventory-'));
  t.after(()=>rmSync(dir,{recursive:true,force:true}));
  const source=Buffer.alloc(512);
  source.write('MZ');source.writeUInt16LE(1,4);source.writeUInt16LE(4,8);source.writeUInt16LE(28,24);
  const sha=createHash('sha256').update(source).digest('hex');
  writeFileSync(join(dir,'source.exe'),source);
  mkdirSync(join(dir,'snapshot.rep'));writeFileSync(join(dir,'snapshot.rep','state'),'synthetic analysis snapshot');
  mkdirSync(join(dir,'export'));
  writeFileSync(join(dir,'export','metadata.tsv'),`source_sha256\t${sha}\nanalysis_tool\tSynthetic analyzer\nlanguage\tx86 synthetic\nfunctions\t1\nbody_bytes_unique\t6\nbody_bytes_outside_executable\t0\n`);
  writeFileSync(join(dir,'export','functions.tsv'),'start\tsize\tranges\n1000:0000\t6\t1000:0000:2;1000:0008:4\n');
  writeFileSync(join(dir,'export','regions.tsv'),'start\tsize\tinitialized\tinstructions\tdata\tundefined\tinstructions_outside\tdata_outside\tundefined_outside\n1000:0000\t16\ttrue\t10\t2\t4\t4\t2\t4\n');
  writeFileSync(join(dir,'export.log'),'COVERAGE_EXPORT_COMPLETE functions=1 uniqueBodyBytes=6');
  const config={source:'source.exe',sourceSha256:sha,xxh3:sourceXxh3(source),
    snapshot:'snapshot.rep',snapshotSha256:snapshotDigest(join(dir,'snapshot.rep')),
    export:'export',log:'export.log',writeRoot:'out',build:'BLD-SYNTHETIC',manifest:'CD:GAME.EXE',format:'MZ'};
  const path=join(dir,'config.json');writeFileSync(path,JSON.stringify(config));
  const result=run(['inventory',path]);
  assert.equal(result.functions,1);
  const body=readFileSync(result.path,'utf8');
  assert.equal(body,'start\tsize\tranges\n1000:0000\t6\t1000:0000..1000:0002 1000:0008..1000:000C\n');
  assert.match(readFileSync(result.path.replace(/\.tsv$/,'.provenance.tsv'),'utf8'),/database_snapshot\tsha256:/);
  assert.match(readFileSync(result.path.replace(/\.tsv$/,'.regions.tsv'),'utf8'),/instructions_outside/);
  writeFileSync(join(dir,'snapshot.rep','state'),'changed snapshot');
  assert.throws(()=>run(['inventory',path]),/snapshot changed/);
  assert.equal(readFileSync(result.path,'utf8'),body);
  writeFileSync(path,JSON.stringify({...config,views:[]}));
  assert.throws(()=>run(['inventory',path]),/Legacy size-only/);
});

test('native inventory-check selects repository metadata and rejects legacy source contracts',t=>{
  const dir=mkdtempSync(join(tmpdir(),'native-check-'));
  t.after(()=>rmSync(dir,{recursive:true,force:true}));
  const path=join(dir,'config.json');
  const config={repositoryRoot:resolve(import.meta.dirname,'../..'),build:'BLD-GOG-EN',manifest:'CD:SETUP.EXE'};
  writeFileSync(path,JSON.stringify(config));
  const result=run(['inventory-check',path]);
  assert.equal(result.inventories.length,1);
  assert.equal(result.inventories[0].path,'coverage/BLD-GOG-EN/@CD/SETUP.EXE.tsv');
  assert.ok(result.inventories[0].functions>0);
  writeFileSync(path,JSON.stringify({...config,manifest:'CD:UNKNOWN.EXE'}));
  assert.throws(()=>run(['inventory-check',path]),/No inventory matches/);
  writeFileSync(path,JSON.stringify({...config,source:'original.exe'}));
  assert.throws(()=>run(['inventory-check',path]),/repository metadata/);
  writeFileSync(path,JSON.stringify({inventory:{path:'legacy.tsv'}}));
  assert.throws(()=>run(['inventory-check',path]),/requires repositoryRoot/);
});

test('operand reports still guard source identities after inventory routing changed',t=>{
  const dir=mkdtempSync(join(tmpdir(),'operand-identity-'));
  t.after(()=>rmSync(dir,{recursive:true,force:true}));
  const source=Buffer.alloc(512);source.write('MZ');source.writeUInt16LE(1,4);source.writeUInt16LE(4,8);
  writeFileSync(join(dir,'source.exe'),source);
  const path=join(dir,'config.json');
  writeFileSync(path,JSON.stringify({source:'source.exe',xxh3:'0'.repeat(32)}));
  assert.throws(()=>run(['operand',path]),/baseline/);
  writeFileSync(path,JSON.stringify({source:'source.exe',sha256:'0'.repeat(64)}));
  assert.throws(()=>run(['operand',path]),/sha256 is no longer read/);
  assert.throws(()=>run(['unknown',path]),/Unknown/);
});
