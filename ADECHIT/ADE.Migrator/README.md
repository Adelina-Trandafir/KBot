# ADE.Migrator — utilizare

<!-- slice: ADE5-05 -->

Utilitar Windows separat, în stilul KBot.Migrator, pentru Access ADECHIT → MariaDB.
Executabilul păstrează numele existent `ADE.Migrator.exe`.

<!-- slice: AD11-01 -->
Înainte de noua migrare se aplică `sql/AD_08_portal_parinti.sql`, după AD_05–07.
Writerul verifică registrul portalului și CodAccesPortal, respinge conflictele CNP/email
în DC și generează un cod aleatoriu comun asocierilor părintelui cu copii fără Plecat.
Codurile nu sunt incluse în jurnalul SQL. Trimiterea emailului se face ulterior din
macheta părinților. [Ghid portal](../PORTAL_PARINTI.md).

<!-- slice: ADE5-06 -->

Sub buton apare permanent starea migrării. Când este blocată, primul motiv apare
acolo, iar lista completă este în **Jurnal de operații și erori**, în partea de jos.
Jurnalul păstrează mesajele sesiunii, blocajele Access/MariaDB și excepțiile complete.
**Încarcă logul erorilor** adaugă ultimele maximum 256 KiB din jurnalul comun
`harness_errors.log`; încărcarea se face numai la apăsarea butonului. Reîncărcarea nu
șterge diagnosticul sesiunii. Conexiunea înregistrează gazda/portul, fără parolă.

<!-- slice: ADE5-10 -->

La fiecare lansare, logul anterior este arhivat în același dosar, cu numele
`harness_errors_ADE_<data_ora>_<identificator>.log`, și se creează un
`harness_errors.log` gol pentru rularea nouă. Caseta de jos pornește goală;
mesajele obișnuite de inițializare nu sunt adăugate în ea. Erorile reale de
inițializare sunt afișate și jurnalizate. Jurnalele SQL de migrare nu sunt resetate.

Un `Connect Timeout expired` la Testează înseamnă că serverul nu a confirmat
conexiunea în timpul alocat. Verificați gazda, portul și accesul la server și repetați
Testează. Conectarea reușită fără un plan citit nu activează Migrează: întâi Citește,
apoi Testează. Eșecul salvării preferințelor locale este afișat și jurnalizat, dar
permite continuarea verificării sursei/destinației.

1. Alegeți MDB-ul și apăsați **Citește**. Această operație citește numai fișierul local.
2. Verificați numerele de rânduri, conversiile, blocajele, bifa **Plecați** și educatorii. Corectați educatorii
   în ultima coloană; modificarea reface planul și cere o nouă verificare a serverului.
3. Completați conexiunea și **DC destinație**, apoi apăsați **Testează**. DC-ul este
   propus din `Unitati.DC`, dar îl puteți modifica. Tabelele de destinație trebuie să existe, să fie goale și să folosească
   InnoDB. Seria existentă de chitanțe nu se suprascrie. Se verifică și coloanele AD_Imports.
4. Apăsați **Migrează** după rezolvarea blocajelor. Utilitarul verifică din nou hash-ul
   MDB și destinația înainte de scriere. Schimbarea fișierului sau a datelor conexiunii
   invalidează verificarea precedentă. Nu folosiți simultan două utilitare pe aceeași bază.

<!-- slice: ADE5-07 -->

<!-- slice: ADE5-08 -->

Lista **Grupe** propune automat bifa **Plecați** când denumirea conține `pleca`,
indiferent de majuscule (de exemplu „Plecați”, „COPII PLECATI”). Puteți bifa
o grupă cu altă denumire sau debifa o propunere nepotrivită. Se acceptă cel mult
o grupă specială; mai multe bife blochează migrarea până la corectare. Fără
nicio bifă apare o observație în plan. Bifa confirmată determină `Tip=PLECATI`
în AD_Grupe și reconstruirea istoricului copiilor plecați.

Schimbarea unei bife reface planul și cere din nou **Testează**. La o nouă
citire a MDB-ului, propunerile se reconstruiesc după denumirile din fișier.
MDB-ul nu este modificat. ADE5-08: build cu 0 erori/avertismente, fără teste
automate, probă vizuală sau migrare executată din chat.

<!-- slice: ADE5-13 -->

Rândurile de educatori și istoric generate de migrator primesc ID-urile `IDGE`
și `IDI` de la MariaDB. Writerul nu caută o cheie Access în aceste rânduri;
maparea ID-urilor sursă se aplică numai tabelelor citite din MDB.

<!-- slice: ADE5-12 -->

Referințele `LunaD.IDV` și `Prezenta.IDV` fără corespondent în `ValoriTaxe`
(inclusiv valoarea 0 când nu există taxa cu IDV=0) se importă ca SQL `NULL`.
Numărul legăturilor lipsă apare în conversiile planului, fără să blocheze migrarea.
Rândurile se păstrează, iar referințele la taxe existente se remapează normal.

<!-- slice: ADE5-11 -->

