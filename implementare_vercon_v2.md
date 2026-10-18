# Implementare VERCON v2

## Actualizare 14.10.2026 — macheta NIR: antet obligatoriu înainte de grid, meniu «Factură Cumpărare (NIR)», `dropDownStyle`

**Fișiere:** `js/vercon/docs.js`, `js/vercon/ui.js`, `js/vercon/app.js`, `js/dgv/datagrid.js`, `js/dgv/editing.js`, `js/components/combobox/combobox.js` + `combobox-ui.js`, `css/dgv.css`, `css/combobox.css`, `css/vercon-receptie.css`, `css/vercon-documente-base.css`, `routes/vercon/capabilities.py`.
- **Stoc inițial:** bifa ascunde complet «Furnizor» și «Număr factură» (și le golește); la debifare reapar. Celula Furnizor păstrează doar bifa.
- **Aliniere celule grid (fără antet):** `#` și `UM` centrate (opțiune nouă de coloană `align: 'center'` → clasa `is-center`); Preț fără TVA, TVA %, Valoare fără TVA, TVA (lei), Total cu TVA la dreapta (NIR).
- **Grid blocat până se completează antetul:** `editGuard` / `onEditBlocked` în DataGrid; `headerLock()` în `docs.js` cere toate câmpurile obligatorii (la NIR și «Comisie», dacă gestiunea are comisii); click în grid sau «+ Adaugă rând» arată mesajul cu câmpurile lipsă și marchează câmpurile goale. Regula e pe client; serverul nu cere comisia.
- **Cont unic:** dacă gestiunea are un singur cont (alimente), «Categorie / Cont» (și «Cont de intrare» / «Cont primitor») e preselectat și dezactivat.
- **Meniu NIR:** butonul se numește «Factură Cumpărare (NIR)» (server) și deschide un meniu popup: Adăugare (macheta nouă direct: pagina se montează cu `startAdd`, fără să deseneze întâi ecranul de consultare; dacă pagina e deja deschisă, `startAdd()`), Editare / Modificare (comportamentul de până acum), separator, Listare (fără acțiune încă). Serverul Flask trebuie repornit pentru noua etichetă.
- **Buton «+ Adaugă rând»:** acțiunea din subsolul gridului stă la stânga, cu text la ~10px de margine (`has-action` în `dgv.css`; celula acțiunii nu mai moștenește alinierea centrată a primei coloane, `#`). «Comisie» e și ea `dropDownStyle: 'simple'`.
- **Grid produse NIR:** rând nou gol mereu vizibil (`newRowAlwaysVisible`) și Esc în două trepte (celulă, apoi rând nevalidat) — vezi `VERCON/CONTROALE_CUSTOM_THREAD.md`, DataGrid. Liniile goale nu se salvează.
- **Tastatura în antet (toate documentele):** Tab / Shift+Tab și Enter merg de la un control editabil la următorul, ca într-un formular Access; se sar bifele, butoanele (săgeți de listă, calendar) și controalele dezactivate sau invizibile; după ultimul control, Enter / Tab pornește editarea în primul rând al gridului (`onHeaderKey` în `docs.js`). Combobox-ul cu scriere păstrează primul rezultat la Tab. Nu se poate deschide încă o listă simplă de la tastatură.
- **Grid produse NIR, părăsirea rândului:** rândul editat e judecat la plecare; dacă e incomplet apare «Rând incomplet» cu «Continuă editarea» (implicit) / «Anulează rândul»; la ieșirea focusului din grid niciun editor nu rămâne deschis (vezi `CONTROALE_CUSTOM_THREAD.md`, DataGrid).
- **Cotă TVA din antet (NIR):** se dezactivează (cu explicație în tooltip) de îndată ce există cel puțin un rând completat în grid (rândul nou gol nu contează) și se reactivează dacă rândurile dispar (`syncVatLock` în `docs.js`).
- **Editorul de documente ca fereastră modală:** adăugarea / modificarea se face într-un `ModalFrame` (dimensiune fixă, fundal blurat, arborele rămâne în spate); mărime de încercat din consolă doar pe localhost: `kbotDebug.modalSize(w, h)` (vezi `CONTROALE_CUSTOM_THREAD.md`). Nevăzut pe ecran.
- **Mod de test (o singură sursă de adevăr):** comutator pe server (`/api/test-mode`, `TEST_MODE` în `config.py` de pe server, implicit pornit) citit de `utils/debug-tools.js`; oprit = consola tace (log/info/debug) și `kbotDebug` nu există; event-bus, instances-registry și listener-tracker îl urmăresc. Cere repornirea serviciului Flask. Nevăzut pe ecran.
- **Tipografie, o singură sursă:** `PYTHON/static/css/typography.css` (baza `--fs-base` = 14px; trepte `--fs-2xs … --fs-2xl` = baza ± N; roluri `--fs-control` / `--fs-label` / `--fs-table` / `--fs-hint`; familii `--font-ui` / `--font-heading` / `--font-mono`), încărcat primul pe toate paginile aplicației web (nu și site-ul de prezentare). 167 de valori absolute din `static/css` migrate (lista completă: `VERCON/TIPOGRAFIE_MIGRARE.md`); documentele de tipărire și valorile `em` rămân neschimbate. Încercare live (doar mod de test): `kbotDebug.fontBase(px)`, `fontFamily(...)`, `fontReset()`. Nevăzut pe ecran: mărimile apropiate au fost normalizate la cea mai apropiată treaptă (unele controale s-au schimbat cu 1–2px, iar reducerile responsive mici pot coincide acum cu mărimea de bază).
- **Arbori și meniu:** arborele ia `--fs-base` și nu mai ascultă bold / italic / culori fixe (se configurează per arbore prin `--tv-*`, vezi `CONTROALE_CUSTOM_THREAD.md`); meniul de operații din VERCON are `--fs-xl`. Nevăzut pe ecran.
- **Control de meniu (`Menu`):** bara și popup-ul într-un singur control (`js/components/menu/menu.js`, `css/menu.css`); bara de operații din VERCON și toate popup-urile (`openMenu`, inclusiv `iconRight`, `checkable`, `below`, `up`) îl folosesc; `adechit/menu.js` re-exportă. Rămân de migrat: meniul propriu din ADECHIT, `portal/menu.js`, meniul grid-ului. Nevăzut pe ecran.
- **Combobox, opțiuni noi:** `dropDownOnEnter`, `maxResults` (expus), `dropDownMinWidth` — vezi `VERCON/CONTROALE_CUSTOM_THREAD.md`.
- **Combobox `dropDownStyle`:** `'search'` | `'simple'` (vezi `VERCON/CONTROALE_CUSTOM_THREAD.md`, secțiunea Combobox); folosit la «Cotă TVA».
- **Verificare:** doar verificare de sintaxă (`node --check`); nimic nu a fost deschis în browser.

## Actualizare 13.10.2026 — TextBox și CheckBox comune; arborele NU se mai reconstruiește la click pe un document

**Controale noi** (documentate în `VERCON/CONTROALE_CUSTOM_THREAD.md`, secțiunile TextBox și CheckBox):
- `js/components/textbox/textbox.js` + `css/textbox.css`: câmp de text cu comutatoare (`type` text/int/decimal/money/search, `required`, `readOnly`, `maxLength`, `min`, `textAlign`, `casing`, `selectOnFocus`, `clearable`, `errorColor`, `placeholder`, `name`, `id`, `ariaLabel`) și callback-uri `onInput` / `onCommit` / `onError`. **Validare:** nimic nu se judecă la tastare; `validate()` este încercarea (la salvare) și un eșec pune câmpul în starea de eroare (culoare `errorColor`); un câmp aflat în eroare se judecă din nou la pierderea focusului și iese din eroare când textul e valid. `money`: sumă pentru citire în afara focusului (`1.234,50`), cifrele exacte rămân pentru `getValue()`.
- `js/components/checkbox/checkbox.js` + `css/checkbox.css`: casetă de bifat cu etichetă (`labelSide`, `className`, `onChange`).
- `js/utils/field-format.js` (nou, partajat): `parse` / `validate` / `format` / `formatMoney`, mutate din `vercon/fields.js`, care le re-exportă (importurile VERCON nu s-au schimbat).
- Pagini: `vercon.html` și `adechit.html` încarcă noile fișiere CSS, înaintea CSS-ului aplicației.

**Migrate:** `field()` din `js/vercon/ui.js` (text/int/decimal; lista și data rămân Combobox / DatePicker; numerele rămân aliniate la stânga în formulare); `docs.js` (`showErrors` pune câmpurile cu eroare în starea de eroare: obligatoriu gol = culoare fără mesaj, ca înainte; bifa «Stoc inițial»); `reports.js` și `stock.js` (câte o casetă); `js/adechit/payers.js` (câmpurile de tip `text`; `tel` / `email` / `date` / `checkbox` neschimbate). **Nemigrate, intenționat:** editorul de celulă din DataGrid (decizie: se revede, după un test de overhead pe o listă mare), căutările din `nomenclatoare.js` / `datagrid.js`, casetele din ADECHIT și din DataGrid.

**Arbore (raportat de utilizator):** la click pe un document, `paint()` → `paintSaved()` distrugea și reconstruia tot arborele (scroll pierdut, ramuri redeschise din set). Acum arborele rămâne viu: la selecție se mută doar marcajul (`markSelected`, ui.js, fără redesenare); datele noi (după încărcare / salvare / anulare) se desenează pe loc cu `refreshTree` (păstrează ramurile deschise, selecția și scroll-ul), semnalate prin `treeDirty`. Arborele ascuns în timpul editării nu mai e distrus.

**Regula câmpurilor obligatorii (feedback utilizator):** niciun control obligatoriu nu mai e marcat roz de la început; marcajul apare doar (1) la pierderea focusului gol și (2) la evenimentul de validare ridicat la o încercare de salvare (`scope().validate()` în ui.js, apelat din `showErrors` în docs.js), la care răspund TextBox (starea de eroare), câmpurile listă / dată din `field()` și grila. Grila: `DataGrid.validate()` și `_requiredShown` / `_seen` în `dgv/editing.js`, folosite de `dgv/datagrid.js` la desenarea celulelor; o celulă obligatorie goală se marchează doar după ce a fost părăsită sau după validare. Atenție: schimbă un comportament al grilei partajate (`dgv/`); în codul de azi numai VERCON declară coloane `required` (căutat în `adechit/`, `portal/`), deci alte ecrane nu sunt afectate.

**Tooltip la eroare:** `TextBox` (opțiunea `errorTooltip`, implicit pornită) pune mesajul de eroare ca `title`; tooltip-ul existent al câmpului se înlocuiește pe durata erorii și se restaurează după. La fel câmpurile listă / dată din `field()` și celulele obligatorii goale marcate din grilă (`datagrid.js`). În `docs.js` am scos `setError('')` pentru câmpurile obligatorii goale, care ar fi șters mesajul pus de `validate()`.

**Feedback pe formular (5 puncte, 13.10.2026):**
1. **O singură culoare de eroare:** variabilele `--err-control-backcolor` / `--err-control-bordercolor` (`css/variables.css`, temă închisă în `portal-theme.css`) folosite de TextBox, câmpurile listă / dată și celulele grilei. Dispare `--vc-required-back`.
2. **Nimic scris pe ecran la eroare, doar tooltip:** `field()` nu mai creează span-ul de eroare; `error.textContent = mesaj` devine culoare + tooltip (și pentru `nomenclatoare.js`, `lista.js`, `editor.js`, care îl foloseau); mesajul de la linii e tooltip pe grilă. Linia de stare din bara de jos a rămas.
3. **Numărul NIR se verifică la părăsirea câmpului** (`onLeave` în TextBox, `leaveNumber` în docs.js), nu la tastare (debounce-ul a fost scos) și rămâne verificat la salvare. Rezultatul e starea de eroare a casetei.
4. **Sub-număr NIR (707,1):** caseta îngustă din aceeași celulă (`attachSub` în docs.js, `js/vercon/receipt-number.js`, CSS în `vercon-receptie.css`). **Server:** `documents.py` (`_sub_number`, `_sub_text`, `_numarsub_column` — o singură căutare în `information_schema` pe cerere, ținută în `ctx`; `_nir_number_taken` și `_assign_number` primesc `sub`; creare și editare scriu `NumarSub`; un sub-număr nu mută contorul), `receipt_number.py` și `doc_routes.py` (`/number?sub=`). Regula este cea decisă la 12.10.2026 (`DECIZII_DESCHISE.md`): 707,1 = 707 + 10, cel mult 2 zecimale, unic pe (Numar, NumarSub). Vizualizarea și previzualizarea arată «707,1». **Nerezolvat:** lista din arbore și rapoartele nu afișează sub-numărul (arborele nu arată numărul; rapoartele citesc `NumarSub` din `receipts.sql`, neverificat), iar ordinea documentelor după (Numar, NumarSub) nu a fost aplicată în interogările de listă. **Python modificat: serverul local trebuie repornit.**
5. **Text care nu trece validarea primește culoarea de eroare:** un câmp cu text se judecă la pierderea focusului (nu doar cele obligatorii).

**`allowCopyPaste` în TextBox** (la solicitarea utilizatorului): copierea / tăierea / lipirea / plasarea de text sunt blocate implicit; se pornește per control (`allowCopyPaste: true`; în VERCON per coloană în `field()`, în ADECHIT `payers.js` îl pornește ca să păstreze comportamentul de până acum). Tratarea lipirii din `docs.js` (`,5` lipit în numărul NIR) a rămas, cum s-a cerut; cu opțiunea oprită nu se mai ajunge la ea decât dacă un câmp o pornește. Câmpurile VERCON afectate acum: toate cele de text din `field()` (de ex. «Număr factură»).

**`mouseCursor` în TextBox** (la solicitarea utilizatorului): `ibeam` (implicit) sau `arrow`, orice altă valoare = `ibeam`, fără excepție; `setMouseCursor`. Câmpurile disabled au mereu săgeata; cele readOnly au cursorul ales (implicit de text; înainte aveau săgeata).

**Neverificat:** niciuna dintre aceste modificări nu a fost rulată în browser; doar verificare de sintaxă JS. Riscuri de aspect de urmărit la prima deschidere: (1) VERCON — înălțimea/chenarul câmpurilor de text (stilul vechi `.vc-field > input` nu mai se potrivește pe `<input>`-ul din `.tb`; reluat prin `.vc-field .tb__input`); (2) ADECHIT — chenarul/inelul de focus al câmpurilor de text din formularele de plătitori (TextBox adaugă `outline` la focus, ADECHIT are propriul stil de input); (3) mărimea fontului câmpurilor (TextBox nu setează `font`, ca să rămână ca până acum). Testul de overhead pentru DataGrid nu a fost făcut.

## Actualizare 13.10.2026 — arbore NIR încărcat la cerere, bifă «Stoc inițial», listă Combobox

**Arborele «Documente salvate» al NIR (la solicitarea utilizatorului).** La deschiderea paginii NIR se citesc numai furnizorii; lunile unui furnizor se citesc la deschiderea lui, iar recepțiile unei luni la deschiderea lunii. Fiecare ramură nedeschisă are un copil-substituent (`Se încarcă…`) ca să apară săgeata. Structura: furnizor → `Luna/An` (ex. `Ianuarie/2026`) → recepție, a cărei etichetă este data NIR și valoarea (fără număr, fără factură); anulatele rămân marcate. Ordinea furnizorilor ca înainte (cel cu recepția cea mai nouă primul). Familia storno și bonurile păstrează lista veche.

- Server: `PYTHON/routes/vercon/receipt_tree.py` (nou: `parties`, `months`, `documents`; furnizorul se identifică prin numele afișat, golul = recepții fără furnizor/stoc inițial; luna se filtrează pe interval de date, nu pe funcție aplicată coloanei) și ruta `GET /api/vercon/doc/nir/tree?level=parties|months|documents&party=&month=YYYY-MM` în `doc_routes.py`, cu același context verificat, drept `receptie` și aceeași formă a răspunsului (`can_write`, `closed`, `zec`, `scale`). `/list` rămâne neschimbat pentru celelalte familii.
- Pagină: `PYTHON/static/js/vercon/receipt-tree.js` (nou: cache pe niveluri, noduri cu id stabil după nume/lună, deduplicarea cererilor, reîncărcarea după salvare/anulare cu deschiderea ramurii documentului curent); `docs.js` (încărcarea inițială, `refresh`, `onToggle`); `ui.js` (`refreshTree`, `docNode`). Dacă o ramură nu se poate încărca, nodul se închide și eroarea apare în linia de stare; deschiderea următoare reîncearcă.
- **Control custom modificat:** `TreeView` primește opțiunea `onToggle(nodeId, isOpen)` (`components/treeview/treeview-nodes.js`, `treeview.js`); documentată în `VERCON/CONTROALE_CUSTOM_THREAD.md`, secțiunea TreeView.

**Corecții ale arborelui NIR (13.10.2026, după feedback):**
- **Bug reparat:** la sosirea datelor unei ramuri, arborele pierdea toate ramurile deschise. Cauza: `setData` → `updateResults` → `clearLocalSearch()` golește `expandedNodes` (`treeview-search.js:33`), iar `refreshTree` nu le punea la loc (`paintTree` o făcea). `refreshTree` (ui.js) salvează și restaurează acum nodurile deschise și selecția, plus poziția de scroll. Neverificat în browser după corecție.
- **Un singur furnizor deschis odată:** opțiune nouă, opțională, `exclusiveRoots` în `TreeView` (implicit oprită; pornită doar pentru arborele NIR). Fiind separată de `autoCollapse` (care nu poate închide rădăcini). După salvare, ramura documentului curent se deschide și celelalte rădăcini se închid.
- **Rândul unei recepții:** `DD.MM.YYYY • nr. factură` (la stânga) și valoarea aliniată numai ea la dreapta, fără «lei», cu mereu 2 zecimale și separator de mii (`1.234.567,89`). Câmp nou de nod în `TreeView`: `rightText` (stil în `css/treeview/treeview_inline.css`; tema închisă în `portal-theme.css`). Formatorul `formatMoney` (`fields.js`) rotunjește pe șirul zecimal (nu pe numere în virgulă mobilă), cu rotunjire în sus la jumătate; verificat pe 14 valori de probă, în afara aplicației. Alte afișări de sume din VERCON nu au fost schimbate.

**Bifa «Stoc inițial (fără furnizor)»** stă pe linia etichetei «Furnizor *», aliniată la dreapta cu combo-ul (`vercon-receptie.css`: scoasă suprascrierea `position: static` de sub 1200px, care o muta sub combo; textul se scurtează cu «…» când nu încape). Verificat pe ecran înainte de modificarea arborelui.

**Combobox în VERCON:** o opțiune lungă are liniile apropiate (`vercon-controls.css`: `line-height: 1.15`, `max-height: none`, numai `.portal.vc-page`), iar când textul ocupă mai mult de un rând, paddingul vertical scade cu 2px sus și jos, minim 1px (6px; 4px sub 400px lățime). **Control custom modificat:** `components/combobox/combobox-ui.js` primește `markMultilineOptions` (clasa `is-multiline`, apelată din `show()` și `renderResults`); stilul e numai în VERCON. Nu s-a verificat pe ecran. Documentat în `CONTROALE_CUSTOM_THREAD.md`, secțiunea Combobox.

**Server local:** serverul pe 5053 rula cod Python mai vechi decât `receipt_choices.py` și provoca eroarea «Serverul nu a trimis lista produselor»; repornit cu `Stop-Localhost.ps1` / `Start-Localhost.ps1`. `Start-Localhost.ps1` nu pornește sub Windows PowerShell 5.1 fără `Add-Type -AssemblyName System.Net.Http` înainte (scriptul nu a fost modificat).

Verificare: sintaxă JS (cele 5 fișiere) și Python (`py_compile`) trecute. **Arborele nou nu a fost rulat** (nici în browser, nici cu interogări către 000_DEMO); necesită repornirea serverului local, fiindcă s-a modificat Python. Nicio schemă modificată, fără operații de scriere în baza de date.

## Actualizare 13.10.2026 — eroare la încărcarea produselor NIR

Utilizatorul a raportat `Cannot read properties of undefined (reading 'filter')` în `productOptions` și a precizat că a publicat modificările și repornit serverul. Ipoteza anterioară a asistentului privind un server nerepornit nu este susținută și a fost retrasă. În sursele locale, `documents.lookups(..., 'nir')` include deja `products`; cauza diferenței față de răspunsul serverului nu a fost stabilită prin examinarea surselor. Nu s-a inspectat sau executat aplicația de pe server.

Corecții scrise: `PYTHON/static/js/vercon/docs.js` folosește un acces protejat comun la catalog și validează răspunsul înainte de montarea documentelor; un răspuns fără catalog este refuzat explicit, fără a prezenta o listă goală ca rezultat valid. Consola înregistrează ruta și numele proprietăților primite, fără valori de produs/furnizor. `PYTHON/routes/vercon/doc_routes.py` garantează lista pentru NIR la nivelul rutei: dacă modulul nu o furnizează, o citește direct din `VC_Produse` prin `receipt_choices.products`, cu subunitatea și gestiunea verificate. Nu se revine la selectarea variantelor de preț.

Stare: **CORECȚII_LOCALE_SCRISE, CAUZĂ_PE_SERVER_NECONFIRMATĂ**. Fără teste, porniri/reporniri, cereri către server sau operații SQL, conform cererii utilizatorului. Nu se declară problema de pe server rezolvată prin execuție.

## Actualizare 13.10.2026 — feedback recepție: fundal, selecții și ciornă

Pagina și headerul albastru ocupă întreaga fereastră, inclusiv la rezoluții mari. Limitarea de 1440px se aplică numai documentului/spațiului de lucru; limitarea verticală de 1080px se aplică numai acestui spațiu. În jur rămâne fundalul bleu existent. Antetul recepției are un bloc centrat de maximum 1076px (patru câmpuri de 260px și trei intervale de 12px), cu etichete/câmpuri aliniate la stânga. Fonturile comboboxurilor VERCON și ale opțiunilor lor sunt mărite cu 2pt față de dimensiunile existente, inclusiv regula desktop din portal.

Selecțiile NIR citesc produsele active direct din `VC_Produse`, fără duplicare după variante/prețuri și fără preț în etichetă. NIR trimite acum `product` pe fiecare linie; serverul verifică produsul/contul și rezolvă varianta după prețul cu TVA calculat la salvare, reutilizând o variantă activă potrivită sau inserând datele unei variante noi în tranzacția documentului. Nu suprascrie prețurile altor variante. Contractul vechi cu `variant` rămâne acceptat de server; familiile de ieșire/surse își păstrează selecțiile proprii. MF folosește de asemenea lista directă a produselor, păstrând scrierea registrului existentă.

Furnizorii sunt filtrați în SQL: numai cei vizibili cu cod fiscal nevid; grupare după codul fiscal curățat de spații exterioare și numele aferent primului `IdPartener` din fiecare grup (echivalent determinist pentru FIRST). Salvarea unei recepții obișnuite respinge și un furnizor fără cod fiscal trimis în afara listei.

Câmpurile obligatorii goale din formular și celulele grilei primesc fundal roz încă de la deschiderea ciornei; culoarea revine după completare/selecție. Mesajele pentru lipsa valorilor obligatorii nu mai adaugă rânduri sub câmpuri ori sub grilă. Erorile privind valori completate, dar incorecte, rămân explicite. În stoc inițial, furnizorul și numărul facturii nu sunt marcate obligatorii. Grila NIR permite trecerea directă între celule la click/Tab/Enter și păstrează valorile nefinalizate în ciornă; validarea strictă se execută la salvare/previzualizare. Opțiunea de editare incompletă este explicită și activată numai pentru NIR, fără schimbarea regulilor altor aplicații.

Fișiere: `PYTHON/static/css/vercon-workspace.css`, `vercon-receptie.css`, `vercon-controls.css`; `PYTHON/static/js/vercon/docs.js`, `ui.js`; `PYTHON/static/js/dgv/datagrid.js`, `editing.js`; `PYTHON/routes/vercon/documents.py`, `receipt_choices.py`. Stare: **IMPLEMENTAT_LOCAL, NERULAT**. La cererea explicită a utilizatorului, nu s-au rulat teste, verificări în aplicație sau SQL și nu s-a pornit/repornit/oprit niciun proces. Fără modificări de schemă. Modificările Python necesită repornirea manuală a serverului de către utilizator.

## Actualizare 13.10.2026 — corecții de dimensiuni și încadrare în fereastră

Cerere: cele cinci capturi de feedback pentru recepție; săgețile desenate indicau mărirea câmpurilor, nu butoane de copiere. Eliminat butoanele introduse greșit și textele informative de sub câmpuri; explicațiile rămân în tooltip, iar erorile reale rămân vizibile. Antet cu lățime egală de 260px pentru toate cele patru coloane, distanță orizontală de 12px și verticală de 8px, fără rânduri minime de 70px. Coloana Cantitate NIR folosește ponderea 64 în loc de 128; macheta grilei are cheie nouă pentru ca vechile lățimi salvate să nu prevaleze.

Pagina este limitată la 1440px. Limita de 2048px citită din ADECHIT a fost respinsă explicit de utilizator și înlocuită. Plafonul vertical adoptat este 1080px. Sub aceste limite folosește spațiul disponibil al ferestrei; peste ele este centrată pe ambele axe, cu fundal alb exterior. Headerul, navigarea și paddingurile sunt incluse în înălțime. Documentul și tabelul folosesc înălțimea rămasă prin flex/grid, înlocuind înălțimea NIR impusă prin clamp. Scrollul documentului HTML este eliminat; tabelele și panourile își gestionează conținutul în interior. Adaptate înălțimile listei de alimente și ale paginii de rapoarte la același spațiu disponibil.

Fișiere: `PYTHON/static/js/vercon/docs.js`, `PYTHON/static/css/vercon-receptie.css`, `PYTHON/static/css/vercon-workspace.css`, `PYTHON/static/css/vercon.css`. Stare: **IMPLEMENTAT_LOCAL, NERULAT**. Au fost citite sursele și captura de proiectare; fără teste, build, verificări în browser, interogări sau modificări MariaDB.

## Actualizare 13.10.2026 — remedierea accesului localhost

Probleme identificate: pagina launcherului SSH nu avea marcajul separat cerut pentru evitarea redirecționării client spre loginul portalului; `preview.py`, comanda folosită de utilizator, continua să pornească demonstrația cu date fictive. Mesajul inițial de pornire a fost insuficient pentru confirmarea accesului real. Sonda PowerShell `Invoke-RestMethod localhost` nu ajungea la server în acest mediu.

Corecții: `PYTHON/static/js/vercon/gate.js` recunoaște marcajul separat `data-vc-local-ssh`; `VERCON/tools/local_ssh.py` îl adaugă numai în pagina locală, păstrând formularele reale și autentificarea locală prin cookie. Expune o stare locală cu identitatea launcherului și baza fixă. Directorul configurărilor de raport este aplicat explicit și când modulele au fost deja importate. `VERCON/tools/preview.py` pornește implicit `local_ssh`; demonstrația fără MariaDB cere `--fixtures`. `Start-Localhost.ps1` raportează succesul numai după răspunsul launcherului corect, prin HttpClient fără proxy pe `127.0.0.1`. Completate `VERCON/tools/README.md` și `LOCALHOST.md`.

Stare actuală: **SERVER_LOCAL_PORNIT, CITIRE_DATE_CONFIRMATĂ_PRIN_API**. La diagnosticarea problemei au fost folosite exclusiv cereri de citire: pagina locală poartă marcajul corect, `/api/portal/me` răspunde `000_DEMO`, contextul real și listele NIR răspund cu succes. Pentru 2026 sunt 509 recepții în Alimente și 46 în Materiale. Nu s-au salvat/anulat documente și nu s-au modificat scheme. Nu au fost rulate teste automate; inspecția vizuală automată a browserului a fost blocată de politica instrumentului. Nu se afirmă verificarea vizuală a formularelor sau verificarea tuturor exporturilor.

## Actualizare 13.10.2026 — Recepție nouă și localhost cu 000_DEMO

Cerere: `VERCON/CAPTURI_SCHIMBARI_V1/Receptie_noua_modificari.png`, apoi posibilitatea testării manuale a tuturor modificărilor curente pe localhost. Utilizatorul a ales explicit baza MariaDB existentă `000_DEMO` prin SSH, cu configurația folosită de KBotAdmin.

### Recepție nouă

Fișiere: `PYTHON/static/js/vercon/docs.js`, `PYTHON/static/js/vercon/ui.js`, `PYTHON/static/css/vercon-receptie.css`, `PYTHON/static/css/vercon.css`, `PYTHON/routes/vercon/receipt_number.py`, `PYTHON/routes/vercon/doc_routes.py`. Antet în patru coloane cu câmpuri de aceeași lățime: furnizor/cont/comisie, factură, NIR, valoare/cotă TVA. Săgeți pentru copierea numărului și datei facturii spre NIR. Număr NIR propus automat la adăugare și verificat la introducere; endpointul propunerii execută doar SELECT, nu rezervă numere. Salvarea verifică din nou numărul în tranzacția existentă. Cotă TVA în antet pentru toate liniile ciornei și ca valoare implicită la rândurile noi; fiecare linie păstrează și selectorul propriu. Cele cinci coloane valorice/TVA au lățimi proporționale egale. Grilă mai înaltă și zona de tabel/acțiuni limitată la 1450px.

Controale comune: `PYTHON/static/js/components/combobox/combobox-ui.js`, `combobox-events.js`, `PYTHON/static/js/dgv/editing.js`: opțiune explicită `showArrow` pentru liste căutabile; activată în recepție. Stilurile sunt numai în CSS. `PYTHON/routes/vercon/__init__.py`: CSP permite resursele blob necesare imaginilor PDF, workerului și iframe-ului de tipărire.

### Localhost real prin SSH

Fișiere noi: `VERCON/tools/local_ssh.py`, `Start-Localhost.ps1`, `Stop-Localhost.ps1`, `LOCALHOST.md`, `.gitignore`; completat `VERCON/tools/README.md`. Server local `http://localhost:5053/vercon`, legat exclusiv de `000_DEMO`. SSH folosește cheia/configurația existentă a proprietarului, citită din `_Admin/admin_settings.json`; conexiunea deja deschisă în KBotAdmin nu este preluată. Credencialele MariaDB sunt citite din configurația serverului și păstrate numai în memoria procesului. Conectorul permite numai `000_DEMO`, cu citire calificată a lățimilor Excel existente din `AVACONT_COMUN`. Salvarea/anularea documentelor produce modificări reale în datele `000_DEMO`. Nu se modifică scheme, nu se publică fișiere și nu se repornește serviciul public.

