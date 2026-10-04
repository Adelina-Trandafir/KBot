# SLICE-000T-03 — the visual designer and the on-screen picker

Third sub-slice of 000T (plan: `~/.claude/plans/i-need-a-tutorial-humble-pretzel.md`). The operator wants to
design tutorials visually while the app is loaded. This sub-slice is the designer window, the picker
(«Alege pe ecran») and saving; the click-through Recorder is 000T-04.

## What changed and why

- `src/KBot.App/Tutorial/TutorialDesignerForm.vb` + `.Designer.vb` (every control declared in the designer file,
  `KBotThemedForm`, card + caption bar like the other dialogs): a combo of the saved tutorials («(tutorial nou)»
  first), a property grid for the tutorial header (`id`, `title`, `part`, `keywords`, `starts`, `host-key`), the list of
  steps with «+ Pas / Șterge / ▲ / ▼ / Copie», a property grid for the step being edited (target, part, anchor, wait,
  when, optional, merge, why, dim, allow) and a text box for the bubble text, «Alege pe ecran...», «Salvează»,
  «Testează de la pasul», «Închide». Unsaved changes are asked about before switching tutorial or closing.
  Tooltips are `KBotToolTip` (Romanian). Opened from a new row «Designer tutoriale» of the MENIU menu, which follows
  the capture mode exactly (`KbotForm.HelpCaptureModeOn`: advanced options + «Mod capturi pentru ajutor»).
- `src/KBot.App/Tutorial/TutorialPicker.vb`: «Alege pe ecran». The designer hides itself; a see-through layer over the
  whole screen catches the mouse; the control or painted part under it gets the tour ring (`HelpTourFrame`) and a small
  label (target / part / the wait that fits). Left click takes it, Esc or right click gives up. Only K-BOT's own windows
  are read. Resolution: z-order from `HelpCaptureNative.TopLevelWindows`, then `GetChildAtPoint` down to the nearest
  NAMED control, `HelpService.ScreenKeys` for `Type.control`, parts by `IKBotHelpParts.HelpPartBounds` with the same
  names the checker allows (tree row button through the new `RightIconRectsOnScreen`, nav item through the new
  `KBotNavList.ItemKeyAt`). A control with no name is refused with an explanation.
- `src/KBot.App/Tutorial/TutorialFlow.vb`: `ToMarkdown()` writes a tutorial in exactly the shape the parser reads
  (keys at their default are left out; bullets back to «- »). Property-grid attributes (`Browsable(False)` on `Steps`,
  `SourcePath`, `Text`).
- `src/KBot.App/Tutorial/TutorialStore.vb`: saves to `<AppDir>\Help\tutorials\<id>.md` and, on a repo build, to
  `src\KBot.App\HelpContent\tutorials\` too (the capture tool's folder search, `HelpCaptureStore.FindSourceFolder`, now
  `Friend`); it refuses what the parser would refuse; UTF-8 without BOM; the help is reloaded after saving.
- `src/KBot.App/Tutorial/TutorialRunner.vb`: `Start(..., k_startIndex)` — «Testează de la pasul» runs the tutorial as it
  is now (unsaved, read back through the real parser) from the chosen step; the designer minimizes and comes back.
- `src/KBot.Controls/NavList/KBotNavList.HelpParts.vb`: `ItemKeyAt(point)`.
- `tools/HelpCheck/Check-Help.ps1`: the three tutorial windows are ignored by the «no topic» list (operator tool).
- `tests/KBot.App.Tests/TutorialFlowTests.vb`: `ToMarkdown` parses back to the same tutorial (round trip).

## Files touched

`src/KBot.App/Tutorial/{TutorialDesignerForm,TutorialDesignerForm.Designer,TutorialPicker,TutorialStore}.vb` (new),
`src/KBot.App/Tutorial/{TutorialFlow,TutorialRunner}.vb`, `src/KBot.App/KbotForm.{Designer,HelpCapture,Nomenclatoare,Tutorial}.vb`,
`src/KBot.App/Help/HelpCaptureStore.vb`, `src/KBot.Controls/NavList/KBotNavList.HelpParts.vb`,
`tools/HelpCheck/Check-Help.ps1`, `tests/KBot.App.Tests/TutorialFlowTests.vb`, status files.

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors** (a `Move` method that shadowed `Control.Move`
  was renamed `MoveStep`).
- Tests: still run from the throw-away project because `tests\KBot.App.Tests` does not compile (old errors, see
  000T-01): **24 passed, 0 failed** after the round-trip test was added.
- `Check-Help.ps1 -Coverage`: **«No errors.»**; the «no topic» list is the same six windows as before.
- **The designer window and the picker were never opened.**

## Left unverified or deferred

- **Nothing seen on screen**: the layout of the designer (docking order of the panels, sizes at 100 / 125 / 150 %),
  the property grids under the themes, the picker's ring and label, the hit-testing of painted parts, the menu row.
- The picker cannot pick something that only exists while a menu / drop-down is open (the layer takes the mouse):
  that is what the Recorder (000T-04) is for.
- The picker writes `target` / `part` / the guessed `wait`; the `anchor:` (rows that depend on data) is typed by hand.
- No tutorial of the designer is in the help (operator tool, like the capture mode); NOUTATI.md not touched.
