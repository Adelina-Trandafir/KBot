# SLICE-VC04-05 — probe de acces si audit

Stare: **PLANIFICAT** — matricea este scrisa (`VERCON/PROBE_ACCES.md`), nimic executat.

- Rezultatele raman goale pana la rulare; conturile/granturile reale se valideaza separat de preview.
- Audit: rezultatele distincte (success, refused, busy, rollback, commit, unknown) sunt definite in `diagnostics.py`; ultimele trei le emit rutele de scriere (VC06).

## Rulare 11.10.2026 (apeluri directe pe preview)
Rezultate in `VERCON/PROBE_ACCES.md`. Defecte gasite si reparate: heartbeat/eliberare acceptau jetonul altui utilizator (acum se verifica si e-mailul titularului); tip necunoscut dadea SELECTION_CHANGED (acum 400 TYPE_UNKNOWN). Neexecutat: A2/A7/A8, R1-R4, L4 la inchiderea filei, MariaDB. Stare: IN_LUCRU.
