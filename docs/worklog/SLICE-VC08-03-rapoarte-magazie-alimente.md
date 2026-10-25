# VC08-03 — rapoarte Magazie–Alimente

13.10.2026. **IMPLEMENTAT_LOCAL, NERULAT.** Scop restrâns de utilizator: Materiale/Alimente acum, rapoarte OI/MF amânate. Excluderile Coduri rămân cele trei din catalog; nu au fost investigate.

## Familii și capturi

Cele 16 capturi din `VERCON/CAPTURI_RAPOARTE/Magazie-Alimente` au fost citite vizual. Machetele locale preiau orientarea, ordinea coloanelor, grupările, casetele și semnăturile de mai jos. Similaritatea finală nu este validată: nu au fost generate PDF-uri sau deschise pagini în browser.

| Familie / raport | Referință din capturi | Implementare locală |
|---|---|---|
| Balanță produse | `Magazie!Produse!balanta_produse.png` | Landscape, 11 coloane SI/intrări/ieșiri/SF, benzi pe cont, totaluri valorice |
| Fișă magazie și furnizor; FM1-u/FM2 | `Magazie!Produse!fisa_magazie.png` | Date produs/cont/UM/preț, sold inițial distinct, 7 coloane, total lunar și general |
| Inventar | `Magazie!Produse!lista_inventar.png` | Landscape, 14 coloane, antete pe trei niveluri, câmpuri faptice/diferențe necompletate, semnături |
| Stoc final | `Magazie!Produse!stoc_final_produse.png` | 5 coloane și totaluri pe cont; cantități zero omise |
| NIR / NIR_SFP / Storno | `Magazie!Intrari!NIR.png` | Antet cu șase câmpuri, furnizor/adresă/certificare/comisie, tabel recepționat și semnături. TVA disponibil ca binding, ascuns implicit ca în captură |
| Bon consum / retur / BUConsum | `Magazie!Iesiri!Bon_Consum.png` | Antet document, cantitate necesară/eliberată, cod/UM/preț/valoare, patru semnături; BUConsum_F are două exemplare |
| Centralizator BC | `Magazie!Iesiri!Centralizator_Bon_Consum.png` | Număr/data/valoare/primitor, grupare pe cont și total general |
| Consumuri produse / unități / Fișa evidenței | `Magazie!Iesiri!Consumuri_Produs.png` | Grupare cont sau primitor, produs/UM/variantă/preț. Nu se totalizează cantități cu UM diferite |
| Furnizori / conturi-furnizori / nou_Furnizori | `Magazie!Intrari!Centralizator_Intrari.png`, `Magazie!Produse!centralizator_conturi_furnizori.png` | Factură/data/NIR/cont/valoare, subtotaluri pe furnizor/cont și total general |
| Centralizator conturi | `Magazie!Produse!centralizator_conturi.png` | Cont și patru valori SI/intrări/ieșiri/SF |
| Comandă / referate / plan / situație intrări | `Magazie!Intrari!nota_comanda.png`, `Magazie!Produse!referat_achizitie.png`, `Magazie!Produse!plan_anual_achizitii.png`, `Magazie!Produse!situatie_intrari.png` | Coloane pe familie, CPV la plan, paragrafe și spațiu pentru referat, semnături. Comanda/Plan/Situație citesc intrări, referatele includ corecția storno |
| Lista zilnică și patru subreports | `Lista_Alimente.png` | Trei rezumate alăturate, aprobări, trei mese, alimente în două coloane, recapitulare economie/depășire, total și semnături; secțiune suplimentară pentru toate BC asociate |

## Surse SQL și configurare per unitate

Modulele VERCON ale acestei implementări sunt `static/js/vercon/reports.js`, `static/css/vercon-rapoarte.css` și `routes/vercon/{report_routes,report_data,report_query,report_layouts,report_body,report_food,report_validation,report_store,report_service}.py`. Administrarea este legată în MainForm prin `KBotAdmin/Views/VerconReportsView.vb` și Designer, `VerconReportDesignerForm.vb` și Designer, respectiv `PYTHON/routes/admin_cli/vercon_reports.py`.

Sursele SELECT implicite sunt cele 11 fișiere din `PYTHON/reports/vercon/queries`: metadata, ledger, receipts, consumption, committee, food_header, food_items, food_bons, food_consumption, food_history, settings. `report_data.py` descrie aliasurile obligatorii; se pot adăuga câmpuri. Sursele comune afectează toate rapoartele care le folosesc. Nu există SQL de raport în JavaScript.

În **KbotAdmin → Rapoarte VERCON**, se alege o unitate din arbore, Citește lista, definiția, Deschide. Interogările sunt editabile ca SELECT; antetul/subsolul și identitatea au câmpuri dedicate. Salvarea SQL face validare de structură și **nu execută interogarea**. La generare, serverul înlocuiește `{{TABLE:Nume}}` cu relații delimitate la contextul autorizat. Nu sunt permise nume brute de tabele, mutații, alte baze sau parametri de context introduși de client.

