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
; Stable AppId: upgrades replace the same entry in "Programs and Features".
AppId={{A9840797-CE1E-4708-BDC0-73E4CCBC2D3A}
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
; to the exe). The operator may still change it; previous location is remembered.
DefaultDirName=C:\KBOT
UsePreviousAppDir=yes
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

[Files]
; Everything from the staging folder except Migrare\ (own component below).
Source: "{#SourceDir}\*";         DestDir: "{app}";         Excludes: "\Migrare\*"; Flags: ignoreversion recursesubdirs; Components: app
Source: "{#SourceDir}\Migrare\*"; DestDir: "{app}\Migrare";                         Flags: ignoreversion recursesubdirs; Components: migrare
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

function InitializeSetup: Boolean;
begin
  Result := True;
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
