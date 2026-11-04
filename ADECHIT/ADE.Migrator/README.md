# ADE.Migrator — utilizare

<!-- slice: AD19-02 -->

## Solduri reportate separat după o migrare din 2021

Pentru copiii care existau în 2020 și continuă în 2021, dar au fost migrați numai începând cu 2021, folosiți **Solduri reportate 2020 → 2021 (migrare existentă)...**. SI existent se păstrează. Componenta aferentă anilor omiși se salvează separat în `AD_SOLDURI_REPORTATE`; nu se reia migrarea. Această variantă înlocuiește proiectarea AD19-01 care ar fi rescris SI.

### Pregătire și utilizare

1. Aplicați manual `sql/AD_19_02_solduri_reportate.sql` pe AVACONT_SURSA, apoi propagați schema către baza unității prin sincronizarea existentă. Publicați și codul Python AD19-02 care citește componenta. Migratorul refuză transferul dacă tabelul nu există; aplicațiile nu îl creează automat.
2. Alegeți MDB-ul complet cu 2020 și 2021, cu aceleași ID-uri Access ca sursa migrării existente. Completați DC destinație, numele exact al subunității existente și conexiunea. Schimbarea fișierului poate goli DC-ul/subunitatea; completați-le din nou.
3. Apăsați **Solduri reportate 2020 → 2021 (migrare existentă)...**. Nu este necesar planul complet prin Citește sau acceptarea unei destinații goale prin Testează. Fereastra pregătește o previzualizare numai prin citire.
4. Examinați, pentru fiecare copil, **SI existent (păstrat)**, **Reportat existent**, **Reportat propus**, **Diferență**, **SI + reportat**, **Originea soldului** și **Stare / motiv**. Pozitiv înseamnă debit; negativ înseamnă credit. Reportat existent gol înseamnă că înregistrarea separată încă nu există.
5. Apăsați **Salvează reportările**. Confirmarea arată numărul copiilor și destinația. Numai rândurile **De aplicat** sunt salvate; cele corecte, omise sau blocate nu se scriu. Succesul este indicat prin **COMMIT confirmat**. Pentru recitire, folosiți **Previzualizează**.

<!-- slice: AD19-03 -->

**Doar copiii nerezolvați** este bifat implicit în fereastra soldurilor reportate. Păstrează rândurile **De aplicat**, **Omise** și **Blocate**, cu explicația din **Stare / motiv**. Ascunde reportările deja corecte și cazurile fără sold suplimentar de reportat. Debifați pentru lista completă. După COMMIT confirmat, rândurile aplicate sunt ascunse de acest filtru; pot fi văzute din nou prin debifare. Filtrul schimbă numai afișarea; toate rândurile De aplicat din previzualizare participă la salvare. Rezumatul arată totalul copiilor, numărul afișat, numărul nerezolvat și câte rânduri sunt de aplicat ori omise/blocate. Starea «rezolvat» se referă la transferul soldului reportat, nu la absența tuturor diferențelor dintre situațiile lunare.

<!-- slice: AD19-04 -->

Pentru copilul existent în Platitori din MDB, reportul calculat din SI și mișcările anterioare lui 2021 nu mai este validat prin comparație cu prima situație salvată din 2021. Neconcordanțele istorice, inclusiv cele produse de anulări ulterioare salvării, nu mai blochează acest transfer. Situațiile salvate își păstrează valorile originale. Restul validărilor, formulele și migrarea completă rămân neschimbate. AD19-03 și AD19-04 sunt scrise local, necompilate și netestate.

<!-- slice: AD19-02 -->

Se inserează sau se înlocuiește o singură înregistrare pe subunitate/copil/an de început, cu sold semnat și proveniența MDB. Repetarea nu adună valoarea încă o dată. Un report propus zero fără înregistrare existentă nu generează un rând inutil. SI existent, versiunea copilului, situațiile salvate, documentele, harta ID-urilor și seria chitanțelor nu se modifică. Jurnalul SQL este în `Jurnale/ADE-SI/<DC>/<data_ora>/`, lângă executabil.

