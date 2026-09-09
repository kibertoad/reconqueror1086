#ifndef MyAppVersion
  #define MyAppVersion "0.1.0"
#endif

#define MyAppName "ReConqueror A.D. 1086"
#define MyAppGroupName "ReConqueror A.D. 1086"
#define MyAppPublisher "Reconqueror1086 contributors"
#define MyAppExeName "Conqueror.Game.exe"
#define PackageRoot "..\..\artifacts\Conqueror1086-win-x64"

[Setup]
AppId={{7AECE907-C51D-4E11-B0A2-E43BA5C8EB62}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\ReConqueror1086
DefaultGroupName={#MyAppGroupName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
SetupArchitecture=x64
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
OutputDir=..\..\artifacts
OutputBaseFilename=ReConqueror1086-Setup
UninstallDisplayIcon={app}\Game\{#MyAppExeName}
SetupLogging=yes

[Files]
Source: "{#PackageRoot}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\Game\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\Game\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon
Name: "{group}\Manage Original Resources"; Filename: "{app}\Manage Original Resources.bat"; WorkingDir: "{app}"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Run]
Filename: "{app}\Game\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; WorkingDir: "{app}"; Flags: postinstall nowait skipifsilent unchecked

[Code]
const
  PurchaseUrl = 'https://www.gog.com/en/game/conqueror_ad_1086';

var
  OriginalPage: TInputDirWizardPage;
  ImportCheckBox: TNewCheckBox;
  PurchaseButton: TNewButton;
  DetectedOriginalPath: String;

function IsOriginalInstall(const Candidate: String): Boolean;
begin
  Result := (Candidate <> '') and
    FileExists(AddBackslash(Candidate) + 'game.gog') and
    FileExists(AddBackslash(Candidate) + 'game.ins') and
    FileExists(AddBackslash(Candidate) + 'C1086.GOB');
end;

function SearchUninstallRecords(const RootKey: Integer; var Found: String): Boolean;
var
  BaseKey, DisplayName, Candidate: String;
  Names: TArrayOfString;
  I: Integer;
begin
  Result := False;
  BaseKey := 'Software\Microsoft\Windows\CurrentVersion\Uninstall';
  if not RegGetSubkeyNames(RootKey, BaseKey, Names) then
    exit;
  for I := 0 to GetArrayLength(Names) - 1 do
    if RegQueryStringValue(RootKey, BaseKey + '\' + Names[I], 'DisplayName', DisplayName) and
       (Pos('CONQUEROR', Uppercase(DisplayName)) > 0) and
       RegQueryStringValue(RootKey, BaseKey + '\' + Names[I], 'InstallLocation', Candidate) and
       IsOriginalInstall(RemoveQuotes(Candidate)) then
    begin
      Found := RemoveQuotes(Candidate);
      Result := True;
      exit;
    end;
end;

function SearchGogRecords(const RootKey: Integer; var Found: String): Boolean;
var
  BaseKey, Candidate: String;
  Names: TArrayOfString;
  I: Integer;
begin
  Result := False;
  BaseKey := 'Software\GOG.com\Games';
  if not RegGetSubkeyNames(RootKey, BaseKey, Names) then
    exit;
  for I := 0 to GetArrayLength(Names) - 1 do
    if RegQueryStringValue(RootKey, BaseKey + '\' + Names[I], 'PATH', Candidate) and
       IsOriginalInstall(RemoveQuotes(Candidate)) then
    begin
      Found := RemoveQuotes(Candidate);
      Result := True;
      exit;
    end;
end;

function FindOriginalInstall: String;
var
  Candidate: String;
  DriveNumber: Integer;
begin
  if ExpandConstant('{param:NOIMPORT|0}') = '1' then
  begin
    Result := '';
    exit;
  end;
  Result := ExpandConstant('{param:ORIGINAL|}');
  if IsOriginalInstall(Result) then exit;
  Result := '';

  if SearchGogRecords(HKCU32, Result) or SearchGogRecords(HKLM32, Result) or
     SearchGogRecords(HKCU64, Result) or SearchGogRecords(HKLM64, Result) or
     SearchUninstallRecords(HKCU32, Result) or SearchUninstallRecords(HKLM32, Result) or
     SearchUninstallRecords(HKCU64, Result) or SearchUninstallRecords(HKLM64, Result) then
    exit;

  for DriveNumber := 67 to 90 do
  begin
    Candidate := Chr(DriveNumber) + ':\GOG Games\Conqueror AD1086';
    if IsOriginalInstall(Candidate) then
    begin
      Result := Candidate;
      exit;
    end;
    Candidate := Chr(DriveNumber) + ':\Program Files (x86)\GOG Galaxy\Games\Conqueror AD1086';
    if IsOriginalInstall(Candidate) then
    begin
      Result := Candidate;
      exit;
    end;
    Candidate := Chr(DriveNumber) + ':\Program Files\GOG Galaxy\Games\Conqueror AD1086';
    if IsOriginalInstall(Candidate) then
    begin
      Result := Candidate;
      exit;
    end;
  end;
end;

procedure OpenPurchasePage(Sender: TObject);
var
  ErrorCode: Integer;
begin
  if not ShellExec('', PurchaseUrl, '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode) then
    MsgBox('Windows could not open the GOG purchase page: ' + SysErrorMessage(ErrorCode), mbError, MB_OK);
end;

procedure InitializeWizard;
begin
  DetectedOriginalPath := FindOriginalInstall;
  OriginalPage := CreateInputDirPage(wpSelectDir,
    'Original game resources',
    'Import resources from a legally owned GOG installation.',
    'Setup can extract the original art, audio, video, and data without modifying the GOG installation. ' +
    'Choose its folder, or clear the import option to install the clean-room game alone.',
    False, '');
  OriginalPage.Add('GOG installation folder:');
  if DetectedOriginalPath <> '' then
    OriginalPage.Values[0] := DetectedOriginalPath
  else
    OriginalPage.Values[0] := ExpandConstant('{sd}\GOG Games\Conqueror AD1086');

  ImportCheckBox := TNewCheckBox.Create(OriginalPage);
  ImportCheckBox.Parent := OriginalPage.Surface;
  ImportCheckBox.Left := OriginalPage.Edits[0].Left;
  ImportCheckBox.Top := OriginalPage.Edits[0].Top + OriginalPage.Edits[0].Height + ScaleY(20);
  ImportCheckBox.Width := OriginalPage.SurfaceWidth;
  ImportCheckBox.Caption := 'Import my original resources automatically after installation';
  ImportCheckBox.Checked := DetectedOriginalPath <> '';

  PurchaseButton := TNewButton.Create(OriginalPage);
  PurchaseButton.Parent := OriginalPage.Surface;
  PurchaseButton.Left := OriginalPage.Edits[0].Left;
  PurchaseButton.Top := ImportCheckBox.Top + ImportCheckBox.Height + ScaleY(18);
  PurchaseButton.Width := ScaleX(210);
  PurchaseButton.Caption := 'Buy a legal copy on GOG';
  PurchaseButton.OnClick := @OpenPurchasePage;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if (CurPageID = OriginalPage.ID) and ImportCheckBox.Checked and
     not IsOriginalInstall(OriginalPage.Values[0]) then
  begin
    MsgBox('That folder is not a complete GOG installation. Select the folder containing ' +
      'game.gog, game.ins, and C1086.GOB, or clear the automatic import option.', mbError, MB_OK);
    Result := False;
  end;
end;

function ShouldImportOriginal: Boolean;
begin
  if WizardSilent then
    Result := IsOriginalInstall(DetectedOriginalPath)
  else
    Result := ImportCheckBox.Checked and IsOriginalInstall(OriginalPage.Values[0]);
end;

function SelectedOriginalPath: String;
begin
  if WizardSilent then
    Result := DetectedOriginalPath
  else
    Result := OriginalPage.Values[0];
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  Importer, Parameters: String;
begin
  if (CurStep <> ssPostInstall) or not ShouldImportOriginal then exit;

  Importer := ExpandConstant('{app}\Tools\Conqueror.Import.exe');
  Parameters := '"' + SelectedOriginalPath + '" "' + ExpandConstant('{app}\UserContent') + '"';
  WizardForm.StatusLabel.Caption := 'Importing and verifying legally owned original resources...';
  if not Exec(Importer, Parameters, ExpandConstant('{app}'), SW_SHOW,
      ewWaitUntilTerminated, ResultCode) then
    MsgBox('The resource importer could not be started. You can retry later with ' +
      'Install Original Resources.bat.', mbError, MB_OK)
  else if ResultCode <> 0 then
    MsgBox('The game was installed, but original resource import returned error ' +
      IntToStr(ResultCode) + '. You can retry later with Install Original Resources.bat.', mbError, MB_OK);
end;
