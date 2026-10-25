# SLICE-VC06-03 — BC și controlul disponibilului

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata; **nimic rulat**, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («fa tot VC06»).

## Ce s-a facut
- **Bon de consum** (`create_bc/edit_bc`): primitor (`VC_Clienti`), categorie/cont, numar (automat, manual doar liber), data, explicatii; linii = varianta + cantitate. Pretul liniei este **pretul cu TVA al variantei la momentul salvarii**; linia il pastreaza apoi (si la editare, pentru o varianta deja pe bon). `ValoareBC` = suma `ValoareCUTVA`. Disponibil si pentru Alimente; nu pentru OI/MF.
- **Stoc negativ interzis** (`protocol.check_timeline`): in aceeasi tranzactie cu orice scriere, stocul fiecarei variante atinse este reparcurs pe zile (intrari la `DataFc`, retururi si iesiri la data bonului, storno la data documentului storno; documentele anulate nu conteaza; adunarile unei zile inaintea scaderilor ei). Daca vreo zi coboara sub 0 → refuz `STOCK_NEGATIVE`, rollback. Acopera si editarea/anularea unui document mai vechi.
- **Disponibil la data** (`GET /api/vercon/stock/available`): informativ in editor; documentul editat nu se numara de doua ori (`exclude`). La salvare decide verificarea de mai sus, la precizia gestiunii (1,0040 pe 1,0000 → refuz).
- **Acces exclusiv:** al doilea utilizator e blocat de `scoped()` (jeton de ocupare), inclusiv pe alt an.
- Pagina: grila BC compacta (NIR), coloana «Disponibil» cu avertizare, arbore **primitor → data → BC + total**.

## Fisiere
`PYTHON/routes/vercon/`: `protocol.py`, `documents.py`, `doc_routes.py`; `PYTHON/static/js/vercon/docs.js`, `app.js`; `PYTHON/static/css/vercon.css`.

## Presupuneri si neverificat
- Stocul se calculeaza pe exercitiu (un exercitiu are stoc initial propriu, cum le creeaza inchiderea anului la VC08); intre exercitii nu se aduna.
- Nimic rulat; nu s-a masurat timpul verificarii pe un volum real (Alimente are mii de linii: verificarea filtreaza pe variantele atinse).

## Corectie 12.10.2026 — stocul pe cont
Descoperit in Access (`QBC.sql`, `QBP.sql`): **un produs apartine unui cont (categoria lui, `VC_Produse.IdMasina`)** si un document are un cont; pe un document pot sta numai produsele contului lui. Pana la corectie, serverul si pagina nu tineau cont de asta (ofereau toate produsele).
- Serverul refuza acum, cu mesaj pe linie, un produs care nu e din contul documentului: NIR (contul recepției este obligatoriu), BC (contul bonului), BP (contul de intrare). Retur si storno mostenesc contul documentului sursa.
- Pagina ofera in combobox **doar produsele contului ales in antet**; la schimbarea contului scoate din linii produsele care nu mai se potrivesc (cu mesaj).
- Nomenclatorul Produse cere acum categoria (contul) produsului.
- Stocul ramane calculat pe varianta: fiindca o varianta apartine unui singur cont, stocul pe varianta **este** stocul pe cont. Ramane o diferenta fata de Access doar pentru istoric inconsistent (un BC cu alt cont decat al produsului): Access nu l-ar fi scazut din stocul contului, noi il scadem.
- Nerulat; doar sintaxa verificata.

## Editor BC și mini fișe — 17.10.2026

Două etape în surse, nerulate: numai citiri/design, fără teste, browser, verificări automate, SQL, acces MariaDB sau publicare. Prima cere publicarea surselor și restart Python; a doua publicarea JS/CSS.

