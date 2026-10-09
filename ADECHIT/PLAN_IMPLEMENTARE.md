# ADECHIT — planul detaliat al tuturor feliilor

Actualizat: 09.10.2026, ADE6-18. Planul descrie livrabilele; starea efectivă și
numerele permanente se urmăresc în [ADECHIT_STATUS](../ADECHIT_STATUS.md).

## Checkpoint de reluare — 09.10.2026

Regulile finale ale interfeței și salvării sunt în [UTILIZARE_WEB](UTILIZARE_WEB.md),
care înlocuiește propunerile intermediare pentru ADE2/ADE6. Importul local a
reconciliat 5.247 de rânduri. ADE6-06 a trecut 16 teste API și probe browser locale;
ADE6-07–17 sunt scrise local, fără teste la cererea utilizatorului. ADE1-06 a
verificat numai pornirea/deschiderea aplicației, nonvizual, pe localhost:5050.
Nimic nu este validat pe MariaDB/server și paritatea completă cu Access nu este demonstrată.

M01–M09 au decizii înregistrate. M03 rămâne de implementat; M06 este parțial
implementat, cu transferul lot și compensarea încă deschise. Rămân drepturile ADE4,
probele Access intermediare și validarea predării. AD_04 adaugă perioade nullable
pentru taxe după AD_03; executarea aparține utilizatorului. Preview-ul necesită
restart după ultimele modificări backend/schema.

Rapoartele ADE9 rămân excluse din acest pas. La cererea explicită a utilizatorului,
ADE8-05 adaugă separat listarea și PDF-ul chitanțelor deja salvate.

## Navigare

