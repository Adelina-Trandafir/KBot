# SLICE-VC06-18 — VERCON online: furnizori exclusiv prin cod fiscal

Data: 2026-10-23. Din acest ansamblu fac parte și SLICE-VC03-07…10 și SLICE-VC05-11.

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
docs/worklog/SLICE-VC06-18-furnizori-exclusiv-cod-fiscal.md.

Limita de scop: DDF, ORD, extrase, nomenclatorul global Parteneri,
migratoarele Access și rutele K-BOT din routes/efactura nu au fost modificate.
Proiecția comună de stare eFactura citește CF din NIR, în acord cu online.
După importuri cu un migrator vechi, repetați migrarea manuală a datelor
înainte de folosirea documentelor noi în VERCON online.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
