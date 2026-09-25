# VIEW

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-VIEW-001` | Scene block definition, one record of the Blocks resource | supported | partial | None | None | supported | Behaviour and colour are read as dwords that include the neighbouring fields; unk_3C and unk_44 are not read. |
| `FMT-VIEW-002` | Scene map, the Map resource | supported | complete | None | None | implemented | None |
| `FMT-VIEW-003` | Scene start position, the Viewer resource | supported | complete | None | None | implemented | None |
| `FMT-VIEW-004` | Scene settings, the Scenario resource | supported | complete | None | None | implemented | None |
| `FMT-VIEW-005` | Backdrop descriptor, the Backdrop resource | supported | complete | None | None | implemented | None |
| `FMT-VIEW-006` | Backdrop pixels, the BackImage resource | supported | complete | None | None | implemented | None |
| `FMT-VIEW-007` | Colour map, one of the Pal0 to Pal127 resources | supported | complete | None | None | implemented | None |
| `FMT-VIEW-008` | Scene texture, a TEX resource | supported | complete | None | None | implemented | None |
| `FMT-VIEW-009` | Combat palette, 256 colours of three bytes | supported | complete | None | None | implemented | None |
| `RULE-VIEW-001` | Heading from one map point to another | supported | complete | None | None | implemented | None |
| `RULE-VIEW-002` | Integer sine, cosine and rotation | supported | complete | None | None | implemented | None |
| `RULE-VIEW-003` | Cast a ray through the scene and find the first opaque surface at a view row | supported | partial | None | None | supported | y is the major axis on a tie, chained targets use their own offsets, and depths are clamped to 0x10; these are guesses. |
| `RULE-VIEW-004` | Surface intersection, depth and pixel test inside the raycaster | unknown | partial | None | None | unknown | The intersection, depth, rounding and texture-row steps are the rebuild's own. |
| `RULE-VIEW-005` | Draw the backdrop | supported | partial | None | None | supported | The view turns in four cardinal facings, and the wrapped remainder of a row is not copied. |
| `RULE-VIEW-006` | Build a scene's distance colour maps | supported | complete | None | None | implemented | None |
| `RULE-VIEW-007` | Colour map of a drawn surface | supported | partial | None | None | supported | A scene with colour maps switched off is rejected instead of drawn unshaded; every shipped scene switches them on. |
