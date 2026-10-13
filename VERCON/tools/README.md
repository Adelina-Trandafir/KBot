# VERCON/tools

Generatoare pentru livrabilele VC00. Toate sunt doar-citire fata de `VERCON_ACCESS` (surse Access, nemodificate).

| Script | Produce |
|---|---|
| `inventar.js` (+ `inventar_trans.js`) | `INVENTAR_OBIECTE.md`, `out/inventar.json` (excludere tranzitiva Coduri) |
| `dump_schema.ps1` | `out/schema_*.json` (DAO ReadOnly: campuri, indexuri, relatii, numar randuri; fara continut) |
| `dictionar.js` | `DICTIONAR_DATE.md` |
| `oracle_probe.ps1`, `oracle_probe2.ps1`, `oracle_probe3.ps1` | `out/oracle_probe*.txt` (agregate pentru CONTRACT_CALCULE) |
| `schite.js` + `schite_render.ps1` | `out/layouts/*.json`, `SCHITE/*.png`, `SCHITE/trasabilitate/*.md` |
| `mapare.js` | `SCHITE/INDEX.md`, `MAPARE_ACCESS_WEB.md`, `RAPOARTE.md` |

Ordinea: inventar -> dump_schema -> dictionar -> schite -> schite_render -> mapare. Nu porneste Access, nu ruleaza VBA, nu scrie in baze.

- `lib_tz.js` — cititorul TZ_SCHEMA si clasificarea coloanelor (folosite de `dictionar.js` si `ddl.js`).
- `ddl.js` — genereaza `sql/VC_02_business.sql` si `SCHEMA_VC_RELATII.mmd` (`node VERCON/tools/ddl.js`; doar citeste `VERCON_ACCESS`).
- `preview.py` — aceeași pornire reală cu `000_DEMO` prin SSH; numai `--fixtures` pornește vechea demonstrație fără MariaDB din `preview_fixture.json`.
- `Start-Localhost.ps1` / `Stop-Localhost.ps1` + `local_ssh.py` — formularele și API-urile reale din codul local, conectate prin SSH exclusiv la `000_DEMO`; vezi [LOCALHOST.md](LOCALHOST.md). Nu execută modificări de schemă.
- `ddl.js` scrie si `PYTHON/routes/vercon/tables.py` (domeniul fiecarui tabel, folosit de `repository.py`).
- `expected.js` — genereaza `VERCON/VRC.Migrator/expected_schema.json` (schema asteptata, incorporata in migrator).
