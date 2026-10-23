# Implementare VERCON v2

## Actualizare 19.10.2026 — loading imediat și blocarea generărilor simultane (ADE9-07)

Rapoartele PDF, documentele oficiale, ciornele și exporturile Excel pornesc
prin componenta comună înainte de generare. Fereastra apare imediat cu
indicator de încărcare; o singură operație poate fi în pregătire în aceeași
pagină. Nu există coadă de clicuri. La preview, loading-ul rămâne până când
PDF.js a desenat prima pagină; erorile sunt afișate în aceeași fereastră.
Exporturile au aceeași blocare până la pregătirea și predarea fișierului
către browser. Regenerarea existentă la snapshot expirat rămâne în aceeași
operație blocată.

Fișiere VERCON: `static/js/vercon/reports.js`, `document-report.js`,
`documents.js`, `ui.js`, `pdf-preview.js`; comune: `static/js/reporting/`,
`static/js/portal/pdfview.js`, `static/css/report-preview.css`.
[Predarea completă ADE9-07](docs/worklog/SLICE-ADE9-07-rapoarte-loading-blocare.md).
TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

## Actualizare 19.10.2026 — feedback comun al butoanelor VERCON/ADECHIT (ADE9-06)

`static/css/vercon-button-effects.css` importă acum
`static/css/application-button-effects.css`, care păstrează efectele VERCON și
le aplică și în ADECHIT. Culorile provin din temă; stările hover, focus,
apăsare, dezactivare și mișcare redusă folosesc aceleași reguli. Butoanele
integrate în controale și meniuri păstrează stilurile lor.

Publicați ambele fișiere CSS împreună. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
Detalii: [ADE9-06](docs/worklog/SLICE-ADE9-06-butoane-stil-vercon.md).

## Actualizare 19.10.2026 — motor și previzualizare de rapoarte comune cu ADECHIT (ADE9-05)

Motorul generic PDF/Excel și snapshot-urile sunt extrase în `PYTHON/reporting/`;
VERCON păstrează adaptoarele sale, machetele configurate, cheile Excel duplicate
și rendererul dedicat NIR. Vizualizatorul PDF și stilurile lui sunt comune în
`static/js/reporting/` și `static/css/report-preview.css`; antetul și meniul de
aplicații VERCON sunt furnizate de adaptorul existent.

Fișiere VERCON: `routes/vercon/report_output.py`, `report_service.py`,
`static/js/vercon/pdf-preview.js`, `static/css/vercon-preview.css`, `vercon.css`.
Fișierele comune și predarea sunt în [worklog ADE9-05](docs/worklog/SLICE-ADE9-05-sistem-rapoarte-comun-vercon.md).
TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
Machetele/SQL-urile editabile VERCON și configurările din instance nu sunt migrate.

## Actualizare 19.10.2026 — Rapoarte: conturi direct în selecție și Selectează TOATE

Eliminată ramura Furnizori din arborele Selectare date. Rădăcina Produse
nu mai este afișată: conturile apar direct la primul nivel, cu bife pentru
selectarea întregului cont; produsele și variantele se încarcă la deschidere
sau la bifarea contului. Încărcarea inițială citește metadatele conturilor,
fără încărcarea recursivă a ramurilor. Se păstrează celelalte familii de
filtre disponibile pentru raport.

Antetele panourilor și opțiunilor sunt centrate orizontal și vertical.
Antetul Selectare date include checkboxul comun „Selectează TOATE”, aliniat
cu bifele conturilor. Corecție de așezare la cererea utilizatorului: checkboxul
este la stânga, pe același rând cu titlul „Selectare date”; titlul rămâne
centrat în întregul antet, iar ambele elemente sunt centrate vertical.
Corecția este numai CSS, nerulată; fără teste sau verificări executate.
Bifarea încarcă recursiv toate ramurile înainte de
selectare, inclusiv ramurile închise; debifarea golește selecția. Bifa indică
selecția parțială și este dezactivată în timpul încărcării. Căutarea nu
limitează selecția tuturor datelor. Selecția integrală se transmite ca lipsă
a filtrelor opționale, fără liste redundante de ID-uri; selecțiile obligatorii
ale raportului se păstrează. Refacerea selecției după schimbarea
perioadei reconciliază variantele împreună pentru toate conturile, fără să
șteargă selecțiile valide din conturi diferite.

Fișa Magazie BF păstrează produsul/varianta obligatorie, dar nu mai cere
obligatoriu un furnizor, deoarece această selecție a fost eliminată din UI.
În absența filtrului de furnizor sunt incluse datele tuturor furnizorilor;
filtrarea existentă prin API rămâne disponibilă.

Fișiere: PYTHON/routes/vercon/report_selection.py, report_catalog.py;
PYTHON/static/js/vercon/report-selection.js, reports.js;
PYTHON/static/css/vercon-rapoarte.css.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; numai analiză de design din surse.
Fără teste, verificări automate, browser, HTTP sau SQL executat, conform
instrucțiunilor proiectului. Nu s-au modificat schema sau datele MariaDB.

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
### 2026-10-19 — Amortizare în arborele Rapoarte / Mijloace fixe

Eliminată intrarea separată Amortizare din meniul Fluxuri / Mișcări și din
catalogul formularelor. Raportul MF existent se selectează din Rapoarte,
direct în secțiunea Mijloace fixe, fără subgrupul cu aceeași denumire.
La selectarea amortizării, panoul perioadei afișează numai „La data”, iar
cererile folosesc acea dată pentru ambele capete ale intervalului. Trecerea
la alte rapoarte reafișează controalele de perioadă și păstrează începutul
intervalului ales. Exporturile și restricția raportului la MF se reutilizează.

Fișiere: PYTHON/routes/vercon/capabilities.py;
PYTHON/static/js/vercon/app.js, reports.js, report-result.js.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Fără teste sau alte verificări
executate, conform instrucțiunilor proiectului; numai analiză de design.
Nu s-au modificat schema sau datele MariaDB.

### 2026-10-19 — Migrator: recuperare univocă IDBO după refacerea unui bon

Utilizatorul a cerut tratarea automată a cazului IDOI 1576: casare 2950051,
produs 2673, IDBO 2950049 dispărut; primire unică existentă în BP 2950052,
același produs/preț/deținător/cont, aceeași dată document și cantitate 1.
Predarea a fost salvată ulterior casării, deci DataSalvare nu este criteriu
de cronologie business. Aceste date au fost citite read-only la cererea
utilizatorului în etapa anterioară, fără modificarea bazei originale.

Noul VrcRunner.OiOrigins.vb clonează registrul și recuperează numai IDBO
nenule inexistente atât în BP, cât și în BT. Cere primire/document active,
potrivire exactă produs/preț/deținător/cont, dată document compatibilă și
disponibil după toate celelalte ieșiri, inclusiv viitoare. Transferurile
cer laturi minus/plus echilibrate și deținători conform bonului; predătorul
se citește din IDC_OLD. Referința se corectează pe ambele laturi. Nu se
alege arbitrar dintre mai mulți candidați și nu se fabrică bonuri.

BuildRow și resolverul OiAccounts folosesc același snapshot normalizat,
astfel IDBO și contul moștenit nu mai raportează erori contradictorii pentru
un caz recuperabil. N14/CONVERSIE păstrează originalul, noul ID de sursă și
familia; raportul enumeră fiecare IDOI. X17/BLOCAJ explică imposibilitatea
recuperării înaintea conversiilor rândului. Sursa Access rămâne nemodificată.
Versionare: FileVersion 0.3.9.0, RuleVersion VC05-OI-ORIGIN-RECOVERY-2026-10-19.

Fișiere: VERCON/VRC.Migrator/VrcRunner.OiOrigins.vb, VrcRunner.vb,
VrcRunner.OiAccounts.vb, VrcRun.vb, VrcPlan.vb, VRC.Migrator.vbproj, README.md;
VERCON/REGISTRU_OI_MF.md, VERCON_STATUS.md, implementare_vercon_v2.md.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; numai analiză de design pentru codul nou.
Fără teste, verificări automate, build sau import. Nu s-au executat scrieri
Access, SQL MariaDB sau schimbări de schemă. Necesită recompilare; datele
deja migrate nu sunt reparate retroactiv.


### 2026-10-19 — Migrator: IntrariStorno fără NIR storno părinte, ignorat NON BLOCKING

Decizie explicită utilizator: se verifică NIR-urile cu Storno bifat, apoi
corespondența IntrariStorno.IdIntrare; liniile fără părinte eligibil se
ignoră fără blocarea importului, inclusiv când IdProdus nu există.

VrcRunner.Storno.vb indexează NIR-urile eligibile pe sursă/gestiune.
MigrateExercise filtrează înaintea validării cheii și conversiei câmpurilor,
numără liniile la SkippedRows și emite N15 cu ID/IdIntrare/IdProdus originale.
Aceste linii nu sunt scrise și nu intră în sumele de reconciliere. Lipsa
oricărui NIR bifat Storno înseamnă omiterea tuturor liniilor IntrariStorno;
un NIR cu Storno nebifat nu este părinte eligibil. Pentru liniile valide,
produsul rămâne obligatoriu și toate validările se păstrează. UsedProducts
include și variantele folosite numai de storno-urile cu părinte eligibil.
Eliminată regula orphan:null de pe IntrariStorno.IdIntrare din import_map;
generatorul o emite deja ca legătură normală spre NIR, fără orphan:null.
Politica veche de importare a storno-urilor fără părinte cu IdIntrare NULL
este înlocuită pentru importurile noi. Retururile și orfanii deja existenți
în aplicația web nu sunt modificați.

Versiuni: FileVersion 0.3.10.0; RuleVersion VC05-STORNO-PARENTS-2026-10-19.
Fișiere: VERCON/VRC.Migrator/VrcRunner.Storno.vb, VrcRunner.vb,
import_map.json, VrcRun.vb, VrcPlan.vb, VRC.Migrator.vbproj, README.md;
VERCON/CODEX_SCHIMBARI_MIGRATOR_2026-10-18.md, VERCON_STATUS.md.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; numai analiză de design.
Fără teste, verificări automate, build, import, citire Access sau SQL pentru
această corecție. Fără modificări de schemă sau date existente. Necesită
recompilarea executabilului înaintea repetării migrării.

### 2026-10-19 — ME01-03: drepturi reale MariaDB din KBotAdmin

Bifele ADECHIT / VERCON și adăugarea, schimbarea rolului ori scoaterea din
bază sincronizează privilegiile MariaDB personale. Se retrag privilegiile
vechi largi și se reconstruiește politica din toate legăturile contului,
pentru toate variantele User@Host. Contabilii au acces la datele unităților
atribuite, fără administrare de utilizatori, structură sau server. Numai
GRANT SELECT/INSERT/UPDATE/DELETE pe tabele concrete, fără EXECUTE pe rutine
sau acces personal la bazele comune/de sistem. Granularitatea SQL este pe
tabele; validările de subunitate și operație rămân în API.

Buton nou «Aplică drepturile MariaDB» pentru conturile existente. Configurație
obligatorie DB_CONFIG_DREPTURI, separată de serviciu; SQL manual în
sql/SEC03_kbotadmin_privilegii.sql. Conturi tehnice/proprietar protejate;
roluri MariaDB moștenite / PROXY / PUBLIC cu privilegii cer intervenție DBA.
Schimbările de metadate sunt tranzacționale, sincronizarea e serializată;
la eroare se încearcă retragerea drepturilor parțiale. Conexiunile MariaDB
personale vechi sunt închise. GRANT/REVOKE nu oferă atomicitate cu metadatele.

Fișiere: PYTHON/routes/admin_cli/{admin_cli,admin_common,admin_rights,
mariadb_privileges,privilege_policy}.py; KBotAdmin/Views/RightsView.vb și
.Designer.vb, RightsForm.vb, AccountForm.vb și .Designer.vb, README.md;
sql/SEC03_kbotadmin_privilegii.sql;
docs/worklog/SLICE-ME01-03-privilegii-mariadb.md.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Numai analiză de design; fără teste,
build, verificări automate, conexiuni MariaDB sau SQL executat. Necesită
configurarea manuală a contului administrativ, publicarea surselor Python
și recompilarea KBotAdmin. Conturile existente se normalizează explicit,
individual; nu s-a făcut migrare automată a privilegiilor pe server.

### 2026-10-19 — Balanță: produse alfabetic, pagină pe cont și filtrare fără mișcări

Rapoartele de tip balanță (produse în magazie și balanțele OI/MF în
folosință) ordonează produsele alfabetic în fiecare grup, fără diferențe
între majuscule/minuscule și cu ordinea literelor românești.

În Rapoarte, la selectarea unei balanțe, apar două opțiuni debifate inițial:
«Grupează pe cont, cu pagină nouă» și «Afiseaza FARA miscari». Prima
aduce contul la nivelul exterior al grupării și inserează pagină nouă între
conturi, după totalul contului anterior; nu adaugă întrerupere după ultimul
cont. Macheta standard păstrează gruparea pe cont și când opțiunea este
debifată, fără pagini noi impuse. Machetele salvate nu sunt rescrise.

Implicit se păstrează numai rândurile cu cel puțin una dintre cantitățile
stoc inițial / intrări / ieșiri / stoc final diferită de zero în perioada
aleasă. Comparația folosește valorile exacte înaintea rotunjirii de afișare,
inclusiv stocuri negative sau sub precizia afișată. Bifarea «Afiseaza FARA
miscari» păstrează toate rândurile furnizate anterior de calculul balanței.
Istoricul folosit la calcularea stocului inițial nu se filtrează suplimentar.

Opțiunile invalidează rezultatul precedent; ecranul, PDF și Excel primesc
același model filtrat și ordonat. PDF folosește întreruperile de pagină,
iar exportul Excel le înscrie ca întreruperi de rând pentru tipărire.

Fișiere: PYTHON/routes/vercon/report_balance.py, report_routes.py,
report_service.py, report_body.py, report_output.py;
PYTHON/static/js/vercon/reports.js; implementare_vercon_v2.md.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT. Numai analiză de design;
fără teste, verificări automate sau verificare vizuală în aplicație,
conform instrucțiunilor proiectului. Fără SQL executat, modificări de
schemă sau date. Necesită publicarea surselor Python/JS.

### 2026-10-19 — Rapoarte: eliminarea zonei de mesaje de sub selecție

Eliminate elementele DOM pentru notele raportului (inclusiv «Stoc evaluat
la prețul curent al variantei; documentele păstrează valorile proprii.»)
și pentru mesajele de stare de lângă butoanele Listare / Export Excel.
Nu mai există un container gol rezervat acestor mesaje. Erorile la
încărcarea catalogului, selecției, ramurilor și la generare/export se
afișează prin dialogul comun messageBox; dialogul se închide la părăsirea
paginii. Motivele de indisponibilitate ale butoanelor rămân în tooltip,
iar starea de încărcare este expusă prin aria-busy.

Fișiere: PYTHON/static/js/vercon/reports.js, report-selection.js;
PYTHON/static/css/vercon-rapoarte.css; implementare_vercon_v2.md.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; numai analiză de design.
Fără teste, verificări automate sau verificare vizuală în aplicație,
conform instrucțiunilor proiectului. Necesită publicarea fișierelor JS/CSS.
Completare ME01-03: RightsForm.Designer.vb rezervă spațiu pentru explicația
rolului Contabil; acesta păstrează acces SQL la toate datele bazei chiar
dacă bifele limitează modulul web. Salvarea incertă a metadatelor este
raportată explicit, fără a pretinde că rollback-ul a fost confirmat.

### 2026-10-19 — Intrări: ajustare NIR salvat pe o unitate rămasă în magazie

Ajustarea diferențelor nenule de cel mult 1 leu cere minimum 1 unitate
disponibilă pe un rând eligibil. Pentru NIR-urile salvate se transmite
cantitatea ajustabilă calculată pe server ca minim al stocului variantei
de la data intrării până la ultima mișcare, limitată la cantitatea rândului.
Predările OI/MF folosesc ieșirea BC din magazie; activele MF cu ieșiri sau
referințe active în registru rămân protejate integral.

