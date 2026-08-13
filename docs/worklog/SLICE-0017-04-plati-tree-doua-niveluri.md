# SLICE-0017-04 — `PlatiView`: arborele pe două niveluri, iconițe din designer, crash la activare

Pasul 4 al feliei 0017 (Plăți). Prima felie a vederii Plăți care a fost **văzută pe ecran și
acceptată de operator** (2026-08-13) — 0017-03 era verde la compilare, dar nu fusese niciodată
rulată. Numărul 04 era rezervat în STATUS pentru „fix Rezervări «+» (one leaf, not all)";
operatorul l-a realocat acestei reveniri asupra Plăților, deci **acel element rămâne nefăcut și
fără număr** (vezi „Rămas nefăcut").

## Ce s-a schimbat și de ce

### 1. Excepția la activarea vederii (defectul raportat)

Click pe «Plăți» în bara de navigare arunca, din constructor:

```
System.ArgumentException: Cheie de coloană duplicată: 'clsf'. (Parameter 'key')
   at KBot.Controls.KBotDataView.AddColumn(...)  KBotDataView.vb:129
   at KBot.App.PlatiView.BuildColumns()          PlatiView.vb:119
   at KBot.App.PlatiView..ctor(...)              PlatiView.vb:67
   at KBot.App.MainForm.CreateView(String key)   MainForm.vb:384
```

O trecere prin designerul Visual Studio autorase **toate cele cinci coloane** ale grilei în
`PlatiView.Designer.vb` (`clsf`, `platitor`, `nrdoc`, `data`, `suma`, cu fonturi, aliniere,
lățimi și iconițe de filtru). `BuildColumns()` le adăuga a doua oară la construcție, iar regula
casei „fără no-op tăcut" transforma cheia duplicată în `ArgumentException` — care dobora tot
lanțul `CreateView` → `ActivateView` → `navViews_SelectionChanged`.

`BuildColumns()` a fost **șters**: designerul deține coloanele. Constantele `COL_*` rămân, cu un
comentariu care spune explicit că sunt singura legătură dintre coloanele autorate și `FillGrid`
și că trebuie să rămână identice cu cheile din designer.

### 2. Arborele — DOUĂ niveluri, frunza = ZIUA

Arborele avea patru straturi (`« TOATE PLĂȚILE »` → lună → zi → plată). Acum are două:

- **rădăcină = luna** (`m_<an*100+lună>`, etichetă `MonthLabel`, Σ lună);
- **frunză = ziua** (`d_yyyyMMdd`, etichetă `dd.MM.yyyy`, Σ zi), care strânge **toate** plățile
  zilei într-un singur nod.

Rădăcina `« TOATE PLĂȚILE »` și nodul per plată (`IdPlataFX`) au dispărut. Fiecare nod poartă în
`Tag` rândurile lui, deci LISTA rămâne **filtru, nu agregat**: click pe o zi cu două plăți
arată două rânduri.

Consecință pe evenimente: `Tree_RightIconClicked` a pierdut `Case 2`. Nivelul 1 e acum ziua,
deci ridică `AdaugaOrdonantareCerut(-1, data)` — exact semantica Access de nivel 1 (`-1` =
„toată ziua, nu o plată anume"). Ramura Access de nivel 2 nu mai are nod care s-o ridice.

### 3. Iconițele vin din `image_list`-ul vederii

Designerul poartă un `ImageList` numit `image_list`, legat prin `tree.NodeImages`, cu trei chei:
`month`, `up`, `down`. Nodurile se desenează din ele:

- **luna** → `month` (folderul). Luna **nu mai** poartă iconiță de stare și **nu mai** e colorată
  verde — oglindește Access, unde rădăcina de lună era `FolderClosed`/`FolderOpen`, nu o stare.
- **ziua** → starea **merjată** a plăților ei: orice `Incarcat` → `up`, altfel orice `Preluat` →
  `down`, altfel neutru. Merjarea e a noastră, nu a Access-ului: Access desena starea per plată,
  dar frunza noastră e ziua.

Verdele INCASARE stă pe zi și doar când **toate** plățile ei sunt încasări (o zi mixtă nu e
verde). Cheile `neutru` și `plus` **nu există încă** în `image_list`, deci acele două cad pe
formele GDI din `PlatiIcons`, care se re-tintează pe paletă.

### 4. Rădăcinile strânse, în afară de cea cu «+»

Cerință de operator, o abatere deliberată de la Access (unde tot ce se construia avea
`Expanded = False`): lunile pornesc strânse, **cu excepția** celei care poartă «+», care pornește
deschisă. «+» însuși urmează Access: îl primește cea mai veche zi ne-ordonantată (`AreOrd =
False`) și luna care o conține — `cLeaf.IconRight` urmat de `cLeaf.ParentNode.IconRight`.

### 5. Defect găsit pe drum: panoul de detaliu era mort

Independent de crash. `HEAD` avea zece apeluri `InitDetailPair` **în interiorul lui
`InitializeComponent`**; o revenire prin designerul Visual Studio a regenerat metoda și **le-a
șters pe toate zece**. Cele zece perechi etichetă/valoare rămâneau construite și denumite, dar
niciodată atașate lui `detailTable` — panoul de detaliu ieșea gol pe ecran, tăcut.

Cablarea s-a mutat într-un `BuildDetailRows()` chemat din constructor, unde designerul nu ajunge.
**Lecția, notată aici fiindcă va mai mușca:** orice cod scris de mână în `InitializeComponent` se
pierde la primul dute-vino prin designer.

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.App/Views/PlatiView.vb` | `BuildColumns` șters; `BuildTree` pe două niveluri; `StareIconOf`/`MergedStare`/`AllIncasare`/`LunaIcon` pe `image_list`; `BuildDetailRows`; `Tree_RightIconClicked` fără `Case 2`; `MonthYearLabel`/`StareOf` (moarte) șterse |
| `src/KBot.App/Views/PlatiView.Designer.vb` | (trecerea operatorului) cele cinci coloane + `image_list` + `tree.NodeImages` |
| `src/KBot.App/Views/PlatiView.resx` | fluxul `image_list.ImageStream` |
| `tests/KBot.App.Tests/PlatiViewTests.vb` | rescrise pe contractul de două niveluri (16 teste) |

## Rezultate de test

- `dotnet build KBot.sln` — **0 erori, 0 avertismente**.
- `dotnet test --filter FullyQualifiedName~Plati` — **16/16 verzi**.
- Teste noi: `Tree_TwoLevels_MonthThenDay`, `Months_Collapsed_ExceptTheOneCarryingPlus`,
  `Months_AllCollapsed_WhenNoPlusAnywhere`, `NodeIcons_ComeFromTheDesignerImageList`,
  `IncasareColouring_MixedDay_IsNotGreen`.

Trei capcane de test, notate fiindcă sunt generale:

1. **`Type.GetMethod` e sensibil la majuscule, VB nu.** `ClickNode` căuta prin reflexie
   `"tree_NodeMouseUp"`, dar handler-ul fusese redenumit `Tree_NodeMouseUp` → `m` era `Nothing`
   → `NullReferenceException` care arăta ca un defect de vedere. Acum cu `BindingFlags.IgnoreCase`
   plus un mesaj explicit dacă tot nu-l găsește.
2. **`Throw failure` resetează stiva.** `RunSta` re-arunca excepția din firul STA cu `Throw
   failure`, deci fiecare eșec raporta linia din `RunSta`, nu locul real. Acum prin
   `ExceptionDispatchInfo.Capture(...).Throw()`.
3. **`ImageList.Images(i)` întoarce un `Bitmap` NOU la fiecare acces**, deci `Assert.Same` nu
   poate trece niciodată pe iconițele din listă. Comparația se face pe pixeli (`SamePixels`),
   plus o aserțiune că `up` și `down` chiar diferă — altfel testul ar trece degeaba.

## Verificare vizuală

**Văzut pe ecran și acceptat de operator, 2026-08-13.** Prima acceptare vizuală a vederii Plăți.
Tot ce ține de date rămâne neverificat: vederea n-a fost **niciodată rulată pe o bază reală**,
deci forma arborelui a fost confirmată doar pe datele de test.

## Rămas nefăcut / de verificat

- **Plăți n-a rulat niciodată pe o bază de date reală** — `GET /api/forexe/plati` neverificat live.
- **`image_list` n-are cheile `neutru` și `plus`**; ambele cad pe formele GDI. De adăugat dacă
  operatorul le vrea din resurse.
- **`nrdoc` și `data` sunt `ValueType.DateTime` în designer**, dar `FillGrid` scrie **șiruri** în
  ele (`r.NrOP`, `ShortDate(...)`). `NrOP` ca dată pare o scăpare de copy-paste la trecerea prin
  designer. Lăsate neatinse — sunt valori autorate, deci alegerea operatorului.
- **„fix Rezervări «+» (one leaf, not all)"**, planificat în STATUS ca 0017-04, **nu s-a făcut**
  și a rămas fără număr, fiindcă 04 a fost realocat aici. Are nevoie de un număr nou.
- Butoanele de sortare din arbore sunt tot un `MsgBox` de rezervă (moștenit din 0022).
