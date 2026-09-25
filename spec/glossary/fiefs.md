# fiefs

The fief records, reached through the list of pointers at `+0x08` of the record the pointer at `0x0009ABEC` points to; record 0 is the home fief. The rules use `wealth` at `+0x04`, `last_month` at `+0x08`, `july_done` at `+0x0C`, `serfs` at `+0x1C` and `months_broke` at `+0x20`, all `INT32`, and `list_28`, `list_2C`, `list_30` and `list_34`, pointers at `+0x28`, `+0x2C`, `+0x30` and `+0x34` to lists of `INT32` words, read as rows of 8, 11, 8 and 10 words [FND-ESTATE-003].
