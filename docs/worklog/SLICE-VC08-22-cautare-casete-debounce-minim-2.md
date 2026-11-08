# SLICE-VC08-22 — Căutare în casetele VERCON: pauză editabilă și minimum 2 caractere

Data: 06.11.2026.

## Cerere și implementare

La cererea Adelinei, toate casetele VERCON Online care declanșează o căutare (cu excepția grilelor DGV, unde lucrează alt agent — VC08-21) așteaptă o pauză în tastare (debounce, implicit 0,3 s, editabilă din setări) și pornesc căutarea după primele 2 caractere tastate, fără a număra spațiile.

- **Regula comună**: `static/js/vercon/search-settings.js` (pauza, minimum 2, `debounced`, `effectiveSearchText`, `searchOptions`) și `static/js/utils/search-text.js` (lungimea textului fără spații). Pauza se păstrează în `localStorage` (cheia `vercon.search.settings`), între 100 și 3000 ms, implicit 300.
- **Setarea**: intrarea «Setări căutare» în meniul aplicațiilor (sigla V), în `search-settings-dialog.js`, cu aspectul ferestrei «Setări tooltip». Se aplică la deschiderea următoarei pagini.
- **Controalele comune** (Combobox, TreeView, ComboTree) primesc opțiunea nouă `minSearchIgnoreSpaces`; fără ea (ADECHIT și celelalte aplicații) comportamentul rămâne identic. Deschiderea explicită a listei (săgeata, focus cu deschidere) nu este oprită de minim (`performSearch('', true)`). La ComboTree, un text sub minim nu filtrează (arată lista completă), iar textul tastat rămâne în casetă. În TreeView, căutarea locală folosește pauza și minimul paginii doar cu opțiunea activă (altfel rămân 300 ms / 3 caractere).
- **Căsuțele conectate**: câmpurile de listă din `ui.js`, copacul `paintTree`, `report-period`, `consumption-ui`, `receipt-allocation`, `storno-editor`; căutarea proprie din `report-selection` (copacul de selecție a rapoartelor, pauza și minimul 2 în loc de 300 ms / 3), `cpv-picker` (denumire și cod), `partners` (Furnizori) și `nomenclator-grid` (filtrul «Caută…»).
- **Neschimbat**: grilele DGV (filtre pe coloane, VC08-21), listele fără tastare de căutare (readonly / simple, tastare-salt), apelurile programatice `search(value)` ale nomenclatoarelor.

## Fișiere proprii create/modificate

- `PYTHON/static/js/utils/search-text.js` (nou)
- `PYTHON/static/js/vercon/search-settings.js` (nou)
- `PYTHON/static/js/vercon/search-settings-dialog.js` (nou)
- `PYTHON/static/js/vercon/application-menu.js`, `ui.js`, `report-period.js`, `consumption-ui.js`, `receipt-allocation.js`, `storno-editor.js`, `report-selection.js`, `cpv-picker.js`, `partners.js`, `nomenclator-grid.js`
- `PYTHON/static/js/components/combobox/combobox.js`, `combobox-search.js`, `combobox-events.js`
- `PYTHON/static/js/components/treeview/treeview.js`, `treeview-search.js`
- `PYTHON/static/js/components/combotree/combotree.js`
- `PYTHON/static/help/vercon.json` (secțiunea «Căutarea în casete», slice VC08-22)
- `docs/worklog/SLICE-VC08-22-cautare-casete-debounce-minim-2.md`, `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md`

## Stare și limite

**ÎNREGISTRAT PENTRU SERVER, după verificarea Adelinei (06.11.2026): 20 de fișiere JS/JSON de server.** Documentația nu se înregistrează. Agentul a verificat doar: sintaxa modulelor JavaScript (`node --check`) și logica `search-settings.js` rulată în Node (fără `localStorage`: revenire la 300 ms). Nu s-au rulat browserul, teste, SQL, publicare sau Git.

Proba pe ecran a făcut-o Adelina; agentul nu a deschis browserul. Numărul slice-ului a fost ales de agent (următorul liber lângă VC08-21), la indicația «VCnn-nn»; se poate redenumi.

Publicarea rămâne a operatorului.