Calculul lunar folosește **SI existent + sold reportat + contracte − plăți + restituiri**. Componenta este aplicabilă din 2021 și intră o singură dată în soldul inițial calculat al fiecărei luni. Fișa cont ADECHIT/K-BOT și fișa părinților păstrează rândul inițial existent și afișează distinct **Sold reportat din anii anteriori**, la începutul anului 2021. Rapoartele deja generate trebuie regenerate. Situațiile salvate ale lunilor închise rămân cele existente.

### Calcul și asociere

Sunt eligibili numai copiii cu prezență sau situație salvată atât în 2020, cât și în 2021 în MDB. Asocierea folosește `AD_IdMap` pentru Platitori, în subunitatea aleasă, și verifică numele/CNP-ul. Nu se asociază automat după nume și nu se presupune egalitatea ID-urilor Access/MariaDB. Copilul trebuie să aibă prezență în destinație în 2021.

Se determină soldul complet înainte de 2021: SI din Access + contracte anterioare − plăți neanulate anterioare + restituiri neanulate anterioare, după ordinea lunilor Access și conversia Access Long. Din rezultat se scade SI de bază al copilului: numai diferența este reportată separat, fără dublarea SI. Restituirile fără IDL folosesc luna prezenței legate. Dacă există istoric anterior lui 2020, acesta participă la soldul complet înainte de 2021; selecția copiilor rămâne strict 2020 și 2021.

<!-- slice: AD19-04 -->

Pentru un copil dispărut din Platitori în MDB, soldul poate fi recuperat din SID−SIC al primei situații salvate unice din 2021, scăzând mișcările anterioare din 2021. Se acceptă automat numai dacă SI existent este zero, cum a fost reconstruit de migrator. Această recuperare și validările ei rămân neschimbate. Pentru copilul existent în Platitori se folosește calculul din SI și mișcări; prima situație salvată nu mai este un control de concordanță.

### Blocaje și tranzacție

Destinația trebuie să fie o subunitate activă existentă, cu luni din 2021 și fără luni anterioare deja importate. Prima lună din 2021 trebuie să coincidă cu sursa. Operația nu completează perioade lipsă din interiorul lui 2021.

Lipsa hărții, identitatea neconcordantă, mai multe ID-uri sursă pentru același copil, lipsa prezenței în 2021 în destinație, soldurile imposibil de stabilit și mișcările relevante fără lună sunt explicate pe rând. Dacă SI existent diferă de baza din MDB, rândul este blocat: trebuie clarificat dacă reportul a fost inclus deja în SI. Nicio corectare automată a SI nu este executată.

MDB-ul este controlat prin SHA-256 înainte și după citire. Înainte de aplicare se reface previzualizarea sub blocările existente și se compară cu cea aprobată. Schimbările de sursă sau destinație cer recitire. Rândurile eligibile sunt salvate într-o singură tranzacție; erorile înainte de COMMIT anulează lotul. Dacă rezultatul COMMIT sau rollback nu este confirmat, sunt necesare recitirea soldurilor și clarificarea rezultatului înainte de reluare.

Stare AD19-02: **SCRIS LOCAL, NECOMPILAT, NETESTAT, NEPUBLICAT**. DDL-ul este numai pregătit ca fișier; nu s-a executat pe nicio bază. Sunt necesare aplicarea/sincronizarea lui, publicarea codului Python și compilarea/distribuirea migratorului.

<!-- slice: ADE10-08 -->

