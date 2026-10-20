# SLICE-0121-01 - Schema: supplier <-> angajament links by CodFiscal (19.10.2026)

Plan: [SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md](SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md).

## What changed and why
One new script, `sql/P121_01_parteneri_angajamente.sql`, with two new tables. `Parteneri` and `Parteneri_Coduri` are NOT touched (no ALTER, no index change); no code reads or writes the new tables yet.

| Table | Purpose | Key |
|-------|---------|-----|
| `Parteneri_Angajamente` | the link supplier <-> angajament | PK `IdLegAng`; unique `(CodFiscal, CodAng)`; index on `CodAng` |
| `Parteneri_Angajamente_Indicatori` | the indicators of a link, with `ContBancar` (NULL = none) | PK `IdLegInd`; unique `(IdLegAng, CodAI)`; index on `CodAI`; FK `IdLegAng` -> link, ON DELETE/UPDATE CASCADE |

Choices (all from the plan, plus the ones I made):
- `CodFiscal` holds the normalized CUI (DDF rule `primite_ubl.normalize_cui`); the pair `(CodFiscal, CodAng)` allows several angajamente of one supplier on the same classification, which the old `(IdPartener, IdClsf)` key forbade.
- No per-unit `IdClsf`: `CodAI` -> `FX_Indicatori.IdClsf` gives it.
- **No foreign key to `FX_Angajamente` / `FX_Indicatori`** (my choice, same reasoning as `EF_PrimiteReceptii`): the pick may come before the angajament is synchronised, a deleted angajament must not silently take the supplier's links with it, and the Migrator report (0121-02) has to be able to show a `CodAI` missing in `FX_Indicatori`. The only FK is indicator -> link.
- All text columns `utf8mb3` / `utf8mb3_general_ci`; `varchar(255)` on `CodFiscal`, `CodAng`, `CodAI` so the Migrator never meets a truncation on odd legacy values (largest unique key: 255 x 3 + 255 x 3 = 1530 bytes, under the InnoDB limit of 3072).
- Date columns follow `Parteneri_Coduri` (`DataAdugare`, `DataModificare`).
- `CREATE TABLE IF NOT EXISTS`: safe to run twice.

## Files touched
- `sql/P121_01_parteneri_angajamente.sql` (new)
- `docs/worklog/SLICE-0121-01-schema.md` (this file)
- `docs/worklog/state/KBOT_STATUS_0120-0129.md`, `docs/worklog/KBOT_STATUS.md` (status)

## Test results
Nothing run: the DDL has not been executed anywhere (the operator runs it by hand; Claude deploys nothing). Not syntax-checked against a live MariaDB.

## Left unverified or deferred
- Operator: run the script on `AVACONT_SURSA`, then sync the structure to the unit databases with KBotAdmin, then refresh `MariaDB_Schema/` (gitignored, by hand).
- `sql/AVACONT_SURSA.sql` (hand-written, already drifted) was not edited.
- No help change: nothing the operator sees changed.
- Next: 0121-02 (Migrator). Operator, 19.10.2026: no way to test the Migrator side now - it is built only, tested when the time comes.
