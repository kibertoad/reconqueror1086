@echo off
setlocal
cd /d "%~dp0"

where dotnet >nul 2>&1
if errorlevel 1 (
    echo The .NET SDK is required to run Conqueror A.D. 1086.
    echo Install the .NET 10 SDK, then run this file again.
    pause
    exit /b 1
)

echo Starting Conqueror A.D. 1086...
set "CONQUEROR_USER_CONTENT=%~dp0UserContent"
dotnet run --project "src\Conqueror.Game\Conqueror.Game.csproj"
if errorlevel 1 (
    echo.
    echo The game could not be started. Review the build error above.
    pause
    exit /b 1
)

endlocal
