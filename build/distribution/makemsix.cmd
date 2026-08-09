@ECHO Off
SETLOCAL

IF %1x == x GoTo Help

SET VERSION="%1"
SET ARCH="%2"

ECHO === MakeMSIX Version=%VERSION% Architecture=%ARCH% ===

SET ROOTDIR=%~dp0..\..
SET BINDIR=%ROOTDIR%\bin\Release\%ARCH%
SET BINEXE=%BINDIR%\AppData\net10.0-windows10.0.17763.0\win-%ARCH%\native\appdata.exe
SET TARGETDIR=%ROOTDIR%\Release
SET TARGET=%TARGETDIR%\AppData-%VERSION%-%ARCH%.msix

SET SCRATCH="%TEMP%\appxdata-temp-msix"
IF EXIST %SCRATCH% RD /s/q %SCRATCH%
MD %SCRATCH% 2>&1 >NUL
MD %SCRATCH%\docs\images 2>&1 >NUL
COPY %BINEXE% %SCRATCH%\*
COPY %ROOTDIR%\build\distribution\appxmanifest.xml %SCRATCH%\*
pwsh.exe -NoProfile -Command "$p='%SCRATCH%\appxmanifest.xml'; $s=[IO.File]::ReadAllText($p); [IO.File]::WriteAllText($p,$s.Replace('$version$','%VERSION%').Replace('$architecture$','%ARCH%'))"
COPY %ROOTDIR%\docs\images\DownloadFromTheMicrosoftStore.png %SCRATCH%\docs\images\*
COPY %ROOTDIR%\LICENSE %SCRATCH%\*
COPY %ROOTDIR%\README.md %SCRATCH%\*
COPY %ROOTDIR%\README.html %SCRATCH%\*
COPY %ROOTDIR%\src\appdata-48x48.png %SCRATCH%\*
COPY %ROOTDIR%\src\appdata-100x100.png %SCRATCH%\*

SET MAKEAPPX=makeappx.exe
SET MAKEAPPX_OPTS=pack /v /o /d %SCRATCH% /p %TARGET%
IF NOT EXIST %TARGETDIR% MD %TARGETDIR% 2>&1 >NUL
IF EXIST %TARGET% DEL /Q %TARGET%
ECHO %MAKEAPPX% %MAKEAPPX_OPTS%
%MAKEAPPX% %MAKEAPPX_OPTS%
IF ERRORLEVEL 1 GOTO TheEnd

GoTo TheEnd

:Help
ECHO Usage: MAKEMSIX version architecture
ECHO        version = version (1.2.3)
ECHO   architecture = x86 or x64 OR arm64

:TheEnd
ENDLOCAL
