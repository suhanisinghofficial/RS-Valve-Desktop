; Inno Setup — RS VALVE desktop app (Windows x64)
; CI: publish\app\  →  installer output: publish\RS-Valve-Setup.exe

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#define AppName "RS VALVE APPLICATION"
#define AppShortName "RS Valve"
#define AppExe "RS-Valve.exe"
#define SourceDir "..\publish\app"
#define OutputDir "..\publish"

[Setup]
AppId={{A7B3C9E1-RSVALVE-DESKTOP-2026}}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=RS VALVE APPLICATION
DefaultDirName={autopf}\{#AppShortName}
DefaultGroupName={#AppShortName}
OutputDir={#OutputDir}
OutputBaseFilename=RS-Valve-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
WizardStyle=modern
UninstallDisplayName={#AppName}
DisableProgramGroupPage=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent
