# SLICE-03EF-09 - «Adaugă operații contabile»: every invoice gets a tick box, one supplier at a time (08.11.2026)

## What changed and why
Operator: when «Adaugă operații contabile» is pressed the tree receives tick boxes; invoices can be ticked for ONE supplier only; ticking an invoice of another supplier unticks the ones ticked for the old supplier and the view finds out.
- `ContareBox` no longer compares the supplier: while a booking is on (`_contareSupplier` set) EVERY invoice leaf has a tick box (before, only the other invoices of the supplier being booked had).
- `Tree_NodeChecked`: a tick on an invoice whose supplier differs from the one being booked → the other ticks are taken off (under `_treeLoading`, no event storm), the booking moves to the new supplier (`_contareSupplier`), the choices made for the old one are dropped (`ResetContareChoices`) and the server is asked for the calculation again; the page shows the new supplier (it drops the client code, the note, the order number and the kind of invoice typed for the old one). The rule «at least one invoice stays ticked» is unchanged.

## Files touched
`KBot.EFactura/Views/FacturiCumparare/FacturiCumparareView.Contare.vb`. Help 0000-172.

## Test results
The build of the projects is clean (KBot.Controls, KBot.EFactura: 0 warnings, 0 errors; KBot.App could not be copied into its output while K-BOT / Visual Studio had it open, but it reports no compile error). No test run, nothing seen on screen.

## Unverified / deferred
Not seen: the repaint of the tree after the old ticks are taken off (`tree.Invalidate` is called). The supplier is compared by `SupplierKey` (the normalised tax code).
