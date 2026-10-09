# rng_state

The state of `rng`, `UINT32`, kept in the global at `0x0009E044` [FND-RNG-003].

FND-RNG-004 identifies the loaded state through relocation targets and observes
the native seed write and first draw. Its runtime address is launch-specific.
