; ==============================================================================
;  KBot.iss -- K-BOT installer (Inno Setup 6.5+)
;
;  Compiled by publish-release.ps1 AFTER the staging folder is complete
;  (app + Migrare\ + Workflows\ + Logs\ + manifest.xml). The script passes:
;    /DSourceDir=<staging publish dir>          required
;    /DAppVersion=<KBot.App FileVersion>        required (x.x.x.x)
;    /DOutputDir=<artifacts dir>                required
;    /DOutputBaseFilename=KBot_Setup_<stamp>    required
;    /DSIGN=1 /Skbotsign="<signtool cmd> $f"    optional: signs Setup + uninstaller
;
;  Operator-visible text is Romanian (with diacritics). Everything else is English
;  ASCII, per RULE 0.
;
;  Installed-version guard (slice 0067-01, see [Code]): an existing K-BOT is
;  detected by the FileVersion of its KBot.App.exe; a package older than it is
;  refused, the same version is asked about, a newer one upgrades in place with
;  the rules of the in-app update system (Logs\ kept, nothing deleted).
;
;  Optional extras picked up automatically when present next to this file:
;    Romanian.isl                                -> wizard UI in Romanian
;                                                   (jrsoftware.org/files/istrans/)
;    prereq\windowsdesktop-runtime-8-win-x64.exe -> bundled .NET Desktop Runtime 8,
;                                                   installed silently when missing
; ==============================================================================

#ifndef SourceDir
  #error SourceDir is required: /DSourceDir=<staging publish dir>
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

#define MyAppName        "K-BOT"
; Stable AppId: upgrades replace the same entry in "Programs and Features" and
; [Code] reads that entry back (uninstall key = AppId + "_is1").
#define MyAppId          "{A9840797-CE1E-4708-BDC0-73E4CCBC2D3A}"
#define MyDefaultDir     "C:\KBOT"
#define MyPublisher      "AVATAR SOFT SRL"
#define MyAppExe         "KBot.App.exe"
#define MyMigratorExe    "KBot.Migrator.exe"
#define RuntimeSetup     "prereq\windowsdesktop-runtime-8-win-x64.exe"
#define RomanianIsl      "Romanian.isl"

#if FileExists(AddBackslash(SourcePath) + RuntimeSetup)
  #define HAVE_RUNTIME
#endif
#if FileExists(AddBackslash(SourcePath) + RomanianIsl)
  #define HAVE_ROMANIAN
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

