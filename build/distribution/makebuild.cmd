@ECHO Off
SETLOCAL

IF %1x == x GoTo Help
IF %2x == x GoTo Help

SET CONFIG="%1"
SET ARCH="%2"
IF %2 == AnyCPU SET ARCH=Any CPU

ECHO => MakeBuild Configuration=%CONFIG% Architecture=%ARCH%

CD /D %~dp0..\..

IF "%VisualStudioVersion%" != "18.0" CALL C:\Util\vc26vars64.bat

ECHO dotnet publish AppData.sln -c %CONFIG% -p:Platform="%ARCH%"
dotnet publish AppData.sln -c %CONFIG% -p:Platform="%ARCH%"
IF ERRORLEVEL 1 GoTo TheEnd

GoTo TheEnd

:Help
ECHO Usage: MAKEBUILD configuration architecture
ECHO   configuration = Debug or Release
ECHO    architecture = Any CPU or x64 OR arm64

:TheEnd
ENDLOCAL
