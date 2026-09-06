# SLICE-0053 — K-BOT Recorder (fazele A–F)

Data: 2026-09-06

## Ce s-a schimbat și de ce

Formular nou în `KBot.Forexe` care andochează browserul Playwright deja pornit, înregistrează
ce face operatorul pe CABWeb și generează un fișier `.wfl`. Câștigul urmărit: dispare munca de
găsit selectori care supraviețuiesc unui rerender Wicket.

### Faza A — andocare falsă
`WorkflowExecutor.Docking.vb`: fereastra Chromium **nu** e reparentată (fără `SetParent`) — e
doar ținută peste `pnlBrowser` și imediat deasupra formularului în z-order. Coordonatele
WinForms (pixeli fizici, PerMonitorV2) se convertesc în DIP pentru
`Browser.setWindowBounds` cu `* 96 / DeviceDpi`.
Gărzi `_isDocked` adăugate în `HideBrowserWindowAsync`, `ShowBrowserWindowAsync` și
`ExecuteMinimizeAsync` — altfel un workflow ar arunca browserul off-screen în timpul
înregistrării. `UndockBrowserAsync` își face propriul `setWindowBounds`, ca să nu fie blocat
de propria gardă.

### Faza B — `Recorder.js`
Resursă embedded nouă, callback propriu `_kbotRecorderCallback` (numele
`_clickMonitorCallback` / `_keyMonitorCallback` sunt deja ocupate de `ClickMonitor.vb`, iar
`ExposeFunctionAsync` aruncă la a doua expunere). Gardă `_kbotRecorderInstalled`.
Listeneri în faza de capture: `mousedown`, `input`, `change`, `focusout`, `keydown`
(doar Enter/Tab). Clasificare widget (select2-mask / -open / -search / -pick, wicket-select,
paginator, row-action). Până la 5 candidați de selector rankați; **`id` nu e niciodată sursă
de selector** (Wicket regenerează id-urile). `matchCount` pentru `:has-text()` se numără
manual, `querySelectorAll` nu înțelege sintaxa Playwright.

### Faza C — captura în .NET
`RecordedStep`, `SelectorCandidate`, `RecordedAncestor`, `RecorderComment` +
`WorkflowExecutor.Recorder.vb`. Instalarea se face o singură dată per executor;
`StopRecording` doar comută un boolean. Scriptul se injectează **și** prin `AddInitScriptAsync`
(navigări viitoare) **și** printr-un `EvaluateAsync` pe pagina curentă — fără al doilea,
primul click nu s-ar înregistra niciodată. Corelarea AJAX folosește monitorul Wicket:
`#animlogo` care iese din `display:none` în 1500 ms după un pas ⇒ `TriggeredAjax`; intrarea
`IDLE` următoare dă `IdleAfterMs`. `#statlogo` e ignorat complet.

### Faza D — `RecorderCompactor.vb`
Clasă pură (fără UI, fără browser, fără stare globală). Cele nouă reguli din plan:
zgomot eliminat, select2 cu căutare (bloc fix de 5 acțiuni), select2 fără căutare (3 acțiuni),
`<select>` Wicket + regula sursă→indicator (`<Wait seconds="1">` obligatoriu),
Fill colapsat, Click, WaitFor automat, notă de buclă la paginator, bloc de reset.

### Faza E — `WflWriter.vb`
`IWorkflowAction` → XML. Atributele se scriu doar când diferă de ce ar reface
`WorkflowParser`. Selectorii cu `:has-text('…')` care conțin apostrof se trunchiază la cel mai
lung fragment fără apostrof de minimum 6 caractere; dacă nu există, aruncă
`InvalidOperationException` cu mesaj în română (fail-fast, nu scrie un selector rupt).

### Faza F — `RecorderForm`
`SplitContainer` vertical: stânga `pnlBrowser` (gazda), dreapta bara de andocare, bara de
înregistrare, opțiunile, lista de pași, panoul de detaliu, bara de generare și previzualizarea.
Orice modificare (candidat, bifă, ștergere, reordonare, opțiune globală) regenerează
previzualizarea. Ștergerea e logică (`Deleted`), ca numerotarea traseului brut să rămână
coerentă. `TopMost` rămâne `False` — un formular TopMost ar acoperi browserul andocat.
Repoziționarea browserului la mutare/redimensionare/splitter e debounce-uită la 150 ms.

## Fișiere atinse

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

## Abateri de la documentul de predare (motivate)

1. **Faza A NU era livrată.** Documentul o marca „Livrat"; în repo nu exista niciun fișier
   `Docking` sau `RecorderForm` (nici urmărit, nici neurmărit). A fost implementată în cadrul
   acestei felii.
2. **`waitNavigation` — documentul greșește.** Documentul spune că `True` e implicit și se
   omite. `ClickAction.WaitNavigation` e `True` în model, dar `WorkflowParser` îl citește cu
   `GetBoolAttribute(e, "waitNavigation", False)`. Parserul e cel care recitește fișierul, deci
   `waitNavigation="true"` se scrie **explicit**. Verificat prin round-trip.
