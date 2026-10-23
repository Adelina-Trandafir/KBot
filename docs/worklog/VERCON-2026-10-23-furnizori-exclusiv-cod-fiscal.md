# VERCON online — furnizori exclusiv prin cod fiscal


Implementare limitată la VERCON online. Identitatea este exclusiv codul
fiscal normalizat (alfanumeric uppercase, prefix RO eliminat înainte de
cifră; păstrează zerourile inițiale și codurile externe cu litere).
CodPartener din VC_NIR și VC_FACTURI conține acest CF, nu codul intern al
unei unități. Câmpurile IdPartener/PARTENER_ID existente rămân neatinse;
nu mai sunt folosite pentru legături/citiri/salvări online.

Un singur modul utils/vercon_suppliers.py gestionează normalizarea Python
și SQL, selecția unei înregistrări de afișare pentru fiecare CF normalizat,
căutarea în nomenclator și ordonarea românească. Nu alege după ID intern.
Dublurile între unități sunt o singură identitate; nume identice cu CF-uri
diferite rămân separate. Selectoarele și API-urile transportă CF ca text.

Acoperire: selectare furnizor, citire/salvare NIR, storno, factura comună,
repartizări și reunire fragmente, arbore lazy și arbore FCS, fișă furnizor,
proveniența mișcărilor/stocurilor și filtrele/ramurile/selețiile rapoartelor,
sursele SQL implicite, preluarea NIR din eFactura și asocierea pe poziții.
Lista asocierii este filtrată server-side prin CF + data facturii citite
din NIR-ul salvat; Active/Inactive/Toate păstrează filtrul. Validitatea
alocărilor folosește aceeași cheie fiscală, fără fallback prin ID, nume sau
CodFiscalSursa. Suprascrierea CPV la salvarea asocierilor rămâne activă.

Sursele personalizate de rapoarte cu aliasuri explicite sunt adaptate în
memorie (alias NIR.IdPartener -> CodPartener; alias Parteneri.IdPartener
-> CodFiscal), fără rescrierea fișierelor utilizatorului. Relația Parteneri
este deduplicată fiscal înainte de join, ca să nu multiplice valorile.
Definițiile cu filtre literale pe vechiul ID sau referințe nemapabile sunt
refuzate la executare și rămân accesibile la editare; un ID numeric nu
este reinterpretat drept CF. Nu s-a executat nicio interogare de raport.

SQL PREGĂTIT, NEEXECUTAT:
- sql/VC_20261023_FURNIZORI_CF_STRUCTURA.sql: coloana CodPartener a NIR
  (dacă lipsește) și aceeași coloană în VC_FACTURI, cu indexuri; fără ștergeri,
  fără schimbări pe coloanele vechi. Manual pe AVACONT_SURSA, apoi sync.
- sql/VC_20261023_FURNIZORI_CF_DATE.sql: migrare manuală în fiecare DC.
  Completează/normalizează CF păstrând întâi valorile fiscale deja salvate;
  numai acest fișier de tranziție citește vechile ID-uri pentru a recupera
  CF din Parteneri; folosește CF sursă pentru NIR și NIR-uri legate unanim
  pentru facturi fără partener rezolvat. Nu deduce furnizori după nume.
  Listează CF-uri lipsă și neconcordanțe NIR/factură pentru rezolvare înainte
  de publicare. Nu modifică valori, cantități, revizii sau ID-uri vechi.

Ordine publicare: structura manuală + sync, scriptul de date în fiecare DC
cu scrierile oprite, rezolvarea rezultatelor neclare, apoi toate modulele
Python/JS/CSS/help și SQL-urile SELECT de rapoarte, repornire serviciu și
reîncărcare completă browser. Guard-ul online indică fișierele necesare
când coloanele fiscale nu sunt instalate. Nu creează automat structura.

Fișiere noi: PYTHON/utils/vercon_suppliers.py;
PYTHON/routes/vercon/supplier_identity.py; cele două scripturi SQL de mai sus.
Fișiere modificate: PYTHON/utils/vercon_efactura.py;
PYTHON/routes/vercon/{receipt_choices,receipts,documents,invoices,
receipt_distribution,receipt_routes,receipt_tree,receipt_context,doc_routes,
stock_source,stock,stock_routes,report_routes,report_selection,
report_incoming_selection,report_query,report_store,efactura_association,
efactura_queue}.py;
PYTHON/reports/vercon/queries/{receipts,ledger}.sql;
PYTHON/static/js/vercon/{docs,receipt-context,receipt-tree,
document-tree-order,storno-editor,efactura-intake}.js;
PYTHON/static/css/vercon-receipt-destinations.css;
PYTHON/static/help/vercon.json; implementare_vercon_v2.md;
docs/worklog/VERCON-2026-10-23-furnizori-exclusiv-cod-fiscal.md.