**Chitanțe pe ani școlari și jurnal rapid.** Dublurile se caută numai în interiorul aceluiași an școlar; același număr în ani diferiți nu mai blochează migrarea (cheia nouă, AD_13). Caseta «Jurnal de operații și erori» taie o intrare la 4000 de caractere, scrie cel mult 20 de constatări una câte una (restul sunt în «Conversii și constatări») și își scoate liniile cele mai vechi peste 400000 de caractere, ca să nu încetinească când sunt mii de constatări. Butonul **Încarcă logul erorilor** rămâne necenzurat. Build curat; NETESTAT, NERULAT. În bazele unităților, sincronizarea de structură adaugă coloana `AnScolar` și creează cheia nouă (cere `schema_diff.py` corectat pe server).

**Chitanțe cu același număr în același an școlar: sub-numere automate, «Golește date» (ADE10-08).** Planul dă singur un sub-număr chitanțelor cu aceeași serie și același număr în același an școlar (septembrie–august), ca la NIR-urile din VERCON: cea mai nouă din grup rămâne fără sub-număr, cele mai vechi primesc ,1 ,2 … (751, 751,1, 751,2). Numărul din Access nu se schimbă, deci nu e nimic de salvat separat; în bază sub-numărul e `AD_Chitante.NumarSub` (în sutimi: 10, 20, …). Situația **nu mai blochează** migrarea: apare o singură notă în «Conversii și constatări» (câte numere, câte chitanțe, 5 exemple); singurul blocaj rămas e un număr care apare de peste 100 de ori în același an școlar. Fereastra «Chitanțe dublate…» a fost scoasă. Cheia din bază este (subunitate, serie, an școlar, număr, sub-număr). Sub-numerele le dă numai migratorul: aplicația web emite numai numere întregi și le arată **numai în Registrul de casă și în Fișa debitor**. Coloana `NumarSub` și cheia nouă vin din `sql/AD_13_02_chitante_numar_original.sql` (numai în AVACONT_SURSA, apoi sincronizare); **Testează** refuză baza fără ea. Butonul **Golește date** scoate din memorie citirea, planul, bifele de grupe, educatorii, verificarea serverului și jurnalul de pe ecran; rămân fișierul, DC-ul, subunitatea și serverul; nu șterge nimic din Access, de pe disc sau de pe server. Build curat; NETESTAT, NERULAT.

<!-- slice: ADE10-07 -->

**Chitanțe dublate și copii lipsă (ADE10-07).** Un copil din `SS_Buget.IDP` care nu mai există în `Platitori` este reconstituit din ultima situație lunară (nume, CNP, grupă, plecat) și primește un rând în `Platitori_sub` cu numele copilului; nota apare în conversii. (ADE10-07: fereastra «Chitanțe dublate…» și regula «taie cifra miilor» au fost scoase în ADE10-08; vezi mai sus.) Build curat; TESTAT, RULAT, PUBLICAT.

<!-- slice: ADE5-05 -->

Utilitar Windows separat, în stilul KBot.Migrator, pentru Access ADECHIT → MariaDB. Executabilul păstrează numele existent `ADE.Migrator.exe`.

Documentele din `Plati` și `Retur` cu IDP/IDS către `Platitori`/`Platitori_sub` inexistente sunt excluse din tabelele obișnuite. Se exclud și `Chitante`/`AlteDoc` legate de aceste plăți sau de plăți inexistente. Rândurile originale complete (inclusiv coloanele care nu se migrează) se păstrează ca JSON în `AD_DateEronate`, cu motivul, cheia Access, anii detectați, fișierul și hash-ul sursei, în aceeași tranzacție și subunitate ca restul importului. Referințele NULL păstrează regula existentă pentru legături opționale. Alte tabele și alte legături păstrează regulile existente.

Aceste documente blochează migrarea numai dacă data documentului sau luna asociată indică **2025 ori 2026** (ani ficși). Pentru chitanțe/alte documente fără an propriu identificabil, se folosesc data și luna plății asociate. Fără an identificabil, documentul se arhivează fără blocaj. Dacă există blocaje, nu se scrie nimic pe server, inclusiv în arhivă, până la corectarea sursei. Când există documente excluse, tabelul și numărul lor apar în plan și în rezultatul importului.

