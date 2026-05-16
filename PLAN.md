# RS Valve — Windows Desktop Video Library + Local HTTP Server

## Goal

Build a **Windows desktop app** that:

- Is **installed like a normal program** (Start menu, uninstaller, optional desktop shortcut).
- **Stores MP4 files** in a managed library folder.
- Lets the user **add** and **remove** videos (**MP4 only**).
- Runs a **small HTTP server on localhost** so **any browser on the same PC** can open and stream those files (e.g. `http://127.0.0.1:PORT/video/...`).
- Ships as **one installer**: after install, **no separate installs** of Node.js, Python, or other runtimes (everything is bundled or uses components already on modern Windows).

---

## Recommended approach: .NET 8 (WPF or WinUI 3) + embedded Kestrel

**Why this fits best**

| Requirement | How .NET solves it |
|-------------|-------------------|
| Single installer, no extra runtime for user | **Self-contained** publish bundles the .NET runtime into your app folder. |
| Desktop UI | **WPF** (mature, simple) or **WinUI 3** (modern Fluent UI). |
| HTTP for browser | **Kestrel** (same server as ASP.NET Core) can run **in-process** inside the desktop app and serve files with correct `Content-Type` for MP4. |
| MP4-only | Validate extension + optional **magic-byte** check on add. |
| Windows-native feel | MSI/EXE via **WiX**, **Inno Setup**, or **MSIX** packaging. |

**WebView2**: Only needed if you build the UI with **WebView2 + HTML/JS** instead of pure XAML. A **pure WPF** UI avoids that dependency entirely.

**Alternative (lighter binary, more engineering): Tauri 2** — Rust backend + small web UI, `axum`/`warp` for HTTP, one installer. Depends on **WebView2** (preinstalled on most Windows 10/11 machines). Excellent if the team prefers web tech for UI.

**Alternative (heavier): Electron** — Familiar stack; installer is large (~100MB+); still “one installer,” no user-installed Node.

**Not recommended for “no outside install”**: A Python/Node script that expects the user to install Python/Node first.

---

## High-level architecture

```text
┌─────────────────────────────────────────────────────────┐
│                    Desktop process                       │
│  ┌─────────────────┐    ┌──────────────────────────┐  │
│  │  UI (WPF/WinUI) │    │  Kestrel (localhost only) │  │
│  │  - list videos  │    │  - GET /api/list          │  │
│  │  - add/remove   │    │  - GET /media/{id}.mp4    │  │
│  └────────┬────────┘    └─────────────┬────────────┘  │
│           │                             │               │
│           └───────────┬─────────────────┘               │
│                 Shared service layer                     │
│           (paths, DB/json index, validation)             │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
              App data folder (user)
              e.g. %AppData%\YourCompany\RSValve\Videos\
```

- **Library storage**: Per-user folder under `%AppData%` (or `%LocalAppData%`) so the app does not need admin rights for normal use.
- **Catalog**: Small **SQLite** or a **JSON index** mapping stable IDs → filenames, add dates, optional display names. SQLite is robust if the library grows.
- **HTTP binding**: **`127.0.0.1` only** (not `0.0.0.0`) so videos are not exposed to the LAN unless you explicitly want that later.
- **Port**: Fixed default (e.g. `8765`) with **fallback** if the port is busy, and show the chosen port in the UI (“Open in browser” copies the base URL).

---

## Feature breakdown

### 1. Video library (MP4 only)

- **Add**: File picker filtered to `.mp4`; copy or **move** into library folder (user choice: “copy” vs “move” avoids duplicates).
- **Remove**: Delete from catalog + optionally delete file from disk (confirm dialog).
- **Validation**: Extension `.mp4` + optional ftyp/`ftyp` box check for false positives.
- **List**: Grid or list with filename, size, date added; optional thumbnail later (out of scope for v1 unless needed).

### 2. Local HTTP API (same machine)

Examples (paths are illustrative):

- `GET http://127.0.0.1:{port}/api/videos` — JSON list of `{ id, name, url }`.
- `GET http://127.0.0.1:{port}/media/{id}` — stream MP4 with `Content-Type: video/mp4`, support **Range** requests for seeking in the browser.

CORS: Allow `http://127.0.0.1` and `http://localhost` origins if you ever call the API from a page opened as `file://` or another port (tune as needed).

### 3. Installer & updates

- **Self-contained** `win-x64` (or `win-arm64` if you target ARM) publish.
- **Code signing** (recommended for SmartScreen): sign the installer and binaries when you have a certificate.
- **Uninstaller**: Standard entry in “Apps & features.”
- **Updates** (later): optional auto-update (e.g. Squirrel, custom) — not required for v1.

