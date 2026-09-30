@echo off
setlocal
cd /d "%~dp0"

set "DOTNET_EXE="
if exist "%USERPROFILE%\.dotnet\dotnet.exe" set "DOTNET_EXE=%USERPROFILE%\.dotnet\dotnet.exe"
if not defined DOTNET_EXE for %%D in (dotnet.exe) do set "DOTNET_EXE=%%~$PATH:D"
if not defined DOTNET_EXE (
  echo The .NET SDK is required. Install it from https://dotnet.microsoft.com/download
  pause
  exit /b 1
)

set "DOTNET_CLI_HOME=%~dp0.dotnet-home"
set "NUGET_PACKAGES=%~dp0.dotnet-home\packages"
set "DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1"
set "DOTNET_CLI_TELEMETRY_OPTOUT=1"
set "DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0"
set "TESTINGPLATFORM_TELEMETRY_OPTOUT=1"
for %%D in ("%DOTNET_EXE%") do set "PATH=%%~dpD;%PATH%"
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File tools\Invoke-Validation.ps1 %*
exit /b %ERRORLEVEL%
