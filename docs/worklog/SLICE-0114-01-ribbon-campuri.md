# SLICE-0114-01 - Ribbon: fields (combo / text box) in a group, conditional buttons, order (15.10.2026)

Sub-slice of 0114 (the slice that made `KBotRibbon`), asked by the operator: changes to the menu go in the slice that made it.
The E-Factura side of the same request is `SLICE-00EF-30-primite-sincronizare-din-ribbon.md`.

## What changed and why
1. **New: `KBotRibbonField`** (`KBot.Controls/Ribbon/KBotRibbonItems.vb`): a small input in a group, authored in the designer
   (Group -> Fields). Two kinds: **Combo** (KBotComboBox, `Editable = False`: pick only, no typing) and **Text** (KBotTextBox:
   `PlaceholderText`, `MaxLength`, `DigitsOnly`). Own `Key`, `Width` (logical, 150), `Visible`, `Enabled`, tooltip. The value
   is read from code (`Text`, `SelectedIndex`); there is no event. `KBotRibbonFieldCollection` makes / drops the real control.
2. **`KBotRibbon`**: `AttachField` / `DetachField` (the control is a child of the ribbon, not made at design time),
   layout = one column after the buttons of the group, a row each (three at most), `ApplyFieldControls` places and shows /
   hides them with the layout (hidden tab, folded ribbon, hidden field); a group with only a visible field is shown;
   `FindField`; `ValidateKeys` also checks the field keys; the tooltip component and the theme reach the field controls.
   The class summary no longer says «no child controls».
3. **E-Factura tab** (`AvacontMain.Designer.vb`): «Cumparare» now stands BEFORE «Vanzare»; the button «Sincronizeaza facturi primite»
   and its two fields start hidden (`Visible = False`) and `AvacontMain.UpdatePrimiteSyncControls` shows them only while the
   page on screen is the received-invoices window (`PrimiteForm`); called from `UpdateCloseButton`, i.e. on every page change.

## Files touched
`src/KBot.Controls/Ribbon/KBotRibbonItems.vb`, `KBotRibbon.vb`, `src/KBot.App/AvacontMain.Designer.vb`, `AvacontMain.vb`.

## Test results
`dotnet build src/KBot.App` (builds Controls and EFactura): 0 warnings, 0 errors. No tests run (operator rule). Not seen on screen.

## Unverified / deferred
- The look of the fields in the panel (row height 22 px logical, 20 px control) and whether the KBot combo / text box keep their
  themed look inside the ribbon in the three themes; the first paint of a field after the page opens.
- `KBotRibbon.md` / `CONTROLS.md` still unwritten (open thread of 0114).
- Fields are not hit by the ribbon's own tooltip (the KBotToolTip component is applied to the controls, through `ToolTipHeader` / `ToolTipText`).

## Follow-up (15.10.2026, after the first look)
The fields overlapped: the KBot combo forces its own height (`PreferredHeight`, taller than a 22 px small-button row), so the text box
under it was covered. `KBotRibbon` now takes the tallest control height as the row height (`FieldHeight`), stacks the rows with a 3 px
gap and centres the block above the group caption. Build KBot.App: 0 warnings, 0 errors; not seen on screen (look at 100% and 150%).
