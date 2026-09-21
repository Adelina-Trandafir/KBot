# SLICE 0074 — Vederea «Browser FOREXE» în shell: pagina andocată, arborele ↔ pagina

**Data:** 21.09.2026
**Cererea operatorului:** «add a new view in the kbotform with the browser inside it which will
only be visible if the user is connected … this WILL not have the recording panel in it - it is
just for user use. it will HAVE the custom js menu and it will do everything we talked about» —
adică, din mesajul anterior: «when one angajament is selected from the treeview, the browser will
do a workflow selecting that angajament and opening it through (`<a href="javascript:;"
id="idfa">Modificare</a>`) … it will stop after clicking on that href. also, if the user searches
in the browser an angajament and opens it, the treeview will know about it and select it (without
triggering the forexe part again)».

## Ce s-a schimbat și de ce

### 1. Vederea `BrowserView` (`Views/BrowserView.vb` + `.Designer.vb`, cheia `browser`)

- Intrare nouă în navigația din stânga, **«Browser FOREXE»** (după «Plăți», înaintea
  separatorului; iconița `Sekkyumu-Developpers-Web-Browser.32`). Poarta ei e **sesiunea**, nu
  nodul: `ApplyViewGating` o arată doar cu `_controller.IsConnected`, iar `KbotForm.Browser.vb`
  o reevaluează la fiecare `StateChanged` al coordonatorului (conectare, sesiune pierdută,
  browser mutat) și, dacă sesiunea moare cu vederea deschisă, cade pe «Sumar».
- Vederea are **doar** pagina: `pnlBrowser` (gazda `SetParent`), o linie de stare sus și butonul
  «Adu browserul aici» (vizibil doar când browserul e în altă parte). Niciun panou de înregistrare.
- **Andocarea urmează vizibilitatea**: la `VisibleChanged = True` cere
  `ForexeController.DockBrowserAsync(pnlBrowser)`; la `False` (s-a ales altă vedere) cere
  `ReleaseBrowserAsync(pnlBrowser)`, care detașează și parchează browserul off-screen **doar dacă
  e chiar în panoul ei**. `Resize` pe panou → `SyncBrowserBoundsAsync` (debounce 150 ms).
- Meniul K-BOT din felia 0073 (`ForexeWatch.js`) intră în pagină la andocare, prin
  `StartWatchingAsync`, ca în recorder.
- **La închiderea shell-ului** (`KbotForm.OnFormClosing`, după ce handlerii au avut ocazia să
  anuleze) browserul e eliberat sincron: `DestroyWindow` ia cu el toți copiii din arborele de
  ferestre, inclusiv fereastra Chromium, și asta ar omorî sesiunea.

### 2. Arbore → pagină

