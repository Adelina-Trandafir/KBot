# SLICE-AD13-01 — Încărcare extrase bancare în ADECHIT (fișiere/folder și e-mail)

Data: 26.10.2026. Stare: **cod scris, compilat sintactic; nimic rulat, nimic văzut pe ecran, SQL nerulat.**

## Ce s-a schimbat și de ce

Asocierea extraselor cu copiii (AD_11) citea doar ce adusese K-BOT/FOREXE în `FX_Extrase`. Operatorul
ADECHIT nu avea cum să aducă un extras singur. Două intrări noi, în meniul **Încărcare extrase bancare**:

1. **Fișiere PDF sau un folder întreg**, cu deduplicare.
2. **Căsuță de e-mail** (IMAP: Gmail, Yahoo, Outlook sau alt server public). Parola se salvează criptat pe
   server (Fernet) și se schimbă din fereastră. «Verifică e-mailul acum» citește mesajele noi și ia
   atașamentele PDF al căror nume conține «ExtrasEP».

Ambele folosesc **același cititor ca K-BOT**: XML-ul din PDF se desface cu un port Python al
`PdfXmlExtractor` (fără bibliotecă PDF), apoi intră prin aceeași scriere în `FX_Extrase_F/H/Extrase`.
Asocierea existentă vede rândurile fără niciun pas în plus; ferestra are butonul «Asociază plăți cu extrase…».

Deduplicare pe trei straturi: SHA-256 al PDF-ului (`AD_EXTRASE_INCARCARI`); numele fișierului + data
extrasului, apoi cheia fiecărei operațiuni (ca la importul FOREXE); un fișier cu alt nume ale cărui
operațiuni există TOATE deja nu mai scrie nici antet gol (parametrul nou `sari_continut_cunoscut`).

Decizii:
- E-mailul se citește **doar la cerere** (buton), nu pe timp. Nu există planificator în aplicația Flask.
- Cutia poștală e **a unității (DC)**, ca și rândurile de extras, nu a subunității.
- Nu se modifică nimic în cutie (deschisă read-only, `BODY.PEEK`). Locul rămas se ține pe UID + UIDVALIDITY;
  prima rulare pornește de la data din setări (implicit azi − 30 zile).
- Parola salvată nu se trimite niciodată altui server: la schimbarea serverului sau a adresei se cere parola din nou.
- Serverul de e-mail trebuie să aibă adrese publice (anti-SSRF), doar IMAP peste TLS, port 993.
- Drepturi: aceleași ca la asociere (`collect` + `catalog`).
- Rutele noi nu țin `AD_Lock(0)` cât răspunde căsuța: `guard(..., plain=True)` (fără tranzacție ADE);
  importurile aceleiași unități se serializează cu `GET_LOCK`.

## Fișiere

Noi: `PYTHON/routes/adechit/extras_pdf.py`, `extras_mail.py`, `extras_intake.py`, `extras_routes.py`;
`sql/AD_14_extrase_incarcare.sql`; `PYTHON/static/js/adechit/extracts-intake.js`;
`PYTHON/static/css/adechit-intake.css`.
Modificate: `PYTHON/routes/forexe/extrase.py` (corpul rutei de import mutat în `importa_extrase`, același
comportament și același răspuns pentru robot; adăugat `rezultate` per fișier doar pentru ADECHIT),
`PYTHON/routes/adechit/__init__.py` (`guard(plain=)`, înregistrare rute), `PYTHON/static/adechit.html`,
`PYTHON/static/js/adechit/app.js` (meniu, `api()` acceptă FormData), `PYTHON/static/css/adechit.css`,
`PYTHON/static/help/adechit.json`, `ADECHIT/UTILIZARE_WEB.md`.

## Rezultate verificări

Doar `py_compile` pe cele 6 module Python și `JSON.parse` pe ajutor. Niciun test scris sau rulat, nicio
conexiune la MariaDB sau la un server de e-mail, nicio redare în browser.

## Neverificat / amânat — de făcut de operator pe server

1. Rulați `AD_14_extrase_incarcare.sql` în fiecare bază ADECHIT (după AD_11). Fără el fereastra spune ce lipsește.
2. Serverul are nevoie de pachetul `cryptography` și de cheia `ADE_MAIL_KEY` (sau se folosește `EF_TOKEN_KEY`
   dacă e singura) în fișierul de mediu al serviciului. `config.py` nu se atinge.
3. Publicați: `PYTHON/routes/adechit/*`, `PYTHON/routes/forexe/extrase.py`, `PYTHON/static/*` de mai sus, apoi restart.
   Robotul FOREXE folosește aceeași rută de import: o rulare de probă a descărcării confirmă că nu s-a schimbat nimic.
4. Neprobat cu PDF-uri reale: cititorul PDF e un port, nu a fost comparat cu K-BOT pe același fișier. Un PDF care
   se deschide în K-BOT și apare «Fără XML» aici = defect de raportat (cu fișierul).
5. Neprobat cu Gmail/Yahoo: BODYSTRUCTURE și numele atașamentelor codate neobișnuit pot scăpa; un atașament
   pierdut se încarcă manual.
6. Aspectul ferestrei (PC/mobil, temă întunecată) nevăzut.
