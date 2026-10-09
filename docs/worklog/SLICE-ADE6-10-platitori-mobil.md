# SLICE-ADE6-10 — Plătitori mobil

09.10.2026. SCRIS LOCAL.

## Schimbări

Pe mobil, dialogul Plătitori ocupă 100vw și 100dvh, fără margini/padding lateral.
Antetul și footerul rămân pe loc; un singur tabel ocupă restul înălțimii. Listele
nu mai sunt stivuite într-un container cu scroll. Derularea este numai în tabel.
Mesajul informativ este ascuns; erorile de încărcare rămân afișabile controlat.

Navigarea este pe niveluri: Grupe, Copii, Plătitori. Clicul scurt selectează rândul,
apăsarea lungă de 550ms pe grupă trece la copiii săi, cea pe copil la plătitorii săi.
Deplasarea de peste 10px, anularea pointerului și ieșirea din tabel anulează gestul.
Clicul generat după apăsarea lungă este consumat pentru a nu selecta accidental
rândul de pe noul nivel. Butonul ↩️, în dreapta antetului, revine la nivelul anterior;
din Grupe închide dialogul. Selecția este păstrată la revenire.

Footerul paginii conține numai ➕ și ✏️ pentru nivelul curent, cu etichete accesibile.
Acțiunile folosesc editorii existenți și drepturile lor. Butoanele text de sub fiecare
listă și Închide din footer sunt ascunse pe mobil. Pe PC rămân cele trei panouri.

Grupe afișează numai Grupa (85%) și PL (15%). Copii afișează Nume/CNP/PL în
proporțiile 50%/35%/15%; Plătitori Nume/CNP/ACT în aceleași proporții.
DataGrid comun are opțiunea explicită proportionalWidths: lățimile coloanelor devin
ponderi din lățimea disponibilă, actualizate la resize și la apariția scrollbarului.
Aceasta este activată numai pentru listele mobile. Resize-ul manual și filtrele din
antetele mobile sunt ascunse; listele nu au scroll orizontal. PL/ACT rămân read-only.

## Fișiere

PYTHON/static/js/adechit/payers.js, js/dgv/datagrid.js, css/adechit.css,
adechit.html (structură și ajutor), ADECHIT_STATUS.md și statusul detaliat.

## Verificare și predare

Nu au fost rulate teste sau probe de browser/vizuale, conform preferinței utilizatorului.
Autorizarea anterioară pentru teste privea numai pornirea serverului/aplicației.
Nu am modificat backendul, schema sau datele. Lucrarea este locală, fără commit/push.
Se preia prin reîncărcarea paginii; publicarea runtime aparține utilizatorului prin
AvacontPush. Nu există SQL nou. Următoarea subfelie liberă: ADE6-11.