Datele locale sunt în `PYTHON/instance/vercon-reports`, sau calea `VERCON_REPORT_CONFIG_DIR`. Configurarea se păstrează per bază/unitate și este exclusă de la push. Salvarea atomică verifică versiunea citită și păstrează versiunea anterioară în `history`. Snapshoturile de raport sunt comune workerilor, legate de utilizator/unitate/subunitate/gestiune/exercițiu, valabile 10 minute. Ecranul, PDF și Excel folosesc același model salvat; lățimile Excel sunt recitite la export.

## Editor vizual

`VerconReportDesignerForm.vb` și `.Designer.vb`: selectare/tragere texte, redimensionare lățime cu marginea dreaptă, adăugare/eliminare/reordonare coloane, binding/etichetă/tip/font/text îngroșat, titlu și orientare. Benzile sunt antet/subsol și texte înainte/după corp; pentru unitate doar antet/subsol. Lista de alimente expune șase tabele separate în selectorul Secțiune. Aplică în ciornă nu salvează pe server; butonul Salvează din pagina de administrare salvează definiția. Nu se editează manual JSON.

Previzualizarea editorului este schematică, fără date reale și fără paginare identică PDF. Adăugarea/eliminarea/reordonarea coloanelor șterge antetele combinate din tabelul modificat, pentru a nu păstra spanuri greșite. Editorul nu implementează expresii Access, designer de grupări/totaluri, imagini, subrapoarte arbitrare sau mutarea fiecărui element fix al corpului. Titlul și coloanele/textele se pot edita acum; pentru paritate completă Access rămâne followup-ul vizual cerut. Îngroșarea PDF folosește accentuare sintetică a fontului Unicode livrat; Excel folosește bold nativ.

## Excel comun tuturor aplicațiilor

`PYTHON/reporting/excel_widths.py` conține cheia coloanei, citirea din `AVACONT_COMUN.RAPOARTE_EXCEL_COLOANE`, adaptorul cursor și politica de lățimi. ADECHIT și VERCON îl folosesc. Algoritmul inițial ADECHIT este păstrat, inclusiv prioritate/maxim/fallback. Importurile istorice ADECHIT sunt reexportate; VERCON nu depinde de pachetul ADECHIT.

Contract VERCON: `NUME_RAPORT = VERCON_` + ID majuscul, caracterele speciale devin `_` (`FM1-u` → `VERCON_FM1_U`). Cheile sunt câmpuri majuscule, duplicările au `_2`, `_3` etc. (`QUANTITY`, `QUANTITY_2`). Configurația Excel nu este lățimea relativă din macheta PDF. Exemple SQL sunt pregătite în `sql/VC08_03_rapoarte_excel_coloane.sql`, fără execuție și fără DDL.

Fișierele extracției Excel comune: `PYTHON/reporting/__init__.py`, `PYTHON/reporting/excel_widths.py`, `PYTHON/routes/adechit/report_excel.py`, `PYTHON/routes/vercon/report_excel.py` și `PYTHON/routes/vercon/report_output.py`. Regula comună este consemnată și în `PYTHON/AGENTS.md`.

## Limite și continuare

Nicio verificare executată: fără teste, build .NET, import Python, browser, generare PDF/Excel sau acces MariaDB. Nu există rezultate de paritate pe date reale. Dependențele de server sunt în `PYTHON/requirements-reporting.txt`. Nu s-au creat/modificat scheme. Stocul este evaluat la prețul curent al variantei; liniile NIR/BC păstrează valoarea istorică, BR/Storno derivă explicit valoarea din cantitate și preț. Fișa pe furnizor selectează variantele acelui furnizor, păstrează toate mișcările lor și nu inventează alocare FIFO.

Rămân acceptarea vizuală și corective followup după rularea autorizată de utilizator. VC08-04 (închidere/redeschidere) și VC08-05 (comparator real Access/import/web) nu sunt implementate prin acest slice; VC08 în ansamblu nu este declarat încheiat.

## Istoric rapoarte din 17.10.2026 — absorbit prin D001-04

Contractul rezultat este în [Resursele rapoartelor](../../PYTHON/reports/vercon/README.md); etapele de mai jos păstrează diagnosticul, variantele înlocuite, fișierele și limitele intervențiilor. Toate cele șapte etape au fost consemnate IMPLEMENTAT/CORECTAT ÎN SURSE, fără teste, builduri, browser, PDF de probă, SQL, modificări de schemă ori publicare pe server în acele intervenții. Nu reprezintă validare vizuală cu exemplul. Serverul local 5052 nu a fost oprit/repornit în etapa resurselor.

### Resurse omise și aliasuri