În câmpurile `Platitori.CNP`, `Platitori_sub.CNP_Platitor` și `SS_Buget.CNP`,
marcajele `FARA CNP` și `-1` se importă ca SQL `NULL`. Recunoașterea ignoră
spațiile de la margini și diferențele de majuscule. Celelalte valori se păstrează
ca text, inclusiv zerourile inițiale. Numărul conversiilor apare în rezumatul
planului. Conversia precedă opțiunea de copiere a CNP-ului copilului către părinte;
un marcaj de CNP lipsă nu este copiat. MDB-ul rămâne nemodificat.

<!-- slice: ADE5-09 -->
Opțiunea **CNP Copil = CNP Părinte**, din zona sursei Access, este implicit
nebifată. Bifați-o când `Platitori.CNP` din Access conține CNP-ul părintelui.
În acest caz, valoarea se copiază și în `AD_Platitori_sub.CNP_Platitor` pentru
toți plătitorii legați prin IDP de copil, inclusiv peste un CNP al plătitorului
deja completat în sursă. CNP-ul copilului se păstrează. Valorile sursă goale
nu suprascriu CNP-ul plătitorului. Fără bifă, maparea existentă rămâne valabilă.
Schimbarea bifei reface planul, arată conversia în rezumat și cere din nou
**Testează** înainte de migrare. Alegerea rămâne la recitirea sursei în aceeași
sesiune; nu este memorată între porniri. Compilare curată; migrarea nu a fost probată.

Caseta **DC destinație** este editabilă între operații. Modificarea ei cere o nouă
apăsare pe Testează; planul Access rămâne încărcat. Verificarea, migrarea și seria
de chitanțe folosesc DC-ul ales. Confirmarea și jurnalul arată separat DC-ul sursei
și destinația. MDB-ul nu este modificat. O nouă citire propune din nou DC-ul din Access.

**Oprește** solicită oprirea migrării înainte de COMMIT. Instrucțiunea SQL curentă
trebuie să se termine înainte ca solicitarea să fie observată; apoi se încearcă rollback.
ADE5-14: oprirea normală nu mai aruncă `OperationCanceledException` în debugger.
Writerul execută rollback explicit, scrie confirmarea în jurnal și întoarce o
stare de oprire; formularul nu o prezintă ca migrare reușită. Erorile reale de
rollback sau de conexiune sunt în continuare raportate.
După trimiterea COMMIT, oprirea nu poate anula o tranzacție confirmată.
La închiderea ferestrei în timpul unei operații, utilitarul așteaptă eliberarea conexiunilor.
Citirea și verificarea conexiunii se așteaptă până la terminare; butonul Oprește este
activ numai în timpul migrării. Nu se închid procese MSACCESS.

Progresul afișează tabelul și rândurile pregătite, la fiecare 100 de rânduri și la finalul
tabelului. Aceste rânduri sunt confirmate numai după COMMIT.

Jurnalele sunt în `Jurnale/ADE/<DC>/<data_ora>/`, lângă executabil:

- `_00_info.txt`: identificarea sursei și a bazei, fără parola conexiunii;
- fișiere `.sql`: reconstrucția comenzilor parametrizate, înregistrate înainte de execuție;
- `_99_final.txt`: COMMIT sau rollback confirmat;
- `_outcome.sql`: rezultat necunoscut dacă serverul nu a confirmat COMMIT/rollback.

Fișierele SQL pot descrie rânduri anulate prin rollback. Dacă rezultatul este necunoscut,
verificați AD_Imports și tabelele pe server înainte de reluare. O eroare de scriere a
jurnalului este înregistrată și afișată prin mecanismul comun; jurnalizarea se dezactivează,
iar tranzacția continuă conform comportamentului SqlDumpWriter din KBot.Migrator.

<!-- slice: ADE10-03 -->

**Subunitate (ADE10-03).** Fiecare MDB se importă într-o *subunitate* a DC-ului destinație. Câmpul **Subunitate** (propus din
`Unitati.Denumire`) alege destinația: un nume nou creează subunitatea, numele uneia existente o alege dacă este activă și goală.
Se poate importa un al doilea MDB în aceeași bază, în altă subunitate. Cheile din Access nu se păstrează: serverul le alocă,
iar referințele se rescriu prin hartă (`AD_IdMap`); `LunaD.Ordine` păstrează ordinea lunilor din Access. Seria și numărul
chitanțelor din MDB intră în `AD_ReceiptConfig` al subunității; `AVACONT_COMUN.Unitati_Chitante` nu se mai atinge.
**Testează** cere schema din `sql/AD_05_subunitati.sql` aplicată și refuză un fișier deja importat în DC. Build curat; migrarea nu a fost rulată.

Schema se pregătește separat de utilizator: AD_03, pentru perioadele taxelor AD_04, iar pentru subunități AD_05.
Utilitarul nu execută DDL și nu inventează perioade pentru datele istorice NULL.

ADE5-05: build verificat; utilitarul, oprirea, interfața și scrierea pe MariaDB rămân
de probat de utilizator. Nu s-au rulat teste automate sau verificări vizuale.
