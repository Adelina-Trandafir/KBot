# Memorie de lucru — controalele custom pentru VERCON

Data: 12.10.2026. Sursă: citirea codului din `PYTHON/static`, în chatul despre mockupurile `MOCKUPURI/2026-10-12-corectate-v2`. Acest fișier păstrează harta pentru continuarea lucrului după compactarea contextului. Este inventar și proiectare; nu confirmă funcționarea în browser.

## Cerința utilizatorului

„le vom FOLOSI PE TOATE si pe pagina asta” — toate familiile de controale custom reutilizabile din static vor fi folosite în pagina VERCON, fiecare în rolul său. Reutilizarea înseamnă importarea componentelor existente, cu CSS-ul lor și ciclul lor de viață, nu imitarea aspectului prin controale locale. Variantele și opțiunile unei componente se aleg după flux; modulele de business ale Portalului/ADECHIT nu devin operații VERCON doar pentru că se află în static.

Referința vizuală: NIR este modelul comun al grilelor; arborii documentelor sunt în dreapta în consultare și se ascund la adăugare/modificare. Lista de alimente admite mai multe BC-uri. Vezi [FLUXURI_UI](FLUXURI_UI.md) și [indexul mockupurilor](MOCKUPURI/2026-10-12-corectate-v2/INDEX.md).

## Harta componentelor

Toate căile de mai jos sunt relative la `PYTHON/static`.

| Familie | Intrare în cod | Stiluri | Rol în VERCON |
|---|---|---|---|
| DataGrid | `js/dgv/datagrid.js` — `DataGrid`, `ValueType`, `Operator` | `css/dgv.css` | Linii NIR/BC/BP/BT/BS, rezumate și feluri de mâncare, BC-uri asociate, registre OI/MF, nomenclatoare, istorice și rapoarte tabelare |
| TreeView | `js/components/treeview/treeview.js` — `TreeView` | `css/treeview/treeview.css`, care importă cele șapte fișiere de stil ale familiei | Produse/categorii în stânga, documente salvate în dreapta, alegere BC, arbori de rapoarte; `inline: true` pentru panouri permanent deschise |
| Combobox | `js/components/combobox/combobox.js` — `Combobox` | `css/combobox.css` | Context, furnizor, cont/categorie, produs și variantă de preț, primitor/predător, comisie, fel de mâncare; și editorul de listă din DataGrid |
| DatePicker | `js/components/datepicker/datepicker.js` — `DatePicker` | `css/datepicker.css` | Date document/factură/intrare și intervale de raport; și editorul de dată din DataGrid |
| Meniu popup comun | `js/adechit/menu.js` — `openMenu`, `closeMenu` | `css/app-menu.css` | Meniu aplicații, opțiuni ⋮ pe document/rând, acțiuni și filtre de footer |
| Dialoguri comune | `js/utils/confirm-box.js` — `confirmBox`, `reasonBox`, `pickBox` | `css/confirm-box.css`, încărcat de modul | Renunțare la draft, confirmări, motiv de anulare, alegeri simple; popupul complex Alege BC compune TreeView într-un dialog |
| Viewer PDF | `js/portal/pdfview.js` — `createPdfView` | `css/pdfview.css` și `vendor/pdfjs/pdf_viewer.min.css` | Previzualizarea PDF a documentelor/rapoartelor; biblioteca și workerul din `vendor/pdfjs` se încarcă la prima deschidere |
| Mesaje comune | `js/portal/messages.js` — `showMessage`, `messageHistory` | Clasele `.msg` și variabilele comune de eroare | Stare, erori, rezultat salvare, jurnalul de mesaje |
| UI sesiune | `js/adechit/session.js` — `startSessionTimer`; familia `js/session/session-monitoring*.js` — singleton `sessionMonitoring` | Stilul timerului paginii; `css/monitoring.css` pentru monitorul complet | Timerul există deja în VERCON. Monitorul complet este o implementare existentă cu avertizare/prelungire; integrarea trebuie să respecte politica serverului și să aibă un singur responsabil pentru expirare |