Utilizarea unui produs în registru nu mai blochează integral modificarea
NIR-ului: protecția MF pe rând și simularea cronologică a stocului rămân
obligatorii. Anularea și NIR-urile cu storno își păstrează blocajele.
Se separă exact 1 unitate pentru corecție, cu TVA real; restul păstrează
prețul cu TVA și varianta inițială, inclusiv la salvarea unei linii istorice.
Rândurile salvate nemodificate nu se recalculează în masă la ajustare.

Dacă selecția nu este eligibilă, se alege alt rând disponibil. La factura
comună, ținta NIR-ului este totalul facturii minus liniile celorlalte
recepții, calculată pe server și păstrată pentru salvare. Butonul își
actualizează disponibilitatea și la introducerea valorii facturii.
Rămâne regula existentă: corecția automată deja prezentă se șterge cu
revenire în grup înaintea unei noi ajustări.

Fișiere: PYTHON/routes/vercon/receipt_adjustment.py, receipt_rules.py,
receipt_routes.py, receipt_distribution.py, receipts.py, receipt_mf_edit.py,
documents.py; PYTHON/static/js/vercon/docs.js; implementare_vercon_v2.md.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; numai analiză de design.
Fără teste, verificări automate sau vizuale, conexiuni MariaDB, SQL
executat ori modificări de schemă, conform instrucțiunilor proiectului.
Necesită publicarea surselor Python/JS.

### 2026-10-19 — Intrări: buton de ajustare activ la diferențe, cu explicații

În adăugare/editare, butonul de ajustare rămâne activ pentru orice
diferență nenulă cunoscută, inclusiv peste 1 leu, fără produse disponibile
sau cu o corecție automată existentă. Aceste situații sunt motive explicate
la apăsare și în tooltip, nu condiții de dezactivare a butonului.
Fără diferență, butonul rămâne dezactivat. Protecția operațiilor de salvare
în curs și regulile economice de ajustare se păstrează.

Tooltipul prezintă valoarea facturii, totalul produselor, diferența exactă
și blocajele cunoscute. Dacă nu există un produs eligibil, sunt enumerate
produsele din NIR cu cantitatea rândului, cantitatea disponibilă pentru
ajustare și motivul excluderii (ieșire/predare, stoc sub 1, cantitate sub 1,
corecție existentă). Datele incomplete sunt explicate pe rând.

La apăsare, blocajele locale și erorile întoarse de server se afișează
prin messageBox-ul custom, cu titlul «Factura nu poate fi ajustată».
Dialogul are zonă de detalii derulabilă pentru NIR-uri cu multe produse;
se închide la eliberarea contextului/paginii. Serverul furnizează detalii
pe produs pentru lipsa eligibilității și valorile concrete la depășirea
plafonului. Nu se aplică ajustări pentru a obține aceste explicații.

Fișiere: PYTHON/static/js/vercon/receipt-adjustment.js, docs.js,
receipt-reconciliation.js; PYTHON/static/js/utils/confirm-box.js;
PYTHON/static/css/vercon-receptie.css;
PYTHON/routes/vercon/receipt_adjustment.py, receipt_rules.py,
receipt_distribution.py; implementare_vercon_v2.md.
Stare: IMPLEMENTAT ÎN SURSE, NERULAT; numai analiză de design.
Fără teste, verificări automate/vizuale, SQL executat sau modificări
de schemă, conform instrucțiunilor proiectului.
Necesită publicarea surselor Python/JS/CSS.

### 2026-10-19 — Zoom comun în bara de meniu VERCON / ADECHIT

Buton de zoom lângă Ajutor, montat o singură dată în bara aplicației,
prin controlul comun js/components/menu/menu-zoom.js și CSS alăturat.
Slider 75–150%, cu valori intermediare și snap la 90%, 100%, 110%, 125%;
procentele snap sunt disponibile și prin butoane. Indicatorul se schimbă
în timpul tragerii, iar zoom-ul se aplică la încheierea gestului.
Nivelul este comun celor două aplicații și se memorează în localStorage,
păstrându-se la navigare, reîncărcare și redeschidere.

Zoom-ul se aplică rădăcinii documentului, inclusiv dialogurilor.
Geometria controalelor comune folosește pixeli de layout prin
js/utils/layout-geometry.js; sunt adaptate meniurile, ComboBox,
ComboTree, TreeView, DatePicker, ModalFrame și DataGrid, inclusiv
redimensionarea coloanelor și poziționarea filtrelor. CSS-ul cu
unități viewport folosește variabilele comune ale controlului,
cu fallback pentru aplicațiile fără zoom. Imprimarea folosește 100%.
Nu sunt instanțiate controale de zoom în paginile individuale.

Fișiere: PYTHON/static/js/components/menu/menu-zoom.js și .css,
js/utils/layout-geometry.js; js/vercon/app.js, receipt-context.js;
js/adechit/app.js; modulele de geometrie din js/components și js/dgv;
css/vercon.css, adechit.css și modulele CSS cu dimensiuni viewport.
Lista detaliată: docs/worklog/SLICE-VC08-10-zoom-meniu-vercon-adechit.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — NIR: valoare factură introdusă din numpad pe tastatură română

Caseta valorii reale a facturii folosește format en-US, cu punct
zecimal. Virgula trimisă de NumpadDecimal pe tastatura română era
respinsă de parser, blocând ajustarea. TextBox normalizează acum
tasta zecimală din numpad la separatorul câmpului (punct pentru
en-US, virgulă în formatul implicit), pentru decimal și money.
Inserarea respectă selecția/cursorul și emite input pentru actualizarea
valorii din draft și a butonului de ajustare. Delete cu Num Lock oprit,
modificatorii și validarea separatoarelor de mii rămân nemodificate.

Fișiere: PYTHON/static/js/components/textbox/textbox.js;
docs/worklog/SLICE-VC08-06-nir-valoare-factura-numpad.md;
implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Toate rapoartele: antet unitate/subunitate, fără DC

Antet comun de tip ADECHIT Situația Debitorilor: denumirea unității,
codul fiscal și adresa în stânga, subunitatea activă în dreapta.
Datele se citesc din AVACONT_COMUN.Unitati/Unitati_Date și VC_Subunitati,
în contextul autentificat. Numele bazei nu mai este identitate implicită.
Datele lipsă cer completare, fără revenire la DC. Textele configurărilor
vechi care conțin identificatorul bazei sunt adaptate doar în memorie;
nu se modifică datele economice sau configurările salvate.

Antetul este aplicat PDF-urilor generice, NIR/FCS, Excel-ului, rezultatului
în pagină și previzualizărilor PDF locale. PDF-urile și Excel-ul imprimat
repetă antetul. NIR își recalculează poziția titlului și spațiul produselor.
Snapshot-urile anterioare fără versiunea nouă de antet cer regenerare.
Endpoint-ul contextual de identitate folosește dreptul de citire și
permite păstrarea imprimării locale a documentelor/ciornelor. Preview-ul
local are identitate demonstrativă explicită, fără bază de date.

Fișiere: routes/vercon/report_identity.py, report_layouts.py,
report_service.py, report_routes.py, report_reception.py;
reporting/output.py; static/js/vercon/report-identity.js, app.js,
snapshot-pdf.js, report-result.js, reports.js;
static/css/vercon-rapoarte.css (toate sub PYTHON);
tools/VerconPreview/preview_server.py; PYTHON/reports/vercon/README.md;
docs/worklog/SLICE-VC08-07-antet-comun-rapoarte-fara-dc.md;
implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Meniu VERCON: ADECHIT ca referință vizuală

Diferențele veneau din stiluri separate: VERCON impunea 46px pentru bară,
față de minimum 53px efectivi în ADECHIT pe desktop, padding lateral de
12px în loc de 28px și alt marcaj/stil pentru Ajutor. Bara preia acum
măsurile ADECHIT din componenta CSS comună application-header.css:
53px desktop / 46px ecrane mici, inset 28px / 8px, maximum 2048px centrat.
Butonul VERCON are tipografia și paddingul butonului ADECHIT; Ajutor
folosește același «?» compact. Sunt aliniate acțiunile din dreapta și
butonul Zoom. Tema întunecată și avertizarea de sesiune păstrează
comportamentul de culoare al barei ADECHIT.

ADECHIT scade înălțimea efectivă a barei din spațiul de lucru. VERCON
păstrează rândul suplimentar al selectoarelor pe ecranele înguste.
Fișiere: PYTHON/static/vercon.html; css/vercon.css, adechit.css;
js/components/menu/application-header.css, menu-zoom.css (sub static);
docs/worklog/SLICE-VC08-08-meniu-aspect-adechit.md; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Intrări/Ieșiri: grupare în două niveluri și lazy fără redraw global

Bifă «Grupeaza pe luni» sub arbore, comună documentelor NIR/FCS,
BC/retur și Bonuri PTC (înlocuiește eticheta veche de ordonare PTC).
Bifat: Luna în română / Anul → Data / NrDoc + valoare în dreapta.
Debifat: Furnizor / Gestionar → Data / NrDoc + valoare în dreapta.
Etichetele folosesc ellipsis și tooltip complet; NIR include subnumărul.
Lunile și documentele sunt descrescătoare, furnizorii/gestionarii alfabetic.
Se păstrează selecția la schimbarea modului, iar bifa revine la starea
anterioară la eroare. Schimbarea grupării nu este permisă în editare.

NIR rămâne lazy: rădăcini luni/furnizori, apoi documentele unei rădăcini.
Endpoint-ul permite furnizor sau lună, cu același scope server-side;
apelurile vechi cu ambele filtre rămân acceptate. Ramurile pot rămâne
deschise simultan. TreeView folosește opțional reconciliere DOM după id,
activată pentru arborii VERCON: expandarea și sosirea datelor lazy nu
înlocuiesc rândurile/subarborii nemodificați. Refresh-ul nu mai golește
containerul, păstrând expansiunea înainte de randare, selecția și scroll-ul.

Fișiere: static/js/vercon/document-tree-order.js, oi-tree-order.js,
receipt-tree.js, docs.js, ui.js; js/components/treeview/treeview-patch.js,
treeview-render.js, treeview-search.js, treeview-checkable.js;
static/css/vercon-document-tree.css, vercon.css;
routes/vercon/receipt_tree.py, doc_routes.py, receipt_invoice_view.py
(toate sub PYTHON); docs/worklog/SLICE-VC08-09-arbori-documente-doua-niveluri-lazy.md;
implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Aspectul bifelor din TreeView / ComboTree

Casetele sunt centrate vertical pe rând, cu bordură de 1px. Semnul bifat
folosește un SVG centrat, în locul bordurilor rotite cu poziții fixe;
și semnul stării parțiale este centrat vertical.
Fișier: PYTHON/static/css/treeview/treeview_checkbox.css (control comun).
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — TreeView fără efect de apăsare pe rând

Eliminată regula `.treeview-item:active` care micșora rândul la 98% și
adăuga umbră interioară la click. Se păstrează stilurile de selecție.
Fișiere: PYTHON/static/css/treeview/treeview_items.css;
JS_COMPONENTS/css/treeview/treeview_items.css; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Butoanele rapoartelor în antet

Previzualizarea PDF comună ADECHIT/VERCON afișează comenzile în același
antet cu informațiile raportului, aliniate la dreapta. Pe ecrane înguste
comenzile pot trece pe rândul următor. Eliminată bara separată de deasupra PDF-ului.
Fișiere: PYTHON/static/js/reporting/report-dialog.js, pdf-preview.js;
PYTHON/static/css/report-preview.css; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Tipografie uniformă în antetul rapoartelor

Titlul, comenzile și indicatorii de pagină/zoom folosesc aceeași familie
și mărime, din variabilele existente --font-ui și --fs-button.
Titlul nu mai preia mărimea mai mare a titlurilor de dialog.
Fișiere: PYTHON/static/css/report-preview.css; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Acțiuni cu emoticoane și meniu Export Raport

Butoanele din antetul previzualizării PDF comune afișează doar emoticoane,
cu tooltipuri explicative și etichete accesibile. Descărcările PDF/Excel
sunt grupate sub Export Raport, folosind meniul comun; dispar butoanele
separate de descărcare. Exportul Excel din previzualizarea rapoartelor
VERCON folosește același snapshot și context ca PDF-ul afișat.
Excel rămâne dezactivat pentru previzualizările care nu oferă acest format.
Fișiere: PYTHON/static/js/reporting/pdf-preview.js;
PYTHON/static/js/vercon/reports.js; PYTHON/static/css/report-preview.css;
implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Aliniere și aspect meniu Export Raport

Meniul se aliniază la marginea stângă a butonului. Opțiunile folosesc
stilul drept al meniului comun, fără colțurile rotunjite și bordurile
moștenite de la butoanele dialogului. PDF folosește 📕, Excel 📗.
Fișiere: PYTHON/static/js/reporting/pdf-preview.js;
PYTHON/static/css/report-preview.css; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Total general integrat în tabel, comun tuturor rapoartelor

Rapoartele grupate VERCON adaugă TOTAL GENERAL ca ultim rând în tabelul
ultimei grupe, cu aceleași coloane și valori calculate. Eliminată secțiunea
separată cu titlu TOTAL GENERAL și antet de coloane repetat. Aceeași regulă
se aplică situației ADECHIT cu grupe separate. Modificarea modelului comun
de date păstrează structura în PDF și Excel, respectiv HTML-ul ADECHIT.
Motorul PDF păstrează fiecare rând total împreună cu rândul precedent,
pentru a evita totalurile izolate la schimbarea paginii.
Fișiere: PYTHON/routes/vercon/report_body.py;
PYTHON/routes/adechit/reports.py; PYTHON/reporting/output.py;
implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Aspect Total general și eliminare pentru un singur grup

