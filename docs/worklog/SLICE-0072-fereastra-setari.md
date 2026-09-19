# SLICE 0072 — Fereastra «Setări»: cinci pagini, comutatoarele operatorului, parola cu doi factori

**Data:** 19.09.2026
**Cererea operatorului:** «i need a configuration form accessible by the user. it will have a left
navbar just like the kbotform. the navbar will contain: 1. info — change his password, 2fa through the
email (the username is the email), commercial info, check for updates; 2. app config with critical
global switches like VerboseLogging or if the showbrowser button is available … settings for the way
PDF, DOC and EXCEL files are open (all the available settings); 3. settings for forexe; 4. theme
settings (ThemeOptionsForm as reference); 5. the logging form (LoginForm.vb). this form will follow all
the theming systems … ALL of its elements in the designer.vb … folder called Setari … including any
DGVs. you will not git anything. no harness tests. no tests»

## Presupuneri declarate

- **Numărul feliei: 0072** — «next free» din `KBOT_STATUS.md` la data cererii.
- **Pagina 5 = fereastra de autentificare** («the logging form» + referința `LoginForm.vb`): ce ține
  minte login-ul (felia 0063), adresa serverului. Comutatorul de *jurnal detaliat* (VerboseLogging)
  stă pe pagina «Aplicație», acolo unde l-a cerut operatorul.
- **Un magazin nou pentru comutatoare**, `%APPDATA%\AVACONT\KBot\app_settings.json` (`AppSettings`).
  Nu în `settings.json`: `SetariFoldere` raportează la pornire ORICE cheie necunoscută (deliberat), iar
  niște booleene acolo ar fi slăbit regula sau ar fi umplut jurnalul. `kbot_paths.json` (Adobe, per
  mașină) și `settings.json` (foldere) rămân unde sunt; fereastra le editează pe loc.
- **Parola = parola MariaDB a operatorului** (modelul «option A» al login-ului): se schimbă cu
  `SET PASSWORD` pe conexiunea operatorului, fără privilegii ale contului de serviciu. Al doilea factor =
  cod pe e-mail (numele de utilizator este e-mailul), trimis de serverul Flask prin SMTP.
- **Datele comerciale** (demo / înregistrat): nu există model pe server; rândul «Tip instalare» există,
  spune ce știe sesiunea și că restul e în lucru.

## Ce s-a schimbat și de ce

### 1. `KBot.Common` — `AppSettings` (nou) + `FeatureSwitches` + `LastLoginStore.Forget`

- `AppSettings.vb`: zece proprietăți (`VerboseLogging As Boolean?`, `LogViewerEnabled`,
  `ShowBrowserButton`, `ForexeHideBrowserChrome`, `ReceptiiCheckedOnOpen`, `AdobeDetachMode`,
  `AdobePopupWatch`, `ExcelRibbon`, `RememberLastLogin`, `RememberLastUnit`), `Current` (încărcat o
  dată), `Load` (lipsă/gol = implicit tăcut, stricat = implicit + log, nu aruncă), `Save` (frontieră
  I/O: log + rearuncă; face instanța `Current` și ridică `Changed`), `Clone`. DTO nullable: o cheie
  lipsă din fișier își păstrează implicitul. **Fiecare implicit = comportamentul de dinainte.**
- `FeatureSwitches.VizualizatorJurnaleActiv` / `ReceptiiBifateLaDeschidere` citesc acum din
  `AppSettings` — apelanții (shell, selectorul de recepții) neatinși, exact rostul clasei.
- `LastLoginStore.Forget()` (butonul «Uită datele memorate»).

### 2. Cine aplică comutatoarele

- `Program.Main`: după `ThemeManager.Initialize`, `RichTextBoxLogger.VerboseLogging` primește valoarea
  explicită din `AppSettings` (dacă există) — înainte de orice consolă.