| Felie | Plan | Dependență principală |
|---|---|---|
| ADE0 | [Analiză și documentare](#slice-ade0) | Sursele Access și deciziile utilizatorului |
| ADE1 | [Integrare și sisteme comune](#slice-ade1) | ADE0; poate începe fără date reale |
| ADE2 | [Grila editabilă](#slice-ade2) | ADE1; probe cu date fictive |
| ADE3 | [Schema și configurația chitanțelor](#slice-ade3) | ADE0; datele/ținta pentru schema finală |
| ADE4 | [Conturi și autorizare](#slice-ade4) | ADE1, ADE3 |
| ADE5 | [Migrare](#slice-ade5) | ADE3, ADE4; extractul real |
| ADE6 | [Cataloage și prezență](#slice-ade6) | ADE2–ADE5 |
| ADE7 | [Calcule și ciclul lunar](#slice-ade7) | ADE3–ADE6; rezultate Access |
| ADE8 | [Încasări și chitanțe](#slice-ade8) | ADE2–ADE7 |
| ADE9 | [Rapoarte și predare](#slice-ade9) | ADE1–ADE8 |

Ordinea de lucru rămâne ADE0 → ADE1 → ADE2 → ADE3 → ADE4 → ADE5 → ADE6 → ADE7 → ADE8 → ADE9.
Contractele și datele fictive pot fi pregătite înaintea dependențelor, dar nu declarăm
acceptată o funcție dependentă până la verificarea lor. ADE5 verifică importul și
soldurile de referință; comparația cu motorul web încă nescris se închide în ADE7-03.

## Reguli comune de implementare și predare

- Nu se citesc/modifică fișiere Claude.md. Sursele Access rămân referință nemodificată.
- Refolosim toate sistemele existente; extensiile comune nu devin copii ADE separate.
- Codex scrie și verifică local. Utilizatorul face push prin AvacontPush, execută SQL
  și testează Linux/MariaDB. Nu există acces direct Codex la aceste servicii.
- Testarea web locală folosește localhost:5050, fără conexiuni de producție. Adaptoarele
  fictive sunt explicit locale; nu devin ocoliri ale autentificării în producție.
- Tabelele business noi au AD_; excepția cerută este AVACONT_COMUN.Unitati_Chitante.
  Scrierea exactă a numelui bazei comune se verifică înaintea scriptului pentru Linux.
- Nu portăm cele șapte cache-uri _L, facturi, bonuri fiscale sau reduceri pentru frați/
  absențe consecutive. Datele istorice care contribuie la sold nu se elimină arbitrar.
- Editarea în celule este obligatorie de la primul ecran de lucru. Nu se amână.
- Formulele și conversiile din [contract](CONTRACT_CALCULE.md) sunt criteriu de acceptare.
  [Deciziile deschise](DECIZII_DESCHISE.md) nu primesc răspunsuri inventate.
- Fiecare subfelie implementată are worklog separat: schimbare/motiv, fișiere exacte,
  comenzi și rezultate efective, limitări; actualizează indexul și starea. Planul nu ține
  loc de worklog pentru cod viitor. Ajutorul afectat primește marcajul feliei.
- Pentru scrieri: identitatea/unitatea vin din sesiune, câmpurile sunt validate pe server,
  erorile sunt afișate și jurnalizate; succesul este emis după confirmarea tranzacției.
- Verificările enumerate mai jos sunt **de executat**, nu teste deja trecute. Nu pornim
  o suită care cere serverul sub pretextul testării locale. Nu schimbăm infrastructura
  live sau numărul de workeri pe baza exportului local.

## Surse tehnice pentru reutilizare

| Sistem | Sursa existentă | Ce preluăm / verificăm în felia responsabilă |
|---|---|---|
| Aplicație Flask | [main.py](../PYTHON/main.py) | Blueprint în aplicația existentă; pornirea actuală directă este pe 5008, preview-ul ADE va fi separat pe 5050 |
| Portal și sesiuni web | [portal.py](../PYTHON/routes/portal/portal.py), [portal.html](../PYTHON/static/portal.html) | Autentificare, alegere unitate, antete, expirare; nu presupunem echivalența cu sesiunea desktop |
| Sesiuni comune | [session_store.py](../PYTHON/routes/auth/session_store.py), [monitorizare JS](../PYTHON/static/js/session/session-monitoring.js) | Refolosirea mecanismelor și adaptarea contextului, fără un al treilea sistem ADE |
| Aspect / navigare | [portal.css](../PYTHON/static/css/portal.css), [app.js](../PYTHON/static/js/portal/app.js), [theme-init.js](../PYTHON/static/js/portal/theme-init.js) | Structură vizuală, teme, comportament desktop/mobil |
| Preferințe grile | [layouts.js](../PYTHON/static/js/portal/layouts.js), [colprefs.js](../PYTHON/static/js/portal/colprefs.js) | Chei ADE distincte în mecanismul comun, fără suprascrierea altor grile |
| Grilă | [datagrid.js](../PYTHON/static/js/dgv/datagrid.js), [engine.js](../PYTHON/static/js/dgv/engine.js) | Componenta actuală doar-citire, virtualizare, filtre, sortare, grupare, totaluri |
| Controale | [Combobox](../PYTHON/static/js/components/combobox/combobox.js), [TreeView](../PYTHON/static/js/components/treeview/treeview.js) | Selectoare, arbore și editori reutilizabili |
| Evenimente | [EventBus](../PYTHON/static/js/event-bus/event-bus.js) | Aceeași instanță și mecanisme de diagnostic, nu magistrală ADE paralelă |
| Urmărire și curățare | [ListenerTracker](../PYTHON/static/js/listener-tracker/listener-tracker-mixin.js), [registru](../PYTHON/static/js/instances-registry.js) | Înregistrarea/distrugerea componentelor, abonamentelor și timerelor |
| Log și timpi | [logger.py](../PYTHON/utils/logger.py), [timing.py](../PYTHON/utils/timing.py) | Corelarea operațiilor cu utilizator/unitate; contextul g.portal vs g.session de adaptat |
| Bază și retry | [database.py](../PYTHON/utils/database.py), [db_retry.py](../PYTHON/utils/db_retry.py) | Conexiuni comune; retry fără dublarea scrierilor business |
| Documente | [pdfview.js](../PYTHON/static/js/portal/pdfview.js) | Vizualizare/print comun; șabloanele ADE provin din rapoartele Access |
| Sincronizare schemă | [README](../PYTHON/routes/schema_sync/README.md), [schema_common.py](../PYTHON/routes/schema_sync/schema_common.py) | Șablon/ținte, propuneri DDL, protejarea tabelelor ADE |
| Migrare existentă | [README](../PYTHON/routes/migrare/README.md) | Evaluarea parserului/validării; nu rulăm automat mapările altui modul |
| Publicare | [AvacontPush](../AvacontPush/README.md), [.pushignore](../PYTHON/.pushignore) | Arborele publicat, extensii, excluderi și predare către utilizator |

## Slice ADE0

**Obiectiv:** fiecare funcție inclusă are sursă, regulă, destinație și criteriu de verificare.
Intrări: [mapare](MAPARE_ACCESS_WEB.md), [inventar](INVENTAR_OBIECTE.md),
[contract](CONTRACT_CALCULE.md), [relații Access](ACCESS_SOURCES/relationships.md).

### SLICE-ADE0-01 — planul inițial

Livrat documentar: indexul în rădăcină, registrul ADE0–ADE9, numerotarea subfeliilor,
deciziile utilizatorului și limitele accesului la server.
Acceptare documentară: reguli consemnate; implementarea și publicarea nu sunt presupuse.
Istoric: [worklog](../docs/worklog/SLICE-ADE0-01-plan-initial.md).

### SLICE-ADE0-02 — maparea Access/web

Livrat documentar: inventar 155 obiecte, matricea operațiilor, model persistent propus,
lanțul 2021 cu opt interogări, conversii și cazuri de referință; M01–M10 identificate.
Acceptare documentară: fiecare obiect inventariat o dată, șapte cache-uri excluse,
surse legate. Paritatea în execuție nu este demonstrată prin inspecție statică.
Istoric: [worklog](../docs/worklog/SLICE-ADE0-02-mapare-access-web.md).

### SLICE-ADE0-03 — documentarea tuturor feliilor

Livrabilul acestei intervenții: prezentul plan pentru 30 de subfelii, registrul deciziilor
deschise și legături din status spre fiecare felie. Nu alocă alte funcții de business.
Verificări: numerotare fără duplicări, acoperirea fiecărei subfelii alocate, ținte și
ancore ale linkurilor, concordanța stărilor. La final se oprește lucrul, conform cererii.
Următoarea subfelie de analiză liberă: ADE0-04; următoarea implementare: ADE1-01.

## Slice ADE1

**Obiectiv:** ADE este un modul al infrastructurii existente și poate fi probat local.
Dependențe: ADE0-03; reutilizările din tabelul tehnic de mai sus.

### SLICE-ADE1-01 — structura proiectului și AvacontPush

Pași: inventarierea importurilor/punctelor de extensie; stabilirea structurii finale;
înregistrarea unui blueprint ADE fără efecte business la import; documentarea pachetului.
Propunere: codul executabil în PYTHON/routes/adechit și PYTHON/static/js/adechit,
cu pagina/rutele în spațiile /adechit și /api/adechit. ADECHIT rămâne rădăcina funcțională
a proiectului, documentelor și surselor de referință. ADE1-01 fixează această alegere;
dacă se păstrează cod executabil în ADECHIT, trebuie ambalare explicită, fără copii manuale.

Livrabile: schelet, înregistrare în main.py, lista căilor publicate și instrucțiuni locale.
AvacontPush are LocalRoot configurabil; poziția proiectului său în repo nu dovedește
ce arbore publică. Nu schimbăm LocalRoot în KBOT pentru a trimite accidental tot repo-ul.
Verificări: import fără MariaDB, hartă de rute fără coliziuni, calcul local al manifestului
după extensii/excluderi; ACCESS_SOURCES și datele de probă nu intră în static/public.
Acceptare: fiecare fișier executabil are o singură sursă și o destinație de publicare.
Necunoscute: configurația efectivă AvacontPush, consemnată de utilizator la predare.

### SLICE-ADE1-02 — pagina de bază și toate sistemele comune

Pași: reutilizarea cadrului portalului, meniului, temei, preferințelor, controalelor și PDF;
legarea sesiunii și a erorilor; conectarea EventBus, ListenerTracker și registrului.
Adăugăm context ADE în logger/audit/timing prin mecanismele comune, fără jurnal paralel.
Inventariem și mecanismele auxiliare întâlnite la integrare; „toate sistemele” nu se reduce
la simpla includere a fișierelor JS. Fiecare are un traseu de utilizare sau o justificare
pentru momentul activării în felia care îl folosește.

Livrabile: pagina de bază cu stări încărcare/gol/eroare/sesiune expirată și matrice
sistem → import → inițializare → utilizare → cleanup → probă.
Verificări: deschideri/închideri repetate, schimbare unitate, teme, preferințe, un eveniment
și o eroare urmărite prin sistemele existente; zero creștere nejustificată a listenerelor.
Acceptare: reutilizare efectivă, utilizator/unitate corelate, fără succes afișat la eroare.
Necunoscute: adaptarea g.portal la jurnal și compatibilitatea monitorizării sesiunii.

### SLICE-ADE1-03 — preview local 5050

Pași: launcher local explicit; injectarea adaptoarelor de date/auth numai în preview;
set fictiv reproductibil cu două unități, luni deschise/închise și erori simulate.
Nu modificăm pornirea de producție 5008 numai pentru preview și nu încărcăm config.py
cu acces real la importul testelor. Documentăm comanda exactă după crearea launcherului.

Livrabile: pornire/oprire/reset local, scenarii și limitări, separarea pachetului publicabil.
Verificări: localhost:5050 servește pagina și modulele; nicio conexiune externă; port ocupat
raportat clar; ruta de producție refuză accesul fără sesiune chiar dacă există preview.
Acceptare: probe repetabile fără Linux/MariaDB; nu se declară autentificare reală validată.

## Slice ADE2

**Obiectiv:** editare reală în DataGrid comun, păstrând comportamentul existent la ceilalți utilizatori.
Dependențe: ADE1. Nu depinde de schema live pentru probele cu adaptor local.

### SLICE-ADE2-01 — contractul editării și editorii

Pași: contract explicit pe coloană (tip, editor, editabilitate, parse/format, validare),
identitate stabilă pe rând și stare de editare independentă de elementele DOM reciclate.
Editorii pentru text, număr, dată și listă folosesc controalele existente unde se potrivesc.
Valorile NULL, șir vid și zero rămân distincte; formatul românesc nu se parsează prin
eliminarea arbitrară a separatorilor. Câmpurile calculate sunt numai pentru citire.

Livrabile: extensia comună și contract API JS documentat, cu editarea dezactivată implicit.
Interacțiuni aprobate: clic unic pentru editare pe PC; pe mobil clic pentru selecție
și dublu clic/Enter pentru editare. Enter/Tab confirmă și trec la următoarea celulă
editabilă; Shift inversează direcția. Escape anulează editorul, fără să închidă
fereastra. Erorile păstrează editorul. Datele folosesc calendarul custom comun;
perioadele taxelor sunt lună/an fără calendar. Filtre ADE numai la Nume/Grupa.
Verificări: tastatură, editor Combobox, focus vizibil, rânduri de grup/total needitabile.
Acceptare: valoarea se poate modifica în celulă; nicio logică de taxe ADE în DataGrid.

### SLICE-ADE2-02 — salvare, validare, erori și conflicte

Pași: stări explicit distincte (editare, modificat, salvare, salvat, eroare, conflict);
callback asincron al paginii către API cu cheie rând/context/revizie și câmpurile permise.
Unitatea reală se verifică în sesiune; UI nu poate conferi drepturi. API răspunde cu
valorile persistate/recalculate. Revizia și formatul exact se finalizează în ADE3/ADE4.
Răspunsul unei cereri vechi nu se aplică altei luni/unități sau unei noi editări.

Livrabile: contract comun de salvare și evenimente, adaptor fictiv succes/eroare/conflict.
Tranzițiile de context cu modificări nesalvate cer salvare/renunțare/rămânere explicită;
scrollul/virtualizarea nu pierde draftul. Escape în timpul salvării nu pretinde anularea
unei tranzacții deja trimise; rezultatul trebuie reconciliat cu răspunsul serverului.
Verificări: refuz validare, timeout cu rezultat necunoscut, sesiune expirată, dublu Enter,
două modificări concurente; evenimentul de succes se emite numai după salvarea confirmată.
Acceptare: fără pierderi tăcute sau suprascrierea unui rând modificat de alt utilizator.

### SLICE-ADE2-03 — regresie și probe pe 5050

Pași: pagină de probă care folosește aceeași componentă comună în mod editabil și doar-citire;
set suficient de mare pentru reciclarea rândurilor; reproducerea operațiilor din ADE2-01/02.
Verificări: scroll în editare, coloane fixe/ascunse, filtrare și sortare după salvare,
grupare/totaluri recalculabile, schimbare layout/temă, mouse/tastatură/mobil, destroy
în timpul unei cereri. Se includ pagini existente ale portalului pentru regresie.
Livrabile: teste necesare ale contractului și matrice de probe browser cu rezultate reale.
Acceptare: mod doar-citire neschimbat, fără listeners/timere rămase, fără erori în consolă;
pragul de performanță se măsoară față de componenta inițială, nu se inventează un rezultat.

## Slice ADE3

**Obiectiv:** model persistent ADE în baza existentă și configurație comună a chitanței.
Dependențe: maparea ADE0; pentru script final sunt necesare ținta, DDL-ul și profilul datelor.

### SLICE-ADE3-01 — tabele, chei și tipuri

Pași: dicționar sursă/câmp → destinație/câmp/tip/NULL/default/conversie; relații și indexuri;
profil pentru chei duplicate/orfane și pentru ordinea IDL. Baza aleasă de sesiune trebuie
să corespundă unității; nu amestecăm modele per-bază/per-DC fără schema reală.
Tabele (08.10.2026; Delegati, Prezenta_sub și MutaCopil au fost scoase): AD_Grupe, AD_Platitori, AD_Platitori_sub, AD_ValoriTaxe,
AD_LunaD, AD_Prezenta, AD_Plati, AD_Chitante, AD_AlteDoc, AD_Retur,
AD_SS_Buget.

Livrabile: dicționar și DDL local; coloane de revizie/idempotentă dacă sunt necesare,
cu justificare și compatibilitate cu importul. Fără tabele _L sau scratch comun golit.
Nu impunem unicitate (IDP,IDL) unde Access permite (IDP,IDL,IDG); nu transformăm automat
Long/Double în bani DECIMAL fără conversiile intermediare cerute de contract.
Verificări: relații pe set fictiv, orfani raportați, precizie/NULL, indexuri pentru IDZ/IDL/IDP.
Acceptare: model justificat din sursă; SQL etichetat nerulat până la proba utilizatorului.

### SLICE-ADE3-02 — Unitati_Chitante

Pași: schema comună legată de DC; mapare CFGs cu f='CH', SERIE/NUMAR/Explicatie;
validare lipsuri/duplicate și numerotare existentă. NUMAR este următorul număr în fluxul
Plati_chitante; fără seed GR40/783 și fără corectarea automată a istoricului.
Datele unității se citesc din Unitati_Detalii/Unitati_Conturi; fără pagină/rută de editare ADE.

Livrabile: DDL comun separat, reguli de import și contract de rezervare a numărului.
Cheia seriei și schimbarea seriei se stabilesc după date; nu presupunem resetare anuală.
Verificări: două unități, configurație absentă, șabloane cu [LA], rollback la eșec.
Acceptare: schema permite emiterea atomică din ADE8; numărul nu avansează pentru o
operație refuzată înaintea emiterii. Politica exactă la eșec/reîncercare se verifică în ADE8.

### SLICE-ADE3-03 — schema_sync și pachetul SQL

Pași: alegere documentată între includerea ADE în șablonul sursă și o regulă explicită
de protejare/sincronizare; verificarea diff-ului pentru tabelele absente din sursă.
DDL-ul bazei comune se distribuie separat de cel per-unitate. Datele nu sunt „migrate”
prin schema_sync. Interogările unice AvacontPush au șablonul drept țintă implicită;
nu trimitem prin acel flux scripturi comune/incompatibile cu șablonul.

Livrabile: ordine de aplicare, ținte explicite, condiții înainte/după și pași de revenire;
instrucțiuni pentru previzualizarea diff-ului de către utilizator, fără executare Codex.
Verificări locale: diff pe metadate fictive cu/fără ADE, fără DROP accidental; repetarea
generării nu introduce schimbări noi. MariaDB DDL poate face commit implicit: nu promitem
rollback tranzacțional al întregului script, se documentează restaurarea din backup.
Acceptare: niciun script ambiguu ca țintă; probele MariaDB rămân distincte de verificările locale.

## Slice ADE4

**Obiectiv:** utilizatori noi cu acces ADE limitat, autorizați efectiv în backend.
Dependențe: ADE1 și ADE3; numele rolurilor/drepturile finale sunt decizii deschise.

### SLICE-ADE4-01 — matricea drepturilor și conturile

Pași: matrice operație × unitate × drept; separarea identității web de contul de serviciu
MariaDB și, dacă este necesar, de conturile SQL dedicate. Loginul SQL singur nu limitează
un API care rulează cu alt cont. Reutilizăm autentificarea existentă și administrarea ei.
Operații de diferențiat: citire, cataloage, taxe/prezență, încasare, anulare, închidere,
redeschidere, rapoarte, import și configurarea numerotării. Niciun rol nou nu primește
toate aceste drepturi implicit și nu inventăm conturi/parole în documentație.
Livrabile: matrice de aprobat înaintea granturilor finale și plan de creare/revocare cont.
Verificări: toate rutele viitoare se mapează unei operații; unitățile vin din asocierea
validată pe server. Acceptare: niciun acces la alte module doar pentru că loginul reușește.

### SLICE-ADE4-02 — autorizare API și privilegii

Pași: guard comun pentru sesiune + modul + unitate + operație; backendul respinge
chei/IDZ/IDL/documente ale altei unități. Alegerea bazei nu vine din SQL sau text liber.
Aplicăm aceleași verificări la citire, salvare, PDF/export și endpointuri apelate direct.
Respectăm mecanismul existent de token/antete și verificăm protecția cererilor de scriere.
Livrabile: integrare API, maparea drepturilor și SQL pentru utilizator, limitat la ADE
și tabele comune explicit necesare; separăm citirea datelor unității de actualizarea
Unitati_Chitante. Schimbarea de context invalidează cererile/drafturile contextului vechi.
Verificări: lipsă sesiune, drept insuficient, identitate falsificată, unitate nepotrivită,
revocare/expirare. Acceptare: UI și API concordante, iar refuzul nu produce scrieri.

### SLICE-ADE4-03 — probe și jurnale

Pași: conturi fictive pentru matricea locală; cazuri permis/refuzat pentru fiecare operație;
capturarea corelării cerere–utilizator–unitate–rezultat în sistemele comune.
Livrabile: rezultate locale și instrucțiuni separate pentru proba granturilor pe server.
Verificări: apel direct la rutele altor module, schimbare unitate, expirare în editare,
anulare/refuz, fără token/parolă sau copii integrale ale datelor personale în jurnal.
Acceptare: toate refuzurile așteptate sunt dovedite; granturile reale se marchează
confirmate numai după rezultatele utilizatorului, fără a confunda mockul cu MariaDB.

## Slice ADE5

**Obiectiv:** import repetabil, reconciliat, fără pierderea istoricului care influențează soldul.
Dependențe: ADE3/ADE4 și datele reale din backendul Access, nu doar exportul de structură.

### SLICE-ADE5-01 — extractul și verificarea înainte de import

Pași: manifest sursă/unitate/data extragerii, tabele incluse și număr de rânduri; alegerea
formatului după instrumentele disponibile, păstrând NULL/șir vid, booleeni, diacritice,
date și precizia numerică. Se verifică linked tables: fișierul frontend poate să nu conțină datele.
Reutilizăm parserul/validările existente unde se potrivesc, fără rutele sau mapările altui modul.
Livrabile: validator cu raport fără scriere, mapare chei și erori de rezolvat înainte de import.
Verificări: orfani, dubluri, IDL, tipuri TIP, anulări, snapshoturi, CFGs/CH și documente numerotate.
Acceptare: fiecare tabel are decizie explicită; facturile/bonurile excluse nu duc la
ștergerea din Plati a unor contribuții necesare parității. Fără corecții CNPPROST/RunOnce automate.

### SLICE-ADE5-02 — import repetabil și jurnal

Pași: import pe o destinație identificată, în ordine de dependențe; identificator de lot
și mapare chei stabile; păstrarea IDL sau a unei mapări care conservă ordinea; asociere DC.
Configurația CH se importă separat, fără suprascrierea Unitati_Detalii existente.
Lotul repetat nu dublează datele. Un extract modificat este detectat și raportat,
nu aplicat ca upsert orb peste date deja editate în web. Scrierile se împart în tranzacții
explicit documentate și au reluare sigură în caz de întrerupere.
Livrabile: unealtă/pachet de import și jurnal prin mecanismele comune, raport per tabel.
Verificări: repetare, întrerupere, rând invalid, ID mapat, conflict cu date țintă;
acceptare locală pe fixture, apoi execuție reală numai de utilizator.

### SLICE-ADE5-03 — reconciliere

Pași: comparație sursă/țintă pe chei și câmpuri, număr de rânduri, relații, sume pe IDL/IDP,
anulări, istoricul numerelor, snapshoturi și configurații. Totalul general egal nu este
suficient dacă diferențele pe persoane se compensează între ele.
Livrabile: raport exact cu diferențe/lipsuri/excluderi justificate și setul de intrări
pentru ADE7-03. Fără transformări noi pentru a „face să iasă” totalurile.
Verificări: comparații pe fixture cunoscut, cel puțin o diferență introdusă intenționat
detectată; reconcilierea reală rămâne în așteptare până la extract și rezultate server.
Acceptare: zero diferențe de import neexplicate; egalitatea motorului web se dovedește în ADE7.

### SLICE-ADE5-04 — utilitar desktop ADE.Migrator

Implementat local: migrare directă MDB → MariaDB pe AD_03, cu reutilizarea
componentelor KBot.Migrator, educatori pe perioade și istoric dedus.
Build verificat; utilitarul și mapările nu sunt probate în execuție.
Detalii: [worklog ADE5-04](../docs/worklog/SLICE-ADE5-04-ade-migrator.md).

### SLICE-ADE5-05 — oprire, jurnal și verificare invalidată la schimbarea intrărilor

Implementat local: oprirea scrierii înainte de COMMIT, așteptarea eliberării
conexiunilor la închidere, progres pe tabel, SqlDumpWriter comun, hash MDB și
reverificarea destinației. Schimbarea intrărilor cere o nouă verificare.
Rezultatul COMMIT necunoscut este raportat fără promisiunea unei baze neschimbate.
Build verificat; scenariile UI/MariaDB rămân de probat de utilizator, fără teste
automate în această intervenție. Continuare: ADE5-06.
[Worklog](../docs/worklog/SLICE-ADE5-05-migrator-oprire-jurnal.md),
[utilizare](ADE.Migrator/README.md).

### SLICE-ADE5-06 — jurnal în formular și motivele blocării

Implementat local: jurnal de operații/excepții în formular, încărcarea logului comun,
starea permanentă a butonului și păstrarea erorii verificării. Preferințele locale
nu blochează verificarea dacă nu pot fi salvate. Logul existent indică un timeout;
conectarea și activarea după verificarea reală rămân de probat de utilizator.
[Worklog](../docs/worklog/SLICE-ADE5-06-migrator-log-formular.md).
Continuare: ADE5-07.

### SLICE-ADE5-07 — DC destinație editabil

Implementat local: DC propus din Access într-o casetă editabilă; modificarea cere
o nouă verificare. Verificarea, scrierea și chitanțele folosesc DC-ul ales, iar jurnalul
păstrează separat identitatea sursei. Build curat; funcționarea rămâne de probat.
[Worklog](../docs/worklog/SLICE-ADE5-07-migrator-dc-editabil.md).
Următoarea subfelie liberă: ADE5-10.

### SLICE-ADE5-08 — Alegerea grupei de plecați

Migratorul bifează automat grupele al căror nume conține «pleca», fără diferență
între litere mari/mici. Utilizatorul poate schimba bifa; alegerea se aplică înainte
de construirea istoricului. Mai multe grupe bifate blochează verificarea/scrierea.
Compilare reușită, fără probe funcționale.
[Worklog](../docs/worklog/SLICE-ADE5-08-grupa-plecati.md).

### SLICE-ADE5-09 — CNP-ul copilului folosit și la părinte

Opțiune implicit nebifată «CNP Copil = CNP Părinte»: la transfer copiază
Platitori.CNP din Access în CNP_Platitor pentru plătitorii asociați prin IDP.
Valorile goale nu suprascriu. Schimbarea opțiunii invalidează verificarea.
Compilare reușită; transferul rămâne de probat de utilizator.
[Worklog](../docs/worklog/SLICE-ADE5-09-cnp-parinte.md).

### SLICE-ADE5-10 — log nou la lansare

Logul comun anterior este arhivat, iar rularea începe cu un fișier nou și caseta
goală. Încărcarea logului în formular este numai manuală. Erorile de pornire
rămân raportate. Build curat; arhivarea/interfața rămân de probat.
[Worklog](../docs/worklog/SLICE-ADE5-10-log-nou-pornire.md).
Următoarea subfelie liberă: ADE5-11.

## Slice ADE6

**Obiectiv:** liste de selecție Grupe/Copii/Plătitori cu editori modali după machete;
Taxe, Prezență și perioadele educatorilor folosesc editarea în DGV comun.
Regulile PC/mobil, dimensiunile, filtrele și I sunt în [UTILIZARE_WEB](UTILIZARE_WEB.md).
Dependențe: ADE2–ADE5; API autorizat și date coerente.

### SLICE-ADE6-01 — cataloagele

Pași: arbore/grile pentru Grupe, Platitori (copiii), Platitori_sub (persoane asociate)
(Delegati a fost scos din model); separarea IDP/IDS. Transpunem regulile din Platitori2016 și subformulare,
inclusiv inițializarea persoanei/delegatului unde este cerută de flux, fără tabele _L.
Livrabile: endpointuri și ecrane integrate în portal; câmpuri editabile declarate explicit,
salvare atomică pentru modificările legate și mesaje de validare în română.
Verificări: adăugare, editare, persoană activă, legături, conflict, refuz fără drepturi;
nu ștergem în cascadă istoricul financiar printr-o operație generică de catalog.
Acceptare: editare prin formulare și date păstrate după reîncărcare; salvarea unui
plătitor nou îl activează și dezactivează atomic ceilalți ai aceluiași copil.
I este numai afișare în liste, iar filtrele rămân numai la nume/denumire.
M06 este decis; lunile închise și jurnalul se păstrează la mutarea individuală.

### SLICE-ADE6-02 — taxe și prezență lunară

Pași: selecție an/lună/grupă, set activ de taxe și preluare copii; zile editabile în grilă;
recalcul conform evenimentelor Access și SitLunara, distinct de motorul situației 2021.
ValoareContract = ZilePrezenta × TaxaZilnica; ValoareTotala urmează exact traseul documentat,
nu devine o formulă nouă unificată. Istoricul IDV nu este înlocuit cu taxa curentă la import.
Taxele au DeLa/PanaLa lună/an (AD_04), Activ checkbox și sfârșit automat pentru
taxa anterior activă la adăugare. PanaLa nu se introduce manual. Numai taxa nouă
necesită DeLa; taxele migrate fără perioade nu sunt blocante.
Livrabile: API preluare/salvare/prezență și UI integrat. Validăm pe server luna deschisă
la momentul scrierii, chiar dacă pagina a fost deschisă înainte de închiderea lunii.
Verificări: zero zile, schimbare taxă, rând existent, copil plecat/transferat, două editări,
lună închisă, diferența NULL/zero și calendarul fără reducerile excluse.
Acceptare: calcule conforme și salvare/refuz vizibile; fără pagina de configurare unitate.

### SLICE-ADE6-03 — transferuri și plecări

Pași: tratament separat pentru transfer individual, transfer lot și schimbare grupă
din catalog. Referință ComboGrupa/Platitori2016; M06 este decis în ADE0-07; implementarea este parțială (ADE6-06).
MutaCopil a fost scos din model (08.10.2026); istoricul nu se reproduce, ci se înlocuiește conform M06: jurnal automat per copil (AD_Platitori_Istoric), istoric al educatorilor per grupă (AD_Grupe_Educator: DeLa/PanaLa; nivelul mica/mijlocie/mare NU se păstrează, decis 08.10.2026), reconstruit la migrare din AD_SS_Buget.Educator/Grupa/IDG cu listă de verificat de operator înainte de scriere, grupă închisă dar vizibilă în perioadele vechi, copil în prezența grupei din momentul închiderii lunii, copii plecați și compensare (AD_Compensare). Păstrăm restricția temporală aplicabilă din Access.
Livrabile: comenzi explicite, istoric și preluarea corectă în luna următoare; nicio
renumerotare izolată a IDZ și nicio mutare implicită de plăți la alt copil.
Verificări: transfer în aceeași lună, prezență în două grupe, plecare cu sold, documente
deja emise, grupă veche în snapshot, acces pe altă unitate.
Acceptare: relațiile/soldurile rămân coerente; semantica aleasă este consemnată cu sursa/decizia.

## Slice ADE7

**Obiectiv:** fidelitate 1 la 1 cu mdl_Situatie și gestionarea corectă a situațiilor salvate.
Dependențe: ADE3–ADE6; [contractul de calcule](CONTRACT_CALCULE.md) prevalează asupra rezumatului.

### SLICE-ADE7-01 — motorul de calcul

Pași: serviciu cu unitate/lună/grupă explicite; faze qPrezenta → qSolduri → Update_Solduri
→ Update_Situatie → qExplicatie → Update_detalii → Update_Compensare → Salvare_Lunara
numai la salvare. Reproducem joinurile, populația, filtrele de anulare și conversiile
intermediare; nu citim COMP ca sursă nouă de sold și nu amestecăm procedura veche cu 2021.
Livrabile: motor și rezultate intermediare comparabile, cu lucru izolat per cerere.
Parametrii grp/tIDL ignorați în VBA nu se transformă automat în altă selecție de lună.
Verificări: cazurile din contract, tipurile Long/Double, NULL, plăți și retururi multiple,
ordine evaluare SFD/Compensare; egalitatea numerică nu se validează doar prin totaluri.
Acceptare: fiecare fază are corespondent explicabil; necunoscutele ACE au probe Access.

### SLICE-ADE7-02 — închidere, redeschidere și luna următoare

Pași: operație atomică pentru starea lunii și snapshot; blocare concurentă astfel încât
închiderea să nu ruleze de două ori sau simultan cu o salvare incompatibilă de prezență.
Snapshotul păstrează câmpurile istorice; citirea lunii închise nu îl regenerează după
taxele/numele curente. Luna următoare preia populația și taxele conform traseului Access.
M05 a fost decis la 08.10.2026: numai ultima lună închisă poate fi redeschisă, iar luna
deschisă următoare se elimină. Mișcările ei rămân orfane și primesc explicit luna/anul de
proveniență, separat de data documentului, pentru reatașare la recrearea perioadei. O setare
per bază poate bloca operația; verificarea mișcărilor acoperă atât luna redeschisă, cât și
luna eliminată. Modelul, setarea și implementarea acestei decizii sunt încă nescrise.
Livrabile: serviciu/API și comenzi UI autorizate, cu efecte descrise înaintea confirmării.
Verificări: închidere repetată, eșec între faze, două cereri, redeschidere cu date ulterioare,
snapshot complet și fără duplicate. Încasările închise rămân permise conform ADE8.
Acceptare: fără închidere parțială sau pierdere de date prin redeschidere neclarificată.

### SLICE-ADE7-03 — paritatea Access/web

Pași: set de referință înghețat, intrări identice și rezultate după fiecare fază în Access
și web; comparație pe chei/câmpuri, nu pe ordinea incidentală a rândurilor.
Acoperire minimă: toate cele 14 cazuri din contract, inclusiv rotunjiri .49/.50/.51,
anulări discordante, luni consecutive, transferuri și încasări după închidere.
Livrabile: raport de diferențe cu primul pas divergent, fixture reutilizabil și concluzie
explicită pe caz. Ordinea nedeterminată a textelor Detalii se tratează separat de sume.
Verificări: comparatorul detectează diferențe reale; nu se aleg toleranțe care ascund
o unitate monetară pierdută sau o conversie diferită. Acceptare: zero diferențe numerice
neexplicate. Fără rezultatele Access, starea rămâne nevalidată, chiar dacă testele locale trec.

## Slice ADE8

**Obiectiv:** încasare/document/snapshot/numerotare coerente, fără facturi sau bonuri fiscale.
Dependențe: ADE2–ADE7; rezolvarea M01–M04/M07 pentru operațiile afectate.

### SLICE-ADE8-01 — încasări, alte documente, restituiri și anulări

Pași: adaptarea Plati_chitante, Plati_Alte, Plati_Retur; selectarea persoanei IDS și a IDZ;
validare sumă/document/context pe server; anulare prin flag, nu ștergere.
Păstrăm încasările după închidere: adunăm în Plata/Plati/Retur din SS_Buget și aplicăm
formula snapshotului, diferită de recalculul general. Prezența rămâne blocată.
Inserarea/anularea respectă IDL selectat + 1 < MAX(Prezenta.IDL) ca regulă de refuz,
cu verificarea semanticii IDL la migrare. Nu o înlocuim cu data calculatorului.
Livrabile: API și grile editabile, tranzacție document/plată/snapshot și audit comun.
M02 a fost decis la 08.10.2026: anularea și orice altă modificare a documentelor rescriu
situația salvată a copilului din luna închisă. M04 păstrează condiția VBA: data este refuzată
numai dacă diferă și luna, și anul. M03, salvarea motivului anulării unui retur, rămâne deschis.
Verificări: toate tipurile, anulat deja, lună prea veche, snapshot absent, eșec parțial.
Acceptare: solduri conforme regulii alese și nicio dublare a efectelor la repetare.

### SLICE-ADE8-02 — chitanța și numerotarea

Pași: folosirea Unitati_Chitante pe DC și datelor unității comune; alocare număr în
tranzacția emiterii, fără MAX+1 concurent neprotejat. Valoarea reală vine din configurație;
șablonul folosește [LA] conform fMail2021, nu LunaD.LA numeric.
Reproducem câmpurile raportului Chitanta și sumele în litere; păstrăm documentul emis
reproductibil când datele comune se schimbă, prin date de emitere/strategie stabilită în ADE3.
Livrabile: emitere și PDF/print în vizualizatorul existent, stare document anulat,
reimprimare fără emiterea unui nou număr. Fără rută de editare a datelor unității.
Verificări: configurație lipsă, serie/număr real, [LA], diacritice, anulare, reprint,
două cereri simultane și aspectul raportului. Printul fizic este probat de utilizator.
Acceptare: un document emis = un număr unic în domeniul definit; nicio valoare exemplu hardcodată.

### SLICE-ADE8-03 — tranzacții, reîncercări și concurență

Pași: identificator idempotent al operației, păstrat la retry; aceeași cheie și același
conținut întorc același rezultat, aceeași cheie cu alt conținut este refuzată.
Scopul cheii include unitatea/operația; se documentează ordinea blocărilor pentru
configurație număr, plată și snapshot, inclusiv concurența cu închiderea/redeschiderea.
Livrabile: teste de eșec/concurență, reguli de recuperare după timeout și listă de probe MariaDB.
Verificări: dublu click, timeout după commit, răspuns pierdut, eroare înainte de commit,
deadlock, două numere simultane, anulare repetată, salvare în timpul închiderii.
Acceptare: un singur efect business; mockurile nu sunt dovada blocărilor MariaDB.
Comportamentul real al tranzacțiilor/granturilor este confirmat de utilizator pe server.

## Slice ADE9

**Obiectiv:** rapoarte conforme, flux complet verificabil și pachet de predare explicit.
Dependențe: ADE1–ADE8, deciziile rapoartelor legacy și rezultatele de referință.

### SLICE-ADE9-01 — rapoartele

Pași: SituațieDebitori buget și variantele de total/grupă, Chitanță, Dispoziție de plată,
Fișă cont, Registru casă, Raport bancă, Documente anulate; surse din inventar/mapare.
Separăm calculul lunii deschise de snapshotul lunii închise și păstrăm filtrele fiecărui
raport, inclusiv flagul de anulare al documentului vs al plății unde Access diferă.
Familia FisaDebitor rămâne condiționată de eliminarea dependențelor excluse fără schimbarea
neaprobată a soldurilor. DocumenteAnulate necesită proba aliasului M08.
Livrabile: endpointuri autorizate, ecrane/PDF și matrice raport → sursă → filtre → totaluri.
Verificări: grupă/toate, interval, anulări, SI/transfer, snapshot istoric, diacritice,
paginare și totaluri; exporturile incluse folosesc aceleași filtre/drepturi.
Acceptare: valori identice sursei acceptate și document lizibil; nu se pretind testate tipăriri neefectuate.

### SLICE-ADE9-02 — proba completă și ajutorul

Pași: la localhost:5050 se parcurge grupă → copil/persoană → prezență → calcul →
încasare/chitanță → restituire/anulare → raport → închidere → încasare în lună închisă
→ luna următoare; se includ schimbarea unității și refuzurile de drepturi.
Regresii: pagini existente doar-citire, teme/layouturi, sesiuni, log/timing, EventBus,
ListenerTracker/registry, PDF și curățare la navigări repetate.
Livrabile: raport cu rezultate efective, ajutor despre editare/erori/închidere/încasări,
capturi utile și limitări; fiecare secțiune de ajutor are marcajul feliei care a schimbat-o.
Verificări: flux pozitiv și erori, context schimbat în timpul salvării, date persistate
după reîncărcare. Acceptare: toate cazurile locale definite trecute sau explicit blocate;
niciun mock nu este prezentat drept test de integrare pe server.

### SLICE-ADE9-03 — pachetul pentru utilizator și consemnarea probelor

Pași de predare: listă exactă fișiere/revizii; ținte SQL și ordinea (schema comună,
schema ADE, drepturi, import/reconciliere, activare aplicație după compatibilități);
condiții înainte/după și revenire. Ordinea finală se adaptează dependențelor constatate
în scripturile efective, nu se execută această listă generică.
AvacontPush este folosit de utilizator pentru publicarea selecției și restart dacă este
necesar. Se verifică LocalRoot/extensii/.pushignore; se exclud datele Access, fixture-urile,
launcherul local și secretele. Nu se publică automat toate modificările din KBOT.
Livrabile: instrucțiuni executabile ale versiunii livrate, raport local și fișă server
cu cine/când/ce rezultat: pornire, login/drepturi, schema/import, calcule, numerotare
concurentă, sesiuni/log, PDF/print și regresii pe portalul existent.
Acceptare locală: pachet complet, verificat, etichetat PREDAT PENTRU PUSH când chiar este predat.
Acceptare server: numai după rezultatele utilizatorului. Lipsa confirmării nu devine
CONFIRMAT PE SERVER și nu este acoperită de testele locale. La finalul predării se oprește
lucrul; nu se configurează monitorizare sau publicare automată.

## Închiderea unei subfelii

În worklog se consemnează rezultatul observat, nu se copiază lista de teste la timpul trecut.
Orice diferență de business descoperită intră în registrul deciziilor înaintea schimbării
regulii. Actualizare 08.10.2026: ADE0-04 consemnează adoptarea
[regulilor proiectului](REGULI_PROIECT.md); ADE0-05 consemnează sursa reală MDB și
interdicția opririi proceselor MSACCESS. Următoarele numere libere sunt ADE0-07,
ADE1-04…ADE9-04 și felia ADE10. Cele 30 de subfelii detaliate mai sus reprezintă planul ADE0-03.
Documentarea tuturor feliilor nu înseamnă că acestea sunt implementate sau validate.