TOTAL GENERAL este marcat distinct de subtotaluri: fundal identic cu banda
categoriei (#f7f8f2), text bold și spațiu de 2 mm deasupra, păstrând coloanele
tabelului și legătura cu rândul precedent la paginarea PDF. Stilul este comun
PDF/Excel, cu echivalent pentru HTML-ul ADECHIT.
Rapoartele grupate VERCON omit totalul general când există o singură grupă
exterioară; fișele cu subtotaluri lunare îl omit când există cel mult o lună.
ADECHIT omite totalul general suplimentar când există o singură grupă;
în situația fără separare pe grupe, totalul unic poartă numele grupei.
Fișiere: PYTHON/routes/vercon/report_body.py; PYTHON/routes/adechit/reports.py,
report_model.py, report_render.py; PYTHON/reporting/output.py;
PYTHON/static/css/adechit-report.css; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Păstrarea selecțiilor între rapoarte din aceeași categorie

Schimbarea raportului în aceeași categorie din arbore păstrează controlul
de selectare date, bifele, selecția, ramurile deschise, căutarea și derularea.
Schimbarea categoriei reîncarcă datele și resetează bifele. Schimbarea perioadei
păstrează reîncărcarea existentă. Catalogul include specificațiile filtrelor,
astfel încât validarea și exportul să folosească regulile raportului curent
chiar când arborele este păstrat. Rădăcinile arborelui de rapoarte se deschid
explicit la inițializare; categoriile rămân pliate.
Fișiere: PYTHON/static/js/vercon/reports.js, report-selection.js, ui.js;
PYTHON/routes/vercon/report_catalog.py; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Ordine rapoarte și selectarea documentelor pe furnizor

Fișa Magazie este prima în Magazie / Produse; FM1-u, FM2 și Fișa Magazie BF
nu mai apar în arborele de rapoarte. Referat periodic produs este afișat
ca Referat Periodic.
Numai Referat Periodic, Plan Achiziții și Situație Intrări au opțiunea
Afișează Documente, nebifată implicit. Pentru acestea dispare rădăcina
Documente; bifarea opțiunii înlocuiește produsele din arborele din dreapta
cu furnizori pliați la rădăcină. Documentele se încarcă lazy la deschiderea
sau bifarea furnizorului. Se poate bifa un document sau furnizorul cu toate
documentele sale. Filtrele produselor ascunse nu se aplică în modul documente.
Debifarea restabilește produsele fără bife și elimină filtrul documentelor.
Comutarea modului resetează căutarea, selecția rândului, derularea și ramurile
deschise; noul conținut pornește pliat la rădăcină.
Schimbarea între cele trei rapoarte păstrează selecțiile din aceeași categorie.
Opțiunea afectează numai selectarea datelor; machetele PDF/Excel rămân existente.
Fișiere: PYTHON/static/js/vercon/report-result.js, report-selection.js, reports.js;
PYTHON/routes/vercon/report_catalog.py, report_selection.py; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Reset complet la comutarea Afișează Documente

Înlocuită comutarea prin modificarea nodurilor controlului existent cu
distrugerea controlului și montarea unui arbore nou, exclusiv pentru modul
ales. Bifarea și debifarea șterg toate selecțiile și bifele, fără salvarea
celor anterioare. Controlul vechi este eliminat imediat, inclusiv când perioada
este invalidă; încărcările sale lazy sunt anulate.
Cererea root include selection_mode=documents numai pentru modul documente;
serverul returnează exclusiv rădăcinile furnizorilor, lazy, fără produse sau
documente preîncărcate. Arborele nou are setul ramurilor deschise gol.
Schimbarea raportului în aceeași categorie și același mod păstrează controlul
și selecțiile, conform cerinței inițiale. Comutarea modului pornește de la zero.
Fișiere: PYTHON/static/js/vercon/reports.js, report-selection.js;
PYTHON/routes/vercon/report_selection.py, report_routes.py; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Corecție mod exclusiv Documente, cu încărcare lazy

Corecția ulterioară a resetului complet la comutare este descrisă în secțiunea
Reset complet la comutarea Afișează Documente; aceasta înlocuiește păstrarea
selecțiilor între modurile produse și documente.

Corectată implementarea inițială a opțiunii Afișează Documente: furnizorii
înlocuiesc conținutul anterior, fără produse afișate simultan și fără filtre
ascunse active. Cererea inițială returnează numai furnizorii ca rădăcini lazy,
fără documentele lor. Deschiderea unei ramuri încarcă numai documentele
furnizorului respectiv; bifarea furnizorului încarcă și selectează documentele
sale prin mecanismul comun. Metadatele documentelor încărcate se cumulează
pentru identificarea selecției în previzualizare. Răspunsurile pentru ramuri
care au fost ascunse între timp nu modifică arborele activ.
Fișiere: PYTHON/static/js/vercon/report-selection.js, reports.js;
PYTHON/routes/vercon/report_selection.py; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Valoare estimată în referate rotunjită în sus la zeci de lei

În referatele de achiziție din Produse și Intrări, numai suma din textul
VALOARE ESTIMATĂ se rotunjește în sus la multiplu de 10 lei, cu Decimal și
ROUND_CEILING, și se afișează fără zecimale: 3.277,8900 devine 3.280 LEI.
Un multiplu exact de 10 rămâne neschimbat. Valorile și totalurile tabelului
păstrează calculul existent. Textul comun este folosit în pagină, PDF și Excel.
Fișiere: PYTHON/routes/vercon/report_body.py; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Magazie / Intrări: selecție pe furnizor și NIR cu stornări

Schimbările sunt limitate la rapoartele din Magazie / Intrări. Arborele
implicit are furnizori la rădăcină, conturi la nivelul următor și produse
la al treilea nivel. NIR folosește clarificarea separată: furnizor → documente
→ produse, fără cont. Rădăcinile sunt pliate, iar fiecare ramură este lazy;
bifarea unei ramuri încarcă descendenții pentru selecția completă.
NIR listează recepțiile și stornările în ordine descrescătoare după data
documentului, inclusiv la generare. Eliminată obligația de a selecta un
singur document NIR. Stornările păstrează valorile negative în modelul NIR.
NIR SFP, NIR Storno și duplicatul Centralizator Conturi Furnizori nu mai apar
în arborele rapoartelor; rămâne intrarea nou Furnizori cu raportul de furnizori.
Referatul din Intrări se numește Referat Periodic. Nota Comandă și acest
referat primesc Afișează Documente, nebifat inițial, fără rădăcina Documente
în arborele implicit. Comutarea resetează selecțiile și înlocuiește arborele
cu furnizori → documente, pliat și lazy, folosind mecanismul existent.
Selecțiile de produse păstrează asocierea cu furnizorul/contul sau cu
documentul, prin identificatori validați și filtrare în modelul raportului;
nu se combină accidental produsele bifate de la furnizori diferiți.
Schimbarea raportului păstrează controlul când categoria și structura
arborelui coincid; trecerea la/de la structura NIR reîncarcă selecția.
Arborele rapoartelor din Produse păstrează comportamentul anterior.
Fișiere: PYTHON/routes/vercon/report_incoming_selection.py (nou),
report_selection.py, report_catalog.py, report_routes.py, report_data.py,
report_service.py; PYTHON/static/js/vercon/report-result.js,
report-selection.js, reports.js; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Subtotaluri integrate și denumiri rapoarte furnizori

Subtotalul unei grupe cu o singură subgrupă nu se mai repetă: totalul
subgrupei acoperă deja aceleași date. Pentru mai multe subgrupe, subtotalul
se adaugă ca rând în ultimul tabel de detalii, fără tabel separat și fără
antet de coloane sau subsol suplimentar. Corecția este în modelul comun,
folosit de afișarea în pagină, PDF și Excel.
Centralizator Furnizori se numește Situație Furnizori, iar nou Furnizori
se numește Centralizator Furnizori. Denumirile se aplică în catalog și în
titlurile rapoartelor generate, inclusiv pentru machete deja configurate.
Identificatorii interni ai rapoartelor rămân aceiași.
Fișiere: PYTHON/routes/vercon/report_body.py, report_catalog.py,
report_layouts.py, report_service.py; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Arbore rapoarte cu majuscule și selecție Ieșiri

Toate etichetele arborelui rapoartelor sunt convertite la majuscule,
inclusiv rădăcinile și categoriile OI/MF. Deschiderea unei ramuri închide
ramurile surori; regula se aplică și rădăcinilor MAGAZIE/OI/MF și nu este
ocolită prin Shift+click. Inițial este deschisă numai prima rădăcină.
BON CONSUM este afișat ca BONURI și include consumuri și retururi. Arborele
implicit este cont → bon → produs; Afișează Documente îl înlocuiește cu
bon → produs. Bonurile sunt ordonate descrescător după dată în fiecare grupă.
BUConsum, BUConsum F, BUConsum F1 și BonRetur nu mai apar în catalogul vizual.
Centralizator BC devine CENTRALIZATOR. Arborele implicit este cont → bon,
iar opțiunea Afișează Primitori îl înlocuiește cu primitor → bon, cu aceeași
ordine descrescătoare. Primitorii sunt încărcați ca rădăcini fără bonuri
preîncărcate. Ambele opțiuni sunt nebifate inițial și resetează selecțiile
și controlul la comutare. Ramurile noi pornesc pliate și se încarcă lazy;
bifarea ramurii și căutarea folosesc mecanismele comune de încărcare.
Selecțiile păstrează asocierea exactă cont/primitor, bon și produs, inclusiv
la generare. BONURI identifică separat fiecare consum/retur; CENTRALIZATOR
păstrează sursa și calculul anterior al bonurilor de consum. Denumirile
noi se aplică și titlurilor rapoartelor cu machete deja configurate.
Fișiere: PYTHON/routes/vercon/report_outgoing_selection.py (nou),
report_selection.py, report_routes.py, report_catalog.py, report_data.py,
report_body.py, report_layouts.py, report_service.py;
PYTHON/static/js/vercon/report-result.js, report-selection.js, reports.js,
ui.js; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-19 — Lățimi coloane și păstrarea arborelui pe categoria de nivel 1

Coloana Tipul raportului este lărgită cu 10%: intervalul 150–204 px devine
165–224,4 px. Diferența se scade din coloana perioadei (190–240 px devine
175–219,6 px), păstrând spațiul coloanei Selectare date și distanțele dintre coloane.
Selecția altui raport păstrează arborele din dreapta exclusiv după categoria
de nivel 1 de sub rădăcină, indiferent de structura implicită a noului raport.
Eliminată condiția suplimentară selectionKind care reîncărca arborele între
BONURI și CENTRALIZATOR sau între NIR și celelalte rapoarte din INTRĂRI.
Controlul, bifele, selecția rândului, căutarea, derularea, ramurile deschise
și încărcările lazy curente rămân aceleași. Se invalidează doar rezultatul
raportului generat. Schimbarea categoriei, perioadei sau comutarea explicită
a opțiunilor arborelui păstrează reîncărcarea/resetul prevăzut.
Filtrele bonurilor păstrate sunt acceptate și de celelalte rapoarte de
consum din aceeași categorie, pentru a nu ignora selecția la generare.
Fișiere: PYTHON/static/css/vercon-rapoarte.css;
PYTHON/static/js/vercon/reports.js, report-selection.js;
PYTHON/routes/vercon/report_catalog.py, report_service.py; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — Simplificare meniu rapoarte Mijloace Fixe

În RAPOARTE → Mijloace Fixe, bonurile sunt primele rapoarte, direct sub
rădăcină, în ordinea Bon Predare, Bon Transfer, Bon Casare. Nodul intermediar
BONURI OI / MF dispare. MF Balanta este redenumit Balanta, iar MF Balanta
SUBINV este ascuns din meniul rapoartelor. MF Lista Inventar devine Lista
Inventar și este mutat direct sub Mijloace Fixe, eliminând nodul intermediar
INVENTAR DETINATORI. Arborele păstrează afișarea comună cu majuscule.
Schimbările structurii sunt limitate la MF; rapoartele OI își păstrează gruparea.
Fișiere: PYTHON/routes/vercon/report_catalog.py;
PYTHON/static/js/vercon/report-result.js; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — Arbori Selectare date specifici rapoartelor MF

Rapoartele din Mijloace Fixe nu mai folosesc rădăcinile generale Conturi în
folosință, Primitori și Documente. Bon Predare/Transfer/Casare folosesc primitor
→ bon cu număr, dată și valoare → produse. Bonurile sunt ordonate descrescător
după dată și număr. Balanta folosește cont → produse, iar Lista Inventar
folosește cont → primitor → produse, pe conturile din registrul de folosință.
Amortizare folosește contul produsului → clasificarea MF → produse. Opțiunea
Grupează după Primitori, nebifată inițial, schimbă arborele în primitor →
clasificare → produse. Deținerea este calculată din istoricul mișcărilor până
la data raportului; activele fără deținere pozitivă au ramura Fără primitor.
Clasificarea provine din CC și catalogul comun existent, prin SELECT cu context.
Produsele includ numărul de inventar și UM, fără nivel separat pentru variante.
Ramurile sunt pliate și lazy; bifarea unei ramuri, Selectează TOATE și căutarea
folosesc încărcarea recursivă comună. Schimbarea raportului MF sau a opțiunii
de grupare resetează arborele și selecțiile. Restul rapoartelor păstrează regula
existentă de reutilizare a selecțiilor pe categorie.
Selecția MF are identificatori compuși și filtrare în modelul comun pentru
afișare, PDF și Excel: cont/produs pentru balanță, cont/primitor/produs pentru
inventar, primitor/bon/produs pentru bonuri și cont/primitor/variantă pentru
amortizare. Aceeași variantă nu este calculată de mai multe ori în amortizare
dacă este selectată din mai multe ramuri. Opțiunea de grupare schimbă arborele
de selecție; macheta exportului și formulele de amortizare rămân cele existente.
Fișiere: PYTHON/routes/vercon/report_mf.py (nou), report_mf_selection.py (nou),
report_catalog.py, report_selection.py, report_routes.py, report_service.py,
report_data.py; PYTHON/static/js/vercon/reports.js, report-selection.js;
implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — MF/OI fără selector de an și perioade de raportare până azi

Selectorul An din meniul principal este ascuns și dezactivat pentru MF și OI.
Contextul intern al istoricului comun este păstrat pentru rutele existente.
La deschiderea rapoartelor MF/OI, perioada implicită este 1 ianuarie din anul
curent până azi, calculată în Europe/Bucharest, cu fallback UTC dacă baza de
fusuri orare nu este instalată. Anul curent este disponibil în selectorul
perioadei inclusiv când ultimele documente sunt din ani anteriori.
TOTI ANII este prima opțiune la rădăcina arborelui perioadelor MF/OI. Selectarea
ei completează data de început cu prima dată efectivă a mișcărilor și data de
sfârșit cu azi. Istoricul folosește liniile documentelor neanulate de intrare,
storno, ieșire, retur și BP/BT/BS, în contextul gestiunii; intrările folosesc
data efectivă COALESCE(DataFc,Data). Fără mișcări, începutul este 1 ianuarie
din anul curent. Dacă toate documentele sunt în viitor, intervalul este azi–azi.
Calendarele permit și anii intermediari fără mișcări pentru alegerea manuală
a intervalelor. Gestiunile cu exerciții anuale păstrează comportamentul existent.
Fișiere: PYTHON/static/js/vercon/app.js, report-period.js;
PYTHON/routes/vercon/report_period.py; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — Ani descrescători și expandare după ultimul an cu date

Arborele perioadelor afișează anii descrescător, cu TOTI anii păstrat ca prima
opțiune. Pentru MF/OI, anul curent se deschide automat numai dacă este ultimul
an cu date efective. Catalogul transmite separat latest_data_year, calculat
înainte de adăugarea anului curent fără mișcări; acest an adăugat nu declanșează
expandarea. În celelalte cazuri MF/OI, toate ramurile anuale pornesc pliate.
Fișiere: PYTHON/static/js/vercon/report-period.js;
PYTHON/routes/vercon/report_period.py; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — Corecție compatibilitate expandare an curent

Arborele perioadelor acceptă și răspunsurile catalogului care încă nu conțin
latest_data_year: în acest caz folosește latest_year. Valorile anilor sunt
normalizate numeric înainte de comparație. Un latest_data_year explicit null
continuă să însemne lipsă de mișcări și nu deschide anul curent. Configurația
expanded se aplică prin API-ul ComboTree după setData și se sincronizează
și cu starea expandedNodes, pentru afișarea inițială și redeschiderea listei.
Cu răspunsul vechi, latest_year este singura informație disponibilă despre
ultimul an; distincția dintre anul cu date și cel adăugat fără mișcări necesită
și versiunea Python care transmite latest_data_year.
Fișiere: PYTHON/static/js/vercon/report-period.js; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — BC: număr propus, produse cu stoc și sumar produs

La adăugare BC se propune următorul număr, editabil, prin citirea seriei BC
și a contorului existent. Sugestia nu rezervă numărul și nu modifică baza;
salvarea păstrează verificarea existentă a numerelor manuale și actualizarea
contorului în tranzacție. Numerele modificate sunt reverificate la părăsirea
câmpului, fără reguli de sub-număr NIR/FCS pentru BC.
Editorul Produs din DGV este TreeCombo: produs → variante cu preț și stoc,
limitat la contul ales și stocul pozitiv la data bonului, diminuat cu celelalte
linii din ciornă. La modificare/consultare disponibilul exclude consumul
salvat al BC-ului curent. Încărcările sunt asociate documentului și datei;
răspunsurile depășite sunt ignorate, iar alegerea produselor este blocată cât
timp stocul nu este disponibil. Schimbarea datei înainte de primul produs
reîncarcă lista. După alegerea primului produs, data și butonul calendarului
sunt dezactivate; eliminarea tuturor produselor permite din nou schimbarea datei.
Cantitatea introdusă este confruntată cu disponibilul ciornei, în plus față
de validarea exactă existentă pe server la salvare.
Panoul din stânga jos înlocuiește Fișa de magazie cu Stoc produs: identitate,
dată, preț, total intrări, total ieșiri și stoc disponibil, în UM. Totalurile
sunt cantități pe întregul istoric al variantei până la data bonului, calculate
exact din mișcările salvate neanulate; în editare este exclus BC-ul curent.
Nu se limitează la ultimele 20 de mișcări sau la începutul anului. Ciorna
nesalvată nu este inclusă în totalurile istorice din panou.
Popup-ul TreeCombo BC nu are derulare orizontală: etichetele lungi se împart
pe rânduri. Clasa popup-ului este transmisă opțional prin editorul comun DGV;
CSS-ul este limitat la BC.
Fișiere: PYTHON/routes/vercon/consumption_number.py (nou), consumption_stock.py
(nou), doc_routes.py, receipt_context.py; PYTHON/static/js/vercon/
consumption-editor.js (nou), consumption-ui.js, document-number.js, docs.js,
receipt-context.js; PYTHON/static/js/dgv/editing.js;
PYTHON/static/css/vercon-consumption.css; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — ComboTree: căutarea păstrează descendenții produsului găsit

ComboTree activează includeDescendantsOnMatch în filtrarea comună a arborelui.
Când denumirea sau cuvintele-cheie ale unui părinte corespund căutării,
rezultatul păstrează și deschide întreaga sa ramură, inclusiv variantele de
preț selectabile. Când corespunde numai un copil, sunt păstrați copilul și
strămoșii săi. Astfel, căutarea unui produs în BC permite alegerea prețului.
Se folosesc exclusiv nodurile deja furnizate controlului, păstrând filtrarea
produselor fără stoc. TreeView-urile care nu activează opțiunea păstrează
filtrarea existentă.
Fișiere: PYTHON/static/js/components/combotree/combotree.js;
PYTHON/static/js/components/treeview/treeview-search.js; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — BC asociat unei liste: editare și recalculare; ordine inversă cronologică

Editarea BC-ului nu mai este blocată de simpla asociere cu o listă de alimente.
După actualizarea liniilor, totalului BC și validarea stocului, se recalculează
ValoareBon din toate BC-urile active ale listei și DEP = ValoareLista − ValoareBon.
Lista primește o versiune nouă și audit, în aceeași tranzacție cu BC-ul;
orice eroare anulează întreaga modificare. Mesele și valoarea listei se păstrează.
Mesajul salvării confirmă recalcularea economiilor / depășirilor.

Modificarea și anularea sunt limitate la ultimul BC activ, respectiv ultima
listă activă, separat pe familie, în subunitatea/gestiunea/exercițiul curent.
Ordine: data documentului, apoi ID descrescător pentru documente din aceeași zi.
Documentele anulate nu intră în ordonare; după anularea ultimului, precedentul
devine ultimul. Schimbarea datei ultimului document nu îl poate muta înaintea
precedentului. Pentru editarea BC-ului asociat, lista recalculată trebuie să fie
și ea ultima listă activă, protejând listele anterioare. Anularea unui BC legat
necesită în continuare scoaterea lui din listă sau anularea listei; retururile
active continuă să protejeze BC-ul sursă. Nu se introduce ștergere fizică.

API-ul transmite blocarea cronologică și explicația în rezumate/documente;
interfața dezactivează Modifică/Anulează pentru documentele anterioare și arată
motivul la deschidere. Serverul aplică aceeași regulă în operațiile de scriere,
sub blocarea tranzacțională existentă a gestiunii.

Fișiere: PYTHON/routes/vercon/document_order.py (nou),
food_list_recalculation.py (nou), documents.py, lista.py, doc_routes.py;
PYTHON/static/js/vercon/docs.js, lista.js; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — BC: primitor unic selectat automat

La adăugarea unui BC, dacă lista conține exact un primitor, acesta este
completat automat în ciornă, iar câmpul Primitor / gestionar este dezactivat.
Cu mai mulți primitori, alegerea rămâne manuală. În modificare, regula se
aplică dacă primitorul existent coincide cu unica opțiune sau lipsește;
un primitor istoric diferit nu este înlocuit automat.
Fișiere: PYTHON/static/js/vercon/docs.js; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — VERCON: schimbarea perioadei direct peste raport

Meniul raportului PDF are un buton calendar „Schimbă perioada”, furnizat numai
de VERCON. Deschide un popup peste raport cu același ComboTree de ani/luni,
aceleași casete și etichete de dată ca în pagina RAPOARTE. Pentru Amortizare
se păstrează selecția unei singure date. Sunt reutilizate catalogul perioadelor,
TOTI anii, ordinea descrescătoare și limitele calendarului.

Popup-ul pornește de la perioada raportului afișat și aplică modificările numai
la apăsarea „Schimbă perioada”. Regenerează același raport schimbând exclusiv
start/end într-o copie a parametrilor originali; produsele, primitorii,
documentele, gestiunea/exercițiul și opțiunile de grupare rămân neschimbate.
Nu se reîncarcă arborele de selecții și nu se elimină bife după noul interval.
Selectarea unui interval în popup nu schimbă formularul RAPOARTE din fundal.

PDF-ul se redesenează în aceeași fereastră; titlul/perioada, tipărirea,
descărcarea PDF și exportul Excel sunt actualizate pentru noul snapshot.
La eșecul generării, popup-ul păstrează datele introduse și raportul anterior
rămâne disponibil; dacă încărcarea PDF-ului nou eșuează, se reîncarcă PDF-ul
anterior. Se poate reveni cu Renunță înainte de aplicare. Resursele popup-ului
se închid odată cu raportul/pagina. Previzualizările oficiale ale documentelor
VERCON primesc aceeași acțiune; ciornele locale nu au sursă pentru regenerare.

Vizualizatorul comun are doar extensii opționale pentru acțiuni și înlocuirea
PDF-ului; alte aplicații nu primesc butonul calendar. Nu sunt necesare rute
noi sau modificări de schemă MariaDB.

Fișiere: PYTHON/static/js/vercon/report-period-action.js (nou), reports.js,
document-report.js, pdf-preview.js; PYTHON/static/js/reporting/pdf-preview.js,
report-dialog.js; PYTHON/static/css/vercon-report-period.css (nou), vercon.css;
implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — Perioade rapoarte: luni descrescătoare până la ultima mișcare

Lunile din fiecare an sunt construite în ordine calendaristică descrescătoare,
de la luna ultimei mișcări din acel an până la ianuarie. Sunt păstrate și lunile
intermediare fără mișcări. Opțiunile TOTI anii și Tot anul rămân disponibile.
Aceeași componentă este folosită în RAPOARTE și în popup-ul de schimbare a perioadei.

Catalogul transmite last_movement_month separat de limitele pentru introducerea
manuală a datelor. Pentru toate gestiunile se citesc mișcările reale din liniile
de intrări/ieșiri, cu datele efective folosite de motorul stocului, inclusiv
storno/retur; documentele anulate și antetele fără linii nu extind lunile.
Pentru MF/OI se includ și bonurile active de predare/transfer/casare.
Data unei liste de alimente nu prelungește intervalul mișcărilor.
Anii sintetici fără mișcări au last_movement_month=0, fără luni fictive;
perioadele implicite și posibilitatea introducerii manuale a datelor se păstrează.

Fișiere: PYTHON/routes/vercon/report_period.py;
PYTHON/static/js/vercon/report-period.js; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — Dropdown perioadă egal cu lățimea casetei combo

Selectorul perioadei configurează minWidth=0, astfel încât dropdown-ul să aibă
lățimea casetei care îl deschide, inclusiv în popup-ul de peste raport.
ComboTree respectă acum valoarea explicită zero (fallback nullish în loc de ||);
configurațiile fără minWidth păstrează minimul implicit de 300px. Poziționarea
și redimensionarea folosesc în continuare geometria controlului și viewport-ul.
Fișiere: PYTHON/static/js/components/combotree/combotree.js;
PYTHON/static/js/vercon/report-period.js; implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — Tooltip comun configurabil pentru ADECHIT, K-BOT și VERCON

Notificările comune folosesc componenta tooltip, cu antet bold și ℹ️, închidere
manuală, trei variante de aspect, culori, poziție și lățime configurabile.
Întârzierea, animația și durata pe ecran se configurează separat; durata 0
menține notificarea până la închidere. Temporizarea se suspendă la hover/focus
și cât timp fila este ascunsă. Sunt afișate cel mult trei notificări simultan
pentru fiecare context (pagina/dialog nativ), restul așteaptă. Mesajele
înlocuite sau ale ferestrelor închise sunt anulate.

Meniul aplicațiilor din toate cele trei pagini include Setări tooltip, cu
previzualizare, revenire la implicit și salvare în browser pentru aceeași
origine. VERCON păstrează mesajele contextuale și jurnalul din sesiune.
ADECHIT afișează confirmările informative în tooltip; erorile rămân și în pagină.
Notificările temporare de sesiune reutilizează componenta comună.

La cererea utilizatorului, numele componentei este tooltip. Fișierele sunt în
js/components/tooltip; API-ul este showTooltip, cu exporturi ES modules și
prefix CSS kbot-tooltip. Containerul oprește moștenirea atributului title,
pentru a evita afișarea unui tooltip nativ peste notificarea personalizată.

Fișiere: PYTHON/static/js/components/tooltip/tooltip-config.js, tooltip.js,
tooltip-settings.js, tooltip.css (noi); PYTHON/static/js/portal/messages.js,
app.js, portal.js, apps-menu.js; PYTHON/static/js/adechit/app.js, payers.js;
PYTHON/static/js/vercon/application-menu.js;
PYTHON/static/js/session/session-monitoring-ui.js;
docs/web-tooltip.md (nou); implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — Tooltipuri din title; proba butoanelor RAPOARTE și oprire pe mobil

În ADECHIT, K-BOT și VERCON, adaptorul comun preia textul existent din title
și elimină atributul din DOM, inclusiv pentru controalele create sau schimbate
dinamic. Hover/focus afișează tooltipul personalizat lângă control, cu legătură
aria-describedby. Escape, click și părăsirea controlului îl închid. Nu se
înregistrează gesturi tactile sau apăsare lungă. Titlul accesibil al iframe-urilor
se păstrează prin aria-label; titlul paginii din browser rămâne disponibil.

Butoanele din vizualizatorul efectiv al raportului și din bara rapoartelor HTML
ADECHIT au delay=100 ms și duration=20000 ms, explicit pentru proba cerută.
Textele sunt cele din title, fără rescriere. Adaptorul este comun și rămâne
în js/components/tooltip. Tooltipul unui control nu așteaptă coada notificărilor.

Toate tooltipurile sunt dezactivate în modul mobil: viewport <=900 px,
hover indisponibil sau pointer principal coarse. Atributele title sunt preluate
și eliminate și pe mobil, iar trecerea la acest mod închide afișările active
și le anulează pe cele întârziate. Mesajele contextuale ale paginii se păstrează.

Fișiere: PYTHON/static/js/components/tooltip/tooltip-environment.js,
tooltip-titles.js (noi), tooltip.js, tooltip.css, tooltip-settings.js;
PYTHON/static/js/adechit/app.js, report-print.js;
PYTHON/static/js/portal/portal.js; PYTHON/static/js/vercon/app.js;
PYTHON/routes/adechit/report_render.py; docs/web-tooltip.md;
implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — Tooltip compact, antet opțional și oprire pe nodurile Selectare date

Antetul tooltipului este opțional (showHeader) și dezactivat implicit global,
inclusiv la încărcarea preferințelor vechi fără această proprietate. Poate fi
activat ulterior din setări sau la apel. Textul folosește --fs-2xs, bold italic,
cu padding exterior de 2 px/latură. Bula se adaptează conținutului, până la
lățimea maximă configurată, fără banda mare de antet. Fundalul implicit este
galben pal, distinct de aplicație; tema întunecată are variantă contrastantă.
Un indicator mic, poziționat din geometria controlului, pointează spre acesta.

Pointerdown/click închide imediat tooltipul și blochează reapariția până la
mișcarea mouse-ului în afara elementului apăsat. Schimbarea focusului,
deschiderea unui meniu și refacerea DOM-ului sub cursor nu anulează blocarea.
Zona nodurilor din RAPOARTE / Selectare date are tooltipurile dezactivate prin
data-kbot-tooltip=off, inclusiv la încărcarea ramurilor și la redesenare;
atributele native title sunt în continuare eliminate din acea zonă.

Fișiere: PYTHON/static/js/components/tooltip/tooltip-config.js,
tooltip-settings.js, tooltip.js, tooltip-titles.js, tooltip.css;
PYTHON/static/js/vercon/report-selection.js; docs/web-tooltip.md;
implementare_vercon_v2.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-20 — UTC pentru momentele tehnice ale serverului AVACONT

Salvările și anulările VERCON generate în Python folosesc acum utc_now(),
independent de fusul sistemului. Conexiunile MariaDB ale aplicației impun UTC
pe sesiune, astfel încât NOW(), valorile implicite și expirările comparate în
SQL folosesc aceeași convenție. Aceeași regulă se aplică serverului FOREXE,
e-Factura, jurnalelor, administrării și scripturilor auxiliare. Procesul web
și comenzile administratorului folosesc UTC și pentru biblioteci/loguri.

Data documentului, anul/perioada și implicitul „azi” rămân date calendaristice
românești, prin bucharest_today(). Chatul păstrează salvarea UTC și afișarea
existentă în browser. Istoricul și valorile importate nu sunt deplasate automat.

Fișiere VERCON: routes/vercon/doc_routes.py, documents.py, lista.py,
migration_links.py, nomenclatoare.py, oi.py, oi_context.py, receipt_products.py,
receipts.py, report_service.py (toate sub PYTHON); comun: PYTHON/utils/clock.py
(nou), database.py, logger.py, timing.py, asociere_log.py, main.py,
gunicorn.conf.py. Politica și publicarea: PYTHON/docs/TIMEZONES.md (nou).
Adaptate și modulele serverului pentru expirări FOREXE, semnături PDF,
metadate/loguri/copii, conexiunile directe și datele implicite de activitate.
Testul existent al semnăturilor PDF a fost adaptat conversiei UTC explicite.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).