## Contracte care trebuie păstrate

### DataGrid

- Constructor: `new DataGrid(container, { columns, rows, ... })`. Rânduri virtualizate, antet/footer fix, coloane înghețate/redimensionabile, sortare, filtre tipizate, grupare multinivel, agregate, selecție și tooltip pentru text tăiat. Teme `modern`, `classic`, `dark`.
- Coloană: `key`, `title`, `valueType`, `width`/`minWidth`, `visible`, `align`, `format`, `decimals`, `aggregate`; filtrele și sortarea se pot dezactiva per coloană. Pentru butonul ⋮: `display: 'button'`, `actionText`, `actionLabel`, callback `onCellAction({ row, key })`.
- Editarea este opt-in: `editable`, `rowKey`, `onCellSave`, plus `editable` pe coloană. Editoare existente: text, `number`, `date`, `list`, `checkbox`, `monthYear`. `parse` și `validate` pe coloană controlează conversia și validarea.
- `onCellSave({ row, key, value, previous, rowId })` poate întoarce rândul actualizat. La documentele VERCON callbackul va actualiza draftul; persistarea documentului rămâne în comanda Salvează document. Componenta nu impune salvare HTTP pentru fiecare celulă.
- API: `setRows`, `setColumns`, `setGroups`, `setFilter`, `clearFilters`, `sortBy`, `selectRow`, `getSelectedRow(s)`, `getColumnState`, `autoSizeColumns`, `setTheme`, `destroy`; editare prin `beginEdit`, `beginNewRowEdit`, `commitEdit`, `cancelEdit`, `hasEdit`.
- `setRows` refuză reîncărcarea în timpul editării: se confirmă/anulează celula înaintea schimbării documentului/contextului. `onEditState`, `onEditError`, `onCellSaved`, `onRowValidate` sunt punctele de integrare.
- Footer: `footer: true`, `footerCaption`, agregate pe coloane, `footerAction: { label, title, active, menu, withCaption }`, `onFooterAction({ button })`. Acesta este punctul comun pentru Adaugă rând.
- Integrare v2 scrisă: footerAction respectă `disabled` și `span`; `setFooter({values,action,caption})` permite totaluri explicite independente de filtrare. Agregatele normale rămân pentru rândurile afișate. `setBusy` setează inert/aria-busy. Nerulat.
- Sumele VERCON păstrează valorile canonice zecimale ca șiruri și calculul pe server. Parserul numeric implicit al grilei întoarce `Number`; pentru valori monetare trebuie adaptor prin `parse`, fără pierderea contractului din `js/vercon/fields.js`.
- `destroy()` anulează editarea, observerul și listenerii, scoate instanța din registry și elimină grila.

### TreeView

- Noduri: `{ id, label, children }`; eticheta poate veni și din `name`/`text`. Extensii deja existente: `bold`, `error`, `tooltip`, `badge`, `rightIcon: { icon, action, title }`.
- Opțiuni: `inline`, `showSearchBox`, `searchPlaceholder`, `onSearch`, `onSelect`, `selectableLevel`, `requireDoubleClick`, `autoCollapse`, `autoResize`, `indent`, `checkable`, `branchChecks`, `onCheckChange`, `formatCheckSummary`.
- `onSelect` primește `{ id, label, path, level }`. ID-urile pentru ramuri și documente trebuie să fie distincte, cu mapare la obiectele business.
- API: `setData`, `updateResults`, `renderTree`/`renderTreeWithData`, `selectNodeById`, `markSelected`, `clearSelection`, `toggleNode`, `show`/`hide`, `destroy`. Datele și randarea sunt pași distincți; urmați integrarea existentă din Portal.
- `selectNodeById` deschide strămoșii, selectează și notifică `onSelect`; evitați încărcarea recursivă la reselectarea documentului salvat.
- `rightIcon` oferă markup pentru acțiune; integrarea deleghează click/tastatură pe `data-action` și `data-node-id`, cu oprirea selecției accidentale. Nu există în codul citit un callback generic `onAction` pe constructor.
- Mod checkbox: rezultatul este setul frunzelor bifate; `getCheckedLeaves`, `setCheckedLeaves`, `clearChecks`, `getCheckState`. Ramurile pot avea stare mixtă când `branchChecks: true`; nivelul superior nu are checkbox. Folosit unde fluxul cere selecție multiplă, fără să transforme automat popupul BC în alt flux.
- Necesită `js/utils/css.js` și `js/utils/z-max.js` înaintea instanțierii. `destroy()` elimină arborele, overlayul, timer-ele și listenerii urmăriți.

