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

Completare de trasabilitate D001-04: descrierea generală din 17.10 consemna explicit că loturile cu prețuri diferite rămân distincte chiar pe aceeași variantă/document de primire; drepturile, motivul anulării și păstrarea ciornei la eșec refoloseau protocolul existent. Fără extinderea sau repornirea serverului demonstrativ pentru acel lot.

## Arbori, numerotare și editori BP/BT/BS — 18.10.2026

D001-09, istoric: **IMPLEMENTAT ÎN SURSE, NERULAT**. Numai design din surse; fără teste/build/verificări automate, browser/HTTP, MariaDB/SQL/DDL, schimbări de schemă/date, migrator, deploy/commit/push. Aspectul nu a fost probat în aplicație.

Etapele de mai jos revizuiesc editorul din 17.10: arborele BP rămâne vizibil/dezactivat, rândul gol înlocuiește footerul Adaugă, iar ultima etapă mută mini-fișele din stânga în dreapta. Ordonarea după identitate descrie implementarea istorică; nu înlocuiește regula curentă AGENTS de sortare după tipul real.

<a id="d001-09-etapa-3"></a>

### Bonuri OI/MF: arbore direct și conturi BP

Arborii BP/BT/BS afișează direct deținătorul și bonurile, fără nivelul lunii. BT se grupează după primitor; BS după predător, fiind document fără primitor. Gruparea folosește identitatea deținătorului, iar bonurile sunt ordonate după număr descrescător, apoi dată și ID, cu număr și valoare în nod. În BP, contul de folosință este sub contul de magazie, în aceeași coloană. Listele țin cont de TIP OIM/OIF sau MFM/MFF; conturile neclasificate (NULL) rămân eligibile conform regulilor existente. Un singur cont eligibil este selectat automat și controlul este disabled, prin mecanismul comun existent.

<a id="d001-09-etapa-16"></a>

### Bonuri MF/OI: grupare opțională după dată

Sub arborele bonurilor BP/BT/BS apare checkboxul Ordoneaza dupa Dată, bifat implicit la deschiderea paginii. Bifat: rădăcini lună în română/an, în ordine cronologică descrescătoare, cu TOTAL pe lună; bonurile copil arată tip/număr, dată și valoare și sunt sortate descrescător după dată, apoi după număr și identitate. Totalurile însumează exact valorile bonurilor neanulate; bonurile anulate rămân vizibile cu marcajul existent. Înregistrările fără dată sunt grupate separat la sfârșit.

Debifat: se reutilizează gruparea existentă pe gestionar, cu aceleași etichete și ordonare ca înainte. Comutarea reface numai arborele și păstrează documentul selectat. Checkboxul este sub zona derulabilă a arborelui și urmează ciclul de viață al controalelor paginii.

<a id="d001-09-etapa-17"></a>

### Bonuri MF/OI: număr implicit separat P/T/C și produse cu disponibil pozitiv

La adăugarea BP/BT/BS, Număr este completat cu următorul număr din seria proprie tipului de bon. Propunerea ia maximul numerelor existente și al contorului acelei serii, inclusiv documentele anulate. Numărul este verificat la părăsirea câmpului și la salvare; același număr poate exista pe tipuri diferite de bon, dar nu pe două bonuri ale aceluiași tip în aceeași gestiune. Propunerea este doar citire; contorul avansează în tranzacția salvării, sub blocarea contextului existentă. Numerotarea comună internă a identităților PTC se păstrează separat de numărul afișat al fiecărui tip.

Clarificare explicită a utilizatorului: la predare se filtrează stocul din magazie; la transfer/casare, stocul rămas la predător. Selectorul Produs include numai disponibil pozitiv la data bonului. La modificare se exclude mișcarea proprie a bonului (BC-ul asociat pentru BP, documentul TC în registrul deținătorului). Se scad și cantitățile din celelalte rânduri ale ciornei, păstrând disponibilul rândului editat. Produsele/loturile cu 0 nu mai apar și nu generează categorii goale în arbore. BP preîncarcă stocul la deschiderea editorului și îl reîncarcă la schimbarea datei; verificările tranzacționale de stoc și cronologie rămân obligatorii la salvare.

Logica UI a numerotării a fost separată din docs.js într-un modul dedicat, păstrând și comportamentul NIR/FCS, pentru dimensiunea și responsabilitățile modulului comun de documente.

<a id="d001-09-etapa-18"></a>

### Bonuri PTC: editare ca la BC și mini-fișe TC

La adăugare/modificare BP/BT/BS, bara de acțiuni are Renunță în stânga, mesajele în centru și Salvează bonul în dreapta, fără previzualizare. Grilele păstrează permanent rândul gol de adăugare și nu mai afișează butonul Adaugă rând. Rândul gol nu intră în documentul trimis la salvare; schimbarea predătorului nu mai cere confirmare pentru un simplu rând gol.

