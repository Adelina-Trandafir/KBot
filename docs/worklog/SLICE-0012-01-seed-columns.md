# SLICE-0012-01 — `GET /api/forexe/seed/columns` (introspecție coloane)

**Data:** 2026-07-19 **Felia:** 0012 (migrare Access → MariaDB), pasul 1. **Plan:** lipit în sesiune.

---

## Ce s-a schimbat și de ce

Migratorul cere coloanele reale ale țintei MariaDB FX_ pentru payload-ul POST /api/forexe/seed/rows. Până aici, VBA introspecta numai TableDef Access: divergențele sursă/țintă (redenumiri, coloane Access rămase în urmă) produceau erori SQL opace per chunk la scriere.

Ruta nouă mută verificarea înainte de prima scriere:

```
GET /api/forexe/seed/columns?db_name=075_CEVM&table=FX_Receptii
-> 200 {"ok": true, "table": "FX_Receptii", "columns": ["IDR", "IDRH", ...]}
```

Doar numele, în ordinea din tabel (`SHOW COLUMNS`), ca apelantul să poată construi INSERT-uri pe poziție.

### De ce NU 404 pentru tabel inexistent

Brief-ul cere distingerea tabelului fără coloane aici (încă nemigrat) de cererea neajunsă (rețea/cheie/proxy). 404 le-ar confunda în tratarea generică 4xx a clienților; contractul este:

| Situație | Răspuns |
|---|---|
| tabel prezent | `200`, `columns` nevid |
| tabel absent din baza dată | `200`, `columns: []` |
| `db_name` invalid | `400` |
| tabel în afara allow-list | `400` |
| fără / cu `X-Api-Key` greșit | `401` |
| eroare de conexiune / SQL | `500` |

### Detaliu de implementare: `SHOW TABLES LIKE` înainte de `SHOW COLUMNS`

SHOW COLUMNS FROM pe tabel absent aruncă errno 1146. Mai întâi SHOW TABLES LIKE %s verifică existența fără eroare pentru tabel absent, cu numele ca valoare parametrizată; SHOW COLUMNS rulează numai dacă există. Două round-trip-uri evită deducția fragilă din errno și înghițirea erorilor reale de permisiuni.

Alternativa (`information_schema.columns ... ORDER BY ORDINAL_POSITION`) ar fi fost un singur query, dar brief-ul cere `SHOW COLUMNS`; iar `information_schema` cere privilegii pe care contul de seed nu le are garantat.

### Garda

`@require_api_key` (X-Api-Key), ca celelalte două rute din fișier — **nu** bearer. Seed-ul e condus de VBA/FOREXE legacy. Notat deja în capul fișierului; testul de gardă pinuiește alegerea.

### Securitate

- `db_name` prin `_DBNAME_RE`, `table` prin aceeași `ALLOWED_TABLES` ca `/schema` și `/rows` — niciun identificator din client nu ajunge în SQL fără allow-list.
- Strict read-only: niciun `INSERT`/`DDL`/`commit`. `conn.close()` în `finally`.
- Mesaje de eroare în română cu diacritice reale, `ensure_ascii=False` (prin `_json`).

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/seed.py` | ruta `seed_columns` + antetul actualizat (2 → 3 endpoint-uri, plus nota deciziei blocate) |
| `PYTHON/tests/test_forexe_seed_columns.py` | **NOU** — 11 teste host-only |
| `docs/worklog/KBOT_STATUS.md` | rând nou în registru |

## Decizia blocată: `seed/schema` rămâne, dar nu se apelează

**Varianta A, nedistructivă:** FX_ există în MariaDB cu DDL manual, tipuri/indecși potriviți. Recrearea din DAO ar pierde date și calitatea schemei: _dao_to_mariadb produce LONGTEXT pentru necunoscute/VARCHAR(255) implicit, iar /schema execută DROP TABLE IF EXISTS înaintea CREATE.

Ruta /schema rămâne pentru baze complet goale; migratorul nu o apelează, folosind /columns.

## Rezultate teste

`PYTHON/.venv` → `python -m pytest tests/ -q`: **75 passed, 10 skipped, 0 fail/error.** (Erau 9 skipped; al zecelea e modulul nou, sărit întreg off-host.)

Cele 11 teste noi sunt **host-only și s-au SĂRIT** aici — `config.py` lipsește de pe stație, ca la `test_forexe_tree.py`. Acoperă: fără cheie → 401/403; cheie greșită → 401/403; `db_name` invalid → 400; `db_name` lipsă → 400; tabel în afara allow-list → 400; `table` lipsă → 400; diacritice literale în mesajul de eroare; tabel valid → listă nevidă de string-uri care conține `CodAngajament`; ordine stabilă între apeluri; tabel inexistent → `200` cu listă goală; `POST` → 405.

## Rămâne NEVERIFICAT

1. **Nicio linie din felia asta nu a atins o bază reală.** Ca la 0008/0009/0011-01.
2. `MISSING_TABLE = "FX_Rezervarii_IMG"` — presupunerea că tabelul acela NU e migrat pe `000_DEMO` e a mea, nu un fapt verificat. Dacă apare, testul se **sare explicit** (cu mesaj), nu pică; dar atunci ramura „tabel inexistent" rămâne neacoperită și trebuie ales alt tabel absent.
3. Contul de seed trebuie să aibă `SHOW` pe baza țintă. Nu a fost verificat live; dacă lipsește, ruta întoarce `500`, nu `200` cu listă goală — ceea ce e corect (e chiar o eroare), dar merită știut înainte de prima rulare.
