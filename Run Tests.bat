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
set "TEST_ARTIFACTS=%TEMP%\reconqueror1086-tests-%RANDOM%-%RANDOM%"

powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File tools\Verify-Repository.ps1
if errorlevel 1 goto failed

"%DOTNET_EXE%" build tests\Conqueror.Tests\Conqueror.Tests.csproj --artifacts-path "%TEST_ARTIFACTS%" -m:1 -p:UseSharedCompilation=false -v:minimal
if errorlevel 1 goto failed
if not exist "%TEST_ARTIFACTS%\bin\Conqueror.Tests\debug\Conqueror.Tests.dll" (
  echo Test build did not produce Conqueror.Tests.dll.
  goto failed
)

"%DOTNET_EXE%" "%TEST_ARTIFACTS%\bin\Conqueror.Tests\debug\Conqueror.Tests.dll" -reporter verbose -noColor
if errorlevel 1 goto failed

"%DOTNET_EXE%" build tests\Conqueror.Specs\Conqueror.Specs.csproj --artifacts-path "%TEST_ARTIFACTS%" -m:1 -p:UseSharedCompilation=false -v:minimal
if errorlevel 1 goto failed
if not exist "%TEST_ARTIFACTS%\bin\Conqueror.Specs\debug\Conqueror.Specs.dll" (
  echo Specification build did not produce Conqueror.Specs.dll.
  goto failed
)

"%DOTNET_EXE%" "%TEST_ARTIFACTS%\bin\Conqueror.Specs\debug\Conqueror.Specs.dll"
if errorlevel 1 goto failed

echo.
echo Tests passed. Isolated build files are in:
echo %TEST_ARTIFACTS%
exit /b 0

:failed
echo.
echo Tests failed with exit code %ERRORLEVEL%.
exit /b %ERRORLEVEL%
