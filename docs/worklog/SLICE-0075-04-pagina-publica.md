# SLICE-0075-04 — pagina publică de înregistrare

A patra trecere din felia 0075 (`docs/PLAN_AutoProvisioning.md`). Acoperă **§4** și **§4.1**
din plan: ecranele pe care le vede solicitantul, peste rutele scrise în 0075-01 și 0075-02.
Luată înaintea lui 0075-03, la alegerea operatorului (22.09.2026).

## Ce s-a schimbat și de ce

O pagină statică servită de Flask (`GET /inregistrare`), un vrăjitor cu **șase ecrane**, un
singur modul JS. Nimic nu se ține în `localStorage`: token-ul de înregistrare și tot ce a
completat solicitantul trăiesc în memoria paginii, deci o reîncărcare reia de la codul fiscal
(de aceea și paza pe `beforeunload`).

### Componentele JS stau acum în `PYTHON/static/js/`

Componentele din `JS_COMPONENTS/` sunt module ES cu importuri **relative**
(`../../listener-tracker/…`), care se rezolvă după URL, nu după disc. Ca arborele și combo-ul
să meargă servite de Flask, folderele partajate trebuie să stea lângă ele, sub aceeași
rădăcină. Alegerea operatorului: **copie vendorizată** în `PYTHON/static/` (35 de fișiere —
`event-bus`, `listener-tracker`, `utils`, `treeview`, `combobox`, `instances-registry`, plus
CSS-ul lor). `JS_COMPONENTS/` rămâne originalul, nemodificat și **în afara commit-ului**.

Toate cele 27 de importuri relative au fost urmărite fișier cu fișier: niciunul nu cade în gol.

**Două divergențe față de original**, ambele în arbore și ambele comentate la fața locului:

- `escapeHtml` scapă și `"` și `'`. Textul ajunge și în atribute (`data-text="…"`,
  `data-path='…'`), iar captiunile vin din nomenclator: o ghilimea în captiune ar fi rupt
  atributul. Pagina e publică, deci asta nu e cosmetică.
- `getComputedTreeHeight` măsoară orice rând randat când nu găsește o frunză. La prima
  deschidere toate rădăcinile sunt strânse, nu există frunză pe ecran, iar rezerva de 24px
  tăia lista.

### Modul «bifabil» al arborelui (D21)

`treeview-checkable.js`, mixin nou, opt-in prin `checkable: true`. Starea stă **numai pe
frunze**; starea unui părinte se deduce din ele (bifat / parțial / nebifat), iar rezultatul
cerut de `/cerere` sunt chiar frunzele bifate, în ordinea arborelui. `setData` păstrează
bifele și le curăță pe cele ale codurilor care nu mai există — de asta depinde reluarea după
`CLSF_*_NECUNOSCUT`.

Numărul ales se arată în `placeholder`-ul input-ului (input-ul e `readOnly`: în componentă,
`value` e text de căutare, deci nu poate ține altceva).

### Arborele din coduri plate (§4.1)

`tree-builder.js`: `xx0000` = rădăcină, `xxyy00` = nivel 1, restul = frunze. **D14**: o frunză
al cărei părinte de nivel 1 lipsește se agață de rădăcina ei. Nodurile fără rădăcină se
aruncă; rădăcinile rămase fără copii, la fel. Captiunea e `cod · denumire`.

### Ecranul 1 — «Date Unitate» (schimbare cerută la 22.09.2026)

Codul fiscal **și** denumirea stau pe același ecran. Solicitantul nu mai vede nimic despre
baza de date: numele propus (`1nn_SSSS`) nu se mai afișează nici pe ecran, nici în rezumat.
`/nume` se cheamă totuși la «Continuă», dar numai pentru verdictul lui — e singurul loc care
ține regula celor patru litere, pe care `/cerere` n-o repetă, iar numele adevărat se
stabilește oricum la aprobare. Așa o denumire din care nu se poate forma un nume de bază e
oprită pe primul ecran, nu la trimitere.

Vrăjitorul are deci șase ecrane, nu șapte: **1** Date Unitate · **2** E-mail · **3** Surse ·
**4** Clasificații · **5** Trimitere · **6** confirmare.

### Ecranul 3 — sursa și sectorul, două combo-uri (schimbare cerută la 22.09.2026)