Autentificarea locală se bazează pe accesul SSH al administratorului, cu toate operațiile VERCON; nu reproduce rolurile și autentificarea portalului. Actor audit implicit `localhost-ssh@local.test`, configurabil prin `-Actor`. Folosește blocarea gestiunii SQL reală. Machetele personalizate ale rapoartelor locale au director separat `.local/report-config`; implicit folosește variantele noi din cod. HTML/CSS/JS se recitesc la reload; modificările Python cer restart.

Stare: **IMPLEMENTAT_LOCAL, SERVER_LOCAL_PORNIT**. Pornirea a reușit: configurația SSH citită, tunel deschis, conexiune MariaDB și citire de pornire din `000_DEMO.VC_Subunitati` reușite; Flask ascultă pe `127.0.0.1:5053`. Mediul local conține Flask, mysql-connector, ReportLab și openpyxl. Fila localhost a fost solicitată în Codex. Nu au fost rulate teste automate, build, operații de salvare sau verificări funcționale în browser. Interfața și exporturile rămân de testat manual de utilizator. Captura originală a rămas nemodificată.

## Actualizare 13.10.2026 — corecții CAPTURI_SCHIMBARI_V1

Scop aprobat: toate marcajele din cele 11 capturi, cu precizarea utilizatorului că pagina Rapoarte preia structura Access și păstrează stilurile VERCON. Implementare locală scrisă; aplicația, exporturile, tipărirea și selecțiile nu au fost executate sau verificate. Fără teste, build, browser, interogări SQL ori modificări de schemă, conform AGENTS.md. Capturile sursă au rămas nemodificate.

### Context și navigare

Fișiere: `PYTHON/static/vercon.html`, `PYTHON/static/js/vercon/app.js`, `PYTHON/static/css/vercon-layout.css`, `PYTHON/static/css/vercon-responsive.css`. Instituția, gestiunea și anul sunt în header. Eliminate căutarea globală, schimbarea unității, tipul duplicat, indicatorul Consultare și bara cu context repetat; fila Stocuri este ascunsă. Etichetele gestiunilor nu repetă tipul în paranteze. Ocuparea gestiunii, drepturile și protecția ciornelor rămân active pe fluxurile existente.

### NIR, BC, storno și retur

Fișiere: `PYTHON/static/js/vercon/docs.js`, `PYTHON/static/js/vercon/ui.js`, `PYTHON/static/css/vercon-documente-layout.css`, `PYTHON/static/css/vercon-documente-panels.css`, `PYTHON/static/css/vercon-integration.css`. Antete compacte, coloane proporționale în tabel, fără spațiul gol din dreapta; prețul și cota TVA NIR sunt îngustate. Panoul Produse disponibile BC a fost eliminat; alegerea produsului și disponibilul pe data bonului rămân în grilă. Arborii salvărilor pornesc restrânși la rădăcină, fără căutare, și își păstrează ramurile deschise când se schimbă documentul. Înălțimea lor urmărește panoul principal.

Totalul grilei este suma liniilor. O valoare istorică diferită din antet se afișează separat, fără rescrierea documentului. Numărul salvat este inclus explicit în antetul pentru previzualizare; cota TVA este afișată ca procent.

### Lista de alimente

Fișiere: `PYTHON/static/js/vercon/lista.js`, `PYTHON/static/css/vercon-lista.css`, `PYTHON/static/css/vercon-responsive.css`. Antet vertical în prima coloană; bonurile asociate sunt sus, în coloanele următoare. Panourile meselor sunt mai înalte. Coloana vizibilă IdFel a fost eliminată; Tip se afișează ca Fel de mâncare, iar Explicații rămâne editabilă. IdFel existent rămâne în date și payload; rândurile noi pot avea IdFel nul, conform contractului existent. Grilele folosesc lățimea disponibilă fără scroll orizontal. Arborele listelor are același comportament restrâns, fără căutare.

### Previzualizare și acțiuni PDF

Fișiere: `PYTHON/static/js/vercon/document-report.js`, `PYTHON/static/js/vercon/pdf-preview.js`, `PYTHON/static/js/vercon/ui.js`, `PYTHON/static/js/vercon/editor.js`, `PYTHON/static/js/portal/pdfview.js`, `PYTHON/static/css/vercon-preview.css`, plus apelurile din docs/lista. Documentele salvate Materiale/Alimente folosesc direct rapoartele oficiale și snapshotul serverului. Ciornele, documentele anulate și familiile amânate folosesc PDF-ul local; un cititor fără dreptul separat de raport păstrează previzualizarea datelor deja încărcate. Fereastra intermediară a documentului nu mai apare. Dialogul PDF ocupă spațiul disponibil și include descărcare, tipărire/alegere imprimantă prin dialogul browserului, zoom, potrivirea lățimii și navigarea paginilor. Ajutorul și istoricul păstrează prezentarea lor tabelară.

### Pagina Rapoarte și selecții multiple

Fișiere: `PYTHON/static/js/vercon/reports.js`, `PYTHON/static/js/vercon/report-selection.js`, `PYTHON/static/js/vercon/report-result.js`, `PYTHON/static/css/vercon-rapoarte.css`, `PYTHON/routes/vercon/report_routes.py`, `PYTHON/routes/vercon/report_data.py`, `PYTHON/routes/vercon/report_service.py`. Perioada și opțiunile sunt în stânga, tipurile grupate Produse/Intrări/Ieșiri în centru, datele cu bife în dreapta. Listare și Export Excel generează raportul cu selecția curentă. Rezultatul tabelar în pagină este opțional. Produsele sunt grupate cont → produs → variante; se pot selecta ramuri întregi. Filtrele opționale acceptă selecții multiple, filtrate și pe server; rapoartele care cer document/produs/furnizor unic păstrează această cerință. Nu se extind drepturile sau limitele unitate/subunitate/gestiune/exercițiu. Exportul Excel păstrează modulul comun de lățimi configurate.

### Macheta NIR

Fișiere: `PYTHON/routes/vercon/report_layouts.py`, `PYTHON/routes/vercon/report_body.py`. Macheta implicită include coloana și totalul TVA, codul fiscal al furnizorului în antet, numărul storno afișat pozitiv, categoria și tabelul comisiei/semnăturilor după modelul furnizat. Diferențele antet–linii sunt indicate și în raport. Configurările personalizate salvate nu sunt suprascrise; noile valori implicite se aplică unde nu există machetă personalizată.

Stare finală a acestui slice: IMPLEMENTAT_LOCAL, NERULAT. Fidelitatea vizuală și funcționarea efectivă a descărcării, tipăririi și selecțiilor rămân neconfirmate în execuție, deoarece utilizatorul nu a cerut verificări.

## Actualizare 13.10.2026 — rapoarte Magazie–Alimente și infrastructura Excel comună

**Scopul aprobat:** catalogul rapoartelor Materiale/Alimente, sursele SELECT editabile per unitate din KbotAdmin, machetele implicite după cele 16 capturi din `VERCON/CAPTURI_RAPOARTE/Magazie-Alimente`, antet/subsol per unitate, generare pe un model comun ecran/PDF/Excel și snapshot de export. OI/MF sunt amânate la cererea utilizatorului; nu se declară fidelitate vizuală validată.

**Implementarea locală a rapoartelor este acum scrisă**, inclusiv pagina `js/vercon/reports.js` cu CSS separat `css/vercon-rapoarte.css`, rutele `report_routes.py`, sursele `report_data.py`/`report_query.py`, modelele `report_layouts.py`/`report_body.py`/`report_food.py`, validarea `report_validation.py`, configurările versionate `report_store.py` și snapshoturile `report_service.py`. Progres pe familii: stoc/fișe/inventar; NIR/Storno/BC/BR; centralizatoare/furnizori/consumuri; achiziții/referate; lista alimentelor cu patru subreports. Detalii și cele 16 referințe sunt în `docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md`.

KbotAdmin are pagina `Views/VerconReportsView.vb` + Designer și `VerconReportDesignerForm.vb` + Designer, legate în MainForm și CLI `routes/admin_cli/vercon_reports.py`. SQL comun editabil pentru fiecare unitate; editor vizual pentru coloane/bindings/etichete/font/text îngroșat/titlu/orientare și texte poziționate. Lista alimentelor expune șase secțiuni editabile. Antet/subsol comune unității și adaosuri specifice raportului. Editorul este schematic, fără date/paginare reală; grupări/totaluri arbitrare, imagini și designer complet Access nu sunt implementate. Schimbarea structurii coloanelor resetează antetele combinate ale tabelului modificat.

**Stare: IMPLEMENTAT_LOCAL, NERULAT**, nu acceptat vizual și fără rezultate reale de paritate. Nicio verificare executată. VC08-04 și VC08-05 rămân separat de acest slice; VC08 în ansamblu nu este declarat încheiat.

**Regulă obligatorie pentru toate aplicațiile:** lățimile Excel provin din `AVACONT_COMUN.RAPOARTE_EXCEL_COLOANE`. Implementarea comună este `PYTHON/reporting/excel_widths.py`: chei stabile, citire la fiecare export, prioritatea configurației, maximul când mai multe tabele folosesc aceeași coloană Excel și fallback-ul inițial ADECHIT. ADECHIT și VERCON folosesc același modul; VERCON nu mai importă ADECHIT. Importurile istorice din `routes/adechit/report_excel.py` rămân compatibile prin reexport. VERCON folosește `NUME_RAPORT=VERCON_<ID_MAJUSCULE>` și chei din câmpuri (`NAME`, `QUANTITY`, `QUANTITY_2` etc.). Macheta PDF nu suprascrie lățimile Excel configurate.

Regula este scrisă în `PYTHON/AGENTS.md`. Fișiere modificate pentru extracție: `PYTHON/reporting/__init__.py`, `PYTHON/reporting/excel_widths.py`, `PYTHON/routes/adechit/report_excel.py`, `PYTHON/routes/vercon/report_excel.py`, `PYTHON/routes/vercon/report_output.py`. Nicio schemă modificată, nicio interogare executată, niciun test/build/browser/verificare rulat. Refactorizarea păstrează algoritmul; compatibilitatea în execuție nu este încă verificată, conform AGENTS.md.

## Actualizare 12.10.2026 — VC08-02, amortizare

**IMPLEMENTAT_LOCAL:** motor `routes/vercon/depreciation.py`, citire separată registru/documente în `depreciation_source.py`, contract readonly în `depreciation_read.py` și ruta `GET /api/vercon/mf/depreciation?date=&variant=`. Numai MF, drept `raport`, context verificat. Un rezultat per variantă, fără multiplicare prin JOIN între recepții și predări.

Calculul folosește lunile calendaristice de la prima BP, DF, ZEC/scara gestiunii și rest exact în ultima lună. Transferul nu repornește amortizarea; casarea integrală oprește calendarul. DNF/DF/date invalide, registrul duplicat, variantele de mai multe bucăți și casarea parțială au diagnostice, fără valori zero inventate sau alocări arbitrare. Baza este prețul curent al variantei, conform interogării Access; `ValoareLuna` este primară, `ValoareLunara` fallback numai la NULL, ambele păstrate. Este o proiecție cu parametrii curenți, nu replay al auditului istoric.

**Registru MF:** în meniul unui rând, „Amortizare la dată” deschide dialogul comun cu DatePicker/DataGrid, parametri/surse/diagnostice și preview/PDF. `js/vercon/depreciation.js`, CSS separat `css/vercon-mf.css`; formatter exact comun `decimal-text.js`, extras fără schimbarea formulei din Stocuri. Răspunsurile vechi/după închidere sunt ignorate; schimbarea datei invalidează calculul anterior.

**Comparator pregătit, nerulat:** `VERCON/tools/test_vc08_depreciation.py`, 11 probe sintetice per activ, inclusiv referința formulei legacy DateDiff și diferențele corectate. Nu s-au executat teste, lint/sintaxă, browser, aplicația sau SQL în această etapă. Nu s-au schimbat scheme și nu s-a accesat MariaDB. [Detalii și limite VC08-02](docs/worklog/SLICE-VC08-02-motor-amortizare.md).

**Următorul:** VC08-03 — catalogul și generarea rapoartelor incluse. Corective followup-ul rămâne ulterior.

---

## Actualizare 12.10.2026 — CSS modular și VC08-01

**CSS separat pentru VERCON, ADECHIT și PORTAL K-BOT.** `css/vercon.css`, `css/adechit.css` și `css/portal.css` sunt puncte de intrare cu importuri ordonate către module pe responsabilități. VERCON: structură/context/navigare, documente, nomenclatoare, lista de alimente, adaptoare controale, preview, responsive. ADECHIT: structură, ledger, rapoarte, controale, cataloage/formulare, anual, workspace, contacte. PORTAL: structură, extrase, clasificări, meniu, temă, angajamente, filtre, certificat, arbori, detalii documente, autentificare. Secțiunile de bază și suprascrierile păstrează **ordinea originală exactă**. CSS-ul controalelor comune și rapoartelor ADECHIT deja separate rămâne la componentele sale.

Regulile sunt în `PYTHON/AGENTS.md`: CSS separat de JS/HTML/Python, module pe responsabilități, controale comune cu CSS propriu și aproximativ maximum 1000 linii/modul; separare reală, fără comprimarea codului.

**Verificare solicitată:** modulele originale reconstruiesc exact cele trei stylesheet-uri, byte cu byte, în aceeași ordine (VERCON 15153 bytes, ADECHIT 34700, PORTAL 27148). Importurile s-au încărcat în Chromium pentru cele trei HTML-uri reale cu JS aplicațiilor oprit, fără resurse lipsă. Numele punctelor de intrare și selectorii/valorile originale sunt păstrate.

**VC08-01 — IMPLEMENTAT_LOCAL:** motor comun `stock_source.py`/`stock.py`, rute `stock_routes.py`, pagină `js/vercon/stock.js`, CSS `vercon-stock.css`. SI/rulaje/SF și fișă pe variantă, OI/MF separat pe deținător, transfer net zero, casare inclusă la capete, disponibil după excluderea documentului/scăderea draftului. Evaluare la prețul curent al variantei (Q9), fără schimbarea documentelor. `protocol.movements` folosește aceeași sursă pentru validarea la salvare. Pagina reutilizează DataGrid/TreeView/DatePicker/Combobox și previewul comun.

**Limită furnizori:** filtrul selectează variante aprovizionate de furnizor și păstrează toate mișcările necesare SI. Gruparea după proveniența documentului păstrează consumul neatribuit separat; nu reprezintă alocare de stoc pe furnizori. Fișa legacy folosea Coduri/IntrariIesiri, excluse. N09 este corectat în cheia integrală a motorului; nu se inventează FIFO.

**Probe locale:** 10/10 cazuri sintetice trecute; fixture Chromium Stocuri: selecție, mișcări, lună/DatePicker, căutare, responsive, dark, cleanup, fără erori JS. Scripturi: `VERCON/tools/test_vc08_stock.py`, `VERCON/tools/check_v2_browser.cjs`. Fără aplicație reală, SQL sau MariaDB; integrarea reală rămâne nevalidată. Detalii: [worklog VC08-01](docs/worklog/SLICE-VC08-01-motor-stoc-si-balante.md).

**Continuare:** VC08-02 — motor amortizare. VC07-05 rămâne PLANIFICAT; corrective followup-ul rămâne pentru etapa ulterioară cerută de utilizator.

---

Început: 12.10.2026. Cerință: rescrierea tuturor ecranelor VERCON deja existente în `PYTHON/static`, după mockupurile v2 și folosind controalele custom comune. Referințe: `VERCON/FLUXURI_UI.md`, `VERCON/CONTROALE_CUSTOM_THREAD.md`, `VERCON/MOCKUPURI/2026-10-12-corectate-v2/INDEX.md`.

Rescrierea inițială încheiată local: 12.10.2026. Toate ecranele existente din tabel au fost rescrise fără teste, build sau rularea aplicației în acea etapă. Verificările solicitate ulterior pentru CSS și VC08-01 sunt consemnate mai sus. IMPLEMENTAT_LOCAL nu certifică integrarea pe date reale.

## Reguli pentru continuarea în alt chat

- Se lucrează în checkoutul curent. Nu se execută teste, build, verificări de sintaxă sau aplicația fără cererea explicită a utilizatorului. Citirea codului și proiectarea sunt permise.
- Nu se execută modificări de scheme MariaDB. Această rescriere este de interfață; API-urile, drepturile, contextul și regulile de salvare se păstrează.
- Grile DataGrid compacte ca NIR, selecții Combobox, date DatePicker, arbori TreeView; meniuri/dialoguri/mesaje/PDF comune. Draftul se salvează la comanda documentului, nu prin HTTP la fiecare celulă.
- Arborele documentelor este în dreapta în consultare, ascuns în editor. Lista de alimente are mai multe BC-uri și nu repetă consumul.
- Se completează acest fișier imediat după fiecare ecran/familie, apoi se continuă cu următorul. „IMPLEMENTAT_LOCAL” înseamnă cod scris, nu verificat sau acceptat.

## Progres

| Ecran / familie | Stare |
|---|---|
| Structură comună, context și navigare | IMPLEMENTAT_LOCAL |
| NIR, inclusiv extensia MF | IMPLEMENTAT_LOCAL |
| Bon de consum | IMPLEMENTAT_LOCAL |
| Storno și retur | IMPLEMENTAT_LOCAL |
| Predare, transfer, casare | IMPLEMENTAT_LOCAL |
| Lista de alimente și alegere BC | IMPLEMENTAT_LOCAL |
| Registru MF | IMPLEMENTAT_LOCAL |
| Nomenclatoare și variante de preț | IMPLEMENTAT_LOCAL |
| Setări gestiune | IMPLEMENTAT_LOCAL |
| Editorul de preview existent | IMPLEMENTAT_LOCAL |

Stocuri, registrul OI și rapoartele complete nu au în prezent un ecran funcțional în această pagină; existența lor în mockupuri nu înseamnă că API-urile sunt implementate. Se consemnează separat la final, fără date fictive în fluxul real.

## Integrarea finală și predarea

- Toate familiile comune sunt utilizate: DataGrid, TreeView, Combobox, DatePicker, openMenu/closeMenu, confirmBox/reasonBox/pickBox, viewer PDF, mesaje/jurnal și timer de sesiune. Layouturile grilelor sunt conectate prin providerul comun și ID-uri proprii VERCON.
- `js/vercon/ui.js` centralizează montarea și cleanupul. `js/vercon/snapshot-pdf.js` produce local PDF-uri paginate ale valorilor din previzualizare, cu diacritice, în viewerul comun; se pot descărca și deschide pentru tipărire. PDF-ul este rasterizat și nu înlocuiește rapoartele oficiale încă neimplementate.
- Antetele și celulele păstrează valori canonice, sumele monetare fiind șiruri. Calculele locale sunt informative; serverul păstrează autoritatea asupra valorilor, disponibilului, drepturilor și reviziilor.
- POST-urile incerte păstrează corpul exact și cheia; câmpurile sunt blocate până la reluarea salvării sau renunțare. Editorul preview folosește reconcilierea existentă. O salvare confirmată este afișată înainte de reîncărcarea arborelui, astfel încât eșecul reîncărcării să nu transforme salvarea într-un răspuns incert.
- Meniurile și overlayurile Combobox se montează în dialogul gazdă; dropdownurile se poziționează la deschidere și scroll/resize. Footerul DataGrid respectă disabled/span și permite totaluri explicite independente de filtrare.
- BC cere disponibilul pentru data documentului și produsul selectat, excluzând bonul editat. Prețurile produselor existente pe BC/BP sunt păstrate din document. BT/BS păstrează lotul și disponibilul predătorului.
- Navigarea ține cont de ciorne, serializarea deschiderii formularelor și context; accesul exclusiv se actualizează după ocupare/pierdere. Timerul existent are cleanup; nu pornește un al doilea monitor.
- Fișiere comune adaptate: `js/dgv/datagrid.js`, `js/dgv/editing.js`, `js/components/combobox/combobox-ui.js`, `js/adechit/menu.js`, `js/adechit/session.js`. Restul modificărilor sunt în `vercon.html`, `css/vercon.css` și `js/vercon`.
- Harta componentelor pentru alt thread este actualizată în `VERCON/CONTROALE_CUSTOM_THREAD.md`. Nu s-au schimbat scheme, nu s-a executat SQL și nu s-au adăugat API-uri fictive.

Pentru o continuare: citiți acest jurnal și harta componentelor, apoi mockupurile/FLUXURI_UI. Validarea în browser și testele necesită o cerere explicită a utilizatorului conform AGENTS.md. Implementarea ecranelor lipsă (stocuri, registru OI, rapoarte oficiale) este o etapă separată; rescrierea ecranelor existente este încheiată.

### 1. Structură comună, context și navigare

Scrise: `PYTHON/static/js/vercon/ui.js`, modificări în `vercon.html`, `app.js`, `css/vercon.css`. Header cu căutare în ecranul curent, ajutor, temă și schimbarea unității; contextul folosește Combobox și pentru tip. Navigarea folosește operațiile primite de la server și meniul comun pentru nomenclatoare. Spațiu de lucru pe lățimea paginii și stil compact NIR; CSS-urile tuturor controalelor sunt încărcate.

Adaptoare comune pentru câmpuri, grile, arbori, meniuri, modale și preview/PDF; resursele au scope și cleanup. DataGrid a primit totaluri explicite independente de filtrare și footerAction dezactivabil. Editorul de listă comun a primit opțiuni per rând, căutare și eticheta `label` corectă. Timerul comun de sesiune întoarce cleanup; politica sesiunii nu se schimbă și nu se pornește un al doilea monitor. Nerulat/netestat.

### 2. NIR, inclusiv MF

Rescris `js/vercon/docs.js`: antet în trei coloane (furnizor/cont/comisie, factură cu valoare activă, NIR), o singură grilă DataGrid. La MF aceeași grilă se extinde cu inventar/data intrării/DNF/DF/valoare lunară. Combobox cu căutare pentru produse/TVA, DatePicker pentru date. Arbore TreeView furnizor → dată → factură/total în dreapta. Totalurile documentului sunt explicite; calculele locale sunt informative, regulile de salvare rămân pe server. Nerulat/netestat.

### 3. Bon de consum

Același modul: panou TreeView pentru produse/categorii în stânga, detalii produs și disponibil pentru liniile draftului, grilă DataGrid în centru, arbore bonuri în dreapta la consultare. Produsele din editor sunt limitate la cont. Disponibilul se cere pentru data bonului, excluzând BC editat; răspunsurile vechi sunt ignorate. Renunță păstrează documentul anterior. Nerulat/netestat.

### 4. Storno și retur

Selector sursă Combobox, cantități sursă/deja/rămas în DataGrid, prețurile sursei păstrate și DatePicker. Numai liniile cu cantitate completată intră în payload. Arborele documentelor și stările consultare/editor sunt comune. Nerulat/netestat.

### 10. Editorul de preview existent

Rescris `js/vercon/editor.js`: antet cu câmpurile comune, produse în DataGrid, footer Adaugă rând, meniu ⋮ și previzualizarea comună. Draftul extern și protocolul preview existent se păstrează. După răspuns incert, editarea este oprită până la reconciliere; conflictul păstrează ciorna și revizia originală. Renunță restaurează ultima stare acceptată. Nerulat/netestat.

### 8. Nomenclatoare și variante de preț

Rescris `js/vercon/nomenclatoare.js`: liste DataGrid pentru nomenclatoarele descrise de server și TreeView categorie → produs. Formulare comune cu Combobox pentru referințe, câmpuri compacte și draft separat; variantele urmează produsul selectat. Schimbarea produsului ține cont și de ciorna variantei. Filtrul active/inactive/toate folosește pickBox, meniurile ⋮ și confirmările sunt comune; dezactivarea păstrează documentele salvate. Salvarea, request_key și reviziile rămân în protocolul existent; conflictele păstrează datele introduse. Nerulat/netestat.

### 9. Setări gestiune

Același modul: câmpuri comune pentru zecimale/scară și DataGrid readonly pentru contoare. Editabilitatea depinde de drepturi și de absența datelor; limita 0–4 este afișată și aplicată în formular. Mesajele serverului rămân lângă câmpuri. Nerulat/netestat.

### 7. Registru MF

Rescris `js/vercon/mf.js` cu DataGrid, DatePicker în celulă și meniul ⋮. Modificarea datei/DNF/DF se păstrează într-o ciornă pentru mijlocul fix selectat, apoi se salvează împreună cu revizia originală. Identitatea, valoarea, deținătorii și casările sunt readonly; conflictul nu aruncă ciorna. Căutare din header și preview comune. Nerulat/netestat.

### 6. Lista de alimente și alegere BC

Rescris `js/vercon/lista.js`: trei panouri de mese alăturate, fiecare cu un singur rând P/VMD/V/D în DataGrid și grila de feluri. Porțiile felurilor reflectă masa; câmpul Tip existent rămâne separat. Antet cu DatePicker, totaluri BC/listă/diferență, footer Adaugă rând și meniuri comune. Popup modal cu TreeView primitor → dată → BC, selecție urmată de Adaugă BC; eliminarea scoate doar legătura din ciornă. Arbore liste an → lună → dată → număr/valoare ascuns în editor. Salvare atomică, revizie păstrată, cerere POST identică la răspuns incert, preview și reasonBox comune. Nerulat/netestat.

### 5. Predare, transfer și casare

Predare: cont de intrare → primitor/cont primitor și produse din cont. Transfer/casare: Combobox produse din loturile eligibile ale predătorului, cu protecție împotriva dublării aceluiași lot în draft. Casarea nu are primitor. Fără panouri Perioada/Deținători; arborele familiei în dreapta în consultare. Schimbarea predătorului cere confirmarea eliminării liniilor existente. Footer Adaugă rând, total și meniul ⋮ comune. Anularea folosește reasonBox; istoricul și previzualizarea ciornei/documentului folosesc dialogul și DataGrid comune. Payloadurile, reviziile și cheile de creare păstrate; nicio schemă schimbată. Nerulat/netestat.


## 14.10.2026 — valori uniforme DECIMAL(20,4), reset demo și reimport

Decizie confirmată: cantități, prețuri, sume și cote TVA fracționare se stochează
direct în DECIMAL(20,4), independent de precizia calculelor/afișării. Vechea
migrare VC există doar în demo; se golește și se reia, fără conversia datelor existente.

- SQL manual: `sql/VC_DEMO_golire_pentru_decimal.sql` golește numai datele VC din
  `000_DEMO`, inclusiv markerii și auditul; păstrează tabelele, interogările și restul
  modulelor. `sql/VC_precizie_valori_4_zecimale.sql` înlocuiește propunerea BIGINT.
  `sql/00EF_24_primite_linii_precizie.sql` acoperă EF emise/primite și totalurile TVA.
- Scheme și generator: `VC_AVACONT_SURSA.sql`, `00EF_02_efactura_unitate.sql`,
  `00EF_17_primite_tva.sql`, `VERCON/tools/ddl.js`, `lib_tz.js`, `ddl_parts/tables_01.sql`
  și `tables_03.sql`. Clasa internă VALOARE-INT este păstrată, cu tip efectiv Decimal.
- VRC.Migrator: conversie directă, precizie fixă 4, capacitate DECIMAL, reconciliere
  decimală și blocare înainte de scriere pe schema INT veche (VrcConvert, VrcRunner,
  VrcPlan, VrcReport, VrcWriter). KBot.Migrator: cantități/sume la 4 zecimale și
  cerință de schemă DECIMAL(20,4) în EfImporter/EfTableSpec; Corectata rămâne întreg.
- Web VC: numeric, documente, nomenclatoare, bonuri, OI/MF, liste de alimente,
  stocuri, rapoarte și amortizare folosesc Decimal direct. Sunt eliminate conversiile
  la int care ar pierde fracțiuni. TVA este fracție directă, fără x10000.
  Setările disting Zec de precizia fixă 4; modificarea Zec nu rescrie documentele.
- Documentație: CONTRACT_CALCULE, DICTIONAR_DATE, generatorul dicționarului și README
  migrator actualizate cu noul model și ordinea aplicării.

Stare: modificări de sursă și SQL pregătite, neaplicate pe server. Nu s-au rulat
compilări, teste, probe sau migrări, conform AGENTS.md. Sunt necesare aplicarea
manuală a SQL-urilor, sincronizarea schemei, compilarea migratoarelor, actualizarea
aplicației web și reimportul demo. Datele EF deja rotunjite nu se recuperează prin ALTER.


## 14.10.2026 — VRC copiază valorile sursei fără recalculări

Eliminate ApplyHeaderTotal/ApplyPriceSign și agregările folosite pentru completarea
antetelor în VrcRunner. Totaluri NIR/BC, inclusiv 0/NULL, semnele cantităților și
prețurilor se păstrează. Valorile negative devin avertismente în VrcPlan/VrcReport,
nu blocaje și nu corecții. Logul de import precizează regula. Numerotarea actuală
rămâne după ultimul număr real, conform clarificării explicite a utilizatorului.

Schema acceptă valori legacy cu semn: generatorul ddl.js și VC_Reguli_DC.sql nu
mai adaugă CHECK-uri de preț nenegativ. SQL manual VC_IMPORT_valori_sursa.sql
elimină numai CHECK-urile vechi pe AVACONT_SURSA și 000_DEMO, dacă există.
Validările/calculul documentelor noi în web rămân neschimbate. Reconcilierea de
copiere sursă/țintă rămâne; diferențele antet/linii nu sunt diferențe de copiere.
Documentația CONTRACT_CALCULE/DECIZII_DESCHISE/README a fost actualizată.

Stare: surse și SQL pregătite; nu s-au executat SQL-uri, compilări, teste sau migrări.


## 14.10.2026 — popup HTML pentru rezultatele VRC.Migrator

Adăugat Raport HTML… în Designer-ul formularului principal, disponibil după
obținerea unui raport și când utilitarul nu este ocupat. Deschide VrcReportForm
cu rezultatul curent și profilarea deja disponibilă; fără recitire Access/server.
Tabelele Markdown sunt formatate prin Markdig 0.45.0; blocajele/avertizările sunt
evidențiate, iar stilul folosește tema curentă. Popup resizabil, salvare HTML complet
și Închide/Escape. Controlul HTML și layout-ul sunt declarate în Designer.

Fișiere: VrcMigratorForm.vb/.Designer.vb, VrcReportForm.vb/.Designer.vb,
VrcReportHtml.vb/.css, VRC.Migrator.vbproj, README. HTML-ul sursă este dezactivat,
linkurile/imaginile active eliminate și navigarea limitată la documentul local.
Stare: surse modificate; la cererea utilizatorului nu s-au rulat teste, compilări,
aplicația sau comenzi Git. Executabilul existent nu a fost recompilat.

## 15.10.2026 — selectoarele din antetul VERCON

În `PYTHON/static/css/vercon-controls.css`, cele trei combo-uri de context
(Subunitate, Gestiune, An) au înălțimea de 28 px și raza colțurilor de 7,5 px,
ca selectorul de subunitate din ADECHIT. Tranziția implicită `all .2s ease`
este dezactivată pe aceste trei câmpuri pentru a elimina animația fundalului
la ieșirea mouse-ului din hover; evidențierea existentă la hover se păstrează.

