# SLICE-00EF-01 - analysis and plan for the new E-Factura app

Slice 00EF (operator, 06.10.2026). Documents only: no code, nothing run.

## What changed and why
- Read the raw Access export (`Surse/RawExport`: 250 forms, 357 tables, 559 queries, 116 modules, 81 classes) with a script
  (`tools/AccessMap/extract.js`) and wrote the relationship maps for the E-Factura area to `docs/access-map/efactura/`
  (overview, one page per form with every control, tables, queries with SQL, code, navigation, form x table matrix, outside
  dependencies). Generator: `tools/AccessMap/build-efactura.js`.
- Wrote `PLAN_00EF_EFactura.md`: the operator's decisions, how the Access side works for issued invoices (verified from the export),
  the target shape (UI app + Flask server that holds the ANAF token + MariaDB), the sub-slice list 00EF-02..09 and five open questions.
- Registered the slice in `KBOT_STATUS.md` and `state/KBOT_STATUS_0000-0009.md` (next to 000T, which also has a letter id).

## Files touched
- New: `docs/access-map/efactura/**`, `tools/AccessMap/extract.js`, `tools/AccessMap/build-efactura.js`,
  `docs/worklog/PLAN_00EF_EFactura.md`, this file.
- Edited: `docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`.

## Test results
None run. The generator output was read and spot-checked against the raw files (forms, handlers, writes found by the scan,
the token functions in `mdl_2025`, `mdl_2026`, the upload/status code in `mdl_EFactura`).

## Unverified / deferred
- The scan reads text: SQL built from variables and forms opened by a name in a variable are not seen; "reads/writes" are leads.
- The maps were generated before the operator's scope decisions, so they still include accounting objects and `_`/`inlucru` copies;
  the plan says what to ignore.
- Not read: `RES\EF.EXE` (token renewal), the contents of the `Scheme` table, any row data.
- The Access screens were never seen on screen; control positions are from the export (`LayoutCached*`).
- Open questions are in `PLAN_00EF_EFactura.md`.
