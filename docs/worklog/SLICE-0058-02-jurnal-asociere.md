# SLICE 0058-02 — Jurnalul asocierii (`asociere.log`), cu întrerupător în `main.py`

A doua trecere peste felia 0058. Prima a reparat două defecte ale verificării F15
(suma pe indicator, numele recepției) și a lăsat scris negru pe alb că **nu se știe
dacă exact acela era cazul care refuza salvarea operatorului**. Rularea adevărată din
09.09.2026, ora 14:18, a răspuns: nu era.

```
2026-09-09 14:18:36,608 - WARNING - PRELUCRARE dc=000_DEMO cod=AAB2EF2MCP4
decizii respinse: Recepția 247: liniile ultimului instantaneu nu se potrivesc cu
cele ale recepției. Lanțul nu se închide.
```

Linia asta e tot ce lasă în urmă o salvare refuzată — și nu spune nici ce lanț s-a
format, nici ce cifre s-a uitat serverul la ele, nici ce a vrut operatorul să facă.
Felia asta scrie exact ce lipsea.

> **Observație despre linia de mai sus, nu o reparație a acestei felii.** Mesajul zice
> «Recepția 247», adică `id_stabil=True`. Pe drumul de ingestie 0058 §4 a pus
> `id_stabil=False` chiar în aceeași zi, deci serverul care a scris linia **rula codul
> de dinaintea feliei 0058**. Reparația de la §4 (suma pe indicator — două linii RHR pe
> același indicator păstrau doar ultima și comparația cădea pe date corecte) **nu era
> pe server când a apărut respingerea**. Se verifică după ce se împinge codul; până
> atunci, cazul rămâne deschis.

---

## 1. Ce s-a făcut

### `PYTHON/utils/asociere_log.py` (nou)

Un jurnal separat, construit după tiparul lui `utils/timing.py`: logger propriu
(`kbot.asociere`), fișier propriu (`asociere.log`), handler propriu și
`propagate = False`, deci **nicio linie nu ajunge în `api_server.log`**.

Un **bloc pe cerere**, ținut în memorie și scris întreg la sfârșit — două cereri
simultane nu-și pot amesteca liniile. Blocul poartă, în ordinea în care le face codul:

| Parte | Ce conține |
|---|---|
| antetul | rută, `dc`, `user`, `cod`, `mod`, statusul HTTP, durata |
| starea | amprenta ȘI cele opt numere din care s-a calculat |
| tabloul | fiecare recepție cu sumele ei pe indicator; fiecare instantaneu cu liniile lui |
| intenția | sugestiile trecerii automate, apoi fiecare hotărâre/comandă cu recepția în care se rezolvă |
| lanțurile | fiecare lanț rezultat, în ordinea datei, exact cum îl văd regulile |
| verdictele | F14 / F15 / F16, fiecare cu cifrele comparate — și cele care TREC, nu doar cea care refuză |
| scrierea | ce s-a scris, sau că nu s-a scris nimic (rollback) |

O respingere se scrie ca `RESPINS` acolo unde s-a întâmplat **și** se repetă la piciorul
blocului, iar ultima linie a blocului e un `SUMAR` de o singură linie. Deci o zi întreagă
se citește cu `grep RESPINS asociere.log` sau `grep SUMAR asociere.log`, fără blocuri în
cale.

Funcțiile publice: `set_enabled`, `enabled`, `traced` (decoratorul de rută), `note`,
`section`, `line`, `table`, `refusal`, plus doi ajutori de formatare (`moment`,
`sume_pe_indicator`). Toate se întorc pe prima linie când jurnalul e închis; `_write` e
prins în `try/except`, ca un jurnal să nu poată strica niciodată o rută.

### Întrerupătorul

`PYTHON/main.py`, sus de tot, deasupra tuturor celorlalte:

```python
ASOCIERE_LOG_ENABLED = True
asociere_log.set_enabled(ASOCIERE_LOG_ENABLED)
```

