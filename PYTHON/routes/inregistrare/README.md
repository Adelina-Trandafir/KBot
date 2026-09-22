# `routes/inregistrare/` — înregistrarea publică a unei unități noi

Felia **0075**. Plan: `docs/PLAN_AutoProvisioning.md`. Trecerea de față, **0075-01**,
acoperă §5.1–5.3 din plan: magazinul dinainte de autentificare, proxy-ul ANAF și
verificarea adresei de e-mail prin cod.

Toate rutele de aici sunt **publice** — pagina o poate deschide oricine de pe internet.
Deci: limitatorul pe fiecare rută, numai SQL parametrizat, cod-motiv lângă fiecare mesaj
românesc, nicio excepție înghițită.

## Modulele

| Fișier | Ce ține |
|---|---|
| `store.py` | Înregistrarea dinaintea autentificării: o *notă* pe `session_store.STORE`, sub un token propriu. Fără cod nou de backend (constatarea F5 a planului). |
| `anaf.py` | Apelul către ANAF, cu `urllib` din bibliotecă standard. Pur: nu atinge baza de date, nu importă `config.py` decât leneș. |
| `inregistrare.py` | Cele trei rute. Singurul fișier care are nevoie de `config.py` (prin `utils.database`). |

`__init__.py` **nu** importă blueprint-ul, ca `anaf.py` și `store.py` să rămână
importabile pe o mașină fără `config.py` (același motiv ca la `routes/migrare/`).
`main.py` îl ia explicit: `from routes.inregistrare.inregistrare import inregistrare_bp`.

## Rutele

| Rută | Corp | Răspuns |
|---|---|---|
| `POST /api/inregistrare/anaf` | `{cf}` | `{token, expires_in, cf, unitate:{cui,denumire,adresa,nr_reg_com}}` |
| `POST /api/inregistrare/cod` | `{email}` | `{email_masked, expires_in}` |
| `POST /api/inregistrare/verifica` | `{cod}` | `{ok, email_masked, expires_in}` |

Ultimele două cer token-ul înapoi în antetul **`X-Registration-Token`**. Antet, nu corp,
fiindcă §5.4 (`/nume`) și rutele de nomenclator care urmează sunt GET-uri.

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
`CODE_ATTEMPTS_EXHAUSTED`

## Config pe VPS

Ambele au valori implicite în cod, deci serverul pornește și fără ele:

```python
ANAF_TVA_URL = "https://webservicesp.anaf.ro/api/PlatitorTvaRest/v9/tva"
ANAF_TIMEOUT = 15
```

`SMTP_*` există deja, de la felia 0072.

## Durate

- Înregistrarea: **30 de minute**, absolut, de la căutarea ANAF. Fiecare scriere pune nota
  la loc cu **cât a mai rămas**, niciodată cu 30 de minute noi — fereastra nu poate fi
  plimbată înainte cerând cod după cod.
- Codul: **10 minute**, cel mult **5** încercări greșite (ca la felia 0072). Un cod nu poate
  trăi mai mult decât înregistrarea care îl poartă, iar `expires_in` spune numărul adevărat.

## Ce nu e verificat

- **Limita de apeluri a ANAF pe v9** nu e cunoscută. Un apel per înregistrare e mult sub
  orice cifră publicată, dar numărul în sine n-a fost confirmat.
- **Forma căii URL.** Access-ul (`InformatiiFirmaOnline2`) cheamă
  `/PlatitorTvaRest/api/v6/ws/tva`; aici e `/api/PlatitorTvaRest/v9/tva`. Decizia
  operatorului: v9. Dacă ANAF mută calea, se schimbă `ANAF_TVA_URL` pe VPS, fără cod.
- **v9 nu are câmpul `cod`.** Funcția din Access decide «negăsit» după `cod <> "200"`;
  răspunsul real de pe 22.09.2026 avea exact două chei de nivel întâi, `found` și
  `notFound`. Aici se citește `found` gol. Diferența e intenționată.
