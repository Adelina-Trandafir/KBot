# SLICE-ADE5-04…14 și ADE10-07 — Migratorul direct ADECHIT

Repere comune: 08.10.2026 — ADE5-04; 09.10.2026 — ADE5-05, ADE5-06, ADE5-07, ADE5-10…14. Aceste etape au starea TESTAT, RULAT, PUBLICAT, confirmată de operator la 23.10.2026. Probele și limitele inițiale rămân în secțiunile lor; această atribuire nu este extinsă asupra ADE5-08/09 sau ADE10-07.

04 introduce utilitarul; 05 adaugă oprirea și jurnalul, 06 explică blocajele, 07 separă DC sursă/destinație, 08/09 aleg grupa și copierea CNP, 10 înlocuiește încărcarea automată a logului cu una manuală, 11/12 tratează explicit NULL, 13 corectează cheile generate, iar 14 înlocuiește excepția de anulare cu rezultat distinct după rollback confirmat. ADE10-07 reconstituie copiii lipsă și corectează în plan numerele chitanțelor, fără schimbarea MDB sau DDL.

Nota comună de închidere din ADE5-04/05/06/07 și ADE10-07: *Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.* Descrierile inițiale „nerulat” și listele de predare de mai jos rămân checkpointurile de la implementare, urmate de această confirmare; nu sunt probe executate în consolidarea documentară. Planul de la ADE5-07 indica următoarea subfelie liberă ADE5-10, iar checkpointul ADE5-14 indica ADE5-15.

