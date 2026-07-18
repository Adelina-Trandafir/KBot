# SLICE-0011-01 — `GET /api/forexe/sumar` (jumătatea de server)

**Data:** 2026-07-18
**Felia:** 0011 (Sumar) — pasul 1 din 2. Pasul 2 = `SLICE-0011-02-sumar-view.md`.
**Plan:** lipit în sesiune (secțiunile 1–5, 7).

---

## Ce s-a schimbat și de ce

Prima vedere reală din cele nouă primește sursa de date: portul lui `qFX_MAIN_SUMAR`
(v1) pe MariaDB, expus ca `GET /api/forexe/sumar?cod=<CodAngajament>`.

Granulația e **un rând per indicator** al angajamentului. Coloanele de antet
(`CodAngajament .. Preluat`) se repetă identic pe fiecare rând în Access; aici sunt
ridicate o singură dată în `header`, ca să nu călătorească duplicate pe fir.

### Blocantul `Clsf` — REZOLVAT, ramura A

Planul cerea explicit ca asta să fie lămurită **citind, nu ghicind**, înainte de a
scrie query-ul. Pe stația de dezvoltare nu există `PYTHON/config.py`, deci un
`SHOW COLUMNS` live era imposibil, iar în repo nu există DDL pentru `Clasificatii`
(`DDL_FX_ListaAngajamente.sql` nu o menționează, iar `FX_System_Export/TABLES/`
conține doar tabele `FX_*` — `ClasificatiiG` e tabel legat extern). Munca s-a oprit
și s-a raportat; **operatorul a furnizat DDL-ul real** (2026-07-18).

Verdict: **`Clasificatii.Clsf` EXISTĂ**, ca o coloană generată stocată:

```sql
`Clsf` varchar(255) GENERATED ALWAYS AS
    (concat_ws('.', `Capitol`, `Subcapitol`, `Articol`, `Alineat`)) STORED,
`Titlu` varchar(10) GENERATED ALWAYS AS (left(coalesce(`Articol`,''),2)) STORED,
```

Deci se selectează **direct** `C.Clsf` — nicio compunere în SQL, nicio invenție de
format. Are și index (`idx_Clsf`), deci și `ORDER BY C.Clsf` e ieftin. Formatul
rezultat se potrivește cu datele reale din `FX_System_Export/TABLES/FX_Receptii.md`
(coloana `Clsf` = `65.02.04.02.20.01.03`). Bonus: `Titlu = left(Articol,2)` explică
`Mid(Clsf,13,2)` din interogările Access — `Capitol`(5) + `.` + `Subcapitol`(5) + `.`
= 12 caractere, deci poziția 13 e începutul lui `Articol`.

### Convenția cheilor MariaDB vs Access — regula generală (citește asta prima)

**Aceeași capcană a costat DOUĂ rulări pe felia asta.** Merită scrisă o dată, ca
regulă, nu ca două note separate:

> Când un tabel migrat are **două coloane cu același nume de bază**, cea specifică
> MariaDB e **cheia primară reală**, iar cealaltă păstrează **id-ul Access** venit din
> import. Un port literal al join-ului din Access leagă cheia GREȘITĂ.

