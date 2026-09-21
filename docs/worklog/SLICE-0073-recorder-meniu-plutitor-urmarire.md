# SLICE 0073 — Recorder: mod de vizualizare, meniu plutitor în pagină, urmărirea operațiunilor FOREXE

**Data:** 21.09.2026
**Cererea operatorului:** «1. two options when showing it (1. default - no recording -
splitmain.panel2 should be hidden, 2 - recording - panel2 should be visible; 2. all the buttons in
the panel 2 must have defaultbutton from theme applied; 3. if possible i need a popup float menu
inserted into the browser which will be always visible for the user in the browser and which will
have zoom in / zoom out options. also, when certain buttons in the browser are clicked it must
start a kind of recording which will send the data back to kbot.app.forexe … when the user, for
instance, creates a new Angajament in the ForexeCab app, my app will instantly know it and when he
finishes pressing a certain button, all the info will be sent back to kbot and saved to the server -
just like when downloading from forexecab. it, then, should start a new window getting the history
from that angajament for the selected timeframe (when the user made the first click until it pressed
that button)» — completat: «it must be able to do it for: modificare rezervare, adaugare/modificare
receptie si creeare anagament (simplu sau multiplu)» și «citeste wfl-urile de aici:
Workflows\Creare. Lipseste DOAR modificare receptie. o sa ramana TODO dupa ce ii aflu fluxul.»

## Ce s-a schimbat și de ce

### 1. Cele două moduri ale recorderului (`RecorderForm`)

- `ViewOnly = True` («Arată browserul») **strânge** panoul din dreapta (`splitMain.Panel2Collapsed`),
  nu-l mai dezactivează: pagina are toată fereastra. `ViewOnly = False` («Recorder») îl desface.
  Comutarea cere o resincronizare a browserului andocat (`ScheduleResync`), fiindcă strângerea nu
  ridică `SplitterMoved`.
- Toate butoanele din panoul din dreapta, la orice adâncime (`ButtonsUnder`), primesc **butonul
  implicit al temei**: `ButtonStyles.ApplyDefault` (nou în `KBot.Theming`) — exact regula generică a
  lui `ThemeManager` pentru un `Button` (sloturile ButtonBack / ButtonText / ButtonBorder, colțuri
  moderne când schema le cere), dar aplicată explicit, deci și sub o schemă care păstrează culorile
  de sistem. Se reaplică la fiecare `ThemeChanged`.

### 2. Meniul plutitor din pagină (`Services/JavaScripts/ForexeWatch.js`, resursă embedată)

- Un panou fix, jos-dreapta, mutabil cu mouse-ul (poziția rămâne în `localStorage`): **−, +, 100 %**
  (zoom CSS pe `documentElement`, meniul contra-scalat ca să-și țină mărimea; factorul rămâne în
  `localStorage`, deci supraviețuiește navigărilor), linia de stare a urmăririi și trei butoane
  manuale: **▶ Începe, ■ Gata, ✕**.
- Se instalează ca `Recorder.js`: `AddInitScriptAsync` (navigările viitoare) + `EvaluateAsync`
  (pagina de pe ecran), gardat împotriva dublei instalări. Un temporizator îl repune dacă dispare.
- **Urmărirea operațiunilor** — reguli `OPS`, toate cu selectori copiați din `.wfl`-urile din
  `Workflows\Creare` (aceiași pe care îi apasă robotul):
  - `angajament`: start `a:has-text('Angajament nou')`; final = **prima** `button.btn-success` din
    `.form-group` (salvarea FINALĂ; a doua e cea intermediară «încă un rând» — deci «multiplu» dă o
    singură operațiune, la salvarea finală).
  - `rezervare` (doar cu `li.tab0.active`): start «Adaug…» (`button.btn-default.btn-small`) sau
    ochiul unui rând (`table.table-striped a:has(.glyphicon-eye-open)`); final
    `button.btn-success.btn-small:nth-child(1)` sau «Continuă» din modalul motivului
    (`div.modal-footer button.btn-success`).
  - `receptie` (doar cu `li.tab1.active`): start `button` «Adaugă»; final «Salvează» din
    `form.form-horizontal`.
  - **`receptie-modificare`: TODO** — fluxul nu e cunoscut; nu există reguli, doar butoanele manuale.
- După apăsarea salvării, scriptul **așteaptă să se așeze pagina** (sondaj la 400 ms, cel mult 30 s):
  `#animlogo` ocupat sau modal deschis → așteaptă; `.feedbackPanelERROR` / `.alert-danger` →
  salvare respinsă, operațiunea rămâne în curs; testul `done` al operațiunii (codul din
  `.well.well-small h4 span:nth-child(2)` prezent + starea paginii) → **finished**. Starea trece
  prin `sessionStorage`, deci o salvare care navighează e confirmată de pagina următoare.
- Evenimente către .NET prin `_kbotWatchCallback`: `started / finished / cancelled / info`, cu
  `op, label, cod, codAtStart, startedAt, finishedAt, url, message`.
- **Suspendare cât rulează robotul**: `ForexeRunner.RunJobAsync` cheamă
  `SetWatchSuspendedAsync(True/False)` în jurul lui `ExecuteAsync` — clicurile robotului ar arma
  operațiuni (Creare Angajament.wfl apasă chiar «Angajament nou»), iar meniul ar sta peste țintele
  lui Playwright. Suspendarea se ține și în `sessionStorage`, pentru navigările robotului.

### 3. Conducta .NET

- `Models/ForexeWatchEvent.vb` (+ `ForexeOperationKind`, `ForexeWatchEventKind`).
- `Executor/WorkflowExecutor.Watch.vb`: `StartWatchingAsync` (instalare o dată, `ExposeFunction`
  `_kbotWatchCallback`), `StopWatching`, `SetWatchSuspendedAsync`, evenimentul `OnWatchEvent`;
  scrie pe consolă liniile pe care le citește operatorul («A început…», «s-a salvat…»).
- `RecorderForm.EnsureDockedAsync` / «Andochează» → `StartWatchAsync` după andocare, în ambele
  moduri (un eșec e spus, nu desface andocarea).
- `IForexeRunner.OperationCaptured` → `ForexeRunner` (abonat la executor, dezabonat la
  `DisposeExecutorAsync`) → `ForexeController.OperatiuneCapturata` → shell.
- `KbotForm.ForexeWatch.vb` (partial nou): la **Finished** — pe firul de UI —
  - `rezervare / receptie / manual`: codul din pagină (la sfârșit, altfel la început), în lipsă
    nodul selectat în arbore (spus pe consolă); apoi **`DownloadNodeAsync`** (fără întrebarea
    recepțiilor de sărit — totul e proaspăt) + **`DuLaIngestieAsync`** (aceleași două faze ca
    iconița nodului) + fereastra de istoric.
  - `angajament`: întâi **sincronizarea listei** (drumul iconiței din subsol, fără caseta de
    mesaj), codurile noi = cele care nu erau în arbore + codul citit din pagină; fiecare e
    descărcat și ingerat pe rând, cu fereastra lui de istoric.
  - o singură operațiune în lucru la un moment dat; a doua e anunțată și lăsată pe iconița nodului.
- **`IstoricIntervalForm`** (nou, `KBotShellForm`): găzduiește `IstoricView`-ul real, încărcat cu
  `SetContextInterval(cod, deLa, panaLa)` — un segment nou de dată **cu oră**
  (`IstoricFilter.SetDataFxRange`), aplicat imediat după `ClearAll()`-ul obligatoriu din
  `LoadAsync`. Intervalul e lărgit cu **±2 minute** (`MarjaMinute`): momentele vin din ceasul
  mașinii, `DataFX` din ceasul serverului FOREXE. «Tot istoricul» scoate limita; «TOATE» din meniul
  de dată și «Reset» fac același lucru.

## Fișiere atinse

- `src/KBot.Theming/ButtonStyles.vb` (+`ApplyDefault`), `KBot.Theming.vbproj` (1.13.0 ▸ 1.13.1)
- `src/KBot.Forexe/Services/JavaScripts/ForexeWatch.js` (nou), `Models/ForexeWatchEvent.vb` (nou),
  `Executor/WorkflowExecutor.Watch.vb` (nou), `Helpers/RecorderForm.vb`, `IForexeRunner.vb`,
  `ForexeRunner.vb`, `KBot.Forexe.vbproj` (resursă + 1.0.10 ▸ 1.0.11)
- `src/KBot.App/Forexe/ForexeController.vb`, `KbotForm.vb` (`LeagaUrmarirea()` în Load),
  `KbotForm.ForexeWatch.vb` (nou), `Forexe/IstoricIntervalForm.vb` + `.Designer.vb` (noi),
  `Views/IstoricView.vb`, `Views/IstoricFilter.vb`, `KBot.App.vbproj` (1.0.32.4 ▸ 1.0.33.0)
- `tests/KBot.App.Tests/ForexeControllerFailureTests.vb` — `FakeRunner` declară noul eveniment
  (doar editat; nicio suită construită sau rulată)
- `docs/worklog/KBOT_STATUS.md`, acest fișier

## Rezultatele testelor

- `dotnet build` pe `src\KBot.Forexe`, `src\KBot.App` (`--no-incremental`), `src\KBot.DevHarness`:
  **0 erori, 0 avertismente**. `node --check ForexeWatch.js`: valid.
- **Nicio suită xUnit / pytest construită sau rulată** (regula casei).
- **Văzut pe ecran** prin `DrawToBitmap` (proiect aruncabil în scratchpad, schema Classic din
  fișierul temei, fără `SetScheme`): recorderul în modul înregistrare (panoul desfăcut, butoanele
  plate în culorile temei) și în modul vizualizare (titlu «K-BOT Browser FOREXE», panoul strâns,
  pagina pe toată fereastra).

## Neverificat / amânat

- **Nimic nu a atins un browser adevărat sau CABWeb.** Meniul plutitor, zoom-ul CSS pe paginile
  Wicket, potrivirea regulilor de start/final pe butoanele reale, sondajul de confirmare a salvării
  și conducta până la `DuLaIngestieAsync` + fereastra de istoric se confirmă la prima operațiune
  reală. Selectorii sunt cei ai robotului, dar robotul îi apasă într-o ordine controlată —
  operatorul poate ajunge pe alte drumuri (ex. pagina de editare a unui indicator deschisă altfel
  decât prin ochi).
- **`receptie-modificare` — TODO** până operatorul aduce fluxul; între timp, ▶ Începe / ■ Gata.
- **Codul unui angajament nou** se ia din antetul paginii după salvarea finală
  (`.well.well-small h4 span:nth-child(2)`, ca în `Creare Angajament.wfl` §4) ȘI din diferența
  listei; dacă antetul nu apare pe pagina de după salvare, rămâne diferența listei — care vede
  doar perioada afișată în arbore.
- **Descărcarea pornește imediat**, fără confirmare, cum a cerut operatorul («instantly»): robotul
  preia browserul andocat chiar după salvare. Dacă operatorul înlănțuie operațiuni pe aceeași
  pagină, a doua e anunțată și lăsată pe iconița nodului.
- Ceasurile: ±2 minute e o presupunere; dacă istoricul iese gol în fereastră, «Tot istoricul» arată
  totul, iar marja se poate mări.
- Nu există încă un fișier de reguli editabil de operator — regulile stau în `ForexeWatch.js`.
