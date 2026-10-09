# ADE5-09 — Opțiune CNP Copil = CNP Părinte

La cererea utilizatorului, migratorul oferă în zona sursei Access o bifă
«CNP Copil = CNP Părinte», implicit oprită. Activarea copiază valoarea
Platitori.CNP din Access, după conversia textuală existentă, și în
AD_Platitori_sub.CNP_Platitor pentru fiecare plătitor asociat prin IDP.
Valoarea copilului rămâne păstrată. CNP-urile sursă necompletate nu suprascriu
valorile plătitorilor; cele completate înlocuiesc valoarea CNP_Platitor din plan.
Fără bifă se păstrează maparea anterioară.

Opțiunea se aplică planului înainte de scriere. Modificarea reface planul,
invalidează verificarea anterioară și cere Testează din nou. Conversia și numărul
plătitorilor afectați apar în note. Controlul este blocat în timpul operațiilor,
prin grupul sursei. Opțiunea rămâne la recitire în aceeași sesiune, fără persistență.

Fișiere: AdePlan.vb, AdeMigratorForm.vb, AdeMigratorForm.Designer.vb,
README.md, PLAN_IMPLEMENTARE.md, ADECHIT_STATUS.md și registrul ADE0–ADE9.
Câmpul CNP_Platitor există în schema locală MariaDB_Schema/000_DEMO.sql și
AVACONT_SURSA.sql. Nu este necesar DDL nou. Sursa Access nu a fost modificată.

Verificare efectuată: compilare locală cu `dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj --no-restore -p:BuildProjectReferences=false -p:UseSharedCompilation=false -m:1 -nr:false -v:minimal`.
Rezultat: 0 erori, 0 avertismente. Nu s-au pornit teste automate, verificări
vizuale sau migrare. Utilizatorul rulează noul ADE.Migrator și verifică planul,
reverificarea destinației și CNP_Platitor după transfer. Nu s-au executat
publicare, SQL remote sau restart de serviciu.

Următoarea subfelie liberă: ADE5-10.
