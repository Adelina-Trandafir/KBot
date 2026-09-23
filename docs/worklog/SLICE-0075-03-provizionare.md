# SLICE-0075-03 — jobul de provizionare (aprobarea unei cereri)

A cincea trecere din felia 0075 (`docs/PLAN_AutoProvisioning.md`). Acoperă **§5.6** și **§6**:
din rândul `FX_Inregistrari` lăsat de `/cerere` iese o unitate care merge — baza, registrul,
clasificațiile, contul MariaDB și linkul de parolă. Cerută de operator pe 23.09.2026 («lets do
step 3»), după ce a întrebat cum se aprobă o cerere: **până acum nu se putea deloc**.

## Ce s-a schimbat și de ce

### Jobul — `routes/inregistrare/provizionare.py`

Trei intrări: `plan()` (toate verificările și toate valorile calculate, **nu scrie nimic**),
`approve()` (jobul întreg) și `link_nou()` (alt link de parolă pentru o cerere aprobată).

| # | Pas | Anulare înregistrată după reușită |
|---|---|---|
| — | Verificări: stare `InAsteptare`/`Esuata`; e-mail cu forma unui nume de cont; e-mail liber în `mysql.user` și `Unitati_Utilizatori`; CF liber în `CAI`/`Unitati` (D11); denumire cu ≥ 4 litere; șablon fără coloanele `*_w` și fără declanșatori/proceduri/evenimente; toate rândurile trec de nomenclatoare; ≤ `MAX_ROWS`; `CodProgram` valid | — |
| 1 | `1nn_SSSS` calculat din nou (`nume.py`), refuzat dacă baza există | — |
| 2 | `CREATE DATABASE` cu setul de caractere al șablonului; fiecare tabel din `SHOW CREATE TABLE` (fără `AUTO_INCREMENT=N`, cu `FOREIGN_KEY_CHECKS=0` pe sesiune); view-urile în runde, fără `DEFINER`; numărătoarea tabelelor/view-urilor comparată cu șablonul | `DROP DATABASE` |
| 3–4a | Sub `GET_LOCK('cai_idunitate')`, într-o tranzacție: `CAI` (un rând pe sursă-sector, `IdUnitate` = MAX+1…), `AVACONT_COMUN.Unitati`, `Unitati_Ani` | trei `DELETE` |
| 5–6 | În baza nouă, într-o tranzacție: `Unitati` (același `IdUnitate`), apoi `Clasificatii` în loturi de 1 000, numărate înainte de commit | pleacă cu baza |
| 7 | `CREATE USER '<email>'@'%'` cu parolă aleatoare; `GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE` **numai** pe baza nouă (D18); `Unitati_Utilizatori` cu `Rol = 'Contabil'` (D24) | `DROP USER`, `DELETE` |
| 8 | Hash-ul linkului, `Stare = 'Aprobata'`, `DbName`, `DataDecizie`, `Decis` — commit; apoi e-mailul | — |

Orice eșec: anulările rulează **de la ultima la prima**; una care pică **nu** oprește restul și
**nu** e înghițită — ajunge, cuvânt cu cuvânt, în `Motiv` («NU s-a putut anula: …»), în jurnal
și în excepția întoarsă, fiindcă e exact ce are operatorul de curățat de mână. Cererea devine
`Esuata` și se poate aproba din nou după reparare.

Alegeri care nu stau scrise în plan:

- **E-mailul de la pasul 8 nu desface nimic.** Unitatea e gata și corectă; un server de e-mail
  căzut nu e motiv s-o dărâmi. Răspunsul spune `link_trimis: false`, iar linia de comandă
  tipărește linkul ca să poată fi dat de mână.
- **O singură aprobare odată pe tot serverul** — `GET_LOCK('kbot_provizionare')` pe o conexiune
  ținută toată rularea. Merge și între linia de comandă și pagina din 0075-05, și se eliberează
  singur dacă procesul moare. Fără stare nouă (`InLucru`) care ar putea rămâne agățată.
- **După ce ia lacătul `cai_idunitate`, conexiunea începe o tranzacție nouă** înainte de
  `MAX(IdUnitate)`: un instantaneu mai vechi ar fi citit un maxim pe care altă rulare îl
  depășise deja.
- **Refuzul verificărilor marchează și el `Esuata`** (după ce cererea s-a încărcat), ca pagina
  din 0075-05 să arate motivul. Excepție: o cerere care nu era aprobabilă (deja `Aprobata`,
  `Respinsa`) nu e atinsă.
- **Numai numele de forma `1nn_SSSS` se pot șterge** — ultima pază din `_drop_database`.
- **E-mailul devine nume de cont**, deci forma lui e restrânsă la `[A-Za-z0-9._%+-]@…`. Pagina
  acceptă mai mult (de ex. un apostrof); o astfel de cerere e refuzată la aprobare, cu motiv.
