# SLICE-VC09-13 — Produse fără tooltip/subliniere; export fără linkuri

Data: 30.10.2026. **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.**

Utilizatorul cere eliminarea tooltipului din balanță, denumiri fără subliniere și eliminarea completă a linkurilor din raportul exportat. Deschiderea fișei de magazie în aplicație rămâne disponibilă.

PDF-ul pentru descărcare este implicit fără acțiunile interne. Adaptorul VERCON clonează modelul și elimină `href` din celulele tuturor tabelelor, inclusiv blocurile cu coloane paralele, `stock_card_href` din NIR/FCS și metadatele `stock_card`. Snapshotul din cache rămâne intact. Excel folosește aceeași eliminare a acțiunilor. Nu se modifică datele, ordinea rândurilor, totalurile sau macheta raportului.

Numai cererea explicită a previzualizării PDF (`preview=1`) păstrează adnotările interne pentru popup. Rendererul comun are opțiune de subliniere, cu comportamentul anterior implicit; adaptorul VERCON o dezactivează. Rendererul NIR/FCS elimină sublinierea propriilor linkuri. PDF-ul exportat are text simplu, fără culoarea de link și fără adnotări către fișa de magazie.

Modulul nou `report-pdf-source.js` livrează previzualizarea și un callback separat pentru PDF-ul exportat, legat de același snapshot/context. Varianta curată se cere la prima descărcare/tipărire și se reutilizează ulterior; nu se descarcă preventiv două PDF-uri la deschiderea raportului. La eroare, cererea poate fi reluată. Rapoartele fără acțiuni interne reutilizează direct octeții deja încărcați. Integrarea acoperă rapoartele, documentele salvate și regenerarea prin schimbarea perioadei.

Viewerul comun acceptă opțional `exportPdf`; descărcarea și tipărirea îl folosesc când este furnizat. În lipsa lui folosesc PDF-ul afișat, ca până acum. Callbackul se schimbă împreună cu raportul după regenerare; operațiile în curs nu descarcă/tipăresc raportul anterior dacă previzualizarea a fost închisă sau înlocuită. Pentru conținutul PDF cu acțiuni de produs se dezactivează tooltipurile prin mecanismul existent `data-kbot-tooltip="off"`; tooltipurile barei de instrumente se păstrează. CSS-ul linkurilor interactive nu adaugă subliniere.

Fișiere funcționale:

- `PYTHON/routes/vercon/report_stock_card.py`, `report_output.py`, `report_routes.py`, `report_reception.py`;
- `PYTHON/reporting/output.py` (opțiune cu implicit păstrat pentru ceilalți consumatori);
- `PYTHON/static/js/vercon/report-pdf-source.js`, `reports.js`, `document-report.js`, `report-period-action.js`;
- `PYTHON/static/js/reporting/pdf-preview.js` (callback opțional pentru descărcare/tipărire și tooltipuri limitate la conținutul interactiv);
- `PYTHON/static/css/vercon-stock-card.css`;
- acest worklog și indicii VERCON.

**Fără teste, verificări automate, build, browser, PDF de probă, HTTP, SQL sau publicare.** Schimbarea este implementată local și analizată în cod, fără confirmare vizuală sau în execuție. Necesită actualizarea serverului/resurselor web și redeschiderea rapoartelor; fișierele PDF deja descărcate nu sunt modificate.
