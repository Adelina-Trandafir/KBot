# SLICE-0114-24 - Ribbon: compact mode and a temporary panel while it is folded (18.10.2026)

## What changed and why
Operator, two points. On low resolutions the ribbon takes too much of the screen; and a folded ribbon gave no way to use a button without unfolding it for good.

1. **Compact ribbon.** New setting `AppSettings.RibbonCompact` (default False) and a tick in Setari > Aplicatie > group «Fereastra principala» (`chkRibbonCompact`, declared in `SetariAplicatieView.Designer.vb`; the rows below it moved down one, `RowCount` 16). `KBotRibbon.Compact` (new property) swaps the measures: tab strip 28 -> 22, large icon 32 -> 24, small icon 16 -> 14, small row 22 -> 18, group caption 18 -> 14 (logical px, still scaled with the DPI), and the text uses `BodyFont()` = the control's font x 0.88 (min 6.5 pt); the caption / bold / right-text fonts derive from it. The input fields of the ribbon (combo, text box) take the same smaller font, so their height follows. `AvacontMain` sets `ribbon.Compact` at start and again on `AppSettings.Changed`, so the tick works at once, no restart.
2. **Fly-out while folded.** A single click on a tab of a folded ribbon opens that tab's panel as a temporary overlay (`KBotRibbonFlyout`, new file): a child of the form, brought to front, outside the docking, so the open page does not move. The panel is laid out by the ribbon itself in the fly-out's own coordinates (panel top 0 while folded), shows the groups of the tab plus the form group and the view group (they were not reachable at all while folded), and the fields stand in it while it is open (reparented, and put back on close). It closes: after a button was used (closed BEFORE `ItemClicked` is raised, so a window the command opens is not under it), on a press anywhere else in the form (`OutsideClickFilter`, a message filter that never eats the press; a press in the list window of a combo is ignored; a press on the tab strip is left to the ribbon), on **Esc** (eaten only when it closed the fly-out and no combo list is open), when the form is deactivated / moved / resized, when the ribbon is hidden or resized. Clicking the open tab again closes it; clicking another tab shows that tab. **Double click** on a tab, or the chevron, unfolds for good (the old «a click on a tab unfolds» is gone: it is the fly-out now).

## Files touched
`src/KBot.Controls/Ribbon/KBotRibbon.vb`, new `src/KBot.Controls/Ribbon/KBotRibbonFlyout.vb`; `src/KBot.Common/AppSettings.vb`; `src/KBot.App/AvacontMain.vb`; `src/KBot.App/Setari/SetariAplicatieView.vb`, `.Designer.vb`. Help `contabil/ribbon.md`, `contabil/setari.md` (0000-100).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. No tests run, nothing run or seen on screen.

## Unverified / deferred
- Never run: the fly-out position (just under the folded ribbon), that the page below really does not move, the look of the compact ribbon at 100 / 125 / 150 %, the height of the fields in compact (they use the smaller font; their own padding may still make them taller than a small row), the fly-out over a hosted Adobe / Excel window (a press inside a foreign window never reaches the message filter, so the fly-out then stays until another click / Esc / the form loses focus).
- The panel height (`PanelPx`) does not include the fields (as before 0114-24); in compact a field row is the height its smaller font asks for.
- Captures: those showing the ribbon were not marked for redo (the normal size is unchanged).
