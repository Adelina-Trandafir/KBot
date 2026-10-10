# SLICE-AD11-01 — Portalul părinților

## Ce s-a schimbat și de ce

Implementarea planului aprobat: CNP+cod permanent+OTP email, părinți cu copii fără
Plecat și CNP/email configurate, selector custom numai pentru mai mulți copii,
dashboard lunar cu grafice și DataGrid comun readonly, fișă cont print/PDF după model,
Interval implicit debifat cu două calendare custom. Istoricul continuu este pe IDP.

Registru unic CNP/email în DC, CodAccesPortal pe părinte, generare la migrare/adăugare,
sincronizare între copii/subunități, buton trimitere numai cu email și validare conflict
cu identificarea celuilalt părinte. Rutele parent folosesc sesiuni izolate, revalidare
la fiecare cerere, OTP cu hash/expirare/încercări/unică utilizare și limitator comun.

Preview dedicat cu acreditări fictive și inbox loopback; aceeași logică de autentificare
ca producția, cu delivery local și memory în loc de SMTP/Redis. Modificările WIP existente
ale altor intervenții au fost păstrate; nu s-au citit/modificat sursele Access.

## Fișiere

- `sql/AD_08_portal_parinti.sql`.
- `PYTHON/routes/adechit/parent_identity.py`, `parent_portal.py`, `parent_statement.py`.
- Integrare: `main.py`, `adechit/__init__.py`, `repository.py`, `service.py`, `catalog_forms.py`, `importer.py`, `schema.json`.
- UI: `static/adechit-parents.html`, `js/adechit/parents.js`, `payers.js`, `statement-print.js`, `css/adechit-parents.css`, `adechit-statement.css`, `templates/adechit/statement.html`.
- Migrator: `ADECHIT/ADE.Migrator/AdeWriter.vb`, README.
- Local: `ADECHIT/tools/preview.py`; documentație `ADECHIT/PORTAL_PARINTI.md`, `UTILIZARE_WEB.md`, `PLAN_IMPLEMENTARE.md`, status.

## Verificări efective

- Python compileall: fără erori; JS node --check: fără erori.
- ADE.Migrator dotnet build --no-restore: 0 erori, 0 avertismente.
- ReportLab instalat în venv local, conform requirements-adechit.txt.
- Server local pornit pe localhost:5050, director SQLite separat.
- GET login parent, lista acreditărilor demo și inbox: HTTP 200 (fără probă vizuală sau autentificare executată).
- Preview-ul mai vechi a fost oprit strict după verificarea comenzii Python; a fost pornit launcherul actualizat.
- Nu s-au pornit teste automate/funcționale sau verificări vizuale: utilizatorul testează la final.

## Nevalidat și continuare

Login OTP, UI, grafice, print/PDF, salvările concurente și migrarea pe MariaDB urmează
probele utilizatorului. AD_08 nerulat, SMTP/server nevalidate, nimic publicat.
Publicarea după proba localhost: instrucțiunile și scenariile sunt în PORTAL_PARINTI.md.
Stare SCRIS LOCAL/PREGĂTIT PENTRU TESTAREA UTILIZATORULUI, nu TESTAT FUNCȚIONAL.
