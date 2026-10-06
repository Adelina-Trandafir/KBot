# SLICE-00EF-02 - E-Factura data model (DDL written, not run)

Slice 00EF (operator, 06.10.2026, fourth pass). Documents and SQL only: no Python, no VB, nothing run.

## What changed and why
- `sql/00EF_02_efactura_unitate.sql`: nine tables for the unit databases, run on `AVACONT_SURSA` and copied to every unit by the schema sync (`EF_Furnizor`, `EF_Clienti`, `EF_Facturi`,
  `EF_FacturiLinii`, `EF_Mesaje`, `EF_Primite`, `EF_PrimiteLinii`, `EF_PrimiteNote`, `EF_PrimiteMesaje`). Issued invoices from Access
  `Factura` / `FacturaC` / `ClientiEF` / `EF_UM` / `UNIT` + `Scheme` row `DPIFV`; received invoices from `EF` / `EFT` / `EFS` /
  `EFT_C` / `EFT_M`. Accounting columns and tables left out (decision 8 of the plan).
- `sql/00EF_02_efactura_comun.sql`: `EF_UM` (units of measure, one list for all units, like BIC) and `EF_Token` (one row per unit, tokens ENCRYPTED, expiry dates, certificate label, last error) and
  `EF_TokenStart` (single-use `state` of the authorise step) in `AVACONT_COMUN`.
- `tools/efactura/efactura.env.example` + `efactura.conf`: where the secrets go on the server, `/etc/avacont/efactura.env`
  (root, 600) read through a systemd `EnvironmentFile`. Template only, no value.
- The plan was updated with the operator's fourth-pass answers: the app never gets a token, only the expiration; warning 7 days before
  the refresh token runs out; the received-invoice tables and the DDF view's reading of received invoices confirmed.

## Files touched
New: `sql/00EF_02_efactura_unitate.sql`, `sql/00EF_02_efactura_comun.sql`, `tools/efactura/efactura.env.example`,
`tools/efactura/efactura.conf`, this file. Edited: `docs/worklog/PLAN_00EF_EFactura.md`, `KBOT_STATUS.md`,
`state/KBOT_STATUS_0000-0009.md`.

## Decisions taken here (state them, change if wrong)
- Names `EF_*`, ASCII; column names keep the Access words. `NumarFactura` unique per (series, number) instead of alone.
- Amounts are `decimal`, not `double`. The XML's received file is kept as `LONGBLOB` (file is UTF-8, the house character set is utf8mb3).
- `CuiNormalizat` is written by the server (no `RO`, spaces, dots), so the join with `FX_DDF_Parteneri.CodFiscal` is
  `EF_Primite.CuiNormalizat = <CodFiscal with the same stripping>`.
- The token table is in `AVACONT_COMUN`, not in the unit database (the refresh job walks one table; `Unitati(DC, CF)` is there).
- `RefreshExpiraLa` is computed by the server (authorisation + `EF_REFRESH_DAYS`, 365 assumed): ANAF's answer as `EF.EXE` reads it
  carries only `expires_in` for the access token.

## Test results
None. The SQL was NOT executed anywhere (no server access from here); it was written against `MariaDB_Schema/000_DEMO.sql` and
`AVACONT_COMUN.sql` (types, collations, key naming) and checked by reading only. MariaDB version not known: the `CHECK` constraint on
`EF_Furnizor` needs 10.2.1+ (the existing schema already uses CHECK constraints, e.g. `CK_FX_PDF_SEMNATURI_DOC`).

## Unverified / deferred
- The DDL has not been run; the service account's rights on the new tables are not checked.
- `EF_UM` has no rows: the Access rows are not in the export.
- The meaning of `Factura.Corectata` (a Double in Access) and how `IdFacturaA/SerieFacturaA/NumarFacturaA` are filled: read with the
  form in 00EF-07.
- The `cryptography` library is not in the project's venv; the VPS is not known to have it (needed in 00EF-03).
- Not decided: whether Access invoices already issued this year are imported or K-BOT starts fresh.

## Change after the first write (06.10.2026)
EF_UM moved from the unit script to the common script: the list (2113 UN/ECE codes) is identical for every unit, so one copy in AVACONT_COMUN, filled by the Migrator tab (00EF-03).
