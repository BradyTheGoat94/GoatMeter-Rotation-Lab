#define AppVersion GetEnv('GOATMETER_VERSION')
[Setup]
AppId={{42A95BCA-5E13-4DBE-96C5-3C420B0D0875}
AppName=GoatMeter Rotation Lab
AppVersion={#AppVersion}
AppPublisher=GoatMeter
DefaultDirName={localappdata}\Programs\GoatMeter-Rotation-Lab
DefaultGroupName=GoatMeter Rotation Lab
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\dist
OutputBaseFilename=GoatMeter-Rotation-Lab-Setup
SetupIconFile=..\src\Assets\Brand\GoatHead.ico
UninstallDisplayIcon={app}\GoatMeter.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
[Tasks]
Name: desktopicon; Description: "Create a desktop shortcut"; Flags: unchecked
[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{group}\GoatMeter Rotation Lab"; Filename: "{app}\GoatMeter.exe"
Name: "{autodesktop}\GoatMeter Rotation Lab"; Filename: "{app}\GoatMeter.exe"; Tasks: desktopicon
[Run]
Filename: "{app}\GoatMeter.exe"; Description: "Launch GoatMeter Rotation Lab"; Flags: nowait postinstall skipifsilent
[Code]
function HasNpcap: Boolean;
begin
  Result := FileExists(ExpandConstant('{sys}\drivers\npcap.sys'));
end;
function NextButtonClick(CurPageID: Integer): Boolean;
var ErrorCode: Integer;
begin
  Result := True;
  if (CurPageID = wpReady) and not HasNpcap then
  begin
    if MsgBox('GoatMeter needs Npcap to read combat traffic. Download and install Npcap from its official website, then return here and click Install again. Open the download page now?', mbInformation, MB_YESNO) = IDYES then
      ShellExec('open', 'https://npcap.com/#download', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
    Result := False;
  end;
end;
