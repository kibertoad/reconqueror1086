@echo off
setlocal
cd /d "%~dp0"
set "SOURCE=%~1"
if "%SOURCE%"=="" set "SOURCE=C:\GOG Games\Conqueror AD1086"
"%~dp0Tools\Conqueror.Import.exe" "%SOURCE%" "%~dp0UserContent"
if errorlevel 1 (
    echo.
    echo Resource import failed. Your original Conqueror installation was not modified.
    pause
    exit /b 1
)
echo.
echo Art, music, sound, video, and other resources from your legal copy are installed in UserContent.
pause