În locul listei de bife: **Sursa** într-un combo, apoi **Sectorul** în al doilea, filtrat pe
sursa aleasă. Perechea e chiar codul `SursaSector` din `DefaSursaSector`. Când o sursă are un
singur sector (`03`, `04`, `05`, `08`), acesta se alege singur.

`/sursasector` întoarce de acum și `sursa` și `sector` separat, nu doar codul, fiindcă
tabela le are ca **coloane** (`Sursa`, `Sectorul`); spargerea codului în două rămâne doar ca
rezervă, pentru rânduri vechi.

**Perechea se verifică la «Continuă».** Cerut explicit de operator: dacă perechea aleasă nu
are rând în `DefaSursaSector`, ecranul o refuză și nu merge mai departe. Cum ambele liste se
construiesc din aceleași rânduri, o pereche fără rând nu e de atins prin interfață — e o plasă
de siguranță pentru cazul în care combo-ul al doilea ar rămâne cu o alegere veche după
schimbarea sursei. Serverul refuză oricum aceeași situație cu `SS_NECUNOSCUT`.

### Restul

- **Token-ul** merge în antetul `X-Registration-Token`, pus de `api.js` la toate cele șapte
  rute. Orice refuz `TOKEN_*` reia de la ecranul 1 cu mesajul serverului.
- **Ceasul** de 30 de minute stă în antet, devine chihlimbar la ultimele 5 minute, iar la zero
  reia înregistrarea. Nu e o măsură de siguranță — expirarea adevărată e a serverului.
- **Refuzurile lui `/cerere`** se întorc la ecranul care le poate repara (`STEP_FOR_REASON`);
  `*_NECUNOSCUT` reîncarcă și lista despre care e vorba.
- **CSP** pe pagină: `script-src 'self'`, `style-src 'self' 'unsafe-inline'` (arborele scrie
  atribute `style` în linie), `frame-ancestors 'none'`, plus `nosniff` și `Referrer-Policy:
  same-origin`.
- Mesajele pentru solicitant sunt singurele locuri cu diacritice; codul și comentariile sunt
  în engleză, fără diacritice (Regula 0).

### A doua rundă de schimbări (operator, 22.09.2026, după încercarea pe server)

- **Denumirea (D27)**: numai litere (și cu diacritice), cifre, `_`, spații și virgule;
  spațiile repetate devin unul. Pagina **scoate** restul pe măsură ce se tastează sau se
  lipește (cursorul rămâne pe loc) și tot așa curăță numele venit de la ANAF înainte să-l
  arate — `ŞCOALA  GIMNAZIALĂ NR. 5 "ION CREANGĂ" - PLOIEŞTI` ▸ `ŞCOALA GIMNAZIALĂ NR 5 ION
  CREANGĂ PLOIEŞTI`. Serverul (`nume.py`, folosit de `/nume` și `/cerere`) doar **refuză**, cu
  `DENUMIRE_CARACTERE_INTERZISE`. În JS nu merge `\w`: acolo e numai ASCII și ar fi scos toate
  diacriticele, deci pagina folosește `\p{L}\p{N}`, iar Python-ul `\w` (care e Unicode).
- **Surse (D29)**: perechea din cele două combo-uri se pune cu «Adaugă» într-o listă, de unde
  se poate scoate; oricâte perechi. O pereche fără rând în `DefaSursaSector` e refuzată la
  «Adaugă»; o pereche aleasă și neadăugată se adaugă singură la «Continuă» (și e refuzată tot
  acolo dacă n-are rând). Aceeași pereche de două ori ▸ «este deja în listă».
- **Arborii (D28)**: trei niveluri de câte două cifre, la F și la E, dar cu reguli de bifare
  diferite. **F**: căsuța stă numai pe ultimul nivel; un nod cu copii doar se deschide.
  **E**: titlul (nivelul 1) n-are căsuță și doar se deschide; articolul cu copii (nivelul 2)
  are căsuță tri-stare care bifează toată ramura. Opțiunea nouă a arborelui: `branchChecks`
  (fals la F, adevărat la E); nivelul de sus n-are niciodată căsuță. Se trimit tot frunzele.