Stare: CSS modificat; fără teste sau verificări în browser, conform AGENTS.md.

## 15.10.2026 — aspectul și deschiderea meniului comun

În `PYTHON/static/css/menu.css`, variabila `--hover-backcolor` controlează
fundalul la hover, butonul cu dropdown deschis și banda iconițelor din popup.
Valoarea implicită este `--sky-2`. Coloana iconițelor își păstrează lățimea
inclusiv pentru etichete lungi. Deschiderea are o animație de 180 ms cu coborâre
și dezvăluire progresivă, inversată pentru meniurile deschise deasupra;
preferința de reducere a mișcării dezactivează animația.

În `PYTHON/static/js/components/menu/menu.js`, popup-ul preia configurarea CSS
de la buton și se poziționează cu centrul iconițelor sub emoticoana din bara
de meniu, în limita spațiului disponibil pe ecran. Animația începe după
poziționare. Modificarea se aplică tuturor aplicațiilor care folosesc controlul.

Stare: surse modificate; fără teste sau verificări în browser, conform AGENTS.md.

### Corecție — dropdown pe toată lățimea butonului și dezvăluire vizibilă

În aceleași două fișiere, dropdown-urile barei au acum lățimea exterioară
a butonului și pornesc de la marginea lui. Alinierea emoticoanelor se obține
prin lățimea benzii interioare, fără deplasarea întregului popup.
Etichetele lungi se pot împărți pe mai multe rânduri în lățimea disponibilă.

Animația anterioară cu clip-path de 180 ms este înlocuită cu o tranziție
a înălțimii vizibile de la zero la înălțimea măsurată, în 320 ms. Starea
închisă se afișează înainte de pornirea tranziției, prin două cadre succesive.
La închidere se anulează cadrele și temporizatorul; după deschidere se
restabilește derularea normală. Se păstrează preferința de reducere a mișcării.

Stare: surse modificate; fără teste sau verificări în browser, conform AGENTS.md.

### Corecție — retragere laterală configurabilă și pornirea tranziției

În `menu.css` și `js/components/menu/menu.js`, `--reduce-by: 2px` retrage
fiecare margine a popup-ului față de buton (4 px în total), păstrând alinierea
iconițelor prin recalcularea benzii. Variabila se poate seta pe containerul meniului.

Identificată la analiza codului o eroare a tranziției: clasa de închidere activa
și tranziția de la limita inițială a viewport-ului spre zero, iar deschiderea
o inversa înainte ca limita să ajungă sub înălțimea conținutului. Starea
închisă are acum `transition: none`; tranziția este activată doar pe starea
dezvăluită, după aplicarea și măsurarea stării zero. `--mn-reveal-duration`
configurează durata, implicit 420 ms. Curățarea urmărește sfârșitul tranziției
și are un temporizator de rezervă calculat din durata efectivă.
Eliminată dezactivarea tranziției prin prefers-reduced-motion pentru acest
control, conform solicitării explicite a efectului vizibil.

Stare: surse modificate; cauza identificată prin analiza codului, fără teste
sau verificări în browser, conform AGENTS.md.

### Lățime minimă a dropdown-ului configurabilă pentru fiecare buton

În `PYTHON/static/js/components/menu/menu.js`, fiecare element al barei acceptă
`dropdownMinWidth`, număr de pixeli pentru lățimea minimă exterioară a propriului
popup. Proprietatea este transmisă către `openMenu`, care o aplică prin variabila
dinamică `--_menu-min-width`. În `PYTHON/static/css/menu.css`, minima prevalează
asupra lățimii butonului când este mai mare, fiind limitată la lățimea ecranului.
Omiterea proprietății păstrează lățimea butonului și retragerea `--reduce-by`.

În `PYTHON/static/js/vercon/app.js`, butonul Configurări are
`dropdownMinWidth: 320`, pentru mai mult spațiu destinat etichetelor.

Stare: surse modificate; fără teste sau verificări în browser, conform AGENTS.md.

### Configurări mai îngust și deplasare între dropdown-uri

În `PYTHON/static/js/vercon/app.js`, minima dropdown-ului Configurări scade
cu 30%, de la 320 la 224 px.

În `PYTHON/static/js/components/menu/menu.js` și `PYTHON/static/css/menu.css`,
trecerea prin click la alt buton cu dropdown din aceeași bară pornește panoul
nou din poziția și dimensiunile vizibile ale celui curent. Poziția, lățimea
și înălțimea trec către noul buton în 280 ms, fără scalarea textului.
Listenerul de pointerdown păstrează popup-ul până la click-ul noului buton,
pentru a putea captura geometria înainte de închidere. Schimbările rapide
reiau mișcarea de la poziția intermediară curentă. Prima deschidere păstrează
dezvăluirea existentă; închiderea anulează cadrele și temporizatorul tranziției.

Stare: surse modificate; fără teste sau verificări în browser, conform AGENTS.md.

### Meniu INTRARI / IESIRI pentru alimente și materiale

În `PYTHON/static/js/vercon/app.js`, gestiunile ALIMENTE și MATERIALE grupează
operațiile existente în două butoane:

- INTRARI: Factură cumpărare (NIR), Factură Stornare (fără FCS), apoi separator
  și Listare, păstrată din meniul anterior. Adăugare nu mai apare în acest meniu;
  alegerea facturii deschide consultarea existentă, fără adăugare automată.
- IESIRI: Bon Consum, Bon Retur, apoi separator și Lista Alimente când operația
  este prezentă în meniul primit de la server (numai la ALIMENTE).

Butoanele separate pentru storno, retur și lista de alimente sunt înlocuite de
submeniuri. Fiecare element păstrează cheia formularului, deschiderea și starea
enabled/reason de la server. Butonul părinte este disponibil dacă există cel
puțin un copil disponibil, iar selecția activă urmărește cheia copilului.
Minimele dropdown-urilor sunt 260 px pentru INTRARI și 224 px pentru IESIRI.
Configurări, Rapoarte și meniurile OI/MF își păstrează structura anterioară.
Listare păstrează acțiunea goală existentă; nu s-a implementat o funcție nouă.

Stare: surse modificate; fără teste sau verificări în browser, conform AGENTS.md.

### NIR — editare în pagină și fișe în locul arborelui de furnizori

În `PYTHON/static/js/vercon/docs.js`, `RECEIPT_MODAL_ENABLED = false` oprește
temporar popup-ul la adăugare/modificare NIR; implementarea ModalFrame rămâne
disponibilă și celelalte familii de documente își păstrează comportamentul.
Arborele NIR rămâne montat în coloana lui, cu nodurile și derularea păstrate.
Este ascuns numai conținutul arborelui în timpul editării, fără modificarea
coloanelor sau a poziției documentului. La salvare sau Renunță reapare arborele.
Adaugă este mutat din bara de jos în antetul arborelui: text urmat de iconița
din dreapta. Disponibilitatea urmează dreptul de adăugare/perioada existentă.

Modul nou `PYTHON/static/js/vercon/receipt-context.js` și CSS separat
`PYTHON/static/css/vercon-receipt-context.css`, importat din `vercon.css`:
fișa furnizorului este aliniată cu antetul NIR până la Produse recepționate;
fișa de magazie începe exact la nivelul DGV-ului și îi urmează înălțimea.
Dimensiunile sunt sincronizate prin ResizeObserver și variabile dinamice;
fișele sunt poziționate în interiorul coloanei existente, fără contribuție
la dimensiunile documentului. Pe layout-ul mobil existent se împart în două
panouri suprapuse în spațiul arborelui de sub document.

Fișa produsului urmărește selecția și valorile confirmate ale celulelor.
Un rând incomplet/fără preț valid sau rândul nou gol păstrează ultima fișă
validă. Se păstrează o copie a ultimei combinații valide inclusiv dacă prețul
aceluiași rând este șters temporar. În lipsa oricărui rând valid se afișează
îndrumarea de completare. Ștergerea rândului memorat îi elimină fișa.
Răspunsurile întârziate sunt ignorate după schimbarea selecției/ciornei/modului.

Backend nou `PYTHON/routes/vercon/receipt_context.py`, înregistrat din
`doc_routes.py`: două citiri protejate de accesul la NIR și contextul rezolvat
pe server, fără scrieri sau creare de variante de preț. Fișa furnizorului arată
ultimele 12 facturi/storno salvate și totalul lor în exercițiul curent, până
la data facturii; include identitățile aceluiași cod fiscal. Nu reprezintă un
sold de plată și nu introduce plăți inexistente în sursele curente.
Fișa de magazie reutilizează motorul de stoc existent, identificând varianta
după produs și preț cu TVA calculat pe server; arată ultimele 20 de mișcări
și stocul la dată. La MF se păstrează identitatea distinctă a activului.
Datele sunt cele salvate; ciorna nu este inclusă în rulajele afișate.

Stare: implementare în surse, analiză de design; fără teste, pornire a aplicației,
verificări în browser sau SQL executat, conform AGENTS.md. Nicio schemă modificată.

### Fișe NIR — spațiul de sus recuperat și afișare cu două zecimale

În `PYTHON/static/js/vercon/receipt-context.js` și
`PYTHON/static/css/vercon-receipt-context.css`, fișa furnizorului începe acum
de la marginea superioară a coloanei, iar înălțimea crește cu spațiul recuperat.
Limita inferioară rămâne la Produse recepționate; fișa de magazie și documentul
nu își schimbă poziția. Ambele fișe afișează valori, prețuri, cantități, solduri
și totaluri cu exact două zecimale, prin formatare decimală textuală.
Calculele și precizia datelor salvate rămân cele existente.

Notele precizează acum „Ultimele … din …”: tabelele scurte afișează ultimele
12 documente / 20 mișcări, dar totalurile folosesc întregul set eligibil până
la data facturii. Ciorna nesalvată nu este inclusă în rulajele afișate.

Stare: surse modificate; fără teste sau verificări în browser, conform AGENTS.md.

### Fișe NIR — eliminarea notelor de subsol

În `PYTHON/static/js/vercon/receipt-context.js` sunt eliminate complet notele
de subsol cu numărul de documente/mișcări, data-limită și mențiunea despre ciornă.
Fișele păstrează datele, totalurile și afișarea cu două zecimale.

Stare: sursă modificată; fără teste sau verificări în browser, conform AGENTS.md.

### Fișe NIR — etichetă total și cele mai noi înregistrări primele

În `PYTHON/static/js/vercon/receipt-context.js`, eticheta devine „Total facturi”,
fără „− storno”. Textul anterior descria formula totalului, nu o sumă distinctă
de facturi storno. Calculul existent rămâne neschimbat.

În `PYTHON/routes/vercon/receipt_context.py`, rândurile ambelor fișe sunt
returnate în ordinea descrescătoare a datei. Calculul soldurilor de magazie
rămâne cronologic, iar inversarea se aplică numai listei afișate.

Stare: surse modificate; fără teste sau verificări în browser, conform AGENTS.md.

### Fișe NIR — totalurile în benzile albastre de jos

În `PYTHON/static/js/vercon/receipt-context.js`, Total facturi și Stoc sunt
elemente footer. În `PYTHON/static/css/vercon-receipt-context.css`, acestea
ocupă benzile albastre de la baza fișelor, cu delimitare și text centrat vertical.
Fișa furnizorului se extinde până la începutul fișei de magazie, folosind spațiul
de la nivelul titlului Produse recepționate pentru footer. Fișa de magazie se
extinde până la marginea inferioară a coloanei, cu footer sub nivelul grilei.
Înălțimile benzilor sunt calculate după geometria documentului; tabelele se
derulează separat, iar totalurile rămân fixe. Panourile sunt în continuare
poziționate în coloana existentă, fără deplasarea antetului sau a DGV-ului.

Stare: surse modificate; fără teste sau verificări în browser, conform AGENTS.md.

### TreeView comun — evidențiere de la începutul vizibil al nodului

În `PYTHON/static/js/components/treeview/treeview-render.js` și
`PYTHON/static/css/treeview/treeview_items.css`, indentarea trece din padding
în marginea exterioară a rândului. Controlul setează dinamic `--tv-node-start`,
iar CSS păstrează 4 px în interior înaintea conținutului vizibil, limitați de
marginea arborelui. La frunze, spațiul fostului expander invizibil intră tot
în marginea exterioară. Textul, bifele și expanderele își păstrează poziția;
fundalul hover/selecție și bara albastră încep la noua margine a nodului.
Schimbarea se aplică controlului comun, inclusiv variantelor inline/dropdown
și culorilor din tema dark, fără suprascrierea culorilor de către aplicație.

Stare: surse modificate; fără teste sau verificări în browser, conform AGENTS.md.

### 2026-10-15 — Nomenclatoare în DGV, Baza de date, Comisii și feluri din TIP

Nomenclatoarele folosesc acum numai grile editabile, cu rând gol pentru adăugare,
salvare pe celulă/rând complet și meniul ⋮ pentru activare/dezactivare și
reîncercarea salvării. Formularul lateral din captura utilizatorului este eliminat.
Produsele și variantele de preț păstrează legătura părinte–copil în două grile.
Ciornele incomplete și salvările neconfirmate sunt păstrate separat; crearea
folosește în continuare RequestKey, iar actualizările folosesc Version.

În Configurări, cele cinci intrări individuale Categorii / Conturi, Cote TVA,
Feluri de mâncare, Comisie Recepție și Comisie Inventariere sunt înlocuite cu:
- Baza de date: categorii în stânga; TVA în dreapta sus; feluri în dreapta jos.
- Comisii: Comisie Recepție NIR și Comisie Inventariere, suprapuse pe toată
  lățimea și înălțimea disponibilă a spațiului de lucru cu limitele existente.

Grilele folosesc înălțimi relative la spațiul de lucru, fără VH propriu.
Coloanele de date au ponderi egale; Stare și ⋮ au lățimi fixe, ⋮ rămâne 36 px.
Calculul lățimilor utilizează API-ul proportionalWidths al DGV, care scade
scrollbarul vertical și bordurile; panourile includ paddingul în border-box,
iar antetul ⋮ nu mai impune paddingul standard de 44 px într-o coloană de 36 px.
Nu sunt aplicate preferințe vechi care ar reintroduce lățimi sau coloane ascunse.

Sub categorii este o mini fișă valorică pentru anul selectat: SI, Intrări,
Ieșiri și SF, cu afișare la două zecimale și calcul prin serviciul existent de
stoc. Documentele STOC_INITIAL din interval intră în SI. Evaluarea folosește
prețul curent al variantei, conform contractului existent al serviciului de stoc.
TVA nu are mini fișă. Dezactivarea categoriei este refuzată dacă are mișcări în
oricare exercițiu, inclusiv pentru produse inactive; TVA este protejat dacă este
utilizat de produse prin liniile VC_Intrari, inclusiv legăturile migrate care
păstrează doar valoarea numerică TVA. Refuzul se aplică și pe server, indiferent
de starea meniului clientului.

VRC.Migrator generează VC_FeluriMancare din TIP distinct în MicDejun, Dejun și
Cina, pe toți anii gestiunii, și completează IdFel țintă din acest nomenclator.
IdFel din Access nu mai este interpretat ca referință către feluri. Nomenclatorul
nu primește explicațiile meselor; TIP și Explicatie ale rândurilor se păstrează.
Denumirile Felul n sunt normalizate; un TIP neconform blochează importul cu X13,
un TIP gol rămâne fără referință. Maparea este consemnată în VC_IdMap și N11,
iar versiunea regulilor planului este actualizată. Lista de alimente alege TIP
prin nomenclator și asociază și IdFel, păstrând textele istorice deja salvate.

Fișiere modificate/adăugate:
- PYTHON/static/js/vercon/nomenclatoare.js
- PYTHON/static/js/vercon/nomenclator-grid.js
- PYTHON/static/js/vercon/app.js
- PYTHON/static/js/vercon/lista.js
- PYTHON/static/css/vercon-nomenclatoare.css
- PYTHON/routes/vercon/capabilities.py
- PYTHON/routes/vercon/nomenclatoare.py
- PYTHON/routes/vercon/nomenclator_usage.py
- PYTHON/routes/vercon/nom_routes.py
- PYTHON/routes/vercon/lista.py
- VERCON/VRC.Migrator/VrcRunner.vb
- VERCON/VRC.Migrator/VrcRunner.Dishes.vb
- VERCON/VRC.Migrator/VrcTargetSchema.vb
- VERCON/VRC.Migrator/target_schema.json
- VERCON/VRC.Migrator/VrcPlan.vb
- VERCON/VRC.Migrator/README.md
- VERCON/tools/ddl.js (descrierea regulii meal_type; generatorul nu a fost rulat)
- sql/VC_Feluri_din_TIP.sql
- implementare_vercon_v2.md

SQL-ul este pregătit pentru executare manuală pe o gestiune deja migrată, după
completarea ID-urilor subunității și gestiunii. Nu modifică schema, nu șterge
nomenclatoare și nu este executat automat.

Stare: modificări de sursă, fără teste, compilări, rulare în browser, importuri,
SQL executat sau alte verificări, conform AGENTS.md. Afișarea efectivă și
comportamentul în aplicație nu sunt confirmate prin rulare.

### 2026-10-15 — Produse: grilă pe 70%, istoric prețuri și arbore de fișe de magazie

Ecranul Produselor folosește acum o coloană stângă de 70% din lățimea
interioară disponibilă, pe toată înălțimea grilei. Coloana dreaptă primește
restul spațiului după un interval orizontal de 20 px. Titlurile sunt pe o
linie de layout comună; grila produselor ocupă două rânduri egale de conținut,
iar istoricul prețurilor ocupă primul dintre ele, adică exact jumătate din
înălțimea grilei produselor. În jumătatea dreaptă inferioară este fișa de magazie.
Nu sunt introduse dimensiuni VH sau măsurători JS pentru această geometrie.

Ordinea coloanelor este Categorie / cont, Denumire produs, UM, CPV, apoi
extensiile existente pentru MF și coloanele tehnice. Categoria are 300 px,
UM 100 px, CPV 200 px, iar Denumirea umple spațiul rămas peste minimul ei.
Lățimile fixe nu sunt convertite în ponderi procentuale. Pe spații prea
înguste pentru lățimile cerute, grila produselor permite derulare orizontală.
Filtrele din antet sunt vizibile pe Denumire, CPV și Cont. Filtrarea și
căutarea Contului folosesc denumirea categoriei, inclusiv când categoria
legată produsului este inactivă; opțiunile editorului rămân cele active.
Căutarea generală a ecranului se aplică produselor, pe cele trei câmpuri.

Istoricul prețurilor se afișează în dreapta sus și include implicit toate
variantele, active și inactive. La selectarea produsului se încarcă arborele
variantelor de preț; fiecare ramură deschide fișa sa de magazie. Selectarea
unui rând de preț deschide aceeași fișă. Variantele cu același preț rămân
identități separate, cu codul și ID-ul variantei disponibile în arbore.
Fișa are Data, Document, Cantitate, Sold și footer Stoc, cu afișare la două
zecimale ca la NIR; prețurile din arbore păstrează patru zecimale.

Endpointul read-only /api/vercon/nom/produse/<id>/statement folosește contextul
serverului, exercițiul selectat, registrul warehouse și algoritmul existent
de sold cantitativ. Returnează ultimele 20 de mișcări în ordine descrescătoare
pentru fiecare variantă; soldurile și Stoc includ toate mișcările eligibile,
inclusiv cele anterioare începutului intervalului pentru OI/MF. Variantele
fără mișcări sunt incluse. Calculul cantitativ nu cere preț de evaluare,
pentru a permite și istoricul migrat cu preț negativ sau NULL. Răspunsurile
întârziate sunt ignorate la schimbarea produsului; variantele produsului
precedent nu rămân editabile în timpul încărcării noului produs.

Fișiere modificate/adăugate:
- PYTHON/static/js/vercon/nomenclator-grid.js
- PYTHON/static/js/vercon/product-nomenclator.js
- PYTHON/static/js/vercon/product-stock.js
- PYTHON/static/css/vercon-products.css
- PYTHON/static/css/vercon.css
- PYTHON/routes/vercon/nomenclator_stock.py
- PYTHON/routes/vercon/nom_routes.py
- PYTHON/static/js/dgv/datagrid.js
- PYTHON/static/js/dgv/engine.js
- implementare_vercon_v2.md

DGV transmite formatterul către motorul de filtrare pentru a folosi aceleași
valori afișate în checklist; noul callback opțional filterValue permite
condițiilor de filtrare pe Cont să compare eticheta păstrând ID-ul în date.

Stare: modificări de sursă; fără teste, compilări, verificări în browser,
interogări executate pe baze de date sau modificări de schemă, conform
AGENTS.md. Aspectul și comportamentul nu sunt confirmate prin rulare.

### 2026-10-15 — Produse: prețuri numai din NIR, produse folosite și selector CPV lazy

Grila Istoric prețuri este eliminată complet din ecranul Produselor. În
nomenclatoare, variantele sunt numai pentru citire: create/update/active
pentru kind=variante sunt refuzate în serviciu. Scrierea automată a prețurilor
din fluxul NIR, care folosește repository-ul documentelor, rămâne disponibilă.
Coloana dreaptă este ocupată integral de arbore și fișa de magazie. Arborele
are un singur nivel, fiecare nod afișând prețul cu TVA și prima dată a unui
NIR neanulat care folosește varianta, din toți anii gestiunii. În lipsa unui
astfel de document se afișează „fără dată”, fără dată inventată.

Produsele afișate implicit sunt cele active, folosite în anul selectat în
context. Folosirea este calculată pe server din documentele de stoc valide,
iar pentru OI/MF include și predările, transferurile și casările din anul
respectiv. Căutarea locală din antetul nomenclatorului nu mai este inserată
în pagină. Butonul Afișare folosește openMenu din controlul comun existent,
cu două opțiuni bifabile: Afișează TOATE produsele și Afișează produse INACTIVE.
Realegerea opțiunii bifate revine la produsele folosite în an. Schimbarea
vizibilității păstrează selecția eligibilă sau golește fișa produsului ascuns.

CPV se alege prin butonul „...” din dreapta codului, vizibil numai pe celula
CPV selectată. Deschide o pagină internă CPV, cu cele două căutări după
Denumire și Cod, fonturile controalelor site-ului și Înapoi la produse.
Pagina păstrează rândul și ciorna nomenclatorului. „➕” apare la hover pe
fiecare nod real, inclusiv la nivelele superioare, și la focus pentru acces
prin tastatură. Alegerea scrie codul în rândul inițial și îl salvează folosind
revizia și auditul nomenclatorului; la eroare pagina rămâne deschisă pentru
reîncercare. Pentru un produs nou incomplet, alegerea rămâne în ciorna rândului.
Un CPV nou atribuit este validat pe server în AVACONT_COMUN.CPV.

Endpointul read-only /api/vercon/cpv citește schema CPV/Expl/L1–L7 din
MariaDB_Schema/AVACONT_COMUN.sql. Nodurile au drept identitate CPV.CPV;
nivelurile și prefixele codului determină ramurile, inclusiv legarea unui
nivel sărit de cel mai apropiat părinte existent. La deschidere se citesc
numai rădăcinile; la expandare numai copiii direcți ai acelei ramuri.
Se reutilizează TreeView și modelul de placeholder/pending/cache din NIR,
fără încărcarea tuturor celor aproximativ 10.000 de înregistrări în client.
Ramurile sunt memorate în pagina deschisă și răspunsurile lor în server timp
de 5 minute, cu cel mult 512 intrări de cache; catalogul comun nu conține aici
date specifice utilizatorului. Căutările externe au debounce de 300 ms,
rezultate flat, numărătoare totală și maximum 200 de înregistrări reale.
Peste limită apare rândul „Plus încă N alte înregistrări. Ajustați căutarea
pentru un rezultat mai restrâns.”, fără icon, selecție sau acțiune la mouse
ori Enter. Răspunsurile întârziate sunt ignorate după schimbarea căutării.

Utilizatorul a confirmat că a adăugat deja cheia primară CPV.CPV. Nu s-a
pregătit DDL și nu s-a executat nicio modificare de schemă sau comandă SQL.

Fișiere modificate/adăugate:
- PYTHON/routes/vercon/cpv_catalog.py
- PYTHON/routes/vercon/nomenclator_products.py
- PYTHON/routes/vercon/nomenclator_stock.py
- PYTHON/routes/vercon/nomenclatoare.py
- PYTHON/routes/vercon/nom_routes.py
- PYTHON/static/js/vercon/cpv-picker.js
- PYTHON/static/js/vercon/product-nomenclator.js
- PYTHON/static/js/vercon/product-stock.js
- PYTHON/static/js/vercon/nomenclator-grid.js
- PYTHON/static/js/dgv/datagrid.js
- PYTHON/static/css/dgv.css
- PYTHON/static/js/components/treeview/treeview-render.js
- PYTHON/static/js/components/treeview/treeview-interactions.js
- PYTHON/static/css/vercon-products.css
- PYTHON/static/css/vercon-cpv.css
- PYTHON/static/css/vercon.css
- implementare_vercon_v2.md

Stare: modificări de sursă, fără teste, compilări, verificări în browser sau
interogări executate pe baze de date, conform AGENTS.md. Afișarea efectivă,
rezultatele SQL și performanța nu sunt confirmate prin rulare.

### 2026-10-15 — Selecție până la marginea arborelui și CPV în ModalFrame

În arborele plat al prețurilor, rândurile nu mai au margine de început sau
padding propriu. Fundalul și bara selecției încep la marginea părintelui.
Insetul calculat de TreeView în --tv-node-start este aplicat etichetei;
paddingul vertical de 4 px este mutat tot pe text. Poziția textului și
înălțimea rândurilor sunt păstrate. Regula este limitată la arborele de
prețuri din VERCON, fără schimbarea indentării arborilor ierarhici comuni.

Selectorul CPV folosește acum componenta comună ModalFrame, deja încărcată
în VERCON, cu width=1000 și height=600 prin API-ul controlului și limite
CSS maxime de 1000 × 600 px. Controlul reduce fereastra pe ecrane mici,
folosește backdrop-ul blurat existent și păstrează navigarea Tab în popup.
Produsele rămân vizibile în fundal, inactive până la închidere. Butonul
Închide și Escape folosesc ciclul de închidere al modalului; după alegerea
CPV, modalul este distrus și focusul revine la controlul inițial. Escape
nu mai este capturat separat de selector, pentru a respecta popup-urile
interioare gestionate de ModalFrame. Citirea lazy și căutările CPV sunt
păstrate.

Fișiere modificate:
- PYTHON/static/js/vercon/cpv-picker.js
- PYTHON/static/js/vercon/product-nomenclator.js
- PYTHON/static/css/vercon-cpv.css
- PYTHON/static/css/vercon-products.css
- implementare_vercon_v2.md

Stare: modificări de sursă; fără teste, compilări sau verificări în browser,
conform AGENTS.md. Aspectul efectiv nu este confirmat prin rulare.

### 2026-10-15 — Caption comun ModalFrame și descrierea CPV curent

ModalFrame deține acum bara caption și butonul „×”, cu stiluri comune
pentru hover, apăsare și focus. Proprietățile title, caption și showClose
permit generarea barei, reutilizarea unui header existent sau ascunderea
ei/butonului. Click pe „×” cere același onRequestClose ca Escape, păstrând
decizia paginii privind salvarea ori renunțarea. La închidere sunt eliminate
controalele generate și listener-ele. Documentele reutilizează bara lor de
titlu; selectorul CPV folosește caption-ul generat, fără buton local Închide.

La deschiderea selectorului pentru un produs cu CPV, descrierea codului
apare sub denumirea produsului. Citirea folosește ruta GET dedicată
/api/vercon/cpv/<code>, cu guard și context scoped, și un SELECT parametrizat
după cheia CPV.CPV. Nu încarcă întregul catalog și nu schimbă citirea lazy
a ramurilor. Sunt afișate mesaje pentru încărcare, descriere absentă sau
eroare; răspunsurile sosite după închidere sunt ignorate. Dimensiunea maximă
1000 × 600 și fundalul blurat rămân gestionate de modal.

Documentația controlului din VERCON/CONTROALE_CUSTOM_THREAD.md a fost
actualizată după modificările de cod, cu API-ul și ciclul de viață caption.

Fișiere modificate:
- PYTHON/static/js/components/modal-frame/modal-frame.js
- PYTHON/static/css/modal-frame.css
- PYTHON/static/js/vercon/docs.js
- PYTHON/static/js/vercon/cpv-picker.js
- PYTHON/static/css/vercon-cpv.css
- PYTHON/routes/vercon/cpv_catalog.py
- PYTHON/routes/vercon/nom_routes.py
- VERCON/CONTROALE_CUSTOM_THREAD.md
- implementare_vercon_v2.md

Stare: modificări de sursă; nu s-au rulat teste, compilări, verificări în
browser sau interogări pe baze de date, conform AGENTS.md. Nu s-a pregătit
DDL și nu s-a modificat schema MariaDB. Aspectul și funcționarea nu sunt
confirmate prin rulare.

### 2026-10-15 — Filtrare DGV comună și adăugare produse pe rând

Indicatorul filtrului activ din controlul comun DataGrid folosește o pâlnie
SVG în locul săgeții, cu fontul butonului redus cu 2 pt. Popupul și iconul
sunt separate în filtering.js. Rândul gol de adăugare nu participă la
filtrare, valori distincte ori totalurile footerului; draftul nou rămâne
vizibil la completare. Opțiunea singleNewRow păstrează un singur draft.

În Produse, Categorie / cont are 240 px (80% din 300), Stare este eliminată,
iar Denumire rămâne coloana care umple spațiul disponibil. Lângă titlu apare
butonul ➕, cu tooltip „Adauga produs nou”. La click, DGV afișează numai
rândul nou și deschide editorul denumirii. Valorile celulelor rămân locale
până la finalizarea rândului, inclusiv când se folosește selectorul CPV.

Validarea rândului cere Denumire, UM și Categorie / cont; schema de validare
server cere acum și UM. Mesajele folosesc sistemele comune showMessage și
confirmBox. Lipsa CPV permite salvarea după avertizare și alegerea explicită
a continuării fără cod. Finalizarea se face la părăsirea rândului, Enter în
ultima celulă editabilă sau „Salvează rândul” din meniul ⋮. Escape permite
anularea draftului prin comportamentul comun al grilei.

După creare se recitește lista cu toate produsele, păstrând filtrele DGV,
sortarea și lățimile. Produsul nou este selectat și adus în zona vizibilă.
Dacă nu corespunde filtrului curent, este afișat temporar la final; filtrul
rămâne activ, iar excepția dispare la următoarea modificare a filtrării.
Documentația controlului comun a fost actualizată la final.