DDL: `sql/AD_12_date_eronate.sql` se aplică **numai în AVACONT_SURSA**, apoi utilizatorul propagă structura prin **KBotAdmin**. Migratorul nu creează tabelul.

Sincronizarea structurii nu copiază rândul de inițializare `AD_Lock.ID=0` (blocarea comună a portalului). Importul îl creează singur în tranzacția lui, dacă lipsește; nu mai este nimic de rulat manual. Pentru baze cu date existente, inițializarea datelor portalului rămâne în `AD_08_02_interogare_unica.sql`.

Chitanțele cu aceeași serie și același număr în același an școlar (septembrie–august) primesc automat sub-număr, vezi ADE10-08 mai sus. Indexul `uq_Sub_Serie_An_Numar` (`sql/AD_13_chitante_an_scolar.sql` și `sql/AD_13_02_chitante_numar_original.sql`) impune unicitatea pe subunitate, serie, an școlar, număr și sub-număr, chitanțele anulate incluse; același număr în ani școlari diferiți este permis, fiindcă numerotarea poate reîncepe în septembrie. Anul școlar este coloana calculată `AnScolar` (din `Data`); migratorul nu o scrie. O chitanță fără dată nu intră în verificare (notă în plan). **AD_13 și AD_13_02 se aplică înainte de Testează.**

<!-- slice: ADE6-24 -->
Emailul și CNP-ul educatorilor (`AD_Grupe_Educator.EMail/CNP`) sunt opționale și se completează numai în ADECHIT web. Baza Access nu conține aceste coloane; migratorul nu le citește și nu le așteaptă. Inserările educatorilor omit aceste câmpuri, astfel încât rămân NULL implicit. Nu este necesară remigrarea datelor. DDL-ul `sql/AD_09_educatori_contact.sql` se aplică numai în AVACONT_SURSA; sincronizarea schemelor bazelor unităților se face prin KBotAdmin.

<!-- slice: AD11-01 -->
Înainte de noua migrare se aplică DDL-ul `sql/AD_08_portal_parinti.sql` pe AVACONT_SURSA, după AD_05–07, apoi se sincronizează schema prin AvacontPush. Datele existente se completează prin `sql/AD_08_02_interogare_unica.sql`, în „Interogări unice”. Writerul verifică registrul portalului și CodAccesPortal și generează un cod aleatoriu comun asocierilor părintelui cu copii fără Plecat. AD11-03: conflictele CNP/email din sursele vechi nu opresc migrarea și nu schimbă contactele importate; părinții afectați au portalul blocat până la corectare, cu avertizări navigabile în fereastra Plătitori. Codurile nu sunt incluse în jurnalul SQL. Trimiterea emailului se face ulterior din macheta părinților. [Ghid portal](../PORTAL_PARINTI.md).

<!-- slice: ADE5-06 -->

Sub buton: starea permanentă a migrării și primul motiv de blocaj. **Jurnal de operații și erori**, jos, păstrează toate motivele, mesajele sesiunii, blocajele Access/MariaDB și excepțiile complete. **Încarcă logul erorilor** adaugă, numai la apăsare, ultimele maximum 256 KiB din `harness_errors.log`, fără ștergerea diagnosticului sesiunii. Conexiunea înregistrează gazda/portul, fără parolă.

<!-- slice: ADE5-10 -->

La lansare, logul anterior se arhivează în același dosar ca `harness_errors_ADE_<data_ora>_<identificator>.log`; noul `harness_errors.log` și caseta de jos pornesc goale. Inițializarea obișnuită nu umple caseta; erorile reale sunt afișate și jurnalizate. Jurnalele SQL de migrare nu se resetează.

Un `Connect Timeout expired` la Testează înseamnă că serverul nu a confirmat conexiunea în timpul alocat. Verificați gazda, portul și accesul la server și repetați Testează. Conectarea reușită fără un plan citit nu activează Migrează: întâi Citește, apoi Testează. Eșecul salvării preferințelor locale este afișat și jurnalizat, dar permite continuarea verificării sursei/destinației.