Limita de scop: DDF, ORD, extrase, nomenclatorul global Parteneri,
migratoarele Access și rutele K-BOT din routes/efactura nu au fost modificate.
Proiecția comună de stare eFactura citește CF din NIR, în acord cu online.
După importuri cu un migrator vechi, repetați migrarea manuală a datelor
înainte de folosirea documentelor noi în VERCON online.

Stare: SCRIS LOCAL, NEPUBLICAT. Analiză de design prin citirea surselor;
fără teste, build, verificări automate/de sintaxă, browser sau acces la
MariaDB, conform instrucțiunilor. Nu există confirmare de funcționare în
execuție și nu s-au aplicat scripturile SQL.


### 2026-10-23 — VERCON: migrare fiscală din KBotAdmin / Interogări unice

Pregătită sql/VC_20261023_FURNIZORI_CF_KBOTADMIN.sql pentru mecanismul
existent routes/one_time/runner.py: fără START TRANSACTION/COMMIT și fără
SELECT-uri finale ale căror rezultate runnerul le-ar consuma fără afișare.
KBotAdmin păstrează tranzacția per bază și scrie registrul numai după
reușita întregii migrări. Nu a fost schimbat runnerul sau KBotAdmin.

Datele migrează ca în scriptul manual. Înainte de final, o gardă în tabelă
temporară oprește baza cu eroare duplicate-key explicită dacă există
NIR_FARA_CF, FACTURA_FARA_CF sau CF_FACTURA_DIFERIT_DE_NIR. Cheia duplicată
este intenționată și indică motivul; nu depinde de CHECK sau strict mode.
Runnerul face rollback la modificările de date și nu marchează baza ca
rulată. Pe eroare de AVACONT_SURSA, runnerul nu continuă la DC-uri.
După corectarea datelor, aceeași interogare poate fi reluată; bazele deja
marcate sunt sărite. Scriptul manual și cele trei SELECT-uri diagnostice
rămân disponibile separat în VC_20261023_FURNIZORI_CF_DATE.sql.

Antetul scriptului de structură a fost adaptat pentru rulare și ca
interogare unică, cu numele VC_20261023_FURNIZORI_CF_STRUCTURA. Poate rula
pe AVACONT_SURSA + DC-urile bifate, apoi migrarea de date pe aceleași baze;
alternativa structură manual pe sursă + sincronizare rămâne valabilă.
DDL este pregătit în fișier și va fi executat exclusiv de utilizator.

Pași KBotAdmin: Interogări unice -> Din fișier pentru structură, nume
VC_20261023_FURNIZORI_CF_STRUCTURA, baze bifate, Vezi (nu execută), Execută.
Apoi Din fișier pentru varianta KBOTADMIN; SCHIMBAȚI explicit numele în
VC_20261023_FURNIZORI_CF_KBOTADMIN (UI păstrează numele anterior dacă nu e
gol), aceleași baze, Vezi (nu execută), Execută. Publicați online după
reușita migrării. Interogarea include întotdeauna AVACONT_SURSA.

Fișiere: sql/VC_20261023_FURNIZORI_CF_KBOTADMIN.sql (nou);
sql/VC_20261023_FURNIZORI_CF_STRUCTURA.sql (antet);
implementare_vercon_v2.md;
docs/worklog/VERCON-2026-10-23-furnizori-exclusiv-cod-fiscal.md.
Stare: SCRIS LOCAL, NEEXECUTAT. Doar analiză de design a runnerului și
interfeței; fără teste, build, verificări automate, SSH, KBotAdmin deschis
sau acces MariaDB. Nicio schimbare de structură/date executată.


### 2026-10-23 — diagnostic NIR_FARA_CF după rularea utilizatorului

Log furnizat de utilizator: Portare_parteneri_codfiscal, hash prefix
24beed22b0f0c6f2, 15 instrucțiuni. AVACONT_SURSA și 15 DC-uri OK; șapte DC-uri
s-au oprit în garda NIR_FARA_CF: 000_DEMO,001_GR23,005_CEVM,012_GR30,
018_GRRS,030_SCTC,050_GRSA. Conform runnerului, datele din bazele eșuate
sunt rollback și bazele nu sunt marcate. Cele trei rânduri raportate de
mai multe baze OK sunt cele trei inserări tehnice în gardă, nu trei NIR-uri.

