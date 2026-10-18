# SLICE-KE12-03 - Tick boxes in the grid, used for the products of the reception (18.10.2026)

Operator (18.10.2026): the product rows of «Produse Factura» get a small first column without header text (as wide as a tick box) - or, better, the grid itself gets a «CheckBoxes» option
(None / Yes (All) / Yes (Rows Only)) that raises RowChecked. For receptions: clearing a line puts 0 in «Recepționată» and locks it; ticking it again unlocks it and takes the sum from «Valoare cu TVA».
The DGV is changed, so the work is a sub-slice of the last slice (KE12).

## What changed and why
- **KBotDataView** (family KBot.Controls/DataView): new property `CheckBoxes` (`KBotCheckBoxMode`: `None`, `All`, `RowsOnly`), new event `RowChecked` (`KBotRowCheckedEventArgs`: `RowIndex`, `Checked`, `Pending`),
  `CheckColumnKey`, `GetRowChecked`, `SetRowChecked`, `GetCheckedRows`. The column is added by the grid at run time (not in the designer), first, without header text, 30 px, not resizable. With `All` the header band of
  a group has a tri-state tick box (the title moves right of it); pressing it ticks / clears every enabled row of the group and raises `RowChecked` for each row that changed (`Pending` counts down so a host can work once at 0).
  Written in `KBotDataView.CheckBoxes.vb`; hooks in `KBotDataView.Input.vb` and `KBotDataView.Grouping.Painting.vb`; documented in `KBotDataView.md`.
- **KeLegaForm** (`gridLinii`): `CheckBoxes = All`; the column «Număr / Dată factură» (the group key, repeated on every row) is hidden, so the first column of the rows is the tick box. A new line is ticked.
  `GridLinii_RowChecked`: the line's «Recepționată» is recomputed (cleared -> 0; ticked -> the value of «Valoare cu TVA», cut as before to what the invoice / the reception still has; a typed sum is forgotten).
  `GridLinii_CellFormatting`: the «Recepționată» cell of a cleared line is read-only (not greyed). Cleared lines are left out of the amounts per classification (`AmountsOf`, `FillAlocari`) and of the sum sent.
  Clearing every line of a class therefore removes that class from the amounts grid.

## Files touched
`KBot.Controls/DataView/KBotCheckBoxMode.vb`, `Events/KBotRowCheckedEventArgs.vb`, `KBotDataView.CheckBoxes.vb` (new), `KBotDataView.Input.vb`, `KBotDataView.Grouping.Painting.vb`, `KBotDataView.md`;
`KBot.EFactura/Forms/KeLegaForm.vb`, `KeLegaForm.Designer.vb`; help `contabil/efactura/index.md` (0000-104).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen, no test written.

## Unverified / deferred
- Not seen: the width of the column, the tick box on the header band next to the triangle, the «some» state, a hidden group-key column (assumed that grouping does not need the column visible).
- The mode does nothing at design time (the column is added at run time only), so the designer does not show it.
- Pressing the header of the check column sorts it like any column header (not blocked).
- Keyboard: Space on the check cell ticks it (the existing rule of CheckBox cells); there is no key for the header tick box.