1. Alegeți MDB-ul și apăsați **Citește**. Această operație citește numai fișierul local.
2. Verificați numerele de rânduri, conversiile, blocajele, bifa **Plecați** și educatorii. Corectați educatorii în ultima coloană; modificarea reface planul și cere o nouă verificare a serverului.
3. Completați conexiunea și **DC destinație**, apoi apăsați **Testează**. DC-ul este propus din `Unitati.DC`, dar îl puteți modifica. Tabelele de destinație trebuie să existe, să fie goale și să folosească InnoDB. Seria existentă de chitanțe nu se suprascrie. Se verifică și coloanele AD_Imports.
4. Apăsați **Migrează** după rezolvarea blocajelor. Utilitarul verifică din nou hash-ul MDB și destinația înainte de scriere. Schimbarea fișierului sau a datelor conexiunii invalidează verificarea precedentă. Nu folosiți simultan două utilitare pe aceeași bază.

<!-- slice: ADE5-07 -->

<!-- slice: ADE5-08 -->

Lista **Grupe** propune automat bifa **Plecați** când denumirea conține `pleca`, indiferent de majuscule (de exemplu „Plecați”, „COPII PLECATI”). Puteți bifa o grupă cu altă denumire sau debifa o propunere nepotrivită. Se acceptă cel mult o grupă specială; mai multe bife blochează migrarea până la corectare. Fără nicio bifă apare o observație în plan. Bifa confirmată determină `Tip=PLECATI` în AD_Grupe și reconstruirea istoricului copiilor plecați.

Schimbarea unei bife reface planul și cere din nou **Testează**. La o nouă citire a MDB-ului, propunerile se reconstruiesc după denumirile din fișier. MDB-ul nu este modificat. ADE5-08: build cu 0 erori/avertismente; TESTAT, RULAT, PUBLICAT.

<!-- slice: ADE5-13 -->

Rândurile de educatori și istoric generate de migrator primesc ID-urile `IDGE` și `IDI` de la MariaDB. Writerul nu caută o cheie Access în aceste rânduri; maparea ID-urilor sursă se aplică numai tabelelor citite din MDB.

<!-- slice: ADE5-12 -->

Referințele `LunaD.IDV` și `Prezenta.IDV` fără corespondent în `ValoriTaxe` (inclusiv valoarea 0 când nu există taxa cu IDV=0) se importă ca SQL `NULL`. Numărul legăturilor lipsă apare în conversiile planului, fără să blocheze migrarea. Rândurile se păstrează, iar referințele la taxe existente se remapează normal.

<!-- slice: ADE5-11 -->

În câmpurile `Platitori.CNP`, `Platitori_sub.CNP_Platitor` și `SS_Buget.CNP`, marcajele `FARA CNP` și `-1` se importă ca SQL `NULL`. Recunoașterea ignoră spațiile de la margini și diferențele de majuscule. Celelalte valori se păstrează ca text, inclusiv zerourile inițiale. Numărul conversiilor apare în rezumatul planului. Conversia precedă opțiunea de copiere a CNP-ului copilului către părinte; un marcaj de CNP lipsă nu este copiat. MDB-ul rămâne nemodificat.

<!-- slice: ADE5-09 -->
Opțiunea **CNP Copil = CNP Părinte**, din zona sursei Access, este implicit nebifată. Bifați-o când `Platitori.CNP` din Access conține CNP-ul părintelui. În acest caz, valoarea se copiază și în `AD_Platitori_sub.CNP_Platitor` pentru toți plătitorii legați prin IDP de copil, inclusiv peste un CNP al plătitorului deja completat în sursă. CNP-ul copilului se păstrează. Valorile sursă goale nu suprascriu CNP-ul plătitorului. Fără bifă, maparea existentă rămâne valabilă. Schimbarea bifei reface planul, arată conversia în rezumat și cere din nou **Testează** înainte de migrare. Alegerea rămâne la recitirea sursei în aceeași sesiune; nu este memorată între porniri. Compilare curată; migrarea: TESTAT, RULAT, PUBLICAT.

