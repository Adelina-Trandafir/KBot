# SLICE-ADE0-06…07 — Deciziile succesive despre schema AD_

Decizii din 08.10.2026, păstrate succesiv: ADE0-06 lasă deschise M03/M06; ADE0-07 închide M06 și completează M03 prin MotivAnulare. Confirmarea comună a rubricilor de nevalidat/amânat din ADE0-06/07: *Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.* Limitele generatorului, codului, migrării și regulilor FK descriu checkpointurile respective.

[ADE0-06](#slice-ADE0-06) · [ADE0-07](#slice-ADE0-07)

## SLICE-ADE0-06 — deciziile despre schema AD_ (prefix, chei străine, tabele și coloane scoase) <a id="slice-ADE0-06"></a>

Data: 08.10.2026. Stare: DECIZII ÎNREGISTRATE; modificările de cod și de DDL sunt TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Ce s-a decis și de ce

Deciziile au fost luate de utilizator în conversație, în ordinea de mai jos. Niciun tabel nu fusese creat pe server, deci schimbările nu cer migrare de date existente.

| # | Decizie | Efect |
|---|---|---|
| 1 | Prefixul tabelelor proprii modulului este **`AD_`**, nu `ADE_` | Toate numele din cod, teste, unelte, SQL și documente. Numele slice-urilor (`SLICE-ADE…`) și cheia de test `ADE_TEST_FACTORY` rămân. Excepția `AVACONT_COMUN.Unitati_Chitante` rămâne. |
| 2 | Schema se pune pe **AVACONT_SURSA** (șablonul bazelor de unitate); are **chei străine** | `sql/AD_01_schema.sql` (fost `ADE_01_schema.sql`) generat din `ADECHIT/tools/build_schema.py`, în `utf8mb3_general_ci`. Coloanele de legătură sunt `NULL DEFAULT NULL` (NULL = fără legătură, nu 0 ca în Access). |
| 3 | Reguli de ștergere ale cheilor străine (alese în sesiune, de confirmat la validarea pe server) | Istoricul de bani și părinții: RESTRICT. `Plati`/`Retur` → `Prezenta`/`LunaD`: SET NULL. `Chitante`/`AlteDoc` → `LunaD`: SET NULL; → `Plati`: RESTRICT. `SS_Buget` → `LunaD`/`Prezenta`: CASCADE. ON UPDATE CASCADE peste tot. |
| 4 | `Platitori`: dispar `IDF`, `Frate`, `IDV`, `Avans`, `Adresa` | Taxa rămâne pe `Prezenta`/`LunaD`. |
| 5 | `Platitori_sub`: apare `CNP_Platitor`; dispare `J`; `Adresa` (existentă, goală) primește adresa copilului | La migrare, adresa din `Platitori` se copiază pe plătitor doar dacă a lui e goală sau are sub 10 caractere (de ex. «Ploiesti»). |
| 6 | `Plati`: dispar `Valoare`, `Anticipat`, `Restanta`; `TIP` devine `INT` (în Access text cu cifre) | Cod adaptat: compararea cu 1 și 2 ca numere; un `TIP` gol la import devine NULL. |
| 7 | Tabelele **`Delegati`, `Prezenta_sub`, `MutaCopil`** nu se mai portează; `Facturi`, `Trimis`, `Ver_db` nu intră | Scoase din model, import, chei străine, formulare și teste. Transferul de grupă nu mai lasă istoric propriu. |
| 8 | `Chitante`: dispare `Valoare` | Suma se ia din plata legată (`Plati.Plata`); răspunsul API după emitere o arată în continuare. |
| 9 | `Prezenta`: dispar `ZileLuna`, `ZileAbsenta`, `MZCPrezenta`, `MZCAbsenta`, `ValoareMancare`, `Detalii`, `Restanta`, `Avans`, `SI` | `ValoareTotala` = valoarea contractului (fără `Prezenta.SI`); soldul inițial vine din `Platitori.SI`. |
| 10 | `LunaD` primește **`ZileLuna`** (zilele lucrătoare ale lunii, folosite din lună, nu din fiecare prezență) | La migrare: valoarea cea mai frecventă din rândurile de prezență ale lunii. Cod: validarea zilelor, situația și crearea lunii următoare. Lună fără valoare: se calculează ca la crearea unei luni noi. |
| 11 | `Retur`: dispare `Motivul` | **Conflict cu M03** (motivul anulării unei restituiri trebuia salvat): nu mai există unde. Decizia M03 rămâne deschisă până se alege un loc nou. |
| 12 | `ValoriTaxe`: rămân doar `TaxaZilnica`, `Activ`, `Expl` | Dispar `TaxaLunara`, `TaxaMicDejun`, `TaxaDejun`, `TaxaMancare`, `Murdar`, `DoarCalculZilnic` (iar `ZileCon`, `ZilePre`, `DisPre`, `DisBro` erau deja excluse). |

Coloanele scoase sunt ignorate la import, ca un extras Access care le conține să nu dea eroare.

### Fișiere atinse

Generator și rezultat: `ADECHIT/tools/build_schema.py`, `sql/AD_01_schema.sql`, `sql/AD_02_chitante.sql` (redenumite din `ADE_…`), `PYTHON/routes/adechit/schema.json`. Cod: `PYTHON/routes/adechit/{__init__,calculations,domain,importer,repository,service}.py`, `PYTHON/routes/schema_sync/schema_diff.py` (exclude `AD_` de la DROP), `PYTHON/static/js/adechit/app.js` (adăugarea unui copil nu mai caută taxa activă). Unelte și teste: `ADECHIT/tools/{preview,verify_import}.py`, `PYTHON/tests/test_adechit.py`. Documente: `ADECHIT/{PLAN_IMPLEMENTARE,MAPARE_ACCESS_WEB,INVENTAR_OBIECTE}.md`, `ADECHIT_STATUS.md`, `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md`, `docs/worklog/SLICE-ADE1-01-implementare-partiala.md`, acest worklog.

### Rezultatele verificărilor

- Generatorul rulează (Python din `PYTHON/.venv`) și produce 11 tabele de business și 5 de sistem.
- Modulele Python modificate se compilează (`py_compile`).
- **Testele nu au fost rulate** (regula proiectului: doar la cerere). Testele și datele de probă au fost adaptate la schema nouă, fără execuție.
- DDL-ul nu a fost rulat pe MariaDB.

### Neverificat sau amânat

- DDL cu chei străine nevalidat pe MariaDB; regulile de ștergere din decizia 3 sunt propunerea sesiunii, de confirmat.
- Migrarea reală (`baza2020_PP.mdb`) nu a fost refăcută cu schema nouă: adresa copilului pe plătitor, `ZileLuna` pe lună și ignorarea coloanelor scoase sunt netestate pe date reale.
- Importul ADE5-02 (5.247 de rânduri reconciliate) a fost făcut pe schema veche; trebuie repetat.
- Unde se salvează motivul anulării unei restituiri (M03) și cum se tratează transferul de grupă fără `MutaCopil` (M06 / ADE6-03) rămân de decis.
- `ALTER TABLE` nu mai este necesar (niciun tabel nu există pe server), dar `ADE3` trebuie recitit: descrierile lui anterioare (`ADE_…`, tabelele scoase) sunt depășite de acest worklog.
- Fără schimbări vizibile pentru operator în K-BOT: nu există pagini de ajutor de actualizat.

## SLICE-ADE0-07 — deciziile despre grupe, istoric, plecați și DDL-ul final al schemei AD_ <a id="slice-ADE0-07"></a>

Data: 08.10.2026. Stare: DECIZII ÎNREGISTRATE; DDL TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Cod Python/JS neschimbat.

### Ce s-a decis și de ce

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

### Fișiere atinse

Nou: `sql/AD_03_schema_finala.sql` — schema finală completă (14 tabele de business + 5 tehnice = 19; împreună cu AVACONT_COMUN.Unitati_Chitante din AD_02 sunt 20, în ordinea dependențelor), pentru AVACONT_SURSA. Înlocuiește `AD_01_schema.sql` pe un server unde nu există tabele `AD_`; are ALTER-uri comentate pentru cazul în care AD_01 a fost deja rulat. Documente: `ADECHIT/DECIZII_DESCHISE.md` (M06), `ADECHIT/PLAN_IMPLEMENTARE.md` (ADE6-03), acest worklog, `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md`, `ADECHIT_STATUS.md`.

### Rezultatele verificărilor

- DDL TESTAT, RULAT, PUBLICAT pe MariaDB.

### Neverificat sau amânat

- `PYTHON/routes/adechit/schema.json` și `ADECHIT/tools/build_schema.py` NU sunt actualizate: generatorul produce încă `AD_01_schema.sql` (cu `AD_Grupe.Educator`, fără tabelele noi). Codul (`repository.py`, `importer.py`, `service.py`, `app.js`) încă folosește `Educator` pe grupă și nu cunoaște jurnalul, educatorii pe perioade, compensarea sau `MotivAnulare`. Se adaptează în ADE6-03, ADE8-01 și ADE5.
- Reconstrucția istoricului (educatori, mutări deduse) nu este scrisă; are nevoie de o verificare doar-citire a completitudinii `Educator`/`Grupa` în cele 5.247 de rânduri (neverificată).
- Regulile de ștergere ale cheilor străine noi sunt propunerea sesiunii (RESTRICT), de confirmat pe server.
- `AD_Compensare`: un `CHECK` (debitor diferit de creditor) este refuzat de MariaDB (eroarea 1901, coloane cu FK ON UPDATE CASCADE); regula se aplică în serviciu, la salvare, când se implementează compensarea.
- Importul ADE5 trebuie repetat pe schema nouă (ca în ADE0-06).
- Fără schimbări vizibile pentru operator în K-BOT (modul web ADE): nu există pagini de ajutor de actualizat.
- Nimic nu a fost comis sau trimis (regula: fără scrieri git la inițiativa mea).
