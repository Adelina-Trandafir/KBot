# SLICE-KE14 - A resolved invoice in the page «Recepții»; linked angajamente locked (19.10.2026)

Operator (19.10.2026), page «Recepții»:
1. an angajament that has receptions associated can no longer be taken off the list of angajamente (its three-dots button hidden on those rows);
2. a single press on an already resolved invoice: on the right no «Recepție nouă» part, only the associated receptions; the products part lists the invoice's products (as when an unassociated invoice is ticked) but nothing can be changed;
3. also on that press (resolved ones only): «Recepții existente» holds ONLY the reception the invoice is associated with; under the table of receptions, for such receptions, a button «Elimină asociere» (first read as the page footer, moved under the table on 19.10.2026 at the operator's word); the right-click of the tree for associated invoices goes away.

## What changed and why
- **Linked angajamente (1)**: `AngItem.Linked` = a link (`EFacturaKeStare.Legaturi`) of one of the invoices that feed the grid has that angajament. The button is hidden for it (`GridAng_CellFormatting`) and refused (`GridAng_ButtonClick`).
  It is read from the links of the ticked invoices (or of the pressed supplier's invoice when nothing is ticked), not from `Sursa`, which a DDF with the same code can hide.
- **View of a resolved invoice (2, 3)**: a press on a locked (covered) invoice calls `BeginView` -> `EnterViewing`. The page then shows, for that invoice only and read-only (`ReadOnlyGrid` on `gridLinii` and `gridAng`: no cell opens, the tick boxes are inert):
  the products (`FillViewedLines`, the amount column holds the line value); the angajament(s) of its links (`FillViewedAngajamente`, no empty row, no button); the receptions it is linked to, one row each (`FillViewedReceptii`; the «Total» column becomes «Valoare factură» = what the
  invoice takes of that reception, «Rest» is hidden); the amounts by classification (`FillViewedAlocari`; first laid out with the classification and the value only, then -- fix of 19.10.2026, the first version left the budget columns empty -- Buget / Recepții / Disponibil / Rămas are filled as in the form of a reception, from `GetKeDisponibilAsync` asked once per angajament and reception day: `BeginViewedDispo`). The tabs, the action combo and the form of a new reception are hidden. The button `btnElimina`, docked at the bottom of the receptions panel (the footer bar is hidden meanwhile), reads «Elimină asociere» (enabled for the role «Contabil») and
  runs `BeginUnlink` (confirmation, `RemoveKeLegaturaAsync` for each link, the tree read again). The titles read «Produse Factura …» / «Recepția asociată».
  `LeaveViewing` brings the page back (tabs, form, lines of the ticks): on a press on any other row, on a tick, and after the tree is read again. Nothing of the new-reception pipeline (defaults, budget, `RefreshOk` notices) runs while viewing.
- **Right-click menu removed** from the tree (`Tree_NodeMouseUp`); the tooltips say a press shows the reception and the products.
- With ticked invoices already on the page a press on a resolved one does nothing but say to untick them first (their typed lines would be lost) -- my choice, say if you prefer otherwise.

## Files touched
`src/KBot.EFactura/Forms/KeLegaForm.vb`, `KeLegaForm.Designer.vb` (tooltip text); help `contabil/efactura/index.md` (0000-109).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen, no test written (house rule).

## Unverified / deferred
- Not seen on screen: the hidden button on linked rows, the read-only grids (the group header of `gridLinii` and the check boxes assumed inert under `ReadOnlyGrid`, read from `KBotDataView.CheckBoxes.vb` / `Editing.vb`), the footer button, the swap of the column header.
- A link whose reception is not downloaded yet shows the date «în așteptare».
- The classification column of the products shows the label the server sends with the line.
