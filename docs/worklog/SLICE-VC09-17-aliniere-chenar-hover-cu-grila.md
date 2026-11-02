# SLICE-VC09-17 — Alinierea chenarului de hover cu grila PDF

Data: 30.10.2026. **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.**

Utilizatorul arată că, după îngroșare, conturul albastru nu se suprapune vizual peste liniile negre ale rândului. Captura a fost citită, inclusiv culorile și pozițiile liniilor, fără a fi copiată sau modificată în proiect.

VC09-16 păstra intenționat conturul în interior (`outline: 2px`, `outline-offset: -2px`), deplasând centrul grosimii față de marginea geometrică a rândului. Offsetul devine **-1px**, centrând conturul de 2px pe acea margine.

Stratul de adnotări al previzualizării interactive VERCON primește explicit **100% din lățimea și înălțimea conținutului paginii**, ca suprafața canvasului, prevalând peste dimensiunile inline independente PDF.js. Pozițiile/dimensiunile procentuale rămân din dreptunghiurile PDF reale, fără translații fixe X/Y; `content-box` din VC09-14 rămâne activ.

Fișier funcțional: `PYTHON/static/css/vercon-stock-card.css`. Selectorii sunt limitați la previzualizarea interactivă VERCON; nu se modifică rendererul, coordonatele din PDF, bibliotecile terțe, datele sau exporturile. Acest worklog este legat din indicii VERCON.

**Fără teste, verificări ale aplicației în execuție, browser, build, PDF de probă, HTTP, SQL sau publicare.** Analiza folosește codul și captura utilizatorului; rezultatul modificării CSS nu este confirmat vizual în aplicație. Necesită actualizarea resurselor web și redeschiderea raportului.
