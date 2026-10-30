# SLICE-0072 — Istoricul ferestrei Setări

Consolidare D001-16: baza și etapele 01…03 sunt reunite cu corpurile tehnice, fișierele, probele și restanțele păstrate. Cererile/regulile/comenzile citate sunt istorice; nu înlocuiesc autorizarea actuală a lucrării documentare.

Baza are cinci pagini, 01 adaugă Jurnal și mută opțiunile Adobe în dialog; 02 introduce paginile ascunse/avansate, 03 repară constructorul paginii FOREXE. DrawToBitmap este randare statică în afara monitorului, nu probă prin clic/taste; testele doar scrise din 01/02 nu devin teste rulate prin reunire.

## SLICE 0072 — Fereastra «Setări»: cinci pagini, comutatoarele operatorului, parola cu doi factori <a id="slice-0072-baza"></a>

**Data:** 19.09.2026 **Cererea operatorului:** «i need a configuration form accessible by the user. it will have a left navbar just like the kbotform. the navbar will contain: 1. info — change his password, 2fa through the email (the username is the email), commercial info, check for updates; 2. app config with critical global switches like VerboseLogging or if the showbrowser button is available … settings for the way PDF, DOC and EXCEL files are open (all the available settings); 3. settings for forexe; 4. theme settings (ThemeOptionsForm as reference); 5. the logging form (LoginForm.vb). this form will follow all the theming systems … ALL of its elements in the designer.vb … folder called Setari … including any DGVs. you will not git anything. no harness tests. no tests»

### Presupuneri declarate

- **Numărul feliei: 0072** — «next free» din `KBOT_STATUS.md` la data cererii.
- **Pagina 5 = fereastra de autentificare** («the logging form» + referința `LoginForm.vb`): ce ține minte login-ul (felia 0063), adresa serverului. Comutatorul de *jurnal detaliat* (VerboseLogging) stă pe pagina «Aplicație», acolo unde l-a cerut operatorul.
- **Magazinul comutatoarelor:** `%APPDATA%\AVACONT\KBot\app_settings.json` (`AppSettings`). `SetariFoldere` raportează orice cheie necunoscută din `settings.json`; booleenele sunt separate pentru a păstra regula. `kbot_paths.json` (Adobe, per mașină) și `settings.json` (foldere) rămân editate pe loc.
- **Parola = parola MariaDB a operatorului** (modelul «option A» al login-ului): se schimbă cu `SET PASSWORD` pe conexiunea operatorului, fără privilegii ale contului de serviciu. Al doilea factor = cod pe e-mail (numele de utilizator este e-mailul), trimis de serverul Flask prin SMTP.
- **Datele comerciale** (demo / înregistrat): nu există model pe server; rândul «Tip instalare» există, spune ce știe sesiunea și că restul e în lucru.

### Ce s-a schimbat și de ce

#### 1. `KBot.Common` — `AppSettings` (nou) + `FeatureSwitches` + `LastLoginStore.Forget`

- `AppSettings.vb`: zece proprietăți (`VerboseLogging As Boolean?`, `LogViewerEnabled`, `ShowBrowserButton`, `ForexeHideBrowserChrome`, `ReceptiiCheckedOnOpen`, `AdobeDetachMode`, `AdobePopupWatch`, `ExcelRibbon`, `RememberLastLogin`, `RememberLastUnit`), `Current` (încărcat o dată), `Load` (lipsă/gol = implicit tăcut, stricat = implicit + log, nu aruncă), `Save` (frontieră I/O: log + rearuncă; face instanța `Current` și ridică `Changed`), `Clone`. DTO nullable: o cheie lipsă din fișier își păstrează implicitul. **Fiecare implicit = comportamentul de dinainte.**
- `FeatureSwitches.VizualizatorJurnaleActiv` / `ReceptiiBifateLaDeschidere` citesc acum din `AppSettings` — apelanții (shell, selectorul de recepții) neatinși, exact rostul clasei.
- `LastLoginStore.Forget()` (butonul «Uită datele memorate»).

#### 2. Cine aplică comutatoarele

