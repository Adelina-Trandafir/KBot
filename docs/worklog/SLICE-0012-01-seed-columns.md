# SLICE-0012-01 — `GET /api/forexe/seed/columns` (introspecție coloane)

**Data:** 2026-07-19
**Felia:** 0012 (migrare Access → MariaDB), pasul 1.
**Plan:** lipit în sesiune.

---

## Ce s-a schimbat și de ce

Utilitarul de migrare are nevoie de lista **reală** de coloane a fiecărui tabel `FX_`
din MariaDB ca să construiască payload-ul pentru `POST /api/forexe/seed/rows`. Până
acum singura sursă de adevăr era `TableDef`-ul Access, introspectat pe partea VBA —
adică schema **sursă**, nu cea **destinație**. Dacă cele două diverg (coloană
redenumită la migrare, coloană Access rămasă în urmă), insertul pică pe o eroare SQL
opacă, la rulare, per chunk.

Ruta nouă mută verificarea înainte de prima scriere:

```
GET /api/forexe/seed/columns?db_name=075_CEVM&table=FX_Receptii
-> 200 {"ok": true, "table": "FX_Receptii", "columns": ["IDR", "IDRH", ...]}
```

Doar numele, în ordinea din tabel (`SHOW COLUMNS`), ca apelantul să poată construi
INSERT-uri pe poziție.

### De ce NU 404 pentru tabel inexistent

Cerință explicită din brief, și e cea corectă: apelantul trebuie să distingă „tabelul
nu are coloane aici" (migrarea nu a rulat pentru el) de „cererea nu a ajuns" (rețea,
cheie greșită, proxy). Un 404 le confundă — clienții HTTP tratează 4xx generic. Deci:

| Situație | Răspuns |
|---|---|
| tabel prezent | `200`, `columns` nevid |
| tabel absent din baza dată | `200`, `columns: []` |
| `db_name` invalid | `400` |
| tabel în afara allow-list | `400` |
| fără / cu `X-Api-Key` greșit | `401` |
| eroare de conexiune / SQL | `500` |

### Detaliu de implementare: `SHOW TABLES LIKE` înainte de `SHOW COLUMNS`

`SHOW COLUMNS FROM` pe un tabel inexistent **aruncă** (errno 1146). Ca să nu ajung să
adulmec codul de eroare din excepție — fragil, și ar înghiți la fel de bine o eroare
reală de permisiuni — existența se testează întâi cu `SHOW TABLES LIKE %s`, care nu
aruncă și primește numele **ca valoare parametrizată**, nu ca identificator. Abia dacă
rândul există se rulează `SHOW COLUMNS`. Două round-trip-uri, zero ghicit de errno.

Alternativa (`information_schema.columns ... ORDER BY ORDINAL_POSITION`) ar fi fost un
singur query, dar brief-ul cere `SHOW COLUMNS`; iar `information_schema` cere
privilegii pe care contul de seed nu le are garantat.

### Garda

`@require_api_key` (X-Api-Key), ca celelalte două rute din fișier — **nu** bearer.
Seed-ul e condus de VBA/FOREXE legacy. Notat deja în capul fișierului; testul de gardă
pinuiește alegerea.

### Securitate

- `db_name` prin `_DBNAME_RE`, `table` prin aceeași `ALLOWED_TABLES` ca `/schema` și
  `/rows` — niciun identificator din client nu ajunge în SQL fără allow-list.
- Strict read-only: niciun `INSERT`/`DDL`/`commit`. `conn.close()` în `finally`.
- Mesaje de eroare în română cu diacritice reale, `ensure_ascii=False` (prin `_json`).

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/seed.py` | ruta `seed_columns` + antetul actualizat (2 → 3 endpoint-uri, plus nota deciziei blocate) |
| `PYTHON/tests/test_forexe_seed_columns.py` | **NOU** — 11 teste host-only |
| `docs/worklog/KBOT_STATUS.md` | rând nou în registru |

## Decizia blocată: `seed/schema` rămâne, dar nu se apelează

**Varianta A, nedistructivă.** Tabelele `FX_` există deja în MariaDB cu DDL curat
(scris de mână, cu tipuri și indecși potriviți). A le recrea din tipurile DAO ale
Access-ului ar fi o regresie: `_dao_to_mariadb` produce `LONGTEXT` pentru orice tip
necunoscut și `VARCHAR(255)` implicit, iar `/schema` face `DROP TABLE IF EXISTS`
înainte de `CREATE` — deci ar șterge date reale ca să pună o schemă mai slabă.

Ruta rămâne în cod (nu e ștearsă: e utilă pentru o bază complet goală), dar utilitarul
de migrare **nu o cheamă**. `/columns` există exact ca să nu fie nevoie de ea.

## Rezultate teste

`PYTHON/.venv` → `python -m pytest tests/ -q`: **75 passed, 10 skipped, 0 fail/error.**
(Erau 9 skipped; al zecelea e modulul nou, sărit întreg off-host.)

Cele 11 teste noi sunt **host-only și s-au SĂRIT** aici — `config.py` lipsește de pe
stație, ca la `test_forexe_tree.py`. Acoperă: fără cheie → 401/403; cheie greșită →
401/403; `db_name` invalid → 400; `db_name` lipsă → 400; tabel în afara allow-list →
400; `table` lipsă → 400; diacritice literale în mesajul de eroare; tabel valid → listă
nevidă de string-uri care conține `CodAngajament`; ordine stabilă între apeluri; tabel
inexistent → `200` cu listă goală; `POST` → 405.

## Rămâne NEVERIFICAT

1. **Nicio linie din felia asta nu a atins o bază reală.** Ca la 0008/0009/0011-01.
2. `MISSING_TABLE = "FX_Rezervarii_IMG"` — presupunerea că tabelul acela NU e migrat pe
   `000_DEMO` e a mea, nu un fapt verificat. Dacă apare, testul se **sare explicit**
   (cu mesaj), nu pică; dar atunci ramura „tabel inexistent" rămâne neacoperită și
   trebuie ales alt tabel absent.
3. Contul de seed trebuie să aibă `SHOW` pe baza țintă. Nu a fost verificat live; dacă
   lipsește, ruta întoarce `500`, nu `200` cu listă goală — ceea ce e corect (e chiar
   o eroare), dar merită știut înainte de prima rulare.