- Traceback-urile din 16/17 octombrie indicau FileNotFoundError pentru /root/AVACONT/reports/vercon/queries/metadata.sql, existent în surse. Citirea inițială VR op=read nu diagnostica raportul. KBotAdmin excludea implicit SQL și nu includea TTF. Au fost incluse explicit cele 11 SELECT-uri, DejaVuSans și licența în PYTHON/.pushignore și tools/Push-Changed.ps1; KBotAdmin/Server/PushService.vb și AvacontPush/Form1.vb respectă includerile peste filtrul extensiilor, iar KBotAdmin/Views/PushFilesView.vb le numără. Filtrul global și protecția configurărilor rămân.
- report_store.py și report_output.py întorc 503/REPORT_RESOURCE_MISSING; docs.js include codul cererii, api.js aruncă AbortError după redirectul de autentificare pentru a evita mesajul suplimentar al paginii care se închide. Wrapperul existent transmite tokenul. Traceback-urile nu stabileau cauza SESIUNE_EXPIRATA; autentificarea nu a fost ocolită. Era necesară publicarea codului și resurselor.
- După publicarea completă, logul operatorului de la 08:24:32 indica MariaDB 1064 la source.rows('committee'): markerul VC_ComisieNIR devenea subquery fără alias. Omisiunea exista și la food_history/settings. PYTHON/reports/vercon/queries/{committee.sql,food_history.sql,settings.sql} au primit aliasurile c/h/s și coloane calificate, cu aceleași câmpuri și filtre. Se republicau numai cele trei SELECT-uri, fără restart deoarece sunt citite la cerere; configurările personalizate cereau corectare explicită în editor. schema_diff*.sql rămâne analiză de schemă, exclusă din publicarea aplicației.

### Fereastră PDF și macheta NIR

- PYTHON/static/css/vercon-preview.css a extins dialogul pe întreg viewportul, fără margini/colțuri rotunjite, cu bara sus și vizualizatorul în restul spațiului. Selectorul aplicației prevalează peste confirm-box.css încărcat ulterior (420px), iar butoanele peste min-width-ul confirmărilor; se împart pe rânduri numai când este necesar. Livrare numai CSS, fără restart.
- Prima variantă după referința utilizatorului era A4 orizontal, 11pt și titlu 24pt; a fost înlocuită în etapa următoare cu A4 portret, 8pt și 16pt. Structura comună, datele reale, fonturile și publicarea sunt descrise în README. Nu se copiază numărul 3 de pagini din captură. Primul antet avea titlu centrat/unitate dedesubt, tabel factură–recepție–comandă cu antet reunit, furnizor/adresă și recepție în același chenar, însoțitoare într-o bandă și opt coloane fără TVA separat; totalurile numai în două celule din dreapta.
- report_reception.py folosește rânduri contextualizate, înălțimi măsurate, antete repetate și totalul numai la sfârșitul documentului. Textul lung se împachetează; linia/antetul imposibil de încadrat produce REPORT_LAYOUT, fără tăiere tacită. Totalurile folosesc report_model.total și precizia gestiunii, fără recalcularea valorilor salvate. report_layouts.py definește nir-classic; report_validation.py acceptă formularul; report_service.py creează snapshotul; report_output.py selectează rendererul. Celelalte rapoarte și machetele personalizate fără form rămân generice.
- Cele trei DejaVuSansMono regular/Bold/Oblique provin din Matplotlib v3.10.7, cu licența DejaVu; .pushignore și Push-Changed le includ explicit. Livrarea inițială cerea Python, fonturi, restart și regenerare. Skillul PDF fusese citit, dar regula utilizatorului interzicea randarea/verificarea; contractul Table/minRowHeights a fost doar citit din dependența locală.
- Corecția portret folosește A4 la pagina PDF și la dimensiunile utile, cu proporții și certificare/subsol adaptate. static/js/portal/pdfview.js: createPdfView acceptă initialScale, implicit page-width pentru portal; static/js/vercon/pdf-preview.js trimite page-actual (100%), păstrând zoom și potrivirea lățimii. Livrare report_reception.py/report_layouts.py și cele două JS, restart pentru Python și regenerare.

### Macheta comună FCS, apoi semnul de afișare

- report_layouts.default și report_store._path au unificat NIR/FCS în layout-nir.json, cu aceeași versionare/istoric; layout-nir_storno.json vechi rămâne neconsultat. report_service și report_reception folosesc același nir-classic portret, cu autorizarea/selecția NIR_Storno distincte. Etapa inițială nu aplica abs/negare: documents.py/_clean_stornolines și create_storno/edit_storno salvau cantități pozitive; calculul existent cantitate/preț când nu exista total stocat rămânea. Acea limită a fost comunicată și ulterior înlocuită numai pentru afișare. Livrare report_layouts.py, report_store.py, report_service.py, report_reception.py, restart și regenerare.
- La corecția cerută ulterior, report_data.py/storno_display aplică Decimal copy_abs/copy_negate numai copiei sursei explicite FCS: quantity/value/vat/header_value negative, preț neschimbat, NULL păstrat și zero fără minus, fără rotunjire. Rendererul comun, rezultatul și Excel primesc aceleași linii. document-report.js normalizează Cant/Valoare/ValoareCUTVA/TVA în copia previzualizării de rezervă, inclusiv fără drept distinct de raport. Formularul, salvarea, centralizatoarele și stocul păstrează convenția existentă, fără a doua negare. Livrare report_data.py/document-report.js, restart și regenerare.

## Lista de alimente — machetă și probe demo, 17.10.2026

