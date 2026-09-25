# army_records

The five army records, reached through the pointers `PTR32[5]` at `0x000A9CD0`. The rules use `name`, 80 bytes of text at `+0x00`; `units`, six rows of 92 bytes from `+0xA4`, each with `count`, `price` and `upkeep` as `INT32` at `+0x00`, `+0x04` and `+0x08`; and `on_map`, `INT32` at `+0x27C`. Only record 0's `price` and `upkeep` are read. Unit rows 0 to 2 are swordsmen, halberdiers and knights [FND-ESTATE-002].
