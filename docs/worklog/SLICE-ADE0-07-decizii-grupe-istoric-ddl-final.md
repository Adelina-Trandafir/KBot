# SLICE-ADE0-07 — deciziile despre grupe, istoric, plecați și DDL-ul final al schemei AD_

Data: 08.10.2026. Stare: DECIZII ÎNREGISTRATE; DDL scris local, nevalidat pe MariaDB. Cod Python/JS
neschimbat, teste nerulate (regula proiectului: doar la cerere).

## Ce s-a decis și de ce

Deciziile au fost luate de utilizator în conversație; ele închid M06 și completează M03.

| # | Decizie | Efect |
|---|---|---|
| 1 | `MutaCopil` din Access este un tabel mort (gol); logica veche a transferului era greșită și nu se reproduce | Nu se importă nimic din el. Înlocuit de punctele 2–3. |
| 2 | Istoricul copilului = jurnal automat, doar adăugare: `AD_Platitori_Istoric` (INTRARE, MUTARE, PLECARE, REVENIRE, IESIRE_DEFINITIVA, COMPENSARE; data, grupa veche/nouă, utilizator, notă) | Scris în aceeași tranzacție cu schimbarea. La migrare: INTRARE/PLECARE din `DataIntrare`/`DataIesire`; MUTARE dedusă din schimbările de grupă ale lunilor închise, marcată `Dedus` (doar luna, fără zi). |
| 3 | Istoricul grupei = educatorii ei în timp: `AD_Grupe_Educator` (`DeLa`/`PanaLa`). Nivelul mica/mijlocie/mare NU se păstrează (nu e relevant) | Nu există `AD_Grupe_An`. Anii în care a existat grupa se deduc din prezență. Coloana `AD_Grupe.Educator` iese după migrarea istoricului. |
| 4 | Reconstrucția istoricului la migrare folosește `AD_SS_Buget` (`IDG`, `Grupa`, `Educator`, `Plecat` pe luni închise) | Migrarea AFIȘEAZĂ lista educatorilor găsiți pe fiecare grupă; operatorul unește variantele; abia apoi se scrie. |
| 5 | Grupa este aceeași de la un an la altul și poate fi închisă: `AD_Grupe.InchisaDinAn`; rămâne vizibilă pentru lunile dinainte, fără ștergere fizică | Alegerile de grupă pentru perioadele noi n-o mai arată; istoricul o păstrează. |
| 6 | Un copil apare în prezența unei singure grupe pe lună: cea în care se află la închiderea lunii; o mutare după închidere se aplică de la luna deschisă următoare | `AD_Prezenta.IDG` rămâne cheia; nicio coloană nouă. |
| 7 | Copii plecați: grupa specială (`AD_Grupe.Tip = 'PLECATI'`). Sold 0 → iese din situația lunii următoare; debit → rămâne până la plată (iese din luna următoare plății); credit → rămâne până la compensare | `AD_Compensare` (decizia consiliului, debit ↔ credit, sumă, notă, utilizator). |
| 8 | Motivul anulării unei restituiri se salvează (M03) | `AD_Retur.MotivAnulare`. Rezolvă punctul rămas deschis în ADE0-06 (decizia 11). |
| 9 | Telefonul plătitorului: `AD_Platitori_sub.Telefon` (VARCHAR(50), NULL). Nu există în Access, deci rămâne gol la migrare. | Coloană nouă în AD_03; ADE.Migrator o include; cod Python/JS neadaptat. |

## Fișiere atinse

- Nou: `sql/AD_03_schema_finala.sql` — schema finală completă (14 tabele de business + 5 tehnice = 19; împreună cu AVACONT_COMUN.Unitati_Chitante din AD_02 sunt 20, în
  ordinea dependențelor), pentru AVACONT_SURSA. Înlocuiește `AD_01_schema.sql` pe un server unde nu există
  tabele `AD_`; are ALTER-uri comentate pentru cazul în care AD_01 a fost deja rulat.
- Documente: `ADECHIT/DECIZII_DESCHISE.md` (M06), `ADECHIT/PLAN_IMPLEMENTARE.md` (ADE6-03), acest worklog,
  `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md`, `ADECHIT_STATUS.md`.

## Rezultatele verificărilor

- DDL nerulat pe MariaDB. Nicio verificare automată.

## Neverificat sau amânat

- `PYTHON/routes/adechit/schema.json` și `ADECHIT/tools/build_schema.py` NU sunt actualizate: generatorul
  produce încă `AD_01_schema.sql` (cu `AD_Grupe.Educator`, fără tabelele noi). Codul (`repository.py`,
  `importer.py`, `service.py`, `app.js`) încă folosește `Educator` pe grupă și nu cunoaște jurnalul,
  educatorii pe perioade, compensarea sau `MotivAnulare`. Se adaptează în ADE6-03, ADE8-01 și ADE5.
- Reconstrucția istoricului (educatori, mutări deduse) nu este scrisă; are nevoie de o verificare doar-citire
  a completitudinii `Educator`/`Grupa` în cele 5.247 de rânduri (neverificată).
- Regulile de ștergere ale cheilor străine noi sunt propunerea sesiunii (RESTRICT), de confirmat pe server.
- `AD_Compensare`: un `CHECK` (debitor diferit de creditor) este refuzat de MariaDB (eroarea 1901, coloane cu FK ON UPDATE CASCADE); regula se aplică în serviciu, la salvare, când se implementează compensarea.
- Importul ADE5 trebuie repetat pe schema nouă (ca în ADE0-06).
- Fără schimbări vizibile pentru operator în K-BOT (modul web ADE): nu există pagini de ajutor de actualizat.
- Nimic nu a fost comis sau trimis (regula: fără scrieri git la inițiativa mea).
