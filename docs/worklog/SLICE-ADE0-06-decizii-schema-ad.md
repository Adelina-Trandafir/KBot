# SLICE-ADE0-06 — deciziile despre schema AD_ (prefix, chei străine, tabele și coloane scoase)

Data: 08.10.2026. Stare: DECIZII ÎNREGISTRATE; modificările de cod și de DDL sunt scrise local,
fără rulare pe MariaDB și fără teste (utilizatorul nu le-a cerut).

## Ce s-a decis și de ce

Deciziile au fost luate de utilizator în conversație, în ordinea de mai jos. Niciun tabel nu
fusese creat pe server, deci schimbările nu cer migrare de date existente.

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

## Fișiere atinse

- Generator și rezultat: `ADECHIT/tools/build_schema.py`, `sql/AD_01_schema.sql`,
  `sql/AD_02_chitante.sql` (redenumite din `ADE_…`), `PYTHON/routes/adechit/schema.json`.
- Cod: `PYTHON/routes/adechit/{__init__,calculations,domain,importer,repository,service}.py`,
  `PYTHON/routes/schema_sync/schema_diff.py` (exclude `AD_` de la DROP),
  `PYTHON/static/js/adechit/app.js` (adăugarea unui copil nu mai caută taxa activă).
- Unelte și teste: `ADECHIT/tools/{preview,verify_import}.py`, `PYTHON/tests/test_adechit.py`.
- Documente: `ADECHIT/{PLAN_IMPLEMENTARE,MAPARE_ACCESS_WEB,INVENTAR_OBIECTE}.md`,
  `ADECHIT_STATUS.md`, `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md`,
  `docs/worklog/SLICE-ADE1-01-implementare-partiala.md`, acest worklog.

## Rezultatele verificărilor

- Generatorul rulează (Python din `PYTHON/.venv`) și produce 11 tabele de business și 5 de sistem.
- Modulele Python modificate se compilează (`py_compile`).
- **Testele nu au fost rulate** (regula proiectului: doar la cerere). Testele și datele de probă
  au fost adaptate la schema nouă, fără execuție.
- DDL-ul nu a fost rulat pe MariaDB.

## Neverificat sau amânat

- DDL cu chei străine nevalidat pe MariaDB; regulile de ștergere din decizia 3 sunt propunerea
  sesiunii, de confirmat.
- Migrarea reală (`baza2020_PP.mdb`) nu a fost refăcută cu schema nouă: adresa copilului pe
  plătitor, `ZileLuna` pe lună și ignorarea coloanelor scoase sunt netestate pe date reale.
- Importul ADE5-02 (5.247 de rânduri reconciliate) a fost făcut pe schema veche; trebuie repetat.
- Unde se salvează motivul anulării unei restituiri (M03) și cum se tratează transferul de grupă
  fără `MutaCopil` (M06 / ADE6-03) rămân de decis.
- `ALTER TABLE` nu mai este necesar (niciun tabel nu există pe server), dar `ADE3` trebuie
  recitit: descrierile lui anterioare (`ADE_…`, tabelele scoase) sunt depășite de acest worklog.
- Fără schimbări vizibile pentru operator în K-BOT: nu există pagini de ajutor de actualizat.
