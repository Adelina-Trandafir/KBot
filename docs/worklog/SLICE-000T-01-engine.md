# SLICE-000T-01…05 — Istoricul motorului și autorării tutorialelor

Consolidare documentară D002-06. Motorul, fluxul de 18 pași, designerul, recorderul și corecțiile bulei sunt etape ale aceluiași plan inițial. Corpurile, inventarele, probele și limitele rămân pe etapă; formulările despre funcții încă neimplementate sunt checkpointuri istorice. Regula inițială de privire înainte este restrânsă în 05; declanșarea la prima literă este corectată ulterior în același 05. Nimic nu certifică starea actuală sau autorizează execuții.

Registrul consemna lipsa schimbărilor de server/SQL și precizarea de proiectare din 03: în loc de HelpPartAt pe interfață, selectorul folosește HelpPartBounds cu numele verificatorului, RightIconRectsOnScreen și ItemKeyAt. Excepția istorică de testare a lui 000T nu se extinde la curățarea documentară. Coliziunea 000T-05 (ajutorul 0000-50 / corecția bulei) rămâne în registru; aici se află corecția.

[01](#slice-000t-01) · [02](#slice-000t-02) · [03](#slice-000t-03) · [04](#slice-000t-04) · [05 — corecții](#slice-000t-05)

<a id="slice-000t-01"></a>

## SLICE-000T-01 — interactive tutorials: the engine

Operator request (04.10.2026): game-style tutorials that point at the next thing, wait for the user to do it (never do it for them), cross windows (main tree -> Rezervari tab -> «+» -> DdfEditForm -> its tabs -> Salveaza), have optional steps with «Sari peste» and a reason, ask «Vrei sa iesi din tutorial?» when the user does anything else, and can be authored visually. Plan: `~/.claude/plans/i-need-a-tutorial-humble-pretzel.md`. This sub-slice is the engine only (000T-02..05 follow).

### What changed and why

- `src/KBot.Theming/KBotTutorial.vb` — `IKBotTutorialHost` (`TutorialSupports`, `TutorialBegin`, `TutorialEnd`, `TutorialAnchor`, event `TutorialSignal`) and `KBotTutorialRequest` (flow id + the flow's `host-key`). A window that knows a tutorial state; the runner tells it to start with a key, so one window can serve several tutorials. Nothing implements it yet (000T-02).
- `src/KBot.App/Tutorial/TutorialFlow.vb` — file model and parser: header (`id`, `title`, `part`, `keywords`, `starts`, `host-key`), step keys (`target`, `part`, `anchor`, `wait`, `when`, `optional`, `merge`, `why`, `dim`, `allow`). Unknown key = `ArgumentException`. An optional step without `why` is refused. `AcceptedFrom(index)` = the look-ahead rule (the step itself; after an optional or merged step the next one too, up to and including the first mandatory one). **Added to the plan:** the `merge: yes` key (step 3 «select the reservation» is completed by the «+» click of step 4: they are one mouse-down) and `when: checked:Type.control` takes a full target.
- `src/KBot.App/Tutorial/TutorialRunner.vb` — one running tutorial: per step it resolves the target (`HelpTourRunner.FindTarget`, parts through `IKBotHelpParts`, anchors through the host), skips a step whose `when:` does not hold, arms what it waits for (tree `NodeMouseUp`, tree `RightIconClicked`, `Click`, `TextChanged` / `SelectedIndexChanged` / `CheckedChanged`) or polls state (nav tab selected, check box ticked, window opened / closed, `TutorialSignal`), ring + dim + bubble per host window created AFTER the window is shown, a 150 ms timer that follows a moved window, hides everything while another program or a non-K-BOT dialog is in front, and the exit question (`KBotMessage.Show`) on a click on the veil, a key typed in the dimmed window outside the allowed places (`IMessageFilter`), «Mă opresc», or the host window closing. «Da» ends it, «Nu» stays on the same step.
- `src/KBot.App/Tutorial/TutorialDim.vb` — the veil: borderless, black 45 %, `Region` with holes, no focus, owned by the host window; a click on it raises `Clicked`.
- `src/KBot.App/Help/HelpTourBubble.vb` — `ShowTutorial(...)`: the same callout bubble, no «Inapoi», right button «Sari peste» / «Inainte» / hidden, «Inchide» reads «Ma opresc»; arrow/Enter keys honour the hidden button. (No designer change.)
- `src/KBot.App/Help/HelpLibrary.vb` — `Tutorials`, `FindTutorial`, loads `<root>\tutorials\*.md`, the topic loader skips that folder. `HelpTour.CleanText` is now `Friend` (shared with the tutorial parser).
- `src/KBot.App/Help/HelpService.vb` — `StartTutorial(id)`.
- `tools/HelpCheck/Check-Help.ps1` — skips `tutorials\` in the topic scan, accepts slice id `000T` / `000T-NN`, validates tutorial files (header keys, targets, parts, wait / when kinds, `why` on optional steps, a warning when an `opens:` window does not implement `IKBotTutorialHost`), reports the tutorial count.
- `tests/KBot.App.Tests/TutorialFlowTests.vb` — parser + look-ahead tests (pure, no windows).

### Files touched

`src/KBot.Theming/KBotTutorial.vb` (new), `src/KBot.App/Tutorial/{TutorialFlow,TutorialRunner,TutorialDim}.vb` (new), `src/KBot.App/Help/{HelpTourBubble,HelpTour,HelpLibrary,HelpService}.vb`, `tools/HelpCheck/Check-Help.ps1`, `tests/KBot.App.Tests/TutorialFlowTests.vb` (new), `docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`.

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `tests\KBot.App.Tests` does **not compile**, from errors that were there before this work (`ForexeAnswerStoreTests.vb(43)`, `MainFormPoartaDdfTests.vb`, `MainFormNavItemsTests.vb`: `KbotForm`'s constructor now needs `capturiApi`). I did not touch those tests. `TutorialFlowTests` was run instead from a throw-away project (compiles only that file against `KBot.App`): **17 passed, 0 failed**.
- `Check-Help.ps1 -Coverage`: «No errors.»; the coverage list is NOT «(none)»: six windows without a topic (`HelpCaptureViewForm`, `IstoricIntervalForm`, `PrintListPage`, `SetariAccessView`, `SetariIstoricView`, `UpdateOfferForm`), none from this slice.
- Nothing was run on screen.

### Left unverified or deferred

- **Nothing seen on screen.** The per-window presenter inside the modal `DdfEditForm`, the veil against drop-down lists and the OS file dialog, the ring / veil following a moved window, the key policing — all written from reading code only.
- No window implements `IKBotTutorialHost` yet, no tutorial file exists, no search / «?» entry starts one: all of that is 000T-02. `HelpService.StartTutorial` has no caller yet.
- A `part:` that is hidden by state is NOT revealed for the step (tours do that through `IKBotHelpReveal`); the tutorial falls back to the whole control.
- Step 10 of the operator's flow (the «Element fundamentare» cell) may be impossible in the «+» flow: the grid is read-only when its rows come from reservations (`DdfDraft.DinRezervari`).
- Out of this slice: the «Parteneri» tab must follow the «Partener asociat» box (corrective of 0094-02).
- Help (`0000-NN`) and NOUTATI.md are not written: no operator-visible behaviour exists until 000T-02 wires a start.
- Weekly usage was 86 % at this point (stop line 95 %).

<a id="slice-000t-02"></a>

## SLICE-000T-02 — the 18-step flow, the hosts, and finding a tutorial

Second sub-slice of 000T (plan: `~/.claude/plans/i-need-a-tutorial-humble-pretzel.md`). Makes the engine of 000T-01 reachable: the operator's «revizie pe baza unei rezervari existente» flow exists as a file, the two windows it crosses are tutorial hosts, and a tutorial is found from a typed question and from every «?».

### What changed and why

- `src/KBot.App/HelpContent/tutorials/revizie-din-rezervare.md` — the flow, 18 steps as the operator listed them: 1 an angajament that has reservations (ring on those rows, the rest dimmed; done by the signal below, or by pressing the tab), 2 the «Rezervari» button (shown, never pressed for the user), 3 the reservation with «+» (`select`, merged with 4: one mouse-down), 4 «+», 5 wait for `DdfEditForm`, 6 Compartiment (`when: enabled`), 7-17 optional (partner box, partner, short description, «Element fundamentare» (`when: editable`), tab «Descriere», long description, tab «Fisiere», attach, tab «Parteneri» (`when: checked:DdfEditForm.chkPartAng`), another partner, «Asociaza»), 18 «Salveaza documentul». Every optional step carries its `why:`. Names checked by `Check-Help.ps1` against the designers.
- `src/KBot.App/KbotForm.Tutorial.vb` — `KbotForm` implements `IKBotTutorialHost`: serves every key; anchor `tree.angajamente-cu-rezervari` (the rows on screen whose `AngajamentTreeInfo.AreRezervari`); anything `rezervari.*` is delegated to the open `RezervariView`. `src/KBot.App/KbotForm.Tree.vb`: `Tree_NodeMouseUp` raises the signal `angajament-cu-rezervari` when the selected row has reservations (one line).
- `src/KBot.App/Views/RezervariView.Tutorial.vb` — anchor `rezervari.plus` = the row button(s) on screen of the tree.
- `src/KBot.Controls/Tree/AdvancedTreeControl.HelpParts.vb` — `RowRectsOnScreen(match)` and `RightIconRectsOnScreen()` (client rectangles of the rows / row buttons on screen; they show nothing for the purpose, unlike the `node.icon` demo of the tours).
- `src/KBot.App/DDF_EDIT/DdfEditForm.Tutorial.vb` — `DdfEditForm` implements the host contract; it serves the key `rezervare-plus` only (another key = the runner tells the user and stops), no anchors, no signals.
- Discovery: `HelpLibrary.SearchTutorials` (stems of title + keywords + step titles, same filler-word / ending rules as the topics); `HelpSearchSession.Search` puts matching tutorials FIRST in the results, `Invoke` starts one; `HelpService.PopupHomeRows` adds a «Tutoriale» group to the «?» popup of EVERY window (those that start in the window of the «?» first); `HelpService.TutorialRow` (uses the existing «tour» row look).
- `src/KBot.App/Tutorial/TutorialRunner.vb` — an optional step with a manual wait gets «Inainte», not «Sari peste» (it has nothing to watch).
- `tests/KBot.App.Tests/TutorialSearchTests.vb` — the typed question finds the tutorial (loaded from a temp folder).

### Files touched

`src/KBot.App/HelpContent/tutorials/revizie-din-rezervare.md` (new), `src/KBot.App/KbotForm.Tutorial.vb` (new), `src/KBot.App/Views/RezervariView.Tutorial.vb` (new), `src/KBot.App/DDF_EDIT/DdfEditForm.Tutorial.vb` (new), `src/KBot.App/KbotForm.Tree.vb`, `src/KBot.Controls/Tree/AdvancedTreeControl.HelpParts.vb`, `src/KBot.App/Tutorial/{TutorialFlow,TutorialRunner}.vb`, `src/KBot.App/Help/{HelpLibrary,HelpSearchSession,HelpService}.vb`, `tests/KBot.App.Tests/TutorialSearchTests.vb` (new), status files.

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors** (it builds `KBot.Controls` too).
- Tests: the repo's `tests\KBot.App.Tests` still does not compile (errors from before 000T, see 000T-01), so the two tutorial test files were run from the throw-away project: **23 passed, 0 failed** (parser, look-ahead, search).
- `Check-Help.ps1`: **«No errors.»**, 1 tutorial; no warning about a window that is not a host.
- Nothing was run on screen.

### Left unverified or deferred

- **Nothing seen on screen**: the whole flow, the veil over the main window, the per-window presenter in the modal `DdfEditForm`, the «+» anchor, the keyboard policing.
- **Step 1 starts blind to the current selection**: if an angajament with reservations is ALREADY selected when the tutorial starts, the signal does not come again; the user selects it again, or presses «Rezervari» (step 2's tab also completes step 1).
- **Steps 6-8 and 10 only exist for the INITIAL reservation** (`when:` skips them otherwise); **step 10 may never show in the «+» flow** (grid read-only when rows come from reservations). Step 15 (`when: checked:...chkPartAng`) assumes the tab follows the box, a corrective of 0094-02 not made yet: until then the «Parteneri» tab is always visible and the step simply applies when the box is ticked.
- Steps 10 and 12 use `wait: manual` («Inainte»): no change event is wired for the grid cell and the rich editor.
- Tutorial uses are NOT logged in the question log (rows of the popup / results are not topic hits).
- The tutorial is not yet in the help text (`0000-NN`) and NOUTATI.md: that is 000T-05, after the designer exists.

<a id="slice-000t-03"></a>

## SLICE-000T-03 — the visual designer and the on-screen picker

Third sub-slice of 000T (plan: `~/.claude/plans/i-need-a-tutorial-humble-pretzel.md`). The operator wants to design tutorials visually while the app is loaded. This sub-slice is the designer window, the picker («Alege pe ecran») and saving; the click-through Recorder is 000T-04.

### What changed and why

- `src/KBot.App/Tutorial/TutorialDesignerForm.vb` + `.Designer.vb` (every control declared in the designer file, `KBotThemedForm`, card + caption bar like the other dialogs): a combo of the saved tutorials («(tutorial nou)» first), a property grid for the tutorial header (`id`, `title`, `part`, `keywords`, `starts`, `host-key`), the list of steps with «+ Pas / Șterge / ▲ / ▼ / Copie», a property grid for the step being edited (target, part, anchor, wait, when, optional, merge, why, dim, allow) and a text box for the bubble text, «Alege pe ecran...», «Salvează», «Testează de la pasul», «Închide». Unsaved changes are asked about before switching tutorial or closing. Tooltips are `KBotToolTip` (Romanian). Opened from a new row «Designer tutoriale» of the MENIU menu, which follows the capture mode exactly (`KbotForm.HelpCaptureModeOn`: advanced options + «Mod capturi pentru ajutor»).
- `src/KBot.App/Tutorial/TutorialPicker.vb`: «Alege pe ecran». The designer hides itself; a see-through layer over the whole screen catches the mouse; the control or painted part under it gets the tour ring (`HelpTourFrame`) and a small label (target / part / the wait that fits). Left click takes it, Esc or right click gives up. Only K-BOT's own windows are read. Resolution: z-order from `HelpCaptureNative.TopLevelWindows`, then `GetChildAtPoint` down to the nearest NAMED control, `HelpService.ScreenKeys` for `Type.control`, parts by `IKBotHelpParts.HelpPartBounds` with the same names the checker allows (tree row button through the new `RightIconRectsOnScreen`, nav item through the new `KBotNavList.ItemKeyAt`). A control with no name is refused with an explanation.
- `src/KBot.App/Tutorial/TutorialFlow.vb`: `ToMarkdown()` writes a tutorial in exactly the shape the parser reads (keys at their default are left out; bullets back to «- »). Property-grid attributes (`Browsable(False)` on `Steps`, `SourcePath`, `Text`).
- `src/KBot.App/Tutorial/TutorialStore.vb`: saves to `<AppDir>\Help\tutorials\<id>.md` and, on a repo build, to `src\KBot.App\HelpContent\tutorials\` too (the capture tool's folder search, `HelpCaptureStore.FindSourceFolder`, now `Friend`); it refuses what the parser would refuse; UTF-8 without BOM; the help is reloaded after saving.
- `src/KBot.App/Tutorial/TutorialRunner.vb`: `Start(..., k_startIndex)` — «Testează de la pasul» runs the tutorial as it is now (unsaved, read back through the real parser) from the chosen step; the designer minimizes and comes back.
- `src/KBot.Controls/NavList/KBotNavList.HelpParts.vb`: `ItemKeyAt(point)`.
- `tools/HelpCheck/Check-Help.ps1`: the three tutorial windows are ignored by the «no topic» list (operator tool).
- `tests/KBot.App.Tests/TutorialFlowTests.vb`: `ToMarkdown` parses back to the same tutorial (round trip).

### Files touched

`src/KBot.App/Tutorial/{TutorialDesignerForm,TutorialDesignerForm.Designer,TutorialPicker,TutorialStore}.vb` (new), `src/KBot.App/Tutorial/{TutorialFlow,TutorialRunner}.vb`, `src/KBot.App/KbotForm.{Designer,HelpCapture,Nomenclatoare,Tutorial}.vb`, `src/KBot.App/Help/HelpCaptureStore.vb`, `src/KBot.Controls/NavList/KBotNavList.HelpParts.vb`, `tools/HelpCheck/Check-Help.ps1`, `tests/KBot.App.Tests/TutorialFlowTests.vb`, status files.

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors** (a `Move` method that shadowed `Control.Move` was renamed `MoveStep`).
- Tests: still run from the throw-away project because `tests\KBot.App.Tests` does not compile (old errors, see 000T-01): **24 passed, 0 failed** after the round-trip test was added.
- `Check-Help.ps1 -Coverage`: **«No errors.»**; the «no topic» list is the same six windows as before.
- **The designer window and the picker were never opened.**

### Left unverified or deferred

- **Nothing seen on screen**: the layout of the designer (docking order of the panels, sizes at 100 / 125 / 150 %), the property grids under the themes, the picker's ring and label, the hit-testing of painted parts, the menu row.
- The picker cannot pick something that only exists while a menu / drop-down is open (the layer takes the mouse): that is what the Recorder (000T-04) is for.
- The picker writes `target` / `part` / the guessed `wait`; the `anchor:` (rows that depend on data) is typed by hand.
- No tutorial of the designer is in the help (operator tool, like the capture mode); NOUTATI.md not touched.

<a id="slice-000t-04"></a>

## SLICE-000T-04 — the Recorder

Fourth sub-slice of 000T. The operator clicks through the real flow once and the designer writes the steps. This covers what the picker cannot reach (a drop-down or a menu that is only open while it is used) and is faster than picking step by step. («Testează de la pasul N» was already done in 000T-03, together with the designer.)

### What changed and why

- `src/KBot.App/Tutorial/TutorialRecorder.vb`: while it runs (an `IMessageFilter` + a 300 ms timer) it turns into steps
  - a mouse press on a K-BOT control that has something a tutorial can wait for (button -> `click`, tree row -> `select`, the row button of a tree -> `click` with `part: node.icon`, nav item -> `tab:<key>` with `part: item:<key>`, tick box -> `checked`); a label or a panel is ignored;
  - the FIRST key typed into an input (`changed`), once per input; the typed value is never read or kept;
  - every window that opens and is a real window (modal or in the taskbar; pop-up lists and menus are not) -> a step `wait: opens:<Type>` with no target. Titles are working titles in the words of the screen («Apasă «Salvează documentul»», «Fila «rezervari»», «Se deschide DdfEditForm»); texts, `optional`, `when` and `why` are filled in by the operator afterwards. The same press twice in a row is one step. The tutorial windows, the designer and the recorder's own bar are never recorded.
- `src/KBot.App/Tutorial/TutorialRecorderBar.vb` + `.Designer.vb`: the small bar (bottom right, top-most, takes no focus): «Înregistrez: N pași...» and «Oprește». All controls in the designer file, `KBotToolTip`.
- `src/KBot.App/Tutorial/TutorialPicker.vb`: the resolution was split into shared functions (`ResolveAt`, `NearestNamed`, `ForControl`) so the recorder and the picker give the same answer for the same control; `TutorialPick.Control`.
- `src/KBot.App/Tutorial/TutorialDesignerForm.vb` + `.Designer.vb`: the button «Înregistrează»: the designer minimizes, «Oprește» brings the steps back after the chosen step.
- `tools/HelpCheck/Check-Help.ps1`: `TutorialRecorderBar` joins the operator-tool windows of the ignore list.

### Files touched

`src/KBot.App/Tutorial/{TutorialRecorder,TutorialRecorderBar,TutorialRecorderBar.Designer}.vb` (new), `src/KBot.App/Tutorial/{TutorialPicker,TutorialDesignerForm,TutorialDesignerForm.Designer}.vb`, `tools/HelpCheck/Check-Help.ps1`, status files.

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `Check-Help.ps1 -Coverage`: **«No errors.»**, same six uncovered windows as before (none from this slice).
- No test was written for the recorder: it only exists against live windows and the message loop (the operator's rule is no windows on screen without a heads-up). **It was never run.**

### Left unverified or deferred

- **Never run**: the bar, the message filter, what a real press resolves to (the filter sees the press BEFORE the control handles it, so a click that rebuilds the tree resolves against the old rows — meant, but unchecked), the «opens» heuristic (`Modal OrElse ShowInTaskbar`), and the focus behaviour of the no-activate bar.
- Closing a window is not recorded (a `closes` step is added by hand); keyboard actions other than the first key in an input (Enter, Tab, shortcuts) are not recorded.
- A press inside a pop-up that is not a K-BOT window (the Windows file chooser) is not seen at all, so «Atașează fișier» records the button press but not the choosing.

<a id="slice-000t-05"></a>

## SLICE-000T-05 — the tutorial's question box opens on top

Corrective on 000T (operator, 04.10.2026): the «Vrei să ieși din tutorial?» box (and the other tutorial boxes) opened UNDER the step card, so the operator could not read or answer it.

### What changed and why

- Cause: the step card (`HelpTourBubble`) and the ring (`HelpTourFrame`) are top-most windows. While the box is open the runner's timer is paused (`_asking`), so they stay up, and an ordinary `MessageBox` (not top-most) opens below them.
- `src/KBot.Theming/KBotMessage.vb`: new `ShowOnTop` (two overloads, with and without owner). Same journal line in `mesaje_operator.log` as `Show`, same `DialogResult`; the box is created with the Win32 style `MB_TOPMOST` (0x40000), passed through `MessageBoxOptions`. The alternative (a custom box) was not needed: `MB_TOPMOST` keeps the real Windows box, its sounds, its keyboard behaviour and the logging path.
- `src/KBot.App/Tutorial/TutorialRunner.vb`: the three boxes of the runner use `ShowOnTop` (the exit question, «Această fereastră nu poate fi folosită...», «pornește dintr-o altă fereastră...»). The designer's boxes are not covered by the card, so they stay on `Show`.

### Second pass (same slice, same day) — three corrections from the first run on screen

1. **Space in the long-description editor moved the tutorial on.** That step waits for «Înainte» (manual), and the bubble's «Înainte» had the keyboard focus: a Space on a focused button is a click. (On the text-box steps the step moves on at the first character, so it was never noticed.) Fix, three layers:
   - `HelpTourBubble.NeverActivates` (new; set only by `TutorialRunner`): the bubble is created with `WS_EX_NOACTIVATE` and `ShowWithoutActivation`, so it neither takes the focus when shown / re-shown nor when clicked; its buttons still work; its own key handling is off. The guided tours keep the old behaviour.
   - `TutorialRunner.KeyFilter` swallows every keyboard message (0x100-0x109) aimed at a control of the bubble and hands the focus back to the window; whatever puts the focus there, a key can no longer click a bubble button.
   - After «Sari peste» / «Înainte» / «Mă opresc» the focus goes back to the window the operator works in (`FocusHost`).
2. **«Salvează documentul» was lit during the optional steps.** Cause: the look-ahead (`TutorialFlow.AcceptedFrom`) ran past all optional steps to the first mandatory one, and every step in that list gets a hole in the veil. Now an optional (or merged) step accepts only the NEXT step; a merged next step passes the permission on. A step further on stays dimmed until its turn; «Sari peste» is the way past an optional step. Consequence for the operator: to reach «Salvează documentul» from the middle of the optional steps, press «Sari peste» on each (it was possible to click «Salvează» straight away before). Help text adapted (see below). Test `AcceptedFrom_OptionalChain...` rewritten to the new rule.
3. **Step «Alege încă un partener»: the text was hidden.** Cause: `HelpTourBubble.FitToText` decided whether the note (the orange «Pas opțional: …» line) counts by `lblNota.Visible`, and `Control.Visible` is False whenever the bubble itself is hidden (it is hidden while the Windows file chooser of the previous step is in front). The height was then measured without the note (here 3 lines: the optional reason + «nu e pe ecran acum»), the labels overlapped and the first lines were covered. It now counts the note by its text.

Help: `HelpContent/contabil/ajutor.md` (tutorials section, `<!-- slice: 000T -->` already on it): optional steps no longer promise a jump to «Salvează documentul»; `tutorials/revizie-din-rezervare.md` last step: «poți salva oricând» replaced by «se pot sări cu «Sari peste»»; `HelpContent/README.md` look-ahead rule updated. `Check-Help.ps1`: «No errors.».

### Third pass — typing completed the step; «Înapoi»

1. **The real cause of «anything typed moves the tutorial on» (step 9/18 «Descriere scurtă»)**: a `wait: changed` step was completed by `TextChanged`, i.e. by the first letter. The step after it («Element fundamentare») does not apply to a document made from reservations (`when: editable`), so the jump read 9 -> 11. The jump 9 -> 11 itself is by design; the first-letter trigger was the bug (the keyboard fix of the second pass was a different, real problem and stays). Now: tick box = `CheckedChanged`; `KBotComboBox` = `SelectedIndexChanged` only (a real choice, not typing in its search box); any other field = typed (`TextChanged` only marks it) AND the focus has left it (`ContainsFocus`, judged on the 150 ms tick). Step 9's text now says «apoi treci în alt câmp».
2. **«Înapoi»** in the tutorial bubble: shown on every step; enabled when `TutorialRunner.PreviousStep` finds an earlier step that applies (`when:`), in the same window as the current one, and neither is an `opens:` wait. It shows that step again as a **revisit**: nothing done by state completes it (tab already open, box already ticked would bounce forward at once), only a new action, or «Înainte ►» (shown on a revisited step); what the operator did meanwhile is not undone. Not possible: back into a window left behind (the list under the document that just opened).
3. **Designer**: `btnInapoi`, `btnInainte`, `btnInchide` were already declared in `HelpTourBubble.Designer.vb` (the bubble is shared with the guided tours; the three buttons are the same ones). The code only sets their Text / Visible / Enabled; position, size, images, fonts are the designer's. The bubble's height now takes the button row from the designer (`tlyCorp` row 5) instead of a fixed 44. Note: the form is shared, so a change there shows in the tours as well.

Files: `Tutorial/TutorialRunner.vb`, `Help/HelpTourBubble.vb`, help (`ajutor.md`, `tutorials/revizie-din-rezervare.md`, `README.md`). Build **0 warnings, 0 errors**; `Check-Help.ps1` «No errors.». Not run on screen. Unverified: whether `KBotTextField` raises `TextChanged` on itself and `ContainsFocus` is true while its inner box has the focus (both expected from the pre-existing code, which hooked the same event); behaviour of the revisit on `signal:` steps (no state completion there, «Înainte ►» is the way on).

### Fourth pass — Enter finishes a text step

Tick box unchanged (`CheckedChanged`); leaving the field still finishes a typed step; NEW: **Enter** in a single-line, editable `TextBoxBase` (`TutorialRunner.OnEnterKey`, called from the key filter) finishes the «changed» step whose control is, or contains, that box. Not for multi-line boxes (`Multiline` — Enter is a new line; `KBotTextBox` and the rich editor are such), nor for tick boxes / `KBotComboBox`. Posted to the window after the box handled the key. Pressing Enter in an empty field finishes the step too (not required that something was typed). Help (`ajutor.md`, step 9 text, README) says so. Build 0 warnings / 0 errors, `Check-Help.ps1` «No errors.»; not run on screen.

### Fifth pass — «Sari la pasul obligatoriu»

New button `btnSariLaObligatoriu` in `HelpTourBubble.Designer.vb` (own auto-size row 5 of `tlyCorp`, so a hidden one takes no space; the buttons row moved to row 6; anchored right, 200x32, editable in the designer like the other three). Shown only on an OPTIONAL tutorial step (never in the guided tours). Click -> `TutorialRunner.OnSkipOptional`: goes to the first later step that is not optional and whose `when:` holds; none left -> `Finish()` (the tutorial ends, bubble closes, no «Vrei să ieși» question). The bubble height counts the button by its own flag. Help (`ajutor.md`) has a bullet. Build 0 warnings / 0 errors; not run on screen. No tooltip on it (the bubble is top-most; a tooltip window might open under it — not tried). A mandatory step whose `when:` fails at click time is skipped like any step; the jump skips the actions of the steps it passes (nothing is marked done).

### Sixth pass — borders came back on the bubble buttons

The operator put `ButtonStyles.ApplyTrans(...)` for the four buttons in `ShowTutorial`; the borders stayed. Cause (read in `KBotThemedForm.OnLoad` / `HandleThemeChanged` and `ThemeManager`): the form runs `ThemeManager.Apply(Me)` — which gives every `Button` the theme's bordered / rounded look; a transparent button is not an "accent" button, so it is not spared — and only THEN `OnThemeChanged()`. The first step is built before the first Show, so Load's Apply overwrote it, and every later theme or scaling broadcast does the same. Moved to `HelpTourBubble.OnThemeChanged` (flag `_tutorialButtons`, set by `ShowTutorial`, so the guided tours keep their buttons). Build 0 warnings / 0 errors; not run on screen. Seen in passing, left as the operator wrote it: the two lines setting `btnInainte.Text` / `btnInchide.Text` in `ShowTutorial` are commented out, so those texts («Sari peste» / «Înainte ►» / «Gata», «Mă opresc») now come only from the designer.

### Seventh pass — «Mă opresc» ends the tutorial without a question

`TutorialRunner.OnStopRequested` now calls `Finish()` directly (was `Ask("Vrei să ieși din tutorial?")`). Esc / Alt+F4 on the bubble raise the same event, so they also end it at once. The question stays for the other exits (a click on the dark area, a key typed elsewhere, closing the window being worked in). Help (`ajutor.md`) adapted. Build 0 / 0, `Check-Help.ps1` «No errors.»; not run on screen.

### Eighth pass — navigation buttons always visible

`HelpTourBubble.ShowTutorial`: «Înapoi», «Înainte/Sari peste» and «Sari la pasul obligatoriu» are all `Visible = True` on every tutorial step and only `Enabled` changes (Înapoi: an earlier step exists; Înainte: manual / optional / revisited step; Sari la pasul obligatoriu: optional step). `_skipShown` is now always true in a tutorial, so the bubble height always includes the skip button's row. Guided tours are unchanged. Help bullet added. Build 0 / 0; not run on screen.

### Files touched

`src/KBot.Theming/KBotMessage.vb`, `src/KBot.App/Tutorial/{TutorialRunner,TutorialFlow}.vb`, `src/KBot.App/Help/HelpTourBubble.vb`, `tests/KBot.App.Tests/TutorialFlowTests.vb` (not compiled: that project has older errors), the three help files above, this worklog, `state/KBOT_STATUS_0000-0009.md`, `KBOT_STATUS.md`.

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- No test written or run (nothing to unit-test: it is the Windows box).
- Help: no change. The help does not describe where the box sits; the texts the operator reads are unchanged.

### Left unverified or deferred

- **Second pass not seen on screen either.** To check: type with spaces in the long-description editor (the step must stay); «Salvează documentul» must stay dark until the last step; step «Alege încă un partener» (right after the file chooser step) must show its text and the whole note; the bubble's buttons must still react to a click.
- Cause 1 is derived from the code (a focused button + Space) and from the 7th character in the editor's counter (the Space reached the editor too); the log or a run was not looked at. The three layers cover it whichever way the focus got there. If the step still moves on, the cause is something else.
- Cause 3 is derived from the code (`Visible` of a child of a hidden form), same caveat.

- **Not seen on screen.** That `MessageBox.Show` passes the extra `0x40000` bit through unchanged is known from how the style word is built, but it was not run here: the operator should trigger the exit question once (click outside the hole during a tutorial) and confirm the box is above the card.
- A top-most box stays above OTHER programs too while it is open (Alt+Tab does not put it behind). It is short-lived and modal, so accepted; if it bothers, the alternative is to hide the card and ring while asking.