| Familie | PK MariaDB | id Access păstrat |
|---|---|---|
| `FX_ORD*` | sufix **„P"** — `FX_ORD_TBL.IDORDTBLP`, `FX_ORD_TBL_REC.IDORDTBLP` | numele fără „P" — `IDORDTBL` |
| `Clasificatii` | `IDClsf` (id-ul „PY") | `IdClsfAcc` |

Cele două lovituri, în ordine:

1. `aggOrd` a fost scris `R.IDORDTBL = T.IDORDTBL` (portare literală din Access).
   Corect: **`R.IDORDTBLP = T.IDORDTBLP`**. Corectat de operator după prima rulare.
2. Cheia spre `Clasificatii` — vezi secțiunea următoare.

**Urmează vederea ORD, care lucrează NUMAI cu familia `FX_ORD`.** Regula de mai sus e
prima verificare de făcut la fiecare join de acolo. Convenția e notată și în capul lui
`sumar.py`, ca să fie sub ochi la următoarea portare.

### A doua capcană, găsită pe drum: cheia de join spre `Clasificatii`

În MariaDB convenția e **inversată** față de Access:

| | Access `ClasificatiiG` | MariaDB `Clasificatii` |
|---|---|---|
| id Access | `IDClsf` (PK) | `IdClsfAcc` |
| id „PY" | `IdClsfPY` | `IDClsf` (PK) |

Dovezi în repo: `routes/clasificatii.py:134` — `SELECT IdClsf as IdClsfPY, IdClsfAcc
as IdClsf`; `mdl_FX_DDF_Salvare.md:65-66` — `rand("IdClsf") = Rs!IdClsfPY` /
`rand("IdClsfAcc") = Rs!IdClsf`.

Un port literal al lui `I.IdClsf = C.IdClsf` ar fi putut lega cheia greșită și ar fi
golit `Clsf` pe TOATE rândurile. S-a ales `I.IdClsf = C.IDClsf` (convenția tabelelor
`FX_` împinse în MariaDB) — **vezi „rămâne neverificat" mai jos.**

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/sumar.py` | **NOU** — query + endpoint |
| `PYTHON/routes/forexe/__init__.py` | înregistrare `from . import sumar` |
| `PYTHON/tests/test_forexe_sumar.py` | **NOU** — 15 teste host-only |

## Deciziile portării (din plan, §2)

1. **Fără filtru SS** — sumarul arată toți indicatorii angajamentului.
2. `ClasificatiiG` → `Clasificatii`, `ParteneriG` → `Parteneri` (nu există tabele „G").
   `Parteneri.DenumirePartener` confirmat în `routes/parteneri.py:146`.
3. Toate predicatele de join pe `IdUnitate` eliminate (o bază = o unitate).
4. **`LEFT JOIN Clasificatii`** (era INNER) — un indicator fără clasificație rămâne în
   grilă, cu `Clsf` gol. Are test dedicat; e motivul principal al feliei.
5. `TotalReceptii = SUM(FX_Receptii.DIF)` — delta, **nu** `Valoare`. Testul pune
   deliberat `Valoare` diferit de `DIF`, ca o „reparație" spre `Valoare` să pice.
6. `INNER JOIN FX_Istoric ... WHERE Descriere = 'Angajament nou.'` — cu **punct final**.
7. `cod` obligatoriu; lipsă/gol → 400 în română.

## Devieri deliberate față de Access

| Ce | De ce |
|---|---|
| `COALESCE(...,0)` pe cele cinci totaluri | Access v1 le lasă `Null`; varianta v2 a operatorului le trece prin `Nz(...,0)`. Grila arată «0,00», nu gol. |
| Filtrul `cod` împins în FIECARE derivată | Access grupa pe tot tabelul apoi făcea join = full scan per agregat per cerere. |
| `aggOrd` restrâns cu `T.CodAngajament = %s` **și** `T.CodAI IN (SELECT CodAI FROM FX_Indicatori WHERE CodAngajament = %s)` | Granulația rămâne pe `CodAI` (join-ul exterior nu se schimbă). Filtrul direct pe `CodAngajament` e posibil pentru că `FX_ORD_TBL` chiar are coloana (`routes/ord/tbl.py:53`) — prima versiune presupunea că nu o are. A doua condiție e redundantă cât timp cele două coloane sunt consistente; se păstrează deliberat, pentru că **nicio constrângere din schemă nu impune consistența lor**. |
| `ORDER BY C.Clsf, I.CodIndicator` adăugat | Access nu are niciunul → grila ar fi instabilă între refresh-uri. |
| `ROUND(...,2)` DOAR pe `TotalRevizii` + `TotalOrdonantari` | Fidelitate: exact ce face Access. Restul rămân `SUM()` simplu; formatarea `N2` e treaba grilei. |

## Corecții după prima rulare pe gazdă (operator, 2026-07-18)

Prima rulare live a feliei a produs trei corecții — toate în cod scris de mine, toate
raportate de operator:

1. **`aggOrd`: `R.IDORDTBL = T.IDORDTBL` → `R.IDORDTBLP = T.IDORDTBLP`.** Vezi
   secțiunea „convenția cheilor" de mai sus. Portare literală din Access = cheia
   greșită.
2. **Fixture-ul folosea `IdUnitate = 0` hardcodat.** `Clasificatii` are
   `FOREIGN KEY (IdUnitate) -> Unitati`, deci insertul pica pe o eroare de
   constrângere, nu pe una de logică. Acum se citește din `Unitati`
   (48 pe această bază, dar **se citește, nu se presupune**), iar curățarea folosește
   ACELAȘI id, ca fixture-ul să nu atingă niciodată rândurile altei unități.
3. **`aggOrd` a primit filtrul suplimentar `T.CodAngajament = %s`** (opțiunea 3 din
   brief), cu join-ul exterior pe `CodAI` păstrat — granulația nu se schimbă.

Notă adăugată în fixture: valorile clasificației de test **nu sunt arbitrare**.
`Clasificatii` are FK-uri către `AVACONT_COMUN.Defa*` **pe coloanele GENERATE**
(`Titlu`, `Articol`, `ClsfE`, `ClsfF`, `SS`), deci combinația Capitol/Subcapitol/
Articol/Alineat trebuie să existe în nomenclatoare. S-a ales una reală, luată din
`FX_System_Export/TABLES/FX_Receptii.md:54`.

## Rezultate teste

`PYTHON/.venv` → `python -m pytest tests/ -q`: **75 passed, 9 skipped, 0 fail/error.**

Cele 15 teste noi din `test_forexe_sumar.py` sunt **host-only și s-au SĂRIT** aici
(`config.py` absent pe stație) — la fel ca `test_forexe_tree.py`. Ele acoperă: fără
sesiune → 401 cu `reason`; `cod` lipsă/gol → 400; diacritice literale în mesajul de
eroare; `cod` necunoscut → 200 cu `header: null, rows: []`; antetul ridicat o singură
dată; forma rândului; un rând per indicator; **indicator fără clasificație apare cu
`clsf` gol**; `clsf` = codul punctat generat; agregatele lipsă = `0`, nu `null`;
însumarea agregatelor; `TotalReceptii` = `SUM(DIF)`; ordonare deterministă.

## Rămâne NEVERIFICAT / amânat

1. **Nimic din felia asta nu a atins o bază reală.** Toate testele sunt host-only și
   se sar în afara gazdei. Aceeași situație ca feliile 0008/0009.
2. **Cheia de join spre `Clasificatii` (`I.IdClsf = C.IDClsf`) este o DECIZIE, nu un
   fapt verificat.** E convenția documentată a tabelelor `FX_` din MariaDB, dar nimeni
   nu a confirmat ce ține efectiv `FX_Indicatori.IdClsf` pe date reale. **Primul lucru
   de verificat la prima rulare live:** dacă `clsf` iese gol pe TOATE rândurile,
   cheia corectă e `C.IdClsfAcc` — se schimbă DOAR în `_SQL` din `sumar.py`.
   `LEFT JOIN`-ul face ca greșeala să fie vizibilă (coloană goală), nu tăcută
   (rânduri dispărute).
3. **Nicio rută din `routes/forexe/` nu scrie `FX_Indicatori.IdClsf`** (verificat prin
   grep). Coloana e populată de push-ul Access, nu de robotul FOREXE. Dacă migrarea
   `FX_` nu a rulat, `IdClsf` poate fi `NULL`/`0` și `Clsf` va fi gol legitim.
4. **Existența tabelelor NU a fost verificată** (`SHOW TABLES` cere o bază live).
   Sumarul atinge unsprezece: `FX_Angajamente`, `FX_Indicatori`, `FX_Istoric`,
   `FX_Rezervari`, `FX_Receptii`, `FX_Plati`, `FX_DDF_REV_SA`, `FX_ORD_TBL`,
   `FX_ORD_TBL_REC`, `Clasificatii`, `Parteneri`. `KBOT_STATUS.md` notează deja că
   **nu există DDL în repo pentru `FX_DDF_REV_SA` și familia `FX_ORD`**, deși
   operatorul confirmă că există live. Un endpoint corect peste tabele goale întoarce
   `rows: []` — **aceea nu e o eroare a feliei**, e migrarea care nu a rulat.
5. Presupunerea „exact un rând `FX_Istoric` cu `Descriere = 'Angajament nou.'` per
   angajament" e confirmată de operator, nu de o constrângere din schemă. Dacă apar
   două, `INNER JOIN`-ul dublează rândurile sumarului.