- `ForexeFooterView`: `btnBrowser.Visible = conectat AndAlso AppSettings.Current.ShowBrowserButton`;
  se abonează la `AppSettings.Changed` (dezabonare în `Dezleaga`), deci butonul dispare/apare fără
  repornire.
- `ForexeRunner.RunJobAsync`: `_executor.HideChromeWhenDocked = AppSettings.Current.ForexeHideBrowserChrome`
  la fiecare lucrare.
- `ReaderHostPreview` + `DdfFisierPreview`: modul de eliberare (A/B) și supraveghetorul de popup-uri nu
  mai sunt constante de compilare — `AdobeHostSettings.ApplyTo(host, log)` (nou, `KBot.Controls\Adobe`),
  cu regula de cădere a folderului (valoare necunoscută → implicit + avertisment în `adobe_preview.log`).
- `DdfFisierPreview.ArataOffice`: panglica Excel din `OfficeHostSettings.CurrentExcelRibbon()` (nou,
  `KBot.Controls\Office`), nu mai e hardcodată `HideDockWindow`.
- `LoginForm`: pre-completarea e-mailului și preselectarea unității respectă `RememberLastLogin` /
  `RememberLastUnit`; cu memoria oprită fișierul `last_login.json` NU se mai scrie deloc.
- `CertificateService.ForgetLastUsedCertificate()` (butonul «Uită certificatul»).

### 3. Parola cu doi factori — server (`PYTHON`) și client (`KBot.Api`, `KBot.Domain`)

- `routes/auth/session_store.py`: **note pe sesiune** (`put_note` / `get_note` / `delete_note`) în
  ambele magazine (memorie: dicționar cu expirare; Redis: o cheie `<prefix>note:<nume>:<token>` cu TTL).
  Codul stă lângă token-ul pe care îl validează garda, deci moare cu sesiunea și e invizibil altui login.
- `routes/auth/mailer.py` (nou): `send_password_code(to, code, minutes)` prin `smtplib` (STARTTLS,
  login opțional), `is_configured()`, `mask_address()` (`a***@domain`). Config din `config.py`:
  `SMTP_HOST/PORT/USER/PASSWORD/FROM/USE_TLS/TIMEOUT`, fiecare suprascriabil din variabila de mediu cu
  același nume. Fără `SMTP_HOST` → `MailNotConfigured` → 503 cu mesaj românesc.
- `routes/auth/auth.py`: `POST /api/auth/password/code` (token; verifică parola actuală cu același
  `_verify_operator` + `LIMITER` ca login-ul, generează 6 cifre cu `secrets`, trimite e-mailul, ține
  **doar hash-ul** SHA-256 ca notă 10 minute) și `POST /api/auth/password/change` (token; codul comparat
  cu `hmac.compare_digest`, ≤ 5 încercări, parolă nouă ≥ 8 caractere și diferită; `SET PASSWORD =
  PASSWORD(%s)` ca operatorul pe serverul K-BOT; apoi **cu bună-credință** pe serverul vechi —
  `legacy_updated` în răspuns). Jurnal: `PASSWORD_CODE`, `PASSWORD_CHANGE`, `AUTH_FAIL`/`AUTH_BLOCKED`.
- `utils/database.py`: `legacy_server_address()` (perechea lui `kbot_server_address`).
- `IAuthApi` / `AuthApi`: `RequestPasswordCodeAsync`, `ChangePasswordAsync`; DTO-uri
  `PasswordCodeInfo` (`email_masked`, `expires_in`) și `PasswordChangeResult` (`ok`, `legacy_updated`)
  în `KBot.Domain\Auth\PasswordChangeInfo.vb`. Nume de câmpuri ASCII pe ambele părți.

### 4. `KBot.App\Setari\` — fereastra și cele cinci pagini (tot în `.Designer.vb`)

- `SetariForm` (`KBotShellForm`, fără chenar, `capBar` + `busyBar` + `navViews` (`KBotNavList`, cinci
  intrări autorite în designer) + `viewHost` + banda de stare cu `lblStatus` / `btnClose`). Paginile se
  creează leneș la prima activare, ca vederile shell-ului; `ShowFor(host, factory)` = o singură
  instanță nemodală, deținută de shell. `OnFormClosing` întreabă fiecare pagină (`ISetariView.CanClose`).
  Escape închide.
