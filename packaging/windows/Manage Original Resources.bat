@echo off
setlocal
cd /d "%~dp0"
if /i "%~1"=="verify" goto verify
if /i "%~1"=="repair" goto repair
if /i "%~1"=="uninstall" goto uninstall
echo Usage: %~nx0 verify ^| repair [original installation] ^| uninstall
exit /b 2

:verify
"%~dp0Tools\Conqueror.Import.exe" --verify "%~dp0UserContent"
exit /b %ERRORLEVEL%

:repair
set "SOURCE=%~2"
if "%SOURCE%"=="" set "SOURCE=C:\GOG Games\Conqueror AD1086"
"%~dp0Tools\Conqueror.Import.exe" --repair "%SOURCE%" "%~dp0UserContent"
exit /b %ERRORLEVEL%

:uninstall
"%~dp0Tools\Conqueror.Import.exe" --uninstall "%~dp0UserContent"
exit /b %ERRORLEVEL%
