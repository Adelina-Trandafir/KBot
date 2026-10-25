# SLICE-VC06-02 — FC/NIR, inclusiv extensia MF

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata (`py_compile`, `node --check`, o verificare automata de nume nedefinite); **nimic rulat**, nicio proba, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («fa tot VC06»).

## Ce s-a facut
- **Recepție NIR** (`documents.create_nir/edit_nir`, `GET/POST/PUT /api/vercon/doc/nir…`): furnizor (Parteneri), categorie/cont, comisie, factura (numar, data, valoare), numar si data NIR, **o singura grila** de produse (combobox cu cautare), cota TVA pe linie. Tip «Stoc inițial» fara furnizor.
- **Calculul il face serverul** (CONTRACT_CALCULE 2.2): `PUTVA = rnd(Pret·(1+cota))`, `Valoare = rnd(Cant·Pret)`, `ValoareCUTVA = rnd(Cant·PUTVA)`, `TVA = ValoareCUTVA − Valoare`, rotunjit o data pe linie la `Zec`, stocat la `Scara`; refuz (nu rotunjire) la prea multe zecimale; pret < 0 si cantitate ≤ 0 refuzate.
- **Totalul antetului N01/N02:** valoarea facturii declarata de operator 0/goala/la mai mult de 1 leu de suma liniilor → suma liniilor (`numeric.header_total`).
- **MF:** aceeasi grila are nr. inventar, data intrarii, durata normala/folosita, valoare lunara (= total cu TVA / durata normala). Fiecare linie creeaza un activ nou: o varianta (`VC_ProduseP`, `COD` = nr. inventar) + un rand `VC_ProduseM`, in aceeasi tranzactie cu NIR. Cantitate intreaga.
- **Numerotare** (`protocol.next_number`): contor `VC_Numbering` sub citire cu blocare + primul numar cu adevarat liber; numar manual doar daca e liber (si ridica contorul).
- **Pagina** (`docs.js`): antet pe doua-trei coloane, grila NIR compacta, `+ Adaugă rând` in footer, totaluri, arbore **furnizor → data → factura + total** in dreapta (ascuns la adaugare/modificare), Previzualizeaza (fereastra tiparibila).
- Cheia cererii (`VC_Operations`): un raspuns pierdut e reluat, nu produce al doilea NIR.

## Fisiere
`PYTHON/routes/vercon/`: `protocol.py`, `documents.py`, `doc_routes.py` (noi); `__init__.py` (inregistrare). `PYTHON/static/js/vercon/docs.js` (nou), `app.js`; `PYTHON/static/css/vercon.css`.

## Presupuneri si neverificat
- Linia alege o **varianta existenta**; pretul de pe linie nu modifica `ProduseP.PUTVA` (valorizarea stocului ramane pretul variantei, modificabil din nomenclator, decizia Q9). Un pret nou = o varianta noua in nomenclator.
- `NumarSub` se scrie (0) doar daca coloana exista pe server (verificat la rulare prin `information_schema`); dump-ul din `MariaDB_Schema` nu o are.
- Perioada: la Materiale/Alimente data NIR trebuie sa fie in anul exercitiului.
- Nimic rulat; ValoareLuna foloseste rotunjirea HALF_EVEN a contractului (de verificat la VC08-05).

## Editorul NIR și activele MF — 18.10.2026

D001-09, istoric: **IMPLEMENTAT ÎN SURSE, NERULAT**. Numai design din surse; fără teste/build/verificări automate, browser/HTTP, MariaDB/SQL/DDL, schimbări de schemă/date, migrator, deploy/commit/push. Aspectul nu a fost probat în aplicație.

Etapele sunt cronologice: DNF inițial preluată din produs devine vizibilă/editabilă în limitele clasificării, apoi dispar coloanele Cantitate/Cotă TVA din grilă; modelul și payloadul le păstrează. Câmpurile, formula și restricțiile MF de mai jos înlocuiesc regulile inițiale diferite din 12.10. Pentru totalul N01/N02, regula inițială de un leu rămâne istorică: vezi contractul ulterior din FACTURI_NIR_IMPLEMENTARE, centralizat prin D001-05.

