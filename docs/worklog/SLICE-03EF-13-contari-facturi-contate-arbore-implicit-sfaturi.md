# SLICE-03EF-13 - «Contari» shows booked invoices; the tree starts by supplier without operations; CPV / unit tooltips; page bottom (08.11.2026)

## What changed and why
Operator (08.11.2026), received invoices window:
1. **A click on an invoice that already has accounting operations** shows them in the page «Contari»: the classification and the templates they were made from are chosen (selected) in the lists «Documente evidentiere» / «Documente plata», the evidence operations are written in the evidence grid (note, document, date, kind, booked value; the total paid on the first row) and the payment operations in the payment grid (note, date, order number, explanation, length, value). The page is read only while it shows them (`ReadOnlyGrid`, clicks on the template lists put the chosen row back, «Salveaza» dimmed). An invoice without operations leaves the page empty. Nothing is shown while a booking is in progress (ticks).
   - Server: new `GET /api/efactura/contari/existente/<id>` (`primite_contari.existente`): `EF_PrimiteOperatii` -> `TZ_Oper` (K-BOT operation by `IdOper`, migrated by `IdOperatieAccess`) -> `TZ_Documente` -> `TZ_NoteC`; payment = document `Fel` P or operation `Tip` P. It also returns the templates fitting the classification (in the subunit of the note); the one used is found by `IdSablon`, for a migrated operation (no template id) by equal explanation.
   - Client: `IEFacturaApi.GetContariExistenteAsync`, `EFacturaContariExistente/Existenta`, `FacturiCumparareContarePage.ShowBooked`, `FacturiCumparareView.ShowBookedAsync` (called after an invoice is read, when «Contari» turns on, after «Renunta» and after a save).
2. **The tree starts grouped by supplier and with «Afisare facturi fara operatii in AVACONT» on** (`FacturiCumparareView.Initialize`, not in the view of an angajament). The header menu changes both.
3. **Tooltips in «Continut»**: hover over a filled CPV shows `AVACONT_COMUN.CPV.Expl`, over «Um» `AVACONT_COMUN.EF_UM.Explicatie`. The server adds `explicatii: {cpv, um}` to the invoice detail (`primite._explicatii_linii`; a CPV without its check digit is matched on its first 8 digits); the page gives the grid a `CellTooltipTableProvider`.
4. Layout (not logged as a feature): `tlyPlata` bottom margin 6 -> 0 in `FacturiCumparareContarePage.Designer.vb` (the gap under the page was that margin, left over since the footer of the page went away).

## Files touched
PYTHON: `routes/efactura/primite_contari.py`, `primite_contari_routes.py`, `primite.py`, `primite_routes.py` (doc line).
VB: `KBot.Domain/EFacturaContari.vb`, `EFacturaPrimite.vb`; `KBot.Api/IEFacturaApi.vb`, `ApiClient.EFactura.Contari.vb`, `ApiClient.EFactura.Primite.vb`; `KBot.EFactura/Views/FacturiCumparare/FacturiCumparareContarePage.vb` (+ Designer), `FacturiCumparareLiniiPage.vb`, `FacturiCumparareView.vb`, `FacturiCumparareView.Contare.vb`. Help 0000-174.

## Test results
KBot.EFactura and KBot.App build clean (0 warnings, 0 errors); the two Python files compile (`py_compile`). No test run, no server call, nothing seen on screen.

## Unverified / deferred
- **Server files changed (`primite_contari*.py`, `primite*.py`): they must reach the server by hand; until then a click on a booked invoice answers 404 (a line in the notice) and the tooltips are simply missing.**
- Assumption: payment operations are told from evidence ones by `TZ_Documente.Fel = 'P'` / `TZ_Oper.Tip = 'P'`. K-BOT does not save payments yet, so only migrated data can show a payment row; if migrated payments are not linked through `EF_PrimiteOperatii` the payment grid stays empty.
- The column «Ramasa» of the evidence grid holds the BOOKED value of an operation while the page shows what is booked (the header is the designer's).
- The tree shows the root «Toate facturile» only when grouped by month, so the list of all invoices is not reachable until «Grupare dupa luna» is chosen.

## Correction (08.11.2026, after the operator tried it)
Clicking a booked invoice loaded nothing: `ShowBookedAsync` read `NrOperatii` from `ShownInvoice()`, which returns the DETAIL read, and the detail never carries `NrOperatii` (only the list query does) - so it was always 0 and the page stayed empty. Now `ShownListed()` takes the invoice from the list (`_items`). The same bug made «Ascunde...» enabled for a booked invoice (the server would refuse); fixed with it.
