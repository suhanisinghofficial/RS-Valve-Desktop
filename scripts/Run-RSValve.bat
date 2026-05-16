@echo off
cd /d "%~dp0"
echo Starting RS VALVE APPLICATION...
RS-Valve.exe
if errorlevel 1 (
  echo.
  echo The app exited with an error.
  echo Check: %%LOCALAPPDATA%%\RS VALVE APPLICATION\logs\startup.log
  pause
)
