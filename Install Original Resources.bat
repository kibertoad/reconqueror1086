@echo off
setlocal
cd /d "%~dp0"
set "SOURCE=%~1"
if "%SOURCE%"=="" set "SOURCE=C:\GOG Games\Conqueror AD1086"
where dotnet >nul 2>&1
if errorlevel 1 (
    echo The .NET 10 SDK is required.
    pause
    exit /b 1
)
echo Installing owned resources from "%SOURCE%"...
dotnet run --project "tools\Conqueror.Import\Conqueror.Import.csproj" -- "%SOURCE%" "%~dp0UserContent"
if errorlevel 1 (
    echo Resource installation failed. No files in the original installation were changed.
    pause
    exit /b 1
)
echo Resources installed. They remain local and are excluded from Git.
pause
