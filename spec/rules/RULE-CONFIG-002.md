---
id: RULE-CONFIG-002
title: Reading and writing a setting
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-CONFIG-002]
conflicting: []
split_with: []
related: [RULE-CONFIG-001, FMT-CONFIG-001, FMT-CONFIG-002]
---

## Summary

A setting is read as the value of the first line whose key matches exactly, or 0 when there is
none. Writing a setting changes every line with that key and rewrites the whole file from the
list, dropping comments and blank lines; a key the file lacks is never added.

## When it runs

`ini_value` whenever a rule reads a setting. `set_ini_value` when the options screen toggles a
switch, and when the first-person view stores its window size.

## Parameters

- `key`: the key, a `char[]`.
- `value`: the new value, a `char[]`.
- `text`: a value to convert.

## Inputs

`ini_entries` and `ini_path` from RULE-CONFIG-001.

## Procedure

```text
define ini_value(key: char[]) -> char[]:
    for each entry in ini_entries:
        if entry.key == key:
            return entry.value
    return 0

define set_ini_value(key: char[], value: char[]):
    # a file that cannot be opened for writing stops the game with fatal error 5 and the path
    for each entry in ini_entries:
        if entry.key == key:
            entry.value = copy(value)
    # the file at ini_path is replaced by sprintf("%s=%s\n", entry.key, entry.value) for each
    # entry in ini_entries, in order, written in text mode

define decimal_value(text: char[]) -> INT32:
    # as C's atoi: white space, an optional sign, then decimal digits up to the first other byte
    return 0
```

## Outputs

The value, or 0; the rewritten file.

## Edge cases

- Keys and values match only in the same case: `on` does not turn a switch on.
- A key present twice is read from its first line and changed on both.
- A setting the file lacks cannot be saved: toggling it in the options screen changes the running
  game only.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

None.