Pregătit numai SQL de diagnostic:
sql/VC_20261023_FURNIZORI_CF_DIAGNOSTIC_KBOTADMIN.sql,
nume Portare_parteneri_codfiscal_diagnostic. Nu modifică tabelele de date;
folosește exclusiv tabele temporare și transmite un sumar prin eroarea
intenționată DIAG_CF (runnerul existent nu afișează rezultate SELECT).
Sumarul include numărul NIR-urilor fără CF după regula exactă a migrării,
numărul cazurilor cu normalizare mascată de prioritate, indicii textuale
STOC/SOLD INITIAL (doar diagnostic, fără reclasificare), până la trei
exemple IdIntrare/Numar și context, existența rândului partener și a CF sursă.
Mesajele duplicate-key pot fi trunchiate de server; prefixul păstrează
numărătorile, apoi exemplele. Baza cu rezultate diagnostice rămâne nemarcată
și se poate reinteroga; fără rezultate se marchează normal în registru.

Nu a fost modificat textul migrării originale deja executate pe unele baze;
nu se va relansa alt text sub numele Portare_parteneri_codfiscal. Nu s-a
relaxat garda, nu s-a inventat CF și nu s-a reclasificat vreun document.
Fără acces MariaDB, SQL executat, teste sau verificări automate. Cauza
concretă a documentelor rămâne de stabilit din diagnosticul utilizatorului.


### 2026-10-23 — migrare VERCON CF v2: documentele istorice fără CF nu blochează baza

Diagnostic furnizat de utilizator (Portare_parteneri_codfiscal_test_1,
hash prefix 14ccf76d81d6702c): N=33/1/147/9/7/2/1 pentru
000_DEMO/001_GR23/005_CEVM/012_GR30/018_GRRS/030_SCTC/050_GRSA,
în total 200 de NIR-uri fără CF recuperabil prin sursele migrării.
FMT=0 în toate bazele: nu sunt coduri valide mascate de ordinea normalizării.
STOC=13/0/13/8/1/0/1, în total 36: numai indicii textuale, nu documente
confirmate ca stoc inițial. Exemplele vizibile au P0,SRC0; nu se deduce
că toate cele 200 au exact aceeași stare a vechii legături de partener.
Diagnosticarea a creat doar tabele temporare, fără schimbarea datelor NIR.
Numărul de rânduri OK include operațiile tehnice, nu documente modificate.

Garda inițială era prea strictă pentru istoricul cu informații lipsă.
Pregătit sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V2.sql,
nume NOU Portare_parteneri_codfiscal_v2. Poate rula pe cele șapte baze
rămase (sau pe toate țintele originale); sursa este inclusă automat.
Vechile fișiere/nume/hash-uri înregistrate rămân neschimbate.

V2 normalizează fiecare candidat fiscal înainte de alegerea priorității,
completează CF-urile recuperabile, păstrează necunoscutele NULL și nu
reclasifică NIR-uri pe baza textului STOC/SOLD INITIAL. Nu creează
furnizori și nu deduce CF după nume. Nu există fallback de identitate
în online; vechile chei sunt citite doar în fișierul de tranziție.
Valorile/cantitățile/reviziile și coloanele istorice nu sunt modificate.
DONE în registru înseamnă conversie aplicată, nu toate CF-urile rezolvate.
Se păstrează rollback-ul pe contradicții între CF factura/NIR cunoscute
și pe facturi care au NIR-uri legate cu mai multe CF-uri cunoscute.
Un NULL și un CF cunoscut nu sunt tratate drept două identități contradictorii.

În arborii NIR/FCS, selectorul de sursă storno și ramurile de rapoarte,
grupul fără CF este etichetat explicit Fără cod fiscal, nu cu numele
istoric arbitrar al primului document. Cantitățile și numele istorice pe
document rămân vizibile; grupul nu reprezintă un furnizor dedus din nume.
Asocierea eFactura rămâne interzisă fără CF. Antetul comun de deployment
indică V2 drept migrare recomandată; help descrie documentele nerezolvate.

Fișiere: sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V2.sql (nou);
PYTHON/routes/vercon/{receipt_tree,report_incoming_selection,
report_selection,supplier_identity}.py;
PYTHON/static/js/vercon/{document-tree-order,storno-editor}.js;
PYTHON/static/help/vercon.json; implementare_vercon_v2.md;
docs/worklog/VERCON-2026-10-23-furnizori-exclusiv-cod-fiscal.md.

Stare: SCRIS LOCAL, NEPUBLICAT; SQL pregătit pentru utilizator, NEEXECUTAT
de asistent. Fără teste, build, verificări automate/de sintaxă, browser,
SSH sau acces MariaDB. NIR-urile fără CF au nevoie în continuare de date
fiscale explicite pentru a primi o identitate de furnizor; migrarea nu
pretinde rezolvarea acestor informații absente.


### 2026-10-23 — migrare CF v3: ignoră partenerii Ascuns=1