- **Declanșatorii, procedurile și evenimentele nu se copiază.** Șablonul n-are niciunul (dump-ul
  din 22.09.2026); dacă apare unul, jobul refuză în loc să facă o bază căreia îi lipsește ceva.
- **View-urile pierd `DEFINER`** (crearea unui view pe numele altui cont cere `SUPER`) și devin
  ale contului de provizionare, cu `SQL SECURITY DEFINER` neschimbat. Șablonul n-are azi niciun
  view, deci deocamdată nu contează.

### Rândurile — `routes/inregistrare/randuri.py`

§6 și D5 ca funcție pură: `Capitol = Left(F,2).Left(SS,2)` (`02E` ▸ `.10`), `Subcapitol`,
`Articol`, `Alineat`, `Denumire` (tăiată la 255). `Sector`/`Sursa`/`SS` din regula unică
`clasificatii_ss.ss_values`, cu litera sursei dată explicit; SS-ul derivat e comparat cu cel
ales. Înainte de orice rând, verificarea față de `DefaSursaSector`, `DefaClsfF`, `DefaClsfE`
(existență + denumire), `DefaArticol`, `DefaTitlu` — **toate** codurile rele numite deodată.

### Linkul de parolă (D13) — `parola.py`, `/parola`, două rute

Token-ul (256 de biți) stă în link **după `#`**, deci browserul nu-l trimite niciodată într-o
adresă: nu ajunge în jurnalele nginx și nici în `Referer`. Pagina îl citește o dată, îl șterge
din bara de adrese și îl trimite numai în corpul a două POST-uri. În baza de date stă **numai
SHA-256-ul lui**, pe rândul cererii (`ParolaHash`, `ParolaExpira`, două coloane noi) — deci
linkul supraviețuiește unei reporniri, iar pagina din 0075-05 poate vedea dacă a fost folosit.

Parola se pune cu `ALTER USER` prin contul de provizionare (nimeni nu are parola veche, deci
`SET PASSWORD` ca la 0072 nu merge), apoi hash-ul se golește: linkul merge o singură dată.
Ordinea e aleasă: parola întâi, hash-ul după — invers ar putea cheltui linkul și lăsa contul
fără parolă. Minim 8 caractere (ca la 0072), maxim 128, fără spațiu la capete. Limitatorul pe
IP, cheia `parola`.

### Linia de comandă — `scripts/aproba_cerere.py`

Pagina de aprobare e 0075-05. Până atunci operatorul aprobă de pe VPS; scriptul cheamă exact
ce va chema pagina: `--lista`, `<id> --plan`, `<id>` (cere `DA` scris), `--cod-program SS=…`
(D25, repetabil; o sursă-sector care nu e în cerere ▸ refuz), `--link-nou`.

### Contul de provizionare și SQL-ul — `sql/0075_03_provizionare.sql`

Trei părți, de rulat de operator: cele două coloane pe `FX_Inregistrari`; ștergerea coloanelor
`*_w` de pe șablon (planul §0.0 — jobul refuză cât timp sunt acolo); contul
`kbot_provizionare` cu drepturile minime: `CREATE USER` + `SHOW DATABASES` global, `SELECT` pe
`mysql.user`, pe tiparul `1__\_____` (`1nn_SSSS`) tot ce trebuie ca să construiască, umple și
șteargă o bază (~~cu `GRANT OPTION`~~ — vezi «Prima aprobare» mai jos), `SELECT, SHOW VIEW` pe
șablon, drepturi pe tabel în `AVACONT_COMUN`. Contul e `@'localhost'`, iar `config.py` se
conectează prin socket (`unix_socket`), singura cale care se potrivește mereu cu `localhost`
(TCP spre `127.0.0.1` se potrivește doar cu `skip_name_resolve` oprit): Flask și MariaDB stau pe aceeași mașină (operator, 23.09.2026), deci
contul care creează utilizatori nu e atins din afara VPS-ului. De completat: numai parola.

`utils/database.py`: `get_kbot_provisioning_connection()` citește `DB_CONFIG_PROVIZIONARE`
leneș și **nu cade niciodată pe contul de serviciu**.

### Prima aprobare pe server (23.09.2026) și repararea pasului 7

