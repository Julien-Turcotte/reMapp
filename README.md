# reMapp

reMapp is a lightweight Windows desktop app that swaps two keyboard keys system-wide while it is running.

## What it does

- Lets you choose **Key A** and **Key B** from a preset list.
- Intercepts key presses globally and swaps the selected keys in real time.
- Restores normal keyboard behavior when you disable swapping or close the app.

## Supported keys

You can swap any two keys from this list:

- CapsLock
- Esc
- Tab
- LShift
- RShift
- LCtrl
- RCtrl
- LAlt
- RAlt
- Enter
- Backspace

## Requirements

- Windows 10/11 x64  
  (target framework: `net10.0-windows10.0.19041.0`)
- .NET 10 SDK

## Build and run

From the repository root:

```bash
dotnet build -c Release /home/runner/work/reMapp/reMapp/App1/App1.csproj
dotnet run -c Release --project /home/runner/work/reMapp/reMapp/App1/App1.csproj
```

You can also run from your own clone path by replacing the absolute path with:

```bash
dotnet build -c Release App1/App1.csproj
dotnet run -c Release --project App1/App1.csproj
```

## How to use

1. Launch the app.
2. Select your two keys in **Key A** and **Key B**.
3. Click **Swap: OFF** to turn swapping on (**Swap: ON**).
4. Click the toggle again to disable swapping.

Default selection on startup:

- Key A = CapsLock
- Key B = Esc

## Important limitations

- Swapping only works while the app is running.
- Elevated apps (Run as Administrator) may not receive swapped input unless reMapp is also elevated.
- Key choices are not persisted between launches.
- Only the keys listed above are currently available.

## Troubleshooting

- **Swap does not work in a specific app:** run reMapp with the same privilege level as that app.
- **Some keys are not available:** only the built-in list is supported right now.
- **Behavior seems stuck:** toggle swap off, then on again; if needed, close and reopen the app.

## Project notes

- The app is configured as unpackaged and self-contained (`WindowsPackageType=None`, `SelfContained=true`).
