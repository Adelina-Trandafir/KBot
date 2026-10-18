# SLICE-0115-02 - Side panel behind the pages, switch in Setari (18.10.2026)

Operator's request of 18.10.2026: an application setting that keeps the sidebar of the main window either visible or behind
the forms; default behind.

## What changed and why
- New setting `AppSettings.SidebarBehindPages` (default `True`), with its DTO field, `ToDto` and `FromDto` lines.
- `AvacontMain.ApplySidebarPlacement` (in `AvacontMain.Sidebar.vb`): the panel is hidden only while a macheta is on screen (a page,
  or the shell = the list of angajamente: `_page` set or `_baseIsShell`) AND the setting is on; `pnlHost` (Dock.Fill) then takes
  the whole width. With no macheta on screen (the opening page) it is shown; with the setting off it is shown all the time. Called
  from the constructor, `UpdateCloseButton` (every page open / put away / close and every base switch already calls it) and
  `AppSettings_Changed`. History: v1 (SidebarAlwaysVisible, panel kept on the opening page and the shell) and v2 (the switch
  everywhere alike) both misread the request; the rule above is the one the operator gave on 18.10.2026.
- Behind the machete the fold button («Ascunde meniul lateral») is not shown: new `KBotSidePanel.CollapseButtonVisible` (default True; hides the button and its band, no hit area, no tooltip). `ApplySidebarPlacement` sets it to `Not SidebarBehindPages` and unfolds the panel meanwhile (`Collapsed = False`) so a panel saved as folded cannot be stuck without its button; `_applyingPlacement` keeps `Sidebar_CollapsedChanged` from saving that, and the saved `HomeSidebarCollapsed` comes back when the setting is turned off.
- `SetariAplicatieView`: new tick `chkSidebarInSpate` («Meniul lateral stă în spatele machetelor», checked = behind) in the «Fereastra principala» group (Designer row 7, the rows below
  shifted by one, `RowCount` 17), saved by `SalveazaComutator`, loaded in `IncarcaComutatoarele`.

## Files touched
`src/KBot.Common/AppSettings.vb`; `src/KBot.Controls/SidePanel/KBotSidePanel.vb`; `src/KBot.App/AvacontMain.vb`, `AvacontMain.Sidebar.vb`;
`src/KBot.App/Setari/SetariAplicatieView.vb`, `SetariAplicatieView.Designer.vb`; help in SLICE-0000-106.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. No tests written or run, nothing run on screen (project rule).

## Unverified / deferred
- Never seen on screen: that open pages (Adobe-hosting ones included) re-fit correctly when the panel appears / disappears
  under them, and the look of the new row in Setari.
- The panel keeps following its events while hidden; when the tick is turned on it is refreshed (`RefreshSidebar`).
