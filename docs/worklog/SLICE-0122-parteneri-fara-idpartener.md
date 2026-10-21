# SLICE-0122 - Break the dependency on Parteneri.IdPartener / CodPartener outside the Access migration (PLAN, 20.10.2026)

**Status: ANALYSIS ONLY (20.10.2026). Nothing built, nothing run. Operator chose: the CodFiscal goes into CodPartener, no new column (see DECIDED). 0122-01 written 20.10.2026 (not run), see `SLICE-0122-01-codfiscal-in-codpartener.md`.**

## Why
Operator (20.10.2026): the same supplier exists as one `Parteneri` row per unit, and that "gets in the way terribly". Goal: outside the Access migration, a partner is identified ONLY by its normalized CodFiscal (same rule as slice 0121: letters and digits, upper case, leading «RO» before a digit dropped). Then `Parteneri` can later be reduced to one row per CodFiscal.
Compatibility: the operator makes the next update mandatory (20.10.2026), so every client has the current version. Old clients (1.1.2.4 and older) are NOT a constraint. What still must hold: data already in the database stays readable, and the server and the client of the current version go out together.

## What depends on Parteneri.IdPartener / CodPartener today (read, not assumed)

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

NOT in scope (kept on purpose): the Access migration (`Migrator` `TableMaps`/`ResolutionMaps`, `routes/parteneri.py` X-Api-Key path, the `mig_*` snapshot tables and `proc_Mig_*` procedures in AVACONT_COMUN); `extrase.py` (`FX_DicPart`: CodFiscal -> CodPartener per unit, writes `FX_Extrase.CodPartener`) is not on the operator's list - it must be looked at before `Parteneri` is reduced to one row per CodFiscal.

## DECIDED (operator, 20.10.2026): no new column - the CodFiscal goes INTO `CodPartener`
On `FX_DDF_REV_SA`, `FX_DDF_REV_SB` and `FX_ORD_TBL` the column `CodPartener` holds the supplier's NORMALIZED CodFiscal; `IdPartener` is no longer written (NULL) and no longer read. The name of the column stays (it is a legacy name); no DDL on these three tables except dropping the foreign keys. K-BOT's DDF editor already writes the CodFiscal there; what changes is the normalization and the old rows.
`VC_NIR` has NO `CodPartener` (only `IdPartener`): operator decision 20.10.2026 - the column `CodPartener` is ADDED to `VC_NIR` too (`sql/P122_01_vc_nir_codpartener.sql`), same convention.

## Proposal (sub-slices, in this order)
| Sub-slice | What |
|-----------|------|
| 0122-01 | **One-time query (KBotAdmin, `sql/0122_01_interogare_unica.sql`).** On `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_ORD_TBL`: `CodPartener` := normalized CodFiscal (from `Parteneri` through `IdPartener` where set; else the existing `CodPartener` normalized, which for the new rows is already the CodFiscal); `IdPartener` := NULL. Idempotent. Check queries: rows whose partner has no CodFiscal (left untouched, listed), rows with both empty. Run BEFORE the foreign keys go, and again after the last unit is migrated (0122-06). |
| 0122-02 | **Server DDF + ORD** (`ddf_edit.py`, `ord_edit.py`): on save, `CodPartener` is normalized and `IdPartener` is stored NULL whatever the client sends; reads return `id_partener` 0; the ORD dictionary copies only the CodFiscal; the existence check on `Parteneri.IdPartener` goes. Client: stops filling `IdPartener` (`DdfDraft`, `OrdDraft`, `ApiClient`); the wire fields stay so nothing breaks. |
| 0122-03 | **Readers**: `parteneri_edit._ACTIVE` by CodFiscal (a partner row may be deleted unless it is the LAST row of its CodFiscal and that CodFiscal has activity), `sumar.py` (name by CodFiscal, not by `CodPartener` + `IdUnitate`), `angajament_dump.py`. |
| 0122-04 | **Drop the three foreign keys** (`FX_DDF_REV_SA_ibfk_3`, `tblDocFund_SB_ibfk_3`, `FX_ORD_TBL_ibfk_1`) - only after 0122-02 and 0122-03 are on the server. Removes the cascade that deletes DDF/ORD rows with a `Parteneri` row. DDL by the operator + structure sync. |
| 0122-05 | **Vercon**: the column `VC_NIR.CodPartener` exists since 0122-01 (decided). The code that uses `VC_NIR.IdPartener` / `VC_FACTURI.PARTENER_ID` as the supplier key moves to `CodPartener` (9 route files, 2 report queries, 2 JS files). `VC_FACTURI` has no `CodPartener` yet - to decide. Own worklog. |
| 0122-06 | **Migrator**: the last unit still to be migrated writes the Access `CodPartener`/`IdPartener` on these three tables; the operator re-runs 0122-01 after it (or the Migrator is changed to write the CodFiscal). |
| 0122-07 | Later, at the operator's word: drop the dead `IdPartener` columns; reduce `Parteneri` to one row per CodFiscal (needs a decision on `IdUnitate NOT NULL`, `UK_Parteneri_IdUnitate_CodPartener` and `extrase.py`). |

Deployment order (operator, by hand): 0122-01 query -> server files + new client TOGETHER (0122-02, 0122-03) -> 0122-04 DDL.

## Still open
1. Vercon code (0122-05) and `VC_FACTURI.PARTENER_ID`: when, and does `VC_FACTURI` get the same column?
2. A `Parteneri` row with activity but NO CodFiscal (if the check queries of 0122-01 find any): the rows keep their old `CodPartener`, and the partner stays undeletable until fixed by hand.
3. Client check at build time: whether anything on the client compares `CodPartener` of a line with the header's raw CodFiscal (the server will now return the normalized one).

## Open threads
- Real data not seen: how many SA/SB/ORD_TBL rows are old vs new, how many point at a partner with no CodFiscal. The check queries of 0122-02 answer it.
- Help: the visible change is small (the delete refusal message of «Parteneri»); recorded as `0000-NN` when 0122-04 is built.
