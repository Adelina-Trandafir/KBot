# SLICE-0017-02…04 — Istoricul vederii Plăți

Reunire documentară D001-19. Etapele de mai jos sunt istorice și își păstrează probele/restanțele. 04 înlocuiește arborele și iconițele din 03; acceptarea vizuală din 13.08.2026 privește date de test, fără verdict pe bază reală. Contractul general al totalurilor rămâne separat în [0017-01](SLICE-0017-01-kbotdataview-totals-row.md).

[0017-02](#slice-0017-02) · [0017-03](#slice-0017-03) · [0017-04](#slice-0017-04)

## SLICE-0017-02 — server: `GET /api/forexe/plati?cod=` <a id="slice-0017-02"></a>

Pass 2 of slice 0017 (Plăți). The raw-reader endpoint the client (pass 03) shapes into the tree + LISTA + bank-statement detail pane.

### What changed and why

New `PYTHON/routes/forexe/plati.py`, registered in `routes/forexe/__init__.py`. `@require_session`, one parameter `cod` (`CodAngajament`); no `db_name` / `id_unitate` — the database is the session's (one database = one unit), exactly like `receptii.py`.

One row per `FX_Plati` record, each carrying its bank statement (`FX_Extrase`) and an `are_ord` flag. The client derives the three-level tree, the filtered LISTA and the detail pane from this single reader.

### Decisions taken / deviations from the plan

- **Classification follows 0011-03 via `FX_Indicatori`.** This overrides plan SQL `WHERE c.<clsf_pk> = p.IdClsf` under its Step 0 instruction to match `sumar.py` / `receptii.py`: P → I on PK `CodAI` (1:1) → scalar `Clasificatii` on `IdClsfAcc = I.IdClsf AND IdUnitate = I.IdUnitate`, `LIMIT 1`. `FX_Indicatori.IdClsf` is the verified Access id; `FX_Plati.IdClsf` direction remains unverified. Returns nomenclator `clsf` / `denumire` plus raw `FX_Plati.Clsf` as `clsf_plata`, used when `clsf` is empty.
- **`LEFT JOIN FX_Extrase`, not the Access `FX_Extrase_H INNER JOIN FX_Extrase`.** The Access detail query inner-joins the statement header but selects nothing from it; an inner join would drop a payment whose extras row has no header. Left join keeps the payment (detail pane shows its empty state).
- **`are_ord` against a `DISTINCT` derived table** (`SELECT DISTINCT IdPlataFX FROM FX_ORD_TBL_REC`), so a payment on several ordonanțare lines cannot duplicate the payment row. Tested explicitly (`test_are_ord_not_duplicated_by_multiple_ord_lines`).
- **`ORDER BY P.Data_plata, P.IdPlataFX`** — the client relies on it for month/day/leaf order and for picking the *oldest* un-ordonanțat day (the «+»). `IdPlataFX` is the stable tiebreaker.
- **`data_doc` passed as a raw string, not ISO.** `FX_Extrase.DataDoc` is a `varchar` in the schema (not a date), so `_iso` would mangle it — it goes through untouched. `Data_plata` / `DataBanca` are real `DATETIME` → date-only ISO (the tree groups on the day).
- **Money coalesced to `0.0` server-side** (`suma`, `suma_debit`, `suma_credit`), booleans as real booleans, `ensure_ascii=False`. A DB error returns a reason-coded 500 with a Romanian message, never an empty list.

### Files touched

`PYTHON/routes/forexe/plati.py` — **new** route. `PYTHON/routes/forexe/__init__.py` — register `plati`. `PYTHON/tests/test_forexe_plati.py` — **new**, host-only, skips off-host.

### Test results

- `py_compile` clean on `plati.py`, `__init__.py`, `test_forexe_plati.py`.
- Full offline Python suite: **75 passed, 13 skipped**, 0 fail/error. `test_forexe_plati.py` skips off-host at collection (no `config.py`), as designed.

### Left unverified / deferred

- **No live database call.** All 15 tests are host-only and skip off-host. Host confirmation must cover seven tables/columns and populated `clsf`; the join risk and `clsf_plata` fallback are detailed above (0011-03). Historical command: `python -m pytest tests/test_forexe_plati.py` on the Flask host.
- **`FX_Extrase` 1:1 with `Referinta` is an operator statement, not a schema constraint.** The guard is `test_row_count_equals_fx_plati_count` (returned rows == `COUNT(*)` from `FX_Plati`); if it fails on real data, the `LEFT JOIN FX_Extrase` is fanning out and needs a de-dup.

---

## SLICE-0017-03 — client: `PlatiView` <a id="slice-0017-03"></a>

Pass 3 of slice 0017 (Plăți). The fourth real view — after Sumar (0011), Rezervări (0014), Recepții (0015) — replacing `MainForm.CreateView("plati")` → `PlaceholderView`.

### What changed and why

A master/detail view: an `AdvancedTreeControl` on the left, a `KBotDataView` LISTA top-right, and a bank-statement detail pane bottom-right. Read-only. Data comes from `GET /api/forexe/plati` through the shell's 401 net (`WithReauth(Of PlatiInfo)`) with the `_requestedCod` stale-guard.

- **Tree — three levels.** `« TOATE PLĂȚILE »` root (Σ over all rows, collapsed, first) → month roots (one per year/month, chronological, Σ month) → day leaves (one per calendar day, **all** rows of that day merged into one node, Σ day) → payment nodes (one per `IdPlataFX`, caption = `NrOP` falling back to `ReferintaTrezor`, value = `Suma`). This revives the dormant Access `Level = 2` handlers (`Show_Plati` only built two levels).
- **LISTA — filter, not aggregate.** Selecting any node shows exactly its rows (ALL → every row; month → month rows; day → day rows; payment → that one row). Columns: Clasificație (Count), Plătitor, Nr. doc, Data plății, Suma (Sum, `ro-RO` money, right-aligned). Totals row on (`ShowTotalsRow = True` from pass 01). `clsf` falls back to `clsf_plata` (`ClsfEfectiv`); Plătitor comes from the row's extras (`platitor_nume`), matching the Access LISTA.
- **Detail pane — bank statement.** Driven by the grid's row-selection event, from data already on the row (no second network call). Ten read-only labelled fields. No extras (`Idfxe` null) → `Fără extras bancar asociat.`; no grid row selected → `Selectați o plată.`
- **The «+» — exactly one day.** The earliest `DataPlata` day containing a row with `AreOrd = False` is marked, and only: that day leaf, its parent month root (Access: `cLeaf.ParentNode.IconRight = "Plus"`), and each level-2 node under that day whose row has `AreOrd = False`. Nothing else, ever. Right-icon clicks raise dormant events (`AdaugaOrdonantariCerut` level 0, `AdaugaOrdonantareCerut` levels 1/2), no subscriber.

### Decisions / assumptions (flagged as the plan requires)

- **Day/month state icon (no Access source — assumption):** *any* row `Incarcat` → up; else *any* row `Preluat` → down; else neutral. Per-row level-2 nodes use the Access rule directly.
- **Day/month INCASARE colouring (assumption):** green (from `palette.SuccessColor`, not `RGB(0,100,0)`) only when *every* row of the day/month is INCASARE. Per-row level-2 nodes are green iff that row is INCASARE (Access per-row). The Access source only colours the per-row leaf; month/day merged colouring is the operator interpretation, stated here.
- **Months get a state icon and can be green** — the plan's icon assumption names "month roots", so months carry the merged state icon; I extended the same all-INCASARE colour rule to months for consistency (Access never colours roots, so this is additive — stated).
- **Plăți «+» has no green variant** (unlike Rezervări) — Access uses only `"Plus"` for Plăți. `PlatiIcons.PlusIcon` is single-tint (accent).

### Files touched

- `src/KBot.Domain/PlatiInfo.vb` — **new**: `PlataRow`, `ExtrasBancar`, `PlatiInfo`, with `ClsfEfectiv` / `EtichetaPlata` / `EsteIncasare` helpers.
- `src/KBot.Api/UpsertAngajamenteRequest.vb` — `GetPlatiResponse` / `GetPlataRow` (flat extras).
- `src/KBot.Api/IApiClient.vb` — `GetPlatiAsync`.
- `src/KBot.Api/ApiClient.vb` — implementation (escaped URL; flat extras folded into a nested `ExtrasBancar`, Nothing when `idfxe` is null).
- `src/KBot.App/PlatiIcons.vb` — **new** GDI icons (up / down / neutral / «+» / «toate»).
- `src/KBot.App/Views/PlatiView.Designer.vb` — **new** (outer split + inner split + detail TableLayoutPanel; all controls declared here).
- `src/KBot.App/Views/PlatiView.vb` — **new** view logic + `LunaAnEventArgs` / `PlataOrdEventArgs`.
- `src/KBot.App/MainForm.vb` — `CreateView("plati")` → `PlatiView`.
- Tests: `GetPlatiAsync` stub added to the three App `FakeApiClient`s; `tests/KBot.Api.Tests/ApiClientTests.vb` (+5 GetPlati); `tests/KBot.App.Tests/PlatiViewTests.vb` (**new**, 11).

### Test results

- Full solution: **build 0 warnings / 0 errors**; all test projects green — Api 48→53 (+5), App 39→50 (+11), Controls 134, Theming 27, Common 7, Domain 1, LocalStore 1.

### Left unverified / deferred

- **`PlatiView` has never been rendered on screen**, like every view before it, and `KBotDataView`'s visual harness (incl. the new pass-01 totals band) is **still unrun**. Run DevHarness before trusting the paint, especially the totals band and the tree icons/colours.
- Pass 02 remains unrun on a live database; its classification risk/fallback are recorded in that checkpoint above.
- The ordonanțare workflow itself is out of scope — the `«+»` events are raised and nothing listens (dormant, as in Access this slice).

---

## SLICE-0017-04 — `PlatiView`: arborele pe două niveluri, iconițe din designer, crash la activare <a id="slice-0017-04"></a>

Pasul 4 al feliei 0017 (Plăți). Prima felie a vederii Plăți care a fost **văzută pe ecran și acceptată de operator** (2026-08-13) — 0017-03 era verde la compilare, dar nu fusese niciodată rulată. Numărul 04 era rezervat în STATUS pentru „fix Rezervări «+» (one leaf, not all)"; operatorul l-a realocat acestei reveniri asupra Plăților, deci **acel element rămâne nefăcut și fără număr** (vezi „Rămas nefăcut").

