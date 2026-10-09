# SLICE-ADE5-04 — ADE.Migrator: migrare directă din baza Access ADECHIT în MariaDB

Data: 08.10.2026. Stare: CONSTRUIT LOCAL (build curat, zero avertismente); neprobat pe date, fără teste, nerulat pe MariaDB.

## Ce s-a făcut și de ce

Un utilitar nou, în stilul lui KBot.Migrator, care înlocuiește calea extract JSON + import Python
(`ADECHIT/tools/extract_access.ps1` + `PYTHON/routes/adechit/importer.py`) cu o migrare directă, pe schema finală
`sql/AD_03_schema_finala.sql` (ADE0-07).

- Fișierul `.mdb` se alege cu un browser de fișiere (deschis în `C:\adechit`, sau `C:\adchit`; dacă folderul are un singur
  `.mdb`, e ales singur). Se deschide cu `AccessProvider` din KBot.Migrator, care încearcă parola comună AVACONT
  (`AccessFilePassword`, «andreI»). Doar citire.
- `DC` se citește din `Unitati!DC` și este numele bazei unității pe serverul MariaDB.
- Fereastra: sus fișierul și serverul (gazdă, port, utilizator, parolă), jos în stânga grila cu tabelele (rânduri în
  Access / de scris / acum în MariaDB), jos în dreapta educatorii găsiți pe fiecare grupă (ultima coloană se corectează) și
  lista conversiilor și constatărilor.
- «Citește» nu atinge serverul; «Testează» verifică conexiunea, baza `DC`, tabelele și coloanele `AD_`, că sunt goale și
  că seria de chitanțe nu există; «Migrează» e activ numai după un plan fără blocaje și un server fără probleme.
- Scrierea este o singură tranzacție (rollback la orice eroare), cu un rând în `AD_Imports`.

## Conversiile (cele afișate operatorului)

- Fiecare coloană: întreg / zecimal / bifă / dată / text; număr întreg rotunjit dintr-o fracție = constatare raportată.
- Coloanele din Access care nu au loc în schema nouă sunt listate pe tabel (nu se mai iau).
- Educator: textul cu doi nume («A | B», «A / B», spații duble, «-») se desparte; un rând în `AD_Grupe_Educator` pe educator
  și perioadă (`DeLa`/`PanaLa`), reconstruite din `AD_SS_Buget` (Educator/Grupa/IDG pe luni). Operatorul unește variantele
  în grila din dreapta înainte de scriere.
- Grupa cu «plecat» în nume → `Tip = PLECATI`; restul `NORMALA`.
- Istoric copii: INTRARE din `DataIntrare`, PLECARE din `DataIesire`, MUTARE/PLECARE/REVENIRE deduse din schimbarea
  grupei între lunile din `AD_SS_Buget` (marcate `Dedus`, data = prima zi a lunii). `MutaCopil` nu se citește (irelevant).
- Ca în importul vechi: adresa copilului pe plătitori (dacă a lor are sub 10 caractere), `ZileLuna` pe lună (valoarea cea mai
  frecventă din prezență), `OriginMonth/OriginYear` pe plăți și restituiri, legături rupte istorice puse NULL.
  Orice altă legătură ruptă, cheie lipsă/dublă sau text prea lung = blocaj.
- Seria de chitanțe (CFGs, familia CH) → `AVACONT_COMUN.Unitati_Chitante`.

## Fișiere atinse

- Nou: `ADECHIT/ADE.Migrator/` (`ADE.Migrator.vbproj`, `Program.vb`, `AdeMigratorForm(.Designer).vb`, `AdeSource.vb`,
  `AdeSchema.vb`, `AdeValue.vb`, `AdeEducators.vb`, `AdePlan.vb`, `AdeWriter.vb`).
- `KBot.sln` (proiectul + folderul `ADECHIT`); acest worklog; `ADECHIT_STATUS.md`; `state/ADECHIT_STATUS_ADE0-ADE9.md`.
- Reutilizate, nemodificate: `KBot.Migrator.AccessProvider/TargetServer`, `KBot.Common`, `KBot.Theming`, `KBot.Controls`.

## Rezultatele verificărilor

- `dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj`: reușit, 0 avertismente, 0 erori.
- Citirea MDB-ului real s-a verificat doar prin PowerShell (nu prin acest cod): se deschide cu parola comună,
  `Unitati!DC = 011_GR28`, 11 grupe, 2 luni, 1.214 rânduri în `Prezenta` și `SS_Buget`; `Educator` are doi nume per grupă.

## Neverificat sau amânat

- Nimic nu a fost rulat: nici citirea, nici planul, nici scrierea pe MariaDB. Planul (în special educatorii pe perioade și
  mutările deduse) trebuie privit pe datele reale înainte de prima migrare.
- Lipsesc: oprirea unei scrieri în curs (e o tranzacție, dar fără buton), jurnal SQL, ecran de progres pe tabel.
- Mapările mari de verificat: `AD_SS_Buget.Luna` (text în MariaDB, valoarea din Access convertită la text), lungimile
  coloanelor text, `Plata` (întreg) față de valorile zecimale din Access.
- Codul Python/JS al modulului (repository/importer/service/app.js) rămâne neadaptat la schema AD_03 (vezi ADE0-07).
- Fără schimbări vizibile pentru operatorul K-BOT: utilitarul nu face parte din aplicație; nu există pagini de ajutor de
  actualizat. Nimic comis sau trimis în git.
