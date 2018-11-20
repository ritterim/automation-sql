@echo Off
pushd %~dp0
setlocal enabledelayedexpansion

set PROGRAMSROOT=%PROGRAMFILES%
if defined PROGRAMFILES(X86) set PROGRAMSROOT=%PROGRAMFILES(X86)%

set CACHED_NUGET=%LOCALAPPDATA%\NuGet\NuGet.exe
if exist "%CACHED_NUGET%" goto CopyNuGet

echo Downloading latest version of NuGet.exe...
if not exist "%LOCALAPPDATA%\NuGet" @md "%LOCALAPPDATA%\NuGet"
@powershell -NoProfile -ExecutionPolicy Unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"

:CopyNuGet
echo Copying NuGet...
if exist .nuget\nuget.exe goto :Build
if not exist .nuget @md .nuget
@copy "%CACHED_NUGET%" .nuget\nuget.exe > nul

:Build
echo.
echo *** STARTING BUILD ***
echo.

dotnet build src/core/core.csproj --configuration release
if %ERRORLEVEL% neq 0 goto :BuildFail

echo.
echo *** BUILD SUCCEEDED ***
echo.

echo.
echo *** STARTING TESTS ***
echo.

dotnet test tests/core.tests/core.tests.csproj --configuration release
if %ERRORLEVEL% neq 0 goto :TestFail

echo.
echo *** TESTS SUCCEEDED ***
echo.

echo.
echo *** STARTING PACK ***
echo.

dotnet pack src/core/core.csproj --configuration release
if %ERRORLEVEL% neq 0 goto :PackFail

echo.
echo *** PACK SUCCEEDED ***
echo.
goto End

:BuildFail
echo.
echo *** BUILD FAILED ***
goto End

:TestFail
echo.
echo *** TEST FAILED ***
goto End

:PackFail
echo.
echo *** PACK FAILED ***
goto End

:End
echo.
popd
exit /B %ERRORLEVEL%