### Ce s-a schimbat și de ce

#### 1. Excepția la activarea vederii (defectul raportat)

Click pe «Plăți» în bara de navigare arunca, din constructor:

```
System.ArgumentException: Cheie de coloană duplicată: 'clsf'. (Parameter 'key')
   at KBot.Controls.KBotDataView.AddColumn(...)  KBotDataView.vb:129
   at KBot.App.PlatiView.BuildColumns()          PlatiView.vb:119
   at KBot.App.PlatiView..ctor(...)              PlatiView.vb:67
   at KBot.App.MainForm.CreateView(String key)   MainForm.vb:384
```

O trecere prin designerul Visual Studio autorase **toate cele cinci coloane** ale grilei în `PlatiView.Designer.vb` (`clsf`, `platitor`, `nrdoc`, `data`, `suma`, cu fonturi, aliniere, lățimi și iconițe de filtru). `BuildColumns()` le adăuga a doua oară la construcție, iar regula casei „fără no-op tăcut" transforma cheia duplicată în `ArgumentException` — care dobora tot lanțul `CreateView` → `ActivateView` → `navViews_SelectionChanged`.

`BuildColumns()` a fost **șters**: designerul deține coloanele. Constantele `COL_*` rămân, cu un comentariu care spune explicit că sunt singura legătură dintre coloanele autorate și `FillGrid` și că trebuie să rămână identice cu cheile din designer.