### Combobox

- Constructor cu container, nu cu `<select>`: `new Combobox(container, options)`.
- Date: șiruri sau `{ value, label }`; callback `onSelect(value, text)`. `readonly: true` cu `staticData` oferă selecție din set fix. Pentru căutare la tastare se furnizează `onSearch(query)`; simpla setare staticData nu implementează filtrarea la tastare în `performSearch`.
- Opțiuni: `placeholder`, `searchDelay`, `minSearchLength`, `maxResults`, `highlightMatches`, `allowEmpty`, `showLoader`, `allowHtml`, `prefixIcon`, `showOnlyIcon`.
- API: `setValue(value, text)`, `getSelectedValue`, `getSelectedText`, `getInputValue`, `setEnabled`, `clear`, `setPrefixIcon`, `destroy`.
- Păstrați tipurile ID consecvente; eticheta afișată este separată de cheia trimisă serverului. La înlocuirea opțiunilor se actualizează `options.staticData` și selecția.
- Integrare v2 scrisă în `dgv/editing.js`: selecția inițială citește `label` cu fallback `text`; opțiunile pot depinde de rând. Editorul cu `searchable` folosește Combobox cu onSearch și invalidează cheia când se tastează fără selectarea unei opțiuni.
- Necesită ZIndexManager. Overlayul este mutat la deschidere în dialogul gazdă, dacă există; dropdownul este poziționat relativ la câmp și urmărește scroll/resize. Același suport pentru top layer este scris și în meniul comun. `destroy()` face cleanup și golește containerul.

### DatePicker

- `new DatePicker(input, { startDate: () => Date | null })` transformă inputul sursă în ascuns și construiește un câmp vizibil cu calendar. Sursa păstrează ISO `YYYY-MM-DD`; afișarea este `zz.ll.aaaa`.
- Modificările emit `change` pe sursă. `setValue(Date | null, notify)`, `iso`, `commitText`, `open`, `close`, `destroy`; nu atribuiți un șir ISO lui setValue.
- Într-un `<dialog>`, calendarul se montează în acel dialog. Distrugerea elimină wrapperul și restaurează tipul sursei la `date`.
- Pentru consultare/dezactivare adaptorul trebuie să controleze câmpul vizibil și butonul; constructorul nu are opțiune generică `disabled`.

### Meniuri, dialoguri, PDF

- `openMenu(anchor, entries, options)` cu intrări `{ icon, label, on, disabled, rule, run }`; opțiuni `label`, `align`, `up`, `checkable`, `iconRight`, `below`. Un singur popup activ; funcția întoarce închiderea, iar `closeMenu()` îl închide global. Navigare cu săgeți și Escape.
- `confirmBox(text, options)` → `Promise<boolean>`; `reasonBox(text, options)` → motiv fără spații marginale sau `null`; `pickBox(title, [{ value, label }], options)` → valoare sau `null`. Renunță este în stânga; confirmarea în dreapta. Motivul obligatoriu blochează confirmarea când e gol.
- `createPdfView(container)` → `{ open(ArrayBuffer | Uint8Array), clear() }`; clear eliberează documentul. PDF.js este o dependență vendor, nu un control nou de rescris. Viewerul este pentru citire.
- `js/adechit/reports.js: outputReport` este helperul separat pentru fereastra HTML de listare/descărcarea PDF și XLSX; `js/vercon/documents.js: createDocuments` îi adaugă contextul VERCON. Previzualizarea în viewer și descărcarea sunt funcții distincte.

