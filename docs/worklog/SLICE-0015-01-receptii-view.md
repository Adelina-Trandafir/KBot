# SLICE-0015-01 — Recepții (endpoint + `ReceptiiView`, tree + LISTA)

**Data:** 2026-07-22
**Felia:** 0015 (Recepții) — a treia vedere reală, după Sumar (0011) și Rezervări (0014).
**Depinde de:** `AdvancedTreeControl` + `KBotDataView` (cod gata / **fără verdict vizual**),
plasa `WithReauth` a shell-ului.
**Pass:** 0015-01 (schelet tree + LISTA). Tooltip-ul de recepție = pass 0015-02.

---

## Ce s-a schimbat și de ce

`MainForm.CreateView("receptii")` întorcea `New PlaceholderView(key, "Recepții")`. Acum
întoarce un `ReceptiiView` real — **a treia dintre cele nouă vederi** care nu mai e un
placeholder. Structura oglindește `frmFX_MAIN_REC`: un **master/detail** cu un arbore de
recepții pe **2 niveluri** la stânga (rădăcină = recepția `R`/`IDRR`, nod = antetul
`H`/`IDRH`) și o grilă continuă (LISTA) la dreapta cu detaliul pe clasificații al
antetului selectat.

Construit ca Rezervări (0014): un singur endpoint cititor brut, `WithReauth(Of T)` +
stale-guard pe client, grila = `KBotDataView` read-only, arborele = `AdvancedTreeControl`,
iconițe GDI tematizabile (nu resurse binare).

### Decizii cheie (din sursa Access, nu ghicite)

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

### Devieri față de schița planului (documentate)

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

## Fișiere atinse

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

## Rezultate teste

- **.NET:** `dotnet build KBot.sln` — **0 warnings, 0 errors**. `dotnet test KBot.sln` —
  **tot verde**: Api 48 (was 42, +6), App 37 (was 30, +7), Controls 111, Theming 27,
  Common 7, Domain 1, LocalStore 1.
- **Python:** `test_forexe_receptii.py` — **1 skipped off-host** (host-only, ca surorile);
  `py_compile` pe rută + `__init__` + test — OK.

## Neverificat / amânat (Open threads)

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
