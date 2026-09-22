# `routes/inregistrare/` — înregistrarea publică a unei unități noi

Felia **0075**. Plan: `docs/PLAN_AutoProvisioning.md`. Trecerile de până acum:
**0075-01** (§5.1–5.3) — magazinul dinainte de autentificare, proxy-ul ANAF, verificarea
adresei prin cod; **0075-02** (§5.4–5.5) — nomenclatoarele, numele bazei, `FX_Inregistrari`
și `/cerere`.

Toate rutele de aici sunt **publice** — pagina o poate deschide oricine de pe internet.
Deci: limitatorul pe fiecare rută, numai SQL parametrizat, cod-motiv lângă fiecare mesaj
românesc, nicio excepție înghițită.

## Modulele

| Fișier | Ce ține |
|---|---|
| `store.py` | Înregistrarea dinaintea autentificării: o *notă* pe `session_store.STORE`, sub un token propriu. Fără cod nou de backend (constatarea F5 a planului). |
| `anaf.py` | Apelul către ANAF, cu `urllib` din bibliotecă standard. Pur: nu atinge baza de date, nu importă `config.py` decât leneș. |
| `nume.py` | Numele bazei, `1nn_SSSS` (D3/D12). Pur, în afară de `used_prefixes`, care citește două liste. Fără nicio diacritică în fișier — pliere prin `unicodedata`. |
| `nomenclatoare.py` | Cele trei liste din care alege pagina. Primesc o conexiune deschisă, întorc rânduri simple. |
| `cerere.py` | Verificarea cererii trimise și scrierea rândului în `FX_Inregistrari`. |
| `inregistrare.py` | Toate rutele. Singurul fișier care are nevoie de `config.py` (prin `utils.database`). |

`__init__.py` **nu** importă blueprint-ul, ca `anaf.py` și `store.py` să rămână
importabile pe o mașină fără `config.py` (același motiv ca la `routes/migrare/`).
`main.py` îl ia explicit: `from routes.inregistrare.inregistrare import inregistrare_bp`.

## Rutele

| Rută | Corp | Răspuns |
|---|---|---|
| `POST /api/inregistrare/anaf` | `{cf}` | `{token, expires_in, cf, unitate:{cui,denumire,adresa,nr_reg_com}}` |
| `POST /api/inregistrare/cod` | `{email}` | `{email_masked, expires_in}` |
| `POST /api/inregistrare/verifica` | `{cod}` | `{ok, email_masked, expires_in}` |
| `GET /api/inregistrare/sursasector` | — | `{surse:[{cod,denumire}]}` |
| `GET /api/inregistrare/clasificatii?tip=F\|E` | — | `{tip, coduri:[{cod,denumire}]}` |
| `GET /api/inregistrare/nume?denumire=` | — | `{db_name, numar, litere, previzualizare}` |
| `POST /api/inregistrare/cerere` | `{denumire, an, sursasector[], clsf_f[], clsf_e[]}` | `{id_cerere, randuri, operator_anuntat}` |

Toate, în afară de `/anaf`, cer token-ul înapoi în antetul **`X-Registration-Token`**.
Antet, nu corp, fiindcă trei dintre ele sunt GET-uri.

## Numele bazei — `1nn_SSSS` (D3, D12)

`SSSS` = primele patru consoane ale denumirii; dacă nu sunt patru, vocalele umplu restul,
în ordinea din nume; sub patru litere cu totul ▸ refuz. `nn` = cea mai mică pereche liberă
din `11..99` **fără cifra zero**, liber însemnând că nici `information_schema.SCHEMATA`,
nici `CAI.DbName` nu au deja un nume care începe cu acel `1nn_`.

De ce fără zero: toate bazele existente sunt `NNN_XXXX` numerotate de la `000` în sus
(`000_DEMO`, `001_GR23`, `053_LTTR`, plus `101_CCDP`, care e dinaintea regulii). Blocul
`111`–`199` nu poate fi atins de numerotarea veche, deci un nume ales singur de server nu
poate cădea peste unul pe care operatorul urmează să îl scrie de mână.

Verificat prin apel direct: `AVATAR SOFT SRL` ▸ `VTRS`, `LICEUL TEORETIC` ▸ `LCLT`,
`Direcția de Asistență Socială` ▸ `DRCT`, `MUN. PLOIEŞTI` ▸ `MNPL` (ambele grafii de
diacritice se pliază), `AEIOU` ▸ `AEIO`, `ANA` ▸ refuzat.

`/nume` e o **previzualizare**. Numele adevărat se recalculează la aprobare (§5.4): un
număr liber acum poate fi luat până ajunge operatorul la cerere.

## De ce numai `ClsfE` e filtrat

`Clasificatii` are patru chei străine spre `AVACONT_COMUN`, iar două sunt pe valori
**derivate** din codul E: `Articol = Left(E,2) + "." + Mid(E,3,2)` și `Titlu = Left(E,2)`.
Cincisprezece coduri din `DefaClsfE` derivă spre un `Articol` sau un `Titlu` inexistent
(numărate pe server: opt fără Articol, patru fără Titlu). Oferite în arbore, ar fi frunze
care nu pot fi salvate — solicitantul alege una, așteaptă aprobarea, iar provizionarea moare
cu `1452` la ultimul pas.

Deci filtrul **este chiar join-ul**: un cod apare doar dacă rândul în care s-ar transforma
își găsește ambii părinți. Așa lista nu se poate îndepărta de constrângere — dacă mâine se
adaugă rândurile lipsă din dicționare, codurile apar singure, fără o listă de cincisprezece
excepții care s-ar învechi în tăcere. `DefaClsfF` nu are nevoie de filtru: `ClsfF` e propria
lui cheie străină.

