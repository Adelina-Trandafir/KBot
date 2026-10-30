# SLICE-VC09-17 — Alinierea chenarului de hover cu grila PDF

Data: 30.10.2026. **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.**

Utilizatorul arată că, după îngroșare, conturul albastru nu se suprapune vizual peste liniile negre ale rândului. Captura a fost citită, inclusiv culorile și pozițiile liniilor, fără a fi copiată sau modificată în proiect.

VC09-16 păstra intenționat conturul complet în interiorul dreptunghiului interactiv (`outline: 2px`, `outline-offset: -2px`). Acest lucru deplasează centrul grosimii spre interior față de marginea geometrică a rândului. Acum offsetul devine **-1px**, pentru a centra conturul de 2px pe acea margine.

În plus, stratul de adnotări al previzualizării interactive VERCON este dimensionat explicit la **100% din lățimea și înălțimea conținutului paginii**, la fel ca suprafața de afișare a canvasului. Această regulă prevalează peste dimensiunile inline independente ale PDF.js. Pozițiile și dimensiunile procentuale ale rândurilor continuă să provină din dreptunghiurile PDF reale; nu se adaugă translații fixe pe X/Y. Regula `content-box` din VC09-14 rămâne activă.

Fișier funcțional: `PYTHON/static/css/vercon-stock-card.css`. Selectorii sunt limitați la previzualizarea interactivă VERCON; nu se modifică rendererul, coordonatele din PDF, bibliotecile terțe, datele sau exporturile. Acest worklog este legat din indicii VERCON.

**Fără teste, verificări ale aplicației în execuție, browser, build, PDF de probă, HTTP, SQL sau publicare.** Analiza folosește codul și captura utilizatorului; rezultatul modificării CSS nu este confirmat vizual în aplicație. Necesită actualizarea resurselor web și redeschiderea raportului.
