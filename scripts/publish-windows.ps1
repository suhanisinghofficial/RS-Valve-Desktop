# Publish RS VALVE APPLICATION for Windows x64 (self-contained folder)
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

$Out = Join-Path $Root "publish"
Write-Host "Publishing Windows x64 (self-contained folder)..." -ForegroundColor Cyan

dotnet publish RSValve.Desktop/RSValve.Desktop.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:PublishTrimmed=false `
    -o $Out

Write-Host ""
Write-Host "App:    $Out\RS-Valve.exe" -ForegroundColor Green
Write-Host "Folder: Copy entire 'publish' folder to the PC." -ForegroundColor Yellow
Write-Host "Installer (Windows + Inno Setup): ISCC.exe installer\installer.iss" -ForegroundColor Yellow
