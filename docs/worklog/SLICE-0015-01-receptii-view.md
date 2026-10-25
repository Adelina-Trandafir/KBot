# SLICE-0015 — Istoricul Recepții

Reunire documentară D001-26. 03 înlocuiește arborele recepție→antet cu lună→recepție→antet, LISTA doar pe antet cu agregarea la orice nivel, descrierea antetului cu denumirea clasificației și fereastra plăților până la prima recepție a lunii următoare. Etapele 01/02 rămân explicate ca variante anterioare. Rezultatele și restanțele sunt păstrate pe etapă, fără rulări noi.

[0015-01](#slice-0015-01) · [0015-02](#slice-0015-02) · [0015-03](#slice-0015-03)

<a id="slice-0015-01"></a>

## SLICE-0015-01 — Recepții (endpoint + `ReceptiiView`, tree + LISTA)

**Data:** 2026-07-22 **Felia:** 0015 (Recepții) — a treia vedere reală, după Sumar (0011) și Rezervări (0014). **Depinde de:** `AdvancedTreeControl` + `KBotDataView` (cod gata / **fără verdict vizual**), plasa `WithReauth` a shell-ului. **Pass:** 0015-01 (schelet tree + LISTA). Tooltip-ul de recepție = pass 0015-02.

---

### Ce s-a schimbat și de ce

`MainForm.CreateView("receptii")` întorcea `New PlaceholderView(key, "Recepții")`. Acum întoarce un `ReceptiiView` real — **a treia dintre cele nouă vederi** care nu mai e un placeholder. Structura oglindește `frmFX_MAIN_REC`: un **master/detail** cu un arbore de recepții pe **2 niveluri** la stânga (rădăcină = recepția `R`/`IDRR`, nod = antetul `H`/`IDRH`) și o grilă continuă (LISTA) la dreapta cu detaliul pe clasificații al antetului selectat.

Construit ca Rezervări (0014): un singur endpoint cititor brut, `WithReauth(Of T)` + stale-guard pe client, grila = `KBotDataView` read-only, arborele = `AdvancedTreeControl`, iconițe GDI tematizabile (nu resurse binare).

#### Decizii cheie (din sursa Access, nu ghicite)

- **Arbore 2 niveluri, NU 3.** `qFX_MAIN_REC_TREE` = `R INNER JOIN H ON IDRR`, coloane R+H.
  În `frmFX_MAIN_REC.Show_Receptii` linia care ar umple frunzele pe clsf (`AggregateRow
  nodeAgg, Mid(RC!clsf,7), …`) e **comentată**, împreună cu acumularea `valAsoc`. Deci
  frunzele pe clsf și tabelele-tooltip de nod/frunză sunt dormante — replicăm arborele viu
  pe 2 niveluri. Nicio frunză pe clsf.
- **Detaliul clsf trăiește în LISTA, nu în arbore** (`qFX_MAIN_REC_LISTA_IND`): un
  `UNION ALL` de un rând-total sintetic (`IDR=-1`, `Valoare = Sum(FX_Receptii.DIF)` pe
  antet) + rânduri per clsf (`Sum(Valoare)` grupat pe clsf, cu `FX_Indicatori.NrCrt` +
  eticheta clasificației). LISTA e condusă de **selecția unui antet**; pe rădăcină se
  **golește** grila (Access conduce LISTA doar la nivel de antet — `mcTree_Click` cu
  `IDRH=-1`).
- **Iconița rădăcinii reflectă starea** (finding 5): `Incarcat` → „sus", altfel `Preluat`
  → „jos", altfel „neutru" (Access `REV_SUS`/`REV_JOS`/`REV_NOT`). Anteturile n-au iconiță.
- **Clasificația — același drum ca Sumar 0011-03**, NU se reghicește: prin `FX_Indicatori`
  (`LEFT JOIN` pe `CodAI`), `Clsf` prin **subinterogare scalară `LIMIT 1`** cheiată pe
  `I.IdClsf` (= id Access) + `I.IdUnitate` (nomenclatorul are duplicate reale → un join ar
  fan-out-a). `NrCrt` = `FX_Indicatori.NrCrt`.
- **`LEFT JOIN FX_Receptii`** (nu INNER): un antet fără linii nu are voie să dispară din
  arbore (qFX_MAIN_REC_TREE nu depinde de FX_Receptii). Vine cu un rând având `idr` NULL;
  clientul îl ține în arbore, iar grila lui arată doar randul-total.
- **Un singur endpoint** întoarce `receptii` + `plati` într-un răspuns; clientul modelează
  arborele, LISTA (și, în 0015-02, tooltip-ul). `plati` (`qFX_MAIN_REC_TT_PLATI`:
  `Data_plata, Suma WHERE CodAngajament=cod`, fără alt filtru — confirmat în export) e
  trimis deja din 0015-01 ca jumătatea de client să fie **un singur apel**.

#### Devieri față de schița planului (documentate)

- **Chei de fir snake_case**, nu PascalCase-ul din schița JSON a planului — pentru
  consistență cu `/sumar` și `/rezervari` (același `PropertyNamingPolicy=Nothing`,
  „snake_case se oprește la fir").
- **Coloana „Descriere" a grilei = Descrierea ANTETULUI** (`FX_Receptii_H.Descriere`),
  „Toți indicatorii" pe randul-total. Planul spunea vag „Descriere from the indicator";
  sursa citată (`qFX_MAIN_REC_LISTA_IND`: `Nz([HH]![Descriere],'Toți indicatorii')`, HH =
  FX_Receptii_H) arată clar că e descrierea antetului. Am purtat `descriere_h` în rând.
- **Am adăugat `sters_h` în rând** (antetul), pentru cumulul DIFH din tooltip-ul 0015-02
  (`qFX_MAIN_REC_TT_DIFH` filtrează `Sters=False`, deși arborele NU filtrează Sters). E o
  coloană brută, nu pre-modelare; se trimite din 0015-01 ca 0015-02 să nu ceară alt câmp.
- **Randul-total al grilei apare PRIMUL** (decizie blocată a planului), deși Access îl
  împinge la coadă cu trucul `Nz(HH.DataH, #12/31/2100#)`. Restul grupurilor: pe NrCrt
  (DataH e constant pe un antet).

### Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/receptii.py` | **NOU** — `GET /api/forexe/receptii?cod=` (receptii + plati) |
| `PYTHON/routes/forexe/__init__.py` | + `from . import receptii` |
| `PYTHON/tests/test_forexe_receptii.py` | **NOU** — 15 teste host-only (skip off-host) |
| `src/KBot.Domain/ReceptiiInfo.vb` | **NOU** — `ReceptieRow` / `ReceptiePlata` / `ReceptiiInfo` |
| `src/KBot.Api/UpsertAngajamenteRequest.vb` | + DTO-urile de fir `GetReceptiiResponse/Row/Plata` |
| `src/KBot.Api/IApiClient.vb` | + `GetReceptiiAsync(cod, ct)` |
| `src/KBot.Api/ApiClient.vb` | + implementarea (URL escapat, mapare wire → POCO) |
| `src/KBot.App/ReceptiiIcons.vb` | **NOU** — iconițe GDI de stare (sus/jos/neutru), tematizabile |
| `src/KBot.App/Views/ReceptiiView.Designer.vb` | **NOU** — SplitContainer + tree + grid + lblEmpty |
| `src/KBot.App/Views/ReceptiiView.vb` | **NOU** — logica vederii (tree 2 niveluri + LISTA) |
| `src/KBot.App/MainForm.vb` | `CreateView`: `"receptii"` → `ReceptiiView` |
| `tests/KBot.App.Tests/RezervariViewTests.vb` | + stub `GetReceptiiAsync` pe `FakeApiClient` |
| `tests/KBot.App.Tests/SumarViewTests.vb` | + stub `GetReceptiiAsync` pe `FakeApiClient` |
| `tests/KBot.Api.Tests/ApiClientTests.vb` | + 6 teste `GetReceptiiAsync` |
| `tests/KBot.App.Tests/ReceptiiViewTests.vb` | **NOU** — 7 teste (comportament + shaping) |

### Rezultate teste

- **.NET:** `dotnet build KBot.sln` — **0 warnings, 0 errors**. `dotnet test KBot.sln` —
  **tot verde**: Api 48 (was 42, +6), App 37 (was 30, +7), Controls 111, Theming 27,
  Common 7, Domain 1, LocalStore 1.
- **Python:** `test_forexe_receptii.py` — **1 skipped off-host** (host-only, ca surorile);
  `py_compile` pe rută + `__init__` + test — OK.

### Neverificat / amânat (Open threads)

- **Endpoint-ul NU a atins niciodată o bază reală.** `test_forexe_receptii.py` skip
  off-host; trebuie rulat pe VPS (confirmă că cele opt tabele/coloane există și că join-ul
  prin `FX_Indicatori` dă `Clsf` populat — blank pe fiecare rând ⇒ cheia de join, nu
  vederea, aceeași capcană ca 0011-03).
- **`ReceptiiView` n-a fost randat niciodată pe ecran** (+ harness-ul `KBotDataView` STILL
  unrun). Nu s-au verificat vizual: iconițele de stare, captions-urile arborelui,
  golirea/reumplerea LISTA la click, formatul banilor.
- **Tooltip-ul de recepție (`NewRootPlatiTooltip`) = pass 0015-02** — nefăcut în această
  pasă. `receptii.py` trimite deja `plati` + `difh`/`sters_h`, deci nu va cere alt endpoint.
- Workflow-urile `bModRel` („modificare legături") și crearea de ordonanțări NU sunt în
  scope — felii ulterioare, dacă vreodată.

---

<a id="slice-0015-02"></a>

## SLICE-0015-02 — Recepții: tooltip de recepție (reconciliere recepții/plăți)

**Data:** 2026-07-22 **Felia:** 0015 (Recepții) — pass 02, peste scheletul tree + LISTA din 0015-01. **Depinde de:** 0015-01 (endpoint-ul trimite deja `plati` + `difh` + `sters_h`, deci NU e nevoie de niciun apel de rețea nou).

---

### Ce s-a schimbat și de ce

Am adăugat **tooltip-ul de recepție** pe rădăcinile arborelui (`ReceptiiView`), oglindind `NewRootPlatiTooltip` din `frmFX_MAIN_REC`. E singura agregare vie din formul Access (frunzele pe clsf și tabelele-tooltip de nod/frunză sunt cod comentat — vezi 0015-01).

Per recepție (`IDRR`), în ordinea `DataR`, un tabel cu patru rânduri:

| Rând | Valoare |
|---|---|
| Data recepție | `DataR` |
| Recepții cumulate | `difhCum` — sumă rulantă a `Sum(DIFH)` pe recepție |
| Plăți cumulate | `platiCum` — `Sum(Suma)` pe plăți cu `DataPlata ≤ MaxDataH` |
| Diferență | `difhCum − platiCum`, **roșu dacă <0, albastru dacă >0** |

#### Fidelitate față de sursă (finding 3, verificat în export)

- **`difhCum`**: `DIFH` e per antet, deci se însumează **anteturile DISTINCTE** ale
  recepției (o dată per `IDRH`), apoi se cumulează pe recepții în ordinea `DataR`.
  **Anteturile șterse sunt EXCLUSE** — `qFX_MAIN_REC_TT_DIFH` filtrează `Sters=False`,
  deși arborele (qFX_MAIN_REC_TREE) NU filtrează Sters. De aceea `sters_h` a fost purtat
  în rând din 0015-01.
- **`platiCum`**: cumulativ, **fără limită inferioară** — toate plățile cu `DataPlata ≤
  MaxDataH` al recepției curente (`MaxDataH` = max `DataH` pe anteturile recepției).
  Sursa plăților (`qFX_MAIN_REC_TT_PLATI`) nu filtrează nimic în plus.
- **`valAsoc` NU e folosit** — linia care l-ar acumula (`sAsocR`) e comentată în Access;
  `NewRootPlatiTooltip` are doar patru rânduri și ignoră `asocCum`.
- Anteturile și grila NU primesc tooltip în această pasă (tabelele de nod/frunză sunt
  dormante în Access).

#### Cum e randat

Tooltip-ul se construiește ca **XML `<table>`** (același contract pe care Access îl emitea cu `clsTT_Table.ToXml`) și se pune pe `TreeItem.Tooltip`. La hover, `AdvancedTreeControl` detectează `IsTableXml` și îl randează cu `TooltipTableParser` + `TooltipPopup` (header „Data recepție / Valoare" + 4 rânduri). Culoarea diferenței = `#CC0000` / `#0033CC` (roșu/ albastru), format acceptat de `AdvancedTreeControl.ParseColor`. Textul e XML-escapat.

Nu există endpoint nou: totul se calculează din `_rows` + `_plati` deja încărcate. Reconstrucția pe schimbarea temei (`ApplyTheme` → `BuildTree` → `ComputeRootTooltips`) reface și tooltip-urile.

### Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.App/Views/ReceptiiView.vb` | + `ComputeRootTooltips` + `BuildRootTooltipXml` (+ helperi `SumDistinctAntetDifh`, `MaxDate`, `AppendTtRow`, `XmlEscape`, clasa `ReceptieTtRow`); hook în `BuildTree` |
| `tests/KBot.App.Tests/ReceptiiViewTests.vb` | + 2 teste (tooltip = `<table>` cumulat; diferență negativă = roșu) |
| `docs/worklog/SLICE-0015-02-receptii-root-tooltip.md` | **NOU** — acest worklog |
| `docs/worklog/KBOT_STATUS.md` | rândul 0015 marcat cu 0015-02 DONE |

### Rezultate teste

- **.NET:** `dotnet build KBot.sln` — **0 warnings, 0 errors**. `dotnet test KBot.sln` —
  **tot verde**: App 37→**39** (+2 tooltip), Api 48, Controls 111, Theming 27, Common 7,
  Domain 1, LocalStore 1.
- **Python:** neschimbat (pass 02 nu atinge serverul); `test_forexe_receptii.py` skip
  off-host.

### Neverificat / amânat (Open threads)

- **Tooltip-ul n-a fost văzut pe ecran.** Shaping-ul (cumul, semn, culoare) e testat
  headless, dar randarea reală a tabelului (`TooltipPopup`) + declanșarea pe hover pe
  rădăcină nu au fost verificate vizual — la fel ca tot restul lui 0015 și `KBotDataView`.
- **Endpoint-ul tot n-a atins o bază reală** (vezi 0015-01): cifrele din tooltip depind de
  `DIFH`/`Sters`/`Data_plata` reale, nereproduse numeric offline.
- `bModRel` („modificare legături") + crearea de ordonanțări rămân în afara scope-ului.

---

<a id="slice-0015-03"></a>

## SLICE-0015-03 — Recepții: revizuire operator (arbore lună, LISTA agregată, tooltip dublu)

**Data:** 2026-07-22 **Felia:** 0015 (Recepții) — pass 03, revizuire cerută de operator după 0015-01/02. **Depinde de:** 0015-01 (endpoint + view) și 0015-02 (tooltip).

---

### Ce s-a schimbat și de ce

Operatorul a cerut trei modificări după ce a văzut structura din 0015-01/02:

1. **Arbore pe 3 niveluri** (era 2): **folder lună/an** (rădăcină, grupat pe `DataR`,
   nume românești ca la Rezervări: „Ianuarie/2026") → **recepție** (IDRR, iconiță de stare
   sus/jos/neutru) → **antet** (IDRH). Totalul de pe folderul de lună = suma `SumaAntet` pe
   recepțiile distincte ale lunii.
2. **LISTA agregată la ORICE nivel.** Click pe lună / recepție / antet umple grila cu
   agregatul rândurilor nodului: un rând-total „Toți indicatorii" (`Sum(DIF)`) + un rând per
   clasificație (`Sum(Valoare)`). Nu mai există „click pe rădăcină golește grila".
3. **Tooltip de reconciliere pe LUNĂ și pe RECEPȚIE.** Fereastra de plăți se întinde acum
   până la **prima recepție a lunii URMĂTOARE** (nu ≤ `MaxDataH` al recepției), ca plățile
   dintre antet și luna următoare să conteze la luna curentă. Ultima lună → toate plățile.

#### Decizie explicită (aprobată de operator): coloana „Descriere" = Denumirea clasificației

Pentru că grila agregă acum peste mai multe anteturi (a căror descriere diferă), coloana „Descriere" arată **`Clasificatii.Denumire`** (bine definită per clsf la orice nivel), nu descrierea unui antet. Am adăugat `denumire` la endpoint (aceeași subinterogare scalară ca la Clsf — drumul 0011-03). `descriere_h` (antetul) rămâne pe fir ca rând brut, dar grila nu îl mai folosește.

#### Semantica tooltip-ului (per lună, cronologic pe DataR)

- `difhCum` = sumă rulantă a `Sum(DIFH)` pe recepție (anteturi DISTINCTE, EXCLUZÂND cele
  șterse — `qFX_MAIN_REC_TT_DIFH` filtrează `Sters=False`). **Recepția** = cumul până la ea;
  **luna** = cumul până la ULTIMA recepție a lunii.
- `platiCum(lună M)` = `Sum(Suma)` peste plăți cu `DataPlata < DataR`-ul primei recepții din
  luna următoare (barieră exclusivă). Toate recepțiile aceleiași luni împart aceeași
  fereastră. Ultima lună → toate plățile.
- `Diferență = difhCum − platiCum`, roșu dacă <0, albastru dacă >0. Randat ca `<table>` XML
  pe `TreeItem.Tooltip` (folderul de lună are eticheta „Lună"/„Ianuarie/2026", recepția
  „Data recepție"/dată).

**Efectul fix-ului:** înainte, o plată din 25.01 (după antetul din 19.01) nu se număra la recepția din Ianuarie (fereastra se oprea la `MaxDataH`), deci Diferența arăta ~valoarea recepției. Acum acea plată intră în fereastra lunii Ianuarie (e înainte de prima recepție din Februarie), deci Diferența reflectă golul real recepții−plăți.

### Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/receptii.py` | + subinterogarea `Denumire` + cheia `denumire` în rând; note actualizate |
| `PYTHON/tests/test_forexe_receptii.py` | + `denumire` în `ROW_KEYS` + aserțiune pe denumire |
| `src/KBot.Api/UpsertAngajamenteRequest.vb` | + `denumire` pe `GetReceptieRow` |
| `src/KBot.Domain/ReceptiiInfo.vb` | + `Denumire` pe `ReceptieRow` |
| `src/KBot.Api/ApiClient.vb` | mapează `denumire` → `ReceptieRow.Denumire` |
| `src/KBot.App/Views/ReceptiiView.vb` | arbore 3 niveluri, `FillGridFromRows` (Descriere=Denumire), click pe orice nod, `ComputeTooltips` (lună+recepție, fereastră lună-următoare), helperi `MonthKeyOf`/`MonthYearLabel`/`MonthLabel` |
| `tests/KBot.Api.Tests/ApiClientTests.vb` | + `denumire` în JSON + aserțiuni |
| `tests/KBot.App.Tests/ReceptiiViewTests.vb` | rescrise pentru arbore 3 niveluri, LISTA agregată, tooltip lună+recepție cu fereastra lunii următoare |

### Rezultate teste

- **.NET:** `dotnet build KBot.sln` — **0 warnings**. `dotnet test KBot.sln` — **tot verde**:
  App 39, Api 48, Controls 111, Theming 27, Common 7, Domain 1, LocalStore 1.
- **Python:** `test_forexe_receptii.py` — **1 skipped off-host** (host-only); `py_compile` OK.

### Neverificat / amânat (Open threads)

- **Endpoint-ul tot n-a atins o bază reală**; `denumire` (nouă subinterogare) și cifrele
  tooltip-ului nu au fost verificate pe date reale.
- **Nimic din 0015 n-a fost văzut pe ecran** — arborele pe 3 niveluri, iconițele, LISTA
  agregată la click și tabelele-tooltip pe lună/recepție sunt testate doar headless.
- Decizia „Descriere = Denumire clasificație" e aprobată de operator, dar neverificată
  vizual; dacă preferă gol pe randurile per-clsf, e o schimbare de o linie în
  `FillGridFromRows`.

## Repere ale registrului 0015

Planul era PLAN_ReceptiiView.md. Notele registrului încă spuneau că tooltipul 02 era nefăcut, deși Status și Focus consemnau 02/03 livrate; cronologia etapelor de mai sus delimitează acea amânare inițială.