### 2026-10-21 — VC09-01: Ajutor VERCON după modelul ADECHIT și chat în ambele ghiduri

Ajutorul VERCON are cuprins cu subcapitole, text formatat și legături interne,
imprimare și export PDF din același JSON. Acces: ? → Cum lucrez cu VERCON;
Mesaje din sesiune rămâne disponibil. Sunt documentate contextul/drepturile,
NIR și repartizarea facturii, ajustările de maximum 1 leu, storno, consum,
retur, liste de alimente, BP/BT/BS, registru MF/amortizare, nomenclatoare,
stocuri, rapoarte și exporturi, perioade și recuperări după migrare.
Paginile Furnizori (Parteneri) și Închidere an sunt prezentate ca în pregătire.

Ambele ajutoare includ capitolul DISCUȚII: conversație comună, filtre V/A/K,
pseudonim/nume, trimitere și răspuns, istoric, necitite, actualizare, ciorne
și sesiune. ADECHIT are și legătură din Primii pași.

Fișiere: PYTHON/static/help/vercon.json (nou), adechit.json;
PYTHON/static/js/help/help.js (nou), adechit/help.js, vercon/help.js (nou),
vercon/app.js; PYTHON/static/css/vercon-help.css (nou), vercon.css;
PYTHON/static/vercon.html; PYTHON/reporting/help_pdf.py (nou);
PYTHON/routes/adechit/help_pdf.py; PYTHON/routes/vercon/help_pdf.py (nou),
__init__.py; docs/worklog/SLICE-VC09-01-ajutor-vercon-si-chat.md (nou).
Rendererul JS și cel PDF sunt comune; fiecare aplicație are conținutul său.
Ruta PDF VERCON este protejată cu dreptul read și nu cere gestiune selectată.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).


### 2026-10-21 — VC09-02: Capturi anonimizate în help, CPV/clasificări MF și timer