<a id="d001-09-etapa-7"></a>

### Intrări: antet rearanjat și consultare/editare MF

Număr NIR și Data NIR schimbă poziția cu Număr factură și Data facturii, pentru toate gestiunile. ValoareFC devine Valoare Reală Factură și este poziționată sub data facturii; protocolul și calculul valorii se păstrează.

La MF, consultarea folosește coloanele obișnuite NIR. Arborele furnizorilor ocupă 70% din coloana dreaptă, iar restul este panoul Informații Mijloc Fix, legat de rândul selectat: Număr inventar, Data intrare, Durata normală folosire, Durata folosită, Valoare lunară, cu etichete și casete readonly. La adăugare/modificare, coloana dreaptă este ascunsă complet și grila se extinde. Coloanele MF suplimentare vizibile sunt Nr. Inv., Data MF, Durata Folosită și Val. Luna. Durata normală se preia din produs și rămâne în payload/validare; valoarea lunară păstrează calculul existent. Grila MF are preferințe separate pentru consultare și editare.

<a id="d001-09-etapa-8"></a>

### Intrări: conturi MAGAZIE, produse noi în DGV și reguli MF

La adăugare în MF/OI, catalogul conturilor este limitat la gestiunea curentă și la conturile MAGAZIE (MFM/OIM, respectiv eticheta istorică MAGAZIE). Mai multe conturi ale aceleiași destinații pot fi bifate; unicul cont eligibil este autoselectat și controlul dezactivat. Serverul aplică aceleași restricții pentru produsele recepționate și pentru cererile de repartizare.

DGV recunoaște denumirea unui produs nou în editorul arbore, activează UM și continuă cu clasificarea obligatorie pentru MF, apoi cu CPV opțional pentru toate tipurile. Închiderea CPV fără alegere cere confirmarea avertismentului. Închiderea clasificării fără alegere oprește continuarea și păstrează rândul incomplet. Dacă sunt bifate mai multe conturi, după UM se cere contul produsului nou dintre cele bifate. Produsele noi au identități temporare în ciornă și sunt inserate numai în tranzacția de salvare a NIR-ului, o singură dată pentru toate copiile care folosesc același produs nou.

MF: Nr. Inv. obligatoriu, propus din maximul numerelor numerice salvate/local, cu verificare în ciornă și pe server. Data MF implicit din Data NIR, obligatorie, centrată și fără depășirea datei NIR. Cantitatea este 1, ne-editabilă. DF este obligatorie, implicit 0. DNF devine vizibilă și editabilă în limitele clasificării transformate din ani în luni. DNF, DF, V. Luna au aceeași lățime și tooltipurile cerute. Formula VBA (Pret + Pret * TVA) / DNF folosește rotunjirea half-even la 4 zecimale atât în ciornă, cât și pe server; la corecția automată se păstrează valoarea brută corectată. Afișarea valorii lunare are exact 4 zecimale, inclusiv în panoul de consultare, independent de preferințele generale.

Meniul rândului MF include Multiplică rând: copie imediat după sursă, identitate nouă de linie/activ, Nr. Inv. incrementat până la unul liber și focus pe copie. Catalogul produselor și numerele de inventar se reîncarcă după salvarea NIR.

<a id="d001-09-etapa-10"></a>

### Intrări MF: DGV fără Cantitate și Cotă TVA

În gestiunile MF, grila NIR de consultare și editare nu mai construiește coloanele Cantitate și Cotă TVA (IDTVA). Lățimea lor implicită cumulată, 90 + 100, se transferă coloanei Produs (234 -> 424), păstrând ponderile celorlalte coloane. Versiunea layoutului MF devine v3 pentru ca preferințele anterioare să nu restaureze vechea lățime a produsului. Cantitatea 1 și cota TVA rămân în modelul rândului, în validări, calcule și payloadul de salvare; selectorul Cotă TVA din antet și coloana cu valoarea TVA se păstrează.

