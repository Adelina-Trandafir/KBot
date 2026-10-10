# SLICE-AD11-04 — lățimi portal mobil

10.10.2026. SCRIS LOCAL, fără teste funcționale sau vizuale, conform preferinței utilizatorului.

Observațiile din cele trei imagini: selectorul copilului mai îngust decât cardurile,
carduri care lățesc pagina și provoacă scroll orizontal, buton Imprimă vizibil pe mobil.

adechit-parents.css: la maximum 800px, selectorul și containerul combobox ocupă 100%
din lățimea disponibilă, înlocuind limita comună de 300px. Coloanele grid folosesc
minmax(0,1fr), iar cardurile au min-width:0: SVG-ul cu lățime fixă nu mai forțează
lățimea coloanei. Graficele ample se derulează în containerul propriu. Fieldset,
labelurile calendarelor și antetul se pot comprima sau împărți pe rânduri.
Imprimă este ascuns numai pe mobil; PDF-ul rămâne disponibil.

Actualizate UTILIZARE_WEB, PORTAL_PARINTI și statusurile. Fără backend sau SQL nou;
nu este necesar restartul preview-ului. Utilizatorul reîncarcă pagina pentru proba mobilă.
Următoarea subfelie liberă: AD11-05.
