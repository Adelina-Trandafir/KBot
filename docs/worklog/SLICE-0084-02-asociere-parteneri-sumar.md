# SLICE 0084/02 — «Asociază parteneri» on Sumar + `FX_DDF_Parteneri` (operator, 01.10.2026)

Operator request: the Sumar view gets a button that associates an angajament with AT LEAST ONE
partner, if it is not associated already; the association also goes to the DDF when the DDF does
not have it. `FX_DDF` has one pair of columns for the partner (`CodFiscal`, `NumePartener`), so a
document can carry only one — documents with several partners need a table of their own.

Filed under 0084/02 because 0084/01 is the last slice that did something to `SumarView` (the
«Credit Bug.» column came with it). The DDF editor half of the same request is **0094-02**; the
help and the new guided tour are **0000-28**.

## Rules decided with the operator in chat

1. **No DDF → no association, and the button is NOT shown.** The partners live on the DDF; an
   angajament without one has nothing to attach them to. (`SumarView` shows the button only for
   `AngajamentTreeInfo.AreDDF`.)
2. **With a DDF the button is always there, without duplicates**: the window lists what is
   associated and adds others; the same fiscal code is never written twice.
3. Slice numbers: Sumar → 0084/02, DDF editor → 0094-02, the editor's guided tour → 0000-28.

## What changed and why

### Database — `sql/0084_02_fx_ddf_parteneri.sql` (the operator runs it)
- `FX_DDF_Parteneri` (`IdDdfPartener` PK auto-increment, `IDDF` FK → `FX_DDF` ON DELETE CASCADE,
  `CodFiscal`, `NumePartener`, `DataAdaugare`), unique on `(IDDF, CodFiscal)`.
- Keyed on the fiscal code like `FX_DDF` itself (one code can be several `Parteneri` rows, one per
  unit). The name is kept as it was when the partner was associated.
- Backfill: the header partner of every document that has `PartAng = 1` becomes its first row.
- **`FX_DDF.CodFiscal` / `NumePartener` / `PartAng` stay** and mean the document's MAIN partner (the
  one on every section-A / B line and in the signed PDF). The main partner is also a row of the new
  table, so the table alone answers «which partners does this DDF have».

### Server — `PYTHON/routes/forexe/ddf_parteneri.py` (new), registered in `routes/forexe/__init__.py`
- `GET /api/forexe/ddf/parteneri-asociati?cod=` → `{ iddf, parteneri: [ {cod_fiscal, nume_partener,
  din_antet} ] }` (main partner first, added from the header when the table has no row for it);
  404 «Angajamentul … nu are încă un document de fundamentare».
- `POST /api/forexe/ddf/parteneri-asociati` `{ cod_angajament, parteneri[] }` → **add only**: inserts
  the partners the DDF does not have (fiscal codes compared as digits, `anaf.normalize_cf`), makes
  sure the main partner has its row, one transaction. Nothing is removed by it. Needs at least one
  partner («Alege cel puțin un partener.»).
- Two functions shared with `ddf_edit.py` (used by 0094-02): `citeste_parteneri`,
  `sincronizeaza_parteneri`.
- A database WITHOUT the table keeps working: a save that carries only the main partner skips it; the
  POST (and a save that needs a second partner) answers with the name of the .sql file to run.

### Client
- **KBot.Domain** (`DdfDraft.vb`): `DdfPartenerAsociat` (+ the shared key `Cheie` = digits of the
  fiscal code), `DdfParteneriAsociati`.
- **KBot.Api**: `IDdfParteneriApi` + `ApiClient.DdfParteneri.vb` — a separate interface, like
  `IDdfProgramApi`, so the test fakes of `IApiClient` need not learn it.
- **KBot.App**: `Views/Ddf/DdfPartnersView` (the list, the picker, «Asociază», «Scoate din
  asociere»; shared with the editor page, 0094-02), `Views/SumarPartnersForm` (the window; keeps the
  picked partners marked «De adăugat» until «Salvează», asks before closing with unsaved ones) +
  `SumarPartnersReauth` (the 401 net, a parameter object like `DdfEditReauth`), `SumarView` (button
  `btnPartners`, hidden unless the angajament has a DDF), `KbotForm.Views.vb` (wiring).

## Files touched
`sql/0084_02_fx_ddf_parteneri.sql` (new), `PYTHON/routes/forexe/ddf_parteneri.py` (new),
`PYTHON/routes/forexe/__init__.py`, `src/KBot.Domain/DdfDraft.vb`,
`src/KBot.Api/IDdfParteneriApi.vb` (new), `src/KBot.Api/ApiClient.DdfParteneri.vb` (new),
`src/KBot.App/Views/Ddf/DdfPartnersView.vb` + `.Designer.vb` (new),
`src/KBot.App/Views/SumarPartnersForm.vb` + `.Designer.vb` (new),
`src/KBot.App/Views/SumarView.vb` + `.Designer.vb`, `src/KBot.App/KbotForm.Views.vb`.
`FileVersion` not bumped again: KBot.Api 1.0.16.0, KBot.App 1.1.1.1 and KBot.Domain 1.2.8.0 were
already bumped, uncommitted, for the current release.

## Test results
- `py_compile` on `ddf_parteneri.py`, `ddf_edit.py`, `__init__.py`: green. The two pure helpers
  (`cheie_cf`, `_curata`) were called by hand: «RO 123» = «123», duplicates and empty codes dropped.
- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- No tests written or run (house rule). The window and the button were **not run and not seen on
  screen**; the server was not started; the DDL was not applied anywhere.

## Left unverified / deferred
- ⚠ **Order on deploy:** apply `sql/0084_02_fx_ddf_parteneri.sql` on every unit database (000_DEMO
  first) AND on `AVACONT_SURSA`, then put `ddf_parteneri.py`, `ddf_edit.py` and `__init__.py` on
  the VPS (restart). Without the table the button's «Salvează» answers with the file name.
- **Interpretation to confirm:** «the association also goes to the DDF if it is not already there» was
  read as: the partners are written to the DDF's own list (`FX_DDF_Parteneri`), skipping the ones it
  has. The DDF HEADER (`FX_DDF.CodFiscal` / `PartAng`) is NOT touched from Sumar: flipping `PartAng` on
  an existing, possibly signed, document would change the lines, the ORD flow and the signed PDF. If
  the first associated partner should also become the header partner of a DDF that has none, that is
  one more call in `adauga_parteneri`.
- The «Parteneri» window's rule «a partner with activity cannot be deleted» (`_ACTIVE` in
  `parteneri_edit.py`) does not know the new table yet; adding it would break that window on a
  database without the table. A partner associated only through this table can still be deleted.
- Only the main partner is written on the section-A / B lines; choosing a partner per line among the
  associated ones is not part of this request.
