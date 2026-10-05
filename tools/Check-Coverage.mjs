#!/usr/bin/env node
// Conqueror's LE address adapter; the shared MZ/FBOV source reader is not used.
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { resolve } from 'node:path';
import { verifyInventory, verifySegmentedInventory, inventoryPath } from './evidence/inventory.mjs';
const root = fileURLToPath(new URL('..', import.meta.url));
// BLD-GOG-EN and FND-RES-009 identify this mapping. Inventory rows use LE
// addresses, not physical source offsets. No relocation or behavior is inferred.
const path = 'coverage/BLD-GOG-EN/@CD/CONQUER.EXE.tsv';
const result = verifyInventory({ranges:[{start:0x10000,end:0x8cb9e}]},
  'BLD-GOG-EN', 'CD:CONQUER.EXE', readFileSync(resolve(root,path),'utf8'), path);
console.log(`Committed LE inventory identity and metadata verified: ${result.destination}`);

const launcherMetadata = JSON.parse(readFileSync(resolve(root, 'coverage/launcher-inventories.json'), 'utf8'));
if (launcherMetadata.schemaVersion !== 1 || launcherMetadata.build !== 'BLD-GOG-EN') throw new Error('Invalid launcher inventory metadata');
for (const launcher of launcherMetadata.inventories) {
  const destination = inventoryPath(launcherMetadata.build, launcher.manifest);
  const text = readFileSync(resolve(root, destination), 'utf8');
  if (launcher.format === 'PE') verifyInventory({ ranges: launcher.ranges }, launcherMetadata.build, launcher.manifest, text, destination);
  else if (launcher.format === 'NE') verifySegmentedInventory(launcherMetadata.build, launcher.manifest, text, launcher.ranges, destination);
  else throw new Error('Unsupported launcher inventory format');
  console.log(`Launcher inventory metadata verified: ${destination}`);
}