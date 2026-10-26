# SLICE-00EF-50 - Sales invoices: spare row, duplicate/delete cells, buttons with text (26.10.2026, operator request)

## What changed and why
Window `FacturiForm` (sales invoices), view «Continut»:
1. **Spare row.** While an invoice is typed or modified (`EditMode.Creating` / `Editing`) the lines grid ends with one empty row (Tag `SpareTag`; a real new line has Tag Nothing, so Nothing could not mark it). Typing in any of its cells (empty text does not count) turns it into a line - number after the highest, unit XPP, quantity 1, price 0, value computed, for the cells not typed - and a fresh spare row appears below. `ApplySpareLine` (called from `ApplyMode`) adds/removes it, so a shown invoice has none. `ReadLines` / `NextLineNumber` skip it; «at least one line» counts real lines only. The empty «Continut» cell shows a dim hint.
2. **«Linie noua» removed**, with its section (`tlyLinii`, `btnLinieNoua`, handler, theme line, tutorial step). The «Continut» tutorial now points at `gridLinii`.
3. **Ribbon buttons with icon AND text**: `btnRenunta` «Renunta», `btnModifica` «Modifica», `btnAdauga` «Adauga», `btnSalveaza` «Salveaza» (and `btnIesire`, which had text over the icon): `AutoSize` + `GrowOnly`, `TextImageRelation.ImageBeforeText`, padding 14,4,14,4, `MaximumSize` height 50 (grow sideways only, never taller).
4. **Delete cell** `sterge`: `ButtonMargin` 0 and `ButtonBorders = None` = a square face filling the whole cell (the cell IS the button).
5. **New cell `duplica`** (glyph «❐») left of delete: copies the line to the end of the lines (above the spare row; the grid only appends, so the spare row steps aside and returns), new number, `Tag` Nothing (a new line), dirty, and puts the cursor in the copy's «Continut».
6. **KBot.Controls** (needed by the spare row): `KBotCellFormattingEventArgs.HideButton` - a Button cell draws nothing (the spare row shows no buttons). Paint only. `KBotDataView.md` updated. KBot.Controls 1.71.0.0, KBot.EFactura 1.12.0.0.

## Files touched
`KBot.EFactura/FacturiForm.Linii.vb`, `FacturiForm.vb`, `FacturiForm.Pagini.vb`, `FacturiForm.Designer.vb`, `Views/Vanzare/VanzareContinutPage.vb` + `.Designer.vb`, `KBot.EFactura.vbproj`; `KBot.Controls/DataView/Events/KBotCellFormattingEventArgs.vb`, `KBotDataView.Painting.vb`, `KBotDataView.md`, `KBot.Controls.vbproj`; `docs/release-notes/NOUTATI.md` (1.1.3.0, operator request).

## Test results
`dotnet build src/KBot.EFactura/KBot.EFactura.vbproj`: 0 errors, 0 warnings. No tests written or run (project rule).

## Unverified / deferred
Nothing seen on screen: the look of the flat delete/duplicate cells, the hidden buttons on the spare row, the ribbon button widths at the operator's DPI, the «❐» glyph (font fallback, like «✕»), and the spare-row promotion while the editor commits are by reading only. Help: 0000-129.
