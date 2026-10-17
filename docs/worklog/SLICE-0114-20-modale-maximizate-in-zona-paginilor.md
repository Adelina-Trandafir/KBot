# SLICE-0114-20 - Modal windows maximise inside the area of the pages, not at the screen corner (operator report) (17.10.2026)

## What changed and why
A modal window (e.g. «Legaturile receptiilor») opens with `MaximumSize` = the opening size (area of the pages minus a margin). Maximised, Windows put it at the monitor's work-area corner and cut the size to that `MaximumSize`: right size, wrong place. `AvacontMain.FitModal` now sets `MaximumSize` to the whole area of the pages (`pnlHost`, under the ribbon, beside the side panel; a smaller `MaximumSize` of the form itself still wins) and `MaximizedBounds` to that rectangle relative to the monitor (`KBotThemedForm.SetMaximizedBounds`, new public method, because `Form.MaximizedBounds` is Protected). The opening size is unchanged (margin all round).

## Files touched
`src/KBot.App/AvacontMain.vb` (`FitModal`), `src/KBot.Theming/KBotThemedForm.vb` (`SetMaximizedBounds`, `HasMaximizedBounds`), `src/KBot.Theming/Interop/NativeMethods.vb` (`ApplyMinMaxInfo`), `src/KBot.Theming/KBotModalForm.vb` (new) and the Designer base of the seven modal forms.

Second round (operator: «nu pare ca s-a schimbat NIMIC», DDF editor): the editors (DDF, ORD, chart, groups...) are `KBotShellForm`, whose `WM_GETMINMAXINFO` handler replaced the maximised place with the monitor work area after WinForms had written `MaximizedBounds`. `ApplyMinMaxInfo` now leaves the message alone when the form has maximised bounds. The first round only fixed the windows based on `KBotThemedForm` (e.g. «Legaturile receptiilor»). 

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: the maximised position on the main monitor and on a second monitor (`ptMaxPosition` is taken as relative to the monitor's corner, as `NativeMethods.ApplyMinMaxInfo` already does).
- A modal window dragged to another monitor keeps the bounds of the monitor of the main window.
- The operator wrote «fara right-panel»; read as the area of the pages (`pnlHost`), which already leaves out the side panel.
- A modal window can now also be dragged larger than the opening size, up to the area of the pages.

## Third round: one type for every modal window (operator: «TOATE machetele popup modal TREBUIE sa fie acelasi TIP»)
New `KBot.Theming/KBotModalForm` (inherits `KBotShellForm`): the base of every window opened through `AvacontMain.ShowModalAsync`. Moved onto it: `DdfEditForm`, `OrdEditForm`, `GrupeForm`, `GraficRezervariForm` (were `KBotShellForm`) and `AsociereForm`, `CabNoteForm`, `DevHarness.MessageCatalogForm` (were `KBotThemedForm`, so they had no resize band and no maximise handler). `ShowModalAsync` / `ShowModal` / `FitModal` now take a `KBotModalForm`, so a modal window of another type no longer compiles. Whole solution builds: 0 warnings, 0 errors. Not seen on screen: the resize band now present on «Legaturile receptiilor» and the CAB notes window.
