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
