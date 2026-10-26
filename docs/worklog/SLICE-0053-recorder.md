# SLICE-0053/0070/0074 — Recorderul, andocarea și gazdele Browser FOREXE

Continuarea 0070/0074 de mai jos înlocuiește întoarcerea browserului vizibil pe desktop cu detașare și parcare off-screen, corectează stilul pentru tastatură și arbitrează cele două gazde. [0073/01/76](SLICE-0073-recorder-meniu-plutitor-urmarire.md) păstrează separat urmărirea paginii și marcajele. Contractele vechi de andocare sunt checkpointuri, nu alternative curente.

Consolidare D001-17: baza și etapele 02…04 sunt reunite cronologic, cu corpurile tehnice, fișierele, probele și limitele păstrate. Comenzile și regulile citate sunt istorice și nu se execută în această lucrare documentară.

Repere: baza folosește andocare simulată; 02 leagă formularul/monitorul; 03 încearcă z-order și eliminarea viewportului prin CDP; 04 înlocuiește andocarea cu SetParent, apoi adăugirea introduce NoViewport la crearea contextului și compensarea numai a benzii superioare. Referințele mai vechi la NoViewport ca alternativă viitoare sunt înlocuite de acea adăugire. DrawToBitmap este randare statică, distinctă de utilizarea unui browser viu/CABWeb, rămasă neprobată în aceste etape.
- [Gazde 0070/0074](#gazde-0070-0074)

## SLICE-0053 — K-BOT Recorder (fazele A–F) <a id="slice-0053-baza"></a>

Data: 2026-09-06

### Ce s-a schimbat și de ce

Formular nou în `KBot.Forexe` care andochează browserul Playwright deja pornit, înregistrează ce face operatorul pe CABWeb și generează un fișier `.wfl`. Câștigul urmărit: dispare munca de găsit selectori care supraviețuiesc unui rerender Wicket.

#### Faza A — andocare falsă
`WorkflowExecutor.Docking.vb`: fereastra Chromium **nu** e reparentată (fără `SetParent`) — e doar ținută peste `pnlBrowser` și imediat deasupra formularului în z-order. Coordonatele WinForms (pixeli fizici, PerMonitorV2) se convertesc în DIP pentru `Browser.setWindowBounds` cu `* 96 / DeviceDpi`. Gărzi `_isDocked` adăugate în `HideBrowserWindowAsync`, `ShowBrowserWindowAsync` și `ExecuteMinimizeAsync` — altfel un workflow ar arunca browserul off-screen în timpul înregistrării. `UndockBrowserAsync` își face propriul `setWindowBounds`, ca să nu fie blocat de propria gardă.

#### Faza B — `Recorder.js`
Resursă embedded nouă, callback propriu `_kbotRecorderCallback` (numele `_clickMonitorCallback` / `_keyMonitorCallback` sunt deja ocupate de `ClickMonitor.vb`, iar `ExposeFunctionAsync` aruncă la a doua expunere). Gardă `_kbotRecorderInstalled`. Listeneri în faza de capture: `mousedown`, `input`, `change`, `focusout`, `keydown` (doar Enter/Tab). Clasificare widget (select2-mask / -open / -search / -pick, wicket-select, paginator, row-action). Până la 5 candidați de selector rankați; **`id` nu e niciodată sursă de selector** (Wicket regenerează id-urile). `matchCount` pentru `:has-text()` se numără manual, `querySelectorAll` nu înțelege sintaxa Playwright.

#### Faza C — captura în .NET
`RecordedStep`, `SelectorCandidate`, `RecordedAncestor`, `RecorderComment` + `WorkflowExecutor.Recorder.vb`. Instalarea se face o singură dată per executor; `StopRecording` doar comută un boolean. Scriptul se injectează **și** prin `AddInitScriptAsync` (navigări viitoare) **și** printr-un `EvaluateAsync` pe pagina curentă — fără al doilea, primul click nu s-ar înregistra niciodată. Corelarea AJAX folosește monitorul Wicket: `#animlogo` care iese din `display:none` în 1500 ms după un pas ⇒ `TriggeredAjax`; intrarea `IDLE` următoare dă `IdleAfterMs`. `#statlogo` e ignorat complet.

#### Faza D — `RecorderCompactor.vb`
Clasă pură (fără UI, fără browser, fără stare globală). Cele nouă reguli din plan: zgomot eliminat, select2 cu căutare (bloc fix de 5 acțiuni), select2 fără căutare (3 acțiuni), `<select>` Wicket + regula sursă→indicator (`<Wait seconds="1">` obligatoriu), Fill colapsat, Click, WaitFor automat, notă de buclă la paginator, bloc de reset.

#### Faza E — `WflWriter.vb`
`IWorkflowAction` → XML. Atributele se scriu doar când diferă de ce ar reface `WorkflowParser`. Selectorii cu `:has-text('…')` care conțin apostrof se trunchiază la cel mai lung fragment fără apostrof de minimum 6 caractere; dacă nu există, aruncă `InvalidOperationException` cu mesaj în română (fail-fast, nu scrie un selector rupt).

#### Faza F — `RecorderForm`
`SplitContainer` vertical: stânga `pnlBrowser` (gazda), dreapta bara de andocare, bara de înregistrare, opțiunile, lista de pași, panoul de detaliu, bara de generare și previzualizarea. Orice modificare (candidat, bifă, ștergere, reordonare, opțiune globală) regenerează previzualizarea. Ștergerea e logică (`Deleted`), ca numerotarea traseului brut să rămână coerentă. `TopMost` rămâne `False` — un formular TopMost ar acoperi browserul andocat. Repoziționarea browserului la mutare/redimensionare/splitter e debounce-uită la 150 ms.

### Fișiere atinse

Noi:
- `src/KBot.Forexe/Executor/WorkflowExecutor.Docking.vb`
- `src/KBot.Forexe/Executor/WorkflowExecutor.Recorder.vb`
- `src/KBot.Forexe/Services/JavaScripts/Recorder.js`
- `src/KBot.Forexe/Models/Recorder/RecordedStep.vb`
- `src/KBot.Forexe/Models/Recorder/SelectorCandidate.vb`
- `src/KBot.Forexe/Models/Recorder/RecordedAncestor.vb`
- `src/KBot.Forexe/Models/Recorder/RecorderComment.vb`
- `src/KBot.Forexe/Services/RecorderCompactor.vb`
- `src/KBot.Forexe/Services/WflWriter.vb`
- `src/KBot.Forexe/Helpers/RecorderForm.vb`
- `src/KBot.Forexe/Helpers/RecorderForm.Designer.vb`

Modificate:
- `src/KBot.Forexe/Executor/WorkflowExecutor.Browser.vb` — gărzi `_isDocked`
- `src/KBot.Forexe/Executor/Actions/WorkflowExecutor.Actions.Minimize.vb` — gardă `_isDocked`
- `src/KBot.Forexe/KBot.Forexe.vbproj` — `Recorder.js` ca `EmbeddedResource`, FileVersion 1.0.4.0 → 1.0.5.0

### Abateri de la documentul de predare (motivate)

1. **Faza A NU era livrată.** Documentul o marca „Livrat"; în repo nu exista niciun fișier `Docking` sau `RecorderForm` (nici urmărit, nici neurmărit). A fost implementată în cadrul acestei felii.
2. **`waitNavigation` — documentul greșește.** Documentul spune că `True` e implicit și se omite. `ClickAction.WaitNavigation` e `True` în model, dar `WorkflowParser` îl citește cu `GetBoolAttribute(e, "waitNavigation", False)`. Parserul e cel care recitește fișierul, deci `waitNavigation="true"` se scrie **explicit**. Verificat prin round-trip.
3. **Comentarii și identificatori în engleză**, nu în română. RULE 0 din `CLAUDE.md` și memoria `code-english-only` au prioritate; româna rămâne doar în textele pe care le vede operatorul (log, UI, MsgBox, `LogValue`). Numele controalelor rămân cele din plan (`btnAndocheaza`, `btnSalveaza`…) — e vocabularul folosit unanim de restul formularelor.
4. **Fără blocuri `Namespace`** (regulă de casă). Modelele recorderului stau în namespace-ul rădăcină, nu în `WorkflowModels` — cerința reală a planului („zero proprietăți noi pe clasele din `WorkflowModels`") e respectată.
5. **Membri redenumiți în engleză**: `InsereazaWaitFor` → `InsertWaitFor`, `Sters` → `Deleted`, `SelectorAles` → `ChosenSelector`, `Comprima` → `Compact`, `Scrie` → `Write`, `InsereazaWaitForAutomat` → `InsertWaitForAutomatically`, `AdaugaBlocReset` → `AddResetBlock`, `TimeoutImplicit` → `DefaultTimeout`.
6. **Câmp nou în payload: `keyName`.** Structura din plan nu distinge Enter de Tab, deși `keydown` prinde ambele. Fără el pasul `key` era inutilizabil.
7. **`RecorderComment`** — regulile 8 și 9 cer comentarii XML în output, dar semnătura cerută returnează `List(Of IWorkflowAction)`. Comentariile călătoresc ca acțiune-marcaj, pe care `WflWriter` o transformă în `XComment`; `WorkflowParser` nu le vede (comentariile nu sunt elemente).
8. **ListView desenat manual.** `ThemeManager` nu acoperă `ListView` — în temă întunecată rămânea alb. Rândurile și antetul se desenează din paleta activă (fără culori literale).

### Rezultate teste

- `dotnet build src\KBot.Forexe\KBot.Forexe.vbproj` — **0 erori, 0 avertismente**.
- `dotnet build KBot.sln` — **0 erori**; cele 7 avertismente MSB3825 sunt preexistente (`.resx` din `KBot.App`, BinaryFormatter), fără legătură cu această felie.
- `node --check Recorder.js` — sintaxă validă.
- Resursele embedate ale assembly-ului conțin `Recorder.js` cu nume exact ⇒ `JsLoader` o găsește, fără coliziune de sufix.
- **Round-trip compactor → writer → `WorkflowParser.Parse`**, pe un traseu sintetic care acoperă: click cu AJAX, Fill colapsat din 3 `input` + 1 `change`, zgomot (`body`, mască select2, input ascuns scris de select2), `<select>` Wicket urmat de select2, select2 cu căutare, select2 fără căutare cu apostrof în text, paginator, Enter. Rezultat: 17 acțiuni recitite, **niciun `LogError`**, niciun selector pierdut. Blocul select2 cu căutare a ieșit exact ca cele cinci acțiuni din workflow-urile scrise de mână, iar `Primăria d'Argeș Muscel` a fost trunchiat corect la `:has-text('Argeș Muscel')`.
- **Aspect verificat pe ecran** (`DrawToBitmap`), în temă Classic și în temă Dark: lista, panoul de detaliu (cu prefixul ⚠ pe candidații fragili) și previzualizarea XML se citesc corect în ambele.

### Rămas neverificat / amânat

- **Nimic nu deschide încă formularul.** Ca și `WicketMonitorForm`, `RecorderForm` nu are niciun call-site: `_executor` trăiește privat în `ForexeRunner` și nu e expus către `KBot.App`. Planul nu prevedea fișier de integrare, deci nu am atins seam-ul public al lui `ForexeRunner`. Până la cablare, forma se poate deschide doar din cod.
- **Andocarea nu a fost rulată împotriva unui browser real.** Rămân de verificat pe ecran, cu o sesiune vie: poziționarea, DPI între monitoare cu scalări diferite (conversia DIP folosește un singur `DeviceDpi`, al gazdei), z-order la activarea formularului, `<Minimize>` blocat și dialogul de PIN peste browserul andocat.
- **Captura nu a fost rulată împotriva CABWeb.** Clasificarea widget-urilor și scorurile candidaților sunt scrise după plan și după workflow-urile existente, nu măsurate pe pagina reală. Fluxul de creare angajament (care conține tot) rămâne testul de referință.
- Regula 7 poate insera un `WaitFor` imediat după `<Wait seconds="1">` al regulii sursă→ indicator. E redundant, nu greșit, și e exact ce cere condiția din plan.
- Antetul `ListView` lasă o bandă nedesenată la dreapta ultimei coloane (zona pe care o pictează sistemul). Cosmetic.
- Fără proiect `tests/KBot.Forexe.Tests` — `RecorderCompactor` și `WflWriter` sunt clase pure și ar merita teste xUnit. Round-trip-ul de mai sus a fost rulat ca program de unică folosință, în afara repo-ului.

## SLICE-0053-02 — Cablarea recorderului (buton în consolă + monitor Wicket) <a id="slice-0053-02"></a>

Data: 2026-09-07

Continuarea feliei 0053 ([SLICE-0053-recorder.md](#slice-0053-baza)), care lăsase formularul scris dar neajuns de nicăieri. Aici se rezolvă punctul (1) din firele deschise: **RecorderForm are acum call-site**, iar `WicketMonitorForm` — care nici el nu se instanția nicăieri — se deschide din interiorul recorderului.

### Ce s-a schimbat și de ce

#### Seam-ul: `ShowRecorder`, nu `Executor`

`RecorderForm` are nevoie de `WorkflowExecutor`, care trăiește privat în `ForexeRunner`. Varianta „expunem executorul” ar fi mutat un obiect de motor în `KBot.App` doar ca să fie pasat înapoi într-un formular care oricum stă în `KBot.Forexe`. În loc de asta, interfața primește o intenție:

```
IForexeRunner:  Sub ShowRecorder(owner As IWin32Window)
```

`ForexeRunner` construiește formularul la prima cerere, îl ține (modeless) și îl leagă la executorul CURENT la fiecare deschidere — o reconectare înseamnă alt executor, deci legarea nu se poate face o singură dată. `FormClosed` anulează referința, așa că închiderea de către operator lasă loc unui formular nou.

Fără sesiune vie `ShowRecorder` **aruncă**, nu tace: exact tiparul lui `ShowBrowserAsync`.

`DisposeExecutorAsync` cheamă `DetachRecorder()` ÎNAINTE de a închide executorul — altfel recorderul ar rămâne cu un executor mort în mână și cu butoanele de andocare aprinse degeaba. Desprinderea se marșalează pe firul de UI (`BeginInvoke`), fiindcă închiderea sesiunii vine de pe firul robotului.

#### Butonul din consolă

`btnRecorder` exista în `ForexeConsoleForm.Designer.vb` (cu iconiță și tooltip) dar n-avea handler, n-avea stare și nu era temat. Acum:

- `BtnRecorder_Click` → `ForexeController.ShowRecorder()` → `IForexeRunner.ShowRecorder`; fără sesiune, un mesaj care spune de ce, nu un buton care nu face nimic;
- `ActualizeazaStarea` îl stinge când nu există sesiune (ca `btnAfiseazaBrowser`);
- `OnThemeChanged` îi aplică `ButtonStyles.ApplySecondary`, ca celorlalte trei.

Proprietarul ferestrei e `ForexeController.Owner` (shell-ul), nu consola: consola se ascunde la închidere, iar un recorder deținut de ea ar dispărea odată cu ea.

#### Monitorul Wicket în recorder

Buton nou `btnMonitor` pe bara de înregistrare (pozițiile celorlalte trei rămân neatinse; `374,6` × `74,28`, dreapta la 448 într-un panou de 454 — verificat pe ecran).

`BtnMonitor_Click` pornește întâi `StartWicketMonitoringAsync` dacă nu rulează deja: `WicketMonitorForm` se hrănește exclusiv din `OnWicketStateChange`, deci fără asta fereastra s-ar deschide goală și ar părea stricată. Apoi creează monitorul o singură dată, îl leagă la același executor și îl arată; la a doua apăsare doar îl aduce în față (și îl restaurează dacă e minimizat).

Monitorul își anulează propriul `FormClosing` la închiderea de către operator și se ascunde, deci NIMIC în afară de recorder nu-l distruge: `RecorderForm.Dispose` îl `Dispose`-uiește direct, ocolind `FormClosing` — exact ce trebuie.

`AttachExecutor` / `DetachExecutor` din recorder duc mai departe schimbarea și către monitor, ca să nu rămână legat de un executor pe care recorderul l-a lăsat deja.

### Fișiere atinse

- `src/KBot.Forexe/IForexeRunner.vb` — `ShowRecorder(owner)`
- `src/KBot.Forexe/ForexeRunner.vb` — `_recorder`, `ShowRecorder`, `Recorder_FormClosed`, `DetachRecorder`, apel în `DisposeExecutorAsync`
- `src/KBot.Forexe/Helpers/RecorderForm.vb` — `_monitor`, `BtnMonitor_Click`, monitorul urmărit în `AttachExecutor`/`DetachExecutor`/`Dispose`, gardă `IsDisposed` în `UpdateButtons`
- `src/KBot.Forexe/Helpers/RecorderForm.Designer.vb` — `btnMonitor`
- `src/KBot.App/Forexe/ForexeController.vb` — `ShowRecorder()`
- `src/KBot.App/Forexe/ForexeConsoleForm.vb` — handler, stare, temă pe `btnRecorder`
- FileVersion: `KBot.Forexe` 1.0.5 ▸ 1.0.6, `KBot.App` 1.0.25 ▸ 1.0.26

### Rezultate

- `dotnet build src\KBot.App\KBot.App.vbproj` — **0 erori**; cele 7 `MSB3825` sunt preexistente (`.resx` din `KBot.App`, BinaryFormatter), fără legătură cu felia.
- **Văzut pe ecran** (`DrawToBitmap`, Classic și Dark): `btnMonitor` stă în bara de înregistrare, nu depășește panoul (dreapta 448 < 456) și e stins cât nu există executor; `btnRecorder` din consolă arată acum ca vecinii lui, nu ca un buton de sistem.

### Rămas neverificat

- Nimic din asta n-a atins un browser adevărat. Deschiderea recorderului dintr-o sesiune vie, andocarea și monitorul cu trafic Wicket real rămân punctele (2) și (3) din firele deschise ale feliei 0053.
- Fereastra e deținută de shell: nu s-a verificat pe ecran cum se comportă z-order-ul când operatorul aduce MainForm în față peste browserul andocat.

## SLICE-0053-03 — Recorder: z-order nedocat, viewport la andocare, conectare fără token în Debug <a id="slice-0053-03"></a>

Data: 2026-09-07

Trei observații ale operatorului după prima încercare de a folosi recorderul cablat în pasul 02 ([SLICE-0053-02-recorder-cablare.md](#slice-0053-02)). Toate trei sunt piedici în calea verificării pe viu a andocării — punctul (2) din firele deschise ale feliei.

### 1. Nedocat, browserul cădea în spatele formularului

**Simptom:** cu browserul NEandocat, orice clic pe recorder trimitea fereastra Chromium
în spatele lui.

Era exact ce trebuia să se întâmple: nimic nu ținea browserul deasupra. Calea andocată avea deja linia care rezolvă asta — `SyncDockedBoundsAsync` cheamă `SetWindowPos(hwnd, ownerHandle, …, NOACTIVATE)`, adică «pune browserul imediat deasupra formularului gazdă, fără să-i dai focusul». Nedocat, `ScheduleResync` ieșea din prima linie (`If Not _executor.IsDocked Then Return`), deci nu se întâmpla nimic.

Acum:

- `WorkflowExecutor.RaiseBrowserAboveAsync(owner)` — perechea nedocată a acelei linii de z-order, fără CDP și fără mutarea ferestrei. `owner.Handle` se citește ÎNAINTE de `Await`: după el continuarea poate să nu mai fie pe firul de UI.
- `RecorderForm.ScheduleResync` nu mai iese când e nedocat, atâta timp cât există browser; `TmrResync_Tick` se ramifică — andocat resincronizează marginile, nedocat doar ridică browserul deasupra.

Temporizatorul era deja acolo pentru amortizarea tragerilor, deci nu s-a adăugat niciun mecanism nou: aceleași evenimente (`Move`, `Resize`, `Activated`, mutarea splitterului) duc acum la două acțiuni diferite, după starea de andocare.

**Consecință de spus cu voce tare:** nedocat, browserul stă acum DEASUPRA recorderului și
îl poate acoperi complet (fereastra detașată e pusă la `100,100` × `1400×900`). Recorderul rămâne cel activ pentru tastatură, dar poate fi nevoie să fie mutat browserul ca să se vadă lista de pași. Andocat, nu se schimbă nimic — panoul e tocmai locul făcut pentru browser.

### 2. Viewportul emulat, la andocare

Playwright desenează pagina într-un viewport EMULAT — contextul se creează în `WorkflowExecutor.Browser.vb` fără `ViewportSize`, deci implicit 1280×720 — iar viewportul acela NU urmează fereastra. Andocarea redimensionează fereastra la panoul gazdă, deci pagina rămânea desenată la dimensiunea veche, cu chenar gol în jur.

`DockBrowserToAsync` cheamă acum `ClearEmulatedViewportAsync()` imediat după prima sincronizare de margini: dacă `_page.ViewportSize` e ceva (adică viewportul e activ), o sesiune CDP trimite `Emulation.clearDeviceMetricsOverride` și pagina începe să urmeze fereastra reală. Este exact modul «no viewport» al lui Playwright, aplicat după crearea contextului — API-ul .NET nu-l poate scoate altfel decât la construcție.

Nu se pune la loc la detașare, intenționat: o pagină care își urmează fereastra e corectă în ambele stări, pe când repunerea celor 1280×720 ar lăsa chenar gol în fereastra detașată de 1400×900.

Eșecul se STRIGĂ în jurnal (`LogWarning`), nu pică andocarea: o pagină desenată la dimensiunea veche e urâtă, nu un motiv să refuzi andocarea pe care operatorul tocmai a cerut-o.

### 3. Conectare fără token, doar în Debug

Fără token în cititor, `CertificateSelectionForm` se deschidea goală, anunța lipsa și se închidea cu `Cancel`; `ForexeController.ConnectAsync` ieșea cu `False` și nu se lansa niciun browser. Adică pe un calculator fără token nu se putea încerca NIMIC din andocare, înregistrare sau monitorizare.

`ConnectAsync` întreabă acum întâi `NoTokenInDebug()`:

- **Release:** `False` întotdeauna — comportament neschimbat, lipsa tokenului oprește conectarea.
- **Debug cu token:** `False` — dialogul apare normal, iar un «Renunță» al operatorului rămâne un «Renunță».
- **Debug fără niciun certificat pe token/smartcard:** se sare peste dialog (ar fi gol) și se merge mai departe cu `certificate = Nothing`.

Merge fiindcă nimic din calea de lansare nu are nevoie de certificat: `ConfigureAutoSelectCertificatePolicy` iese din prima linie când e `Nothing`, iar `_windowsSecurityAutomation` nu se instanțiază în build-ul curent. Autentificarea la FOREXE va pica — dar **browserul rămâne deschis** (decizia A3 din `ForexeRunner.RunAsync`: «Browserul rămâne deschis pentru investigație»), deci `HasLiveSession` devine `True`, `btnRecorder` se aprinde și andocarea/monitorul pot fi încercate.

Starea afișată spune ce s-a întâmplat («Fără token — pornesc sesiunea NEAUTENTIFICATĂ (doar Debug).»), iar `lblCert` din consolă rămâne pe «—», onest.

### Fișiere atinse

- `src/KBot.Forexe/Executor/WorkflowExecutor.Docking.vb` — `ClearEmulatedViewportAsync`, `RaiseBrowserAboveAsync`, apelul de curățare a viewportului în `DockBrowserToAsync`
- `src/KBot.Forexe/Helpers/RecorderForm.vb` — `ScheduleResync` / `TmrResync_Tick` ramificate pe starea de andocare
- `src/KBot.App/Forexe/ForexeController.vb` — `NoTokenInDebug`, ramura din `ConnectAsync`
- FileVersion: `KBot.Forexe` 1.0.6 ▸ 1.0.7, `KBot.App` 1.0.26 ▸ 1.0.27

### Rezultate

- `dotnet build KBot.sln -v q --nologo` — **0 erori**, 7 avertismente (`MSB3825` pe `.resx`-urile din `KBot.App`, preexistente).

### Rămas neverificat

- **Nimic din cele trei n-a fost rulat pe un browser adevărat.** Toate trei ating exact punctul (2) al feliei — andocarea nerulată vreodată pe viu — deci se verifică odată cu el.
- `Emulation.clearDeviceMetricsOverride` se trimite pe o sesiune CDP NOUĂ, nu pe cea a lui Playwright. Emularea se aplică pe `WebView`, deci ștergerea dintr-o a doua sesiune ar trebui să funcționeze (ultimul scrie câștigă), dar asta rămâne de confirmat pe ecran: dacă pagina tot rămâne la 1280×720 după andocare, alternativa e crearea contextului cu `ViewportSize.NoViewport` în `WorkflowExecutor.Browser.vb` — schimbare care atinge însă TOATE workflow-urile, nu doar recorderul.
- Ridicarea browserului deasupra formularului nedocat n-a fost văzută: nu se știe cât de vizibil e pâlpâitul dintre activarea formularului și tick-ul temporizatorului.

## SLICE-0053-04 — Andocare REALĂ (`SetParent`) + banc de probă pentru andocare și monitor <a id="slice-0053-04"></a>

Data: 2026-09-07

Operatorul: «andocarea tot nu merge». Și: «fă un banc de probă care deschide un browser, navighează la <https://mfinante.gov.ro/web/forexepublic> și face tot ce trebuie să facă formularul de monitorizare, inclusiv andocarea». Și, explicit: «ignoră regula după care fereastra Chromium nu are voie să fie andocată cu `SetParent`; ai voie».

### 1. De ce nu mergea andocarea: fereastra nu era găsită

`GetOrRefreshBrowserHwndAsync` căuta fereastra plimbând TOATE procesele mașinii și comparând `Process.MainWindowTitle` cu marcajul unic `WF_BROWSER_<guid>`. Marcajul se scrie o singură dată, în `document.title`, pe `about:blank`, la pornirea browserului.

**Prima navigare reală șterge marcajul.** Titlul ferestrei devine titlul sitului, iar căutarea
nu mai găsește nimic: 20 de încercări × 250 ms = cinci secunde de scanat procese, apoi `IntPtr.Zero`, apoi excepția «Nu am găsit fereastra browserului. Andocarea nu poate continua.» Handle-ul nu era pus în cache la pornire decât pe calea *stealth* (`ApplyStealthWindowStyle`), deci în mod normal nu exista niciun handle memorat care să salveze situația.

Acum:

- handle-ul se rezolvă **la pornire**, cât timp marcajul e încă pe pagină — o singură dată, în `LaunchAndPositionBrowserAsync`, imediat după ce se scrie titlul;
- dacă totuși cache-ul e gol mai târziu, căutarea **repune marcajul** în `document.title`, găsește fereastra, apoi **pune titlul real la loc** (operatorul nu rămâne cu o fereastră numită `WF_BROWSER_<guid>`);
- căutarea folosește `EnumWindows` + filtru pe clasa `Chrome_WidgetWin_1`, nu `Process.MainWindowTitle`: vede fereastra chiar dacă nu e fereastra principală a procesului, nu plimbă toate procesele, și nu prinde din greșeală ferestrele-ajutor invizibile pe care Chromium le atârnă de același titlu — o astfel de fereastră mutată prin ecran arată exact ca o andocare care nu face nimic.
- `GetBrowserHwndAsync`, geamănul mort al funcției (nechemat de nimeni), a fost șters.

### 2. Andocare adevărată: `SetParent`

Regula «fereastra Chromium nu se reparentează» a căzut la cererea operatorului, iar odată cu ea a căzut și tot mecanismul construit ca s-o ocolească.

Înainte: fereastra rămânea de nivel superior, era ținută peste panou cu CDP `Browser.setWindowBounds` (coordonate fizice → DIP cu `* 96 / DeviceDpi`) și împinsă imediat deasupra formularului cu `SetWindowPos`, la fiecare mutare, redimensionare și activare. Două lucruri se rupeau constant: găsirea ferestrei (§1) și cursa de z-order cu fiecare activare a formularului gazdă.

Acum, în `DockBrowserToAsync`:

1. se rețin stilurile, părintele și dreptunghiul ferestrei, ca detașarea să le poată da înapoi;
2. `ShowWindow(SW_RESTORE)` — o fereastră maximizată se luptă cu dimensiunea primită;
3. stilurile devine de copil: fără `WS_CAPTION`, `WS_THICKFRAME`, `WS_SYSMENU`, butoanele de minimizare/maximizare și `WS_POPUP`; cu `WS_CHILD` și `WS_VISIBLE`; scoasă din bara de activități;
4. `SetParent(hwnd, panou)`. **Rezultatul lui `SetParent` nu spune nimic** — întoarce părintele ANTERIOR, care e null și pentru o fereastră de nivel superior, și null și la eșec — deci se întreabă `GetParent` cine e părintele acum. Dacă nu e panoul: stilurile se pun la loc și se aruncă. O fereastră rămasă fără stiluri e mai rea decât o andocare care n-a avut loc;
5. `AttachBrowserInput` leagă coada de intrare a firului UI de cea a firului ferestrei browserului. Fără asta, un copil Chromium reparentat primește mouse-ul dar **nu** și tastatura: focusul urmează fereastra de prim-plan, care e a firului formularului. Best effort — o tastare capricioasă nu e motiv să refuzi andocarea cerută;
6. viewportul emulat se elimină ca înainte (`Emulation.clearDeviceMetricsOverride`).

`SyncDockedBoundsAsync` s-a redus la un `MoveWindow(hwnd, 0, 0, latime, inaltime, True)`: coordonatele unui copil sunt pixelii CLIENT ai părintelui, adică exact ce raportează WinForms.
**Nicio conversie DPI, niciun drum prin CDP, niciun z-order de întreținut** — un copil nu poate
cădea în spatele propriului părinte. Funcția nu mai e `Async` de fapt; întoarce `Task` ca să nu se schimbe niciun apelant.

`UndockBrowserAsync` face drumul invers: dezleagă cozile de intrare, `SetParent` la părintele dinainte, repune stilurile și dreptunghiul (sau `100,100 × 1400×900` dacă cel memorat nu era folosibil), `SWP_FRAMECHANGED`.

`RaiseBrowserAboveAsync` rămâne, dar **numai pentru browserul NEdocat** — are acum o gardă `If _isDocked Then Return`.

#### Prețul, spus cu voce tare

Fereastra browserului aparține acum panoului. **Eliberarea panoului sau închiderea formularului gazdă fără detașare distruge fereastra Chromium și, cu ea, sesiunea.** De-aia `RecorderForm` și bancul de probă detașează în `FormClosing`, înaintea oricărei eliberări. Comentariul-antet din `RecorderForm.vb`, care spunea exact pe dos («browserul nu e reparentat»), a fost măturat.

### 3. Bancul de probă: `BrowserDockHarnessForm`

Nou în `KBot.DevHarness`, se vede în banc la categoria **FOREXE**, sub numele «FOREXE — Andocare browser + monitor Wicket». Nu cere certificat și nu cere token: sesiunea nu se autentifică niciodată, singurul lucru care contează e fereastra.

Un buton («Pornește + navighează») face toată secvența:

```
lansează Chromium → instalează monitorul Wicket → navighează la pagina publică → andochează
```

**Ordinea nu e întâmplătoare.** `StartWicketMonitoringAsync` își pune scriptul cu
`AddInitScriptAsync`, iar un script de inițializare rulează abia la navigarea URMĂTOARE — deci monitorul intră ÎNAINTE de încărcarea paginii, niciodată după. Butonul «Reîncarcă pagina» există pentru același motiv: e felul în care un monitor pornit târziu devine viu.

Monitorul **nu e copiat**: `pnlMonitor` găzduiește `WicketMonitorForm`-ul real (`TopLevel = False`), legat de același executor. Ce se vede în banc e literalmente ce vede operatorul în aplicație — inclusiv culorile pe fluxuri `[WFL]` / `[WICKET]` / `IDLE` / `[CLICK]` / `[KEY]`.

Restul barei: «Andochează», «Detașează», «Resincronizează», «Pornește/Oprește monitorul». Jos: jurnalul executorului (`RichTextBoxLogger` real, care scrie și în `<AppDir>\Logs\harness_dock_<stamp>.log`) și verdictul uman («Andocarea merge» / «nu merge»), ca la celelalte probe vizuale din banc.

Pe pagina publică FOREXE **fluxul `[WICKET]` rămâne tăcut**: `#statlogo` / `#animlogo` sunt elemente ale aplicației Wicket și nu există acolo, iar `WicketMonitor.js` iese din prima linie când nu le găsește. `[CLICK]` și `[KEY]` funcționează pe orice pagină. Formularul spune asta în jurnal la pornire, ca tăcerea să nu fie citită drept defect.

### Fișiere atinse

- `src/KBot.Forexe/Executor/WorkflowExecutor.Docking.vb` — rescris pe reparentare
- `src/KBot.Forexe/Executor/WorkflowExecutor.Browser.vb` — căutarea ferestrei prin `EnumWindows` + remarcare a titlului, handle rezolvat la pornire, `GetBrowserHwndAsync` șters
- `src/KBot.Forexe/Helpers/RecorderForm.vb` — comentariul-antet, care descria vechea andocare
- `src/KBot.DevHarness/Internal/BrowserDockHarnessForm.vb` / `.Designer.vb` — bancul
- `src/KBot.DevHarness/Tests/BrowserDockHarnessTest.vb` — intrarea în banc
- FileVersion: `KBot.Forexe` 1.0.7 ▸ 1.0.8, `KBot.DevHarness` 1.0.25 ▸ 1.0.26

### Rezultate

- `dotnet build KBot.sln -v q --nologo` — **0 erori**, 7 avertismente `MSB3825` preexistente pe `.resx`-urile din `KBot.App`.
- **Formularul a fost randat cu `DrawToBitmap`** înainte de raportare, și randarea a prins o eroare adevărată: `SplitContainer` proaspăt construit are 150 px lățime, iar `EndInit` refuză o `SplitterDistance` care nu încape între `Panel1MinSize` și `Width - Panel2MinSize` — formularul **arunca în constructor**, deci bancul nu s-ar fi deschis niciodată. Reparat prin `Size` înaintea minimelor și a distanței (`Dock = Fill` preia la prima trecere de aranjare).

### Rămas neverificat

- **Nimic n-a atins încă un browser adevărat.** Exact ăsta e rostul bancului: prima rulare spune dacă reparentarea prinde.
- Comportamente de urmărit la prima rulare, în ordinea probabilității de a supăra: **(a)** Chromium își desenează propriul chenar (banda de file ESTE bara de titlu); scoaterea lui `WS_CAPTION` s-ar putea să nu schimbe nimic vizual — ceea ce e în regulă, bara de adresă rămâne folosibilă. **(b)** Tastarea în pagina andocată: dacă `AttachThreadInput` nu ajunge, se vede imediat în jurnal («Firele de input nu au putut fi legate»). **(c)** DPI: un copil moștenește contextul de DPI al părintelui, deci pe un monitor cu altă scalare pagina poate ieși la scară greșită. **(d)** Dialogul de certificat / PIN peste browserul andocat.
- Butonul de copiere din `WicketMonitorForm` (`btnCopyText`) are ca text emoji-ul `🗐`, pe care fontul Calibri al aplicației nu-l redă: în randare apare un pătrat gol. Preexistent, nu ține de felia asta.

### Adăugire — bara browserului ascunsă în panou

Cerință: cât timp e andocat, browserul nu trebuie să-și arate **banda de file** și **bara de adrese**. Nu se face cu un argument de Chromium (`--app` / `--kiosk` se bat cu felul în care Playwright deschide paginile și cu dimensionarea panoului), ci prin **poziționare**:

- fereastra primește un `Top` negativ, exact cât banda ei de sus, și o înălțime mai mare cu aceeași valoare — zona paginii cade fix peste panou, iar banda rămâne deasupra marginii de sus a panoului;
- o fereastră-copil e decupată de părinte, deci banda nu se desenează și nu se poate apăsa;
- fereastra rămâne neatinsă: la detașare, sau cu `HideChromeWhenDocked = False`, bara revine.

**Se compensează DOAR partea de sus.** Prima variantă compensa și stânga, dreapta și jos, cu
aceeași măsurătoare — și operatorul a rămas fără **barele de derulare** ale paginii: fereastra copilului de randare nu le acoperă, așa că lărgirea le împingea dincolo de marginile panoului. O bordură reală rămasă pe laturi înseamnă câțiva pixeli morți; o bară de derulare lipsă înseamnă o pagină pe care nu o mai poți citi.

Banda e **măsurată, nu presupusă**: pagina se desenează într-o fereastră-copil de clasă `Chrome_RenderWidgetHostHWND`, iar distanța de la marginea de sus a cadrului la ea dă înălțimea reală, oricare ar fi DPI-ul, tema sau bara de marcaje. O cifră neverosimilă (peste 400 px) e respinsă; dacă măsurarea eșuează se folosește ultima bună, iar dacă nu există niciuna bara rămâne vizibilă și jurnalul o spune **o singură dată** pe andocare. Valoarea măsurată e scrisă în jurnal la fiecare schimbare, ca o bandă greșită să se vadă în cifre, nu doar în imagine.

`WorkflowExecutor.HideChromeWhenDocked` (implicit `True`) comută starea, cu efect imediat pe un browser deja andocat. Bancul are bifa «Ascunde bara browserului», ca ambele stări să poată fi văzute fără repornire.

#### Viewport: contextul se creează cu `NoViewport`

Cu bara ascunsă a ieșit la iveală un defect mai vechi: **pagina nu urma dimensiunea panoului**, așa că barele ei de derulare cădeau în afara lui. Vinovat e viewport-ul implicit al Playwright — 1280x720 **emulat**, împins cu `Emulation.setDeviceMetricsOverride` — care nu urmează fereastra. Andocat, fereastra e panoul, iar pagina continua să se așeze la lățimea veche.

Ștergerea override-ului la andocare (`ClearEmulatedViewportAsync`) **nu e de ajuns**: Playwright îl pune la loc. Reparația stă la crearea contextului:

```vb
_context = Await _browser.NewContextAsync(New BrowserNewContextOptions() With {
    .IgnoreHTTPSErrors = True,
    .AcceptDownloads = True,
    .ViewportSize = ViewportSize.NoViewport
})
```

Fără emulare, pagina **este** fereastra ei, corect și andocată și liberă. `ScreenshotAsync` se face peste tot cu `FullPage = True`, deci nimic din fluxuri nu depindea de 1280x720. `ClearEmulatedViewportAsync` rămâne ca plasă de siguranță pentru o sesiune construită altfel.

#### Fișiere atinse

- `src/KBot.Forexe/Executor/WorkflowExecutor.Docking.vb` — `HideChromeWhenDocked`, `ApplyDockedBounds`, măsurarea benzii (`MeasureChromeBandPx` / `FindLargestRenderWidget`)
- `src/KBot.Forexe/Executor/WorkflowExecutor.Browser.vb` — contextul cu `NoViewport`
- `src/KBot.DevHarness/Internal/BrowserDockHarnessForm.vb` / `.Designer.vb` — bifa

#### Rămas neverificat

- **Nici asta n-a atins încă un browser adevărat**, din același motiv ca restul feliei.
- Cu bara ascunsă, operatorul **nu mai poate tasta o adresă**: navigarea rămâne treaba fluxului. Dacă vreodată e nevoie de o adresă scrisă de mână, bifa o aduce înapoi.
- Scurtături ca `Ctrl+T` deschid o filă pe care nu o mai vede nimeni.

## Restanțe din registru, păstrate pe etapă

Fluxul de creare angajament rămânea proba de referință pentru captură: select2 cu/fără căutare, select Wicket, textarea și butoane; nu fusese măsurat pe CABWeb. Restanțele browserului viu includ și Minimize blocat, PIN, DPI, tastatură, z-order nedocat. Registrul încă descria NoViewport ca alternativă; adăugirea finală a etapei 04 îl consemnează deja implementat, fără a declara proba live făcută. Lipsa proiectului Forexe.Tests și proba sintetică de unică folosință rămân limitele bazei.

## SLICE-0070/74 — Istoric al gazdelor Browser FOREXE <a id="gazde-0070-0074"></a>

19–21.09.2026: browserul trece de la fereastră liberă la gazdă K-BOT și apoi primește vederea din shell. 0070-02 este ipoteza/corecția tastaturii raportate de operator, cu proba ulterioară încă cerută; 0074 adaugă a doua gazdă, selecția arbore ↔ pagină și arbitrajul recorder/vedere. În 0070 panoul recorderului este stins; [0073](SLICE-0073-recorder-meniu-plutitor-urmarire.md#slice-0073) îl strânge ulterior. Rezultatele cu runner fals sau DrawToBitmap nu sunt probe de browser real.

[0070-browserul](#slice-0070) · [0074-vedere](#slice-0074)

<a id="slice-0070"></a>

### SLICE 0070 — Browserul FOREXE nu mai apare niciodată singur pe ecran

**Data:** 19.09.2026 **Cererea operatorului:** «in the kbot.forexe we make the following changes: 1. the chromium browser CAN NEVER be displayed standalone! this allows the user to click the close button, and then the forexe flow is lost. so whenever the user wants to see the actual browser, it will open in its own form. like the recorder does. actually, we will use the recorder form, but if it is opened only for view, then all the recording options (the right panel) is disabled. also, this means that the form wil auto dock the browser on load. 2. when the browser is hidden by button or when the parent form is closed, the browser window must be auto-detached and hidden just like the Ascunde/Arata browser does. this is a new slice. no git. no tests!»

#### Ce s-a găsit

- «Arată browserul» din consolă (`ForexeController.ToggleBrowserAsync` → `ForexeRunner.ShowBrowserAsync` → `WorkflowExecutor.ShowBrowserWindowAsync`) punea fereastra Chromium LIBERĂ pe ecran, la (100,100), cu buton de închidere — și `AddHandler _page.Close` din executor confirmă ce urmează: «Browser-ul a fost inchis. Aplicatia se va inchide automat!».
- Recorderul (felia 0053) andochează REAL (`SetParent` în `pnlBrowser`, felia de andocare), dar «Detașează» și `FormClosing` dădeau fereastra înapoi pe desktop, tot liberă și tot vizibilă (`UndockBrowserAsync` repunea dreptunghiul dinaintea andocării sau 1400×900 la (100,100)).
- `JobRequest.ShowBrowser` (implicit False, nesetat nicăieri în App) putea cere un browser lansat la vedere, tot liber.

#### Ce s-a schimbat și de ce

##### `KBot.Forexe` — executorul

- **`Executor/WorkflowExecutor.Docking.vb`** — `UndockBrowserAsync` = **detașare + ascundere**: ia fereastra din panou, îi dă înapoi stilurile de ramă, dar o parchează la (-3000,0) ca fereastră-unealtă (`WS_EX_TOOLWINDOW`, fără `WS_EX_APPWINDOW`), exact starea de stealth de la lansare. Rămâne ARĂTATĂ în sensul Win32 (`SWP_SHOWWINDOW`, niciodată `SW_HIDE`) — o fereastră ascunsă nu poate găzdui dialogul de certificat (`ApplyStealthWindowStyle` spune de ce). `_isBrowserVisible = False`. Se completează sincron (un `SetParent` + un `SetWindowPos`), lucru scris în doc: `DetachExecutor` din recorder se bazează pe el. `_preDockBounds` și `DockSwShow` scoase — nu mai are cine să le folosească. **Eveniment nou `OnDockStateChanged(docked)`**, ridicat la sfârșitul andocării și al detașării: fără el, «Ascunde browserul» din consolă detașa browserul de sub recorder și recorderul rămânea cu butoanele aprinse pe dos. `RaiseBrowserAboveAsync` (ținea browserul LIBER deasupra recorderului în z-order) scos: nu mai există browser liber de ținut.
- **`Executor/WorkflowExecutor.Browser.vb`** — `ShowBrowserWindowAsync` **ȘTERS** (singura cale spre un browser liber vizibil). `HideBrowserWindowAsync`: andocat → `UndockBrowserAsync` (ascunderea E detașarea); liber → ca înainte, off-screen prin CDP. Locul de parcare e acum o singură pereche de constante (`StealthLeft/Top/Width/Height`) folosită și de `--window-position` la lansare.
- **`JobModels.vb`** — `JobRequest.ShowBrowser` **ȘTERS**; comentariul spune de ce (KBOT_IPC avea comutatorul, aici fereastra se naște întotdeauna ascunsă).

##### `KBot.Forexe` — recorderul și runner-ul

- **`Helpers/RecorderForm.vb`** — **`ViewOnly`** (proprietate, se poate schimba oricând): `True` = titlul «K-BOT Browser FOREXE», `splitMain.Panel2.Enabled = False` (tot panoul din dreapta: andocare, înregistrare, opțiuni, pași, previzualizare), o înregistrare în curs e oprită (nu poate merge în spatele unui panou stins — operatorul n-ar mai putea-o opri); `False` = «K-BOT Recorder», totul aprins. **Andocare automată la `Shown`** (`EnsureDockedAsync`, publică — runner-ul o cheamă și pe un formular deja deschis, poate operatorul detașase între timp; gardată cu `_docking`, fiindcă `Shown` și runner-ul pot cere andocarea în aceeași clipă, iar verificarea `_isDocked` a executorului stă înaintea primului `Await`). Un eșec de andocare e SPUS operatorului (`KBotMessage`), nu lăsat ca panou gol. `AttachExecutor` cu ACELAȘI executor nu mai desprinde și reatașează (ar fi detașat browserul din fața operatorului). `DetachExecutor` detașează întâi browserul dacă e andocat aici (executorul urmează să moară la reconectare; un Chromium care moare copil al panoului lasă panoul cu un handle mort) și se abonează/dezabonează la `OnDockStateChanged` → `UpdateButtons` pe firul de UI. Timerul de resincronizare nu mai are ramura «neandocat»: browserul neandocat e în afara ecranului, n-are ce ține deasupra formularului.
- **`ForexeRunner.vb`** — `ShowBrowserAsync(owner)` deschide recorderul cu `ViewOnly = True` (formular nou → se andochează singur la `Shown`; formular deja deschis → `Activate` + `EnsureDockedAsync`). Modul se poate doar RIDICA: «Arată browserul» peste un recorder în care operatorul înregistrează îl lasă în modul de înregistrare; «Recorder» peste un formular doar pentru privit îi aprinde panoul (`ShowRecorderCore(owner, viewOnly)`, comun). `HideBrowserAsync`: ÎNTÂI `_executor.HideBrowserWindowAsync()` (detașează de oriunde), APOI închide formularul dacă e `ViewOnly` (nu mai are ce arăta); cel de înregistrare rămâne cu pașii lui și își stinge singur butoanele prin eveniment. `stealthMode:=True` întotdeauna. `DetachRecorder` (la moartea executorului) închide și el formularul doar-pentru-privit. **Eveniment nou `BrowserVisibilityChanged`** (din `OnDockStateChanged`), ca butonul din consolă să-și schimbe eticheta când operatorul închide fereastra cu X-ul, fără să fi apăsat nimic în consolă.
- **`IForexeRunner.vb`** — `ShowBrowserAsync(owner As IWin32Window)` (avea nevoie de proprietar pentru fereastră) + `Event BrowserVisibilityChanged As EventHandler`.

##### `KBot.App`

- **`Forexe/ForexeController.vb`** — `ToggleBrowserAsync` / `ShowBrowserAsync` dau `Owner` runner-ului; se abonează la `BrowserVisibilityChanged` și îl retransmite ca `StateChanged`, deci `ForexeConsoleForm.ActualizeazaStarea` recitește eticheta «Arată/Ascunde browserul».
- **`Forexe/ForexeConsoleForm.Designer.vb`** — textul tooltip-ului spune ce face acum butonul («Deschide o fereastră K-BOT cu pagina … Închiderea ferestrei ascunde browserul la loc.»).

##### Altele

- **`tests/KBot.App.Tests/ForexeControllerFailureTests.vb`** — `FakeRunner` urmează interfața (semnătura `ShowBrowserAsync(owner)`, evenimentul nou). Doar ca proiectul de teste să compileze; nimic rulat.
- **`KBot.DevHarness/Internal/BrowserDockHarnessForm.vb`** — linia de stare la detașare spune «Detașat și ascuns (în afara ecranului)», fiindcă asta se întâmplă acum.

#### Rezultate

- `dotnet build` pe `src\KBot.Forexe`, `src\KBot.App`, `src\KBot.DevHarness`: **0 erori, 0 avertismente** (în afara `MSB3825` preexistent pe `.resx` în App).
- **NIMIC nu s-a rulat**: niciun browser lansat, nicio andocare făcută, nimic pe ecran, nicio suită de teste construită sau rulată (cerere explicită: «no git. no tests!»). **Nimic comis.**

#### Neverificat / amânat

- **Andocarea automată la `Shown` pe un formular nou** — `EnsureDockedAsync` pornește o dată din `ShowBrowserAsync` (imediat după `Show`) și o dată din `Shown`; gardul `_docking` trebuie să lase una singură să treacă. Verificat doar prin citire; de văzut la prima apăsare reală pe «Arată browserul».
- **Închiderea recorderului cu X-ul în timp ce andocarea e în curs** (fereastra abia deschisă, browserul încă neandocat): `FormClosing` găsește `IsDocked = False` și închide; andocarea care se termină după aceea ar reparenta într-un panou eliberat. Caz de o fracțiune de secundă, netratat.
- **Recorderul doar-pentru-privit nu are nicio comandă** — panoul din dreapta e vizibil dar stins, cum a cerut operatorul («disabled»). Dacă spațiul mort deranjează, `splitMain.Panel2Collapsed` în `ApplyMode` e o linie.
- `ExecuteMinimizeAsync` (pasul `<Minimize>`) mută browserul liber la (-2000,-2000): tot ascuns, dar în alt loc decât parcarea de stealth. Inofensiv; nealiniat.

<a id="slice-0070-02"></a>

#### Corecție 0070-02 — tastatura nu ajungea în pagina andocată (19.09.2026)

**Văzut pe ecran de operator:** cu browserul andocat, mouse-ul merge, dar nimic tastat nu ajunge în pagină.

**Cauza:** rama Chromium era andocată ca `WS_CHILD` adevărat. Windows nu activează niciodată o fereastră-copil — activează strămoșul de nivel superior, adică formularul nostru — iar Chromium trimite caracterele tastate prin metoda lui de intrare doar cât timp propria fereastră este cea activă (`WM_ACTIVATE`). Nici nu se poate falsifica: Chromium aruncă un `WM_ACTIVATE` adresat unei ferestre cu `WS_CHILD`. Apăsările de taste ajungeau, caracterele erau pierdute înainte de pagină. Aceeași familie de problemă ca la Office (`OfficeDocumentHost.PulseActivation`, unde mesajul lipsă era `WM_ACTIVATEAPP` și acolo se putea trimite de mână).

**Schimbarea (`Executor/WorkflowExecutor.Docking.vb`):**
- Rama e reparentată **fără `WS_CHILD`** — fereastră de nivel superior ca stil, agățată de panou prin `SetParent` (forma clasică «notepad în panou»). Rămâne poziționată în pixelii de client ai panoului, tăiată de el, mutată cu formularul; dar un clic pe pagină activează fereastra browserului, ca atunci când stătea singură, și tastarea ajunge în pagină. Efect vizibil: cât operatorul tastează în pagină, bara de titlu a formularului gazdă se desenează inactivă.
- `WS_EX_TOOLWINDOW` rămâne PUS și andocat (înainte era scos): o fereastră de nivel superior ca stil și-ar putea câștiga un buton în bara de activități.
- `GetParent` înlocuit cu `GetAncestor(GA_PARENT)` (`ParentOf`): pentru o fereastră fără `WS_CHILD`, `GetParent` răspunde cu proprietarul, nu cu părintele, și verificarea de după `SetParent` ar fi picat.
- Activarea unei ferestre nu ridică strămoșul ei: executorul ascultă `Deactivate` pe formularul gazdă (`ListenToHostForm` / `OnHostFormDeactivate`) și, când activarea a plecat CĂTRE propriul browser, ridică formularul cu `SetWindowPos(HWND_TOP, SWP_NOACTIVATE)` — altfel un formular pe jumătate acoperit rămânea acoperit cât operatorul tasta. Orice altă pierdere de activare e lăsată în pace.
- `AttachThreadInput` rămâne.

**Rezultat:** `dotnet build` pe `src\KBot.Forexe` 0/0, `src\KBot.App` 0 erori (doar `MSB3825`). Diagnostic din citirea codului Chromium din memorie, nu măsurat; de confirmat pe ecran la prima tastare. Nimic rulat, nimic comis.

**Neverificat:** dacă și fără `WS_CHILD` Windows dă activarea tot rădăcinii (formularului) — atunci
tastarea tot nu merge, iar următorul pas este `SetFocus` pe rama browserului din firul UI (firele sunt legate) la `WM_PARENTNOTIFY` pe panou; gapul lateral al ramei poate diferi cu un pixel-doi.

<a id="slice-0074"></a>

### SLICE 0074 — Vederea «Browser FOREXE» în shell: pagina andocată, arborele ↔ pagina

**Data:** 21.09.2026 **Cererea operatorului:** «add a new view in the kbotform with the browser inside it which will only be visible if the user is connected … this WILL not have the recording panel in it - it is just for user use. it will HAVE the custom js menu and it will do everything we talked about» — adică, din mesajul anterior: «when one angajament is selected from the treeview, the browser will do a workflow selecting that angajament and opening it through (`<a href="javascript:;" id="idfa">Modificare</a>`) … it will stop after clicking on that href. also, if the user searches in the browser an angajament and opens it, the treeview will know about it and select it (without triggering the forexe part again)».

#### Ce s-a schimbat și de ce

##### 1. Vederea `BrowserView` (`Views/BrowserView.vb` + `.Designer.vb`, cheia `browser`)

- Intrare nouă în navigația din stânga, **«Browser FOREXE»** (după «Plăți», înaintea separatorului; iconița `Sekkyumu-Developpers-Web-Browser.32`). Poarta ei e **sesiunea**, nu nodul: `ApplyViewGating` o arată doar cu `_controller.IsConnected`, iar `KbotForm.Browser.vb` o reevaluează la fiecare `StateChanged` al coordonatorului (conectare, sesiune pierdută, browser mutat) și, dacă sesiunea moare cu vederea deschisă, cade pe «Sumar».
- Vederea are **doar** pagina: `pnlBrowser` (gazda `SetParent`), o linie de stare sus și butonul «Adu browserul aici» (vizibil doar când browserul e în altă parte). Niciun panou de înregistrare.
- **Andocarea urmează vizibilitatea**: la `VisibleChanged = True` cere `ForexeController.DockBrowserAsync(pnlBrowser)`; la `False` (s-a ales altă vedere) cere `ReleaseBrowserAsync(pnlBrowser)`, care detașează și parchează browserul off-screen **doar dacă e chiar în panoul ei**. `Resize` pe panou → `SyncBrowserBoundsAsync` (debounce 150 ms).
- Meniul K-BOT din felia 0073 (`ForexeWatch.js`) intră în pagină la andocare, prin `StartWatchingAsync`, ca în recorder.
- **La închiderea shell-ului** (`KbotForm.OnFormClosing`, după ce handlerii au avut ocazia să anuleze) browserul e eliberat sincron: `DestroyWindow` ia cu el toți copiii din arborele de ferestre, inclusiv fereastra Chromium, și asta ar omorî sesiunea.

##### 2. Arbore → pagină

- **`adlop - Deschide Angajament.wfl`** (nou, `Workflows\`, copiat în output): preambulul din «Prelucrare Completa» — reset (`#select2-drop-mask`, `#statlogo`), «Listă angajamente», `Fill` `input[name='cod']`, «Caută», `Exit` dacă lista e goală, meniul rândului (`a.haction i.glyphicon-list:visible`), `.divaction a:has-text('Modificare')` cu navigare, `WaitFor li.tab0` — și **se oprește acolo**. Selectorul lui «Modificare» e cel al robotului (același `<a id="idfa">` pe care l-a indicat operatorul), nu unul nou.
- `WorkflowCatalog.DeschideAngajamentFile`, `JobBuilder.BuildDeschideAngajament(cod)`, `ForexeController.DeschideAngajamentAsync(cod)` (porțile obișnuite: ocupat, sesiune; `IntraInLucru` / `IesDinLucru`; eșecul în `LastFailure` și pe consolă; `<Exit>`-ul fluxului = angajamentul nu e în lista FOREXE).
- **Când pornește robotul** — regula e strictă, altfel se calcă pe robotul urmăririi: `SetContext` **doar înregistrează** selecția. Deschiderea se face în exact două momente: (a) **clicul operatorului** pe un nod cu vederea deschisă (`Tree_NodeMouseUp` → `BrowserView.DeschideSelectia()`), (b) **activarea vederii** cu un nod selectat (după andocare). Niciodată la `SetContext`-ul unei reîncărcări de arbore: `PreiaOperatiuneaAsync` (0073) reîncarcă arborele în mijlocul descărcării, iar o deschidere pornită acolo ar lovi «Rulează deja o operație» pe unicul browser și l-ar lua pe operator de pe angajamentul abia făcut de mână.
- Înainte de a trimite robotul, vederea compară codul selectat cu **codul din pagină** (`_codPagina`): același cod → nimic. La activare codul din pagină se citește **sincron** (`ReadPageAngajamentAsync` → `window._kbotWatch.getCod()`), fiindcă evenimentul «page» al reluării urmăririi ar veni după decizie.

##### 3. Pagină → arbore

- `ForexeWatch.js` emite **`page`** cu `cod` de fiecare dată când codul din antet (`.well.well-small h4 span:nth-child(2)`) **se schimbă** — la `boot`, la bătaia de 2 s (re-randările Wicket nu sunt navigări) și **forțat la `setSuspended(false)`**, ca pagina lăsată de robot să fie raportată în clipa în care e dată înapoi. Expune și `reportPage()` / `getCod()`.
- .NET: `ForexeWatchEventKind.PageOpened` (nou), logat pe consolă la Debug; `KbotForm.ForexeWatch.vb` îl trimite la `TrateazaPaginaDeschisa(cod)` (`KbotForm.Browser.vb`): întâi `BrowserView.NoteazaCodulPaginii(cod)`, apoi — dacă nodul nu e deja selectat — găsește nodul după `Tag` în `tree.Items`, `SelectAndReveal`, `ApplyViewGating`, `SetContext`, `RefreshInfoForm`: exact ce face un clic, **fără** `DeschideSelectia`, deci fără robot. Un cod care nu e în lista perioadei e spus pe consolă, nu înghițit.

##### 4. Două gazde pentru un singur browser (runner / recorder)

- `WorkflowExecutor.DockHost` (nou): panoul în care e andocat browserul.
- `IForexeRunner`: `DockBrowserAsync(host)` (preia browserul dacă e andocat altundeva; un recorder deschis doar pentru privit se închide, unul de înregistrare rămâne și își stinge butoanele), `ReleaseBrowserAsync(host)` (tăcut dacă nu e al gazdei), `SyncBrowserBoundsAsync`, `BrowserHost`, `ReadPageAngajamentAsync`. `FakeRunner` din teste le declară (doar editat).
- `RecorderForm`: `DockedHere` — toate verificările de andocare (Shown, «Andochează», detașare la închidere, resync, `DetachExecutor`) înseamnă acum «andocat ÎN panoul meu»; `EnsureDockedAsync` preia browserul din vederea shell-ului când recorderul e cerut.
- `StartWatchingAsync` nu mai trezește urmărirea dacă **un job o ține suspendată** (vederea poate andoca în timpul unei descărcări; o trezire sub robot i-ar arma operațiuni pe clicuri).
- Butonul «browser» din banda de jos a shell-ului duce acum la **vederea** «Browser FOREXE» (și înapoi la «Sumar» dacă e deja acolo); butonul din consolă rămâne pe recorderul în mod privit (care ia browserul; vederea oferă «Adu browserul aici»).

#### Fișiere atinse

- `src/KBot.Forexe/Workflows/adlop - Deschide Angajament.wfl` (nou), `KBot.Forexe.vbproj` (copiere + 1.0.11 ▸ **1.0.12**), `WorkflowCatalog.vb`, `JobBuilder.vb`, `IForexeRunner.vb`, `ForexeRunner.vb`, `Executor/WorkflowExecutor.Docking.vb` (`DockHost`), `Executor/WorkflowExecutor.Watch.vb` (`ReadPageAngajamentAsync`, `PageOpened`, reluarea condiționată), `Models/ForexeWatchEvent.vb`, `Services/JavaScripts/ForexeWatch.js`, `Helpers/RecorderForm.vb`
- `src/KBot.App/Views/BrowserView.vb` + `.Designer.vb` (noi), `KbotForm.Browser.vb` (nou), `KbotForm.vb` (CreateView, gating, clic, butonul din bandă), `KbotForm.Designer.vb` (`KBotNavItem9`), `KbotForm.ForexeWatch.vb`, `Forexe/ForexeController.vb`, `KBot.App.vbproj` (1.0.34 ▸ **1.0.35**)
- `tests/KBot.App.Tests/ForexeControllerFailureTests.vb` (`FakeRunner`, doar editat)
- `docs/worklog/KBOT_STATUS.md`, acest fișier

#### Rezultatele testelor

- `dotnet build` pe `src\KBot.Forexe`, `src\KBot.App` (`--no-incremental`), `src\KBot.DevHarness`: **0 erori, 0 avertismente**. `node --check ForexeWatch.js`: valid. `.wfl`-ul nou apare în `bin\...\Workflows\`.
- **Nicio suită xUnit / pytest construită sau rulată** (regula casei).
- **Văzut pe ecran** prin `DrawToBitmap` (proiect aruncabil, runner fals, schema Classic): vederea neconectată (mesajul din panou, fără buton) și conectată cu andocarea refuzată (mesajul erorii rămâne în panou, «Adu browserul aici» sus-dreapta).

#### Neverificat / amânat

- **Nimic nu a atins un browser adevărat sau CABWeb.** De confirmat la prima sesiune: andocarea în `viewHost` (formularul shell-ului e `KBotShellForm` fără margini — `ListenToHostForm` și bara ascunsă a browserului sunt cele din recorder), comutarea între vederi (detașare / andocare la fiecare schimbare), fluxul «Deschide Angajament» pe pagina reală, evenimentul `page` pe re-randările Wicket, și lupta recorder ↔ vedere.
- Pagina de după o descărcare (0073) nu are antet de angajament, deci după descărcare vederea arată «alegeți un angajament»; un clic pe nod îl readuce. Nu se revine automat — deliberat (vezi §2).
- Nu există legătură pagină → arbore pentru un angajament din **altă perioadă** decât cea din combo-uri: se spune pe consolă și atât.
