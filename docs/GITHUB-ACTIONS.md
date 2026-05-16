# GitHub Actions — Windows build & installer

Automated pipeline: **push to `main`** → Windows runner → `dotnet publish` → **Inno Setup** → **`RS-Valve-Setup.exe`** artifact.

## Folder structure

```
.github/workflows/build.yml   # CI pipeline
installer/installer.iss       # Inno Setup script
Directory.Build.props         # App version (single source of truth)
RSValve.Desktop/              # .NET 8 Avalonia app → RS-Valve.exe
publish/                      # Build output (gitignored)
```

## One-time setup

1. Create a GitHub repository and push this project:
   ```bash
   git init
   git add .
   git commit -m "Add RS Valve desktop app with CI"
   git branch -M main
   git remote add origin https://github.com/YOUR_ORG/YOUR_REPO.git
   git push -u origin main
   ```

2. Ensure the default branch is **`main`** (workflow triggers on push to `main`).

3. No secrets required for basic build/upload artifacts.

## How to trigger a build

| Method | When it runs |
|--------|----------------|
| **Push to `main`** | Automatic on every push |
| **Manual** | GitHub → **Actions** → **Build Windows Installer** → **Run workflow** |

## Where to download the installer

1. Open your repo on GitHub.
2. Go to **Actions** → select the latest successful run.
3. Scroll to **Artifacts**.
4. Download **`RS-Valve-Setup-v1.0.0`** (version matches `Directory.Build.props`).
5. Unzip → run **`RS-Valve-Setup.exe`** on a Windows PC.

Artifacts are kept **90 days** (installer) / **14 days** (full publish folder).

## What gets installed on Windows

- **Location:** `C:\Program Files\RS Valve\`
- **Executable:** `RS-Valve.exe`
- **Start menu** shortcut
- **Desktop** shortcut (optional during install)
- **Uninstall** entry in Settings → Apps

## How to update the app version

1. Edit **`Directory.Build.props`**:
   ```xml
   <Version>1.0.1</Version>
   <AssemblyVersion>1.0.1.0</AssemblyVersion>
   <FileVersion>1.0.1.0</FileVersion>
   <InformationalVersion>1.0.1</InformationalVersion>
   ```
2. Commit and push to **`main`**.
3. CI reads version automatically and passes it to Inno Setup.
4. Download the new artifact from Actions.

Also update your server **`update.json`** at  
`https://rsvalve.tractioncontrolsbc.com/releases/window/update.json`  
so in-app update checks match.

## Local build on Mac (app only)

```bash
dotnet publish RSValve.Desktop/RSValve.Desktop.csproj -c Release -r win-x64 --self-contained true -o publish
```

Installer must be built on **Windows** (or via GitHub Actions) because Inno Setup is Windows-only.

## Local build on Windows (app + installer)

```powershell
dotnet publish RSValve.Desktop/RSValve.Desktop.csproj -c Release -r win-x64 --self-contained true -o publish
& "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe" installer\installer.iss
```

Output: `publish\RS-Valve-Setup.exe`

## Deploy to your update server

After each release, upload to your server:

- `releases/window/RS-Valve.exe` (or the installer)
- `releases/window/update.json` with the new `version` and `file` name
