# ForceFeedbackX

Open-source force feedback support for **Microsoft Flight Simulator 2024**, targeting the **Microsoft Sidewinder Force Feedback 2 (FFB2)** joystick.

## Features (Planned)

- Reads real-time flight data from MSFS 2024 via SimConnect
- Drives spring, damper, constant-force, and vibration FFB effects
- **Hydraulic/cable aircraft:** stick forces scale with IAS², trim offsets spring center, G-loading via constant force, stall buffet vibration
- **Fly-by-wire aircraft:** light centering spring + stall shaker only
- Per-aircraft JSON profiles (auto-detected by aircraft title)
- Live settings UI (force multiplier, damper gain)

---

## Prerequisites

| Requirement | Version |
|---|---|
| Windows | 10 or 11 (x64) |
| Microsoft Flight Simulator 2024 | Any edition |
| .NET SDK | 8.0+ |
| Microsoft Sidewinder FFB2 | (or any DirectInput FFB joystick) |

---

## Setup

### 1. Copy SimConnect DLLs

The SimConnect managed wrapper is not on NuGet. You must copy two DLLs from your MSFS installation into the `lib/` folder. See **[lib/README.md](lib/README.md)** for exact paths.

### 2. Build

```bash
cd src/ForceFeedbackX
dotnet restore
dotnet build -c Release
```

### 3. Run

```bash
dotnet run --project src/ForceFeedbackX
```

Or run the compiled `ForceFeedbackX.exe` from `bin/Release/net8.0-windows/`.

> **Start MSFS 2024 first**, then launch ForceFeedbackX. It will auto-connect and reconnect on sim restart.

---

## Architecture

```
ForceFeedbackX (SimConnect external app — out-of-process)
│
├── SimConnect/
│   └── SimConnectClient   — opens SimConnect session, subscribes to SimVars,
│                            fires FlightDataReceived event each sim frame
│
├── Physics/
│   ├── FlightData         — SimVar snapshot (IAS, G, trim, stall, etc.)
│   ├── ForceOutput        — normalized FFB parameters (spring, damper, CF, vibration)
│   ├── AircraftType       — HydraulicCable | FlyByWire | Unknown
│   └── ForceCalculator    — converts FlightData + AircraftProfile → ForceOutput
│
├── FFB/
│   └── FfbEngine          — DirectInput device management, effect creation & update
│
├── Profiles/
│   ├── AircraftProfile    — per-aircraft settings (force multiplier, type, trim scale…)
│   └── ProfileManager     — loads profiles.json, matches by aircraft TITLE SimVar
│
└── UI/
    ├── MainWindow.xaml    — status, live telemetry, sliders
    └── App.xaml.cs        — .NET Generic Host DI wiring
```

**Why a SimConnect external app?**
WASM modules run sandboxed inside MSFS and cannot access OS hardware (DirectInput, HID).
An out-of-process SimConnect client gets full hardware access while the sim remains stable if the tool crashes.

---

## Roadmap

| Phase | Description | Status |
|---|---|---|
| 1 | SimConnect + DirectInput FFB scaffold (this commit) | ✅ In progress |
| 2 | Complete DirectInput effect creation & tuning | 🔲 Planned |
| 3 | FBW aircraft detection & sidestick model | 🔲 Planned |
| 4 | Profile editor UI, import/export | 🔲 Planned |
| 5 | Runway rumble, engine vibration, landing thud | 🔲 Planned |

---

## Contributing

PRs welcome! Please open an issue first to discuss larger changes.

## License

MIT
