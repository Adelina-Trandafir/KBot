# SLICE-VC08-16 — Ordonare după tip și prețuri istorice în rapoarte

Data: 2026-10-23. Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Regula cerută pentru toate aplicațiile este memorată în AGENTS.md: texte alfabetic în română, date cronologic și numere numeric, inclusiv zecimale și valori negative. Cheile tehnice nu înlocuiesc denumirile.

## Controale comune

Modulul nou utils/typed-order.js centralizează comparația după tip. Numerele zecimale sunt comparate fără conversia la Number care pierde precizia valorilor DECIMAL(20,4). Sunt acceptate forma canonică și forma românească cu virgulă, inclusiv separatori de mii. Textul se compară cu Intl.Collator pentru română; datele reale și etichetele calendaristice complete se compară cronologic.

TreeView ordonează frații la fiecare randare, inclusiv după încărcarea lazy, căutare și reîmprospătare; ComboTree moștenește regula. Combobox ordonează rezultatele înaintea limitării și navigării cu tastatura. Etichetele HTML folosesc textul vizibil. DataGrid aplică implicit ordonarea crescătoare după prima coloană sortabilă, compară numerele exact și folosește etichetele referințelor. Filtrele grilelor ordonează opțiunile după valoarea reală și tipul coloanei, nu după textul formatat.

Documentele și lunile VERCON folosesc explicit chei calendaristice, cu numărul numeric pentru departajare; sensul descrescător existent pentru documente rămâne. Ramurile documentelor din selecțiile rapoartelor au aceleași metadate, inclusiv în arborii OI/MF. Produsele sunt alfabetice, variantele sunt numerice după preț. Panoul prețurilor arată și ID/cod. Previzualizarea unei machete păstrează ordinea modelului și totalurile; sortarea cerută de utilizator în grilă folosește tipul și valoarea brută.

## Eroarea de evaluare

Mesajul generic era ridicat de report_data.balances pentru un preț NULL sau negativ. Politica importului din 14.10.2026 păstrează explicit semnul prețurilor istorice; citirea rapoartelor nu trebuie să le respingă.

stock_valuation.price validează existența și caracterul finit al prețului, acceptând semnul istoric. Este reutilizat de rapoartele magaziei, OI/MF și serviciul de stoc. Calculele folosesc prețul cu semnul său; nu se modifică prețurile, cantitățile, stocurile sau documentele salvate. Regulile de introducere a documentelor noi nu sunt relaxate.

În rapoartele finale, rândurile cu stoc final zero sunt eliminate înaintea validării prețului; în balanță sunt eliminate mai întâi rândurile pe care opțiunea de afișare le ascunde deja. Un preț lipsă pentru o poziție care trebuie evaluată rămâne eroare: mesajul identifică denumirea, varianta, codul și contul și indică identificarea în Configurări → Produse și necesitatea completării prețului din sursă. Nu se inventează preț zero. Helpul explică aceste cazuri.

## Fișiere

AGENTS.md — regula persistentă a tuturor aplicațiilor. PYTHON/static/js/utils/typed-order.js (nou). PYTHON/static/js/components/treeview/treeview-render.js. PYTHON/static/js/components/combobox/combobox-state.js. PYTHON/static/js/dgv/{datagrid.js,engine.js,filtering.js}. PYTHON/static/js/vercon/{ui.js,nomenclator-grid.js,document-tree-order.js, receipt-tree.js,product-stock.js,report-selection.js,report-result.js}. PYTHON/routes/vercon/{report_order.py (nou),stock_valuation.py (nou), report_data.py,report_oi.py,stock.py,report_selection.py, report_incoming_selection.py,report_outgoing_selection.py,report_mf_selection.py}. PYTHON/static/help/vercon.json; PYTHON/reports/vercon/README.md; implementare_vercon_v2.md și acest jurnal.

Numai analiză de design pe surse. Nu s-au rulat teste, verificări automate, verificări de sintaxă, browser ori generări PDF/Excel. Fără acces MariaDB, SQL executat sau modificări de schemă. Capturile de help sunt nemodificate. Nu se poate identifica din captură dacă prețul concret este negativ sau NULL; nu s-au inspectat datele bazei. Publicarea cere toate modulele Python/JS și helpul împreună, urmate de repornirea serviciului web.
