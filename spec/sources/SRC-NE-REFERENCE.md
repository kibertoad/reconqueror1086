---
id: SRC-NE-REFERENCE
title: Open Watcom NE container structures and relocation constants
superseded_by: []
author: Open Watcom contributors
date: "2026-10-05"
location: https://github.com/open-watcom/open-watcom-v2/blob/master/bld/watcom/h/exeos2.h
xxh3: null
licence: Sybase Open Watcom Public License 1.0
---

## Use

Checked on the date above. The maintained container header supplies NE segment
flags and relocation source/target constants. These interpret SETUP metadata in
FND-RES-025, not the application behavior or a loaded runtime selector.
No header code is copied into the research description.

## Known errors

None identified for these constants. A relocation table alone does not identify
all references to a fixed data offset or prove a call's arguments.
