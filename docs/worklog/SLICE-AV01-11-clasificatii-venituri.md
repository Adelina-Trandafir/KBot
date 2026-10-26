# SLICE-AV01-11 - «Clasificatii»: vertical bar «Cheltuieli» / «Venituri» and the revenue page (operator, 26.10.2026)

## What it is
1. **The window is a shell.** `ClasificatiiForm` holds the title bar, the vertical bar (`navClasificatii`, a `KBotNavList` like `navViews`: collapsible, flyout, corner radius 8; icons `ef_iesiri` for «Cheltuieli», `ef_intrari` for «Venituri») and a
   host panel with the two views. Before the other page takes over, and before the window closes, the page in front settles what it has pending (`IClasificatiiPage.CommitAsync`: Yes / No / Cancel; Cancel puts the bar back).
2. **«Cheltuieli» = `ClasificatiiBugetView`** (the expenses classifications with their budget and corrections, as the window was before the bar).
3. **«Venituri» = `ClasificatiiVenituriView`** (`Avacont/Configurari/ClasificatiiVenituri/Views/`): tree Capitol > Subcapitol > Paragraf (names of the two upper levels from `DefaClsfV`; sub-chapter `00` hangs straight under the chapter), the
   same two grids as «Cheltuieli» (budget VERSIONS with «Inceput», corrections with document + date), the same summary at a node above the leaves, the same row under the grids, «Salveaza» in one transaction. No check boxes under the tree, no
   «Verifica bugetul», no «Trimite in Access». «+» in the tree footer opens `ClasificatiiVenituriAddForm` (a tree of the `DefaClsfV` codes of type VN; the name comes from the nomenclator in capitals; an existing code shows «(exista deja)»).
4. **The budget belongs to a unit**: the sector+source of the title bar and the working year name it; the server finds the non-hidden row of `Unitati` (none or several = a Romanian refusal). On «Toate» the tree shows but the grids stay
   empty with a message. A change of year or source (`SessionContext.PeriodChanged`) settles what is typed first; the page always saves for the year + source it READ (`_scopeAn`, `_scopeSs`), not for what the title bar says now.
5. **Child subunits work on the parent's data** (operator: the parent has the SAME sector+source, so it is one unit). Nothing is blocked; the footer names the child subunits of the source (`PYTHON/utils/subunitati_copil.py`
   `child_sources`, advice only: a database without `TZ_Subunitati` / `IdSubunitateParinte` (errors 1146 / 1054) gets no notice, never an error, so `AV01_04d` is not required). If data ever has a child whose parent has ANOTHER source,
   the page still uses the child's own source.
6. **Server** `PYTHON/routes/avacont/configurari/clasificatii_venituri/clasificatii_venituri.py`: `GET /api/avacont/clasificatii-venituri` (+ `/nomenclator`, `/sumar`, `/<id>/buget`), `POST /adauga`, `POST /<id>/buget`. The key of a new
   classification is the largest + 1 inside the transaction (the column is a plain int on the server). Client: `IClasificatiiVenituriApi` + `ApiClient.ClasificatiiVenituri.vb` (reuses the wire shapes and `ReadBudget` of the expenses window).
7. **DDL** `sql/AVACONT/Configurari/ClasificatiiVenituri/AV01_11_clasificatii_venituri_buget.sql` (run): new `Clasificatii_Venituri_Buget`; `Clasificatii_Venituri_Rectificari` Trim1..4 `varchar` -> `double`, new `IdUnitate`,
   unique (IdClsfV, IdUnitate, Data, Document); unique (Capitol, SubCapitol, Paragraf) on `Clasificatii_Venituri` (later replaced, see AV01-12).

## Trap: schema sync
Statement 2 of `AV01_11` adds a unique index that starts with `IdClsfV`; MariaDB silently drops the older FK index `Clasificatii_Venituri__Clasificatii_Venituri_Rectificari` from the template, so the sync planned a `DROP INDEX` on every unit
database and it failed (errno 1553: the unit database's FK still needs it; the sync drops before it adds). Statement 2b puts the index back in the template (a redundant index is harmless).

## Files touched
`src/KBot.App/Views/Nomenclatoare/{ClasificatiiForm,ClasificatiiBugetView,IClasificatiiPage}` (+ Designers); `src/KBot.App/Avacont/Configurari/ClasificatiiVenituri/Views/{ClasificatiiVenituriView,ClasificatiiVenituriAddForm}` (+ Designers);
`src/KBot.App/KbotForm.Nomenclatoare.vb`; `src/KBot.Api/.../ClasificatiiVenituri/{IClasificatiiVenituriApi,ApiClient.ClasificatiiVenituri}.vb`; `src/KBot.Domain/.../ClasificatiiVenituri/ClasificatieVenit.vb`;
`PYTHON/routes/avacont/configurari/clasificatii_venituri/`, `PYTHON/routes/avacont/__init__.py`, `PYTHON/utils/subunitati_copil.py`; the DDL above; help (0000-131).

## Test results
`dotnet build` of `KBot.Api` and `KBot.App`: 0 warnings, 0 errors. Python: `py_compile`. Tested and run by the operator (26.10.2026).

## Unverified / deferred
- Still to confirm: «Toate» leaves the grids empty; only codes of type `VN` can be added (3 revenue codes without a `Tip` - `140310`, `141310`, `333400` - are not offered until `DefaClsfV.Tip` is filled).
- No modify of a classification (operator: no point); delete is in AV01-13. No export to Excel, no copy of last year's budget. No role check on the new routes (same as the other AV routes).
- `Config/mesaje_catalog.json` still names the old `ClasificatiiForm.*` members for the messages that moved into `ClasificatiiBugetView`; they find no entry (the code's text is shown). The Debug catalog window regenerates it.
- Stale before this slice, not touched: the tutorial `parteneri.md` still has a step on `chkFaraActivitate` (removed by AV01-10); `Check-Help` reports it.
