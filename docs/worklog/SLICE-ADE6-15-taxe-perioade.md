# SLICE-ADE6-15 — taxe cu perioade lună/an

09.10.2026. SCRIS LOCAL, fără teste.

## Schimbări

Escape este consumat în faza capture a editorului comun DataGrid și anulează
doar editarea celulei, inclusiv cu calendar/combobox; dialogul rămâne deschis.
Controlul checkbox este opțional în componenta comună și disponibil la Activ
în Taxe. Checkboxurile I din Plătitori rămân read-only.

Am eliminat mesajul cu instrucțiuni din Taxe. Elementul pentru mesaje rămâne
ascuns și este folosit numai pentru erori, pentru a păstra raportarea controlată.
Începutul taxei se introduce ca text ll.aaaa (fără calendar), stocat YYYY-MM.
Sfârșitul este coloană doar pentru afișare, fără input/editor, inclusiv pe rând nou.

O taxă nouă este activă implicit și primește propunerea lunii curente sau a lunii
următoare celei mai recente taxe, dacă există deja una cu început ulterior.
Taxa anterioară devine inactivă și primește sfârșitul în luna precedentă noului
început. UI afișează modificarea în draft; API o aplică în tranzacția atomică
la salvare, cu versiuni/idempotency existente. IDV din lunile istorice nu se schimbă.
Începutul taxei noi trebuie să fie ulterior începuturilor deja cunoscute.

## Date migrate

Conform precizării explicite, taxele migrate NU au date. DeLa/PanaLa sunt nullable;
nu se impune început pentru rândurile existente fără dată, nu se inventează date
istorice și nu se modifică importul/calculul pentru a le cere. Rândurile existente
se pot edita și folosi fără perioade. Începutul se cere numai la creare prin catalog.
Importul folosește validate_values, nu comanda de creare din catalog, și acceptă
câmpurile lipsă/NULL. Taxa migrată activă poate primi sfârșitul calculat atunci când
este adăugată o taxă nouă; începutul său rămâne necunoscut.

## Fișiere

- PYTHON/static/js/dgv/editing.js, datagrid.js, engine.js; css/dgv.css.
- PYTHON/static/js/adechit/catalogs.js și adechit.html (ajutor și mesaj ascuns).
- PYTHON/routes/adechit/domain.py, schema.json, service.py.
- sql/AD_04_taxe_perioade.sql; statusul ADE și acest worklog.

## Predare și limite

Nu am rulat teste, pornire sau probe vizuale, conform preferinței utilizatorului.
Nu am accesat serverul și nu am executat SQL. Exportul local MariaDB_Schema nu are
AD_; câmpurile noi sunt cerute explicit de utilizator și adăugate local ca extensie
la AD_03 aprobat, fără a declara schema live verificată.

Preview-ul trebuie repornit: initializerul său existent adaugă câmpurile nullable
în SQLite; valorile vechi rămân NULL. Nu se modifică baza Access.
Pentru server, după AD_03 se aplică o singură dată AD_04 în baza unității, apoi
se publică fișierele runtime prin AvacontPush. Scriptul nu completează date istorice.
Fără commit/push/publicare în acest chat. Următoarea subfelie liberă: ADE6-16.
