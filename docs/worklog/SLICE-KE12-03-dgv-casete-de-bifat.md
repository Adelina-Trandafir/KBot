# SLICE-KE12-03 - Tick boxes in the grid, used for the products of the reception (18.10.2026)

Operator (18.10.2026): the product rows of «Produse Factura» get a small first column without header text (as wide as a tick box) - or, better, the grid itself gets a «CheckBoxes» option (None / Yes (All) / Yes (Rows Only)) that raises RowChecked. For receptions: clearing a line puts 0 in «Recepționată» and locks it; ticking it again unlocks it and takes the sum from «Valoare cu TVA». The DGV is changed, so the work is a sub-slice of the last slice (KE12).

## What changed and why
- **KBotDataView** (KBot.Controls/DataView): new `CheckBoxes` (`KBotCheckBoxMode`: `None`, `All`, `RowsOnly`), `RowChecked` event (`KBotRowCheckedEventArgs`: `RowIndex`, `Checked`, `Pending`), `CheckColumnKey`, `GetRowChecked`, `SetRowChecked`, `GetCheckedRows`. Runtime-only first column, no header text, fixed 30 px. `All`: group-band tri-state box, title to its right; ticks/clears every enabled group row, raising `RowChecked` per changed row (`Pending` counts down, allowing one host action at 0). Implementation: `KBotDataView.CheckBoxes.vb`; hooks: `KBotDataView.Input.vb`, `KBotDataView.Grouping.Painting.vb`; docs: `KBotDataView.md`.
- **the former reception window** (`gridLinii`): `CheckBoxes = All`; hides the repeated group-key column «Număr / Dată factură», leaving tick boxes first. New lines start ticked. `GridLinii_RowChecked` recomputes «Recepționată»: cleared → 0; ticked → «Valoare cu TVA», capped as before by remaining invoice/reception amounts, forgetting typed sums. `GridLinii_CellFormatting`: cleared cells read-only, not greyed. Cleared lines are excluded from classification amounts (`AmountsOf`, `FillAlocari`) and the sent sum; clearing a whole class removes it from the amounts grid.

## Files touched
`KBot.Controls/DataView/KBotCheckBoxMode.vb`, `Events/KBotRowCheckedEventArgs.vb`, `KBotDataView.CheckBoxes.vb` (new), `KBotDataView.Input.vb`, `KBotDataView.Grouping.Painting.vb`, `KBotDataView.md`; `KBot.EFactura/Forms/the former reception window`, the former reception window; help `contabil/efactura/index.md` (0000-104).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen, no test written.

## Unverified / deferred
- Not seen: the width of the column, the tick box on the header band next to the triangle, the «some» state, a hidden group-key column (assumed that grouping does not need the column visible).
- The mode does nothing at design time (the column is added at run time only), so the designer does not show it.
- Pressing the header of the check column sorts it like any column header (not blocked).
- Keyboard: Space on the check cell ticks it (the existing rule of CheckBox cells); there is no key for the header tick box.