- `Program.Main`: după `ThemeManager.Initialize`, `RichTextBoxLogger.VerboseLogging` primește valoarea explicită din `AppSettings` (dacă există) — înainte de orice consolă.
- `ForexeFooterView`: `btnBrowser.Visible = conectat AndAlso AppSettings.Current.ShowBrowserButton`; se abonează la `AppSettings.Changed` (dezabonare în `Dezleaga`), deci butonul dispare/apare fără repornire.
- `ForexeRunner.RunJobAsync`: `_executor.HideChromeWhenDocked = AppSettings.Current.ForexeHideBrowserChrome` la fiecare lucrare.
- `ReaderHostPreview` + `DdfFisierPreview`: modul de eliberare (A/B) și supraveghetorul de popup-uri nu mai sunt constante de compilare — `AdobeHostSettings.ApplyTo(host, log)` (nou, `KBot.Controls\Adobe`), cu regula de cădere a folderului (valoare necunoscută → implicit + avertisment în `adobe_preview.log`).
- `DdfFisierPreview.ArataOffice`: panglica Excel din `OfficeHostSettings.CurrentExcelRibbon()` (nou, `KBot.Controls\Office`), nu mai e hardcodată `HideDockWindow`.
- `LoginForm`: pre-completarea e-mailului și preselectarea unității respectă `RememberLastLogin` / `RememberLastUnit`; cu memoria oprită fișierul `last_login.json` NU se mai scrie deloc.
- `CertificateService.ForgetLastUsedCertificate()` (butonul «Uită certificatul»).

#### 3. Parola cu doi factori — server (`PYTHON`) și client (`KBot.Api`, `KBot.Domain`)

- `routes/auth/session_store.py`: **note pe sesiune** (`put_note` / `get_note` / `delete_note`) în ambele magazine (memorie: dicționar cu expirare; Redis: o cheie `<prefix>note:<nume>:<token>` cu TTL). Codul stă lângă token-ul pe care îl validează garda, deci moare cu sesiunea și e invizibil altui login.
- `routes/auth/mailer.py` (nou): `send_password_code(to, code, minutes)` prin `smtplib` (STARTTLS, login opțional), `is_configured()`, `mask_address()` (`a***@domain`). Config din `config.py`: `SMTP_HOST/PORT/USER/PASSWORD/FROM/USE_TLS/TIMEOUT`, fiecare suprascriabil din variabila de mediu cu același nume. Fără `SMTP_HOST` → `MailNotConfigured` → 503 cu mesaj românesc.
- `routes/auth/auth.py`: `POST /api/auth/password/code` (token; verifică parola actuală cu același `_verify_operator` + `LIMITER` ca login-ul, generează 6 cifre cu `secrets`, trimite e-mailul, ține **doar hash-ul** SHA-256 ca notă 10 minute) și `POST /api/auth/password/change` (token; codul comparat cu `hmac.compare_digest`, ≤ 5 încercări, parolă nouă ≥ 8 caractere și diferită; `SET PASSWORD = PASSWORD(%s)` ca operatorul pe serverul K-BOT; apoi **cu bună-credință** pe serverul vechi — `legacy_updated` în răspuns). Jurnal: `PASSWORD_CODE`, `PASSWORD_CHANGE`, `AUTH_FAIL`/`AUTH_BLOCKED`.
- `utils/database.py`: `legacy_server_address()` (perechea lui `kbot_server_address`).
- `IAuthApi` / `AuthApi`: `RequestPasswordCodeAsync`, `ChangePasswordAsync`; DTO-uri `PasswordCodeInfo` (`email_masked`, `expires_in`) și `PasswordChangeResult` (`ok`, `legacy_updated`) în `KBot.Domain\Auth\PasswordChangeInfo.vb`. Nume de câmpuri ASCII pe ambele părți.

#### 4. `KBot.App\Setari\` — fereastra și cele cinci pagini (tot în `.Designer.vb`)

