# SLICE 0081-10 — Classification without «.02», the partner list, the header of revision 0

**Date:** 26.09.2026. **Request (operator), in the DDF editor:**
1. In the classification combo «65.02» must be just «65». The «02» after the chapter is an Access
   relic: not shown, and not filled into the sending workflows. Change only the row window and the
   workflow inputs.
2. The partners in `DdfEditForm`:
   1. «Angajament nou» + tick «Partener asociat»: the list is empty.
   2. A new document on an existing angajament: the «Partener asociat» box is disabled.
   3. A NEW document (revision 0) on an angajament DOWNLOADED from forexecab: everything but the
      compartment is disabled. It worked before 0081-09.

## What was wrong

- **(1)** `Clasificatii.Clsf` is `concat_ws('.', Capitol, Subcapitol, Articol, Alineat)`, and
  `Capitol` is Access's «65.02». The send passed it through: `Clasificatia` = «65.02.04.02.20.01.01»
  and `ClsfSal` = «65020402200101», while the workflows expect «65.04.02.20.01.01» /
  «650402200101» (their own headers say so; the table's `ClsfSal` column already drops it).
- **(2.2, 2.3)** 0081-09 locked the header whenever `RevizieNoua` was set on an angajament not
  created now. `ForFirstRevisionOfExisting` (revision 0 of a downloaded angajament) is a NEW
  document with `RevizieNoua = True`, so it was locked too; only the compartment escaped, through
  the "open while empty" exception.
- **(2.1)** `/api/forexe/ddf/parteneri` scoped partners to the units of the angajament's
  `FX_Indicatori` rows. A new angajament (code «!…») has none yet, so nothing matched.

## What changed

- **Domain:** `DdfSendInputs.ForexeClsf` drops the second dotted part only when the text has
  exactly seven parts (the Access shape); anything else comes back trimmed, unchanged.
  `DdfSendLine.FromDraft` uses it, so `Clasificatia`, `ClsfSal` and the difference messages all get
  forexecab's form. Stored data (`FX_DDF_SA.Clsf`) is untouched.
- **`DdfEditLinieAForm`:** the combo items use `ForexeClsf`; the mask is `00.00.00.00.00.00`.
- **`DdfEditForm.AplicaEnablement`:** the lock is back to "a new revision on a document that
  already exists" (`RevizieNoua AndAlso Not Nou`) -- which Access's rule already locked. Revision 0
  of a downloaded angajament and the change of an existing revision follow Access's rule again.
  The "open while empty" exception for compartment / object stays.
- **Server** (`ddf_edit.py`, `_SQL_PARTENERI`): when the angajament has no indicator with a unit,
  every unit of the database is offered (the database is one DC).

## Files touched

- `src/KBot.Domain/DdfSendInputs.vb`
- `src/KBot.App/DDF_EDIT/DdfEditLinieAForm.vb`, `DdfEditLinieAForm.Designer.vb`, `DdfEditForm.vb`
- `PYTHON/routes/forexe/ddf_edit.py`
- `tests/KBot.Domain.Tests/DdfSendInputsTests.vb`

## Test results

- `dotnet build src\KBot.App` (scratch folder): **0 errors, 0 warnings**. `ddf_edit.py` compiles.
- The row window rendered off-screen: items read «65.04.02.20.01.30 — …».
- VB tests written, **not run**. The partner SQL change was **not run** against a database.

## Left unverified / deferred

- The section-A grid (column «Clsf») still shows the stored «65.02…» -- the request limited the
  change to the row window and the workflows.
- `ddf_edit.py` (this change and 0081-09's route) must be deployed to the VPS.
- The header enablement was not seen on screen: the editor needs the API to open.
