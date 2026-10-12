# SLICE-VC03-05 — aplicare, sincronizare, proba MariaDB

Stare: **IMPLEMENTAT_LOCAL** doar ca scripturi; **proba pe MariaDB NU a fost facuta**.

- `sql/VC_00_preflight.sql` (verificari doar-citire) si `sql/VC_99_probe.sql` (13 probe: INT/overflow, pret negativ, FK intre gestiuni, import unic, al doilea titular, tip si scara inghetate, OI/MF neinchise, motiv la anulare, RESTRICT) cu rollback.
- Ordinea, schema sync si ce nu e tinta: `VERCON/SCHEMA_VC.md` si `VERCON/INTEGRARE_SISTEME.md`.
- Acceptarea cere rularea pe o baza de test si completarea rezultatelor; pana atunci regulile sunt doar declarate in DDL.