#### 2. Arborele — DOUĂ niveluri, frunza = ZIUA

Arborele avea patru straturi (`« TOATE PLĂȚILE »` → lună → zi → plată). Acum are două:

- **rădăcină = luna** (`m_<an*100+lună>`, etichetă `MonthLabel`, Σ lună);
- **frunză = ziua** (`d_yyyyMMdd`, etichetă `dd.MM.yyyy`, Σ zi), care strânge **toate** plățile zilei într-un singur nod.

Rădăcina `« TOATE PLĂȚILE »` și nodul per plată (`IdPlataFX`) au dispărut. Fiecare nod poartă în `Tag` rândurile lui, deci LISTA rămâne **filtru, nu agregat**: click pe o zi cu două plăți arată două rânduri.

Consecință pe evenimente: `Tree_RightIconClicked` a pierdut `Case 2`. Nivelul 1 e acum ziua, deci ridică `AdaugaOrdonantareCerut(-1, data)` — exact semantica Access de nivel 1 (`-1` = „toată ziua, nu o plată anume"). Ramura Access de nivel 2 nu mai are nod care s-o ridice.

#### 3. Iconițele vin din `image_list`-ul vederii

Designerul poartă un `ImageList` numit `image_list`, legat prin `tree.NodeImages`, cu trei chei: `month`, `up`, `down`. Nodurile se desenează din ele:

- **luna** → `month` (folderul). Luna **nu mai** poartă iconiță de stare și **nu mai** e colorată verde — oglindește Access, unde rădăcina de lună era `FolderClosed`/`FolderOpen`, nu o stare.
- **ziua** → starea **merjată** a plăților ei: orice `Incarcat` → `up`, altfel orice `Preluat` → `down`, altfel neutru. Merjarea e a noastră, nu a Access-ului: Access desena starea per plată, dar frunza noastră e ziua.

Verdele INCASARE stă pe zi și doar când **toate** plățile ei sunt încasări (o zi mixtă nu e verde). Cheile `neutru` și `plus` **nu există încă** în `image_list`, deci acele două cad pe formele GDI din `PlatiIcons`, care se re-tintează pe paletă.

#### 4. Rădăcinile strânse, în afară de cea cu «+»

Cerință de operator, o abatere deliberată de la Access (unde tot ce se construia avea `Expanded = False`): lunile pornesc strânse, **cu excepția** celei care poartă «+», care pornește deschisă. «+» însuși urmează Access: îl primește cea mai veche zi ne-ordonantată (`AreOrd = False`) și luna care o conține — `cLeaf.IconRight` urmat de `cLeaf.ParentNode.IconRight`.

#### 5. Defect găsit pe drum: panoul de detaliu era mort

Independent de crash. `HEAD` avea zece apeluri `InitDetailPair` **în interiorul lui `InitializeComponent`**; o revenire prin designerul Visual Studio a regenerat metoda și **le-a șters pe toate zece**. Cele zece perechi etichetă/valoare rămâneau construite și denumite, dar niciodată atașate lui `detailTable` — panoul de detaliu ieșea gol pe ecran, tăcut.

Cablarea s-a mutat într-un `BuildDetailRows()` chemat din constructor, unde designerul nu ajunge. **Lecția, notată aici fiindcă va mai mușca:** orice cod scris de mână în `InitializeComponent` se pierde la primul dute-vino prin designer.

### Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.App/Views/PlatiView.vb` | `BuildColumns` șters; `BuildTree` pe două niveluri; `StareIconOf`/`MergedStare`/`AllIncasare`/`LunaIcon` pe `image_list`; `BuildDetailRows`; `Tree_RightIconClicked` fără `Case 2`; `MonthYearLabel`/`StareOf` (moarte) șterse |
| `src/KBot.App/Views/PlatiView.Designer.vb` | (trecerea operatorului) cele cinci coloane + `image_list` + `tree.NodeImages` |
| `src/KBot.App/Views/PlatiView.resx` | fluxul `image_list.ImageStream` |
| `tests/KBot.App.Tests/PlatiViewTests.vb` | rescrise pe contractul de două niveluri (16 teste) |

### Rezultate de test

- `dotnet build KBot.sln` — **0 erori, 0 avertismente**.
- `dotnet test --filter FullyQualifiedName~Plati` — **16/16 verzi**.
- Teste noi: `Tree_TwoLevels_MonthThenDay`, `Months_Collapsed_ExceptTheOneCarryingPlus`, `Months_AllCollapsed_WhenNoPlusAnywhere`, `NodeIcons_ComeFromTheDesignerImageList`, `IncasareColouring_MixedDay_IsNotGreen`.

Trei capcane de test, notate fiindcă sunt generale:

1. **`Type.GetMethod` e sensibil la majuscule, VB nu.** `ClickNode` căuta prin reflexie `"tree_NodeMouseUp"`, dar handler-ul fusese redenumit `Tree_NodeMouseUp` → `m` era `Nothing` → `NullReferenceException` care arăta ca un defect de vedere. Acum cu `BindingFlags.IgnoreCase` plus un mesaj explicit dacă tot nu-l găsește.
2. **`Throw failure` resetează stiva.** `RunSta` re-arunca excepția din firul STA cu `Throw failure`, deci fiecare eșec raporta linia din `RunSta`, nu locul real. Acum prin `ExceptionDispatchInfo.Capture(...).Throw()`.
3. **`ImageList.Images(i)` întoarce un `Bitmap` NOU la fiecare acces**, deci `Assert.Same` nu poate trece niciodată pe iconițele din listă. Comparația se face pe pixeli (`SamePixels`), plus o aserțiune că `up` și `down` chiar diferă — altfel testul ar trece degeaba.

### Verificare vizuală

**Văzut pe ecran și acceptat de operator, 2026-08-13.** Prima acceptare vizuală a vederii Plăți. Tot ce ține de date rămâne neverificat: vederea n-a fost **niciodată rulată pe o bază reală**, deci forma arborelui a fost confirmată doar pe datele de test.

### Rămas nefăcut / de verificat

- **Plăți n-a rulat niciodată pe o bază de date reală** — `GET /api/forexe/plati` neverificat live.
- **`image_list` n-are cheile `neutru` și `plus`**; ambele cad pe formele GDI. De adăugat dacă operatorul le vrea din resurse.
- **`nrdoc` și `data` sunt `ValueType.DateTime` în designer**, dar `FillGrid` scrie **șiruri** în ele (`r.NrOP`, `ShortDate(...)`). `NrOP` ca dată pare o scăpare de copy-paste la trecerea prin designer. Lăsate neatinse — sunt valori autorate, deci alegerea operatorului.
- **„fix Rezervări «+» (one leaf, not all)"**, planificat în STATUS ca 0017-04, **nu s-a făcut** și a rămas fără număr, fiindcă 04 a fost realocat aici. Are nevoie de un număr nou.
- Butoanele de sortare din arbore sunt tot un `MsgBox` de rezervă (moștenit din 0022).

## Repere unice din registrul 0017

Planul inițial a fost furnizat în sesiune. Registrul consemna pentru totaluri creșterea Controls 120 → 134 (+14). Focusul 0017-04 mai cerea controlul cablării din `IstoricView` și `DdfView`, ale căror designere fuseseră regenerate în aceeași rundă: același risc `InitDetailPair`/`InitializeComponent` ca la Plăți. Această restanță nu este închisă prin reunirea documentelor. Fixul Rezervări «+» rămâne nefăcut, fără număr, după realocarea lui 0017-04.