`False` + repornirea serverului = nu se deschide niciun fișier și nu se construiește
nicio linie. Ordinea în care se decide: **mediul întâi** (`KBOT_ASOCIERE=0`), apoi
comutatorul din `main.py`, apoi `ASOCIERE_LOG_ENABLED` din `config.py`, apoi pornit.

Mediul bate `main.py` **dinadins**: jumătate din suita de teste face `from main import
app`, deci un comutator care ar fi bătut mediul ar fi aprins jurnalul în fiecare rulare
de pytest. `tests/conftest.py` pune `KBOT_ASOCIERE=0` exact ca la cronometru.

Fișierul se mută cu `KBOT_ASOCIERE_LOG` în mediu sau `ASOCIERE_LOG_PATH` în `config.py`.
Se rotește la 10 MB, cu 5 copii — ca celelalte două jurnale.

### Unde s-au pus apelurile

**`routes/forexe/prelucrare_asociere.py`** — inima asocierii, deci acolo e cea mai mare
parte:

- `amprenta` — cele opt numere, nu doar hash-ul: când două faze nu se potrivesc, singura
  întrebare utilă e CARE dintre ele s-a mișcat.
- `citeste_receptii` / `citeste_instantanee` / `citeste_instantanee_context` — tablourile,
  ca tabele, cu sumele pe indicator pe fiecare rând.
- `pas4c_automat` — candidații după sumă, apoi fiecare potrivire și fiecare ratare, pe
  ture (tura 1 = de la nou la vechi, tura 2 = restul).
- `verifica_acoperirea` — câte hotărâri pentru câte instantanee, câte lipsesc.
- `materializeaza_reconstituite` — ce recepție se naște din ce lanț și din ce rânduri.
- `valideaza_plasarile` — **partea care contează**: pentru fiecare lanț, recepția cu
  sumele ei, tabelul lanțului întreg, apoi F14 / F16 pe fiecare instantaneu și F15 cu
  **amândouă tablourile de valori scrise unul sub altul**. Trec și verdictele bune, nu
  doar cel care refuză: «F15 trece» spune la fel de mult ca «F15 cade».
- `aplica_decizii` — hotărârile ca tabel, ancorele pasului 4b, apoi «ce vrea să facă
  rularea» rând cu rând, **inclusiv instantaneele deja legate pe care le trage în lanț**
  (F15 și F16 se uită la lanțul întreg, deci o respingere poate veni dintr-un rând pe care
  operatorul nu l-a atins — până acum asta nu se vedea de nicăieri).
- `recalculeaza_final`, `marcheaza_reconstituirile_nesigure` — trecerile `Final`/`Partial`
  și marcajul F28.

**`routes/forexe/prelucrare.py`** — `@journal.traced("prelucrare")` sub `@timing.timed`,
antetul (`dc`, `user`, `cod`, `mod`, rândurile sosite pe tabel), sfârșitul fiecărei căi:
amprentă nepotrivită, alegere de unitate, decizii respinse, sarcină de nefolosit, eroare
MariaDB, rollback-ul propunerii, commit-ul salvării.

**`routes/forexe/asociere.py`** — `@journal.traced("asociere GET")` și `("asociere POST")`,
tabloul citit cu blocajele lui, comenzile ca tabel (cu `IDRR` de acum lângă `IDRR` cerut),
demarcarea `Sters`, numărătorile, și fiecare cale de refuz (instantaneu blocat, comandă
invalidă, cădere).

---

## 2. Cum arată

Bloc adevărat, scos dintr-o rulare de probă pe funcțiile pure (scriptul e temporar, nu e
în depozit):

