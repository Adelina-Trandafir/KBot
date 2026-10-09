# SLICE-ADE6-14 — filtre și culori DGV

09.10.2026. SCRIS LOCAL.

Filtrarea coloanelor rămâne exclusiv pentru Nume și Grupa, în toate DGV ADE,
în ambele moduri PC/mobil. CNP, I, Educatori, datele, valorile numerice,
coloana de navigare și coloanele Taxe nu permit filtrare. Coloanele mobile
nu mai suprascriu în bloc filter:false și păstrează filtrul denumirii.
Componentele comune din alte aplicații nu își schimbă filtrarea.

Am eliminat suprascrierea culorilor antetului/footerului listelor Plătitori cu
fundalul alb. Ele folosesc variabilele și tema DataGrid comună, la fel ca
DGV principal din Prezență, inclusiv în tema întunecată. Nu există culori noi.

Fișiere: PYTHON/static/js/adechit/payers.js, catalogs.js, app.js,
css/adechit.css, ajutorul adechit.html și statusul ADE.
Nu am rulat teste sau verificări vizuale, conform cerinței utilizatorului.
Fără modificări backend/SQL/date, commit sau publicare. Se preia după reîncărcare;
utilizatorul publică prin AvacontPush. Următoarea subfelie: ADE6-15.
