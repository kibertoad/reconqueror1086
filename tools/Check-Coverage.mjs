#!/usr/bin/env node
// Conqueror's LE address adapter; the shared MZ/FBOV source reader is not used.
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { resolve } from 'node:path';
import { verifyInventory } from './evidence/inventory.mjs';
const root = fileURLToPath(new URL('..', import.meta.url));
// BLD-GOG-EN and FND-RES-009 identify this mapping. Inventory rows use LE
// addresses, not physical source offsets. No relocation or behavior is inferred.
const path = 'coverage/BLD-GOG-EN/@CD/CONQUER.EXE.tsv';
const result = verifyInventory({ranges:[{start:0x10000,end:0x8cb9e}]},
  'BLD-GOG-EN', 'CD:CONQUER.EXE', readFileSync(resolve(root,path),'utf8'), path);
console.log(`Committed LE inventory identity and metadata verified: ${result.destination}`);
