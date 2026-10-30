# 2026-10-17 — OI: conturi multiple, documente și rapoarte

Cerere: implementarea predării, transferului, casării și evidenței/rapoartelor OI, cu mai multe conturi posibile atât în magazie, cât și în folosință.

## Documente

BP alege independent contul de magazie (dintre cele cu recepții în context) și contul de folosință (din nomenclatorul activ), fără clasificare după prefix sau cont unic pe gestiune. Un BP păstrează o pereche sursă–destinație în antet; alte perechi cer alte BP. Selectorul cu mai multe conturi rămâne activ.

BT/BS aleg numai loturile predătorului, identificate prin documentul de proveniență, variantă, cont de folosință și preț. Un bon admite conturi diferite, păstrate cu prețul pe fiecare linie. Prețul clientului identifică lotul; serverul salvează valoarea citită din lotul eligibil.

Disponibilul la data documentului exclude documentul editat și respectă ieșirile ulterioare; salvarea îl revalidează tranzacțional, cu cronologie pe deținător/lot/cont/preț. Dependențele protejează anularea/editarea; proveniențele/datele incoerente primesc diagnostic, fără reparare tacită. BP păstrează ieșirea unică de magazie și încărcarea registrului; BT, cele două sensuri cu net zero.

Editorul OI este în pagină, cu arbore ascuns și spațiul folosit integral. Grilele au footer pentru adăugare și totaluri; BT/BS aleg prin arbore cont–lot/produs/preț. Casarea nu are primitor. Ciornele OI calculează valorile prin întregi BigInt, la patru zecimale de stocare și rotunjirea ZEC, iar serverul păstrează Decimal. Denumirile conturilor/deținătorilor inactivi rămân disponibile în consultarea documentului istoric.

Fișiere: `PYTHON/routes/vercon/oi.py`, `oi_custody.py`, `doc_routes.py`, `protocol.py`; `PYTHON/static/js/vercon/docs.js`, `oi-ui.js`, `oi-values.js`, `consumption-ui.js`; `PYTHON/static/css/vercon-oi.css`, `vercon.css`.

## Evidență și rapoarte

Stocul în folosință se grupează deținător–contul mișcării–variantă, fără moștenirea contului de magazie. Grila/selecția păstrează identitatea contului; utilitarul decimal comun adună cantități cu precizii diferite în același răspuns.

Meniul/catalogul OI oferă BP/BT/BS, centralizatoare, balanță, stocuri pe deținător/produs, situații/fișe și inventariere. Contul de folosință se selectează independent de arborele produselor și contul lor de magazie. Perioada acoperă primul NIR/BP–ultimul an relevant, numai ultimul expandat inițial; admite intervale între ani.

Bonurile păstrează valorile istorice ale liniilor. Balanțele și fișele sunt evaluate la prețul curent, cu notă explicită. Soldurile inițiale preced intervalul, iar predările/transferurile/casările includ ambele capete. Totalurile pe produse păstrează conturile distincte. Inventarierea lasă goale câmpurile pentru constatarea faptică; nu inventează diferențe.

KBotAdmin configurează patru surse SELECT: `oi_documents`, `custody_ledger`, `oi_accounts`, `oi_holders`. Se reutilizează modelul comun de raport, snapshoturile, PDF și lățimile Excel. Preview oficial pentru bon OI salvat; local pentru ciorne/anulate și cititori fără drept separat de raport. MF rămâne amânat, ca anterior.

Corectată mascarea funcției `selected` de variabila locală din balanța magaziei, care bloca filtrarea comună a rapoartelor de magazie, inclusiv în OI.

Fișiere: `PYTHON/routes/vercon/stock_source.py`, `stock.py`, `capabilities.py`, `report_catalog.py`, `report_data.py`, `report_oi.py`, `report_routes.py`, `report_service.py`, `report_layouts.py`, `report_body.py`, `report_store.py`; `PYTHON/routes/admin_cli/vercon_reports.py`; `PYTHON/static/js/vercon/stock.js`, `reports.js`, `report-selection.js`, `report-result.js`, `document-report.js`; cele patru fișiere SQL din `PYTHON/reports/vercon/queries/` și README-ul lor.

## Stare și limite