- `SetariForm` (`KBotShellForm`, fără chenar, `capBar` + `busyBar` + `navViews` (`KBotNavList`, cinci intrări autorite în designer) + `viewHost` + banda de stare cu `lblStatus` / `btnClose`). Paginile se creează leneș la prima activare, ca vederile shell-ului; `ShowFor(host, factory)` = o singură instanță nemodală, deținută de shell. `OnFormClosing` întreabă fiecare pagină (`ISetariView.CanClose`). Escape închide.
- `ISetariView`: `ViewKey`, `Activated()` (re-citește magazinele), `CanClose()`, evenimentele `StatusChanged` / `BusyChanged` (banda de stare a ferestrei).
- `SetariInfoView`: contul (operator, unitate, rol, bază/perioadă), «Tip instalare», versiunea (`AppUpdateService.CurrentVersion`) + «Caută actualizări» (`RunManualCheckAsync`; `True` → fereastra se închide și `Application.Exit()`), **schimbarea parolei în doi pași** (câmpurile pasului al doilea se deschid abia după ce codul a plecat; parolele se golesc după orice deznodământ).
- `SetariAplicatieView`: comutatoarele (combo «Implicit / Pornit / Oprit» pentru VerboseLogging, cu efect imediat pe consolele deschise; trei bife), documentele (motor / mod / instanță nouă → `kbot_paths.json` prin `AdobeViewerSettings.Persist`, același apel ca `DdfDocumentPage`; eliberare A/B, popup, panglica Excel → `app_settings.json`), **folderele ca `KBotDataView`** (coloanele `Setare`, `Ce este`, `Implicit`, `Calea configurată` — doar ultima editabilă) + «Salvează folderele» (`SetariFoldere.Salveaza`, cu nota că se verifică la pornire).
- `SetariForexeView`: starea coordonatorului (urmărește `StateChanged`), certificatul memorat + «Uită certificatul», bara browserului andocat, folderele robotului (doar citire, cu tooltip pe calea completă).
- `SetariTemaView`: portul `ThemeOptionsForm` (combo de schemă, `PropertyGrid` pe `SchemeOptionsProxy`, «Restaurează implicit» / «Salvează», scalare / factor / DPI-unaware / mărime text / baza ferestrei) — aceeași logică, rând cu rând; modificările nesalvate se întreabă la închiderea ferestrei.
- `SetariAutentificareView`: cele două bife de memorie, ce e memorat acum + «Uită datele memorate», adresa serverului și timpul de așteptare (doar citire).
- Toate paginile: `KBotThemedUserControl` + **`IThemedContainer`** (copiii primesc regulile generice, apoi pagina își pune suprafața, etichetele estompate și stilurile de buton). Tooltip-uri doar prin `KBotToolTip`. Mesaje doar prin `KBotMessage.Show`.
- `KbotForm`: rândul «S&etări…» în meniul butonului de opțiuni (`OPT_SETARI`); constructorul primește `Func(Of SetariForm)`. `Program.ConfigureServices`: `SetariForm` transient + fabrica.

#### 5. Documentație

- `docs\SETARI_UTILIZATOR.md` §6: fereastra, tabelul cheilor din `app_settings.json`, fluxul parolei și configurarea SMTP.

### Fișiere atinse

Noi: `src\KBot.Common\AppSettings.vb`, `src\KBot.Controls\Adobe\AdobeHostSettings.vb`, `src\KBot.Controls\Office\OfficeHostSettings.vb`, `src\KBot.Domain\Auth\PasswordChangeInfo.vb`, `src\KBot.App\Setari\{ISetariView, SetariForm(+Designer), SetariInfoView(+Designer), SetariAplicatieView(+Designer), SetariForexeView(+Designer), SetariTemaView(+Designer), SetariAutentificareView(+Designer)}.vb`, `PYTHON\routes\auth\mailer.py`, `docs\worklog\SLICE-0072-fereastra-setari.md`. Modificate: `src\KBot.Common\FeatureSwitches.vb`, `LastLoginStore.vb`; `src\KBot.Api\IAuthApi.vb`, `AuthApi.vb`; `src\KBot.Forexe\ForexeRunner.vb`, `Services\CertificateService.vb`; `src\KBot.App\Program.vb`, `KbotForm.vb`, `LoginForm.vb`, `Forexe\ForexeFooterView.vb`, `Views\Ddf\DdfFisierPreview.vb`, `Views\Ddf\ReaderHostPreview.vb`; `PYTHON\routes\auth\auth.py` (**gitignored**), `PYTHON\config.py` (**gitignored**), `PYTHON\routes\auth\session_store.py`, `PYTHON\utils\database.py`; `docs\SETARI_UTILIZATOR.md`, `docs\worklog\KBOT_STATUS.md`.

### Rezultate

