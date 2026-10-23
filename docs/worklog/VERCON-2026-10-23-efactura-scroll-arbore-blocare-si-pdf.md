# eFactura: overflow, NIR asociat protejat și afișare PDF — 23.10.2026

Stare: SCRIS LOCAL, NEPUBLICAT. Numai analiză de design pe surse. Fără
teste, verificări automate/de sintaxă, browser, PDF generat sau MariaDB.

## Dimensiunile dialogului

Antetele DGV au padding orizontal de 22 px pe fiecare parte. Coloanele
de acțiuni _move/_action au 40 px și nu aveau headerAction; paddingul le
făcea mai late decât cei 40 px calculați. Astfel apărea scroll orizontal
în DGV-ul NIR gol și în grila asocierilor. Paddingul și gap-ul acestor
antete sunt acum zero, numai în dialogul de asociere. Nu s-a ascuns
overflow-ul grilelor și nu s-a schimbat componenta DGV comună.

ComboBox-ul era limitat la max-width:300px din stylesheetul comun.
Selectorul facturii are acum o bază flexibilă de 460 px și max-width:none,
limitat la acest dialog; se poate restrânge pe ecrane înguste.

## NIR-urile asociate

efactura_receipts.py calculează metadatele pentru lotul de NIR-uri al unei
ramuri: asocieri integrale din fluxul vechi și alocări parțiale valabile
din fluxul nou. Datele sunt în contextul subunitate/gestiune/exercițiu.
Asocierile expirate sau eliminate nu blochează NIR-ul. Fără tabelele
eFactura existente, citirea documentelor continuă fără aceste metadate.

Lista lazy, lista generală și documentul deschis primesc has_efactura,
lista facturilor asociate și edit_locked/edit_message. Nodurile sunt
bold italic, inclusiv valoarea. Rendererul TreeView acceptă opțional
italic; stilul este limitat la NIR. După salvarea/eliminarea asocierilor
se reîncarcă documentul și ramurile deschise, păstrând gruparea și selecția.

Modifică este dezactivat atât în popup, cât și în bara documentului.
_guard_dependents refuză și pe server editarea NIR-ului asociat cu
EF_NIR_LOCKED, înainte de calcul sau scriere; acoperă și validarea ciornei.
Asocierea poate fi corectată/eliminată prin dialogul existent. Regula
anulării documentelor nu este schimbată.

## Afișarea facturii

Meniul unui NIR asociat oferă «Afișează eFactura». Se deschide factura
originală în vizualizatorul PDF comun, reutilizând primite.pdf_anaf și XML-ul
salvat. PDF-ul este randat de serviciul ANAF existent. Dacă sunt mai multe
facturi, utilizatorul alege din lista ordonată după furnizor/data/număr.

GET /api/vercon/doc/nir/<nir>/efactura/<invoice>/pdf cere sesiunea și
drepturile VERCON existente, validează contextul și verifică asocierea
activă a facturii cu acel NIR. Nu permite accesul la altă factură prin
înlocuirea ID-ului în URL. Erorile XML/ANAF sunt transmise prin protocolul
VERCON; nu s-a făcut nicio cerere ANAF în această lucrare.

Fișiere noi: PYTHON/routes/vercon/efactura_receipts.py;
PYTHON/static/js/vercon/efactura-view.js.
Integrare: PYTHON/utils/vercon_efactura.py;
PYTHON/routes/vercon/{receipt_tree,invoices,documents,doc_routes,receipt_distribution}.py;
PYTHON/static/js/vercon/{docs,document-tree-order}.js;
PYTHON/static/js/components/treeview/treeview-render.js;
PYTHON/static/css/{vercon-efactura,vercon-document-tree}.css;
PYTHON/static/help/vercon.json; implementare_vercon_v2.md; acest jurnal.

Fără DDL nou sau execuție SQL. Publicare: Python/JS/CSS/help și rendererul
TreeView împreună, repornirea serviciului și reîncărcarea paginii.
