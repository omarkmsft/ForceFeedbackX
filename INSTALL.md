# ForceFeedbackX — Phase 1 Install & Run Guide

## What Phase 1 Does

When running, ForceFeedbackX will:
- ✅ Connect to MSFS 2024 automatically
- ✅ Display live airspeed, G-force, and stall warning
- ✅ Detect your Sidewinder FFB2 (or any DirectInput FFB joystick)
- ✅ Show connection status (MSFS + FFB device)
- ✅ Auto-create aircraft profiles (A320, 737, GA default)
- ⚠️ Force feedback effects are NOT active yet (Phase 2)

---

## Requirements

- Windows 10 or 11 (64-bit)
- Microsoft Flight Simulator 2024 (any edition)
- .NET 8 SDK — download from https://dotnet.microsoft.com/download/dotnet/8
- Microsoft Sidewinder FFB2 (or any DirectInput FFB joystick) plugged in

---

## Step 1 — Get the Code

```
git clone https://github.com/omarkmsft/ForceFeedbackX.git
cd ForceFeedbackX
```

Or download the ZIP from GitHub and extract it.

---

## Step 2 — Copy SimConnect DLLs

ForceFeedbackX needs two DLLs from your MSFS 2024 installation.

### Find them here:

**Microsoft Store / Game Pass:**
```
%LOCALAPPDATA%\Packages\Microsoft.Limitless_8wekyb3d8bbwe\LocalCache\Packages\Official\OneStore\fs-base\lib\managed\
```

**Steam:**
```
%LOCALAPPDATA%\Packages\Microsoft.Limitless_8wekyb3d8bbwe\LocalCache\Packages\Official\Steam\fs-base\lib\managed\
```

### Copy these two files into the `lib\` folder:
- `SimConnect.dll`
- `Microsoft.FlightSimulator.SimConnect.dll`

---

## Step 3 — Build

Open a command prompt in the repo root and run:

```
cd src\ForceFeedbackX
dotnet restore
dotnet build -c Release
```

The output will be in:
```
src\ForceFeedbackX\bin\Release\net8.0-windows\
```

---

## Step 4 — Run

1. **Start MSFS 2024 first** and load into a flight (or the main menu)
2. Run `ForceFeedbackX.exe` from the build output folder
3. The status dots will turn green when MSFS is detected
4. If your FFB joystick is plugged in, it will be shown in the top status bar

---

## Troubleshooting

| Problem | Fix |
|---|---|
| "MSFS: Disconnected" | Make sure MSFS 2024 is running. App retries every 5 seconds automatically. |
| "FFB: No device found" | Make sure your joystick is plugged in before launching the app. |
| Build error about SimConnect | Check that both DLLs are in the `lib\` folder (Step 2). |
| App crashes on launch | Make sure you're on .NET 8 and Windows x64. |

---

## What's Next (Phase 2)

Phase 2 will complete the DirectInput force effects — spring resistance, damper, G-force loading, and stall buffet vibration. That's when the stick comes alive.
