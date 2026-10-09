#!/usr/bin/env node
// Compatibility CLI over the pinned shared decoder. BLD-GOG-EN identifies the source.
import {readFileSync,writeFileSync,statSync} from 'node:fs';
import {resolve} from 'node:path';
import {pathToFileURL} from 'node:url';
import {sourceXxh3,readerTool} from '@scientific-method/executable-reader';
import {unpack,MAX_PACKED_BYTES} from '@scientific-method/executable-reader/unpack';

export function unpackLzexe(input) {
  const result=unpack(input);
  return {bytes:result.bytes,info:{...result.header,
    packer:result.packer,layout:result.layout,tool:readerTool(),
    loadModule:result.loadModuleSize,compressedEnd:result.packed.stream.end,
    decompressorStart:result.packed.decompressor}};
}

if (import.meta.url===pathToFileURL(resolve(process.argv[1]??'')).href) {
  const [input,output,...extra]=process.argv.slice(2);
  if (!input || !output || extra.length) {
    console.error('Usage: node tools/evidence/unlzexe.mjs <packed.exe> <unpacked.exe>');
    process.exitCode=2;
  } else {
    try {
      const stat=statSync(input);
      if (!stat.isFile() || stat.size>MAX_PACKED_BYTES) throw new Error('Input must be a file of at most 1 MiB');
      const {bytes,info}=unpackLzexe(readFileSync(input));
      writeFileSync(output,bytes,{flag:'wx'});
      console.log(JSON.stringify({size:bytes.length,xxh3:sourceXxh3(bytes),...info}));
    } catch(error) {
      console.error(error.message);
      process.exitCode=1;
    }
  }
}
