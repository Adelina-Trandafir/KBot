# SLICE-00EF-49 - «Trimite la Vercon»: one account for all the lines, one supplier at a time (23.10.2026)

Operator's request: next to the note box, a combo that shows the gestiuni (as in the grid) and puts the SAME account on every row of the grid, however many invoices are ticked; and NOT several suppliers ticked at once.

## What changed and why
- **Combo `cmbCont`** (KBotComboBox, not editable, placeholder «Cont pentru toate liniile») on the note row of `VerconForm`, right of `txtNota` (a small gap panel `pnlGapNota` between). Same items as the column «Cont Vercon» (`_items`: «(fără cont)», then the accounts under their gestiune), filled in `BuildChoices`. Picking an item writes it on ALL rows of the grid (`CmbCont_SelectedIndexChanged`), then the summary line is redone. Lines added later (another invoice ticked) that have no choice yet take the account held in the combo (`ComboAccount`); a line the operator already chose keeps its choice. The combo is disabled while busy / without the Contabil role.
- To make room, the label `lblNota` reads «Notă» (was «Notă pentru Administrator»; the tooltip of the box still says it is for the Administrator).
- **One supplier at a time:** `KeepOneSupplier` runs on every tick; if ticks of two suppliers exist, the newly ticked supplier stays and the others are unticked (tree's own propagation), and a notice says so (`_supplierNote`, shown by `FillGrid`). The same supplier under several months is fine (the key is the tax code). **Months no longer have a tick** (`HasCheckBox` off): a month would tick several suppliers. In the grouping by supplier nothing else changes.
- Text of the page's intro line updated. KBot.EFactura FileVersion 1.10.0.0 -> 1.11.0.0.
- **Help** 0000-122 (`contabil/efactura/index.md`, tutorial `efactura-trimite-la-vercon.md`).

## Files touched
`KBot.EFactura/Forms/VerconForm.vb`, `VerconForm.Designer.vb`, `KBot.EFactura.vbproj`; help (2 files); status.

## Test results
`dotnet build src\KBot.EFactura\KBot.EFactura.vbproj`: 0 warnings, 0 errors. No test written or run (operator's rule); nothing run on screen.

## Unverified / deferred
- **Nothing seen on screen:** the width of the note box next to the combo at the minimum window size (about 150 px of text), the placeholder, the folding groups in this combo, and a tree month with no tick.
- A tree node `HasCheckBox = False` at the top level of a tree whose children have ticks: the control's propagation to a parent without a box was not read in detail.
