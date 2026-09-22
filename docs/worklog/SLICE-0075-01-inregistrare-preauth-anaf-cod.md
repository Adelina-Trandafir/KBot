# SLICE-0075-01 — înregistrare: magazinul dinainte de autentificare, ANAF, `/cod`, `/verifica`

A doua trecere din felia 0075 (creare automată a unei baze de unitate prin pagină publică +
aprobarea operatorului, `docs/PLAN_AutoProvisioning.md`). Acoperă **§5.1–5.3** din plan.

## Ce s-a schimbat și de ce

Primele două ecrane ale paginii publice au acum server: codul fiscal (căutat la ANAF) și
adresa de e-mail (dovedită printr-un cod de șase cifre). Un blueprint nou,
`PYTHON/routes/inregistrare/`, cu trei rute publice:

| Rută | Corp | Răspuns |
|---|---|---|
| `POST /api/inregistrare/anaf` | `{cf}` | `{token, expires_in, cf, unitate:{cui,denumire,adresa,nr_reg_com}}` |
| `POST /api/inregistrare/cod` | `{email}` | `{email_masked, expires_in}` |
| `POST /api/inregistrare/verifica` | `{cod}` | `{ok, email_masked, expires_in}` |

Ultimele două cer token-ul în antetul **`X-Registration-Token`** — antet, nu corp, fiindcă
§5.4 (`/nume`) și rutele de nomenclator care urmează sunt GET-uri, iar un singur mod de a
purta token-ul bate două.

### Magazinul dinainte de autentificare (§5.1)

Constatarea F5 a planului s-a confirmat pe cod: `session_store.STORE.put_note / get_note /
delete_note` acceptă **orice** șir drept token și nu ating niciodată cheile de sesiune pe
care le validează paza bearer. Deci înregistrarea e **o notă**, numită `register`, pe un
token pe care `store.py` îl bate singur. **Zero cod nou de backend**, și memoria și Redis se
poartă identic — de aceea valoarea notei e numai JSON simplu (backend-ul Redis face
`json.dumps` pe ea).

Durata: **30 de minute, absolut**, de la căutarea ANAF. Fiecare scriere pune nota la loc cu
**cât a mai rămas**, niciodată cu 30 de minute noi — altfel fereastra ar putea fi plimbată
înainte la nesfârșit cerând cod după cod.

### De ce token-ul se naște la `/anaf`, nu la `/cod`

**Planul se contrazice singur**, și asta a fost lămurit cu operatorul: §4 spune «token-ul de
la pasul 2», dar §5.1 pune `cf` și `anaf` printre câmpurile notei — care sunt date de la
pasul 1. Decizia (22.09.2026): token-ul vine de la `/anaf`.

Motivul e `FX_Inregistrari`, care are **două** coloane de denumire: `DenumireAnaf` (ce a spus
ANAF) și `Denumire` (ce a scris solicitantul). Operatorul a confirmat că solicitantul **are
voie** să schimbe denumirea, iar §7 cere ca pagina de aprobare să le arate una lângă alta.
Dacă denumirea de la ANAF ar călători prin browser, pagina de aprobare ar compara ce a scris
solicitantul cu ce a scris solicitantul. La fel și codul fiscal: D11 refuză un CF care are
deja bază — ținut în browser, cineva ar putea trece verificarea la pasul 1 cu un cod liber și
trimite altul la pasul 6.

### Proxy-ul ANAF (§5.2)

Portat din `InformatiiFirmaOnline2` (modulul Access `Module__EFactura`, citit de la operator
pe 22.09.2026): POST, `Content-Type: application/json`, corp
`[{"cui": <cifre, fără ghilimele>, "data": "yyyy-mm-dd"}]`, iar un răspuns care începe cu
`<html>` înseamnă că ANAF s-a stricat, nu că nu știe codul. Verificarea aceea e păstrată
literă cu literă. `urllib` din biblioteca standard (F10: nu există `requests` în venv).

