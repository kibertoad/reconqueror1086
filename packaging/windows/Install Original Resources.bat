@echo off
setlocal
cd /d "%~dp0"
set "SOURCE=%~1"
if "%SOURCE%"=="" set "SOURCE=C:\GOG Games\Conqueror AD1086"
"%~dp0Tools\Conqueror.Import.exe" "%SOURCE%" "%~dp0UserContent"
if errorlevel 1 (
    echo.
    echo Resource installation failed. The original installation was not modified.
    pause
    exit /b 1
)
echo.
echo Owned resources are installed locally in UserContent.
pause
