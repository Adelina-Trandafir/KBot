# SLICE-0000-30 — guided tours show what the app hides, and say how to get it

Operator request, 01.10.2026: «all elements hidden by various facts (switches, not being connected
to FOREXE...) in the guided tour must become visible when the step talks about them and must mention
how they can get visible; also this must happen in the help».

## What changed and why

Before: a tour step about a part that was not on screen (a nav button the angajament has no data for,
«Browser FOREXE» while not connected, a hidden FOREXE-band button, the «+» of a row, the footer menu
icon of Rezervari) was skipped or got the note «Partea ... nu e pe ecran acum». The operator never saw
it and the text said «gri» for things the app really HIDES (`KbotForm.Views.ApplyViewGating` uses
`SetItemVisible`).

Engine (`IKBotHelpReveal`, new, `KBot.Theming/KBotHelp.vb`):
- `HelpTourRunner.ShowStep`: (1) a control named by `target: View.control` that is not visible but sits
  under a visible parent is shown for the step (`FindHiddenTarget`, put back by `EndDemo`) -- covers
  `SumarView.btnPartners`, `ForexeFooterView.lblCert / btnIstoric / btnBrowser / btnExtinde`;
  (2) a `part:` that is missing is asked of `IKBotHelpReveal` (target and every control above it,
  `TryRevealPart`), then measured again. When anything was shown the bubble adds the note «Îl vezi acum
  doar pentru tur: în mod obișnuit K-BOT îl ascunde ...».
- `KBotNavList` (`.HelpParts.vb`): a hidden item (`part: item:<Key>`) is shown, put back by `HelpRevealEnd`.
- `AdvancedTreeControl` (`.HelpParts.vb`): `node.icon` (picture on the selected leaf / first leaf) and
  `footer.left` from two new hidden properties `HelpDemoRightIcon`, `HelpDemoFooterLeftIcon`, which the
  VIEW fills: `RezervariView` (the «+» and `_iconitaMeniu`), `PlatiView` (the «+»).

Help (all tagged `0000-30`): every step / section about something conditional now says what makes it
appear for real.
- Tours: `tur-fereastra` (MENIU rows, intro of «Vederile» -- the old «gri» was wrong --, one condition per
  view step), `tur-forexe` (certificate label, Istoric, Browser, console buttons), `tur-sumar`
  (Asociaza parteneri), `tur-ddf`, `tur-rezervari`, `tur-plati`, `tur-ord` (the «+»), `tur-ddf-editor`
  (Sectiunea B), `tur-notecab`.
- Topics: `contabil.fereastra` (section «Vederile»: a table, view -> when it appears),
  `contabil.notecab`, `contabil.ddf.rezervare`, `contabil.forexe` (band table), `contabil.vederi.sumar`,
  `contabil.ddf.editor`, `contabil.ajutor` (tours: what hidden things do).
- Maintainer docs: `HelpContent/README.md` (Guided tours), `docs/HELP_SYSTEM.md` (tours + writing rule).

## Files touched

- Code: `src/KBot.Theming/KBotHelp.vb`, `src/KBot.App/Help/HelpTourRunner.vb`,
  `src/KBot.Controls/NavList/KBotNavList.HelpParts.vb`,
  `src/KBot.Controls/Tree/AdvancedTreeControl.HelpParts.vb`, `src/KBot.App/Views/RezervariView.vb`,
  `src/KBot.App/Views/PlatiView.vb`.
- Help: files above under `src/KBot.App/HelpContent/`, `help-version.txt`.

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors.
- `Check-Help.ps1 -Coverage`: see the STATUS row. Nothing was run on screen (operator rule).

## Left unverified or deferred

- **Not seen on screen.** The reveal needs one look by the operator: tour «Fereastra principală» on an
  angajament with few views (the hidden view buttons appear one by one, with the note, and disappear
  after their step); tour «Legătura cu FOREXE» while not connected; «Drumul unui document de
  fundamentare» on an angajament with a «+» left.
- **MENIU rows are NOT opened by the tour** («Jurnal activitate», «(!) Operațiuni necorelate»): the
  popup menu closes when its host window is deactivated (`KBotDropDownMenu.HostForm_Deactivate`) and the
  bubble takes the focus. The step text says when each row appears. Needs a decision if wanted
  (e.g. a bubble that does not activate).
- **A view hidden as a whole** (no data for the angajament) cannot be shown by a tour: the step keeps its
  note («selectează un angajament care o are»).
- **Finding for the operator (not changed):** `ForexeFooterView.btnIstoric` is hidden in the constructor
  and no code ever shows it -- the button is invisible in the real app, yet the help (`contabil.forexe`,
  `consola`) and `tur-forexe` described it as part of the band. The help now says it is hidden and no
  setting shows it; decide whether to show it or drop it from the help.
- `btnCoada` («Coadă N», slice 0098) is shown only while the robot queue has work; its help is part of the
  0098 backlog, not covered here.
- Capture ids to re-shoot: none.