3. **Comentarii și identificatori în engleză**, nu în română. RULE 0 din `CLAUDE.md` și memoria
   `code-english-only` au prioritate; româna rămâne doar în textele pe care le vede operatorul
   (log, UI, MsgBox, `LogValue`). Numele controalelor rămân cele din plan (`btnAndocheaza`,
   `btnSalveaza`…) — e vocabularul folosit unanim de restul formularelor.
4. **Fără blocuri `Namespace`** (regulă de casă). Modelele recorderului stau în namespace-ul
   rădăcină, nu în `WorkflowModels` — cerința reală a planului („zero proprietăți noi pe clasele
   din `WorkflowModels`") e respectată.
5. **Membri redenumiți în engleză**: `InsereazaWaitFor` → `InsertWaitFor`, `Sters` → `Deleted`,
   `SelectorAles` → `ChosenSelector`, `Comprima` → `Compact`, `Scrie` → `Write`,
   `InsereazaWaitForAutomat` → `InsertWaitForAutomatically`, `AdaugaBlocReset` → `AddResetBlock`,
   `TimeoutImplicit` → `DefaultTimeout`.
6. **Câmp nou în payload: `keyName`.** Structura din plan nu distinge Enter de Tab, deși
   `keydown` prinde ambele. Fără el pasul `key` era inutilizabil.
7. **`RecorderComment`** — regulile 8 și 9 cer comentarii XML în output, dar semnătura cerută
   returnează `List(Of IWorkflowAction)`. Comentariile călătoresc ca acțiune-marcaj, pe care
   `WflWriter` o transformă în `XComment`; `WorkflowParser` nu le vede (comentariile nu sunt
   elemente).
8. **ListView desenat manual.** `ThemeManager` nu acoperă `ListView` — în temă întunecată
   rămânea alb. Rândurile și antetul se desenează din paleta activă (fără culori literale).

## Rezultate teste

- `dotnet build src\KBot.Forexe\KBot.Forexe.vbproj` — **0 erori, 0 avertismente**.
- `dotnet build KBot.sln` — **0 erori**; cele 7 avertismente MSB3825 sunt preexistente
  (`.resx` din `KBot.App`, BinaryFormatter), fără legătură cu această felie.
- `node --check Recorder.js` — sintaxă validă.
- Resursele embedate ale assembly-ului conțin `Recorder.js` cu nume exact ⇒ `JsLoader` o
  găsește, fără coliziune de sufix.
- **Round-trip compactor → writer → `WorkflowParser.Parse`**, pe un traseu sintetic care
  acoperă: click cu AJAX, Fill colapsat din 3 `input` + 1 `change`, zgomot (`body`, mască
  select2, input ascuns scris de select2), `<select>` Wicket urmat de select2, select2 cu
  căutare, select2 fără căutare cu apostrof în text, paginator, Enter.
  Rezultat: 17 acțiuni recitite, **niciun `LogError`**, niciun selector pierdut. Blocul select2
  cu căutare a ieșit exact ca cele cinci acțiuni din workflow-urile scrise de mână, iar
  `Primăria d'Argeș Muscel` a fost trunchiat corect la `:has-text('Argeș Muscel')`.
- **Aspect verificat pe ecran** (`DrawToBitmap`), în temă Classic și în temă Dark: lista,
  panoul de detaliu (cu prefixul ⚠ pe candidații fragili) și previzualizarea XML se citesc
  corect în ambele.

## Rămas neverificat / amânat

- **Nimic nu deschide încă formularul.** Ca și `WicketMonitorForm`, `RecorderForm` nu are
  niciun call-site: `_executor` trăiește privat în `ForexeRunner` și nu e expus către
  `KBot.App`. Planul nu prevedea fișier de integrare, deci nu am atins seam-ul public al lui
  `ForexeRunner`. Până la cablare, forma se poate deschide doar din cod.
- **Andocarea nu a fost rulată împotriva unui browser real.** Rămân de verificat pe ecran, cu
  o sesiune vie: poziționarea, DPI între monitoare cu scalări diferite (conversia DIP
  folosește un singur `DeviceDpi`, al gazdei), z-order la activarea formularului, `<Minimize>`
  blocat și dialogul de PIN peste browserul andocat.
- **Captura nu a fost rulată împotriva CABWeb.** Clasificarea widget-urilor și scorurile
  candidaților sunt scrise după plan și după workflow-urile existente, nu măsurate pe pagina
  reală. Fluxul de creare angajament (care conține tot) rămâne testul de referință.
- Regula 7 poate insera un `WaitFor` imediat după `<Wait seconds="1">` al regulii sursă→
  indicator. E redundant, nu greșit, și e exact ce cere condiția din plan.
- Antetul `ListView` lasă o bandă nedesenată la dreapta ultimei coloane (zona pe care o
  pictează sistemul). Cosmetic.
- Fără proiect `tests/KBot.Forexe.Tests` — `RecorderCompactor` și `WflWriter` sunt clase pure
  și ar merita teste xUnit. Round-trip-ul de mai sus a fost rulat ca program de unică
  folosință, în afara repo-ului.
