# SLICE-VC09-15 — Chenar subtil pe întregul rând al produsului la hover

Data: 30.10.2026. **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.**

Utilizatorul cere un chenar subtil în jurul întregului rând al produsului peste care se află cursorul, în raportul afișat în aplicație.

Previzualizarea interactivă VERCON folosește acum o adnotare transparentă pe dreptunghiul real al rândului. Coordonatele sunt preluate după dimensionarea și paginarea tabelului, inclusiv pentru denumiri cu mai multe linii și pentru tabele continuate pe alte pagini. Nu se estimează înălțimea din font sau din distanța până la produsul următor. În tabelele paralele, zona se limitează la coloanele tabelului produsului respectiv, fără a cuprinde produsul din secțiunea vecină.

La hover, CSS-ul VERCON afișează un chenar interior albastru discret de 1px, cu transparență, fără umplerea galbenă implicită PDF.js. Zona cuprinde toate coloanele rândului și deschide aceeași fișă la click; denumirea rămâne albastră, fără subliniere și tooltip. Focalizarea prin tastatură păstrează indicatorul existent, iar chenarul rândului se aplică și la focus. Totalurile și rândurile fără un produs identificabil nu primesc acțiuni noi.

Implementarea folosește helperul opt-in `reporting/pdf_row_links.py`, care păstrează comportamentul de paginare al tabelelor ReportLab și adaugă adnotările numai pentru celulele marcate. Rendererul comun activează acest comportament doar la cererea adaptorului VERCON, când există metadatele `stock_card`. NIR/FCS folosesc același helper pentru propriul tabel. Alți consumatori ai rendererului comun păstrează comportamentul anterior.

PDF-ul descărcat sau tipărit și Excel rămân fără linkuri interne: modelul de export elimină în continuare adresele și metadatele înainte de randare. Chenarul este exclusiv CSS în previzualizare și nu se desenează în PDF. Nu se modifică datele, macheta, ordinea produselor sau totalurile. Regula `content-box` din VC09-14 rămâne aplicabilă alinierii canvasului cu adnotările la zoom.

Fișiere: `PYTHON/reporting/pdf_row_links.py`, `PYTHON/reporting/output.py`, `PYTHON/routes/vercon/report_output.py`, `PYTHON/routes/vercon/report_reception.py`, `PYTHON/static/css/vercon-stock-card.css`; acest worklog și indicii VERCON.

**Fără teste, verificări automate, build, browser, PDF de probă, HTTP, SQL sau publicare.** Implementarea este analizată în cod, fără confirmare vizuală sau în execuție. Necesită actualizarea serverului/resurselor web și redeschiderea raportului pentru PDF-ul de previzualizare cu zone pe rânduri.
