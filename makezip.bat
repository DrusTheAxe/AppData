@ECHO Off
SETLOCAL

IF %1x == x GoTo Help

SET VER=%1

SET Z=C:\Util.W32\ZIP.EXE
SET ZOPTS=-9jDX
SET ZTARGET=bin\AppData-%VER%.zip
SET ROOTDIR=%~dp0
SET BINDIR=%ROOTDIR%AppData\bin\Release
SET DOCDIR=%ROOTDIR%docs
SET FILES="%BINDIR%\AppData.exe" "%BINDIR%\AppData.exe.config" CHANGELOG LICENSE "%DOCDIR%\README.txt" "%DOCDIR%\README.html"

SET _7ZDIR=C:\Util.W32\7-ZipPortable\App
SET _7Zx86=%_7ZDIR%\7-Zip\7z.exe
SET _7Zx64=%_7ZDIR%\7-Zip64\7z.exe
SET _7Z=%_7Zx86%
IF %PROCESSOR_ARCHITECTURE% == AMD64 SET _7Z=%_7Zx64%
SET _7ZOPTS=
SET _7ZTARGET=bin\AppData-%VER%.7z


IF EXIST %ZTARGET% DEL /Q %ZTARGET%
ECHO %Z% %ZOPTS% %ZTARGET% %FILES%
%Z% %ZOPTS% %ZTARGET% %FILES%
IF ERRORLEVEL 1 GOTO TheEnd

IF EXIST %_7ZTARGET% DEL /Q %_7ZTARGET%
ECHO %_7Z% a %_7ZOPTS% %_7ZTARGET% %FILES%
%_7Z% a %_7ZOPTS% %_7ZTARGET% %FILES%
IF ERRORLEVEL 1 GOTO TheEnd

GoTo TheEnd

:Help
ECHO Usage: MAKEZIP version

:TheEnd
ENDLOCAL
