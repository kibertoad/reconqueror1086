#!/usr/bin/env node
import { readFileSync, realpathSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";
const ROOT = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const text = (root, path) => readFileSync(resolve(root, path), "utf8");
const json = (root, path) => JSON.parse(text(root, path));
const escape = (value) => value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
// tools/toolkit-packages.json names the toolkit release this repository adopts. The checker's commit and
// version also live in tools/upstream-lock.json, which tools/upstream.mjs checks against the CI action.
export function verifyToolkitPackages(root = ROOT) {
  const adoption = json(root, "tools/toolkit-packages.json");
  if (adoption.repository !== "kibertoad/refurbished-dinosaurs-toolkit" || !/^[a-f0-9]{40}$/.test(adoption.revision ?? "")) throw new Error("Invalid toolkit adoption revision");
  const names = ["@scientific-method/executable-reader", "@scientific-method/standard-checker"];
  if (Object.keys(adoption.packages ?? {}).sort().join() !== names.sort().join()) throw new Error("Toolkit adoption must name both packages");
  const { checker } = json(root, "tools/upstream-lock.json");
  if (checker?.revision !== adoption.revision || checker?.version !== adoption.packages["@scientific-method/standard-checker"]) throw new Error("Toolkit adoption differs from the checker the upstream lock pins");
  const manifest = json(root, "package.json"), lock = text(root, "pnpm-lock.yaml");
  for (const name of names) {
    const version = adoption.packages[name], quoted = escape(`'${name}`);
    const importer = lock.match(new RegExp(`\\n {6}${quoted}':\\r?\\n {8}specifier: (\\S+)\\r?\\n {8}version: (\\S+)\\r?\\n`));
    if (!/^\d+\.\d+\.\d+$/.test(version) || manifest.devDependencies?.[name] !== version || importer?.[1] !== version || importer?.[2] !== version) throw new Error(`Toolkit version is not exactly locked: ${name}`);
    const resolution = lock.match(new RegExp(`\\n {2}${quoted}@${escape(version)}':\\r?\\n {4}resolution: \\{integrity: ([^}\\s]+)\\}`));
    if (!/^sha512-[A-Za-z0-9+/]+={0,2}$/.test(resolution?.[1] ?? "")) throw new Error(`Missing package integrity: ${name}`);
    let installed;
    try { installed = json(root, `node_modules/${name}/package.json`); }
    catch { throw new Error(`Install locked toolkit packages with pnpm install --frozen-lockfile: ${name}`); }
    if (installed.version !== version || installed.name !== name) throw new Error(`Installed toolkit differs from lock: ${name}`);
  }
  const requirements = text(root, "requirements-evidence.txt");
  if (!/^\d+\.\d+\.\d+$/.test(adoption.engine ?? "") || !requirements.includes(`scientific-method-engine==${adoption.engine} \\`)) throw new Error("Engine requirement differs from toolkit adoption");
  return adoption;
}
const direct = (() => { try { return process.argv[1] && import.meta.url === pathToFileURL(realpathSync(process.argv[1])).href; } catch { return false; } })();
if (direct) {
  try { if (process.argv.length !== 2) throw new Error("Usage: node tools/Verify-ToolkitPackages.mjs"); const pin = verifyToolkitPackages(); console.log(`Toolkit packages verified at ${pin.revision}; engine ${pin.engine}`); }
  catch (error) { console.error(error.message); process.exitCode = 1; }
}
