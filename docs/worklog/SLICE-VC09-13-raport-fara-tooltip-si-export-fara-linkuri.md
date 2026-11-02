# SLICE-VC09-13 — Produse fără tooltip/subliniere; export fără linkuri

Data: 30.10.2026. **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.**

Utilizatorul cere eliminarea tooltipului din balanță, denumiri fără subliniere și eliminarea completă a linkurilor din raportul exportat. Deschiderea fișei de magazie în aplicație rămâne disponibilă.

PDF-ul pentru descărcare este implicit fără acțiunile interne. Adaptorul VERCON clonează modelul și elimină `href` din celulele tuturor tabelelor, inclusiv blocurile cu coloane paralele, `stock_card_href` din NIR/FCS și metadatele `stock_card`. Snapshotul din cache rămâne intact. Excel folosește aceeași eliminare a acțiunilor. Nu se modifică datele, ordinea rândurilor, totalurile sau macheta raportului.

Numai cererea explicită a previzualizării PDF (`preview=1`) păstrează adnotările interne pentru popup. Rendererul comun are opțiune de subliniere, cu comportamentul anterior implicit; adaptorul VERCON o dezactivează. Rendererul NIR/FCS elimină sublinierea propriilor linkuri. PDF-ul exportat are text simplu, fără culoarea de link și fără adnotări către fișa de magazie.

Noul `report-pdf-source.js` livrează previzualizarea și callbackul PDF-ului exportat pentru același snapshot/context. Varianta curată se cere numai la prima descărcare/tipărire, apoi se reutilizează; cererea e reluabilă la eroare. Fără acțiuni interne se reutilizează octeții încărcați. Integrare: rapoarte, documente salvate, regenerare prin schimbarea perioadei.

Viewerul comun descarcă/tipărește prin `exportPdf` opțional, altfel PDF-ul afișat, ca înainte. Callbackul urmează regenerarea; operațiile în curs nu exportă raportul anterior după închiderea/înlocuirea previzualizării. `data-kbot-tooltip="off"` existent oprește tooltipurile numai în conținutul PDF cu acțiuni de produs, păstrându-le pe bara de instrumente. CSS-ul linkurilor interactive nu adaugă subliniere.

Fișiere funcționale:

- `PYTHON/routes/vercon/report_stock_card.py`, `report_output.py`, `report_routes.py`, `report_reception.py`;
- `PYTHON/reporting/output.py` (opțiune cu implicit păstrat pentru ceilalți consumatori);
- `PYTHON/static/js/vercon/report-pdf-source.js`, `reports.js`, `document-report.js`, `report-period-action.js`;
- `PYTHON/static/js/reporting/pdf-preview.js` (callback opțional pentru descărcare/tipărire și tooltipuri limitate la conținutul interactiv);
- `PYTHON/static/css/vercon-stock-card.css`;
- acest worklog și indicii VERCON.

**Fără teste, verificări automate, build, browser, PDF de probă, HTTP, SQL sau publicare.** Schimbarea este implementată local și analizată în cod, fără confirmare vizuală sau în execuție. Necesită actualizarea serverului/resurselor web și redeschiderea rapoartelor; fișierele PDF deja descărcate nu sunt modificate.
