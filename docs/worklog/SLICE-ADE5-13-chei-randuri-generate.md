# SLICE-ADE5-13 — cheile rândurilor generate în writer

Data: 09.10.2026. Stare: CONSTRUIT LOCAL; fără teste sau migrare.

## Problemă și corecție

Utilizatorul a raportat KeyNotFoundException pentru IDGE în InsertRows.
BuildEducators/BuildHistory construiesc rânduri fără IDGE/IDI, cu SourceRows=-1;
cheile sunt alocate de MariaDB. Writerul citea cheia înainte de a verifica
SourceRows, deci verificarea ulterioară nu împiedica excepția.

Citirea cheii și adăugarea în hartă sunt acum în interiorul condiției
SourceRows >= 0. Inserarea și remaparea relațiilor rămân aceleași.

## Fișiere și verificări

AdeWriter.vb, README.md, ADECHIT_STATUS.md, PLAN_IMPLEMENTARE.md și registrul ADE0–ADE9.
Build în `bin/Debug/generated-keys`: 0 erori, 0 avertismente.
Verificare statică a ambelor tipuri de rânduri generate; fără teste automate,
probe vizuale, migrare sau acces la server. Utilizatorul folosește executabilul
nou și verifică rollback-ul rulării eșuate înainte de reluare.
