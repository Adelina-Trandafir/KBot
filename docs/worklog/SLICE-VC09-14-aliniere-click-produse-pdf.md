# SLICE-VC09-14 — Alinierea zonelor de click cu produsele din PDF

Data: 30.10.2026. **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.**

Utilizatorul semnalează, printr-o captură, decalajul dintre produsul desenat în raport și zona lui interactivă.

Cauză identificată prin CSS: `static/site/site.css`, încărcat de `vercon.html`, aplică global `box-sizing: border-box`. PDF.js dimensionează `.pdfViewer .page` după viewport și adaugă `--page-border` (implicit 9px/latură). Canvasul ocupă 100% din conținut: border-box îi scade bordurile din suprafață, în timp ce adnotările păstrează viewportul. Diferența de coordonate crește cu poziția în pagină.

`PYTHON/static/css/vercon-stock-card.css` restabilește explicit `box-sizing: content-box` numai pentru paginile din previzualizarea interactivă VERCON (`.vc-page .report-view-pdf--product-links .pdfViewer .page`). Canvasul și adnotările folosesc astfel aceeași dimensiune de conținut, inclusiv la redimensionarea prin zoom. Nu se adaugă deplasări fixe în pixeli și nu se modifică bibliotecile PDF.js sau rendererul rapoartelor. Celelalte aplicații și previzualizările fără acțiuni de produs nu primesc această regulă.

Se păstrează comportamentul VC09-13: fără tooltip/subliniere în conținutul interactiv și fără linkuri interne la descărcare/tipărire. Nu se modifică datele, perioada, ordinea, rândurile sau exporturile raportului.

Fișiere: `PYTHON/static/css/vercon-stock-card.css`, acest worklog și trimiterile din `VERCON/VERCON_STATUS.md` și `implementare_vercon_v2.md`.

**Fără teste, verificări automate, build, browser, PDF de probă, HTTP, SQL sau publicare.** Cauza este identificată prin analiza codului și a capturii furnizate; rezultatul vizual și clickul la diferite niveluri de zoom nu au fost probate în execuție. Captura utilizatorului nu a fost copiată în proiect. Necesită actualizarea resurselor web și redeschiderea raportului.