Cererea 1: pașii 1–6 au mers (`111_TRND` creată, 44 de tabele, `CAI` 96–98 cu `IdUnitate`
201–203, `Unitati`, `Unitati_Ani`, 180 de clasificații, contul MariaDB creat), apoi **pasul 7
a picat cu 1044** la `GRANT … ON 111_TRND.* TO …`. **Desfacerea a mers întreagă**, în ordine
inversă: `DROP USER`, `Unitati_Ani`, `Unitati`, `CAI`, `DROP DATABASE` — nimic rămas; cererea
e `Esuata` și se poate relua. (`CAI.IdCai` sare acum peste 96–98; gaură inofensivă.)

Cauza: în `GRANT … ON db.*` MariaDB tratează numele bazei ca **tipar**, iar drepturile de nivel
bază ale acordantului contează atunci numai dacă au fost date pe **exact același șir**.
Drepturile contului stau pe tiparul `1__\_____` — bune pentru `CREATE DATABASE`, tabele și
`INSERT`, niciodată pentru a da mai departe. Singurul drept care ar trece verificarea e unul
**global**, adică superutilizator. Deci: procedura `AVACONT_COMUN.proc_Provizionare_Grant`
(`sql/0075_03_02_grant_procedura.sql`), a lui root, `SQL SECURITY DEFINER`, face exact acel
GRANT după ce verifică: baza are forma `1nn_SSSS` (sensibil la majuscule) și există;
utilizatorul are forma unei adrese și există pe `'%'`. Liniuța de jos e scăpată (`111\_TRND`),
ca dreptul să numească baza, nu un tipar. Contul de provizionare primește `EXECUTE` pe ea și
pierde `GRANT OPTION`. `_create_account` cheamă procedura în loc de `GRANT`.

Procedura a fost creată de operator ca **`Admin`@`%`** (nu root) — merge la fel, fiindcă Admin are
`ALL PRIVILEGES ON *.* WITH GRANT OPTION`. ⚠ Legătură cu 0075-06: dacă `Admin` e șters sau i se
taie drepturile globale, procedura trebuie recreată întâi sub un cont care le păstrează, altfel
orice aprobare pică la pasul 7 (scris și în plan, §13).

A doua încercare (tot 23.09.2026) a picat la pasul 7 cu `1644 proc_Provizionare_Grant: contul nu
exista`, desfacere iar întreagă (`CAI` 99–101 sărite). Cauza: **mysql.connector NU transformă `%%`
în `%`** (înlocuiește doar `%s`), deci `CREATE USER %s@'%%'` a creat contul pe gazda literală
`'%%'`, iar procedura îl caută pe `'%'`. Reparat în `provizionare.py` (CREATE/DROP USER) și
`parola.py` (ALTER USER): gazda se scrie `'%'`.

**A treia încercare a trecut** (23.09.2026): `111_TRND` creată, contul
`jollieandreea@gmail.com` cu rolul `Contabil`, e-mailul cu linkul de parolă sosit. Rămâne din
§9 verificarea 7: parola aleasă prin link, intrarea prin `LoginForm`, `SHOW GRANTS` care
numește numai `111_TRND`.

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/inregistrare/provizionare.py` | **nou** — jobul |
| `PYTHON/routes/inregistrare/randuri.py` | **nou** — rândurile `Clasificatii` + verificarea lor |
| `PYTHON/routes/inregistrare/parola.py` | **nou** — linkul de parolă |
| `PYTHON/routes/inregistrare/inregistrare.py` | `GET /parola`, `POST /api/inregistrare/parola/stare`, `POST /api/inregistrare/parola` |
| `PYTHON/routes/inregistrare/README.md` | module, rute, coduri-motiv, config, secțiunea «Aprobarea» |
| `PYTHON/routes/auth/mailer.py` | `send_account_ready` |
| `PYTHON/utils/database.py` | `get_kbot_provisioning_connection` |
| `PYTHON/scripts/aproba_cerere.py` | **nou** — linia de comandă |
| `PYTHON/static/parola.html` | **nou** — pagina linkului |
| `PYTHON/static/js/inregistrare/parola.js` | **nou** |
| `PYTHON/static/js/inregistrare/api.js` | `linkState`, `setPassword` |
| `sql/0075_03_provizionare.sql` | **nou** — coloanele, curățarea șablonului, contul |
| `sql/0075_03_02_grant_procedura.sql` | **nou** — procedura de GRANT, după eșecul de la prima aprobare |
| `.claude/launch.json` | a doua intrare, `parola-stub` (ciot în scratchpad-ul acestei sesiuni) |

## Rezultatele testelor

**Fără fișiere de test** (decizia operatorului). Ce s-a verificat, fără server:

- `ast.parse` pe cele șapte fișiere Python atinse; toate modulele noi **se importă** (`routes.
  inregistrare.inregistrare`, `provizionare`, `randuri`, `scripts.aproba_cerere`).
- **Regula 0**, cu `tokenize`: diacriticele apar numai în literali de tip șir (mesaje pentru
  operator și solicitant), niciodată în nume sau comentarii.
- **Constructorul de rânduri, apel direct** cu nomenclatoare inventate: 4 surse-sector × 2 F × 2
  E ▸ 16 rânduri; `01A` ▸ `65.01`, `02E` ▸ `65.10`/`02`/`E`, `03A` ▸ `65.03`, `02C` ▸ `65.02`/`C`;
  `650402`/`200104` ▸ `04.02`, `20.01`, `04` (forma din §2a); denumirea de 300 de caractere ▸
  255. Cu coduri rele ▸ un singur refuz care le numește pe toate cinci (sursă, F, E, articol,
  titlu). `CodProgram`: `01A` ▸ `0000002510`, `02E` ▸ `0000000000`, suprascrierea `03A` ia
  loc; o suprascriere pentru `02E` absentă din cerere ▸ refuzată.
- Expresiile regulate: `AUTO_INCREMENT=17` scos, `DEFINER=`…`@`…`` scos, `111_VTRS` acceptat
  de paza de ștergere, `000_DEMO` refuzat.
- **Pagina `/parola` în browser**, pe un ciot care servește fișierele adevărate cu aceleași
  antete CSP și mimează cele două rute: linkul valid ▸ formularul cu unitatea și adresa mascată,
  iar `#…` dispare din bara de adrese; parolă scurtă ▸ mesaj; parole diferite ▸ mesaj; bună ▸
  ecranul final; același link a doua oară ▸ «Linkul nu mai este valabil»; token greșit ▸ la fel;
  fără token ▸ «Linkul este incomplet». În jurnalul ciotului token-ul apare **numai** în corpul
  POST-urilor, niciodată într-un `GET`. Pe telefon (375 px) cardul stă la 16 px de margini,
  fără derulare orizontală.

