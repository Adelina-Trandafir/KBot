# SLICE-VC04-04 — ocupare exclusiva, drafturi, joburi

Stare: **IMPLEMENTAT_LOCAL**. Verificata doar sintaxa (Python `ast`, `node --check`); nimic rulat, previzualizat sau aplicat; nimic validat pe server.

## Facut
- `routes/vercon/lock.py`: `LockService` (acquire/heartbeat/release/validate) peste `SqlLockStore` (VC_ContextLock, timp din baza de date) si `MemoryLockStore`. Jeton aleator per fila; preluare doar dupa expirare sau, de acelasi utilizator, cu confirmare; jetonul vechi este refuzat (`LOCK_LOST`).
- Rute: `POST /api/vercon/lock/acquire|heartbeat|release`; `context` raporteaza `lock_mine` si meniul se dezactiveaza cand altcineva detine gestiunea.
- Pagina: `js/vercon/lock.js` (un singur interval de heartbeat, eliberare la iesire cu `keepalive`), antet `X-Vercon-Lock`/`X-Vercon-Version` pe fiecare cerere; la pierdere datele raman in formular. Drafturile sunt pe context (draft.js).
- Preview: ocupare in memorie cu ttl 20 s; al doilea utilizator `/vercon/preview/user/altul`.

## Limite
Migratorul rezerva gestiunile subunitatii prin acelasi tabel (VC05). Joburile/rapoartele cu ocupare valida vin cu rutele lor (VC08). Inchiderea anului va folosi aceeasi ocupare. Nerulat.
