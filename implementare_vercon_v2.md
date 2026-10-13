# Implementare VERCON v2

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
