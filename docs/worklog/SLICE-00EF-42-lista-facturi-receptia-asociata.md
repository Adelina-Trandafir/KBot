# SLICE-00EF-42 - The list of all the received invoices: «Receptie asociata» instead of «Furnizor» (17.10.2026)

## What changed and why
Operator request: in the grid of 00EF-41 the column «Furnizor» is replaced by the associated RECEPTION (if there is one), with its date, description («expl») and value in the same cell.

- **Server** (`PYTHON/routes/efactura/primite.py`): `lista()` now adds to every invoice `Receptii`, a list built by the new `_receptii_pe_factura`: one item per reception the invoice is linked to through `EF_PrimiteReceptii` (the link names the reception by `IDRR`, or by `IDRH` until the download resolves it): `DataR`, `Descriere` (the K-BOT marker «(IDRH: n; IDR: m)» is cut out), `Suma` (what the links of THAT invoice allocate on the reception, all classifications added) and `AsteaptaDescarcarea` (the link has no reception row yet). One query per 500 invoices, not one per invoice. If `EF_PrimiteReceptii`, `FX_Receptii_R` or `FX_Receptii_H` does not exist the list answers as before with empty `Receptii`.
- **Client**: `EFacturaPrimitaReceptie` (new, `KBot.Domain`) and `EFacturaPrimita.Receptii`; `KBot.Api` reads the new field. `PrimiteListaPage`: the column is «Recepție asociată» (key `receptie`, the fill column); the cell is `dd.MM.yyyy · description · value` per reception, several separated by «  |  », «așteaptă descărcarea din FOREXE · value» while the reception is not in K-BOT yet, empty when there is no link. The supplier is no longer in the grid (it stays in the tree tooltips and in the messages of the two actions).
- The list is read again when the root is chosen after a link was made or removed in the «Recepții» view (`_listStale`), so the new column does not show an old state.

## Files touched
`PYTHON/routes/efactura/primite.py`, `src/KBot.Domain/EFacturaPrimite.vb`, `src/KBot.Api/ApiClient.EFactura.Primite.vb`, `src/KBot.EFactura/Views/Primite/PrimiteListaPage.vb`, `PrimiteListaPage.Designer.vb`, `PrimiteView.Lista.vb`, `PrimiteView.vb`, `PrimiteView.Receptii.vb`; FileVersion KBot.EFactura 1.5.0.0 -> 1.6.0.0, KBot.Domain 1.2.14.0 -> 1.2.15.0, KBot.Api 1.0.22.0 -> 1.0.23.0. Help: SLICE-0000-98.

## Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. The marker regex was tried in the PYTHON venv on one sample («plata factura 12/01.09.2026 (IDRH: 12; IDR: 34)» -> «plata factura 12/01.09.2026»); `primite.py` compiles. The new query was NOT run against a database; no server test run (operator's rule), nothing seen on screen.

## Unverified / deferred
- **The server file must be sent to the VPS** (`primite.py`): until then the list comes without `Receptii` and the column stays empty. Nothing was deployed.
- The query was written from the shapes in `sql/KE_01_primite_receptii.sql` and the live DDL (`FX_Receptii_R.DataR / Descriere`, `FX_Receptii_H.IDRH / IDRR`); not tried. A reception deleted on the site (`FX_Receptii_R.Sters = 1`) is still shown here (the «Recepții» view marks it «ștearsă în FOREXE»; this cell does not).
- A reception in a link row without any `FX_Receptii_R` row is reported as «așteaptă descărcarea» (no date and no description).
- A very long description makes a wide cell; the column has no tooltip with the full text.
