@ECHO OFF
SETLOCAL

IF %1x == x GoTo Help

SET VERSION=%1

CD /D %~dp0

IF "%VisualStudioVersion%" != "18.0" CALL C:\Util\vc26vars64.bat

CALL .\makebuild.cmd Release AnyCPU
IF ERRORLEVEL 1 GoTo TheEnd
CALL .\makebuild.cmd Release x86
IF ERRORLEVEL 1 GoTo TheEnd
CALL .\makebuild.cmd Release x64
IF ERRORLEVEL 1 GoTo TheEnd
CALL .\makebuild.cmd Release arm64
IF ERRORLEVEL 1 GoTo TheEnd

CALL .\makenuget.cmd %VERSION%
IF ERRORLEVEL 1 GoTo TheEnd

CALL .\makezip.cmd %VERSION%
IF ERRORLEVEL 1 GoTo TheEnd

CALL .\makemsix.cmd %VERSION% x86
IF ERRORLEVEL 1 GoTo TheEnd
CALL .\makemsix.cmd %VERSION% x64
IF ERRORLEVEL 1 GoTo TheEnd
CALL .\makemsix.cmd %VERSION% arm64
IF ERRORLEVEL 1 GoTo TheEnd

ECHO.
ECHO IT'S ALIVE!!!

GoTo TheEnd

:Help
ECHO Usage: MAKEALL version

:TheEnd
ENDLOCAL