IMPLEMENTAT ÎN SURSE, NERULAT. Nu s-au executat teste, verificări de sintaxă, build, cereri HTTP, browser sau interogări MariaDB, conform instrucțiunilor proiectului și cererii de implementare înaintea probelor. Au fost citite sursele și analizate contractele de date/interfață pentru proiectare. Machetele OI folosesc infrastructura existentă și încă necesită acceptare vizuală; nu se declară paritate dinamică cu rapoartele Access.

Nicio schemă sau bază de date modificată. Nu s-au adăugat DDL, seed ori creare automată de tabele: schimbările folosesc coloanele existente. Nu s-a publicat și nu s-a extins serverul demonstrativ local la OI.

Completare de trasabilitate D001-04: descrierea generală din 17.10 consemna explicit că loturile cu prețuri diferite rămân distincte chiar pe aceeași variantă/document de primire; drepturile, motivul anulării și păstrarea ciornei la eșec refoloseau protocolul existent. Fără extinderea sau repornirea serverului demonstrativ pentru acel lot.

## Arbori, numerotare și editori BP/BT/BS — 18.10.2026

D001-09, istoric: **IMPLEMENTAT ÎN SURSE, NERULAT**. Numai design din surse; fără teste/build/verificări automate, browser/HTTP, MariaDB/SQL/DDL, schimbări de schemă/date, migrator, deploy/commit/push. Aspectul nu a fost probat în aplicație.

Etapele de mai jos revizuiesc editorul din 17.10: arborele BP rămâne vizibil/dezactivat, rândul gol înlocuiește footerul Adaugă, iar ultima etapă mută mini-fișele din stânga în dreapta. Ordonarea după identitate descrie implementarea istorică; nu înlocuiește regula curentă AGENTS de sortare după tipul real.

<a id="d001-09-etapa-3"></a>

### Bonuri OI/MF: arbore direct și conturi BP

Arborii BP/BT/BS au deținător → bonuri, fără lună: BT după primitor, BS după predător (fără primitor). Grupare după identitatea deținătorului; bonuri după număr descrescător, apoi dată și ID, cu număr/valoare în nod. BP pune contul de folosință sub cel de magazie, în aceeași coloană. Listele respectă TIP OIM/OIF sau MFM/MFF și eligibilitatea existentă a conturilor NULL. Mecanismul comun selectează automat unicul cont eligibil și dezactivează controlul.

<a id="d001-09-etapa-16"></a>

### Bonuri MF/OI: grupare opțională după dată

Checkboxul Ordoneaza dupa Dată, sub arborele BP/BT/BS, este bifat implicit: lună în română/an descrescător, TOTAL lunar exact din bonurile neanulate; copii cu tip/număr, dată, valoare, descrescători după dată, apoi număr și identitate. Anulatele rămân marcate; cele fără dată formează ultimul grup separat.

Debifat: gruparea, etichetele și ordonarea existente pe gestionar. Comutarea reface numai arborele, păstrând documentul selectat. Checkboxul stă sub zona derulabilă și urmează ciclul de viață al paginii.

<a id="d001-09-etapa-17"></a>

### Bonuri MF/OI: număr implicit separat P/T/C și produse cu disponibil pozitiv

La adăugare BP/BT/BS, Număr propune următorul din seria tipului: maximul numerelor existente (inclusiv anulate) și al contorului. Se verifică la ieșirea din câmp și la salvare; unic pe tip/gestiune, admis pe tipuri diferite. Propunerea doar citește; contorul avansează tranzacțional sub blocarea contextului. Identitățile interne PTC păstrează numerotarea comună, distinctă de numărul afișat pe tip.

Clarificarea utilizatorului: BP filtrează stocul magaziei; BT/BS, pe cel al predătorului. Produs oferă numai disponibil pozitiv la data bonului, excluzând la editare mișcarea proprie (BC asociat BP, document TC în registru). Scade celelalte rânduri ale ciornei, păstrând disponibilul celui editat. Loturile/produsele cu 0 și categoriile astfel goale dispar. BP încarcă stocul la deschiderea editorului și schimbarea datei; salvarea păstrează verificările tranzacționale de stoc/cronologie.

Numerotarea UI a trecut din docs.js într-un modul dedicat pentru delimitarea responsabilităților/dimensiunii, cu comportamentul NIR/FCS păstrat.

<a id="d001-09-etapa-18"></a>

### Bonuri PTC: editare ca la BC și mini-fișe TC

