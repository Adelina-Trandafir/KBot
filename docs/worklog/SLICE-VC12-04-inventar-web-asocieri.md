# SLICE-VC12-04 — Miniaplicație web pentru asocierea inventarului OI

Data: 06.11.2026.

## Scopul final autorizat

Pagină web separată, la **`/vercon-inventar`**, cu autentificarea și drepturile VERCON Online. Alegerea finală a Adelinei este varianta web. Miniaplicația lucrează **exclusiv cu obiecte de inventar (OI)**. Precizarea ulterioară «NU LE FACEM PE MF!» este aplicată în interfață, reader și server. Nu există formular MF, clasificări sau durate, iar operațiile pe o gestiune MF sunt refuzate. Modulele existente de mijloace fixe nu sunt modificate.

Sunt păstrate cerințele și răspunsurile explicite ale Adelinei:

- Comparare cu rândurile existente din `VC_ProduseP`, în baza și Gestiune / Subgestiune / Cont selectate.
- Două grile alăturate. Excel: **NrCrt, Denumire, UM, Cant, PU**. VERCON: **Ordinea, Denumire, UM, Cant, PU**.
- La alegerea unui rând Excel apar toate variantele eligibile din context, fără limita de cinci și fără pragul de excludere textuală al vechii comparații. Ordinea implicită este scorul descrescător, cu departajare după denumire în română și preț numeric.
- Cantitate asociată, preț Excel, preț VERCON și buton Asociază în partea dreaptă. Când cantitatea rămasă ajunge la zero, poziția dispare din lista disponibilă. Asocierile se văd separat și se pot anula înaintea trimiterii.
- Diferența de preț cere confirmare numai dacă valorile rotunjite HALF_UP la două zecimale diferă. Prețul exact din Excel se păstrează la precizia acceptată de stoc (maximum patru zecimale).
- Situația VERCON este cea la **31.12.2025**, cumulând magazia și folosința; eligibilitatea cere o intrare efectivă pozitivă dintr-un document neanulat, nestorno, datat **înainte de 01.01.2026**. Data creării/modificării rândului din nomenclator nu înlocuiește data intrării.
- La trimitere, cantitățile asociate păstrează informațiile Excel, iar cantitățile Excel neasociate se adaugă în stoc inițial la **31.12.2025**.
- Pentru asocieri parțiale, informațiile Excel se aplică numai cantității asociate, cu păstrarea restului VERCON. Adelina a ales explicit **corecții documentate la 31.12.2025**, cu păstrarea documentelor istorice.

## Interfață și autentificare

Intrare directă: `/vercon-inventar`. În meniul de aplicații VERCON există și **Inventar Excel**. În lipsa sesiunii se deschide `/portal?next=/vercon-inventar`, cu autentificarea existentă (parolă și cod / certificatul existent), apoi se revine în miniaplicație. Nu există autentificare nouă, acces SSH sau credențiale de administrator.

Se reutilizează controalele proiectului: `DataGrid`, `Combobox`, `TextBox`, confirmările și tooltipurile custom, timerul sesiunii și mecanismul de lock VERCON. Numerele au tip numeric în grile, textele se ordonează în română, iar sensul de sortare ales se păstrează la reumplerea grilelor. Selectorii cu etichete compuse folosesc numele propriu pentru ordonare.

Selectorul Cont oferă conturile active **MAGAZIE / OIM**, corespunzătoare contului produsului și destinației stocului inițial. Cantitatea de folosință a variantelor acelui cont este inclusă indiferent de contul de folosință al loturilor; deținătorii și conturile loturilor se păstrează la corecție.

Asocierile sunt locale, în memoria paginii, până la «Trimite în VERCON». Schimbarea contextului/fișierului/foii cere confirmare dacă sunt asocieri nesalvate. Reîncărcarea/închiderea paginii le pierde; există protecția browserului la părăsirea paginii. Expirarea sesiunii folosește politica comună a aplicației. Nu se persistă automat un proiect de asociere pe disc.

