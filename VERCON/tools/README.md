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

- `lib_tz.js` — cititorul TZ_SCHEMA și clasificarea coloanelor Access (folosite de `dictionar.js` și `import_map.js`).
- `import_map.js` — generează regulile Access din `VRC.Migrator/import_map.json`, domeniile aplicației din `PYTHON/routes/vercon/tables.py` și diagrama mapării `SCHEMA_VC_RELATII.mmd`. Nu generează SQL. Structura fizică provine din AVACONT_SURSA de pe server; VRC o citește prin infrastructura KBot.Migrator. Înlocuiește vechiul `ddl.js`, eliminat împreună cu scriptul complet de creare.
- `preview.py` — aceeași pornire reală cu `000_DEMO` prin SSH; numai `--fixtures` pornește vechea demonstrație fără MariaDB din `preview_fixture.json`.
- `Start-Localhost.ps1` / `Stop-Localhost.ps1` + `local_ssh.py` — formularele și API-urile reale din codul local, conectate prin SSH exclusiv la `000_DEMO`; vezi [LOCALHOST.md](LOCALHOST.md). Nu execută modificări de schemă.
- `expected.js` — generează `VERCON/VRC.Migrator/expected_schema.json` (structura așteptată a sursei Access, folosită de profiler; nu este schema țintă MariaDB).