- `dotnet build`: `src\KBot.App` **Debug și Release**, `src\KBot.DevHarness`, `src\KBot.Forexe.Editor`, `src\KBot.Migrator` — **0 erori, 0 avertismente** (fiecare trage `Common`, `Api`, `Domain`, `Controls`, `Forexe`, `Theming`).
- Python: `ast.parse` verde pe `auth.py`, `mailer.py`, `session_store.py`, `database.py`, `config.py`; fum pe `mask_address` și pe notele `SessionStore` (put/get/expire) în `PYTHON\.venv`. **Nicio suită de teste rulată** (cerut).
- **Randare pe ecran, în afara monitorului** (`DrawToBitmap`, nu un test): fereastra pe fiecare pagină, pe Classic / Întunecat / Modern, la 150 %. Prima randare a prins un defect real — tabelele imbricate `AutoSize` din rândurile `AutoSize` erau tăiate, fiindcă `KBotTableLayoutPanel.GetPreferredSize` (regula 0062: doar din conținut) sub-raportează un `KBotTextField` / un buton andocat, pe când motorul de bază le așază la înălțimea lor. Reparat cu `AutoFitToTheme = False` pe tabelele imbricate (au doar rânduri AutoSize, deci potrivirea 0062 n-avea ce face acolo); a doua randare arată toate cele cinci pagini întregi, grila de foldere cu rândurile ei și pagina «Aplicație» derulabilă.
- Fără git (cerut). Fără teste (cerut).

### Neverificat / amânat

- **Serverul**: `auth.py` și `config.py` sunt **gitignored** — copia care rulează e cea de pe VPS. Rutele noi, `mailer.py`, notele din `session_store.py` și `legacy_server_address` din `database.py` trebuie duse acolo de mână, iar **contul SMTP** pus în `config.py` de pe server (sau în mediu). Până atunci butonul «Trimite codul» răspunde onest cu 503.
- **`SET PASSWORD` nu a rulat pe MariaDB** (nici nou, nici vechi). Presupunere: contul operatorului își poate schimba propria parolă fără privilegii (comportamentul standard MariaDB; o politică `strict_password_validation` / `simple_password_check` de pe server poate refuza o parolă — mesajul ajunge la operator ca «Parola nu a putut fi schimbată pe server» + detaliul în jurnalul serverului).
- **Serverul vechi** primește schimbarea cu bună-credință; dacă refuză, operatorul află și rămâne cu două parole până le aliniază un administrator. Nu există (încă) un drum de re-aliniere din aplicație.
- **Datele comerciale** (demo / înregistrat) — fără sursă pe server; rândul există, conținutul e «în lucru».
- `RedisSessionStore.size()` numără și notele (același prefix) — puține și scurte, dar cifra din logul `AUTH_LOGIN` poate fi cu 1 mai mare cât timp un cod e viu.
- Fereastra a fost VĂZUTĂ doar prin `DrawToBitmap` la 150 %, în afara monitorului: nici clic, nici tastă, nici 100 % / 125 %, nici designerul VS. Butonul «Caută actualizări» și fluxul parolei n-au vorbit cu niciun server.

## SLICE 0072-01 — Setări: pagina «Jurnal», dialogul ferestrei găzduite Adobe, cursorul cu opriri, meniurile barei de titlu <a id="slice-0072-01"></a>

**Data:** 20.09.2026 **Cererea operatorului:** «in the setari: 1. i added a new button - Jurnal. that needs to show the journal form (but make it a view) … the elements in the DGV must be sorted descending. the mesaj column must be hidden and that info only shown in the bottom txtDetaliu (which must be a custom Textbox). 2. in aplicatie page: the extra settings for the pdf opening MUST NOT be on the view, but when the motor previzualizare is set to Fereastra Gazduita - a new small popup form will open with the extra settings. 3. in temaview, the slider for the text size must have a few snapping points (at 100%, 110% and 125%). in capbar custom control: the themeview button will ONLY show the slider (if using the "font din tema") also with the snapping points and the existing themes. that's all. the setari button will only show the Jurnal (if enabled from setari) and the Setari option. if jurnal is not enabled, then the setari form will open directly without a popup menu. also, the two options will have icons and a separator»

### Presupuneri declarate

