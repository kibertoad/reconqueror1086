# Orphan cleanup log

- 2026-09-13T12:48:56+03:00 — Stopped PID 10172 (`powershell.exe`), started
  2026-09-13 12:46:55 local time, after its repository/Ghidra recursive search
  remained active beyond the completed analysis command. Its command line
  targeted `analysis\original\ghidra-project`, so repository ownership was
  confirmed. Reusable `dotnet` MSBuild nodes using `/nodeReuse:true` were
  deliberately left running because repository policy exempts them.

- 2026-09-17T22:58:12+03:00 - Stopped PID 35732 (`cmd.exe`), started
  2026-09-17 20:29:45 local time by FAR Manager for
  `C:\GOG Games\reimp\Start Conqueror 1086.bat`. The failed game process had
  exited and the launcher shell remained childless at its error pause. Reusable
  `dotnet` MSBuild nodes using `/nodeReuse:true` were deliberately left running;
  active Ghidra and inspector jobs targeting other game repositories were also
  left running because they do not belong to this work.
