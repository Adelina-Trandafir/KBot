# SLICE-ADE6 — Cataloage, liste și editori

Repere comune pentru etapele indicate: 09.10.2026 — ADE6-06, ADE6-07, ADE6-10, ADE6-11, ADE6-12, ADE6-13, ADE6-14, ADE6-16, ADE6-17. Fiecare are starea TESTAT, RULAT, PUBLICAT, confirmată de operator la 23.10.2026; probele și limitele inițiale rămân în secțiunile lor.

01 păstrează descrierea inițială CRUD (inclusiv delegații, eliminați ulterior din model conform planului ADE0-07). 05 are ferestrele inițiale cu editare în celule; 06 le înlocuiește cu trei liste și editori separați, păstrând Taxe editabil. 10 introduce navigarea mobilă prin apăsare lungă, înlocuită în 12 de butonul explicit; 13 adaugă marginea exterioară. 11 definește I ca inversul Activ numai la afișare; 14 și 16 corectează filtrele și lățimile. 17 aplică activarea unică la salvarea plătitorului copilului vizat; 23 elimină mesajul permanent, iar 24 adaugă contactele educatorilor și predarea absorbită. Ghidul [UTILIZARE_WEB](../../ADECHIT/UTILIZARE_WEB.md) rămâne contractul de utilizare, iar [prezența](SLICE-ADE6-02-prezenta.md), [taxele](SLICE-ADE6-15-taxe-perioade.md) și [deadlockul](SLICE-ADE6-25-deadlock-lock-row.md) au istorice distincte.

Nota comună Rezolvat din 05/06/24: *Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.* Limitele inițiale și rezultatele locale rămân în etapa lor; această confirmare nu este o probă nouă a consolidării.

