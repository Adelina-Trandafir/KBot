# SLICE-ADE6-12 — deschidere explicită pe mobil

09.10.2026. SCRIS LOCAL.

## Schimbări

Plătitori mobil păstrează pagina pe întregul ecran și scrollul numai în tabel,
dar primește 8px de padding la margini. Grupe și Copii au o coloană suplimentară
de 40px, cu antet gol, fără filtru/sortare și cu buton ➡️ pe fiecare rând.
Butonul deschide copiii grupei sau plătitorii copilului. Apăsarea lungă pentru
navigare a fost eliminată; clicul pe restul rândului rămâne selecție.
Platitori_sub nu primește coloană de deschidere, conform excepției solicitate.

Lățimea acțiunii se scade numai din denumire: Grupa = 85% minus 40px,
Copil = 50% minus 40px, CNP rămâne 35%, I rămâne 15%. La plătitori proporțiile
rămân 50/35/15. DataGrid comun permite explicit o coloană de lățime fixă și
deducerea ei dintr-o coloană proporțională, plus buton de acțiune în celulă.
Opțiunile sunt folosite numai de aceste liste mobile.

Rândurile DGV din ADE (Prezență, Taxe, liste și educatorii din editor) cresc pe
mobil cu exact 20%: 28px → 33,6px. Opțiunea mobileRowScale a componentei comune
este activată explicit din ADE; alte aplicații nu sunt afectate. La schimbarea
modului PC/mobil geometria DGV este recalculată. ANI păstrează înălțimea rândurilor
aliniată cu tabelul educatorilor.

## Fișiere și predare

PYTHON/static/js/dgv/datagrid.js, css/dgv.css, js/adechit/payers.js,
catalogs.js, app.js, css/adechit.css, ajutorul adechit.html și statusul ADE.
Nu au fost rulate teste sau verificări vizuale, conform cerinței utilizatorului.
Fără schimbări backend/date/SQL, commit sau publicare. Utilizatorul preia fișierele
runtime prin AvacontPush. Următoarea subfelie liberă: ADE6-13.
