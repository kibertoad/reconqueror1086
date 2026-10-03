# Shared runtime migration

Versioned JSON settings, shared atomic content writes and cached neutral controller bindings.

The migration uses published `1.4.0` packages from nuget.org for shared paths,
persistence and input bindings. Package locks identify the public release.
Game bindings use the released `InputBindings.Create` factory.

Game save payloads, version admission, slot naming, defaults, audio routing, fades, voice limits
and control policies remain local. Synthetic checks establish migration behavior, not parity
with the original game or live device behavior. No original assets entered this change.