<a id="d001-09-etapa-11"></a>

### MF: mesaj DNF cu clasificare și limite; status centrat

Mesajul pentru DNF în afara intervalului indică acum codul clasificării și limitele în luni: Pentru clasificarea cccc durata (in luni) de folosire minima este nn si maxima mm. Același format este folosit în interfață și pe server. Lipsa clasificării sau a limitelor valide are un mesaj distinct, fără limite inventate. Textul vc-save-status este centrat; în containerul mesajelor documentului, statusul ocupă lățimea disponibilă pentru centrarea efectivă.

<a id="d001-09-etapa-12"></a>

### Clasificări MF: selecție numai pentru elemente cu DNF

Selectorul comun de clasificări MF oferă plusul de selecție numai pentru înregistrările cu limite DNF valide. Regula se aplică indiferent de nivel (root, nod intermediar, frunză), atât în arbore, cât și în rezultatele căutării. Elementele fără DNF rămân vizibile și pot fi expandate. Handlerul selecției refuză aceleași elemente. Catalogul CPV păstrează comportamentul existent.

<a id="d001-09-etapa-13"></a>

### DGV: ștergerea erorii după validarea/anularea rândului

Mesajul de eroare din vc-save-status este eliminat când rândul devine valid, la părăsirea cu succes a rândului, la anularea unui rând nou ori restabilirea unui rând existent și după scoaterea cu succes a rândului/corecției. Mesajul nu se șterge când operația eșuează sau când se salvează doar o celulă iar rândul rămâne invalid. Mesajele informative sunt păstrate.

<a id="d001-09-etapa-14"></a>

### Adăugare intrări MF: padding antet DGV de 2px

Celulele antetului DGV au padding stânga/dreapta de 2px numai în modul de adăugare a unui NIR MF. Starea is-adding delimitează regula de consultare și modificare; celulele produselor păstrează paddingul existent.

<a id="d001-09-etapa-15"></a>

### Modificare NIR MF: numai active fără ieșire; alinieri fișă MF

La modificarea unui NIR MF, DGV-ul afișează numai activele care NU au și ieșire neanulată din contul de intrare. Identificarea este pe IdProdus (activ/variantă), SubunitId/GestiuneId și contul produsului; caută în VC_Iesiri cu antet VC_Iesiri_H neanulat, neretur și cantitate pozitivă, fără limitare la anul curent. O ieșire anulată nu ascunde activul. Consultarea NIR-ului păstrează toate liniile. Filtrarea se aplică și rândurilor MF din contextul facturii comune afișate în grilă.

Liniile cu ieșire rămân în ciornă și în totalul/reconcilierea facturii, dar sunt excluse din grila de modificare și totalurile ei vizibile. Serverul recitește starea, refuză modificarea activelor cu ieșire și păstrează liniile originale chiar dacă lipsesc din cererea clientului. Nu sunt șterse sau rescrise la salvarea celorlalte active. Modificarea cotei TVA din antet și corecția totalului folosesc doar liniile editabile.

Panoul Informații Mijloc Fix: Nr. inventar și Data intrare centrate; DNF/DF la dreapta cu unitatea Luni, valoarea lunară la dreapta, în continuare cu exact 4 zecimale, conform imaginii utilizatorului.

### Fișiere pe etapă

Căi istorice; numele fără prefix continuă directorul precedent.