Caseta **DC destinație** este editabilă între operații. Modificarea ei cere o nouă apăsare pe Testează; planul Access rămâne încărcat. Verificarea, migrarea și seria de chitanțe folosesc DC-ul ales. Confirmarea și jurnalul arată separat DC-ul sursei și destinația. MDB-ul nu este modificat. O nouă citire propune din nou DC-ul din Access.

**Oprește** solicită oprirea migrării înainte de COMMIT. Instrucțiunea SQL curentă trebuie să se termine înainte ca solicitarea să fie observată; apoi se încearcă rollback. ADE5-14: oprirea normală nu mai aruncă `OperationCanceledException` în debugger. Writerul execută rollback explicit, scrie confirmarea în jurnal și întoarce o stare de oprire; formularul nu o prezintă ca migrare reușită. Erorile reale de rollback sau de conexiune sunt în continuare raportate. După trimiterea COMMIT, oprirea nu poate anula o tranzacție confirmată. La închiderea ferestrei în timpul unei operații, utilitarul așteaptă eliberarea conexiunilor. Citirea și verificarea conexiunii se așteaptă până la terminare; butonul Oprește este activ numai în timpul migrării. Nu se închid procese MSACCESS.

Progresul afișează tabelul și rândurile pregătite, la fiecare 100 de rânduri și la finalul tabelului. Aceste rânduri sunt confirmate numai după COMMIT.

Jurnalele sunt în `Jurnale/ADE/<DC>/<data_ora>/`, lângă executabil:

- `_00_info.txt`: identificarea sursei și a bazei, fără parola conexiunii;
- fișiere `.sql`: reconstrucția comenzilor parametrizate, înregistrate înainte de execuție;
- `_99_final.txt`: COMMIT sau rollback confirmat;
- `_outcome.sql`: rezultat necunoscut dacă serverul nu a confirmat COMMIT/rollback.

Fișierele SQL pot descrie rânduri anulate prin rollback. Dacă rezultatul este necunoscut, verificați AD_Imports și tabelele pe server înainte de reluare. O eroare de scriere a jurnalului este înregistrată și afișată prin mecanismul comun; jurnalizarea se dezactivează, iar tranzacția continuă conform comportamentului SqlDumpWriter din KBot.Migrator.

<!-- slice: ADE10-03 -->

**Subunitate (ADE10-03).** Fiecare MDB se importă într-o *subunitate* a DC-ului destinație. Câmpul **Subunitate** (propus din `Unitati.Denumire`) alege destinația: un nume nou creează subunitatea, numele uneia existente o alege dacă este activă și goală. Se poate importa un al doilea MDB în aceeași bază, în altă subunitate. Cheile din Access nu se păstrează: serverul le alocă, iar referințele se rescriu prin hartă (`AD_IdMap`); `LunaD.Ordine` păstrează ordinea lunilor din Access. Seria și numărul chitanțelor din MDB intră în `AD_ReceiptConfig` al subunității; `AVACONT_COMUN.Unitati_Chitante` nu se mai atinge. **Testează** cere schema din `sql/AD_05_subunitati.sql` aplicată și refuză un fișier deja importat în DC. Build curat; migrarea: TESTAT, RULAT, PUBLICAT.

Schema se pregătește separat de utilizator: AD_03, pentru perioadele taxelor AD_04, iar pentru subunități AD_05. Utilitarul nu execută DDL și nu inventează perioade pentru datele istorice NULL.

ADE5-05: build verificat; utilitarul, oprirea, interfața și scrierea pe MariaDB: TESTAT, RULAT, PUBLICAT.
