#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\dist\publish"
#endif
#ifndef OutputDir
  #define OutputDir "..\dist"
#endif

[Setup]
AppId={{5E9ABF3D-4D32-4A1B-AE64-BC37F891F512}
AppName=2FARecoveryManager
AppVersion={#AppVersion}
AppPublisher=Leszek Dedko
DefaultDirName={autopf}\2FARecoveryManager
DefaultGroupName=2FARecoveryManager
OutputDir={#OutputDir}
OutputBaseFilename=2FARecoveryManager-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\2FARecoveryManager.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\2FARecoveryManager"; Filename: "{app}\2FARecoveryManager.exe"
Name: "{autodesktop}\2FARecoveryManager"; Filename: "{app}\2FARecoveryManager.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
Filename: "{app}\2FARecoveryManager.exe"; Description: "{cm:LaunchProgram,2FARecoveryManager}"; Flags: nowait postinstall skipifsilent
