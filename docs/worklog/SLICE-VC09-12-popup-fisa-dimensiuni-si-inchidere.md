# SLICE-VC09-12 — Fișa din raport: popup mai lat, mai scurt și închidere la exterior

Data: 30.10.2026. **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.**

Utilizatorul a trimis captura popupului îngust și a cerut lățime dublă, înălțime redusă cu 25%, eliminarea derulării orizontale, butonul Închide vizibil și închidere la click în exterior.

Din CSS rezultă că regula comună `.confirm-box` are lățimea 420px și poate prevala peste `.vc-modal` la aceeași specificitate. Popupul primește regula explicită `.confirm-box.vc-stock-card`: **840px**, limitat la 94% din lățimea disponibilă. Înălțimea este **67,5% din viewportul aplicației** (75% din limita anterioară de 90%), cu suport pentru viewportul dinamic și zoomul aplicației. Nu se schimbă dimensiunile altor dialoguri.

Dialogul are structură flex verticală: antet și subsol fixe, corpul ocupă spațiul rămas și se poate derula vertical. Butonul Închide este în subsolul permanent vizibil, aliniat la dreapta. Grila ocupă spațiul rămas în corp, în locul înălțimii proprii de 48vh care împingea subsolul în jos. Coloanele folosesc în continuare dimensionarea proporțională a controlului; derularea orizontală este dezactivată strict în corpul și grila acestui popup. Controlul DataGrid comun nu este modificat.

Clickul pe backdrop închide numai fișa și anulează cererea în curs prin ciclul de viață existent. Se verifică începutul și sfârșitul gestului în afara dreptunghiului dialogului, pentru a evita închiderea la o selecție/tragere începută în interior. Clickurile în conținut nu închid fișa. Închide și Escape rămân disponibile; raportul principal nu se reîncarcă.

Fișiere: `PYTHON/static/css/vercon-stock-card.css`, `PYTHON/static/js/vercon/report-stock-card.js`; acest worklog și trimiterile din `VERCON/VERCON_STATUS.md` și `implementare_vercon_v2.md`.

**Fără teste, verificări automate, browser, capturi noi, build, SQL sau publicare.** Analiza dimensiunilor este din CSS și captura utilizatorului, nu din măsurarea unei ferestre live. Captura nu a fost salvată în proiect. Aspectul efectiv la diferite rezoluții și gestul de închidere rămân neprobate în execuție.