```
====================================================================================
2026-09-09 17:32:30.932  rulare 1  [proba F15 pe linii]  status 200  0 ms
  dc=000_DEMO  user=proba  cod=AAB2EF2MCP4  mod=salvare
--- verificarea lanțurilor (1 recepții atinse; F15 = veto) -------------------------
  recepția 247 (Recepția din 18.05.2026 · 1200.00) SumaAntet=1200.00  linii=AAB=1150.00
  IDRH | rand | DataH      | Total   | stergere | linii
  -----+------+------------+---------+----------+------------
  9000 | 2    | 12.05.2026 | 900.00  | nu       | AAB=900.00
  9001 | 3    | 20.05.2026 | 1200.00 | nu       | AAB=1200.00
  F14 și F16 trec pe toate cele 2 instantanee ale lanțului
  F15 pe linii: instantaneu AAB=1200.00
                recepție    AAB=1150.00
  F15 CADE pe linii: tablourile de mai sus nu sunt egale
  RESPINS: Recepția din 18.05.2026 · 1200.00: liniile ultimului instantaneu nu se
  potrivesc cu cele ale recepției (AAB: instantaneu 1200.00 / recepție 1150.00). ...
--- ce a refuzat rularea -----------------------------------------------------------
  RESPINS: ...
SUMAR rulare=1 ruta=... status=200 ms=0 respingeri=1 dc=000_DEMO cod=AAB2EF2MCP4
```

(«status 200» fiindcă proba nu e o rută Flask, ci un apel direct: statusul se citește
din răspunsul rutei, iar aici nu există unul. Într-o rulare adevărată ar scrie 400.)

Pentru cazul din 14:18 asta ar fi arătat, dintr-o privire: ce recepție, ce lanț s-a
format, care instantaneu a fost socotit ultimul, ce indicator nu se potrivea și cu cât.

---

## 3. Fișiere atinse

- `PYTHON/utils/asociere_log.py` — **nou**, 316 rânduri.
- `PYTHON/main.py` — importul, comutatorul `ASOCIERE_LOG_ENABLED` și predarea lui.
- `PYTHON/routes/forexe/prelucrare_asociere.py` — importul + apelurile de jurnal
  (niciun rând de logică schimbat; singurul cod nou care nu e jurnal e `_ca_text`,
  un ajutor de formatare).
- `PYTHON/routes/forexe/prelucrare.py` — decoratorul, antetul, capetele de drum.
- `PYTHON/routes/forexe/asociere.py` — decoratoarele pe cele două rute, tabloul,
  comenzile, capetele de drum.
- `PYTHON/tests/conftest.py` — `KBOT_ASOCIERE=0`.

---

## 4. Rezultatele rulărilor

- `PYTHON/.venv` suita întreagă, jurnalul ÎNCHIS (cum îl ține `conftest.py`) —
  **514 trecute / 19 sărite / 0 picate**, neschimbat față de 0058.
- Aceeași suită cu `KBOT_ASOCIERE=1` și `KBOT_ASOCIERE_LOG` într-un fișier temporar —
  **514 trecute / 19 sărite / 0 picate**, iar fișierul s-a scris (57 KB). Asta acoperă
  și drumul în care jurnalul e chemat FĂRĂ nicio rulare deschisă (testele cheamă
  funcțiile pure direct): liniile ies neindentate, dar ies.
- Rulare de probă pe `valideaza_plasarile` și `pas4c_automat` cu jurnalul pornit, cu
  trei blocuri: F14 cade, F15 cade pe linii, lanț care se închide. Blocul de mai sus e
  de acolo.

---

## 5. Ce NU s-a verificat

- **Nicio cerere adevărată n-a trecut prin jurnal.** Nu s-a pornit serverul, nu s-a
  atins baza vie, deci blocul complet — cu tabloul citit din MariaDB și cu antetul de
  sesiune — n-a fost văzut niciodată. Ce s-a văzut e partea de verificare a lanțurilor,
  pe date făcute de mână.
- **Cazul operatorului din 14:18 rămâne nediagnosticat.** Jurnalul e unealta, nu
  răspunsul. Ca să dea răspunsul trebuie: (1) împins codul pe server, (2) repornit
  serverul cu `ASOCIERE_LOG_ENABLED = True`, (3) refăcută salvarea pe `AAB2EF2MCP4`,
  (4) citit blocul.
- **Nu s-a măsurat cât încetinește.** Un bloc de propunere pe un angajament mare are
  câteva sute de rânduri; costul e o listă de șiruri în memorie plus o scriere la
  sfârșit, dar nu e cronometrat. `forexe_timing.log` va arăta diferența dacă vreodată
  contează.
- **Nimic nu s-a comis și nimic nu s-a împins.**