---

## Security notes (important)

- Bind to **loopback only** unless you have a strong reason to expose the LAN.
- **No directory traversal**: Map URLs only to known IDs in your catalog, not raw paths from the client.
- Do not run the server as elevated admin for normal operation.

---

## Project structure (suggested)

```text
RSValve.Desktop/
  RSValve.Desktop.csproj      # WPF host
  App.xaml / MainWindow.xaml
  Services/
    VideoLibraryService.cs    # add/remove/list, MP4 validation
    HttpServerService.cs      # start/stop Kestrel, port selection
  Models/
    VideoItem.cs
```

Single solution is enough for v1; split API project only if you want cleaner separation.

---

## Implementation phases

| Phase | Deliverable |
|-------|-------------|
| **1** | WPF shell: empty window, app data path, logging. |
| **2** | `VideoLibraryService`: folder + SQLite/JSON, add/remove/list, MP4 checks. |
| **3** | Kestrel in same process: list + file streaming with Range support. |
| **4** | UI: bind list, add/remove buttons, show URL + “Copy link” / “Open in browser.” |
| **5** | Self-contained publish + installer (WiX/Inno/MSIX) + README for internal testers. |

---

## Tech stack summary

- **Language / runtime**: C# / .NET 8  
- **UI**: WPF (simplest) or WinUI 3  
- **HTTP**: ASP.NET Core Kestrel (Microsoft.AspNetCore.App / minimal hosting)  
- **Data**: SQLite (Microsoft.Data.Sqlite) or JSON file  
- **Packaging**: Self-contained single-file or folder publish + installer tool of your choice  

---

## What “no outside install” means for the user

After running your **one setup executable**:

- The app appears in the Start menu.
- All **.NET runtime** files needed by the app are **inside the installation directory** (self-contained).
- **No** Node, Python, or separate “server” product to install.

**Optional OS component**: If you later use **WebView2** for UI, the WebView2 **Evergreen** runtime is usually already on Windows 10/11; if missing, Microsoft’s installer can bootstrap it — plan that only if you pick a WebView-based UI.

---

## Open decisions (pick before coding)

1. **WPF vs WinUI 3** — WPF = faster to ship; WinUI 3 = newer look.  
2. **Copy vs move** on add — default “copy” is safer for originals.  
3. **Default port** and whether to persist the port in settings if changed.  
4. **SQLite vs JSON** — SQLite recommended if you expect many files or metadata later.

---

## Next step

Scaffold a **.NET 8 WPF** app, add a **hosted Kestrel** `WebApplication` or `IHost` with minimal endpoints, wire **VideoLibraryService**, then add the **installer** pipeline to the same repo (CI optional).

This document is the implementation blueprint; actual code can follow this plan in the same repository.

---

## Implementation (RS VALVE v1)

The solution **`RSValve.sln`** contains **`RSValve.Desktop`**: an **Avalonia 11** + **.NET 8** desktop app with **Kestrel** on **127.0.0.1** only. Library files and `library.json` live under **`%LocalAppData%\RS VALVE\Library\`**.

### Versioning

Edit **`Directory.Build.props`** at the repo root: bump **`Version`**, **`AssemblyVersion`**, **`FileVersion`**, and **`InformationalVersion`** together when you ship a new build. The main window shows **`InformationalVersion`**.

### Build (requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0))

From the directory that contains **`RSValve.sln`**:

```bash
dotnet restore RSValve.sln
dotnet build RSValve.sln -c Release
```

### Publish self-contained Windows x64 (no separate .NET install on target PC)

```bash
dotnet publish RSValve.Desktop/RSValve.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o ./publish/win-x64
```

Then zip the **`publish/win-x64`** folder or wrap it with **WiX / Inno Setup / MSIX** for a single installer. **`PublishSingleFile=true`** is optional; test first, as single-file can affect native dependencies.

### HTTP endpoints (while the app is running)

| Method | Path | Purpose |
|--------|------|--------|
| GET | `/` | Short text help |
| GET | `/api/videos` | JSON: `id`, `name`, `url` (absolute `http://127.0.0.1:PORT/media/{id}`) |
| GET | `/media/{id}` | MP4 stream with **Range** support (browser seeking) |

### Future: update system

Ship later via **Squirrel.Windows**, **ClickOnce**, or a small **in-app downloader** that replaces files under the install directory; keep version in **`Directory.Build.props`** and optionally expose it via `/api/version` when you add auto-update.
