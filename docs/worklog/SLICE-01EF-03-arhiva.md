# SLICE-01EF-03 — the archive of the received invoices

## What changed and why
Table `EF_PrimiteArhiva` (DDL `sql/01EF_01_efactura_arhiva.sql`, run by the operator). Server `primite_arhiva.py` / `primite_arhiva_routes.py`: `GET /api/efactura/arhiva`, `POST /api/efactura/primite/<id>/arhiva` (body `{motiv?}`; role Contabil; refused with `ARE_OPERATII` when `EF_PrimiteOperatii` has rows for the invoice; the server sets `DataArhivare` and `UN`), `DELETE` (takes out). Client: `GetArhivaAsync` / `ArchiveAsync` / `UnarchiveAsync`; in `PrimiteView` (window with all invoices only) archived invoices leave the working list, the header menu has «Afișare facturi din arhivă» (shows ONLY them), the invoice menu has «Arhivează factura» / «Scoate din arhivă», the tooltip says when and by whom. The reason (`Motiv`) is supported by the server and the API, but the window does not ask for it.

## Files touched
`PYTHON/routes/efactura/primite_arhiva.py`, `primite_arhiva_routes.py`, `__init__.py`; `KBot.Domain/EFacturaArhiva.vb`; `KBot.Api/ApiClient.EFactura.Arhiva.vb`, `IEFacturaApi.vb`; `KBot.EFactura/Views/Primite/PrimiteView.vb`; help 0000-144.

## Test results
`py_compile`; builds of KBot.Api, KBot.EFactura, KBot.App: 0 errors, 0 warnings. Nothing run, nothing seen.

## Unverified / deferred
- **The migration of the archive from Access is NOT done.** An AVC.Migrator step was written and REMOVED (operator, 31.10.2026: AVC.Migrator never migrates invoices of any kind). The invoice data stay with the `KBot.Migrator` E-Factura tab; whether that tab should fill `EF_PrimiteArhiva` (Access `EFT.Complet = True` and no `EFT_O` row; `DataArhivare` and `UN` NULL) is for the operator to decide.
- The `AVC.Migrator` step for the existing operations (`NoteC`, `Documente`, `Oper`, `Debit`, `Credit`) waits for the posting tables of AV stage 2 (none exist).
- The view of an angajament (DDF) does not offer the archive.