Patru etape de machetă și serverul demonstrativ sunt preluate din jurnalul general prin D001-06. Corecțiile formularului și soldurilor sunt în [VC07-01](SLICE-VC07-01-lista-alimente.md#evoluție-ui-și-solduri--17102026). Stările de nerulare ale etapelor anterioare din acest jurnal rămân istorice; numai probele explicite de mai jos au fost consemnate.

### Contractul machetei și succesiunea corecțiilor

- Secțiunea tipărită BC asociate este scoasă, dar citirea bonurilor și refuzul unui bon anulat rămân. Intervalul nu mai apare sub ListaAlimente2015, filtrarea perioadei rămâne. Numărul/data listei sunt centrate în PDF, pagină și Excel. Recapitularea drepturi/cheltuieli/porții are font cu 1 pt sub coloana/fontul raportului, minimum 5 pt; Alocație fără zecimale, Efectiv inclusiv totalul/Economii/Depășiri cu două. Precizia calculelor/surselor rămâne; regulile se aplică și machetelor salvate la generare, fără rescrierea lor.
- PDF.js invalidează cache-ul fontului după redimensionarea canvasului de măsurare: redimensionarea reseta fontul la 10 px, iar cache-ul vechi împiedica fontul cerut la primul text de aceeași dimensiune cu proba de ascendență. Se corectează limitele selecției textului, nu dimensiunea/centrarea/desenarea titlului.
- Spațiul de 2 mm trece de sub banda ALIMENTE deasupra ei, după mese; banda rămâne cu tabelul următor la paginare. Se scoate rândul Feluri de mâncare, păstrând mesele/explicațiile salvate. Antetul alimentelor are font −1 pt, conținutul −2 pt față de configurația coloanei/fontul raportului, minimum 5 pt; cantități/prețuri/valori la două zecimale.
- TOTAL ALIMENTE este footerul tabelului comun celor două jumătăți: etichetă stânga, sumă dreapta, fundal de antet; se calculează din valorile originale și se formatează numai la afișare. Nu se separă de ultimul rând la paginare. Întocmit/Confirmat/Gestionar/Bucătar sunt grup indivizibil la baza cadrului ultimei pagini, deasupra subsolului, prin TopPadder; aprobările superioare își păstrează poziția.
- Modelul comun separă header_size/vizibilitate antet. Excel preia fonturile, ascunderea rândului felurilor și footerul cu sumă dreapta, lățimile prin modulul comun; HTML ascunde antetul felurilor și folosește footerul nou.
- Dreapta antetului devine grila pentru pix: Mese servite / Mic dejun / Dejun / Cină și Efectiv la hrană / Porții preparate / Rămase / Distribuite. Toate cele 12 valori sunt șiruri goale, fără preluare de porții/distribuit, fără zero sau marcaj de lipsă. Prima variantă are antet alb/etichete accentuate, și în machetele vechi, fără rescriere de configurație/documente.
- Aprobat și Vizat compartiment financiar contabil au fiecare 25% din lățimea utilă (jumătate dinainte), la extremități, cu 50% alb între ele, fără chenar/separator al spațiului; înălțimea semnării rămâne 14 mm. Prima variantă folosește delimitări verticale de 1,5 pt (2 px CSS) și unește numai cele trei celule ale ultimului rând gol din recapitularea centrală, păstrând celelalte linii/margini. Excel preia grila goală, antetul alb și unirea centrală; HTML casetele de aprobare separate.
- Corecția finală înlocuiește separatoarele de 2 px cu linii uniforme de 0,75 pt (1 px CSS) și antetul alb al grilei pentru pix cu gri #ececec, păstrând valorile goale. Ultimul rând din prima secțiune devine footer gri: primele trei celule reunite TOTAL, valoarea în ultima la dreapta, fără cele două linii verticale interne. Unirea centrală rămâne. La unirea tabelelor paralele rendererul PDF păstrează culoarea/accentul celulelor; numai prima secțiune are footer colorat.

### Mediul demonstrativ și probele efective

La cererea explicită pentru localhost și alegerea datelor fictive, `tools/VerconPreview` folosește formularul real și report_service/report_food/PDF/Excel cu DemoSource injectat; report_service.build acceptă sursa opțională, producția păstrând Source SQL implicit. Nu importă main.py, nu înregistrează blueprint SQL și nu conectează MariaDB. Datele/salvările/snapshoturile sunt numai în memorie, fără schimbare de schemă/configurare de raport. Ascultă exclusiv 127.0.0.1:5053, cu Host/Origin validate la scriere; procesul care ocupa 5052 nu a fost atins. Launcher PowerShell în prim-plan/fundal, oprire cu identificarea procesului, pagină de intrare și endpointuri numai pentru Lista de alimente/raport. Celelalte module nu sunt incluse; salvarea fictivă nu validează stocuri/drepturi/tranzacții/concurență MariaDB.

| Moment | Probă consemnată | Limite |
|---|---|---|
| Pornirea previewului / prima corecție antet | Health, formular, solduri, PDF deschis; export Excel HTTP 200. textLayer: ~844 → 281 px pe pagina de 792 px, scaleX 3,06172 → 1,02057. | Date fictive, fără suită, MariaDB sau publicare reală. |
| Tabel compact / semnături | Designul PDF al listei demo 130 examinat în browser, inclusiv baza paginii, pe 5053. | Fără probe multipagină sau nou export Excel; fără teste automate, MariaDB/publicare reală. |
| Grilă pentru pix / delimitări | PDF demo 130 examinat în browser, preview local 5053 actualizat. | Noul Excel nerulat; fără teste/verificări automate, MariaDB/publicare reală. |
| Linii uniforme / footer prima secțiune | Serverul 5053 repornit, PDF demo 130 regenerat și design examinat în browser. | Fără teste/verificări automate, MariaDB/publicare reală. |

### Fișiere pe etape

| Etapă | Fișiere (relative la grupul indicat) |
|---|---|
| Antet/recapitulare și textLayer | PYTHON/routes/vercon/: report_food.py, report_service.py, report_model.py, report_body.py, report_output.py, report_validation.py; PYTHON/static/js/vercon/report-result.js; PYTHON/static/css/vercon-rapoarte.css; PYTHON/static/vendor/pdfjs/: pdf.min.js și README.md |
| Tabel compact/semnături | PYTHON/routes/vercon/: report_food.py, report_body.py, report_output.py, report_validation.py; PYTHON/static/js/vercon/report-result.js; PYTHON/static/css/vercon-rapoarte.css |
| Grilă pentru pix | PYTHON/routes/vercon/: report_food.py, report_layouts.py, report_body.py, report_output.py; PYTHON/static/js/vercon/report-result.js; PYTHON/static/css/vercon-rapoarte.css |
| Linii uniforme | PYTHON/routes/vercon/: report_food.py, report_output.py |
| Preview nou | tools/VerconPreview/: preview_server.py, demo_data.py, index.html, preview.css, Start-VerconPreview.ps1, Stop-VerconPreview.ps1, README.md, .gitignore; PYTHON/routes/vercon/report_service.py |

Toate etapele actualizau și `implementare_vercon_v2.md`; această absorbție nu repetă probele.

## Revizii comune ale rapoartelor — 17–19.10.2026 <a id="d001-11-rapoarte"></a>

Preluare D001-11 din 20 de însemnări ale jurnalului general. Numerele din paranteze identifică rândurile tabelului de fișiere. Sunt etape istorice: catalogul MF/OI și perioadele sunt revizuite din nou la [20/23 octombrie](SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md#rapoarte-mf-și-perioade--20102026). Amânarea OI/MF de la începutul acestui jurnal descrie 13.10.

### Antet și export (1, 8)

- Antetul previzualizării reutilizează butonul/iconița VERCON și meniul aplicațiilor; păstrează confirmarea ciornei și eliberarea ocupării. Butonul copiat nu duplică ID-ul vc-apps, iar meniul se deschide în dialog.
- Titlul este urmat de data documentului pentru NIR/BC/retur/FCS, celelalte documente și liste, inclusiv ciorne. În Rapoarte: un document = etichetă și dată românească; mai multe = număr și perioadă; centralizate = perioadă. Stocuri/MF/amortizare transmit perioada sau data lor. Antetul se împachetează pe ecrane înguste; macheta PDF nu este schimbată de acest antet (17.10).
- În revizia 19.10, acțiunile viewerului comun devin emoticoane cu tooltipuri și etichete accesibile. Export Raport, meniul comun, înlocuiește descărcările separate PDF/Excel. Excel folosește snapshotul/contextul PDF-ului afișat și este dezactivat unde formatul nu există.

### Perioada și încărcarea selecției, 18.10 (2–4)

- ComboTree permite tastare/căutare an/lună; numai ultimul an cu documente este expandat. Materiale/Alimente păstrează exercițiul din antet; OI/MF citesc istoric NIR/FCS (date document/factură), ieșiri/retururi și BP/BT/BS, inclusiv transfer/casare. Listele contribuiau la limitele Alimente. Sunt expuși numai anii cu documente, cu fallback la anul selectat. Aceste limite/luni sunt înlocuite de revizia din 20.10, nu sunt reguli curente universale.
- Catalogul transmite min/max reale, dar alegerea anului/lunii completează DI/DSF cu întregul interval calendaristic, independent de mișcări; toate lunile anilor disponibili sunt selectabile. Calendarele extind limitele la începutul primului/sfârșitul ultimului an, acceptă introducere manuală cu aceleași limite, dezactivează zile/navigare în afara intervalului și au grilă de ani în popup, cu paginare. Confirmarea datei neschimbate nu emite schimbări/reîncărcări.
- Inițial arborele tipurilor avea rădăcini deschise și nivelul 1 închis; rapoartele MF erau încă indisponibile în acea etapă, fără activare nouă prin calculul perioadei. Activarea MF ulterioară din 18.10 este consemnată în [context/meniu](SLICE-VC02-01-alegerea-contextului.md#d001-09-etapa-2). Coloana tipurilor scade 250–340 → 150–204 px (60%), eliberând spațiu selecției; responsive rămâne.
- Selectare date primește spațiul rămas, pornește cu rădăcini închise și numai definițiile filtrelor. Produsele se citesc cont → produs → variantă, cu SQL parametrizat pe ramură și context subunitate/gestiune/exercițiu; sursele SELECT configurabile rămân. Furnizorii, primitorii/deținătorii, conturile de folosință și documentele se citesc la cererea ramurii.
- Bifarea grupului încarcă descendenții; căutarea de minimum 3 caractere încarcă și ramurile necitite. Cache numai pe selecția curentă; perioada revalidează ramurile bifate și închide rădăcinile. Schimbarea raportului/perioadei ori închiderea paginii anulează cererile vechi. Exportul așteaptă încărcarea; o ramură eșuată se reîncearcă la redeschidere. API-ul complet anterior rămâne fără branch.
- Corecția erorii server 1054: DATALA se citește din antetul VC_LABC, conform food_history.sql, în loc de VC_ListaAlimente.

### Selecție directă și Selectează TOATE, începutul reviziilor 19.10 (0)

- Dispar ramura Furnizori și rădăcina Produse: conturile sunt primul nivel, bifabile integral; produse/variante se citesc la expandare/bifare. Inițial se citesc numai metadatele conturilor, fără recursivitate. Celelalte familii de filtre rămân. Reviziile ulterioare specifice Intrări reintroduc furnizorul în structurile de mai jos.
- Antetele/opțiunile sunt centrate orizontal/vertical. Selectează TOATE stă la stânga, pe același rând cu titlul centrat în întregul antet, aliniat cu bifele conturilor; ultima corecție de poziție este numai CSS, nerulată.
- Bifa încarcă recursiv inclusiv ramurile închise înainte de selectare; debifarea golește selecția. Arată starea parțială și este disabled în timpul încărcării; căutarea nu limitează selectarea tuturor datelor. Selecția integrală se transmite prin absența filtrelor opționale, păstrând selecțiile obligatorii. După schimbarea perioadei, variantele se reconciliază împreună pentru toate conturile, fără ștergerea selecțiilor valide din alte conturi.
- Fișa Magazie BF păstrează produs/variantă obligatorie, dar furnizorul devine opțional după eliminarea lui din UI: lipsa filtrului include toți furnizorii, API-ul păstrează filtrarea. Ascunderea ulterioară a BF din catalog este o etapă diferită.

### Amortizare și balanțe, 19.10 (5–6)

- Amortizare dispare din Fluxuri/Mișcări și catalogul formularelor; raportul MF existent este direct sub Mijloace fixe, fără subgrup omonim. Se afișează numai La data, transmisă la ambele capete ale intervalului; la alt raport reapar controalele și se păstrează începutul ales. Exporturile și restricția MF rămân.
- Balanțele magazie/OI/MF sortează produsele alfabetic românește în fiecare grup, fără diferență de majuscule. Două opțiuni inițial debifate: Grupează pe cont, cu pagină nouă aduce contul la exterior și pune pauza după totalul contului anterior, fără pauză după ultimul; macheta standard grupează pe cont și fără bifa aceasta, fără pagini impuse. Machetele salvate nu se rescriu.
- Implicit rămân rândurile cu cel puțin una dintre SI/intrări/ieșiri/SF cantitativ nenulă, comparată exact înainte de afișare, inclusiv negative și valori sub precizia vizibilă. Afiseaza FARA miscari păstrează toate rândurile calculului anterior; istoricul pentru SI nu se filtrează suplimentar. Opțiunile invalidează rezultatul. Pagina/PDF/Excel folosesc același model filtrat/ordonat; pauzele PDF devin întreruperi de rând la tipărirea Excel.

### Mesaje și totaluri comune (7, 9–10, 17)

- Sunt eliminate DOM-ul și spațiul rezervat notelor/stării de lângă Listare/Excel, inclusiv nota de evaluare la preț curent. Calculul nu se schimbă prin eliminarea notei. Erorile catalog/selecție/ramuri/generare/export folosesc messageBox comun, închis la părăsirea paginii; motivele indisponibilității rămân în tooltip, încărcarea în aria-busy. Nota ME01-03 aflată aici este transferată în [jurnalul de drepturi](SLICE-ME01-03-privilegii-mariadb.md).
- TOTAL GENERAL devine ultimul rând al tabelului ultimei grupe, cu aceleași coloane/valori, fără secțiune separată sau antet repetat. Modelul comun păstrează regula în PDF/Excel și în HTML ADECHIT cu grupe separate. PDF ține fiecare total cu rândul precedent.
- Revizia de aspect: fundal #f7f8f2 ca banda categoriei, bold, 2 mm deasupra, coloane păstrate, stil comun PDF/Excel și echivalent HTML ADECHIT. VERCON omite totalul general pentru o singură grupă exterioară; fișele lunare pentru cel mult o lună. ADECHIT omite totalul suplimentar la o grupă; fără separare, totalul unic poartă numele grupei.
- Subtotalul grupei cu o singură subgrupă nu se repetă. La mai multe subgrupe, subtotalul intră în ultimul tabel de detalii, fără tabel/antet/subsol suplimentar; același model în pagină/PDF/Excel.

### Reutilizarea și schimbarea arborelui, etape 19.10 (11–14, 16, 18–19)

- Prima regulă: raportul din aceeași categorie păstrează controlul, bifele, selecția, ramurile, căutarea și derularea. Altă categorie resetează bifele/reîncarcă; perioada păstrează reîncărcarea existentă. Catalogul transmite specificațiile filtrelor pentru validarea/exportul raportului curent. Inițial rădăcinile erau deschise explicit, categoriile pliate.
- Afișează Documente a înlocuit produsele cu furnizori pliați, cu documente lazy la expandare/bifare. Produsul ascuns nu filtrează; un document sau tot furnizorul sunt selectabile. Debifarea restabilește produse fără bife și elimină filtrul documentelor. PDF/Excel nu-și schimbă macheta prin această opțiune.
- Corecția finală a comutării distruge imediat controlul vechi și cererile lui lazy, inclusiv cu perioadă invalidă, apoi montează un arbore exclusiv modului ales: toate bifele/selecțiile, căutarea, rândul curent, derularea și ramurile se resetează, fără memorare între moduri. Root folosește selection_mode=documents numai în acel mod și primește doar furnizori lazy, fără produse/documente preîncărcate; setul ramurilor deschise este gol. Același raport/categorie/mod poate păstra controlul.
- Expandarea furnizorului citește numai documentele lui; bifarea le încarcă/selectează prin mecanismul comun. Metadatele citite se cumulează pentru identificarea în preview. Răspunsurile ramurilor ascunse nu ating arborele activ.
- Intermediar, Intrări păstra controlul numai când categoria și structura coincideau, trecerea la/de la NIR reîncărcând. Ultima revizie elimină această condiție selectionKind: numai categoria de nivel 1 sub rădăcină decide păstrarea între rapoarte, chiar BONURI/CENTRALIZATOR sau NIR/alte Intrări. Rămân controlul, bifele, rândul, căutarea, derularea, ramurile și încărcările lazy în curs; se invalidează numai rezultatul generat. Schimbarea categoriei/perioadei și comutările explicite păstrează resetul lor. Filtrele bonurilor păstrate sunt acceptate de celelalte rapoarte de consum din categorie, fără ignorarea selecției la generare.
- Ultima lățime a Tipului raportului: 150–204 → 165–224,4 px (+10%); diferența se ia din perioadă, 190–240 → 175–219,6 px, fără schimbarea selecției/distanțelor.

### Catalogul și identitățile selecțiilor, 19.10 (12, 16–18)

- Fișa Magazie este prima în Magazie/Produse; FM1-u/FM2/Fișa Magazie BF dispar din arbore; Referat periodic produs devine Referat Periodic. Inițial numai acesta, Plan Achiziții și Situație Intrări aveau Afișează Documente, debifat, fără rădăcină Documente separată; trecerea între ele păstra selecțiile pe categorie. Intrările adaugă ulterior Nota Comandă și Referat Periodic la acest mecanism.
- Exclusiv Magazie/Intrări: implicit furnizor → cont → produs, iar NIR furnizor → document → produs, fără cont; rădăcini pliate/lazy și bifare recursivă. NIR include recepții/stornări descrescător după data documentului, inclusiv la generare, permite mai multe documente și păstrează stornările negative. Dispar NIR SFP, NIR Storno și duplicatul Centralizator Conturi Furnizori; rămâne nou Furnizori. Referatul din Intrări devine Referat Periodic. Selecțiile păstrează furnizor/cont sau document în identificatori validați și în filtrarea modelului, fără încrucișarea produselor unor furnizori diferiți. Produse păstrează comportamentul său anterior.
- Centralizator Furnizori devine Situație Furnizori; nou Furnizori devine Centralizator Furnizori. Numele se aplică în catalog/titluri, inclusiv machetelor configurate, fără schimbarea identificatorilor interni.
- Toate etichetele arborelui rapoartelor devin majuscule, inclusiv rădăcini/categorii OI/MF. Deschiderea închide surorile, inclusiv MAGAZIE/OI/MF și Shift+click; inițial numai prima rădăcină este deschisă.
- BON CONSUM devine BONURI, incluzând consumuri și retururi: implicit cont → bon → produs; Afișează Documente = bon → produs. Bonurile sunt descrescătoare după dată în grupă. Dispar BUConsum/BUConsum F/BUConsum F1/BonRetur din catalogul vizual. Centralizator BC devine CENTRALIZATOR: implicit cont → bon, Afișează Primitori = primitor → bon, aceeași ordine descrescătoare; primitorii sunt rădăcini fără bonuri preîncărcate. Ambele opțiuni sunt debifate/resetante, rădăcini pliate/lazy, bifare/căutare comune. Identitățile cont/primitor–bon–produs se păstrează până la generare. BONURI identifică separat consumul/returul; CENTRALIZATOR păstrează sursa/calculul bonurilor de consum. Numele noi ajung și în titlurile machetelor configurate.

### Valoarea estimată (15)

Numai textul VALOARE ESTIMATĂ din referatele Produse/Intrări se rotunjește în sus la 10 lei, Decimal/ROUND_CEILING, fără zecimale: 3.277,8900 → 3.280 LEI; multiplii exacți rămân. Calculele/totalurile tabelului nu se schimbă. Același text în pagină/PDF/Excel.

### Probe și fișiere istorice

Etapele 0, 2, 4–7: IMPLEMENTAT ÎN SURSE, NERULAT, cu limitele explicite fără teste/probe automate, browser/HTTP sau SQL/MariaDB din descrierile lor și fără schimbări de schemă/date. Etapele 1 și 3: IMPLEMENTAT ÎN SURSE, fără teste sau verificări runtime; 1 preciza și lipsa modificărilor în baza de date. Analiza de design nu devine probă de funcționare. Etapele 6 și 7 cereau publicarea Python/JS, respectiv JS/CSS; corecția de aliniere din 0 era numai CSS, nerulată. Etapele 8–19: **Confirmat 23.10**, exact domeniul confirmat în jurnalul general; confirmarea nu se extinde la 0–7. Această consolidare nu execută probe sau publicare.

Tabelul păstrează și mențiunea jurnalului general când era enumerată. Căile fără prefix continuă directorul precedent; numerele sunt cele din capitolele de mai sus.

| Etapă | Fișiere |
|---|---|
| 0 | PYTHON/routes/vercon/report_selection.py, report_catalog.py; PYTHON/static/js/vercon/report-selection.js, reports.js; PYTHON/static/css/vercon-rapoarte.css. |
| 1 | PYTHON/static/js/vercon/application-menu.js, report-header.js, app.js, ui.js, pdf-preview.js, document-report.js, docs.js, lista.js, reports.js, stock.js, depreciation.js, mf.js, editor.js; PYTHON/static/css/vercon-preview.css; implementare_vercon_v2.md. |
| 2 | PYTHON/routes/vercon/report_period.py, report_routes.py; PYTHON/static/js/vercon/report-period.js, reports.js; PYTHON/static/js/components/datepicker/datepicker.js; PYTHON/static/css/datepicker.css, vercon-rapoarte.css; implementare_vercon_v2.md. |
| 3 | PYTHON/static/css/vercon-rapoarte.css; implementare_vercon_v2.md. |
| 4 | PYTHON/routes/vercon/report_selection.py, report_routes.py, report_data.py, report_query.py, report_period.py; PYTHON/static/js/vercon/report-selection.js, reports.js; implementare_vercon_v2.md. |
| 5 | PYTHON/routes/vercon/capabilities.py; PYTHON/static/js/vercon/app.js, reports.js, report-result.js. |
| 6 | PYTHON/routes/vercon/report_balance.py, report_routes.py, report_service.py, report_body.py, report_output.py; PYTHON/static/js/vercon/reports.js; implementare_vercon_v2.md. |
| 7 | PYTHON/static/js/vercon/reports.js, report-selection.js; PYTHON/static/css/vercon-rapoarte.css; implementare_vercon_v2.md. |
| 8 | PYTHON/static/js/reporting/pdf-preview.js; PYTHON/static/js/vercon/reports.js; PYTHON/static/css/report-preview.css; implementare_vercon_v2.md. |
| 9 | PYTHON/routes/vercon/report_body.py; PYTHON/routes/adechit/reports.py; PYTHON/reporting/output.py; implementare_vercon_v2.md. |
| 10 | PYTHON/routes/vercon/report_body.py; PYTHON/routes/adechit/reports.py, report_model.py, report_render.py; PYTHON/reporting/output.py; PYTHON/static/css/adechit-report.css; implementare_vercon_v2.md. |
| 11 | PYTHON/static/js/vercon/reports.js, report-selection.js, ui.js; PYTHON/routes/vercon/report_catalog.py; implementare_vercon_v2.md. |
| 12 | PYTHON/static/js/vercon/report-result.js, report-selection.js, reports.js; PYTHON/routes/vercon/report_catalog.py, report_selection.py; implementare_vercon_v2.md. |
| 13 | PYTHON/static/js/vercon/reports.js, report-selection.js; PYTHON/routes/vercon/report_selection.py, report_routes.py; implementare_vercon_v2.md. |
| 14 | PYTHON/static/js/vercon/report-selection.js, reports.js; PYTHON/routes/vercon/report_selection.py; implementare_vercon_v2.md. |
| 15 | PYTHON/routes/vercon/report_body.py; implementare_vercon_v2.md. |
| 16 | PYTHON/routes/vercon/report_incoming_selection.py (nou), report_selection.py, report_catalog.py, report_routes.py, report_data.py, report_service.py; PYTHON/static/js/vercon/report-result.js, report-selection.js, reports.js; implementare_vercon_v2.md. |
| 17 | PYTHON/routes/vercon/report_body.py, report_catalog.py, report_layouts.py, report_service.py; implementare_vercon_v2.md. |
| 18 | PYTHON/routes/vercon/report_outgoing_selection.py (nou), report_selection.py, report_routes.py, report_catalog.py, report_data.py, report_body.py, report_layouts.py, report_service.py; PYTHON/static/js/vercon/report-result.js, report-selection.js, reports.js, ui.js; implementare_vercon_v2.md. |
| 19 | PYTHON/static/css/vercon-rapoarte.css; PYTHON/static/js/vercon/reports.js, report-selection.js; PYTHON/routes/vercon/report_catalog.py, report_service.py; implementare_vercon_v2.md. |
