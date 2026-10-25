# SLICE-VC06-06 — facturi comune, corecții NIR și schemă unică

Data: 16.10.2026. Stare: **IMPLEMENTAT_LOCAL, FĂRĂ EXECUȚIE**.

Slice-ul include implementarea noului editor NIR aprobată ulterior în același
thread: [VC06-06-D01–D11](../../VERCON/DECIZII_DESCHISE.md#g-decizii-nir-și-contul-produsului--16102026-slice-vc06-06).
Documentația și sursele descriu acum același flux. Macheta separată de factură
respinsă de utilizator a fost eliminată.

## Comportament implementat în surse

Contractele sunt întreținute o singură dată: [editor NIR](../../VERCON/FACTURI_NIR_IMPLEMENTARE.md) pentru introducere/repartizare, TVA/AUTO și limite; [decizii D01–D11](../../VERCON/DECIZII_DESCHISE.md#g-decizii-nir-și-contul-produsului--16102026-slice-vc06-06) pentru cerințe; [schema](../../VERCON/SCHEMA_VC.md#infrastructura-de-schemă-implementată-anterior) pentru sursa reală și import. Acest slice a înlocuit macheta separată cu Adăugare NIR, a livrat selecția globală de conturi/produse, salvarea atomică pe destinații și blocarea contului produsului cu mișcări. Evoluția, alternativele retrase și probele izolate rămân în etapele datate de mai jos; nu sunt declarate probe ale întregului NIR.

## Livrabile

- Backend nou: `receipt_destinations.py`, `receipt_distribution.py`.
- Integrare backend: `doc_routes.py`, `documents.py`, `receipts.py`, `invoices.py`,
  `nomenclatoare.py`, `nomenclator_usage.py`; mecanismele anterioare de calcul,
  AUTO, cronologie și facturi rămân reutilizate.
- Frontend: `receipt-allocation.js`, `docs.js`, `receipt-context.js`,
  `nomenclator-grid.js`, ComboTree comun, integrarea în `dgv/editing.js` și
  inițializarea TreeView în containere încă neatașate paginii.
- CSS: `combotree/combotree.css`, `vercon-receipt-destinations.css`, `vercon.css`.
  Eliminate `js/vercon/invoices.js` și `css/vercon-invoices.css`.
- Livrările anterioare ale slice-ului: adaptările VRC/Migrator pentru schema
  reală, ordinea FK și fragmentele provizorii; SQL incremental manual
  [VC_FACTURI_NIR_20261016.sql](../../sql/VC_FACTURI_NIR_20261016.sql).
- Documentație aliniată: DECIZII_DESCHISE, FLUXURI_UI, PLAN_IMPLEMENTARE,
  VERCON_STATUS, SCHEMA_VC, CONTRACT_CALCULE, ANALIZA_FLUX_NIR_2026-10-16,
  FACTURI_NIR_IMPLEMENTARE, README VRC.Migrator și implementare_vercon_v2.

## Activare și limitele consemnării

Utilizatorul aplică DDL-ul facturilor pe AVACONT_SURSA și sincronizează schema
în DC-uri. Noul editor și blocarea contului nu cer un DDL suplimentar.
Aplicația și VRC nu creează/repară schema automat. Nu s-a pregătit un script
pentru eliminarea coloanei redundante, deoarece utilizatorul a confirmat că
nu aplicase încă DDL-ul. VRC 0.2.1.0 include KBot.Migrator 1.19.0.0.

Fluxul este în RON. Comisia trebuie să existe cu aceiași membri în destinațiile
alese; aplicația nu creează nomenclatoare sau exerciții automat. Fiecare NIR
rezultat se consultă/modifică în contextul său. API-ul de facturi istorice și
reconciliere rămâne disponibil, fără vechiul ecran respins.

Nu s-au rulat teste, build, browser, verificări automate sau SQL. Nu s-au
modificat baze, nu s-a făcut publicare sau commit. Citirea surselor pentru
proiectare nu certifică funcționarea în mediul utilizatorului. Modificările
fără legătură din KBot.App și directoarele Surse nu fac parte din acest slice.

## Etapele din jurnalul general absorbite prin D001-04 — 16.10.2026

Aceste note păstrează evoluția din același thread, inclusiv variantele retrase. Contractul principal de mai sus este cel adoptat pentru introducerea/repartizarea NIR; pozițiile și comportamentele UI au corecțiile succesive consemnate aici. Tot ce este marcat «în surse, nerulat» rămâne astfel; numai pagina izolată ComboTreePreview are probele explicite de mai jos.

### Calcul, factură și schemă

1. **Prima implementare FC/NIR:** separarea documents → receipts și motor comun de simulare/validare fără variante scrise la preview; cronologie (Numar,NumarSub) în seria anuală, poziții anulate păstrate, fără mutare în alt an; TVA comun derivat, ascuns pentru cote mixte, propagare blocată de AUTO. Corecție inversă cu TVA real, maximum 1 leu și egalitate exactă; origine/cantitate/preț inițial persistate. Tokenuri opace STORE pentru rânduri noi/reveniri, legate de DC/utilizator/context/document, fără cheie de configurare nouă. Editare/anulare cu cerere repetabilă și ciornă păstrată la rezultat necunoscut; dependențele OI/MF anulate nu blochează numai prin existența istorică. Liniile istorice nemodificate nu se recalculează la editarea antetului.
   Ecranul inițial «Factură comună» oferea recepții locale în etape, bugete de cantitate/valoare, reconciliere/reunire explicită; receive-batch atomic folosea token/revizie per destinație, lockuri ordonate și READ COMMITTED după blocarea facturii. Ecranul a fost respins și eliminat ulterior; API-ul istoric a rămas. VRC păstra fragmente provizorii/antete/mapare și asociere reluabilă fără reimport. Fluxul era RON, catalogul DNF numai dacă exista în sursa profilată; corecțiile Access la TVA 0% nu primeau retroactiv AUTO, fără dovadă a originii.

2. **Clasificări MF:** utilizatorul a anunțat AVACONT_COMUN.VC_ClasificariMF; VC_MF_CLASIFICARI local a fost scos din DDL, cerințele Python/VRC și importul Access. VC_ProduseM rămâne registrul activelor. După exportul comun furnizat de operator, receipts valida Cod=VC_Produse.CC și transforma anii în luni; limitele NULL ori clasificarea neasociată nu inventează interval. Catalogul comun este numai citit.

3. **Schema și importul:** TargetSchema.Load citește schema reală DC la Testează și înaintea tranzacției; AVACONT_SURSA live rămâne referința sincronizării, dumpul local nu este sursa migratorului. SchemaProblems verifică tabele/coloane necesare, obligatorii fără implicit/mapare, generate, AUTO_INCREMENT și DECIMAL(20,4), reutilizând coloanele scrierii. WriteOrder.Derive folosește FK reale + dependențe Access, păstrează overloadul vechi și verifică fazele nomenclatoare/documente. NULL/text sunt validate după metadatele reale; identitatea generată se citește fără presupunere RowId sau nume de cheie Access.
   target_schema.json/VrcTargetSchema au devenit import_map.json/VrcImportMap (identitate sursă, conversii/remapare, fără cache al nulabilității); expected_schema.json rămâne sursa Access. Eliminate sql/VC_AVACONT_SURSA.sql, tools/ddl.js și tables_01/tables_03.sql; tools/import_map.js emite mapare/domenii/diagramă, fără SQL. VC_Golire.sql rămâne listă istorică fixă, fără regenerare sau rol de schemă. VRC 0.2.0.0 cere biblioteca KBot.Migrator 1.19.0.0 cu overloadul nou; AssemblyVersion stabil.

4. **Metadatele calculului invers:** existența VC_NIR_CALCULE indică GROSS, păstrând TOTAL_BRUT, PRET_INITIAL, ORIGINE_JSON. write_calculation nu mai scrie un mod redundant; citirea selectează explicit ID linie + cele trei valori și derivă calc_mode=GROSS. Semnarea/blocarea/revenirea AUTO păstrează API-ul. DDL și cerințele VRC nu mai includ coloana; VRC devine 0.2.1.0. Fără script de eliminare, fiindcă operatorul confirmase că DDL nu fusese încă aplicat.

5. **Stagingul cerut atunci:** gruparea în VC06-06 și lista explicită de fișiere/înlocuiri/ștergeri excludeau Surse/EF_SURSA și Surse/SURSA_XFA_WRITTER; fără commit, utilizatorul ceruse staging și mesaj scurt. Ulteriorul lot doar documentar de decizii D01–D11 nu modifica indexul Git; apoi implementarea a fost autorizată separat. Acestea sunt acțiuni istorice, fără operații Git în D001-04.

6. **Resetul demo manual:** sql/VC_DEMO_golire_vercon.sql pregătit, nerulat. Bloc anonim MariaDB citește tabelele VC_ existente din information_schema și generează TRUNCATE calificat numai în 000_DEMO, inclusiv facturi/corecții/markeri de import, fără lista fixă istorică. Restaurează FOREIGN_KEY_CHECKS și la eroare, fără proceduri/tabele noi, fără Parteneri/AVACONT_COMUN. TRUNCATE netranzacțional poate lăsa reset parțial, raportat ca eroare; indexul Git pregătit anterior nu a fost schimbat pentru această completare.

Deciziile D01–D11 au fost întâi numai Markdown (fără cod/SQL/teste/build/verificări/commit), prevalând asupra machetei respinse. Implementarea ulterioară nu creează NIR goale; contextul produsului furnizează precizia/TVA și fișa sa, procentul din antet fiind mapat la ID TVA local. Semantica «aceeași subunitate» din nota inițială este explicitată în contractul adoptat prin destinația SubunitId/GestiuneId. Celelalte reguli sunt păstrate o singură dată în comportamentul de mai sus și în [FACTURI_NIR_IMPLEMENTARE](../../VERCON/FACTURI_NIR_IMPLEMENTARE.md).

### ComboTree și editorul DGV

- **Suprapunere/focus:** căutarea se suprapune exact pe poziția/lățimea/înălțimea inputului celulei, border-box; lista are mărime independentă, minWidth 300 px în NIR, încadrată sus/jos în viewport. Mousedown pe nod păstrează focusul ca DGV să nu confirme înainte de click; produsul confirmă prin onCellSave. Tastarea în oricare input invalidează alegerea, Escape/Tab anulează/confirmă, căutările amânate sunt oprite la închidere. selectParents implicit False, explicit False pe Produs: ramurile doar extind/restrâng, inclusiv Enter; expanderul nu selectează nici când opțiunea este True.
- **Tastatură:** combotree-keyboard.js separă navigarea. Sus/jos mută focusul din textbox în arbore și parcurge nodurile vizibile cu scroll; stânga restrânge/urcă, dreapta extinde/intră în primul copil; reafișarea păstrează nodul activ. Tastele de text revin în căutare și introduc caracterul; stânga/dreapta în textbox rămân native. Enter urmează clickul etichetei, inclusiv părinți/checkboxuri; focusul popupului este sincron, fără timer care să-l fure. focusAfterSelect:Cant pornește cantitatea aceluiași rând numai după salvare reușită și dacă nu a început alt edit; preview folosește quantity.
- **Corecția după proba operatorului:** beginEdit refocaliza inputul ascuns după deschiderea căutării; focusEdit alege acum inputul vizibil la intrare/refocalizare/eroare. highlighted exista fără fundal în TreeView: marcaj albastru limitat la ComboTree, fără chenarul de focus al listei. updateHighlight derulează imediat numai lista; tranzițiile și scroll anchoring ale popupului sunt dezactivate.
- **Aspect/numere:** editorul pornește cu același formatValue ca celula (0.5550 → 0,5550); textul neschimbat păstrează valoarea internă exactă, fără conversie/rotunjire/salvare. Parserul implicit acceptă gruparea românească la standard/currency/euro și semantica percent; parse explicit are prioritate. Editorul moștenește aliniere/font/spațiere/stilul cifrelor și ajustează paddingul după bordură; ComboTree primește aspectul prin API după montare. Outline nativ eliminat numai la editoarele DGV fără checkbox, păstrând bordura accent și aria-invalid.
- **Tab ulterior, fără probă:** Tab/Shift+Tab închide căutarea și reia navigarea de la inputul original, fără redeschidere la focus; tratat și pe popup, deoarece listenerul TreeView nu includea Tab. Categorie/cont folosește onTab și ordinea antetului, nu poziția popupului în body/dialog. În DGV rămâne prioritar handlerul capture de confirmare și celula următoare/precedentă.

### Pagina izolată ComboTreePreview — probe istorice efective

tools/ComboTreePreview/{index.html,demo.js,demo.css,preview_server.py,README.md}, cu componentele reale din PYTHON/static, 18 rânduri și arbore fictiv cu trei categorii/produse/subramură. Opțiuni selectParents, coloane 160/234/360 px, dropdown minim 300/420/560 px, jurnal/valoare confirmată/reset. CSS separat și importuri ordonate. Server stdlib exclusiv 127.0.0.1:5052, fără importul aplicației/DB; date numai în memoria paginii, fișiere în tools în afara publicării.

| Etapă | Proba consemnată, fără suite automate sau NIR pe baza reală |
|---|---|
| Prima pagină | server pornit/pagină deschisă; restrângerea categoriei lasă lista deschisă, alegerea actualizează Produs/UM/jurnal, ALMETTE găsește două produse; la coloana 160 px inputul/căutarea coincid 159,33×33,33 px, lista 420 px; celelalte combinații neprobate |
| Tastatură/Cant | server repornit; click/Enter trec în Cant, sus/jos în arbore, stânga/dreapta părinte/copil, textul revine la căutare, stânga în textbox mișcă cursorul; Enter pe rădăcină doar extinde/restrânge când selectParents=False |
| Focus/marcaj | probă prin focusul real după click, fără locator care refocalizează: tastare, săgeți și scroll repetat până la INVENTAR; nod integral vizibil și scroll păstrat ulterior, Enter către Cant. Proba precedentă cu focus explicit nu surprinsese defectul. Server/pagină rămăseseră disponibile |
| Aspect numeric | preview extins cu Preț/Valoare/UM editabil, două rânduri identice, font 15/20 px; server repornit, Preț lăsat în editare. 0,5550 și aliniere dreapta identice; Calibri 20 px/400/linie 20 px identic, verificat și 15 px; UM centrat, ComboTree 20 px; nemodificarea nu emite salvare, 1.234,50→1.234,60 salvează 1234.6, prețul original rămâne șirul 0.5550. Eliminarea outline a continuat aceeași probă |

La schimbarea ValoareFC, operatorul a cerut oprirea serverului 5052; toate corecțiile NIR/Tab de după au rămas numai în surse și serverul nu a fost repornit. Nicio probă de mai sus nu a executat SQL sau modificat baze de date.

### Antet NIR, reconciliere și acțiuni — evoluție în surse, nerulată

- ValoareFC: aliniat dreapta, numberLocale opțional en-US la TextBox money, format 1,234.56; citirea folosește valoarea canonică. Corectează a trecut lângă valoare, întâi ⚖️ apoi SVG monocrom currentColor cu tooltip/nume accesibil. Operatorul a înlocuit pragul inițial >=1 cu **0 < |diferență| ≤ 1 leu**; rămân restricțiile AUTO/drepturi/busy și recitirea condițiilor după confirmarea grilei. Comparația exactă BigInt la precizia documentului colorează ValoareFC roșu/bold la diferență; ulterior numai când există cel puțin un rând diferit de rândul gol. Nu s-a schimbat algoritmul serverului.
- Coloana Calcul/AUTO a fost scoasă din UI, calc_mode rămâne metadată; rândurile GROSS sunt bold/italic prin rowClassName, inclusiv după virtualizare. NrFact centrat; NIR/auto/subnumăr la dreapta. Fontul întregii grile NIR, inclusiv editori, redus cu 1 pt, fără alte grile. Footer Adaugă rând retras, rândul gol și totalurile păstrate. Bara editării: Renunță stânga, mesaje mijloc, Salvează NIR dreapta, fără Previzualizează (rămâne la consultare). Fișa fără mișcări spune PRODUS NOU.
- Stoc inițial fără furnizor a trecut în titlu, Ciornă la dreapta; numai adăugare, aceeași logică de furnizor/factură. Container golit la refacere și control eliberat prin scope. showMessage ascundea spanul gol și întindea Salvează în coloana flexibilă; container permanent și poziții grid explicite păstrează Renunță/Salvează la lățime egală 8em.
- vercon-button-effects.css importat ultimul: contur/umbră temă și ridicare 1 px la hover/focus, revenire/umbră interioară la apăsare, fără schimbare de layout; disabled exclus, Dark adaptat, reduced-motion fără mișcare/tranziții. Ulterior sunt excluse săgețile ComboBox/ComboTree, calendarul, clear textbox/căutare și butoanele Menu/popup (hover/focus/apăsare), care păstrează stilurile lor.
- vercon-control-actions.css: acțiuni listă/calendar în interior, 28 px, colțuri dreapta, hover temă, inclusiv lista simplă Comisie; eliminat hoverul verde ComboTree, săgeata ascunsă pe inline. Click pe opțiune/săgeată anulează activarea implicită label care refocaliza/redeschidea; selecție/închidere/căutare/destroy invalidează răspunsurile întârziate. Fișa laterală nu mai repetă furnizorul, identitatea produsului rămâne.
- La afișare se păstrează valorile validate de server: recalcularea binară paintLines/draftRows putea introduce 0,0001 lei după corecție. Modificările datelor păstrează recalcul + validare; footerul însumează exact cu BigInt prin receipt-reconciliation, mesajul corecției folosind formatul grilei.
- Comisie a trecut ulterior sub Categorie/cont, din prima coloană pe primele două, cu adaptarea existentă pe ecrane înguste. Blurul pornea confirmarea asincronă, iar Salvează primea False din al doilea commitEdit pending și se oprea fără mesaj: commitEdit reutilizează promisiunea. Formularul rezervă busy înaintea confirmărilor; la NIR suspendă row-leave în acea etapă, validarea strictă refuză totuși rândul incomplet. Fiecare oprire are mesaj și enumeră câmpurile blocante; «rezultat neconfirmat» apare numai după începerea scrierii, păstrând cheia/ciorna pentru reluare. Cauză din surse, nereprodusă în browser.
- Lista Comisie: limita comună 320 px a fost scoasă numai pentru acest dropdown; poziționarea urmează lățimea casetei pe două coloane, cu limita viewportului.

### Fișierele etapelor absorbite și limite

Backend: PYTHON/routes/vercon/{receipts,receipt_rules,receipt_auto,receipt_number,receipt_routes,invoices,documents,doc_routes,__init__,protocol,nomenclatoare,nomenclator_usage,receipt_destinations,receipt_distribution,tables,lock}.py; frontend: PYTHON/static/js/vercon/{docs,ui,receipt-reconciliation,receipt-context,receipt-allocation,nomenclator-grid}.js; fostul invoices.js și vercon-invoices.css sunt istoric retrase. Notele inițiale le includeau și pe acestea înaintea eliminării.

Controale: components/combotree/{combotree,combotree-keyboard}.js; components/combobox/{combobox,combobox-events,combobox-search,combobox-ui}.js; components/textbox/textbox.js; components/treeview/treeview.js; dgv/{editing,datagrid}.js (sub PYTHON/static/js). CSS: PYTHON/static/css/{dgv,vercon,vercon-receptie,vercon-button-effects,vercon-control-actions,vercon-receipt-destinations}.css și combotree/combotree.css. Pagina izolată și fișierele sale sunt enumerate mai sus.

Import: VERCON/VRC.Migrator/{VrcImportMap,VrcWriter,VrcWriter.Schema,VrcRunner,VrcRunner.Dishes,VrcRunner.Invoices}.vb, import_map.json, VRC.Migrator.vbproj și README; src/KBot.Migrator/MariaDb/WriteOrder.vb și KBot.Migrator.vbproj; VERCON/tools/{import_map.js,lib_tz.js,README.md}; SQL incremental VC_FACTURI_NIR_20261016.sql, VC_Golire.sql și resetul demo. Lista istorică include și scriptul complet/generatorul/fragmentele retrase, descrise mai sus.

Documente: VERCON/{DECIZII_DESCHISE,FLUXURI_UI,PLAN_IMPLEMENTARE,VERCON_STATUS,SCHEMA_VC,CONTRACT_CALCULE,ANALIZA_FLUX_NIR_2026-10-16,FACTURI_NIR_IMPLEMENTARE}.md, SCHEMA_VC_RELATII.mmd, README VRC, acest jurnal și indexul general. Surse și WIP KBot.App fără legătură au rămas în afara lotului istoric.

În afara probelor izolate explicit consemnate, fără teste/build/verificări automate/browser/SQL/MariaDB/publicare/commit. Generatorul mapării nu a fost rulat. DDL și sincronizarea sunt exclusiv manuale pentru utilizator; notele nu autorizează executarea lor în D001-04.

## Fișe NIR și editare în pagină — 15.10.2026

Cinci intervenții de surse, nerulate, fără teste, aplicație, browser, SQL sau schimbare de schemă; detaliile sunt absorbite din jurnalul general prin D001-06.

- `docs.js/RECEIPT_MODAL_ENABLED=false` oprește temporar popupul NIR la adăugare/modificare, păstrând implementarea ModalFrame și celelalte familii. Arborele rămâne montat cu noduri/scroll; numai conținutul său se ascunde în editare, fără schimbarea coloanelor/poziției documentului, și reapare la salvare/Renunță. Adaugă trece în antetul arborelui, inițial text cu icon la dreapta, cu dreptul/perioada existente; componenta/iconul au evoluția din [VC06-01](SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#salvare-pe-rând-și-butonul-comun).
- `receipt-context.js` și `vercon-receipt-context.css` importat din vercon.css poziționează fișele în coloana existentă, fără a dimensiona/deplasa documentul: furnizor până la Produse recepționate, magazie de la DGV pe înălțimea lui. ResizeObserver/variabile dinamice sincronizează geometria; pe mobil panouri suprapuse în locul arborelui sub document.
- Fișa urmărește selecția și celulele confirmate. Rând incomplet/fără preț valid sau gol păstrează ultima combinație validă, inclusiv ștergerea temporară a prețului pe același rând. Fără vreo combinație validă apare îndrumare; ștergerea rândului memorat îi scoate fișa. Răspunsurile după schimbarea selecției/ciornei/modului sunt ignorate.
- `receipt_context.py` nou, înregistrat în doc_routes.py: două citiri cu drept NIR/contextul serverului, fără scriere/variantă nouă. Furnizor = ultimele 12 facturi/storno și totalul setului eligibil din exercițiul curent până la data facturii, inclusiv identități cu același cod fiscal; nu este sold de plată și nu introduce plăți. Magazie = motorul existent, variantă prin produs/preț cu TVA calculat în server, ultimele 20 de mișcări și stoc la dată; MF păstrează activul distinct. Numai date salvate, fără ciornă.
- Ulterior furnizorul începe de susul coloanei și crește cu spațiul recuperat, fără mutarea fișei de magazie/documentului. Ambele afișează toate valorile/prețurile/cantitățile/soldurile/totalurile la exact două zecimale prin formatare decimală textuală, fără reducerea preciziei datelor/calculului. Notele «Ultimele … din …» explicitau tabelele limitate versus totalul întregului set și excluderea ciornei; apoi sunt eliminate complet, cu datele/totalurile neschimbate.
- Eticheta devine «Total facturi», fără «− storno» (textul vechi descria aceeași formulă). Listele ambelor fișe sunt descrescătoare după dată; soldurile rămân calculate cronologic, inversarea numai la afișare.
- În etapa finală Total facturi/Stoc devin footere în benzile albastre inferioare, delimitate/centrate vertical. Furnizorul ajunge la începutul fișei de magazie, folosind nivelul titlului pentru footer; magazia la baza coloanei, cu footer sub grilă. Înălțimi după geometria documentului, tabele cu scroll separat/totaluri fixe, fără deplasarea antetului/DGV.

Fișiere: prima etapă `PYTHON/static/js/vercon/{docs,receipt-context}.js`, `PYTHON/static/css/{vercon-receipt-context,vercon}.css`, `PYTHON/routes/vercon/{receipt_context,doc_routes}.py`; celelalte patru modificau receipt-context.js/CSS, cu receipt_context.py pentru ordinea datelor. Notele erau scoase numai din receipt-context.js.

## Factură cumulată și schimbarea secțiunii — 17.10.2026

Implementare în surse, nerulată: fără teste, browser, build, alte verificări, SQL sau schimbare de schemă. Serverul local 5052 nu a fost repornit; asocierea și reîncărcarea cer confirmare în utilizare.

- Numai NIR ascunde Ciornă/Salvat din titlu; Anulat și marcajele celorlalte familii rămân. Factura comună afișează ValoareFC globală, cu fallback la totalul recepționat dacă nu există valoare declarată; arborele citește aceeași valoare cumulată într-o interogare pe luna deschisă. VC_NIR local nu se rescrie, iar facturile istorice fără legătura existentă nu sunt reunite implicit.
- Citirea separată a celorlalte NIR-uri active ale facturii dă linii cu chei distincte, destinație, TVA, precizie/marcaj corecție. DGV le însumează exact: externe italic fără bold, corectate italic+bold inclusiv externe, rândul gol ultimul. Externele sunt readonly, fără ștergere și excluse din payloadul de validare/salvare.
- În editarea unui NIR al facturii comune, ValoareFC globală este readonly; ținta locală rămâne intern pentru corecție/salvare. Reconcilierea vizuală este globală, corecția numai locală. Previewul/raportul primesc în continuare antetul/liniile locale; SELECT-urile rapoartelor oficiale nu sunt schimbate.
- Schimbarea contextului păstrează secțiunea dacă există; documentele reîncarcă în același panou fără demontarea structurii, celelalte pagini se remontează în secțiunea aleasă. Dacă secțiunea lipsește, alege NIR când e disponibil (BC → OI/MF). La factura deschisă se redeschide NIR-ul legat al gestiunii/anului nou dacă există.
- Selecție izolată per context; cererile vechi se anulează înainte, API comun combină semnalele paginii/cererii. Confirmarea abandonului rămâne; contextul este blocat la scriere/rezultat neconfirmat. Ocuparea se eliberează/reacordă prin fluxul existent; refuzul gestiunii ocupate nu reactivează date vechi sub selecția nouă.

Fișiere: `PYTHON/routes/vercon/{receipt_invoice_view,invoices,receipt_tree}.py` (receipt_invoice_view nou); `PYTHON/static/js/vercon/{receipt-invoice-view,docs,app,api,receipt-allocation}.js` (receipt-invoice-view nou); `PYTHON/static/css/vercon-receptie.css`; jurnalul general.
