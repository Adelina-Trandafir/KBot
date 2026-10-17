# SLICE-00EF-39 - Received invoices: the classifications of the lines move into «Leaga de receptie» (17.10.2026)

## What changed and why
Operator request: the logic of the window «Clasificatii pe linii» (`KeClasificariForm`) moves into the window opened by «Leaga de receptie…» (`KeLegaForm`), and the old window and its button are removed from the «Receptii» view of a received invoice (the one view is hosted by the «Cumparare» form `PrimiteForm` and by the E-Factura view of `KbotMain`, so one button was removed from both).

`KeLegaForm` now has, under the angajament row, a grid of the invoice lines (Nr, Denumire, Valoare cu TVA, **Clasificatie** as a combo). Everything the old window did is there:
- the choices are the INDICATORS of the angajament chosen (a classification a line already has that is not an indicator stays visible, marked «nu e indicator al angajamentului»); a choice from another angajament's list goes back to «(nealeasa)» when the angajament changes; only a value from the list is accepted in a cell; «Ca la prima linie» copies the first line's choice onto all lines; before any angajament is opened a line keeps the classification it has.
- NEW in the window (it replaces what the server used to answer from the saved classifications): the «De alocat» grid is computed locally from the choices: for each chosen classification, the sum of its lines' values WITH VAT, minus what existing links allocate (`EFacturaKeStare.Legaturi`), is the rest. It is recomputed on every change of a classification. The three modes (existing, new, modified reception) all use it.
- The window still only COLLECTS: on OK it hands back `Clasificari` (every line with its choice) and `ClasificariSchimbate`. `PrimiteView.LinkAsync` saves them first (`SetKeClasificatiiAsync`, only if something changed, then `ApplyKe` with the state the server answers), and only then runs the server's check and the rest of the link, because the server's check reads the classification from the saved lines.
- Removed: `KeClasificariForm` (+ designer), `PrimiteReceptiiPage.btnClasificatii` and its event `ClassificationsRequested`, `PrimiteView.PgReceptii_ClassificationsRequested`. The four remaining buttons of the page share the width. The note under the summary now says the classification is chosen at «Leaga de receptie».

Behaviour change worth knowing: the classifications are saved when the accountant presses the button of the link window (Leaga / Creeaza / Modifica), not on a separate «Salveaza»; if the server's check then refuses the link, the classifications stay saved. Cancelling the window saves nothing.

## Files touched
`src/KBot.EFactura/Forms/KeLegaForm.vb`, `KeLegaForm.Designer.vb` (grid `gridLinii`, `btnToate`, `lblSep2`; the window is 120 px taller), `src/KBot.EFactura/Views/Primite/PrimiteView.Receptii.vb`, `PrimiteReceptiiPage.vb`, `PrimiteReceptiiPage.Designer.vb`, `src/KBot.EFactura/KBot.EFactura.vbproj` (FileVersion 1.2.0.0 -> 1.3.0.0). Deleted: `Forms/KeClasificariForm.vb`, `Forms/KeClasificariForm.Designer.vb`. Help: see SLICE-0000-95.

## Test results
`dotnet build src/KBot.EFactura` and `dotnet build src/KBot.App`: 0 warnings, 0 errors. Nothing run, nothing seen on screen; no test (operator's rule).

## Unverified / deferred
- The new grid and the 120 px of extra height were never seen; the designer coordinates are written by hand and the dock order (Top, Top, Top, Top, Fill) is what makes the layout, so open the window once in the designer / on screen.
- `KBotDataView` combo column + `CellValueChanged` on a combo commit: the event is assumed to fire with `ColumnKey = "clsf"` when a combo value is committed (the old window never listened to it); if it does not, the allocation grid only follows after «Ca la prima linie» or a new angajament.
- The old plan (KE02) said the classification is chosen automatically when the angajament has a single indicator; the old window never did that and this one does not either.
- `docs/worklog/SLICE-KE02-*.md` and `SLICE-KE03-*.md` still describe the old window (history, left as written).