## Infrastructura comună și variantele existente

- `js/event-bus/event-bus.js`: singletonul EventBus (`on`, `once`, `emit`, etc.), folosit de controale și mesaje. Evenimentele UI nu trebuie să transporte inutil date business.
- `js/listener-tracker/*`: `ListenerTracker.applyTo`, listeneri DOM/bus, timer-e și callbackuri de cleanup; componentele dependente îl importă. Păstrați cleanup-ul la schimbarea formularului/contextului și `pagehide`.
- `js/instances-registry.js`: `registerInstance`, `getInstance`, `unregisterInstance`; DataGrid se înregistrează singur. Nu goliți global registry-ul pentru a demonta un singur ecran.
- `js/utils/css.js`: măsurători de stil; `js/utils/z-max.js`: ZIndexManager. `css/variables.css`, `css/portal.css` și `js/portal/theme-init.js` dau tema comună. Tema persistentă folosește cheia `kbot-portal-theme`.
- `js/portal/layouts.js` + `grid-layouts.defaults.js`: layouturi DataGrid `{ order, hidden, widths, fill }`, `layoutId`, `installLayouts`, overrideuri de administrator, import/export. VERCON necesită ID-uri de grilă proprii; nu reutilizați accidental configurația unei grile Portal.
- `js/portal/menu.js: createMenu` este varianta veche cu intrări de business Portal fixate în modul; `js/portal/apps-menu.js` folosește popupul comun. Reutilizarea pentru VERCON se face prin primitivele de meniu comune și datele meniului VERCON, păstrând vizibilitatea calculată de server.
- Monitorul complet `js/session/session-monitoring.js` și timerul ADECHIT sunt două mecanisme existente. Nu porniți simultan două mecanisme de avertizare/expirare și nu introduceți prelungirea sesiunii contrar politicii serverului. Înaintea integrării, alegeți/adaptați responsabilul comun; harta nu autorizează schimbarea politicii de sesiune.

## Situația după rescrierea v2 (cod scris, nerulat)

- `vercon.html` include stilurile tuturor familiilor. `ui.js` compune câmpuri, grile, arbori, meniuri și dialoguri cu scope/cleanup.
- `docs.js`, `lista.js`, `editor.js`, `mf.js`, `nomenclatoare.js` au fost rescrise cu componentele comune; fără salvări HTTP per celulă la documente.
- Arborele documentelor este în dreapta în consultare și ascuns în editor; trei mese alăturate și popup Alege BC cu TreeView și comandă explicită de adăugare.
- Previewul tabelar poate produce un PDF local paginat prin `snapshot-pdf.js`, afișat în viewerul PDF comun, descărcat și deschis pentru tipărire. Este o imagine a valorilor afișate, nu implementarea rapoartelor oficiale.
- `app.js` folosește Combobox pentru toate selectoarele, meniurile comune, istoricul mesajelor, layouturile DataGrid și timerul unic de sesiune. Timerul are cleanup; monitorul complet nu este pornit în paralel.
- Evidența pe fiecare ecran, limitele și detaliile de continuare sunt în [implementare_vercon_v2.md](../implementare_vercon_v2.md). Nu au fost executate teste, build, verificări de sintaxă, aplicația sau schimbări MariaDB.

## Reguli pentru continuare

1. Toate familiile reutilizabile de mai sus intră în proiectarea VERCON; o variantă se activează doar unde fluxul o cere.
2. Aspectul compact NIR, diferența consultare/editor, draftul și protocolul de salvare rămân contracte ale paginii; componentele sunt adaptoare UI ale acestor contracte.
3. Fără teste/rulări/alte verificări fără solicitare explicită a utilizatorului; citirea codului și proiectarea sunt permise. Pentru această hartă nu s-au rulat teste sau aplicația.
4. Nu se modifică scheme MariaDB și nu se adaugă creare automată de schemă; eventualul SQL se pregătește în fișiere pentru operator, conform AGENTS.md.
