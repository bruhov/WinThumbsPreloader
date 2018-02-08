#define MyAppName "WinThumbsPreloader"
#define MyAppReleaseDirectory "..\WinThumbsPreloader\WinThumbsPreloader\bin\Release"
#define MyAppFilename MyAppName + ".exe"
#define MyAppFilepath MyAppReleaseDirectory + "\" + MyAppFilename
#dim Version[4]
#expr ParseVersion(MyAppFilepath, Version[0], Version[1], Version[2], Version[3])
#define MyAppVersion Str(Version[0]) + "." + Str(Version[1]) + "." + Str(Version[2])
#define MyAppPublisher "Dmitry Bruhov"
#define MyAppId "CF49DD18-AA76-4E79-97C2-4FEAED1AED5F"

#include <idp.iss>
#include <idplang\Russian.iss> 

[Setup]
AppCopyright=Copyright (c) 2018 {#MyAppPublisher}
AppId={#MyAppId}
AppMutex={#MyAppId}
AppName={#MyAppName}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://bruhov.com/WinThumbsPreloader
AppSupportURL=https://github.com/bruhov/WinThumbsPreloader/issues
AppUpdatesURL=https://github.com/bruhov/WinThumbsPreloader/releases
AppVerName={#MyAppName} {#MyAppVersion}
AppVersion={#MyAppVersion}
ArchitecturesAllowed=x86 x64 ia64
ArchitecturesInstallIn64BitMode=x64 ia64
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DirExistsWarning=no
DisableReadyPage=yes
DisableProgramGroupPage=yes
LicenseFile=license.txt
MinVersion=6.1
OutputBaseFilename={#MyAppName}-{#MyAppVersion}-setup
OutputDir=Output
ShowLanguageDialog=no
UninstallDisplayIcon={app}\{#MyAppFilename}
UninstallDisplayName={#MyAppName}
VersionInfoTextVersion={#MyAppVersion}
VersionInfoVersion={#MyAppVersion}
WizardImageFile=WizardImageFile.bmp
WizardImageStretch=no
WizardSmallImageFile=WizardSmallImageFile.bmp
SolidCompression=yes

[Languages]
Name: en; MessagesFile: "compiler:Default.isl"
Name: ru; MessagesFile: "compiler:Languages\Russian.isl"

[CustomMessages]
; .Net installer
en.NetFrameworkInstallerCaption=Installing .NET Framework 4.5.2. This might take a few minutes...
ru.NetFrameworkInstallerCaption=Установка .NET Framework 4.5.2...

en.NetFrameworkInstallerFail=.NET installation failed with code
ru.NetFrameworkInstallerFail=Ошибка установки .NET Framework. Код ошибки

; Context menu items
en.PreloadThumbnails=Preload thumbnails
ru.PreloadThumbnails=Загрузить эскизы

en.PreloadThumbnailsRecursively=Preload thumbnails recursively
ru.PreloadThumbnailsRecursively=Загрузить эскизы включая подпапки

[Files]
Source: "{#MyAppFilepath}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppFilepath}.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\ru\*.resources.dll"; DestDir: "{app}\Languages\ru"; Flags: ignoreversion

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppFilename}"

[Run]
Filename: "{app}\{#MyAppFilename}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]
Root: "HKCR"; Subkey: "Directory\shell\{#MyAppName}"; Flags: uninsdeletekey
Root: "HKCR"; Subkey: "Directory\shell\{#MyAppName}"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{#MyAppName}"
Root: "HKCR"; Subkey: "Directory\shell\{#MyAppName}"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\{#MyAppFilename}"",0"
Root: "HKCR"; Subkey: "Directory\shell\{#MyAppName}"; ValueType: string; ValueName: "ExtendedSubCommandsKey"; ValueData: "Directory\shell\{#MyAppName}"
Root: "HKCR"; Subkey: "Directory\shell\{#MyAppName}\Shell"
Root: "HKCR"; Subkey: "Directory\shell\{#MyAppName}\Shell\Preload"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:PreloadThumbnails}"
Root: "HKCR"; Subkey: "Directory\shell\{#MyAppName}\Shell\Preload"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\{#MyAppFilename}"",0"
Root: "HKCR"; Subkey: "Directory\shell\{#MyAppName}\Shell\Preload\command"; ValueType: string; ValueData: """{app}\{#MyAppFilename}"" ""%1"""
Root: "HKCR"; Subkey: "Directory\shell\{#MyAppName}\Shell\PreloadRecursively"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:PreloadThumbnailsRecursively}"
Root: "HKCR"; Subkey: "Directory\shell\{#MyAppName}\Shell\PreloadRecursively"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\{#MyAppFilename}"",0"
Root: "HKCR"; Subkey: "Directory\shell\{#MyAppName}\Shell\PreloadRecursively\command"; ValueType: string; ValueData: """{app}\{#MyAppFilename}"" -r ""%1"""

[Code]
function Framework45IsNotInstalled(): Boolean;
var
  bSuccess: Boolean;
  regVersion: Cardinal;
begin
  Result := True;

  bSuccess := RegQueryDWordValue(HKLM, 'Software\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', regVersion);
  if (True = bSuccess) and (regVersion >= 378389) then begin
    Result := False;
  end;
end;

procedure InitializeWizard;
begin
  if Framework45IsNotInstalled() then
  begin
    idpAddFile('http://go.microsoft.com/fwlink/?LinkId=397707', ExpandConstant('{tmp}\NetFrameworkInstaller.exe'));
    idpDownloadAfter(wpReady);
  end;
end;

procedure InstallFramework;
var
  StatusText: string;
  ResultCode: Integer;
begin
  StatusText := WizardForm.StatusLabel.Caption;
  WizardForm.StatusLabel.Caption := ExpandConstant('{cm:NetFrameworkInstallerCaption}');
  WizardForm.ProgressGauge.Style := npbstMarquee;
  try
    if not Exec(ExpandConstant('{tmp}\NetFrameworkInstaller.exe'), '/passive /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
    begin
      MsgBox(ExpandConstant('{cm:NetFrameworkInstallerFail}') + ': ' + IntToStr(ResultCode) + '.', mbError, MB_OK);
    end;
  finally
    WizardForm.StatusLabel.Caption := StatusText;
    WizardForm.ProgressGauge.Style := npbstNormal;

    DeleteFile(ExpandConstant('{tmp}\NetFrameworkInstaller.exe'));
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  case CurStep of
    ssPostInstall:
      begin
        if Framework45IsNotInstalled() then
        begin
          InstallFramework();
        end;
      end;
  end;
end;