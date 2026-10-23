# SLICE-0122-08 - Hide the Parteneri rows that nothing uses (23.10.2026)

**Status: WRITTEN, NOT RUN (no test, by the operator's rule). First written as a DELETE; the operator asked (23.10.2026) to HIDE instead (`Ascuns = 1`): nothing is deleted, it can be undone.**

## Operator request (23.10.2026)
A query to run on the `Parteneri` table of every database that removes ALL rows not used right now - neither in VERCON nor in K-BOT. Changed to: HIDE them (`Ascuns = 1`) instead of deleting.

## What was written
- `sql/0122_08_interogare_unica.sql` (name in KBotAdmin: `0122_08_parteneri_nefolosite_ascunse`): reads first (temporary tables), the UPDATE `Ascuns = 1` is the LAST statement. Rows already hidden are not touched.
- `sql/0122_08_parteneri_nefolosite_PREVIEW.sql`: the same reading, then ONE result: the rows that would be hidden + three totals (to hide / already hidden / in Parteneri). Changes nothing. For HeidiSQL, on one database.

## What counts as «used» (a row is kept when ANY holds)
1. By row id: `IdPartener` on `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_ORD_TBL`, `VC_NIR`, `VC_FACTURI.PARTENER_ID`.
2. By code in the unit (`IdUnitate` + `CodPartener`): on SA, SB, ORD_TBL and `FX_Extrase` (bank statements).
3. By normalized CodFiscal: `FX_DDF`, `FX_DDF_Parteneri`, `FX_ORD_PART`, `CodPartener` of SA / SB / ORD_TBL, `VC_NIR.CodPartener` and `CodFiscalSursa`, `Parteneri_Angajamente`, `VC_FACTURI.CodPartener`, `EF_Primite.CuiNormalizat`, `EF_VERCON_CORELARI.CUI`.
Optional tables/columns (VC_FACTURI, EF_Primite, EF_VERCON_CORELARI) are read through PREPARE and skipped when absent (a missing one holds no usage). The tests are by CodFiscal, so ALL rows of a used CodFiscal stay: duplicates across units are NOT merged (a separate step).

## What it does not know
- The schema folder `MariaDB_Schema/` is of 22.09.2026: it has none of the newer tables, so the list of users was assembled from the code and the `sql/` scripts (Codex's VC_20261023_FURNIZORI_CF_* scripts, EV_02, 0121). A table I did not find that points at a partner would be missed. Read the PREVIEW before the real run.
- Nothing is deleted, so `Parteneri_Coduri` and every other row stay. A hidden partner is not listed in the window «Parteneri», the DDF partner combo or Vercon for a new receipt. The Migrator for the last unit can bring partners back from Access, so it should run before this.

## Left unverified
Nothing was run: not the temporary tables, not the PREPARE of `DO 0`, not the multi-table UPDATE. Not checked either whether hiding changes something else in the Vercon supplier list beyond the `Ascuns` filter it already applies.

## Order for the operator
PREVIEW on one database -> the one-time query on that database -> look in the application -> the others. Undo: set `Ascuns` back to 0 on the rows the PREVIEW listed.
