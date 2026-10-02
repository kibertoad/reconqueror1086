# Bounded evidence reporters

Use the installed toolkit packages through Conqueror's wrapper:

```sh
npm ci --ignore-scripts
python -m pip install --require-hashes -r requirements-evidence.txt
node tools/evidence/report.mjs x86-returns <local-config.json>
```

The toolkit's [reporter guide](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/blob/98395df03fab3990bca3a3de5cb1dcd1ae0df3a3/docs/bounded-evidence-reporters.md)
defines the report commands, controls, caps and limitations. The packaged reader
prepares MZ/FBOV queries and the engine reads instruction paths; the existing
Conqueror wrapper keeps its own inventory and review contracts.
Set `EVIDENCE_PYTHON` to select the Python interpreter with the locked engine.
The direct Python entry point is `python -m scientific_method_engine`.

Conqueror's DOS/16M-bound LE image is outside the generic MZ/FBOV/PE loader set.
Keep its existing LE mapping and committed-inventory adapter. This dependency
migration does not make generic loader reports evidence for LE behavior.
Original query inputs and reports stay local under `GAME_DIR`; use synthetic
state for CI. No evidence status changes follow from package adoption.
