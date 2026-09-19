#ifndef PluginSource
  #define PluginSource "..\Community.PowerToys.Run.Plugin.LocalTranslator\bin\x64\Release\net9.0-windows10.0.26100.0"
#endif
#ifndef OutputDirectory
  #define OutputDirectory "..\artifacts"
#endif
#ifndef AppVersion
  #define AppVersion "0.1.1"
#endif
#ifndef VersionInfoVersion
  #define VersionInfoVersion "0.1.1.0"
#endif

#define AppName "LocalTranslator for PowerToys Run"

[Setup]
AppId={{B008DD20-E265-4C09-96AD-590FA0313A2C}
AppName={#AppName}
AppVerName={#AppName} {#AppVersion}
AppVersion={#AppVersion}
AppPublisher=LocalTranslator Contributors
DefaultDirName={localappdata}\Microsoft\PowerToys\PowerToys Run\Plugins\LocalTranslator
DisableDirPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
UsePreviousAppDir=no
Uninstallable=yes
UninstallDisplayName={#AppName}
OutputDir={#OutputDirectory}
OutputBaseFilename=LocalTranslator-Setup-{#AppVersion}-x64
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern dynamic
CloseApplications=yes
RestartApplications=no
SetupLogging=yes
MinVersion=10.0.19041
ArchitecturesAllowed=x64compatible
VersionInfoVersion={#VersionInfoVersion}
VersionInfoCompany=LocalTranslator Contributors
VersionInfoDescription=Offline Chinese-English translator for PowerToys Run
VersionInfoProductName={#AppName}
VersionInfoProductVersion={#AppVersion}
VersionInfoCopyright=LocalTranslator Contributors

[Languages]
Name: "chinesesimplified"; MessagesFile: "ChineseSimplified.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#PluginSource}\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs

[Run]
Filename: "{code:GetPowerToysPath}"; Description: "{cm:LaunchPowerToys}"; Flags: nowait postinstall skipifsilent; Check: IsPowerToysInstalled

[CustomMessages]
english.LaunchPowerToys=Start PowerToys
english.PowerToysRunning=PowerToys is currently running.%n%nExit PowerToys from its system tray icon, then run this installer again. This prevents locked plugin files and an incomplete installation.
english.PowerToysRunningUninstall=PowerToys is currently running.%n%nExit PowerToys from its system tray icon, then run the uninstaller again.
chinesesimplified.LaunchPowerToys=启动 PowerToys
chinesesimplified.PowerToysRunning=PowerToys 当前正在运行。%n%n请先从系统托盘图标退出 PowerToys，然后重新运行安装程序，以免插件文件被占用或安装不完整。
chinesesimplified.PowerToysRunningUninstall=PowerToys 当前正在运行。%n%n请先从系统托盘图标退出 PowerToys，然后重新运行卸载程序。

[Code]
function IsPowerToysRunning: Boolean;
var
  ResultCode: Integer;
  PowerShellPath: String;
  Parameters: String;
begin
  PowerShellPath := ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe');
  Parameters := '-NoProfile -NonInteractive -Command "if (Get-Process -Name PowerToys -ErrorAction SilentlyContinue) { exit 42 } else { exit 0 }"';
  Result := Exec(PowerShellPath, Parameters, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 42);
end;

function GetPowerToysPath(Param: String): String;
var
  Candidate: String;
begin
  Candidate := ExpandConstant('{commonpf}\PowerToys\PowerToys.exe');
  if FileExists(Candidate) then
  begin
    Result := Candidate;
    Exit;
  end;

  Candidate := ExpandConstant('{localappdata}\PowerToys\PowerToys.exe');
  if FileExists(Candidate) then
  begin
    Result := Candidate;
    Exit;
  end;

  Candidate := ExpandConstant('{localappdata}\Microsoft\PowerToys\PowerToys.exe');
  if FileExists(Candidate) then
  begin
    Result := Candidate;
    Exit;
  end;

  Result := '';
end;

function IsPowerToysInstalled: Boolean;
begin
  Result := GetPowerToysPath('') <> '';
end;

function InitializeSetup: Boolean;
begin
  Result := True;
  if IsPowerToysRunning then
  begin
    SuppressibleMsgBox(CustomMessage('PowerToysRunning'), mbError, MB_OK, IDOK);
    Result := False;
  end;
end;

function InitializeUninstall: Boolean;
begin
  Result := True;
  if IsPowerToysRunning then
  begin
    MsgBox(CustomMessage('PowerToysRunningUninstall'), mbError, MB_OK);
    Result := False;
  end;
end;
