import test from "node:test";
import assert from "node:assert/strict";
import { mkdtempSync, mkdirSync, writeFileSync, readFileSync, copyFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { resolve, join, dirname } from "node:path";
import { createHash } from "node:crypto";
import { spawnSync } from "node:child_process";
import { run as packageRun, PREPARED_PROTOCOL } from "@scientific-method/executable-reader";
import { run } from "../../tools/evidence/report.mjs";
import { verifyToolkitPackages } from "../../tools/Verify-ToolkitPackages.mjs";
const root = resolve(import.meta.dirname, "../..");

test("evidence environment follows changed pins and rejects transitive drift", t => {
  const dir = scratch(t), metadata = join(dir, "metadata");
  mkdirSync(join(dir, "tools")); mkdirSync(metadata);
  writeFileSync(join(dir, "tools/toolkit-packages.json"), JSON.stringify({engine:"9.8.7"}));
  const requirements = "scientific-method-engine==9.8.7\ncapstone==1.2.3\npypcode==4.5.6\n";
  writeFileSync(join(dir, "requirements-evidence.txt"), requirements);
  const distribution = (name, version) => {
    const path = join(metadata, name.replaceAll("-", "_") + "-synthetic.dist-info");
    mkdirSync(path, {recursive:true});
    writeFileSync(join(path, "METADATA"), `Metadata-Version: 2.1\nName: ${name}\nVersion: ${version}\n`);
  };
  distribution("scientific-method-engine", "9.8.7"); distribution("capstone", "1.2.3"); distribution("pypcode", "4.5.6");
  const runCheck = () => spawnSync(process.env.EVIDENCE_PYTHON || "python", ["-B", join(root, "tools/Verify-EvidenceEnvironment.py"), "--root", dir],
    {encoding:"utf8", env:{...process.env, PYTHONPATH:metadata}});
  let result = runCheck(); assert.equal(result.status, 0, result.error?.message || result.stderr);
  distribution("pypcode", "4.5.7"); result = runCheck(); assert.notEqual(result.status, 0); assert.match(result.stderr, /pypcode: expected 4\.5\.6, installed 4\.5\.7/);
  distribution("pypcode", "4.5.6"); writeFileSync(join(dir, "requirements-evidence.txt"), requirements + "synthetic-env-proof==8.0.0\n");
  result = runCheck(); assert.notEqual(result.status, 0); assert.match(result.stderr, /Missing evidence distribution: synthetic-env-proof==8\.0\.0/);
  writeFileSync(join(dir, "requirements-evidence.txt"), requirements.replace("capstone==", "capstone>="));
  result = runCheck(); assert.notEqual(result.status, 0); assert.match(result.stderr, /expected an exact distribution pin/);
  writeFileSync(join(dir, "requirements-evidence.txt"), requirements.replace("9.8.7", "9.8.8"));
  result = runCheck(); assert.notEqual(result.status, 0); assert.match(result.stderr, /Engine requirement differs/);
});
function scratch(t) { const dir = mkdtempSync(join(tmpdir(), "conqueror-packages-")); t.after(() => rmSync(dir, { recursive: true, force: true })); return dir; }
test("Conqueror x86 wrapper forwards package reports and retains synthetic return behavior", t => {
  const dir = scratch(t), bytes = Buffer.from([0xb8, 0x34, 0x12, 0xc3]);
  writeFileSync(join(dir, "source.bin"), bytes);
  const config = {source:"source.bin", sourceKind:"synthetic-raw", sha256:createHash("sha256").update(bytes).digest("hex"), entry:0,
    regions:[{name:"synthetic",start:0,end:4,segment:4096,ip:0,entries:[0],evidence:"synthetic test"}]};
  const file = join(dir, "config.json"); writeFileSync(file, JSON.stringify(config));
  assert.equal(PREPARED_PROTOCOL, 1);
  const report = run(["x86-returns", file]);
  assert.deepEqual(report, packageRun(["returns", file]));
  // Baseline was produced entirely from the fabricated four-byte source above.
  // New return-flow fields are allowed, but every prior field and value must survive.
  const baseline = JSON.parse(readFileSync(join(root, "tests/fixtures/synthetic/toolkit/return-baseline.json")));
  const retain = (actual, previous) => Array.isArray(previous) ? actual.map((value, i) => retain(value, previous[i])) :
    previous && typeof previous === "object" ? Object.fromEntries(Object.entries(previous).map(([key, value]) => [key, retain(actual[key], value)])) : actual;
  assert.deepEqual(retain(report, baseline), baseline);

  assert.equal(report.completeWithinModel, true);
  assert.equal(report.paths.length, 1);
  assert.equal(report.paths[0].registers.ax.value, 4660);
  writeFileSync(file, JSON.stringify({...config,sha256:"0".repeat(64)}));
  assert.throws(() => run(["x86-returns", file]), /baseline/);
});
test("package adoption rejects manifest, integrity, installed-version and action drift", t => {
  const dir = scratch(t);
  for (const path of ["package.json", "package-lock.json", "requirements-evidence.txt", "tools/toolkit-packages.json", ".github/workflows/ci.yml"]) {
    mkdirSync(dirname(join(dir,path)), {recursive:true}); copyFileSync(join(root,path),join(dir,path));
  }
  for (const name of ["executable-reader", "standard-checker"]) {
    const path=`node_modules/@scientific-method/${name}/package.json`;mkdirSync(dirname(join(dir,path)),{recursive:true});copyFileSync(join(root,path),join(dir,path));
  }
  verifyToolkitPackages(dir);
  const change = (path, mutate, pattern) => {
    const previous=readFileSync(join(dir,path),"utf8"), value=JSON.parse(previous);mutate(value);writeFileSync(join(dir,path),JSON.stringify(value));
    assert.throws(() => verifyToolkitPackages(dir), pattern);writeFileSync(join(dir,path),previous);
  };
  change("package.json", x=>x.devDependencies["@scientific-method/executable-reader"]="^0.2.0", /exactly locked/);
  change("package-lock.json", x=>x.packages["node_modules/@scientific-method/standard-checker"].integrity="", /integrity/);
  change("node_modules/@scientific-method/executable-reader/package.json", x=>x.version="0.0.0", /Installed toolkit/);
  const ci=join(dir,".github/workflows/ci.yml"),text=readFileSync(ci,"utf8"),revision=JSON.parse(readFileSync(join(dir,"tools/toolkit-packages.json"))).revision;
  writeFileSync(ci,text.replace(revision,"a".repeat(40)));assert.throws(()=>verifyToolkitPackages(dir),/CI checker revision/);
});
