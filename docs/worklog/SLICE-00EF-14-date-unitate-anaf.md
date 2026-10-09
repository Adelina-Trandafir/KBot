# SLICE-00EF-14 — Unitati_Date, ANAF reluabil si automat

## What changed and why
- Table `AVACONT_COMUN.Unitati_Detalii` renamed `Unitati_Date` (operator's name); `Unitati_Conturi` unchanged. Both were already
  moved out of the unit databases by 00EF-13; no `FX_Furnizor` / `EF_Furnizor` table is created anywhere any more (only the
  commented copy block at the bottom of `sql/00EF_13_unitati_detalii.sql` names the old ones).
- The ANAF take is no longer one-time: `AnafPreluat` removed (column, server, wire DTO, `EFacturaFurnizor`), `ANAF_DEJA_PRELUAT`
  gone; `DataAnafPreluat` -> `DataAnaf` (last take).
- `DateUnitateForm`: on opening, an empty name runs ANAF by itself (tax code from `Unitati.CF`, no question); the button
  «Preia de la ANAF» stays always enabled and asks before it overwrites name/county/city/address.
- The take now also fills county (ISO code) and city: `anaf._fields` reads `adresa_sediu_social` (`scod_JudetAuto`,
  `sdenumire_Judet`, `sdenumire_Localitate`); `routes/efactura/adresa_anaf.py` maps it, with the one-line address as fallback.
  `furnizor_mark_anaf` keeps the old county/city when ANAF gives none.
- Help (`contabil/efactura/index.md`, «Date unitate») updated, tag `00EF-14`.

## Files touched
`sql/00EF_02/06/12/13*.sql`, `PYTHON/routes/efactura/{facturi,facturi_store,factura_routes,validare,ubl,__init__}.py`,
new `adresa_anaf.py`, `PYTHON/routes/inregistrare/anaf.py`, `src/KBot.Api/ApiClient.EFactura.Facturi.vb`,
`src/KBot.Domain/EFacturaFacturi.vb`, `src/KBot.EFactura/Forms/DateUnitateForm.vb`, `src/KBot.Migrator/EFactura/EfImporter.vb`,
help `index.md`.

## Test results
KBot.EFactura builds, 0 warnings. `adresa_anaf.county_and_city` checked by hand on three samples (PH/Ploiesti, B/SECTOR1,
address fallback). No test suites run (not asked).

## Unverified / deferred
- The ANAF field names `scod_JudetAuto` / `sdenumire_Judet` / `sdenumire_Localitate` are from ANAF's v9 documentation, NOT seen
  in a live answer from this repo; the address-string fallback covers a miss. Check once against a real CUI.
- The SQL is NOT run anywhere (table never created live), so the rename needs no migration; the file name
  `00EF_13_unitati_detalii.sql` was kept.
- Not done: `docs/worklog/KBOT_STATUS.md` / `state/` update, commit.
