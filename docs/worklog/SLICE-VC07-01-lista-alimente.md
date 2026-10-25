# SLICE-VC07-01 — ListaAlimente

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata (`py_compile`, `node --check`, verificare automata de nume nedefinite); **nimic rulat**, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («continuă cu VC07»).

## Ce s-a facut
- **Lista de alimente** (`lista.py`, `js/vercon/lista.js`, rutele `/api/vercon/doc/lista…`): antet (numar automat/manual, data, comentarii), cele trei mese (mic dejun, dejun, cina/gustare) cu **porții, valoare porție, valoare masă (= porții × valoare porție, rotunjit o data la ZEC) si distribuit**, feluri pe masa (fel din nomenclator + tip + explicatie; randurile fara explicatie nu se salveaza, ca in Access) si **setul de BC-uri**.
- **O lista are mai multe BC-uri** (`lista_bc.set_bonuri`, 1:N): DGV in stilul NIR, un rand per bon, `+ Adaugă rând` deschide popupul **Alege B.C.** (bonurile gestiunii si exercitiului, neanulate, fara retur, fara a fi in alta lista), alegerea ADAUGA la set, ⋮ scoate doar legatura din draft. Footer: TOTAL BC, valoare lista, diferenta.
- **Salvarea e atomica** (o tranzactie): antet + mese + feluri + intreg setul LegBCLA; nu creeaza si nu resalveaza bonuri, nu repeta consumul; un BC devenit neeligibil opreste salvarea intregului set (`BON_INVALID`), draftul ramane. Lista fara bonuri cere confirmare. `ValoareLista` = suma meselor; `ValoareBon` = suma bonurilor neanulate; `DEP` = lista − bonuri.
- **Anulare** cu motiv: lista ramane marcata anulata, **bonurile sunt eliberate** (legaturile se sterg, bonurile nu se ating). **Editare** cu revizie (`CONFLICT`).
- **Nomenclator «Feluri de mâncare»** (`VC_FeluriMancare`) adaugat in meniul Configurari pentru gestiunile Alimente.
- Arbore **luna → data → lista + valoare** in dreapta, ascuns la adaugare/modificare.
- O modificare de reteta nu rescrie o lista salvata (lista isi pastreaza textul si valorile).

## Fisiere
`PYTHON/routes/vercon/`: `lista.py` (nou), `doc_routes.py`, `nomenclatoare.py`, `capabilities.py`; `PYTHON/static/js/vercon/lista.js` (nou), `app.js`, `nomenclatoare.js`.

## Presupuneri si neverificat
- **Retetarul (`VC_Retetar`) nu este implementat**: tabelul nu are `Activ`, are cheia proprie `IdFel` fara legatura clara cu `FeluriMancare` si **0 randuri** in bazele reale; nu s-a inventat un model. Ramane deschis.
- Sensul coloanelor `D` («distribuit») si `VMD`/`VD`/`VC` (valoare/portie) este luat din formularul Access (`MicDejun_V = porții × VMD`); distribuitul nu este limitat la porții.
- «Alimente necesare»/costuri din reteta nu exista (eliminate prin decizie, FLUXURI_UI).
- Cele 127 de liste istorice nu au fost reconciliate pe snapshot (necesita rulare); legaturile lor LegBCLA existente se citesc ca atare.
- Nimic rulat.

## Evoluție UI și solduri — 17.10.2026

Opt etape din jurnalul general, absorbite în D001-06. Starea fiecăreia: implementat în surse, numai analiză de cod/design (și a logului pentru editorii FELUL), fără teste, compilări, browser, verificări automate sau acces MariaDB. Sursele nu au fost publicate. Pythonul nou cere publicare și restart; schimbările numai JS/CSS cer publicarea fișierelor lor. Acest istoric înlocuiește particularitățile UI anterioare, fără a certifica executarea.

### Antet, mese și arbore

- Număr/Data sunt pe primul rând al antetului cu două coloane, Comentarii pe ambele; ulterior Număr și Data au 70% din coloana lor (−30%), cu pozițiile păstrate. Antetul/BC asociate ocupă inițial două părți egale ale zonei de sus.
- Cele trei totaluri devin TextBox readonly, fără DGV separat: Valoare listă/Total BC/Diferență, din ciornă. Etapa compactă: centrate sub Comentarii, maximum 480 px, câmpuri egale cu text/valoare centrate; pe ecrane mici vertical, maximum 180 px fiecare. formatMoney = mii punct, virgulă, două zecimale, inclusiv la focus; precizia calculului/payloadului nu se reduce. Această prezentare este înlocuită ulterior de recapitularea în cinci coloane.
- Mesele au sumar numai Porții/Preț/Valoare; D este ascuns, dar valoarea istorică se păstrează la salvare. Felurile au FELUL/Explicații/⋮, fără ordinal/Porții, Explicații umple restul. Rând nou permanent în editor, fără Adaugă în footer; rândurile complet goale nu se salvează și nu apar în previewul ciornei.
- Explicații este Combobox cu tastare liberă și sugestii pentru felul ales: textele listelor neanulate din subunitate/gestiune/exercițiu și cele din draft. Textul nou salvat cu lista devine sugestie. `allowCustom` se aplică numai coloanelor care îl cer: păstrează textul liber și nu alege automat prima sugestie; celelalte coloane rămân.
- ActionAddButton din antetul arborelui respectă dreptul de scriere/perioada închisă; + Adaugă din bara paginii este eliminat. Arbore cu două niveluri Lună/An în română → listă (număr/data în etichetă, valoare dreapta ca NIR), cu ID-uri stabile pentru luni.
- Headerul actions/ordinal cu padding lateral 22 px impunea minimum 45 px, depășind lățimile rândurilor și producând hscroll gol chiar fără vscroll. Paddingul devine zero numai pentru BC asociate și cele trei grile de feluri; scroll/calculul lățimilor rămân când conținutul chiar depășește. Previzualizare este scoasă din bara editării.
- Antetul formularului are etichete bold. Numai în Lista de alimente, fonturile DGV sunt cu 1 pt sub fs-table la antet și cu 1 pt sub fs-ms la celule/rânduri/footer; editorii moștenesc celula.

