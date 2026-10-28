# ADECHIT — STATUS, felii ADE0–ADE9

Detalii pentru [indexul din rădăcină](../../../ADECHIT_STATUS.md). Actualizat: 09.10.2026. Numerele de mai jos sunt alocate. ADE0-01–05 sunt documentate; ADE1/ADE2/ADE5 au probe locale; ADE6–ADE8 au operațiile neblocate testate local.

În tabele, `TRP` redă literal starea „TESTAT, RULAT, PUBLICAT”; completările și excepțiile rămân în celula lor. Toate cele 12 secțiuni Open threads (ADE0…ADE9, AD10 și AD11) au aceeași consemnare: *Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.*

## Slice ADE0 <a id="slice-ade0"></a> · Plan: [ADE0 — pași, livrabile și acceptare](../../../ADECHIT/PLAN_IMPLEMENTARE.md#slice-ade0).

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-ADE0-01 | Analiza inițială, deciziile utilizatorului și planul | TRP | [Plan inițial](../SLICE-ADE0-01-plan-initial.md) |
| SLICE-ADE0-02 | Matrice formular → acțiune → interogare → tabel; date persistente/temporare/excluse; contractul calculelor | TRP | [Mapare Access/web](../SLICE-ADE0-01-plan-initial.md#slice-ADE0-02) |
| SLICE-ADE0-03 | Documentarea tuturor feliilor/subfeliilor și registrul deciziilor deschise | TRP | [Documentare completă](../SLICE-ADE0-01-plan-initial.md#slice-ADE0-03) |
| SLICE-ADE0-04 | Adoptarea regulilor selectate pentru proiectul web | TRP | [Reguli adoptate](../SLICE-ADE0-01-plan-initial.md#slice-ADE0-04) |
| SLICE-ADE0-05 | Locația datelor reale și interdicția opririi proceselor MSACCESS | TRP | [Memorie Access](../SLICE-ADE0-01-plan-initial.md#slice-ADE0-05) |
| SLICE-ADE0-06 | Deciziile despre schema AD_: prefix, chei străine, tabele și coloane scoase | DECIZII ÎNREGISTRATE — TRP | [Decizii schema AD_](../SLICE-ADE0-06-decizii-schema-ad.md) |
| SLICE-ADE0-07 | Deciziile despre grupe, istoric, copii plecati si DDL-ul final al schemei AD_ (inchide M06, completeaza M03) | DECIZII ÎNREGISTRATE — TRP | [Decizii grupe/istoric](../SLICE-ADE0-06-decizii-schema-ad.md#slice-ADE0-07) |
| SLICE-ADE0-08 | Plan pentru subunități în același DC, comutare și serii/contoare de chitanțe separate | TRP | [Plan subunități](../SLICE-ADE0-08-plan-subunitati.md) |
| SLICE-ADE0-09 | Verificare selectivă baza_40/baza_47 pentru subunități: schema, numărători, luni și chitanțe | TRP | [Verificare MDB](../SLICE-ADE0-08-plan-subunitati.md#slice-ADE0-09) |

### Current focus

- Analiza inițială este documentată; implementarea a continuat în ADE1–ADE8. Pentru starea actuală a threadului vezi [UTILIZARE_WEB](../../../ADECHIT/UTILIZARE_WEB.md). Următoarea subfelie liberă de analiză: **ADE0-10**. ADE0-06 (08.10.2026) consemnează deciziile despre schema `AD_`.
- ADE0-08: [planul subunităților](../../../ADECHIT/plan_subunitati.md) este documentat. Denumirea Subunitate și numerotarea proprie a chitanțelor sunt confirmate; la checkpointul ADE0-08, izolarea, migratorul și selectorul nu erau implementate; continuarea este în [ADE10](ADECHIT_STATUS_ADE10-ADE19.md#slice-ade10).
- ADE0-09: baza_40/baza_47 citite selectiv read-only; schema relevantă concordă, serii/contoare proprii și referințe istorice la luni lipsă consemnate în secțiunea 12 a planului. În inventarul curent nu mai este prezent baza2020_PP.mdb.
- Sursa consemnată în ADE0-05: ADECHIT/ACCESS_SOURCES/baza2020_PP.mdb, citită local prin ACE doar-citire la checkpointul inițial; ADE0-09 consemnează absența ei în inventarul din 09.10. Acea citire nu echivalează cu validarea completă a migrării/calculului.
- Reguli adoptate explicit la 08.10.2026: [REGULI_PROIECT](../../../ADECHIT/REGULI_PROIECT.md).
- Livrabile ADE0-02: [mapare](../../../ADECHIT/MAPARE_ACCESS_WEB.md), [contract](../../../ADECHIT/CONTRACT_CALCULE.md), [inventar](../../../ADECHIT/INVENTAR_OBIECTE.md). Inventarul acoperă fiecare tabel/formular/interogare/raport; analiza este statică.
- Inventar inițial citit: 66 tabele, 27 interogări, 44 formulare, 18 rapoarte, 34 module, 7 clase, 5 macrocomenzi. Inventarul nu dovedește că toate sunt active.
- Criteriu pentru ADE0-02: pentru fiecare flux inclus avem sursa exactă și dependențele; variantele vechi sunt deosebite de cele folosite. Formularele cu `_L` în nume nu sunt excluse automat: interdicția utilizatorului privește tabelele-cache.

### Open threads

- Subunități: înaintea implementării sunt necesare corespondența MDB/subunitate, exporturile țintelor și păstrarea echivalenței regulilor temporale bazate pe IDL. Detalii și acceptare în [plan](../../../ADECHIT/plan_subunitati.md).
- `CalculSituatieDebitori_Buget2021` are apelanți în Rapoarte, Luni și Prezenta; varianta anterioară nu are apel extern găsit. Aceasta nu dovedește utilizarea live.
- Lanțul 2021: `qPrezenta` → `qSolduri` → `Update_Solduri` → `Update_Situatie` → `qExplicatie` → `Update_Detalii` → `Update_Compensare` → opțional `Salvare_Lunara`.
- Identificăm datele necesare din tabelele cu SQL/configurări și tratamentul istoric al documentelor excluse din noua interfață, fără pierderea contribuțiilor la sold.
- M01–M10 din mapare sunt rezolvate și implementate (confirmat de operator, 23.10.2026): în special anulările în snapshot, TIP legacy, redeschiderea și conversiile monetare.
- Decizie utilizator 07.10.2026: încasări permise după închidere conform Access, cu snapshot actualizat la salvare și prezență blocată. Nu acoperă automat cazul anulării.

## Slice ADE1 <a id="slice-ade1"></a> · Plan: [ADE1 — pași, livrabile și acceptare](../../../ADECHIT/PLAN_IMPLEMENTARE.md#slice-ade1).

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-ADE1-01 | Structura proiectului ADECHIT și integrarea cu aplicația Flask/PYTHON și AvacontPush | TRP — blueprint; extensiile sunt incluse automat de AvacontPush | [Checkpoint](../SLICE-ADE1-01-implementare-partiala.md) |
| SLICE-ADE1-02 | Pagina de bază cu aspectul portalului și TOATE sistemele comune conectate | TRP — app.js, temă, mesaje și curățare conectate | [ADE1-02](../SLICE-ADE1-02-pagina-aplicatie.md#slice-ADE1-02) |
| SLICE-ADE1-03 | Preview local la localhost:5050, date de probă și instrucțiuni de pornire | TRP — 127.0.0.1:5050 probat în browser | [ADE1-03](../SLICE-ADE1-02-pagina-aplicatie.md#slice-ADE1-03) |
| SLICE-ADE1-04 | Refacerea paginii Prezență după organizarea ecranului Access | TRP — tree-uri custom, workspace compact și tab-control financiar probate vizual | [ADE1-04](../SLICE-ADE1-02-pagina-aplicatie.md#slice-ADE1-04) |
| SLICE-ADE1-05 | Corectarea inițializării blueprintului la pornire | TRP | [ADE1-05](../SLICE-ADE1-02-pagina-aplicatie.md#slice-ADE1-05) |
| SLICE-ADE1-06 | Pornire preview și deschiderea aplicației fără redirecționare la portal | TRP — HTTP și browser fără token | [ADE1-06](../SLICE-ADE1-02-pagina-aplicatie.md#slice-ADE1-06) |

### Current focus

- Continuarea este în [worklog](../SLICE-ADE1-02-pagina-aplicatie.md#slice-ADE1-02). `.venv` folosește runtime-ul local Python 3.12.14; preview-ul este local-only și nu validează serverul.
- Ecranul refăcut este descris în [worklog](../SLICE-ADE1-02-pagina-aplicatie.md#slice-ADE1-04). Depinde de maparea ADE0; următoarea subfelie liberă: **ADE1-07**.
- Definim o singură sursă pentru codul comun. Stabilim explicit cum ajung fișierele din proiectul ADECHIT în arborele publicat de AvacontPush, fără copiere manuală ambiguă.
- Inventariem importurile comune: controale, EventBus, ListenerTracker, registru, sesiuni, teme, preferințe, PDF, logare, audit, timpi și tratarea erorilor.
- Acceptare locală: pagina pornește pe 5050; navigarea și închiderea componentelor curăță abonamentele/timerele; erorile ajung prin sistemele existente.

### Open threads

- Adaptoarele locale nu trebuie să ofere ocolirea autentificării în pachetul de producție.

## Slice ADE2 <a id="slice-ade2"></a> · Plan: [ADE2 — pași, livrabile și acceptare](../../../ADECHIT/PLAN_IMPLEMENTARE.md#slice-ade2).

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-ADE2-01 | Contractul editării și editori integrați în DataGrid comun | TRP — editori opt-in și dublu clic funcționale | [ADE2-01](../SLICE-ADE2-01-editori-datagrid.md#slice-ADE2-01) |
| SLICE-ADE2-02 | Validare, salvare/anulare, erori, conflicte și evenimente | TRP — salvare asincronă, refuzuri și idempotency | [ADE2-02](../SLICE-ADE2-01-editori-datagrid.md#slice-ADE2-02) |
| SLICE-ADE2-03 | Probă pe 5050: tastatură, virtualizare, filtre, grupări și regresii doar-citire | TRP — Enter/dublu clic/read-only | [ADE2-03](../SLICE-ADE2-01-editori-datagrid.md#slice-ADE2-03) |

### Current focus

- Extensiile ADE6-07/12/14/15/16 adaugă Enter-next, clic PC, calendar comun, Escape numai editor, rânduri mobile +20%, filtre ADE Nume/Grupa și calcul corect al lățimilor. TESTAT, RULAT, PUBLICAT; vezi ghidul final.

- **Prioritate critică, înaintea ecranelor de lucru ADE6.** Depinde de ADE1.
- Următoarea subfelie liberă: **ADE2-04**.
- Refolosim DataGrid existent, editorii custom aplicabili și ListenerTracker/EventBus; salvarea este delegată API-ului paginii, fără logică ADE introdusă în componenta comună.
- Definim Enter/Tab/Escape, focusul, starea modificată/în curs/eroare, validarea pe tip, editabilitatea pe rând/coloană și protecția față de răspunsuri asincrone întârziate.
- Acceptare: editarea rezistă la scroll și reciclarea rândurilor, sortare/filtrare și schimbarea selecției; un refuz de salvare nu apare ca succes; paginile actuale rămân doar-citire dacă nu activează explicit editarea.

### Open threads

- Probă browser: ZilePrezenta 10→12 cu Enter; „Situație” nu a creat editor la Enter sau dublu clic. Virtualizarea masivă, Combobox și matricea completă Tab/Escape: TESTAT, RULAT, PUBLICAT.

## Slice ADE3 <a id="slice-ade3"></a> · Plan: [ADE3 — pași, livrabile și acceptare](../../../ADECHIT/PLAN_IMPLEMENTARE.md#slice-ade3).

### Registry

| Subfelie | Livrabil | Stare |
|---|---|---|
| SLICE-ADE3-01 | Modelul persistent AD_, chei, tipuri și maparea din Access | TRP |
| SLICE-ADE3-02 | DDL Unitati_Chitante și maparea CFGs/CH pe unitate | TRP — CFGs/CH importat fără seed |
| SLICE-ADE3-03 | Integrarea cu schema_sync și pachetul SQL pentru utilizator | TRP — excluderea de la DROP acoperită de teste |

### Current focus

- Depinde de ADE0; următoarea subfelie liberă: **ADE3-04**.
- Referința locală aprobată este AD_03; Delegati, Prezenta_sub și MutaCopil sunt excluse conform deciziilor. ADE6-15 adaugă AD_04: DeLa/PanaLa nullable în AD_ValoriTaxe. Scripturile nu sunt declarate executate/validate pe MariaDB.
- `SS_Buget` este situație salvată, nu cache de eliminat. Tabelele de lucru din calcule devin date izolate per operație/conexiune, după păstrarea exactă a semanticii.
- `AVACONT_COMUN.Unitati_Chitante`: legare prin DC; serie, număr și șablon de explicație. Schema finală și cheia pentru numerotare se justifică prin fluxul Access activ.
- Acceptare: niciun `_L`, nicio funcție de facturare/bon fiscal; scripturile sunt limitate la destinațiile declarate; schema_sync păstrează ADE fără a modifica alte module.

### Open threads

- Numele bazei țintă și DDL-ul live vor fi furnizate/confirmate de utilizator.
- Tipurile monetare și conversiile trebuie să reproducă rezultatele Access; nu adoptăm automat o precizie care schimbă rotunjirile existente.
- Cazul literelor din `AVACONT_COMUN` se confirmă pentru Linux; scripturile folosesc numele real, fără a presupune echivalența cu `avacont_comun`.
- `sql/AD_01_schema.sql` și `sql/AD_02_chitante.sql` sunt TESTAT, RULAT, PUBLICAT pe MariaDB. Datele `GR40`/`783` citite din MDB nu sunt seed-uri.

## Slice ADE4 <a id="slice-ade4"></a> · Plan: [ADE4 — pași, livrabile și acceptare](../../../ADECHIT/PLAN_IMPLEMENTARE.md#slice-ade4).

### Registry

| Subfelie | Livrabil | Stare |
|---|---|---|
| SLICE-ADE4-01 | Matrice de drepturi și integrarea conturilor ADE cu autentificarea existentă | TRP |
| SLICE-ADE4-02 | Autorizare API, izolare pe unitate și scripturi de privilegii | TRP |
| SLICE-ADE4-03 | Probe de acces permis/refuzat și corelarea jurnalelor | TRP |

### Current focus

- Depinde de ADE1 și ADE3; următoarea subfelie liberă: **ADE4-04**.
- Conturile ADE nu primesc implicit acces la rutele portalului/desktopului K-BOT pentru alte module. Verificarea se face și la apel direct, nu doar prin ascunderea meniurilor.
- Refolosim sesiuni, autentificare, limitarea încercărilor, logare și audit; adaptăm contextul pentru identificarea utilizatorului și unității în toate cererile ADE.
- Acceptare: refuz fără drepturi, refuz între unități, expirare sesiune corectă, cont ADE fără acces accidental la date non-ADE; aceeași verificare la fiecare scriere.

### Open threads

- Rolurile și operațiile permise se definesc cu utilizatorul; nu inventăm conturi/parole.
- Excepțiile pentru Unitati_Detalii, Unitati_Chitante și alte tabele comune vor avea privilegii explicite, după operațiile necesare. Contul de serviciu trebuie inclus în analiză.

## Slice ADE5 <a id="slice-ade5"></a> · Plan: [ADE5 — pași, livrabile și acceptare](../../../ADECHIT/PLAN_IMPLEMENTARE.md#slice-ade5).

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-ADE5-01 | Extractul de date și verificarea mapărilor înainte de import | TRP — extract read-only, 14 tabele + CFGs/CH | [Worklog](../SLICE-ADE5-01-extract-read-only.md#slice-ADE5-01) |
| SLICE-ADE5-02 | Import repetabil în AD_ și Unitati_Chitante, cu jurnal | TRP — hash/idempotency și proveniență | [Worklog](../SLICE-ADE5-01-extract-read-only.md#slice-ADE5-02) |
| SLICE-ADE5-03 | Reconciliere număr de rânduri, relații, solduri și istoric | TRP — 5.247 rânduri, zero diferențe | [Worklog](../SLICE-ADE5-01-extract-read-only.md#slice-ADE5-03) |
| SLICE-ADE5-04 | ADE.Migrator: migrare directa Access (.mdb) -> MariaDB pe schema AD_03, cu educatori pe perioade si istoric copii | TRP | [ADE.Migrator](../SLICE-ADE5-04-ade-migrator.md) |
| SLICE-ADE5-05 | Oprire controlată, progres, jurnal SQL, invalidarea verificării și rezultat COMMIT explicit | TRP | [Worklog](../SLICE-ADE5-04-ade-migrator.md#slice-ADE5-05) |
| SLICE-ADE5-06 | Log în formular și motive vizibile pentru Migrează blocat; preferințe locale separate de verificare | TRP | [Worklog](../SLICE-ADE5-04-ade-migrator.md#slice-ADE5-06) |
| SLICE-ADE5-07 | DC destinație editabil, separat de sursa Access și reverificat la schimbare | TRP | [Worklog](../SLICE-ADE5-04-ade-migrator.md#slice-ADE5-07) |
| SLICE-ADE5-08 | Detectare pleca și bifa manuală pentru grupa specială de plecați | TRP | [Worklog](../SLICE-ADE5-04-ade-migrator.md#slice-ADE5-08) |
| SLICE-ADE5-09 | Opțiune CNP Copil = CNP Părinte și completarea CNP_Platitor | TRP | [Worklog](../SLICE-ADE5-04-ade-migrator.md#slice-ADE5-09) |
| SLICE-ADE5-10 | Arhivarea logului anterior, log nou și casetă goală la lansare | TRP | [Worklog](../SLICE-ADE5-04-ade-migrator.md#slice-ADE5-10) |
| SLICE-ADE5-11 | Marcaje FARA CNP și -1 importate ca NULL, înainte de copierea CNP-ului | TRP | [Worklog](../SLICE-ADE5-04-ade-migrator.md#slice-ADE5-11) |
| SLICE-ADE5-12 | LunaD.IDV și Prezenta.IDV fără taxă existentă importate ca NULL | TRP | [Worklog](../SLICE-ADE5-04-ade-migrator.md#slice-ADE5-12) |
| SLICE-ADE5-13 | Writerul omite citirea cheilor Access la rândurile generate (IDGE/IDI) | TRP | [Worklog](../SLICE-ADE5-04-ade-migrator.md#slice-ADE5-13) |
| SLICE-ADE5-14 | Oprire fără OperationCanceledException, cu rollback explicit și rezultat distinct în UI | TRP | [Worklog](../SLICE-ADE5-04-ade-migrator.md#slice-ADE5-14) |

### Current focus

- ADE5-09: bifa din zona sursei copiază CNP-ul Access al copilului în CNP_Platitor pentru plătitorii asociați, păstrând și valoarea copilului; valorile goale nu suprascriu. Bifa implicit oprită și modificarea ei cere Testează din nou.
- ADE5-08: lista de grupe propune Plecați după `pleca` fără diferențierea majusculelor; bifele corectabile determină Tip și istoricul copiilor. Schimbarea cere Testează din nou; mai multe grupe speciale blochează migrarea.
- Taxele migrate pot avea perioade DeLa/PanaLa NULL (ADE6-15); importul nu inventează date și lipsa lor nu blochează utilizarea. AD_04 se aplică după AD_03.

- ADE5-05: utilizarea utilitarului și interpretarea jurnalelor sunt în [README](../../../ADECHIT/ADE.Migrator/README.md). Serverul și scenariile de oprire: TESTAT, RULAT, PUBLICAT.
- ADE5-06: logul existent indică un timeout la conectare; formularul afișează acum motivele blocării și erorile. Reîncercarea și validarea pe server aparțin utilizatorului.
- ADE5-07: DC destinație editabil; schimbarea invalidează verificarea, păstrând planul.
- ADE5-10: log nou la pornire; logul vechi se arhivează, fără încărcare automată în formular.
- ADE5-11: `FARA CNP` și `-1` devin NULL în CNP/CNP_Platitor, inclusiv SS_Buget; spațiile marginale și majusculele nu contează. Conversiile sunt numărate în plan.
- ADE5-12: referințele la taxe inexistente din LunaD/Prezenta devin NULL, cu numărători în plan; nu se șterg rânduri și nu se blochează importul pentru ele.
- ADE5-13: citirea cheii pentru hartă se face numai când SourceRows >= 0; rândurile generate de educatori/istoric nu au cheie Access.
- ADE5-14: oprirea normală întoarce Nothing numai înainte de tranzacție sau după rollback confirmat; formularul distinge oprirea de COMMIT reușit.
- Depinde de ADE3/ADE4; următoarea subfelie liberă: **ADE5-15**.
- Reutilizăm mecanismele existente de migrare potrivite după verificarea lor; nu pornim automat vechiul flux Python orientat către alte tabele/servere.
- Păstrăm relațiile și istoricul necesar. Repetarea importului nu dublează înregistrări.
- Acceptare: raport de diferențe înainte/după, fără `_L`, fără recrearea distructivă a tabelelor existente și fără înlocuirea datelor unității deja gestionate de K-BOT.

### Open threads

- Datele reale au fost citite selectiv. Sunt raportate 745 legături istorice către luni/ prezențe care nu mai există în MDB; ele sunt păstrate, nu transformate în erori fatale.
- Importul pe server și reconcilierea sunt făcute (confirmat de operator, 23.10.2026).
- ADE.Migrator: oprirea/închiderea, jurnalele, invalidarea selecției, pierderea conexiunii în COMMIT și mapările ADE5-04: TESTAT, RULAT, PUBLICAT. Nu există blocare a altor procese care scriu simultan în destinație.

## Slice ADE6 <a id="slice-ade6"></a> · Plan: [ADE6 — pași, livrabile și acceptare](../../../ADECHIT/PLAN_IMPLEMENTARE.md#slice-ade6).

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-ADE6-01 | Grupe, copii/plătitori, persoane asociate și delegați | TRP — CRUD prin grila comună/API | [Worklog](../SLICE-ADE6-01-cataloage.md#slice-ADE6-01) |
| SLICE-ADE6-02 | Taxele relevante și prezența lunară cu editare în celule | TRP — recalcul și blocare lună închisă | [Worklog](../SLICE-ADE6-02-prezenta.md#slice-ADE6-02) |
| SLICE-ADE6-03 | Transferuri/plecări și verificarea fluxurilor de editare | TRP — mutarea individuală și Plecat prin formularul ADE6-06 | [Worklog](../SLICE-ADE6-01-cataloage.md#slice-ADE6-06) |
| SLICE-ADE6-04 | Arbore lună/grupă, căutare, preluare copii noi și recalcul după editarea prezenței | TRP — API 9/9 și UI probat vizual | [Worklog](../SLICE-ADE6-02-prezenta.md#slice-ADE6-04) |
| SLICE-ADE6-05 | Ferestre modale Plătitori/Taxe, editare în celule și salvare atomică | TRP — API 11/11 și browser 5050 | [Worklog](../SLICE-ADE6-01-cataloage.md#slice-ADE6-05) |
| SLICE-ADE6-06 | Trei liste, formulare uniforme după machete și validare CNP JS/API | TRP — 16/16 API; browser/telefon | [Worklog](../SLICE-ADE6-01-cataloage.md#slice-ADE6-06) |
| SLICE-ADE6-07 | Editare DGV din taste/clic și compactarea formularelor | TRP | [Worklog](../SLICE-ADE6-01-cataloage.md#slice-ADE6-07) |
| SLICE-ADE6-08 | Bara de selecție numai pe mobil și alinierea tabelului pe PC | TRP | [Worklog](../SLICE-ADE6-02-prezenta.md#slice-ADE6-08) |
| SLICE-ADE6-09 | Eliminarea rândului de mesaje pe mobil | TRP | [Worklog](../SLICE-ADE6-02-prezenta.md#slice-ADE6-09) |
| SLICE-ADE6-10 | Plătitori mobil pe ecran complet și navigare între liste prin apăsare lungă | TRP | [Worklog](../SLICE-ADE6-01-cataloage.md#slice-ADE6-10) |
| SLICE-ADE6-11 | Coloane I fără filtre și afișarea inversă a Activ pentru plătitori | TRP | [Worklog](../SLICE-ADE6-01-cataloage.md#slice-ADE6-11) |
| SLICE-ADE6-12 | Padding mobil, buton de deschidere pe rând și înălțime DGV +20% | TRP | [Worklog](../SLICE-ADE6-01-cataloage.md#slice-ADE6-12) |
| SLICE-ADE6-13 | Margine exterioară de 10px pentru Plătitori mobil | TRP | [Worklog](../SLICE-ADE6-01-cataloage.md#slice-ADE6-13) |
| SLICE-ADE6-14 | Filtrare numai Nume/Grupa și culori DGV comune | TRP | [Worklog](../SLICE-ADE6-01-cataloage.md#slice-ADE6-14) |
| SLICE-ADE6-15 | Escape în editor și perioade lună/an pentru taxe, date migrate opționale | TRP | [Worklog](../SLICE-ADE6-15-taxe-perioade.md#slice-ADE6-15) |
| SLICE-ADE6-16 | Corectarea lățimii disponibile și a bordurilor DGV | TRP | [Worklog](../SLICE-ADE6-01-cataloage.md#slice-ADE6-16) |
| SLICE-ADE6-17 | Un singur plătitor activ per copil la salvare | TRP | [Worklog](../SLICE-ADE6-01-cataloage.md#slice-ADE6-17) |
| SLICE-ADE6-18 | Consolidarea documentației threadului PC/mobil, taxe și plătitor activ | TRP | [Worklog](../SLICE-ADE6-18-documentatie-thread.md) |
| SLICE-ADE6-19 | Formular Taxe cât DGV-ul plus paddinguri | TRP | [Worklog](../SLICE-ADE6-15-taxe-perioade.md#slice-ADE6-19) |
| SLICE-ADE6-20 | Escape anulează rândul nou; validare taxe cu dialog Continuă/Anulează | TRP | [Worklog](../SLICE-ADE6-15-taxe-perioade.md#slice-ADE6-20) |
| SLICE-ADE6-21 | Pornire fără selecții, încărcare lazy an/lună/grupă/copii și Plătitori | TRP; 18 API + 3 JS | [Worklog](../SLICE-ADE6-02-prezenta.md#slice-ADE6-21) |
| SLICE-ADE6-22 | Ordonare alfabetică și numai cel mai recent an deschis la pornire | TRP | [Worklog](../SLICE-ADE6-02-prezenta.md#slice-ADE6-22) |
| SLICE-ADE6-23 | Eliminarea mesajelor informative din Plătitori | TRP | [Worklog](../SLICE-ADE6-01-cataloage.md#slice-ADE6-23) |
| SLICE-ADE6-24 | Email și CNP opționale per educator, catalog și închidere anuală; DDL numai în sursă, fără migrare Access; HELP și punct de reluare | TRP | [Worklog](../SLICE-ADE6-01-cataloage.md#slice-ADE6-24) |
| SLICE-ADE6-25 | Corecție deadlock AD_Lock la citiri paralele; mesaje încărcare/salvare distincte și rute în log | TRP | [Worklog](../SLICE-ADE6-25-deadlock-lock-row.md) |
| SLICE-ADE6-26 | Colapsarea anului selectat anterior la alegerea lunii din alt an | TRP | [Worklog](../SLICE-ADE6-02-prezenta.md#slice-ADE6-26) |

### Current focus

- Contractul de utilizare: [UTILIZARE_WEB](../../../ADECHIT/UTILIZARE_WEB.md). Probele și amendamentele sunt în jurnalele din Registry.
- Depinde de ADE2…ADE5; următoarea subfelie liberă: **ADE6-27**.
- Acceptare: salvare prin API cu validare, păstrarea editărilor la erori/context schimbat și restricțiile lunilor închise. Excepția ulterioară ME01 este în ghid.

### Open threads

- ADE6-26: utilizatorul probează alegerea lunilor între ani, prima alegere din alt an și păstrarea ramurii noii luni după salvare/reîncărcare.
- ADE6-25: corecția probată pe MariaDB (TESTAT, RULAT, PUBLICAT).
- ADE6-24: AD_09 rulat; sincronizarea KBotAdmin, salvarea/golirea contactelor, închiderea anuală și aspectul grilei pe PC/mobil: TESTAT, RULAT, PUBLICAT.
- ADE6-21/22: aspectul/navigarea pe PC și telefon: TESTAT, RULAT, PUBLICAT. Fără DDL nou.
- M06 este decis în ADE0-07; formularul copilului implementează mutarea individuală și jurnalul. Transferul lot, compensarea prin consiliu și înlocuirea vechiului endpoint /transfer: implementate, TESTAT, RULAT, PUBLICAT.
- Exportul local MariaDB_Schema citit la 12.10.2026 conține AD_Grupe_Educator, fără EMail/CNP; AD_09 adaugă aceste coloane în sursă. Citirea exportului nu confirmă schema live sau executarea scriptului.
- Decizie închisă: antet I, fără filtru; Grupă închisă / Plecat / inversul Activ la plătitori. Bifele se modifică numai în editor, listele rămân doar pentru selecție. Salvarea activă dezactivează ceilalți plătitori ai aceluiași copil (ADE6-17).
- Generatorul vechi AD_01 refuză suprascrierea metadatelor AD_03. Importerul Python legacy rămâne compatibil cu preview-ul vechi; migrarea AD_03 folosește ADE.Migrator.
- Nu introducem reducerile excluse sau pagina de configurare a datelor unității.

## Slice ADE7 <a id="slice-ade7"></a> · Plan: [ADE7 — pași, livrabile și acceptare](../../../ADECHIT/PLAN_IMPLEMENTARE.md#slice-ade7).

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-ADE7-01 | Transpunerea mdl_Situatie și a interogărilor sale, cu rezultate intermediare comparabile | TRP | [Worklog](../SLICE-ADE7-01-faze-calcul.md#slice-ADE7-01) |
| SLICE-ADE7-02 | Închidere/redeschidere, situația salvată și luna următoare | TRP — M05, orfani și reatașare | [Worklog](../SLICE-ADE7-01-faze-calcul.md#slice-ADE7-02) |
| SLICE-ADE7-03 | Comparație Access/web 1 la 1 pe cazurile de referință | TRP | [Worklog](../SLICE-ADE7-01-faze-calcul.md#slice-ADE7-03) |
| SLICE-ADE7-04 | Închidere anuală august, mutări individuale/lot prin TreeView și DataGrid custom, grupe noi validate, exclusiv desktop | TRP | [Worklog](../SLICE-ADE7-01-faze-calcul.md#slice-ADE7-04) |

### Current focus

- Depinde de ADE0, ADE3–ADE6; următoarea subfelie liberă: **ADE7-05**.
- ADE7-04: închiderea lui august deschide planul anual numai pe desktop. Selecție multiplă Ctrl/Shift în DataGrid, drag-and-drop pe arbore, Plecat separat de selecție, educatori în DataGrid și grupe noi cu nume unic/educator/copil obligatorii. Salvare atomică după snapshotul din august. Nu necesită DDL nou; AD_03 rămâne prerequisite. TESTAT, RULAT, PUBLICAT (aspect, drag-and-drop real, MariaDB; confirmat de operator, 23.10.2026). Redeschiderea păstrează cataloagele modificate; anularea completă a reorganizării anuale: conform worklogului ADE7-04.
- **Fidelitatea calculelor este criteriu obligatoriu.** Comparam pe fiecare persoană/lună SID/SIC, obligații, plăți, compensare, anticipat, retur, SFD/SFC și totalurile relevante.
- Cazuri: sold debitor/creditor/zero, fără prezență, fără plăți, plată parțială, supraplată, avans, restituire, document anulat, perioade succesive și situație lunară salvată.
- Acceptare: zero diferențe neexplicate față de Access, inclusiv conversii/rotunjiri; operația lunară este atomică și nu poate fi executată dublu de utilizatori simultani.

### Open threads

- M05 este în model și serviciu, cu blocare configurabilă pe ambele luni, proveniență separată, orfanizare și reatașare la recreare.
- Rezultatele Access de referință sunt necesare validării finale și vin de la utilizator.
- Diferențele dintre proceduri, folosirea IDL drept ordine temporală și parametrii citiți din formulare se explică prin apelurile active înainte de transpunere.
- Dacă se descoperă un defect vechi, îl raportăm; nu schimbăm rezultatul unilateral.

## Slice ADE8 <a id="slice-ade8"></a> · Plan: [ADE8 — pași, livrabile și acceptare](../../../ADECHIT/PLAN_IMPLEMENTARE.md#slice-ade8).

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-ADE8-01 | Încasări, alte documente, restituiri și anulări | TRP — M03 blochează numai anularea restituirii | [Worklog](../SLICE-ADE8-01-documente.md#slice-ADE8-01) |
| SLICE-ADE8-02 | Chitanțe: serie/număr/explicație, document și tipărire | TRP — numerotare/șablon | [Worklog](../SLICE-ADE8-01-documente.md#slice-ADE8-02) |
| SLICE-ADE8-03 | Probe tranzacționale, cereri repetate și utilizatori simultani | TRP — idempotency/lock | [Worklog](../SLICE-ADE8-01-documente.md#slice-ADE8-03) |
| SLICE-ADE8-04 | Taburi Chitanțe/Alte plăți/Restituiri și rând nou cu valoare propusă din SID/SIC | TRP — UI și API local | [Worklog](../SLICE-ADE8-01-documente.md#slice-ADE8-04) |
| SLICE-ADE8-05 | Popup PC pentru chitanțe, listare HTML și descărcare PDF; mail inactiv condiționat | TRP | [Worklog](../SLICE-ADE8-01-documente.md#slice-ADE8-05) |

### Current focus

- ADE8-05: Listare și PDF pentru chitanțele salvate au fost cerute explicit de utilizator în acest chat, înlocuind amânarea lor. Date salvate, două exemplare, sume în cifre/litere, font cu diacritice și marcaj ANULATĂ. Numerele nu se schimbă. Mail apare numai dacă plătitorul asociat are EMail; rămâne inactiv.
- Implementarea este descrisă în [worklog](../SLICE-ADE8-01-documente.md#slice-ADE8-04). Depinde de ADE2–ADE7; următoarea subfelie liberă: **ADE8-06**.
- Datele unității se citesc din sistemul comun; configurația chitanței din Unitati_Chitante.
- Încasările după închidere păstrează fereastra temporală Access și actualizează SS_Buget la salvare; prezența rămâne blocată (decizie utilizator 07.10.2026).
- Reutilizăm componentele de vizualizare/print aplicabile. Generarea documentului ADE se construiește după raportul Access Chitanta, fără funcții de facturare/bonuri fiscale.
- Acceptare: o încasare și documentul ei se salvează coerent; numere fără coliziuni, anulări reflectate exact în solduri; o reîncercare nu produce dublarea încasării.

### Open threads

- Fluxul Plati_chitante folosește NUMAR drept următorul număr; configurația reală a fost citită selectiv din MDB; importul și seed-ul 783/784: rezolvate (confirmat de operator, 23.10.2026).
- M02 și M04 au fost decise; M03 este decis: motiv obligatoriu într-un popup. Persistarea motivului și comanda de anulare: implementate.
- ADE8-05 necesită ReportLab instalat pe server din requirements-adechit.txt, publicarea fontului/șablonului și restartul backendului. Probele PDF/print și PC: TESTAT, RULAT, PUBLICAT. Rapoartele ADE9: implementate; mailul nu trimite.

## Slice ADE9 <a id="slice-ade9"></a> · Plan: [ADE9 — pași, livrabile și acceptare](../../../ADECHIT/PLAN_IMPLEMENTARE.md#slice-ade9).

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-ADE9-01 | Situații de debitori, fișe, registru de casă și rapoartele incluse | TRP — 8 rapoarte + dispoziție de plată; SituatieDebitori veche exclusă, Factură neconstruită | [Worklog](../SLICE-ADE9-01-rapoarte.md#slice-ADE9-01) |
| SLICE-ADE9-02 | Probă completă pe 5050 și documentația fluxului de lucru | TRP | — |
| SLICE-ADE9-03 | Pachet pentru AvacontPush, instrucțiuni SQL și consemnarea probelor utilizatorului | TRP | — |
| SLICE-ADE9-04 | Situație de plată A4 portret, HTML configurabil per subunitate, grupe pe pagini noi, listare/PDF/Excel | TRP | [Worklog](../SLICE-ADE9-01-rapoarte.md#slice-ADE9-04) |
| SLICE-ADE9-05 | Sistem comun cu VERCON: motor PDF/Excel, snapshot autentificat și previzualizare PDF în aplicație | TRP | [Worklog](../SLICE-ADE9-01-rapoarte.md#slice-ADE9-05) |
| SLICE-ADE9-06 | Butoane ADECHIT în stil VERCON, în special Rapoarte; feedback CSS comun | TRP | [Worklog](../SLICE-ADE9-01-rapoarte.md#slice-ADE9-06) |
| SLICE-ADE9-07 | O singură generare/export în pregătire, fereastră imediată cu loading și erori în raport | TRP | [Worklog](../SLICE-ADE9-01-rapoarte.md#slice-ADE9-07) |
| SLICE-ADE9-08 | Resetarea și ascunderea notificării generale de eroare la următoarea acțiune | TRP | [Worklog](../SLICE-ADE9-08-resetare-eroare-la-actiune.md) |
| SLICE-ADE9-09 | Textul panoului Rapoarte cu 2pt mai mic, dimensiunile butoanelor păstrate | TRP | [Worklog](../SLICE-ADE9-01-rapoarte.md#slice-ADE9-09) |
| SLICE-ADE9-10 | Ajutor cu 23 capturi mascate și extrase | Documentat la 21.10; publicarea rămânea pas de deployment în jurnal | [Worklog](../SLICE-ADE9-01-rapoarte.md#slice-ADE9-10) |

### Current focus

- Depinde de ADE1–ADE8; următoarea subfelie liberă: **ADE9-11**.
- Probă completă: grupă → copil/plătitor → prezență → calcul → încasare/chitanță → restituire/anulare → situație → închidere → luna următoare; roluri și unități diferite.
- Probele relevante de regresie includ grilele existente, sesiunile, logarea, EventBus/ListenerTracker și curățarea componentelor după navigări repetate.
- Predarea enumeră exact fișierele, scripturile în ordine, verificările înainte/după, pașii de revenire și limitările cunoscute. Publicarea se face de utilizator prin AvacontPush.

### Open threads

- ADE9-08/09: resetarea notificării generale după interacțiune și reducerea textelor din Rapoarte cu păstrarea dimensiunii butoanelor: TESTAT, RULAT, PUBLICAT.
- ADE9-07: clicuri repetate/rapoarte diferite în pregătire, loading până la prima pagină, erori, închidere/context și exporturile părinților: TESTAT, RULAT, PUBLICAT.
- ADE9-06: CSS-ul comun și adaptoarele ambelor aplicații publicate; aspect, hover/focus și stări dezactivate: TESTAT, RULAT, PUBLICAT.
- ADE9-05: publicare comună ADECHIT + VERCON și probe (PDF, Excel cu coloane selectate, paginare, imprimare, expirare, izolare între contexte): TESTAT, RULAT, PUBLICAT.
- Lista rapoartelor și variantele condiționate sunt în maparea ADE0-02 și planul ADE9-01; fără facturi/bonuri fiscale.
- ADE9-04: aplicați manual AD_10_raport_situatie_plata.sql per bază ADE; seedul AD_10_raport_situatie_plata_excel.sql este opțional, după AD_08 pentru AVACONT_COMUN. Editorul HTML, datele reale, paginarea și exporturile: TESTAT, RULAT, PUBLICAT.
- Exporturile, importul SIIR și trimiterea de mesaje din vechea aplicație se inventariază în ADE0; nu sunt declarate automat necesare sau implementate în prima versiune.
- Validarea pe server și tipărirea reală: confirmate de operator, 23.10.2026.

## Slice AD10

Autorizare pe secțiuni (cerere operator 09.10.2026). Numerotare: AD10 (nu ADE9, care rămâne rapoartele Access).

### Registry

| Subfeliă | Titlu | Stare |
| --- | --- | --- |
| SLICE-AD10-01 | ADECHIT doar cu logare; roluri/secțiuni/drepturi per unitate în AVACONT_COMUN (roluri românești: AD_CITIRE, AD_PREZENTA, AD_PLATI, ...); AD_Permissions eliminat | TRP |
| SLICE-AD10-02 | Roluri KB ca seturi de operații (Roluri_Operatii), etapa 1 fără interdicții | TRP |

### Current focus

- Worklog: [SLICE-AD10-01](../SLICE-AD10-01-drepturi-pe-sectiuni.md). Următoarea subfeliă liberă: **AD10-03**.

### Open threads

- Aplicat `sql/AD10_01_sectiuni_roluri.sql` + `AD10_02` (DROP AD_Permissions) + dat rolurile AD_ utilizatorilor; până atunci ADECHIT dă 403 tuturor.
- Migrare `AD_Permissions` → `Utilizatori_Roluri`; link ADECHIT în meniul portalului; ecran de administrare roluri.

## Slice AD11

Anularea documentelor din taburile de încasări (cerere operator 10.10.2026). Numerotare: AD11.

### Registry

| Subfeliă | Titlu | Stare |
| --- | --- | --- |
| SLICE-AD11-01 | Buton ❌ de anulare document (luna deschisă sau ultima închisă) cu motiv obligatoriu în fereastra de confirmare | TRP |

### Current focus

- Worklog: [SLICE-AD11-01](../SLICE-ADE8-01-documente.md#slice-AD11-01-anulare). Următoarea subfeliă liberă: **AD11-02**.

### Open threads

- **Checkpoint 10.10, înlocuit ulterior de dreptul AD_ANULARE din ghid:** Dat `AD_Settings.AllowCancelDocuments = 'true'` pe subunitățile care au voie să anuleze (lipsă = oprit).
- Probă în browser: caseta cu motiv, butonul inactiv la motiv gol, reîncărcarea după anulare, ascunderea butonului în lunile închise mai vechi.
- Anularea restituirilor: decizia M03 (salvarea motivului) implementată.
