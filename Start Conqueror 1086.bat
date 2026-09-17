@echo off
setlocal
cd /d "%~dp0"

set "DOTNET_EXE="
if exist "%USERPROFILE%\.dotnet\dotnet.exe" set "DOTNET_EXE=%USERPROFILE%\.dotnet\dotnet.exe"
if not defined DOTNET_EXE for %%D in (dotnet.exe) do set "DOTNET_EXE=%%~$PATH:D"
if not defined DOTNET_EXE (
    echo The .NET SDK is required to run Conqueror A.D. 1086.
    echo Install the .NET 10 SDK, then run this file again.
    pause
    exit /b 1
)

set "RECONQUEROR_USER_CONTENT=%~dp0UserContent"
echo Building Conqueror A.D. 1086...
rem A source archive or branch update can leave an older bin output with a newer
rem timestamp than the restored source files. Force compilation so dotnet never
rem launches that stale game assembly.
"%DOTNET_EXE%" build "src\Conqueror.Game\Conqueror.Game.csproj" --no-incremental
if errorlevel 1 (
    echo.
    echo Conqueror A.D. 1086 could not be built. Review the build error above.
    pause
    exit /b 1
)

echo Starting Conqueror A.D. 1086...
"%DOTNET_EXE%" run --no-build --project "src\Conqueror.Game\Conqueror.Game.csproj" -- %*
if errorlevel 1 (
    echo.
    echo The game could not be started. Review the build error above.
    pause
    exit /b 1
)

endlocal
