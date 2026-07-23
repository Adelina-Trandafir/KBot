# SLICE-0017-02 — server: `GET /api/forexe/plati?cod=`

Pass 2 of slice 0017 (Plăți). The raw-reader endpoint the client (pass 03) shapes into the
tree + LISTA + bank-statement detail pane.

## What changed and why

New `PYTHON/routes/forexe/plati.py`, registered in `routes/forexe/__init__.py`.
`@require_session`, one parameter `cod` (`CodAngajament`); no `db_name` / `id_unitate` — the
database is the session's (one database = one unit), exactly like `receptii.py`.

One row per `FX_Plati` record, each carrying its bank statement (`FX_Extrase`) and an
`are_ord` flag. The client derives the three-level tree, the filtered LISTA and the detail pane
from this single reader.

## Decisions taken / deviations from the plan

- **Classification via `FX_Indicatori` (the 0011-03 path), NOT the plan skeleton's
  `p.IdClsf`.** The plan's SQL skeleton keys `Clasificatii` directly on `p.IdClsf`
  (`WHERE c.<clsf_pk> = p.IdClsf`), but the plan's own Step 0 note says *"match whatever
  sumar.py / receptii.py already do."* Those go **P → `FX_Indicatori` (join on `CodAI`, its PK
  → 1:1, no fan-out) → `Clasificatii` scalar subquery on `IdClsfAcc = I.IdClsf AND IdUnitate =
  I.IdUnitate` with `LIMIT 1`**, because `FX_Indicatori.IdClsf` is the *verified* Access id
  (STATUS Locked decision) and `FX_Plati.IdClsf`'s direction is unverified. This is a genuine
  deviation from the plan's literal SQL, resolved by the plan's Step 0 instruction. The row
  returns **both** `clsf`/`denumire` (nomenclator) and `clsf_plata` (raw `FX_Plati.Clsf`); the
  client falls back to `clsf_plata` when `clsf` is empty.
- **`LEFT JOIN FX_Extrase`, not the Access `FX_Extrase_H INNER JOIN FX_Extrase`.** The Access
  detail query inner-joins the statement header but selects nothing from it; an inner join would
  drop a payment whose extras row has no header. Left join keeps the payment (detail pane shows
  its empty state).
- **`are_ord` against a `DISTINCT` derived table** (`SELECT DISTINCT IdPlataFX FROM
  FX_ORD_TBL_REC`), so a payment on several ordonanțare lines cannot duplicate the payment row.
  Tested explicitly (`test_are_ord_not_duplicated_by_multiple_ord_lines`).
- **`ORDER BY P.Data_plata, P.IdPlataFX`** — the client relies on it for month/day/leaf order
  and for picking the *oldest* un-ordonanțat day (the «+»). `IdPlataFX` is the stable tiebreaker.
- **`data_doc` passed as a raw string, not ISO.** `FX_Extrase.DataDoc` is a `varchar` in the
  schema (not a date), so `_iso` would mangle it — it goes through untouched. `Data_plata` /
  `DataBanca` are real `DATETIME` → date-only ISO (the tree groups on the day).
- **Money coalesced to `0.0` server-side** (`suma`, `suma_debit`, `suma_credit`), booleans as
  real booleans, `ensure_ascii=False`. A DB error returns a reason-coded 500 with a Romanian
  message, never an empty list.

## Files touched

- `PYTHON/routes/forexe/plati.py` — **new** route.
- `PYTHON/routes/forexe/__init__.py` — register `plati`.
- `PYTHON/tests/test_forexe_plati.py` — **new**, host-only, skips off-host.

## Test results

- `py_compile` clean on `plati.py`, `__init__.py`, `test_forexe_plati.py`.
- Full offline Python suite: **75 passed, 13 skipped**, 0 fail/error. `test_forexe_plati.py`
  skips off-host at collection (no `config.py`), as designed.

## Left unverified / deferred

- **The endpoint has never touched a live database.** All 15 tests are host-only and skip
  off-host. Run `python -m pytest tests/test_forexe_plati.py` on the Flask host to confirm the
  seven tables/columns exist and that the `FX_Indicatori` join yields a populated `clsf` — if
  `clsf` comes back blank on every row, the cause is the join key, not the view (the 0011-03
  trap), and `clsf_plata` is the fallback.
- **`FX_Extrase` 1:1 with `Referinta` is an operator statement, not a schema constraint.** The
  guard is `test_row_count_equals_fx_plati_count` (returned rows == `COUNT(*)` from `FX_Plati`);
  if it fails on real data, the `LEFT JOIN FX_Extrase` is fanning out and needs a de-dup.
