# SLICE-0114-19…24 — Corecții ale ecranelor E-Factura

Istoric 17–21.10.2026. Etapa 22 înlocuiește păstrarea filtrului „fără operații” din [0114-17](SLICE-0114-ribbon.md#slice-0114-17), numai pentru vederea angajamentului; fereastra tuturor facturilor rămâne distinctă. Direcția 7.3 este închisă de operator la 21.10; mesajul cu id-ul folosit nu înseamnă o remediere a datelor. Numărul 0114-24 exista și la ribbon; aici are ancora tematică E-Factura, fără renumerotare.

- [0114-19](#slice-0114-19)
- [0114-21](#slice-0114-21)
- [0114-22](#slice-0114-22)
- [0114-23](#slice-0114-23)
- [0114-24 — E-Factura](#slice-0114-24-efactura)

## SLICE-0114-19 - E-Factura Vanzare: the month list removed, the tree rows no longer cut (operator item 7.1, 7.2) (17.10.2026) <a id="slice-0114-19"></a>

### What changed and why
The month list above the tree of the issued invoices (`cmbLuna`, its fill, handler and filter) is removed: the tree shows every invoice of the year. The rows were cut after a few letters because the designer pinned the left text of the tree to 100 px (`tree.LeftTextWidth = 100`); that line is gone (0 = dynamic, as in the received-invoices tree), so the text takes the room between the icon and the amount. **Item 7.3 (the id used to download the invoice as ANAF draws it) was NOT changed**: the client sends only the invoice id and the server downloads with the `id_descarcare` stored on the invoice (`trimitere.descarca` -> `anaf_api.descarca`); nothing in the code picks another number, so if the stored value is not the one ANAF calls the download id (the 4xxx one) it was written wrong earlier. It needs the stored value or the error the operator sees.

### Files touched
`src/KBot.EFactura/FacturiForm.vb`, `FacturiForm.Arbore.vb`, `FacturiForm.Designer.vb`.

### Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

### Unverified / deferred
- Not seen on screen: the new buttons in the view group, the popup of the document button (opens at the mouse), the fields in the ribbon, the tree rows of the issued invoices.
- Item 7.3 CLOSED by the operator (21.10.2026). Had needed the id stored in `EF_Facturi.id_descarcare` of an invoice that fails, and the error text of the «Factura ANAF» view.

## SLICE-0114-21 - «Facturi primite» window: the empty strip at the bottom is gone (17.10.2026) <a id="slice-0114-21"></a>

### What changed and why
Operator: «de ce la factura cumparare exista spatiul ala alb in footer? nu il vreau». The window «Facturi primite» (`PrimiteForm`) had a 58 px strip at the bottom (`pnlJos`) with the «Iesire» button and a state line (`lblStare`). As a page of the main window, the «Iesire» button is hidden (the ribbon's «Inchide» closes the page), and the state line only repeated what the footer of the tree already says, so the strip was empty white space. The strip, its two controls, the `CancelButton` link, the `SyncStatus` handler and the button's click handler are removed; the tree card now reaches the bottom of the page.

### Files touched
`src/KBot.EFactura/Forms/PrimiteForm.vb`, `PrimiteForm.Designer.vb`.

**Probe:** [B2](#probe-b2).

### Unverified / deferred
- Not seen on screen. Escape no longer closes the page through the removed cancel button (the ribbon's «Inchide» does).
- The bottom padding of the card (9 px) stays.
- The «Vanzare» window (`FacturiForm`) has a similar strip with «Iesire» and a state line; not touched, the operator asked only about «Cumparare».
- `PrimiteView.SyncStatus` still raises its event (nobody listens now).

## SLICE-0114-22 - E-Factura VIEW: no options button in the tree header, no supplier level (17.10.2026) <a id="slice-0114-22"></a>

### What changed and why
Operator (second time asked for the grouping): in the VIEW of the angajament (`EFacturaView`, not the window «Facturi primite»): (1) the tree's header-right button goes; (2) the tree is «Toate facturile» / month-year / invoice, no supplier level. `PrimiteView` gets a designer property `AngajamentView` (set True in `EFacturaView.Designer.vb`): it takes the options (funnel) icon off the header (the icon the designer gave is kept and given back if the property is False again), `BuildMonthTree` puts the invoices (newest first) directly under their month, and a stray click on the header-right area does nothing. The invoice rows are built by one shared routine (`AddInvoiceNodes`) for both shapes. The window «Facturi primite» is unchanged (supplier level, options menu). With the button gone, the view has no «fara operatii» filter any more (it was in that menu).

### Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `src/KBot.App/Views/EFactura/EFacturaView.Designer.vb`; help `contabil/efactura/index.md` (0000-93).

**Probe:** [B2](#probe-b2).

### Unverified / deferred
- Not seen on screen: the header with only the magnifier, the month opening on the selected invoice, the month rows without a supplier level.
- The capture `efactura-facturi`/view captures may show the old tree; not marked for redo here (the view's capture is not among those named in the help).

## SLICE-0114-23 - E-Factura: no bottom padding on the sub-views, narrower trees, «Atasamente» folded into «Continut» (17.10.2026) <a id="slice-0114-23"></a>

### What changed and why
Operator, five points.
1. **Bottom padding of the sub-views** (the 2-3 px strip under the XML / content views, in the view of the angajament and in the window «Facturi primite»): the pages `PrimiteLiniiPage`, `PrimiteMesajePage`, `PrimiteReceptiiPage` and the XML view (`pgXml`) had `Padding(3)`; now `Padding(3, 3, 3, 0)` (left, top and right stay, the bottom goes), so the content reaches the bottom of the tree's frame.
2. **«Facturi primite» (the control, so the view and the window): the tree 25 % narrower**: the column split 32 / 68 became 24 / 76.
3. **Vanzare (`FacturiForm`): tree 25 % narrower**: the body split 30 / 70 became 22.5 / 77.5 (the footer buttons' split follows, so the footer stays under the same columns).
4. **Vanzare: the view «Atasamente» is gone** (nav item, `pgAtasamente`, the class `VanzareAtasamentePage` and its two files deleted).
5. **The tick «Ataseaza factura originala»** is now the footer row of the view «Continut» (`VanzareContinutPage`, a third row under the grid); `FacturiForm` binds `chkAtasament` to it, nothing else in its logic changed (saved with the invoice, locked outside edit mode as before).

### Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.Designer.vb`, `PrimiteLiniiPage.Designer.vb`, `PrimiteMesajePage.Designer.vb`, `PrimiteReceptiiPage.Designer.vb`; `FacturiForm.Designer.vb`, `FacturiForm.vb`, `FacturiForm.Pagini.vb`, `FacturiForm.Pdf.vb`; `Views/Vanzare/VanzareContinutPage.Designer.vb`, `VanzareContinutPage.vb`; deleted `Views/Vanzare/VanzareAtasamentePage.vb` and `.Designer.vb`. Help `contabil/efactura/index.md` (0000-94).

**Probe:** [B2](#probe-b2).

### Unverified / deferred
- Not seen on screen: whether the 3 px strip is really gone in both places (the page padding was the only source found; if a strip stays, it is the card around the view), the tick's place and look in the footer of «Continut», the narrower trees (row text is cut earlier now; the width 0 = dynamic text still shows what fits).
- The other Vanzare pages keep their 6 px padding all round (not asked).
- Captures: `efactura-factura-noua` and the ones showing the «Atasamente» tab (if any) were not marked for redo.

## SLICE-0114-24 - E-Factura: the download id is named in the ANAF download error (17.10.2026 request, 7.3 still open) <a id="slice-0114-24-efactura"></a>

### What changed and why
Operator item 7.3 (view «Factura ANAF» of an issued invoice): «nu cred ca foloseste ID-ul corect pentru descarcare, de obicei incepe cu 4xxx»; later: the error text differs from error to error, so there is no single text to send. The path was read end to end: the client sends only the invoice id (`GET /facturi/{id}/pdf-anaf`) -> `trimitere.pdf_anaf` -> `trimitere.descarca` -> `anaf_api.descarca(token, row["id_descarcare"])`; `id_descarcare` is written only by `verifica` (`stareMesaj` -> attribute `id_descarcare`, the 4xxx number) when ANAF says «ok», and the state «acceptata» needs it numeric. Nothing in the code picks `id_incarcare` (5xxx) for the download. So a wrong id can only be a WRONG STORED VALUE (older rows, or written outside this code). To make the next occurrence readable without asking for a database read, `anaf_api.descarca` now ends its two errors with «(id descarcare folosit: N)».

### Files touched
`PYTHON/routes/efactura/anaf_api.py` (`descarca`). Not deployed (the operator does that by hand).

### Test results
Syntax checked only; no test run. Nothing seen on screen.

### Unverified / deferred
- 7.3 CLOSED by the operator (21.10.2026, «considera inchisa directia asta»); what follows is the note kept for the record: when the error shows, the id in the brackets tells whether the stored value is the 4xxx download id or the 5xxx upload id; if it is the upload id, the fix is data (re-run «Verifica» / the state check for that invoice) or a repair routine, to be decided with that evidence.
- The text is also shown for the other ANAF downloads (messages, received invoices), which use the same function.

### Probe comune <a id="probe-b2"></a>

**B2.** `dotnet build KBot.sln`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

Trasabilitate ajutor din registru: 19 → 0000-91, 22 → 0000-93, 23 → 0000-94.
