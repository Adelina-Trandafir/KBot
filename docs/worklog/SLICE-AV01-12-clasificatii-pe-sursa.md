# SLICE-AV01-12 - «Clasificatii» follows the source of the title bar (operator, 26.10.2026)

## What it is
On one source of the title bar both pages show only that source's classifications; on «Toate» they show every source. `Clasificatii_Venituri` got a source column; everything transferred so far is on 02E.
1. **«Cheltuieli»** (`ClasificatiiBugetView`): `Bind` gets the session. The catalogue already carries `Clasificatie.Ss`, so the filter is on the client (`ScopeItems`, under the two existing filters «Arata toate» and «DOAR FOREXE»). When the
   source changes (`SessionContext.PeriodChanged`) what is typed is settled first (Yes / No / Cancel), then the tree is rebuilt; the open classification stays if still visible, otherwise the grids empty. A year change alone does not touch the
   tree. The status line says «(SS)»; a source with nothing says so. The check window's double click on a classification of another source says so instead of doing nothing.
2. **«Venituri»**: column `Clasificatii_Venituri.SS`. Server: `GET /api/avacont/clasificatii-venituri?ss=` (none or `*` = every source; items carry `ss`), `GET .../nomenclator?ss=` (`exista` per source, a concrete source is required),
   `POST .../adauga {clsfv, ss}` (duplicate checked per source). Client: `IClasificatiiVenituriApi` takes the source on those three calls; `ClasificatieVenit.Ss`. `ClasificatiiVenituriView` reads the tree for the title bar's source and
   re-reads it when the source changes; on «Toate» with several sources a leaf shows «cod (SS)»; «+» on «Toate» says to choose a source.
3. **DDL** `sql/AVACONT/Configurari/ClasificatiiVenituri/AV01_12_clasificatii_venituri_ss.sql` (run): `SS varchar(16) NOT NULL DEFAULT '02E'` (the schema sync gives 02E to the existing rows of the unit databases by itself) and the unique
   key `UK_Clasificatii_Venituri_CSP` replaced by `UK_Clasificatii_Venituri_SS_CSP (SS, Capitol, SubCapitol, Paragraf)`. The default stays on the column on purpose (older writers insert without SS).

## Why a unit does not see 02E in the source combo
The combo is filled by `GET /api/auth/periods` (`routes/auth/auth.py`, `auth_periods`) from `AVACONT_COMUN.Unitati_Ani` (`DC`, `AN`, `SS`, `CodProgram`), one row per year and source, plus the «Toate» row added by the client
(`KbotForm.Periods.vb`). In the dump of 22.09.2026 the units 000_DEMO, 112_RSLS and 114_CMNS have no 02E row. Nothing in the code hides 02E: the row is added to `Unitati_Ani` when needed (operator).

## Files touched
`PYTHON/routes/avacont/configurari/clasificatii_venituri/clasificatii_venituri.py`; the DDL above; `src/KBot.Domain/.../ClasificatieVenit.vb`; `src/KBot.Api/.../IClasificatiiVenituriApi.vb`, `ApiClient.ClasificatiiVenituri.vb`;
`src/KBot.App/Views/Nomenclatoare/ClasificatiiBugetView.vb`, `ClasificatiiForm.vb`; `src/KBot.App/Avacont/Configurari/ClasificatiiVenituri/Views/ClasificatiiVenituriView.vb`, `ClasificatiiVenituriAddForm.vb`; help (0000-132).

## Test results
`dotnet build` of `KBot.App`: 0 warnings, 0 errors. `py_compile` of the route. Tested and run by the operator (26.10.2026).

## Unverified / deferred
The readers of `Clasificatii_Venituri` that had no source (`extrase.py`, the Access migration) are handled in AV01-13.
