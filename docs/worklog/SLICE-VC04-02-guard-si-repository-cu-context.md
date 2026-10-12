# SLICE-VC04-02 — guard si repository cu context verificat

Stare: **IMPLEMENTAT_LOCAL**. Verificata doar sintaxa (Python `ast`, `node --check`); nimic rulat, previzualizat sau aplicat; nimic validat pe server.

## Facut
- `bp.scoped(operation)` in `routes/vercon/__init__.py`: re-rezolva contextul pe server, cere ca TIPUL sa permita operatia, ca revizia (`X-Vercon-Version`) sa fie a clientului si, la scriere, jetonul de ocupare curent.
- `routes/vercon/repository.py`: `ScopedRepository` — fiecare instructiune poarta SubunitId/GestiuneId[/ExercitiuId]; fara UPDATE/DELETE fara conditie; contextul nu vine din valori; tabelele din `tables.py` (generat de `ddl.js`).
- Eroarea la citirea drepturilor nu deschide accesul: `drepturi` intoarce gol la eroare MariaDB, orice alta exceptie → 500.

## Limite
Nerulat. Nicio ruta business nu foloseste inca `scoped`/`ScopedRepository` (VC06).