### De ce token-ul se naște la `/anaf`, nu la `/cod`

Planul se contrazice singur: §4 spune «token-ul de la pasul 2», dar §5.1 pune `cf` și
`anaf` printre câmpurile notei — date de la pasul 1. Operatorul a tranșat (22.09.2026):
token-ul vine de la `/anaf`.

Motivul e `FX_Inregistrari`, care are **două** coloane de denumire: `DenumireAnaf` (ce a
spus ANAF) și `Denumire` (ce a scris solicitantul, care **are voie** să o schimbe). Pagina
de aprobare le arată una lângă alta. Dacă denumirea de la ANAF ar călători prin browser,
pagina de aprobare ar compara ce a scris solicitantul cu ce a scris solicitantul. La fel
și codul fiscal: D11 refuză un CF care are deja bază — ținut în browser, cineva ar putea
trece verificarea la pasul 1 cu un cod liber și trimite altul la pasul 6.

## Codurile-motiv

ASCII, stabile, pentru ca pagina să poată ramifica fără să citească textul.

`CF_INVALID` · `CF_EXISTS` · `ANAF_NOT_FOUND` · `ANAF_INCOMPLETE` · `ANAF_UNAVAILABLE` ·
`RATE_LIMITED` · `DB_ERROR` · `TOKEN_ABSENT` · `TOKEN_UNKNOWN` · `TOKEN_EXPIRED` ·
`EMAIL_INVALID` · `EMAIL_TOO_LONG` · `EMAIL_TAKEN` · `MAIL_NOT_CONFIGURED` ·
`MAIL_FAILED` · `CODE_ABSENT` · `CODE_NOT_REQUESTED` · `CODE_EXPIRED` · `CODE_WRONG` ·
`CODE_ATTEMPTS_EXHAUSTED` · `TIP_INVALID` · `DENUMIRE_ABSENTA` · `DENUMIRE_PREA_SCURTA` ·
`NUMAR_EPUIZAT` · `EMAIL_NEVERIFICAT` · `DENUMIRE_PREA_LUNGA` · `AN_INVALID` ·
`AN_IN_AFARA_INTERVALULUI` · `SS_ABSENT` · `SS_NECUNOSCUT` · `CLSF_F_ABSENT` ·
`CLSF_E_ABSENT` · `CLSF_F_NECUNOSCUT` · `CLSF_E_NECUNOSCUT` · `CLSF_F_NU_E_FRUNZA` ·
`CLSF_E_NU_E_FRUNZA` · `PREA_MULTE_RANDURI`

## Config pe VPS

Toate au valori implicite în cod (sau lipsa lor e raportată, nu e eroare), deci serverul
pornește și fără ele:

```python
ANAF_TVA_URL   = "https://webservicesp.anaf.ro/api/PlatitorTvaRest/v9/tva"
ANAF_TIMEOUT   = 15
OPERATOR_EMAIL = "..."   # cui i se anunță o cerere nouă; gol ▸ nu se anunță nimeni
```

`SMTP_*` există deja, de la felia 0072.

## Tabela

`sql/0075_fx_inregistrari.sql` creează `AVACONT_COMUN.FX_Inregistrari` pe serverul K-BOT.
**Fără chei străine, intenționat:** o cerere e o corespondență, nu o unitate — există
înaintea bazei, a rândului din `CAI` și a contului, se poate sfârși `Respinsa` fără să arate
spre nimic, iar o rulare eșuată trebuie să lase rândul în urmă cu `Motiv`-ul intact, nu să
fie luat de o cascadă odată cu resturile.

## Plafonul de rânduri — adăugat, nu din plan

O cerere devine un rând `Clasificatii` per frunză F × frunză E × sursă-sector. Exemplul
planului e 20 × 40 × 2 = 1 600. Aritmetica nu se oprește acolo: toate frunzele ambilor
arbori și toate cele 14 surse ar însemna 531 × 686 × 14 — cinci milioane de rânduri, puse la
coadă dintr-un formular anonim. `cerere.MAX_ROWS` (50 000) e paza. **Nu e din plan**, e o
constantă și operatorul o poate muta.

## Durate

- Înregistrarea: **30 de minute**, absolut, de la căutarea ANAF. Fiecare scriere pune nota
  la loc cu **cât a mai rămas**, niciodată cu 30 de minute noi — fereastra nu poate fi
  plimbată înainte cerând cod după cod.
- Codul: **10 minute**, cel mult **5** încercări greșite (ca la felia 0072). Un cod nu poate
  trăi mai mult decât înregistrarea care îl poartă, iar `expires_in` spune numărul adevărat.

## Ce nu e verificat

- **Nicio rută n-a fost pornită.** Nimic de aici n-a atins un Flask viu sau un MariaDB.
- **Coloana de captiune a lui `DefaSursaSector`** nu e cunoscută: nimic din depozit nu face
  join pe tabela asta, iar cele 14 valori s-au citit ca simple coduri. De aceea
  `read_sursasector` face `SELECT *` și ia prima coloană de captiune pe care o găsește
  (`Denumire`, `Explicatie`, `Descriere`); dacă nu există niciuna, codul își ține loc de
  etichetă. O listă de 14 coduri face mai mult decât un 500 pe un nume de coloană.
- **Limita de apeluri a ANAF pe v9** nu e cunoscută. Un apel per înregistrare e mult sub
  orice cifră publicată, dar numărul în sine n-a fost confirmat.
- **v9 nu are câmpul `cod`.** Funcția din Access decide «negăsit» după `cod <> "200"`;
  răspunsul real de pe 22.09.2026 avea exact două chei de nivel întâi, `found` și
  `notFound`. Aici se citește `found` gol. Diferența e intenționată.
