# SLICE-0011 — Istoricul Sumar

Reunire documentară D001-26. 01 conține deciziile inițiale; 03 corectează cheia și fan-out-ul după măsurătorile operatorului. Regula IdClsf din iulie este un reper istoric, urmat de contractul 0080 din septembrie, nu o descriere a unei baze migrate astăzi. 02 păstrează clientul și probele sale. Rezultatele și restanțele sunt păstrate pe etapă, fără rulări noi.

[0011-01](#slice-0011-01) · [0011-02](#slice-0011-02) · [0011-03](#slice-0011-03)

<a id="slice-0011-01"></a>

## SLICE-0011-01 — `GET /api/forexe/sumar` (jumătatea de server)

**Data:** 2026-07-18 **Felia:** 0011 (Sumar) — pasul 1 din 2. Pasul 2 = `SLICE-0011-02-sumar-view.md`. **Plan:** lipit în sesiune (secțiunile 1–5, 7).

---

### Ce s-a schimbat și de ce

Prima vedere reală din cele nouă primește sursa de date: portul lui `qFX_MAIN_SUMAR` (v1) pe MariaDB, expus ca `GET /api/forexe/sumar?cod=<CodAngajament>`.

Granulația e **un rând per indicator** al angajamentului. Coloanele de antet (`CodAngajament .. Preluat`) se repetă identic pe fiecare rând în Access; aici sunt ridicate o singură dată în `header`, ca să nu călătorească duplicate pe fir.

#### Blocantul `Clsf` — REZOLVAT, ramura A

Planul cerea explicit ca asta să fie lămurită **citind, nu ghicind**, înainte de a scrie query-ul. Pe stația de dezvoltare nu există `PYTHON/config.py`, deci un `SHOW COLUMNS` live era imposibil, iar în repo nu există DDL pentru `Clasificatii` (`DDL_FX_ListaAngajamente.sql` nu o menționează, iar `FX_System_Export/TABLES/` conține doar tabele `FX_*` — `ClasificatiiG` e tabel legat extern). Munca s-a oprit și s-a raportat; **operatorul a furnizat DDL-ul real** (2026-07-18).

Verdict: **`Clasificatii.Clsf` EXISTĂ**, ca o coloană generată stocată:

```sql
`Clsf` varchar(255) GENERATED ALWAYS AS
    (concat_ws('.', `Capitol`, `Subcapitol`, `Articol`, `Alineat`)) STORED,
`Titlu` varchar(10) GENERATED ALWAYS AS (left(coalesce(`Articol`,''),2)) STORED,
```

Deci se selectează **direct** `C.Clsf` — nicio compunere în SQL, nicio invenție de format. Are și index (`idx_Clsf`), deci și `ORDER BY C.Clsf` e ieftin. Formatul rezultat se potrivește cu datele reale din `FX_System_Export/TABLES/FX_Receptii.md` (coloana `Clsf` = `65.02.04.02.20.01.03`). Bonus: `Titlu = left(Articol,2)` explică `Mid(Clsf,13,2)` din interogările Access — `Capitol`(5) + `.` + `Subcapitol`(5) + `.` = 12 caractere, deci poziția 13 e începutul lui `Articol`.

#### Convenția cheilor MariaDB vs Access — regula generală (citește asta prima)

**Aceeași capcană a costat DOUĂ rulări pe felia asta.** Merită scrisă o dată, ca regulă, nu ca două note separate:

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

**Urmează vederea ORD, care lucrează NUMAI cu familia `FX_ORD`.** Regula de mai sus e prima verificare de făcut la fiecare join de acolo. Convenția e notată și în capul lui `sumar.py`, ca să fie sub ochi la următoarea portare.

#### A doua capcană, găsită pe drum: cheia de join spre `Clasificatii`

În MariaDB convenția e **inversată** față de Access:

| | Access `ClasificatiiG` | MariaDB `Clasificatii` |
|---|---|---|
| id Access | `IDClsf` (PK) | `IdClsfAcc` |
| id „PY" | `IdClsfPY` | `IDClsf` (PK) |

Dovezi în repo: `routes/clasificatii.py:134` — `SELECT IdClsf as IdClsfPY, IdClsfAcc as IdClsf`; `mdl_FX_DDF_Salvare.md:65-66` — `rand("IdClsf") = Rs!IdClsfPY` / `rand("IdClsfAcc") = Rs!IdClsf`.

Un port literal al lui `I.IdClsf = C.IdClsf` ar fi putut lega cheia greșită și ar fi golit `Clsf` pe TOATE rândurile. S-a ales `I.IdClsf = C.IDClsf` (convenția tabelelor `FX_` împinse în MariaDB) — **vezi „rămâne neverificat" mai jos.**

### Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/sumar.py` | **NOU** — query + endpoint |
| `PYTHON/routes/forexe/__init__.py` | înregistrare `from . import sumar` |
| `PYTHON/tests/test_forexe_sumar.py` | **NOU** — 15 teste host-only |

### Deciziile portării (din plan, §2)

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

### Devieri deliberate față de Access

| Ce | De ce |
|---|---|
| `COALESCE(...,0)` pe cele cinci totaluri | Access v1 le lasă `Null`; varianta v2 a operatorului le trece prin `Nz(...,0)`. Grila arată «0,00», nu gol. |
| Filtrul `cod` împins în FIECARE derivată | Access grupa pe tot tabelul apoi făcea join = full scan per agregat per cerere. |
| `aggOrd` restrâns cu `T.CodAngajament = %s` **și** `T.CodAI IN (SELECT CodAI FROM FX_Indicatori WHERE CodAngajament = %s)` | Granulația rămâne pe `CodAI` (join-ul exterior nu se schimbă). Filtrul direct pe `CodAngajament` e posibil pentru că `FX_ORD_TBL` chiar are coloana (`routes/ord/tbl.py:53`) — prima versiune presupunea că nu o are. A doua condiție e redundantă cât timp cele două coloane sunt consistente; se păstrează deliberat, pentru că **nicio constrângere din schemă nu impune consistența lor**. |
| `ORDER BY C.Clsf, I.CodIndicator` adăugat | Access nu are niciunul → grila ar fi instabilă între refresh-uri. |
| `ROUND(...,2)` DOAR pe `TotalRevizii` + `TotalOrdonantari` | Fidelitate: exact ce face Access. Restul rămân `SUM()` simplu; formatarea `N2` e treaba grilei. |

### Corecții după prima rulare pe gazdă (operator, 2026-07-18)

Prima rulare live a feliei a produs trei corecții — toate în cod scris de mine, toate raportate de operator:

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

Notă adăugată în fixture: valorile clasificației de test **nu sunt arbitrare**. `Clasificatii` are FK-uri către `AVACONT_COMUN.Defa*` **pe coloanele GENERATE** (`Titlu`, `Articol`, `ClsfE`, `ClsfF`, `SS`), deci combinația Capitol/Subcapitol/ Articol/Alineat trebuie să existe în nomenclatoare. S-a ales una reală, luată din `FX_System_Export/TABLES/FX_Receptii.md:54`.

### Rezultate teste

`PYTHON/.venv` → `python -m pytest tests/ -q`: **75 passed, 9 skipped, 0 fail/error.**

Cele 15 teste noi din `test_forexe_sumar.py` sunt **host-only și s-au SĂRIT** aici (`config.py` absent pe stație) — la fel ca `test_forexe_tree.py`. Ele acoperă: fără sesiune → 401 cu `reason`; `cod` lipsă/gol → 400; diacritice literale în mesajul de eroare; `cod` necunoscut → 200 cu `header: null, rows: []`; antetul ridicat o singură dată; forma rândului; un rând per indicator; **indicator fără clasificație apare cu `clsf` gol**; `clsf` = codul punctat generat; agregatele lipsă = `0`, nu `null`; însumarea agregatelor; `TotalReceptii` = `SUM(DIF)`; ordonare deterministă.

### Rămâne NEVERIFICAT / amânat

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

---

<a id="slice-0011-02"></a>

## SLICE-0011-02 — `SumarView` (jumătatea de client)

**Data:** 2026-07-18 **Felia:** 0011 (Sumar) — pasul 2 din 2. Pasul 1 = `SLICE-0011-01-sumar-endpoint.md`. **Depinde de:** `KBotDataView` (felia 0010, cod gata / **fără verdict vizual**).

---

### Ce s-a schimbat și de ce

`MainForm.CreateView("sumar")` întorcea `New PlaceholderView("sumar", "Sumar")`. Acum întoarce un `SumarView` real — **prima dintre cele nouă vederi** care nu mai e un placeholder, și primul consumator al lui `KBotDataView`.

Structura oglindește `frmFX_MAIN_Sumar`: un bloc de antet cu datele angajamentului (care în Access sunt textbox-uri legate la coloanele repetate ale query-ului) plus o grilă continuă dedesubt, cu un rând per indicator.

### Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Domain/SumarInfo.vb` | **NOU** — POCO-urile `SumarHeader` / `SumarRow` / `SumarInfo` |
| `src/KBot.Api/UpsertAngajamenteRequest.vb` | + DTO-urile de fir `GetSumarResponse/Header/Row` |
| `src/KBot.Api/IApiClient.vb` | + `GetSumarAsync(cod, ct)` |
| `src/KBot.Api/ApiClient.vb` | + implementarea (URL escapat, mapare wire → POCO) |
| `src/KBot.App/Views/SumarView.Designer.vb` | **NOU** — toate controalele (regula casei) |
| `src/KBot.App/Views/SumarView.vb` | **NOU** — logica vederii |
| `src/KBot.App/MainForm.vb` | `CreateView`: `"sumar"` → `SumarView` |
| `tests/KBot.Api.Tests/ApiClientTests.vb` | + 6 teste `GetSumarAsync` |
| `tests/KBot.App.Tests/SumarViewTests.vb` | **NOU** — 4 teste de comportament |

### Decizii de proiectare

**DI — semnătura constructorului.** Planul avertiza că scaffolding-ul a greșit o dată o semnătură DI. S-a citit `MainForm.vb` înainte: câmpurile reale sunt `_forexeRunner, _session, _apiClient, _authApi, _loginFactory`. `SumarView` ia doar ce îi trebuie: `(apiClient, withReauth)`.

**Plasa 401 rămâne în shell.** `MainForm.WithReauth(Of T)` e `Private`, deci vederea nu o poate apela direct. În loc să dublăm politica de re-login în vedere, se pasează **specializată pe `SumarInfo`**: `Func(Of Func(Of Task(Of SumarInfo)), Task(Of SumarInfo))`, iar `MainForm` o dă ca `Function(op) WithReauth(Of SumarInfo)(op)`. Politica rămâne într-un singur loc.

**snake_case se oprește la fir.** Spre deosebire de `/tree` (care emite PascalCase), `/sumar` emite snake_case. Cum `PropertyNamingPolicy = Nothing`, DTO-urile de fir au proprietăți snake_case verbatim, iar `ApiClient` le mapează pe POCO-uri curate — deci `SumarView` nu vede niciodată `total_rezervari`.

**Coloane de bani = `Text` + `FormatString = "N2"` + aliniere dreapta.** Nu se editează nimic (`ReadOnlyGrid = True`), deci un tip numeric de coloană ar fi degeaba. `FrozenColumnCount = 1` ține Clasificația fixă la stânga la derulare orizontală.

**Reintrare / răspunsuri depășite.** Operatorul poate parcurge arborele rapid, iar răspunsurile pot veni în altă ordine decât cererile. `SumarView` reține codul CERUT (`_requestedCod`) și **aruncă** orice răspuns a cărui cheie nu mai e cea curentă — altfel grila unui angajament ar apărea sub antetul altuia. Are test dedicat.

**Fără apel de rețea pe context gol.** `SetContext(Nothing)` sau `CodAngajament` gol golește antetul + grila și arată starea goală, fără să atingă rețeaua. Are test.

**Erori.** `LoadAsync` e pornit fără `await` din `SetContext` (un handler sincron al shell-ului nu are voie să blocheze firul UI), deci **nu există cine să prindă o excepție** — metoda își tratează singură toate erorile: loghează prin `GlobalErrorLog` și arată mesajul românesc din câmpul `error` al serverului (niciodată JSON brut). `ApplyTheme` loghează și înghite (boundary de pictare/temă), ca în `PlaceholderView`.

**Temă.** Zero culori literale. Grila **nu** se atinge în `ApplyTheme`: `KBotDataView` implementează el însuși `IThemedControl`, iar `ThemeManager.Traverse` ajunge la el.

### Rezultate teste

`dotnet build KBot.sln` — **0 erori, 0 avertismente.** `dotnet test KBot.sln` — **164 trecute, 0 eșecuri** (Api 30 → 36, App 18 → 22).

Cele 6 teste `GetSumarAsync`: URL + bearer + escaparea lui `cod` (spațiu → `cod=A%20100`) + absența oricărui `ss=`; `cod` gol aruncă **înainte** de orice cerere; deserializarea antet + rând; `header: null` → `SumarInfo` gol, nu excepție; textele `null` (`clsf` al unui indicator fără clasificație) devin `String.Empty`; non-2xx → mesaj românesc + `reason`.

Cele 4 teste `SumarViewTests` rulează pe un fir STA (crearea unui `UserControl` instalează un `WindowsFormsSynchronizationContext`, deci continuările `Async Sub` se pompează cu `Application.DoEvents()` — același tipar ca `HarnessTestsRunTest`): `SetContext(Nothing)` nu apelează API-ul și golește grila; cod gol nu apelează API-ul; o selecție cere codul corect și umple grila; **un răspuns depășit pentru un cod înlocuit e aruncat**.

### Rămâne NEVERIFICAT / amânat

1. **NU s-a rulat niciodată vizual.** `SumarView` nu a fost văzut pe ecran de nimeni:
   nici așezarea antetului, nici lățimile coloanelor, nici culorile, nici coloana
   îngheațată la derulare orizontală. Moștenește și avertismentul feliei 0010 —
   **harness-ul vizual al lui `KBotDataView` tot nu a fost rulat**, iar pasul 0010-06
   a găsit un bug (toate anteturile de coloană erau `Nothing`) pe care niciun test
   headless nu l-ar fi prins.
2. **NU s-a rulat împotriva unei baze reale** — vezi `SLICE-0011-01`, punctele 1–5.
   În special: dacă `clsf` iese gol pe toate rândurile, cauza e cheia de join din
   endpoint, nu vederea.
3. Starea goală acoperă grila cu o etichetă; **antetul rămâne vizibil** cu datele
   angajamentului chiar dacă nu are indicatori. Alegere deliberată (operatorul vrea
   să vadă ce angajament e selectat), dar neconfirmată cu operatorul.
4. Nu există indicator de „ocupat" în timpul aducerii datelor — se arată doar textul
   «Se încarcă sumarul…» în locul grilei. Shell-ul are un `KBotBusyBar`, dar vederile
   nu au acces la el; de reevaluat dacă apelurile se dovedesc lente.
5. Celelalte opt vederi rămân `PlaceholderView`.

---

<a id="slice-0011-03"></a>

## SLICE-0011-03 — fix join `Clasificatii` în Sumar (fan-out + cheie greșită)

**Data:** 2026-07-18 **Felia:** 0011 (Sumar) — pasul 3. Pașii 1–2: `SLICE-0011-01`, `SLICE-0011-02`. **Sursa:** rulare pe date reale (`000_DEMO`) de către operator.

---

### Problema

Query-ul Sumar **multiplica rândurile-indicator**. Cifre măsurate pe `000_DEMO`:

```
SELECT COUNT(*) FROM FX_Indicatori;                               -->  29
SELECT COUNT(*) FROM FX_Indicatori WHERE IdClsf <> 0;             -->  25
JOIN Clasificatii ON I.IdClsf = C.IDClsf                          -->   0   (cheie greșită)
JOIN Clasificatii ON I.IdClsf = C.IdClsfAcc                       -->  67   (fan-out multi-unitate)
JOIN ... ON I.IdClsf = C.IdClsfAcc AND I.IdUnitate = C.IdUnitate  -->  50   (fan-out duplicate)
```

Trei defecte suprapuse, toate în cod scris în 0011-01:

1. **Cheie greșită.** `FX_Indicatori.IdClsf` ține **id-ul Access**, deci se potrivește
   cu `Clasificatii.IdClsfAcc`, nu cu `Clasificatii.IDClsf` (PK MariaDB). Zero
   potriviri ⇒ `Clsf` gol pe fiecare rând în producție.
2. **`IdUnitate` scos greșit.** `Clasificatii` din baza per-unitate conține **8
   unități** (48, 75, 76, 121, 123, 135, 136, 157). Regula „drop `IdUnitate`" se
   aplică tabelelor `FX_`, **nu nomenclatoarelor**.
3. **Duplicate în nomenclator.** Chiar cu ambele predicate corecte rămâne fan-out ×2:
   `(75,79)`, `(75,84)`, `(75,90)`, `(75,92)`, `(75,93)` — fiecare de două ori.

### Fixul

`LEFT JOIN Clasificatii` **eliminat**. `Clsf` e doar text afișat, deci se ia prin subinterogare scalară:

```sql
(SELECT C.Clsf FROM Clasificatii C
  WHERE C.IdClsfAcc = I.IdClsf
    AND C.IdUnitate = I.IdUnitate
  LIMIT 1) AS Clsf
```

Un rând per indicator **garantat**, indiferent de duplicate; „fără clasificație → gol" se păstrează. `ORDER BY` mutat pe aliasul de ieșire (`ORDER BY Clsf, I.CodIndicator`), fiindcă aliasul `C` nu mai există.

#### Extindere față de brief: același defect era și la `Parteneri`

Brief-ul cerea doar `Clasificatii`, dar secțiunea „de consemnat" generalizează regula la nomenclatoarele partajate, iar `Parteneri` avea **exact aceeași structură de defect** — `LEFT JOIN Parteneri P ON SA.CodPartener = P.CodPartener`, fără `IdUnitate`. `FX_DDF_REV_SA` **are** `IdUnitate` (`FX_System_Export/TABLES/ FX_DDF_REV_SA.md:26`).

Acolo era **mai grav decât un defect de afișare**: join-ul stătea **înaintea lui `GROUP BY`**, deci un partener duplicat multiplica `SUM(SA.ValCur)` și **umfla `TotalRevizii`** — o cifră de bani greșită, nu o coloană goală. Mutat și el în subinterogare scalară:

```sql
MIN((SELECT P.DenumirePartener FROM Parteneri P
      WHERE P.CodPartener = SA.CodPartener
        AND P.IdUnitate   = SA.IdUnitate
      LIMIT 1)) AS Partener
```

`SUM` rămâne peste rândurile `SA` reale; `MIN` peste subinterogare păstrează semantica originală. **Nu a fost cerut explicit** — motivul extinderii e că e același defect, descoperit prin regula pe care tocmai o primisem, iar a-l lăsa ar fi însemnat livrarea conștientă a unei sume greșite.

### Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/sumar.py` | join `Clasificatii` → subinterogare scalară; join `Parteneri` → subinterogare scalară; `ORDER BY` pe alias; antetul fișierului corectat |
| `PYTHON/tests/test_forexe_sumar.py` | fixtură „murdară" intenționat + 3 teste noi |

### Teste

Fixtura **reproduce defectul**, nu îl presupune:

- aceeași `(IdClsfAcc, IdUnitate)` inserată de **două ori** → duplicat real;
- același `IdClsfAcc` sub **altă** `IdUnitate` → vecin de altă unitate;
- `IdUnitate` real citit din `Unitati` (FK `Clasificatii_ibfk_6`), nu `0`.

⚠️ **Schimbare importantă de fixtură:** `FX_Indicatori.IdClsf` se umple acum cu `CLSF_ACC` (id-ul Access), **nu** cu `cur.lastrowid` (PK-ul MariaDB). Numele coloanei minte — exact defectul 1.

Trei teste noi:

| Test | Ce prinde |
|---|---|
| `test_duplicate_classification_does_not_fan_out` | IND-A apare **o singură dată** deși nomenclatorul are duplicatul, cu `Clsf` populat |
| `test_other_unit_classification_does_not_add_a_row` | exact 2 rânduri; clasificația altei unități nu adaugă niciunul |
| `test_row_count_equals_indicator_count` | plasă generală: `len(rows)` = numărul de indicatori — prinde **orice** join viitor care multiplică |

Ultimul e deliberat mai larg decât brief-ul: defectul a apărut de trei ori pe felia asta, deci merită o santinelă care nu depinde de cunoașterea cauzei.

**Rezultat local:** `75 passed, 9 skipped, 0 fail/error`. Cele 18 teste din `test_forexe_sumar.py` (15 vechi + 3 noi) sunt host-only și **s-au sărit aici** — `config.py` lipsește pe stație. **Verificarea care contează e tot pe gazdă.**

### De consemnat (intrat în `KBOT_STATUS.md`)

**Regula «drop `IdUnitate`» se limitează la tabelele `FX_`.** Nomenclatoarele partajate (`Clasificatii`, `Parteneri` și altele de acest fel) **păstrează** predicatul `IdUnitate` — baza per-unitate le conține pentru mai multe unități.

**Convenții de denumire, ambele confirmate pe schema vie:**

- `Clasificatii`: `IDClsf` = PK MariaDB, `IdClsfAcc` = id Access păstrat.
  **DAR `FX_Indicatori.IdClsf` ține id-ul Access — nu urmează convenția.**
  Numele coloanei nu e o dovadă.
- Familia `FX_ORD`: sufix „P" = PK MariaDB (`IDORDTBLP`), fără „P" = id Access
  (`IDORDTBL`).

**Concluzie operațională, mai utilă decât ambele tabele:** nu deduce cheia din numele coloanei — **numără rândurile înainte și după join**. Un join care întoarce `0` sau mai multe rânduri decât tabelul din stânga e un defect, nu o particularitate a datelor. Cele trei rulări pierdute pe felia asta ar fi fost prinse de un singur `COUNT(*)`.

### Rămâne NEVERIFICAT / fire deschise

1. **Nu am rulat pe date reale** — testele host se sar pe stație. Cifrele din acest
   worklog sunt măsurate de operator, nu de mine.
2. **Duplicatele din `Clasificatii`** (fir deschis, nu blochează felia): de verificat
   dacă rândurile diferă prin `Sursa`/`Sector` — caz în care **lipsește o dimensiune
   din join** și `LIMIT 1` alege arbitrar între două clasificații diferite — sau sunt
   identice, caz în care sunt date de curățat. `LIMIT 1` e corect pentru al doilea caz
   și doar *acceptabil* pentru primul.
3. `MIN(...)` peste subinterogarea `Parteneri` păstrează semantica Access, dar dacă un
   `CodPartener` are mai mulți parteneri pe aceeași unitate, `LIMIT 1` alege arbitrar.
   Nu s-a observat așa ceva; nu a fost verificat.

## Repere unice din registrul 0011

Vechiul focus număra șapte vederi încă PlaceholderView după Sumar și Rezervări, lăsând deschise Tier 1 din 0004 și configurarea VPS din 0003. Cereau proba end-to-end a fixului cu cele 18 teste host-only și existența tuturor celor 11 tabele. DDL-ul Clasificatii furnizat de operator era cerut în repo, pentru a nu-l mai solicita într-un fir nou; ulterior, 0075 consemnează dumpuri locale gitignorate și alte revizii ale coloanelor, fără a transforma acest jurnal într-un inventar actual de scheme.
