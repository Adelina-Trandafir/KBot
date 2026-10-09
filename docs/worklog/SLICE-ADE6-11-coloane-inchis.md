# SLICE-ADE6-11 — coloane I

09.10.2026. SCRIS LOCAL.

Toate coloanele checkbox de stare din listele Grupe/Copii/Plătitori au titlul I,
de la închis, pe PC și mobil. Coloanele folosesc filter:false în DataGrid comun,
deci nu au buton/săgeată de filtrare și nici comandă de filtrare în meniul coloanei.

La Grupe bifa reflectă închiderea grupei, la Copii reflectă Plecat. La Platitori_sub
se calculează numai pentru afișare Closed = !Boolean(Activ): activ → nebifat,
inactiv → bifat. Nu se inversează valoarea stocată în Activ și nu se schimbă
semantica checkboxului Activ din editor. Bifele listelor rămân read-only.

Fișiere: PYTHON/static/js/adechit/payers.js, ajutorul adechit.html și statusul ADE.
Nu am rulat teste sau probe vizuale, conform cerinței utilizatorului. Fără modificări
backend/schema/date, fără SQL, commit sau publicare. Fișierele statice se preiau
după reîncărcare; utilizatorul publică prin AvacontPush. Următoarea subfelie: ADE6-12.
