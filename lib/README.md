# SimConnect Library Files

This folder must contain the SimConnect managed wrapper DLLs from your MSFS 2024 SDK installation.

## Required Files

| File | Description |
|---|---|
| `SimConnect.dll` | Native SimConnect client DLL (x64) |
| `Microsoft.FlightSimulator.SimConnect.dll` | Managed (.NET) wrapper |

## Where to find them

**MSFS 2024 (Microsoft Store / Game Pass):**
```
%LOCALAPPDATA%\Packages\Microsoft.Limitless_8wekyb3d8bbwe\LocalCache\Packages\Official\OneStore\fs-base\lib\managed\
```

**MSFS 2024 (Steam):**
```
%LOCALAPPDATA%\Packages\Microsoft.Limitless_8wekyb3d8bbwe\LocalCache\Packages\Official\Steam\fs-base\lib\managed\
```

**MSFS SDK (if installed):**
```
C:\MSFS 2024 SDK\SimConnect SDK\lib\managed\
```

## Setup Steps

1. Copy both DLLs into this `lib/` folder.
2. Build the project — the `.csproj` will automatically reference and copy them.

> ⚠️ These files are copyrighted by Microsoft and cannot be redistributed.
> They are excluded from source control via `.gitignore`.
