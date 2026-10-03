# SLICE-0107 — tree control: a right click does not select a row

Operator request, 03.10.2026 (follow-up of 0106, item 3): a row that is not selected must not
become selected by a right click alone; the operator left clicks first.

## What changed and why

- `AdvancedTreeControl` gets a property **`RightClickSelects`** (default `True` = behaviour as
  before; `[Category("K-BOT")]`, `[DefaultValue(True)]`, so a freshly dropped tree writes no
  designer line). With it `False`:
  - a right press on a row that is NOT selected (or on empty space) is refused whole in
    `OnMouseDown`: no selection change, no expand/collapse, no inspector, no `NodeMouseDown`,
    no drag arming; the matching release is swallowed in `OnMouseUp` (`_rightPressIgnored`,
    same pattern as `_pressWhileLocked`), so no `NodeMouseUp` either;
  - a right press on the selected row (or any row of the selected group, `IsRowSelected`) goes
    the old way and raises the events, so the context menu opens.
- `DdfView` and `OrdView` set `tree.RightClickSelects = False` in their `.Designer.vb`. Together
  with 0106 (right click only opens the menu, never reloads the PDF) the result is: left click
  selects and loads; right click works only on the already selected row and only opens the menu.
- `AsociereForm` (both trees, `treeLant` and `treeLibere`) also set it `False` (operator, same day; its menu reads the node it is given, and multi-select keeps working: a right click on a row of the selected group still opens the group menu). Other trees (`NoteCabView`, the main tree, popups) are NOT changed: the
  default keeps them as they were. To apply the same rule to one of them, set the property in its
  designer. `RaiseLeftClickOnRightClick` (an old unused public field) was left alone.
- Help (slice 0000-43): the DDF and ORD topics and tour steps that say «clic dreapta» now say to
  left click the row first.

## Files touched

- `src/KBot.Controls/Tree/AdvancedTreeControl.Properties.vb` - property + flag
- `src/KBot.Controls/Tree/AdvancedTreeControl.Overrides.vb` - `OnMouseDown`, `OnMouseUp`
- `src/KBot.App/Views/DdfView.Designer.vb`, `src/KBot.App/Views/OrdView.Designer.vb`, `src/KBot.App/Forexe/AsociereForm.Designer.vb`
- Help: see `SLICE-0000-43-ajutor-clic-dreapta-ddf-ord.md`

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on
screen (operator rule: no test runs / UI driving unless asked).

## Left unverified / deferred

- Not seen: right click on an unselected row (nothing happens, selection stays), right click on
  the selected row (menu opens, PDF stays), left click (selects, loads PDF), right click on
  empty space of the tree (now does nothing; before it cleared the selection).
- With a multi-row selection (`MultiSelect`) a right click on any selected row keeps the group;
  not exercised, DDF/ORD do not use multi-select.
- ORD: «Adaugă ordonanțare…» / «Generare în lot…» now need a selected row to right click on
  (they were reachable from any row before); the «Adaugă» icon in the tree footer is unchanged.

## Also under slice 0107 (03.10.2026): «Clasificatii bugetare», aligned grids and a final row

Same slice, operator request: the two grids of `ClasificatiiForm` keep their columns on the same
vertical lines, and a row under both adds the last budget and all the corrections.

- Columns: every stretching column (`Clsf` of the budget grid, `Nr. doc.`, the `Clsf` columns) is authored at
  150 (MinWidth 150); the other columns are 110 in every grid. Equal authored totals (leaf 844, node 700) make
  the grids shrink or stretch the same way. Before, the budget grid's `Clsf` was authored at 200 and shrank every
  column of that grid on a narrow panel, while the corrections grid did not.
- Leaf: the budget grid shows `Clsf` first (`Clsf | Inceput | Trim 1-4 | Total | x` over `Nr. doc. | Data | ...`).
- `KBotDataView`: `VScrollShown`, event `VScrollShownChanged` (raised after the layout pass), property
  `ReserveVScrollSpace`. `ClasificatiiForm.Grid_VScrollShownChanged` reserves the bar's strip on all three grids
  only while one of the two upper grids has its bar on.
- `gridTotal` (one row, no header / footer, read only, text «Buget + rectificari»): newest-dated budget +
  all corrections per quarter and Total; for a node, the last budget of every classification under it +
  their corrections totals. `UpdateTotalRow` follows every edit, new row and delete.
- Files: `KBotDataView.Layout.vb`, `KBotDataView.AutoSize.vb`, `ClasificatiiForm.Designer.vb`,
  `ClasificatiiForm.vb`; help `clasificatii.md`, `tur-clasificatii.md` (0000-46).
- `gridTotal` is not selectable (new `KBotDataView.Selectable`, default True) and its «Inceput» cell stays empty.
- New check box `chkForexe` («Arata DOAR clasificatiile folosite in FOREXE», right of «Arata toate clasificatiile», both in `tlyBife`): the tree shows only the classifications that have a row in `FX_Indicatori_Buget` (by `IdClsf`); it wins over the movement filter. Server: `GET .../clasificatii` items carry `in_forexe` (`EXISTS` on `FX_Indicatori_Buget`) - **server not deployed**; `Clasificatie.InForexe`. - «Verifica bugetul»: double click on a row closes the window and `ClasificatiiForm.GoToClassification` selects that leaf and loads it (same path as a tree click, so unsaved changes are asked about); a leaf hidden by the filters makes the tree show all classifications first. `BudgetCheckForm.RunAsync(..., onPick)`; only when opened from this window (the automatic check after a download passes no `onPick`, double click does nothing there). Row id kept in `KBotDataRow.Tag`. Not seen on screen.
- «Verifica bugetul» (`check_budget`, also the automatic check after a download): only classifications with a row in `FX_Indicatori_Buget` are checked; ones with K-BOT values but no FOREXE credit are left out (operator, 03.10.2026). The check box uses the same test. Both stay empty until a download with slice 0108 has filled `FX_Indicatori_Buget`.
- Build `KBot.App`: 0 warnings, 0 errors. Not seen on screen: the alignment at the operator's DPI with and without
  the bar, the strip appearing on the other grids when one starts to scroll, the height of the last row.