Fișiere modificate/adăugate:
- PYTHON/static/js/dgv/datagrid.js
- PYTHON/static/js/dgv/editing.js
- PYTHON/static/js/dgv/filtering.js
- PYTHON/static/css/dgv.css
- PYTHON/static/js/vercon/product-nomenclator.js
- PYTHON/static/js/vercon/nomenclator-grid.js
- PYTHON/static/css/vercon-products.css
- PYTHON/routes/vercon/nomenclatoare.py
- VERCON/CONTROALE_CUSTOM_THREAD.md
- implementare_vercon_v2.md

Stare: modificări de sursă; fără teste, compilări, browser sau interogări
executate pe baze de date, conform AGENTS.md. Nu s-a modificat schema
MariaDB. Aspectul și funcționarea nu sunt confirmate prin rulare.

### 2026-10-15 — Aspect buton Adaugă produs

Butonul ➕ de lângă Produse are acum 44 × 40 px, icon de 24 px, colțuri
rotunjite și centrare flex. Are cursor pointer când este disponibil,
fundal și bordură evidențiate la hover și contur la focus din tastatură.
Starea dezactivată păstrează cursorul normal și opacitate redusă.

Fișiere modificate:
- PYTHON/static/css/vercon-products.css
- implementare_vercon_v2.md

Stare: modificări CSS; fără teste sau verificări executate, conform
AGENTS.md. Aspectul efectiv nu este confirmat prin rulare.

### 2026-10-15 — Componentă comună ActionAddButton pentru Produse și NIR

Adăugată componenta ActionAddButton în static/js/components/action-add-button,
cu JS, CSS și fișier MD propriu. Creează un button nativ, doar cu icon sau
cu text și icon în stânga/dreapta. Stilul este cel folosit la Produse:
înălțime 40 px, varianta doar icon de 44 px lățime, cursor pointer, fundal
și bordură la hover, focus vizibil și stare disabled. Fonturile sunt
configurabile prin --action-add-button-font-size și --action-add-button-icon-size;
și dimensiunile/culorile sunt variabile CSS documentate.

Produse și butonul Adaugă din antetul arborelui NIR folosesc aceeași
componentă și același CSS. NIR păstrează mărimea textului și iconului
anterioare prin variabile. CSS-ul local de aspect al celor două butoane
a fost eliminat. Regula generică pentru butoanele barei de nomenclatoare
exclude controlul comun. Instanțele sunt distruse odată cu pagina/scope-ul
arborelui. Acțiunile existente de adăugare sunt păstrate.

Documentația componentei a fost scrisă la final, cu exemple, opțiuni, API,
variabile CSS și responsabilități de cleanup; inventarul comun a fost
actualizat.

Fișiere modificate/adăugate:
- PYTHON/static/js/components/action-add-button/action-add-button.js
- PYTHON/static/js/components/action-add-button/action-add-button.css
- PYTHON/static/js/components/action-add-button/ActionAddButton.md
- PYTHON/static/js/vercon/nomenclator-grid.js
- PYTHON/static/js/vercon/docs.js
- PYTHON/static/css/vercon-products.css
- PYTHON/static/css/vercon-receipt-context.css
- PYTHON/static/css/vercon-integration.css
- PYTHON/static/vercon.html
- VERCON/CONTROALE_CUSTOM_THREAD.md
- implementare_vercon_v2.md

Stare: modificări de sursă, fără teste, compilări sau verificări executate,
conform AGENTS.md. Aspectul și funcționarea în browser nu sunt confirmate
prin rulare. Nu s-au executat interogări și nu s-a modificat schema MariaDB.

### 2026-10-15 — Denumire specifică ActionAddButton

La cererea utilizatorului, componenta de adăugare se numește ActionAddButton,
în directorul static/js/components/action-add-button, cu fișierele
action-add-button.js, action-add-button.css și ActionAddButton.md.
Importurile din Produse și NIR, linkul CSS al paginii, clasele k-action-add-button
și variabilele --action-add-button-* folosesc aceeași denumire. Documentația
componentei și inventarul comun au fost actualizate. Comportamentul și
aspectul rămân cele implementate anterior.

Fișiere afectate: directorul componentei (JS, CSS, MD), vercon/docs.js,
vercon/nomenclator-grid.js, css/vercon-integration.css,
css/vercon-receipt-context.css, static/vercon.html,
VERCON/CONTROALE_CUSTOM_THREAD.md și implementare_vercon_v2.md.

Stare: redenumire în surse; fără teste sau verificări executate, conform AGENTS.md.

### 2026-10-15 — Buton NIR doar cu icon

Eliminat textul „Adaugă” din butonul de adăugare al arborelui NIR.
ActionAddButton folosește varianta doar cu ➕; tooltipul și numele accesibil
„Adaugă NIR”, stilul și acțiunea sunt păstrate. Exemplul din documentația
componentei a fost actualizat.

Fișiere modificate:
- PYTHON/static/js/vercon/docs.js
- PYTHON/static/js/components/action-add-button/ActionAddButton.md
- implementare_vercon_v2.md

Stare: modificări de sursă; fără teste sau verificări executate, conform AGENTS.md.

### 2026-10-15 — Padding condiționat pentru titlul arborelui

Adăugat selectorul CSS .vc-doc__tree h4:has(button): padding sus, jos și
stânga de 4 px numai când titlul conține un buton. Paddingul din dreapta
și aspectul titlurilor fără buton sunt păstrate.

Fișiere modificate:
- PYTHON/static/css/vercon-documente-panels.css
- implementare_vercon_v2.md

Stare: modificare CSS; fără teste sau verificări executate, conform AGENTS.md.

### 2026-10-15 — Corecție latură padding titlu cu buton

La precizarea utilizatorului, regula .vc-doc__tree h4:has(button) aplică
4 px sus, jos și dreapta. Paddingul din stânga rămâne cel existent.
Titlurile fără buton își păstrează aspectul.

Fișiere modificate:
- PYTHON/static/css/vercon-documente-panels.css
- implementare_vercon_v2.md

Stare: modificare CSS; fără teste sau verificări executate, conform AGENTS.md.

### 2026-10-15 — Bandă colorată uniformă în submeniuri

Eliminat din controlul comun Menu calculul care lărgea coloana iconurilor
pentru alinierea lor cu iconul din meniul principal. Submeniurile folosesc
acum banda colorată CSS de 30 px, ca popupul Afișare produse, cu emoji
centrate în ea. Poziția popupului, textele, dimensiunile și animațiile
existente sunt păstrate. Documentația comună a fost actualizată.

Fișiere modificate:
- PYTHON/static/js/components/menu/menu.js
- PYTHON/static/css/menu.css
- VERCON/CONTROALE_CUSTOM_THREAD.md
- implementare_vercon_v2.md

Stare: modificări de sursă; fără teste sau verificări executate, conform AGENTS.md.

### 2026-10-15 — Butonul de meniu deschis continuă chenarul popupului

Adăugat chenar superior și lateral pe butonul cu dropdown deschis, fără
latura de jos. Se folosesc aceeași culoare --line, grosime de 1 px și
colțuri superioare de 3 px ca la popup. Pseudo-elementul nu modifică
geometria butonului. Marcajul inferior de selecție este ascuns doar în
starea deschisă. Linia superioară a popupului este întreruptă sub buton,
prin gradient și coordonate calculate în API-ul controlului, pentru efectul
unei singure piese. Restul liniei rămâne când popupul este mai lat.

Joncțiunea se aplică popupurilor barei lipite sub buton, fără up/below sau
deplasare verticală la marginea ecranului. Acțiunile, banda iconurilor și
animațiile rămân cele existente. Documentația comună a fost actualizată.

Fișiere modificate:
- PYTHON/static/css/menu.css
- PYTHON/static/js/components/menu/menu.js
- VERCON/CONTROALE_CUSTOM_THREAD.md
- implementare_vercon_v2.md

Stare: modificări de sursă; fără teste sau verificări executate, conform AGENTS.md.

### 2026-10-15 — Umbră dropdown doar jos și dreapta

Modificat stilul comun .kmenu-popup: umbră deplasată 4 px spre dreapta și
jos, blur 10 px; clip-path păstrează extensia doar pe aceste două laturi.
Astfel nu apare umbră deasupra sau în stânga dropdownului. Documentația
comună a fost actualizată.

Fișiere modificate:
- PYTHON/static/css/menu.css
- VERCON/CONTROALE_CUSTOM_THREAD.md
- implementare_vercon_v2.md

Stare: modificare CSS; fără teste sau verificări executate, conform AGENTS.md.

### 2026-10-16 — Factură comună, validări NIR și corecții automate nemodificabile

Cerere utilizator: implementarea analizei FC/NIR, inclusiv cronologia dată–număr,
TVA comun în antet, egalitatea exactă cu valoarea declarată, factura repartizată
între conturi/gestiuni/subunități și migrarea conservatoare din Access.
Completare explicită: rândurile auto-calculate nu se editează; ștergerea returnează
cantitatea în grupul original și este refuzată dacă lasă stoc negativ.

Implementat în surse:
- Logica NIR separată din documents.py în receipts.py; motor comun de simulare a
  ciornei și validare finală, fără scriere de variante la previzualizare.
- Cronologie după (Numar, NumarSub) în seria calendaristică a gestiunii, cu
  pozițiile anulate păstrate și refuzul mutării unui NIR în alt an.
- TVA de antet derivat din rânduri, ascuns pentru cote mixte; propagare validată
  pentru cota comună. Rândurile AUTO blochează propagarea până la revenire.
- Corecție inversă din brut cu TVA real, diferență automată de cel mult 1 leu,
  egalitate exactă a totalului, variantă de preț distinctă și metadate persistente.
- Rânduri AUTO nemodificabile; originea, cantitatea și prețul inițial sunt
  persistate. Ștergerea revine în grup și simulează ambele variante. Permisiunile
  pentru rândurile noi și reveniri sunt note opace în STORE, legate de DC,
  utilizator, context și document; nu se cere o cheie de configurare nouă.
- Factură comună în DC, linii și repartizări către NIR-uri locale; cantități și
  valori bugetate, recepții parțiale, reconciliere exactă și reunire explicită a
  fragmentelor. Interfață nouă în butonul Factură comună al paginii NIR.
- Endpoint de lot atomic pentru mai multe contexte, cu token/revizie pentru
  fiecare destinație; blocări ordonate și tranzacții READ COMMITTED pentru a citi
  alocările deja confirmate după blocarea antetului comun.
- Chei de repetare pentru editare/anulare, cu păstrarea comenzii în UI când
  răspunsul este necunoscut. Dependențele OI/MF anulate nu mai blochează numai
  prin existența rândurilor lor în istoric.
- Liniile istorice nemodificate nu sunt recalculate la o editare de antet.
  Asocierea facturii nu rescrie documentele sau mișcările migrate.
- VRC.Migrator creează fragmente provizorii, păstrează antetul sursă și maparea
  originală; pentru importuri existente există asociere reluabilă, fără reimport.

Fișiere principale adăugate/modificate:
- PYTHON/routes/vercon/{receipts,receipt_rules,receipt_auto,receipt_number,
  receipt_routes,invoices,documents,doc_routes,__init__,protocol}.py
- PYTHON/static/js/vercon/{docs,invoices}.js
- PYTHON/static/css/{vercon,vercon-invoices}.css
- VERCON/VRC.Migrator/{VrcRunner.Invoices,VrcRunner,VrcWriter}.vb
- sql/VC_FACTURI_NIR_20261016.sql, sql/VC_AVACONT_SURSA.sql, VERCON/tools/ddl.js
- VERCON/ANALIZA_FLUX_NIR_2026-10-16.md, VERCON/FACTURI_NIR_IMPLEMENTARE.md

Activare: utilizatorul execută fișierul SQL și sincronizează schema în DC-uri,
apoi publică sursele împreună. Aplicația nu creează schema. Fluxul economic nou
este în RON; catalogul DNF se importă numai dacă există în sursa profilată.
Interfața permite recepții locale în etape; endpointul receive-batch acoperă
loturile trimise simultan. Rândurile Access deja corectate la TVA 0% nu sunt
marcate retroactiv AUTO, deoarece sursa nu conține o dovadă sigură a originii.

Stare: implementare în surse, fără execuție. Nu s-au rulat teste, compilări,
verificări automate, browser sau interogări pe baze de date, conform AGENTS.md.
Nu s-a modificat nicio schemă MariaDB și nu s-a făcut publicare sau commit.

### 2026-10-16 — Separarea clasificărilor MF de schema facturilor

Utilizatorul a anunțat nomenclatorul comun AVACONT_COMUN.VC_ClasificariMF.
Eliminat catalogul local VC_MF_CLASIFICARI din cele două fișiere DDL propuse,
din cerințele de schemă ale facturilor Python și ale migratorului și din
copierea clasificărilor Access la import. VC_ProduseM rămâne registrul activelor.

Fișiere modificate: PYTHON/routes/vercon/invoices.py,
VERCON/VRC.Migrator/VrcWriter.vb, VERCON/VRC.Migrator/VrcRunner.Invoices.vb,
sql/VC_FACTURI_NIR_20261016.sql, sql/VC_AVACONT_SURSA.sql,
PYTHON/routes/vercon/receipts.py, VERCON/FACTURI_NIR_IMPLEMENTARE.md și
implementare_vercon_v2.md.

După actualizarea exportului MariaDB_Schema/AVACONT_COMUN.sql de utilizator,
validarea DNF din receipts.py citește AVACONT_COMUN.VC_ClasificariMF prin
Cod=VC_Produse.CC; limitele din ani se compară în luni cu DNF al activului.
Limitele NULL sau clasificările neasociate păstrează comportamentul anterior
fără interval inventat. Nomenclatorul comun este doar citit de validarea NIR.

Stare: implementat în surse, fără execuție. Nu s-au rulat teste, compilări,
verificări automate sau interogări MariaDB și nu s-a executat DDL.

### 2026-10-16 — Sursă unică pentru schema MariaDB și infrastructură comună de import

Cerere: VRC.Migrator să reutilizeze mecanismul KBot.Migrator și să nu mai
genereze o a doua descriere fizică a schemei AVACONT_SURSA.

Implementat în surse:
- VRC folosește KBot.Migrator.TargetSchema.Load pentru schema reală a DC-ului,
  atât la Testează, cât și imediat înaintea tranzacției de transfer. Baza
  AVACONT_SURSA de pe server rămâne referința pentru sincronizarea separată;
  dump-urile locale nu sunt citite de migrator.
- Aceeași poartă SchemaProblems verifică tabelele/coloanele necesare,
  coloanele obligatorii fără valori implicite și fără mapare, coloanele
  generate, identitatea AUTO_INCREMENT și contractul DECIMAL(20,4). Lista
  coloanelor business este reutilizată din scriere, nu întreținută separat.
- Ordinea tabelelor folosește WriteOrder.Derive din KBot.Migrator, cu FK
  reale și dependențe logice Access. Suprasarcina veche este păstrată;
  fazele nomenclatoare/documente sunt controlate înainte de import.
- NULL și limitele textelor se verifică după metadatele reale. Identitatea
  generată se citește din MariaDB, fără presupunerea RowId sau a numelui
  cheii Access pentru cheia fizică a țintei.
- target_schema.json/VrcTargetSchema au fost înlocuite cu
  import_map.json/VrcImportMap: numai identitatea rândului sursă, clasele de
  conversie și regulile de remapare. Nulabilitatea nu mai este cache-uită în
  resursa încorporată. expected_schema.json rămâne descrierea sursei Access.
- Eliminat sql/VC_AVACONT_SURSA.sql, VERCON/tools/ddl.js și fragmentele sale
  tables_01.sql/tables_03.sql. Noul tools/import_map.js emite numai maparea,
  domeniile aplicației și diagrama orientativă; nu emite SQL.
- DDL-urile incrementale rămân pentru aplicare manuală. Nu s-a adăugat
  creare sau reparare automată de baze/tabele în VRC. VC_Golire.sql este
  păstrat ca listă istorică fixă, fără regenerare și fără rol de schemă.

Fișiere principale: src/KBot.Migrator/MariaDb/WriteOrder.vb,
src/KBot.Migrator/KBot.Migrator.vbproj, VERCON/VRC.Migrator/{VrcImportMap,
VrcWriter,VrcWriter.Schema,VrcRunner,VrcRunner.Dishes}.vb,
VERCON/VRC.Migrator/{import_map.json,VRC.Migrator.vbproj,README.md},
VERCON/tools/{import_map.js,lib_tz.js,README.md},
PYTHON/routes/vercon/{tables,lock}.py, VERCON/{SCHEMA_VC.md,
SCHEMA_VC_RELATII.mmd,VERCON_STATUS.md,FACTURI_NIR_IMPLEMENTARE.md},
sql/VC_Golire.sql și acest jurnal. Fișierele vechi de schemă/mapare au fost
eliminate sau înlocuite conform listei de mai sus.

Versiuni sursă: VRC.Migrator 0.2.0.0; KBot.Migrator 1.19.0.0.
AssemblyVersion rămâne stabil; publicarea VRC trebuie să includă biblioteca
KBot.Migrator actualizată pentru noua suprasarcină de sortare.

Stare: implementat în surse, necompilat și nerulat. Nu s-au executat teste,
verificări automate, generatorul mapării, interogări pe server sau SQL.
S-au făcut citiri și analiză de proiectare în surse. Nu s-a făcut publicare,
commit sau modificare a vreunei baze de date.

### 2026-10-16 — Calcul invers NIR identificat prin existența metadatelor

Cerere: eliminarea modului de calcul redundant din persistența corecțiilor.
Înregistrarea în VC_NIR_CALCULE indică direct calculul invers din totalul cu
TVA. Se păstrează TOTAL_BRUT, PRET_INITIAL și ORIGINE_JSON.

Implementat în surse:
- invoices.write_calculation nu mai scrie modul de calcul. Citirea selectează
  explicit doar identificatorul liniei și cele trei valori necesare; derivă
  calc_mode=GROSS în API când există înregistrarea de metadate.
- Blocarea editării rândurilor AUTO, semnarea lor, calculul invers la recitire
  și ștergerea cu revenire în grup păstrează același contract API.
- DDL-ul de creare și cerințele de schemă ale VRC.Migrator nu mai includ
  coloana redundantă. Versiunea sursă VRC.Migrator devine 0.2.1.0.
- Documentația descrie existența înregistrării drept indicator al calculului
  invers. Nu există script de migrare/eliminare: utilizatorul a confirmat că
  nu a aplicat încă DDL-ul în MariaDB.

Fișiere modificate: PYTHON/routes/vercon/invoices.py,
VERCON/VRC.Migrator/{VrcWriter.vb,VRC.Migrator.vbproj},
sql/VC_FACTURI_NIR_20261016.sql, VERCON/FACTURI_NIR_IMPLEMENTARE.md și
implementare_vercon_v2.md.

Stare: modificări în surse, fără execuție. Nu s-au rulat teste, compilări,
verificări automate sau interogări pe baze și nu s-a aplicat SQL.

### 2026-10-16 — Grupare în SLICE-VC06-06 și pregătire staging

La cererea utilizatorului, modificările din acest thread sunt grupate în
SLICE-VC06-06 — facturi comune, corecții NIR și schemă unică. Adăugat worklog
dedicat în docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md
și înregistrat slice-ul în VERCON/PLAN_IMPLEMENTARE.md și VERCON/VERCON_STATUS.md.
Documentul FACTURI_NIR_IMPLEMENTARE trimite la slice; contractul calculelor
descrie corecția prin existența metadatelor, iar README-ul VRC indică versiunea
curentă 0.2.1.0.

Pregătirea indexului Git folosește o listă explicită a fișierelor acestui thread,
inclusiv înlocuirile și ștergerile aprobate; Surse/EF_SURSA și
Surse/SURSA_XFA_WRITTER sunt excluse. Nu s-au rulat teste, compilări sau SQL.
Nu se execută commit; utilizatorul a cerut staging și un mesaj scurt de commit.

### 2026-10-16 — SQL manual pentru repetarea migrării pe 000_DEMO

Cerere: golirea tabelelor VERCON din 000_DEMO pentru un nou import de test.
Adăugat sql/VC_DEMO_golire_vercon.sql: bloc anonim MariaDB care citește
tabelele VC_ existente din information_schema și generează/execută TRUNCATE
numai cu nume calificate în 000_DEMO, la rularea manuală de către operator.
Include facturile/corecțiile noi dacă există și șterge și markerii de import;
nu depinde de lista fixă istorică. Restaurează FOREIGN_KEY_CHECKS la final și
în caz de eroare. Nu creează proceduri sau tabele; nu atinge Parteneri ori
AVACONT_COMUN. TRUNCATE nu este tranzacțional; o eroare poate lăsa un reset
parțial, raportat prin eroarea MariaDB.

Stare: fișier SQL pregătit, nerulat. S-au consultat sursele locale și
documentația oficială MariaDB pentru sintaxă; fără teste, interogări pe
server sau schimbări de baze de date. Indexul Git pregătit anterior nu a
fost modificat pentru această completare.

### 2026-10-16 — VC06-06, decizii pentru NIR cu mai multe conturi

Cerere: consemnarea deciziilor aprobate în documentele VC, fără cod încă.
Adăugat registrul VC06-06-D01–D11 în VERCON/DECIZII_DESCHISE.md și aliniat
fluxurile UI, planul, statusul, schema explicată, contractul calculelor,
analiza NIR, documentul implementării anterioare, README-ul migratorului și
worklog-ul SLICE-VC06-06.

Decizii: factura direct în Adăugare NIR, fără buton/machetă separată;
arbore de conturi din întregul DC, rădăcina curentă prima/expandată;
blocare selector numai pentru un singur cont global; ComboTree produse cu
căutare, înălțime minimă utilizabilă și deschidere în sus când nu încape;
Comisie pe două coloane sub Data Facturii. Mai multe conturi din aceeași
subunitate rămân în același NIR, cu numărul disponibil. Numai subunități
diferite impun «auto» și NIR-uri distincte, numerotate la salvare.

Contul liniei se deduce prin relația existentă ProduseP → Produse → cont,
fără câmp nou în linie. Contul unui produs cu mișcări, în orice an și prin
oricare variantă, nu mai poate fi schimbat, cu verificare UI și server.
Fără mișcări, rămâne editabil. Importul Access nu se adaptează la acest nou
flux: sursa are un NIR cu un singur cont din unitatea respectivă.

Stare la consemnarea inițială: decizii documentate; implementarea autorizată ulterior este descrisă în continuare. Notele prevalează asupra
propunerilor contrare și disting explicit sursele anterioare de macheta
respinsă. Modificate numai fișiere Markdown. Fără cod, SQL, teste, build,
verificări automate sau operații pe baze. Indexul Git nu a fost modificat;
nu s-a făcut commit. Modificările fără legătură din KBot.App rămân intacte.


### 2026-10-16 — VC06-06, implementarea editorului NIR pe mai multe conturi

La cererea explicită a utilizatorului, implementate în același slice deciziile
VC06-06-D01–D11. Eliminată macheta separată de factură și butonul ei; factura
se introduce în Adăugare NIR, cu toate produsele într-o singură grilă.

Implementat:
- Arbore pentru conturile întregului DC: rădăcina curentă prima/expandată,
  celelalte restrânse; checkbox pe conturi; selector blocat numai pentru
  un singur cont global. Mai multe conturi din aceeași destinație rămân într-un
  NIR cu număr editabil; numai destinații diferite impun «auto».
- ComboTree comun în celula Produs: conturi → produse, căutare la tastare,
  popup cu înălțime minimă utilă, deschidere sus când este nevoie și scroll
  în viewport. Adaptate focusul popupului în DGV și inițializarea TreeView
  înainte de atașarea containerului în pagină.
- Comisia sub Data Facturii pe două coloane, până la marginea Data NIR.
- Salvare atomică a facturii și câte unui NIR per destinație cu produse;
  validări pe întregul lot, ocupări temporare în ordine stabilă, serii locale,
  cronologie și repetarea sigură a cererii. Nu se creează documente goale.
- TVA și precizia produselor sunt ale destinației lor; propagarea din antet
  mapează procentul către ID-ul TVA local. Fișa produsului folosește contextul
  produsului. Consultarea antetului și a facturii derivă conturile din linii.
- Nu există câmp nou de cont per linie. IdMasina în antet este NULL pentru
  conturi multiple și contul derivat pentru cazul unic.
- Contul produsului cu mișcări în orice an și prin orice variantă este blocat
  în UI și server; verificarea include variante inactive și mișcări anulate.
  Schimbarea contului este serializată cu documentele, inclusiv prima mișcare.
- AUTO rămâne nemodificabil, cu ștergere/revenire în grup și simularea stocului.
  La editarea unui NIR generat de noul flux, liniile facturii deținute exclusiv
  de acel NIR urmează schimbările și revenirile; nu se rescriu ținte istorice
  ori împărțite cu alte NIR-uri. Originea noului flux este în auditul existent,
  fără coloană nouă. ValoareFC confirmată rămâne exact totalul recepțiilor.

Fișiere noi: PYTHON/routes/vercon/receipt_destinations.py,
receipt_distribution.py; PYTHON/static/js/vercon/receipt-allocation.js;
PYTHON/static/js/components/combotree/combotree.js;
PYTHON/static/css/combotree/combotree.css și vercon-receipt-destinations.css.
Integrări: doc_routes.py, documents.py, receipts.py, invoices.py,
nomenclatoare.py, nomenclator_usage.py, docs.js, receipt-context.js,
nomenclator-grid.js, dgv/editing.js, components/treeview/treeview.js și vercon.css.
Eliminate: PYTHON/static/js/vercon/invoices.js și static/css/vercon-invoices.css.

Actualizate documentele DECIZII_DESCHISE, FLUXURI_UI, PLAN_IMPLEMENTARE,
VERCON_STATUS, SCHEMA_VC, CONTRACT_CALCULE, ANALIZA_FLUX_NIR_2026-10-16,
FACTURI_NIR_IMPLEMENTARE, README VRC și worklog-ul SLICE-VC06-06.
Importul Access nu se schimbă pentru acest flux și nu este necesar DDL nou.
Rămâne SQL-ul manual al schemei de facturi pregătit anterior. Comisiile cu
aceiași membri și exercițiile trebuie să existe în destinațiile selectate;
aplicația nu creează automat structuri sau nomenclatoare.

Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Nu s-au rulat teste, build, browser,
verificări automate sau SQL, conform AGENTS.md. Nu s-au modificat baze, nu
s-a făcut publicare sau commit. Modificările fără legătură din KBot.App și
Surse rămân în afara implementării.

### 2026-10-16 — ComboTree în DGV: căutare suprapusă și selecție NIR

Corectat în surse selectorul ComboTree după captura și logurile furnizate:
câmpul de căutare se suprapune peste controlul din celulă, cu poziția,
lățimea și înălțimea lui. Inputul folosește border-box și nu depășește
celula; lista are dimensiuni independente și minWidth (300 px în NIR),
limitată la viewport, cu deschidere în sus/jos ca înainte.

Clickurile pe rândurile arborelui păstrează focusul în editor, astfel încât
DGV să nu încheie editarea la mousedown înainte de procesarea clickului.
Alegerea unui produs confirmă imediat editarea celulei prin onCellSave.
Tastarea în oricare dintre inputuri invalidează selecția anterioară;
Escape/Tab din popup anulează/confirmă editarea în DGV. Căutările amânate
sunt oprite la închiderea popupului.

Adăugată opțiunea ComboTree selectParents, implicit false și dezactivată
explicit pentru coloana Produs în NIR. Clickul pe rădăcină/ramură doar
extinde/restrânge când opțiunea este false; true permite selecția părintelui.
Expanderul nu selectează, indiferent de opțiune. Aceeași regulă se aplică
selecției cu Enter.

Fișiere: PYTHON/static/js/components/combotree/combotree.js,
PYTHON/static/css/combotree/combotree.css, PYTHON/static/js/dgv/editing.js,
PYTHON/static/js/vercon/docs.js și acest jurnal.

Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Analiză de proiectare pe surse,
captură și loguri; fără teste, build, browser sau verificări automate,
conform AGENTS.md. Fără SQL sau modificări de baze de date.

### 2026-10-16 — Pagină locală de probă pentru DGV + ComboTree

La cererea utilizatorului, adăugat tools/ComboTreePreview cu index.html,
demo.js, demo.css, preview_server.py și README.md. Folosește componentele
reale din PYTHON/static, 18 rânduri și un arbore fictiv cu trei categorii,
produse și o subramură. Include selectParents, lățimi ale coloanei de
160/234/360 px, minimum dropdown 300/420/560 px, jurnalul schimbărilor,
valoarea confirmată și resetarea datelor. CSS separat, importuri ordonate.

Server Python standard library, numai pe 127.0.0.1:5052; nu importă
aplicația sau modulele bazei de date. Fișierele demonstrației sunt în tools,
în afara aplicației publicate; datele rămân în memoria paginii.

Stare: server pornit și pagina deschisă în browser. La cererea de probă
s-au efectuat interacțiuni în browser: restrângerea categoriei păstrează
lista deschisă, alegerea produselor actualizează Produs/UM și jurnalul,
căutarea ALMETTE filtrează la cele două produse. Dimensiunile DOM pentru
coloana de 160 px: input și căutare au aceeași poziție și dimensiuni
(159,33 × 33,33 px), lista are 420 px. Nu s-au rulat suite automate;
celelalte combinații rămân disponibile pentru probe manuale. Fără SQL
sau modificări de baze de date.

### 2026-10-16 — ComboTree: tastatură și focus automat în Cantitate

La cererea utilizatorului, coloana Produs din Adăugare NIR configurează
focusAfterSelect: Cant. Editorul DGV confirmă alegerea și începe editarea
Cantității din același rând numai dacă salvarea celulei reușește și nu a
început între timp altă editare. Pagina de probă folosește aceeași opțiune
pentru coloana quantity.

Navigarea ComboTree este separată în combotree-keyboard.js. Sus/jos din
textbox mută focusul în arbore și parcurge nodurile vizibile, cu derulare.
În arbore, stânga restrânge nodul deschis sau merge la părinte; dreapta
extinde nodul sau intră în primul copil. Reafișarea arborelui păstrează
nodul activ. Tastele de text revin în căutare și introduc caracterul;
stânga/dreapta în textbox păstrează mișcarea nativă a cursorului. Enter
urmează aceeași cale ca un click pe eticheta nodului, inclusiv pentru
părinți și checkboxuri. Popupul primește focus sincron, astfel încât un
timer de la deschidere să nu fure focusul din arbore sau din Cantitate.

Fișiere: PYTHON/static/js/components/combotree/combotree.js,
combotree-keyboard.js, PYTHON/static/js/dgv/editing.js,
PYTHON/static/js/vercon/docs.js, tools/ComboTreePreview/demo.js,
index.html, README.md și acest jurnal.

