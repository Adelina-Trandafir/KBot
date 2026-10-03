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
