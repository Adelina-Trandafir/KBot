# SLICE-0097-02 — Corective updates, second pass (operator request, 01.10.2026)

Seven operator requests (six in one message, the seventh added while the work was running).
Answers given by the operator before the code (01.10.2026): point 5 means the login window must
**not** have the «?» button (F1 stays); the work is recorded as **0097-02** (not a new slice).
The help for it is `SLICE-0000-25`.

## What changed and why

### 1. The main window's tour starts by itself
- `AppSettings.ShowInitialTour` (default **True**). After `KbotForm` has loaded its data
  (`MainForm_Load`, last step, posted with `BeginInvoke`) it calls
  `HelpService.StartInitialTour()`: the tour `tur-fereastra` (`HelpService.InitialTourId`) runs
  when the setting is on and the login reads the tour's part (a director never gets it; the
  DevHarness window without a session never gets it).
- It runs **at every start** until:
  1. it was **seen to the end** («Gata» on the last step, or the last steps skipped because their
     parts are not on screen) — counted however the tour was started (by itself or from «?»);
  2. the operator ticks **«Nu mai arăta turul inițial»** and closes the tour.
  Both write `ShowInitialTour = False` (`HelpService.InitialTourSeen`, called by
  `HelpTourRunner.Finish`). Closing without the box leaves it on.
- **Where the box is** (the operator left the place open): on the tour bubble itself, its own row
  between the text and the buttons (`HelpTourBubble.chkNuMaiArata`), shown only on the automatic
  tour (`ShowNeverAgain`). The button row has no room for it at 440 px, hence a row of its own;
  `FitToText` counts it.
- The way back: **Setări › Aplicație › Generale** «Arată turul ferestrei principale la pornirea
  K-BOT» (`chkTurInitial`), which also follows `AppSettings.Changed` (it unticks itself when the
  tour ends with the settings window open).

### 2. Confirmation questions in the in-page browser are answered «Da»
- `ForexeWatch.js`, new section 15. While the OPERATOR drives the page (never while the robot
  does), a FOREXE window that asks is answered by K-BOT: a visible `.modal` whose text contains
  «sigur», with **nothing to fill in** (no visible `textarea` / `select` / `input` other than
  hidden / button / submit) and with **both** a yes and a no button in its footer. «Yes» is known
  by its word (Da / OK / Continuă / Confirmă), else by FOREXE's green button (`btn-success`); «no»
  by its word (Nu / Renunță / Anulează / Închide / Înapoi), else any other button.
- So: a window that waits for data (the motive of a reservation, the description of a
  definitivare) is left alone, and so is one that only tells something (a single button).
- One press per opening (`_kbotAnswered` on the element, cleared when the window is hidden). It is
  looked for on every DOM change (inserted, or only shown through class / style), 150 ms after the
  burst, and on the 2 s beat. Each answer is written on the FOREXE console (`[Urmărire] întrebare
  FOREXE confirmată de K-BOT …`, Debug level).

### 3. «Adăugare angajamente...» in the menu
- `menuNou` (designer): the first row is now a **folder** «Adăugare angajamente...» with
  «Angajament nou» (the old row, same key `angajament_nou`, same action) and the new
  **«Creează angajament în FOREXE»** (key `angajament_forexe`).
- The new row (`KbotForm.CreeazaAngajamentInForexe`, `KbotForm.Browser.vb`):
  1. robot busy → a message, nothing else;
  2. no FOREXE session → `ForexeController.ConnectAsync()` (the same as the band's «Conectare»);
     still none → a message;
  3. the tree selection is cleared (`_currentInfo = Nothing`, `tree.SelectedNode = Nothing`, the
     view gate on «no node») — first, so the view does not send the robot after the old node;
  4. the «Browser FOREXE» view is selected;
  5. `BrowserView.CereAngajamentNouAsync` → `ForexeController.DeschideAngajamentNouAsync` runs the
     new **`Workflows\adlop - Angajament Nou.wfl`**: reset, Home through the logo, the «Angajament
     nou» link, wait for `textarea[name='descriere']`, stop. Every step is copied from sections 1
     and 2 of «Creare\adlop - Creare Angajament.wfl». A request made while the browser is still
     docking waits for that dock (`_nouCerut`) and is dropped if the dock fails.
- `ForexeWatch.js` section 18: the robot's click on «Angajament nou» is not watched (the watcher
  sleeps during a job), so a page handed back on the empty form **starts the «Angajament nou»
  operation** as if the operator had pressed the link. Without it the save that follows would never
  be reported and K-BOT would not download the new angajament.

