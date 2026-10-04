# SLICE-000T-04 — the Recorder

Fourth sub-slice of 000T. The operator clicks through the real flow once and the designer writes the steps. This
covers what the picker cannot reach (a drop-down or a menu that is only open while it is used) and is faster than
picking step by step. («Testează de la pasul N» was already done in 000T-03, together with the designer.)

## What changed and why

- `src/KBot.App/Tutorial/TutorialRecorder.vb`: while it runs (an `IMessageFilter` + a 300 ms timer) it turns into steps
  - a mouse press on a K-BOT control that has something a tutorial can wait for (button -> `click`, tree row -> `select`,
    the row button of a tree -> `click` with `part: node.icon`, nav item -> `tab:<key>` with `part: item:<key>`, tick
    box -> `checked`); a label or a panel is ignored;
  - the FIRST key typed into an input (`changed`), once per input; the typed value is never read or kept;
  - every window that opens and is a real window (modal or in the taskbar; pop-up lists and menus are not) -> a step
    `wait: opens:<Type>` with no target.
  Titles are working titles in the words of the screen («Apasă «Salvează documentul»», «Fila «rezervari»»,
  «Se deschide DdfEditForm»); texts, `optional`, `when` and `why` are filled in by the operator afterwards. The same press
  twice in a row is one step. The tutorial windows, the designer and the recorder's own bar are never recorded.
- `src/KBot.App/Tutorial/TutorialRecorderBar.vb` + `.Designer.vb`: the small bar (bottom right, top-most, takes no focus):
  «Înregistrez: N pași...» and «Oprește». All controls in the designer file, `KBotToolTip`.
- `src/KBot.App/Tutorial/TutorialPicker.vb`: the resolution was split into shared functions (`ResolveAt`, `NearestNamed`,
  `ForControl`) so the recorder and the picker give the same answer for the same control; `TutorialPick.Control`.
- `src/KBot.App/Tutorial/TutorialDesignerForm.vb` + `.Designer.vb`: the button «Înregistrează»: the designer minimizes,
  «Oprește» brings the steps back after the chosen step.
- `tools/HelpCheck/Check-Help.ps1`: `TutorialRecorderBar` joins the operator-tool windows of the ignore list.

## Files touched

`src/KBot.App/Tutorial/{TutorialRecorder,TutorialRecorderBar,TutorialRecorderBar.Designer}.vb` (new),
`src/KBot.App/Tutorial/{TutorialPicker,TutorialDesignerForm,TutorialDesignerForm.Designer}.vb`,
`tools/HelpCheck/Check-Help.ps1`, status files.

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `Check-Help.ps1 -Coverage`: **«No errors.»**, same six uncovered windows as before (none from this slice).
- No test was written for the recorder: it only exists against live windows and the message loop (the operator's rule is
  no windows on screen without a heads-up). **It was never run.**

## Left unverified or deferred

- **Never run**: the bar, the message filter, what a real press resolves to (the filter sees the press BEFORE the control
  handles it, so a click that rebuilds the tree resolves against the old rows — meant, but unchecked), the «opens» heuristic
  (`Modal OrElse ShowInTaskbar`), and the focus behaviour of the no-activate bar.
- Closing a window is not recorded (a `closes` step is added by hand); keyboard actions other than the first key in an
  input (Enter, Tab, shortcuts) are not recorded.
- A press inside a pop-up that is not a K-BOT window (the Windows file chooser) is not seen at all, so «Atașează fișier»
  records the button press but not the choosing.
