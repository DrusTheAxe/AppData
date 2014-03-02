@ECHO Off
SETLOCAL

SET P=C:\Python32\python.exe

ECHO %P% -m markdown --file=docs\README.html --output_format=html4 README.md
%P% -m markdown --file=docs\README.html --output_format=html4 README.md
IF ERRORLEVEL 1 GOTO TheEnd

ECHO COPY README.md to docs\README.txt
TYPE README.md >docs\README.txt

:TheEnd
ENDLOCAL