### 4. The in-page mini menu follows a setting
- `AppSettings.ForexeShowPageMenu` (default **True** = as before) → `ForexeWatchConfig` sends
  `menu` → `ForexeWatch.js` `config.menu` (`syncMenu`, one place for the menu's visibility).
- **Setări › FOREXE › Browserul**: «Arată mini-meniul K-BOT în pagina FOREXE (mărire /
  micșorare, starea urmăririi)» (`chkMeniuPagina`), saved at once and pushed into the open page
  like the developer-tools switch.
- Hidden, nothing else changes: the watcher, the markers, the pictures, the guards all work. The
  zoom last chosen stays applied (its buttons are in the menu).

### 5. The login window has no «?»
- `LoginForm.Designer.vb`: `capBar.ShowHelpButton = False`. F1 still opens the login topic.

### 6. Start maximized
- `AppSettings.StartMaximized` (default **False** = as before). `KbotForm.OnLoad`, after the base
  (theme, fit, centring on the normal size): `WindowState = Maximized`, so «Restore» gives back the
  designed window. **Setări › Aplicație › Generale**: «Fereastra principală pornește mărită (pe
  tot ecranul)» (`chkStartMaximized`); applies from the next start.
- Only the main window; the director's window is untouched.

### 7. A second reception on the same date
- `ForexeWatch.js` section 17. When «Adaugă» starts a new reception, the dates of the receptions
  already in the list are read **from the page** (`receptionDates`: the table
  `table.table-striped.table-bordered.table-condensed`, column «Data» — the one «Receptii
  Angajament.wfl» scrapes) and kept with the operation (`state.recDates`, in sessionStorage, so a
  navigation to the form keeps them).
- When the date in the form is one of them, K-BOT asks, in the same box as the
  save-without-changes message (now with two buttons): «Angajamentul are deja o recepție cu data
  …» / «NU este recomandat să aveți mai multe recepții pe aceeași dată.» / «Continuați cu această
  dată?» — «Nu, schimb data» (default; Esc and Enter) puts the cursor back in the date field,
  «Da, continui» remembers the answer for that date (`state.recAck`).
- Asked twice at most: 250 ms after the date field is left / its value committed (`focusout`,
  `change`), and in any case at the «Salvează» click, which is held and replayed on «Da» (after the
  form guard, before the marker). A date already agreed to is not asked again.
- New receptions only; editing an existing reception is not checked.

## Files touched

- `src/KBot.Common/AppSettings.vb` (three settings), `KBot.Common.vbproj` (FileVersion 1.5.9.0)
- `src/KBot.Forexe/Services/JavaScripts/ForexeWatch.js` (sections 15–18, `configFrom`, `syncMenu`,
  the two-button blocking box)
- `src/KBot.Forexe/Models/ForexeWatchConfig.vb`, `WorkflowCatalog.vb`, `JobBuilder.vb`,
  `KBot.Forexe.vbproj` (the new .wfl copied to output; FileVersion 1.0.19.0)
- **new** `src/KBot.Forexe/Workflows/adlop - Angajament Nou.wfl`
- `src/KBot.App/Forexe/ForexeController.vb` (`DeschideAngajamentNouAsync`)
- `src/KBot.App/Views/BrowserView.vb` (`CereAngajamentNouAsync`, `PornesteAngajamentNou`)
- `src/KBot.App/KbotForm.vb` (`OnLoad`, `PornesteTurulInitial`), `KbotForm.Browser.vb`,
  `KbotForm.Nomenclatoare.vb`, `KbotForm.CabNotes.vb` (menu button tooltip),
  `KbotForm.Designer.vb` (the folder; tooltip)
- `src/KBot.App/Help/HelpService.vb`, `HelpTourRunner.vb`, `HelpTourBubble.vb`,
  `HelpTourBubble.Designer.vb`
- `src/KBot.App/Setari/SetariAplicatieView.vb` + `.Designer.vb`, `SetariForexeView.vb` +
  `.Designer.vb`
- `src/KBot.App/LoginForm.Designer.vb`
- Help: see `SLICE-0000-25-ajutor-pentru-0097-02.md`

## Test results

- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**.
- `node --check ForexeWatch.js`: syntax OK.
- `tools\HelpCheck\Check-Help.ps1 -Coverage`: «No errors.» (coverage still names
  `RobotQueueForm`, from 0098 — already in the 0000 open threads).
- **No tests run, no test code written, nothing run on screen** (operator rule).

## Left unverified or deferred

- **Nothing was run.** The tour at start, the box on the bubble, the menu folder, the two Setări
  pages, the maximized start and the login bar were not seen on screen.
- **FOREXE's confirmation windows were not seen** (point 2). The rule is written from what the
  repository already says about them (bootstrap `.modal`, a green «Da» in the footer — the
  «Renunțare» question in `ForexeWatch.js`, «confirmare modală» in the workflows). If a real
  window is worded without «sigur», has no «no» button, or is not a `.modal`, K-BOT leaves it for
  the operator. To be checked on a client PC: the console line above appears for each window
  answered.
- **The same-date question** (point 7): the date field's behaviour with FOREXE's date picker was
  not seen; the check at «Salvează» is the certain one, the one after leaving the field depends on
  the events the picker raises. A list of receptions longer than one page (if FOREXE pages it) is
  read only as far as the page shows.
- **«Angajament Nou.wfl» was never run.** Its steps are copies of steps that run today in
  «Creare Angajament», but the flow itself is new.
- Point 3 does not go through the robot queue (like the other direct callers of the «Browser
  FOREXE» view — see the 0098 open threads): with a queued job running it answers «Rulează deja o
  operație FOREXE».
- The automatic tour starts after the tree is loaded; a message box that K-BOT opens at the same
  moment (signed PDFs left to upload) is not coordinated with the top-most bubble.
- The floating menu hidden = no way to change the page zoom from the page until it is shown again.
- `KBot.App` FileVersion not bumped (done at `push-update.ps1`).