- **ClsfE era gol pe server.** Cauza: join-ul pe `DefaArticol` din `nomenclatoare.py` aruncă
  toate rândurile `xx0000` (`xx.00` nu e articol), iar vechiul `tree-builder.js` arunca orice
  frunză fără rădăcină ▸ nimic. Acum `/clasificatii` întoarce și `grupuri` — numele titlurilor
  din `DefaTitlu` și ale articolelor din `DefaArticol` —, iar arborele nu mai depinde de rândul
  `xx0000`. Cauza e dedusă din cod și din date, **nu văzută pe server**.

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/static/inregistrare.html` | **nou** — cele șase ecrane + confirmarea |
| `PYTHON/static/css/inregistrare.css` | **nou** — stilul paginii |
| `PYTHON/static/js/inregistrare/wizard.js` | **nou** — tot vrăjitorul |
| `PYTHON/static/js/inregistrare/api.js` | **nou** — cele șapte rute, `ApiError(status, reason, message)` |
| `PYTHON/static/js/inregistrare/tree-builder.js` | **nou** — §4.1 |
| `PYTHON/static/js/components/treeview/treeview-checkable.js` | **nou** — modul bifabil (D21) |
| `PYTHON/static/js/components/treeview/*`, `combobox/*`, `event-bus/*`, `listener-tracker/*`, `utils/*`, `instances-registry.js` | copie vendorizată din `JS_COMPONENTS/` |
| `PYTHON/static/css/treeview/treeview_checkbox.css` | **nou** — caseta tri-stare |
| `PYTHON/static/css/{variables,combobox}.css`, `css/treeview/*` | copie vendorizată |
| `PYTHON/routes/inregistrare/inregistrare.py` | `GET /inregistrare` + antetele paginii |
| `PYTHON/routes/inregistrare/nomenclatoare.py` | `/sursasector` dă și `sursa`, și `sector` |
| `PYTHON/routes/inregistrare/README.md` | răspunsul `/sursasector` adus la zi |
| `PYTHON/routes/inregistrare/nume.py` | `normalize_name`, `has_forbidden_characters` (D27) |
| `PYTHON/routes/inregistrare/cerere.py` | refuzul `DENUMIRE_CARACTERE_INTERZISE`, denumirea normalizată |
| `PYTHON/routes/inregistrare/nomenclatoare.py` | `read_group_captions` — numele nivelurilor de sus la E |
| `.claude/launch.json` | **nou** — pornește ciotul de mai jos pentru privit pagina |

## Rezultatele testelor

Verificat **în browser**, pe un ciot din biblioteca standard care servește fișierele
**adevărate** din `PYTHON/static` cu aceleași antete și mimează cele șapte rute
(în scratchpad-ul sesiunii: `inregistrare_stub.py`; `.claude/launch.json` îl pornește pe
`8765`). Nimic nu a atins Flask-ul adevărat, MariaDB sau ANAF.

- Ecranul 1: zero erori în consolă, CSP curat; cod inexistent ▸ mesaj ANAF; cod cu bază
  existentă ▸ `CF_EXISTS`; fișa ANAF și ceasul apar; **denumire din care nu ies patru litere ▸
  oprit pe loc** («Denumirea unității trebuie să conțină cel puțin patru litere»), corectată ▸
  trece la 2. Nicăieri numele bazei.
- Ecranul 2: cod greșit ▸ mesaj; cod bun ▸ trece singur la 3.
- Ecranul 3: «Continuă» fără sursă ▸ «Alegeți sursa»; cu sursă, fără sector ▸ «Alegeți
  sectorul»; lista sectoarelor se filtrează (`01` ▸ A, D, E, F, G); `03` își alege singur
  sectorul; la schimbarea sursei alegerea veche cade (02C ▸ sursa 01 ▸ sectorul gol).
- Ecranul 4: arborii se construiesc după §4.1 (6 și 9 frunze pe datele ciotului); bifarea unei
  rădăcini bifează cele 4 frunze de sub ea; tri-starea, căutarea cu evidențiere și captiunea
  lungă tăiată cu `title` — toate bune; nota «va crea 4 clasificații (funcționale ×
  economice)».
- Ecranul 5: rezumatul are 7 rânduri, **fără** rândul bazei de date; anul se alege; trimiterea
  ▸ ecranul de confirmare cu numărul cererii.
- Corpul trimis la `/cerere`, citit din jurnalul ciotului: numai frunze, o singură pereche ▸
  `{"denumire":"…","an":2026,"sursasector":["01E"],"clsf_f":["650201","650202","650301","650500"],"clsf_e":["590100"]}`.
- **O eroare găsită și reparată la ultima trecere:** ecranul de confirmare nu are casetă de
  mesaje, iar `clearMessage` o cerea oricum ▸ excepție în consolă la fiecare trimitere
  reușită, prinsă de `catch`-ul trimiterii, care lăsa în urmă un mesaj de eroare pe ecranul 5
  (invizibil, fiindcă ecranul tocmai se ascunsese). Acum cele două ajutoare trec peste un
  ecran fără casetă. Verificat după reparare: consola curată, niciun mesaj rămas pe ecranul 5.
- Expirarea (ciot: 25 de secunde pe CF `222`) ▸ reia de la ecranul 1 cu mesajul serverului.
- Pe telefon (375×812): fără derulare pe orizontală; pe ecran îngust rămân numerele pașilor,
  fără etichete (titlul cardului spune oricum unde ești).
- **A doua rundă, tot pe ciot** (E fără rânduri `xx0000`, ca pe server, plus `grupuri`):
  numele ANAF curățat; tastarea reală a `"`, `-`, `.`, `!`, `@` ▸ scoase, spațiile triple ▸
  unul, cursorul pe loc; ecranul 3: «Continuă» cu lista goală ▸ «Adăugați cel puțin o
  sursă-sector», «Adaugă» fără sursă ▸ «Alegeți sursa», 01E adăugat, 03A ales singur și
  adăugat, 01E a doua oară ▸ refuzat, 03A eliminat, 02C ales și neadăugat ▸ intră la
  «Continuă»; E ▸ 10 frunze pe trei niveluri, `10`/`20`/`59` fără căsuță, bifarea lui
  `2001` ▸ cele 4 frunze de sub el, debifarea uneia ▸ `2001` parțial, `200200` și `590100`
  frunze pe nivelul 2, clic pe numele unui titlu ▸ doar se deschide; F ▸ 6 frunze, `6503` fără nume ▸ «03»,
  `700101` fără rădăcină ▸ lipsă. Corpul trimis: `"sursasector":["01E","02C"]`,
  `"clsf_f":["650301","650500"]`, `"clsf_e":["200101","200130","200200"]`. Consola curată;
  telefonul la 375px fără derulare orizontală.
- **Fără fișiere de test** (decizia operatorului). Acoperire automată: **zero**.

## Neverificat / amânat

- **Nimic nu a rulat pe Flask-ul adevărat.** Rutele au fost exersate numai prin ciot, deci
  potrivirea paginii cu serverul e verificată pe contract, nu pe server viu.
- **Perechea sursă-sector fără rând în tabelă** nu a putut fi provocată din interfață (ambele
  liste vin din aceleași rânduri), deci refuzul cerut de operator e verificat prin citirea
  codului, nu exersat. Cele două refuzuri vecine (fără sursă / fără sector) sunt exersate.
- **Numele grupurilor de nivel 2 la F lipsesc aproape toate**: `DefaClsfF` are un singur rând
  `xxyy00` (§2a), deci subcapitolele se văd doar ca două cifre («04»). Pentru ele nu există
  nicio tabelă de nume.
- **Căutarea din arbore** caută în etichete, iar etichetele au acum doar cele două cifre ale
  nivelului — `650101` întreg nu se mai găsește, «Învățământ» da.
- **Scoaterea caracterelor lipește cuvinte**: `S.C.X` ▸ `SCX`, `DRAGU-BRĂTULEȘTI` ▸
  `DRAGUBRĂTULEȘTI`. Așa a cerut operatorul («removed»); dacă se vrea spațiu în loc, e o
  singură linie în `tidyName`.
- `debugMode` al arborelui e `true` în componentă (zgomot în consolă pe o pagină publică).
- Fișierele statice se servesc **fără amprentă de versiune**: după o actualizare, un browser
  poate rămâne pe JS vechi până i se golește cache-ul.
- «Contactați-ne» (mesajul de la `CF_EXISTS`) nu are pe pagină niciun telefon sau e-mail.
- `.claude/launch.json` e nou în arbore și pornește ciotul din scratchpad-ul **acestei**
  sesiuni; calea moare odată cu sesiunea.
