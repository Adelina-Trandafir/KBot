# SLICE-AV01-15 - SablonSalarii: the salary templates (operator, 27.10.2026)

## What changed and why
The Access button OCS (form `OCS`, tables `DefaSalarii` + `DefaSalariiVA` + `DefaSalariiPC` + `DefaSalariiPN`) is ported as **SablonSalarii**, as the third page («Salarii») of the
«Sabloane» window of AV01-14, laid out like the Access screen the operator sent: the chapter and the list of articles on the left; on the right the classification of the article and
four tables (commitment, retentions, viraments, cash payments) with the columns CM / Explicatie / C. Debit / P. Debit / C. Credit / P. Credit.

1. **DDL** `sql/AVACONT/Configurari/SablonSalarii/AV01_15_sablon_salarii.sql` (on `AVACONT_SURSA`, then schema sync): `TZ_SablonSalarii` (article of a subunit: classification NN.NN.NN,
   explanation, base debit / credit account + partner mode) and `TZ_SablonSalariiLinie` (kind RETINERE / VIRAMENT / NUMERAR, `IdParinte` = the retention a virament pays, `Ordine`, `EsteCM`).
   Lines go with their article (CASCADE); the parent link is SET NULL. No `Activ` column. No year column.
2. **Server** `PYTHON/routes/avacont/configurari/sablon_salarii/sablon_salarii.py`: `GET` / `POST /api/avacont/sabloane-salarii`. One transaction; the lines of an article are updated by id, added, or
   deleted when not sent; a `ref` number per line lets a virament name a retention that is not saved yet. Rules: an article needs at least ONE virament (operator, 23.10.2026); a retention's debit is
   forced to the article's credit; at most one CM per article and kind; accounts / partners must exist; classification NN.NN.NN.
3. **Client** `KBot.Domain/Avacont/Configurari/SablonSalarii/SablonSalariiInfo.vb`, `KBot.Api/.../ISablonSalariiApi.vb` + `ApiClient.SablonSalarii.vb`.
4. **Window** `Avacont/Configurari/SablonSalarii/Views/SablonSalariiView` (+ Designer, generated once from `gen-sal-designer.js` and hand-kept after), a third item «Salarii» in the bar of
   `SabloaneForm` (`KbotForm.Sabloane.vb` passes the second API). The chapter combo lists the subunits of the title bar's source; articles in a flat list under «+ adauga»; the partner cell holds a
   partner CODE, `XXX` (the partner of the operation) or nothing; the debit cells of the retentions mirror the article's credit (read-only); a new virament suggests account 7700000.
5. **Migration** `AVC.Migrator` step `SablonSalarii` (after Subunitati): reads the four Access tables; article -> the subunit of its unit (chapter `Cap` must match); hidden rows (`Ascuns`) are not
   migrated; base account = first seven digits; PC keeps the link to its VA (`IDVAA`); an article with anything the target would refuse (no virament, account or partner missing, chapter mismatch)
   is left out whole and named in the verification; an existing article (same subunit, classification, explanation) is not overwritten.
6. **Help** 0000-135 (section «Salarii» in `contabil/nomenclatoare/sabloane.md`, label `AV01-15`).

## Files touched
`sql/.../AV01_15_sablon_salarii.sql`; `PYTHON/routes/avacont/__init__.py`, `.../sablon_salarii/{__init__,sablon_salarii}.py`; `SablonSalariiInfo.vb`, `ISablonSalariiApi.vb`, `ApiClient.SablonSalarii.vb`;
`SablonSalariiView.vb` + `.Designer.vb`, `SabloaneForm.vb` + `.Designer.vb`, `KbotForm.Sabloane.vb`; `AVC.Migrator/AvcCatalog.vb`, `Configurari/SablonSalarii/AvcSablonSalariiStep.vb`; help `sabloane.md`, `nomenclatoare/index.md`.

## Test results
`dotnet build` of `KBot.Api`, `KBot.App`, `AVC.Migrator`: 0 warnings, 0 errors. `py_compile` of the Python files: ok. No tests run (operator rule); nothing seen on screen; the DDL, the routes and the
migration step were never run.

## Unverified / deferred
- **DDL not run.** The page cannot work until `AV01_15` is run on `AVACONT_SURSA` and the schema synced.
- Nothing seen on screen: layout and row heights of the right-hand table, the four grids with combo columns (about 1000 accounts, all partner codes), the retention-debit mirror, the CM exclusivity.
- A partner code is shown alone; `Parteneri.CodPartener` is unique per unit, so a database with several units can hold the same code twice - the window then takes the first one.
- The Access column names (`IDS`, `Clsf`, `Explicatie`, `ContD`, `PartD`, `ContC`, `PartC`, `Cap`, `Ascuns`; VA `IDVA`, `IDD`, `CM`; PC `IDPC`, `IDVAA`; PN `IDPN`) come from the reading sheet
  `sablon-operatii-ocs.md`, not from a run against the file; `Clsf` is assumed NN.NN.NN (six digits are reformatted).
- Not ported, on purpose: `GRUPA` and `Articol` of PN, `ContB` of VA, `SRT` / `Sef` / `Proiect`; automatic creation of a virament for every new retention (Access did it); the red warnings of the
  Access form (retention without virament); the treasury account as a unit setting (7700000 is only a suggestion on a new virament); «Duplica articolul».
- Open questions of the sheet that stay open: how the CM lines are calculated, what the two cash rows mean, a block (not only a warning) for a retention without a virament.
