#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet not found. Install the .NET SDK from:"
  echo "https://dotnet.microsoft.com/download/dotnet/10.0"
  exit 1
fi

echo "Using: $(dotnet --version)"
dotnet build RSValve.sln -c Debug -v minimal

APP="./RSValve.Desktop/bin/Debug/net8.0/RSValve"
if [[ ! -x "$APP" ]]; then
  echo "App binary not found at $APP"
  exit 1
fi

echo "Launching RS VALVE APPLICATION…"
# Run the native host directly so macOS shows the GUI window reliably.
exec "$APP"
