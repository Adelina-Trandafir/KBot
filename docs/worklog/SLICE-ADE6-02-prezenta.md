# SLICE-ADE6 — Prezență, context și încărcare lazy

02/04 introduc editarea prezenței și workspace-ul. 08/09 adaptează bara și mesajele pe mobil; 21 amână încărcările până la selecție, 22 deschide numai ultimul an, iar 26 colapsează anul selectat anterior la alegerea unei luni din alt an. Deschiderea manuală a unui an nu schimbă selecția. Probele 9 API, 18 API și 3 JS și limitele browser/server sunt păstrate separat.

Nota comună Rezolvat din 04/21/26: *Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.* Descrierile inițiale de predare sunt istorice; regulile operative sunt în [ghid](../../ADECHIT/UTILIZARE_WEB.md), inclusiv excepția ulterioară de editare a ultimei luni închise prin dreptul ME01.

- [ADE6-02](#slice-ADE6-02)
- [ADE6-04](#slice-ADE6-04)
- [ADE6-08](#slice-ADE6-08)
- [ADE6-09](#slice-ADE6-09)
- [ADE6-21](#slice-ADE6-21)
- [ADE6-22](#slice-ADE6-22)
- [ADE6-26](#slice-ADE6-26)

## SLICE-ADE6-02 — prezență <a id="slice-ADE6-02"></a>

08.10.2026 — TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Se editează numai zilele, se recalculează valorile și lunile închise refuză modificarea. Proba browser și testul API au trecut.

## SLICE-ADE6-04 — workspace-ul de prezență <a id="slice-ADE6-04"></a>

### Ce s-a schimbat și de ce

Situația lunară afișează copilul, SID/SIC, prezența, valorile și mișcările lunii. Numai Prezența este editabilă; după salvare se recitește calculul complet. A fost adăugată operația tranzacțională pentru preluarea copiilor activi încă neincluși în luna și grupa selectate.

### Fișiere atinse

- `PYTHON/routes/adechit/__init__.py`
- `PYTHON/routes/adechit/service.py`
- `PYTHON/static/js/adechit/app.js`
- `PYTHON/tests/test_adechit.py`

### Verificări

- suită ADE: 9 teste trecute, inclusiv metadatele editării și preluarea repetată refuzată.
- salvarea, validarea și recalcularea folosesc în continuare endpointurile comune existente.

### Neverificat sau amânat


Operația nu este probată pe MariaDB. Transferul rămâne blocat de M06.

## SLICE-ADE6-08 — bara de selecție numai pe mobil <a id="slice-ADE6-08"></a>

09.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Schimbare

Pe PC, ade-toolbar este ascunsă complet, iar grila centrală nu mai rezervă rândul de 40px. Tabelul începe de sus, la nivelul panoului LUNA / ANUL din aceeași zonă. Pe mobil (max-width 980px), rândul barei există pentru cele două comboboxuri. Textul lunii/grupei este ascuns în ambele moduri; identificatorii rămân în DOM pentru actualizările existente din app.js. Nu există text vizibil suplimentar.

### Fișiere și verificare

PYTHON/static/css/adechit.css, PYTHON/static/adechit.html, indexul și statusul ADE. Ajutorul paginii descrie afișarea pe PC/mobil cu referință ADE6-08. Nu am rulat teste sau verificări vizuale, conform instrucțiunilor utilizatorului; autorizarea precedentă pentru testare a fost limitată la pornirea aplicației. Nu s-a modificat backendul sau schema. Fișierele statice se preiau după reîncărcare.

### Predare

Modificări locale, fără commit/publicare. Utilizatorul publică prin AvacontPush. Nu există SQL nou. Următoarea subfelie: ADE6-09.

## SLICE-ADE6-09 — mobil fără rândul de mesaje <a id="slice-ADE6-09"></a>

09.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

La cererea utilizatorului, ade-notice-row este ascuns pe mobil (max-width 980px). Grila ade-main nu mai rezervă cei 24px ai mesajului. Primul rând al zonei de lucru este bara comboboxurilor. Afișarea pe PC rămâne cea existentă.

Fișiere: PYTHON/static/css/adechit.css, ajutorul din adechit.html și statusul ADE. Nu am rulat teste sau verificări vizuale. Modificare locală, fără commit/publicare, fără backend/SQL. Reîncărcarea paginii preia CSS-ul. Publicarea aparține utilizatorului, prin AvacontPush. Următoarea subfelie liberă: ADE6-10.

## SLICE-ADE6-21 — Pornire fără selecții și încărcare lazy <a id="slice-ADE6-21"></a>

09.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). 18 teste API și 3 teste JS trecute. Fără probă vizuală, server sau MariaDB.

### Ce s-a schimbat și de ce

Pagina nu mai selectează ultima lună sau toate grupele la pornire și nu cere situația copiilor până la alegerea lunii și grupei. Contextul conține numai metadate/drepturi. Endpointul years citește anii DISTINCT, fără lunile lor; arborele pornește restrâns, iar deschiderea anului cere numai lunile acelui an. Pe mobil selecția folosește Anul, Luna și Grupa. Grupele se încarcă după alegerea lunii. Opțiunea Toate este eliminată din arbore și combobox.

Situația primește IDG și descarcă numai copiii grupei. Citirea SQL păstrează istoricul copiilor selectați din toate grupele/lunile lor pentru soldurile inițiale. Formulele calculate sunt nemodificate. Lunile închise citesc snapshotul filtrat după IDL și IDG. Endpointul situației păstrează citirea nefiltrată pentru compatibilitate, dar pagina trimite întotdeauna grupa.

Plătitori încarcă inițial numai grupele. Se cer copiii după IDG și persoanele după IDP, cu resetarea listelor dependente și ignorarea răspunsurilor vechi. Editorul grupei citește separat anii și numai istoricul educatorilor săi. Filtrele rows sunt validate și aplicate SQL, nu după descărcarea întregii tabele.

### Fișiere

- API: PYTHON/routes/adechit/__init__.py, repository.py, catalog_forms.py.
- UI: PYTHON/static/js/adechit/app.js, payers.js, static/adechit.html și css/adechit.css.
- Probe: PYTHON/tests/test_adechit.py, adechit_lazy.test.mjs.
- Documentație: ADECHIT/UTILIZARE_WEB.md, ADECHIT_STATUS.md și registrul ADE0–ADE9.

### Verificări efective

18 teste pytest API trecute, inclusiv lipsa citirilor copiilor la context/catalog, filtre SQL, situație pe grupă egală cu situația completă, sold din grupa anterioară și snapshot închis. O fixture veche de taxe a primit Expl și DeLa cerute deja de ADE6-20; implementarea taxelor nu s-a schimbat.

3 teste JS prin node:test/VM, cu DOM și componente simulate, trecute: pornire goală, ordinea cererilor lazy, navigare Plătitori PC/mobil și răspunsuri întârziate. Aceste probe nu reprezintă verificarea vizuală în browser. Sintaxa modulelor JS a fost verificată. Prima încercare pytest a fost blocată de directorul temporar al sandboxului; rerularea a folosit un director dedicat în artifacts.

Testele au fost rulate înainte de citirea preferinței proiectului de a lăsa testarea utilizatorului. Nu s-au mai pornit probe după identificarea regulii.

### Neverificat și predare


Publicarea prin AvacontPush și restartul backendului aparțin utilizatorului. Se publică cele 7 fișiere runtime enumerate mai sus. Nu există DDL nou, commit, push sau restart din chat. Verificarea vizuală PC/mobil și MariaDB rămâne utilizatorului. Următoarea subfelie liberă: ADE6-22.

## SLICE-ADE6-22 — Ordonare după denumire și ultimul an deschis <a id="slice-ADE6-22"></a>

09.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Ce s-a schimbat și de ce

Ordonarea inițială a tuturor listelor cu nume/denumire este alfabetică în română: grupele din arbore și combobox, copiii din Prezență și Plătitori, persoanele asociate, educatorii și taxele după Explicație. Grilele folosesc sortBy din DataGrid comun, păstrând ordonarea și după editare sau adăugare. Documentele se afișează după Explicație, cu rândul nou la sfârșit. Comboboxul lunilor este ordonat după denumire; arborele LUNA / ANUL rămâne cronologic descrescător.

La încărcarea paginii se cer numai lunile celui mai recent an și se deschide ramura lui. Ceilalți ani rămân închiși și se încarcă lazy. Deschiderea anului nu selectează luna sau grupa și nu descarcă date despre copii. Restul fluxului lazy din ADE6-21 se păstrează.

### Fișiere atinse

- PYTHON/static/js/adechit/app.js, payers.js, catalogs.js.
- PYTHON/static/adechit.html: ajutor actualizat.
- PYTHON/tests/adechit_lazy.test.mjs: adaptarea simulării DataGrid și a așteptărilor existente la ultimul an deschis. Testele: TESTAT, RULAT, PUBLICAT.
- ADECHIT/UTILIZARE_WEB.md, ADECHIT_STATUS.md și registrul ADE0–ADE9.

### Verificări și predare

Citire/revizie a codului și contractului sortBy; fără execuție de teste, server sau browser. Probele ADE6-21 sunt istorice și nu validează această subfelie. Utilizatorul publică cele trei module JS și adechit.html prin AvacontPush și verifică pe PC/mobil ordonarea și anul deschis.

Fără schimbare API, DDL, commit, push sau restart executat din chat. Următoarea subfelie liberă: ADE6-23.

## SLICE-ADE6-26 — Colapsarea anului selectat anterior <a id="slice-ADE6-26"></a>

12.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Ce s-a schimbat și de ce

La alegerea unei luni din alt an în arborele LUNA / ANUL, ramura anului selectat anterior se colapsează, iar ramura anului noii luni rămâne extinsă. Prima alegere folosește drept an anterior anul deschis automat la pornire. Schimbarea între luni ale aceluiași an păstrează ramurile existente. Extinderea manuală a unui an pentru consultare nu colapsează anul selectat.

Logica este în chooseMonth, înaintea sincronizării selecției și încărcării grupelor. Starea ramurilor este păstrată de mecanismul existent la salvare și reîncărcare; nu se schimbă componenta TreeView comună.

### Fișiere atinse

- PYTHON/static/js/adechit/app.js.
- ADECHIT/UTILIZARE_WEB.md.
- ADECHIT_STATUS.md și docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md.
- Acest worklog.

### Rezultatele testelor

Nu s-au rulat teste, verificări de sintaxă, server sau browser. Starea este TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Neverificat și predare


Utilizatorul publică PYTHON/static/js/adechit/app.js prin AvacontPush și probează prima alegere din alt an, trecerea între ani și păstrarea ramurii noii luni după salvare/reîncărcare. Fără SQL, publicare, commit sau push executate din chat. Următoarea subfelie liberă: ADE6-27.
