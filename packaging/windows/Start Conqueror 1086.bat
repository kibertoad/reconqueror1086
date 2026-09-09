@echo off
setlocal
cd /d "%~dp0"
set "RECONQUEROR_USER_CONTENT=%~dp0UserContent"
"%~dp0Game\Conqueror.Game.exe"
if errorlevel 1 (
    echo.
    echo Conqueror A.D. 1086 exited with an error.
    pause
    exit /b 1
)
