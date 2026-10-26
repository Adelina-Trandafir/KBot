# ADECHIT — STATUS, felii ADE10–ADE19

Detalii pentru [indexul din rădăcină](../../../ADECHIT_STATUS.md).
Actualizat: 12.10.2026.

## Slice ADE10

Subunități în același DC (plan: [plan_subunitati.md](../../../ADECHIT/plan_subunitati.md), analizat în ADE0-08/09).

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-ADE10-01 | Schema: `AD_Subunits`, `AD_ReceiptConfig`, `AD_IdMap`, `SubunitId` pe 17 tabele, chei și relații compuse, `LunaD.Ordine`, conversia DC-urilor populate | TESTAT, RULAT, PUBLICAT | [Schema](../SLICE-AD10-01-schema-subunitati.md#slice-ADE10-01) |
| SLICE-ADE10-02 | API: context DC + subunitate, repository limitat, blocare/reîncercări/chitanțe pe subunitate, import cu hartă de ID-uri | TESTAT, RULAT, PUBLICAT | [API](../SLICE-AD10-01-schema-subunitati.md#slice-ADE10-02) |
| SLICE-ADE10-03 | ADE.Migrator: subunitate nouă/existentă, ID-uri remapate, serie și contor în subunitate | TESTAT, RULAT, PUBLICAT | [Migrator](../SLICE-AD10-01-schema-subunitati.md#slice-ADE10-03) |
| SLICE-ADE10-04 | Selectorul „Subunitate", comutarea per filă cu reîncărcare | TESTAT, RULAT, PUBLICAT | [Interfață](../SLICE-AD10-01-schema-subunitati.md#slice-ADE10-04) |
| SLICE-ADE10-06 | Corecturi după review: snapshot înainte de blocare, reverificare completă în migrator, comutare fără pierderi, reconciliere, amprente vechi, acces pe utilizator, sincronizare PK | TESTAT, RULAT, PUBLICAT | [ADE10-02…05](../SLICE-AD10-01-schema-subunitati.md#slice-ADE10-02) |
| SLICE-ADE10-07 | ADE.Migrator: copii reconstituiți din SS_Buget (Platitori + Platitori_sub), fereastra «Chitanțe dublate…» cu numere editabile, reverificare după fiecare schimbare și regula «taie cifra miilor» | TESTAT, RULAT, PUBLICAT | [Migrator](../SLICE-ADE5-04-ade-migrator.md#slice-ADE10-07) |
| SLICE-ADE10-08 | Chitanțe pe ani școlari: cheie `uq_Sub_Serie_An_Numar` (AD_13, coloană calculată `AnScolar`), dubluri căutate doar în același an școlar, o singură constatare + jurnal rapid în ADE.Migrator, casetă de resetare a numerotării la închiderea anuală | Cod făcut, build ADE.Migrator curat; NETESTAT, NERULAT, AD_13 NEAPLICAT | [ADE10-08](../SLICE-ADE10-08-chitante-an-scolar.md) |
| SLICE-ADE10-05 | Predarea: ordinea SQL/publicare, probele utilizatorului, deciziile luate | TESTAT, RULAT, PUBLICAT | [Predare](../SLICE-AD10-01-schema-subunitati.md#slice-ADE10-05) |
| SLICE-AD10-11 | Subunitatea pe rapoarte și zilele lucrătoare pe luni; corecturi 404, coloană Stare și libere suplimentare | TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026) | [AD10-11](../SLICE-AD10-11-zile-lucratoare-subunitate-rapoarte.md) |

### Current focus

- Checkpointul inițial consemna implementarea locală și predarea probelor către utilizator conform ADE10-05; confirmarea ulterioară este din 23.10.2026. Subfelia **ADE10-08** (chitanțe pe ani școlari) a fost luată la 26.10.2026; următoarea liberă este **ADE10-09**. Deschis în ADE10-08: la 26.10.2026 sincronizarea a șters cheia veche în 24 de baze, dar nu a putut crea cheia nouă (sincronizarea nu adăuga coloane calculate; corectat în `schema_diff.py`, neverificat) — după ce `schema_diff.py` ajunge pe server, se reia sincronizarea; fereastra anuală neverificată pe ecran. Fișierul AD10-11 este indexat cu identificatorul său existent, fără renumerotare.

### Open threads

*Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.*

- `sql/AD_05_subunitati.sql` aplicat; numele cheilor străine vin din exportul 09.10.2026.
- Aplicația veche și migratorul vechi devin incompatibile după AD_05.
- Plan §11 (ecran de administrare a subunităților, restricții pe subunitate, rapoarte consolidate, completarea unei subunități populate, reconcilierea datelor amestecate din mai multe surse): implementat (confirmat de operator, 23.10.2026).
- Ajutor: paragraf adăugat în pagină (ADE10-04) și în `ADECHIT/UTILIZARE_WEB.md`; documentația migratorului în README.

## Slice AD11

Portalul părinților, conform planului aprobat și modelului de fișă de cont.

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-AD11-01 | CNP/cod/OTP, CodAccesPortal, email unic pe CNP, migrator, selector copii, dashboard/grafice, fișă print/PDF cu Interval, login/inbox localhost | TESTAT, RULAT, PUBLICAT | [Portal](../SLICE-AD11-01-portal-parinti.md) |
| SLICE-AD11-02 | Font opțiuni copil, gri subtotal/total print și PDF, scroll vertical mobil | TESTAT, RULAT, PUBLICAT | [Corecturi](../SLICE-AD11-01-portal-parinti.md#slice-AD11-02) |
| SLICE-AD11-03 | Conflicte istorice tolerate la import, portal blocat dinamic, avertizări navigabile Plătitori; DDL sursă și query date AvacontPush separate | TESTAT, RULAT, PUBLICAT | [Conflicte](../SLICE-AD11-01-portal-parinti.md#slice-AD11-03) |
| SLICE-AD11-04 | Selector mobil pe toată lățimea, carduri fără depășirea paginii, Imprimă ascuns pe mobil | TESTAT, RULAT, PUBLICAT | [Mobil](../SLICE-AD11-01-portal-parinti.md#slice-AD11-04) |
| SLICE-AD11-05 | «+» Prezență = copil + părinte atomic, erori cu aspect comun și linkuri (cross-subunitate), dublu clic, email doar pe copii activi/subunitatea curentă, mesaj CNP/email similare per DC, copac lună deschis după salvare, meniu «Aplicații» pe logo-ul portalului K-BOT | TESTAT, RULAT, PUBLICAT | [Copil+părinte](../SLICE-AD11-01-portal-parinti.md#slice-AD11-05) |

### Current focus

Utilizatorul testează pe localhost:5050. Acreditări fictive: `/adechit/preview/parents`;
login: `/adechit/parinti/preview`; inbox: `/adechit/preview/inbox`.
AD11-03 separă DDL pentru AVACONT_SURSA de queryul de date pentru AvacontPush;
conflictele din datele vechi blochează portalul identităților afectate, nu migrarea.
AD11-04 corectează lățimile mobile și ascunde Imprimă la maximum 800px.
AD11-05: vezi worklog; identitatea portalului rămâne pe tot DC-ul (poate bloca portalul pentru emailuri acceptate la salvare).
Următoarea subfelie liberă: **AD11-06**.

### Open threads

*Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.*

- AD12-01 (ajutor ADECHIT, `static/help/adechit.json`): aspectul, imprimarea și PDF-ul: TESTAT, RULAT, PUBLICAT.
- AD_08/SMTP/MariaDB și publicarea: executate.
- Login complet, selector, retragere acces, PDF/print și migrarea: TESTAT, RULAT, PUBLICAT.
- Datele istorice lipsă din import nu sunt reconstruite; SI se păstrează ca sold inițial.
- Ghidul și pașii de publicare: [PORTAL_PARINTI.md](../../../ADECHIT/PORTAL_PARINTI.md).

## Slice AD12

Ajutor ADECHIT, o singură sursă pentru fereastră, imprimare și PDF.

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-AD12-01 | Buton «?», cuprins, text formatat, imprimare și export PDF | TESTAT, RULAT, PUBLICAT | [Ajutor](../SLICE-AD12-01-ajutor-adechit.md) |

### Current focus

- ADE6-24 actualizează în aceeași intervenție ajutorul despre Email/CNP la educatori,
  în catalog și închiderea anuală. Sursa: `PYTHON/static/help/adechit.json`.
  Următoarea subfelie liberă: **AD12-02**.

### Open threads

*Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.*

- Citirea textului de către operator, aspectul PC/mobil, imprimarea și PDF-ul: confirmate
  de operator, 23.10.2026.

## Slice AD13

Încărcarea extraselor bancare în ADECHIT, înaintea asocierii cu copiii.

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-AD13-01 | Fereastra «Încărcare extrase bancare»: PDF-uri/folder cu deduplicare și căsuță de e-mail IMAP cu parolă criptată pe server; cititor PDF portat din K-BOT; import comun cu FOREXE (importa_extrase); tabele AD_EXTRASE_EMAIL, AD_EXTRASE_INCARCARI; ajutor actualizat | COD SCRIS, compilat sintactic; nimic rulat sau văzut | [Încărcare](../SLICE-AD13-01-incarcare-extrase.md) |

### Current focus

- Următoarea subfelie liberă: **AD13-02**.

### Open threads

- De făcut de operator: sql/AD_14_extrase_incarcare.sql în fiecare bază ADECHIT; pachetul cryptography + cheia ADE_MAIL_KEY pe server; publicarea fișierelor din worklog; probe cu PDF-uri reale și cu o căsuță Gmail/Yahoo (neprobate).
- Aspectul ferestrei, PC/mobil și tema întunecată: nevăzut.

## Slice AD14

Închiderea lunii cu schimbarea taxei active.

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-AD14-01 | Fereastra «Închidere lună» (modală, fundal estompat) în locul confirmării simple: tabelul de taxe cu taxa activă prima, «Adaugă taxă» care poziționează tabelul pe rândul nou (devine activ, celelalte se opresc), «Închide luna» / «Renunță»; ajutor + UTILIZARE_WEB actualizate | COD SCRIS, compilat sintactic (node --check); nimic rulat sau văzut | [Fereastra de închidere](../SLICE-AD14-01-inchidere-luna-taxa.md) |

### Current focus

- Următoarea subfelie liberă: **AD14-02**.

### Open threads

- Neprobat de operator: aspectul ferestrei (PC/mobil, tema întunecată), «Adaugă taxă» în fereastră, închiderea după schimbarea taxei, «Renunță»/Escape.
- Fără DDL, fără fișiere Python modificate: se publică numai fișierele web din worklog.
- Închiderea lunii august folosește în continuare fereastra anuală, fără selecția taxei (de decis dacă se dorește și acolo).
