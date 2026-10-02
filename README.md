# reMapp
- Requires .NET 10 SDK and Windows 10 (19041+) x64.
- Build: `dotnet build -c Release /home/runner/work/reMapp/reMapp/App1/App1.csproj`
- Run: `dotnet run -c Release --project /home/runner/work/reMapp/reMapp/App1/App1.csproj`
- App is unpackaged and self-contained (`WindowsPackageType=None`, `SelfContained=true`).
- Select Key A / Key B, then toggle `Swap: ON` to swap them system-wide.
- Toggle `Swap: OFF` or close the window to restore normal behavior.
- Limitation: Hook cannot intercept keys in elevated windows unless this app is also elevated, and it only works while the app runs.
