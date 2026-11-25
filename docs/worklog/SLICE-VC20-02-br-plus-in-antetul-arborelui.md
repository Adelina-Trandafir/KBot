# SLICE-VC20-02 — Bon de retur (BR): «+» în arbore, editare pe loc ca la BC / FCS

18.11.2026 — Cerință (Adelina): la Bonuri de retur, «+» în headerul arborelui ca în celelalte machete; apoi, în același slice: (1) «Bonul returnat» combo cu tastare, doar bonurile cu produse de returnat (la Alimente, fără cele de pe liste); (2) tabel clasic cu rând gol, ca la FCS; (3) butoanele eliminate; (4) în aceeași fereastră, nu popup, ca la bonul de consum; (5) în arbore, la hover, opțiunile de la BC.

## Ce s-a schimbat

- **Editare pe loc (4).** BR era singura familie ne-inline și se edita într-un popup (`ModalFrame`). Acum e inline ca BC și FCS: titlu, antet, tabel și bara de jos în pagină, arborele rămâne alături. Fără panourile de context (`receiptContext` nu se creează pentru BR).
- **Butonul «+» (cerința inițială).** În antetul arborelui («Documente salvate»), titlu «Adaugă bon de retur»; «+ Adaugă» din bara de jos dispare.
- **Bonul returnat (1).** Combo cu tastare (`return-editor.js`, `returnSourceField`, același control ca la FCS): bonurile grupate pe primitor, căutare după text. Serverul (`documents.return_sources`, ruta `/doc/br/sources`) întoarce numai bonurile de consum neanulate, cu cel puțin un produs cu cantitate rămasă de returnat (retururile existente scăzute; la modificare, returul editat nu se numără) și care nu sunt legate de o listă de alimente. Lista se reia la fiecare «+».
- **Tabel clasic (2, 3).** Coloanele Produs, UM, Cantitate sursă, Deja, Rămas, Cantitate, Preț cu TVA, Valoare; rând gol mereu la sfârșit; produsul se alege dintr-un arbore al liniilor bonului (`returnProductNodes`, numai cele cu «Rămas» > 0 și neluate pe alt rând); cantitatea trebuie să fie pozitivă și cel mult «Rămas». Dispar «+ Adaugă rând» și «Previzualizează» (bara editării: «Renunță» / «Salvează bonul», ca la BC). Schimbarea bonului returnat cu produse alese întreabă înainte să le scoată.
- **Arbore (5).** Grupare cu fereastra Data / Primitor / Cont, ca la BC, și «Modifică număr» în meniul ⋮ al unui retur (regula BC: luna în curs, ordinea datelor): `number_change.py` cunoaște acum `br` (rânduri `VC_Iesiri_H` cu `Retur=1`, prefix BX, contorul `BR`).
- **CSS.** Reguli `.vc-doc--br` oglindite după BC / FCS în `vercon-consumption.css`, plus `.vc-doc--br` în listele existente din `vercon-documente-panels.css`, `vercon-integration.css`, `vercon-status.css`, `vercon-responsive.css`.
- **Ajutor.** `vercon.json`, secțiunea `br`: titlu «Adăugarea unui retur» (VC20-02), pașii rescriși.

## Fișiere

- `PYTHON/static/js/vercon/docs.js`, `return-editor.js` (nou)
- `PYTHON/routes/vercon/documents.py`, `doc_routes.py`, `number_change.py`
- `PYTHON/static/css/vercon-consumption.css`, `vercon-documente-panels.css`, `vercon-integration.css`, `vercon-status.css`, `vercon-responsive.css`
- `PYTHON/static/help/vercon.json`, `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md`, acest worklog

## Presupuneri

- «Eliminate butoanele» = «+ Adaugă rând» și «Previzualizează» din editare (cele două care nu există la BC/FCS); «Renunță» și «Salvează bonul» rămân.
- «Opțiunile de la BC în arbore, la hover» = meniul ⋮ al nodului (inclusiv «Modifică număr») și fereastra de grupare Data / Primitor / Cont.

## Neverificat

Doar sintaxa (`py_compile`, `node --check`, JSON valid). Nu s-a rulat nimic: nici pagina (aspect, selecția produsului, rândul gol, mesajele), nici interogarea SQL `return_sources` (GROUP BY / HAVING pe date reale), nici «Modifică număr» pe un retur, nici editarea unui retur salvat (liniile se completează din bonul sursă). Un retur vechi cu produse nereturnate pe rânduri goale își păstrează doar rândurile cu cantitate. Nepublicat; `docs.js` depășea deja 1000 de linii (crește cu ~25). Înregistrarea pentru publicare se face abia după proba Adelinei (fișierele VC20-02 se adaugă celor VC20-01 deja înregistrate, `docs.js` fiind comun).
