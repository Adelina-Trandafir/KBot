# SLICE-VC09-14 — Alinierea zonelor de click cu produsele din PDF

Data: 30.10.2026. **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.**

Utilizatorul semnalează, printr-o captură, decalajul dintre produsul desenat în raport și zona lui interactivă.

Analiza CSS identifică o incompatibilitate: `vercon.html` încarcă `static/site/site.css`, care aplică global `box-sizing: border-box`. PDF.js dimensionează `.pdfViewer .page` după viewportul PDF și adaugă bordura definită prin `--page-border` (implicit 9px pe fiecare latură). Canvasul are dimensiuni de afișare de 100% din conținutul paginii. Cu modelul global border-box, bordurile se scad din suprafața desenată, dar stratul de adnotări păstrează dimensiunile viewportului. Coordonatele nu mai reprezintă aceeași suprafață, iar diferența se acumulează cu poziția în pagină.

`PYTHON/static/css/vercon-stock-card.css` restabilește explicit `box-sizing: content-box` numai pentru paginile din previzualizarea interactivă VERCON (`.vc-page .report-view-pdf--product-links .pdfViewer .page`). Canvasul și adnotările folosesc astfel aceeași dimensiune de conținut, inclusiv la redimensionarea prin zoom. Nu se adaugă deplasări fixe în pixeli și nu se modifică bibliotecile PDF.js sau rendererul rapoartelor. Celelalte aplicații și previzualizările fără acțiuni de produs nu primesc această regulă.

Se păstrează comportamentul VC09-13: fără tooltip/subliniere în conținutul interactiv și fără linkuri interne la descărcare/tipărire. Nu se modifică datele, perioada, ordinea, rândurile sau exporturile raportului.

Fișiere: `PYTHON/static/css/vercon-stock-card.css`, acest worklog și trimiterile din `VERCON/VERCON_STATUS.md` și `implementare_vercon_v2.md`.

**Fără teste, verificări automate, build, browser, PDF de probă, HTTP, SQL sau publicare.** Cauza este identificată prin analiza codului și a capturii furnizate; rezultatul vizual și clickul la diferite niveluri de zoom nu au fost probate în execuție. Captura utilizatorului nu a fost copiată în proiect. Necesită actualizarea resurselor web și redeschiderea raportului.