**Un lucru NU s-a portat, și contează.** Funcția din Access decide «negăsit» citind
`cod <> "200"`. **v9 nu are deloc câmpul `cod`** — verificat pe un răspuns real din
22.09.2026, care avea exact două chei de nivel întâi, `found` și `notFound`. Deci testul de
aici e `found` gol. Diferența e intenționată, nu o scăpare.

Câmpurile citite, din `found[0].date_generale`: `denumire`, `adresa`, `cui`, `nrRegCom`.
Structura `adresa_sediu_social` e lăsată în pace — șirul plat e ce recunoaște solicitantul,
și tot el ajunge pe pagina de aprobare. ANAF întoarce diacritice cu **sedilă**
(`PLOIEŞTI`, `Administraţia`), nu cu virgulă dedesubt; nimic nu le rescrie.

### Verificarea prin cod (§5.3)

Aceeași mașinărie ca schimbarea parolei din felia 0072 (`auth.py`): șase cifre din `secrets`,
se păstrează **numai** hash-ul SHA-256, comparație cu `hmac.compare_digest`, **10 minute**,
cel mult **5** încercări greșite și codul se aruncă. Singura diferență e unde stă — o notă pe
token-ul de înregistrare, nu pe o sesiune.

Un cod nu poate trăi mai mult decât înregistrarea care îl poartă, iar `expires_in` spune
numărul adevărat (`min(10 min, cât a mai rămas)`). Un cod nou îl înlocuiește pe cel vechi, iar
schimbarea adresei **dez-verifică** înregistrarea: cine a dovedit vechea cutie poștală n-a
dovedit nimic despre cea nouă.

### Cele două verificări în bază

- `/anaf` refuză un CF care are deja bază (**D11**), căutat în `CAI` **și** în
  `AVACONT_COMUN.Unitati` — F0 a arătat că listele nu coincid, iar oricare dintre ele e motiv
  de refuz. Codurile sunt ținute și goale și cu prefix `RO`, deci se întreabă **ambele
  grafii** — așa comparația rămâne pe o coloană indexată, fără `REPLACE()` în jurul ei.
- `/cod` refuză o adresă care e deja cont MariaDB **sau** apare deja în
  `Unitati_Utilizatori`, cu un mesaj care nu spune unui străin care dintre cele două a fost.
  Contul MariaDB contează fiindcă pasul 7 al provizionării ar pica la `CREATE USER` **după**
  ce baza e deja construită.

**Verificat pe mașină (22.09.2026):** contul de serviciu din `DB_CONFIG_NEW` chiar poate citi
`mysql.user` pe serverul K-BOT (`SELECT COUNT(*)` ▸ 10 conturi). Era singurul lucru care ar fi
putut obliga la altă formă a verificării.

## Fișiere atinse

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

`__init__.py` **nu** importă blueprint-ul, ca `anaf.py` și `store.py` să rămână importabile pe
o mașină fără `config.py` — același motiv scris în `routes/migrare/__init__.py`. `main.py` îl
ia explicit.

## Codurile-motiv

ASCII, stabile, ca pagina să ramifice fără să citească textul românesc:

`CF_INVALID` · `CF_EXISTS` · `ANAF_NOT_FOUND` · `ANAF_INCOMPLETE` · `ANAF_UNAVAILABLE` ·
`RATE_LIMITED` · `DB_ERROR` · `TOKEN_ABSENT` · `TOKEN_UNKNOWN` · `TOKEN_EXPIRED` ·
`EMAIL_INVALID` · `EMAIL_TOO_LONG` · `EMAIL_TAKEN` · `MAIL_NOT_CONFIGURED` · `MAIL_FAILED` ·
`CODE_ABSENT` · `CODE_NOT_REQUESTED` · `CODE_EXPIRED` · `CODE_WRONG` ·
`CODE_ATTEMPTS_EXHAUSTED`

## Config de adăugat pe VPS

Ambele au valori implicite în cod, deci serverul pornește și fără ele:

```python
ANAF_TVA_URL = "https://webservicesp.anaf.ro/api/PlatitorTvaRest/v9/tva"
ANAF_TIMEOUT = 15
```

`SMTP_*` există deja de la felia 0072.

## Rezultatele testelor

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

## Neverificat / amânat

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
