; Inno Setup — RS VALVE desktop app (Windows x64)
; Compiled locally:  ISCC.exe installer\installer.iss
; CI:               GitHub Actions (see .github/workflows/build.yml)

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#define AppName "RS VALVE APPLICATION"
#define AppShortName "RS Valve"
#define AppExe "RS-Valve.exe"
#define PublishDir "..\publish"

[Setup]
AppId={{A7B3C9E1-RSVALVE-DESKTOP-2026}}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=RS VALVE APPLICATION
DefaultDirName={autopf}\{#AppShortName}
DefaultGroupName={#AppShortName}
OutputDir=..\publish
OutputBaseFilename=RS-Valve-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
WizardStyle=modern
UninstallDisplayName={#AppName}
UninstallDisplayIcon={app}\{#AppExe}
DisableProgramGroupPage=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Entire publish output (exe + all .NET / Avalonia dependencies)
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
