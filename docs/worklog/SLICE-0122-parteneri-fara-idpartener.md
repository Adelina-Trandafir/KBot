# SLICE-0122 — Identitatea furnizorului după CodFiscal

Planul și patru etape sunt reunite. „Analiză / nerulat” la 20.10 precede confirmarea operatorului din 23.10 pe o singură bază. 02/03 păstrează clientul neschimbat și protecția prin id până la eliminarea cheilor; 08 înlocuiește propunerea DELETE cu ascundere reversibilă. Etapele 04…07 rămân propuneri, inclusiv lucrul Vercon făcut separat în paralel. Nu se execută interogări, DDL, ascunderi sau migrări.

- [0122 — analiză, decizii și etape încă propuse](#slice-0122-plan)
- [0122-01](#slice-0122-01)
- [0122-02](#slice-0122-02)
- [0122-03](#slice-0122-03)
- [0122-08](#slice-0122-08)

## SLICE-0122 - Break the dependency on Parteneri.IdPartener / CodPartener outside the Access migration (PLAN, 20.10.2026) <a id="slice-0122-plan"></a>

**Checkpoint 20.10.2026:** numai analiză și decizia CodFiscal în CodPartener. **Evoluție 23.10:** 01/01b și controalele au rulat curat pe o bază; 02/03 sunt construite, nerulate; 08 este scris, nerulat. Etapele de mai jos păstrează limitele și confirmările exacte.

### Why
Operator (20.10.2026): the same supplier exists as one `Parteneri` row per unit, and that "gets in the way terribly". Goal: outside the Access migration, a partner is identified ONLY by its normalized CodFiscal (same rule as slice 0121: letters and digits, upper case, leading «RO» before a digit dropped). Then `Parteneri` can later be reduced to one row per CodFiscal. Compatibility: the operator makes the next update mandatory (20.10.2026), so every client has the current version. Old clients (1.1.2.4 and older) are NOT a constraint. What still must hold: data already in the database stays readable, and the server and the client of the current version go out together.

### What depends on Parteneri.IdPartener / CodPartener today (read, not assumed)

Schema (`MariaDB_Schema/AVACONT_SURSA.sql`):
| Table | Columns | Foreign key to `Parteneri` |
|-------|---------|----------------------------|
| `FX_DDF_REV_SA` | `CodPartener`, `IdPartener` | `FX_DDF_REV_SA_ibfk_3`, **ON DELETE CASCADE** |
| `FX_DDF_REV_SB` | `CodPartener`, `IdPartener` | `tblDocFund_SB_ibfk_3`, **ON DELETE CASCADE** |
| `FX_ORD_TBL` | `CodPartener`, `IdPartener` | `FX_ORD_TBL_ibfk_1`, **ON DELETE CASCADE** |
| `VC_NIR` | `IdPartener` (+ `CodFiscalSursa`, history only) | `fk_NIR_Partener`, RESTRICT |
| `VC_FACTURI` (`sql/VC_FACTURI_NIR_20261016.sql`) | `PARTENER_ID` | none |
| `Parteneri_Coduri` | `IdPartener` | cascade (dies with 0121-06) |
Deleting a `Parteneri` row today silently deletes DDF/ORD rows through the cascade; only the window's «activ» test (`parteneri_edit._ACTIVE`) stands in the way.

Already by CodFiscal: `FX_DDF`, `FX_DDF_Parteneri`, `FX_ORD_PART`, `EF_*`, `Parteneri_Angajamente*` (0121), the DDF header combo (`GROUP BY CodFiscal`).

**Key finding.** The K-BOT DDF editor ALREADY writes `CodPartener = header CodFiscal` and `IdPartener = 0 -> NULL` on section A / B rows (`DdfEditForm.CmbPartener_SelectedIndexChanged`, `DdfDraft.ImpingePartenerulPeLinii(CodFiscal, 0)`, `InheritHeaderPartner`, `DdfEditSectiuneaAPage`, `DdfSectiuneaAReguli`). So the table holds two kinds of rows: **old** (from Access: real unit `CodPartener` + `IdPartener`) and **new** (CodFiscal in `CodPartener`, no `IdPartener`). The column `CodPartener` already means two things.

Server readers/writers of those columns:
- `forexe/ddf_edit.py`: read (SA/SB select, wire `cod_partener`/`id_partener`), write (insert/update of SA/SB).
- `forexe/ord_edit.py`: `incarca_dic_part_ind` copies the section-A partner onto the ORD lines by CodIndicator; read/write of `FX_ORD_TBL`; `_verifica_existenta(... "Parteneri", "IdPartener" ...)` on save.
- `forexe/parteneri_edit.py`: `_ACTIVE` (activity test by IdPartener on SA/SB/ORD_TBL) guards delete and the «activ» flag.
- `forexe/sumar.py:153`: partner name = `Parteneri` joined on `CodPartener` + `IdUnitate` (already finds nothing for the new rows: shows no name).
- `forexe/angajament_dump.py:107`: diagnostic dump, `Parteneri` by `CodPartener`.
- `vercon/*` (`documents`, `receipts`, `receipt_choices`, `receipt_context`, `receipt_tree`, `receipt_distribution`, `invoices`, `report_query`, `stock_source`) + `reports/vercon/queries/{ledger,receipts}.sql` + `static/js/vercon/{docs,receipt-context}.js`: `VC_NIR.IdPartener` / `party_id` as the supplier key (the combo `receipt_choices` already groups by CodFiscal but stores `MIN(IdPartener)`).
- Client: `DdfDraft`, `OrdDraft`, `ApiClient.vb` (DDF/ORD wire), `OrdBeneficiariPage` (lists `CodPartener` of the lines), `ParteneriForm` (the window itself: needs the row id, stays).
- Added 23.10.2026 after a second inventory (Codex, checked against the code): the e-invoice -> Vercon flows also key by `IdPartener` after finding the partner by CodFiscal - `efactura/primite_vercon.py` (`_partener` returns the LOWEST `IdPartener` of the CUI, `_duplicate` searches `VC_NIR` by that one id, so a NIR saved under another unit's row is missed), `static/js/vercon/efactura-intake.js` (draft and NIR save), `routes/efactura/primite_vercon.py`; `utils/vercon_efactura.py` (`receipt_supplier_sql`) already reads `VC_NIR.CodPartener` when the column exists, then `Parteneri.CodFiscal` through the id, then `CodFiscalSursa`: a fallback chain that the filled `VC_NIR.CodPartener` makes unnecessary. The Access-side `VERCON/VRC.Migrator/VrcRunner.vb` resolves `Furnizori.IDF` -> CF -> `IdPartener` and writes `IdPartener` + `CodFiscalSursa` (Access migration: kept, but must also fill `VC_NIR.CodPartener`). **`IDF` is the supplier key of the OLD Vercon in Access, not `IDDF`.**
- Sales e-invoices (`facturi_store.py`) use `EF_Clienti.IdClient`: a different subject (customers), not in 0122.
- Working tree 23.10.2026: many `PYTHON/routes/vercon/*`, `efactura/primite_vercon.py` and `forexe/ddf_edit.py` carry uncommitted edits that are not from this slice. 0122-02 and 0122-05 re-read those files before touching them.

NOT in scope (kept on purpose): the Access migration (`Migrator` `TableMaps`/`ResolutionMaps`, `routes/parteneri.py` X-Api-Key path, the `mig_*` snapshot tables and `proc_Mig_*` procedures in AVACONT_COMUN); `extrase.py` (`FX_DicPart`: CodFiscal -> CodPartener per unit, writes `FX_Extrase.CodPartener`) is not on the operator's list - it must be looked at before `Parteneri` is reduced to one row per CodFiscal.

### DECIDED (operator, 20.10.2026): no new column - the CodFiscal goes INTO `CodPartener`
On `FX_DDF_REV_SA`, `FX_DDF_REV_SB` and `FX_ORD_TBL` the column `CodPartener` holds the supplier's NORMALIZED CodFiscal; `IdPartener` is no longer written (NULL) and no longer read. The name of the column stays (it is a legacy name); no DDL on these three tables except dropping the foreign keys. K-BOT's DDF editor already writes the CodFiscal there; what changes is the normalization and the old rows. `VC_NIR` has NO `CodPartener` (only `IdPartener`): operator decision 20.10.2026 - the column `CodPartener` is ADDED to `VC_NIR` too (`sql/P122_01_vc_nir_codpartener.sql`), same convention.

### Proposal (sub-slices, in this order)

În 01…03, propunerea inițială 02 de modificare a clientului rămâne distinctă de implementarea fără schimbare de client.
| Sub-slice | What |
|-----------|------|
| 0122-01 | [Coloană VC_NIR, conversie și corecția 01b](#slice-0122-01). |
| 0122-02 | [Server DDF/ORD: normalizare, fără schimbarea clientului](#slice-0122-02). |
| 0122-03 | [Cititori: activitate, nume și diagnostic](#slice-0122-03). |
| 0122-04 | **Drop the three foreign keys** (`FX_DDF_REV_SA_ibfk_3`, `tblDocFund_SB_ibfk_3`, `FX_ORD_TBL_ibfk_1`) - only after 0122-02 and 0122-03 are on the server. Removes the cascade that deletes DDF/ORD rows with a `Parteneri` row. DDL by the operator + structure sync. |
| 0122-05 | **Vercon**: the column `VC_NIR.CodPartener` exists since 0122-01 (decided). The code that uses `VC_NIR.IdPartener` / `VC_FACTURI.PARTENER_ID` as the supplier key moves to `CodPartener` (9 route files, 2 report queries, 2 JS files). `VC_FACTURI` has no `CodPartener` yet - to decide. Own worklog. |
| 0122-06 | **Migrator**: the last unit still to be migrated writes the Access `CodPartener`/`IdPartener` on these three tables; the operator re-runs 0122-01 after it (or the Migrator is changed to write the CodFiscal). |
| 0122-07 | Later, at the operator's word: drop the dead `IdPartener` columns; reduce `Parteneri` to one row per CodFiscal (needs a decision on `IdUnitate NOT NULL`, `UK_Parteneri_IdUnitate_CodPartener` and `extrase.py`). |

Deployment order (operator, by hand): 0122-01 query -> server files + new client TOGETHER (0122-02, 0122-03) -> 0122-04 DDL.

### Still open
1. Vercon code (0122-05) and `VC_FACTURI.PARTENER_ID`: when, and does `VC_FACTURI` get the same column?
2. A `Parteneri` row with activity but NO CodFiscal (if the check queries of 0122-01 find any): the rows keep their old `CodPartener`, and the partner stays undeletable until fixed by hand.
3. Client check at build time: whether anything on the client compares `CodPartener` of a line with the header's raw CodFiscal (the server will now return the normalized one).

### Open threads
- Datele nu erau văzute la analiza inițială. [01/01b](#slice-0122-01) păstrează constatarea ulterioară și confirmarea pe o singură bază; celelalte baze rămân de parcurs. Interogările de control aparțin 01, nu serverului 02.
- Help: the visible change is small (the delete refusal message of «Parteneri»); recorded as `0000-NN` when 0122-04 is built.

### Added 23.10.2026
- 0122-08 (operator): hide (not delete) the `Parteneri` rows nothing uses, see [0122-08](#slice-0122-08) (`sql/0122_08_interogare_unica.sql` + `_PREVIEW`). Written, not run.
- Codex works on the Vercon side in parallel (`sql/VC_20261023_FURNIZORI_CF_*.sql`, `VC_FACTURI.CodPartener`, `utils/vercon_suppliers.py`): 0122-05 is NOT started here to avoid a second implementation.

## SLICE-0122-01 - CodFiscal in CodPartener: the column of VC_NIR and the one-time query (20.10.2026) <a id="slice-0122-01"></a>

**Status: WRITTEN, NOT RUN (no test, by the operator's rule).**

### Operator decisions (20.10.2026)
- The CodFiscal (normalized) goes INTO `CodPartener` on `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_ORD_TBL`: no new column there.
- `VC_NIR` gets the same: a new column `CodPartener` ("we put CodPartener in VC_NIR too, we add the column").
- The next update is mandatory, so old clients are not a constraint.

### What was written
- `sql/P122_01_vc_nir_codpartener.sql`: `ALTER TABLE VC_NIR ADD COLUMN IF NOT EXISTS CodPartener varchar(50) NULL AFTER IdPartener` + index `ix_NIR_CodPartener`. utf8mb3/utf8mb3_general_ci. Idempotent. The operator runs it on AVACONT_SURSA and then syncs the structure to the units.
- `sql/0122_01_interogare_unica.sql` (name in KBotAdmin: `0122_01_codfiscal_in_codpartener`): for `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_ORD_TBL` sets `CodPartener` := normalized CodFiscal (from `Parteneri` through `IdPartener` when set, else the existing `CodPartener` normalized) and `IdPartener` := NULL. For `VC_NIR` fills only the new `CodPartener` and KEEPS `IdPartener` (Vercon still uses it until 0122-05). Rows whose partner has no CodFiscal are not touched. Idempotent. Check queries as trailing comments.
- Normalization as everywhere (0121): letters and digits, upper case, a leading RO before a digit dropped.

### Order for the operator
P122_01 on AVACONT_SURSA -> structure sync -> the one-time query from KBotAdmin, right before the new server files and the new K-BOT go out (0122-02, 0122-03) -> only then the foreign keys are dropped (0122-04).

### Known gap between the query and the new server
After the query the old rows have `IdPartener` NULL. Until the new `parteneri_edit._ACTIVE` (0122-03) is on the server, the window «Parteneri» no longer sees that activity through `IdPartener` (a DDF header with the same CodFiscal still counts). So the query and the server files go out together.

### Left unverified
- Nothing was run: the temporary-table statements follow the pattern of 0121_07 (which the operator ran), the UPDATE ... JOIN on a temporary table is new.
- Real data not seen: how many rows still carry an IdPartener afterwards, and whether any row has an old unit code in `CodPartener` with no `IdPartener` (the third check query lists them).
- `VC_FACTURI.PARTENER_ID` (no foreign key) is not covered: decide with Vercon (0122-05).

### Follow-up 23.10.2026: 0122_01b
The first check on one database found (a) the 246 `VC_NIR` rows not filled (the column was probably missing when the first query reached that block) and (b) 41 DDF/ORD rows holding an old unit code (025, 026) with no IdPartener. Written: `sql/0122_01b_interogare_unica.sql` (name in KBotAdmin `0122_01b_completari_codfiscal`): matches such a code to the partner of the same unit and fills `VC_NIR.CodPartener` (with `CodFiscalSursa` as fallback). The operator ran it on that database and `sql/0122_01_verificari.sql` came back clean. The other databases are still to be done.

## SLICE-0122-02 - Server DDF + ORD: the supplier of a row is its CodFiscal (23.10.2026) <a id="slice-0122-02"></a>

**Status: BUILT (py_compile OK), NOT RUN, no test (operator's rule).**

### Data state before this (operator, 23.10.2026)
On one database the one-time queries `0122_01` + `0122_01b` were run and `sql/0122_01_verificari.sql` came back clean: no row left with an IdPartener on SA / SB / ORD_TBL, `VC_NIR` without CodPartener = 0.

### What changed
- `PYTHON/routes/forexe/ddf_edit.py`: the section A / B rows are saved with `CodPartener = partner_links.normalize_cui(cod_partener)` and `IdPartener = NULL` whatever the client sends. They are read with the normalized CodFiscal in `cod_partener` and `id_partener` = 0 (the wire field stays). The unrelated uncommitted edits of this file (program of the DDF, slice 0114-27) were left exactly as they were.
- `PYTHON/routes/forexe/ord_edit.py`: the dictionary that copies the section-A partner onto the payment lines (`incarca_dic_part_ind`) returns the normalized CodFiscal and no id; the payment lines are read with the normalized `cod_partener` and `id_partener` null; saved with `CodPartener` = normalized CodFiscal and `IdPartener` = NULL; the save no longer checks that an `IdPartener` exists in `Parteneri`.
- Client: NO change. `id_partener` still travels (0 / null) and `ApiClient` already reads both; the editors already put the header's CodFiscal in the lines' partner.

### Known differences
- The header (`FX_DDF.CodFiscal`) keeps the code as picked from `Parteneri` (for example with RO), while the lines now come back normalized (no RO). Nothing on the client compares the two (checked by reading: the cell is only displayed and edited).
- Rows of other databases that were NOT yet through the one-time queries still hold their old values; they are read normalized, written normalized when saved.

### Left unverified
Nothing was run: no database, no editor. The save paths (SA / SB inserts and updates, the ORD dictionary) are changed by reading, not exercised.

### Next
0122-03 (readers: `parteneri_edit._ACTIVE`, `sumar.py`, `angajament_dump.py`), then 0122-04 (drop the three foreign keys) once 0122-02 and 0122-03 are on the server.

## SLICE-0122-03 - Readers: activity, partner name and dump by CodFiscal (23.10.2026) <a id="slice-0122-03"></a>

**Status: BUILT (py_compile OK, the SQL strings printed and read), NOT RUN, no test (operator's rule).**

### What changed
- `PYTHON/routes/forexe/partner_links.py`: new `fiscal_sql(expression)`, the SQL form of `normalize_cui` (letters and digits, upper case, a leading RO before a digit dropped).
- `PYTHON/routes/forexe/parteneri_edit.py`: the activity test (`_ACTIVE`, the «activ» flag of the list) is the OR of two tests: `_ACTIVE_BY_ID` (the old one, by IdPartener) and `_ACTIVE_BY_CF` (the normalized CodFiscal of the partner is a CodPartener of a DDF section A / B row or an ORD table row, or the CodFiscal is on a DDF header). The delete of a partner is refused when document rows still name it BY ID (the cascade would delete them) or when it is the LAST Parteneri row of a CodFiscal that documents use; if another row has the same CodFiscal the delete is allowed. Docstring updated.
- `PYTHON/routes/forexe/sumar.py`: the «Partener» name of the DDF revisions is looked up by the normalized CodFiscal (lowest IdPartener), with the old lookup by unit code + IdUnitate as a fallback.
- `PYTHON/routes/forexe/angajament_dump.py`: the partners of the dump are matched by CodPartener OR by the normalized CodFiscal.
- Client: no change (no wire change). No visible text changed, so no help entry.

### Why the old tests stay
Until 0122-04 drops the three foreign keys, deleting a `Parteneri` row still deletes the document rows that name it by IdPartener. A database the one-time query 0122_01 has not reached still has such rows, and on it the by-CodFiscal test sees nothing. So the by-id test stays and is a hard refusal. Remove it with 0122-04 / 0122-07.

### Left unverified
Nothing was run. The `IN (SELECT ... UNION ...)` of the activity test and the correlated lookups were composed and read, not executed against MariaDB. `VC_NIR` is NOT part of the activity test (deleting a partner with receipts still fails on the RESTRICT key, as before).

### Next
0122-04 (the operator drops the three foreign keys - DDL by hand, only after 0122-02 and 0122-03 are on the server and the one-time queries ran on every database), then Vercon (0122-05).

## SLICE-0122-08 - Hide the Parteneri rows that nothing uses (23.10.2026) <a id="slice-0122-08"></a>

**Status: WRITTEN, NOT RUN (no test, by the operator's rule). First written as a DELETE; the operator asked (23.10.2026) to HIDE instead (`Ascuns = 1`): nothing is deleted, it can be undone.**

### Operator request (23.10.2026)
A query to run on the `Parteneri` table of every database that removes ALL rows not used right now - neither in VERCON nor in K-BOT. Changed to: HIDE them (`Ascuns = 1`) instead of deleting.

### What was written
- `sql/0122_08_interogare_unica.sql` (name in KBotAdmin: `0122_08_parteneri_nefolosite_ascunse`): reads first (temporary tables), the UPDATE `Ascuns = 1` is the LAST statement. Rows already hidden are not touched.
- `sql/0122_08_parteneri_nefolosite_PREVIEW.sql`: the same reading, then ONE result: the rows that would be hidden + three totals (to hide / already hidden / in Parteneri). Changes nothing. For HeidiSQL, on one database.

### What counts as «used» (a row is kept when ANY holds)
1. By row id: `IdPartener` on `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_ORD_TBL`, `VC_NIR`, `VC_FACTURI.PARTENER_ID`.
2. By code in the unit (`IdUnitate` + `CodPartener`): on SA, SB, ORD_TBL and `FX_Extrase` (bank statements).
3. By normalized CodFiscal: `FX_DDF`, `FX_DDF_Parteneri`, `FX_ORD_PART`, `CodPartener` of SA / SB / ORD_TBL, `VC_NIR.CodPartener` and `CodFiscalSursa`, `Parteneri_Angajamente`, `VC_FACTURI.CodPartener`, `EF_Primite.CuiNormalizat`, `EF_VERCON_CORELARI.CUI`.
Optional tables/columns (VC_FACTURI, EF_Primite, EF_VERCON_CORELARI) are read through PREPARE and skipped when absent (a missing one holds no usage). The tests are by CodFiscal, so ALL rows of a used CodFiscal stay: duplicates across units are NOT merged (a separate step).

### What it does not know
- The schema folder `MariaDB_Schema/` is of 22.09.2026: it has none of the newer tables, so the list of users was assembled from the code and the `sql/` scripts (Codex's VC_20261023_FURNIZORI_CF_* scripts, EV_02, 0121). A table I did not find that points at a partner would be missed. Read the PREVIEW before the real run.
- Nothing is deleted, so `Parteneri_Coduri` and every other row stay. A hidden partner is not listed in the window «Parteneri», the DDF partner combo or Vercon for a new receipt. The Migrator for the last unit can bring partners back from Access, so it should run before this.

### Left unverified
Nothing was run: not the temporary tables, not the PREPARE of `DO 0`, not the multi-table UPDATE. Not checked either whether hiding changes something else in the Vercon supplier list beyond the `Ascuns` filter it already applies.

### Order for the operator
PREVIEW on one database -> the one-time query on that database -> look in the application -> the others. Undo: set `Ascuns` back to 0 on the rows the PREVIEW listed.
