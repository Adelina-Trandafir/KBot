# SLICE-02EF-01 - «Receptii» of the classification table = all receptions of the classification (06.11.2026)

## What changed and why
Window «Creeaza receptia» (the former reception window), table of the classifications: the column **Receptii** showed only the receptions of THIS angajament taken before the day
(`SUM(Valoare)` grouped by `CodIndicator`). Operator: it must be ALL the receptions of the classification. Answer chosen (06.11.2026): the whole classification,
every angajament, still only those dated BEFORE the reception's day (so «Disponibil» = Buget - Receptii stays right when a reception is modified).

`PYTHON/routes/efactura/primite_ke.py`, function `disponibil()`: the sum is now `SELECT IdClsf, SUM(DIF) FROM FX_Receptii WHERE IdClsf IN (classifications of the
angajament) AND DATE(Data) < day GROUP BY IdClsf`, read per `IdClsf` (the same key the budget uses). `SUM(DIF)` (the delta of each snapshot) replaces `SUM(Valoare)`:
every re-posted reception adds a new row, so `Valoare` counted the same money again (rule already used by Sumar and `ddf_edit.py`). The client is unchanged (it reads
`Receptii` per `IdClsf`). Help: the sentence about «Recepții» in `contabil/efactura/index.md` (0000-153).

## Files touched
`PYTHON/routes/efactura/primite_ke.py`; `src/KBot.App/HelpContent/contabil/efactura/index.md`; this worklog, `SLICE-0000-153-...md`, `state/KBOT_STATUS_02EF.md`,
`state/KBOT_STATUS_0000-0009.md`, `KBOT_STATUS.md`.

## Test results
`py_compile` OK. Operator checked the result (06.11.2026). No automated test run; the live query was not run by me.

## Unverified / deferred
- The `DIF` vs `Valoare` change is mine, not asked: the figure may differ from the old one by more than the extra angajamente.
- `FX_Receptii.IdUnitate` is not filtered (one database = one unit).
- Receptions of the CURRENT angajament dated on or after the day stay out of the sum, as before.
