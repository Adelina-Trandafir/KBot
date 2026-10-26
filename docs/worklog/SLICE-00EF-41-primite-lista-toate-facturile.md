# SLICE-00EF-41…42 — Lista tuturor facturilor primite și recepția asociată

41 introduce lista, selecția, tipărirea și salvarea XML; 42 înlocuiește coloana Furnizor cu Recepție asociată și recitește lista după legare. Limitele imprimării și ale interogării nedeployate rămân la etapa lor.

- [00EF-41](#slice-00EF-41)
- [00EF-42](#slice-00EF-42)

## SLICE-00EF-41 - Received invoices: the root «Toate facturile» shows a grid with ticks, «Listeaza» and «Salveaza XML» (17.10.2026) <a id="slice-00EF-41"></a>

### What changed and why
Operator request: in the E-Factura view, choosing «Toate facturile» must do what the DDF and ORD views do for their roots: show a grid with all the invoices (of the supplier / angajament the view holds), with a tick column like the DDF view and two actions on the ticked rows, «Listeaza» and «Salveaza XML»; the last column of every row is an options button with the same two actions (for the selected row only).

Built as the DDF view's `PrintListPage` (slice 0099), in the shape of this project:
- New page `PrimiteListaPage` (`Views/Primite`, own designer, all controls declared there): `KBotDataView` with columns tick (header icon ticks / unticks all), Data, Numar, Furnizor, Scadenta, Total (footer sum) and the button column «…»; footer with a status line and the two buttons. The page only collects: it raises `ListRequested` / `SaveXmlRequested` with the invoices (the ticked ones, or the one of the row whose «…» menu was used).
- `PrimiteView` (new partial `PrimiteView.Lista.vb`): choosing the root node (`RootKey`) calls `ShowListSurface` (the bar of views and the pages are hidden, the list takes their place, filled from `ShownItems()` newest first); choosing an invoice brings them back (`ShowDetailSurface`); a rebuild of the tree refills the list and keeps the root chosen; the supplier-grouped tree (no root) never shows it; changing the DDF or `ClearAll` hides it.
- «Listeaza» = makes each invoice ready (the classic invoice K-BOT draws: `FacturaPrimitaPdf.Write`, the default view of «Factura PDF») and sends it to the printer WITHOUT opening it, like «Genereaza si imprima» of the DDF list: the printer and the number of copies are asked once for the whole batch. «Salveaza XML»: one invoice = the usual save window; several = one folder is asked and every XML goes in it (name as the single save suggests, «(2)», «(3)»… when taken). An invoice that fails is logged and named in a message at the end; the others go on. Messages go through `KBotMessage`.
- `PdfPrinter` / `PrinterChoice` (the printto helper of the print list) moved from `KBot.App/Views/Print` to `KBot.Controls/Print`, now Public, so this project can use it; the print list of the DDF / ORD views is unchanged (it already imports `KBot.Controls`).
- New resource `check_boxes` (copy of the App's tick-box icon) in `KBot.EFactura`.

Reading of the request (assumptions): «pentru furnizorul respectiv» = the invoices the view holds (in the view of an angajament, those of its DDF's suppliers; in the window «Cumparare», the year's invoices, where the same root exists); «doar pentru randul selectat» = the options button of a row acts on that row only, whatever is ticked (the button is drawn on every row, not only the selected one).

### Files touched
`src/KBot.EFactura/Views/Primite/PrimiteListaPage.vb` + `.Designer.vb` (new), `PrimiteView.Lista.vb` (new), `PrimiteView.vb`, `PrimiteView.Designer.vb` (`pgLista`), `src/KBot.EFactura/My Project/Resources.resx` + `Resources.Designer.vb` + `Resources/check_boxes.png`, `src/KBot.Controls/Print/PdfPrinter.vb` (moved, Public), deleted `src/KBot.App/Views/Print/PdfPrinter.vb`; FileVersion KBot.EFactura 1.4.0.0 -> 1.5.0.0, KBot.Controls 1.65.0.0 -> 1.66.0.0. Help: SLICE-0000-97.

### Test results
`dotnet build src/KBot.App` (builds all its projects): 0 warnings, 0 errors. Nothing run, nothing seen on screen; no test.

### Unverified / deferred
- Never seen: the layout of the list, the button column (text «…» on the full cell), the tick icon in the header, the pop-up of a row at the mouse position.
- The printing goes through the shell's «printto» verb (needs Adobe Reader, like the DDF print list); not tried on a printer, nor with a credit note («NC» rows print the same classic invoice).
- A month node still has no page of its own (the DDF view lists the revisions of a month too; here only the root was asked).
- Printed invoices are not counted anywhere (the DDF list counts prints on the server; received invoices have no such counter).
- The classic invoice is read from the server one by one (one call per invoice); a large batch is slow, there is no cancel.

## SLICE-00EF-42 - The list of all the received invoices: «Receptie asociata» instead of «Furnizor» (17.10.2026) <a id="slice-00EF-42"></a>

### What changed and why
Operator request: in the grid of 00EF-41 the column «Furnizor» is replaced by the associated RECEPTION (if there is one), with its date, description («expl») and value in the same cell.

- **Server** (`PYTHON/routes/efactura/primite.py`): `lista()` now adds to every invoice `Receptii`, a list built by the new `_receptii_pe_factura`: one item per reception the invoice is linked to through `EF_PrimiteReceptii` (the link names the reception by `IDRR`, or by `IDRH` until the download resolves it): `DataR`, `Descriere` (the K-BOT marker «(IDRH: n; IDR: m)» is cut out), `Suma` (what the links of THAT invoice allocate on the reception, all classifications added) and `AsteaptaDescarcarea` (the link has no reception row yet). One query per 500 invoices, not one per invoice. If `EF_PrimiteReceptii`, `FX_Receptii_R` or `FX_Receptii_H` does not exist the list answers as before with empty `Receptii`.
- **Client**: `EFacturaPrimitaReceptie` (new, `KBot.Domain`) and `EFacturaPrimita.Receptii`; `KBot.Api` reads the new field. `PrimiteListaPage`: the column is «Recepție asociată» (key `receptie`, the fill column); the cell is `dd.MM.yyyy · description · value` per reception, several separated by «  |  », «așteaptă descărcarea din FOREXE · value» while the reception is not in K-BOT yet, empty when there is no link. The supplier is no longer in the grid (it stays in the tree tooltips and in the messages of the two actions).
- The list is read again when the root is chosen after a link was made or removed in the «Recepții» view (`_listStale`), so the new column does not show an old state.

### Files touched
`PYTHON/routes/efactura/primite.py`, `src/KBot.Domain/EFacturaPrimite.vb`, `src/KBot.Api/ApiClient.EFactura.Primite.vb`, `src/KBot.EFactura/Views/Primite/PrimiteListaPage.vb`, `PrimiteListaPage.Designer.vb`, `PrimiteView.Lista.vb`, `PrimiteView.vb`, `PrimiteView.Receptii.vb`; FileVersion KBot.EFactura 1.5.0.0 -> 1.6.0.0, KBot.Domain 1.2.14.0 -> 1.2.15.0, KBot.Api 1.0.22.0 -> 1.0.23.0. Help: SLICE-0000-98.

### Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. The marker regex was tried in the PYTHON venv on one sample («plata factura 12/01.09.2026 (IDRH: 12; IDR: 34)» -> «plata factura 12/01.09.2026»); `primite.py` compiles. The new query was NOT run against a database; no server test run (operator's rule), nothing seen on screen.

### Unverified / deferred
- **The server file must be sent to the VPS** (`primite.py`): until then the list comes without `Receptii` and the column stays empty. Nothing was deployed.
- The query was written from the shapes in `sql/KE_01_primite_receptii.sql` and the live DDL (`FX_Receptii_R.DataR / Descriere`, `FX_Receptii_H.IDRH / IDRR`); not tried. A reception deleted on the site (`FX_Receptii_R.Sters = 1`) is still shown here (the «Recepții» view marks it «ștearsă în FOREXE»; this cell does not).
- A reception in a link row without any `FX_Receptii_R` row is reported as «așteaptă descărcarea» (no date and no description).
- A very long description makes a wide cell; the column has no tooltip with the full text.