Stare: implementat în surse; serverul de probă repornit pe 127.0.0.1:5052,
pagina deschisă și lăsată disponibilă. Probe în browser autorizate de
cerere: selecția cu click și Enter mută focusul în Cantitate, sus/jos
mută focusul în arbore, stânga/dreapta restrâng/extind și navighează
părinte/copil, tastarea din arbore revine la căutare, stânga în textbox
mută cursorul. Enter pe rădăcină restrânge/extinde fără selecție când
selectParents este false. Nu s-au rulat suite automate sau NIR pe baza
reală; integrarea Adăugare NIR este modificată în surse. Fără SQL sau
modificări de baze de date.

### 2026-10-16 — ComboTree: corecții după proba utilizatorului în browser

Reprodus focusul greșit după click în celula DGV: beginEdit selecta inputul
original după ce ComboTree focaliza căutarea suprapusă. focusEdit alege acum
explicit câmpul vizibil de căutare pentru editorul tree și îl folosește la
intrarea în editare, refocalizare și erori. Tastarea directă după click nu
mai ajunge în inputul de dedesubt.

Reprodus nodul activ invizibil: clasa highlighted exista, dar stilul comun
TreeView avea fundalul comentat. Adăugat marcaj albastru numai pentru
ComboTree; eliminat chenarul de focus al listei în favoarea marcajului
nodului. updateHighlight derulează imediat numai lista pentru a păstra
nodul activ integral vizibil. Dezactivate tranzițiile listei/rândurilor și
ancorarea automată a derulării în popupul ComboTree.

Fișiere: PYTHON/static/js/dgv/editing.js,
PYTHON/static/js/components/combotree/combotree-keyboard.js,
PYTHON/static/css/combotree/combotree.css și acest jurnal.

Probe în pagina locală, autorizate de cererea utilizatorului: click urmat
de tastare prin focusul curent, fără focalizare suplimentară a textboxului;
săgeți trimise tot către focusul curent, cu marcaj vizibil și derulare
repetată până la INVENTAR; nod activ integral vizibil și scroll păstrat
la citirea ulterioară. Enter pe produs păstrează trecerea în Cantitate,
iar tastarea din arbore revine în căutare. Proba anterioară cu locatori
care focalizau explicit textboxul nu surprinsese focusul greșit la click.
Serverul 5052 rămâne pornit, pagina reîncărcată și disponibilă. Nu s-au
rulat suite automate sau NIR pe baza reală; fără SQL sau modificări de baze.

### 2026-10-16 — DGV: aspectul și formatul celulei păstrate la editare

La cererea utilizatorului, editoarele text/numerice pornesc cu textul
formatat prin același formatValue folosit de celulă. Prețul 0.5550 este
afișat și în editare ca 0,5550. Dacă textul nu se schimbă, rămâne valoarea
internă originală, fără conversie, rotunjire sau salvare suplimentară.
Parserul numeric implicit acceptă formatul românesc cu grupare pentru
standard/currency/euro și păstrează semantica formatului percent; parserul
explicit al coloanei continuă să aibă prioritate.

Celulele transmit acum alinierea textului și către editor, iar inputurile
moștenesc fontul, spațierea și stilul cifrelor. Paddingul editorului ține
cont de bordură pentru a păstra poziția textului. Căutarea suprapusă
ComboTree primește aspectul inputului din celulă prin API-ul controlului,
după montarea editorului în DGV.

Extinsă pagina tools/ComboTreePreview cu Preț, Valoare, UM editabil,
două rânduri identice și comutator font 15/20 px pentru comparație.
Fișiere: PYTHON/static/js/dgv/editing.js, PYTHON/static/css/dgv.css,
PYTHON/static/js/components/combotree/combotree.js,
PYTHON/static/css/combotree/combotree.css,
tools/ComboTreePreview/demo.js, demo.css, index.html, README.md și jurnalul.

Stare: serverul local 5052 repornit, pagina deschisă și lăsată cu Preț
în editare. Probe locale cerute explicit: textul 0,5550 și alinierea dreapta
identice înainte/după click; font Calibri, 20 px, greutate 400, linie 20 px
identic în celulă și editor; test și la 15 px; UM centrat; căutare ComboTree
la 20 px. Confirmarea nemodificată nu emite salvare. 1.234,50 modificat în
1.234,60 se salvează numeric 1234.6; prețul original rămâne șirul 0.5550.
Fără suite automate sau probe NIR pe baze reale; fără SQL/modificări de baze.

### 2026-10-16 — DGV: eliminarea conturului negru de focus

Eliminat outline-ul nativ al browserului pentru inputurile editorului DGV
(exceptând checkboxurile). Rămâne bordura explicită în culoarea accentului
grilei, inclusiv la focus; regula existentă pentru aria-invalid rămâne activă.
Fișier: PYTHON/static/css/dgv.css. Continuarea probei locale pe portul 5052,
fără suite automate, SQL sau operații pe baze de date.

### 2026-10-16 — NIR: ValoareFC și butonul de corecție în antet

În adăugare/editare NIR, ValoareFC este aliniat la dreapta și afișat în
formatul standard cerut explicit: 1,234.56 (virgulă la mii, punct la
zecimale, două zecimale). TextBox money primește opțional numberLocale
en-US; celelalte controale își păstrează formatul. Citirea și colectarea
antetului folosesc valoarea canonică a controlului, nu textul cu grupare.

Mutat butonul Corectează din bara de jos lângă ValoareFC, ca ⚖️ cu tooltip
și etichetă accesibilă. Utilizatorul a clarificat pragul inițial >=1:
regula aprobată este diferență nenulă de cel mult 1 leu, în valoare absolută.
Butonul respectă acest prag și restricțiile existente pentru corecții AUTO,
drepturi și salvarea în curs. Acțiunea recitește condițiile după confirmarea
editării curente a grilei.

La orice diferență validă față de totalul calculat al liniilor, textul
ValoareFC devine roșu și bold; revine la aspectul normal când totalurile
sunt egale. Starea este actualizată la editarea antetului, la calculul
totalurilor și după răspunsurile de validare/corecție. Comparația folosește
unități întregi BigInt la precizia documentului, pentru limite exacte la
zero și un leu. Nu s-a modificat algoritmul de corecție de pe server.

Fișiere: PYTHON/static/js/vercon/docs.js, ui.js,
receipt-reconciliation.js (nou),
PYTHON/static/js/components/textbox/textbox.js,
PYTHON/static/css/vercon-receptie.css și acest jurnal.

Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Serverul local de probă de pe portul
5052 a fost oprit la cererea utilizatorului. Numai analiză de proiectare
pe surse; fără teste locale, browser, build, verificări automate sau SQL.
Fără modificări de baze de date.

### 2026-10-16 — NIR: rânduri corectate, antet și bară de acțiuni

Aplicate cerințele vizuale din capturile utilizatorului:
- Eliminată coloana Calcul/AUTO din specificațiile grilei NIR. calc_mode
  rămâne metadată internă pentru calcule, blocări, salvare și revenirea
  corecțiilor. Rândurile GROSS sunt bold și italic, prin opțiunea nouă
  rowClassName a DGV, aplicată și la reafișarea rândurilor virtualizate.
- Înlocuit emoji-ul de corecție cu un SVG monocrom în currentColor,
  folosind culoarea temei și păstrând un contrast mai bun când e dezactivat.
  Tooltipul și regula de activare aprobate rămân aceleași.
- Numărul facturii este centrat numai în textbox; numărul NIR, inclusiv
  auto și sub-numărul opțional, este aliniat la dreapta.
- Fontul grilei de linii NIR este micșorat cu 1 pt, inclusiv antetul,
  celulele și editoarele care îl moștenesc. Celelalte grile nu se schimbă.
- Eliminat butonul Adaugă rând din footerul NIR; se păstrează mecanismul
  existent al rândului gol pentru introducere și totalurile din footer.
- Bara din adăugare/editare NIR are trei coloane: Renunță la stânga,
  mesaje la mijloc, Salvează NIR la dreapta. Mesajele se încadrează în
  spațiul lor și nu schimbă pozițiile butoanelor. Previzualizează nu mai
  este creat în această bară în timpul adăugării/editării; rămâne la consultare.
- Mesajul fără mișcări din Fișa de magazie este acum simplu: PRODUS NOU.

Fișiere: PYTHON/static/js/vercon/docs.js, receipt-context.js,
PYTHON/static/js/dgv/datagrid.js, PYTHON/static/css/vercon-receptie.css
și acest jurnal.

Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Nu s-au executat teste, browser,
build sau verificări automate, conform cererii. Serverul local 5052 nu
a fost repornit. Fără SQL sau modificări de baze de date.

### 2026-10-16 — NIR: Stoc inițial în titlu și butoane cu lățimi egale

Mutată bifa Stoc inițial (fără furnizor) din câmpul Furnizor în bara de
titlu, imediat după titlu. Marcajul Ciornă este împins la dreapta panoului.
Bifa rămâne disponibilă în adăugare, ca înainte, cu aceeași logică pentru
furnizor, factură și stoc inițial. Containerul ei este golit la refacerea
antetului, iar controlul este curățat prin scope-ul existent.

Corectată cauza întinderii butonului Salvează când nu există mesaj:
showMessage ascunde span-ul gol, ceea ce lăsa butonul în coloana flexibilă.
Mesajele au acum un container permanent; Renunță și Salvează au poziții
grid explicite și aceeași lățime compactă, 8em. Spațiul rămas la mijloc
este gol sau conține mesajul, fără să deplaseze ori să lățească butoanele.

Fișiere: PYTHON/static/js/vercon/docs.js,
PYTHON/static/css/vercon-receptie.css și acest jurnal.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Fără teste, browser, build sau
verificări automate, conform cererii. Serverul 5052 rămâne oprit.
Fără SQL sau modificări de baze de date.

### 2026-10-16 — VERCON: efect unitar pentru butoane

Adăugat vercon-button-effects.css, importat la sfârșitul stylesheetului de
intrare. Toate butoanele VERCON, inclusiv acțiunile din grile, dialoguri și
elementele role=button, primesc același feedback: contur evidențiat în
culoarea temei, umbră și ridicare vizuală de 1 px la hover/focus din tastatură;
la apăsare revin și primesc o umbră interioară. Efectul nu modifică lățimea
sau poziția din layout. Butoanele dezactivate nu primesc efectul interactiv.
Umbra este adaptată pentru dark, iar prefers-reduced-motion păstrează
evidențierea fără mișcare sau tranziții.

Fișiere: PYTHON/static/css/vercon-button-effects.css (nou), vercon.css
și acest jurnal. Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Fără teste, browser,
build sau verificări automate; serverul local 5052 rămâne oprit.
Fără SQL sau modificări de baze de date.

### 2026-10-16 — ComboTree: Tab din căutare către controlul următor

Tab din căutarea popupului închide lista și reia navigarea de la inputul
original al ComboTree, fără să-l redeschidă la refocalizare. Shift+Tab
folosește direcția inversă. Tratat explicit Tab și pe popup, deoarece
listenerul comun al căutării TreeView nu includea această tastă.

Selectorul Categorie / cont din NIR transmite navigarea prin onTab către
ordinea existentă a controalelor din antet. Evenimentul este interpretat
în raport cu inputul din formular, nu cu popupul mutat în body/dialog.
În DGV rămâne prioritară tratarea existentă în capture pentru confirmarea
celulei și trecerea la următoarea/precedenta celulă.

Fișiere: PYTHON/static/js/components/combotree/combotree-keyboard.js,
PYTHON/static/js/vercon/receipt-allocation.js, docs.js și acest jurnal.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Fără teste, browser, build sau
verificări automate. Serverul local 5052 rămâne oprit; fără SQL/operații pe baze.

### 2026-10-16 — NIR: acțiuni integrate, selecție furnizor și total corectat

Săgețile ComboBox/ComboTree, calendarul și butoanele de ștergere din
textbox/căutare sunt excluse din efectul comun de ridicare. Un stylesheet
VERCON separat aliniază acțiunile de listă și calendar în interiorul
câmpului, cu lățime de 28 px, colțurile din dreapta și hover comun din
paleta temei, inclusiv pentru lista simplă Comisie. Eliminat hoverul verde
al ComboTree; arborii inline păstrează săgeata dropdown ascunsă.

Clickul pe opțiunea ComboBox și pe săgeată anulează activarea implicită
a etichetei label, care putea refocaliza inputul și redeschide lista după
prima selecție. Închiderea/selecția, schimbarea căutării și distrugerea
controlului invalidează căutările întârziate pentru a evita redeschiderea
de către un răspuns vechi.

ValoareFC devine roșu bold numai când există diferență și cel puțin un
rând diferit de rândul gol automat. Eliminată identitatea redundantă a
furnizorului din fișa laterală; identitatea produsului rămâne afișată.

La afișarea NIR se păstrează valorile validate de server. Recalcularea
din paintLines/draftRows cu numere binare putea altera un rând normal
după corecție, producând diferența de 0,0001 lei față de totalul validat.
Recalcularea la modificarea datelor rămâne urmată de validarea existentă.
Footerul NIR însumează exact zecimalele cu BigInt, prin același modul ca
reconcilierea. Mesajul de corecție folosește formatul numeric al grilei.

Fișiere: PYTHON/static/css/vercon-button-effects.css,
vercon-control-actions.css (nou), vercon.css;
PYTHON/static/js/components/combobox/combobox-events.js,
combobox-search.js, combobox-ui.js, combobox.js;
PYTHON/static/js/vercon/docs.js, receipt-reconciliation.js,
receipt-context.js și acest jurnal.

Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Numai analiză de surse/design;
fără teste, browser, build sau alte verificări executate, conform
instrucțiunilor proiectului. Serverul local 5052 nu a fost repornit.
Fără SQL sau modificări de baze de date.

### 2026-10-16 — VERCON: meniuri fără conturul comun la hover

Excluse butoanele barei de meniu și opțiunile popupurilor din efectele
comune de buton, inclusiv hover, focus și apăsare. Meniurile păstrează
stilurile controlului Menu, fără border, contur interior sau ridicare
adăugate de stylesheetul VERCON pentru butoanele obișnuite.

Fișiere: PYTHON/static/css/vercon-button-effects.css și acest jurnal.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Fără teste sau alte verificări
executate, conform instrucțiunilor proiectului. Serverul 5052 nu a fost
repornit. Fără modificări de baze de date.

### 2026-10-17 — NIR: factură cumulată și păstrarea secțiunii la schimbarea gestiunii

Ascunse marcajele Ciornă și Salvat din titlul NIR; marcajul Anulat rămâne
pentru documentele anulate. Celelalte familii de documente își păstrează
marcajele existente.

Pentru facturile comune, ValoareFC arată totalul facturii, cu fallback la
totalul recepționat când factura nu are o valoare declarată. Arborele NIR
folosește aceeași valoare cumulată, citită într-o interogare pe luna deschisă.
Sumele locale persistate în VC_NIR nu se schimbă. Nu se reunesc implicit
facturi istorice fără legătura comună existentă.

Adăugat un modul de citire a liniilor celorlalte NIR-uri active asociate
facturii. Sunt furnizate separat de liniile documentului local, cu chei
distincte, destinație, TVA, precizie și marcajul existent al corecției.
Grila le afișează împreună și însumează exact toate rândurile. Liniile din
alte gestiuni sunt italic, fără bold; liniile corectate sunt italic+bold,
inclusiv când sunt externe. Rândul gol automat rămâne ultimul.

Liniile externe sunt numai pentru consultare: nu pot fi editate sau
șterse și nu intră în payloadul de validare/salvare. În editarea unui NIR
dintr-o factură comună, ValoareFC cumulată este read-only; se păstrează
intern ținta locală a NIR-ului pentru corecție și salvare. Reconcilierea
vizuală folosește totalul întregii facturi. Corecția operează numai pe
liniile locale. Previzualizarea/raportul continuă să primească antetul și
liniile locale; interogările rapoartelor oficiale nu au fost modificate.

Schimbarea contextului păstrează secțiunea disponibilă în noul meniu.
Paginile de documente reîncarcă datele în același panou, fără demontarea
structurii paginii. Dacă secțiunea nu există în noua gestiune, este aleasă
NIR, dacă este disponibilă (de exemplu BC -> OI/MF). Pentru factura deja
deschisă, se redeschide NIR-ul legat din noua gestiune/an, dacă există.
Alte tipuri de pagini se remontează în aceeași secțiune selectată.

Selecția documentului este izolată per context; cererile vechi sunt
anulate înaintea schimbării. API-ul comun combină semnalul paginii cu
semnalul cererii. Se păstrează confirmarea abandonării modificărilor;
schimbarea este blocată pe durata unei operații de scriere sau cu un
rezultat neconfirmat. Ocuparea gestiunii este eliberată și reacordată
conform fluxului existent. Refuzul preluării unei gestiuni ocupate nu
reactivează datele vechi sub noua selecție.

Fișiere: PYTHON/routes/vercon/receipt_invoice_view.py (nou), invoices.py,
receipt_tree.py; PYTHON/static/js/vercon/receipt-invoice-view.js (nou),
docs.js, app.js, api.js, receipt-allocation.js;
PYTHON/static/css/vercon-receptie.css și acest jurnal.

Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Numai analiză de surse/design;
fără teste, browser, build sau alte verificări executate, conform
instrucțiunilor proiectului. Asocierea facturilor și reîncărcarea la
schimbarea contextului necesită confirmare în utilizare. Serverul local
5052 nu a fost repornit. Fără SQL executat sau modificări de scheme.

### 2026-10-16 — NIR: Comisie sub Categorie și salvare după editarea celulei

Comisie începe din prima coloană, sub Categorie / cont, și ocupă primele
două coloane ale antetului. Pe ecrane înguste se păstrează adaptarea
existentă la două coloane sau la o singură coloană.

Identificată în surse o oprire fără mesaj a salvării: blurul celulei
DGV pornește confirmarea asincronă, iar clickul ulterior pe Salvează
apela commitEdit în timp ce aceeași confirmare era pending. DGV returna
false, iar formularul abandona salvarea. commitEdit reutilizează acum
promisiunea acelei confirmări, astfel încât salvarea o așteaptă, fără
trimiterea de două ori a modificării celulei.

Formularul rezervă starea busy înaintea confirmării controalelor, pentru
a evita salvări concurente. La NIR, verificarea de părăsire a rândului
este suspendată în această etapă; validarea strictă a documentului
continuă să blocheze rândurile incomplete. Fiecare oprire din cauza
câmpurilor sau a celulei are mesaj în bara de acțiuni; câmpurile din
antet care blochează salvarea sunt enumerate. Mesajul de rezultat
neconfirmat se aplică numai după inițierea cererii de scriere, păstrând
cheia de idempotentă și ciorna pentru reluare.

Fișiere: PYTHON/static/css/vercon-receipt-destinations.css,
PYTHON/static/js/dgv/editing.js, PYTHON/static/js/vercon/docs.js și acest
jurnal. Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Cauza identificată prin
analiză de surse; nu a fost reprodusă în browser. Fără teste, build sau
alte verificări executate, conform instrucțiunilor proiectului.
Serverul local 5052 nu a fost repornit. Fără SQL sau modificări de baze.

### 2026-10-16 — NIR: lista Comisie la lățimea casetei

Eliminată limita CSS comună de 320 px numai pentru dropdownul Comisie
din antetul NIR. Poziționarea controlului calculează deja lățimea după
input; lista poate ocupa acum aceeași lățime ca acea casetă pe două
coloane, cu limita existentă de încadrare în viewport.

Fișiere: PYTHON/static/css/vercon-receipt-destinations.css și acest jurnal.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Fără teste sau alte verificări
executate, conform instrucțiunilor proiectului. Serverul 5052 nu a fost
repornit. Fără modificări de baze de date.

### 2026-10-17 — Sesiune: avertizări custom, ieșire fără confirmare și probă locală

Eliminate toate apelurile native alert/confirm/prompt și protecțiile
beforeunload găsite în sursele web proprii. Erau prezente la ieșirea din
VERCON, ADECHIT (plătitori/nomenclatoare), înregistrare și în eroarea PDF
a raportului ADECHIT. Navigarea internă păstrează confirmările custom;
în ADECHIT schimbarea aplicației verifică explicit editările nesalvate.
Închiderea sau reîncărcarea filei nu mai produce o întrebare Chrome.

Adăugat messageBox cu un singur buton în controlul comun confirm-box,
cu actualizarea textului, închidere programatică și suport pentru
fereastra raportului. Timerul comun VERCON/ADECHIT afișează la 15 minute
un mesaj custom cu timpul rămas și recomandarea de încheiere/salvare;
la 5 minute îl înlocuiește cu mesajul serios, iar headerul devine
portocaliu. Mesajele apar o singură dată pe prag în pagina curentă și
nu prelungesc sesiunea. Timerul este vizibil în ultimele 15 minute;
în VERCON a fost mutat între selectorii de context și cont/ajutor.

La expirare sunt oprite timerele, este blocată interacțiunea, se închid
dialogurile și ferestrele scriptate de raport/chitanță/ajutor urmărite de
aplicație, apoi se navighează imediat la autentificare. Logoutul este
trimis keepalive, fără a aștepta rețeaua. VERCON anulează cererile paginii
și eliberează ocuparea gestiunii. Expirarea monitorului portalului nu
mai așteaptă cele 3 secunde ale unei notificări. Confirmările de abandon
nu sunt parcurse la expirare, inclusiv cu modificări nesalvate.

Probă locală izolată în tools/ComboTreePreview: /session-demo pornește
la click un interval de 3 minute cu același modul comun, dar praguri
120/60 secunde. Se pot lăsa text, formular și fereastră de probă deschise.
La zero se trece direct la /session-ended. Numai demo-ul folosește
pragurile scurtate; configurația publicată păstrează 15/5 minute.

Validare cerută explicit de utilizator:
- Verificare sintactică Node pentru cele 13 module JS modificate;
  repetată pentru cele 3 module ADECHIT ajustate ulterior: fără erori.
- Căutarea în sursele web proprii nu mai găsește apeluri native sau
  beforeunload care să genereze confirmarea browserului.
- Toate cele 7 rute/asseturi principale ale probei: HTTP 200.
- Parcurs real în in-app browser, 3 minute: text nesalvat și formular
  deschis; observat primul dialog custom, apoi mesajul serios la 01:00.
  Headerul are rgb(198, 91, 0) în etapa critică. Fără erori în consola
  paginii. La zero, navigare automată la /session-ended cu formularul
  și avertizarea lăsate deschise; getJsDialog nu raportează dialog nativ.
- Pagina a fost readusă la starea Pregătit, 03:00, pentru proba utilizatorului.

Fișiere: PYTHON/static/js/utils/confirm-box.js, session-exit.js (nou);
PYTHON/static/js/adechit/session.js, app.js, catalogs.js, payers.js,
reports.js, help.js; PYTHON/static/js/vercon/app.js, api.js;
PYTHON/static/js/session/session-monitoring.js;
PYTHON/static/js/inregistrare/wizard.js; PYTHON/static/vercon.html;
PYTHON/static/css/confirm-box.css, vercon-base.css, adechit-layout.css;
tools/ComboTreePreview/preview_server.py, session-demo.html,
session-demo.js, session-demo.css, session-ended.html, README.md.

Stare: IMPLEMENTAT ÎN SURSE, probă locală efectuată. Serverul 5052 este
PORNIT la cererea utilizatorului, numai pe localhost. Nu s-a folosit
sesiune reală sau acces la baze de date; logoutul backend în producție
nu a fost exercitat. Fără SQL executat sau modificări de scheme.

### 2026-10-17 — Previzualizare: resurse de raport omise la publicare

Traceback-urile furnizate pentru 16/17 octombrie indică FileNotFoundError
pentru /root/AVACONT/reports/vercon/queries/metadata.sql. Fișierul există în
surse. Filtrul implicit KBotAdmin exclude *.sql; fontul PDF .ttf nu este nici
el în lista implicită de extensii publicate. Citirea reușită `VR op=read`
trimisă inițial nu diagnostichează operația de raport.

PYTHON/.pushignore include explicit cele 11 interogări SELECT ale rapoartelor,
fontul DejaVuSans și licența. KBotAdmin/Server/PushService.vb și
AvacontPush/Form1.vb respectă includerea explicită și peste filtrul extensiilor,
fără a extinde global lista configurată. KBotAdmin/Views/PushFilesView.vb
afișează numărul de fișiere de trimis și pentru astfel de excepții.
tools/Push-Changed.ps1 include aceleași resurse și în etapa de enumerare,
păstrând interdicția pentru fișierele protejate. README-ul din
PYTHON/reports/vercon explică republicarea și utilizarea unui executabil vechi.

report_store.py și report_output.py întorc 503/REPORT_RESOURCE_MISSING cu
resursa lipsă în locul mesajului generic de server. docs.js afișează și codul
cererii pentru corelare cu logs. api.js oprește fluxul cu AbortError după
redirectul de autentificare, astfel încât pagina care se închide să nu mai
afișeze o eroare suplimentară de operație.

Răspunsul SESIUNE_EXPIRATA rămâne distinct de această eroare de instalare:
previzualizarea trimite tokenul prin wrapperul API existent. Traceback-urile
primite nu stabilesc cauza invalidării sesiunii. Nu s-a eliminat sau ocolit
autentificarea.

Stare: IMPLEMENTAT ÎN SURSE; necesită publicarea resurselor și codului pe
server. Doar analiză de surse/design, fără teste, build, cereri de raport sau
conectare/publicare pe server în această intervenție, conform AGENTS.md.
Fără SQL executat, fără modificări de schemă. Serverul local 5052 nu a fost
oprit sau repornit în această intervenție.

### 2026-10-17 — Previzualizare: aliasuri în sursele SQL ale rapoartelor

După publicarea completă, logul utilizatorului de la 08:24:32 arată eroarea
MariaDB 1064 la source.rows('committee'). Interogarea implicită nu avea alias
după markerul VC_ComisieNIR. report_query.execute înlocuiește markerul cu
un subquery filtrat pe context, iar MariaDB cere alias după acest subquery.
Aceeași omisiune exista în sursele food_history și settings.

Corectate PYTHON/reports/vercon/queries/committee.sql, food_history.sql și
settings.sql: aliasuri explicite c/h/s și coloane calificate, păstrând
numele câmpurilor întoarse și filtrarea existentă. README-ul rapoartelor
documentează cerința și configurările personalizate, care nu se suprascriu.
Fișierele schema_diff*.sql rămân în afara publicării aplicației; sunt
rezultate pentru analiza diferențelor de schemă, fără rol în previzualizare.

Stare: CORECTAT ÎN SURSE, necesită republicarea celor trei fișiere SELECT.
Interogările implicite se citesc la cerere, deci această corecție SQL nu
necesită repornirea serviciului. Dacă există versiuni personalizate ale
acestor surse, ele trebuie corectate explicit în editor. Doar analiză de
surse/design; fără teste sau execuții SQL, fără acces/publicare pe server
și fără modificări de schemă, conform AGENTS.md.

### 2026-10-17 — Previzualizare PDF pe întreg spațiul disponibil

PYTHON/static/css/vercon-preview.css: dialogul PDF din VERCON ocupă întregul
viewport, fără margini sau colțuri rotunjite, cu bara de acțiuni deasupra și
vizualizatorul extins în spațiul rămas. Selectorul specific aplicației are
prioritate peste lățimea implicită de 420px din confirm-box.css, încărcat
ulterior. Butoanele barei au prioritate peste min-width-ul comun al
confirmărilor și se distribuie pe rânduri doar când spațiul o cere.

Stare: IMPLEMENTAT ÎN SURSE. Doar analiză de surse/design; fără teste sau
probă în browser, conform AGENTS.md. Pentru publicare este necesar numai
fișierul vercon-preview.css, fără restart de serviciu. Fără acces la baze
de date sau publicare pe server.

### 2026-10-17 — Macheta NIR după referința vizuală a utilizatorului

Referința cerută înlocuiește aspectul generic al PDF-ului NIR: A4 orizontal,
titlu centrat și unitate dedesubt, monospațiat cu diacritice, tabel compact
pentru factură/recepție/comandă cu antet reunit, furnizor/adresă în același
chenar cu textul de recepție, documente însoțitoare pe o singură bandă,
opt coloane de produse fără coloana separată TVA, TOTALURI numai în cele
două celule din dreapta, diagonală în spațiul liber și certificarea comisiei
în partea de jos. Nu se copiază datele exemplului ca valori fixe; unitatea,
furnizorul, comisia și subsolul provin din document/configurare. Numărul de
pagini este cel real, nu valoarea 3 din captura exemplu.

Noul report_reception.py construiește documentele din rândurile deja filtrate
pe context și desenează formularul. Paginația folosește înălțimile măsurate,
repetă antetul și antetul produselor și păstrează totalul numai la sfârșitul
documentului. Textul lung se încadrează pe mai multe rânduri; o linie/antet
care nu încape întoarce REPORT_LAYOUT, fără trunchiere tacită. Cantitățile
conform documentului ascund doar zerourile finale, fără rotunjire la întreg.
Codul articolului revine la identificatorul variantei dacă nu există COD;
numărul NIR păstrează subnumărul exprimat în sutimi. Totalurile folosesc
report_model.total și precizia gestiunii, fără recalcularea valorilor salvate.

report_layouts.py: formular implicit nir-classic pentru NIR/NIR_SFP, opt
coloane, font 11pt, titlu 24pt și orientare orizontală. report_validation.py
acceptă explicit formularul; report_service.py include documentele în
snapshot; report_output.py selectează rendererul. Restul rapoartelor folosesc
rendererul existent. Definițiile personalizate din instance/ nu sunt
rescrise; cele vechi fără form rămân generice. Rendererul clasic fixează
structura celulelor după referință, editorul liber rămâne pentru generic.

Adăugate static/fonts/DejaVuSansMono.ttf, DejaVuSansMono-Bold.ttf și
DejaVuSansMono-Oblique.ttf din distribuția Matplotlib v3.10.7, cu licența
DejaVu deja inclusă. PYTHON/.pushignore și tools/Push-Changed.ps1 le includ
explicit. README-ul rapoartelor documentează formularul și publicarea.

Stare: IMPLEMENTAT ÎN SURSE, fără probă vizuală de PDF sau teste, conform
AGENTS.md. Skillul PDF a fost citit pentru proiectarea machetei; regula
utilizatorului de a nu executa verificări are prioritate față de fluxul de
randare/validare al skillului. Analiza de surse a inclus contractul Table /
minRowHeights din dependența locală, fără execuția rendererului. Nu se afirmă
potrivire vizuală verificată cu exemplul. Necesită publicarea modulelor
Python și a celor trei fonturi, repornirea serviciului și regenerarea
previzualizării. Fără acces/execuții SQL, modificări de schemă sau publicare
pe server în această intervenție.

### 2026-10-17 — Corecție orientare NIR: A4 portret și zoom inițial 100%

La precizarea utilizatorului, orientarea orizontală din intervenția anterioară
este înlocuită cu A4 portret. report_reception.py folosește A4 atât pentru
calculul lățimii/înălțimii utile, cât și pentru pagina PDF. Tabelele păstrează
proporțiile în lățimea portret, iar zona de certificare/subsolul se raportează
la noua înălțime. report_layouts.py: NIR/NIR_SFP portret, font implicit 8pt,
titlu 16pt; textele auxiliare din renderer sunt ajustate pentru noua lățime.