| Etapă | Fișiere |
|---|---|
| 7 | PYTHON/static/js/vercon/docs.js, receipt-mf-info.js; PYTHON/static/css/vercon-receptie.css, vercon-receipt-destinations.css. |
| 8 | PYTHON/routes/vercon/receipt_products.py, receipt_destinations.py, receipts.py, receipt_rules.py; PYTHON/static/js/vercon/receipt-product-entry.js, receipt-allocation.js, docs.js, cpv-picker.js, receipt-mf-info.js; PYTHON/static/js/dgv/editing.js, datagrid.js; PYTHON/static/css/vercon-receptie.css, vercon-receipt-destinations.css. |
| 10 | PYTHON/static/js/vercon/docs.js. |
| 11 | PYTHON/static/js/vercon/receipt-product-entry.js; PYTHON/routes/vercon/receipt_products.py, receipts.py; PYTHON/static/css/vercon-controls.css. |
| 12 | PYTHON/static/js/vercon/cpv-picker.js. |
| 13 | PYTHON/static/js/vercon/docs.js. |
| 14 | PYTHON/static/js/vercon/docs.js; PYTHON/static/css/vercon-receptie.css. |
| 15 | PYTHON/routes/vercon/receipt_mf_edit.py, documents.py, receipts.py, receipt_rules.py, receipt_invoice_view.py; PYTHON/static/js/vercon/docs.js, receipt-invoice-view.js, receipt-mf-info.js. |

## Antet compact NIR/BC — 23.10.2026 <a id="d001-12-antet-nir"></a>

Două revizii preluate D001-12, ambele **Confirmat 23.10**.

