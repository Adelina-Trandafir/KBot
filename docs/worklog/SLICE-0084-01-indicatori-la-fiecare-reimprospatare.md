# SLICE 0084/01 — the indicator grid on every refresh of an angajament

**Date:** 26.09.2026. **Request (operator):** every time an angajament is refreshed (KbotForm,
RezervariView, ReceptiiView trees), the indicator grid of its FOREXE page (tab0) updates
`FX_Indicatori.Prevedere_Bugetara_Initiala`, `Credit_Bugetar_Initial`, `Angajament_Legal`,
`Credit_Bugetar_Definitiv` for every row. House rule: element ids on the page are not relied on.

## What changed

**Nothing — already covered (verified by reading the code).**

- Server: step 2 of `/api/forexe/prelucrare` (`_step2_indicatori` in
  `PYTHON/routes/forexe/prelucrare.py`) updates exactly those four columns for every row of
  `TabelIndicatori_results` (`_IND_UPDATE_SQL`) and inserts missing rows. Mapping:
  Credit bugetar → Prevedere_Bugetara_Initiala, Total credit angajament → Credit_Bugetar_Initial,
  Angajament legal → Angajament_Legal, Credit bugetar rezervat definitiv an curent →
  Credit_Bugetar_Definitiv.
- The flows that read the grid: «Prelucrare Completa» / «… Reverse» (KbotForm tree) and
  «Rezervari Angajament» (RezervariView tree); «Rezervari Editate» sends the rows the page kept.
- A grid section was drafted for «Receptii Angajament.wfl» and «Receptie Editata.wfl», then
  **reverted at the operator's request** (not needed there).

## Files touched

This worklog and `KBOT_STATUS.md` only.

## Test results

None (no code change).

## Left unverified / deferred

Nothing.

---

## Part 2 — rename `FX_Indicatori.Prevedere_Bugetara_Initiala` → `Credit_Bugetar` (operator, 26.09.2026)

### What changed

- **Server (Python)**, the only four places that name the column:
  `prelucrare.py` (`_IND_INSERT_SQL`, `_IND_UPDATE_SQL`), `prelucrare_pasi.py` (`_REZ_SELECT`,
  alias `R_CreditBug` unchanged), `ddf_edit.py` (alias `Buget` unchanged). The VB client only sees
  the aliases, so nothing changes there.
- **Migrator**: `TableMaps.Forexe` — `FX_Indicatori` keeps its name match plus
  `.Rename("Prevedere_Bugetara_Initiala", "Credit_Bugetar")` (Access keeps the old name).
  KBot.Migrator FileVersion 1.16.0.0 → 1.16.1.0.
- `sql/AVACONT_SURSA.sql` (reference DDL): column renamed, with the `rename:` comment.

### The database side (done by the operator)

Rename in the template WITH the comment, so `proc_SchemaDiff_DDL` generates a RENAME
(step 3a, `CHANGE COLUMN`) in the unit databases and NOT drop + add (step 4 would lose the data):

```sql
ALTER TABLE AVACONT_SURSA.FX_Indicatori
  CHANGE COLUMN `Prevedere_Bugetara_Initiala` `Credit_Bugetar` double NULL DEFAULT NULL
  COMMENT 'rename:Prevedere_Bugetara_Initiala';
```

Then `proc_SchemaDiff_DDL` on the unit databases; check `schema_diff_log` shows RENAME for
FX_Indicatori before `proc_ExecuteSchemaDiff`.

### Files touched

`PYTHON/routes/forexe/prelucrare.py`, `prelucrare_pasi.py`, `ddf_edit.py`,
`src/KBot.Migrator/Transfer/TableMaps.vb`, `src/KBot.Migrator/KBot.Migrator.vbproj`,
`sql/AVACONT_SURSA.sql`.

### Test results

- `py_compile` on the three Python files: green. `dotnet build src\KBot.Migrator` (scratch
  output): **0 errors, 0 warnings**. No tests written or run, nothing live.

### Left unverified / deferred

- ⚠ **Order on deploy:** the three Python files must reach the VPS together with the database
  rename. Either one alone → «Unknown column» (500) on every angajament processing and on DDF edit.
- ⚠ A Migrator run BEFORE the database rename skips the column («ținta «Credit_Bugetar» nu
  există») — the value would not be copied.
- `MariaDB_Schema/` (gitignored dump) still shows the old name until it is refreshed.
