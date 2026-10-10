# SLICE-ADE5-11 — CNP lipsă importat ca NULL

Data: 09.10.2026. Stare: CONSTRUIT LOCAL; fără teste sau migrare.

## Schimbare

La cererea utilizatorului, ADE.Migrator transformă marcajele `FARA CNP` și `-1`
în `Nothing`, transmis de writer ca `DBNull.Value` (SQL NULL). Se aplică numai
câmpurilor CNP/CNP_Platitor: Platitori, Platitori_sub și SS_Buget. Comparația
ignoră spațiile marginale și majusculele; alte valori se păstrează exact, ca text.
Conversia precedă CopyParentCnp, iar numărătorile apar în notele planului, fără
afișarea CNP-urilor reale. Sursele MDB și datele deja importate nu sunt modificate.

## Fișiere

- ADECHIT/ADE.Migrator/AdePlan.vb și README.md.
- ADECHIT_STATUS.md, ADECHIT/PLAN_IMPLEMENTARE.md și registrul ADE0–ADE9.

## Verificări și predare

Verificare statică: câmpurile permit NULL în exportul local 000_DEMO;
ConvertTables rulează înainte de CopyParentCnp; writerul folosește DBNull.Value.
Nu s-au rulat teste automate, verificări vizuale, DDL sau migrare pe server.
Build în dosarul separat `bin/Debug/cnp-null`: 0 erori, 0 avertismente.
Buildul în dosarul obișnuit a eșuat la copierea DLL-urilor folosite de un proces deschis;
nu s-a oprit procesul. Se folosește versiunea nouă pentru importurile următoare.
Utilizatorul probează ambii marcatori, variante cu spații/majuscule, un CNP cu
zero inițial și opțiunea de copiere către părinte.