createPdfView din static/js/portal/pdfview.js acceptă initialScale, cu
valoarea implicită page-width păstrată pentru portal. VERCON/pdf-preview.js
transmite page-actual: fiecare raport deschis pornește la 100%, folosind
același mod ca butonul existent 100%. Mărirea/micșorarea și potrivirea
lățimii rămân disponibile. README-ul rapoartelor reflectă cerința corectată.

Stare: IMPLEMENTAT ÎN SURSE; fără teste, PDF de probă sau browser, conform
AGENTS.md. Necesită publicarea celor două module Python și a celor două
module JS modificate, repornirea serviciului pentru Python și regenerarea
previzualizării. Fără execuții SQL, modificări de schemă sau publicare pe server.

### 2026-10-17 — FCS folosește exact macheta NIR

report_layouts.default trimite documentele storno la aceeași definiție NIR,
inclusiv titlul, coloanele, antetul, fonturile, totalurile și certificarea.
report_store._path folosește aceeași configurare layout-nir.json pentru
NIR și NIR_Storno la citire/salvare, deci machetele nu pot diverge după o
editare în administrare. Eventualul layout-nir_storno.json vechi este
păstrat, fără rescriere sau ștergere, dar nu mai este consultat. Versiunile
și istoricul de configurare rămân cele ale machetei comune.

report_service include și familia storno în datele rendererului comun
nir-classic. report_reception rămâne aceeași implementare A4 portret, fără
ramificații vizuale pentru FCS. Selecția și autorizarea documentelor storno
rămân specifice raportului NIR_Storno; cantitatea este preluată exact din
sursă, fără negare suplimentară sau abs. Calculul existent al valorii storno
din cantitate/preț, unde tabela nu are total stocat, nu a fost modificat.

Limitare observată în surse și comunicată utilizatorului: documents.py,
_clean_stornolines, validează cantități pozitive, iar create_storno / edit_storno
le salvează astfel. Rendererul nu transformă acele valori în negative.
Această intervenție privește macheta, fără schimbarea convenției de salvare
sau a datelor existente. README-ul rapoartelor documentează macheta comună.

Stare: IMPLEMENTAT ÎN SURSE; fără teste sau PDF de probă, conform AGENTS.md.
Necesită publicarea report_layouts.py, report_store.py, report_service.py
și report_reception.py, restartul serviciului și regenerarea raportului.
Fără SQL executat, modificări de schemă sau publicare pe server.

### 2026-10-17 — Cantitate și valori negative la afișarea FCS

Utilizatorul a corectat convenția: cantitățile storno sunt salvate pozitive,
iar raportul trebuie să arate stornarea cu minus. report_data.py adaugă
storno_display, aplicat numai sursei explicite a documentelor FCS după
conversia exactă a valorilor. Copia pentru raport primește -abs pentru
quantity, value, vat și header_value; prețul unitar rămâne neschimbat.
Valorile deja negative nu își schimbă semnul, NULL rămâne NULL și zero nu
primește minus. Operațiile Decimal copy_abs/copy_negate nu rotunjesc cifre.
Rendererul comun, blocurile pentru rezultat și exportul Excel primesc
aceleași linii semnate, deci valoarea și totalul FCS sunt negative.

Nu se modifică datele salvate, validarea de introducere sau convenția de
stoc. Ramurile centralizatoarelor/referatelor, care aplicau deja scăderea
stornării, nu primesc o a doua negare. document-report.js normalizează
Cant/Valoare/ValoareCUTVA/TVA și în copiile pentru previzualizarea de rezervă
(inclusiv cititorul fără dreptul distinct de raport), fără a modifica
obiectele formularului. README-ul rapoartelor reflectă convenția actualizată.

Stare: IMPLEMENTAT ÎN SURSE; fără teste, PDF de probă sau browser, conform
AGENTS.md. Necesită republicarea report_data.py și document-report.js,
restartul serviciului și regenerarea raportului. Fără execuții SQL,
modificări de schemă sau publicare pe server.

### 2026-10-17 — FCS: formular în ecranul normal și stoc la data documentului

Adăugarea și modificarea FCS folosesc spațiul normal de lucru al recepțiilor.
Antetul are patru coloane; Recepția stornată și Comisie ocupă fiecare două
coloane. Recepția se alege prin ComboTree, grupată pe furnizori, dintre
NIR-urile cu produse eligibile din gestiunea curentă la Data FCS. Facturile
repartizate în mai multe gestiuni oferă numai recepția și liniile locale.

Grila are coloanele Produs, UM, Cantitate, Preț, TVA %, Valoare, TVA și TOTAL,
cu rând nou permanent, selecție ComboTree și eliminarea rândurilor ca la NIR.
Produsele sunt exclusiv variantele din recepția selectată care mai au stoc
la Data FCS. Prețul și TVA sunt preluate din recepția sursă; salvarea păstrează
convenția existentă Cant pozitiv / preț cu TVA stocat în VC_IntrariStorno.
Cantitatea este limitată pe server atât de restul nestornat din NIR, cât și
de stocul variantei la Data FCS. FCS viitoare rezervă în continuare cantitatea
din sursă; documentul editat este exclus din calcul pentru a nu fi numărat
de două ori. Validarea cronologică integrală a stocului rămâne la salvare.

În editare, arborele salvat este înlocuit de mini fișa furnizorului și mini
fișa de magazie a produsului selectat, la Data FCS, cu autorizare pentru FCS.
Nu există buton de previzualizare în acest mod; Renunță este la stânga,
Salvează FCS la dreapta. În consultare, adăugarea este în antetul arborelui
FCS salvate și butonul + Adaugă de jos este eliminat.

Numerotarea anuală NIR/FCS este comună în gestiune, comparând ABS(Numar) și
NumarSub; numerele anulate rămân ocupate. Pentru o FCS nouă introdusă înaintea
unui NIR sau FCS ulterior se propune un sub-număr disponibil, cu maximum două
zecimale. Data și unicitatea se validează din nou sub blocarea tranzacției.
NIR-urile iau și FCS în calcul la propunerea următorului număr și la limitele
cronologice. Convenția existentă de stocare a numărului FCS negativ rămâne.
Se utilizează coloana existentă NumarSub; nu se creează automat scheme.

Fișiere: PYTHON/static/js/vercon/docs.js, storno-editor.js, receipt-context.js;
PYTHON/static/css/vercon.css, vercon-storno.css;
PYTHON/routes/vercon/documents.py, storno_stock.py, doc_routes.py,
receipt_context.py, receipt_number.py, receipt_rules.py.

Stare: IMPLEMENTAT ÎN SURSE. Au fost făcute doar analize de cod și de design;
fără teste, compilare, verificări automate, browser sau acces la baza de date,
conform AGENTS.md. Funcționarea în aplicație nu este confirmată prin execuție.
Necesită publicarea surselor și repornirea serviciului Python. Nu s-a publicat
pe server și nu s-au executat SQL sau modificări de schemă MariaDB.

### 2026-10-17 — Sesiune web configurabilă, reînnoită la refresh

Durata sesiunii portalului, comună VERCON/ADECHIT/K-BOT web, se citește la
pornirea serviciului din PORTAL_SESSION_MINUTES în config.py. Valoarea
implicită este 30 de minute. Se acceptă un întreg strict pozitiv; o setare
invalidă produce o eroare explicită la pornire. Setarea se adaugă manual în
config.py pe server. Fișierul exemplu local creat inițial a fost eliminat la
cererea utilizatorului. Configurația privată a serverului nu a fost accesată
și nu a fost modificată.

POST /api/portal/session/renew reînnoiește numai o sesiune încă valabilă.
Se păstrează tokenul, identitatea și unitatea deschisă; renewed_at reține
începutul noii perioade, separat de momentul autentificării issued_at.
Timerul comun VERCON/ADECHIT și monitorul portalului solicită reînnoirea la
inițializarea paginii. Refreshul/încărcarea paginii începe o nouă perioadă de
durata configurată. Pollingul, revenirea într-un tab și activitatea obișnuită
nu repornesc termenul. Sesiunile expirate necesită autentificare din nou.
Fereastra existentă de reutilizare a codului după verificarea parolei rămâne
separată de durata sesiunii și nu se prelungește prin refresh.

Contoarele confirmă cu serverul expirarea înainte de deconectare, pentru a
respecta o reînnoire făcută într-o altă pagină care folosește același token.
Avertismentele locale sunt retrase/reprogramate când termenul este prelungit.

Fișiere: PYTHON/routes/portal/session_settings.py, portal.py;
PYTHON/static/js/adechit/session.js;
PYTHON/static/js/session/session-monitoring.js,
session-monitoring-extension.js, session-monitoring-timers.js.

Stare: IMPLEMENTAT ÎN SURSE, fără teste sau verificări de execuție, conform
AGENTS.md. Necesită publicarea modulelor modificate, configurarea pe server
și repornirea serviciului Python. Fără publicare pe server, acces la baza
de date, execuții SQL sau modificări de schemă MariaDB.

### 2026-10-17 — FCS fără selecții repetate, cache mini fișe și eliminare Listare NIR

ComboTree Produs din FCS exclude combinațiile produs + preț cu TVA deja
introduse în DGV. Lista se construiește din ciorna actuală la fiecare
deschidere; eliminarea sau schimbarea unui rând eliberează combinația.
Același produs la alt preț rămâne disponibil. Prețurile sunt normalizate
exact la cele patru zecimale stocate, fără comparare prin numere binare.
Validarea de salvare pe server și protecția la editarea celulei refuză
repetarea aceleiași combinații, inclusiv dacă are alt identificator de variantă.

Mini fișele de magazie din editorul comun NIR/FCS sunt păstrate într-un cache
în memoria instanței editorului: maximum 128 de fișe, valabilitate două minute
de la răspuns, cu eliminarea celor utilizate cel mai demult când se atinge
limita. Cheia include parametrii compleți ai fișei: contextul, data și
varianta sau produsul/prețul/TVA, după familie. Revenirea pe un rând cu o
fișă validă o afișează imediat. Cererile încă în curs sunt reutilizate;
erorile nu sunt păstrate. Răspunsurile sosite după o invalidare nu repopulează
cache-ul. Nu se utilizează localStorage sau sessionStorage.

Cache-ul se golește la schimbarea ciornei/documentului, ieșirea din editare
(inclusiv după salvare), schimbarea contextului și distrugerea editorului.
Schimbarea datei/prețului/variantei utilizează altă cheie. Fișele sunt doar
informative; verificarea stocului la salvare rămâne pe server.

Listare a fost eliminată atât din submeniul NIR, cât și din meniul grupat
INTRARI care adăuga aceeași opțiune fără acțiune.

Fișiere: PYTHON/static/js/vercon/app.js, docs.js, storno-editor.js,
receipt-context.js, statement-cache.js; PYTHON/routes/vercon/documents.py.

Stare: IMPLEMENTAT ÎN SURSE. Fără teste, verificări automate, browser sau
acces la baza de date, conform AGENTS.md. Necesită publicarea modulelor
modificate și restartul serviciului pentru validarea Python. Nu s-au făcut
publicări pe server, execuții SQL sau modificări de schemă MariaDB.

### 2026-10-17 — Cache mini fișe de magazie configurabil în config.py

Configurația cache-ului NIR/FCS se citește din config.py la pornirea
serviciului: VERCON_MINI_STOCK_CACHE_ENABLED este True implicit, iar
VERCON_MINI_STOCK_CACHE_SECONDS este 120 implicit. False dezactivează
cache-ul; valoarea 0 pentru durată îl dezactivează de asemenea. Activarea
trebuie să fie bool, durata un întreg nenegativ de secunde; valorile invalide
produc o eroare explicită la pornire.

Endpointul lookups trimite editorului numai aceste două valori publice;
restul configurației nu este expus. Editorul aplică politica la încărcarea
datelor și la schimbarea contextului. Durata nu mai este fixată la două
minute în JavaScript. Când este dezactivat, răspunsurile nu sunt păstrate
în cache și revenirile pe alte rânduri cer din nou fișele de la server.
Limita de 128 de fișe și regulile de invalidare existente se păstrează când
cache-ul este activ.

Nu s-a creat niciun nou fișier de exemplu pentru configurare. Exemplul
PYTHON/config_sesiune.example.py creat anterior a fost eliminat, la cererea
utilizatorului, pentru a nu fi inclus accidental în publicarea pe server.
Setările de pus în config.py au fost furnizate direct în conversație.

Fișiere: PYTHON/routes/vercon/receipt_context.py, doc_routes.py;
PYTHON/static/js/vercon/docs.js, receipt-context.js, statement-cache.js;
eliminat PYTHON/config_sesiune.example.py.

Stare: IMPLEMENTAT ÎN SURSE. Fără teste, verificări automate, browser sau
acces la baza de date, conform AGENTS.md. Necesită publicarea modificărilor,
setarea valorilor în config.py și restartul serviciului Python. Config.py
nu a fost citit sau modificat; fără publicare pe server ori modificări MariaDB.

### 2026-10-17 — FCS fără comparație între valoarea din antet și totalul liniilor

În docs.js, totals se oprește pentru familia storno după actualizarea
totalurilor DGV. Nu se mai calculează diferența între valoarea salvată în
antet și suma liniilor și nu se mai creează mesajul de reconciliere de sub
grilă, nici la vizualizare, nici la adăugare/modificare. Totalurile produselor
și comportamentul celorlalte familii rămân cele existente.

Fișier modificat: PYTHON/static/js/vercon/docs.js.
Stare: IMPLEMENTAT ÎN SURSE. Fără teste, verificări de execuție, browser sau
acces la baza de date, conform AGENTS.md. Necesită publicarea modulului JS
modificat. Nu s-au publicat surse și nu s-au modificat scheme sau date MariaDB.

### 2026-10-17 — Bonuri de consum: arbore, editare în pagină și mini fișe

Arborele BC este grupat după Primitor și lună, cu data și numărul bonului
în etichetă și valoarea cu două zecimale aliniată la dreapta, ca la NIR.
Identificatorii ramurilor sunt stabili; după salvare/anulare se deschide
ramura documentului selectat. Butonul comun de adăugare este în antetul
arborelui, iar „+ Adaugă” din bara de jos a fost eliminat pentru BC.

Adăugarea și modificarea BC rămân în pagina curentă. Primitorul folosește
Combobox cu tastare și săgeată, categoria folosește ComboTree cu selecția
unui singur cont din gestiunea curentă. Primul rând are Primitor, Categorie,
Număr și Data, iar Explicații ocupă primele trei coloane ale rândului doi.
În editare, Renunță este la stânga, Salvează bonul la dreapta, iar acțiunea
de previzualizare a ciornei a fost eliminată. Previzualizarea documentelor
salvate rămâne disponibilă.

DGV folosește mecanismul NIR cu rând gol permanent și ignoră rândurile goale
la salvare; acțiunea „Adaugă rând” din footer a fost eliminată pentru BC.
Produsul rămâne Combobox cu tastare, cu trecere la Cantitate după selecție.
„Disponibil” este înainte de Cantitate; stocul informativ se citește la data
bonului și în consultare. În modificare, cantitățile documentului curent
sunt excluse din calculul disponibilului, ca în comportamentul existent.

Coloana laterală din editare afișează fișa gestionarului selectat și fișa
de magazie a produsului de pe rândul curent. Fișa gestionarului citește
bonurile și retururile salvate, neanulate, din contextul/exercițiul curent
până la data bonului: ultimele 12 documente și totalul consumului net
(BC minus retururi). Fișa de magazie reutilizează serviciul de stoc și
politica de cache configurabilă din config.py. Fișele nu includ ciorna.
Noile citiri respectă dreptul BC și contextul subunitate/gestiune/exercițiu.

Fișiere: PYTHON/static/js/vercon/docs.js, consumption-ui.js,
receipt-context.js; PYTHON/static/css/vercon.css, vercon-consumption.css;
PYTHON/routes/vercon/receipt_context.py, doc_routes.py;
implementare_vercon_v2.md.

Stare: IMPLEMENTAT ÎN SURSE. Au fost făcute numai citiri de cod și analiză
de design. Fără teste, verificări automate, rulare în browser sau acces
la baza de date, conform AGENTS.md. Necesită publicarea surselor și
restartul serviciului Python. Nu s-au publicat surse și nu s-au modificat
scheme sau date MariaDB.

### 2026-10-17 — BC: disponibil consecvent, fonturi și spațiul mini fișelor

Diferența dintre Disponibil în consultare și modificare provenea din
excluderea BC-ului salvat numai în modificare. Acum ambele moduri cer
stocul la data aleasă fără consumul BC-ului curent, folosind aceeași
excludere de document. La adăugare nu se exclude niciun document salvat;
celelalte consumuri și retururi rămân în calcul. Această regulă înlocuiește
comportamentul de consultare descris în intrarea precedentă.

Comboboxurile VERCON (câmpul și opțiunile listei) folosesc variabila
--vc-combobox-font-size, cu 1px mai mică decât dimensiunea anterioară,
păstrând diferențierea existentă între desktop și ecranele mici.

Mini fișele NIR/FCS/BC folosesc același font UI și dimensiunea comună
--vc-mini-statement-font-size = calc(var(--fs-sm) - 1pt), inclusiv pentru
antet, identitate, note, tabel, mesaje și total. Tabelele au layout fix și
coloane explicite pentru dată și cifre. Document/Factură primește numai
lățimea rămasă; textele lungi sunt pe un singur rând, cu elipsă și text
complet în tooltip. Corpul permite numai derulare verticală și rezervă
spațiul scrollbarului, fără extinderea tabelului pe orizontală.

Antetele NIR și BC au aceeași înălțime minimă comună pe desktop, calculată
pentru trei rânduri de câmpuri și două intervale de 12px. Câmpurile BC
rămân pe primele două rânduri, iar spațiul liber de dedesubt mărește și
fișa gestionarului, a cărei limită inferioară urmărește începutul DGV.
Pe ecranele mici rămâne organizarea responsive existentă.

Fișiere: PYTHON/static/js/vercon/docs.js, receipt-context.js;
PYTHON/static/css/vercon-controls.css, vercon-documente-layout.css,
vercon-receipt-context.css; implementare_vercon_v2.md.

Stare: IMPLEMENTAT ÎN SURSE. Numai analiză de cod și design; fără teste,
verificări automate, rulare în browser sau acces la baza de date, conform
AGENTS.md. Necesită publicarea fișierelor JS/CSS modificate. Nu s-au
publicat surse și nu s-au modificat scheme sau date MariaDB.

### 2026-10-17 — Lista de alimente: antet, mese și arbore

Antetul listei are două coloane: Număr listă și Data pe primul rând,
Comentarii pe ambele coloane. Sub Comentarii sunt trei TextBox-uri
needitabile (Valoare listă, Total BC, Diferență), actualizate din ciornă.
DGV-ul separat al acestor trei totaluri a fost eliminat. Antetul și
bonurile asociate împart acum zona de sus în două părți egale.

În fiecare masă, sumarul are numai Porții, Preț și Valoare. Coloana D
nu mai apare în pagină; valoarea istorică rămâne păstrată la salvare.
Tabelele felurilor au FELUL, Explicații și meniul de rând; coloanele
ordinal și Porții au fost eliminate. Explicații ocupă spațiul rămas.
Adăugarea se face prin rândul gol permanent în editare, fără acțiune de
adăugare în footer. Rândurile complet goale nu se trimit la salvare.
Previzualizarea ciornei folosește aceleași denumiri și omite rândul gol.

Explicații folosește Combobox cu tastare liberă și sugestii pentru felul
selectat. Sugestiile se citesc din textele listelor neanulate ale
subunității/gestiunii/exercițiului curent și includ textele ciornei.
Textul nou se salvează odată cu lista și devine sugestie reutilizabilă.
Editorul comun DGV acceptă opțiunea allowCustom; doar coloanele care o
activează păstrează textul liber și dezactivează alegerea automată a
primei sugestii. Celelalte coloane își păstrează comportamentul.

Butonul comun ActionAddButton este în antetul arborelui și respectă
dreptul de scriere/perioada închisă. Butonul + Adaugă din bara paginii
a fost eliminat. Arborele are două niveluri: Lună/An (lunile în română)
și lista cu numărul și data în etichetă, valoarea la dreapta ca la NIR.
Ramurile lunilor au identificatori stabili.

Fișiere: PYTHON/static/js/vercon/lista.js;
PYTHON/static/css/vercon-lista.css, vercon-lista-totals.css;
PYTHON/routes/vercon/lista.py; PYTHON/static/js/dgv/editing.js;
PYTHON/static/js/components/combobox/combobox-search.js;
implementare_vercon_v2.md.

Stare: IMPLEMENTAT ÎN SURSE. Numai analiză de cod și design; fără teste,
verificări automate, rulare în browser sau acces la baza de date,
conform AGENTS.md. Necesită publicarea surselor și restartul serviciului
Python. Nu s-au publicat surse și nu s-au modificat scheme sau date MariaDB.

### 2026-10-17 — Lista de alimente: derulare orizontală și bara de editare

Antetele coloanelor înguste actions și ordinal aveau padding lateral de
22 px, ceea ce impunea o lățime efectivă de minimum 45 px, chiar când
DGV calcula o coloană mai îngustă. Antetul depășea astfel suma lățimilor
rândurilor și provoca derulare orizontală cu spațiu gol în dreapta,
inclusiv fără suficiente rânduri pentru derulare verticală.

În Lista de Alimente, paddingul lateral al acestor două antete este
acum zero. Corecția se aplică atât bonurilor asociate, cât și celor trei
tabele de feluri; mecanismul de derulare și calculul lățimilor rămân
active pentru situațiile în care conținutul chiar depășește grila.
Butonul Previzualizare a fost eliminat din bara de adăugare/modificare.

Fișiere: PYTHON/static/css/vercon-lista.css;
PYTHON/static/js/vercon/lista.js; implementare_vercon_v2.md.

Stare: IMPLEMENTAT ÎN SURSE. Numai analiză de cod și design; fără teste,
verificări automate sau rulare în browser, conform AGENTS.md. Necesită
publicarea fișierelor JS/CSS modificate. Nu s-au publicat surse și nu
s-au modificat scheme sau date MariaDB.

### 2026-10-17 — Lista de alimente: totaluri compacte și tipografie

Grupul Valoare listă / Total BC / Diferență este centrat sub Comentarii,
cu lățime totală de cel mult 480 px și trei câmpuri egale. Etichetele și
valorile sunt centrate. Pe ecrane mici, câmpurile se așază vertical,
cu lățime maximă de 180 px. Valorile folosesc formatMoney: separator
de mii punct, virgulă zecimală și două zecimale (de exemplu 2.592,00).
TextBox-urile needitabile afișează textul formatat inclusiv când primesc
focus; calculele și valorile trimise la salvare își păstrează precizia.

Toate etichetele câmpurilor din antetul Listei de Alimente sunt bold.
Fontul antetelor DGV este redus cu 1 pt față de fs-table, iar cel al
celulelor/rândurilor/footerului cu 1 pt față de fs-ms. Editorii de celule
moștenesc fontul celulelor. Selectorii sunt limitați la Lista de Alimente.

Fișiere: PYTHON/static/js/vercon/lista.js;
PYTHON/static/css/vercon-lista.css, vercon-lista-totals.css;
implementare_vercon_v2.md.

Stare: IMPLEMENTAT ÎN SURSE. Numai analiză de cod și design; fără teste,
verificări automate sau rulare în browser, conform AGENTS.md. Necesită
publicarea fișierelor JS/CSS modificate. Nu s-au publicat surse și nu
s-au modificat scheme sau date MariaDB.

### 2026-10-17 — Lista de alimente: Număr și Data mai înguste

Câmpurile Număr listă și Data ocupă acum 70% din lățimea coloanei lor,
reducere orizontală de 30%, păstrând poziționarea în antet.

Fișiere: PYTHON/static/css/vercon-lista.css; implementare_vercon_v2.md.
Stare: IMPLEMENTAT ÎN SURSE. Fără teste sau verificări de execuție,
conform AGENTS.md. Necesită publicarea CSS-ului modificat.

### 2026-10-17 — Lista de alimente: economii și depășiri anterioare

Sub cele trei totaluri existente sunt două TextBox-uri needitabile:
Economii anterioare și Depășiri anterioare. Sunt compacte și centrate,
cu separator de mii și două zecimale, inclusiv la focus. Coloana fără
sold rămâne goală, conform recapitulării raportului.

Calculul comun este în food_balance.py și este folosit atât de raportul
Listei de Alimente, cât și de noua citire GET /api/vercon/doc/lista/balance.
Se reutilizează sursele configurate food_history și settings ale raportului,
cu același context subunitate/gestiune/exercițiu. Soldul anterior în semnul
raportului este SI_LA minus suma DEP a listelor cu data strict mai mică
decât data listei; soldul negativ devine economie, cel pozitiv depășire.
Sursa implicită exclude listele anulate. Diferențele lipsă produc aceeași
eroare ca în raport, fără înlocuire artificială cu zero.

Valorile se citesc la deschiderea listei, adăugare/modificare și la schimbarea
datei. În modificare este exclusă lista curentă, inclusiv când data ei este
mutată după data salvată. Răspunsurile întârziate nu se aplică altei liste
sau altei date. Citirea respectă dreptul Alimente și contextul serverului;
calculele folosesc Decimal și nu modifică documentele sau soldul inițial.

Fișiere: PYTHON/routes/vercon/food_balance.py (nou), report_food.py,
doc_routes.py; PYTHON/static/js/vercon/lista.js;
PYTHON/static/css/vercon-lista-totals.css; implementare_vercon_v2.md.

Stare: IMPLEMENTAT ÎN SURSE. Numai analiză de cod și design; fără teste,
verificări automate, rulare în browser sau acces la baza de date,
conform AGENTS.md. Necesită publicarea surselor și restartul serviciului
Python. Nu s-au publicat surse și nu s-au modificat scheme sau date MariaDB.

### 2026-10-17 — Lista de alimente: recapitulare pe cinci coloane

Recapitularea urmează exemplul primit: sold anterior, Valoare listă,
Total bon consum, Diferență și sold final. Fiecare coloană are 150 px,
cu eticheta bold deasupra valorii, separatoare și fontul aplicației.
Blocul este centrat pe un singur rând, sub zona antet/bonuri, pe ambele
coloane ale zonei de sus. La lățimi insuficiente se poate derula blocul,
fără comprimarea coloanelor. Înlocuiește cele trei totaluri urmate de
cele două câmpuri distincte pentru economii/depășiri anterioare.

Prima etichetă este Economii Anterioare SAU Depășiri Anterioare, iar
ultima Economii SAU Depășiri, după semnul soldului în convenția raportului.
Valorile acestor două câmpuri sunt absolute, cu separator de mii și două
zecimale. Pentru sold zero se folosește convenția raportului: Depășiri.
Până la primirea calculului anterior, câmpurile de sold rămân fără valoare.

Soldul final = soldul anterior - diferența listei (Valoare listă - Total BC),
echivalent cu rândul La finele perioadei din raport. Se recalculează și în
ciornă când se schimbă mesele sau bonurile. Suma soldului anterior și a
diferenței cu semn invers folosește receiptTotal, cu aritmetică BigInt la
precizia de stocare de patru zecimale; afișarea păstrează două zecimale.

Fișiere: PYTHON/static/js/vercon/lista.js;
PYTHON/static/css/vercon-lista-totals.css; implementare_vercon_v2.md.

Stare: IMPLEMENTAT ÎN SURSE. Numai analiză de cod și design; fără teste,
verificări automate sau rulare în browser, conform AGENTS.md. Necesită
publicarea fișierelor JS/CSS modificate. Nu s-au publicat surse și nu
s-au modificat scheme sau date MariaDB.

### 2026-10-17 — Lista de alimente: recapitularea în interiorul antetului

vc-lista__totals-wrap, cu vc-lista__totals în interior, este acum copil
al vc-grid vc-doc__header, imediat după Comentarii, conform structurii
cerute. Nu mai este copil separat al vc-lista__overview.
CSS-ul ajustat de utilizator a fost păstrat fără modificări.

Fișiere: PYTHON/static/js/vercon/lista.js; implementare_vercon_v2.md.
Stare: IMPLEMENTAT ÎN SURSE. Fără teste sau verificări de execuție,
conform AGENTS.md. Necesită publicarea modulului JS modificat.

### 2026-10-17 — Lista de alimente: editori FELUL și Explicații

Coloana FELUL are lățime fixă 57 px prin configurația DGV și padding zero
în antet prin CSS. Dropdown-ul are minimum 120 px pentru citirea opțiunilor.
Lista opțiunilor se calculează la deschiderea editorului și omite felurile
folosite pe alte rânduri ale aceleiași mese; rândul curent nu se exclude.
După eliminarea unui rând, felul revine între opțiuni. Salvarea locală a
celulei refuză și o alegere duplicată. La intrarea în celula FELUL a unui
rând fără fel, dropdown-ul se deschide automat după încheierea clickului.

Logul primit arată un blur urmat de click/deschidere și apoi expirarea
unui timer de 150 ms. Comboboxul readonly păstrează focusul la mousedown,
iar timerul de blur nu mai închide o listă redeschisă ulterior și respectă
focusul din dropdown. DGV păstrează focusul editorului când reconstruiește
rândurile virtualizate. Deschiderea automată verifică editorul și focusul
curent înainte de a afișa lista.

Containerul real al editorului este simultan dgv__editor și
combobox-container. Regula comună CSS acoperă acum și acest caz, eliminând
limita implicită de 300 px și lăsând editorul să ocupe toată celula.
În Lista de alimente, celula editată și inputul Explicații au padding zero;
opțiunile dropdown-ului sunt pe un singur rând, cu elipsă, fără hscroll.
CSS-ul recapitulării modificat de utilizator nu a fost schimbat.

Fișiere: PYTHON/static/js/vercon/lista.js;
PYTHON/static/js/dgv/editing.js, datagrid.js;
PYTHON/static/js/components/combobox/combobox-events.js;
PYTHON/static/css/dgv.css, vercon-lista.css; implementare_vercon_v2.md.

Stare: IMPLEMENTAT ÎN SURSE. Numai analiza logului, a codului și a designului;
fără teste, verificări automate sau rulare în browser, conform AGENTS.md.
Necesită publicarea fișierelor JS/CSS modificate. Nu s-au publicat surse
și nu s-au modificat scheme sau date MariaDB.
### 2026-10-17 — Raport Lista de alimente: antet și recapitulare