La BP, arborele rămâne vizibil și este dezactivat în timpul editării, inclusiv adăugarea și ordonarea. În PTC, arborele și checkboxul folosesc aceleași rânduri de layout ca grila și bara de acțiuni: arborele se termină la baza zonei grilei, iar checkboxul rămâne în stânga, centrat pe verticală în rândul de jos.

La BT/BS dispar din grilă coloanele Cont în folosință și Lot proveniență, iar disponibilul se numește Disponibil. Identitatea lotului și contul sunt păstrate în ciornă și în validarea de stoc. În editare, coloana din stânga afișează fișa predătorului, fișa primitorului numai la BT și mișcările produsului selectat. Istoricul include BP/BT/BS cu număr, dată și valoare, iar pentru produs și cantitatea și traseul predător–primitor. Fișele deținătorilor semnează negativ ieșirile și pozitiv intrările.

Citirea fișelor este exclusiv read-only, delimitată de subunitate, gestiune, exercițiu și data bonului; bonurile anulate și ciorna nesalvată nu sunt incluse. Cererile depășite sunt ignorate la schimbarea selecției/contextului. Editorul sub-numărului NIR/FCS a fost separat într-un modul dedicat pentru a păstra dimensiunea modulului comun de documente.

<a id="d001-09-etapa-19"></a>

### Bon transfer: Primitor și Predător diferiți

Selectoarele bonului de transfer exclud reciproc deținătorul ales în celălalt câmp, la fiecare căutare, atât la adăugare, cât și la modificare. Alegerea aceleiași persoane este respinsă și la schimbarea câmpului; validarea locală marchează ambele câmpuri și blochează salvarea unei asemenea ciorne. Validarea existentă pe server continuă să respingă transferurile către sine. Adaptorul comun de câmpuri acceptă opțiuni calculate din starea curentă; listele statice își păstrează comportamentul.

<a id="d001-09-etapa-20"></a>

### Mini-fișele transfer/casare în dreapta

Corecție de poziționare cerută de utilizator: toate mini-fișele BT/BS sunt în coloana din dreapta, cu formularul și grila în stânga. Se păstrează ordinea predător, primitor (numai BT), mișcări produs. Ordinea DOM urmează formularul, apoi panourile. Pe ecrane înguste se păstrează așezarea sub formular.

### Fișiere pe etapă

Căi istorice; numele fără prefix continuă directorul precedent.

| Etapă | Fișiere |
|---|---|
| 3 | PYTHON/static/js/vercon/oi-ui.js; PYTHON/static/css/vercon-oi.css; PYTHON/routes/vercon/oi.py. |
| 16 | PYTHON/static/js/vercon/oi-tree-order.js, docs.js; PYTHON/static/css/vercon-oi.css. |
| 17 | PYTHON/routes/vercon/oi_number.py, oi.py, doc_routes.py; PYTHON/static/js/vercon/document-number.js, oi-product-choices.js, oi-ui.js, docs.js. |
| 18 | PYTHON/static/js/vercon/docs.js, oi-context.js, oi-tree-order.js, document-subnumber.js; PYTHON/static/css/vercon-oi.css; PYTHON/routes/vercon/oi_context.py, doc_routes.py. |
| 19 | PYTHON/static/js/vercon/docs.js, ui.js. |
| 20 | PYTHON/static/css/vercon-oi.css, PYTHON/static/js/vercon/oi-context.js. |

## Diagnosticul listelor BP/BT/BS — 18.10.2026 <a id="d001-12-diagnostic-oi"></a>

Preluare D001-12. Ambele corecții sunt în surse, nerulate: numai design pe baza logului/sursei, fără teste/probe automate/runtime, browser/HTTP ori MariaDB/SQL executat de asistent. Fără schemă/date/migrator/deploy.

- Logul BS indica MariaDB 1064 pentru aliasul neîncadrat lines. Interogarea comună BP/BT/BS folosește line_count, răspunsul API păstrând lines.
- Diagnosticul SQL al utilizatorului în 000_DEMO confirma charset/collation binary pentru CONCAT la BP/BS. Lista citește separat MAX(ID), SUM(Version) și construiește textul Python în formatul _rev, id-maxim–sumă-versiuni cu separator ASCII '-'; cheia API rămâne version, fără rezultat binar CONCAT. Guardul VERCON emite success numai după pregătirea JSON, evitând succesul înaintea unei erori de serializare.

Fișiere istorice (numele fără prefix continuă directorul precedent):

| Etapă generală | Fișiere |
|---|---|
| 9 | PYTHON/routes/vercon/oi.py; implementare_vercon_v2.md. |
| 10 | PYTHON/routes/vercon/oi.py, PYTHON/routes/vercon/__init__.py; implementare_vercon_v2.md. |
