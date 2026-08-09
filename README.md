# APPDATA.EXE README

## Overview

APPDATA.EXE is a Windows command-line utility for accessing and managing
[ApplicationData](http://msdn.microsoft.com/library/windows/apps/BR241587).

It supports the full ApplicationData feature set, from its introduction in Windows 8 through the
[latest enhancements in Windows App SDK](https://github.com/microsoft/WindowsAppSDK/blob/main/specs/applicationdata/ApplicationData.md).

APPDATA.EXE is designed for developers building MSIX packages, administrators managing packaged
applications, and anyone curious about what packaged apps store on their behalf.

Run with no parameters for help.

### Why?

The tool was born out of practical necessity during many late nights and weekends troubleshooting
packaged applications. Its technical design was heavily inspired by
[REG.EXE](http://www.microsoft.com/resources/documentation/windows/xp/all/proddocs/en-us/reg.mspx?mfr=true).

Think of APPDATA.EXE to ApplicationData as REG.EXE is to the Registry.

## Installation

There are multiple ways to install APPDATA.EXE - choose whichever you prefer!

### Install via Microsoft Store (recommended)

[![Download from the Microsoft Store](docs/images/DownloadFromTheMicrosoftStore.png)](https://apps.microsoft.com/detail/9NKMF1FM25CL?hl=en-us&gl=US&ocid=pdpshare)

### Install via WinGet

`winget install --exact --id 9NKMF1FM25CL --source msstore`

### Requirements

APPDATA.EXE requires Windows 10.0.17763.0 (aka RS5) or newer.

## Support

The development and support home for APPDATA.EXE is https://github.com/DrusTheAxe/AppData.

## License

See [LICENSE](https://github.com/DrusTheAxe/AppData/blob/main/LICENSE) for details.

## Privacy

See [PRIVACY.md](https://github.com/DrusTheAxe/AppData/blob/main/PRIVACY.md) for details.
