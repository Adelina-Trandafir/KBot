# SLICE-0122-11 - Parteneri without activity missing from the Parteneri window and the DDF partner combo

Date: 03.11.2026. Sub-slice of 0122 (the query 0122_08 is what hid the partners).

## What was reported

In «Parteneri», ticking «Arata partenerii fara activitate» shows nothing new, and the partners without activity are also gone
from «Asociere parteneri cu DDF», where they MUST be offered.

## Cause (read from code, NOT confirmed on a database)

Two server queries left out every `Parteneri` row with `Ascuns = 1`:

- `GET /api/forexe/nomenclatoare/parteneri` (`parteneri_edit.py`, `_SQL_PARTNERS`: `COALESCE(P.Ascuns, 0) = 0`)
- `GET /api/forexe/ddf/parteneri` (`ddf_edit.py`, `_SQL_PARTENERI`: `COALESCE(P.Ascuns, 0) = 0`)

The one-time query `sql/0122_08_interogare_unica.sql` sets `Ascuns = 1` on every partner nothing uses. So after it ran, exactly the
partners WITHOUT activity were hidden: the window never received them (the client choice `_showNoActivity` in
`ParteneriForm.IsListed` was correct and had nothing to show), and the DDF combo never offered them.
Unverified: that the query was run on the operator's database (no database access in this task).

## What changed

- `PYTHON/routes/forexe/parteneri_edit.py`: the list no longer filters on `Ascuns`; hidden rows are sent with `"ascuns": true` (the
  client already draws them italic and dim). The rows are ordered visible first, then by `IdPartener`, so when one fiscal code has a
  visible and a hidden row the visible one is the one kept by the one-per-fiscal-code rule. Module docstring and the comment above
  `_SQL_PARTNERS` updated.
- `PYTHON/routes/forexe/ddf_edit.py`: `_SQL_PARTENERI` no longer filters on `Ascuns`; comment added.

No client (VB.NET) change: `IsListed` already shows `Activ` partners always and the others when the choice is ticked;
`DdfEditForm` and `SumarPartnersForm` do not filter the combo list.

## Files touched

- `PYTHON/routes/forexe/parteneri_edit.py`
- `PYTHON/routes/forexe/ddf_edit.py`
- `docs/worklog/SLICE-0122-11-parteneri-ascunse-fara-activitate.md` (this file)
- `docs/worklog/state/KBOT_STATUS_0120-0129.md`

## Test results

The operator tested it on 03.11.2026 and confirmed it works. Before that only `ast.parse` on both files.

## Left unverified / deferred

- A partner the operator hid BY HAND cannot be told apart from one hidden by `0122_08`: it now also appears in the DDF combo and,
  when the choice is ticked, in the Parteneri list (and is italic/dim there).
- VERCON is NOT changed: it has its own supplier query (`utils/vercon_suppliers.py`) that still excludes hidden partners.
- The two server files are NOT deployed by me (the operator publishes). They are registered with `tools\Register-DeploymentTask.ps1`
  as SLICE-0122-11, after the operator tested and confirmed (03.11.2026).
- Help not changed: the choice and the window look the same.