S-a eliminat secțiunea tipărită Bonuri de consum asociate; citirea bonurilor
și refuzul unui bon anulat se păstrează. Intervalul raportului nu mai este
tipărit sub titlul ListaAlimente2015; filtrarea perioadei rămâne activă.
Numărul/data listei sunt centrate în PDF, rezultatul în pagină și Excel.
Recapitularea (drepturi, cheltuieli, porții) are fontul cu 1 pt mai mic
decât configurația coloanei sau fontul raportului, cu minimum 5 pt.
Alocația se afișează cu 0 zecimale, Efectiv (inclusiv totalul), Economii și
Depășiri cu 2 zecimale. Calculele și valorile sursă își păstrează precizia.
Regulile se aplică la generare și machetelor salvate, fără rescrierea lor.

Limitele textului din PDF.js se corectează prin invalidarea cache-ului de
font după redimensionarea canvasului de măsurare. Redimensionarea readuce
fontul real la 10px, dar cache-ul vechi împiedica aplicarea fontului cerut
la primul text de aceeași dimensiune cu proba de ascendență. Titlul își
păstrează dimensiunea, centrarea și desenarea PDF; se corectează selecția.

Fișiere: PYTHON/routes/vercon/report_food.py, report_service.py,
report_model.py, report_body.py, report_output.py, report_validation.py;
PYTHON/static/js/vercon/report-result.js;
PYTHON/static/css/vercon-rapoarte.css;
PYTHON/static/vendor/pdfjs/pdf.min.js și README.md; implementare_vercon_v2.md.
Stare: IMPLEMENTAT ÎN SURSE. Ulterior, utilizatorul a cerut server local
pentru testare și a ales date demonstrative. În acel mediu s-au deschis
formularul și PDF-ul, s-au observat soldurile și formatarea cerută, iar
exportul Excel a răspuns HTTP 200. Limitele titlului din textLayer au trecut
de la aproximativ 844px la 281px pe pagina de 792px (scaleX 3,06172 → 1,02057).
Fără suită de teste, fără publicare și fără acces MariaDB.

### 2026-10-17 — Server local VERCON cu date demonstrative

La cererea explicită pentru probă pe localhost, cu alegerea datelor demo,
s-a adăugat tools/VerconPreview: launcher PowerShell în prim-plan/fundal,
oprire cu identificarea procesului, pagină de intrare, date fictive în
memorie și endpoint-uri numai pentru Lista de alimente și raportul ei.
Folosește interfața reală și generarea report_service/report_food/PDF/Excel,
cu DemoSource injectat explicit. Semnătura report_service.build acceptă
opțional sursa datelor; fluxul de producție păstrează Source SQL implicit.

Serverul nu importă main.py, nu înregistrează blueprint-urile SQL și nu
deschide conexiuni MariaDB. Datele, salvările și snapshot-urile există
numai în memorie; nicio schemă sau configurare de raport nu se modifică.
Ascultă numai pe 127.0.0.1:5053, cu verificarea Host și Origin la scrieri.
Portul 5052 era ocupat de alt proces, care nu a fost modificat sau oprit.

Fișiere noi: tools/VerconPreview/preview_server.py, demo_data.py,
index.html, preview.css, Start-VerconPreview.ps1, Stop-VerconPreview.ps1,
README.md și .gitignore. Backend comun: report_service.py.

Stare: PORNIT LOCAL pe http://localhost:5053/. Verificări limitate la
cererea explicită de probă: health, deschiderea formularului, soldurile,
generarea/vizualizarea PDF și răspunsul exportului Excel. Nu s-a rulat
suita de teste. Salvarea demonstrativă este pentru interfață și nu
validează stocuri, drepturi, tranzacții sau concurența din MariaDB.
Celelalte module Vercon nu sunt încă incluse în acest preview.

### 2026-10-17 — Raport Lista de alimente: tabel compact și semnături la bază

Spațiul de 2 mm dintre banda ALIMENTE și antetul tabelului a fost eliminat
și adăugat deasupra benzii, după mesele servite. Banda se păstrează cu
tabelul următor la paginare. Rândul Feluri de mâncare a fost eliminat,
păstrând etichetele meselor și explicațiile salvate.

În tabelul de alimente, antetul are fontul cu 1 pt mai mic, iar conținutul
cu 2 pt mai mic decât configurația inițială a coloanei/fontul raportului,
cu minimum 5 pt. Cantitățile, prețurile și valorile au două zecimale.
TOTAL ALIMENTE este footer al tabelului comun celor două jumătăți,
cu eticheta la stânga, suma la dreapta și fundalul antetului. Totalul se
calculează din valorile originale și se formatează numai la afișare.
Footerul nu se separă la paginare de ultimul rând de alimente.

Casetele finale Întocmit/Confirmat/Gestionar/Bucătar sunt aliniate la
baza cadrului ultimei pagini, deasupra subsolului, prin TopPadder.
Grupul final nu se fragmentează între pagini. Aprobările din partea
superioară a documentului își păstrează poziția.

Modelul comun transmite separat header_size și vizibilitatea antetului.
Excel preia dimensiunile fonturilor, ascunderea rândului Feluri de mâncare
și footerul cu suma la dreapta; lățimile continuă prin modulul comun.
Rezultatul HTML ascunde antetul felurilor și afișează noul footer.

Fișiere: PYTHON/routes/vercon/report_food.py, report_body.py,
report_output.py, report_validation.py;
PYTHON/static/js/vercon/report-result.js;
PYTHON/static/css/vercon-rapoarte.css; implementare_vercon_v2.md.

Stare: IMPLEMENTAT ÎN SURSE și încărcat pe serverul demonstrativ local
http://localhost:5053/. Designul PDF pentru lista demo 130 a fost examinat
în browser, inclusiv baza paginii. Fără suită de teste sau alte verificări
automate; cazurile cu mai multe pagini și noul export Excel nu au fost
executate. Fără acces MariaDB și fără publicare pe serverul real.

### 2026-10-17 — Raport Lista de alimente: grilă pentru pix și delimitări antet

Secțiunea din dreapta antetului a fost înlocuită cu grila pentru completare
manuală din referință: Mese servite / Mic dejun / Dejun / Cină și rândurile
Efectiv la hrană, Porții preparate, Rămase, Distribuite. Toate cele 12 celule
de completat sunt șiruri goale, fără preluarea porțiilor sau distribuitului,
fără zerouri sau marcaje de valoare lipsă. Antetul grilei este alb, iar
etichetele sunt accentuate. Structura se aplică și machetelor vechi salvate,
fără rescrierea configurației pe disc sau a documentelor.

Casetele Aprobat și Vizat compartiment financiar contabil au fiecare
jumătate din lățimea anterioară (25% din lățimea utilă), la extremitățile
paginii, cu spațiu alb de 50% între ele. Înălțimea pentru semnare rămâne
14 mm. Spațiul dintre casete nu are chenar sau separator orizontal.

Cele două delimitări verticale dintre secțiunile antetului au 1,5 pt,
echivalentul a 2 px CSS. Numai ultimul rând gol al recapitulării centrale
are cele trei celule reunite, eliminând liniile verticale interioare;
marginile secțiunii și liniile celorlalte rânduri rămân.

Fișiere: PYTHON/routes/vercon/report_food.py, report_layouts.py,
report_body.py, report_output.py;
PYTHON/static/js/vercon/report-result.js;
PYTHON/static/css/vercon-rapoarte.css; implementare_vercon_v2.md.
Excel preia grila goală, antetul alb și celulele reunite ale recapitulării.
Rezultatul HTML preia casetele de aprobare separate.

Stare: IMPLEMENTAT ÎN SURSE și încărcat în preview-ul local de pe 5053.
Designul PDF al listei demo 130 a fost examinat în browser. Fără teste
sau verificări automate și fără execuția noului export Excel; fără
acces MariaDB sau publicare pe serverul real.

### 2026-10-17 — Raport Lista de alimente: linii uniforme și footer în prima secțiune

Antetul are acum linii uniforme de 0,75 pt (1 px CSS), fără separatoarele
de 2 px dintre secțiuni. Antetul grilei pentru completare cu pixul este
colorat cu același gri #ececec ca celelalte secțiuni; celulele de completat
rămân goale. Ultimul rând din prima secțiune este un footer gri: primele
trei celule sunt reunite pentru eticheta TOTAL, eliminând cele două linii
verticale interioare, iar valoarea rămâne la dreapta în ultima coloană.
Unirea rândului gol din secțiunea centrală se păstrează.

Rendererul PDF păstrează culoarea și accentuarea la nivel de celulă când
unește tabelele paralele; doar prima secțiune are footerul colorat.
Fișiere: PYTHON/routes/vercon/report_food.py, report_output.py;
implementare_vercon_v2.md.

Stare: IMPLEMENTAT ÎN SURSE, serverul demo 5053 repornit și PDF-ul listei
130 regenerat. Designul a fost examinat în browser. Fără teste sau alte
verificări automate, fără acces MariaDB și fără publicare pe serverul real.

### 2026-10-17 — Rapoarte: antet VERCON cu meniu, document și dată

Antetul comun al previzualizărilor afișează butonul VERCON cu iconița
preluată din antetul principal și același meniu de aplicații. Navigarea
reutilizează protecția modificărilor nesalvate și eliberarea ocupării.
Butonul copiat nu duplică ID-ul vc-apps; meniul se deschide în dialog.

Titlul este urmat de data documentului pentru NIR, BC, retur, FCS,
celelalte documente și listele de alimente, inclusiv ciorne. În pagina
Rapoarte, selecția unui document afișează eticheta lui cu data în format
românesc; selecțiile multiple afișează numărul documentelor și perioada,
iar rapoartele centralizate afișează perioada. Previzualizările de stocuri,
mijloace fixe și amortizare transmit perioada sau data corespunzătoare.
Antetul permite trecerea pe mai multe rânduri pe ecrane înguste.
Modificarea privește fereastra de previzualizare; macheta PDF rămâne
cea a raportului.

Fișiere: PYTHON/static/js/vercon/application-menu.js, report-header.js,
app.js, ui.js, pdf-preview.js, document-report.js, docs.js, lista.js,
reports.js, stock.js, depreciation.js, mf.js, editor.js;
PYTHON/static/css/vercon-preview.css; implementare_vercon_v2.md.

Stare: IMPLEMENTAT ÎN SURSE. Fără teste sau verificări runtime, conform
instrucțiunilor proiectului. Fără modificări în baza de date.

### 2026-10-17 — NIR, BC, retur și FCS: ordonare alfabetică în arbore

Furnizorii și primitorii de la primul nivel al arborilor de documente
salvate sunt ordonați alfabetic după eticheta afișată, cu regulile limbii
române și fără diferențiere între majuscule și minuscule. Ordonarea se
aplică la afișare și reîmprospătare, inclusiv după încărcarea unei ramuri
NIR. Ordinea existentă a datelor/lunilor și documentelor din interiorul
ramurilor se păstrează.

Fișiere: PYTHON/static/js/vercon/docs.js; implementare_vercon_v2.md.
Stare: IMPLEMENTAT ÎN SURSE. Nu s-au rulat teste sau alte verificări,
conform instrucțiunilor proiectului. Fără modificări în baza de date.

### 2026-10-17 — Obiecte de inventar: conturi multiple, BP/BT/BS și rapoarte

La cererea utilizatorului, s-au implementat completările OI, cu selecție
independentă între conturile de magazie și cele de folosință. Nu există un
singur cont implicit pentru gestiune. BP păstrează perechea sursă–destinație
aleasă; BT/BS pot lua loturi de pe conturi diferite, păstrând pe fiecare
linie contul, proveniența și prețul. Loturile cu prețuri diferite rămân
distincte chiar când au aceeași variantă și același document de primire.

Editorul OI este în pagină, cu arborele ascuns în editare, grile compacte,
alegere prin arbore cont–produs/lot, adăugare și totaluri în footer. Casarea
nu are primitor. Disponibilul se calculează la data bonului și ține cont
de documentele ulterioare; serverul revalidează cronologia în tranzacție.
Protecția documentelor dependente, drepturile, motivul anulării și ciorna
la eșec folosesc mecanismele existente. Valorile ciornelor OI se calculează
exact cu BigInt; prețurile istorice și calculul serverului rămân păstrate.

Evidența în folosință folosește contul mișcării, separat de contul produsului
din magazie. Rapoartele OI au fost activate: bonuri și centralizatoare
BP/BT/BS, balanță, solduri, fișe/situații pe deținător/produs și inventariere.
Conturile în folosință au filtru propriu. Perioada are arbore multianual de
la primul NIR/BP, cu numai ultimul an expandat inițial. PDF și Excel
folosesc snapshotul comun și modulul comun de lățimi Excel. Sursele SELECT
suplimentare și machetele sunt disponibile și în KBotAdmin. MF rămâne
amânat pentru rapoarte. Nu s-a presupus o clasificare a conturilor după
prefix și nu s-a schimbat nomenclatorul sau schema.

Fișiere backend: PYTHON/routes/vercon/oi.py, oi_custody.py, doc_routes.py,
protocol.py, stock_source.py, stock.py, capabilities.py, report_catalog.py,
report_data.py, report_oi.py, report_routes.py, report_service.py,
report_layouts.py, report_body.py, report_store.py;
PYTHON/routes/admin_cli/vercon_reports.py.
Fișiere frontend: PYTHON/static/js/vercon/docs.js, oi-ui.js, oi-values.js,
consumption-ui.js, stock.js, reports.js, report-selection.js,
report-result.js, document-report.js;
PYTHON/static/css/vercon-oi.css, vercon.css.
Resurse: PYTHON/reports/vercon/queries/oi_documents.sql,
custody_ledger.sql, oi_accounts.sql, oi_holders.sql; README.md din același
director de rapoarte. Detalii în
docs/worklog/SLICE-VC07-OI-conturi-documente-rapoarte.md.

Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Fără teste, verificări de sintaxă,
build, browser, HTTP, acces MariaDB ori publicare. Machetele OI și fluxurile
pe date reale așteaptă probele cerute ulterior de utilizator. Nu s-a
modificat schema și nu s-a adăugat creare automată de tabele. Serverul
demonstrativ existent nu a fost extins sau repornit pentru această lucrare.

### 2026-10-18 — Rapoarte: ComboTree perioadă, limite DI/DSF și ramuri inițiale

Perioada folosește controlul comun ComboTree, cu tastare/căutare după an
sau lună. Numai cel mai recent an cu documente este expandat la deschiderea
listei; ceilalți ani pornesc închiși. Pentru Materiale/Alimente se păstrează
numai exercițiul selectat în antet, conform precizării utilizatorului.
Istoricul OI/MF ia în calcul NIR/FCS (data documentului și a facturii),
ieșiri/retururi și toate bonurile BP/BT/BS, inclusiv transferuri și casări.
Listele de alimente contribuie la limite pentru Alimente. Se expun numai
anii cu documente; în lipsa documentelor se folosește anul selectat.

Catalogul transmite limitele reale minime/maxime ale datelor. Conform
precizării ulterioare a utilizatorului, alegerea anului/lunii completează
DI/DSF cu perioada calendaristică integrală: 1 ianuarie–31 decembrie,
respectiv prima–ultima zi a lunii, indiferent de mișcări. Toate lunile
anilor disponibili sunt selectabile. Limitele calendarelor sunt extinse
la începutul primului an și sfârșitul ultimului an disponibil. Datele se
pot introduce și manual. Calendarele comune acceptă limite și ani disponibili,
dezactivează zilele și navigarea în afara intervalului și afișează o grilă
de ani în același popup la click pe an, cu paginare pentru istorice lungi.
Limitele sunt aplicate și la tastare. Confirmarea unei date neschimbate
nu mai emite schimbări care ar reîncărca inutil selecția raportului.

Arborele tipurilor de rapoarte pornește cu rădăcinile deschise (nivel 0)
și grupurile de la nivelul 1 închise. Rapoartele MF rămân indisponibile,
conform catalogului și meniului existente; calculul perioadei recunoaște
documentele comune OI/MF, fără activarea unor rapoarte noi.

Fișiere: PYTHON/routes/vercon/report_period.py, report_routes.py;
PYTHON/static/js/vercon/report-period.js, reports.js;
PYTHON/static/js/components/datepicker/datepicker.js;
PYTHON/static/css/datepicker.css, vercon-rapoarte.css;
implementare_vercon_v2.md.

Stare: IMPLEMENTAT ÎN SURSE, NERULAT. S-a analizat designul și integrarea
cu controalele existente. Nu s-au rulat teste, verificări de sintaxă,
build, browser, HTTP sau interogări MariaDB, conform instrucțiunilor
proiectului. Fără modificări de schemă ori creare automată de tabele.

### 2026-10-18 — Rapoarte: arbore tipuri mai îngust cu 40%

În aranjarea cu trei coloane, lățimea coloanei cu tipurile de rapoarte
scade de la 250–340 px la 150–204 px (60% din lățimea anterioară).
Spațiul eliberat revine coloanei de selectare date. Aranjările responsive
existente pentru ecrane mici se păstrează.

Fișiere: PYTHON/static/css/vercon-rapoarte.css; implementare_vercon_v2.md.
Stare: IMPLEMENTAT ÎN SURSE. Fără teste sau verificări runtime,
conform instrucțiunilor proiectului.

### 2026-10-18 — Rapoarte: Selectare date închis la rădăcini și încărcare lazy

Coloana Selectare date primește în continuare tot spațiul rămas în grila
cu trei coloane. Arborele pornește cu toate rădăcinile închise și citește
inițial numai definițiile filtrelor raportului. Ramurile se încarcă la
expandare; Produse se citește pe niveluri cont–produs–variantă, cu filtre
SQL parametrizate pentru contul/produsul cerut și cu delimitarea existentă
la subunitate, gestiune și exercițiu. Sursele SELECT configurabile rămân
folosite. Listele de furnizori, primitori/deținători, conturi de folosință
și documente se citesc numai când se solicită ramura corespunzătoare.

Bifarea unui grup citește descendenții lui înainte de selectare. Căutarea
de minimum trei caractere încarcă ramurile rămase pentru a căuta și în
datele încă neafișate. Ramurile citite se reutilizează pe durata selecției
curente. La schimbarea perioadei, se revalidează numai ramurile care aveau
bife și rădăcinile pornesc din nou închise. Cererile vechi sunt anulate la
schimbarea raportului/perioadei sau închiderea paginii. Exportul așteaptă
încărcarea selecției. Erorile unei ramuri permit reîncercarea la redeschidere.
API-ul anterior de selecție completă rămâne disponibil fără parametrul branch.

Corecție pentru eroarea 1054 raportată de utilizator din server: în
catalogul perioadelor, DATALA se citește din antetul VC_LABC, conform
sursei existente food_history.sql, în loc de VC_ListaAlimente.

Fișiere: PYTHON/routes/vercon/report_selection.py, report_routes.py,
report_data.py, report_query.py, report_period.py;
PYTHON/static/js/vercon/report-selection.js, reports.js;
implementare_vercon_v2.md.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Analiză de design a integrării cu
arborele comun și sursele SQL existente. Fără teste, verificări de sintaxă,
build, browser, HTTP ori interogări MariaDB, conform instrucțiunilor
proiectului. Fără modificări de schemă ori creare automată de tabele.

### 2026-10-18 — OI: corecție alias SQL la listarea BP/BT/BS

Logul furnizat pentru lista bonurilor de casare indică eroarea MariaDB
1064 la aliasul SQL neîncadrat `lines`. Interogarea comună de listare
folosește acum aliasul `line_count`, iar răspunsul API păstrează câmpul
`lines` așteptat de interfață. Corecția se aplică tuturor familiilor
BP, BT și BS care folosesc aceeași interogare.

Fișiere: PYTHON/routes/vercon/oi.py; implementare_vercon_v2.md.
Stare: CORECTAT ÎN SURSE, NERULAT. Analiză de design pe baza logului și
a sursei; fără teste, verificări runtime sau acces MariaDB, conform
instrucțiunilor proiectului. Fără modificări de schemă.


### 2026-10-18 — VC06-04: retururi si linii storno migrate fara sursa

Stare: **IMPLEMENTAT ÎN SURSE, NERULAT**. Implementarea cerintelor 3 si 3b din
`VERCON/CODEX_SCHIMBARI_MIGRATOR_2026-10-18.md`.

Retururile cu `Retur=1` si `IdRetur IS NULL` apar cu **Fără bon sursă** in
arbore si antet. In consultare, operatorul alege **Bonul returnat** si apasa
**Asociază cu un bon de consum**. Ruta `POST /api/vercon/doc/br/<id>/link`
valideaza contextul, starea, versiunea returului, tipul/starea BC si toate
produsele, insumand cantitatile pe varianta. Foloseste disponibilul din
`return_eligible(..., exclude=doc_id)`; mesajul `source` enumera produsele
absente sau cantitatile neincadrate. Asocierea schimba numai `IdRetur` si
`Version`, scrie audit `br.link` si intoarce versiunea/documentul actualizat.
Nu modifica liniile, preturile, valorile sau stocul si nu apeleaza
`check_timeline`. **Modifică** solicita asocierea prealabila pentru un retur
orfan; dupa asociere se folosesc regulile normale. Anularea BR ramane pe
fluxul existent, care nu solicita bonul sursa si aplica regulile de stoc.

In **Factură storno**, butonul **Linii de storno fără recepție** deschide lista
liniilor orfane, inclusiv cele anulate: produs, UM, cantitate, pret cu TVA,
data salvarii si stare. Operatorul poate lega individual o linie de o FCS
existenta sau o poate anula cu motiv. Rute: `GET /api/vercon/doc/storno/orphans`
si `POST /api/vercon/doc/storno/orphans/<id>/link|cancel`. Asocierea cere FCS
neanulata si receptie sursa valida din acelasi context; disponibilul include
cantitatile deja stornate, inclusiv liniile existente pe FCS aleasa, si limita
de stoc la data FCS, prin regulile storno existente. Pastreaza cantitatea si
pretul liniei. Cresc versiunea liniei si versiunea antetului FCS, prevenind
suprascrierea de catre un editor deschis anterior. `check_timeline`, actualizarea
si auditul `storno.link` se executa in aceeasi tranzactie. Anularea orfanei
scrie motiv, utilizator, data si audit; nu introduce o miscare in stoc.

Toate scrierile cer dreptul familiei, context verificat, blocarea curenta,
perioada deschisa si versiunea citita. UI trimite cheia cererii, pastreaza
aceeasi cerere la raspuns pierdut si ofera reluarea ei. Nu se fac asocieri sau
anulari automate. Ajutorul web descrie pasii operatorului (slice VC06-04).

Criterii de acceptare pregatite, **TOATE NERULATE**:

| Scenariu | Rezultat asteptat |
| --- | --- |
| BR fara `IdRetur`, consultare | Marcaj Fără bon sursă in lista si antet; alegerea BC disponibila numai cu drept si document activ. |
| BR orfan, Modifica / PUT direct | Mesaj explicit de asociere prealabila, fara pierderea liniilor. |
| Asociere BR cu BC potrivit | `IdRetur` completat, versiune +1, audit `br.link`; liniile, valorile si stocul neschimbate. |
| BC fara produs sau cantitate ramasa insuficienta | Refuz la `source` cu toate produsele incompatibile; nicio modificare. Cantitatile mai multor linii ale aceleiasi variante sunt insumate. |
| BR dupa asociere, Modifica | Editorul normal foloseste BC asociat si cantitatile eligibile. |
| BR inainte/dupa asociere, Anuleaza | Fluxul obisnuit cu motiv si versiune, fara dependenta de existenta sursei; regulile existente de stoc raman aplicabile. |
| Anularea BC cu retur asociat activ | Blocata de `_guard_dependents` pana la anularea returului. |
| Lista storno fara receptie | Afiseaza liniile din context, cantitate, pret, data salvarii, stare; cele anulate raman vizibile si nu se pot lega. |
| Legare storno de FCS potrivita | Legatura si versiunile cresc; audit; linia incepe sa conteze in stoc numai dupa commit. |
| Storno: produs absent, depasire, sursa nevalida sau cronologie negativa | Refuz si rollback integral, inclusiv versiunea antetului. |
| Anularea unei linii storno orfane | Motiv obligatoriu, `Anulat=1`, versiune +1 si audit; ramane in lista, fara efect in stoc. |
| Context diferit, perioada inchisa, blocare lipsa, versiune veche | Refuz fara modificari. |
| Raspuns pierdut la asociere/anulare orfana | Reia aceeasi cerere cu aceeasi cheie, fara dublarea efectului sau auditului. |

Fisiere: `PYTHON/routes/vercon/documents.py`, `doc_routes.py`,
`migration_links.py` (nou); `PYTHON/static/js/vercon/docs.js`,
`orphan-storno.js` (nou), `app.js`; `PYTHON/static/css/vercon.css`,
`vercon-migration.css` (nou); `VERCON/VERCON_STATUS.md`;
`docs/worklog/SLICE-VC06-04-br-si-fcs.md`; `implementare_vercon_v2.md`.

Limite: doar analiza de design prin citirea surselor si a dumpului local de
schema. Fara teste, build, verificare de sintaxa, browser, HTTP, interogari
MariaDB ori verificari automate. Nicio modificare de schema, nicio creare
automata de tabele. `config.py`, rapoartele, AGENTS.md si migratorul nu au fost
modificate in aceasta lucrare. Sursele sunt locale, fara deploy/commit/push.

Decizie deschisa, NEIMPLEMENTATA: identificatorii OI BP/BT/BC numerotati separat
in migrator pot produce coliziuni intre loturi. Avertismentul este inclus in
ajutor; pana la decizia operatorului pentru contorul unic in migrator, loturile
OI migrate nu se folosesc ca reper. Nu s-au schimbat fluxurile subunitatilor,
IDM/IdMasinaP/IDBO, NumarSub sau CodFiscalSursa.

### 2026-10-18 — OI: versiunea listei ca text și jurnalizare după serializare

Diagnosticul SQL executat de utilizator în 000_DEMO a confirmat charset
și collation binary pentru expresia CONCAT a versiunii la BP și BS.
Lista comună BP/BT/BS citește acum separat MAX(ID) și SUM(Version),
apoi construiește în Python textul versiunii în același format ca
_rev: id-maxim–sumă-versiuni, cu separatorul ASCII '-'. Răspunsul API
păstrează cheia version, fără a expune rezultatul binar al CONCAT.

În guard-ul VERCON, mesajul success se emite după pregătirea răspunsului
JSON, astfel încât o eroare de serializare să nu mai fie precedată de
un succes pentru aceeași cerere.

Fișiere: PYTHON/routes/vercon/oi.py, PYTHON/routes/vercon/__init__.py;
implementare_vercon_v2.md.
Stare: CORECTAT ÎN SURSE, NERULAT. Fără teste, verificări automate,
browser, HTTP ori interogări MariaDB executate de asistent, conform
instrucțiunilor proiectului. Schema, datele și VRC.Migrator nu au fost
modificate în această corecție. Fără deploy.

### 2026-10-18 — Meniu: selecție progresivă Subunitate / Gestiune / An

La deschiderea paginii, dintre selectorii contextului se afișează doar
Subunitate. Endpoint-ul context cere alegerea explicită a subunității,
inclusiv când există una singură; celelalte apeluri păstrează comportamentul
anterior al resolverului. După alegerea subunității, o gestiune unică se
selectează automat pe server și selectorul ei rămâne ascuns. Selectorul
Gestiune apare numai pentru subunități cu mai multe gestiuni. Anul apare
după stabilirea gestiunii, cu ultima valoare disponibilă selectată conform
regulii existente; operațiile continuă să provină din contextul serverului.

Gestiunea și anul sunt ascunse și în HTML-ul inițial, înaintea încărcării
JavaScript. Controalele ascunse sunt dezactivate, iar resetarea contextului
golește opțiunile vechi. Ajutorul din aplicație descrie noua succesiune.
Se păstrează confirmarea modificărilor nesalvate la schimbarea contextului.

Fișiere: PYTHON/routes/vercon/context.py, __init__.py;
PYTHON/static/vercon.html; PYTHON/static/js/vercon/app.js;
PYTHON/static/css/vercon-controls.css; implementare_vercon_v2.md.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Analiză de design prin citirea
surselor; fără teste, verificări automate, browser, HTTP ori interogări
MariaDB, conform instrucțiunilor proiectului. Fără modificări de schemă,
migrator, deploy, commit sau push.

### 2026-10-18 — Meniu comun și Fluxuri / Mișcări pentru OI/MF

Configurări, Intrări și Rapoarte sunt comune tuturor tipurilor de gestiune.
Intrări grupează NIR și Factură Stornare pentru toate tipurile, cu deschiderea
paginii de consultare. Ieșiri apare la Alimente/Materiale cu Bon Consum și
Bon Retur; Lista Alimente rămâne exclusiv la Alimente. OI/MF primesc
Fluxuri / Mișcări cu Bon Predare, Bon Transfer și Bon Casare; MF adaugă
Amortizare și Registru MF. Grupurile folosesc disponibilitatea și motivul
dezactivării primite de la server; verificarea la deschiderea directă
păstrează aceleași reguli de tip, drepturi și ocupare.

Rapoartele MF nu mai sunt excluse global din catalog și din formular.
Pentru raportul Amortizare se reutilizează proiecția existentă depreciation_read,
cu întregul istoric până la data calculului, filtre de variante și observații
pe activ. Macheta implicită are coloane de durate, valori și stare, fără
totalizarea implicită a activelor neeligibile. Opțiunea Amortizare din meniu
deschide direct acest raport, cu un singur câmp La data și exporturile comune
PDF/Excel. Din Rapoarte, calculul folosește data de sfârșit a perioadei, explicată
în notele raportului. Exportul Excel păstrează modulul comun de lățimi.
Rapoartele de deținere MF sunt grupate în arbore sub Mijloace fixe.

Fișiere: PYTHON/static/js/vercon/app.js, reports.js, report-result.js;
PYTHON/routes/vercon/capabilities.py, report_catalog.py, report_data.py,
report_layouts.py, report_service.py, report_routes.py;
implementare_vercon_v2.md.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Analiză de design prin citirea
surselor; fără teste, verificări automate, browser, HTTP ori interogări
MariaDB, conform instrucțiunilor proiectului. Fără modificări de schemă,
migrator, deploy, commit sau push.

### 2026-10-18 — Bonuri OI/MF: arbore direct și conturi BP

Arborii BP/BT/BS afișează direct deținătorul și bonurile, fără nivelul lunii.
BT se grupează după primitor; BS după predător, fiind document fără primitor.
Gruparea folosește identitatea deținătorului, iar bonurile sunt ordonate după
număr descrescător, apoi dată și ID, cu număr și valoare în nod.
În BP, contul de folosință este sub contul de magazie, în aceeași coloană.
Listele țin cont de TIP OIM/OIF sau MFM/MFF; conturile neclasificate (NULL)
rămân eligibile conform regulilor existente. Un singur cont eligibil este
selectat automat și controlul este disabled, prin mecanismul comun existent.

