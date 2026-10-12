# SLICE-VC05-08 — `Parteneri.Provenienta`

Stare: **IMPLEMENTAT_LOCAL** (DDL aplicat). Python compilat, VB compilat. **ALTER-ul și sincronizarea pe DC-uri au fost rulate de operator (12.10.2026)**; codul Python nu e deployat pe server, iar nicio inserare nu a scris încă `Provenienta`. Cererea operatorului, 12.10.2026.

## Facut
- Coloană nouă `Parteneri.Provenienta varchar(30) NULL`, după `Ascuns`: de unde a venit rândul.
  - `MIGRARE_VERCON`: partenerii din ANAF adăugați de VRC.Migrator (`VrcPartnerStore.vb`).
  - `manual`: partenerii adăugați în fereastra Parteneri a aplicației (`PYTHON/routes/forexe/parteneri_edit.py`).
  - `avacont`: rândurile inserate din Access prin `POST /api/parteneri/syncdb` și `POST /api/parteneri/upsert` (`PYTHON/routes/parteneri.py`); la upsert, `ON DUPLICATE KEY UPDATE` nu atinge coloana, deci un rând care există deja își păstrează originea.
  - gol (NULL): rândul exista deja, adică `migrare_kbot`. Toate rândurile de până acum rămân așa; nici migratorul KBot, nici cel din Python nu scriu coloana (o coloană nulabilă fără sursă nu le încurcă).
- `sql/Parteneri_Provenienta.sql` (ALTER pe `AVACONT_SURSA`), `sql/AVACONT_SURSA.sql` (DDL de referință), `docs/MAPARE_NOMENCLATOARE.md` (rândul coloanei), `VERCON/VRC.Migrator/README.md`.

## De rulat, în ordine
1. ~~`ALTER`-ul din `sql/Parteneri_Provenienta.sql` pe `AVACONT_SURSA`.~~ Făcut de operator.
2. ~~Sincronizarea pe fiecare DC (KBotAdmin).~~ Făcută de operator.
3. De făcut: codul nou al celor două rute și al `parteneri_edit.py` (INSERT-urile numesc coloana: fără DDL cad cu «Unknown column»). VRC.Migrator scrie coloana tot la inserarea partenerilor, deci cere și el DDL-ul aplicat înaintea unei noi migrări cu parteneri din ANAF.

## Neacoperit
- `MariaDB_Schema/` e ignorat de git și nu a fost reîmprospătat.
- Rutele nu au fost rulate nici local, nici pe server; nu s-au scris teste (regula operatorului).
- Ajutorul și NOUTATI nu se ating (nimic despre Access în ele).
