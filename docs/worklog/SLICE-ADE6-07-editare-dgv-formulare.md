# SLICE-ADE6-07 — editare DGV și formulare compacte

09.10.2026. SCRIS LOCAL; utilizatorul verifică funcționarea.

## Schimbări și motiv

Conform cerințelor operatorului, Enter confirmă și deschide următoarea celulă editabilă
în ordinea vizibilă, inclusiv pe rândul următor; celulele indisponibile sunt sărite.
Tab păstrează aceeași navigare, Shift inversează sensul. Erorile păstrează celula curentă.
Pe PC (lățime peste 980px și pointer precis), clicul unic deschide editorul. Clicul pe
altă celulă confirmă mai întâi editarea curentă. Pe mobil clicul simplu selectează;
dublu clic/Enter rămân disponibile pentru editare. Grilele fără drepturi rămân read-only.

Celulele de dată folosesc DatePicker comun, cu introducere zz.ll.aaaa și calendarul
folosit la plăți; sursa păstrează valoarea ISO. Controlul este distrus la finalul editării.

Antetele tuturor dialogurilor ADE folosesc aceleași variabile de culoare ca antetul paginii.
Editorul grupei are lățime maximă 760px, Educator umple spațiul disponibil în tabel.
ANI și tabelul au înălțime 250px, antete și rânduri de 28px; culorile ANI provin din
tema DataGrid. + Adaugă este acțiune în footerul tabelului, confirmând celula curentă
înainte de crearea educatorului. Formularele copil/plătitor au lățime maximă 460px
(50% din 920px) și gap 10px (50% din 20px); perechile sunt așezate vertical pentru
a păstra spațiu pentru câmpuri. Lățimea rămâne limitată la 96vw pe ecrane mici.

## Fișiere

- PYTHON/static/js/dgv/editing.js și datagrid.js; css/dgv.css.
- PYTHON/static/js/adechit/payers.js și catalogs.js; css/adechit.css; adechit.html.
- ADECHIT_STATUS.md și state/ADECHIT_STATUS_ADE0-ADE9.md.

## Verificări și limite

Nu au fost rulate teste, browser automatizat sau verificări de execuție, conform
instrucțiunii explicite «NU MAI FACE TU TESTE». Probele din ADE6-06 sunt anterioare
acestor modificări și nu validează ADE6-07. Operatorul verifică aspectul și navigarea.
Nu s-au modificat backendul, calculele, schema, datele Access sau serverul.

## Predare

Modificările sunt locale, fără commit/publicare. Fișierele runtime enumerate se publică
prin AvacontPush de către utilizator. Nu există SQL nou. Următoarea subfelie: ADE6-08.
