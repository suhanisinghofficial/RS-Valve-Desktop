#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet not found. Install the .NET 8 SDK (or newer) from:"
  echo "https://dotnet.microsoft.com/download/dotnet/8.0"
  exit 1
fi

echo "Using: $(dotnet --version)"
dotnet build RSValve.Desktop/RSValve.Desktop.csproj -c Debug -v minimal

APP_DLL="./RSValve.Desktop/bin/Debug/net8.0/RSValve.dll"
if [[ ! -f "$APP_DLL" ]]; then
  echo "App not found at $APP_DLL"
  exit 1
fi

echo "Launching RS VALVE APPLICATION…"
# Roll forward when only a newer runtime is installed (e.g. .NET 10 on Mac).
export DOTNET_ROLL_FORWARD=Major
exec dotnet "$APP_DLL"
