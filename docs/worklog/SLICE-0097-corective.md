# SLICE-0097 — Corecturi desktop, autentificare și perioade

Istoric 30.09–25.10.2026. 0097-04 înlocuiește condiția inițială de vizibilitate a bifei pentru parolă; 0097-05 înlocuiește forțarea reautentificării și limitarea intervalului fără avansate. 0097-06 înlocuiește ascunderea selectorului Sursă/Sector după sortarea pe dată din 0097-03. Corpurile de mai jos sunt checkpointuri datate, nu reguli concurente. Nota istorică de captură din 0097-03 nu înlocuiește regula actuală AGENTS pentru mascarea datelor.

[0097 — prima trecere](#slice-0097) · [0097-02](#slice-0097-02) · [0097-03](#slice-0097-03) · [0097-04](#slice-0097-04) · [0097-05](#slice-0097-05) · [0097-06](#slice-0097-06)

## SLICE-0097 — Corective updates (operator request, 30.09.2026) <a id="slice-0097"></a>

Nine operator requests in one message. Answers given by the operator before the code (30.09.2026): a signed DDF revision keeps only «Trimite / Reia trimiterea» in its menu; the new tree roots are in ORD **and** DDF; a month / «toate» delete is **refused** (not offered) when any document under it is signed; the re-login interval acts **at session expiry**, not on a timer.

### What changed and why

#### ORD / DDF views
1. **No message after a delete** (ORD single / ORD group / DDF revision / document / month / «toate»). The counts that the box showed go to `mesaje_operator.log` only (`OperatorLog`). Errors are still shown.
2. **Root node** in both trees: «Toate ordonanțările» (`OrdView`) and «Toate reviziile» (`DdfView`), key `all`, months under it — the same shape as «Tot istoricul» (0095). Clicking it = what the view shows on load (every line).
3. **Delete on the month and on the root.**
   - ORD: «Șterge TOATE ordonanțările lunii» / «Șterge TOATE ordonanțările» → new `OrdActiune.StergeGrup`; the shell (`StergeOrdonantarileAsync`) confirms once, then one `DELETE /api/forexe/ord/{idordp}` per ordonanțare (same call as the single delete), stopping at the first failure and saying how many went. No new server route.
   - DDF: the month entry existed; the root adds «Șterge documentul (TOATE reviziile)» → `DdfActiune.StergeToate` → the existing `DeleteDdfAsync(iddf)`.
   - Both offered only when **no** document under the node is signed; the shell re-checks.
4. **Signed = at least one signature**: `Semnatura` non-empty OR a signed PDF on the server (`ArePdfSemnat`) — `OrdView.EsteSemnata` / `DdfView.EsteSemnata`.
   - ORD leaf signed → **no context menu at all** (so «Adaugă ordonanțare» and «Generare în lot» disappear with it).
   - DDF leaf signed → no «Modifică / Șterge revizia / Șterge documentul»; «Trimite / Reia trimiterea» stays when the state allows it; no entry left → no menu.

#### KbotForm
5. **Collapsed nav flyout too narrow outside 150%** (`KBotNavList`, `KBotNavFlyout`): width previously matched caption measured on screen DC with different draw flags. It now uses the window DC, actual draw flags, wider regular/semibold font and a scaled gap. Flyout ignores `WM_DPICHANGED` suggested resizing across monitors. **Hypothesis, unproven cause:** no reproduction at another scale.
6. **«Note corecție» only when the branch has notes**, like ORD/DDF. Server: `GET /api/forexe/tree` returns `AreNoteCab` = `EXISTS FX_NoteCAB_Corectii.CodAngajament`, or a literal 0 when that table does not exist on the unit database (checked in `information_schema` per request — `000_DEMO` does not have it). Client: `AngajamentTreeInfo .AreNoteCab`, gating in `ApplyViewGating` / `IsViewEnabled`; a note saved on the selected angajament raises the flag locally (`AprindePoartaNotelor`).

#### Caption bar — unit selector
7. `KBotCaptionBar` gains a painted non-editable drop-down after the title (`KBotCaptionBar.UnitSelector.vb`: `SetSelectorItems`, `ClearSelector`, `SelectorSelectedKey`, event `SelectorChanged`). Shown only with two or more choices; otherwise the bar is unchanged. The list is a `CustomPopup` with a check on the current unit.
   - Server (gitignored `routes/auth/auth.py`): `GET /api/auth/my-units` (the operator's SELECT on `Unitati_Utilizatori ⋈ Unitati`, filtered by the session's UN) and `POST /api/auth/switch-unit` (bearer; same access check and answer as `/login`, new token, old token revoked, journal action `SWITCH_UNIT`). No password needed: the live token proves identity.
   - Client: `IAuthApi.GetMyUnitsAsync` / `SwitchUnitAsync`; `UnitInfo` gets `CF`, `Rol`; `KbotForm.Units.vb` loads the list after the tree, and on a pick: switch → session → the unit remembered (`LastLoginStore`) → combos, tree and views reset → periods + tree + «operațiuni necorelate» mark, as after login.
   - **Unit switch first closes FOREXE silently** (operator, 30.09.2026), preventing downloads into the wrong session database. Separate `IForexeDisconnect` leaves `IForexeRunner` doubles unchanged; `ForexeRunner.DisconnectAsync` uses the reconnect teardown, via `ForexeController.DisconnectAsync(forgetCertificate)`. `CertificateService.LastUsedCertificate` is forgotten only when `AppSettings.ForexeForgetCertificateOnUnitSwitch` is checked («Setări → FOREXE → Uită certificatul memorat când schimb unitatea din bara de titlu», default off, operator 30.09). Switch is refused while FOREXE is busy or when the target role is Director (signing window).

#### Setări → Autentificare + login window
8. New section «În timpul lucrului (sesiunea expirată)»:
   - «Reafișează fereastra de autentificare când expiră sesiunea» (`ReloginPrompt`) — changeable only with the advanced options; without them it is always on.
   - «Reafișează cel mult o dată la» (`ReloginMinutes`): 10, 15, 20, 30, 45, 60 minutes; with the advanced options also 5, 90, 120, 240, 480. Without them the value in effect is clamped to 10..60.
   - At startup the login window is always shown (unchanged).
   - `WithReauth` calls `KbotForm.ReautentificaAsync` on 401. With prompt off or a typed login younger than the interval, it silently calls `/api/auth/login` on the SAME unit with the in-memory password. Otherwise, or on failure, it shows the window. Semaphore serializes silent re-login; server refusal clears the password.
9. Login window: «Ține minte parola până la repornirea calculatorului» under the password. Visible only with the advanced options AND the new switch «Arată în fereastra de autentificare bifa…» (`RememberPasswordOption`, default on, shown only with the advanced options) plus «Uită parola memorată». Storage (`KBot.Common/SessionCredentials.vb`): DPAPI (current Windows user, via crypt32 P/Invoke — no package) in a **volatile** HKCU key `Software\AVACONT\KBot\Session`, which Windows discards at sign-out / restart. Unticked at login → the stored pair is erased. The in-process copy for item 8 is DPAPI-encrypted too.

### Files touched

- `src/KBot.App/Views/OrdView.vb`, `Views/Ord/OrdComanda.vb`, `KbotForm.Ord.vb`
- `src/KBot.App/Views/DdfView.vb`, `Views/Ddf/DdfComanda.vb`, `KbotForm.Ddf.vb`, `KbotForm.DdfDelete.vb`
- `src/KBot.Controls/NavList/KBotNavList.vb`, `NavList/KBotNavFlyout.vb`
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.vb`, **new** `CaptionBar/KBotCaptionBar.UnitSelector.vb`
- `src/KBot.App/KbotForm.vb`, `KbotForm.Views.vb`, `KbotForm.CabNotes.vb`, **new** `KbotForm.Units.vb`
- `src/KBot.App/LoginForm.vb`, `LoginForm.Designer.vb`
- `src/KBot.App/Setari/SetariAutentificareView.vb`, `SetariAutentificareView.Designer.vb`
- `src/KBot.Common/AppSettings.vb`, **new** `KBot.Common/SessionCredentials.vb`
- `src/KBot.Api/IAuthApi.vb`, `AuthApi.vb`, `ApiClient.vb`, `UpsertAngajamenteRequest.vb`
- `src/KBot.Domain/AngajamentTreeInfo.vb`, `Auth/UnitInfo.vb`
- `src/KBot.Forexe/ForexeRunner.vb`, **new** `KBot.Forexe/IForexeDisconnect.vb`, `src/KBot.App/Forexe/ForexeController.vb`, `src/KBot.App/Setari/SetariForexeView.vb`, `SetariForexeView.Designer.vb`
- `PYTHON/routes/forexe/tree.py`; `PYTHON/routes/auth/auth.py` (**gitignored — deploy by hand**)

### Test results

- `dotnet build src/KBot.App/KBot.App.vbproj --no-incremental`: **0 errors, 0 warnings**. `KBot.DevHarness`: 0 errors.
- Python: syntax check (`ast.parse`) of `tree.py` and `auth.py` only.
- **No tests run, no test code written** (operator rule). Nothing seen on screen.

### Left unverified or deferred

- Nothing run, nothing on screen: menus, roots, caption selector, settings section, login box.
- **Flyout**: the fix removes two plausible causes; the real one was not reproduced.
- **Server not deployed**: `tree.py` (AreNoteCab) and `auth.py` (my-units / switch-unit) must be copied to the VPS. Until then: the notes view never shows (the flag is missing → false) and the unit list call fails silently (the title stays the plain unit name).
- `PYTHON/tests/test_forexe_tree.py` fakes the cursor with the old 22 columns and one `execute`; it will need the extra `information_schema` call and the 23rd column (not touched: no tests).
- ORD group delete leaves the «ord» nav entry on after every ordonanțare is gone (same as the single delete: corrected on the next tree reload).
- Unit switch: open secondary windows (Nomenclatoare, Extrase de cont, …) are not closed; they keep the previous unit's data until reopened. The FOREXE disconnect on a switch was never run: whether the docked «Browser FOREXE» view empties cleanly is unverified.
- The re-login window still lets the operator pick another unit (existing behaviour); the caption follows the session afterwards.
- FileVersion of KBot.App / KBot.Controls / KBot.Common / KBot.Api / KBot.Domain not bumped (`push-update.ps1` asks).
- **Help to update** (0000): `contabil.vederi.ord` (root, group delete, menu gone on signed), `contabil.ddf` / `contabil.ddf.semnare` (root, «toate», signed = no edit/delete), `contabil.fereastra` (unit selector in the title; «Note corecție» only with notes), `contabil.notecab`, `contabil.autentificare` (remember-password box; silent re-login), `contabil.setari` (the new Autentificare section); tours `tur-ord`, `tur-ddf`, `tur-setari`.

---

## SLICE-0097-02 — Corective updates, second pass (operator request, 01.10.2026) <a id="slice-0097-02"></a>

Seven operator requests (six in one message, the seventh added while the work was running). Answers given by the operator before the code (01.10.2026): point 5 means the login window must **not** have the «?» button (F1 stays); the work is recorded as **0097-02** (not a new slice). The help for it is `SLICE-0000-25`.

### What changed and why

#### 1. The main window's tour starts by itself
- `AppSettings.ShowInitialTour` (default **True**). After `KbotForm` has loaded its data (`MainForm_Load`, last step, posted with `BeginInvoke`) it calls `HelpService.StartInitialTour()`: the tour `tur-fereastra` (`HelpService.InitialTourId`) runs when the setting is on and the login reads the tour's part (a director never gets it; the DevHarness window without a session never gets it).
- It runs **at every start** until:
  1. it was **seen to the end** («Gata» on the last step, or the last steps skipped because their parts are not on screen) — counted however the tour was started (by itself or from «?»);
  2. the operator ticks **«Nu mai arăta turul inițial»** and closes the tour. Both write `ShowInitialTour = False` (`HelpService.InitialTourSeen`, called by `HelpTourRunner.Finish`). Closing without the box leaves it on.
- **Placement chosen here:** `HelpTourBubble.chkNuMaiArata` gets a row between text and buttons, counted by `FitToText`, shown only for automatic tours (`ShowNeverAgain`). The 440 px button row had no room.
- The way back: **Setări › Aplicație › Generale** «Arată turul ferestrei principale la pornirea K-BOT» (`chkTurInitial`), which also follows `AppSettings.Changed` (it unticks itself when the tour ends with the settings window open).

#### 2. Confirmation questions in the in-page browser are answered «Da»
- `ForexeWatch.js`, new section 15. While the OPERATOR drives the page (never while the robot does), a FOREXE window that asks is answered by K-BOT: a visible `.modal` whose text contains «sigur», with **nothing to fill in** (no visible `textarea` / `select` / `input` other than hidden / button / submit) and with **both** a yes and a no button in its footer. «Yes» is known by its word (Da / OK / Continuă / Confirmă), else by FOREXE's green button (`btn-success`); «no» by its word (Nu / Renunță / Anulează / Închide / Înapoi), else any other button.
- So: a window that waits for data (the motive of a reservation, the description of a definitivare) is left alone, and so is one that only tells something (a single button).
- One press per opening (`_kbotAnswered` on the element, cleared when the window is hidden). It is looked for on every DOM change (inserted, or only shown through class / style), 150 ms after the burst, and on the 2 s beat. Each answer is written on the FOREXE console (`[Urmărire] întrebare FOREXE confirmată de K-BOT …`, Debug level).

#### 3. «Adăugare angajamente...» in the menu
- `menuNou` (designer): the first row is now a **folder** «Adăugare angajamente...» with «Angajament nou» (the old row, same key `angajament_nou`, same action) and the new **«Creează angajament în FOREXE»** (key `angajament_forexe`).
- The new row (`KbotForm.CreeazaAngajamentInForexe`, `KbotForm.Browser.vb`):
  1. robot busy → a message, nothing else;
  2. no FOREXE session → `ForexeController.ConnectAsync()` (the same as the band's «Conectare»); still none → a message;
  3. the tree selection is cleared (`_currentInfo = Nothing`, `tree.SelectedNode = Nothing`, the view gate on «no node») — first, so the view does not send the robot after the old node;
  4. the «Browser FOREXE» view is selected;
  5. `BrowserView.CereAngajamentNouAsync` → `ForexeController.DeschideAngajamentNouAsync` runs the new **`Workflows\adlop - Angajament Nou.wfl`**: reset, Home through the logo, the «Angajament nou» link, wait for `textarea[name='descriere']`, stop. Every step is copied from sections 1 and 2 of «Creare\adlop - Creare Angajament.wfl». A request made while the browser is still docking waits for that dock (`_nouCerut`) and is dropped if the dock fails.
- `ForexeWatch.js` §18 starts the «Angajament nou» operation when the robot returns the empty form. The watcher sleeps during jobs and misses the robot click; without this, the later save would neither be reported nor download the new angajament.

#### 4. The in-page mini menu follows a setting
- `AppSettings.ForexeShowPageMenu` (default **True** = as before) → `ForexeWatchConfig` sends `menu` → `ForexeWatch.js` `config.menu` (`syncMenu`, one place for the menu's visibility).
- **Setări › FOREXE › Browserul**: «Arată mini-meniul K-BOT în pagina FOREXE (mărire / micșorare, starea urmăririi)» (`chkMeniuPagina`), saved at once and pushed into the open page like the developer-tools switch.
- Hidden, nothing else changes: the watcher, the markers, the pictures, the guards all work. The zoom last chosen stays applied (its buttons are in the menu).

#### 5. The login window has no «?»
- `LoginForm.Designer.vb`: `capBar.ShowHelpButton = False`. F1 still opens the login topic.

#### 6. Start maximized
- `AppSettings.StartMaximized` (default **False** = as before). `KbotForm.OnLoad`, after the base (theme, fit, centring on the normal size): `WindowState = Maximized`, so «Restore» gives back the designed window. **Setări › Aplicație › Generale**: «Fereastra principală pornește mărită (pe tot ecranul)» (`chkStartMaximized`); applies from the next start.
- Only the main window; the director's window is untouched.

#### 7. A second reception on the same date
- `ForexeWatch.js` §17: Adaugă recepție reads existing dates from the PAGE (`receptionDates`, Data column of `table.table-striped.table-bordered.table-condensed`, as Receptii Angajament.wfl) into `state.recDates`/sessionStorage, surviving navigation to the form.
- Duplicate date: the save-without-changes box now has two buttons and asks «Angajamentul are deja o recepție cu data …» / «NU este recomandat să aveți mai multe recepții pe aceeași dată.» / «Continuați cu această dată?». Default «Nu, schimb data» (Esc/Enter) refocuses the date; «Da, continui» stores that date in `state.recAck`.
- Asked at most twice: 250ms after focusout/change and at Salvează, held/replayed on Da after the form guard and before the marker. An acknowledged date is not asked again.
- New receptions only; editing an existing reception is not checked.

### Files touched

- `src/KBot.Common/AppSettings.vb` (three settings), `KBot.Common.vbproj` (FileVersion 1.5.9.0)
- `src/KBot.Forexe/Services/JavaScripts/ForexeWatch.js` (sections 15–18, `configFrom`, `syncMenu`, the two-button blocking box)
- `src/KBot.Forexe/Models/ForexeWatchConfig.vb`, `WorkflowCatalog.vb`, `JobBuilder.vb`, `KBot.Forexe.vbproj` (the new .wfl copied to output; FileVersion 1.0.19.0)
- **new** `src/KBot.Forexe/Workflows/adlop - Angajament Nou.wfl`
- `src/KBot.App/Forexe/ForexeController.vb` (`DeschideAngajamentNouAsync`)
- `src/KBot.App/Views/BrowserView.vb` (`CereAngajamentNouAsync`, `PornesteAngajamentNou`)
- `src/KBot.App/KbotForm.vb` (`OnLoad`, `PornesteTurulInitial`), `KbotForm.Browser.vb`, `KbotForm.Nomenclatoare.vb`, `KbotForm.CabNotes.vb` (menu button tooltip), `KbotForm.Designer.vb` (the folder; tooltip)
- `src/KBot.App/Help/HelpService.vb`, `HelpTourRunner.vb`, `HelpTourBubble.vb`, `HelpTourBubble.Designer.vb`
- `src/KBot.App/Setari/SetariAplicatieView.vb` + `.Designer.vb`, `SetariForexeView.vb` + `.Designer.vb`
- `src/KBot.App/LoginForm.Designer.vb`
- Help: see `SLICE-0000-25-ajutor-pentru-0097-02.md`

### Test results

- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**.
- `node --check ForexeWatch.js`: syntax OK.
- `tools\HelpCheck\Check-Help.ps1 -Coverage`: «No errors.» (coverage still names `RobotQueueForm`, from 0098 — already in the 0000 open threads).
- **No tests run, no test code written, nothing run on screen** (operator rule).

### Left unverified or deferred

- **Nothing was run.** The tour at start, the box on the bubble, the menu folder, the two Setări pages, the maximized start and the login bar were not seen on screen.
- **FOREXE's confirmation windows were not seen** (point 2). The rule is written from what the repository already says about them (bootstrap `.modal`, a green «Da» in the footer — the «Renunțare» question in `ForexeWatch.js`, «confirmare modală» in the workflows). If a real window is worded without «sigur», has no «no» button, or is not a `.modal`, K-BOT leaves it for the operator. To be checked on a client PC: the console line above appears for each window answered.
- **The same-date question** (point 7): the date field's behaviour with FOREXE's date picker was not seen; the check at «Salvează» is the certain one, the one after leaving the field depends on the events the picker raises. A list of receptions longer than one page (if FOREXE pages it) is read only as far as the page shows.
- **«Angajament Nou.wfl» was never run.** Its steps are copies of steps that run today in «Creare Angajament», but the flow itself is new.
- Point 3 does not go through the robot queue (like the other direct callers of the «Browser FOREXE» view — see the 0098 open threads): with a queued job running it answers «Rulează deja o operație FOREXE».
- The automatic tour starts after the tree is loaded; a message box that K-BOT opens at the same moment (signed PDFs left to upload) is not coordinated with the top-most bubble.
- The floating menu hidden = no way to change the page zoom from the page until it is shown again.
- `KBot.App` FileVersion not bumped (done at `push-update.ps1`).

---

## SLICE-0097-03 — Year and Sector selectors move into the caption bar (operator request, 01.10.2026) <a id="slice-0097-03"></a>

Follow-up to 0097 (the unit selector in the caption bar). Help recorded as 0000-33.

### What changed and why

The operator asked for three things:
1. the **year** selector («An Date») moves to the caption bar, next to the unit selector;
2. the **source / sector** selector («Sursă/Sector», the old `cboSs`) moves there too;
3. both are **removed from KbotForm** (the header band under the caption).

- `KBotCaptionBar` paints three named selectors after the title: SelectorUnit (0097, unchanged, ≥2 units), SelectorYear/SelectorSector (≥1 choice). Layout: «K-BOT — [Unit ▾]   An Date [2026 ▾]   Sursă/Sector [02A ▾]»; year/sector have dim labels before their boxes. Selectors that do not fit are omitted.
  - API: SetSelectorItems(selector, items, key), ClearSelector(selector), GetSelectorKey, SetSelectorKey (no event), SetSelectorShown (host hides sector for date-sorted tree). Unnamed unit overloads remain compatible.
  - `SelectorChanged` now carries `Selector` + `Key` (`CaptionSelectorChangedEventArgs(selector, key)`).
  - Help parts: `year`, `ss` added next to `unit` (`HelpPartBounds`); only the unit's text is blurred in captures (the year / sector are not sensitive).
  - `KBotCaptionBar.UnitSelector.vb` is replaced by `KBotCaptionBar.Selectors.vb` (the code of the old file, generalised from one selector to a small `TitleSelector` per name).
- `KbotForm` removes cboAn/cboSs/lblAn/lblSs from Designer; tlyHeader keeps MENIU/operator label columns. KbotForm.Periods.vb uses the bar through AnulAles()/SsAles(); CapBar_PeriodChanged on capBar.SelectorChanged replaces SelectedIndexChanged handlers, while the unit handler ignores year/sector. No `_suppressPeriodEvents`: setting keys emits no event. KbotForm.Tree/TreeOptions/Units/Chrome/vb follow.
- **Preserved:** highest year first, LastSS; year rebuilds SS/reloads tree; SS persists server-side/reloads tree; unit clears/reloads both selectors; date sorting hides sector and requests all sources.
- **Changed:** unreadable/absent periods omit selectors instead of disabled combos. Painted selectors have labels instead of the former An/Sursă / sector tooltips.

### Files touched

`src/KBot.Controls/CaptionBar/KBotCaptionBar.Selectors.vb` (**new**, replaces `KBotCaptionBar.UnitSelector.vb`), `KBotCaptionBar.vb`, `KBotCaptionBar.HelpParts.vb`; `src/KBot.App/KbotForm.Designer.vb`, `KbotForm.vb`, `KbotForm.Periods.vb`, `KbotForm.Tree.vb`, `KbotForm.TreeOptions.vb`, `KbotForm.Units.vb`, `KbotForm.Chrome.vb`; Help (0000-33): `HelpContent/tours/tur-fereastra.md`, `contabil/fereastra.md`, `contabil/autentificare.md`; `tools/HelpCheck/Check-Help.ps1` (caption bar parts list)

### Test results

- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 errors, 0 warnings** (incl. KBot.Controls, KBot.DevHarness).
- `Check-Help.ps1 -Coverage`: the only error left is `contabil\fereastra.md: slice '0077-3' is not in KBOT_STATUS.md`, in a tag this task did not touch.
- **No tests run, no test code written** (operator rule). Nothing seen on screen.

### Left unverified or deferred

- Nothing run, nothing on screen: the three selectors side by side, the label spacing, the cut with «…» on a narrow window, the popup under each box, the DPI scaling of the labels.
- The caption bar's tour steps «Anul de lucru» / «Sursa și sectorul» now point at the bar parts (`year`, `ss`); the old steps targeting `cboAn` / `cboSs` are gone.
- Pictures: `fereastra-zone` shows the old header band — to re-shoot («Capturi de refăcut»).
- `KBotCaptionBar.md` (control doc) not updated with the selectors API (the unit selector was not in it either).
- FileVersion of KBot.App / KBot.Controls not bumped (`push-update.ps1` asks).

---

## SLICE-0097-04 — «Ține minte parola»: bifa nu mai depinde de opțiunile avansate <a id="slice-0097-04"></a>

Cererea operatorului, 19.10.2026. (Numărul 0097-04 e ales de mine, urmând după 0097-03; de confirmat.)

### What changed and why

Pe unele calculatoare fereastra de autentificare nu arăta bifa «Ține minte parola până la repornirea calculatorului», deși în Setări era bifată. Cauza (din `app_settings.json` de pe un astfel de PC): `"AdvancedOptions": false` — `AppSettings.RememberPasswordInEffect` cerea opțiunile avansate pornite, iar bifa din Setări era ascunsă fără ele.

Regula nouă (operator): starea bifei rămâne cea stabilită, indiferent de opțiunile avansate; **doar schimbarea ei** cere opțiunile avansate.

- `AppSettings.RememberPasswordInEffect` = `RememberPasswordOption` (implicit `True`).
- Setări › Autentificare: bifa «Arată în fereastra de autentificare bifa …» e mereu vizibilă, dezactivată (`Enabled = False`) fără opțiunile avansate. «Uită parola memorată» rămâne doar cu avansate.
- Ajutor: `contabil/autentificare.md`, `contabil/setari.md` (tag `0097-04`).

### Files touched

`src/KBot.Common/AppSettings.vb`; `src/KBot.App/LoginForm.vb` (doar comentariul); `src/KBot.App/Setari/SetariAutentificareView.vb`; `src/KBot.App/HelpContent/contabil/autentificare.md`, `setari.md`

### Test results

`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Netestat pe ecran.

### Unverified / deferred

- Neprobat pe un PC cu problema. Operatorul raportase că bifa din Setări „nu rămâne bifată”; cu avansatele oprite bifa din Setări era ascunsă, deci nu e clar cum se vedea — de reverificat după deploy.
- Cine a oprit `AdvancedOptions` pe acel PC nu se știe din cod (nimic nu o resetează singur).
- `Check-Help.ps1` nerulat.
- Numărul 0097-04 și rândurile de STATUS: de confirmat.

---

## SLICE-0097-05 — Opțiunile avansate doar permit schimbarea, nu blochează aplicarea <a id="slice-0097-05"></a>

Cererea operatorului, 23.10.2026. Numărul 0097-05 a fost confirmat de operator.

### What changed and why

Operatorul a observat că bifa «Reafișează fereastra de autentificare când expiră sesiunea» se întoarce pe «bifat» (credea că la fiecare update). Cauza din cod: `AppSettings.ReloginPromptInEffect` întorcea `ReloginPrompt OrElse Not AdvancedOptions`, deci cu opțiunile avansate oprite bifa era forțată pe «bifat», oricum fusese salvată. Fișierul de setări (`%APPDATA%\AVACONT\KBot`) nu e atins de update; nu există cod care să reseteze valoarea.

Regula (operator, generalizată): **`AdvancedOptions` doar permite SCHIMBAREA unei setări; nu oprește niciodată aplicarea uneia salvate.** Aplicată peste tot unde o setare era blocată de avansate:

- `AppSettings.ReloginPromptInEffect` = `ReloginPrompt`.
- `AppSettings.ReloginMinutesInEffect` = `Max(1, ReloginMinutes)` (fără limitarea 10..60 fără avansate). În Setări › Autentificare, lista de intervale conține mereu valoarea în vigoare (înainte afișa cea mai apropiată din listă, deci un 120 salvat se vedea ca 60).
- `KbotForm.HelpCaptureModeOn` = `AppSettings.Current.HelpCaptureMode` (înainte cerea și avansate).
- Bifa din Setări rămâne dezactivată fără avansate (schimbarea cere avansate), dar afișează valoarea salvată.
- Ajutor: `contabil/setari.md` (tag `0097-05`).

Lăsate neschimbate, pentru că nu sunt setări aplicate, ci unelte/conținut vizibile doar cu avansate: jurnalele de pe server (`SetariJurnalView`), secțiunea «Istoric (avansat)» (`SetariForexeView`), partea «Opțiuni avansate» din ajutor (`HelpService`), paginile avansate din `SetariForm`.

### Files touched

`src/KBot.Common/AppSettings.vb`; `src/KBot.App/KbotForm.HelpCapture.vb`; `src/KBot.App/Setari/SetariAutentificareView.vb`; `src/KBot.App/HelpContent/contabil/setari.md`

### Test results

`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Netestat, nevăzut pe ecran.

### Unverified / deferred

- Nerulat pe ecran; nu s-a verificat cu un `app_settings.json` real cu `ReloginPrompt: false` și avansate oprite.
- Nu s-a găsit niciun cod care să reseteze bifa la update; ipoteza „reset la update" a operatorului rămâne neconfirmată (efectul observat se explică prin forțarea de mai sus). Cine oprește `AdvancedOptions` pe un PC nu se știe din cod.
- `ReloginMinutesMin` / `ReloginMinutesMax` au rămas în `AppSettings` neîntrebuințate.
- `Check-Help.ps1` nerulat.
- Ce rămâne vizibil doar cu avansate (jurnale server, istoric, partea avansată din ajutor) a fost lăsat așa; de confirmat cu operatorul dacă vrea și acestea mereu vizibile.

---

## SLICE-0097-06 — Sursa «Toate» în bara de titlu și mesaj de perioadă pe tot aplicația <a id="slice-0097-06"></a>

Cererea operatorului, 25.10.2026. Slice-ul 0097-03 a mutat anul și sursa/sectorul în bara de titlu; numărul 0097-06 este ultimul sub-slice liber al lui 0097 (cerut de operator).

### What changed and why

Până acum selectorul Sursă/Sector era legat de arborele din fereastra principală: la sortarea după dată se ascundea, iar arborele aducea toate sursele. Operatorul vrea altfel (mai multe machete vor răspunde la sursă, nu doar fereastra principală):

- **Alegerea «Toate»** (cheia `*`, aceeași pe care o trimite cererea de arbore pentru toate sursele) este primul rând al selectorului.
- **Mesaj pe tot aplicația:** `SessionContext.PeriodChanged` (cu `PeriodChangedEventArgs`: an, sursă, program, «Toate»). Se ridică din `SetPeriod` doar când ceva s-a schimbat, pe firul UI. Orice machetă se abonează la el.
- **Sesiunea păstrează mereu o sursă și un program concrete** (robotul FOREXE și documentele au nevoie de ele); `SessionContext.ToateSursele` spune doar că lista nu e filtrată. «Toate» nu se salvează ca `LastSS`.
- **Arborele:** selectorul nu se mai ascunde la sortare (doar cât e aleasă o grupă). La fiecare schimbare a sortării (după nume sau după dată) selectorul trece pe «Toate» și arborele se reîncarcă. O sursă aleasă după aceea aduce doar angajamentele ei, în ordinea sortării.
- **Descărcarea listei din FOREXE** (`ForexeController`) întreabă `ToateSursele`, nu sortarea după dată.
- **Schimbarea anului:** «Toate» rămâne «Toate»; la prima citire, «Toate» este alegerea când sortarea e după dată.
- Parametrii noi sau atinși din `SessionContext.SetPeriod` / `PeriodChangedEventArgs` au prefix `k_`.
- Ajutor: `contabil/fereastra.md` (secțiunile «Lista angajamentelor» și «Anul și sursa/sectorul»), `tours/tur-fereastra.md` (pasul «Sursa și sectorul»), `contabil/forexe/lista.md` — toate cu tag `0097-06`.

Interpretare (de confirmat): „la fel și la ordonare nume" = și la trecerea pe sortarea după nume selectorul trece pe «Toate».

### Files touched

`src/KBot.Common/SessionContext.vb`; `src/KBot.App/KbotForm.Periods.vb`; `src/KBot.App/KbotForm.Tree.vb`; `src/KBot.App/KbotForm.TreeOptions.vb`; `src/KBot.App/Forexe/ForexeController.vb`; `src/KBot.App/HelpContent/contabil/fereastra.md`, `tours/tur-fereastra.md`, `contabil/forexe/lista.md`

### Test results

`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Netestat, nevăzut pe ecran.

### Unverified / deferred

- Nerulat pe ecran: nu s-a văzut «Toate» în selector, trecerea pe «Toate» la sortare, nici filtrarea pe o sursă.
- Nicio altă machetă nu e abonată încă la `PeriodChanged`; evenimentul e pregătit.
- Starea de pe server a `LastSS` nu se schimbă la «Toate» (rămâne ultima sursă concretă).
- Rândul grupei active: cât e aleasă o grupă selectorul rămâne ascuns, ca înainte.
- `Check-Help.ps1` nerulat; nu s-au scris teste (regula proiectului).
- Nu s-a făcut nicio operațiune git (regula proiectului).

## Repere suplimentare din registru

Proba etichetei barei strânse era cerută la 100% / 125%; cauza rămâne ipoteză. Dacă o confirmare FOREXE vizată de 0097-02 rămâne pe ecran, regula trebuie lărgită și explicația din ajutor restrânsă la ce poate face; ferestrele reale nu au fost văzute.