### Sold anterior și recapitulare

- Mai întâi au fost adăugate două TextBox readonly Economii anterioare/Depășiri anterioare sub totaluri, compacte/centrate, mii și două zecimale inclusiv la focus, coloana fără sold goală.
- `food_balance.py` este calculul comun pentru raport și GET `/api/vercon/doc/lista/balance`, folosind sursele configurate food_history/settings, contextul subunitate/gestiune/exercițiu și dreptul Alimente. Sold anterior în convenția raportului = SI_LA − suma DEP a listelor cu data strict mai mică; negativ = economie, pozitiv = depășire. Sursa implicită exclude anulatele; diferențele lipsă produc eroarea raportului, fără zero inventat. Decimal, fără modificarea documentelor sau SI.
- Citire la deschidere/adăugare/modificare și schimbarea datei; exclude lista curentă la editare, inclusiv mutarea după data salvată. Răspunsurile întârziate nu se aplică altei liste/date.
- Etapa finală înlocuiește toate cele cinci câmpuri separate cu o recapitulare: sold anterior / Valoare listă / Total bon consum / Diferență / sold final. Coloane fixe de 150 px, etichete bold deasupra, separatoare/fontul aplicației, un singur rând centrat inițial sub antet/BC pe ambele coloane; la lipsă de spațiu se derulează, nu se comprimă.
- Prima/ultima etichetă sunt Economii Anterioare ori Depășiri Anterioare / Economii ori Depășiri după semn; valorile sunt absolute, două zecimale/mii; zero folosește Depășiri. Până la citire, soldurile sunt goale. Sold final = sold anterior − (Valoare listă − Total BC), ca «La finele perioadei»; se recalculează și în ciornă la mese/BC prin receiptTotal/BigInt la patru zecimale de stocare, afișat la două.
- Apoi `vc-lista__totals-wrap`, cu `vc-lista__totals`, devine copil al `vc-grid vc-doc__header`, imediat după Comentarii, nu copil separat al `vc-lista__overview`. CSS-ul recapitulării modificat de utilizator este păstrat.

### Editorii FELUL și Explicații

- FELUL: 57 px, padding zero în antet, dropdown minimum 120 px. Opțiunile se calculează la deschidere, fără felurile de pe alte rânduri ale aceleiași mese, păstrând felul curent; ștergerea îl readuce. Alegerea duplicată este refuzată și local. Pe rând fără fel lista se deschide automat după click, numai dacă editorul/focusul curent sunt încă valabile.
- Logul arăta blur → click/deschidere → timer de 150 ms. Combobox readonly păstrează focusul la mousedown; timerul nu închide o listă redeschisă și respectă focusul din dropdown. DGV păstrează focusul la reconstruirea rândurilor virtualizate.
- Editorul are simultan clase dgv__editor/combobox-container; CSS comun acoperă cazul și scoate limita implicită 300 px, pentru toată celula. Numai aici celula editată/inputul Explicații au padding zero; opțiunile pe un rând cu elipsă, fără hscroll. CSS-ul recapitulării utilizatorului rămâne neatins.

### Fișiere pe etape

Toate etapele actualizau și jurnalul general; căile fără prefix sunt în grupul indicat.

| Etapă | Fișiere |
|---|---|
| Antet/mese/arbore și allowCustom | PYTHON/static/js/vercon/lista.js; PYTHON/static/css/: vercon-lista.css, vercon-lista-totals.css; PYTHON/routes/vercon/lista.py; PYTHON/static/js/dgv/editing.js; PYTHON/static/js/components/combobox/combobox-search.js |
| Hscroll și bara editării | PYTHON/static/css/vercon-lista.css; PYTHON/static/js/vercon/lista.js |
| Totaluri compacte/fonturi | PYTHON/static/js/vercon/lista.js; PYTHON/static/css/: vercon-lista.css, vercon-lista-totals.css |
| Număr/Data 70% | PYTHON/static/css/vercon-lista.css |
| Solduri anterioare | PYTHON/routes/vercon/: food_balance.py nou, report_food.py, doc_routes.py; PYTHON/static/js/vercon/lista.js; PYTHON/static/css/vercon-lista-totals.css |
| Cinci coloane | PYTHON/static/js/vercon/lista.js; PYTHON/static/css/vercon-lista-totals.css |
| Mutarea în antet | PYTHON/static/js/vercon/lista.js |
| FELUL/Explicații | PYTHON/static/js/vercon/lista.js; PYTHON/static/js/dgv/: editing.js, datagrid.js; PYTHON/static/js/components/combobox/combobox-events.js; PYTHON/static/css/: dgv.css, vercon-lista.css |

Rapoartele și probele demonstrative din aceeași zi sunt în [istoricul raportului](SLICE-VC08-03-rapoarte-magazie-alimente.md#lista-de-alimente--machetă-și-probe-demo-17102026).