## Excel și sursa VERCON

Reader XLSX server-side prin ZIP/XML, fără biblioteci noi și fără fișiere temporare persistate. Macheta acceptată este cea furnizată: NrCrt în A, denumire B, cod/număr de inventar C, UM D, cantitate scriptică F și PU I. Secțiunile se identifică din antetul K/L. Antetele repetate, totalurile și semnăturile nu devin poziții.

Secțiunea MF este omisă înaintea validării/citirii câmpurilor ei de bunuri. Pentru foaia analizată anterior, `scoala 2025`, scopul este reprezentat de cele **575 OI**; cele 8 poziții MF sunt omise. Fișierul original nu este modificat și nu este copiat în proiect. Formulele folosesc valorile memorate în XLSX; aplicația nu recalculează Excelul.

Limite de citire: 20 MB XLSX, 25 MB per componentă XML, 10.000 componente ZIP și 5000 poziții OI. Prețurile și cantitățile sunt validate pentru `DECIMAL(20,4)`; sunt acceptate formele cu punct/virgulă și se normalizează numai zgomotul numeric permis de modulul comun `numeric`.

`VC_Produse` furnizează numai numele, UM și contul prin JOIN cu `VC_ProduseP`. Nu se adaugă candidați artificiali pentru produse fără variantă. Stocurile se calculează din motoarele existente `stock_source`, `stock` și `oi_custody`, cu identitatea completă a loturilor din folosință. Variantele fără cantitate pozitivă la data inventarului nu apar ca disponibile. Variantele inactive cu cantitate sunt vizibile, cu explicația inactivității.

Scorul reutilizează motorul existent de similitudine textuală/preț; este orientativ, nu probabilitate. Parametrul nou al motorului are valoarea implicită veche, astfel încât funcția KBOTADMIN își păstrează pragul de candidați. Numai miniaplicația cere toate variantele și le ordonează după scor.

## Aplicarea în VERCON

Serverul cere dreptul VR `read` pentru citire și dreptul VR **`import`** pentru trimitere, cu context reverificat și lock exclusiv al gestiunii. Nu sunt introduse drepturi noi. Citirile folosesc tranzacție READ ONLY cu snapshot REPEATABLE READ în sintaxa MariaDB; trimiterea folosește o singură tranzacție, cu blocarea rândului gestiunii, commit numai la final și rollback integral la eroare.

Lotul se verifică față de amprenta stocurilor și a metadatelor încărcate. Modificarea lor între încărcare și trimitere oprește lotul și cere recitirea comparației. Pentru corecțiile care chiar schimbă informații se respectă și cantitățile consumate/predate/transferate/casate ulterior datei inventarului: nu se lasă stoc sau lot negativ în viitor. Pozițiile identice pot fi asociate fără această retragere fizică; rămân existente și sunt consemnate în audit.

Pașii pentru date diferite sunt:

1. Se creează poziția țintă cu denumirea, UM, codul și PU din Excel, separat pentru fiecare rând Excel. Variantele sunt reale `VC_ProduseP`.
2. Din magazie, cantitatea asociată este reclasificată prin linii semnate ale documentelor `STOC_INITIAL`, cu marcaj **CORECTIE INVENTAR EXCEL**, la 31.12.2025: scădere pe varianta veche și creștere pe cea nouă. Documentele istorice nu se rescriu.
3. Din folosință, loturile sunt alocate în ordine cronologică, după folosirea cantității disponibile din magazie. Se folosesc familiile existente **BS și BP** pentru corecția de ieșire/intrare la același deținător și în același cont de folosință. Explicația este **CORECTIE INVENTAR EXCEL 31.12.2025 — RECLASIFICARE, nu casare fizica**. BP generează și ieșirea de magazie normală; nu se dublează cantitățile între magazie și folosință. Confirmarea din ecran anunță explicit familiile BS/BP înainte de trimitere.
4. Resturile Excel se adaugă separat în documente `STOC_INITIAL` cu marcaj **STOC INITIAL EXCEL**, la aceeași dată. Prețurile nu primesc încă o dată TVA; este necesară o cotă TVA 0 activă, deja definită în gestiune.
5. Se verifică liniile de timp ale variantelor atinse și conservarea cantității rămase pe fiecare variantă veche. Cantitatea pozițiilor noi trebuie să coincidă cu totalul Excel care nu a fost păstrat pe poziții identice. Auditul păstrează legătura rând Excel → variantă veche/nouă, cantitatea, deținătorul și datele originale Excel.

