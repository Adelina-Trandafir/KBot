# SLICE-VC04-01 — matricea operatiilor VR

Istoricul aceleiași limite de acces VERCON: operație/rol → context verificat → ocupare exclusivă; privilegiile migratorului și probele rămân etape distincte. Catalogul scenariilor și rezultatele detaliate sunt în [PROBE_ACCES](../../VERCON/PROBE_ACCES.md).

<a id="slice-vc04-01"></a>

Stare inițială comună VC04-01…04: **IMPLEMENTAT_LOCAL**. Verificata doar sintaxa (Python `ast`, `node --check`); nimic rulat, previzualizat sau aplicat; nimic validat pe server.

## Facut
- `routes/vercon/operations.py`: 16 operatii (read, nomenclatoare, receptie, storno, consum, retur, alimente, oi_predare/transfer/casare, mf, raport, import, configure, close, reopen), cu tipurile de gestiune pentru care sunt valide; `effective` = drept SI tip. Formularele din `capabilities.py` cer acum operatia lor, nu un generic `edit`.
- `sql/VC04_01_roluri_vr.sql` (AVACONT_COMUN): cate un rol per operatie (`VR_*`), rerulabil, fara acordare implicita; operatiile se citesc din `Roluri_Operatii` (la fel ca AD).

## Limite
Matricea operatie→ruta se completeaza cu rutele business (VC06+). Nimic rulat; rolurile nu sunt inserate pana nu rulezi scriptul.

<a id="slice-vc04-02"></a>

## SLICE-VC04-02 — guard si repository cu context verificat

## Facut
- `bp.scoped(operation)` in `routes/vercon/__init__.py`: re-rezolva contextul pe server, cere ca TIPUL sa permita operatia, ca revizia (`X-Vercon-Version`) sa fie a clientului si, la scriere, jetonul de ocupare curent.
- `routes/vercon/repository.py`: `ScopedRepository` — fiecare instructiune poarta SubunitId/GestiuneId[/ExercitiuId]; fara UPDATE/DELETE fara conditie; contextul nu vine din valori; tabelele din `tables.py` (generat de `ddl.js`).
- Eroarea la citirea drepturilor nu deschide accesul: `drepturi` intoarce gol la eroare MariaDB, orice alta exceptie → 500.

## Limite
Nerulat. Nicio ruta business nu foloseste inca `scoped`/`ScopedRepository` (VC06).

<a id="slice-vc04-03"></a>

## SLICE-VC04-03 — ACL locale si contul migratorului

## Facut
- Decizie aplicata: fara ACL pe subunitate/gestiune; ACL absenta = rolurile pe DC deschid toate subunitatile/gestiunile DC-ului, iar ocuparea exclusiva separa utilizatorii.
- `sql/VC04_03_privilegii_migrator.sql` (sablon cu <DC>, fara parole): contul migratorului scrie doar tabelele VC_ ale DC-ului tinta, citeste Parteneri si doar identitatea unitatii din AVACONT_COMUN; fara DELETE (exceptie ContextLock), DDL sau GRANT OPTION; verificare cu SHOW GRANTS si revocare.

## Limite
Nerulat; conturile reale se creeaza si se valideaza de administrator, separat de preview.

<a id="slice-vc04-04"></a>

## SLICE-VC04-04 — ocupare exclusiva, drafturi, joburi

## Facut
- `routes/vercon/lock.py`: `LockService` (acquire/heartbeat/release/validate) peste `SqlLockStore` (VC_ContextLock, timp din baza de date) si `MemoryLockStore`. Jeton aleator per fila; preluare doar dupa expirare sau, de acelasi utilizator, cu confirmare; jetonul vechi este refuzat (`LOCK_LOST`).
- Rute: `POST /api/vercon/lock/acquire|heartbeat|release`; `context` raporteaza `lock_mine` si meniul se dezactiveaza cand altcineva detine gestiunea.
- Pagina: `js/vercon/lock.js` (un singur interval de heartbeat, eliberare la iesire cu `keepalive`), antet `X-Vercon-Lock`/`X-Vercon-Version` pe fiecare cerere; la pierdere datele raman in formular. Drafturile sunt pe context (draft.js).
- Preview: ocupare in memorie cu ttl 20 s; al doilea utilizator `/vercon/preview/user/altul`.

## Limite
Migratorul rezerva gestiunile subunitatii prin acelasi tabel (VC05). Joburile/rapoartele cu ocupare valida vin cu rutele lor (VC08). Inchiderea anului va folosi aceeasi ocupare. Nerulat.

<a id="slice-vc04-05"></a>

## SLICE-VC04-05 — probe de acces si audit

Stare: **PLANIFICAT** — matricea este scrisa (`VERCON/PROBE_ACCES.md`), nimic executat.

- Rezultatele raman goale pana la rulare; conturile/granturile reale se valideaza separat de preview.
- Audit: rezultatele distincte (success, refused, busy, rollback, commit, unknown) sunt definite in `diagnostics.py`; ultimele trei le emit rutele de scriere (VC06).

## Rulare 11.10.2026 (apeluri directe pe preview)
Rezultate in `VERCON/PROBE_ACCES.md`. Defecte gasite si reparate: heartbeat/eliberare acceptau jetonul altui utilizator (acum se verifica si e-mailul titularului); tip necunoscut dadea SELECTION_CHANGED (acum 400 TYPE_UNKNOWN). Neexecutat: A2/A7/A8, R1-R4, L4 la inchiderea filei, MariaDB. Stare: IN_LUCRU.