- [ADE6-01](#slice-ADE6-01)
- [ADE6-05](#slice-ADE6-05)
- [ADE6-06](#slice-ADE6-06)
- [ADE6-07](#slice-ADE6-07)
- [ADE6-10](#slice-ADE6-10)
- [ADE6-11](#slice-ADE6-11)
- [ADE6-12](#slice-ADE6-12)
- [ADE6-13](#slice-ADE6-13)
- [ADE6-14](#slice-ADE6-14)
- [ADE6-16](#slice-ADE6-16)
- [ADE6-17](#slice-ADE6-17)
- [ADE6-23](#slice-ADE6-23)
- [ADE6-24](#slice-ADE6-24)

## SLICE-ADE6-01 — cataloage <a id="slice-ADE6-01"></a>

08.10.2026 — TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Grupe, copii, persoane asociate, delegați și taxe folosesc același endpoint CRUD cu validare, versiune optimistă și editare DataGrid. Transferul nu este inclus.

## SLICE-ADE6-05 — ferestre Plătitori și Taxe <a id="slice-ADE6-05"></a>

### Ce s-a schimbat și de ce

Din antet se deschid două ferestre modale. Fundalul este inert, Escape și clicul pe fundal nu închid fereastra. Editarea folosește DataGrid comun. Plătitori afișează copiii și persoanele asociate copilului selectat; se pot edita copiii și edita/adăuga persoane. Taxe permite editare și rânduri noi. Câmpurile Da/Nu sunt editabile ca text validat. «Salvează și închide» confirmă inclusiv celula încă deschisă și trimite toate modificările printr-o comandă atomică, autorizată catalog, cu versiuni și idempotency key. La eroare fereastra și editările rămân; reîncercarea aceluiași corp refolosește cheia. Fără modificări, butonul închide fără scriere. Dacă încărcarea eșuează, se permite închiderea ferestrei goale. Navigarea cu editări nesalvate cere confirmarea browserului. Activarea taxei dezactivează vechea taxă prin serviciul existent; nu rescrie IDV istoric. Ajutorul din pagină a fost actualizat cu referința ADE6-05.

### Fișiere atinse

- PYTHON/static/adechit.html
- PYTHON/static/css/adechit.css
- PYTHON/static/js/adechit/app.js și catalogs.js (nou)
- PYTHON/routes/adechit/__init__.py și service.py
- PYTHON/tests/test_adechit.py
- ADECHIT_STATUS.md și docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md

### Verificări efective

- API: 11 teste trecute (pytest, mediul virtual al proiectului), inclusiv rollback integral la conflict/validare, idempotency, drepturi și păstrarea taxei istorice.
- Sintaxa JS verificată cu Node; git diff --check pentru fișierele lucrării.
- Browser Edge izolat, localhost:5050: Escape/fundal păstrează fereastra, editarea nu scrie înainte de salvare, salvarea confirmă celula curentă, persoana nouă persistă.
- Browser: refuz HTTP 409 simulat păstrează fereastra și draftul; reîncercarea salvează. Tema întunecată și ferestrele pe telefon (390 px) au fost inspectate din capturi.
- Preview-ul Python pornit dimineață a fost repornit pentru a încărca ruta nouă; baza locală a fost păstrată. Datele din proba browser sunt fictive.
- Prima rulare pytest a eșuat la crearea directorului temporar în sandbox; reluată cu --basetemp artifacts/ade6-05-pytest, toate testele au trecut.

### Neverificat sau amânat

Nu s-au executat SQL remote, publicare, validare MariaDB sau probe Access. Nu se schimbă schema bazei. Transferurile/plecările și configurarea unității nu fac parte din această lucrare. Browserul poate închide un tab după confirmare; fereastra modală nu poate garanta păstrarea datelor după închiderea browserului. Modificările sunt locale, necomise; fișierele ADE aveau deja modificări ample necomise.

### Predare pentru server

Publicați cele șase fișiere de implementare enumerate mai sus prin AvacontPush, împreună cu lucrările ADE anterioare necesare. Nu există script SQL nou pentru ADE6-05. După restartul API, verificați deschiderea ambelor ferestre, editarea/adăugarea unui plătitor și a unei taxe, salvarea, reîncărcarea și păstrarea editărilor la conflict.

## SLICE-ADE6-06 — machete Plătitori, grupe și copii <a id="slice-ADE6-06"></a>

### Ce s-a schimbat și de ce

Plătitori urmează cele patru machete furnizate: trei liste dependente Grupe → Copii → Plătitori, fiecare cu Adaugă/Modifică, și formulare separate, uniforme, pe fundal alb. Listele permit selecție; nu permit editare în celule. PL este exclusiv checkbox pentru afișare, modificabil numai în editorul elementului: Grupă închisă la grupe, Plecat la copii, conform precizării utilizatorului. ACT este tot checkbox pentru afișare. Bifa grupei păstrează anul de închidere existent sau folosește anul curent la prima închidere; debifarea elimină închiderea. Nu există câmp suplimentar de an în formular.

Editorul grupei include numele, bifa, lista ANI și perioadele educatorilor. Editările grupei și perioadelor se salvează atomic; perioadele aceluiași educator nu se suprapun. Editorul copilului include numele, CNP, combobox comun pentru grupă, calendar pentru intrare/ieșire și bifa Plecat. Editorul plătitorului include toate câmpurile machetei, inclusiv telefon, email și Activ. CNP se validează în JS și API după algoritmul VBA: 13 cifre, limitele superioare pentru lună/zi/județ, checksum 279146358279, rest 10 → 1. CNP necompletat rămâne permis; nu reinterpretăm regulile VBA drept validare calendaristică.

Ferestrele blochează fundalul; Escape și clicul în afară nu închid editorul. Salvează închide numai după reușită. Renunță abandonează explicit editările, conform machetelor ulterioare cererii inițiale. Erorile păstrează formularul și datele; reîncercarea aceluiași corp refolosește cheia idempotentă. Pe telefon conținutul derulează, butoanele rămân vizibile. Taxe păstrează editarea în celule și salvarea atomică din ADE6-05.

Mutarea individuală a copilului scrie istoricul și modifică numai prezențele lunilor deschise. Lunile închise păstrează grupa și snapshotul. Starea Plecat necesită dreptul transfer și data ieșirii. Copiii plecați cu sold sunt preluați în grupa specială PLECATI la luna nouă; nu implementăm compensarea. Educatorii se citesc din istoricul perioadelor, inclusiv pentru situația lunară; textul legacy este fallback doar dacă nu există istoric.

### Fișiere atinse

- PYTHON/static/adechit.html, css/adechit.css, js/adechit/app.js și catalogs.js; modulele noi payers.js și cnp.js.
- Componente comune: js/dgv/datagrid.js și css/dgv.css (selecție publică și afișare checkbox opt-in), combobox-ui.js/combobox-events.js, datepicker.js. Popupurile apar în dialogul nativ; alegerea din combobox nu redeschide lista.
- PYTHON/routes/adechit/__init__.py, catalog_forms.py (nou), domain.py, repository.py, service.py, calculations.py și schema.json; PYTHON/tests/test_adechit.py.
- ADECHIT/tools/preview.py și build_schema.py. Preview-ul are tabelele locale necesare; generatorul AD_01 refuză să suprascrie metadatele AD_03.
- ADECHIT_STATUS.md, state/ADECHIT_STATUS_ADE0-ADE9.md și acest worklog.

### Verificări efective

- 16/16 teste pytest în mediul virtual al proiectului: CNP, validare, rollback atomic, conflicte, drepturi, idempotency, istoric, transfer fără modificarea lunilor închise, soldul copilului plecat și perioadele educatorilor; regresiile existente.
- Edge izolat, localhost:5050, unitate și date fictive: creare grupă/educator/copil/plătitor, telefon persistent, combobox, calendare în dialog, CNP invalid, Escape/fundal blocat, conflict 409 simulat și reîncercare, Renunță fără scriere, telefon și tema întunecată.
- Proba dedicată PL: checkboxurile ambelor liste sunt disabled; clicul selectează rândul fără schimbarea bifei; închiderea/redeschiderea grupei se salvează numai din editor; Plecat și data ieșirii copilului persistă. Formularul grupei conține numai bifa.
- Adaugă copil din Prezență folosește formularul și preia copilul salvat în luna aleasă.
- Regresie browser Taxe: deschidere și salvare/închidere. Sintaxa JS și whitespace verificate.
- Probe locale reproductibile în artifacts/ade6-06-browser.cjs, ade6-06-add-child.cjs, ade6-06-checkbox.cjs; capturi PNG în același director, inspectate vizual.

### Neverificat și amânat

Exportul local MariaDB_Schema nu conține tabele AD_. Întrebarea privind schema serverului rămâne fără răspuns. Implementarea locală folosește AD_03 aprobat în ADE0-07; nu afirmăm că baza live corespunde. Nu am accesat MariaDB/Linux și nu am schimbat sursele Access. Transferul lot, compensarea, înlocuirea endpointului legacy /transfer și paritatea integrală Access rămân în feliile lor. Importerul Python legacy rămâne pentru preview-ul vechi; migrarea AD_03 folosește ADE.Migrator. Nu există SQL nou în această intervenție.

### Predare și publicare

Lucrarea rămâne locală, împreună cu modificările preexistente; nu am creat commit și nu am publicat. Prin AvacontPush trebuie predate modulele ADECHIT Python/JS/CSS/HTML și componentele comune enumerate mai sus. Instrumentele preview/testele nu sunt runtime server. Înaintea probei pe server trebuie confirmată schema finală AD_03 (Grupe_Educator, Platitori_Istoric, Telefon, Tip și InchisaDinAn); nu se rulează AD_01 peste AD_03. Nu există script suplimentar de ordonat pentru această felie. Pe server se probează încărcarea listelor, salvarea celor trei formulare, validarea CNP, perioadele educatorilor, PL numai din editor, drepturile și refuzul conflictelor, apoi Taxe. Următoarea subfelie liberă: ADE6-07.

## SLICE-ADE6-07 — editare DGV și formulare compacte <a id="slice-ADE6-07"></a>

### Schimbări și motiv

Conform cerințelor operatorului, Enter confirmă și deschide următoarea celulă editabilă în ordinea vizibilă, inclusiv pe rândul următor; celulele indisponibile sunt sărite. Tab păstrează aceeași navigare, Shift inversează sensul. Erorile păstrează celula curentă. Pe PC (lățime peste 980px și pointer precis), clicul unic deschide editorul. Clicul pe altă celulă confirmă mai întâi editarea curentă. Pe mobil clicul simplu selectează; dublu clic/Enter rămân disponibile pentru editare. Grilele fără drepturi rămân read-only.

Celulele de dată folosesc DatePicker comun, cu introducere zz.ll.aaaa și calendarul folosit la plăți; sursa păstrează valoarea ISO. Controlul este distrus la finalul editării.

Antetele tuturor dialogurilor ADE folosesc aceleași variabile de culoare ca antetul paginii. Editorul grupei are lățime maximă 760px, Educator umple spațiul disponibil în tabel. ANI și tabelul au înălțime 250px, antete și rânduri de 28px; culorile ANI provin din tema DataGrid. + Adaugă este acțiune în footerul tabelului, confirmând celula curentă înainte de crearea educatorului. Formularele copil/plătitor au lățime maximă 460px (50% din 920px) și gap 10px (50% din 20px); perechile sunt așezate vertical pentru a păstra spațiu pentru câmpuri. Lățimea rămâne limitată la 96vw pe ecrane mici.

### Fișiere

- PYTHON/static/js/dgv/editing.js și datagrid.js; css/dgv.css.
- PYTHON/static/js/adechit/payers.js și catalogs.js; css/adechit.css; adechit.html.
- ADECHIT_STATUS.md și state/ADECHIT_STATUS_ADE0-ADE9.md.

### Verificări și limite

Nu au fost rulate teste, browser automatizat sau verificări de execuție, conform instrucțiunii explicite «NU MAI FACE TU TESTE». Probele din ADE6-06 sunt anterioare acestor modificări și nu validează ADE6-07. Operatorul verifică aspectul și navigarea. Nu s-au modificat backendul, calculele, schema, datele Access sau serverul.

### Predare

Modificările sunt locale, fără commit/publicare. Fișierele runtime enumerate se publică prin AvacontPush de către utilizator. Nu există SQL nou. Următoarea subfelie: ADE6-08.

## SLICE-ADE6-10 — Plătitori mobil <a id="slice-ADE6-10"></a>

### Schimbări

Pe mobil, dialogul Plătitori ocupă 100vw și 100dvh, fără margini/padding lateral. Antetul și footerul rămân pe loc; un singur tabel ocupă restul înălțimii. Listele nu mai sunt stivuite într-un container cu scroll. Derularea este numai în tabel. Mesajul informativ este ascuns; erorile de încărcare rămân afișabile controlat.

Navigarea este pe niveluri: Grupe, Copii, Plătitori. Clicul scurt selectează rândul, apăsarea lungă de 550ms pe grupă trece la copiii săi, cea pe copil la plătitorii săi. Deplasarea de peste 10px, anularea pointerului și ieșirea din tabel anulează gestul. Clicul generat după apăsarea lungă este consumat pentru a nu selecta accidental rândul de pe noul nivel. Butonul ↩️, în dreapta antetului, revine la nivelul anterior; din Grupe închide dialogul. Selecția este păstrată la revenire.

Footerul paginii conține numai ➕ și ✏️ pentru nivelul curent, cu etichete accesibile. Acțiunile folosesc editorii existenți și drepturile lor. Butoanele text de sub fiecare listă și Închide din footer sunt ascunse pe mobil. Pe PC rămân cele trei panouri.

Grupe afișează numai Grupa (85%) și PL (15%). Copii afișează Nume/CNP/PL în proporțiile 50%/35%/15%; Plătitori Nume/CNP/ACT în aceleași proporții. DataGrid comun are opțiunea explicită proportionalWidths: lățimile coloanelor devin ponderi din lățimea disponibilă, actualizate la resize și la apariția scrollbarului. Aceasta este activată numai pentru listele mobile. Resize-ul manual și filtrele din antetele mobile sunt ascunse; listele nu au scroll orizontal. PL/ACT rămân read-only.

### Fișiere

PYTHON/static/js/adechit/payers.js, js/dgv/datagrid.js, css/adechit.css, adechit.html (structură și ajutor), ADECHIT_STATUS.md și statusul detaliat.

### Verificare și predare

Nu au fost rulate teste sau probe de browser/vizuale, conform preferinței utilizatorului. Autorizarea anterioară pentru teste privea numai pornirea serverului/aplicației. Nu am modificat backendul, schema sau datele. Lucrarea este locală, fără commit/push. Se preia prin reîncărcarea paginii; publicarea runtime aparține utilizatorului prin AvacontPush. Nu există SQL nou. Următoarea subfelie liberă: ADE6-11.

## SLICE-ADE6-11 — coloane I <a id="slice-ADE6-11"></a>

Toate coloanele checkbox de stare din listele Grupe/Copii/Plătitori au titlul I, de la închis, pe PC și mobil. Coloanele folosesc filter:false în DataGrid comun, deci nu au buton/săgeată de filtrare și nici comandă de filtrare în meniul coloanei.

La Grupe bifa reflectă închiderea grupei, la Copii reflectă Plecat. La Platitori_sub se calculează numai pentru afișare Closed = !Boolean(Activ): activ → nebifat, inactiv → bifat. Nu se inversează valoarea stocată în Activ și nu se schimbă semantica checkboxului Activ din editor. Bifele listelor rămân read-only.

Fișiere: PYTHON/static/js/adechit/payers.js, ajutorul adechit.html și statusul ADE. Nu am rulat teste sau probe vizuale, conform cerinței utilizatorului. Fără modificări backend/schema/date, fără SQL, commit sau publicare. Fișierele statice se preiau după reîncărcare; utilizatorul publică prin AvacontPush. Următoarea subfelie: ADE6-12.

## SLICE-ADE6-12 — deschidere explicită pe mobil <a id="slice-ADE6-12"></a>

### Schimbări

Plătitori mobil păstrează pagina pe întregul ecran și scrollul numai în tabel, dar primește 8px de padding la margini. Grupe și Copii au o coloană suplimentară de 40px, cu antet gol, fără filtru/sortare și cu buton ➡️ pe fiecare rând. Butonul deschide copiii grupei sau plătitorii copilului. Apăsarea lungă pentru navigare a fost eliminată; clicul pe restul rândului rămâne selecție. Platitori_sub nu primește coloană de deschidere, conform excepției solicitate.

Lățimea acțiunii se scade numai din denumire: Grupa = 85% minus 40px, Copil = 50% minus 40px, CNP rămâne 35%, I rămâne 15%. La plătitori proporțiile rămân 50/35/15. DataGrid comun permite explicit o coloană de lățime fixă și deducerea ei dintr-o coloană proporțională, plus buton de acțiune în celulă. Opțiunile sunt folosite numai de aceste liste mobile.

Rândurile DGV din ADE (Prezență, Taxe, liste și educatorii din editor) cresc pe mobil cu exact 20%: 28px → 33,6px. Opțiunea mobileRowScale a componentei comune este activată explicit din ADE; alte aplicații nu sunt afectate. La schimbarea modului PC/mobil geometria DGV este recalculată. ANI păstrează înălțimea rândurilor aliniată cu tabelul educatorilor.

### Fișiere și predare

PYTHON/static/js/dgv/datagrid.js, css/dgv.css, js/adechit/payers.js, catalogs.js, app.js, css/adechit.css, ajutorul adechit.html și statusul ADE. Nu au fost rulate teste sau verificări vizuale, conform cerinței utilizatorului. Fără schimbări backend/date/SQL, commit sau publicare. Utilizatorul preia fișierele runtime prin AvacontPush. Următoarea subfelie liberă: ADE6-13.

## SLICE-ADE6-13 — margine exterioară pe mobil <a id="slice-ADE6-13"></a>

Clarificarea utilizatorului cere spațiu între fereastră și marginile ecranului, păstrând paddingul interior deja implementat. Dialogul Plătitori mobil are acum margin:10px și lățime/înălțime egală cu viewportul minus 20px. Se folosește dvh pentru înălțimea disponibilă, cu fallback vh. Paddingul interior de 8px se păstrează.

Fișiere: PYTHON/static/css/adechit.css, ajutorul adechit.html și statusul ADE. Nu am rulat teste sau verificări vizuale, conform preferinței utilizatorului. Modificare locală, fără backend/SQL, commit sau publicare. Fișierele statice se preiau după reîncărcare; utilizatorul publică prin AvacontPush. Următoarea subfelie: ADE6-14.

## SLICE-ADE6-14 — filtre și culori DGV <a id="slice-ADE6-14"></a>

Filtrarea coloanelor rămâne exclusiv pentru Nume și Grupa, în toate DGV ADE, în ambele moduri PC/mobil. CNP, I, Educatori, datele, valorile numerice, coloana de navigare și coloanele Taxe nu permit filtrare. Coloanele mobile nu mai suprascriu în bloc filter:false și păstrează filtrul denumirii. Componentele comune din alte aplicații nu își schimbă filtrarea.

Am eliminat suprascrierea culorilor antetului/footerului listelor Plătitori cu fundalul alb. Ele folosesc variabilele și tema DataGrid comună, la fel ca DGV principal din Prezență, inclusiv în tema întunecată. Nu există culori noi.

Fișiere: PYTHON/static/js/adechit/payers.js, catalogs.js, app.js, css/adechit.css, ajutorul adechit.html și statusul ADE. Nu am rulat teste sau verificări vizuale, conform cerinței utilizatorului. Fără modificări backend/SQL/date, commit sau publicare. Se preia după reîncărcare; utilizatorul publică prin AvacontPush. Următoarea subfelie: ADE6-15.

## SLICE-ADE6-16 — lățimi DGV fără depășire accidentală <a id="slice-ADE6-16"></a>

Utilizatorul a raportat scroll orizontal în toate cele trei liste Plătitori pe PC. În componenta comună, DGV avea width:100% plus borduri (content-box), iar lățimea disponibilă folosea offsetWidth întreg, care poate rotunji în sus la zoom.

Rootul și benzile header/footer folosesc border-box; containerul de scroll poate scădea în flex fără min-size implicit. Lățimea coloanelor este limitată la spațiul interior real, calculat din clientWidth și getBoundingClientRect minus borduri și scrollbarul existent. Dacă este anticipat un scrollbar vertical, se rezervă numai lățimea încă neocupată. Rezultatul este rotunjit în jos pentru a evita depășirile fracționare. Nu am ascuns scrollbarul orizontal ca soluție pe PC; rămâne disponibil când coloanele sunt efectiv mai late decât spațiul disponibil.

Fișiere: PYTHON/static/css/dgv.css, js/dgv/datagrid.js, ajutorul adechit.html și statusul ADE. Nu am rulat teste sau verificări vizuale, conform instrucțiunilor. Fără backend/schema/date/SQL, commit sau publicare. Fișierele statice comune se preiau după reîncărcare; utilizatorul publică prin AvacontPush. Următoarea subfelie: ADE6-17.

## SLICE-ADE6-17 — plătitor activ unic per copil <a id="slice-ADE6-17"></a>

La crearea prin catalog a unui Platitori_sub, API forțează Activ=True și dezactivează ceilalți plătitori activi cu același IDP. La salvarea unui plătitor existent activ, ceilalți plătitori ai copilului sunt dezactivați în același mod. Nu sunt afectate persoanele asociate altor copii. Un plătitor existent poate fi dezactivat explicit.

Toate scrierile sunt în tranzacția și sub lockul de unitate existente, cu rollback, versiuni și idempotency. Regula se aplică și apelurilor API și catalog-save, nu doar interfeței. Nu este o curățare globală a datelor migrate sau o modificare a importului.

Editorul unui plătitor nou arată Activ bifat și blocat; salvarea trimite True. Pentru un plătitor existent bifa rămâne editabilă. Reîncărcarea catalogului după salvare arată I nebifat pentru cel activ și bifat pentru ceilalți, conform ADE6-11.

Fișiere: PYTHON/routes/adechit/service.py, static/js/adechit/payers.js, ajutorul adechit.html și statusul ADE. Nu am rulat teste sau verificări vizuale, conform cerinței utilizatorului. Fără SQL nou, commit sau publicare. Preview-ul trebuie repornit pentru schimbarea backendului; publicarea aparține utilizatorului prin AvacontPush. Următoarea subfelie liberă: ADE6-18.

## ADE6-23 — Eliminarea mesajului introductiv din Plătitori <a id="slice-ADE6-23"></a>

Mesajul «Selectați grupa, copilul și plătitorul. Folosiți Adaugă sau Modifică.» nu mai apare la deschidere, reîncărcare sau salvare. Elementul ade-payers-message rămâne ascuns pentru fluxul normal și este folosit numai pentru erori.

Fișiere: PYTHON/static/js/adechit/payers.js, PYTHON/static/adechit.html, UTILIZARE_WEB.md și documentele de stare. Încărcarea inițială păstrează numai grupele; copiii și plătitorii se încarcă după selecție.

Nu s-au executat teste automate sau verificări funcționale/vizuale. Publicarea și proba în browser revin utilizatorului.

Următoarea subfelie liberă: ADE6-24.

## SLICE-ADE6-24 — Email și CNP opționale pentru educatori <a id="slice-ADE6-24"></a>

12.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Ultima intervenție înaintea opririi momentane a lucrului la ADECHIT solicitate de utilizator.

### Ce s-a schimbat și de ce

- Fiecare rând/perioadă din `AD_Grupe_Educator` are `EMail` și `CNP`, opționale. Editorul grupei are două coloane noi de text și lățime maximă 1120px (96vw). Datele sunt incluse în payloadul salvării atomice, cu verificarea versiunilor.
- API-ul verifică formatul emailului și validarea CNP existentă numai dacă sunt completate. JS folosește aceeași regulă CNP; CNP rămâne text. Golirea salvează NULL. Datele educatorilor nu sunt înregistrate ca identități de părinte în portal.
- Închiderea anuală preia și permite editarea contactelor; păstrează datele când înlocuiește perioadele. Modificarea numai a contactelor actualizează rândurile existente aplicabile la 1 septembrie. Contractul vechi cu nume simple rămâne acceptat și păstrează contactele educatorilor cu același nume.
- **Instrucțiune explicită:** Access NU conține coloanele; nu se așteaptă migrarea lor. `AdeSchema.GrupeEducator` are doar IDGE/IDG/Educator/DeLa/PanaLa; `AdePlan.AddEducatorRow` și writerul cu lista explicită de coloane rămân neschimbate. Câmpurile noi rămân NULL implicit la import; fără remigrare.
- DDL separat: `sql/AD_09_educatori_contact.sql`, numai în **AVACONT_SURSA**. **KBotAdmin** face sincronizarea celorlalte baze, conform cererii utilizatorului. Scriptul este repetabil pentru adăugarea coloanelor și nu modifică datele existente.
- HELP: conținutul JSON actualizat pentru catalog și închidere anuală, cu referință `slice: ADE6-24` în metadatele blocurilor; imprimarea/PDF folosesc aceeași sursă.
- Ghidul web, planul, README migratorului, indexul și stările au fost actualizate. Punctul de reluare a fost absorbit în acest jurnal și în registrul ADE6, prin D001-04. AD12 avea deja AD12-01; următorul număr principal liber este corectat la AD13.

### Fișiere atinse

- `PYTHON/routes/adechit/{schema.json,service.py,catalog_forms.py,annual.py}`.
- `PYTHON/static/js/adechit/{payers.js,annual.js}`, `PYTHON/static/css/adechit.css`, `PYTHON/static/help/adechit.json`.
- `sql/AD_09_educatori_contact.sql` (nou).
- `ADECHIT/{UTILIZARE_WEB.md,PLAN_IMPLEMENTARE.md,STARE_LA_PAUZA.md (nou)}`, `ADECHIT/ADE.Migrator/README.md`, `ADECHIT_STATUS.md`, cele două fișiere de stare.
- Acest worklog.

### Verificări efective

- Schema locală reală citită: `MariaDB_Schema/AVACONT_SURSA.sql`, tabelul `AD_Grupe_Educator` are Educator/DeLa/PanaLa și nu are EMail/CNP în export.
- Parse Python AST: service/catalog_forms/annual fără erori; JSON schema/HELP valide.
- `node --check` pentru payers.js și annual.js fără erori; `git diff --check` curat pentru fișierele intervenției.
- Citire punctuală a migratorului: coloanele de inserare nu includ contactele.
- Nu s-au rulat teste automate, probe UI sau conectări MariaDB; testarea funcțională rămâne la utilizator, conform preferinței proiectului. Sintaxa nu validează fluxul.

### Neverificat / amânat

- DDL nerulat, KBotAdmin/publicare/restart neexecutate din chat.
- Salvare/golire, erori de validare, conflict, grila PC/mobil și planul anual neprobate.
- HELP/imprimare/PDF nu au fost deschise pentru verificare vizuală.
- Acceptarea finală, paritatea Access și probele server rămân neconfirmate; Acestea sunt limitele predării din 12.10.2026, închise prin confirmarea de mai sus.

Următoarea subfelie de catalog: **ADE6-25**.

### Predarea absorbită din punctul de reluare — D001-04

Reper inițial: 12.10.2026, ADE6-24/25; acceptare finală confirmată de operator la 23.10.2026, inclusiv DDL, sincronizare, publicare, paritate financiară/M10 pe faze Access și ADE9-02/03 (proba completă și predarea). Nu este o probă nouă efectuată în D001-04. Emailul chitanțelor rămâne inactiv și este distinct de emailurile de autentificare ale portalului. Compensarea prin consiliu, SIIR, mesajele de business, exporturile condiționate și rapoartele consolidate nu aveau puncte deschise conform aceleiași confirmări; aceasta nu inventează detalii de implementare sau rezultate individuale.

Ordinea predării: AD_09 numai în AVACONT_SURSA → sincronizarea celorlalte baze prin KBotAdmin → publicarea fișierelor din lista de mai sus și restart backend → probe educator nou/existent cu câmpuri goale, completare/golire, email/CNP invalid, reîncărcare și închidere anuală cu păstrarea contactelor. Fără remigrare; schimbarea numai a contactelor nu înlocuiește perioadele. Pentru deadlock se publică separat cele două module și HELP din [ADE6-25](SLICE-ADE6-25-deadlock-lock-row.md); proba consemnată în punctul de reluare era «Plătitori → Modifică grupa». Cauza exactă rămâne calificată în jurnalul tehnic.

Punctul de reluare acoperea grupele/copiii/părinții/taxele/prezența (ADE6-24), încasările/anulările/restituirile și listare/PDF, ciclul lunar/anual, opt rapoarte și dispoziția de plată (ADE9-01), subunitățile/migrarea, portalul AD11-01…05 și ajutorul AD12-01. Toate aveau confirmarea de mai sus. Numerotarea AD10/AD11 a fost reutilizată istoric pentru obiective distincte: identificarea se face și după titlu, fără renumerotare. Numerele propuse atunci (ADE6-26, AD11-06, AD12-02, AD13) sunt istorice; continuarea folosește registrul actual.

## Completări din registru — confirmarea din 23.10.2026

M06: transferul lot, compensarea prin consiliu și înlocuirea vechiului endpoint `/transfer` sunt consemnate implementate, TESTAT, RULAT, PUBLICAT. Aceasta închide limitele inițiale din 06; nu adaugă detalii de implementare sau rezultate individuale de probă. Reducerile excluse și pagina de configurare a datelor unității nu se introduc.