BS/BP de corecție apar în familiile de documente existente, inclusiv în rulajele acelor familii, cu explicația de reclasificare; nu este introdusă o familie contabilă nouă. Corecțiile sunt documente separate, nu redenumirea globală a nomenclatorului și nici modificarea cantității tuturor pozițiilor cu aceeași denumire.

Repetarea aceluiași lot folosește `VC_Operations`, pentru a nu duplica scrierile la pierderea răspunsului. Cheia depinde de pozițiile OI normalizate ale sursei, foaie și cont, în contextul gestiunii; modificarea doar a secțiunii MF sau a formatării Excel nu produce o cheie nouă. O altă repartizare pentru aceeași sursă deja aplicată este refuzată. Dacă răspunsul este incert, interfața păstrează lotul nemodificabil și permite reluarea exact a aceleiași cereri.

Comparația server-side este păstrată temporar în STORE, legată de utilizator, unitate și subunitate/gestiune, cu termen de 4 ore reînnoit la acces. Nu există tabele noi, DDL sau creare automată de schemă.

## Stare și livrare

**ÎNREGISTRAT PENTRU SERVER DUPĂ CONFIRMAREA ADELINEI (06.11.2026).** Adelina a confirmat «inregistreaza fisierele. am verificat», apoi a cerut completarea înregistrărilor lipsă ale inventarului. Sunt înregistrate toate cele 21 de fișiere proprii, inclusiv documentația, cu motivul acestui slice. Agentul a analizat codul și contractele controalelor/motoarelor existente pentru proiectare; fără teste, verificări de execuție, compilare, previzualizare în browser, SQL, conexiuni la baze de date/SSH, publicare sau Git.

Pentru probă sunt necesare publicarea fișierelor Python/static și reîncărcarea backendului prin fluxul autorizat. Nu este necesară recompilarea KBOTADMIN. Pagina este în proiectul Flask existent; nu este creat sau publicat un Site extern.

Înregistrarea prin `tools/Register-DeploymentTask.ps1` păstrează motivele celorlalte slice-uri ale acelorași fișiere și nu execută publicare sau Git pe server. Consemnarea confirmării nu afirmă că agentul a testat fiecare flux de scriere în baza de date.

## Fișiere proprii create/modificate în acest task

- `PYTHON/routes/admin_cli/inventory_similarity.py`
- `PYTHON/routes/vercon/doc_routes.py`
- `PYTHON/routes/vercon/inventory_excel.py`
- `PYTHON/routes/vercon/inventory_source.py`
- `PYTHON/routes/vercon/inventory_write.py`
- `PYTHON/routes/vercon/inventory_routes.py`
- `PYTHON/static/vercon-inventar.html`
- `PYTHON/static/css/vercon-inventar.css`
- `PYTHON/static/css/vercon-inventar-layout.css`
- `PYTHON/static/css/vercon-inventar-controls.css`
- `PYTHON/static/js/vercon-inventar/api.js`
- `PYTHON/static/js/vercon-inventar/app.js`
- `PYTHON/static/js/vercon-inventar/actions.js`
- `PYTHON/static/js/vercon-inventar/controls.js`
- `PYTHON/static/js/vercon-inventar/grids.js`
- `PYTHON/static/js/vercon-inventar/state.js`
- `PYTHON/static/js/portal/portal.js`
- `PYTHON/static/js/vercon/application-menu.js`
- `docs/worklog/SLICE-VC12-04-inventar-web-asocieri.md`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`
