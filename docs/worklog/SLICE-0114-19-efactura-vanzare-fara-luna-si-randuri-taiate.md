# SLICE-0114-19…24 — Corecții ale ecranelor E-Factura

Istoric 17–21.10.2026. Etapa 22 înlocuiește păstrarea filtrului „fără operații” din [0114-17](SLICE-0114-ribbon.md#slice-0114-17), numai pentru vederea angajamentului; fereastra tuturor facturilor rămâne distinctă. Direcția 7.3 este închisă de operator la 21.10; mesajul cu id-ul folosit nu înseamnă o remediere a datelor. Numărul 0114-24 exista și la ribbon; aici are ancora tematică E-Factura, fără renumerotare.

- [0114-19](#slice-0114-19)
- [0114-21](#slice-0114-21)
- [0114-22](#slice-0114-22)
- [0114-23](#slice-0114-23)
- [0114-24 — E-Factura](#slice-0114-24-efactura)

## SLICE-0114-19 - E-Factura Vanzare: the month list removed, the tree rows no longer cut (operator item 7.1, 7.2) (17.10.2026) <a id="slice-0114-19"></a>

### What changed and why
The issued-invoice tree now shows the whole year: `cmbLuna`, its fill/handler/filter are removed. Removing `tree.LeftTextWidth = 100` restores dynamic text width (0, as in received invoices), using the space between icon and amount. **Item 7.3 was not changed at this checkpoint:** the client sends the invoice id; the server uses stored `id_descarcare`, not another chosen number. The stored value/error was still needed; the complete path and later diagnostic are in [0114-24](#slice-0114-24-efactura).

### Files touched
`src/KBot.EFactura/FacturiForm.vb`, `FacturiForm.Arbore.vb`, `FacturiForm.Designer.vb`.

### Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

### Unverified / deferred
- Not seen on screen: the new buttons in the view group, the popup of the document button (opens at the mouse), the fields in the ribbon, the tree rows of the issued invoices.
- Item 7.3 CLOSED by the operator (21.10.2026). Had needed the id stored in `EF_Facturi.id_descarcare` of an invoice that fails, and the error text of the «Factura ANAF» view.

## SLICE-0114-21 - «Facturi primite» window: the empty strip at the bottom is gone (17.10.2026) <a id="slice-0114-21"></a>

### What changed and why
Operator: «de ce la factura cumparare exista spatiul ala alb in footer? nu il vreau». `PrimiteForm.pnlJos` was a 58 px strip with «Iesire» and `lblStare`. Hosting as a page hid the button (ribbon «Inchide» closes it), while the tree footer already showed the state. Removed the strip, both controls, `CancelButton`, `SyncStatus` and the button handler; the tree card reaches the page bottom.

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
Operator's repeated grouping request applies only to angajament VIEW `EFacturaView`: no header-right button; «Toate facturile» / month-year / invoice, without supplier. New designer `PrimiteView.AngajamentView` (True in `EFacturaView.Designer.vb`) removes funnel icon, retaining/restoring its designer value on False, and ignores stray header-right clicks. `BuildMonthTree` puts newest invoices directly under month; shared `AddInvoiceNodes` builds both shapes. «Facturi primite» window keeps suppliers/options; view loses the menu's «fara operatii» filter.

### Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `src/KBot.App/Views/EFactura/EFacturaView.Designer.vb`; help `contabil/efactura/index.md` (0000-93).

**Probe:** [B2](#probe-b2).

### Unverified / deferred
- Not seen on screen: the header with only the magnifier, the month opening on the selected invoice, the month rows without a supplier level.
- The capture `efactura-facturi`/view captures may show the old tree; not marked for redo here (the view's capture is not among those named in the help).

## SLICE-0114-23 - E-Factura: no bottom padding on the sub-views, narrower trees, «Atasamente» folded into «Continut» (17.10.2026) <a id="slice-0114-23"></a>

### What changed and why
Operator, five points.
1. **Bottom padding** (2-3 px strip below XML/content in angajament view and «Facturi primite»): `PrimiteLiniiPage`, `PrimiteMesajePage`, `PrimiteReceptiiPage`, XML `pgXml` change `Padding(3)` to `Padding(3, 3, 3, 0)`. Only bottom goes, extending content to tree-frame bottom.
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
Operator item 7.3 («Factura ANAF», issued invoice): «nu cred ca foloseste ID-ul corect pentru descarcare, de obicei incepe cu 4xxx»; later reported varying error texts. Path read end to end: invoice id via `GET /facturi/{id}/pdf-anaf` → `trimitere.pdf_anaf` → `trimitere.descarca` → `anaf_api.descarca(token, row["id_descarcare"])`. Only `verifica` writes that 4xxx attribute from `stareMesaj` when ANAF says «ok»; «acceptata» requires numeric value. Code never picks 5xxx `id_incarcare` for download: diagnosis was WRONG STORED VALUE (older/external writes). To expose the next case without database reading, both `anaf_api.descarca` errors append «(id descarcare folosit: N)».

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