- **Numărul feliei: 0072-01** — a doua trecere peste fereastra «Setări» (0072), nu o felie nouă; «next free» rămâne 0073.
- **`LogViewerForm` rămâne**, dar goală pe dinăuntru: bara de titlu + pagina nouă andocată. O folosesc în continuare launcher-ul de pornire («Jurnale», fără shell și fără login) și bancul de probă (`ILogViewerLauncher`); shell-ul nu o mai deschide. Testele ei existente merg mai departe prin cârligele `Debug*`, înaintate una la una către pagină.
- **«Sincronizare (server)» eliminată** din meniul de opțiuni la cererea «only the Jurnal … and the Setari option». `KbotForm.SincronizeazaAsync` rămâne fără apel UI; operatorul va decide mutarea sau ștergerea.
- **«Font din temă» mutat** lângă cursor în pagina «Temă», ca acesta să poată fi reactivat. Meniul cerut: «will ONLY show the slider (if using the font din tema) … and the existing themes».
- **«Opțiuni temă…» și «Stiluri…» eliminate** din meniu fără înlocuitor: prima este pagina «Temă» portată în 0072; `ThemeEditorForm` (excepții pe controale) rămâne fără apel UI, chestiune deschisă.
- **«Opțiuni fereastră găzduită…»** sub combo, activ doar pentru acel motor, redeschide dialogul fără comutarea motorului dus-întors. Dialogul se deschide și la alegerea motorului, conform cererii; setările nu rămân pe pagină.
- **Toleranța de lipire: 3 unități** (procente): 97–103 → 100, 107–113 → 110, 122–128 → 125. Doar la mouse; săgețile merg în pași de 5 și nimeresc singure punctele. Aceleași trei puncte în meniu (`KBotCaptionBar.TextScaleSnapPoints`) și în pagina «Temă».

### Ce s-a schimbat și de ce

#### 1. `KBot.Controls` — cursorul cu puncte de oprire; meniul de temă redus

- `Popup/CustomPopupItem.vb`: `SliderSnapPoints As Integer()` (gol = liber), `SliderSnapTolerance = 3`, `SnapSliderValue(v)` (regula pură), fabrica `Slider(key, text, min, max, value, snapPoints)` (punctele din afara intervalului se aruncă, dublurile se strâng, lista iese sortată).
- `Popup/CustomPopup.Slider.vb`: `SliderValueAt` trece valoarea brută prin `SnapSliderValue` (mouse); `DrawSliderRow` desenează liniuțe fine în culoarea conturului la fiecare punct, SUB deget și sub partea parcursă; `SliderXForValue` e inversa lui `SliderValueAt`, ca liniuțele și degetul să stea pe același calcul.
- `CaptionBar/KBotCaptionBar.ThemeButton.vb`: `ConstruiesteElementeleMeniului` include cursorul dacă `ShowTextScaleSlider` ȘI `ThemeManager.WritesFormFont`, separator numai între părți, apoi schemele alegibile. Eliminate: «Font din temă» cu bifa/`BifaPentru`, «Opțiuni temă…», «Stiluri…», constantele lor, `DeschideOptiunileDeTema`, `DeschideEditorulDeStiluri`. `ShowThemeOptions`/`ShowThemeEditor` sunt șterse, nu inerte (no-op tăcut interzis), inclusiv scrierile din 12 `.Designer.vb` (11 `KBot.App`, `MigratorForm` din `KBot.Migrator`). `TextScaleSnapPoints = {100, 110, 125}` este public pentru pagina «Temă».
- `CaptionBar/KBotCaptionBar.md`, `Popup/CustomPopup.md`: actualizate.

#### 2. `KBot.App` — pagina «Jurnal» și `LogViewerForm` ca simplu înveliș

