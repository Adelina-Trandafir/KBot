# 2026-10-17 — OI: conturi multiple, documente și rapoarte

Cerere: implementarea predării, transferului, casării și evidenței/rapoartelor
OI, cu mai multe conturi posibile atât în magazie, cât și în folosință.

## Documente

BP alege explicit contul din magazie și contul în folosință, independent.
Sursa este oferită dintre conturile cu recepții în context; destinația este
aleasă din nomenclatorul de conturi activ. Nu se inventează o clasificare
după prefix și nu se impune un singur cont în întreaga gestiune. Un BP are
o pereche sursă–destinație; alte perechi se folosesc în alte BP, pe modelul
antetului existent. Dacă selectorul are mai multe conturi, rămâne activ.

BT și BS aleg produse numai din loturile predătorului. Cheia de lot include
documentul de proveniență, varianta, contul în folosință și prețul. Un bon
poate conține loturi de pe conturi diferite; contul și prețul sunt păstrate
pe fiecare linie. Prețul trimis de client identifică lotul, dar valoarea
salvată este întotdeauna cea citită de server din lotul eligibil.

Disponibilul folosește data documentului, exclude documentul editat și
respectă și ieșirile ulterioare. La salvare se validează din nou în tranzacția
existentă; cronologia se urmărește pe deținător/lot/cont/preț. Anularea și
editarea documentelor care au alimentat alte documente rămân protejate.
Registrul cu proveniențe/date incoerente primește diagnostic, fără reparații
tacite. Predarea păstrează ieșirea unică de magazie și încărcarea registrului;
transferul păstrează cele două sensuri, cu efect net zero.

Editorul OI este în pagină, cu arbore ascuns și spațiul folosit integral.
Grilele au footer pentru adăugare și totaluri; BT/BS aleg prin arbore
cont–lot/produs/preț. Casarea nu are primitor. Ciornele OI calculează valorile
prin întregi BigInt, la patru zecimale de stocare și rotunjirea ZEC, iar
serverul păstrează Decimal. Denumirile conturilor/deținătorilor inactivi
rămân disponibile în consultarea documentului istoric.

Fișiere: `PYTHON/routes/vercon/oi.py`, `oi_custody.py`, `doc_routes.py`,
`protocol.py`; `PYTHON/static/js/vercon/docs.js`, `oi-ui.js`, `oi-values.js`,
`consumption-ui.js`; `PYTHON/static/css/vercon-oi.css`, `vercon.css`.

## Evidență și rapoarte

Stocul în folosință folosește contul fiecărei mișcări, cu gruparea
deținător–cont–variantă. Nu mai moștenește contul de magazie al produsului.
Grila și selecția mișcărilor păstrează identitatea contului. Adunarea
cantităților afișate folosește utilitarul decimal comun pentru a suporta
valori cu un număr diferit de zecimale în același răspuns.

Rapoartele OI sunt disponibile în meniul aplicației și în catalog:
bonuri BP/BT/BS, centralizatoare, balanță, stocuri pe deținător/produs,
situații/fișe și liste de inventariere. Selecția contului de folosință este
independentă de arborele produselor și de contul lor de magazie. Arborele
perioadei cuprinde anii de la primul NIR/BP până la ultimul an relevant,
cu numai ultimul an expandat inițial; intervalele pot traversa anii.

Bonurile păstrează valorile istorice ale liniilor. Balanțele și fișele sunt
evaluate la prețul curent, cu notă explicită. Soldurile inițiale preced
intervalul, iar predările/transferurile/casările includ ambele capete.
Totalurile pe produse păstrează conturile distincte. Inventarierea lasă
goale câmpurile pentru constatarea faptică; nu inventează diferențe.

Patru surse SELECT suplimentare sunt configurabile în KBotAdmin:
`oi_documents`, `custody_ledger`, `oi_accounts`, `oi_holders`. Se folosesc
modelul comun de raport, snapshoturile, PDF și infrastructura comună de
lățimi Excel. Preview-ul unui bon OI salvat folosește raportul oficial;
ciornele/anulate și cititorii fără drept separat de raport păstrează
preview-ul local. Rapoartele MF rămân în starea lor anterioară, amânată.

S-a corectat și mascarea funcției `selected` printr-o variabilă locală în
calculul balanței magaziei, deoarece aceasta împiedica folosirea filtrării
comune în rapoartele de magazie disponibile și în gestiunea OI.

Fișiere: `PYTHON/routes/vercon/stock_source.py`, `stock.py`, `capabilities.py`,
`report_catalog.py`, `report_data.py`, `report_oi.py`, `report_routes.py`,
`report_service.py`, `report_layouts.py`, `report_body.py`, `report_store.py`;
`PYTHON/routes/admin_cli/vercon_reports.py`;
`PYTHON/static/js/vercon/stock.js`, `reports.js`, `report-selection.js`,
`report-result.js`, `document-report.js`;
cele patru fișiere SQL din `PYTHON/reports/vercon/queries/` și README-ul lor.

## Stare și limite

IMPLEMENTAT ÎN SURSE, NERULAT. Nu s-au executat teste, verificări de sintaxă,
build, cereri HTTP, browser sau interogări MariaDB, conform instrucțiunilor
proiectului și cererii de implementare înaintea probelor. Au fost citite
sursele și analizate contractele de date/interfață pentru proiectare.
Machetele OI folosesc infrastructura existentă și încă necesită acceptare
vizuală; nu se declară paritate dinamică cu rapoartele Access.

Nicio schemă sau bază de date modificată. Nu s-au adăugat DDL, seed ori
creare automată de tabele: schimbările folosesc coloanele existente.
Nu s-a publicat și nu s-a extins serverul demonstrativ local la OI.