- `ISetariView`: `ViewKey`, `Activated()` (re-citește magazinele), `CanClose()`, evenimentele
  `StatusChanged` / `BusyChanged` (banda de stare a ferestrei).
- `SetariInfoView`: contul (operator, unitate, rol, bază/perioadă), «Tip instalare», versiunea
  (`AppUpdateService.CurrentVersion`) + «Caută actualizări» (`RunManualCheckAsync`; `True` → fereastra
  se închide și `Application.Exit()`), **schimbarea parolei în doi pași** (câmpurile pasului al doilea
  se deschid abia după ce codul a plecat; parolele se golesc după orice deznodământ).
- `SetariAplicatieView`: comutatoarele (combo «Implicit / Pornit / Oprit» pentru VerboseLogging, cu
  efect imediat pe consolele deschise; trei bife), documentele (motor / mod / instanță nouă → `kbot_paths.json`
  prin `AdobeViewerSettings.Persist`, același apel ca `DdfDocumentPage`; eliberare A/B, popup, panglica
  Excel → `app_settings.json`), **folderele ca `KBotDataView`** (coloanele `Setare`, `Ce este`,
  `Implicit`, `Calea configurată` — doar ultima editabilă) + «Salvează folderele» (`SetariFoldere.Salveaza`,
  cu nota că se verifică la pornire).
- `SetariForexeView`: starea coordonatorului (urmărește `StateChanged`), certificatul memorat +
  «Uită certificatul», bara browserului andocat, folderele robotului (doar citire, cu tooltip pe calea
  completă).
- `SetariTemaView`: portul `ThemeOptionsForm` (combo de schemă, `PropertyGrid` pe `SchemeOptionsProxy`,
  «Restaurează implicit» / «Salvează», scalare / factor / DPI-unaware / mărime text / baza ferestrei) —
  aceeași logică, rând cu rând; modificările nesalvate se întreabă la închiderea ferestrei.
- `SetariAutentificareView`: cele două bife de memorie, ce e memorat acum + «Uită datele memorate»,
  adresa serverului și timpul de așteptare (doar citire).
- Toate paginile: `KBotThemedUserControl` + **`IThemedContainer`** (copiii primesc regulile generice,
  apoi pagina își pune suprafața, etichetele estompate și stilurile de buton). Tooltip-uri doar prin
  `KBotToolTip`. Mesaje doar prin `KBotMessage.Show`.
- `KbotForm`: rândul «S&etări…» în meniul butonului de opțiuni (`OPT_SETARI`); constructorul primește
  `Func(Of SetariForm)`. `Program.ConfigureServices`: `SetariForm` transient + fabrica.

### 5. Documentație

- `docs\SETARI_UTILIZATOR.md` §6: fereastra, tabelul cheilor din `app_settings.json`, fluxul parolei
  și configurarea SMTP.

## Fișiere atinse

Noi: `src\KBot.Common\AppSettings.vb`, `src\KBot.Controls\Adobe\AdobeHostSettings.vb`,
`src\KBot.Controls\Office\OfficeHostSettings.vb`, `src\KBot.Domain\Auth\PasswordChangeInfo.vb`,
`src\KBot.App\Setari\{ISetariView, SetariForm(+Designer), SetariInfoView(+Designer),
SetariAplicatieView(+Designer), SetariForexeView(+Designer), SetariTemaView(+Designer),
SetariAutentificareView(+Designer)}.vb`, `PYTHON\routes\auth\mailer.py`,
`docs\worklog\SLICE-0072-fereastra-setari.md`.
Modificate: `src\KBot.Common\FeatureSwitches.vb`, `LastLoginStore.vb`; `src\KBot.Api\IAuthApi.vb`,
`AuthApi.vb`; `src\KBot.Forexe\ForexeRunner.vb`, `Services\CertificateService.vb`;
`src\KBot.App\Program.vb`, `KbotForm.vb`, `LoginForm.vb`, `Forexe\ForexeFooterView.vb`,
`Views\Ddf\DdfFisierPreview.vb`, `Views\Ddf\ReaderHostPreview.vb`; `PYTHON\routes\auth\auth.py`
(**gitignored**), `PYTHON\config.py` (**gitignored**), `PYTHON\routes\auth\session_store.py`,
`PYTHON\utils\database.py`; `docs\SETARI_UTILIZATOR.md`, `docs\worklog\KBOT_STATUS.md`.

