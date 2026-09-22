# SLICE-0075-02 — nomenclatoare, numele bazei, `FX_Inregistrari`, `/cerere`

A treia trecere din felia 0075 (`docs/PLAN_AutoProvisioning.md`). Acoperă **§5.4–5.5** din
plan, plus împrospătarea lui `sql/avacont_comun_login.sql` cerută de §8.

## Ce s-a schimbat și de ce

Restul paginii publice are acum server. Patru rute noi, toate în spatele token-ului de
înregistrare:

| Rută | Corp / parametri | Răspuns |
|---|---|---|
| `GET /api/inregistrare/sursasector` | — | `{surse:[{cod,denumire}]}` |
| `GET /api/inregistrare/clasificatii` | `?tip=F\|E` | `{tip, coduri:[{cod,denumire}]}` |
| `GET /api/inregistrare/nume` | `?denumire=` | `{db_name, numar, litere, previzualizare}` |
| `POST /api/inregistrare/cerere` | `{denumire, an, sursasector[], clsf_f[], clsf_e[]}` | `{id_cerere, randuri, operator_anuntat}` |

### Numele bazei (§5.4, D3/D12) — `nume.py`

`SSSS` = primele patru consoane; sub patru consoane, vocalele umplu restul în ordinea din
nume; sub patru litere cu totul ▸ refuz, nu umplutură inventată. `nn` = cea mai mică pereche
liberă din `11..99` **fără cifra zero**.

**De ce fără zero.** Toate bazele existente sunt `NNN_XXXX`, numerotate de la `000` în sus
(`000_DEMO`, `001_GR23`, `053_LTTR`, plus `101_CCDP`, dinaintea regulii). Blocul `111`–`199`
nu poate fi atins de numerotarea veche, deci un nume ales singur de server nu poate cădea
peste unul pe care operatorul urmează să îl scrie de mână.

**Ce se compară e NUMĂRUL, nu numele întreg.** §5.4 spune «perechea liberă… absentă din
`SCHEMATA` și din `CAI.DbName`», fără să spună ce anume se caută acolo. Rândurile existente
poartă fiecare un `NNN` distinct, ceea ce se citește ca număr de unitate, nu ca
dezambiguizator — deci `1nn` e liber când **niciun** nume din cele două liste nu începe cu
`1nn_`. Citirea e scrisă ca atare în cod; dacă operatorul o vrea altfel, e o linie.

**Zero diacritice în fișier**, și fără tabel de cazuri speciale: plierea trece prin
`unicodedata` — NFD desparte litera de semnele ei, iar aruncarea semnelor lasă baza. Acoperă
ambele grafii ale românei (virgulă dedesubt **și** sedilă, cea pe care o trimite ANAF), plus
ă/â/î.

Verificat prin apel direct (nu fișier de test): `AVATAR SOFT SRL` ▸ `VTRS`,
`LICEUL TEORETIC` ▸ `LCLT`, `Direcția de Asistență Socială` ▸ `DRCT`,
`MUN. PLOIEŞTI` ▸ `MNPL`, `Scoala Gimnaziala Nr. 1` ▸ `SCLG`, `AEIOU` ▸ `AEIO` (zero
consoane, umplut cu vocale), `ANA` și `IAE` ▸ refuzate. Primele numere: `111…119`, apoi
`121`; cu `111`/`112` ocupate ▸ `113`. `db_name('111','VTRS')` ▸ `111_VTRS`.

`/nume` e **previzualizare** și o spune în răspuns (`previzualizare: true`): numele adevărat
se recalculează la aprobare, fiindcă un număr liber acum poate fi luat până ajunge operatorul
la cerere.

### Nomenclatoarele — `nomenclatoare.py`

**De ce numai `ClsfE` e filtrat.** `Clasificatii` are patru chei străine spre `AVACONT_COMUN`
și două sunt pe valori **derivate** din codul E: `Articol = Left(E,2) + "." + Mid(E,3,2)` și
`Titlu = Left(E,2)`. Cele cincisprezece coduri pe care §2a le-a numărat pe server (opt fără
`Articol`, patru fără `Titlu`) derivă spre părinți inexistenți. Oferite în arbore, ar fi
frunze care nu pot fi salvate: solicitantul alege una, așteaptă aprobarea, iar provizionarea
moare cu `1452` la ultimul pas.

Deci filtrul **este chiar join-ul** — un cod apare doar dacă rândul în care s-ar transforma
își găsește ambii părinți. Așa lista nu se poate îndepărta de constrângere: dacă mâine se
adaugă rândurile lipsă, codurile apar singure, fără o listă de cincisprezece excepții care
s-ar învechi în tăcere. `DefaClsfF` nu are nevoie de filtru — `ClsfF` e propria lui cheie
străină, deci orice rând al dicționarului e prin definiție inserabil.

`GROUP BY`, nu `DISTINCT`: §2a spune că `DefaClsfF` **nu are cheie primară**, `ClsfF` e
nullable și indexul nu e unic — două rânduri cu același cod și captiuni diferite ar trece
amândouă printr-un `DISTINCT` și arborele ar arăta codul de două ori. Plus D16: un rând
`DefaClsfE` fără captiune nu se arată.