; C:\KBOT is the fixed home the app expects (Logs\, Workflows\, .playwright\ next
; to the exe). The operator may change it on a FIRST install only: an upgrade goes
; where the app already is (auto = the folder page is skipped when the registry
; knows a previous install), exactly like KBot.Updater writes over its own folder.
DefaultDirName={#MyDefaultDir}
UsePreviousAppDir=yes
DisableDirPage=auto
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
WizardImageFile=images\wizard-large-164.bmp,images\wizard-large-246.bmp,images\wizard-large-328.bmp
WizardSmallImageFile=images\wizard-small-55.bmp,images\wizard-small-83.bmp,images\wizard-small-110.bmp,images\wizard-small-138.bmp

Compression=lzma2/max
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
PrivilegesRequired=admin
; Ask the operator to close a running K-BOT instead of failing on locked files.
CloseApplications=yes
RestartApplications=no
ShowLanguageDialog=no

#ifdef SIGN
SignTool=kbotsign
SignedUninstaller=yes
#endif

[Languages]
#ifdef HAVE_ROMANIAN
Name: "romanian"; MessagesFile: "{#RomanianIsl}"
#else
Name: "english"; MessagesFile: "compiler:Default.isl"
#endif

[Messages]
WelcomeLabel2=Acest program va instala {#MyAppName} {#AppVersion} pe calculatorul dumneavoastră.%n%n{#MyAppName} conține:%n  •  aplicația {#MyAppName} (angajamente, rezervări, recepții, plăți) împreună cu robotul FOREXE;%n  •  utilitarul de migrare a datelor din Access în MariaDB.%n%nSe recomandă închiderea celorlalte aplicații înainte de a continua.
FinishedLabel=Instalarea {#MyAppName} s-a încheiat. Aplicația poate fi pornită din meniul Start sau de pe desktop.%n%nPentru dezinstalare folosiți «Programe și caracteristici» din Windows.

[Types]
Name: "full";   Description: "Instalare completă"
Name: "custom"; Description: "Instalare personalizată"; Flags: iscustom

[Components]
Name: "app";     Description: "Aplicația {#MyAppName} și robotul FOREXE (obligatoriu)"; Types: full custom; Flags: fixed
Name: "migrare"; Description: "Utilitarul de migrare Access → MariaDB";               Types: full

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "playwright";  Description: "Descarcă browserul Chromium folosit de robotul FOREXE (necesită internet)"; GroupDescription: "Robot FOREXE:"

[Dirs]
; Logs\ is created empty and never written by Setup (see [Files]): on an upgrade the
; operator's journals stay exactly as they are, the same rule KBot.Updater applies
; (UpdateApplier.PreservedFolders).
Name: "{app}\Logs"

[Files]
; Everything from the staging folder except Migrare\ (own component below) and
; Logs\ (only a placeholder in staging; the folder comes from [Dirs]). Nothing that
; is already in {app} and not in the package is ever deleted: the data folders
; (Asociere\, WorkflowResults\, Extrase\, ...) and kbot_paths.json survive an
; upgrade, as they do an automatic update.
Source: "{#SourceDir}\*";         DestDir: "{app}";         Excludes: "\Migrare\*,\Logs\*"; Flags: ignoreversion recursesubdirs; Components: app
Source: "{#SourceDir}\Migrare\*"; DestDir: "{app}\Migrare";                                 Flags: ignoreversion recursesubdirs; Components: migrare
#ifdef HAVE_RUNTIME
Source: "{#RuntimeSetup}"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not DotNetDesktop8Present
#endif

[Icons]
Name: "{group}\{#MyAppName}";                          Filename: "{app}\{#MyAppExe}";                 WorkingDir: "{app}";         Components: app
Name: "{group}\{#MyAppName} Migrare (Access → MariaDB)"; Filename: "{app}\Migrare\{#MyMigratorExe}"; WorkingDir: "{app}\Migrare"; Components: migrare
Name: "{group}\Dezinstalare {#MyAppName}";             Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}";                    Filename: "{app}\{#MyAppExe}";                 WorkingDir: "{app}";         Tasks: desktopicon

[Run]
#ifdef HAVE_RUNTIME
Filename: "{tmp}\{#ExtractFileName(RuntimeSetup)}"; Parameters: "/install /quiet /norestart"; StatusMsg: "Se instalează .NET Desktop Runtime 8..."; Check: not DotNetDesktop8Present; Flags: waituntilterminated
#endif
; Chromium lands in the LOGGED-IN user's %LOCALAPPDATA%\ms-playwright, so it runs
; as the original (non-elevated) user, not as the admin that approved the UAC prompt.
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\playwright.ps1"" install chromium"; WorkingDir: "{app}"; StatusMsg: "Se descarcă browserul Chromium pentru robotul FOREXE..."; Tasks: playwright; Flags: runasoriginaluser waituntilterminated
Filename: "{app}\{#MyAppExe}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent runasoriginaluser

[UninstallDelete]
; Logs are written at runtime, so Inno would otherwise leave the folder behind.
Type: filesandordirs; Name: "{app}\Logs"

[Code]
// .NET Desktop Runtime 8 (x64) is a prerequisite: the app is published
// framework-dependent. The runtime installer records versions under the 32-bit
// registry view even on x64 (verified: HKLM\SOFTWARE\WOW6432Node\dotnet\...).
// The folder scan is a fallback for machines where the registry entry is absent.
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

// ------------------------------------------------------------------------------
//  Installed-version guard (slice 0067-01).
//
//  The rules mirror the in-app update system (src\KBot.App\Update,
//  src\KBot.Updater, src\KBot.Domain\Update\UpdatePolicy):
//    * the number that counts is the FileVersion of the installed KBot.App.exe
//      (AppUpdateService.CurrentVersion), NOT the registry: KBot.Updater rewrites
//      the files without touching "Programs and Features", so DisplayVersion goes
//      stale after the first automatic update. The registry is a fallback only,
//      for a registered folder whose exe is gone.
//    * four-part comparison, a missing part counts as 0 (UpdatePolicy.Normalize);
//    * installed > package -> refused (no downgrade, ever);
//      installed = package -> asked; refused in silent mode;
//      installed < package -> upgrade, in the folder the app is in;
//    * an upgrade writes only what is in the package: Logs\ is excluded in [Files]
//      and nothing already in the folder is deleted (UpdateApplier rules 2 and 3).
// ------------------------------------------------------------------------------
const
  UninstallKey = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#MyAppId}_is1';

var
  InstalledVersion: String;   // '' = no K-BOT found on this PC
  InstalledDir: String;
  InstalledSource: String;    // 'exe' or 'registry', for the log
  IsUpgrade: Boolean;
  IsReinstall: Boolean;       // same version, operator said yes

// "a.b.c.d" -> packed number. Missing parts are 0, so "1.0.31" = "1.0.31.0"
// (UpdatePolicy.Normalize). False on anything that is not 1..4 numeric parts.
function TryParseVersion(const Text: String; var Packed: Int64): Boolean;
var
  S, Part: String;
  Parts: array[0..3] of Integer;
  I, P, N: Integer;
begin
  Result := False;
  for I := 0 to 3 do
    Parts[I] := 0;
  S := Trim(Text);
  if S = '' then
    Exit;
  I := 0;
  while S <> '' do
  begin
    if I > 3 then
      Exit;
    P := Pos('.', S);
    if P > 0 then
    begin
      Part := Copy(S, 1, P - 1);
      Delete(S, 1, P);
    end
    else
    begin
      Part := S;
      S := '';
    end;
    N := StrToIntDef(Part, -1);
    if (Part = '') or (N < 0) or (N > 65535) then
      Exit;
    Parts[I] := N;
    I := I + 1;
  end;
  Packed := PackVersionComponents(Parts[0], Parts[1], Parts[2], Parts[3]);
  Result := True;
end;

// FileVersion of KBot.App.exe in Dir; '' when the exe is not there or has no
// version resource (logged: a real K-BOT build always carries one).
function ExeVersionIn(const Dir: String): String;
var
  Exe: String;
begin
  Result := '';
  if Dir = '' then
    Exit;
  Exe := AddBackslash(Dir) + '{#MyAppExe}';
  if FileExists(Exe) then
    if not GetVersionNumbersString(Exe, Result) then
    begin
      Log('Installed exe carries no version resource: ' + Exe);
      Result := '';
    end;
end;

function ReadRegString(const ValueName: String; var Value: String): Boolean;
begin
  // Setup runs in 64-bit mode, so the key lives in the 64-bit view; the 32-bit
  // view is checked too in case an older build registered there.
  Result := RegQueryStringValue(HKLM64, UninstallKey, ValueName, Value);
  if not Result then
    Result := RegQueryStringValue(HKLM32, UninstallKey, ValueName, Value);
  if not Result then
    Value := '';
end;

// Fills InstalledVersion / InstalledDir / InstalledSource. Order: the exe in the
// registered folder, the exe in the default folder (installs older than this
// installer have no registry entry), then the registry's DisplayVersion.
procedure DetectInstalled;
var
  RegDir, RegVersion: String;
begin
  InstalledVersion := '';
  InstalledDir := '';
  InstalledSource := '';
  ReadRegString('InstallLocation', RegDir);
  ReadRegString('DisplayVersion', RegVersion);

  InstalledVersion := ExeVersionIn(RegDir);
  if InstalledVersion <> '' then
  begin
    InstalledDir := RemoveBackslashUnlessRoot(RegDir);
    InstalledSource := 'exe';
    Exit;
  end;
  InstalledVersion := ExeVersionIn('{#MyDefaultDir}');
  if InstalledVersion <> '' then
  begin
    InstalledDir := '{#MyDefaultDir}';
    InstalledSource := 'exe';
    Exit;
  end;
  if RegVersion <> '' then
  begin
    InstalledVersion := RegVersion;
    InstalledDir := RemoveBackslashUnlessRoot(RegDir);
    InstalledSource := 'registry';
  end;
end;

// True = this Setup may go on. False = refused, with the reason already shown.
function CheckInstalledVersion: Boolean;
var
  Installed, Incoming: Int64;
  Cmp: Integer;
begin
  Result := True;
  IsUpgrade := False;
  IsReinstall := False;
  DetectInstalled;
  if InstalledVersion = '' then
  begin
    Log('No installed K-BOT found; fresh install of {#AppVersion}.');
    Exit;
  end;
  Log('Installed K-BOT ' + InstalledVersion + ' in "' + InstalledDir + '" (' + InstalledSource +
      '); this package: {#AppVersion}');

  if not TryParseVersion('{#AppVersion}', Incoming) then
    RaiseException('AppVersion is not a valid version number: {#AppVersion}');

  if not TryParseVersion(InstalledVersion, Installed) then
  begin
    Log('Refused: the installed version cannot be parsed.');
    SuppressibleMsgBox(
      'Pe acest calculator există o instalare {#MyAppName} (' + InstalledDir + ') a cărei versiune ' +
      'nu poate fi citită («' + InstalledVersion + '»), deci nu se poate stabili dacă acest pachet este mai nou.' + #13#10#13#10 +
      'Instalarea se oprește. Dezinstalați {#MyAppName} din «Programe și caracteristici», apoi reluați.',
      mbError, MB_OK, IDOK);
    Result := False;
    Exit;
  end;

  Cmp := ComparePackedVersion(Incoming, Installed);
  if Cmp < 0 then
  begin
    Log('Refused: package {#AppVersion} is older than the installed ' + InstalledVersion + '.');
    SuppressibleMsgBox(
      'Pe acest calculator este deja instalat {#MyAppName} ' + InstalledVersion + ' (' + InstalledDir + ').' + #13#10 +
      'Acest pachet conține versiunea {#AppVersion}, mai veche.' + #13#10#13#10 +
      'Instalarea unei versiuni mai vechi peste una mai nouă nu este permisă. ' +
      'Dacă aveți nevoie de versiunea veche, dezinstalați mai întâi {#MyAppName} din «Programe și caracteristici».',
      mbError, MB_OK, IDOK);
    Result := False;
    Exit;
  end;

  if Cmp = 0 then
  begin
    // Not an upgrade. Interactively the operator may still rewrite the same version
    // (a repair); silently the strict rule holds and the same version is refused.
    Result := SuppressibleMsgBox(
      '{#MyAppName} ' + InstalledVersion + ' este deja instalat (' + InstalledDir + ').' + #13#10#13#10 +
      'Reinstalați aceeași versiune? Fișierele aplicației se rescriu; jurnalele (Logs\) și datele locale rămân neatinse.',
      mbConfirmation, MB_YESNO, IDNO) = IDYES;
    if not Result then
    begin
      Log('Refused: same version already installed (operator or silent mode).');
      Exit;
    end;
    IsReinstall := True;
  end;
  IsUpgrade := True;
end;

function InitializeSetup: Boolean;
begin
  Result := CheckInstalledVersion;
  if not Result then
    Exit;
#ifndef HAVE_RUNTIME
  if not DotNetDesktop8Present then
    Result := MsgBox(
      'Pe acest calculator nu este instalat «.NET Desktop Runtime 8 (x64)», de care ' +
      'K-BOT are nevoie ca să pornească.' + #13#10#13#10 +
      'Îl puteți descărca de la:' + #13#10 +
      'https://dotnet.microsoft.com/download/dotnet/8.0' + #13#10#13#10 +
      'Continuați instalarea K-BOT oricum?',
      mbConfirmation, MB_YESNO) = IDYES;
#endif
end;

// On an upgrade the welcome page says what will happen -- the same sentence the
// in-app offer uses -- and the destination is pinned to the folder the app is in.
procedure InitializeWizard;
var
  What: String;
begin
  if not IsUpgrade then
    Exit;
  if IsReinstall then
    What := 'Acest program îl reinstalează (aceeași versiune), în același folder.'
  else
    What := 'Acest program îl actualizează la versiunea {#AppVersion}, în același folder.';
  WizardForm.WelcomeLabel2.Caption :=
    'Pe acest calculator este instalat {#MyAppName} ' + InstalledVersion + ' (' + InstalledDir + ').' + #13#10 +
    What + #13#10#13#10 +
    'Ca la actualizarea automată: se înlocuiesc doar fișierele din pachet, jurnalele (Logs\) și ' +
    'datele locale rămân neatinse, nimic nu se șterge.' + #13#10#13#10 +
    'Dacă {#MyAppName} este pornit, vi se va cere să îl închideți.';
  if InstalledDir <> '' then
    WizardForm.DirEdit.Text := InstalledDir;
end;
