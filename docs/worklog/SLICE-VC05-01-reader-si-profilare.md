# SLICE-VC05-01 — reader si profilare fara scriere

Stare: **IMPLEMENTAT_LOCAL**. Proiectul se compileaza (fara erori sau avertismente); **nu a fost pornit si nu a citit nicio baza reala**.

## Facut
- `VERCON/VRC.Migrator` (VB.NET, x64, in KBot.sln): `VrcAccess` (deschidere `Mode=Read`, parola comuna prin `AccessFilePassword`, fara logarea ei), `VrcProfiler` (schema vs asteptat, absent≠NULL, randuri, statistici numerice, antete N01/N02 numarate, config, tabele legate, `Ver` doar detectat, hash inainte/dupa), `VrcReport` (Markdown cu propuneri), fereastra (Adauga/Elimina/Citeste/Salveaza) si modul `--profile`.
- `VERCON/tools/expected.js` genereaza schema asteptata din TZ_SCHEMA (`expected_schema.json`, incorporata).
- `README.md` al migratorului.

## Limite (neverificate)
- Neprobat pe bazele reale: nu stim daca `Mode=Read` este acceptat de ACE pe toate fisierele, daca `GetOleDbSchemaTable` raporteaza corect tabelele legate, daca ID-urile negative se citesc corect sau timpul pe baze mari.
- Compatibilitatea 32/64 biti cu motorul instalat nu este verificata; nu s-a verificat prezenta ACE pe alte statii.
- Acceptarea (hash/schema/date nemodificate dupa o rulare reala, `Ver` neinfiintat) cere rularea pe copii ale bazelor.