Clarificare explicită a utilizatorului: partenerii cu Ascuns=1 NU au NIR-uri
sau alte documente. Nu este necesară și nu s-a implementat vreo logică de
propagare/excludere pentru documente ale unor parteneri ascunși.

Pregătit sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V3.sql,
nume NOU Portare_parteneri_codfiscal_v3. Păstrează V2 și adaugă doar
AND COALESCE(P.Ascuns,0)<>1 la cele două căutări în Parteneri (recuperarea
CF pentru NIR și factura comună). NULL este tratat ca 0. Tabela Parteneri
nu este actualizată, ca și în variantele precedente. Documentele istorice
fără CF nu blochează baza; contradicțiile între CF-uri cunoscute rămân
blocante. Nu se inventează CF și nu se reclasifică documente.

Fișierele și interogările originale/V2 rămân neschimbate, deoarece pot
fi deja înregistrate în Interogari_Unice. V3 folosește nume și hash noi;
nu inversează schimbări ale unei rulari deja încheiate.
Fișiere: sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V3.sql (nou);
PYTHON/routes/vercon/supplier_identity.py (referință la V3);
implementare_vercon_v2.md;
docs/worklog/VERCON-2026-10-23-furnizori-exclusiv-cod-fiscal.md.
Stare: SQL PREGĂTIT, NEEXECUTAT de asistent. Fără teste, verificări
automate/de sintaxă sau acces MariaDB. Nicio modificare de date/structură
executată. De rulat de utilizator prin Interogări unice, cu numele V3.

### 2026-10-23 — VRC.Migrator: aceeași identitate fiscală ca VERCON online

Cerere: alinierea strict a VRC.Migrator la fluxul fiscal VERCON, cu protejarea
celorlalte module și a partenerilor Ascuns=1 (care nu au documente).

Eliminat dicționarul CF → IdPartener și încărcarea ID-urilor din Parteneri.
Importul NIR scrie direct CF normalizat în CodPartener, păstrând separat
CodFiscalSursa; facturile provizorii copiază aceeași identitate din NIR în
VC_FACTURI.CodPartener, fără PARTENER_ID. Lipsa rândului din Parteneri nu
anulează CF-ul sursă. Lipsa CF/IDF nerecunoscut produce X04, valoare NULL;
CF mai lung de 50 produce X07, fără trunchierea identității. Stocul inițial
păstrează regula existentă și identitatea NULL. Normalizarea elimină RO doar
înainte de 0–9, ca implementarea online, păstrând literele și zerourile inițiale.

Actualizate mesajele preflight/raport/ANAF. Setul codurilor existente include
și partenerii ascunși EXCLUSIV ca gardă împotriva propunerii/creării unui
duplicat: nu se modifică/reactivează niciun partener existent. AddInTransaction
normalizează și codul pregătit înaintea verificării existenței și a inserării.
Numerele locale deja ocupate (inclusiv ascunse) se rezervă ca înainte pentru
a evita coliziuni; codul local Parteneri.CodPartener nu leagă documentele.

Controlul de schemă folosit de Testează și înaintea tranzacției cere CodPartener
textual, scriibil, minimum 50, NULL permis, pe VC_NIR și VC_FACTURI; ID-urile
istorice, dacă există, trebuie să permită NULL. DDL-ul existent este doar indicat
utilizatorului, nu executat. Nu s-a introdus rularea automată a V3, reimport,
actualizare a documentelor istorice sau creare automată de schemă.
Versiune fișier VRC.Migrator: 0.3.12.0; RuleVersion:
VC05-SUPPLIERS-CF-2026-10-23. CPV din Access păstrează maparea existentă;
asocierea eFactura rămâne responsabilitatea aplicației online.

Fișiere: VERCON/VRC.Migrator/VrcRunner.vb, VrcRunner.Invoices.vb,
VrcWriter.vb, VrcWriter.Schema.vb, VrcConvert.vb, VrcPartnerStore.vb,
VrcMigratorForm.vb, VrcPartnersForm.vb, VrcRun.vb, VrcImportMap.vb,
VrcPlan.vb, VRC.Migrator.vbproj și README.md; jurnalul de față și
implementare_vercon_v2.md. Nicio modificare în src/KBot.Migrator,
ADE.Migrator, migratorul contabil, DDF/ORD/extrase sau maparea produselor.

Stare: SCRIS LOCAL, NEPUBLICAT. Citire și analiză de proiectare în surse;
fără teste, build, verificări automate/de sintaxă, rulare de executabil,
importuri sau acces MariaDB/DDL, conform AGENTS.md. Executabilul existent
în _VrcMigrator trebuie recompilat/publicat înainte de folosirea regulii noi.

