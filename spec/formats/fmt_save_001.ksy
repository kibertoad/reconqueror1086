meta:
  id: fmt_save_001
  title: Saved game, SAVEGAME\CONQn.SAV
  license: MIT
  endian: le
doc-ref: FMT-SAVE-001
seq:
  - id: magic
    contents: '.RES'
  - id: directory_offset
    type: u4
    doc: As in fmt_res_001; the entries are TITLE, VERSION and the saved files.
