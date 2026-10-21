# ADECHIT — STATUS, felii ADE10–ADE19

Detalii pentru [indexul din rădăcină](../../../ADECHIT_STATUS.md).
Actualizat: 12.10.2026.

## Slice ADE10

Subunități în același DC (plan: [plan_subunitati.md](../../../ADECHIT/plan_subunitati.md), analizat în ADE0-08/09).

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-ADE10-01 | Schema: `AD_Subunits`, `AD_ReceiptConfig`, `AD_IdMap`, `SubunitId` pe 17 tabele, chei și relații compuse, `LunaD.Ordine`, conversia DC-urilor populate | SCRIS LOCAL — DDL nerulat | [Schema](../SLICE-ADE10-01-schema-subunitati.md) |
| SLICE-ADE10-02 | API: context DC + subunitate, repository limitat, blocare/reîncercări/chitanțe pe subunitate, import cu hartă de ID-uri | SCRIS LOCAL — netestat | [API](../SLICE-ADE10-02-api-subunitati.md) |
| SLICE-ADE10-03 | ADE.Migrator: subunitate nouă/existentă, ID-uri remapate, serie și contor în subunitate | CONSTRUIT LOCAL — build curat, nerulat | [Migrator](../SLICE-ADE10-03-migrator-subunitati.md) |
| SLICE-ADE10-04 | Selectorul „Subunitate", comutarea per filă cu reîncărcare | SCRIS LOCAL — neprobat în browser | [Interfață](../SLICE-ADE10-04-interfata-subunitati.md) |
| SLICE-ADE10-06 | Corecturi după review: snapshot înainte de blocare, reverificare completă în migrator, comutare fără pierderi, reconciliere, amprente vechi, acces pe utilizator, sincronizare PK | SCRIS LOCAL — netestat | [ADE10-02…05](../SLICE-ADE10-02-api-subunitati.md) |
| SLICE-ADE10-07 | ADE.Migrator: copii reconstituiți din SS_Buget (Platitori + Platitori_sub), fereastra «Chitanțe dublate…» cu numere editabile, reverificare după fiecare schimbare și regula «taie cifra miilor» | CONSTRUIT LOCAL — build curat, nerulat, fereastra nevăzută | [Migrator](../SLICE-ADE10-07-migrator-chitante-dublate-si-platitori-reconstituiti.md) |
| SLICE-ADE10-05 | Predarea: ordinea SQL/publicare, probele utilizatorului, deciziile luate | DOCUMENTAT LOCAL | [Predare](../SLICE-ADE10-05-predare-subunitati.md) |

### Current focus

- Implementarea planului este scrisă local; urmează testarea de către utilizator conform ADE10-05.
  Următoarea subfelie liberă: **ADE10-08**.

### Open threads

- `sql/AD_05_subunitati.sql` neaplicat (nici pe `000_DEMO`); numele cheilor străine vin din exportul 09.10.2026.
- Aplicația veche și migratorul vechi devin incompatibile după AD_05.
- Neimplementat (plan §11): ecran de administrare a subunităților, restricții pe subunitate, rapoarte consolidate,
  completarea unei subunități populate, reconcilierea datelor amestecate din mai multe surse.
- Ajutor: paragraf adăugat în pagină (ADE10-04) și în `ADECHIT/UTILIZARE_WEB.md`; documentația migratorului în README.

## Slice AD11

Portalul părinților, conform planului aprobat și modelului de fișă de cont.

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-AD11-01 | CNP/cod/OTP, CodAccesPortal, email unic pe CNP, migrator, selector copii, dashboard/grafice, fișă print/PDF cu Interval, login/inbox localhost | SCRIS LOCAL — compilare/build curate, GET locale 200; funcțional/UI/PDF și server nevalidate | [Portal](../SLICE-AD11-01-portal-parinti.md) |
| SLICE-AD11-02 | Font opțiuni copil, gri subtotal/total print și PDF, scroll vertical mobil | SCRIS LOCAL — proba utilizatorului în curs | [Corecturi](../SLICE-AD11-02-font-totaluri-scroll.md) |
| SLICE-AD11-03 | Conflicte istorice tolerate la import, portal blocat dinamic, avertizări navigabile Plătitori; DDL sursă și query date AvacontPush separate | SCRIS LOCAL — sintaxă/build curate, SQL/UI neprobate | [Conflicte](../SLICE-AD11-03-conflicte-query-avacontpush.md) |
| SLICE-AD11-04 | Selector mobil pe toată lățimea, carduri fără depășirea paginii, Imprimă ascuns pe mobil | SCRIS LOCAL — proba vizuală aparține utilizatorului | [Mobil](../SLICE-AD11-04-latimi-mobil.md) |
| SLICE-AD11-05 | «+» Prezență = copil + părinte atomic, erori cu aspect comun și linkuri (cross-subunitate), dublu clic, email doar pe copii activi/subunitatea curentă, mesaj CNP/email similare per DC, copac lună deschis după salvare, meniu «Aplicații» pe logo-ul portalului K-BOT | SCRIS LOCAL — doar sintaxă; UI/server neprobate | [Copil+părinte](../SLICE-AD11-05-copil-parinte-linkuri.md) |

### Current focus

Utilizatorul testează pe localhost:5050. Acreditări fictive: `/adechit/preview/parents`;
login: `/adechit/parinti/preview`; inbox: `/adechit/preview/inbox`.
AD11-03 separă DDL pentru AVACONT_SURSA de queryul de date pentru AvacontPush;
conflictele din datele vechi blochează portalul identităților afectate, nu migrarea.
AD11-04 corectează lățimile mobile și ascunde Imprimă la maximum 800px.
AD11-05: vezi worklog; identitatea portalului rămâne pe tot DC-ul (poate bloca portalul pentru emailuri acceptate la salvare).
Următoarea subfelie liberă: **AD11-06**.

### Open threads

- AD12-01 (ajutor ADECHIT, `static/help/adechit.json`): aspectul, imprimarea și PDF-ul neprobate; textul de citit de operator.
- AD_08/SMTP/MariaDB și publicarea nu au fost executate.
- Login complet, selector, retragere acces, PDF/print și migrarea urmează testarea utilizatorului.
- Datele istorice lipsă din import nu sunt reconstruite; SI se păstrează ca sold inițial.
- Ghidul și pașii de publicare: [PORTAL_PARINTI.md](../../../ADECHIT/PORTAL_PARINTI.md).

## Slice AD12

Ajutor ADECHIT, o singură sursă pentru fereastră, imprimare și PDF.

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-AD12-01 | Buton «?», cuprins, text formatat, imprimare și export PDF | SCRIS LOCAL — sintaxă și generare PDF inițială; aspect neprobat | [Ajutor](../SLICE-AD12-01-ajutor-adechit.md) |

### Current focus

- ADE6-24 actualizează în aceeași intervenție ajutorul despre Email/CNP la educatori,
  în catalog și închiderea anuală. Sursa: `PYTHON/static/help/adechit.json`.
  Următoarea subfelie liberă: **AD12-02**.

### Open threads

- Citirea textului de către operator, aspectul PC/mobil, imprimarea și PDF-ul rămân
  de confirmat; actualizarea conținutului nu reprezintă validare vizuală.