## Rezultate

- `dotnet build`: `src\KBot.App` **Debug și Release**, `src\KBot.DevHarness`, `src\KBot.Forexe.Editor`,
  `src\KBot.Migrator` — **0 erori, 0 avertismente** (fiecare trage `Common`, `Api`, `Domain`,
  `Controls`, `Forexe`, `Theming`).
- Python: `ast.parse` verde pe `auth.py`, `mailer.py`, `session_store.py`, `database.py`, `config.py`;
  fum pe `mask_address` și pe notele `SessionStore` (put/get/expire) în `PYTHON\.venv`. **Nicio suită de
  teste rulată** (cerut).
- **Randare pe ecran, în afara monitorului** (`DrawToBitmap`, nu un test): fereastra pe fiecare
  pagină, pe Classic / Întunecat / Modern, la 150 %. Prima randare a prins un defect real —
  tabelele imbricate `AutoSize` din rândurile `AutoSize` erau tăiate, fiindcă
  `KBotTableLayoutPanel.GetPreferredSize` (regula 0062: doar din conținut) sub-raportează un
  `KBotTextField` / un buton andocat, pe când motorul de bază le așază la înălțimea lor. Reparat cu
  `AutoFitToTheme = False` pe tabelele imbricate (au doar rânduri AutoSize, deci potrivirea 0062 n-avea
  ce face acolo); a doua randare arată toate cele cinci pagini întregi, grila de foldere cu rândurile ei
  și pagina «Aplicație» derulabilă.
- Fără git (cerut). Fără teste (cerut).

## Neverificat / amânat

- **Serverul**: `auth.py` și `config.py` sunt **gitignored** — copia care rulează e cea de pe VPS.
  Rutele noi, `mailer.py`, notele din `session_store.py` și `legacy_server_address` din `database.py`
  trebuie duse acolo de mână, iar **contul SMTP** pus în `config.py` de pe server (sau în mediu). Până
  atunci butonul «Trimite codul» răspunde onest cu 503.
- **`SET PASSWORD` nu a rulat pe MariaDB** (nici nou, nici vechi). Presupunere: contul operatorului își
  poate schimba propria parolă fără privilegii (comportamentul standard MariaDB; o politică
  `strict_password_validation` / `simple_password_check` de pe server poate refuza o parolă — mesajul
  ajunge la operator ca «Parola nu a putut fi schimbată pe server» + detaliul în jurnalul serverului).
- **Serverul vechi** primește schimbarea cu bună-credință; dacă refuză, operatorul află și rămâne cu
  două parole până le aliniază un administrator. Nu există (încă) un drum de re-aliniere din aplicație.
- **Datele comerciale** (demo / înregistrat) — fără sursă pe server; rândul există, conținutul e «în lucru».
- `RedisSessionStore.size()` numără și notele (același prefix) — puține și scurte, dar cifra din logul
  `AUTH_LOGIN` poate fi cu 1 mai mare cât timp un cod e viu.
- Fereastra a fost VĂZUTĂ doar prin `DrawToBitmap` la 150 %, în afara monitorului: nici clic, nici tastă,
  nici 100 % / 125 %, nici designerul VS. Butonul «Caută actualizări» și fluxul parolei n-au vorbit cu
  niciun server.