- `Setari/SetariJurnalView.vb` + `.Designer.vb` (noi): corpul `LogViewerForm` devine `KBotThemedUserControl`, cu `ISetariView`/`IThemedContainer`. Diferențe: **(a)** `OrdoneazaCeleMaiNoiPrimele`: descrescător stabil după marcajul corectat, o dată după încărcare, identic pentru local/server/cârligul de test; fără dată, la coadă în ordinea fișierului. **(b)** Fără coloana «Mesaj»; text întreg în `txtDetaliu`, acum `KBotTextBox` (Consolas, ambele scrollbars tematizate, fără wrap). **(c)** Proprietatea `ApiClient` înlocuiește argumentul constructorului pentru gazda designerului fără parametri. **(d)** Prima `Activated()` construiește lista și încarcă «Toate fișierele»; următoarele recitesc selecția, deoarece jurnalele pot crește între pagini. **(e)** `StatusChanged`/`BusyChanged` urcă spre banda «Setări». **(f)** `AratăGol` devine `ArataGol`. Comentarii englezești; numele controalelor/metodelor private rămân cele mutate din fereastră, referite de teste/cerere (`txtDetaliu`).
- `Views/LogViewerForm.vb` + `.Designer.vb`: rescrise ca înveliș — `pnlRoot` cu `capBar` sus și `jurnal As SetariJurnalView` andocat; `OnLoad` cheamă `jurnal.Activated()`; cele nouă cârlige `Debug*` înaintează către pagină (+ `DebugIntrareaRandului(index)`, nou).
- `Setari/SetariForm.vb`: constructor cu `apiClient As IApiClient` (DI îl rezolvă singur); `CreateView("jurnal")`; `ShowPage(key)` public (cheie goală → `ArgumentException`). `SetariForm.Designer.vb`: rândul «Jurnal» + separatorul, adăugate de operator (necomise), au rămas cum le-a pus.
- `KbotForm.vb`: meniul butonului de opțiuni = «Arată jurnal» (pictograma editorului de text) + separator + «Setări…» (pictograma de setări); cu `LogViewerEnabled` stins **nu se deschide niciun meniu**, clicul deschide direct fereastra «Setări». `ShowLog()` = `SetariForm.ShowFor(...).ShowPage("jurnal")` (aceeași fereastră nemodală, o singură instanță). Câmpul `_logViewer`, `OPT_SINCRONIZARE` și rândul lui au plecat; `MeniuOptiuni_ItemClicked` nu mai e `Async`.

#### 3. `KBot.App` — dialogul ferestrei găzduite Adobe

- `Setari/AdobeGazduireForm.vb` + `.Designer.vb` (noi): `KBotThemedForm` cu titlu, explicație, mod vizualizator/instanță nouă `/n`/eliberare fereastră/fereastră plutitoare, «Renunță»/«Salvează». Citește la `Load`, nu în constructor; numai «Salvează» scrie ambele magazine: `AdobeViewerSettings.Persist` cu `WindowHost` și `AppSettings` clonat/salvat. `Rezumat`: linie pentru banda ferestrei. `DetachItem` mutat aici din pagina unde era privat.
- `Setari/SetariAplicatieView.vb` + `.Designer.vb`: «Documente» păstrează combo-ul motorului și `btnAdobeGazduire` («Opțiuni fereastră găzduită…», activ numai pe `WindowHost`); elimină `cboAdobeMod`, `cboAdobeInst`, `cboAdobeDetach`, `chkAdobePopup` cu etichete. Alegerea salvează motorul, recitind neschimbate celelalte două valori din `kbot_paths.json`, și pentru «Fereastră găzduită» deschide dialogul modal peste «Setări»; încărcarea cu `_suppress` nu-l deschide. Tabel: 6 → 3 rânduri; «Foldere» urcă 122 px.

#### 4. `KBot.App` — pagina «Temă»

- `Setari/SetariTemaView.vb` + `.Designer.vb`: `chkThemeFont` («Font din temă», lângă valoarea cursorului) legat la `ThemeManager.WritesFormFont` (setterul persistă și difuzează). Cursorul `TrackBar` se lipește pe `Scroll` (gestul), nu pe `ValueChanged` (valoarea din magazin trebuie să cadă exact unde spune magazinul): `Lipeste(v)` e regula pură, `_snapping` oprește reintrarea.

#### 5. Versiuni

`KBot.App` 1.0.30 ▸ **1.0.31**, `KBot.Controls` 1.50 ▸ **1.51**, `KBot.Migrator` 1.9 ▸ **1.10** (doar `MigratorForm.Designer.vb` măturat). `AssemblyVersion` neatins.

### Fișiere atinse

**Noi:** `src/KBot.App/Setari/SetariJurnalView.vb`, `SetariJurnalView.Designer.vb`, `AdobeGazduireForm.vb`, `AdobeGazduireForm.Designer.vb`, `docs/worklog/SLICE-0072-01-setari-jurnal-si-meniuri.md`.

