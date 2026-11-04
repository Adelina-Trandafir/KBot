# SLICE-AD19-01 — ADE.Migrator: numai soldurile inițiale 2020 → 2021

04.11.2026. Adelina a identificat cauza diferențelor dintre situația salvată și prima lună nesalvată: migrarea a început din 2021, deși unii copii aveau istoric din 2020. A cerut o operație în migrator care să transfere DOAR soldul inițial pentru copiii din 2020 care continuă în 2021, fără reluarea migrării. A confirmat explicit că are MDB-ul complet cu aceleași ID-uri ale copiilor.

## Implementare și limită

Comanda separată «Completează doar soldurile inițiale 2020 → 2021 (migrare existentă)...» deschide o previzualizare, independent de planul migrării complete și de condiția destinației goale. Folosește fișierul și conexiunea completate în formularul principal, DC-ul și subunitatea existente. Nu apelează AdeWriter.Write și nu modifică fluxul migrării complete.

Citirea reutilizează AdeSource.Read și verifică SHA-256 după citire. Eligibilitatea sursei cere rând în Prezenta sau SS_Buget în ambele intervale 2020 și 2021. Soldul propus reprezintă soldul semnat înainte de prima lună Access din 2021: SI original plus contractele anterioare, minus plățile neanulate, plus restituirile neanulate, cu conversie Access Long midpoint-to-even. Se păstrează condiția pe IDL a calculului existent și se refuză ordinea lunilor care nu corespunde anilor. O restituire veche fără IDL folosește luna prezenței legate. Istoricul anterior lui 2020, dacă există, participă la soldul complet înainte de 2021; selecția copiilor rămâne strict 2020 și 2021.

Pentru copiii lipsă din Platitori în MDB, dar păstrați în situațiile salvate, soldul poate fi recuperat din prima situație salvată unică din 2021: SID−SIC minus mișcările anterioare din 2021, ca să nu fie preluate de două ori. Când Platitori există și există situație salvată din 2021, se compară cele două proveniențe; diferențele sunt blocate pe rând. Lipsa unei situații complete, mișcările relevante fără lună sau conversiile imposibile sunt raportate.

Destinația trebuie să fie o subunitate existentă/activă, cu luni din 2021 și fără luni anterioare lui 2021. Prima lună din 2021 trebuie să coincidă cu sursa, pentru a nu trata drept lipsă doar 2020 când au fost omise și luni din 2021. Asocierea folosește AD_IdMap pentru Platitori; sunt verificate numele și CNP-ul când este disponibil în ambele surse. Lipsa hărții nu declanșează asociere după nume. Copilul trebuie să aibă prezență în destinație în 2021; mai multe SourceId pentru un TargetId și mișcările relevante orfane sunt blocate.

Previzualizarea afișează Copil, SI existent, SI propus, Diferență, Originea soldului și Stare/motiv. Copiii sunt ordonați alfabetic cu cultura ro-RO; sumele rămân valori numerice în grila comună. Numai rândurile De aplicat intră în scriere; cele deja corecte, omise sau blocate rămân nemodificate. Rândurile eligibile sunt aplicate împreună, după confirmarea din fereastră a numărului și destinației.

În tranzacție se iau prin SELECT FOR UPDATE blocările existente AD_Lock(0) și AD_Lock(subunitate), fără inserarea unor blocări noi. Se reface previzualizarea și se compară amprenta cu cea aprobată. Singura comandă de modificare a datelor este UPDATE AD_Platitori SET SI=..., Version=Version+1, limitată la SubunitId/IDP/versiune/SI vechi. Nu sunt modificate schema, SS_Buget, documentele, AD_IdMap sau AD_Imports și nu sunt inserați copii ori istoric. SI se înlocuiește cu valoarea completă; repetarea nu dublează soldul.

