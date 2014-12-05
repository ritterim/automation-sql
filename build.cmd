@echo Off
pushd %~dp0
setlocal enabledelayedexpansion

set PROGRAMSROOT=%PROGRAMFILES%
if defined PROGRAMFILES(X86) set PROGRAMSROOT=%PROGRAMFILES(X86)%

:: Find the highest version of MSBuild installed on the system, or default to the version installed with .NET v4.0.
set MSBUILD_VERSION=0
for /D %%x in ("%PROGRAMSROOT%\MSBuild\*.0") do (
  set "FN=%%~nx"
  if !FN! gtr !MSBUILD_VERSION! set MSBUILD_VERSION=!FN!
)

set MSBUILD="%PROGRAMSROOT%\MSBuild\%MSBUILD_VERSION%.0\Bin\MSBuild.exe"
if not exist %MSBUILD% set MSBUILD="%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

set CACHED_NUGET=%LOCALAPPDATA%\NuGet\NuGet.exe
if exist %CACHED_NUGET% goto :CopyNuGet

echo Downloading latest version of NuGet.exe...
if not exist %LOCALAPPDATA%\NuGet @md %LOCALAPPDATA%\NuGet
@powershell -NoProfile -ExecutionPolicy Unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"

:CopyNuGet
if exist .nuget\nuget.exe goto :Build
if not exist .nuget @md .nuget
@copy %CACHED_NUGET% .nuget\nuget.exe > nul

:Build
%MSBUILD% build\Build.proj /m /v:m %* /fl /flp:LogFile=msbuild.log;Verbosity=Detailed /nr:false /nologo

if %ERRORLEVEL% neq 0 goto :BuildFail

:BuildSuccess
echo.
echo *** BUILD SUCCEEDED ***
goto End

:BuildFail
echo.
echo *** BUILD FAILED ***
goto End

:End
echo.
popd
exit /B %ERRORLEVEL%
