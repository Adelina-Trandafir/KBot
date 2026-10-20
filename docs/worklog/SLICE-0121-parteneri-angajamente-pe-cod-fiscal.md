# SLICE-0121 - Supplier <-> angajament links by CodFiscal (PLAN, 19.10.2026)

**Status: 0121-01 DDL written 19.10.2026 (`sql/P121_01_parteneri_angajamente.sql`, not run, see `SLICE-0121-01-schema.md`); 0121-02 Migrator built 19.10.2026, never run (see `SLICE-0121-02-migrator.md`); 0121-03 «Parteneri» window built 19.10.2026, never run (see `SLICE-0121-03-fereastra-parteneri.md`); 0121-04 hooks built 19.10.2026, never run (see `SLICE-0121-04-carlige-asociere.md`); 0121-05 page «Recepții» built 19.10.2026, never run (see `SLICE-0121-05-pagina-receptii.md`); 0121-06 is open (the operator's word).** Slice number = the next free one (operator, 19.10.2026); every point below is a sub-slice `0121-NN`.

## Why
Operator (19.10.2026), page «Recepții» (KE16): an angajament picked for a supplier belongs to the SUPPLIER, never to the invoice. Today the pick lives only in memory (`KeLegaForm._added`, per supplier since KE16) and the durable partner data (`Parteneri`, `Parteneri_Coduri`) is per unit/SS (`IdUnitate`, `CodPartener`), so a partner would have to exist in every unit an angajament touches. Decision: keep `Parteneri` as it is, and key the NEW links by the normalized **CodFiscal** (one DC database = several units; a table keyed by CUI is DC-wide by construction).

## Decisions (all from the operator, 19.10.2026)
- `Parteneri` is NOT altered (no ALTER, indexes untouched). Partners without a CodFiscal (Access has some) are never linked to angajamente, invoices or reports; they stay in `Parteneri` and just have no rows in the new tables. They are counted and listed by the Migrator, none is lost.
- Several angajamente of one supplier on the same classification must be possible (the old key `(IdPartener, IdClsf)` forbids it; the new indicator key is the `CodAI`).
- `ContBancar` goes on the indicator row, as in `Parteneri_Coduri` today.
- Same CodFiscal with different `CodAng` on the same classification in two units: the operator believes it does not happen; the Migrator's report must show whether that is true.
- CodFiscal normalization is the one of the DDF (`primite_ubl.normalize_cui`), written normalized, read normalized.
- No path back to Access for `ParteneriAng` (none exists, none wanted for now).
- The server DDL is run by the operator by hand; nothing is deployed by Claude (see memory `never-deploy-to-server`).

## Facts read (not assumed)
- `Parteneri`: unique `(IdUnitate, CodPartener)`; FKs by `IdPartener` from `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_ORD_TBL`, `Parteneri_Coduri`, `VC_NIR` (RESTRICT) -> the table is NOT touched.
- Already by CodFiscal: `FX_DDF`, `FX_DDF_Parteneri(IDDF, CodFiscal)`, `FX_ORD_PART`, `EF_*`.
- `Parteneri_Coduri` is read/written only by the Parteneri window (`ParteneriForm`, `ApiClient.Nomenclatoare`), `parteneri_edit.py` and the Migrator (`TableMaps`, `ResolutionMaps`, `Verifier`, `TransferRunner`); DDF/ORD editors do not use it.
- `FX_Indicatori` already gives `CodAI` (`CodAng-CodInd`) -> `IdClsf`, so the indicator link needs no per-unit `IdClsf`.

## Sub-slices
| Sub-slice | What | Depends on |
|-----------|------|-----------|
| 0121-01 | **Schema.** DDL script in `sql/`: (a) the supplier <-> angajament link: `CodFiscal` (normalized) + `CodAng`, unique pair, dates; (b) its indicators: link id (FK, cascade) + `CodAI` + `CodInd` + `ContBancar` nullable, unique `(link, CodAI)`. `utf8mb3`/`utf8mb3_general_ci`. `Parteneri` and `Parteneri_Coduri` untouched. Refresh `MariaDB_Schema/` after the operator runs it. | - |
| 0121-02 | **Migrator.** `ParteneriAng` -> the two new tables through the partner's CodFiscal (normalized). «Verifică» report: partners without CodFiscal (count + list, skipped), same-CUI conflicts across units, `CodAI` missing in `FX_Indicatori`, duplicates merged. No longer needs the partner to exist in the unit. | 01 |
| 0121-03 | **«Parteneri» window + `parteneri_edit.py`** read/write «Coduri angajament» on the new tables by the partner's CodFiscal; a partner without one: the part is disabled with a message. Help 0000-NN. | 01 |
| 0121-04 | **Hooks.** Saving an association partner <-> DDF (`ddf_parteneri.py`) or partner <-> angajament fills the link and the angajament's indicators when missing; never overwrites, never on a mere view. | 01 |
| 0121-05 | **Page «Recepții» (KE16).** Proposals read the supplier's angajamente from the new tables; the pick in the table «Angajamente» is saved through 0121-04; the in-memory `_added` is retired. Help 0000-NN. | 01, 04 |
| 0121-07 | **One-time query (added 19.10.2026).** `sql/0121_07_interogare_unica.sql`: copies the old `Parteneri_Coduri` into the two new tables, run from KBotAdmin; see `SLICE-0121-07-copiere-parteneri-coduri.md`. | 01 |
| 0121-06 | **Close.** After the operator verifies the migrated data, `Parteneri_Coduri` is read by nobody; it is dropped only at the operator's word. | 02, 03, 05 |

## Open threads
- DDL not written, not run; the real data of `Parteneri_Coduri` (is `CodAng` filled?) not seen.
- The exact normalization of CodFiscal for persons (CNP) and foreign VAT ids to be checked in the Migrator report (0121-02).