- Arbore Primitor → lună, etichetă data/număr și valoare dreapta la două zecimale; ID-uri stabile, ramura documentului selectat se deschide după salvare/anulare. ActionAddButton în antet, fără + Adaugă de jos.
- Adăugare/modificare în pagina curentă: Primitor Combobox cu tastare/săgeată, Categorie ComboTree cu un cont al gestiunii. Primul rând Primitor/Categorie/Număr/Data, Explicații pe primele trei coloane ale rândului doi. Renunță stânga/Salvează bonul dreapta, fără preview de ciornă; documentele salvate păstrează preview.
- DGV ca NIR, rând nou permanent/ignorarea golurilor la salvare, fără Adaugă rând în footer; Produs Combobox cu tastare și focus în Cantitate după alegere. Disponibil precede Cantitate, citit informativ la data bonului.
- Inițial numai modificarea excludea BC salvat. Corecția ulterioară face **aceeași excludere în consultare și modificare**, ca disponibilul să coincidă; adăugarea nu exclude document salvat. Celelalte consumuri/retururi rămân.
- Lateral în editare: fișa gestionarului și mini fișa produsului de pe rând. Gestionar = ultimele 12 BC/retururi salvate, neanulate, din context/exercițiu până la data bonului și totalul net BC − retururi. Fișa produsului folosește serviciul de stoc și [cache-ul comun configurabil](SLICE-VC06-04-br-si-fcs.md#cache-ul-comun-al-mini-fișelor). Nicio fișă nu include ciorna; noile citiri cer drept BC și contextul serverului.
- Comboboxurile VERCON (câmp/opțiuni) au `--vc-combobox-font-size` cu 1 px sub anterior, păstrând diferența desktop/mic. NIR/FCS/BC au `--vc-mini-statement-font-size=calc(var(--fs-sm) - 1pt)`, același font UI pentru antet/identitate/note/tabel/mesaje/total.
- Mini tabelele au layout fix, coloane explicite dată/cifre, Document/Factură primește restul; text pe un rând, elipsă și tooltip complet. Numai vscroll, cu spațiul scrollbarului rezervat, fără lățire orizontală.
- Pe desktop antetele NIR/BC au aceeași înălțime minimă pentru trei rânduri și două intervale de 12 px. BC folosește primele două rânduri; spațiul rămas mărește fișa gestionarului până la începutul DGV. Responsive-ul mic rămâne.

Fișiere: prima etapă `PYTHON/static/js/vercon/{docs,consumption-ui,receipt-context}.js`, `PYTHON/static/css/{vercon,vercon-consumption}.css`, `PYTHON/routes/vercon/{receipt_context,doc_routes}.py`; corecția `docs.js`/`receipt-context.js` și `PYTHON/static/css/{vercon-controls,vercon-documente-layout,vercon-receipt-context}.css`; ambele actualizau jurnalul general.

## Revizii BC — 20/23.10.2026 <a id="d001-12-bc"></a>

Preluare D001-12; toate cele patru etape erau **Confirmat 23.10**, distinct de nerularea implementărilor anterioare.

- La adăugare se propune editabil următorul număr din seria BC/contor: simplă citire, fără rezervare/scriere. Salvarea verifică manualul și actualizează contorul în tranzacție; numărul se reverifică la părăsire, fără subnumere NIR/FCS.
- Produs devine TreeCombo produs → variante cu preț/stoc, limitat la cont și stoc pozitiv la data bonului minus celelalte rânduri din ciornă. Modificarea/consultarea exclud BC salvat curent. Încărcările sunt legate de document/dată, cele depășite ignorate; alegerea e blocată până există stocul. Data se poate schimba/reîncărca înaintea primului produs; după primul, câmpul și calendarul sunt dezactivate până se scot toate produsele. Cantitatea se confruntă cu disponibilul ciornei, pe lângă validarea exactă a serverului.
- Panoul Stoc produs înlocuiește Fișa de magazie: identitate, dată, preț, total intrări/ieșiri și disponibil în UM. Cantități exacte pe întreg istoricul variantei până la data bonului, numai mișcări salvate neanulate, excluzând BC curent în editare; fără limitare la 20/an și fără ciorna în totalurile istorice. Popupul BC împarte etichetele pe rânduri, fără hscroll; clasa este opțională prin DGV, CSS numai BC.
- Asocierea cu lista nu mai interzice singură editarea BC: după linii/total și validarea stocului, ValoareBon se recalculează din toate BC active ale listei și DEP = ValoareLista − ValoareBon. Versiunea și auditul listei sunt în aceeași tranzacție; eroarea anulează tot, mesele/valoarea listei rămân. Mesajul confirmă recalcularea economiilor/depășirilor.
- Editare/anulare numai ultimul BC activ, respectiv ultima listă activă, separat pe familie/context subunitate–gestiune–exercițiu. Ordinea istorică: dată, apoi ID descrescător în aceeași zi; anulatele nu contează, precedentul devine ultimul după anulare. Schimbarea datei ultimului nu îl poate muta înaintea precedentului. Lista recalculată trebuie să fie și ea ultima activă. Anularea BC legat cere încă scoatere din listă/anularea listei; retururile active protejează sursa, fără ștergere fizică. API-ul transmite blocaj/motiv, UI dezactivează Modifică/Anulează la anterioare și explică la deschidere; serverul aplică regula sub lock-ul tranzacțional existent. Descrierea istorică a ordinii nu înlocuiește AGENTS actual.
- Primitor unic la adăugare: completat în ciornă, control disabled; mai multe opțiuni = manual. La modificare, numai dacă existentul coincide cu unica opțiune sau lipsește; istoricul diferit nu se înlocuiește.
- Antetul BC final din 23.10 are trei rânduri/două coloane: Primitor | Număr BC; Cont | Data; Explicații pe toată lățimea. Eticheta Cont în toate mărimile; folosește integral cele trei rânduri rezervate. Sub 800px rămân două coloane și ordinea din document.

Fișiere istorice (numele fără prefix continuă directorul precedent):

| Etapă generală | Fișiere |
|---|---|
| 15 | PYTHON/routes/vercon/consumption_number.py (nou), consumption_stock.py (nou), doc_routes.py, receipt_context.py; PYTHON/static/js/vercon/ consumption-editor.js (nou), consumption-ui.js, document-number.js, docs.js, receipt-context.js; PYTHON/static/js/dgv/editing.js; PYTHON/static/css/vercon-consumption.css; implementare_vercon_v2.md. |
| 17 | PYTHON/routes/vercon/document_order.py (nou), food_list_recalculation.py (nou), documents.py, lista.py, doc_routes.py; PYTHON/static/js/vercon/docs.js, lista.js; implementare_vercon_v2.md. |
| 18 | PYTHON/static/js/vercon/docs.js; implementare_vercon_v2.md. |
| 28 | css/vercon-consumption.css, js/vercon/docs.js. |