`read_codes` / `read_sursasector_codes` folosesc **aceleași** interogări ca listele, ca ce se
validează la `/cerere` să fie exact ce s-a oferit. Un check construit dintr-o altă interogare
e felul în care o pagină ajunge să ofere un cod pe care propriul ei server îl refuză apoi.

### Cererea — `cerere.py` + `sql/0075_fx_inregistrari.sql`

Totul se verifică din nou, pe server, din dicționarele citite **în acel moment**. Motivul
mai probabil nu e un client ostil, ci fereastra de 30 de minute: un cod oferit la minutul doi
poate să nu mai existe la minutul douăzeci. Re-citirea transformă asta într-un refuz limpede
acum, nu într-o cheie străină picată în timpul provizionării, după ce baza e deja construită.

**Ce înseamnă «frunză» (§4.1).** `xx0000` = rădăcină, niciodată aleasă singură; `xxyy00` =
nivel 1, aleasă **doar** dacă nimic nu atârnă sub ea; `xxyyzz` = frunză, mereu. Cazul din
mijloc merită spus: datele reale au `650500` și `590100`, coduri de nivel 1 pe care nimeni nu
le-a subîmpărțit, și sunt rânduri legitime. Deci «e frunză?» nu se poate răspunde din cod —
are nevoie de tot dicționarul, de aceea verificarea e aici și nu în pagină.

D11 se verifică **încă o dată** la `/cerere` (planul o cere): jumătate de oră e destul ca
operatorul să fi creat unitatea de mână între timp.

`FX_Inregistrari`: **fără chei străine, intenționat.** O cerere e o corespondență, nu o
unitate — există înaintea bazei, a rândului din `CAI` și a contului, se poate sfârși
`Respinsa` fără să arate spre nimic, iar o rulare eșuată trebuie să lase rândul în urmă cu
`Motiv`-ul intact, nu să fie luat de o cascadă odată cu resturile.

Nota către operator: dacă `OPERATOR_EMAIL` lipsește sau trimiterea pică, **cererea rămâne
înregistrată** — se scrie un avertisment în log și răspunsul poartă `operator_anuntat: false`.
Niciodată o eroare care ar invita solicitantul să trimită tot a doua oară.

La reușită, nota de înregistrare se șterge: altfel același token ar putea depune o a doua
cerere pe o adresă deja dovedită.

### Plafonul de rânduri — **adăugat de mine, nu din plan**

O cerere devine un rând `Clasificatii` per frunză F × frunză E × sursă-sector, iar exemplul
planului e 20 × 40 × 2 = 1 600. Aritmetica nu se oprește acolo: toate frunzele ambilor arbori
și toate cele 14 surse ar însemna 531 × 686 × 14 — **cinci milioane** de rânduri, puse la
coadă dintr-un formular anonim. `cerere.MAX_ROWS = 50 000` e paza. Nu e cerut de plan; e o
constantă, se mută dintr-o linie. Semnalat aici fiindcă e o decizie de proiectare luată de
mine, nu una a operatorului.

### `sql/avacont_comun_login.sql`

Operatorul a spus (22.09.2026) că DDL-urile din depozit sunt din schema veche și nu merită
efort. Deci s-a făcut minimul onest: un antet care spune apăsat că **fișierul e referință
învechită, nu serverul**, plus cele două corecturi pe care §2a le-a verificat pe mașină —
cheia primară e `IdCai` AUTO_INCREMENT (nu `IdUnitate`), iar `IdUnitate` e coloană simplă,
neunică. «Întrebarea deschisă» de la coada tabelei e acum **răspunsă**: o unitate are un rând
`CAI` per sursă-sector, deci mai multe rânduri împart și `DbName` și `IdUnitate` — de asta
§5.6 pasul 3 are nevoie de `GET_LOCK` explicit.

## Fișiere atinse

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

## Rezultatele testelor

- `ast.parse` pe toate fișierele Python atinse: **curat** (5/5).
- **Regula 0 verificată prin AST**, nu prin căutare: s-au adunat intervalele de linii ale
  tuturor literalilor de tip șir din cele șapte fișiere ale feliei și s-a căutat orice
  diacritică **în afara** lor ▸ **zero**. `nume.py` și `nomenclatoare.py` n-au nicio
  diacritică deloc; cele 15 linii din `cerere.py` sunt toate mesaje pentru solicitant.
- Algoritmul numelui verificat prin apel direct (valorile de mai sus).
- **Fără fișiere de test** (decizia operatorului). Acoperire automată: **zero**.
- **Nicio rută n-a fost pornită.** Nimic n-a atins un Flask viu, un MariaDB sau ANAF.

## Neverificat / amânat

- **Coloana de captiune a lui `DefaSursaSector`.** Nimic din depozit nu face join pe tabela
  asta, iar cele 14 valori s-au citit ca simple coduri, deci numele coloanei de captiune
  **nu e cunoscut**. `read_sursasector` face `SELECT *` și ia prima dintre `Denumire`,
  `Explicatie`, `Descriere` pe care o găsește; fără niciuna, codul își ține loc de etichetă și
  se scrie un avertisment în log. De lămurit la prima rulare adevărată.
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
