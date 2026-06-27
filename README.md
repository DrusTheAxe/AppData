# APPDATA.EXE README

## OVERVIEW

APPDATA.EXE is a Windows command-line utility for accessing and managing
[ApplicationData](http://msdn.microsoft.com/library/windows/apps/BR241587).

It supports the full ApplicationData feature set, from its introduction in Windows 8 through the
[latest enhancements in Windows App SDK](https://github.com/microsoft/WindowsAppSDK/blob/main/specs/applicationdata/ApplicationData.md).

APPDATA.EXE is designed for developers building MSIX packages, administrators managing packaged
applications, and anyone curious about what packaged apps store on their behalf.

Run with no parameters for help.

The tool was born out of practical necessity during many late nights and weekends troubleshooting
packaged applications. Its technical design was heavily inspired by
[REG.EXE](http://www.microsoft.com/resources/documentation/windows/xp/all/proddocs/en-us/reg.mspx?mfr=true).

Think of APPDATA.EXE to ApplicationData as REG.EXE is to the Registry.


## INSTALLATION

APPDATA.EXE requires Windows 10.0.17763.0 (aka RS5) or newer.


## SUPPORT

The development and support home for APPDATA.EXE is https://github.com/DrusTheAxe/AppData.


## LICENSE

See LICENSE for details.
