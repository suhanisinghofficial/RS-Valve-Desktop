# GitHub Actions — Windows build & installer

Automated pipeline: **push to `main`** → Windows runner → `dotnet publish` → **Inno Setup** → **`RS-Valve-Setup.exe`** artifact → **GitHub Release**.

## Folder structure

```
.github/workflows/build.yml   # CI pipeline
installer/installer.iss       # Inno Setup script
Directory.Build.props         # App version (single source of truth)
RSValve.Desktop/              # .NET 8 Avalonia app → RS-Valve.exe
publish/                      # Build output (gitignored)
```

## One-time setup

1. Create a GitHub repository and push this project.
2. Ensure the default branch is **`main`** (workflow triggers on push to `main`).
3. No extra secrets required — `GITHUB_TOKEN` publishes releases automatically.

## How to trigger a build

| Method | When it runs |
|--------|----------------|
| **Push to `main`** | Automatic on every push |
| **Manual** | GitHub → **Actions** → **Build Windows Installer** → **Run workflow** |

## Where to download the installer

**Option A — GitHub Release (recommended for users & in-app updates)**

1. Open **Releases** on the repo.
2. Download **`RS-Valve-Setup.exe`** from the latest release (tag `v1.0.0`, etc.).

**Option B — Actions artifacts (build logs / short-term)**

1. **Actions** → latest successful run → **Artifacts** → **`RS-Valve-Setup-v1.0.0`**.

Artifacts are kept **90 days** (installer) / **14 days** (full publish folder). Releases stay until you delete them.

## In-app update checks

The desktop app calls the **GitHub Releases API** (no custom server `update.json`):

- Repo: `suhanisinghofficial/RS-Valve-Desktop`
- Compares latest release tag (`v1.0.1`) to the installed version in `Directory.Build.props`
- **Download update** opens the release asset **`RS-Valve-Setup.exe`** (or the release page)

Each successful CI run on `main` publishes/updates the GitHub Release for that version.

## How to ship a new version

1. Edit **`Directory.Build.props`**:
   ```xml
   <Version>1.0.1</Version>
   <AssemblyVersion>1.0.1.0</AssemblyVersion>
   <FileVersion>1.0.1.0</FileVersion>
   <InformationalVersion>1.0.1</InformationalVersion>
   ```
2. Commit and push to **`main`**.
3. CI builds the installer and creates/updates release **`v1.0.1`** with **`RS-Valve-Setup.exe`** attached.
4. Installed apps on **1.0.0** will show the update banner after the release is live.

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