**Modificate:** `src/KBot.Controls/Popup/CustomPopupItem.vb`, `CustomPopup.Slider.vb`, `CustomPopup.md`, `src/KBot.Controls/CaptionBar/KBotCaptionBar.vb`, `KBotCaptionBar.ThemeButton.vb`, `KBotCaptionBar.md`, `src/KBot.Controls/KBot.Controls.vbproj`; `src/KBot.App/KbotForm.vb`, `KbotForm.Designer.vb` (pictograma butonului de opțiuni — schimbarea operatorului), `Setari/SetariForm.vb`, `SetariForm.Designer.vb`, `Setari/SetariAplicatieView.vb`, `SetariAplicatieView.Designer.vb`, `Setari/SetariTemaView.vb`, `SetariTemaView.Designer.vb`, `Views/LogViewerForm.vb`, `LogViewerForm.Designer.vb`, `My Project/Resources.*` (+ două PNG-uri noi în `Resources/`, aduse de operator), `KBot.App.vbproj`; măturarea `ShowThemeEditor/ShowThemeOptions` în `DDF_EDIT/DdfEditForm`, `Forexe/AlegereUnitateForm`, `Forexe/AsociereBenziForm`, `Forexe/AsociereForm`, `Forexe/GraficeAsociereForm`, `ORD_EDIT/OrdEditForm`, `Views/GraficRezervariForm`, `Views/Ord/OrdTextForm`, `Views/Ord/OrdZiuaForm`, `LoginForm` (toate `.Designer.vb`), `src/KBot.Migrator/MigratorForm.Designer.vb`, `KBot.Migrator.vbproj`; `docs/SETARI_UTILIZATOR.md` §6; `docs/worklog/KBOT_STATUS.md`.

**Teste (scrise, NERULATE — regula casei):** `tests/KBot.Controls.Tests/KBotCaptionBarThemeButtonTests.vb` (trei teste ale uneltelor rescrise ca `Meniul_are_doar_cursorul_si_schemele`, `Cursorul_de_marime_are_punctele_de_oprire`, `Fara_cursor_raman_doar_schemele`; teoria de designer și serializarea explicită fără cele două proprietăți șterse), `tests/KBot.Controls.Tests/CustomPopupSliderTests.vb` (+ `Valoarea_se_lipeste_de_punctele_de_oprire` ×9, `Fara_puncte_de_oprire_valoarea_ramane`, `Punctele_de_oprire_se_curata_la_construire`, `Mouse_ul_se_lipeste_de_punctul_apropiat`, `Sagetile_nu_se_lipesc`; `Meniul_de_tema_incepe_cu_cursorul_de_marime` fără rândul de font), `tests/KBot.App.Tests/LogViewerFormTests.vb` (cinci coloane fără «mesaj», `txtDetaliu` e `KBotTextBox`,
+ `Randurile_SuntOrdonateDescrescator_CeleMaiNoiPrimele`).

### Rezultate

- `dotnet build` pe `src\KBot.Controls` (Debug), `src\KBot.App` (Debug + Release), `src\KBot.Migrator` (Release), `src\KBot.Forexe.Editor` (Release): **0 erori, 0 avertismente**.
- Proiectele de test **nu au fost construite și nu au fost rulate** (regula casei); testele noi și cele rescrise sunt scrise, nu dovedite.
- **Văzut prin `DrawToBitmap`, în afara monitorului** (un proiect de o clipă în scratchpad, șters după): `LogViewerForm` (deci pagina «Jurnal») pe Classic / Dark / Modern — cinci coloane, panoul de detaliu cu chenarul casei; `AdobeGazduireForm` pe cele trei scheme; `SetariTemaView` cu bifa «Font din temă» lângă cursor; `SetariAplicatieView` cu secțiunea «Documente» la trei rânduri și butonul sub combo; meniul de temă (`CustomPopup`) cu cursorul la 110 %: liniuțele de la 100 și 125 se văd de o parte și de alta a degetului, cea de la 110 e sub el.

### Nerulat / neverificat / amânat

