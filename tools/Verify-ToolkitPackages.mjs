#!/usr/bin/env node
import { readFileSync, realpathSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";
const ROOT = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const json = (root, path) => JSON.parse(readFileSync(resolve(root, path), "utf8"));
export function verifyToolkitPackages(root = ROOT) {
  const adoption = json(root, "tools/toolkit-packages.json");
  if (adoption.repository !== "kibertoad/refurbished-dinosaurs-toolkit" || !/^[a-f0-9]{40}$/.test(adoption.revision ?? "")) throw new Error("Invalid toolkit adoption revision");
  const names = ["@scientific-method/executable-reader", "@scientific-method/standard-checker"];
  if (Object.keys(adoption.packages ?? {}).sort().join() !== names.sort().join()) throw new Error("Toolkit adoption must name both packages");
  const manifest = json(root, "package.json"), lock = json(root, "package-lock.json");
  for (const name of names) {
    const version = adoption.packages[name];
    if (!/^\d+\.\d+\.\d+$/.test(version) || manifest.devDependencies?.[name] !== version || lock.packages?.[""].devDependencies?.[name] !== version) throw new Error(`Toolkit version is not exactly locked: ${name}`);
    const dependency = lock.packages?.[`node_modules/${name}`];
    if (dependency?.version !== version || !/^sha512-[A-Za-z0-9+/]+={0,2}$/.test(dependency.integrity ?? "")) throw new Error(`Missing package integrity: ${name}`);
    let installed;
    try { installed = json(root, `node_modules/${name}/package.json`); }
    catch { throw new Error(`Install locked toolkit packages with npm ci --ignore-scripts: ${name}`); }
    if (installed.version !== version || installed.name !== name) throw new Error(`Installed toolkit differs from lock: ${name}`);
  }
  const requirements = readFileSync(resolve(root, "requirements-evidence.txt"), "utf8");
  if (!/^\d+\.\d+\.\d+$/.test(adoption.engine ?? "") || !requirements.includes(`scientific-method-engine==${adoption.engine} \\`)) throw new Error("Engine requirement differs from toolkit adoption");
  const ci = readFileSync(resolve(root, ".github/workflows/ci.yml"), "utf8");
  const pins = ci.split(/\r?\n/).filter(line => line.includes("kibertoad/refurbished-dinosaurs-toolkit/actions/check-documentation@"));
  if (pins.length !== 1 || !pins[0].trim().startsWith(`- uses: kibertoad/refurbished-dinosaurs-toolkit/actions/check-documentation@${adoption.revision}`)) throw new Error("CI checker revision differs from toolkit adoption");
  return adoption;
}
const direct = (() => { try { return process.argv[1] && import.meta.url === pathToFileURL(realpathSync(process.argv[1])).href; } catch { return false; } })();
if (direct) {
  try { if (process.argv.length !== 2) throw new Error("Usage: node tools/Verify-ToolkitPackages.mjs"); const pin = verifyToolkitPackages(); console.log(`Toolkit packages verified at ${pin.revision}; engine ${pin.engine}`); }
  catch (error) { console.error(error.message); process.exitCode = 1; }
}
