@echo off
setlocal
cd /d "%~dp0"

set "DOTNET_EXE="
for %%D in (dotnet.exe) do set "DOTNET_EXE=%%~$PATH:D"
if not defined DOTNET_EXE (
    echo The .NET SDK is required to run Conqueror A.D. 1086.
    echo Install the .NET 10 SDK, then run this file again.
    pause
    exit /b 1
)

echo Starting Conqueror A.D. 1086...
set "RECONQUEROR_USER_CONTENT=%~dp0UserContent"
"%DOTNET_EXE%" run --project "src\Conqueror.Game\Conqueror.Game.csproj"
if errorlevel 1 (
    echo.
    echo The game could not be started. Review the build error above.
    pause
    exit /b 1
)

endlocal