Fișiere: PYTHON/static/js/vercon/oi-ui.js; PYTHON/static/css/vercon-oi.css;
PYTHON/routes/vercon/oi.py.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Numai analiză de design din surse;
fără teste, verificări automate, browser, HTTP sau interogări MariaDB.

### 2026-10-18 — Produse MF și clasificări din catalogul comun

La MF, grila produselor ocupă toată lățimea disponibilă; cele două panouri
din dreapta nu mai sunt montate. Celula CC are selector cu aceeași fereastră,
căutare și arbore ca CPV, parametrizat pentru Clasificare MF. Catalogul este
citit exclusiv din AVACONT_COMUN.VC_ClasificariMF (Cod, Denumire, DNF_MIN,
DNF_MAX); intervalul de durată este prezentat în ani. Se salvează codul CC
prin protocolul produselor existent, cu validarea existenței în catalog.
Nu se creează și nu se modifică structura catalogului comun.

Fișiere: PYTHON/static/js/vercon/product-nomenclator.js, cpv-picker.js,
nomenclator-grid.js; PYTHON/static/css/vercon-products.css;
PYTHON/routes/vercon/mf_classifications.py, nom_routes.py, nomenclatoare.py.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Structura catalogului nu a fost
interogată; coloanele provin din referințele și contractele existente.
Fără teste, verificări automate, browser, HTTP sau interogări MariaDB.

### 2026-10-18 — Produse OI: fișa traseului și deținerea curentă

Fișa inferioară din dreapta arată traseul variantei de produs: recepție,
storno, predare, transfer între gestionari/deținători și casare, cu date,
documente, cantități, cont și gestiunea curentă. Utilizatorul a clarificat
că solicitarea privește gestionarii/deținătorii, nu transferuri între
alte gestiuni sau subunități. Sunt folosite identitățile produsului și
variatelor din context, fără asocieri după denumire.

Istoricul este multianual și păstrează toate documentele active. BC-ul
generat de BP nu dublează predarea în fișă. Transferul este afișat o dată,
cu sursa și destinația; soldurile curente folosesc separat registrul
magaziei și cel al deținătorilor, până la data curentă. Cantitățile rămase
sunt afișate în magazie și la fiecare deținător/cont. Documentele viitoare
pot apărea în istoric, dar nu intră în poziția curentă.

Fișiere: PYTHON/routes/vercon/product_movements.py, nomenclator_stock.py;
PYTHON/static/js/vercon/product-stock.js; PYTHON/static/css/vercon-products.css.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Fără teste, verificări automate,
browser, HTTP sau interogări MariaDB; numai analiză de design din surse.

### 2026-10-18 — Baza de date: Feluri numai la Alimente și TIP controlat

Feluri de mâncare este oferit numai gestiunilor ALIMENTE, inclusiv prin
filtrarea nomenclatoarelor la deschiderea formularului și refuzul API-urilor
directe de listare/scriere pentru celelalte tipuri. În lipsa panoului Feluri,
TVA ocupă întreaga înălțime a coloanei din dreapta.

TIP este un combo comun în grila categoriilor: NULL este permis peste tot;
MAGAZIE/FOLOSINȚĂ reprezintă OIM/OIF la OI și MFM/MFF la MF; ALIMENTE apare
numai la Alimente; Materiale permite numai NULL. Alte valori sunt respinse
la salvare pe server. Valorile vechi nu sunt modificate automat.

Fișiere: PYTHON/routes/vercon/nomenclatoare.py, nom_routes.py, __init__.py;
PYTHON/static/js/vercon/nomenclator-grid.js;
PYTHON/static/css/vercon-nomenclatoare.css.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Fără teste, verificări automate,
browser, HTTP sau interogări MariaDB; fără migrare automată a datelor.

### 2026-10-18 — Intrări: antet rearanjat și consultare/editare MF

Număr NIR și Data NIR schimbă poziția cu Număr factură și Data facturii,
pentru toate gestiunile. ValoareFC devine Valoare Reală Factură și este
poziționată sub data facturii; protocolul și calculul valorii se păstrează.

La MF, consultarea folosește coloanele obișnuite NIR. Arborele furnizorilor
ocupă 70% din coloana dreaptă, iar restul este panoul Informații Mijloc Fix,
legat de rândul selectat: Număr inventar, Data intrare, Durata normală
folosire, Durata folosită, Valoare lunară, cu etichete și casete readonly.
La adăugare/modificare, coloana dreaptă este ascunsă complet și grila se
extinde. Coloanele MF suplimentare vizibile sunt Nr. Inv., Data MF,
Durata Folosită și Val. Luna. Durata normală se preia din produs și rămâne
în payload/validare; valoarea lunară păstrează calculul existent. Grila MF
are preferințe separate pentru consultare și editare.

Fișiere: PYTHON/static/js/vercon/docs.js, receipt-mf-info.js;
PYTHON/static/css/vercon-receptie.css, vercon-receipt-destinations.css.
Stare generală: IMPLEMENTAT ÎN SURSE, NERULAT. Fără teste, verificări
automate, browser, HTTP sau interogări MariaDB, conform instrucțiunilor
proiectului. Fără DDL, modificări de schemă, deploy, commit sau push.

### 2026-10-18 — Intrări: conturi MAGAZIE, produse noi în DGV și reguli MF

La adăugare în MF/OI, catalogul conturilor este limitat la gestiunea curentă
și la conturile MAGAZIE (MFM/OIM, respectiv eticheta istorică MAGAZIE).
Mai multe conturi ale aceleiași destinații pot fi bifate; unicul cont eligibil
este autoselectat și controlul dezactivat. Serverul aplică aceleași restricții
pentru produsele recepționate și pentru cererile de repartizare.

DGV recunoaște denumirea unui produs nou în editorul arbore, activează UM și
continuă cu clasificarea obligatorie pentru MF, apoi cu CPV opțional pentru
toate tipurile. Închiderea CPV fără alegere cere confirmarea avertismentului.
Închiderea clasificării fără alegere oprește continuarea și păstrează rândul
incomplet. Dacă sunt bifate mai multe conturi, după UM se cere contul produsului
nou dintre cele bifate. Produsele noi au identități temporare în ciornă și sunt
inserate numai în tranzacția de salvare a NIR-ului, o singură dată pentru toate
copiile care folosesc același produs nou.

MF: Nr. Inv. obligatoriu, propus din maximul numerelor numerice salvate/local,
cu verificare în ciornă și pe server. Data MF implicit din Data NIR, obligatorie,
centrată și fără depășirea datei NIR. Cantitatea este 1, ne-editabilă. DF este
obligatorie, implicit 0. DNF devine vizibilă și editabilă în limitele clasificării
transformate din ani în luni. DNF, DF, V. Luna au aceeași lățime și tooltipurile
cerute. Formula VBA (Pret + Pret * TVA) / DNF folosește rotunjirea half-even
la 4 zecimale atât în ciornă, cât și pe server; la corecția automată se păstrează
valoarea brută corectată. Afișarea valorii lunare are exact 4 zecimale, inclusiv
în panoul de consultare, independent de preferințele generale.

Meniul rândului MF include Multiplică rând: copie imediat după sursă, identitate
nouă de linie/activ, Nr. Inv. incrementat până la unul liber și focus pe copie.
Catalogul produselor și numerele de inventar se reîncarcă după salvarea NIR.

Fișiere: PYTHON/routes/vercon/receipt_products.py, receipt_destinations.py,
receipts.py, receipt_rules.py; PYTHON/static/js/vercon/receipt-product-entry.js,
receipt-allocation.js, docs.js, cpv-picker.js, receipt-mf-info.js;
PYTHON/static/js/dgv/editing.js, datagrid.js;
PYTHON/static/css/vercon-receptie.css, vercon-receipt-destinations.css.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Numai analiză de design din surse;
fără teste, build, verificări automate, browser, HTTP sau interogări MariaDB.
Fără DDL, modificări de schemă, deploy, commit sau push.

### 2026-10-18 — Combouri: denumirea selecției unice

Controlul comun TreeView/ComboTree cu bife afișează denumirea elementului
când există o singură selecție, inclusiv la autoselectarea contului unic și
când controlul este dezactivat. Rezumatul cu numărul selecțiilor este folosit
în continuare la selecții multiple. Câmpul intern rămâne gol pentru a păstra
comportamentul de căutare al controlului.
Fișier: PYTHON/static/js/components/treeview/treeview-checkable.js.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; fără teste sau alte verificări.

### 2026-10-18 — Intrări MF: DGV fără Cantitate și Cotă TVA

În gestiunile MF, grila NIR de consultare și editare nu mai construiește
coloanele Cantitate și Cotă TVA (IDTVA). Lățimea lor implicită cumulată,
90 + 100, se transferă coloanei Produs (234 -> 424), păstrând ponderile
celorlalte coloane. Versiunea layoutului MF devine v3 pentru ca preferințele
anterioare să nu restaureze vechea lățime a produsului. Cantitatea 1 și cota
TVA rămân în modelul rândului, în validări, calcule și payloadul de salvare;
selectorul Cotă TVA din antet și coloana cu valoarea TVA se păstrează.
Fișier: PYTHON/static/js/vercon/docs.js.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; fără teste sau alte verificări.

### 2026-10-18 — MF: mesaj DNF cu clasificare și limite; status centrat

Mesajul pentru DNF în afara intervalului indică acum codul clasificării și
limitele în luni: Pentru clasificarea cccc durata (in luni) de folosire minima
este nn si maxima mm. Același format este folosit în interfață și pe server.
Lipsa clasificării sau a limitelor valide are un mesaj distinct, fără limite
inventate. Textul vc-save-status este centrat; în containerul mesajelor
documentului, statusul ocupă lățimea disponibilă pentru centrarea efectivă.
Fișiere: PYTHON/static/js/vercon/receipt-product-entry.js;
PYTHON/routes/vercon/receipt_products.py, receipts.py;
PYTHON/static/css/vercon-controls.css.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; fără teste sau alte verificări.

### 2026-10-18 — Clasificări MF: selecție numai pentru elemente cu DNF

Selectorul comun de clasificări MF oferă plusul de selecție numai pentru
înregistrările cu limite DNF valide. Regula se aplică indiferent de nivel
(root, nod intermediar, frunză), atât în arbore, cât și în rezultatele căutării.
Elementele fără DNF rămân vizibile și pot fi expandate. Handlerul selecției
refuză aceleași elemente. Catalogul CPV păstrează comportamentul existent.
Fișier: PYTHON/static/js/vercon/cpv-picker.js.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; fără teste sau alte verificări.

### 2026-10-18 — DGV: ștergerea erorii după validarea/anularea rândului

Mesajul de eroare din vc-save-status este eliminat când rândul devine valid,
la părăsirea cu succes a rândului, la anularea unui rând nou ori restabilirea
unui rând existent și după scoaterea cu succes a rândului/corecției. Mesajul
nu se șterge când operația eșuează sau când se salvează doar o celulă iar
rândul rămâne invalid. Mesajele informative sunt păstrate.
Fișier: PYTHON/static/js/vercon/docs.js.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; fără teste sau alte verificări.

### 2026-10-18 — Adăugare intrări MF: padding antet DGV de 2px

Celulele antetului DGV au padding stânga/dreapta de 2px numai în modul de
adăugare a unui NIR MF. Starea is-adding delimitează regula de consultare
și modificare; celulele produselor păstrează paddingul existent.
Fișiere: PYTHON/static/js/vercon/docs.js;
PYTHON/static/css/vercon-receptie.css.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; fără teste sau alte verificări.

### 2026-10-18 — Modificare NIR MF: numai active fără ieșire; alinieri fișă MF

La modificarea unui NIR MF, DGV-ul afișează numai activele care NU au și
ieșire neanulată din contul de intrare. Identificarea este pe IdProdus
(activ/variantă), SubunitId/GestiuneId și contul produsului; caută în
VC_Iesiri cu antet VC_Iesiri_H neanulat, neretur și cantitate pozitivă,
fără limitare la anul curent. O ieșire anulată nu ascunde activul.
Consultarea NIR-ului păstrează toate liniile. Filtrarea se aplică și
rândurilor MF din contextul facturii comune afișate în grilă.

Liniile cu ieșire rămân în ciornă și în totalul/reconcilierea facturii,
dar sunt excluse din grila de modificare și totalurile ei vizibile.
Serverul recitește starea, refuză modificarea activelor cu ieșire și
păstrează liniile originale chiar dacă lipsesc din cererea clientului.
Nu sunt șterse sau rescrise la salvarea celorlalte active. Modificarea
cotei TVA din antet și corecția totalului folosesc doar liniile editabile.

Panoul Informații Mijloc Fix: Nr. inventar și Data intrare centrate;
DNF/DF la dreapta cu unitatea Luni, valoarea lunară la dreapta,
în continuare cu exact 4 zecimale, conform imaginii utilizatorului.

Fișiere: PYTHON/routes/vercon/receipt_mf_edit.py, documents.py, receipts.py,
receipt_rules.py, receipt_invoice_view.py;
PYTHON/static/js/vercon/docs.js, receipt-invoice-view.js, receipt-mf-info.js.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; numai analiză de design din surse,
fără teste, build, verificări automate, browser, HTTP sau interogări MariaDB.
Fără modificări de schemă și fără executare SQL.

### 2026-10-18 — Bonuri MF/OI: grupare opțională după dată

Sub arborele bonurilor BP/BT/BS apare checkboxul Ordoneaza dupa Dată,
bifat implicit la deschiderea paginii. Bifat: rădăcini lună în română/an,
în ordine cronologică descrescătoare, cu TOTAL pe lună; bonurile copil
arată tip/număr, dată și valoare și sunt sortate descrescător după dată,
apoi după număr și identitate. Totalurile însumează exact valorile
bonurilor neanulate; bonurile anulate rămân vizibile cu marcajul existent.
Înregistrările fără dată sunt grupate separat la sfârșit.

Debifat: se reutilizează gruparea existentă pe gestionar, cu aceleași
etichete și ordonare ca înainte. Comutarea reface numai arborele și
păstrează documentul selectat. Checkboxul este sub zona derulabilă a
arborelui și urmează ciclul de viață al controalelor paginii.

Fișiere: PYTHON/static/js/vercon/oi-tree-order.js, docs.js;
PYTHON/static/css/vercon-oi.css.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; numai analiză de design din surse,
fără teste, build, verificări automate, browser, HTTP sau interogări MariaDB.

### 2026-10-18 — Bonuri MF/OI: număr implicit separat P/T/C și produse cu disponibil pozitiv

La adăugarea BP/BT/BS, Număr este completat cu următorul număr din seria
proprie tipului de bon. Propunerea ia maximul numerelor existente și al
contorului acelei serii, inclusiv documentele anulate. Numărul este verificat
la părăsirea câmpului și la salvare; același număr poate exista pe tipuri
diferite de bon, dar nu pe două bonuri ale aceluiași tip în aceeași gestiune.
Propunerea este doar citire; contorul avansează în tranzacția salvării,
sub blocarea contextului existentă. Numerotarea comună internă a identităților
PTC se păstrează separat de numărul afișat al fiecărui tip.

Clarificare explicită a utilizatorului: la predare se filtrează stocul din
magazie; la transfer/casare, stocul rămas la predător. Selectorul Produs
include numai disponibil pozitiv la data bonului. La modificare se exclude
mișcarea proprie a bonului (BC-ul asociat pentru BP, documentul TC în
registrul deținătorului). Se scad și cantitățile din celelalte rânduri ale
ciornei, păstrând disponibilul rândului editat. Produsele/loturile cu 0
nu mai apar și nu generează categorii goale în arbore. BP preîncarcă stocul
la deschiderea editorului și îl reîncarcă la schimbarea datei; verificările
tranzacționale de stoc și cronologie rămân obligatorii la salvare.

Logica UI a numerotării a fost separată din docs.js într-un modul dedicat,
păstrând și comportamentul NIR/FCS, pentru dimensiunea și responsabilitățile
modulului comun de documente.
Fișiere: PYTHON/routes/vercon/oi_number.py, oi.py, doc_routes.py;
PYTHON/static/js/vercon/document-number.js, oi-product-choices.js, oi-ui.js, docs.js.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; numai analiză de design din surse,
fără teste, build, verificări automate, browser, HTTP sau interogări MariaDB.
Fără DDL, modificări de schemă, commit, push sau deploy.

### 2026-10-18 — Identitate comună P/T/C și semantica registrului OI/MF

Clarificare utilizator: în OI_Produse, IDBC identifică bonul curent pentru
predare, transfer sau casare. IDBP este completat numai la predare, IDBT
numai la transfer; casarea nu are marcaj dedicat. IDBO este proveniența
BP/BT pentru transfer/casare, pe ambele laturi ale transferului.

Migratorul alocă ID-uri interne dintr-o secvență comună pe gestiune, dar
păstrează identitatea sursă distinctă după fișier și familie. Documentele,
marcajele IDBP/IDBT, IDBC curent și IDBO reutilizează aceeași mapare.
Resolver separat pentru IDBC al registrului, cu verificarea documentului
sursă; IDBC gol/0 la predare/transfer (VBA vechi) se completează din marcaj.
Referințele contradictorii, proveniențele lipsă și cele ambigue blochează
importul cu explicații. Generatorul import_map.js păstrează regulile
oi_document și origin. Versiunea regulilor planului a fost actualizată.

Aplicația Python identifică familia din IDBP/IDBT, respectiv casare dacă
ambele sunt goale; nu mai cere exclusivitate între IDBP/IDBT și IDBC.
Bonurile noi scriu IDBC pentru toate familiile. Citirea loturilor și
ștergerea registrului la modificare/anulare disting casările de celelalte
tipuri care folosesc IDBC. Pentru date deja migrate, IDBC nenul care diferă
de marcaj produce eroare explicită cu IDOI și valorile; nu se ignoră și
nu se repară automat. Numerotarea afișată a bonurilor, cantitățile,
prețurile, valorile și semnele rămân neschimbate.

Fișiere: VERCON/VRC.Migrator/VrcRunner.vb, VrcRunner.Oi.vb,
VrcImportMap.vb, import_map.json, VrcPlan.vb; VERCON/tools/import_map.js;
PYTHON/routes/vercon/oi.py, oi_custody.py.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; numai analiză de design din surse,
fără teste, build, verificări automate, browser, HTTP sau interogări MariaDB.
Fără DDL sau modificări de schemă. Datele deja migrate nu au fost modificate;
corecția importului nu le remapează retroactiv.

### 2026-10-18 — Alinierea completă a aplicației la registrul OI/MF

Citirea registrului și construirea rândurilor noi sunt centralizate în
oi_custody: identificarea familiei BP/BT/BS, IDBC comun al bonului curent,
IDBO numai pentru proveniența transferului/casării și tolerarea IDBC=0
din VBA vechi la BP/BT. Primirea prin transfer formează un lot identificat
prin transferul curent; scăderea consumă lotul indicat în IDBO.

Reîncărcarea loturilor unui bon folosește aceeași citire și verificare a
referințelor. Protecția la modificare/anulare caută transferurile/casările
ulterioare prin IDBO în evenimentele normalizate, excluzând documentul
propriu prin perechea familie/ID. Ștergerea registrului rămâne delimitată
după familie, inclusiv pentru casare, deoarece IDBC este comun.

Proveniența IDBO este verificată în bonurile BP/BT active pentru produs:
trebuie să existe, să fie neambiguă, să nu indice propriul bon și să nu
fie ulterioară mișcării. Erorile arată poziția IDOI și referința afectată.
Nu se corectează automat legăturile istorice deja migrate incorect.
Rapoartele de stoc/amortizare citesc separat tabelele documentelor P/T/C
și păstrează cheile lor specifice; nu interpretează IDBC din registru ca
o coloană dedicată casării. Nu au necesitat schimbări pentru această regulă.

Fișiere: PYTHON/routes/vercon/oi_custody.py, oi.py.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; numai analiză de design din surse,
fără teste, build, verificări automate, browser, HTTP sau interogări MariaDB.
Fără modificări de schemă sau modificări ale datelor existente.

### 2026-10-18 — Bonuri PTC: editare ca la BC și mini-fișe TC

La adăugare/modificare BP/BT/BS, bara de acțiuni are Renunță în stânga,
mesajele în centru și Salvează bonul în dreapta, fără previzualizare.
Grilele păstrează permanent rândul gol de adăugare și nu mai afișează
butonul Adaugă rând. Rândul gol nu intră în documentul trimis la salvare;
schimbarea predătorului nu mai cere confirmare pentru un simplu rând gol.

La BP, arborele rămâne vizibil și este dezactivat în timpul editării,
inclusiv adăugarea și ordonarea. În PTC, arborele și checkboxul folosesc
aceleași rânduri de layout ca grila și bara de acțiuni: arborele se termină
la baza zonei grilei, iar checkboxul rămâne în stânga, centrat pe verticală
în rândul de jos.

La BT/BS dispar din grilă coloanele Cont în folosință și Lot proveniență,
iar disponibilul se numește Disponibil. Identitatea lotului și contul sunt
păstrate în ciornă și în validarea de stoc. În editare, coloana din stânga
afișează fișa predătorului, fișa primitorului numai la BT și mișcările
produsului selectat. Istoricul include BP/BT/BS cu număr, dată și valoare,
iar pentru produs și cantitatea și traseul predător–primitor. Fișele
deținătorilor semnează negativ ieșirile și pozitiv intrările.

Citirea fișelor este exclusiv read-only, delimitată de subunitate, gestiune,
exercițiu și data bonului; bonurile anulate și ciorna nesalvată nu sunt
incluse. Cererile depășite sunt ignorate la schimbarea selecției/contextului.
Editorul sub-numărului NIR/FCS a fost separat într-un modul dedicat pentru
a păstra dimensiunea modulului comun de documente.

Fișiere: PYTHON/static/js/vercon/docs.js, oi-context.js, oi-tree-order.js,
document-subnumber.js; PYTHON/static/css/vercon-oi.css;
PYTHON/routes/vercon/oi_context.py, doc_routes.py.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; numai analiză de design din surse.
Fără teste, build, browser, HTTP sau interogări MariaDB, conform regulilor
proiectului. Alinierea vizuală nu a fost verificată în aplicație.
Fără DDL, modificări de schemă sau modificări ale datelor existente.

### 2026-10-18 — Migrator: valoare compactă pentru VC_Imports.ToolVersion

Raport utilizator: rollback la crearea contextului, înaintea migrării,
cu Data too long for column ToolVersion. Câmpul primea numele migratorului
concatenat cu identificatorul lung al regulilor. Acum primește versiunea
reală a executabilului din FileVersionInfo (0.3.7.0). Identificatorul complet
al regulilor rămâne în rezumatul importului și în manifest/hashul planului.
Nu se trunchiază identificatorul regulilor și nu se schimbă schema.

Fișiere: VERCON/VRC.Migrator/VrcRunner.vb, VRC.Migrator.vbproj.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; fără teste, build, rulare migrator
sau interogări MariaDB. Necesită recompilarea executabilului înaintea
repetării migrării. Nu s-au modificat schema sau datele existente.

### 2026-10-18 — Documentarea contractului OI/MF și a corecției ToolVersion

Creat VERCON/REGISTRU_OI_MF.md ca referință durabilă: sensurile IDBC/IDBP/
IDBT/IDBO/IDOI, exemplul predare → transfer → casare înainte și după
remapare, lotul primit prin transfer, compatibilitatea IDBC=0 din VBA vechi,
ID-urile interne comune distincte de numerele bonurilor, validările,
diagnosticul care nu mai trebuie folosit și limitele reparării istoricului.
Include eroarea ToolVersion raportată de utilizator, corecția din 0.3.7.0,
păstrarea RuleVersion și faptul că nu este necesară schimbarea schemei.

Aliniate VERCON/DICTIONAR_DATE.md, PLAN_IMPLEMENTARE.md, VERCON_STATUS.md,
VRC.Migrator/README.md și CODEX_SCHIMBARI_MIGRATOR_2026-10-18.md. Eliminate
afirmațiile depășite că identitatea comună OI/MF ar fi încă o decizie
deschisă sau că aplicația nu ar fi fost aliniată pentru această regulă.
Celelalte cerințe istorice nu sunt declarate rezolvate prin această actualizare.

Stare: DOCUMENTAT, pe baza deciziilor utilizatorului și surselor modificate.
Numai fișiere Markdown schimbate în această etapă; fără teste, build,
verificări automate, aplicație, migrator sau interogări MariaDB rulate.
Nu există confirmare de import reușit după corecția ToolVersion.

### 2026-10-18 — Bon transfer: Primitor și Predător diferiți

Selectoarele bonului de transfer exclud reciproc deținătorul ales în celălalt
câmp, la fiecare căutare, atât la adăugare, cât și la modificare. Alegerea
aceleiași persoane este respinsă și la schimbarea câmpului; validarea locală
marchează ambele câmpuri și blochează salvarea unei asemenea ciorne.
Validarea existentă pe server continuă să respingă transferurile către sine.
Adaptorul comun de câmpuri acceptă opțiuni calculate din starea curentă;
listele statice își păstrează comportamentul.

Fișiere: PYTHON/static/js/vercon/docs.js, ui.js.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; numai analiză de design din surse.
Fără teste sau alte verificări, conform instrucțiunilor proiectului.
Fără modificări de schemă sau modificări ale datelor existente.

### 2026-10-18 — Mini-fișele transfer/casare în dreapta

Corecție de poziționare cerută de utilizator: toate mini-fișele BT/BS sunt
în coloana din dreapta, cu formularul și grila în stânga. Se păstrează
ordinea predător, primitor (numai BT), mișcări produs. Ordinea DOM urmează
formularul, apoi panourile. Pe ecrane înguste se păstrează așezarea sub formular.

Fișiere: PYTHON/static/css/vercon-oi.css, PYTHON/static/js/vercon/oi-context.js.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; fără teste sau alte verificări,
conform instrucțiunilor proiectului.

### 2026-10-18 — Diagnostic disponibil BT pentru produs predat și casat

Pregătit sql/VC_DIAG_disponibil_transfer.sql pentru cazul din captură:
BADEA ION / CALCULATOR PORTABIL ASUS, BP 27 și BS 5. Scriptul identifică
contextul și varianta, afișează documentele cu semnele așteptate, registrul
brut al variantei pentru toți deținătorii și cheia de grupare folosită în
aplicație (proveniență, variantă, cont, preț), cu soldurile la data bonului.
ID-urile contextului se completează explicit; nu se alege arbitrar prima
variantă sau primul context. IDBC comun și marcajele BP/BT sunt interpretate
conform contractului documentat. Fără corecții automate sau ocolirea
registrului prin calcul paralel al disponibilului.

Stare: SQL DE DIAGNOSTIC PREGĂTIT, NERULAT. Nicio interogare MariaDB,
verificare în aplicație sau test rulat. Cauza concretă a diferenței din
captură nu este încă demonstrată. Nu s-au modificat schema sau datele.

### 2026-10-18 — Cauza confirmată a disponibilului BP 27 / BS 5

Rezultatele SQL furnizate de utilizator pentru 000_DEMO, subunitate 10,
gestiune 8, exercițiu 9, deținător 164, variantă 3878 confirmă:
BP ID 12 și BS ID 38 au sold net 0 în bonuri. Registrul conține ambele
mișcări și același lot de proveniență 12/preț 4408,1600, dar IDOI 1821 are
+1 pe IDM 34 (MF IN FOLOSINTA), iar IDOI 1848 are -1 pe IDM 33 (MF IN MAGAZIE).
Gruparea pe lot/variantă/cont/preț separă cele două rânduri: +1 pe contul 34
rămâne eligibil, iar grupa -1 de pe contul 33 nu are intrare pozitivă și
este omisă de batches. Acesta este mecanismul demonstrat al simptomului.
Originea nepotrivirii (date Access sau migrare) nu este încă demonstrată.

Extins diagnosticul cu secțiunea 6: ambele conturi din BP și mapările ROW
ale liniilor/conturilor pentru comparația cu sursa Access. Nu se schimbă
contul, proveniența sau cheia de grupare în lipsa acestei comparații.
Stare: CAUZĂ IMEDIATĂ IDENTIFICATĂ din rezultatele utilizatorului; SQL
suplimentar pregătit, nerulat de asistent. Fără teste sau alte verificări
rulate, fără modificări de cod funcțional, date ori schemă.

### 2026-10-18 — Cont OI/MF moștenit din BP până la casare, în aplicație și migrator

Utilizatorul a confirmat eroarea de cont din Access și a cerut ambele
corecții: aplicația ignoră contul TC divergent, iar migratorul îl corectează
urmărind proveniența. Reperul este contul primirii BP din registru, nu
IdMasina din documentul TC și nu contul de magazie al produsului.

Noul oi_accounts.py rezolvă recursiv IDBO prin BP/BT, pe variantă și preț,
cu memoizare și detectarea ciclurilor/proveniențelor ambigue. Citirea
registrului păstrează IDM brut separat și oferă contul efectiv moștenit.
Disponibilul și cronologia folosesc aceeași grupare cu contul efectiv;
BP-uri distincte nu se amestecă. TC se reîncarcă folosind registrul și
cantitatea/prețul liniei. La salvare, contul trimis de client nu schimbă
lotul; contul se preia din lotul eligibil și se scrie pe liniile noi.
Evidența și rapoartele păstrează cantitățile/datele sursei SQL configurate,
dar încadrează liniile pe contul rezolvat din proveniență; numele contului
este actualizat corespunzător. Nicio citire nu rescrie datele existente.

Migratorul are resolver separat pe fiecare sursă, cu index pentru primiri
și linii TC și memoizarea conturilor. BuildRow convertește IDM/IdMasina TC
înaintea remapării cheii contului; nu modifică dicționarele/fișierele Access.
Conversiile N13 consemnează originalul, ținta și contul de proveniență;
X16 blochează sursele lipsă, ciclurile sau conturile ambigue. FileVersion
este 0.3.8.0; RuleVersion VC05-OI-ACCOUNT-LINEAGE-2026-10-18.

Pentru cazul furnizat, BS ID 38/IDOI 1848 moștenește contul 34 al BP ID 12,
în loc de 33; rezultatul așteptat este +1-1=0 pe același lot și excluderea
produsului din lista eligibilă. Acesta este un rezultat de design, nu o
probă executată. SQL-ul de diagnostic păstrează explicit cheile BRUTE.

Fișiere: PYTHON/routes/vercon/oi_accounts.py, oi_custody.py, oi.py,
stock_source.py, report_data.py, report_oi.py;
VERCON/VRC.Migrator/VrcRunner.OiAccounts.vb, VrcRunner.vb, VrcPlan.vb,
VRC.Migrator.vbproj, README.md; VERCON/REGISTRU_OI_MF.md;
sql/VC_DIAG_disponibil_transfer.sql.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; numai analiză de design din surse.
Fără teste, verificări automate, build, import, HTTP sau SQL executat.
Nu s-au modificat schema, datele MariaDB sau sursa Access. Migratorul
necesită recompilare; importurile deja făcute nu sunt rescrise automat.