Jurnalul SQL reutilizează SqlDumpWriter, în Jurnale/ADE-SI. Erorile se propagă la UI și sunt înregistrate prin GlobalErrorLog. O eroare înainte de COMMIT produce rollback integral; lipsa confirmării COMMIT sau rollback este raportată ca rezultat neconfirmat, fără presupunerea succesului. Înainte de aplicare sursa este recitită și verificată; schimbarea sursei sau a destinației cere o previzualizare nouă.

## Fișiere proprii create/modificate

- `ADECHIT/ADE.Migrator/AdeOpeningBalanceSource.vb` — nou: selecție 2020/2021 și determinarea soldului reportat.
- `ADECHIT/ADE.Migrator/AdeOpeningBalanceTransfer.vb` — nou: asociere, previzualizare și tranzacție limitată la SI.
- `ADECHIT/ADE.Migrator/AdeOpeningBalanceForm.vb` — nou: comportamentul ferestrei separate.
- `ADECHIT/ADE.Migrator/AdeOpeningBalanceForm.Designer.vb` — nou: macheta cu grilă comună, sume numerice și comenzi separate.
- `ADECHIT/ADE.Migrator/AdeMigratorForm.vb` — accesul la noua comandă, independent de migrarea completă.
- `ADECHIT/ADE.Migrator/AdeMigratorForm.Designer.vb` — butonul comenzii în rând separat.
- `ADECHIT/ADE.Migrator/README.md` — utilizarea și limitele operației.
- `ADECHIT_STATUS.md` — indexul AD19 și următoarea felie AD20.
- `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md` — starea AD19.
- `docs/worklog/SLICE-AD19-01-migrator-numai-solduri-initiale.md` — acest worklog.

Motiv concret pentru înregistrarea celor 10 fișiere: «Comandă separată în ADE.Migrator pentru completarea exclusivă a soldurilor inițiale ale copiilor din 2020 care continuă în 2021, pe migrarea existentă, cu previzualizare și fără remigrare.»

## Stare și limite

SCRIS LOCAL, NECOMPILAT, NETESTAT, NEPUBLICAT. S-au citit sursele și schema locală relevantă din MariaDB_Schema/000_DEMO.sql și s-a analizat designul. Nu s-au rulat compilări, teste, verificări automate sau vizuale, utilitarul, citiri ale datelor reale din MDB, conexiuni la server ori comenzi SQL. Nu s-au făcut Git, publicare sau restart. Nu se declară o probă reușită a calculelor, a tranzacției sau a interfeței.

Executabilul trebuie compilat și distribuit ulterior. Nu există DDL de aplicat. Proba operatorului trebuie să acopere debit, credit, zero, copil numai într-un an, copil reconstituit, hartă lipsă/identitate diferită, sold deja corect, repetare, sursă/destinație schimbată după previzualizare, blocaje și rollback. Situațiile salvate nu se recalculează prin această comandă; rapoartele generate anterior trebuie regenerate pentru a vedea soldurile curente actualizate.

Nu s-au realizat capturi și nu există un topic K-BOT sau o captură de migrator care să fi fost modificată. Instrucțiunile sunt în README-ul utilitarului.

## Înregistrare

Varianta descrisă în acest worklog este istorică și a fost înlocuită de AD19-02: implementarea actuală scrie soldurile în tabela separată și păstrează SI. Adelina a autorizat înregistrarea la 04.11.2026 prin «inregisreaza daca trebuie», apoi a semnalat fișierele proprii omise din primul lot. Se completează evidența pentru cele 10 fișiere proprii, în versiunile finale actuale, cu motivul «Comanda separată de previzualizare și aplicare în ADE.Migrator, introdusă în AD19-01 și finalizată cu stocare separată în AD19-02; documentarea variantei inițiale înlocuite.». Nu se înregistrează și nu se reactivează vechea variantă de rescriere SI. Rezultatul scriptului este raportat în chat și în registrul imuabil. Starea NECOMPILAT, NETESTAT și NEPUBLICAT se păstrează.