- [ADE5-04](#slice-ADE5-04)
- [ADE5-05](#slice-ADE5-05)
- [ADE5-06](#slice-ADE5-06)
- [ADE5-07](#slice-ADE5-07)
- [ADE5-08](#slice-ADE5-08)
- [ADE5-09](#slice-ADE5-09)
- [ADE5-10](#slice-ADE5-10)
- [ADE5-11](#slice-ADE5-11)
- [ADE5-12](#slice-ADE5-12)
- [ADE5-13](#slice-ADE5-13)
- [ADE5-14](#slice-ADE5-14)
- [ADE10-07](#slice-ADE10-07)

## SLICE-ADE5-04 — ADE.Migrator: migrare directă din baza Access ADECHIT în MariaDB <a id="slice-ADE5-04"></a>

Build curat, zero avertismente.

### Ce s-a făcut și de ce

Un utilitar nou, în stilul lui KBot.Migrator, care înlocuiește calea extract JSON + import Python (`ADECHIT/tools/extract_access.ps1` + `PYTHON/routes/adechit/importer.py`) cu o migrare directă, pe schema finală `sql/AD_03_schema_finala.sql` (ADE0-07).

- Fișierul `.mdb` se alege cu un browser de fișiere (deschis în `C:\adechit`, sau `C:\adchit`; dacă folderul are un singur `.mdb`, e ales singur). Se deschide cu `AccessProvider` din KBot.Migrator, care încearcă parola comună AVACONT (`AccessFilePassword`, «andreI»). Doar citire.
- `DC` se citește din `Unitati!DC` și este numele bazei unității pe serverul MariaDB.
- Fereastra: sus fișierul și serverul (gazdă, port, utilizator, parolă), jos în stânga grila cu tabelele (rânduri în Access / de scris / acum în MariaDB), jos în dreapta educatorii găsiți pe fiecare grupă (ultima coloană se corectează) și lista conversiilor și constatărilor.
- «Citește» nu atinge serverul; «Testează» verifică conexiunea, baza `DC`, tabelele și coloanele `AD_`, că sunt goale și că seria de chitanțe nu există; «Migrează» e activ numai după un plan fără blocaje și un server fără probleme.
- Scrierea este o singură tranzacție (rollback la orice eroare), cu un rând în `AD_Imports`.

### Conversiile (cele afișate operatorului)

- Fiecare coloană: întreg / zecimal / bifă / dată / text; număr întreg rotunjit dintr-o fracție = constatare raportată.
- Coloanele din Access care nu au loc în schema nouă sunt listate pe tabel (nu se mai iau).
- Educator: textul cu doi nume («A | B», «A / B», spații duble, «-») se desparte; un rând în `AD_Grupe_Educator` pe educator și perioadă (`DeLa`/`PanaLa`), reconstruite din `AD_SS_Buget` (Educator/Grupa/IDG pe luni). Operatorul unește variantele în grila din dreapta înainte de scriere.
- Grupa cu «plecat» în nume → `Tip = PLECATI`; restul `NORMALA`.
- Istoric copii: INTRARE din `DataIntrare`, PLECARE din `DataIesire`, MUTARE/PLECARE/REVENIRE deduse din schimbarea grupei între lunile din `AD_SS_Buget` (marcate `Dedus`, data = prima zi a lunii). `MutaCopil` nu se citește (irelevant).
- Ca în importul vechi: adresa copilului pe plătitori (dacă a lor are sub 10 caractere), `ZileLuna` pe lună (valoarea cea mai frecventă din prezență), `OriginMonth/OriginYear` pe plăți și restituiri, legături rupte istorice puse NULL. Orice altă legătură ruptă, cheie lipsă/dublă sau text prea lung = blocaj.
- Seria de chitanțe (CFGs, familia CH) → `AVACONT_COMUN.Unitati_Chitante`.

### Fișiere atinse

- Nou: `ADECHIT/ADE.Migrator/` (`ADE.Migrator.vbproj`, `Program.vb`, `AdeMigratorForm(.Designer).vb`, `AdeSource.vb`, `AdeSchema.vb`, `AdeValue.vb`, `AdeEducators.vb`, `AdePlan.vb`, `AdeWriter.vb`).
- `KBot.sln` (proiectul + folderul `ADECHIT`); acest worklog; `ADECHIT_STATUS.md`; `state/ADECHIT_STATUS_ADE0-ADE9.md`.
- Reutilizate, nemodificate: `KBot.Migrator.AccessProvider/TargetServer`, `KBot.Common`, `KBot.Theming`, `KBot.Controls`.

### Rezultatele verificărilor

- `dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj`: reușit, 0 avertismente, 0 erori.
- Citirea MDB-ului real s-a verificat doar prin PowerShell (nu prin acest cod): se deschide cu parola comună, `Unitati!DC = 011_GR28`, 11 grupe, 2 luni, 1.214 rânduri în `Prezenta` și `SS_Buget`; `Educator` are doi nume per grupă.

### Neverificat sau amânat

- Nimic nu a fost rulat: nici citirea, nici planul, nici scrierea pe MariaDB. Planul (în special educatorii pe perioade și mutările deduse) trebuie privit pe datele reale înainte de prima migrare.
- Lipsesc: oprirea unei scrieri în curs (e o tranzacție, dar fără buton), jurnal SQL, ecran de progres pe tabel.
- Mapările mari de verificat: `AD_SS_Buget.Luna` (text în MariaDB, valoarea din Access convertită la text), lungimile coloanelor text, `Plata` (întreg) față de valorile zecimale din Access.
- Codul Python/JS al modulului (repository/importer/service/app.js) rămâne neadaptat la schema AD_03 (vezi ADE0-07).
- Fără schimbări vizibile pentru operatorul K-BOT: utilitarul nu face parte din aplicație; nu există pagini de ajutor de actualizat. Nimic comis sau trimis în git.

## SLICE-ADE5-05 — oprire, jurnal SQL și invalidarea verificării în ADE.Migrator <a id="slice-ADE5-05"></a>

### Ce s-a schimbat și de ce

- Buton Oprește în Designer, CancellationToken între rânduri și înainte de COMMIT; rollback la oprirea scrierii, fără terminarea procesului în timpul conexiunilor active. FormClosing așteaptă terminarea operației; citirea/testarea se așteaptă fără anulare.
- Progres pe tabel la fiecare 100 de rânduri și la final; rândurile pregătite nu sunt prezentate ca persistate înainte de COMMIT.
- Reutilizarea SqlDumpWriter și ValueConverter.ToLiteral din KBot.Migrator: jurnal pe tabel, configurația chitanțelor și AD_Imports, cu rezultat final și hash sursă. Înlocuirea parametrilor se face într-o singură trecere pentru a nu modifica textele care conțin numele altor parametri. Parola conexiunii nu intră în jurnal.
- Schimbarea fișierului/conexiunii/educatorilor invalidează verificarea; refacerea eșuată a planului nu păstrează planul vechi activ. Controalele sunt blocate în operații.
- Writer-ul reverifică destinația și hash-ul sursei; verifică AD_Imports și InnoDB inclusiv pentru configurația din baza comună. Nu schimbă schema/mapările business.
- UI distinge succesul confirmat de eroarea afișării și de rezultatul necunoscut. Eșecul confirmării COMMIT sau al rollback-ului nu promite o bază neschimbată.
- Erorile UI sunt afișate și înregistrate; salvarea setărilor propagă eroarea. Writer-ul propagă cu Throw, iar limita UI îl înregistrează. Rollback-ul eșuat păstrează ambele excepții. Comentarii noi în engleză; mesaje românești cu diacritice.

### Fișiere atinse

- ADECHIT/ADE.Migrator/AdeMigratorForm.vb
- ADECHIT/ADE.Migrator/AdeMigratorForm.Designer.vb
- ADECHIT/ADE.Migrator/AdeWriter.vb
- ADECHIT/ADE.Migrator/README.md — utilizare și interpretarea jurnalelor
- ADECHIT/PLAN_IMPLEMENTARE.md, ADECHIT_STATUS.md, docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md și acest worklog

### Verificări efective

- Citire statică a regulilor ADE, CODE_WORKFLOW și convenției WinForms.
- Consultate punctual exporturile MariaDB_Schema/000_DEMO.sql și AVACONT_COMUN.sql; AD_Imports are coloanele folosite și InnoDB în exportul DEMO. Destinația efectivă se verifică de utilitar, la rularea utilizatorului; exportul nu dovedește schema fiecărei unități.
- `dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj --no-restore --verbosity minimal`: build final reușit, 0 avertismente, 0 erori. O eroare intermediară de sintaxă tuple VB.NET a fost corectată înainte de buildul reușit.
- Fără teste automate, probe vizuale, rulare MDB sau conexiuni MariaDB, conform regulii de testare. Sursele Access nu au fost modificate. Fără commit, push sau DDL executat.

### Neverificat și amânat

- Utilizatorul probează citirea MDB, educatorii, blocajele, invalidarea verificării, oprirea înainte de COMMIT, închiderea în lucru și interpretarea jurnalelor.
- Scrierea efectivă, rollback-ul, pierderea conexiunii în COMMIT și reconcilierea pe MariaDB rămân neverificate. Verificarea tabelelor goale nu este o blocare concurentă a destinației; nu se rulează două migratoare sau scrieri web simultan pe aceeași bază.
- Citirea MDB și verificarea conexiunii nu sunt anulabile în această subfelie.
- Mapările și calculele ADE5-04 rămân de validat pe date, inclusiv lungimile textelor, AD_SS_Buget.Luna și conversia Plata; reconstrucția istoricului nu a fost probată.
- Numele proiectului și executabilului rămâne ADE.Migrator. Nu s-a schimbat distribuția.

## SLICE-ADE5-06 — jurnalul erorilor și motivele blocării în formular <a id="slice-ADE5-06"></a>

### Ce s-a schimbat și de ce

Utilizatorul raportează că Migrează nu se activează și că nu vede niciun mesaj. Citirea jurnalului local din bin/Debug/net8.0-windows/Logs/harness_errors.log a identificat ultima verificare la 17:01:11: Connect Timeout expired în TestConnection. Aceasta explică lipsa unei verificări reușite; nu dovedește cauza de rețea a timeout-ului. Nu s-a accesat serverul pentru diagnostic.

- Motiv permanent lângă Migrează și lista tuturor motivelor în jurnalul formularului. Eroarea ultimei verificări este păstrată până la modificarea intrărilor sau reîncercare.
- Jurnal în Designer: mesaje, blocaje Access și server, excepții complete, gazdă/port la verificare. Nu se scrie parola conexiunii. Se păstrează GlobalErrorLog comun.
- Buton pentru citirea ultimilor maximum 256 KiB din logul comun, cu FileShare.ReadWrite; încărcare inițială la Shown, fără blocarea inițializării setărilor dacă citirea eșuează.
- Preferințele locale se salvează separat: eroarea este afișată/înregistrată, dar nu abandonează citirea sursei sau verificarea destinației deja începută.
- Testarea fără plan citit afișează pasul necesar. Condițiile de validare rămân active; butonul nu se activează când conexiunea/verificarea au eșuat sau există blocaje.
- Comentarii și identificatori noi în engleză; mesaje în română cu diacritice.

### Fișiere atinse

AdeMigratorForm.vb, AdeMigratorForm.Designer.vb și README.md din ADECHIT/ADE.Migrator; ADECHIT_STATUS.md, ADECHIT/PLAN_IMPLEMENTARE.md, docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md și acest worklog.

### Verificări efective

- Inspecție statică a condițiilor butonului și citirea jurnalului existent.
- Build: `dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj --no-restore --verbosity minimal -m:1`: 0 avertismente, 0 erori. Unele încercări cu build paralel au eșuat fără diagnostice; buildul serial a reușit. Nu atribuim o cauză neverificată acelor eșecuri.
- Fără teste automate, probe vizuale, rulare a utilitarului, conexiuni MariaDB sau DDL.

### Neverificat și amânat

Utilizatorul redeschide executabilul actualizat, verifică jurnalul și reîncearcă conectarea cu gazda/portul corecte. Accesul la server, cauza timeout-ului și activarea butonului după o verificare reală reușită rămân de probat de utilizator. Identificatorii românești existenți nu sunt redenumiți în această corecție. Fără commit sau publicare în această intervenție.

## SLICE-ADE5-07 — DC destinație editabil <a id="slice-ADE5-07"></a>

### Schimbare și motiv

La cererea utilizatorului, DC-ul propus din Access este acum o casetă editabilă, declarată în Designer ca txtTargetDc. Se blochează numai în timpul operațiilor, împreună cu celelalte intrări. Schimbarea invalidează verificarea destinației, fără pierderea planului. DC gol blochează migrarea și produce un mesaj explicit.

Verificarea, numărătorile, confirmarea, tranzacția și configurația chitanțelor folosesc DC-ul ales. AD_Imports.unit înregistrează destinația; source_unit păstrează DC-ul descoperit. Jurnalul arată ambele identități. Sursa Access rămâne nemodificată. O nouă citire resetează propunerea la DC-ul descoperit în MDB.

### Fișiere

ADECHIT/ADE.Migrator/AdeMigratorForm.vb, AdeMigratorForm.Designer.vb, AdeWriter.vb, README.md; ADECHIT_STATUS.md, ADECHIT/PLAN_IMPLEMENTARE.md, docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md și acest worklog.

### Verificări

Build: dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj --no-restore --verbosity minimal -m:1 reușit, 0 erori, 0 avertismente. Inspecție statică a apelurilor care folosesc DC-ul. Fără teste automate, rulare UI, conexiuni MariaDB, DDL sau publicare.

### Rămâne de probat

Utilizatorul verifică editarea DC, invalidarea butonului, Testează pe noua destinație și jurnalul importului. Schema și regulile de tabel gol rămân obligatorii.

## ADE5-08 — Grupa de plecați în migrator <a id="slice-ADE5-08"></a>

La citirea Access, migratorul afișează grupele alfabetic și bifează Plecați dacă denumirea conține «pleca», fără diferență între litere mari/mici. Bifa poate fi schimbată manual, inclusiv debifarea unei grupe detectate automat. Recitirea sursei reface detectarea. Modificarea alegerii invalidează verificarea anterioară.

Planul primește opțiunile înainte de construirea istoricului și scrie tipul PLECATI/NORMALA corespunzător. Mai multe grupe de plecați blochează planul, deoarece serviciul lunar acceptă cel mult una. Lipsa grupei produce doar o notă.

Fișiere: AdePlan.vb, AdeMigratorForm.vb, designerul formularului, README și documentele de stare/plan. Nu s-au modificat bazele de date sau serviciile remote.

Compilare locală: `dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj --no-restore -p:BuildProjectReferences=false -p:UseSharedCompilation=false -m:1 -nr:false -v:minimal`. Rezultat: 0 avertismente, 0 erori. Nu s-au executat teste automate sau verificări funcționale/vizuale; acestea revin utilizatorului.

Următoarea subfelie liberă: ADE5-09.

## ADE5-09 — Opțiune CNP Copil = CNP Părinte <a id="slice-ADE5-09"></a>

La cererea utilizatorului, migratorul oferă în zona sursei Access o bifă «CNP Copil = CNP Părinte», implicit oprită. Activarea copiază valoarea Platitori.CNP din Access, după conversia textuală existentă, și în AD_Platitori_sub.CNP_Platitor pentru fiecare plătitor asociat prin IDP. Valoarea copilului rămâne păstrată. CNP-urile sursă necompletate nu suprascriu valorile plătitorilor; cele completate înlocuiesc valoarea CNP_Platitor din plan. Fără bifă se păstrează maparea anterioară.

Opțiunea se aplică planului înainte de scriere. Modificarea reface planul, invalidează verificarea anterioară și cere Testează din nou. Conversia și numărul plătitorilor afectați apar în note. Controlul este blocat în timpul operațiilor, prin grupul sursei. Opțiunea rămâne la recitire în aceeași sesiune, fără persistență.

Fișiere: AdePlan.vb, AdeMigratorForm.vb, AdeMigratorForm.Designer.vb, README.md, PLAN_IMPLEMENTARE.md, ADECHIT_STATUS.md și registrul ADE0–ADE9. Câmpul CNP_Platitor există în schema locală MariaDB_Schema/000_DEMO.sql și AVACONT_SURSA.sql. Nu este necesar DDL nou. Sursa Access nu a fost modificată.

Verificare efectuată: compilare locală cu `dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj --no-restore -p:BuildProjectReferences=false -p:UseSharedCompilation=false -m:1 -nr:false -v:minimal`. Rezultat: 0 erori, 0 avertismente. Nu s-au pornit teste automate, verificări vizuale sau migrare. Utilizatorul rulează noul ADE.Migrator și verifică planul, reverificarea destinației și CNP_Platitor după transfer. Nu s-au executat publicare, SQL remote sau restart de serviciu.

Următoarea subfelie liberă: ADE5-10.

## SLICE-ADE5-10 — log nou și formular gol la pornire <a id="slice-ADE5-10"></a>

### Schimbare și motiv

La cererea utilizatorului, Program pregătește un log de erori nou înainte de inițializarea aplicației. Fișierul anterior este mutat într-o arhivă cu dată/oră și GUID în același dosar; nu este șters. GlobalErrorLog continuă să folosească harness_errors.log. Eșecul pregătirii propagă eroarea spre limita Program.Main.

Formularul nu mai încarcă logul la Shown și nu adaugă stările obișnuite de inițializare. Caseta pornește goală la deschiderea normală; erorile de inițializare rămân afișate și înregistrate. Butonul încarcă numai la cerere logul curent. Jurnalele SQL nu se modifică. Cod/comentarii noi în engleză, mesaje cu diacritice.

### Fișiere

ADECHIT/ADE.Migrator/Program.vb, AdeMigratorForm.vb și README.md; ADECHIT_STATUS.md, ADECHIT/PLAN_IMPLEMENTARE.md, docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md și acest worklog.

### Verificări și limite

Build serial al proiectului ADE.Migrator: 0 erori, 0 avertismente. Arhivarea și caseta goală la lansare: TESTAT, RULAT, PUBLICAT.

## SLICE-ADE5-11 — CNP lipsă importat ca NULL <a id="slice-ADE5-11"></a>

### Schimbare

La cererea utilizatorului, ADE.Migrator transformă marcajele `FARA CNP` și `-1` în `Nothing`, transmis de writer ca `DBNull.Value` (SQL NULL). Se aplică numai câmpurilor CNP/CNP_Platitor: Platitori, Platitori_sub și SS_Buget. Comparația ignoră spațiile marginale și majusculele; alte valori se păstrează exact, ca text. Conversia precedă CopyParentCnp, iar numărătorile apar în notele planului, fără afișarea CNP-urilor reale. Sursele MDB și datele deja importate nu sunt modificate.

### Fișiere

- ADECHIT/ADE.Migrator/AdePlan.vb și README.md.
- ADECHIT_STATUS.md, ADECHIT/PLAN_IMPLEMENTARE.md și registrul ADE0–ADE9.

### Verificări și predare

Verificare statică: câmpurile permit NULL în exportul local 000_DEMO; ConvertTables rulează înainte de CopyParentCnp; writerul folosește DBNull.Value. Nu s-au rulat teste automate, verificări vizuale, DDL sau migrare pe server. Build în dosarul separat `bin/Debug/cnp-null`: 0 erori, 0 avertismente. Buildul în dosarul obișnuit a eșuat la copierea DLL-urilor folosite de un proces deschis; nu s-a oprit procesul. Se folosește versiunea nouă pentru importurile următoare. Utilizatorul probează ambii marcatori, variante cu spații/majuscule, un CNP cu zero inițial și opțiunea de copiere către părinte.

## SLICE-ADE5-12 — referințe la taxe lipsă importate ca NULL <a id="slice-ADE5-12"></a>

### Schimbare

La cererea utilizatorului, LunaD.IDV și Prezenta.IDV intră în lista explicită DanglingAllowed. FixLinks transformă în Nothing numai referințele fără părinte în ValoriTaxe, inclusiv 0 dacă nu există taxa cu IDV=0. Planul afișează numărătorile ca observații, nu ca blocaje. Rândurile și valorile de business se păstrează; referințele existente sunt remapate de writer. SQL primește DBNull.Value.

### Fișiere și verificări

- ADECHIT/ADE.Migrator/AdeSchema.vb și README.md.
- ADECHIT_STATUS.md, PLAN_IMPLEMENTARE.md și registrul ADE0–ADE9.

Build în `bin/Debug/cnp-null`: 0 erori, 0 avertismente (include și ADE5-11). Dosarul obișnuit are DLL-uri folosite de un proces deschis; nu s-a oprit procesul. Verificare statică: exportul local 000_DEMO permite NULL în ambele coloane; FixLinks rulează înainte de scriere și păstrează referințele existente. Nu s-au rulat teste automate, probe vizuale sau migrare; MDB-urile și serverul rămân nemodificate. Utilizatorul recitește sursa și verifică faptul că cele 3/2305 legături raportate anterior apar la conversii, nu la blocaje, apoi testează destinația.

## SLICE-ADE5-13 — cheile rândurilor generate în writer <a id="slice-ADE5-13"></a>

### Problemă și corecție

Utilizatorul a raportat KeyNotFoundException pentru IDGE în InsertRows. BuildEducators/BuildHistory construiesc rânduri fără IDGE/IDI, cu SourceRows=-1; cheile sunt alocate de MariaDB. Writerul citea cheia înainte de a verifica SourceRows, deci verificarea ulterioară nu împiedica excepția.

Citirea cheii și adăugarea în hartă sunt acum în interiorul condiției SourceRows >= 0. Inserarea și remaparea relațiilor rămân aceleași.

### Fișiere și verificări

AdeWriter.vb, README.md, ADECHIT_STATUS.md, PLAN_IMPLEMENTARE.md și registrul ADE0–ADE9. Build în `bin/Debug/generated-keys`: 0 erori, 0 avertismente. Verificare statică a ambelor tipuri de rânduri generate; fără teste automate, probe vizuale, migrare sau acces la server. Utilizatorul folosește executabilul nou și verifică rollback-ul rulării eșuate înainte de reluare.

## SLICE-ADE5-14 — oprire normală fără excepție în debugger <a id="slice-ADE5-14"></a>

### Schimbare

La cererea utilizatorului, oprirea normală nu mai folosește ThrowIfCancellationRequested. InsertRows întoarce numărul pregătit dacă tokenul este anulat; Write verifică imediat rezultatul prin token și face rollback înainte de alte scrieri sau COMMIT. RollbackIfStopped confirmă rollback-ul și scrie jurnalul, apoi Write întoarce Nothing. Înainte de tranzacție, oprirea întoarce Nothing fără scrieri. Formularul recunoaște Nothing ca oprire, invalidează verificarea destinației și nu marchează migrarea drept reușită.

Erorile reale de SQL/rollback/jurnal nu sunt ascunse. Nu se verifică tokenul după COMMIT, astfel încât o oprire tardivă nu prezintă importul confirmat ca anulat.

### Fișiere și verificări

AdeWriter.vb, AdeMigratorForm.vb, README.md, ADECHIT_STATUS.md, PLAN_IMPLEMENTARE.md și registrul ADE0–ADE9. Verificare statică a tuturor punctelor de oprire și a singurului apelant Write. Build în `bin/Debug/stop-without-exception`: 0 erori, 0 avertismente. Fără teste automate, probe vizuale, migrare sau acces la server. Utilizatorul probează Oprește în debug, confirmarea rollback-ului în jurnal și închiderea normală; rerularea cere Testează din nou.

## ADE10-07 — ADE.Migrator: chitanțe dublate corectate de mână, copii reconstituiți din SS_Buget <a id="slice-ADE10-07"></a>

Data: 2026-10-21.

### Ce s-a schimbat și de ce

Două blocaje frecvente la migrare nu aveau altă ieșire decât corectarea sursei Access:

1. **`SS_Buget.IDP` arată către un copil care nu mai există în `Platitori`** (ex.: 1154). Acum planul reconstituie copilul din situațiile lunare: numele, CNP-ul, grupa și «plecat» din ULTIMA situație (după an, apoi IDL), plus un rând în `Platitori_sub` (doar numele copilului, fără adresă sau contact: Access nu le păstrează în `SS_Buget`). Apare o notă cu IDP-urile reconstituite. Pentru acest pas nu există opțiune: se aplică întotdeauna. Efect lateral dorit: `Prezenta`, `Plati` etc. care arătau către același IDP găsesc acum părintele.
2. **Chitanțe cu aceeași serie și același număr** (cheia `uq_Sub_Serie_Numar`). Butonul nou **«Chitanțe dublate…»** (activ când există dubluri) deschide o fereastră cu lista chitanțelor în dublu: numărul se schimbă de mână în coloana «Număr nou», iar după fiecare schimbare toată lista se verifică din nou («Încă în dublu cu IDC …», «Schimbat», «Rezolvat»). Butonul **«Taie cifra miilor la cele mai vechi»**: în fiecare grup de numere identice, toate în afară de cea mai nouă (după dată, apoi IDC) pierd cifra miilor (1234 → 234; 19387 → 1387), numai dacă numărul nou este liber în serie; celelalte rămân și sunt numărate. **«Anulează schimbările»** pune numerele din Access la loc. **«Gata»** refă planul cu numerele schimbate (nu și Access-ul), apoi trebuie repetat «Testează». Numerele schimbate sunt uitate la o nouă citire a MDB-ului.

**Regula «adaugă .1» NU s-a făcut:** `AD_Chitante.Numar` este `int(11)`, deci «1234.1» nu încape; ar cere o coloană nouă (de ex. `NumarSub`, ca la NIR în VERCON), cheia unică extinsă și schimbări în aplicația web (afișare, căutare, următorul număr). Nu am făcut DDL.

### Fișiere

`ADECHIT/ADE.Migrator/AdePlan.vb` (reconstituire, aplicare numere, mesaj), `AdeReceiptNumbers.vb` (nou: analiza și regula), `AdeReceiptsForm.vb` + `AdeReceiptsForm.Designer.vb` (nou: fereastra), `AdeMigratorForm.vb` + `.Designer.vb` (butonul, refacerea planului), `README.md`.

### Verificări

`dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj`: reușit, 0 avertismente, 0 erori. Fără teste, fără rularea migratorului, fără deschiderea ferestrei (Designer-ul scris de mână, niciodată deschis în Visual Studio), fără MariaDB.

### Nerezolvat / neverificat

- Fereastra nu a fost văzută pe ecran; aspectul coloanelor, comportamentul editării în `KBotDataView` și butonul nou din `tlpActiune` rămân de probat.
- Copiii reconstituiți primesc `SI = 0` și fără `DataIntrare`; `Platitori_sub.Activ` = nu «plecat». De confirmat dacă e bine.
- Dacă o chitanță dublă nu are serie sau număr, nu e listată (cheia unică permite NULL).
- Regula «.1» așteaptă decizia pentru schimbarea schemei.
- Ajutorul pentru utilizator (dacă migratorul are) nu e actualizat.