- **Nimic apăsat cu mâna**: tragerea cursorului (lipirea la mouse), deschiderea dialogului Adobe la alegerea motorului, meniul butonului de opțiuni cu / fără «Arată jurnal», comutarea paginii «Jurnal» din shell — toate numai prin cod și randări statice.
- **`SincronizeazaAsync` fără cale de apel** din interfață (vezi presupunerile). De hotărât.
- **`ThemeEditorForm` («Stiluri…») și `ThemeOptionsForm` fără cale de apel** din interfață; `ThemeOptionsForm` e dublată de pagina «Temă», `ThemeEditorForm` nu are înlocuitor.
- Pagina «Jurnal» în fereastra «Setări» (946 × 735 la 150 %) e mai strâmtă decât fostul `LogViewerForm` (1173 × 869): coloana fișierelor e fixată la 250 px, restul curge; de văzut pe ecran dacă rândul de filtre are loc la lățimea minimă a ferestrei (1000 px).
- Terminațiile de linie: câteva `.Designer.vb` erau deja cu terminații amestecate (salvări VS); git le normalizează la comitere.

## SLICE 0072-02 — «Activează opțiuni avansate» (24.09.2026) <a id="slice-0072-02"></a>

### Request

Operator, 24.09.2026: a checkbox «Activeaza optiuni avansate» on the application page of Setări. It shows or hides four pages: the «Documente» tab of «Aplicație», «Pagina FOREXE», «Temă» and «Căi fișiere». Ticking it asks for a password.

### What changed

- `AppSettings.AdvancedOptions` (JSON `AdvancedOptions`, default False, saved per Windows user).
- `SetariAplicatieView`: checkbox `chkAvansate` on the «Generale» tab (row 4 of `tlyComutatoare`). Ticking opens `ParolaAvansataForm`; a wrong password keeps the dialog open with a red line; «Renunță» / Escape puts the tick back. Unticking needs no password. The «Documente» tab is hidden with `navPagini.SetItemVisible`; if it was the open tab, the page goes back to «Generale».
- `SetariForm`: keys `pagina`, `tema`, `foldere` hidden / shown from `AppSettings.AdvancedOptions` at Load and on every `AppSettings.Changed`; a hidden page that was on screen hands over to «Aplicație».
- `ParolaAvansataForm` (new, Designer + code). Only the SHA-256 of the password is in the build (`PasswordHash`), compared case-sensitively. It keeps an operator out of those pages; it is not a security boundary (the value is a plain flag in app_settings.json).
- Tests written (not run): `tests/KBot.App.Tests/ParolaAvansataFormTests.vb`.

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 errors, 0 warnings. No tests run.

### Unverified

- Not run; the checkbox and the password dialog not seen on screen.
- Choice made without asking: the switch is SAVED, so once unlocked it stays unlocked on that Windows user until unticked.

## SLICE-0072-03 — «Setări ▸ FOREXE» no longer crashes on open (operator report, 01.10.2026) <a id="slice-0072-03"></a>

### What changed and why
`NullReferenceException` in `SetariForexeView.ChkMeniuPagina_CheckedChanged` when the page was created (`$VB$Me._controller was Nothing`). The designer sets `chkMeniuPagina.Checked = True` / `CheckState` inside `InitializeComponent()`; that raises `CheckedChanged` while the constructor is still running, before `_controller` is assigned, and the page's own guard (`_suppress`) was still `False`.

Fix: the constructor holds `_suppress = True` around `InitializeComponent()`. Every other handler on the page checks `_suppress` before it touches `_controller`, so this covers all of them, not only that check box. The defect was already in the code (not part of slice 0100); it surfaced while debugging the fork.

### Files
`src/KBot.App/Setari/SetariForexeView.vb`.

### Test results
`dotnet build src\KBot.App`: no compile errors. The DLL copy step failed only because the app was running under the debugger. Nothing run on screen.

### Unverified
The page was not opened after the fix.

## Completări din registrul istoric

- Comutatorul jurnalului detaliat este rezolvat în baza 0072: valoarea la pornire și modificarea din pagină afectează consolele; **fișierul-jurnal și istoricul nu depind de el**.
- Registrul consemna 0072-01 «comis», iar baza 0072 «nimic comis» la reperele lor. Sunt stări ale intervențiilor de atunci, nu operații Git efectuate în această consolidare.
- Raportul operatorului pentru 0072-03 includea «dacă sar peste funcție, restul merge»; corecția constructorului nu a fost probată prin redeschiderea paginii după reparație.
- Publicarea rutelor/SMTP, proba SET PASSWORD și lipsa realinierii serverului vechi rămân în limitele bazei, fără repetarea instrucțiunilor în registru.
