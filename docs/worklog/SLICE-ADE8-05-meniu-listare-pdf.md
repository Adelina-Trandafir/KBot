# ADE8-05 — Meniu de listare și PDF pentru chitanțe

Utilizatorul a confirmat că Listare și Descarcă PDF trebuie să fie funcționale.
Pe PC, 📥 de lângă salvare deschide meniul ultimei chitanțe salvate a copilului;
este inactiv înainte de prima salvare. Fiecare chitanță existentă are și buton
propriu. Meniul are aspect Windows, pictograme în stânga, navigare din taste și
închidere prin Escape/clic în afară. Este ascuns pe mobil.

Listare deschide documentul și dialogul browserului. PDF-ul se generează cu
ReportLab. Ambele folosesc aceeași chitanță salvată, plata, plătitorul asociat și
datele unității. Nu emit alt număr și nu salvează automat rândul nou. Documentul
are două exemplare și marcaj ANULATĂ când este cazul. Structura câmpurilor se
bazează pe raportul Access Chitanta; echivalența vizuală nu a fost verificată.

Trimitere pe mail apare numai când plătitorul asociat are email completat și
rămâne inactivă. Nu se trimite niciun mesaj.

Endpointurile print/pdf reutilizează autorizarea și contextul unității. Cererile
trimit tokenul/contextul în headere; acestea nu sunt introduse în URL. Răspunsurile
nu se păstrează în cache. Șablonul HTML escapează datele, PDF-ul escapează textul
pentru Paragraph, iar meniul curăță listenerii la închidere/navigare.

Fișiere principale: routes/adechit/receipt_output.py și __init__.py,
static/js/adechit/app.js și receipt-print.js, static/css/adechit.css și
adechit-receipt.css, templates/adechit/receipt.html, static/fonts/DejaVuSans.ttf
și licența, requirements-adechit.txt, ajutorul și documentele de stare.

Verificare numai de sintaxă: py_compile pentru modulele Python și node --check
pentru app.js și receipt-print.js, reușite. Nu s-au executat teste automate,
generări PDF de probă sau verificări funcționale/vizuale, conform regulii ADECHIT.

Publicarea trebuie să includă fontul, licența, șablonul, CSS/JS și modulele API.
Pe server se instalează `pip install -r requirements-adechit.txt` în mediul
backendului, apoi se repornește backendul. Nu există DDL nou. Nu s-a efectuat
publicare, instalare sau restart remote. Utilizatorul verifică meniul, emailul
condițional, listarea, diacriticele, suma în litere și PDF-ul după publicare.

Următoarea subfelie liberă: ADE8-06.
