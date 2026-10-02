---
id: SRC-BMP-REFERENCE
title: Microsoft bitmap file header, information header and palette reference
superseded_by: []
author: Microsoft
date: "2026-10-02"
location: https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapfileheader
xxh3: null
licence: null
---

## Use

The official file-header reference supplies field roles for a DIB file.
The [information-header reference](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapinfoheader)
describes dimensions, uncompressed indexed color tables, positive-height row
orientation and DWORD-aligned stride. The
[palette reference](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-rgbquad)
gives the four-byte blue, green, red and reserved entry order.
Checked on the date above. These references interpret the matching stored fields
in FND-RES-015 / FMT-RES-009; they are not evidence that a shipped application
uses a particular API or accepts every variant covered by the references.

## Known errors

None identified for the uncompressed 8-bit case used here. The information-header
page also discusses video semantics; those paths are outside this use. Current
API availability requirements do not establish the age or behavior of the owned
file's consumer.
