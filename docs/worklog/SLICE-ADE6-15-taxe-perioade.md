# SLICE-ADE6 — Taxe, perioade și validarea rândurilor

Repere comune pentru etapele indicate: 09.10.2026 — ADE6-15, ADE6-19, ADE6-20. Fiecare are starea TESTAT, RULAT, PUBLICAT, confirmată de operator la 23.10.2026; probele și limitele inițiale rămân în secțiunile lor.

15 păstrează perioadele lună/an și excepția taxelor migrate fără date; 19 restrânge lățimea ferestrei, iar 20 extinde Escape la întregul rând nou înainte de prima confirmare și întărește validarea. După prima celulă confirmată, Escape anulează numai celula. Anularea unui draft reface taxa anterior activă. Regula pentru educatorii noi din 20 rămâne parte a aceleiași extensii DataGrid; [catalogul](SLICE-ADE6-01-cataloage.md) o folosește.

[ADE6-15](#slice-ADE6-15) · [ADE6-19](#slice-ADE6-19) · [ADE6-20](#slice-ADE6-20)

## SLICE-ADE6-15 — taxe cu perioade lună/an <a id="slice-ADE6-15"></a>

### Schimbări

Escape este consumat în faza capture a editorului comun DataGrid și anulează doar editarea celulei, inclusiv cu calendar/combobox; dialogul rămâne deschis. Controlul checkbox este opțional în componenta comună și disponibil la Activ în Taxe. Checkboxurile I din Plătitori rămân read-only.

Am eliminat mesajul cu instrucțiuni din Taxe. Elementul pentru mesaje rămâne ascuns și este folosit numai pentru erori, pentru a păstra raportarea controlată. Începutul taxei se introduce ca text ll.aaaa (fără calendar), stocat YYYY-MM. Sfârșitul este coloană doar pentru afișare, fără input/editor, inclusiv pe rând nou.

O taxă nouă este activă implicit și primește propunerea lunii curente sau a lunii următoare celei mai recente taxe, dacă există deja una cu început ulterior. Taxa anterioară devine inactivă și primește sfârșitul în luna precedentă noului început. UI afișează modificarea în draft; API o aplică în tranzacția atomică la salvare, cu versiuni/idempotency existente. IDV din lunile istorice nu se schimbă. Începutul taxei noi trebuie să fie ulterior începuturilor deja cunoscute.

### Date migrate

**Taxele migrate nu au date**, conform precizării utilizatorului: DeLa/PanaLa rămân nullable, fără date istorice inventate sau schimbarea importului/calculului. Rândurile existente pot fi editate/folosite fără perioade; începutul este obligatoriu numai la crearea prin catalog. Importul folosește `validate_values` și acceptă lipsă/NULL. Adăugarea taxei noi poate închide taxa migrată activă, păstrându-i începutul necunoscut.

### Fișiere

PYTHON/static/js/dgv/editing.js, datagrid.js, engine.js; css/dgv.css. PYTHON/static/js/adechit/catalogs.js și adechit.html (ajutor și mesaj ascuns). PYTHON/routes/adechit/domain.py, schema.json, service.py. sql/AD_04_taxe_perioade.sql; statusul ADE și acest worklog.

### Predare și limite

Nu am rulat teste, pornire sau probe vizuale, conform preferinței utilizatorului. Nu am accesat serverul și nu am executat SQL. Exportul local MariaDB_Schema nu are AD_; câmpurile noi sunt cerute explicit de utilizator și adăugate local ca extensie la AD_03 aprobat, fără a declara schema live verificată.

Preview-ul trebuie repornit: initializerul său existent adaugă câmpurile nullable în SQLite; valorile vechi rămân NULL. Nu se modifică baza Access. Pentru server, după AD_03 se aplică o singură dată AD_04 în baza unității, apoi se publică fișierele runtime prin AvacontPush. Scriptul nu completează date istorice. Fără commit/push/publicare în acest chat. Următoarea subfelie liberă: ADE6-16.

## SLICE-ADE6-19 — formular Taxe compact <a id="slice-ADE6-19"></a>

Formularul Taxe are lățime maximă 744px în loc de 1120px: aproximativ 34% mai mic, calculat din cele cinci coloane DGV (710px), paddingurile laterale (32px) și bordurile ferestrei (2px). Limita 96vw păstrează adaptarea la ecrane înguste. Regula CSS este limitată la #ade-catalog, fără modificarea altor editori.

Fișier publicabil: PYTHON/static/css/adechit.css. Documentația actualizată: ADECHIT/UTILIZARE_WEB.md, ADECHIT_STATUS.md și registrul detaliat ADE0–ADE9. Fără teste sau verificări vizuale, conform instrucțiunii utilizatorului. Fără SQL, commit sau publicare. Următoarea subfelie liberă: ADE6-20.

## SLICE-ADE6-20 — Escape pe rând nou și validarea taxelor <a id="slice-ADE6-20"></a>

DataGrid comun oferă beginNewRowEdit(row, key, onCancelRow): Escape în editorul activat la adăugare anulează întregul rând prin callbackul paginii. După prima confirmare, Escape păstrează comportamentul obișnuit: anulează doar celula. Am conectat toate adăugările cu activare automată ale DGV din ADE: Taxe și perioadele educatorilor. Nu se șterg înregistrări deja persistate prin Escape.

Taxele cer valoare >0, explicație nevidă și început pentru rândurile noi; începutul nou sau modificat trebuie să fie după ultimul început existent. Validarea este în UI și API. Excepția deja decisă pentru migrare se păstrează: începutul NULL al unei taxe existente nu blochează dacă nu este modificat. Valoarea și explicația sunt obligatorii la salvarea taxei.

La eroare de celulă, ieșire din rând din taste sau salvare, UI deschide un dialog cu «Continuă editarea» / «Anulează înregistrarea». Continuarea revine în editor; anularea elimină taxa nouă sau abandonează modificările rândului existent. Previzualizarea taxelor active/perioadelor se reconstruiește din datele originale și drafturile rămase, astfel încât anularea să refacă taxa anterior activă. Mai multe taxe noi păstrează ordinea începuturilor la validarea batchului.

Fișiere runtime: PYTHON/static/js/dgv/editing.js, PYTHON/static/js/adechit/catalogs.js, PYTHON/static/js/adechit/payers.js, PYTHON/static/css/adechit.css, PYTHON/routes/adechit/service.py, PYTHON/static/adechit.html (ajutor). Documentație: ghidul web, deciziile, statusul rădăcină și registrul detaliat.

Nu am rulat teste, serverul sau probe vizuale. Backendul preview necesită restart pentru validarea nouă. Nu există DDL suplimentar față de AD_04, commit, push sau publicare din chat. Următoarea subfelie liberă: ADE6-21.