Capturi reale din VERCON DEMO (Local, MF, Alimente Cantina, OI), realizate
în browserul in-app: documente și fișe, Configurări, rapoarte, clasificări MF,
CPV, registru/amortizare, liste de alimente, chat, context și timer.
Contul conectat, identificatorii și numele persoanelor sunt mascate opac.
Cataloagele au și capturi de căutare; ferestrele sunt decupate pentru lizibilitate.
Nu au fost salvate/anulate documente și nu au fost trimise mesaje în chat.
La Alimente Cantina s-a preluat ocuparea de la altă filă a aceluiași cont DEMO.

Conținutul VERCON include capitole distincte CPV și Clasificări MF, cu pașii
de acces/alegere și diferența ani în catalog / luni în DNF. Ambele helpuri
explică timerul: 15/5 minute, închiderea avertizării, salvarea înainte de
refresh și pierderea ciornelor la expirare. Capturile din această tranșă sunt
VERCON; ADECHIT primește explicațiile comune pentru sesiune.

Fișiere: PYTHON/static/help/vercon.json, adechit.json;
PYTHON/static/help/images/vercon/ (JPEG-uri anonimizate, manifest, README);
PYTHON/static/js/help/help.js; PYTHON/static/css/adechit-help.css;
PYTHON/reporting/help_pdf.py; tools/HelpCapture/Protect-WebHelpCapture.ps1;
docs/worklog/SLICE-VC09-02-capturi-ajutor.md; implementare_vercon_v2.md.
Motorul comun include imagini/legende în dialog, imprimare și PDF.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Completare VC09-02: set final 24 de capturi, inclusiv timerul real și
avertizarea critică apărută după pragul de 5 minute. Avertizarea a fost
închisă după captură; originalele temporare au fost șterse, iar proiectul
păstrează numai JPEG-urile anonimizate și manifestul public.

### 2026-10-21 — VC09-03: Simplificare Gestionari, Rapoarte și totaluri în copaci

Gestionari afișează numai Denumire și Locatie; CF, Nr. reg. com., CNP și
Cont au fost scoase din specificația nomenclatorului. Locatie este doar
eticheta vizibilă pentru Adresa; coloana internă și valorile istorice ale
câmpurilor eliminate sunt păstrate. Rapoarte nu mai are checkboxul și zona
de afișare a rezultatului în pagină. Listare și Export Excel păstrează fluxul
existent, iar Opțiuni afișare este ascuns când nu există opțiuni specifice.
Sumele grupurilor lunare de documente OI/MF apar fără prefixul TOTAL.

Fișiere: PYTHON/routes/vercon/nomenclatoare.py;
PYTHON/static/js/vercon/reports.js, document-tree-order.js;
docs/worklog/SLICE-VC09-03-simplificare-gestionari-rapoarte-copaci.md;
implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-21 — VC09-04: Help și capturi după publicarea interfeței

După confirmarea publicării de către utilizator, au fost refăcute în
browserul in-app capturile 07 (Gestionari cu Denumire/Locatie), 09 și 10
(Rapoarte și perioadă fără checkboxul eliminat), 22 (bon OI, sume în copac
fără prefix TOTAL). Datele personale sunt mascate opac; originalele
temporare au fost șterse. Nu au fost modificate datele gestiunii.

Helpul VERCON descrie coloanele actuale din Gestionari, totalurile numerice
ale grupurilor lunare și opțiunile specifice rapoartelor. A fost eliminată
descrierea afișării rezultatului în pagină.

Fișiere: PYTHON/static/help/vercon.json;
PYTHON/static/help/images/vercon/{07-configurari-gestionari.jpg,
09-rapoarte-selectie.jpg,10-rapoarte-perioada.jpg,22-bon-predare-oi.jpg,
manifest.json,README.md}; docs/worklog/SLICE-VC09-04-help-dupa-publicarea-interfetei.md;
implementare_vercon_v2.md.

Stare: ACTUALIZAT LOCAL, NEPUBLICAT. Examinare vizuală a celor patru capturi
cerute; fără teste, verificări automate sau randare PDF. Fără SQL sau
modificări de schemă. Publicare: JSON și cele patru JPEG-uri împreună;
această tranșă nu necesită repornirea serviciului web.

### 2026-10-21 — VC06-07 / VC06-08: NIR din facturile primite prin e-Factura

Contabilul trimite din K-BOT o factură primită (felia 00EF-45/47, `docs/EV_PLAN.md`); Administratorul o preia în „Adăugare NIR”. Nimic nu se copiază:
K-BOT scrie `EF_PrimiteVercon` / `EF_PrimiteVerconLinii` (cu TOATE liniile și, opțional, contul propus pe fiecare), VERCON le citește din aceeași bază.

- **VC06-07 (citire, completare):** `routes/vercon/efactura_queue.py` — `GET /api/vercon/doc/nir/efactura` (lista celor care așteaptă, pe unitate, nu pe gestiune) și
  `GET .../efactura/<id>` (antetul, furnizorul găsit în `Parteneri` după cod fiscal, liniile cu cantitate, preț, cotă TVA și contul propus). `js/vercon/efactura-intake.js`:
  fereastra cu lista („NIR din e-Factura…” / „Din e-Factura…” în bara NIR) și `apply` care completează ciorna: furnizor, număr, data facturii, valoarea facturii, conturile propuse
  (bifate în „Categorie / cont”; un cont din altă gestiune decât cele oferite de context nu se bifează și se spune) și liniile **fără produs** (coloana „Din factura primită” arată
  textul furnizorului). Administratorul alege produsul fiecărei linii. `docs.js`: import, butoane, funcția `fromEfactura`, `efactura` în corpul salvării, coloana — `docs.js` are acum 999 de linii (la limita de ~1000).
- **VC06-08 (scriere):** `POST .../efactura/<id>/return` — returnează factura contabilului cu motiv obligatoriu (3–500 caractere), `RETURNATA`. `receipt_distribution.save`: când corpul are
  `efactura`, `efactura_queue.close` rulează în **aceeași tranzacție** cu salvarea NIR-ului: trimiterea devine `PRELUATA` și NIR-urile create intră în `EF_PrimiteVerconNir`; o trimitere care nu mai
  așteaptă (retrasă, preluată de altul) oprește tot salvarea. O trimitere `PRELUATA` ale cărei NIR-uri sunt toate anulate sau șterse reapare în listă (calculat la citire).
- Fără tabelele K-BOT (`sql/EV_01_primite_vercon.sql`) lista răspunde `available: false`, goală; nimic nu cade. Ajutorul VERCON (`static/help/vercon.json`): secțiunea „NIR dintr-o factură trimisă de contabil”.

Fișiere: PYTHON/routes/vercon/efactura_queue.py (nou), doc_routes.py, receipt_distribution.py; PYTHON/static/js/vercon/efactura-intake.js (nou), docs.js; PYTHON/static/css/vercon-efactura.css (nou), vercon.css;
PYTHON/static/help/vercon.json; docs/worklog/SLICE-VC06-07-nir-din-efactura-lista-si-completare.md, SLICE-VC06-08-nir-din-efactura-inchiderea-trimiterii.md; implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-21 — Portal: conectare cu certificat lângă Continuă

În formularul comun `/portal`, inclusiv `/portal?next=%2Fvercon`, butonul
„Intră cu certificat digital” este mutat în stânga lui „Continuă”, pe același
rând, aliniat cu marginea stângă a selectorului Aplicația. Separatorul „sau”
este eliminat. Textul butoanelor se poate împărți pe rânduri la lățimi mici.
Formularele de autentificare și cod permit derularea paginii sub 901 px,
pentru acces la toate controalele când înălțimea disponibilă este mică.

Fișiere: PYTHON/static/portal.html; PYTHON/static/css/portal-certificate.css,
portal-signin.css; implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-21 — SLICE-VC09-05: Reglare registru OI (VERCON) și produs completat la migrare

Registrul `OI_Produse` migrat are pozițiile de primire (BP) fără produs, iar Balanța OI refuza să se listeze («contul nu
poate fi stabilit»). Fereastra **Reglare registru OI** (butonul din bara paginilor Bon de predare / transfer / casare) listează
aceste poziții cu produsul propus din liniile bonului (aceeași cantitate și același preț): Sigur, De verificat (mai multe
produse la fel de potrivite, în ordinea liniilor), De ales, Fără linie potrivită. «Aplică propunerile sigure» sau produs ales
pe o poziție. Se scrie numai `IdProdus`, cu revizie, sub blocarea gestiunii, cu audit și cheie de cerere; după aplicare se
afișează dacă registrul mai are o altă problemă. Mesajele din `oi_accounts.py` și `oi_custody.py` sunt rescrise în română
clară și trimit la fereastră. Migratorul 0.3.11.0 completează produsul sigur din `OI_Predare` și ia contul scoaterii din
aceeași sursă (ca în Access).

