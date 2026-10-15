# SLICE-00EF-30 - Received invoices: sync from the ribbon, no dialog; view polish (15.10.2026)

E-Factura side of the request; the ribbon side (fields, order, conditional visibility) is `SLICE-0114-01-ribbon-campuri.md`.

## What changed and why
1. **Tooltips off the lines grid**: `PrimiteLiniiPage` no longer sets tooltips on the grid (nor on the supplier block / notes box, whose
   «Furnizorul» tip covered the notes).
2. **Bar of views as high as the header of the tree**: `navDetaliu` 40 -> 30 (= `tree.HeaderHeight`).
3. **Gap bar / tree = gap grid / tree**: the bar sits in `pnlNav` with a 3 px side padding, which is what the grids have
   (`Padding(3)` of the pages), so the bar starts where the grids do.
4. **«Sincronizeaza cu ANAF» button of the page removed** (`PrimiteForm`: control, handler, designer). The old dialog
   `SincronizarePrimiteForm` is no longer opened by anything: also the item «Sincronizeaza facturi primite» of the old title-bar
   menu of `FacturiForm` is gone (the class files stay, unused).
5. **Sync without a dialog**: `PrimiteForm.SyncFromAnafAsync(days)` (same batch loop as the dialog: 20 per call until nothing is
   left or a batch added nothing); progress and the result on the bottom line of the page (`lblStare`); the list is read again when
   something was added; unreadable messages are told in one warning box; no box on success. `KbotForm.SyncPrimite(days)` calls it
   (the old `OpenSincronizarePrimite` and the `ef_primite_sync` case of `RunEFacturaCommand` are gone).
6. **Days**: `AvacontMain.ReadSyncDays`: the text box when filled (1-60, anything else = a warning and no sync), else the combo
   (7, 15, 30, 45, 60; 7 by default).

## Files touched
`src/KBot.EFactura/Forms/PrimiteForm.vb`, `PrimiteForm.Designer.vb`, `src/KBot.EFactura/FacturiForm.Designer.vb`,
`FacturiForm.Furnizor.vb`, `src/KBot.EFactura/Views/Primite/PrimiteView.Designer.vb`, `PrimiteLiniiPage.Designer.vb`,
`src/KBot.App/KbotForm.EFactura.vb`, `AvacontMain.vb`; help: `contabil/efactura/index.md`, `contabil/ribbon.md` (0000-76).

## Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. No tests run. Nothing run or seen on screen; no real sync against ANAF.

## Unverified / deferred
- Nothing was run: the sync from the ribbon, the progress line and the 30 px bar are not seen.
- While a sync runs the ribbon button can be pressed again: `_busy` ignores the second press without any message.
- If the operator leaves the page during a sync the loop goes on (it stops by itself when the form is disposed after the next call).
- The page of the received invoices must be the one on screen for the button to exist; the DDF view has no sync button.