- **`adlop - Deschide Angajament.wfl`** (nou, `Workflows\`, copiat în output): preambulul din
  «Prelucrare Completa» — reset (`#select2-drop-mask`, `#statlogo`), «Listă angajamente», `Fill`
  `input[name='cod']`, «Caută», `Exit` dacă lista e goală, meniul rândului
  (`a.haction i.glyphicon-list:visible`), `.divaction a:has-text('Modificare')` cu navigare,
  `WaitFor li.tab0` — și **se oprește acolo**. Selectorul lui «Modificare» e cel al robotului
  (același `<a id="idfa">` pe care l-a indicat operatorul), nu unul nou.
- `WorkflowCatalog.DeschideAngajamentFile`, `JobBuilder.BuildDeschideAngajament(cod)`,
  `ForexeController.DeschideAngajamentAsync(cod)` (porțile obișnuite: ocupat, sesiune;
  `IntraInLucru` / `IesDinLucru`; eșecul în `LastFailure` și pe consolă; `<Exit>`-ul fluxului =
  angajamentul nu e în lista FOREXE).
- **Când pornește robotul** — regula e strictă, altfel se calcă pe robotul urmăririi:
  `SetContext` **doar înregistrează** selecția. Deschiderea se face în exact două momente:
  (a) **clicul operatorului** pe un nod cu vederea deschisă (`Tree_NodeMouseUp` →
  `BrowserView.DeschideSelectia()`), (b) **activarea vederii** cu un nod selectat (după andocare).
  Niciodată la `SetContext`-ul unei reîncărcări de arbore: `PreiaOperatiuneaAsync` (0073)
  reîncarcă arborele în mijlocul descărcării, iar o deschidere pornită acolo ar lovi
  «Rulează deja o operație» pe unicul browser și l-ar lua pe operator de pe angajamentul abia
  făcut de mână.
- Înainte de a trimite robotul, vederea compară codul selectat cu **codul din pagină**
  (`_codPagina`): același cod → nimic. La activare codul din pagină se citește **sincron**
  (`ReadPageAngajamentAsync` → `window._kbotWatch.getCod()`), fiindcă evenimentul «page» al
  reluării urmăririi ar veni după decizie.

### 3. Pagină → arbore

- `ForexeWatch.js` emite **`page`** cu `cod` de fiecare dată când codul din antet
  (`.well.well-small h4 span:nth-child(2)`) **se schimbă** — la `boot`, la bătaia de 2 s
  (re-randările Wicket nu sunt navigări) și **forțat la `setSuspended(false)`**, ca pagina lăsată
  de robot să fie raportată în clipa în care e dată înapoi. Expune și `reportPage()` / `getCod()`.
- .NET: `ForexeWatchEventKind.PageOpened` (nou), logat pe consolă la Debug;
  `KbotForm.ForexeWatch.vb` îl trimite la `TrateazaPaginaDeschisa(cod)` (`KbotForm.Browser.vb`):
  întâi `BrowserView.NoteazaCodulPaginii(cod)`, apoi — dacă nodul nu e deja selectat — găsește
  nodul după `Tag` în `tree.Items`, `SelectAndReveal`, `ApplyViewGating`, `SetContext`,
  `RefreshInfoForm`: exact ce face un clic, **fără** `DeschideSelectia`, deci fără robot. Un cod
  care nu e în lista perioadei e spus pe consolă, nu înghițit.

### 4. Două gazde pentru un singur browser (runner / recorder)

- `WorkflowExecutor.DockHost` (nou): panoul în care e andocat browserul.
- `IForexeRunner`: `DockBrowserAsync(host)` (preia browserul dacă e andocat altundeva; un recorder
  deschis doar pentru privit se închide, unul de înregistrare rămâne și își stinge butoanele),
  `ReleaseBrowserAsync(host)` (tăcut dacă nu e al gazdei), `SyncBrowserBoundsAsync`, `BrowserHost`,
  `ReadPageAngajamentAsync`. `FakeRunner` din teste le declară (doar editat).
- `RecorderForm`: `DockedHere` — toate verificările de andocare (Shown, «Andochează», detașare la
  închidere, resync, `DetachExecutor`) înseamnă acum «andocat ÎN panoul meu»; `EnsureDockedAsync`
  preia browserul din vederea shell-ului când recorderul e cerut.
- `StartWatchingAsync` nu mai trezește urmărirea dacă **un job o ține suspendată** (vederea poate
  andoca în timpul unei descărcări; o trezire sub robot i-ar arma operațiuni pe clicuri).
- Butonul «browser» din banda de jos a shell-ului duce acum la **vederea** «Browser FOREXE»
  (și înapoi la «Sumar» dacă e deja acolo); butonul din consolă rămâne pe recorderul în mod
  privit (care ia browserul; vederea oferă «Adu browserul aici»).

## Fișiere atinse

- `src/KBot.Forexe/Workflows/adlop - Deschide Angajament.wfl` (nou), `KBot.Forexe.vbproj`
  (copiere + 1.0.11 ▸ **1.0.12**), `WorkflowCatalog.vb`, `JobBuilder.vb`, `IForexeRunner.vb`,
  `ForexeRunner.vb`, `Executor/WorkflowExecutor.Docking.vb` (`DockHost`),
  `Executor/WorkflowExecutor.Watch.vb` (`ReadPageAngajamentAsync`, `PageOpened`, reluarea
  condiționată), `Models/ForexeWatchEvent.vb`, `Services/JavaScripts/ForexeWatch.js`,
  `Helpers/RecorderForm.vb`
- `src/KBot.App/Views/BrowserView.vb` + `.Designer.vb` (noi), `KbotForm.Browser.vb` (nou),
  `KbotForm.vb` (CreateView, gating, clic, butonul din bandă), `KbotForm.Designer.vb`
  (`KBotNavItem9`), `KbotForm.ForexeWatch.vb`, `Forexe/ForexeController.vb`, `KBot.App.vbproj`
  (1.0.34 ▸ **1.0.35**)
- `tests/KBot.App.Tests/ForexeControllerFailureTests.vb` (`FakeRunner`, doar editat)
- `docs/worklog/KBOT_STATUS.md`, acest fișier

## Rezultatele testelor

- `dotnet build` pe `src\KBot.Forexe`, `src\KBot.App` (`--no-incremental`), `src\KBot.DevHarness`:
  **0 erori, 0 avertismente**. `node --check ForexeWatch.js`: valid. `.wfl`-ul nou apare în
  `bin\...\Workflows\`.
- **Nicio suită xUnit / pytest construită sau rulată** (regula casei).
- **Văzut pe ecran** prin `DrawToBitmap` (proiect aruncabil, runner fals, schema Classic):
  vederea neconectată (mesajul din panou, fără buton) și conectată cu andocarea refuzată
  (mesajul erorii rămâne în panou, «Adu browserul aici» sus-dreapta).

## Neverificat / amânat

- **Nimic nu a atins un browser adevărat sau CABWeb.** De confirmat la prima sesiune: andocarea
  în `viewHost` (formularul shell-ului e `KBotShellForm` fără margini — `ListenToHostForm` și
  bara ascunsă a browserului sunt cele din recorder), comutarea între vederi (detașare / andocare
  la fiecare schimbare), fluxul «Deschide Angajament» pe pagina reală, evenimentul `page` pe
  re-randările Wicket, și lupta recorder ↔ vedere.
- Pagina de după o descărcare (0073) nu are antet de angajament, deci după descărcare vederea
  arată «alegeți un angajament»; un clic pe nod îl readuce. Nu se revine automat — deliberat
  (vezi §2).
- Nu există legătură pagină → arbore pentru un angajament din **altă perioadă** decât cea din
  combo-uri: se spune pe consolă și atât.