Fișiere: PYTHON/routes/vercon/oi_register_repair.py (nou), doc_routes.py, oi_accounts.py, oi_custody.py;
PYTHON/static/js/vercon/oi-register-repair.js (nou), docs.js; PYTHON/static/css/vercon-migration.css;
PYTHON/static/help/vercon.json; VERCON/VRC.Migrator/*; docs/worklog/SLICE-VC09-05-reglare-registru-oi-si-produs-la-migrare.md;
VERCON/VERCON_STATUS.md; implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Nu se deschide automat la prima intrare în VERCON.

### 2026-10-23 — VC08-11: Rapoarte OI: două niveluri și selecție ca la MF

MAGAZIE rămâne rădăcină separată în OI, cu toate rapoartele și grupările
existente, ca în celelalte gestiuni. Numai ramura OBIECTE DE INVENTAR
păstrează exact cele 11 rapoarte din referința primită, în ordinea și
grupurile BONURI, PRODUSE, PRIMITORI. Cele două niveluri de grupare sunt
Root (OBIECTE DE INVENTAR) și Niv1 (cele trei grupuri), apoi frunzele
rapoartelor. Eliminarea inițială a MAGAZIE a fost o interpretare greșită,
corectată local la solicitarea utilizatorului.

Toate cele 11 rapoarte folosesc selecția ierarhică existentă MF, fără
ramurile independente „Conturi în folosință” și „Deținători”: bonuri pe
primitor–bon–produs, produse pe cont–produs și primitori pe primitor–cont–produs.
Se reutilizează protocolul intern MF pentru selecții corelate și filtrarea
aceluiași snapshot PDF/Excel. Schimbarea raportului reîncarcă selecția.

Fișiere: PYTHON/routes/vercon/{report_catalog.py, report_mf.py,
report_mf_selection.py, report_service.py};
PYTHON/static/js/vercon/{report-result.js, reports.js};
PYTHON/static/help/vercon.json; PYTHON/reports/vercon/README.md;
docs/worklog/SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md;
implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC08-12: Antet comun bonuri P/T/C și Consum

Bonurile de predare, transfer, casare și consum folosesc antetul tabelar
din referința primită: gestiune, număr document, Data cu Ziua/Luna/Anul,
tip gestiune, Predător, Primitor, Nr. comandă și titlul bonului în celula
unită vertical din dreapta. Retururile de consum folosesc aceeași structură.
Se folosesc valorile bonului curent; Nr. comandă rămâne gol când lipsește
din sursă. Explicațiile se păstrează sub antet.

Structura este comună PDF/Excel și previzualizărilor locale de documente
și ciorne. Listarea bonurilor MF salvate folosește raportul oficial MF
când există dreptul de rapoarte, cu fallbackul local păstrat la lipsa lui.
Machetele personalizate sunt adaptate în memorie pentru antetul comun;
configurările salvate nu se rescriu. Excel păstrează modulul comun de lățimi.

Fișiere: PYTHON/routes/vercon/{report_voucher.py (nou), report_body.py,
report_service.py, report_routes.py}; PYTHON/reporting/{form_grid.py (nou),
output.py}; PYTHON/static/js/vercon/{voucher-header.js (nou),
document-report.js, docs.js, snapshot-pdf.js, report-result.js};
PYTHON/static/css/vercon-rapoarte.css; PYTHON/reports/vercon/README.md;
docs/worklog/SLICE-VC08-12-antet-comun-bonuri.md; implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC08-13: Corecții balanță OI, bonuri și selecție pe primitor

Balanța OI preia macheta efectivă a balanței de magazie, cu aceleași coloane
și titlul OI, inclusiv când existau coloane OI personalizate anterior.
Calculează un rând pe cont–variantă: SI din predări minus casări anterioare,
Intrări numai din predările perioadei, Ieșiri numai din casările perioadei,
SF = SI + Intrări − Ieșiri. Transferurile nu participă în această balanță.
În PRIMITORI, Selectare date este grupată primitor → cont → produs.

Generarea din browser trimite selecțiile prin POST în corp, cu contextul
scurt în URL, inclusiv la listarea documentului și schimbarea perioadei.
Exporturile snapshotului nu retrimit filtrele în URL. GET rămâne compatibil.
Schimbarea elimină URL-urile foarte lungi; cauza exactă a HTTP 400 raportat
nu este confirmată în lipsa jurnalului de pe server și a unei reproduceri.

Fișiere: PYTHON/routes/vercon/{report_oi.py, report_layouts.py,
report_service.py, report_mf_selection.py, report_catalog.py,
report_routes.py, stock_routes.py, kbot_bridge.py};
PYTHON/static/js/vercon/{report-request.js (nou), reports.js,
document-report.js, report-period-action.js}; PYTHON/static/help/vercon.json;
PYTHON/reports/vercon/README.md;
docs/worklog/SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md;
docs/worklog/SLICE-VC08-13-corectii-balanta-oi-bonuri-primitori.md;
implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC08-14: HTTP 500 la generare: parametrii cererii suprascriși

La trecerea pe POST, parserul de selecție suprascria variabila care păstra
`request.values` cu lista de ID-uri sau None, apoi apela `.get(...)` pe
aceasta la citirea opțiunilor. Acest defect produce HTTP 500 înaintea
construirii raportului și poate pierde filtrele din corpul cererii.
Parametrii cererii și lista de ID-uri folosesc acum variabile separate.

Fișiere: PYTHON/routes/vercon/report_routes.py;
docs/worklog/SLICE-VC08-14-generare-http-500-parametri.md;
implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC07-06: Numărul copiilor în orice listă din ultima lună

Orice listă activă din ultima lună cu liste de alimente active a gestiunii
permite modificarea numărului copiilor/porțiilor. Luna este determinată pe
toate exercițiile, nu din data calculatorului sau anul selectat. Pentru
listele mai vechi din această lună, „Modifică numărul copiilor” permite
numai Porții la cele trei mese. Restul câmpurilor și bonurile rămân protejate.
Modificarea completă și anularea păstrează ordinea invers cronologică.

O rută dedicată verifică dreptul, blocarea, perioada deschisă, luna, starea
și versiunea listei; recalculează valorile meselor, valoarea listei și DEP
cu Decimal la ZEC și salvează sumarul, antetul, versiunea și auditul în
aceeași tranzacție. Modificarea porțiilor din formularul complet verifică
aceeași lună. Economiile/depășirile listelor următoare se actualizează la
consultare și regenerarea raportului prin calculul existent din DEP.

Fișiere: PYTHON/routes/vercon/{food_list_portions.py (nou), lista.py,
doc_routes.py}; PYTHON/static/js/vercon/lista.js;
PYTHON/static/help/vercon.json;
docs/worklog/SLICE-VC07-06-copii-liste-ultima-luna.md;
implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC06-09: BC din luna activă, stoc protejat și istoric cu motiv

Orice BC din luna activă poate fi modificat, cu perioada deschisă, dreptul,
blocarea și versiunea curente. Alimente folosește ultima lună cu liste
active (fallback BC dacă nu există liste), Materiale ultima lună cu BC
active, pe toate exercițiile. Data rămâne în luna activă; retururile active,
listele asociate din alte luni și stocurile deja deschise într-un an ulterior
sunt protejate. Anularea păstrează regula ultimului document.

Salvarea validează întregul istoric al variantelor vechi și noi prin
check_timeline, inclusiv soldurile ulterioare bonului. Mișcările fără dată
sau cantitate produc eroare explicită. Soldul negativ produce rollback
complet, inclusiv pentru recalcularea listei și audit. Lista asociată se
recalculează și dacă nu este ultima din luna activă.

Istoricul BC afectate înregistrează stocul recalculat la sfârșit de zi;
istoricul listelor afectate înregistrează DEP/EC și soldurile cumulative
înainte/după. Cauza identifică bonul sau lista sursă prin număr, dată, ID,
versiune și explicație lizibilă. Aceeași auditare se aplică modificării
porțiilor. Liniile și prețurile bonurilor ulterioare se păstrează. Istoricul
arată înregistrări expandabile cu motiv și Înainte/După, inclusiv din pagina
Lista Alimente. Helpul descrie explicit ambele reguli ale ultimei luni
deschise și istoricul recalculărilor.

Fișiere: PYTHON/routes/vercon/{consumption_period.py (nou),
recalculation_audit.py (nou), document_order.py, documents.py, doc_routes.py,
food_list_recalculation.py, food_list_portions.py, lista.py, stock_source.py};
PYTHON/static/js/vercon/{document-permissions.js (nou), document-history.js
(nou), docs.js, lista.js}; PYTHON/static/css/vercon-history.css;
PYTHON/static/help/vercon.json; PYTHON/reports/vercon/README.md;
docs/worklog/SLICE-VC06-09-bc-luna-activa-stoc-si-istoric.md;
implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC07-07: Lista Alimente: ordinea și alinierea butoanelor

În consultare, bara listei afișează Vizualizare, apoi Modifică. Istoric
este eliminat din această bară și rămâne în meniul contextual al listei
din arbore. La adăugare sau modificare, inclusiv numai porții, Renunță
este în stânga și Salvează lista în dreapta; Vizualizare nu apare.
Mesajele ocupă spațiul dintre butoane sau rândul următor pe ecrane înguste.
Helpul descrie etichetele și comenzile actuale; capturile se păstrează.

Fișiere: PYTHON/static/js/vercon/lista.js;
PYTHON/static/css/vercon-lista.css; PYTHON/static/help/vercon.json;
docs/worklog/SLICE-VC07-07-lista-butoane-actiuni.md;
implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC08-15 / VC06-10: Tooltipuri arbori și salvarea comisiilor la ieșirea de pe rând

Tooltipul comun al arborilor măsoară textul fără spațiile de formatare și
compară lățimea cu limitele care îl decupează efectiv. Depășirea verticală
a dreptunghiului fontului peste propria casetă nu mai declanșează tooltipul.
Ajutorul acțiunilor din dreapta își păstrează mecanismul separat.

Comisiile de recepție și inventariere folosesc salvarea pe rând: editările
celulelor rămân local până la schimbarea rândului sau ieșirea din tabel.
Validarea este pe rând; rămâne comanda explicită de salvare/reîncercare.
Un rând nou salvat permite continuarea navigării fără o a doua încercare.
Helpul explică regula comisiilor.

Fișiere: PYTHON/static/js/components/treeview/treeview-tooltip.js;
PYTHON/static/js/vercon/nomenclator-grid.js; PYTHON/static/help/vercon.json;
docs/worklog/SLICE-VC08-15-tooltip-arbori.md, docs/worklog/SLICE-VC06-10-comisii-salvare-rand.md;
implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC08-16: Ordonare după tip și evaluarea prețurilor istorice

Regula tuturor aplicațiilor este memorată în AGENTS.md: texte alfabetic,
date cronologic, numere numeric. Comparatorul comun typed-order.js se
aplică arborilor la fiecare nivel/randare și selectoarelor, inclusiv lazy.
Grilele și filtrele compară după tip, prețurile zecimale fără pierderea
cifrelor, iar referințele după etichete. Sortarea implicită a grilei este
după prima coloană sortabilă. Documentele/lunile și variantele de preț au
chei explicite; sensul descrescător al documentelor se păstrează.

Rapoartele respingeau prețurile negative, contrar politicii importului
care păstrează valorile sursei cu semn. stock_valuation.py acceptă acum
prețurile istorice negative fără transformarea lor. Un preț lipsă sau
nevalid produce diagnostic cu produs, variantă, cod și cont. Rândurile
eliminate deja de opțiunile raportului nu blochează evaluarea. Fișa
prețurilor afișează ID/cod pentru identificare. Helpul explică remedierea.

Fișiere: AGENTS.md; PYTHON/static/js/utils/typed-order.js (nou);
components/treeview/treeview-render.js și components/combobox/combobox-state.js;
dgv/{datagrid.js,engine.js,filtering.js}; vercon/{ui.js,nomenclator-grid.js,
document-tree-order.js,receipt-tree.js,product-stock.js,report-selection.js,
report-result.js} (toate sub PYTHON/static/js); PYTHON/routes/vercon/
{report_order.py (nou),stock_valuation.py (nou),report_data.py,report_oi.py,
stock.py,report_selection.py,report_incoming_selection.py,
report_outgoing_selection.py,report_mf_selection.py};
PYTHON/static/help/vercon.json; PYTHON/reports/vercon/README.md;
docs/worklog/SLICE-VC08-16-ordonare-tipuri-si-preturi-istorice.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC08-17: Fișa de magazie: selecție multiplă și pagini separate

Fișa de magazie și variantele sale FM1-u, FM2 și Fisa_Magazie_BF permit
selectarea mai multor produse și variante de preț prin filtrul existent
variant_ids. Eliminată restricția unei singure variante din catalog.

report_statement.py construiește fișe independente pe variantă și introduce
o pagină nouă între ele. Produsele sunt ordonate alfabetic și prețurile
numeric; soldul inițial, mișcările, totalurile și semnăturile rămân proprii
fiecărei fișe. Blocurile comune de întrerupere se aplică în PDF și la
imprimarea Excelului. Machetele existente păstrează coloanele și stilul;
gruparea pe variantă este impusă numai în memorie. Helpul este actualizat.

Fișiere: PYTHON/routes/vercon/{report_catalog.py,report_statement.py (nou),
report_service.py}; PYTHON/static/help/vercon.json;
PYTHON/reports/vercon/README.md;
docs/worklog/SLICE-VC08-17-fisa-magazie-selectie-multipla.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC06-11: Asociere NIR manual cu eFactura, parțial și drag and drop

Corecție cerută ulterior: asocierea compară data facturii și CUI-ul
furnizorului. Numărul facturii nu este condiție în listă, la salvare sau
la calculul valabilității asocierilor; seria și spațiile pot diferi.
Actualizate efactura_association.py, efactura_queue.py,
utils/vercon_efactura.py, efactura-association.js, helpul și jurnalul slice-ului.
Corecția nu necesită DDL nou. Fără teste sau alte verificări executate.

În NIR-ul de cumpărare salvat apare «Asociază eFactura…». Două liste permit
drag and drop între produse NIR și poziții eFactura, cu alternativă de
selectare/buton. Cantitățile se împart pe mai multe rânduri și NIR-uri,
inclusiv în subunități diferite; resturile sunt controlate pe ambele
documente, în tranzacția salvării. UM diferite au cantități independente.

Potrivirile sunt propuneri: denumiri confirmate anterior pentru furnizor,
CPV exact și cantitate la ambiguități. Utilizatorul rezolvă restul și
confirmă prin salvare. Corespondențele de denumiri se pot memora pentru
CUI/gestiune/produs. Repetările ambigue nu sunt alese arbitrar.

K-BOT și VERCON calculează aceeași stare PARTIALA până la acoperirea
cantităților tuturor pozițiilor. Editarea NIR-ului invalidează asocierile
reviziei anterioare și cere reconfirmare; anularea/dispariția eliberează
cantitățile. Legăturile integrale vechi sunt păstrate fără conversie.
Asocierea nu rescrie documente sau stocuri.

Fișiere noi: sql/EV_02_vercon_asocieri_partiale.sql;
PYTHON/utils/vercon_efactura.py;
PYTHON/routes/vercon/efactura_association.py;
PYTHON/static/js/vercon/{efactura-association,efactura-match}.js.
Integrare: routes/vercon/{doc_routes,efactura_queue}.py;
routes/efactura/primite_vercon.py; static/js/vercon/{docs,efactura-intake}.js;
static/css/vercon-efactura.css; static/help/vercon.json (sub PYTHON);
src/KBot.Domain/EFacturaVercon.vb;
src/KBot.EFactura/Forms/VerconForm.vb; VERCON/FACTURI_NIR_IMPLEMENTARE.md;
docs/worklog/SLICE-VC06-11-efactura-asociere-nir-partiala.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC06-12: NIR: asociere în popup și grupare inițială pe furnizori

Mutată «Asociază eFactura…» din footer în meniul ⋮ al fiecărui NIR de
cumpărare din arbore. Acțiunea încarcă NIR-ul țintă înainte de asociere și
păstrează restricțiile de drepturi, anulare, editare și operații neconfirmate.

La deschiderea formularului NIR, «Grupeaza pe luni» este debifat; arborele
afișează furnizorii alfabetic în română. Modul inițial este configurat
explicit pentru NIR prin initialByDate, iar utilizatorul poate alege apoi
gruparea lunară. Celelalte familii păstrează configurația lor inițială.

Fișiere: PYTHON/static/js/vercon/{docs,document-tree-order}.js;
PYTHON/static/help/vercon.json;
docs/worklog/SLICE-VC06-11-efactura-asociere-nir-partiala.md;
docs/worklog/SLICE-VC06-12-nir-popup-asociere-si-grupare-furnizori.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC06-13 / VC07-08: Asociere eFactura în DGV, cantități automate și listă compactă

Înlocuit tabelul perechilor NIR/eFactura cu DGV-ul comun folosit la NIR/BC:
cantități editabile în celule, sortare/filtrare și eliminare prin ⋮.
Ambele cantități se propun automat cu minimul resturilor disponibile,
inclusiv pentru UM scrise diferit (KG/KGM). Utilizatorul poate modifica
separat cantitățile, cu validare exactă pe celulă și controlul resturilor
în tranzacția serverului. Celula activă se confirmă înainte de operații.

Lista de alimente nu mai generează mesajul cronologic permanent la
deschiderea documentului. Spanul de stare și containerul său nu sunt în
footer dacă nu au un mesaj activ; rămân erorile și confirmările operațiilor.
Restricțiile operațiilor sunt păstrate.

Aliniat scriptul EV_02 la CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci.
Fișiere: PYTHON/static/js/vercon/{efactura-association-grid (nou),
efactura-association,lista}.js; PYTHON/static/css/{vercon-efactura,vercon-lista}.css;
PYTHON/static/help/vercon.json; sql/EV_02_vercon_asocieri_partiale.sql;
docs/worklog/SLICE-VC06-13-efactura-dgv-cantitati-automate.md, docs/worklog/SLICE-VC07-08-lista-alimente-fara-mesaj-permanent.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC06-14: eFactura: DGV-uri sursă și memorare implicită manuală

Eliminate explicația și bifa de memorare din dialog. Butoanele «Asociază
rândurile selectate» și «Propune potriviri» sunt pe rândul selectorului,
după «Deschide factura», cu lățimea selectorului redusă.

Listele NIR/eFactura de sus sunt acum DGV-uri comune, cu buton ↕️ pentru
alegerea destinației, după modelul ADECHIT Prezență. La drag este evidențiat
rândul eFactura țintă prin fundal și contur punctat. Rândurile fără rest
dispar; reapar la reducerea/eliminarea unei asocieri. Sortarea, filtrarea
și lățimile se păstrează la actualizări.

Memoria denumirilor se completează implicit numai pentru perechile făcute
prin drag ori editate manual, la salvarea asocierilor. Propunerile și
asocierile din butoane nu generează singure memorie. Marcajul este per
pereche, aplicat și de server, fără bifa/flagul global anterior.

Fișiere: PYTHON/static/js/vercon/{efactura-source-grids (nou),
efactura-association,efactura-association-grid}.js;
PYTHON/routes/vercon/efactura_association.py;
PYTHON/static/css/vercon-efactura.css; PYTHON/static/help/vercon.json;
docs/worklog/SLICE-VC06-11-efactura-asociere-nir-partiala.md;
docs/worklog/SLICE-VC06-14-efactura-grile-surse-drop-si-memorare-implicita.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC08-18: status între acțiuni, elipsis și tooltip custom

Uniformizate barele documentelor, listei de alimente, registrului MF,
editorului de previzualizare, setărilor, ferestrelor eFactura și amortizării:
grup stânga / mesaj cu lățime rămasă rezervată / Modifică ori Salvează.
Apariția și lungimea mesajului nu schimbă poziția butoanelor. Statusul
eFactura este în footer, inclusiv în selectorul facturilor de preluat.

Toate elementele vc-save-status folosesc o singură linie cu elipsis.
Opt-in-ul generic data-kbot-tooltip-overflow din tooltipul comun afișează
conținutul integral la hover/focus numai dacă textul este trunchiat.
Mesajele independente din nomenclatoare și CPV primesc aceeași tratare.

Fișiere: PYTHON/static/js/vercon/status-bar.js, ui.js, docs.js, lista.js,
editor.js, mf.js, nomenclatoare.js, depreciation.js,
efactura-association.js, efactura-intake.js;
PYTHON/static/js/components/tooltip/tooltip-titles.js;
PYTHON/static/css/vercon-status.css, vercon.css, vercon-efactura.css;
PYTHON/static/help/vercon.json;
docs/worklog/SLICE-VC08-18-status-intre-actiuni.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC06-15: facturi eFactura inactive global și informare în gestiune

La intrarea într-o gestiune care oferă NIR, pagina citește situația
facturilor trimise de contabil și informează despre cele active cu rest
de preluat/asociat. Facturile complete și inactive nu ridică informarea.
Cererile vechi sunt anulate/ignorate la schimbarea contextului, fără polling.

Marcajul Inactiv este GLOBAL pe unitate, conform corecției explicite a
utilizatorului: ascunderea de către un gestionar se aplică tuturor.
Lista de preluare folosește DGV-ul comun cu bifă Inactiv. Lista și
asocierea au filtre Active/Inactive/Toate, implicit Active. Restaurarea
se face debifând Inactiv din Inactive/Toate. Facturile ascunse complet
preluate rămân accesibile pentru restaurare. Nu se șterg asocieri/stocuri.
Asocierile nesalvate sunt protejate înainte de schimbarea afișării.
Stările parțiale din listă/informare se citesc în lot, fără bugete și
verificări de structură separate pentru fiecare factură.

Fișiere: sql/EV_03_vercon_facturi_inactive.sql;
PYTHON/routes/vercon/efactura_visibility.py, efactura_queue.py, doc_routes.py;
PYTHON/utils/vercon_efactura.py;
PYTHON/static/js/vercon/efactura-notice.js, app.js, efactura-queue-dialog.js,
efactura-intake.js, efactura-association.js;
PYTHON/static/css/vercon-efactura.css; PYTHON/static/help/vercon.json;
docs/worklog/SLICE-VC06-15-efactura-informare-inactive.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). DDL pentru aplicare MANUALĂ în AVACONT_SURSA și sincronizarea structurii în DC-uri. Identificatori noi uppercase, utf8mb3/utf8mb3_general_ci. Fără creare automată de structură. Lipsa EV_03 dezactivează bifa cu explicație, fără a bloca lista existentă.

### 2026-10-23 — eFactura: mesajul de stare între butoanele footerului

Mutat vc-efi__status din corpul dialogului în footer, între «Închide» și
«Salvează asocierile», în locul spacerului. Mesajul ocupă spațiul flexibil
dintre butoane; butoanele rămân la extremități și când mesajul este gol.
Fișiere: PYTHON/static/js/vercon/efactura-association.js;
PYTHON/static/css/vercon-efactura.css.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC06-16: eFactura: scroll corectat, NIR protejat și afișare factură

Corectat paddingul antetelor coloanelor de acțiuni de 40 px în DGV-urile
dialogului eFactura, cauza overflow-ului inclusiv în grila goală. Eliminată
limita comună de 300 px numai pentru ComboBox-ul facturii; baza lui este
460 px, cu restrângere la nevoie.

NIR-urile cu asocieri active eFactura sunt bold italic în arbore și au
editarea blocată în interfață și în _guard_dependents pe server, inclusiv
la validarea ciornei. Metadatele sunt calculate pentru ramurile lazy,
lista generală și documentul deschis, atât pentru legături integrale vechi,
cât și pentru alocări noi valabile. Asocierea/eliminarea ei reîmprospătează
documentul și arborele.

Popupul nodului oferă «Afișează eFactura», cu alegere dacă există mai multe
facturi asociate. Ruta PDF VERCON verifică asocierea în context și folosește
XML-ul și conversia ANAF existente, cu vizualizatorul comun.

Fișiere noi: PYTHON/routes/vercon/efactura_receipts.py;
PYTHON/static/js/vercon/efactura-view.js.
Modificate: PYTHON/utils/vercon_efactura.py;
PYTHON/routes/vercon/{receipt_tree,invoices,documents,doc_routes,receipt_distribution}.py;
PYTHON/static/js/vercon/{docs,document-tree-order}.js;
PYTHON/static/js/components/treeview/treeview-render.js;
PYTHON/static/css/{vercon-efactura,vercon-document-tree}.css;
PYTHON/static/help/vercon.json;
docs/worklog/SLICE-VC06-16-efactura-scroll-arbore-blocare-si-pdf.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC06-17: eFactura: ComboBox custom și încărcare automată

Selectorul facturii este Combobox-ul comun în mod simple. Eliminat butonul
«Deschide factura»; prima factură potrivită după dată/CUI se încarcă automat
la deschidere, iar schimbarea opțiunii încarcă direct pozițiile alese.
Filtrul opțional partner din lista API folosește CUI-ul normalizat; numărul
facturii nu este criteriu. Facturile integrale din fluxul vechi nu sunt
oferite în acest dialog.

Schimbarea selecției protejează ciorna și confirmă celula în editare.
Renunțarea la schimbare reface selecția facturii curente. Selectorul este
blocat în timpul cererilor și pentru salvări neconfirmate.

Bara conține ComboBox, «🔗 Asociaza», apoi «💡 Propune», cu descrierile
complete în title. Fișiere: PYTHON/static/js/vercon/efactura-association.js;
PYTHON/routes/vercon/efactura_queue.py; PYTHON/static/css/vercon-efactura.css;
PYTHON/static/help/vercon.json;
docs/worklog/SLICE-VC06-17-efactura-combo-custom-incarcare-automata.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
### 2026-10-23 — Luni în arbori: ordonare calendaristică explicită

Adăugate chei de sortare de tip dată nodurilor de lună din lista de alimente,
bonurile de consum pe primitori, perioada stocurilor și selectorul perioadei
rapoartelor. Controlul comun folosește anul/luna reală, independent de
denumirea românească. Sensul existent se păstrează: descrescător pentru
documente și rapoarte, crescător pentru perioada stocurilor. În rapoarte,
«TOTI anii» și «Tot anul» rămân înaintea perioadelor individuale.

Fișiere: PYTHON/static/js/vercon/{consumption-ui,lista,stock,report-period}.js;
implementare_vercon_v2.md. Arborii NIR și ceilalți arbori comuni de documente
au deja chei calendaristice explicite. Celelalte etichete își păstrează
ordonarea existentă.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).


### 2026-10-23 — eFactura: suprascrierea CPV-ului produsului la salvarea asocierilor

Cauza: asocierea pe poziții citea CPV numai pentru propuneri; salvarea nu
citea Cpv de pe poziția eFacturii și nu actualiza VC_Produse.
La Salvează asocierile, toate perechile validate copiază acum CPV-ul din
poziția eFactura în produsul nomenclatorului din subunitatea/gestiunea NIR,
independent de memorarea denumirilor și de metoda asocierii. Se rezolvă
codul complet din AVACONT_COMUN.CPV după cele opt cifre, inclusiv pentru
surse cu cifră de control fără separator. CPV lipsă/cu format invalid nu
șterge codul existent. Codurile absente din catalog și mai multe CPV-uri
pentru același produs în aceeași cerere opresc întreaga salvare cu mesaj.
Actualizarea și auditul CPV sunt în tranzacția asocierii; se incrementează
Version și se actualizează DataSalvare numai la schimbare. Repetarea unei
cereri confirmate folosește răspunsul idempotent. Eliminarea asocierii nu
restaurează CPV anterior. Asocierile existente se pot resalva pentru copiere.
Fluxul vechi de preluare integrală nu are mapare explicită a pozițiilor și
nu este inclus în această schimbare.

Fișiere: PYTHON/routes/vercon/efactura_product_cpv.py (nou);
PYTHON/routes/vercon/efactura_association.py; PYTHON/static/help/vercon.json;
implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — VC08-19: mesaje în dialogul comun; eliminare vc-message

Șters efectiv p#vc-message din HTML, conform cererii explicite. Informarea
facturilor și mesajele generale nu mai introduc text în pagină și nu mai
folosesc notificări mici de tip tooltip. Se reutilizează messageBox din
utils/confirm-box.js: dialog centrat, backdrop modal pe întreg ecranul și
buton Am înțeles, același control ca la mesaje/confirmări existente.

Adaptorul messages.js păstrează jurnalul sesiunii și textele statusurilor
din barele de acțiuni deja autorizate, dezactivând tooltipul automat al
mesajelor. ui.js îl exportă pentru notificările operațiilor și erorilor.
Un mesaj nou înlocuiește notificarea modală curentă; eliminarea ecranului
închide mesajul său. Informarea eFactura își închide propriul dialog la
schimbarea gestiunii și păstrează excluderea celor inactive.

Fișiere: PYTHON/static/vercon.html;
PYTHON/static/js/vercon/messages.js (nou), ui.js, app.js, efactura-notice.js;
PYTHON/static/help/vercon.json;
docs/worklog/SLICE-VC08-19-mesaje-dialog-comun.md.

Preferință explicită: nu se adaugă elemente vizuale în pagină fără acordul
utilizatorului. Nu s-a creat un control vizual sau un stylesheet nou pentru
mesaje; se folosește dialogul comun existent.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### 2026-10-23 — eFactura: diagnostic furnizor/dată și propuneri KG/KGM

Analiză: furnizorul era deja comparat pe Parteneri.CodFiscal normalizat
cu EF_Primite.CuiNormalizat; ID-ul identifica doar partenerul NIR, iar
denumirea nu participa. Același mesaj generic acoperea și diferențele
între VC_NIR.DataFc și EF_Primite.DataFact, ascunzând condiția respinsă.
Separat acum mesajele pentru furnizor absent, CUI lipsă/diferit și dată
lipsă/diferită; mesajele includ valorile comparate și explică Data facturii
versus Data NIR. Criteriile furnizor/dată nu au fost relaxate.

Propunerile cereau UM egală textual, deci KG/KGM nu erau echivalente.
Adăugată această echivalență fără conversii numerice. Competitorii sunt
acum filtrați prin aceleași criterii de eligibilitate ca perechea candidată,
pentru a nu bloca o potrivire printr-o altă poziție cu UM incompatibilă
sau o corespondență memorată neeligibilă. Rămân obligatorii restul pozitiv,
CPV/corespondența memorată și lipsa ambiguității; încărcarea dialogului
poate deja adăuga propunerile înainte de apăsarea butonului Propune.

Fișiere: PYTHON/routes/vercon/efactura_association.py;
PYTHON/static/js/vercon/efactura-match.js; PYTHON/static/help/vercon.json;
implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Cazul concret al utilizatorului nu poate fi atribuit uneia dintre condiții fără valorile NIR/eFactura.

### 2026-10-23 — eFactura: furnizor fiscal al NIR-ului și listă strict filtrată

Corectat diagnosticul introdus anterior: un Parteneri negăsit prin
VC_NIR.IdPartener nu dovedește că furnizorul fiscal nu există. NIR-urile
pot păstra identitatea fiscală în CodPartener (coloana pregătită de P122)
sau, pentru documente migrate fără legătură rezolvată, în CodFiscalSursa.
Nu au fost consultate datele concrete ale utilizatorului și nu se afirmă
că una dintre aceste situații a fost confirmată în baza sa.

Rezolvarea fiscală comună pentru asociere folosește, în ordine, primul
cod normalizat nevid: CodPartener al NIR-ului dacă acea coloană există,
CodFiscal al rândului Parteneri legat, CodFiscalSursa dacă este disponibil.
Aceeași expresie se folosește în lista de facturi, deschiderea/salvarea
asocierii și proiecția valabilității alocărilor (inclusiv bugete/stare și
protecția NIR). Nu se modifică automat IdPartener ori antetul NIR-ului.
Normalizarea SQL folosește CASE/SUBSTRING, fără backreference dependent
de escaparea backslash-urilor; normalizează și CUI-ul eFacturii.

Dialogul trimite acum numai ID-ul NIR-ului curent împreună cu contextul
verificat. Serverul citește documentul din subunitate/gestiune/exercițiu și
impune filtrul CUI + Data facturii din NIR. Nu depinde de un filtru partner
opțional, furnizat de browser. Include_completed fără NIR valid este
respins (și pentru JS vechi); CUI nedeterminabil oprește lista explicit.
Active/Inactive/Toate și reîncărcarea după bifă păstrează filtrul. Lista
generală de preluare NIR din eFactura și informarea globală rămân separate.

Fișiere: PYTHON/utils/vercon_efactura.py;
PYTHON/routes/vercon/{efactura_association,efactura_queue}.py;
PYTHON/static/js/vercon/efactura-association.js;
PYTHON/static/help/vercon.json; implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Schimbarea este limitată la asocierea eFactura; nu pretinde că migrarea generală 0122-05 a tuturor cititorilor/scriitorilor VERCON pe CodPartener ar fi fost implementată.


### 2026-10-23 — VC06-18: VERCON ONLINE: o singură identitate fiscală a furnizorului

Implementare limitată la VERCON online. Identitatea este exclusiv codul
fiscal normalizat (alfanumeric uppercase, prefix RO eliminat înainte de
cifră; păstrează zerourile inițiale și codurile externe cu litere).
CodPartener din VC_NIR și VC_FACTURI conține acest CF, nu codul intern al
unei unități. Câmpurile IdPartener/PARTENER_ID existente rămân neatinse;
nu mai sunt folosite pentru legături/citiri/salvări online.

Un singur modul utils/vercon_suppliers.py gestionează normalizarea Python
și SQL, selecția unei înregistrări de afișare pentru fiecare CF normalizat,
căutarea în nomenclator și ordonarea românească. Nu alege după ID intern.
Dublurile între unități sunt o singură identitate; nume identice cu CF-uri
diferite rămân separate. Selectoarele și API-urile transportă CF ca text.

Acoperire: selectare furnizor, citire/salvare NIR, storno, factura comună,
repartizări și reunire fragmente, arbore lazy și arbore FCS, fișă furnizor,
proveniența mișcărilor/stocurilor și filtrele/ramurile/selețiile rapoartelor,
sursele SQL implicite, preluarea NIR din eFactura și asocierea pe poziții.
Lista asocierii este filtrată server-side prin CF + data facturii citite
din NIR-ul salvat; Active/Inactive/Toate păstrează filtrul. Validitatea
alocărilor folosește aceeași cheie fiscală, fără fallback prin ID, nume sau
CodFiscalSursa. Suprascrierea CPV la salvarea asocierilor rămâne activă.

Sursele personalizate de rapoarte cu aliasuri explicite sunt adaptate în
memorie (alias NIR.IdPartener -> CodPartener; alias Parteneri.IdPartener
-> CodFiscal), fără rescrierea fișierelor utilizatorului. Relația Parteneri
este deduplicată fiscal înainte de join, ca să nu multiplice valorile.
Definițiile cu filtre literale pe vechiul ID sau referințe nemapabile sunt
refuzate la executare și rămân accesibile la editare; un ID numeric nu
este reinterpretat drept CF. Nu s-a executat nicio interogare de raport.

SQL PREGĂTIT, NEEXECUTAT:
- sql/VC_20261023_FURNIZORI_CF_STRUCTURA.sql: coloana CodPartener a NIR
  (dacă lipsește) și aceeași coloană în VC_FACTURI, cu indexuri; fără ștergeri,
  fără schimbări pe coloanele vechi. Manual pe AVACONT_SURSA, apoi sync.
- sql/VC_20261023_FURNIZORI_CF_DATE.sql: migrare manuală în fiecare DC.
  Completează/normalizează CF păstrând întâi valorile fiscale deja salvate;
  numai acest fișier de tranziție citește vechile ID-uri pentru a recupera
  CF din Parteneri; folosește CF sursă pentru NIR și NIR-uri legate unanim
  pentru facturi fără partener rezolvat. Nu deduce furnizori după nume.
  Listează CF-uri lipsă și neconcordanțe NIR/factură pentru rezolvare înainte
  de publicare. Nu modifică valori, cantități, revizii sau ID-uri vechi.

Ordine publicare: structura manuală + sync, scriptul de date în fiecare DC
cu scrierile oprite, rezolvarea rezultatelor neclare, apoi toate modulele
Python/JS/CSS/help și SQL-urile SELECT de rapoarte, repornire serviciu și
reîncărcare completă browser. Guard-ul online indică fișierele necesare
când coloanele fiscale nu sunt instalate. Nu creează automat structura.

Fișiere noi: PYTHON/utils/vercon_suppliers.py;
PYTHON/routes/vercon/supplier_identity.py; cele două scripturi SQL de mai sus.
Fișiere modificate: PYTHON/utils/vercon_efactura.py;
PYTHON/routes/vercon/{receipt_choices,receipts,documents,invoices,
receipt_distribution,receipt_routes,receipt_tree,receipt_context,doc_routes,
stock_source,stock,stock_routes,report_routes,report_selection,
report_incoming_selection,report_query,report_store,efactura_association,
efactura_queue}.py;
PYTHON/reports/vercon/queries/{receipts,ledger}.sql;
PYTHON/static/js/vercon/{docs,receipt-context,receipt-tree,
document-tree-order,storno-editor,efactura-intake}.js;
PYTHON/static/css/vercon-receipt-destinations.css;
PYTHON/static/help/vercon.json; implementare_vercon_v2.md;
docs/worklog/SLICE-VC06-18-furnizori-exclusiv-cod-fiscal.md.

Limita de scop: DDF, ORD, extrase, nomenclatorul global Parteneri,
migratoarele Access și rutele K-BOT din routes/efactura nu au fost modificate.
Proiecția comună de stare eFactura citește CF din NIR, în acord cu online.
După importuri cu un migrator vechi, repetați migrarea manuală a datelor
înainte de folosirea documentelor noi în VERCON online.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).


### 2026-10-23 — VC03-07: VERCON: migrare fiscală din KBotAdmin / Interogări unice

Pregătită sql/VC_20261023_FURNIZORI_CF_KBOTADMIN.sql pentru mecanismul
existent routes/one_time/runner.py: fără START TRANSACTION/COMMIT și fără
SELECT-uri finale ale căror rezultate runnerul le-ar consuma fără afișare.
KBotAdmin păstrează tranzacția per bază și scrie registrul numai după
reușita întregii migrări. Nu a fost schimbat runnerul sau KBotAdmin.

Datele migrează ca în scriptul manual. Înainte de final, o gardă în tabelă
temporară oprește baza cu eroare duplicate-key explicită dacă există
NIR_FARA_CF, FACTURA_FARA_CF sau CF_FACTURA_DIFERIT_DE_NIR. Cheia duplicată
este intenționată și indică motivul; nu depinde de CHECK sau strict mode.
Runnerul face rollback la modificările de date și nu marchează baza ca
rulată. Pe eroare de AVACONT_SURSA, runnerul nu continuă la DC-uri.
După corectarea datelor, aceeași interogare poate fi reluată; bazele deja
marcate sunt sărite. Scriptul manual și cele trei SELECT-uri diagnostice
rămân disponibile separat în VC_20261023_FURNIZORI_CF_DATE.sql.

Antetul scriptului de structură a fost adaptat pentru rulare și ca
interogare unică, cu numele VC_20261023_FURNIZORI_CF_STRUCTURA. Poate rula
pe AVACONT_SURSA + DC-urile bifate, apoi migrarea de date pe aceleași baze;
alternativa structură manual pe sursă + sincronizare rămâne valabilă.
DDL este pregătit în fișier și va fi executat exclusiv de utilizator.

Pași KBotAdmin: Interogări unice -> Din fișier pentru structură, nume
VC_20261023_FURNIZORI_CF_STRUCTURA, baze bifate, Vezi (nu execută), Execută.
Apoi Din fișier pentru varianta KBOTADMIN; SCHIMBAȚI explicit numele în
VC_20261023_FURNIZORI_CF_KBOTADMIN (UI păstrează numele anterior dacă nu e
gol), aceleași baze, Vezi (nu execută), Execută. Publicați online după
reușita migrării. Interogarea include întotdeauna AVACONT_SURSA.

Fișiere: sql/VC_20261023_FURNIZORI_CF_KBOTADMIN.sql (nou);
sql/VC_20261023_FURNIZORI_CF_STRUCTURA.sql (antet);
implementare_vercon_v2.md;
docs/worklog/SLICE-VC03-07-migrare-fiscala-kbotadmin-interogari-unice.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).


### 2026-10-23 — VC03-08: diagnostic NIR_FARA_CF după rularea utilizatorului

Log furnizat de utilizator: Portare_parteneri_codfiscal, hash prefix
24beed22b0f0c6f2, 15 instrucțiuni. AVACONT_SURSA și 15 DC-uri OK; șapte DC-uri
s-au oprit în garda NIR_FARA_CF: 000_DEMO,001_GR23,005_CEVM,012_GR30,
018_GRRS,030_SCTC,050_GRSA. Conform runnerului, datele din bazele eșuate
sunt rollback și bazele nu sunt marcate. Cele trei rânduri raportate de
mai multe baze OK sunt cele trei inserări tehnice în gardă, nu trei NIR-uri.

Pregătit numai SQL de diagnostic:
sql/VC_20261023_FURNIZORI_CF_DIAGNOSTIC_KBOTADMIN.sql,
nume Portare_parteneri_codfiscal_diagnostic. Nu modifică tabelele de date;
folosește exclusiv tabele temporare și transmite un sumar prin eroarea
intenționată DIAG_CF (runnerul existent nu afișează rezultate SELECT).
Sumarul include numărul NIR-urilor fără CF după regula exactă a migrării,
numărul cazurilor cu normalizare mascată de prioritate, indicii textuale
STOC/SOLD INITIAL (doar diagnostic, fără reclasificare), până la trei
exemple IdIntrare/Numar și context, existența rândului partener și a CF sursă.
Mesajele duplicate-key pot fi trunchiate de server; prefixul păstrează
numărătorile, apoi exemplele. Baza cu rezultate diagnostice rămâne nemarcată
și se poate reinteroga; fără rezultate se marchează normal în registru.

Nu a fost modificat textul migrării originale deja executate pe unele baze;
nu se va relansa alt text sub numele Portare_parteneri_codfiscal. Nu s-a
relaxat garda, nu s-a inventat CF și nu s-a reclasificat vreun document.
Fără acces MariaDB, SQL executat, teste sau verificări automate. Cauza
concretă a documentelor rămâne de stabilit din diagnosticul utilizatorului.


### 2026-10-23 — VC03-09: migrare VERCON CF v2: documentele istorice fără CF nu blochează baza

Diagnostic furnizat de utilizator (Portare_parteneri_codfiscal_test_1,
hash prefix 14ccf76d81d6702c): N=33/1/147/9/7/2/1 pentru
000_DEMO/001_GR23/005_CEVM/012_GR30/018_GRRS/030_SCTC/050_GRSA,
în total 200 de NIR-uri fără CF recuperabil prin sursele migrării.
FMT=0 în toate bazele: nu sunt coduri valide mascate de ordinea normalizării.
STOC=13/0/13/8/1/0/1, în total 36: numai indicii textuale, nu documente
confirmate ca stoc inițial. Exemplele vizibile au P0,SRC0; nu se deduce
că toate cele 200 au exact aceeași stare a vechii legături de partener.
Diagnosticarea a creat doar tabele temporare, fără schimbarea datelor NIR.
Numărul de rânduri OK include operațiile tehnice, nu documente modificate.

Garda inițială era prea strictă pentru istoricul cu informații lipsă.
Pregătit sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V2.sql,
nume NOU Portare_parteneri_codfiscal_v2. Poate rula pe cele șapte baze
rămase (sau pe toate țintele originale); sursa este inclusă automat.
Vechile fișiere/nume/hash-uri înregistrate rămân neschimbate.

V2 normalizează fiecare candidat fiscal înainte de alegerea priorității,
completează CF-urile recuperabile, păstrează necunoscutele NULL și nu
reclasifică NIR-uri pe baza textului STOC/SOLD INITIAL. Nu creează
furnizori și nu deduce CF după nume. Nu există fallback de identitate
în online; vechile chei sunt citite doar în fișierul de tranziție.
Valorile/cantitățile/reviziile și coloanele istorice nu sunt modificate.
DONE în registru înseamnă conversie aplicată, nu toate CF-urile rezolvate.
Se păstrează rollback-ul pe contradicții între CF factura/NIR cunoscute
și pe facturi care au NIR-uri legate cu mai multe CF-uri cunoscute.
Un NULL și un CF cunoscut nu sunt tratate drept două identități contradictorii.

În arborii NIR/FCS, selectorul de sursă storno și ramurile de rapoarte,
grupul fără CF este etichetat explicit Fără cod fiscal, nu cu numele
istoric arbitrar al primului document. Cantitățile și numele istorice pe
document rămân vizibile; grupul nu reprezintă un furnizor dedus din nume.
Asocierea eFactura rămâne interzisă fără CF. Antetul comun de deployment
indică V2 drept migrare recomandată; help descrie documentele nerezolvate.

Fișiere: sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V2.sql (nou);
PYTHON/routes/vercon/{receipt_tree,report_incoming_selection,
report_selection,supplier_identity}.py;
PYTHON/static/js/vercon/{document-tree-order,storno-editor}.js;
PYTHON/static/help/vercon.json; implementare_vercon_v2.md;
docs/worklog/SLICE-VC03-09-migrare-cf-v2-documente-istorice-fara-cf.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). NIR-urile fără CF au nevoie în continuare de date fiscale explicite pentru a primi o identitate de furnizor; migrarea nu pretinde rezolvarea acestor informații absente.


### 2026-10-23 — VC03-10: migrare CF v3: ignoră partenerii Ascuns=1

Clarificare explicită a utilizatorului: partenerii cu Ascuns=1 NU au NIR-uri
sau alte documente. Nu este necesară și nu s-a implementat vreo logică de
propagare/excludere pentru documente ale unor parteneri ascunși.

Pregătit sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V3.sql,
nume NOU Portare_parteneri_codfiscal_v3. Păstrează V2 și adaugă doar
AND COALESCE(P.Ascuns,0)<>1 la cele două căutări în Parteneri (recuperarea
CF pentru NIR și factura comună). NULL este tratat ca 0. Tabela Parteneri
nu este actualizată, ca și în variantele precedente. Documentele istorice
fără CF nu blochează baza; contradicțiile între CF-uri cunoscute rămân
blocante. Nu se inventează CF și nu se reclasifică documente.

Fișierele și interogările originale/V2 rămân neschimbate, deoarece pot
fi deja înregistrate în Interogari_Unice. V3 folosește nume și hash noi;
nu inversează schimbări ale unei rulari deja încheiate.
Fișiere: sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V3.sql (nou);
PYTHON/routes/vercon/supplier_identity.py (referință la V3);
implementare_vercon_v2.md;
docs/worklog/SLICE-VC03-10-migrare-cf-v3-parteneri-ascunsi.md.
Stare: SQL PREGĂTIT, NEEXECUTAT de asistent. Fără teste, verificări
automate/de sintaxă sau acces MariaDB. Nicio modificare de date/structură
executată. De rulat de utilizator prin Interogări unice, cu numele V3.

### 2026-10-23 — VC05-11: VRC.Migrator: aceeași identitate fiscală ca VERCON online

Cerere: alinierea strict a VRC.Migrator la fluxul fiscal VERCON, cu protejarea
celorlalte module și a partenerilor Ascuns=1 (care nu au documente).

Eliminat dicționarul CF → IdPartener și încărcarea ID-urilor din Parteneri.
Importul NIR scrie direct CF normalizat în CodPartener, păstrând separat
CodFiscalSursa; facturile provizorii copiază aceeași identitate din NIR în
VC_FACTURI.CodPartener, fără PARTENER_ID. Lipsa rândului din Parteneri nu
anulează CF-ul sursă. Lipsa CF/IDF nerecunoscut produce X04, valoare NULL;
CF mai lung de 50 produce X07, fără trunchierea identității. Stocul inițial
păstrează regula existentă și identitatea NULL. Normalizarea elimină RO doar
înainte de 0–9, ca implementarea online, păstrând literele și zerourile inițiale.

Actualizate mesajele preflight/raport/ANAF. Setul codurilor existente include
și partenerii ascunși EXCLUSIV ca gardă împotriva propunerii/creării unui
duplicat: nu se modifică/reactivează niciun partener existent. AddInTransaction
normalizează și codul pregătit înaintea verificării existenței și a inserării.
Numerele locale deja ocupate (inclusiv ascunse) se rezervă ca înainte pentru
a evita coliziuni; codul local Parteneri.CodPartener nu leagă documentele.

Controlul de schemă folosit de Testează și înaintea tranzacției cere CodPartener
textual, scriibil, minimum 50, NULL permis, pe VC_NIR și VC_FACTURI; ID-urile
istorice, dacă există, trebuie să permită NULL. DDL-ul existent este doar indicat
utilizatorului, nu executat. Nu s-a introdus rularea automată a V3, reimport,
actualizare a documentelor istorice sau creare automată de schemă.
Versiune fișier VRC.Migrator: 0.3.12.0; RuleVersion:
VC05-SUPPLIERS-CF-2026-10-23. CPV din Access păstrează maparea existentă;
asocierea eFactura rămâne responsabilitatea aplicației online.

Fișiere: VERCON/VRC.Migrator/VrcRunner.vb, VrcRunner.Invoices.vb,
VrcWriter.vb, VrcWriter.Schema.vb, VrcConvert.vb, VrcPartnerStore.vb,
VrcMigratorForm.vb, VrcPartnersForm.vb, VrcRun.vb, VrcImportMap.vb,
VrcPlan.vb, VRC.Migrator.vbproj și README.md; jurnalul de față și
implementare_vercon_v2.md. Nicio modificare în src/KBot.Migrator,
ADE.Migrator, migratorul contabil, DDF/ORD/extrase sau maparea produselor.

Stare: SCRIS LOCAL, NEPUBLICAT. Citire și analiză de proiectare în surse;
fără teste, build, verificări automate/de sintaxă, rulare de executabil,
importuri sau acces MariaDB/DDL, conform AGENTS.md. Executabilul existent
în _VrcMigrator trebuie recompilat/publicat înainte de folosirea regulii noi.


## 23.10.2026 — text care urmează lățimea ferestrei (panou de test)

`typography.css`: fontul de bază `--fs-base` devine valoarea în vigoare acum
(`clamp(min, max - (start - lățime) * rată, max)`), cu `--fs-max` (16px),
`--fs-fit-start` (1920px), `--fs-fit-min` (11px) și `--fs-fit-rate` (0 = oprit,
deci nicio schimbare vizibilă). Pașii `--fs-2xs … --fs-2xl` sunt acum proporții
ale bazei (la 16px dau exact valorile vechi). Compus cu zoomul din meniu prin
`--application-vw`.

Panou de test, doar în modul test: butonul «Aa» (stânga jos), Ctrl+Alt+F sau
`kbotDebug.fontFit()`. Glisoare pentru maxim, minim, lățimea de la care scade și
lățimea la care atinge minimul; valorile rămân între pagini (localStorage);
«Copiază valorile» dă liniile de pus în `typography.css`; «Resetare» le scoate.

Fișiere: PYTHON/static/css/typography.css, PYTHON/static/css/font-fit-debug.css,
PYTHON/static/js/utils/font-fit-panel.js, PYTHON/static/js/utils/debug-tools.js.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Rămân de ales valorile finale și paginile pe care se aplică (CSS-urile care folosesc `calc(var(--fs-base) ± Npx)` urmează baza, dar cu pas fix în px).

## 23.10.2026 — antet NIR / BC: etichetele lângă control când gridul are sub 10 rânduri

Când gridul de produse nu poate afișa cel puțin 10 rânduri, antetul documentului
(NIR și BC) primește clasa `is-labels-inline`: eticheta stă pe aceeași linie cu
controlul, antetul nu depășește lățimea gridului (celulele pot să se micșoreze,
eticheta lungă se taie cu «…»), iar cele trei rânduri rezervate pierd linia de
etichetă. Numărarea se face pe antetul înalt (etichete deasupra), în același cadru
cu comutarea, ca să nu apară pâlpâire la limită.

Fișiere: PYTHON/static/js/vercon/header-fit.js (nou), css/vercon-header-inline.css
(nou), css/vercon.css (import), js/vercon/docs.js (import + o linie după crearea
rădăcinii). Prag 10 rânduri în `minRows` (docs.js).

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). docs.js are 1028 de linii (peste ținta de ~1000, deja 1026 înainte). Ajutorul VERCON nu a fost actualizat.

## 23.10.2026 — antet NIR pe o linie: Cotă TVA deasupra, Furnizor și Cont egale

Doar în modul «etichete lângă control» (`is-labels-inline`): «Cotă TVA» are eticheta
deasupra listei, într-o coloană îngustă; lățimea cedată merge la prima coloană
(Furnizor, Cont, Comisie); Furnizor și Cont au aceeași lățime de etichetă, deci aceeași
lățime de control; «Categorie / cont» se arată «Cont» (două texte în același
`<span>`, comută CSS-ul; în modul normal rămâne «Categorie / cont»).

Fișiere: css/vercon-header-inline.css, js/vercon/receipt-allocation.js.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Cotă TVA urcă pe primul rând al antetului (coloana 4) în același mod; css/vercon-header-inline.css.

Corecție: Cotă TVA — eticheta pe rândul 1, lista pe rândul 2 al grilei antetului (display: contents), aliniată cu celelalte controale; regulile NIR ale modului sunt acum doar peste 800px.

## 23.10.2026 — antet BC pe trei rânduri

Antetul bonului de consum (BC) are acum trei rânduri pe două coloane: Primitor |
Număr BC; Cont | Data; Explicații pe toată lățimea. «Categorie / cont» se numește
«Cont» la BC (în toate mărimile). Cele trei rânduri rezervate (documente-layout)
sunt acum folosite integral. Sub 800px rămân regulile existente (2 coloane, ordinea
din document).

Fișiere: css/vercon-consumption.css, js/vercon/docs.js.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Panoul de test al fontului: butonul «Aa» scos (rămâne în cod, `mount({ button: true })`); se deschide doar cu Ctrl+Alt+F sau kbotDebug.fontFit(), iar fără deschidere nu rulează nimic, deci valorile de test păstrate în browser nu mai ating paginile.

## 23.10.2026 — Rapoarte: butoanele de jos prea sus

Grila paginii Rapoarte avea patru rânduri declarate (minmax(0,1fr) auto auto auto); cu doar panouri și butoane, ultimele două rânduri goale își adăugau fiecare câte 12px de spațiu sub butoane. Acum un singur rând fix pentru panouri și rânduri automate dedesubt (grid-auto-rows). Fișier: css/vercon-workspace.css. Neîncercat în browser; ipoteza vine din CSS, nu dintr-o măsurătoare.

Rapoarte: .vc-reports__actions are acum și padding-bottom: 10px (css/vercon-rapoarte.css).
