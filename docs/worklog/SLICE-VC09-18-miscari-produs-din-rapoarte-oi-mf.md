# SLICE-VC09-18 — Mișcările produsului din rapoartele OI și MF

09.11.2026 — Implementat local.

- În previzualizarea rapoartelor pentru Obiecte de inventar și Mijloace fixe, clickul pe rândul unei variante identificabile deschide **Mișcări produs** pentru gestiunea și perioada raportului. Popupul arată data, tipul și numărul documentului, traseul, contul de folosință și cantitatea.
- Datele provin din documentele active de magazie și de folosință. O predare cu bon de consum asociat apare o singură dată. Ieșirea BU este marcată „Transferat în afara subunității”.
- În rapoartele de Materiale și Alimente, clickul deschide **Fișa de magazie**. Totalurile și rândurile fără o variantă univocă rămân fără acțiune. Linkurile sunt disponibile în previzualizarea din aplicație, iar exporturile păstrează valorile raportului.

Fișierele slice-ului sunt `PYTHON/routes/vercon/product_movements.py`, `stock_routes.py`, `report_stock_card.py`, `PYTHON/static/js/vercon/report-stock-card.js`, `PYTHON/static/css/vercon-stock-card.css`, acest worklog, `VERCON/VERCON_STATUS.md` și indexul `implementare_vercon_v2.md`.

Fără teste, browser, SQL sau publicare executate. Înregistrarea pentru publicare așteaptă confirmarea explicită după probă.
