# sprite_set

A loaded CSF file, a 16-byte record: `+0x00` `count`, the number of frames, `UINT32`; `+0x04` `data`, the file's bytes, FMT-MEDIA-001; `+0x08` `rows`, for each frame the list of its row starts; `+0x0C` `frames`, the start of each frame in `data` [FND-MEDIA-001].