## Neverificat / amânat

- ⚠ **Jobul n-a rulat niciodată.** Niciun pas și nicio anulare n-au atins un MariaDB. Prima lor
  rulare e pe serverul adevărat — de aceea `--plan` întâi, pe o cerere de probă.
- **Contul există pe server** (operator, 23.09.2026): `kbot_provizionare@localhost`, iar
  `SHOW GRANTS` arată exact cele 14 drepturi din `sql/0075_03_provizionare.sql`, nimic în plus
  sau în minus. Socket-ul este `/run/mysqld/mysqld.sock`, confirmat.
- **Primul `--plan` pe server** (operator, 23.09.2026, cererea 1): contul se conectează prin
  socket și citește tot ce-i trebuie; verificările trec (deci și coloanele `*_w` au fost scoase
  de pe șablon); `111_TRND`, `IdUnitate` 201–203 (MAX din `CAI` = 200), 44 de tabele / 0 view-uri,
  3 surse-sector × 2 F × 30 E = 180 de rânduri, `CodProgram` după D25. **Pașii care scriu n-au
  rulat încă.** Pe VPS scriptul merge numai cu `/root/AVACONT/.venv/bin/python3`.
- **Drepturile contului de provizionare au fost acordate, dar nu folosite încă.** Cele mai
  nesigure: (1) ~~infirmat pe server, vezi «Prima aprobare»~~ că `GRANT … WITH GRANT OPTION` pe tiparul `1__\_____` îi dă voie să acorde
  drepturi pe o bază creată după grant; (2) că `information_schema.TRIGGERS/ROUTINES/EVENTS` îi
  arată ce e pe șablon fără drepturi pe acele obiecte — dacă nu, verificarea trece pe lângă ele.
  Dacă (1) pică, pasul 7 pică, iar rularea se desface: e sigur, doar supărător.
- **Tiparul `1__\_____` prinde și vechiul `101_CCDP`.** Jobul nu șterge decât ce a creat în
  aceeași rulare, dar contul ar avea drepturi pe el.
- **`CAI.CF`** se scrie ca în cerere (numai cifre). Rândurile vechi au ambele forme; verificarea
  D11 le caută pe amândouă, deci nu contează pentru duplicare.
- **`Unitati.Detalii`** din baza nouă = denumirea unității (ce afișează `AlegereUnitateForm`).
  Dedus din cod, nu dintr-o regulă scrisă.
- **Refuzul («Respinge») și pagina de aprobare** sunt 0075-05. Jurnalul (`Jurnal`) nu primește
  încă nimic de la provizionare.
- **Conturile legacy** `NNN_XXXX_Contabil` nu se mai creează (D26); nimic nu se scrie pe serverul
  vechi (D8).
- `.claude/launch.json` pornește ciotul din scratchpad-ul **acestei** sesiuni; calea moare odată
  cu ea (la fel ca intrarea lăsată de 0075-04).
