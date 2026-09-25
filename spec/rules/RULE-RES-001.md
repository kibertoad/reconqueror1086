---
id: RULE-RES-001
title: Opening, searching, reading and writing the open archive
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-RES-001, FND-RES-002, FND-RES-005]
conflicting: []
split_with: []
related: [RULE-RES-002, RULE-RES-003, RULE-RES-004, FMT-RES-001, FMT-RES-002]
---

## Summary

The game has one archive open at a time. Opening one reads its directory into memory; an entry is
found by name, ignoring ASCII case, or by its index, and read by decoding its stored bytes with the
routine its kind names. The same routines create an archive and add entries to it, compressing each
with the kind asked for.

## When it runs

Whenever the game loads a resource, and when it saves a game (FND-RES-005). RULE-RES-004 gives which
archive is open when.

## Parameters

`open_archive` takes the path and `writing`, 1 for the modes `w+b` and `wb` and 0 for `rb`.
`close_archive` takes `sort`. `find_entry` takes a name, `entry_at` an index, `read_entry` a record
and the output list, and `add_entry` a name, a kind, the value for `unk_24`, a size and the bytes.

## Inputs

`archive_file`, `archive_path`, `archive_data_end`, `archive_changed`, `archive_count` and
`archive_directory`.

## Procedure

```text
define open_archive(path: char[], writing: INT32) -> INT32:
    # a path with no "." first gets the extension ".RES" appended, in the caller's buffer
    archive_path = path
    if writing != 0:
        # archive_file becomes a new file holding the four bytes ".RES" and the UINT32LE 8
        archive_data_end = 8
        archive_changed = 1
        archive_count = 0
        return archive_count
    # archive_file becomes the file at path; a failed open returns -1
    let f = read_file(path, FMT-RES-001)
    # a file shorter than its header or directory, or without ".RES", returns -1
    archive_data_end = f.directory_offset
    archive_count = f.entry_count
    archive_directory = f.directory
    archive_changed = 0
    return archive_count

define close_archive(sort: INT32):
    if archive_file == 0:
        return
    if archive_changed != 0 or sort != 0:
        if sort != 0:
            # sorts archive_directory with qsort, comparing names_differ(a.name, b.name, 31)
        # writes archive_data_end at byte 4 and, at archive_data_end, archive_count
        # followed by archive_directory
    # closes archive_file
    archive_file = 0
    archive_directory = []

define lower_ascii(c: UINT8) -> UINT8:
    if c >= 0x41 and c <= 0x5A:
        return c + 0x20
    return c

define names_differ(a: char[], b: char[], limit: INT32) -> INT32:
    let i = 0
    while i < limit:
        let x = lower_ascii(a[i])
        let y = lower_ascii(b[i])
        if x != y:
            return x - y
        if y == 0:
            return 0
        i = i + 1
    return 0

define find_entry(name: char[]) -> FMT-RES-002:
    for i in 0..archive_count:
        if names_differ(name, archive_directory[i].name, 0x7FFFFFFF) == 0:
            return archive_directory[i]
    return 0

define entry_at(i: UINT32) -> FMT-RES-002:
    # i is not checked against archive_count
    return archive_directory[i]

define read_entry(e: FMT-RES-002, out: UINT8[]) -> UINT32:
    if e == 0:
        return 0
    let f = read_file(archive_path, FMT-RES-001)
    let src: UINT8[] = []
    for i in 0..e.stored_size:
        append(src, f.data[e.offset - 8 + i])
    if e.kind == STORAGE_PLAIN:
        for i in 0..e.stored_size:
            append(out, src[i])
    else if e.kind == STORAGE_BLOCKS:
        decode_kind1(src, e.stored_size, out)
    else if e.kind == STORAGE_LZW:
        decode_kind2(src, e.stored_size, out)
    else if e.kind == STORAGE_KIND_3:
        fn_000491D4(src, out, e.stored_size, e.expanded_size)
    else:
        return 0
    # a failed seek or short read returns 0 instead
    return e.expanded_size

define add_entry(name: char[], kind: UINT32, extra: UINT32, size: UINT32, bytes: UINT8[]) -> INT32:
    if find_entry(name) != 0:
        return 0
    let e = new FMT-RES-002
    # e.name takes at most the first 31 characters of name
    e.name = name
    e.kind = kind
    e.unk_24 = extra
    e.expanded_size = size
    e.offset = archive_data_end
    append(archive_directory, e)
    archive_count = archive_count + 1
    let stored = size
    if kind == 1:
        stored = fn_00048090(bytes, size)
    else if kind == 2:
        stored = fn_0004830C(bytes, size)
    else if kind == 3:
        stored = fn_00049124(bytes, size)
    else if kind != 0:
        return 0
    if stored == 0:
        e.kind = 0
        stored = size
    # writes the stored bytes, the encoder's output or bytes itself, at archive_data_end;
    # a failed write returns 0
    e.stored_size = stored
    archive_data_end = archive_data_end + stored
    archive_changed = 1
    return 1
```

## Outputs

`open_archive` returns the entry count, or -1. `find_entry` and `entry_at` return a record of
`archive_directory`. `read_entry` fills `out` and returns the expanded size, or 0 when it failed.
`add_entry` returns 1, or 0 when the name is taken or the write failed. `close_archive` writes the
directory when the archive changed or `sort` is not 0.

## Edge cases

- `read_entry` returns the record's expanded size whatever the decoder wrote. It chooses the decoder
  by `kind` alone, so a kind-1 entry whose stored and expanded sizes are equal, as `Pal102` of
  `BAR0` is, is still decoded.
- The decoders write past `out` if the caller's buffer is shorter than the expanded size; callers
  allocate `expanded_size` bytes.
- `open_archive` does not close an archive that is already open; its callers close it first.
- A failed read of a kind-1, 2 or 3 entry leaves the staging buffer allocated.
- The compare in `close_archive`'s sort looks at the first 31 characters only, and `qsort` leaves
  equal names in no defined order; no writer found sorts.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_000491D4` decodes, and what `fn_00048090`, `fn_0004830C` and `fn_00049124` produce beyond
  the formats FMT-RES-003 and FMT-RES-004 give; the encoders' choices between equal encodings are
  not recorded.