La adăugare/editare BP/BT/BS: Renunță stânga, mesaje centru, Salvează bonul dreapta, fără preview. Grilele au permanent rând gol, fără Adaugă rând; acesta nu se salvează și nu cere confirmare la schimbarea predătorului.

BP păstrează arborele vizibil/dezactivat în editare, inclusiv adăugarea și ordonarea. În PTC, arborele se aliniază cu zona grilei; checkboxul rămâne stânga, centrat vertical în rândul barei de acțiuni.

BT/BS ascund coloanele Cont în folosință/Lot proveniență și denumesc disponibilul Disponibil; identitățile lotului/contului rămân în ciornă și validarea stocului. În editare, stânga: fișa predătorului, a primitorului numai la BT, mișcările produsului selectat. Istoricul BP/BT/BS arată număr/dată/valoare; la produs, și cantitatea/traseul predător–primitor. Fișele semnează ieșirile negativ și intrările pozitiv.

Fișele se citesc read-only pe subunitate/gestiune/exercițiu/data bonului, fără anulate sau ciorna nesalvată; ignoră cererile depășite la schimbarea selecției/contextului. Sub-numărul NIR/FCS are editor separat, pentru dimensiunea modulului comun.

<a id="d001-09-etapa-19"></a>

### Bon transfer: Primitor și Predător diferiți

În adăugare/editare BT, fiecare căutare exclude deținătorul celuilalt câmp. Aceeași persoană este respinsă și la schimbarea câmpului: validarea locală marchează ambele câmpuri și blochează salvarea; serverul păstrează refuzul transferului către sine. Adaptorul comun admite opțiuni calculate din starea curentă, cu listele statice neschimbate.

<a id="d001-09-etapa-20"></a>

### Mini-fișele transfer/casare în dreapta

Corecția utilizatorului: mini-fișele BT/BS trec în dreapta, formularul/grila în stânga. Ordine: predător, primitor (numai BT), mișcări produs; DOM: formular, apoi panouri. Pe ecrane înguste rămân sub formular.

### Fișiere pe etapă

Căi istorice; numele fără prefix continuă directorul precedent.

| Etapă | Fișiere |
|---|---|
| 3 | PYTHON/static/js/vercon/oi-ui.js; PYTHON/static/css/vercon-oi.css; PYTHON/routes/vercon/oi.py. |
| 16 | PYTHON/static/js/vercon/oi-tree-order.js, docs.js; PYTHON/static/css/vercon-oi.css. |
| 17 | PYTHON/routes/vercon/oi_number.py, oi.py, doc_routes.py; PYTHON/static/js/vercon/document-number.js, oi-product-choices.js, oi-ui.js, docs.js. |
| 18 | PYTHON/static/js/vercon/docs.js, oi-context.js, oi-tree-order.js, document-subnumber.js; PYTHON/static/css/vercon-oi.css; PYTHON/routes/vercon/oi_context.py, doc_routes.py. |
| 19 | PYTHON/static/js/vercon/docs.js, ui.js. |
| 20 | PYTHON/static/css/vercon-oi.css, PYTHON/static/js/vercon/oi-context.js. |

## Diagnosticul listelor BP/BT/BS — 18.10.2026 <a id="d001-12-diagnostic-oi"></a>

Preluare D001-12. Ambele corecții sunt în surse, nerulate: numai design pe baza logului/sursei, fără teste/probe automate/runtime, browser/HTTP ori MariaDB/SQL executat de asistent. Fără schemă/date/migrator/deploy.

- Logul BS indica MariaDB 1064 pentru aliasul neîncadrat lines. Interogarea comună BP/BT/BS folosește line_count, răspunsul API păstrând lines.
- Diagnosticul SQL al utilizatorului în 000_DEMO confirma charset/collation binary pentru CONCAT la BP/BS. Lista citește separat MAX(ID), SUM(Version) și construiește textul Python în formatul _rev, id-maxim–sumă-versiuni cu separator ASCII '-'; cheia API rămâne version, fără rezultat binar CONCAT. Guardul VERCON emite success numai după pregătirea JSON, evitând succesul înaintea unei erori de serializare.

Fișiere istorice (numele fără prefix continuă directorul precedent):

| Etapă generală | Fișiere |
|---|---|
| 9 | PYTHON/routes/vercon/oi.py; implementare_vercon_v2.md. |
| 10 | PYTHON/routes/vercon/oi.py, PYTHON/routes/vercon/__init__.py; implementare_vercon_v2.md. |
