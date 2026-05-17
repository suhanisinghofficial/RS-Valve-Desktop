#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#define AppName "RS VALVE APPLICATION"
#define AppShortName "RS Valve"
#define AppExe "RS-Valve.exe"
#define AppIcon "..\RSValve.Desktop\Assets\logo.ico"
#define SourceDir "..\publish\app"
#define OutputDir "..\publish"

[Setup]
SetupIconFile={#AppIcon}
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
CloseApplications=force
RestartApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"; IconFilename: "{#AppIcon}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; IconFilename: "{#AppIcon}"; Tasks: desktopicon

[Code]
function ShouldLaunchAfterSilentUpdate: Boolean;
begin
  Result := WizardSilent;
end;

[Run]
Filename: "{app}\{#AppExe}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent
Filename: "{app}\{#AppExe}"; Flags: nowait postinstall; Check: ShouldLaunchAfterSilentUpdate
