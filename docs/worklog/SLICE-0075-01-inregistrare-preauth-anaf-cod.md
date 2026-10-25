# SLICE-0075-01…05 — Istoricul înregistrării și aprobării conturilor

Reunire documentară D001-22. Ordinea efectivă a fost 01, 02, 04, 03, 05; secțiunile sunt indexate numeric și păstrează toate probele, compensările, deciziile și restanțele. Mențiunile inițiale «nimic viu» din 03/05 sunt urmate de aprobările și confirmările operatorului din 23.09.2026. 04 evoluează de la o singură pereche sursă-sector la lista D29; denumirea bazei rămâne ascunsă solicitantului, dar editabilă operatorului în 05. Planul consemnează ulterior confirmarea formei URL ANAF v9, față de întrebarea încă deschisă din jurnalul 01. Pagina web operator descrie reperul din septembrie; indexul general consemnează mai târziu înlocuirea ei prin ME01. Contractul distinct al clasificațiilor rămâne în [0075-00](SLICE-0075-00-clasificatii-sursa-scrisa.md).

[0075-01](#slice-0075-01) · [0075-02](#slice-0075-02) · [0075-03](#slice-0075-03) · [0075-04](#slice-0075-04) · [0075-05](#slice-0075-05)

<a id="slice-0075-01"></a>

## SLICE-0075-01 — înregistrare: magazinul dinainte de autentificare, ANAF, `/cod`, `/verifica`

A doua trecere din felia 0075 (creare automată a unei baze de unitate prin pagină publică + aprobarea operatorului, `docs/PLAN_AutoProvisioning.md`). Acoperă **§5.1–5.3** din plan.

### Ce s-a schimbat și de ce

Primele două ecrane ale paginii publice au acum server: codul fiscal (căutat la ANAF) și adresa de e-mail (dovedită printr-un cod de șase cifre). Un blueprint nou, `PYTHON/routes/inregistrare/`, cu trei rute publice:

| Rută | Corp | Răspuns |
|---|---|---|
| `POST /api/inregistrare/anaf` | `{cf}` | `{token, expires_in, cf, unitate:{cui,denumire,adresa,nr_reg_com}}` |
| `POST /api/inregistrare/cod` | `{email}` | `{email_masked, expires_in}` |
| `POST /api/inregistrare/verifica` | `{cod}` | `{ok, email_masked, expires_in}` |

Ultimele două cer token-ul în antetul **`X-Registration-Token`** — antet, nu corp, fiindcă §5.4 (`/nume`) și rutele de nomenclator care urmează sunt GET-uri, iar un singur mod de a purta token-ul bate două.

#### Magazinul dinainte de autentificare (§5.1)

Constatarea F5 a planului s-a confirmat pe cod: `session_store.STORE.put_note / get_note / delete_note` acceptă **orice** șir drept token și nu ating niciodată cheile de sesiune pe care le validează paza bearer. Deci înregistrarea e **o notă**, numită `register`, pe un token pe care `store.py` îl bate singur. **Zero cod nou de backend**, și memoria și Redis se poartă identic — de aceea valoarea notei e numai JSON simplu (backend-ul Redis face `json.dumps` pe ea).

Durata: **30 de minute, absolut**, de la căutarea ANAF. Fiecare scriere pune nota la loc cu **cât a mai rămas**, niciodată cu 30 de minute noi — altfel fereastra ar putea fi plimbată înainte la nesfârșit cerând cod după cod.

#### De ce token-ul se naște la `/anaf`, nu la `/cod`

**Planul se contrazice singur**, și asta a fost lămurit cu operatorul: §4 spune «token-ul de la pasul 2», dar §5.1 pune `cf` și `anaf` printre câmpurile notei — care sunt date de la pasul 1. Decizia (22.09.2026): token-ul vine de la `/anaf`.

Motivul e `FX_Inregistrari`, care are **două** coloane de denumire: `DenumireAnaf` (ce a spus ANAF) și `Denumire` (ce a scris solicitantul). Operatorul a confirmat că solicitantul **are voie** să schimbe denumirea, iar §7 cere ca pagina de aprobare să le arate una lângă alta. Dacă denumirea de la ANAF ar călători prin browser, pagina de aprobare ar compara ce a scris solicitantul cu ce a scris solicitantul. La fel și codul fiscal: D11 refuză un CF care are deja bază — ținut în browser, cineva ar putea trece verificarea la pasul 1 cu un cod liber și trimite altul la pasul 6.

#### Proxy-ul ANAF (§5.2)

Portat din `InformatiiFirmaOnline2` (modulul Access `Module__EFactura`, citit de la operator pe 22.09.2026): POST, `Content-Type: application/json`, corp `[{"cui": <cifre, fără ghilimele>, "data": "yyyy-mm-dd"}]`, iar un răspuns care începe cu `<html>` înseamnă că ANAF s-a stricat, nu că nu știe codul. Verificarea aceea e păstrată literă cu literă. `urllib` din biblioteca standard (F10: nu există `requests` în venv).

**Un lucru NU s-a portat, și contează.** Funcția din Access decide «negăsit» citind `cod <> "200"`. **v9 nu are deloc câmpul `cod`** — verificat pe un răspuns real din 22.09.2026, care avea exact două chei de nivel întâi, `found` și `notFound`. Deci testul de aici e `found` gol. Diferența e intenționată, nu o scăpare.

Câmpurile citite, din `found[0].date_generale`: `denumire`, `adresa`, `cui`, `nrRegCom`. Structura `adresa_sediu_social` e lăsată în pace — șirul plat e ce recunoaște solicitantul, și tot el ajunge pe pagina de aprobare. ANAF întoarce diacritice cu **sedilă** (`PLOIEŞTI`, `Administraţia`), nu cu virgulă dedesubt; nimic nu le rescrie.

#### Verificarea prin cod (§5.3)

Aceeași mașinărie ca schimbarea parolei din felia 0072 (`auth.py`): șase cifre din `secrets`, se păstrează **numai** hash-ul SHA-256, comparație cu `hmac.compare_digest`, **10 minute**, cel mult **5** încercări greșite și codul se aruncă. Singura diferență e unde stă — o notă pe token-ul de înregistrare, nu pe o sesiune.

Un cod nu poate trăi mai mult decât înregistrarea care îl poartă, iar `expires_in` spune numărul adevărat (`min(10 min, cât a mai rămas)`). Un cod nou îl înlocuiește pe cel vechi, iar schimbarea adresei **dez-verifică** înregistrarea: cine a dovedit vechea cutie poștală n-a dovedit nimic despre cea nouă.

#### Cele două verificări în bază

- `/anaf` refuză un CF care are deja bază (**D11**), căutat în `CAI` **și** în
  `AVACONT_COMUN.Unitati` — F0 a arătat că listele nu coincid, iar oricare dintre ele e motiv
  de refuz. Codurile sunt ținute și goale și cu prefix `RO`, deci se întreabă **ambele
  grafii** — așa comparația rămâne pe o coloană indexată, fără `REPLACE()` în jurul ei.
- `/cod` refuză o adresă care e deja cont MariaDB **sau** apare deja în
  `Unitati_Utilizatori`, cu un mesaj care nu spune unui străin care dintre cele două a fost.
  Contul MariaDB contează fiindcă pasul 7 al provizionării ar pica la `CREATE USER` **după**
  ce baza e deja construită.

**Verificat pe mașină (22.09.2026):** contul de serviciu din `DB_CONFIG_NEW` chiar poate citi `mysql.user` pe serverul K-BOT (`SELECT COUNT(*)` ▸ 10 conturi). Era singurul lucru care ar fi putut obliga la altă formă a verificării.

### Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/inregistrare/__init__.py` | nou — pachetul; **nu** importă blueprint-ul |
| `PYTHON/routes/inregistrare/README.md` | nou — contractul rutelor, codurile-motiv, config-ul |
| `PYTHON/routes/inregistrare/store.py` | nou — înregistrarea ca notă pe `session_store.STORE` |
| `PYTHON/routes/inregistrare/anaf.py` | nou — apelul ANAF v9, pur, `urllib` |
| `PYTHON/routes/inregistrare/inregistrare.py` | nou — cele trei rute |
| `PYTHON/routes/auth/mailer.py` | `send_registration_code` (F12: fișierul avea un singur mesaj) |
| `PYTHON/main.py` | blueprint importat și înregistrat |
| `docs/PLAN_AutoProvisioning.md` | §0.0 adus la zi; §4/§5.1 lămurite; §5.2 corectat pe `cod`; §8 fără pytest |

`__init__.py` **nu** importă blueprint-ul, ca `anaf.py` și `store.py` să rămână importabile pe o mașină fără `config.py` — același motiv scris în `routes/migrare/__init__.py`. `main.py` îl ia explicit.

### Codurile-motiv

ASCII, stabile, ca pagina să ramifice fără să citească textul românesc:

`CF_INVALID` · `CF_EXISTS` · `ANAF_NOT_FOUND` · `ANAF_INCOMPLETE` · `ANAF_UNAVAILABLE` · `RATE_LIMITED` · `DB_ERROR` · `TOKEN_ABSENT` · `TOKEN_UNKNOWN` · `TOKEN_EXPIRED` · `EMAIL_INVALID` · `EMAIL_TOO_LONG` · `EMAIL_TAKEN` · `MAIL_NOT_CONFIGURED` · `MAIL_FAILED` · `CODE_ABSENT` · `CODE_NOT_REQUESTED` · `CODE_EXPIRED` · `CODE_WRONG` · `CODE_ATTEMPTS_EXHAUSTED`

### Config de adăugat pe VPS

Ambele au valori implicite în cod, deci serverul pornește și fără ele:

```python
ANAF_TVA_URL = "https://webservicesp.anaf.ro/api/PlatitorTvaRest/v9/tva"
ANAF_TIMEOUT = 15
```

`SMTP_*` există deja de la felia 0072.

### Rezultatele testelor

- `ast.parse` pe toate fișierele atinse: **curat** (6/6).
- Regula 0 verificată prin căutare: în `.py` diacriticele apar **numai** în mesajele
  românești pentru solicitant; niciun identificator, comentariu sau cheie JSON nu le poartă.
- **Nu s-a scris niciun fișier de test.** Decizia operatorului, 22.09.2026: «no pytest files,
  i don't use them anyway». Planul §8 cerea pytest la această trecere; §8 a fost corectat ca
  să nu mai mintă. Consecința, spusă apăsat: **acoperirea acestei treceri e zero** — nimic nu
  demonstrează automat limitele codului, numărătoarea încercărilor sau desfacerea ferestrei.
- **Nicio rută nu a fost pornită.** Nimic de aici nu a atins un Flask viu, un MariaDB sau
  ANAF. Singurul lucru atins de-adevăratelea pe 22.09.2026 a fost apelul ANAF făcut de mână,
  de operator, de pe VPS (`curl`), al cărui răspuns e transcris în `anaf.py`.

### Neverificat / amânat

- **Forma căii URL.** Access-ul cheamă `/PlatitorTvaRest/api/v6/ws/tva`; aici e
  `/api/PlatitorTvaRest/v9/tva`. Operatorul a ales v9 și apelul lui de probă a răspuns, dar
  **nu s-a notat pe care dintre cele două căi l-a făcut**. Dacă e cealaltă, se schimbă
  `ANAF_TVA_URL` pe VPS, fără cod.
- **Limita de apeluri a ANAF pe v9** rămâne necunoscută (planul o marca deja UNVERIFIED). Un
  apel per înregistrare e mult sub orice cifră publicată; numărul în sine n-a fost confirmat.
- **`AnafIncomplete`** (ANAF găsește codul dar nu dă denumire) e o ramură **teoretică** — nu
  s-a văzut niciodată. Există fiindcă `Denumire` e `NOT NULL` mai departe în flux.
- **Limitatorul e în proces, la un singur worker** (vezi avertismentul din `ratelimit.py`).
  Pe o pagină publică asta contează mai mult decât pe login: o repornire de gunicorn redă
  atacatorului cele 5/30 încercări. Nu e nou și nu blochează nimic, dar acum e expus spre
  internet.
- **Nota de înregistrare nu e legată de IP.** Cine fură token-ul preia înregistrarea. Are 30
  de minute de viață și nu duce la nimic fără cutia poștală, dar merită spus.
- Rămâne din 0075-00: de ce cele trei coloane virtuale de pe `AVACONT_SURSA.Clasificatii` au
  trebuit șterse de mână, și dacă cele șapte baze de unitate au trecut curat.

---

<a id="slice-0075-02"></a>

## SLICE-0075-02 — nomenclatoare, numele bazei, `FX_Inregistrari`, `/cerere`

A treia trecere din felia 0075 (`docs/PLAN_AutoProvisioning.md`). Acoperă **§5.4–5.5** din plan, plus împrospătarea lui `sql/avacont_comun_login.sql` cerută de §8.

### Ce s-a schimbat și de ce

Restul paginii publice are acum server. Patru rute noi, toate în spatele token-ului de înregistrare:

| Rută | Corp / parametri | Răspuns |
|---|---|---|
| `GET /api/inregistrare/sursasector` | — | `{surse:[{cod,denumire}]}` |
| `GET /api/inregistrare/clasificatii` | `?tip=F\|E` | `{tip, coduri:[{cod,denumire}]}` |
| `GET /api/inregistrare/nume` | `?denumire=` | `{db_name, numar, litere, previzualizare}` |
| `POST /api/inregistrare/cerere` | `{denumire, an, sursasector[], clsf_f[], clsf_e[]}` | `{id_cerere, randuri, operator_anuntat}` |

#### Numele bazei (§5.4, D3/D12) — `nume.py`

`SSSS` = primele patru consoane; sub patru consoane, vocalele umplu restul în ordinea din nume; sub patru litere cu totul ▸ refuz, nu umplutură inventată. `nn` = cea mai mică pereche liberă din `11..99` **fără cifra zero**.

**De ce fără zero.** Toate bazele existente sunt `NNN_XXXX`, numerotate de la `000` în sus (`000_DEMO`, `001_GR23`, `053_LTTR`, plus `101_CCDP`, dinaintea regulii). Blocul `111`–`199` nu poate fi atins de numerotarea veche, deci un nume ales singur de server nu poate cădea peste unul pe care operatorul urmează să îl scrie de mână.

**Ce se compară e NUMĂRUL, nu numele întreg.** §5.4 spune «perechea liberă… absentă din `SCHEMATA` și din `CAI.DbName`», fără să spună ce anume se caută acolo. Rândurile existente poartă fiecare un `NNN` distinct, ceea ce se citește ca număr de unitate, nu ca dezambiguizator — deci `1nn` e liber când **niciun** nume din cele două liste nu începe cu `1nn_`. Citirea e scrisă ca atare în cod; dacă operatorul o vrea altfel, e o linie.

**Zero diacritice în fișier**, și fără tabel de cazuri speciale: plierea trece prin `unicodedata` — NFD desparte litera de semnele ei, iar aruncarea semnelor lasă baza. Acoperă ambele grafii ale românei (virgulă dedesubt **și** sedilă, cea pe care o trimite ANAF), plus ă/â/î.

Verificat prin apel direct (nu fișier de test): `AVATAR SOFT SRL` ▸ `VTRS`, `LICEUL TEORETIC` ▸ `LCLT`, `Direcția de Asistență Socială` ▸ `DRCT`, `MUN. PLOIEŞTI` ▸ `MNPL`, `Scoala Gimnaziala Nr. 1` ▸ `SCLG`, `AEIOU` ▸ `AEIO` (zero consoane, umplut cu vocale), `ANA` și `IAE` ▸ refuzate. Primele numere: `111…119`, apoi `121`; cu `111`/`112` ocupate ▸ `113`. `db_name('111','VTRS')` ▸ `111_VTRS`.

`/nume` e **previzualizare** și o spune în răspuns (`previzualizare: true`): numele adevărat se recalculează la aprobare, fiindcă un număr liber acum poate fi luat până ajunge operatorul la cerere.

#### Nomenclatoarele — `nomenclatoare.py`

**De ce numai `ClsfE` e filtrat.** `Clasificatii` are patru chei străine spre `AVACONT_COMUN` și două sunt pe valori **derivate** din codul E: `Articol = Left(E,2) + "." + Mid(E,3,2)` și `Titlu = Left(E,2)`. Cele cincisprezece coduri pe care §2a le-a numărat pe server (opt fără `Articol`, patru fără `Titlu`) derivă spre părinți inexistenți. Oferite în arbore, ar fi frunze care nu pot fi salvate: solicitantul alege una, așteaptă aprobarea, iar provizionarea moare cu `1452` la ultimul pas.

Deci filtrul **este chiar join-ul** — un cod apare doar dacă rândul în care s-ar transforma își găsește ambii părinți. Așa lista nu se poate îndepărta de constrângere: dacă mâine se adaugă rândurile lipsă, codurile apar singure, fără o listă de cincisprezece excepții care s-ar învechi în tăcere. `DefaClsfF` nu are nevoie de filtru — `ClsfF` e propria lui cheie străină, deci orice rând al dicționarului e prin definiție inserabil.

`GROUP BY`, nu `DISTINCT`: §2a spune că `DefaClsfF` **nu are cheie primară**, `ClsfF` e nullable și indexul nu e unic — două rânduri cu același cod și captiuni diferite ar trece amândouă printr-un `DISTINCT` și arborele ar arăta codul de două ori. Plus D16: un rând `DefaClsfE` fără captiune nu se arată.

`read_codes` / `read_sursasector_codes` folosesc **aceleași** interogări ca listele, ca ce se validează la `/cerere` să fie exact ce s-a oferit. Un check construit dintr-o altă interogare e felul în care o pagină ajunge să ofere un cod pe care propriul ei server îl refuză apoi.

#### Cererea — `cerere.py` + `sql/0075_fx_inregistrari.sql`

Totul se verifică din nou, pe server, din dicționarele citite **în acel moment**. Motivul mai probabil nu e un client ostil, ci fereastra de 30 de minute: un cod oferit la minutul doi poate să nu mai existe la minutul douăzeci. Re-citirea transformă asta într-un refuz limpede acum, nu într-o cheie străină picată în timpul provizionării, după ce baza e deja construită.

**Ce înseamnă «frunză» (§4.1).** `xx0000` = rădăcină, niciodată aleasă singură; `xxyy00` = nivel 1, aleasă **doar** dacă nimic nu atârnă sub ea; `xxyyzz` = frunză, mereu. Cazul din mijloc merită spus: datele reale au `650500` și `590100`, coduri de nivel 1 pe care nimeni nu le-a subîmpărțit, și sunt rânduri legitime. Deci «e frunză?» nu se poate răspunde din cod — are nevoie de tot dicționarul, de aceea verificarea e aici și nu în pagină.

D11 se verifică **încă o dată** la `/cerere` (planul o cere): jumătate de oră e destul ca operatorul să fi creat unitatea de mână între timp.

`FX_Inregistrari`: **fără chei străine, intenționat.** O cerere e o corespondență, nu o unitate — există înaintea bazei, a rândului din `CAI` și a contului, se poate sfârși `Respinsa` fără să arate spre nimic, iar o rulare eșuată trebuie să lase rândul în urmă cu `Motiv`-ul intact, nu să fie luat de o cascadă odată cu resturile.

Nota către operator: dacă `OPERATOR_EMAIL` lipsește sau trimiterea pică, **cererea rămâne înregistrată** — se scrie un avertisment în log și răspunsul poartă `operator_anuntat: false`. Niciodată o eroare care ar invita solicitantul să trimită tot a doua oară.

La reușită, nota de înregistrare se șterge: altfel același token ar putea depune o a doua cerere pe o adresă deja dovedită.

#### Plafonul de rânduri — **adăugat de mine, nu din plan**

O cerere devine un rând `Clasificatii` per frunză F × frunză E × sursă-sector, iar exemplul planului e 20 × 40 × 2 = 1 600. Aritmetica nu se oprește acolo: toate frunzele ambilor arbori și toate cele 14 surse ar însemna 531 × 686 × 14 — **cinci milioane** de rânduri, puse la coadă dintr-un formular anonim. `cerere.MAX_ROWS = 50 000` e paza. Nu e cerut de plan; e o constantă, se mută dintr-o linie. Semnalat aici fiindcă e o decizie de proiectare luată de mine, nu una a operatorului.

#### `sql/avacont_comun_login.sql`

Operatorul a spus (22.09.2026) că DDL-urile din depozit sunt din schema veche și nu merită efort. Deci s-a făcut minimul onest: un antet care spune apăsat că **fișierul e referință învechită, nu serverul**, plus cele două corecturi pe care §2a le-a verificat pe mașină — cheia primară e `IdCai` AUTO_INCREMENT (nu `IdUnitate`), iar `IdUnitate` e coloană simplă, neunică. «Întrebarea deschisă» de la coada tabelei e acum **răspunsă**: o unitate are un rând `CAI` per sursă-sector, deci mai multe rânduri împart și `DbName` și `IdUnitate` — de asta §5.6 pasul 3 are nevoie de `GET_LOCK` explicit.

### Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/inregistrare/nume.py` | nou — `1nn_SSSS`, pliere prin `unicodedata` |
| `PYTHON/routes/inregistrare/nomenclatoare.py` | nou — cele trei liste, filtrul E prin join |
| `PYTHON/routes/inregistrare/cerere.py` | nou — verificarea și scrierea cererii |
| `PYTHON/routes/inregistrare/inregistrare.py` | patru rute noi |
| `PYTHON/routes/inregistrare/README.md` | adus la zi cu trecerea 0075-02 |
| `PYTHON/routes/auth/mailer.py` | `operator_address`, `send_registration_notice` |
| `sql/0075_fx_inregistrari.sql` | **nou** — DDL-ul tabelei de cereri |
| `sql/avacont_comun_login.sql` | antet «referință învechită» + `IdCai` PK, `IdUnitate` neunic |
| `docs/PLAN_AutoProvisioning.md` | §0.0, §5.4, §5.5, §8 aduse la zi |

### Rezultatele testelor

- `ast.parse` pe toate fișierele Python atinse: **curat** (5/5).
- **Regula 0 verificată prin AST**, nu prin căutare: s-au adunat intervalele de linii ale
  tuturor literalilor de tip șir din cele șapte fișiere ale feliei și s-a căutat orice
  diacritică **în afara** lor ▸ **zero**. `nume.py` și `nomenclatoare.py` n-au nicio
  diacritică deloc; cele 15 linii din `cerere.py` sunt toate mesaje pentru solicitant.
- Algoritmul numelui verificat prin apel direct (valorile de mai sus).
- **Fără fișiere de test** (decizia operatorului). Acoperire automată: **zero**.
- **Nicio rută n-a fost pornită.** Nimic n-a atins un Flask viu, un MariaDB sau ANAF.

### Adăugat după commit — schema adevărată (22.09.2026, seara)

Operatorul a pus în `MariaDB_Schema/` cele trei scheme luate de pe serverul K-BOT (19:21, MariaDB 10.11.14). Folderul e **în `.gitignore`**, deci trăiește numai pe discul lui. Ce a schimbat pentru trecerea asta:

- **`DefaSursaSector` e `(SursaSector PK, Sursa, Sectorul, Denumire)`** — necunoscuta de mai jos
  e închisă, `read_sursasector` găsește `Denumire` din prima. Căutarea de captiune rămâne în
  cod: nu costă nimic și nu se mai uită de ce e acolo.
- **`FX_Inregistrari` e pe server** — scriptul a fost rulat. `AUTO_INCREMENT = 1`, deci nicio
  cerere încă. Forma e cea scrisă, cu `CHECK (json_valid(Payload))` cu tot.
- **`Clasificatii` chiar nu are cheie străină pe `ClsfE`** — patru spre `AVACONT_COMUN`
  (`DefaArticol`, `DefaClsfF`, `DefaSS`, `DefaTitlu`) plus `Clasificatii__Unitati`, locală.
  Deci cele 15 coduri neinserabile mor pe `Articol`/`Titlu`, exact unde `nomenclatoare.py` face
  join. Filtrul e îndreptat unde trebuie.
- **`DefaClsfE` ARE cheie primară pe `ClsfE`; `DefaClsfF` nu are niciuna** — `GROUP BY`-ul era
  necesar pentru F și e doar nevătămător pentru E.
- ⚠ **`CAI` nu are niciun index pe `IdUnitate`.** `ix_CAI_IdUnitate`, pe care trecerea asta l-a
  adăugat în `sql/avacont_comun_login.sql`, **nu există pe server**. Restul corecturii ține:
  `IdCai` e cheia AUTO_INCREMENT (la 96). Lipsa oricărui index face `GET_LOCK`-ul din §5.6 și
  mai necesar. Corectat în plan §2a; fișierul din `sql/` rămâne referință de înțelesuri.
- ⚠ **Pentru 0075-03:** `AVACONT_SURSA.Clasificatii` încă poartă `Sector_w`, `Sursa_w`, `SS_w`
  de la 0075-00; `000_DEMO` nu le are. Cum §5.6 face baza nouă clonând șablonul, orice unitate
  creată de acum ar moșteni trei coloane moarte. De șters pe șablon.

### Neverificat / amânat
- **Numărătorile din §2a nu au fost re-numărate** (52/531 la F, 25/317/686 la E, cele 15
  coduri E neinserabile). Codul nu depinde de ele — filtrul e join-ul, nu o listă — dar
  numerele din comentarii vin din plan, nu dintr-o citire proprie.
- **`Payload` se scrie, nu se citește încă.** Cine îl va citi e pagina de aprobare (0075-05)
  și jobul de provizionare (0075-03); forma `{"ss","f","e"}` n-a fost exersată de niciun
  cititor.
- **`OPERATOR_EMAIL` nu e încă în `config.py`** pe VPS, deci până e pus, `operator_anuntat`
  va fi `false` la fiecare cerere. Nu blochează nimic — cererea se scrie oricum.
- Rămâne din 0075-00: de ce cele trei coloane virtuale de pe `AVACONT_SURSA.Clasificatii` au
  trebuit șterse de mână, și dacă cele șapte baze de unitate au trecut curat.

---

<a id="slice-0075-03"></a>

## SLICE-0075-03 — jobul de provizionare (aprobarea unei cereri)

A cincea trecere din felia 0075 (`docs/PLAN_AutoProvisioning.md`). Acoperă **§5.6** și **§6**: din rândul `FX_Inregistrari` lăsat de `/cerere` iese o unitate care merge — baza, registrul, clasificațiile, contul MariaDB și linkul de parolă. Cerută de operator pe 23.09.2026 («lets do step 3»), după ce a întrebat cum se aprobă o cerere: **până acum nu se putea deloc**.

### Ce s-a schimbat și de ce

#### Jobul — `routes/inregistrare/provizionare.py`

Trei intrări: `plan()` (toate verificările și toate valorile calculate, **nu scrie nimic**), `approve()` (jobul întreg) și `link_nou()` (alt link de parolă pentru o cerere aprobată).

| # | Pas | Anulare înregistrată după reușită |
|---|---|---|
| — | Verificări: stare `InAsteptare`/`Esuata`; e-mail cu forma unui nume de cont; e-mail liber în `mysql.user` și `Unitati_Utilizatori`; CF liber în `CAI`/`Unitati` (D11); denumire cu ≥ 4 litere; șablon fără coloanele `*_w` și fără declanșatori/proceduri/evenimente; toate rândurile trec de nomenclatoare; ≤ `MAX_ROWS`; `CodProgram` valid | — |
| 1 | `1nn_SSSS` calculat din nou (`nume.py`), refuzat dacă baza există | — |
| 2 | `CREATE DATABASE` cu setul de caractere al șablonului; fiecare tabel din `SHOW CREATE TABLE` (fără `AUTO_INCREMENT=N`, cu `FOREIGN_KEY_CHECKS=0` pe sesiune); view-urile în runde, fără `DEFINER`; numărătoarea tabelelor/view-urilor comparată cu șablonul | `DROP DATABASE` |
| 3–4a | Sub `GET_LOCK('cai_idunitate')`, într-o tranzacție: `CAI` (un rând pe sursă-sector, `IdUnitate` = MAX+1…), `AVACONT_COMUN.Unitati`, `Unitati_Ani` | trei `DELETE` |
| 5–6 | În baza nouă, într-o tranzacție: `Unitati` (același `IdUnitate`), apoi `Clasificatii` în loturi de 1 000, numărate înainte de commit | pleacă cu baza |
| 7 | `CREATE USER '<email>'@'%'` cu parolă aleatoare; `GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE` **numai** pe baza nouă (D18); `Unitati_Utilizatori` cu `Rol = 'Contabil'` (D24) | `DROP USER`, `DELETE` |
| 8 | Hash-ul linkului, `Stare = 'Aprobata'`, `DbName`, `DataDecizie`, `Decis` — commit; apoi e-mailul | — |

Orice eșec: anulările rulează **de la ultima la prima**; una care pică **nu** oprește restul și **nu** e înghițită — ajunge, cuvânt cu cuvânt, în `Motiv` («NU s-a putut anula: …»), în jurnal și în excepția întoarsă, fiindcă e exact ce are operatorul de curățat de mână. Cererea devine `Esuata` și se poate aproba din nou după reparare.

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

#### Rândurile — `routes/inregistrare/randuri.py`

§6 și D5 ca funcție pură: `Capitol = Left(F,2).Left(SS,2)` (`02E` ▸ `.10`), `Subcapitol`, `Articol`, `Alineat`, `Denumire` (tăiată la 255). `Sector`/`Sursa`/`SS` din regula unică `clasificatii_ss.ss_values`, cu litera sursei dată explicit; SS-ul derivat e comparat cu cel ales. Înainte de orice rând, verificarea față de `DefaSursaSector`, `DefaClsfF`, `DefaClsfE` (existență + denumire), `DefaArticol`, `DefaTitlu` — **toate** codurile rele numite deodată.

#### Linkul de parolă (D13) — `parola.py`, `/parola`, două rute

Token-ul (256 de biți) stă în link **după `#`**, deci browserul nu-l trimite niciodată într-o adresă: nu ajunge în jurnalele nginx și nici în `Referer`. Pagina îl citește o dată, îl șterge din bara de adrese și îl trimite numai în corpul a două POST-uri. În baza de date stă **numai SHA-256-ul lui**, pe rândul cererii (`ParolaHash`, `ParolaExpira`, două coloane noi) — deci linkul supraviețuiește unei reporniri, iar pagina din 0075-05 poate vedea dacă a fost folosit.

Parola se pune cu `ALTER USER` prin contul de provizionare (nimeni nu are parola veche, deci `SET PASSWORD` ca la 0072 nu merge), apoi hash-ul se golește: linkul merge o singură dată. Ordinea e aleasă: parola întâi, hash-ul după — invers ar putea cheltui linkul și lăsa contul fără parolă. Minim 8 caractere (ca la 0072), maxim 128, fără spațiu la capete. Limitatorul pe IP, cheia `parola`.

#### Linia de comandă — `scripts/aproba_cerere.py`

Pagina de aprobare e 0075-05. Până atunci operatorul aprobă de pe VPS; scriptul cheamă exact ce va chema pagina: `--lista`, `<id> --plan`, `<id>` (cere `DA` scris), `--cod-program SS=…` (D25, repetabil; o sursă-sector care nu e în cerere ▸ refuz), `--link-nou`.

#### Contul de provizionare și SQL-ul — `sql/0075_03_provizionare.sql`

Trei părți, de rulat de operator: cele două coloane pe `FX_Inregistrari`; ștergerea coloanelor `*_w` de pe șablon (planul §0.0 — jobul refuză cât timp sunt acolo); contul `kbot_provizionare` cu drepturile minime: `CREATE USER` + `SHOW DATABASES` global, `SELECT` pe `mysql.user`, pe tiparul `1__\_____` (`1nn_SSSS`) tot ce trebuie ca să construiască, umple și șteargă o bază (~~cu `GRANT OPTION`~~ — vezi «Prima aprobare» mai jos), `SELECT, SHOW VIEW` pe șablon, drepturi pe tabel în `AVACONT_COMUN`. Contul e `@'localhost'`, iar `config.py` se conectează prin socket (`unix_socket`), singura cale care se potrivește mereu cu `localhost` (TCP spre `127.0.0.1` se potrivește doar cu `skip_name_resolve` oprit): Flask și MariaDB stau pe aceeași mașină (operator, 23.09.2026), deci contul care creează utilizatori nu e atins din afara VPS-ului. De completat: numai parola.

`utils/database.py`: `get_kbot_provisioning_connection()` citește `DB_CONFIG_PROVIZIONARE` leneș și **nu cade niciodată pe contul de serviciu**.

#### Prima aprobare pe server (23.09.2026) și repararea pasului 7

Cererea 1: pașii 1–6 au mers (`111_TRND` creată, 44 de tabele, `CAI` 96–98 cu `IdUnitate` 201–203, `Unitati`, `Unitati_Ani`, 180 de clasificații, contul MariaDB creat), apoi **pasul 7 a picat cu 1044** la `GRANT … ON 111_TRND.* TO …`. **Desfacerea a mers întreagă**, în ordine inversă: `DROP USER`, `Unitati_Ani`, `Unitati`, `CAI`, `DROP DATABASE` — nimic rămas; cererea e `Esuata` și se poate relua. (`CAI.IdCai` sare acum peste 96–98; gaură inofensivă.)

Cauza: în `GRANT … ON db.*` MariaDB tratează numele bazei ca **tipar**, iar drepturile de nivel bază ale acordantului contează atunci numai dacă au fost date pe **exact același șir**. Drepturile contului stau pe tiparul `1__\_____` — bune pentru `CREATE DATABASE`, tabele și `INSERT`, niciodată pentru a da mai departe. Singurul drept care ar trece verificarea e unul **global**, adică superutilizator. Deci: procedura `AVACONT_COMUN.proc_Provizionare_Grant` (`sql/0075_03_02_grant_procedura.sql`), a lui root, `SQL SECURITY DEFINER`, face exact acel GRANT după ce verifică: baza are forma `1nn_SSSS` (sensibil la majuscule) și există; utilizatorul are forma unei adrese și există pe `'%'`. Liniuța de jos e scăpată (`111\_TRND`), ca dreptul să numească baza, nu un tipar. Contul de provizionare primește `EXECUTE` pe ea și pierde `GRANT OPTION`. `_create_account` cheamă procedura în loc de `GRANT`.

Procedura a fost creată de operator ca **`Admin`@`%`** (nu root) — merge la fel, fiindcă Admin are `ALL PRIVILEGES ON *.* WITH GRANT OPTION`. ⚠ Legătură cu 0075-06: dacă `Admin` e șters sau i se taie drepturile globale, procedura trebuie recreată întâi sub un cont care le păstrează, altfel orice aprobare pică la pasul 7 (scris și în plan, §13).

A doua încercare (tot 23.09.2026) a picat la pasul 7 cu `1644 proc_Provizionare_Grant: contul nu exista`, desfacere iar întreagă (`CAI` 99–101 sărite). Cauza: **mysql.connector NU transformă `%%` în `%`** (înlocuiește doar `%s`), deci `CREATE USER %s@'%%'` a creat contul pe gazda literală `'%%'`, iar procedura îl caută pe `'%'`. Reparat în `provizionare.py` (CREATE/DROP USER) și `parola.py` (ALTER USER): gazda se scrie `'%'`.

**A treia încercare a trecut** (23.09.2026): `111_TRND` creată, contul `jollieandreea@gmail.com` cu rolul `Contabil`, e-mailul cu linkul de parolă sosit. Rămâne din §9 verificarea 7: parola aleasă prin link, intrarea prin `LoginForm`, `SHOW GRANTS` care numește numai `111_TRND`.

### Fișiere atinse

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

### Rezultatele testelor

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

### Neverificat / amânat

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

---

<a id="slice-0075-04"></a>

## SLICE-0075-04 — pagina publică de înregistrare

A patra trecere din felia 0075 (`docs/PLAN_AutoProvisioning.md`). Acoperă **§4** și **§4.1** din plan: ecranele pe care le vede solicitantul, peste rutele scrise în 0075-01 și 0075-02. Luată înaintea lui 0075-03, la alegerea operatorului (22.09.2026).

### Ce s-a schimbat și de ce

O pagină statică servită de Flask (`GET /inregistrare`), un vrăjitor cu **șase ecrane**, un singur modul JS. Nimic nu se ține în `localStorage`: token-ul de înregistrare și tot ce a completat solicitantul trăiesc în memoria paginii, deci o reîncărcare reia de la codul fiscal (de aceea și paza pe `beforeunload`).

#### Componentele JS stau acum în `PYTHON/static/js/`

Componentele din `JS_COMPONENTS/` sunt module ES cu importuri **relative** (`../../listener-tracker/…`), care se rezolvă după URL, nu după disc. Ca arborele și combo-ul să meargă servite de Flask, folderele partajate trebuie să stea lângă ele, sub aceeași rădăcină. Alegerea operatorului: **copie vendorizată** în `PYTHON/static/` (35 de fișiere — `event-bus`, `listener-tracker`, `utils`, `treeview`, `combobox`, `instances-registry`, plus CSS-ul lor). `JS_COMPONENTS/` rămâne originalul, nemodificat și **în afara commit-ului**.

Toate cele 27 de importuri relative au fost urmărite fișier cu fișier: niciunul nu cade în gol.

**Două divergențe față de original**, ambele în arbore și ambele comentate la fața locului:

- `escapeHtml` scapă și `"` și `'`. Textul ajunge și în atribute (`data-text="…"`,
  `data-path='…'`), iar captiunile vin din nomenclator: o ghilimea în captiune ar fi rupt
  atributul. Pagina e publică, deci asta nu e cosmetică.
- `getComputedTreeHeight` măsoară orice rând randat când nu găsește o frunză. La prima
  deschidere toate rădăcinile sunt strânse, nu există frunză pe ecran, iar rezerva de 24px
  tăia lista.

#### Modul «bifabil» al arborelui (D21)

`treeview-checkable.js`, mixin nou, opt-in prin `checkable: true`. Starea stă **numai pe frunze**; starea unui părinte se deduce din ele (bifat / parțial / nebifat), iar rezultatul cerut de `/cerere` sunt chiar frunzele bifate, în ordinea arborelui. `setData` păstrează bifele și le curăță pe cele ale codurilor care nu mai există — de asta depinde reluarea după `CLSF_*_NECUNOSCUT`.

Numărul ales se arată în `placeholder`-ul input-ului (input-ul e `readOnly`: în componentă, `value` e text de căutare, deci nu poate ține altceva).

#### Arborele din coduri plate (§4.1)

`tree-builder.js`: `xx0000` = rădăcină, `xxyy00` = nivel 1, restul = frunze. **D14**: o frunză al cărei părinte de nivel 1 lipsește se agață de rădăcina ei. Nodurile fără rădăcină se aruncă; rădăcinile rămase fără copii, la fel. Captiunea e `cod · denumire`.

#### Ecranul 1 — «Date Unitate» (schimbare cerută la 22.09.2026)

Codul fiscal **și** denumirea stau pe același ecran. Solicitantul nu mai vede nimic despre baza de date: numele propus (`1nn_SSSS`) nu se mai afișează nici pe ecran, nici în rezumat. `/nume` se cheamă totuși la «Continuă», dar numai pentru verdictul lui — e singurul loc care ține regula celor patru litere, pe care `/cerere` n-o repetă, iar numele adevărat se stabilește oricum la aprobare. Așa o denumire din care nu se poate forma un nume de bază e oprită pe primul ecran, nu la trimitere.

Vrăjitorul are deci șase ecrane, nu șapte: **1** Date Unitate · **2** E-mail · **3** Surse · **4** Clasificații · **5** Trimitere · **6** confirmare.

#### Ecranul 3 — sursa și sectorul, două combo-uri (schimbare cerută la 22.09.2026)

În locul listei de bife: **Sursa** într-un combo, apoi **Sectorul** în al doilea, filtrat pe sursa aleasă. Perechea e chiar codul `SursaSector` din `DefaSursaSector`. Când o sursă are un singur sector (`03`, `04`, `05`, `08`), acesta se alege singur.

`/sursasector` întoarce de acum și `sursa` și `sector` separat, nu doar codul, fiindcă tabela le are ca **coloane** (`Sursa`, `Sectorul`); spargerea codului în două rămâne doar ca rezervă, pentru rânduri vechi.

**Perechea se verifică la «Continuă».** Cerut explicit de operator: dacă perechea aleasă nu are rând în `DefaSursaSector`, ecranul o refuză și nu merge mai departe. Cum ambele liste se construiesc din aceleași rânduri, o pereche fără rând nu e de atins prin interfață — e o plasă de siguranță pentru cazul în care combo-ul al doilea ar rămâne cu o alegere veche după schimbarea sursei. Serverul refuză oricum aceeași situație cu `SS_NECUNOSCUT`.

#### Restul

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

#### A doua rundă de schimbări (operator, 22.09.2026, după încercarea pe server)

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

### Fișiere atinse

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

### Rezultatele testelor

Verificat **în browser**, pe un ciot din biblioteca standard care servește fișierele **adevărate** din `PYTHON/static` cu aceleași antete și mimează cele șapte rute (în scratchpad-ul sesiunii: `inregistrare_stub.py`; `.claude/launch.json` îl pornește pe `8765`). Nimic nu a atins Flask-ul adevărat, MariaDB sau ANAF.

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

### Neverificat / amânat

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

---

<a id="slice-0075-05"></a>

## SLICE-0075-05 — pagina operatorului: aprobarea cererilor

Data: 23.09.2026. Plan: `docs/PLAN_AutoProvisioning.md` §7, D10, D25. Cerută de operator după prima aprobare reușită din linia de comandă («let's do the next part of this slice»), cu o cerință nouă venită în timpul lucrului: **«the page should allow ME to edit the name of the database, before confirming it!»**

### Decizii (operator, 23.09.2026)

- **Autentificare: parola K-BOT + cod pe e-mail**, iar adresa trebuie să fie în `OPERATORI`
  din `config.py`. Închide întrebarea lăsată deschisă de plan la §7 («Decide in 0075-05»).
- **Numele bazei se poate schimba pe pagină înainte de aprobare**, pe lângă `CodProgram`
  (D25). Ținut la aceleași reguli ca numele calculat — vezi mai jos de ce nu e o regulă de stil.

### Ce s-a construit

| Fișier | Ce |
|---|---|
| `PYTHON/routes/inregistrare/operator.py` | **nou** — blueprint `operator_bp`: 11 rute API + `GET /operator` |
| `PYTHON/static/operator.html` | **nou** — pagina: autentificare (2 panouri), listă, detaliu, job |
| `PYTHON/static/js/operator/operator.js`, `api.js` | **noi** — logica paginii și clientul (`X-Operator-Token`) |
| `PYTHON/static/css/operator.css` | **nou** — peste `inregistrare.css` |
| `PYTHON/routes/inregistrare/provizionare.py` | `plan()`/`approve()` primesc `db_name`; `_choose_name()`; `reject()` nou |
| `PYTHON/scripts/aproba_cerere.py` | `--nume-baza`; întrebarea DA numește baza |
| `PYTHON/routes/auth/mailer.py` | `send_operator_code`, `send_registration_rejected`, `operator_page_link`; anunțul către operator poartă linkul paginii |
| `PYTHON/routes/auth/auth.py` | nume publice `verify_operator` / `log_action` (aceleași funcții, nu copii) |
| `PYTHON/main.py` | `operator_bp` înregistrat |
| `PYTHON/config.py` (local, în `.gitignore`) | `OPERATORI = []` |
| `.claude/launch.json` | `operator-stub` (port 8767, ciot din scratchpad-ul sesiunii) |

**Nimic nou în SQL.** Contul de provizionare are deja `SELECT, UPDATE` pe `FX_Inregistrari` (lista, detaliul, respingerea), iar nomenclatoarele se citesc cu contul de serviciu, ca pe pagina publică.

### Cum merge

**Autentificarea.** `/login` verifică parola printr-o conectare MariaDB CA operatorul (aceeași funcție ca `LoginForm`), apoi `OPERATORI`, apoi trimite codul; răspunde cu un token `pending` (o notă de 10 minute). `/verifica` schimbă `pending` + cod pe tokenul de sesiune. Parola bună a unui cont care NU e operator primește `NU_ESTE_OPERATOR` și se numără la limitator ca un eșec. Sesiunea e o notă în `STORE` sub numele `operator_session`, în antetul `X-Operator-Token` — **nu** sesiunea K-BOT (aceea e legată de o bază, iar rutele de date o cred pe cuvânt). Pagina ține tokenul în `sessionStorage`: o filă închisă = sesiune închisă.

**Detaliul** cheamă `provizionare.plan()` pentru cererile `InAsteptare`/`Esuata` și arată numele propus, `IdUnitate ≈`, `CodProgram` (editabile), numărul de rânduri — sau motivul pentru care nu se poate aproba. Denumirea ANAF apare lângă cea scrisă de solicitant numai când diferă. Codurile F și E sunt în liste pliabile, cu denumiri.

**Numele bazei.** Câmp pe pagină, precompletat cu propunerea, buton «Propunerea» care o pune la loc. «Verifică din nou» rulează `plan()` cu valorile de pe ecran. La «Aprobă» pagina trimite **numele de pe ecran** — jobul construiește exact ce a confirmat operatorul, nu un nume recalculat între timp. `_choose_name()` cere: forma `^1[1-9]{2}_[A-Z]{4}$`, un număr pe care nu-l folosește altă bază (`SCHEMATA` + `CAI.DbName`, aceeași regulă ca la calcul), baza să nu existe. **Forma nu e o preferință:** contul de provizionare poate crea numai baze care se potrivesc tiparului `1__\_____`, iar `proc_Provizionare_Grant` refuză orice alt nume. Un nume greșit e refuzat **înainte** de pasul 2 și **nu** trece cererea în `Esuata`.

**Aprobarea** pornește un fir (`threading.Thread`, daemon) și răspunde `202 {job}`; pagina citește `/joburi/<job>?de_la=N` la 0,8 s și afișează rândurile (erorile roșu, anulările galben). Un al doilea «Aprobă» pe aceeași cerere ▸ `409 JOB_IN_CURS`; două cereri diferite nu se pot încurca oricum (`GET_LOCK('kbot_provizionare')`). Joburile încheiate stau o oră.

**Respingerea** cere motiv (5–2000 de caractere), ia același lacăt ca aprobarea (nu poate cădea în mijlocul unei aprobări din linia de comandă), scrie `Respinsa` + `Motiv`, apoi trimite motivul solicitantului. E-mailul picat nu desface respingerea — pagina spune «NU a plecat. Anunțați solicitantul altfel».

**Linkul nou** (cereri `Aprobata`): starea linkului e pe pagină (valabil până la…, expirat, parola aleasă); dacă e-mailul nu pleacă, linkul apare pe pagină, de dat de mână.

Totul ajunge în `Jurnal`: `OPERATOR_LOGIN`, `OPERATOR_AUTH_FAIL`, `OPERATOR_DENIED`, `CERERE_APROBARE_START`, `CERERE_APROBATA` / `CERERE_APROBARE_ESUATA`, `CERERE_RESPINSA`, `CERERE_LINK_NOU`.

### Verificat

- Importurile (blueprint cu 12 reguli), Regula 0 prin `tokenize` pe cele cinci fișiere
  Python și pe comentariile JS/CSS: **zero** diacritice în afara textelor pentru ecran.
  (`import main` pică local numai pe `pandas`, lipsă din venv — fără legătură.)
- **În browser, pe un ciot** care montează `operator_bp` și `operator.html` **adevărate**,
  cu baza, e-mailul, `plan/approve/reject/link_nou` mimate:
  parolă greșită / bună, cod greșit / bun, filtrele și numerele lor, detaliul (tabelul SS cu
  `IdUnitate ≈` și `CodProgram`, codurile F/E), numele cu formă greșită (oprit pe pagină),
  numărul ocupat (refuzul serverului, «Aprobă» dezactivat, reactivat la o editare), numele
  propriu `115_TRND` acceptat, **corpul ajuns la job: `115_TRND` și `02E=ABC123`, exact ce s-a
  tastat**, jobul urmărit până la capăt, cererea redeschisă ca `Aprobata`; cererea eșuată
  cu motivul și refuzul planului; textul ANAF cu `<b>` afișat ca text; respingerea (motiv prea
  scurt, apoi reușită); linkul nou cu e-mail picat (linkul pe pagină); ieșirea; un token mort
  la reîncărcare ▸ înapoi la autentificare cu «Sesiunea a expirat».
- Telefonul la 375 px: nimic nu iese din pagină; tabelele se derulează în cutia lor.

### NU e verificat

- **Nimic viu**: nicio rută n-a atins un Flask pe VPS, un MariaDB sau SMTP-ul adevărat.
- Firul de aprobare sub gunicorn `gthread` (în ciot a rulat sub serverul Flask).
- ⚠ **O repornire a gunicorn în timpul unei aprobări** omoară firul fără anulare. Același
  risc îl are linia de comandă la Ctrl+C; nu e nou, dar pagina îl face mai ușor de atins.
- `RedisSessionStore` cu notele operatorului (numai backend-ul `memory` e folosit azi).

### Pe server (23.09.2026)

Pusă pe VPS de operator: **pagina merge**. Tot atunci, verificarea 7 din §9 a trecut pentru cererea 1 și 0075-06 a fost făcută.

### De făcut de operator pe VPS (făcut)

1. Fișierele de mai sus (în afară de `config.py` local și `launch.json`) ▸ `/root/AVACONT`.
2. În `config.py`: `OPERATORI = ["adresa-ta@..."]` — un cont K-BOT existent, litere mici.
3. Repornirea gunicorn, apoi `https://kbot.avatarsoft.ro/operator`.

## Repere unice din registrul 0075

Focusul consemna trei dumpuri din 22.09.2026, 19:21: AVACONT_COMUN 23 de tabele, AVACONT_SURSA 44, 000_DEMO 43, folder gitignorat la linia 507. Sunt repere istorice despre copii locale, fără recitirea dumpurilor acum. Cele două `Unitati` aveau sensuri distincte: comună `(DC, NumeUnitate, CF)` pentru login, locală `(IdUnitate, Detalii, SursaSector, An, CodProgram, Ascuns)` pentru clasificații; jobul le scria pe amândouă. Configul de provizionare putea avea și `PUBLIC_BASE_URL` opțional. Registrul cerea inițial completarea gazdei și parolei; jurnalul 03 a stabilit ulterior localhost prin socket. Registrul preciza că joburile în memorie și pagina funcționează cu un singur worker gunicorn.

Confirmările operatorului din 23.09.2026 închideau pagina web publicată, verificarea 7 (§9: parolă prin link, LoginForm, SHOW GRANTS numai pentru 111_TRND) și 0075-06 pe server; detaliile reparației conturilor vechi nu au fost notate aici. Aceste confirmări nu certifică toate ramurile de anulare sau toate probele planului.
