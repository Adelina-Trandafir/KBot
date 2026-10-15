# SLICE-00EF-35 - Received invoices: the options menu of the tree header (15.10.2026)

## What changed and why
The button on the right of the tree header (it had one disabled line) opens the real menu:
1. **«Afisare facturi fara operatii in AVACONT»** (checked while in force): only the invoices with `NrOperatii = 0`.
   Server: `primite.lista` adds `NrOperatii` = the rows of `EF_PrimiteOperatii` of each invoice (the table is filled by the
   migrator; when the table does not exist yet every invoice has 0 and the list still works -- the check is on `information_schema`).
   Client: `EFacturaPrimita.NrOperatii`.
2. **«Afisare facturi noi»**, offered only when this session brought invoices (`PrimiteView._newIds`, filled by `ShowSyncResult`):
   without a sync of this session nothing says which invoices are new.
3. separator.
4. **Grouping**: «Grupare dupa furnizor» (tree = supplier roots, their invoices under them; no «Toate facturile» root) or «Grupare dupa
   luna» (what existed: Toate facturile > month > supplier > invoice). Only the one NOT in force is offered.
5. In supplier grouping only: a separator and **«Ordonare crescatoare facturi»** / **«Ordonare descrescatoare facturi»** (by invoice
   date, the one in force checked). Under a month the invoices stay newest first.
The options are of the session (not stored). The tree is built again with the shown invoice kept selected.

## Files touched
`PYTHON/routes/efactura/primite.py` (py_compile ok, **not deployed**), `src/KBot.Domain/EFacturaPrimite.vb`,
`src/KBot.Api/ApiClient.EFactura.Primite.vb`, `src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `PrimiteView.Designer.vb` (tooltip),
`contabil/efactura/index.md` (0000-80).

## Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. Nothing run, nothing seen; the list query was not run against a database.

## Unverified / deferred
- Until the server is redeployed the list has no `NrOperatii`: every invoice counts as 0, so the first filter shows everything.
- Whether EF_PrimiteOperatii has rows for the unit (the migrator fills it; the DDL run is not confirmed in the status).
- The options are not kept between sessions (nothing was asked); the search of the header and the options work together.
