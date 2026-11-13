; ==============================================================================
;  KBotTray.iss -- installer of the K-BOT signer edition (KBot.Tray), Inno Setup 6.5+
;
;  Compiled by push-tray.ps1 AFTER the tray folder is complete (KBot.Tray.exe, signed, and
;  the DLLs it opens the signing windows from; no other .exe). The script passes:
;    /DSourceDir=<the tray package folder>      required
;    /DAppVersion=<KBot.Tray FileVersion>       required (x.x.x.x)
;    /DOutputDir=<artifacts dir>                required
;    /DOutputBaseFilename=KBot_Tray_Setup_<v>   required
;    /DSIGN=1 /Skbotsign="<signtool cmd> $f"    optional: signs Setup + uninstaller
;
;  It is a PER-USER install (no administrator, no UAC): the tray replaces its own files when
;  a newer version is published, so its folder must be one the user can write to.
;  Default folder: %LOCALAPPDATA%\AVACONT\KBotSemnare. Windows start-up: the tray puts itself in
;  the user's Run key on its first start; the uninstaller takes it out.
;
;  Operator-visible text is Romanian (with diacritics; this file is saved with a BOM so Inno
;  reads it as UTF-8). Everything else is English ASCII, per RULE 0.
; ==============================================================================

#ifndef SourceDir
  #error SourceDir is required: /DSourceDir=<tray package folder>
#endif
#ifndef AppVersion
  #error AppVersion is required: /DAppVersion=x.x.x.x
#endif
#ifndef OutputDir
  #error OutputDir is required: /DOutputDir=<folder>
#endif
#ifndef OutputBaseFilename
  #error OutputBaseFilename is required: /DOutputBaseFilename=<name without .exe>
#endif

#define MyAppName        "K-BOT Semnare"
; Its own AppId: this program is NOT K-BOT and lives next to it without touching it.
#define MyAppId          "{B5C0A3E2-7D41-4C6B-9E1A-2F8D6A4C1B07}"
#define MyPublisher      "AVATAR SOFT SRL"
#define MyAppExe         "KBot.Tray.exe"
#define RuntimeSetup     "..\KBotInstaller\prereq\windowsdesktop-runtime-8-win-x64.exe"

#if FileExists(AddBackslash(SourcePath) + RuntimeSetup)
  #define HAVE_RUNTIME
#endif

[Setup]
AppId={{#MyAppId}
AppName={#MyAppName}
AppVersion={#AppVersion}
AppVerName={#MyAppName} {#AppVersion}
AppPublisher={#MyPublisher}
AppCopyright=Copyright (C) {#MyPublisher}
VersionInfoVersion={#AppVersion}
VersionInfoCompany={#MyPublisher}
VersionInfoProductName={#MyAppName}
VersionInfoDescription=Instalare {#MyAppName}

PrivilegesRequired=lowest
DefaultDirName={localappdata}\AVACONT\KBotSemnare
UsePreviousAppDir=yes
DisableDirPage=yes
DirExistsWarning=no
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableWelcomePage=no

OutputDir={#OutputDir}
OutputBaseFilename={#OutputBaseFilename}
SetupIconFile=..\..\src\KBot.App\kbot.ico
UninstallDisplayIcon={app}\{#MyAppExe}
UninstallDisplayName={#MyAppName}

WizardStyle=modern
WizardImageFile=..\KBotInstaller\images\wizard-large-164.bmp,..\KBotInstaller\images\wizard-large-246.bmp,..\KBotInstaller\images\wizard-large-328.bmp
WizardSmallImageFile=..\KBotInstaller\images\wizard-small-55.bmp,..\KBotInstaller\images\wizard-small-83.bmp,..\KBotInstaller\images\wizard-small-110.bmp,..\KBotInstaller\images\wizard-small-138.bmp

Compression=lzma2/max
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
ShowLanguageDialog=no
; A running tray is closed by [Code] (PrepareToInstall), not by the restart manager.
CloseApplications=no
RestartApplications=no

#ifdef SIGN
SignTool=kbotsign
SignedUninstaller=yes
#endif

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
WelcomeLabel2=Acest program va instala {#MyAppName} {#AppVersion} pe calculatorul dumneavoastră.%n%n{#MyAppName} vă arată documentele care așteaptă semnătura dumneavoastră, vă lasă să le vedeți și să le semnați. Rămâne în zona de notificări de lângă ceas și pornește odată cu Windows.%n%nNu are nevoie de drepturi de administrator.
FinishedLabel=Instalarea {#MyAppName} s-a încheiat. Programul pornește din meniul Start; iconița lui apare lângă ceas.%n%nPentru dezinstalare folosiți «Aplicații instalate» din Windows.

[Files]
; Everything of the package. Nothing that is already in {app} and not in the package is deleted on an upgrade.
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
#ifdef HAVE_RUNTIME
Source: "{#RuntimeSetup}"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not DotNetDesktop8Present
#endif

[Icons]
Name: "{group}\{#MyAppName}";              Filename: "{app}\{#MyAppExe}"; WorkingDir: "{app}"
Name: "{group}\Dezinstalare {#MyAppName}"; Filename: "{uninstallexe}"

[Run]
#ifdef HAVE_RUNTIME
; The runtime is installed for the whole PC, so this one step asks Windows for administrator rights (the only one).
Filename: "{tmp}\{#ExtractFileName(RuntimeSetup)}"; Parameters: "/install /quiet /norestart"; StatusMsg: "Se instalează .NET Desktop Runtime 8..."; Check: not DotNetDesktop8Present; Verb: "runas"; Flags: waituntilterminated
#endif
Filename: "{app}\{#MyAppExe}"; Description: "Pornește {#MyAppName}"; WorkingDir: "{app}"; Flags: nowait postinstall

[UninstallDelete]
; The tray adds and renames files by itself when it updates (*.old, _update\): they are not in the install log.
Type: filesandordirs; Name: "{app}"

[Code]
// .NET Desktop Runtime 8 (x64) is a prerequisite: the tray is published framework-dependent.
// Same detection as the K-BOT installer (the runtime records versions under the 32-bit registry view).
function DotNetDesktop8Present: Boolean;
var
  Names: TArrayOfString;
  I: Integer;
  FR: TFindRec;
begin
  Result := False;
  if RegGetValueNames(HKLM32, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', Names) then
    for I := 0 to GetArrayLength(Names) - 1 do
      if Copy(Names[I], 1, 2) = '8.' then
        Result := True;
  if not Result then
    if FindFirst(ExpandConstant('{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App\8.*'), FR) then
    begin
      Result := True;
      FindClose(FR);
    end;
end;

#ifndef HAVE_RUNTIME
function InitializeSetup: Boolean;
begin
  Result := True;
  if not DotNetDesktop8Present then
  begin
    MsgBox('Pe acest calculator lipsește «.NET Desktop Runtime 8» (x64), de care are nevoie {#MyAppName}.' + #13#10 + #13#10 +
           'Instalați-l (de la Microsoft, fișierul «windowsdesktop-runtime-8…-win-x64.exe»; cere drepturi de administrator) și porniți din nou această instalare.',
           mbError, MB_OK);
    Result := False;
  end;
end;
#endif

// A tray that is running holds its files: it is ended before they are written. Only this user's own process can be ended here.
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
begin
  Result := '';
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM {#MyAppExe}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

// The uninstaller also takes out what the tray wrote on its own: the Windows start-up entry and its remembered choice.
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultCode: Integer;
begin
  if CurUninstallStep = usUninstall then
  begin
    Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM {#MyAppExe}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'KBotTray');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\AVACONT\KBot\Tray');
  end;
end;
