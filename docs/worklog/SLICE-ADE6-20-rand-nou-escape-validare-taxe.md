# SLICE-ADE6-20 — Escape pe rând nou și validarea taxelor

09.10.2026. SCRIS LOCAL, fără teste la cererea utilizatorului.

DataGrid comun oferă beginNewRowEdit(row, key, onCancelRow): Escape în editorul
activat la adăugare anulează întregul rând prin callbackul paginii. După prima
confirmare, Escape păstrează comportamentul obișnuit: anulează doar celula.
Am conectat toate adăugările cu activare automată ale DGV din ADE: Taxe și
perioadele educatorilor. Nu se șterg înregistrări deja persistate prin Escape.

Taxele cer valoare >0, explicație nevidă și început pentru rândurile noi;
începutul nou sau modificat trebuie să fie după ultimul început existent.
Validarea este în UI și API. Excepția deja decisă pentru migrare se păstrează:
începutul NULL al unei taxe existente nu blochează dacă nu este modificat.
Valoarea și explicația sunt obligatorii la salvarea taxei.

La eroare de celulă, ieșire din rând din taste sau salvare, UI deschide un dialog
cu «Continuă editarea» / «Anulează înregistrarea». Continuarea revine în editor;
anularea elimină taxa nouă sau abandonează modificările rândului existent.
Previzualizarea taxelor active/perioadelor se reconstruiește din datele originale
și drafturile rămase, astfel încât anularea să refacă taxa anterior activă.
Mai multe taxe noi păstrează ordinea începuturilor la validarea batchului.

Fișiere runtime: PYTHON/static/js/dgv/editing.js,
PYTHON/static/js/adechit/catalogs.js, PYTHON/static/js/adechit/payers.js,
PYTHON/static/css/adechit.css, PYTHON/routes/adechit/service.py,
PYTHON/static/adechit.html (ajutor). Documentație: ghidul web, deciziile,
statusul rădăcină și registrul detaliat.

Nu am rulat teste, serverul sau probe vizuale. Backendul preview necesită restart
pentru validarea nouă. Nu există DDL suplimentar față de AD_04, commit, push sau
publicare din chat. Următoarea subfelie liberă: ADE6-21.
