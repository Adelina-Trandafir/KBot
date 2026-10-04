# SLICE-000T-02 — the 18-step flow, the hosts, and finding a tutorial

Second sub-slice of 000T (plan: `~/.claude/plans/i-need-a-tutorial-humble-pretzel.md`). Makes the engine of
000T-01 reachable: the operator's «revizie pe baza unei rezervari existente» flow exists as a file, the two
windows it crosses are tutorial hosts, and a tutorial is found from a typed question and from every «?».

## What changed and why

- `src/KBot.App/HelpContent/tutorials/revizie-din-rezervare.md` — the flow, 18 steps as the operator listed them:
  1 an angajament that has reservations (ring on those rows, the rest dimmed; done by the signal below, or by
  pressing the tab), 2 the «Rezervari» button (shown, never pressed for the user), 3 the reservation with «+»
  (`select`, merged with 4: one mouse-down), 4 «+», 5 wait for `DdfEditForm`, 6 Compartiment (`when: enabled`),
  7-17 optional (partner box, partner, short description, «Element fundamentare» (`when: editable`), tab
  «Descriere», long description, tab «Fisiere», attach, tab «Parteneri» (`when: checked:DdfEditForm.chkPartAng`),
  another partner, «Asociaza»), 18 «Salveaza documentul». Every optional step carries its `why:`. Names checked by
  `Check-Help.ps1` against the designers.
- `src/KBot.App/KbotForm.Tutorial.vb` — `KbotForm` implements `IKBotTutorialHost`: serves every key; anchor
  `tree.angajamente-cu-rezervari` (the rows on screen whose `AngajamentTreeInfo.AreRezervari`); anything
  `rezervari.*` is delegated to the open `RezervariView`. `src/KBot.App/KbotForm.Tree.vb`:
  `Tree_NodeMouseUp` raises the signal `angajament-cu-rezervari` when the selected row has reservations (one line).
- `src/KBot.App/Views/RezervariView.Tutorial.vb` — anchor `rezervari.plus` = the row button(s) on screen of the tree.
- `src/KBot.Controls/Tree/AdvancedTreeControl.HelpParts.vb` — `RowRectsOnScreen(match)` and `RightIconRectsOnScreen()`
  (client rectangles of the rows / row buttons on screen; they show nothing for the purpose, unlike the
  `node.icon` demo of the tours).
- `src/KBot.App/DDF_EDIT/DdfEditForm.Tutorial.vb` — `DdfEditForm` implements the host contract; it serves the key
  `rezervare-plus` only (another key = the runner tells the user and stops), no anchors, no signals.
- Discovery: `HelpLibrary.SearchTutorials` (stems of title + keywords + step titles, same filler-word / ending rules
  as the topics); `HelpSearchSession.Search` puts matching tutorials FIRST in the results, `Invoke` starts one;
  `HelpService.PopupHomeRows` adds a «Tutoriale» group to the «?» popup of EVERY window (those that start in the
  window of the «?» first); `HelpService.TutorialRow` (uses the existing «tour» row look).
- `src/KBot.App/Tutorial/TutorialRunner.vb` — an optional step with a manual wait gets «Inainte», not «Sari peste»
  (it has nothing to watch).
- `tests/KBot.App.Tests/TutorialSearchTests.vb` — the typed question finds the tutorial (loaded from a temp folder).

## Files touched

`src/KBot.App/HelpContent/tutorials/revizie-din-rezervare.md` (new), `src/KBot.App/KbotForm.Tutorial.vb` (new),
`src/KBot.App/Views/RezervariView.Tutorial.vb` (new), `src/KBot.App/DDF_EDIT/DdfEditForm.Tutorial.vb` (new),
`src/KBot.App/KbotForm.Tree.vb`, `src/KBot.Controls/Tree/AdvancedTreeControl.HelpParts.vb`,
`src/KBot.App/Tutorial/{TutorialFlow,TutorialRunner}.vb`, `src/KBot.App/Help/{HelpLibrary,HelpSearchSession,HelpService}.vb`,
`tests/KBot.App.Tests/TutorialSearchTests.vb` (new), status files.

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors** (it builds `KBot.Controls` too).
- Tests: the repo's `tests\KBot.App.Tests` still does not compile (errors from before 000T, see 000T-01), so the two
  tutorial test files were run from the throw-away project: **23 passed, 0 failed** (parser, look-ahead, search).
- `Check-Help.ps1`: **«No errors.»**, 1 tutorial; no warning about a window that is not a host.
- Nothing was run on screen.

## Left unverified or deferred

- **Nothing seen on screen**: the whole flow, the veil over the main window, the per-window presenter in the modal
  `DdfEditForm`, the «+» anchor, the keyboard policing.
- **Step 1 starts blind to the current selection**: if an angajament with reservations is ALREADY selected when the
  tutorial starts, the signal does not come again; the user selects it again, or presses «Rezervari» (step 2's tab
  also completes step 1).
- **Steps 6-8 and 10 only exist for the INITIAL reservation** (`when:` skips them otherwise); **step 10 may never
  show in the «+» flow** (grid read-only when rows come from reservations). Step 15 (`when: checked:...chkPartAng`)
  assumes the tab follows the box, a corrective of 0094-02 not made yet: until then the «Parteneri» tab is always
  visible and the step simply applies when the box is ticked.
- Steps 10 and 12 use `wait: manual` («Inainte»): no change event is wired for the grid cell and the rich editor.
- Tutorial uses are NOT logged in the question log (rows of the popup / results are not topic hits).
- The tutorial is not yet in the help text (`0000-NN`) and NOUTATI.md: that is 000T-05, after the designer exists.
