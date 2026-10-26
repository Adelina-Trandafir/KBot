# SLICE-00EF-51 - Sales invoice lines: Tab walks the editable cells (26.10.2026, operator request)

## What changed and why
- From «Continut» Tab now goes to «Um», then «Cant», then «PU», and after «PU» to the first field of the next row (the spare row after the last line), like Enter. Shift+Tab steps back to the previous editable cell. Before, Tab went to the next enabled column, read-only ones included.
- `KBot.Controls`: new `KBotDataView.TabKeyMode` (`KBotTabKeyMode`: `NextColumn` default = unchanged, `NextEditableCell`); the shared walk of Enter's `NextEditableCell` mode is now `MoveToNextEditableCell`. `gridLinii.TabKeyMode = NextEditableCell` in `VanzareContinutPage.Designer.vb`. `KBotDataView.md` updated. KBot.Controls 1.72.0.0.
- Reported in the same message, NOT fixed: «negative-quantity lines are left out of the total». The total (grid footer sum and `lblTotal`) adds the `Valoare` of every line and nothing in `FacturiForm.Linii.vb` or the footer treats the sign differently, so no cause was found by reading; waiting for the exact steps (which total, where it was typed).

## Files touched
`KBot.Controls/DataView/KBotTabKeyMode.vb` (new), `KBotDataView.Editing.vb`, `KBotDataView.md`, `KBot.Controls.vbproj`; `KBot.EFactura/Views/Vanzare/VanzareContinutPage.Designer.vb`; `docs/release-notes/NOUTATI.md`; help `contabil/efactura/index.md`.

## Test results
`dotnet build src/KBot.EFactura/KBot.EFactura.vbproj`: succeeded, 0 errors. No tests run.

## Unverified / deferred
Not seen on screen: Tab out of the «Um» combo (open and closed list), Tab on the last line into the spare row. The negative-quantity total (above).
