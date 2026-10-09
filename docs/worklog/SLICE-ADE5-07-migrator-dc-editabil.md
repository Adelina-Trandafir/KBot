# SLICE-ADE5-07 — DC destinație editabil

Data: 09.10.2026. Stare: CONSTRUIT LOCAL; versiunea modificată nerulată.

## Schimbare și motiv

La cererea utilizatorului, DC-ul propus din Access este acum o casetă editabilă,
declarată în Designer ca txtTargetDc. Se blochează numai în timpul operațiilor,
împreună cu celelalte intrări. Schimbarea invalidează verificarea destinației,
fără pierderea planului. DC gol blochează migrarea și produce un mesaj explicit.

Verificarea, numărătorile, confirmarea, tranzacția și configurația chitanțelor folosesc
DC-ul ales. AD_Imports.unit înregistrează destinația; source_unit păstrează DC-ul
descoperit. Jurnalul arată ambele identități. Sursa Access rămâne nemodificată.
O nouă citire resetează propunerea la DC-ul descoperit în MDB.

## Fișiere

ADECHIT/ADE.Migrator/AdeMigratorForm.vb, AdeMigratorForm.Designer.vb, AdeWriter.vb,
README.md; ADECHIT_STATUS.md, ADECHIT/PLAN_IMPLEMENTARE.md,
docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md și acest worklog.

## Verificări

Build: dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj --no-restore --verbosity minimal -m:1
reușit, 0 erori, 0 avertismente. Inspecție statică a apelurilor care folosesc DC-ul.
Fără teste automate, rulare UI, conexiuni MariaDB, DDL sau publicare.

## Rămâne de probat

Utilizatorul verifică editarea DC, invalidarea butonului, Testează pe noua destinație
și jurnalul importului. Schema și regulile de tabel gol rămân obligatorii.
