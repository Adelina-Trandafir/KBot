# SLICE-VC05-01 — reader si profilare fara scriere

Stare: **IMPLEMENTAT_LOCAL**. Proiectul se compileaza (fara erori sau avertismente); **nu a fost pornit si nu a citit nicio baza reala**.

## Facut
- VERCON/VRC.Migrator (VB.NET x64, KBot.sln): VrcAccess (Mode=Read, parola comună AccessFilePassword fără logare); VrcProfiler (schema/așteptat, absent≠NULL, rânduri/statistici numerice, antete N01/N02 numărate, config, tabele legate, Ver doar detectat, hash înainte/după); VrcReport (Markdown cu propuneri); fereastră Adauga/Elimina/Citeste/Salveaza și --profile.
- `VERCON/tools/expected.js` genereaza schema asteptata din TZ_SCHEMA (`expected_schema.json`, incorporata).
- `README.md` al migratorului.

## Revizie 12.10.2026 (cerinta operatorului)
- Alegere folder; KBotDataView cu bifă Citeste per fișier. tz_aaaa/tz_2100, .accdb și .mdb; tz.mdb/tz.accdb excluse întotdeauna; alte tz_... apar nebifate.
- tz_2100 = bază specială OI/MF, nu an. Raportul deduce tipul din steaguri și avertizează pentru tz_2100 fără OBINV/MF sau fișier de an cu steag.
- `--profile raport.md <folder|fisiere>`.
- Compilat curat; nepornit.

## Limite (neverificate)
- Neprobat pe bazele reale: nu stim daca `Mode=Read` este acceptat de ACE pe toate fisierele, daca `GetOleDbSchemaTable` raporteaza corect tabelele legate, daca ID-urile negative se citesc corect sau timpul pe baze mari.
- Compatibilitatea 32/64 biti cu motorul instalat nu este verificata; nu s-a verificat prezenta ACE pe alte statii.
- Acceptarea (hash/schema/date nemodificate dupa o rulare reala, `Ver` neinfiintat) cere rularea pe copii ale bazelor.
