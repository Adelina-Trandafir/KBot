# SLICE-0121-04 - Hooks: partner <-> DDF / angajament association fills the supplier's link (19.10.2026)

Plan: [SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md](SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md). Needs 0121-01 run and synced.

## What changed and why
When a supplier is associated with a document, its link with the document's angajament - and the angajament's indicators - are written if missing, so the operator does not have to type them in the «Parteneri» window.

- `partner_links.py`:
  - `ensure_link(cursor, cod_fiscal, cod_angajament)`: finds or inserts the link `(normalized CodFiscal, CodAng)`, then inserts every indicator of `FX_Indicatori.CodAngajament` that the link does not hold (`CodAI`, `CodInd = CodIndicator`, no bank account). **Insert only**: an existing link/indicator is never changed (the `ContBancar` and `CodInd` the operator saved stay), nothing is deleted.
  - `ensure_for_partners(cursor, db_name, cod_angajament, cods_fiscale)`: the DDF hook; skips silently (a warning in the log) when the database has no link tables, so a DDF save never depends on 0121-01 being deployed.
  - `tables_present`: positive-only cache, like `FX_DDF_Parteneri`'s probe.
  - New route `POST /api/forexe/parteneri/angajament {cod_fiscal, cod_angajament}` -> `{id_leg, legatura_noua, indicatori_adaugati}` (400 no fiscal code / tables missing - names the SQL file, 404 unknown angajament): the save the page «Recepții» will call in 0121-05. Journalled in `actiuni.py` («Legat un furnizor de un angajament»).
- `ddf_parteneri.py`: `sincronizeaza_parteneri` (the DDF editor's save) and `adauga_parteneri` (the Sumar button) take `cod_angajament` and call the hook for the partners they store / add. `parteneri is None` (an older client that sent no list) still touches nothing.
- `ddf_edit.py`: the save passes the document's `cod`; `post_ddf_parteneri_asociati` passes the code of the request.
- Runs inside the caller's transaction and only on a SAVE - a mere view/GET writes nothing. A database error in the hook is not swallowed (it fails the save, as any other write of it would).

## Files touched
`PYTHON/routes/forexe/partner_links.py`, `ddf_parteneri.py`, `ddf_edit.py`, `PYTHON/routes/actiuni.py`.

## Test results
`py_compile` of the four files: OK. **No test written or run, no database touched** (operator: no tests).

## Left unverified or deferred
- Never run: the SQL, the transaction behaviour and the link counts are unverified. The partners named by a save get the link; the plan said «partner <-> angajament» too - that is the new route, which nothing calls until 0121-05.
- Backfill: a DDF whose partners were associated before this slice gets its links the next time its partners are saved; the Migrator (0121-02) covers the old `ParteneriAng` rows.
- No client and no help change (nothing the operator sees). Server files go to the VPS by the operator's hand.
