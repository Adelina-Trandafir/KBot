# SLICE-0121-05 - Page «Recepții» (KE16): the supplier's angajamente from the new tables, the pick saved (19.10.2026)

Plan: [SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md](SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md). Needs 0121-01 run and synced; the save uses the route of 0121-04.

## What changed and why
An angajament picked in the table «Angajamente» of the page belongs to the SUPPLIER (operator, 19.10.2026) and is now kept durably, by the supplier's normalized tax code.

Server:
- `primite_ke.py` `_angajamente_candidate`: also reads `Parteneri_Angajamente` WHERE `CodFiscal = invoice.CuiNormalizat` and offers those angajamente with the source **«furnizor»** (after the DDF ones, so a DDF angajament keeps «ddf»; a linked one stays «legatura»). A database without the table keeps working (`_are_legaturi_furnizor`, a probe on `DATABASE()`). Because the bulk-link proposals (`_facturi_neasociate`) already take their angajamente from `_angajamente_candidate`, **the proposals now read the supplier's angajamente from the new tables** with no change in `primite_ke_propuneri.py`.
- `partner_links.py`: new `POST /api/forexe/parteneri/angajament/scoate {cod_fiscal, cod_angajament}` -> `{sterse}` (deletes the link, its indicators go by cascade); the save is the route of 0121-04. Journalled in `actiuni.py`.

Client:
- `IEFacturaApi` / `ApiClient.EFactura.Ke.vb`: `LinkSupplierAngajamentAsync`, `UnlinkSupplierAngajamentAsync` (paths under `/api/forexe/parteneri/angajament`).
- `KeLegaForm`: `_added` and `AddedForContext` are **retired**. `BeginAddAngajament` saves the pick for every supplier of the context (ticked invoices, else the one pressed) with its tax code (`SupplierCui`); a supplier with no tax code on the invoice gets a notice and no link; then the states of that supplier's invoices are dropped and the ones on screen read again (`RefreshContextStatesAsync`, new `_browsedId`) and the table is rebuilt from the server. «Elimină asociere» on a «furnizor» row asks, then removes the link for those suppliers. `KePropuneriForm` is given an empty list of picked codes (the server finds them); the `angajamente` field of the two KE16 routes stays for older clients.
- Help 0000-114 (separate worklog).

## Files touched
`PYTHON/routes/efactura/primite_ke.py`, `PYTHON/routes/forexe/partner_links.py`, `PYTHON/routes/actiuni.py`, `src/KBot.Api/IEFacturaApi.vb`, `ApiClient.EFactura.Ke.vb`, `src/KBot.EFactura/Forms/KeLegaForm.vb`; FileVersion: Api 1.0.26.0, EFactura 1.8.0.0; help `contabil/efactura/index.md`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. `py_compile` of the Python files: OK. **No test written or run, no database touched, nothing seen on screen.**

## Left unverified or deferred
- Never run: the `furnizor` candidates, the save/remove round trips and the redraw of the table after a pick are unverified. The scenario to check: supplier TRANSPORT CALATORI EXPRESS - pick an angajament, press another supplier and back, close and reopen the page.
- The route of the save requires a session only (like the «Parteneri» window), not the role «Contabil» that the KE writes check.
- Removing a «furnizor» link deletes its indicators and bank accounts too (asked in the question). A link made by the DDF hook (0121-04) reappears at the next DDF save of that partner.
- The embedded variants (a view of one invoice / one angajament) use the same table; only the pick path changed.
- Server files go to the VPS by the operator's hand.