- Dacă grila NIR/BC nu poate afișa minimum 10 rânduri, header-fit aplică is-labels-inline: etichete lângă control, antet cel mult cât grila, celule micșorabile/etichetă cu elipsă, cele trei rânduri rezervate fără linie separată de etichetă. Se numără pe antetul înalt, în același cadru cu comutarea, evitând pâlpâirea la prag. minRows=10 în docs.js; acel fișier avea istoric 1028 linii, față de 1026 înainte, peste ținta ~1000. Ajutorul VERCON nu a fost actualizat.
- În modul compact NIR, lățimea cedată de coloana îngustă TVA merge la prima coloană Furnizor/Cont/Comisie. Furnizor și Cont au aceeași lățime de etichetă/control. Categorie / cont devine Cont prin două texte în același span comutate CSS; modul normal păstrează numele lung.
- TVA a avut întâi eticheta deasupra și mutarea pe primul rând/coloana 4. **Corecția finală:** eticheta pe rândul 1, lista pe rândul 2 al grilei antetului prin display: contents, aliniată cu restul; regulile NIR compacte se aplică numai peste 800px. Antetul BC ulterior este în [jurnalul BC](SLICE-VC06-03-bc-si-controlul-disponibilului.md#d001-12-bc).

Fișiere istorice (numele fără prefix continuă directorul precedent):

| Etapă generală | Fișiere |
|---|---|
| 26 | PYTHON/static/js/vercon/header-fit.js (nou), css/vercon-header-inline.css (nou), css/vercon.css (import), js/vercon/docs.js (import + o linie după crearea rădăcinii). Prag 10 rânduri în `minRows` (docs.js). |
| 27 | css/vercon-header-inline.css, js/vercon/receipt-allocation.js. |

## Etapele editorului NIR — 13/14.10.2026 <a id="d001-13-editor-nir"></a>

Preluare D001-13. Sunt variante istorice: marcajul roz inițial din prima etapă este înlocuit ulterior cu validarea la părăsire/salvare; editorul modal și TVA blocată sunt reperele din 14.10, înaintea contractelor ulterioare ale editorului în pagină.

- **13.10, prima etapă:** pagina/headerul albastru ocupă fereastra; numai workspace-ul are max 1440px/lățime și 1080px/înălțime, cu bleu în jur. Antet centrat max 1076px = 4×260 + 3×12, etichete/câmpuri stânga. Comboboxurile/opțiunile VERCON au +2pt, inclusiv regula desktop portal.
- Produsele active vin direct din VC_Produse, fără duplicate pe preț/variantă și fără preț în etichetă. Linia NIR transmite product; serverul verifică produs/cont și rezolvă după prețul cu TVA calculat o variantă activă potrivită ori inserează una în tranzacție, fără suprascrierea altor prețuri. Vechiul variant rămâne acceptat; ieșirile/sursele păstrează selecțiile lor. MF folosește aceeași listă directă și scrierea registrului existentă.
- Furnizori SQL: numai vizibili, cod fiscal nevid, grupare pe cod fără spații exterioare și numele primului IdPartener (FIRST determinist). Salvarea obișnuită respinge și furnizorul fără cod trimis în afara listei. Stocul inițial nu cere furnizor/număr factură.
- Prima variantă marca obligatorii goale roz de la deschiderea ciornei, fără mesaje sub câmp/grilă, dar cu erori explicite pentru valori incorecte. NIR permitea editare incompletă, click/Tab/Enter între celule și păstrarea draftului, cu validare strictă la salvare/preview; opțiune explicită numai NIR. **Revizia ulterioară 13.10** elimină marcajul inițial și folosește contractul [TextBox/DataGrid](../../VERCON/CONTROALE_CUSTOM_THREAD.md#textbox).
- **Arbore 13.10:** numai furnizorii la deschidere, apoi luni și recepții la expandare, cu substituent Se încarcă…; familia storno/bonurile păstrau lista veche. Inițial etichetă dată/valoare fără număr/factură și furnizorul cu ultima recepție primul; apoi recepția devine DD.MM.YYYY • nr. factură, valoare separată dreapta fără lei, exact două zecimale/mii. Ordonarea furnizorilor este revizuită alfabetic pe 17.10.
- receipt_tree.py: parties/months/documents, party identificat prin numele afișat, gol = fără furnizor/stoc inițial; luna filtrată pe interval, fără funcție pe coloană. GET /api/vercon/doc/nir/tree?level=parties|months|documents&party=&month=YYYY-MM păstrează contextul/dreptul receptie și can_write/closed/zec/scale. /list rămâne pentru celelalte familii. receipt-tree.js ține cache pe niveluri, ID-uri stabile nume/lună, deduplică cereri și reîncarcă/deschide documentul după salvare/anulare. Eșecul închide nodul, anunță în status și permite reîncercare la redeschidere.
- Selecția nu mai distruge arborele: markSelected înlocuiește paint/paintSaved; treeDirty semnalează date noi după load/save/anulare, refreshTree păstrează ramuri/selecție/scroll și arborele ascuns rămâne viu. Corecția setData/expandedNodes și exclusiveRoots sunt descrise o singură dată în [TreeView](../../VERCON/CONTROALE_CUSTOM_THREAD.md#treeview); după salvare se deschide documentul și se închid celelalte rădăcini.
- Bifa Stoc inițial este pe linia etichetei Furnizor, la dreapta cu combo-ul; se elimină position:static sub 1200px, text cu elipsă. Numărul NIR este verificat la părăsire/salvare, fără debounce la tastare. Subnumăr în aceeași celulă, max 2 zecimale/sutimi, unic (Numar,NumarSub), fără mutarea contorului; _numarsub_column consultă information_schema o singură dată/cerere, în ctx. create/edit, /number?sub= și preview folosesc 707,1 = 707 + 10. Rămâneau nerezolvate afișarea subnumărului în arbore/rapoarte și ordinea listelor pe (Numar,NumarSub); receipts.sql îl citea, neverificat.
- **14.10:** Stoc inițial ascunde/golește Furnizor/Număr factură, le readuce la debifare și păstrează numai bifa în celula Furnizor. Celulele #/UM sunt centrate, Preț fără TVA/TVA %/Valoare fără TVA/TVA lei/Total cu TVA la dreapta. Contul unic Alimente este preselectat/disabled, inclusiv Cont de intrare/primitor; este regula de atunci, înaintea destinațiilor DC din 16.10.
- headerLock cere toate obligatorii înainte de grid, inclusiv Comisie dacă există, numai pe client (serverul nu o cerea în acea etapă). editGuard/onEditBlocked marchează lipsurile și explică la click sau Adaugă rând. Acțiunea footer era stânga, text la ~10px, fără centrarea moștenită de la #; Comisie și Cotă TVA folosesc simple. Rândul gol permanent/Esc și validarea la părăsirea rândului rămân în contractul DataGrid; golurile nu se salvează.
- Tastatura antetului tuturor documentelor: Tab/Shift+Tab/Enter între editabile, sărind bife, butoane/săgeți/calendare, disabled/invizibile; după ultimul, Enter/Tab începe primul rând. Combobox cu scriere păstrează primul rezultat la Tab. Lista simple nu se putea deschide încă de la tastatură la acel reper; corecția ulterioară din 14.10 este în contractul Combobox. TVA antet era blocată de primul rând complet, cu tooltip, reactivată după scoaterea rândurilor; golul nu contează.
- Adăugare/modificare erau ModalFrame fix/blur, arbore în spate, modalSize numai localhost. Starea și dimensiunile ulterioare sunt în contractul componentei. Numele Factură Cumpărare (NIR) venea de pe server: popup Adăugare (mount startAdd fără consultare intermediară ori startAdd pe pagina existentă), Editare/Modificare, separator/Listare încă goală; eticheta nouă cerea restart Flask. [Meniul Intrări ulterior](SLICE-VC02-02-meniuri-capabilitati-coloane.md#d001-13-meniu) înlocuiește adăugarea automată.

### Probe și fișiere ale etapelor 13/14.10

- Prima variantă a recepției: IMPLEMENTAT_LOCAL, NERULAT; fără teste, aplicație/SQL sau procese pornite/oprite/repornite; fără schemă, restart Python manual necesar. Fișiere: PYTHON/static/css/{vercon-workspace,vercon-receptie,vercon-controls}.css; js/vercon/{docs,ui}.js; js/dgv/{datagrid,editing}.js; PYTHON/routes/vercon/{documents,receipt_choices}.py.
- Etapa controalelor/validării: numai sintaxă JS, fără browser; riscurile și fișierele sunt în [integrarea controalelor](../../VERCON/CONTROALE_CUSTOM_THREAD.md#d001-13-integrare).
- Etapa arborelui: sintaxă JS (5 fișiere) și py_compile trecute. formatMoney zecimal pe șir, half-up, probat pe 14 valori în afara aplicației, fără schimbarea altor afișări de sume. Bifa a fost văzută înaintea modificării arborelui; arborele nou/correția expandedNodes și stilul opțiunilor multiline nu au fost probate în browser sau pe 000_DEMO. Fără scrieri/schemă. receipt_tree.py, doc_routes.py; receipt-tree.js, docs.js, ui.js; componente TreeView/Combobox și CSS-urile lor sunt cele din contractul comun.
- Istoric server: 5053 avea cod Python mai vechi decât receipt_choices.py și cauza Serverul nu a trimis lista produselor; repornit atunci cu Stop-Localhost.ps1/Start-Localhost.ps1. Sub Windows PowerShell 5.1, Start-Localhost cerea Add-Type -AssemblyName System.Net.Http înainte; scriptul nu a fost modificat. Arborele nou necesita alt restart după schimbarea Python. Niciun proces nu este manipulat prin această consolidare.
- Etapa 14.10: numai node --check, fără browser. Fișiere relative la PYTHON/static: js/vercon/{docs,ui,app}.js, js/dgv/{datagrid,editing}.js, js/components/combobox/{combobox,combobox-ui}.js, css/{dgv,combobox,vercon-receptie,vercon-documente-base}.css; plus PYTHON/routes/vercon/capabilities.py. Componentele Menu/debug și inventarul tipografic au sursele unice legate în jurnalul general.
