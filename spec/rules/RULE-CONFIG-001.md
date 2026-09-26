---
id: RULE-CONFIG-001
title: Finding and loading CONQUER.INI
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-CONFIG-001, FND-CONFIG-002]
conflicting: []
split_with: []
related: [RULE-CONFIG-002, RULE-RES-004, RULE-SOUND-003, FMT-CONFIG-001, FMT-CONFIG-002]
---

## Summary

At startup the game finds `CONQUER.INI`, reads every line that has a key into a list of key and
value pairs, and then takes the archive paths and the switches from it. A missing file, or a line
without `=`, ends the program.

## When it runs

Once, early in startup, before the resource archive is opened.

## Parameters

None.

## Inputs

The environment variables `SORCERY` and `PATH`, the directory the program was started from, and the
files on disk.

## Procedure

```text
define start_config():
    ini_path = find_ini_file(sprintf("CONQUER.INI"))
    if ini_path == 0:
        # the game stops with fatal error 0 and "Conq-INI: File not found. Aborting."
        return
    load_ini(ini_path)
    set_archive_paths()
    # then the seven switches are read as read_sound_options reads them

define find_ini_file(name: char[]) -> char[]:
    # the first of these paths where the file exists is returned, and 0 when none holds it:
    #   the directory in the environment variable SORCERY, as sprintf("%s/%s", directory, name)
    #   sprintf("./%s", name), in the current directory
    #   the directory the program was started from, up to its last "\"
    #   each directory of the environment variable PATH, split at spaces, tabs and ";"
    return 0

define load_ini(path: char[]):
    free_ini()
    # a file that cannot be opened stops the game with fatal error 2 and the path
    let file = read_file(path, FMT-CONFIG-001)
    for each line in file.lines:
        let entry = parse_ini_line(line)
        if entry != 0:
            append(ini_entries, entry)
    if ini_cleanup_registered == 0:
        ini_cleanup_registered = 1
        # free_ini is registered to run when the program exits

define parse_ini_line(line: char[]) -> FMT-CONFIG-002:
    # leading white space is skipped; an empty line, or one starting with "#" or ";", gives 0
    # the key runs to the first "="; a line without one stops the game with fatal error 15 and
    # "Bad cfg line, missing '='"
    # white space before the "=" is dropped, as is white space after it and at the line's end
    let entry = new FMT-CONFIG-002
    # entry.key and entry.value are set to copies of the two parts
    return entry

define free_ini():
    ini_entries = []
```

## Outputs

`ini_path` and `ini_entries`. Each fatal error prints its message and ends the program with exit code
100 plus its number: 100 for a missing file, 102 for a file that cannot be opened, 101 when memory
runs out, 103 when the second pass cannot seek, and 115 for a line without `=`.

## Edge cases

- Under the GOG configuration the current directory is `C:\`, so the installed file is read and
  the disc's copy never is.
- A file of only comments and blank lines loads with no entries, and every key reads as missing.
- A line longer than 1,023 bytes is read as two lines, and the second must hold `=` too.

## What the sources say

None of the sources describe this. The manual does not mention `CONQUER.INI`.

## Differences between builds

None known.

## Open questions

- How the search walks `PATH` after its first directory, and the flags of the existence test.
- Whether an empty file makes the list start at an unfilled node.
