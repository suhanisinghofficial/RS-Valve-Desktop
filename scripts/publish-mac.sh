#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

echo "Publishing macOS arm64 (self-contained)..."
dotnet publish RSValve.Desktop/RSValve.Desktop.csproj \
  -c Release \
  -r osx-arm64 \
  --self-contained true \
  -o publish/osx-arm64

echo ""
echo "Output: $(pwd)/publish/osx-arm64/RSValve"
echo "Run: ./publish/osx-arm64/RSValve"
