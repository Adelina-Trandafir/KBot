# SLICE-0025 — suprafețe editabile din designer (`KBotNavList`, `KBotDataView`, `KBotCaptionBar`, `KBotBusyBar`)

Plan: `PLAN_DesignerSurfaces.md` (lipit în sesiune). Planul presupunea felia **0024**;
`KBOT_STATUS.md` spune «Next free slice number: **0025**» (0024 e luată de profilurile Adobe de pe
fila «Document»), deci felia și worklog-ul poartă **0025**, cum cere §0 al planului.

---

## 0. Reconcilieri față de plan (nu ocolite în tăcere — CODE_WORKFLOW §4)

| # | Ce spune planul | Ce am găsit în repo | Ce am făcut |
|---|---|---|---|
| 1 | «Slice 0024» | 0024 = profiluri Adobe (`SLICE-0024-01/02/03`), next free = 0025 | Felie **0025**, fișierul redenumit corespunzător |
| 2 | «`KBotNavList` currently has **no** xUnit tests at all (open item from slice 0018)» | `tests/KBot.Theming.Tests/KBotNavListSelectionTests.vb` EXISTĂ (4 teste, adăugate de commit-ul `5f94f44` pentru bug-ul `SelectedKey = Nothing` din DdfView) | Firul 0018 rămâne **parțial** deschis: existau teste pentru `SelectedKey`, dar NU pentru separatori / aliniere / vizibilitate / orientare. Felia asta le adaugă (20 de teste noi), deci firul se poate închide acum |
| 3 | «Existing counts before this slice: `KBot.Controls.Tests` 134, `KBot.App.Tests` 136, solution 436 — confirm these numbers against `KBOT_STATUS.md`» | Confirmat contra STATUS + rulare reală: **Controls 240, App 143, soluție 715** (cifrele planului sunt de pe vremea feliei 0022; 0023/0024 au adăugat ~280 de teste) | Folosite cifrele reale |
| 4 | Helper-ul de design-time e descris ca `Friend Shared Function IsDesignTime` | `Friend` nu traversează assembly-uri, iar `KBot.Controls` (alt assembly) are nevoie de el | `Public NotInheritable Class KBotDesignTime` cu `<EditorBrowsable(Advanced)>` — scos din lista comună IntelliSense, dar vizibil pentru `KBot.Controls`. Fără `InternalsVisibleTo` suplimentar |
| 5 | Pasul 1 cere proba rotundei **în Visual Studio**, altfel «stop and report» | Nu pot deschide designer-ul VS din acest mediu | Am construit **proba programatică cea mai apropiată** (vezi §5) și am mers mai departe pe pașii 2–5. **Pasul 6 (verificările manuale) tot NU a rulat.** Pasul 7 s-a oprit inițial din acest motiv (regula planului), apoi **a fost cerut explicit de operator și livrat** — vezi §6 |
| 6 | §9.3: «keep `navViews.SelectedKey = "sumar"` in code, **after** `ApplyViewGating(Nothing)` **as today**» | În cod era ÎNAINTEA lui `ApplyViewGating(Nothing)`, nu după | Păstrată ordinea REALĂ. Ambele funcționează; «as today» descria greșit codul, iar o schimbare de ordine pe care nimeni nu a cerut-o nu merită riscul |

---

## 1. Ce s-a schimbat și de ce, per control

### 1.1 `KBotNavList` (+ `KBotNavItem`, `KBotNavItemCollection`) — `KBot.Theming`

Modelul de element a ieșit din control. Clasa privată imbricată `NavItem` a fost **înlocuită** cu
`KBotNavItem` public, cu constructor fără parametri (cerința dialogului de colecție), toate cele
șapte proprietăți atribuite `Category`/`Description`/`DefaultValue`, și `Bounds` lăsat **`Friend`**
ca designer-ul să nu-l vadă și să nu-l serializeze (e stare derivată, recalculată la fiecare layout).

`ToString()` e motivul pentru care **un singur tip** de element e suportabil: lista din stânga
dialogului de colecție arată `ddf — "DDF" (Far)` pentru un buton și
`──────── separator (Far) ────────` pentru un separator, deci diferența se vede dintr-o privire.
Un element fără cheie apare ca `<fără cheie> — …`. (Decizia 1 din plan: două tipuri într-o
colecție ordonată ar cere un editor de colecție PROPRIU, care cere un assembly de design-time —
în afara scopului.)

`KBotNavItemCollection : Collection(Of KBotNavItem)` cu referință `Friend` la bară; fiecare
`InsertItem`/`SetItem`/`RemoveItem`/`ClearItems` cheamă baza și apoi `Owner?.InvalidateLayout()`.
`Nothing` e respins. **Cheile NU se validează aici**: dialogul inserează elementul în clipa în care
apeși «Add», înainte să fi tastat ceva în el.

Pe control:
- `<ToolboxItem(True)>` + `<DefaultProperty("Items")>` + `<DefaultEvent("SelectionChanged")>`;
- `Implements ISupportInitialize`;
- `Items` public, `<DesignerSerializationVisibility(Content)>`;
- `AddItem`/`AddSeparator`/`SetBadge`/`SetItemEnabled`/`SetItemVisible` — **semnături și
  comportament de rulare NESCHIMBATE**, inclusiv fiecare `ArgumentException`; scriu prin `Items`;
- `FindIndex` **sare peste separatori** ȘI respinge cheia vidă. A doua parte nu e cosmetică:
  `OnKeyDown` cheamă `FindIndex(_selectedKey)` cu `Nothing` când nu e nimic selectat, iar
  `String.Equals(Nothing, Nothing)` = True ar fi potrivit un element autorit în designer cu cheia
  încă goală și ar fi selectat elementul greșit;
- cheile separatorilor: `NextSeparatorKey()` incrementează `_sepSeq` până nimerește o cheie
  **nefolosită de niciun element** (inclusiv de un separator căruia i s-a tastat manual `__sep_1`
  în designer), și e folosită atât de `AddSeparator` cât și de `EndInit`;
- `SelectedKey` între `BeginInit` și `EndInit`: **se reține fără validare** și getter-ul întoarce
  valoarea reținută. Designer-ul nu are nicio obligație să emită `Items` înaintea lui
  `SelectedKey`, iar setter-ul de rulare ar arunca pe o cheie care apare o linie mai jos. În afara
  inițializării, comportamentul e exact cel de azi;
- `EndInit`: (1) dă chei separatorilor fără cheie, (2) validează butoanele (cheie vidă / duplicată
  → `ArgumentException` care numește indexul și cheia, în română cu diacritice reale), (3) aplică
  selecția reținută pe drumul normal. **În designer, (2) și (3) se sar** — dar valoarea reținută
  se scrie totuși în `_selectedKey`, ca să se re-serializeze corect (dacă am ignora-o, designer-ul
  ar pierde-o la următoarea regenerare; asta e o mică abatere de la plan, în plus, nu în minus);
- `OnPaint`, doar design-time: un buton cu cheie vidă sau duplicată primește un **chenar roșu de
  2px** în locul ramei normale. La rulare calea nu se execută niciodată.

### 1.2 `KBotDataColumn` + `KBotDataColumnCollection` + `KBotDataView` — `KBot.Controls`

`KBotDataColumn` primește constructor fără parametri (cel de patru argumente rămâne **identic**,
inclusiv aruncarea pe cheie vidă) și un `Friend Property Owner As KBotDataView`, setat de colecție
la inserare și golit la scoatere.

`Key` și `ColumnType` trec din `ReadOnly` în citire/scriere, dar **aruncă
`InvalidOperationException` când coloana aparține unei grile cu rânduri**: valorile celulelor sunt
ținute pe cheia coloanei în dicționarul lui `KBotDataRow`, deci o redenumire peste date ar orfelina
fiecare celulă stocată și coloana ar picta gol — adică pierdere de date deghizată în no-op.
`Key` cheamă apoi `Owner.OnColumnKeyChanged` (reconstruiește indexul și mută selecția curentă dacă
stătea pe cheia veche); `ColumnType` abandonează editarea deschisă (`CancelEdit`), fiindcă ea
aparține tipului VECHI. Cât `Owner` e `Nothing` — situația designer-ului — ambele sunt libere.

Toată suprafața publică a coloanei e atribuită `Category("K-BOT")`/`Description`/`DefaultValue`.
`IsEffectivelyVisible`, `ComboItems` și `Tag` sunt `Browsable(False)` + `DesignerSerializationVisibility(Hidden)`:
primul e stare derivată, ultimele două **nu pot face rotunda** prin `InitializeComponent` (o
`IList(Of Object)` și un `Object` arbitrar), iar o sursă combo pe jumătate serializată e mai rea
decât niciuna. `UserSized`/`AutoHidden` erau deja `Friend`, deci nu se serializează.

`KBotDataColumnCollection` face pe `KBotDataView`: `RebuildColumnIndex()` + `RecomputeTotals()` +
`LayoutChanged()` la fiecare adăugare/înlocuire/ștergere/golire (prin `OnColumnsChanged`).
**Asta închide jumătate din limitarea consemnată la felia 0013** («fără back-reference
coloană→grilă»): add și remove declanșează layout singure. Editarea PROPRIETĂȚILOR unei coloane
după încărcare se comportă ca azi și tot are nevoie de `AutoSizeColumns()` explicit.
Cheile duplicate se **sar în tăcere la reconstrucția indexului** — validarea e la `AddColumn` și la
`EndInit`, din același motiv ca la navlist.

`KBotDataView`: `Implements ISupportInitialize` (`EndInit` validează cheile, se sare în designer),
`Columns` trece de la `IReadOnlyList(Of KBotDataColumn)` la `KBotDataColumnCollection` (atribuit
`Content`), `AddColumn` își păstrează semnătura, cele două aruncări și valoarea întoarsă dar nu mai
duplică `_columnIndex(key) = col` / `RecomputeTotals()` / `LayoutChanged()` (vin de la colecție).
`Column(key)` neatins. Proprietățile de grilă au primit `Category`/`Description`/`DefaultValue`;
`Rows`, `RowCount`, `Item`, `CurrentRowIndex`, `CurrentColumnKey`, `CurrentRow`, `IsEditing` sunt
`Browsable(False)` + `Hidden` — sunt DATE de rulare, iar un designer care le-ar scrie în
`InitializeComponent` ar îngheța în formular datele de test ale cuiva.

**Un defect de rotundă prins pe drum, care nu e în plan:** `TotalsRowHeight` NU putea primi
`<DefaultValue(0)>`. Getter-ul lui întoarce `HeaderHeight` cât timp banda e pe «urmărește antetul»,
deci designer-ul ar fi serializat numărul REZOLVAT (30) și ar fi fixat banda pentru totdeauna — o
rotundă care schimbă tăcut înțelesul. Rezolvat cu `ShouldSerializeTotalsRowHeight` /
`ResetTotalsRowHeight`, pe care designer-ul le respectă înaintea oricărui `DefaultValue`. Test dedicat.

### 1.3 `KBotCaptionBar` / `KBotBusyBar`

Ambele trec pe `<ToolboxItem(True)>` și primesc `Category`/`Description`/`DefaultValue` pe
`IconImage`, `ShowMinimize`, `ShowMaximize`, respectiv `Running`. `Text` (moștenit) rămâne neatins.

`KBotBusyBar.Running`: când `KBotDesignTime.IsDesignTime` e True, valoarea se reține, se cere
repictare, și **cronometrul nu se atinge**. Un timer la 15 ms care ticăie în procesul designer-ului
repictează suprafața la infinit și arde CPU în `devenv`/`DesignToolsServer`. `Dispose` rămâne
corect pe calea asta (opreșterea unui timer nepornit e no-op) — verificat prin citire.

**`IconImage` NU se setează în designer-ul lui `MainForm`/`LoginForm`.** `MainForm.vb:163` face
`capBar.IconImage = My.Resources.kbot_64` în cod și acea atribuire câștigă la rulare; setarea și în
designer ar copia imaginea a doua oară în `.resx`-ul formularului, degeaba.

### 1.4 Helper-ul comun de design-time

`KBotDesignTime.IsDesignTime(c)` întoarce True dacă: `LicenseManager.UsageMode = Designtime`; sau
`c.Site.DesignMode`; sau un părinte urcând lanțul e sitat în design mode; sau procesul curent e
unul dintre `devenv` / `DesignToolsServer` / `Microsoft.VisualStudio.DesignTools.DesignToolsServer` /
`XDesProc` / `Blend`. `Control.DesignMode` singur nu ajunge: e False în constructor și False pentru
un control imbricat în alt control.

**Toate** apelurile `GlobalErrorLog.Write` din pictarea / mouse-ul / tastatura celor patru controale
sunt acum sărite la design time (11 locuri). `GlobalErrorLog` scrie în
`AppContext.BaseDirectory\Logs\harness_errors.log`, adică **lângă `devenv.exe`** când rulează în
designer — zgomot în cel mai bun caz, sursă de excepții în cel mai rău.

**Abatere deliberată de la regula casei, consemnată:** `IsDesignTime` și `DetectDesignerProcess`
prind excepția și întorc `False` **fără să logheze**. Sunt singurele două locuri din felie care fac
asta, și motivul e că logarea e exact lucrul pe care predicatul îl suprimă; comentariul din cod o
spune la fața locului.

---

## 2. Fișiere atinse

**Noi**
- `src/KBot.Theming/KBotDesignTime.vb`
- `src/KBot.Theming/KBotNavItem.vb` (`KBotNavItem` + `KBotNavItemCollection`)
- `src/KBot.Controls/KBotDataColumnCollection.vb`
- `tests/KBot.Theming.Tests/KBotNavListTests.vb`
- `tests/KBot.Controls.Tests/KBotDataColumnDesignerTests.vb`
- `docs/worklog/SLICE-0025-designer-surfaces.md` (acesta)

**Modificate**
- `src/KBot.Theming/KBotNavList.vb` — model extern, `Items`, `ISupportInitialize`, `FindIndex`,
  chei de separator, marcaj roșu, cârlige `Friend` pentru teste
- `src/KBot.Theming/KBotCaptionBar.vb`, `src/KBot.Theming/KBotBusyBar.vb`
- `src/KBot.Controls/KBotDataColumn.vb` (rescris), `src/KBot.Controls/KBotDataView.vb`
- `src/KBot.Controls/KBotDataView.{AutoSize,Layout,Input,Editing}.vb` — atribute + `Imports System.ComponentModel`
- `src/KBot.Theming/KBot.Theming.vbproj` — `FileVersion` 1.0.0.0 → **1.1.0.0**
- `src/KBot.Controls/KBot.Controls.vbproj` — `FileVersion` 1.2.0.0 → **1.3.0.0**
- `src/KBot.App/MainForm.Designer.vb` — cele opt intrări `navViews.Items` + `BeginInit`/`EndInit` (pasul 7)
- `src/KBot.App/MainForm.vb` — blocul `AddItem`/`AddSeparator` șters din calea de încărcare (pasul 7)
- `src/KBot.App/KBot.App.vbproj` — `FileVersion` 1.0.5.0 → **1.0.6.0** (pasul 7)
- `tests/KBot.App.Tests/MainFormNavItemsTests.vb` (nou, pasul 7)
- `docs/worklog/KBOT_STATUS.md`

**`AssemblyVersion` rămâne 1.0.0.0 în ambele.** Pentru `KBot.Theming` e evident (suprafața doar
crește). Pentru `KBot.Controls` **NU** e evident și merită spus pe față: `Columns` **și-a schimbat
tipul declarat** din `IReadOnlyList(Of KBotDataColumn)` în `KBotDataColumnCollection`, ceea ce e o
rupere la compilare pentru orice cod care îl ținea ca `IReadOnlyList`. Toată soluția se
recompilează împreună și niciun apelant nu îl ținea așa (verificat prin grep la pasul 0: cele 11
locuri folosesc doar `.Count`, indexatorul și `For Each`, toate oferite de `Collection(Of T)`),
deci `AssemblyVersion` stă pe loc — dar contractul manifestului cere ca decizia să fie scrisă, nu
subînțeleasă.

`KBot.App` a primit bump (1.0.5.0 → 1.0.6.0) fiindcă pasul 7 a rulat.

---

## 3. Rezultate de teste

Publicat înainte de rulare (`.\publish-debug.ps1` → `artifacts\KBot_Debug_20260806_155847`), rulat
pe mașina clientului, ieșirea scrisă în
`src\KBot.App\bin\Debug\net8.0-windows\win-x64\Logs\test_20260806_155928_226.log`.

| Proiect | Înainte | După | Δ |
|---|---:|---:|---:|
| KBot.Theming.Tests | 31 | **51** | +20 |
| KBot.Controls.Tests | 240 | **261** | +21 |
| KBot.App.Tests | 143 | **148** | +5 (pasul 7) |
| KBot.DevHarness.Tests | 162 | 162 | — |
| KBot.Api.Tests | 68 | 68 | — |
| KBot.Xfa.Tests | 39 | 39 | — |
| KBot.Domain.Tests | 17 | 17 | — |
| KBot.Common.Tests | 14 | 14 | — |
| KBot.LocalStore.Tests | 1 | 1 | — |
| **Total** | **715** | **761** | **+46** |

**761 passed / 0 failed / 0 skipped.** `dotnet build KBot.sln`: **0 erori, 0 avertismente** (de
orice fel — cele 16 NU1701 au fost reduse la tăcere în felia 0024-03).

Cifrele «înainte» sunt derivate: singurele fișiere de test adăugate sunt cele trei de mai sus, cu
20, 21 și 5 `<Fact>` și zero `<Theory>` (numărate). Cifrele din planul feliei (Controls 134, App
136, soluție 436) erau de pe vremea feliei 0022 și au fost corectate contra rulării reale.

A doua rulare (după pasul 7) e logată în
`src\KBot.App\bin\Debug\net8.0-windows\win-x64\Logs\test_20260807_093959_095.log`, cu publicare
înainte (`artifacts\KBot_Debug_20260806_161919`).

Ce acoperă cele 41 de teste noi:
- `Items` add/remove/clear schimbă ce întoarce `IndexAt` (deci invalidarea de layout chiar are loc);
- ordinea Near/Far se păstrează prin colecție, cu separator între grupuri, verificată **geometric**
  (grupul Far ancorat la `Height − margine`);
- `EndInit` aruncă pe cheie duplicată și pe cheie vidă pe un ne-separator;
- `EndInit` dă `__sep_N` separatorilor fără cheie, nu se ciocnește cu `AddSeparator` și lasă în pace
  o cheie de separator scrisă de mână;
- `SelectedKey` setat între `BeginInit`/`EndInit` nu aruncă pe o cheie necunoscută și se aplică la
  `EndInit`, ridicând `SelectionChanged` **o singură dată** (și zero ori cât timp inițializează);
- `AddItem`/`SetItemVisible`/`SetItemEnabled`/`SetBadge` aruncă în continuare pe cheie vidă,
  duplicată și necunoscută; cheia internă de separator nu e accesibilă prin nicio căutare;
- un element ascuns primește `Rectangle.Empty`, e sărit de `IndexAt` și de navigarea cu tastatura
  (**acoperirea care lipsea din 0018**);
- constructorul fără parametri produce o coloană utilizabilă; `Key`/`ColumnType` aruncă peste
  rânduri și reușesc fără; scoaterea din grilă golește `Owner` și ridică garda;
- `Columns.Add` se poartă identic cu `AddColumn` (index, layout, totaluri), remove/clear reconstruiesc
  indexul; `AddColumn` e neschimbat bit cu bit (aceleași aruncări, aceeași valoare întoarsă, o
  singură intrare în `Columns`);
- `EndInit` aruncă pe chei de coloană duplicate/vide;
- rândurile și starea de selecție nu se serializează niciodată; `ComboItems`/`Tag`/`IsEffectivelyVisible`
  nici atât; `TotalsRowHeight` nu se serializează cât urmărește antetul.

---

## 4. Pasul 6 — verificările manuale în Visual Studio: **NU AU RULAT**

Niciuna dintre cele șase verificări din §8 al planului nu a fost făcută. Nu am acces la designer-ul
Visual Studio din mediul în care s-a scris felia; ele cer un om în fața ecranului.

| # | Verificare | Stare |
|---|---|---|
| 1 | Cele patru controale apar în Toolbox | **NERULAT** |
| 2 | Fiecare se poate arunca pe un formular de probă fără să arunce | **NERULAT** |
| 3 | `navViews` → `Items` → adaugă 3 butoane + 1 separator, `Align = Far` pe două, `Badge = 3` pe unul, reordonează, șterge unul, redeschide | **NERULAT** |
| 4 | Cheie vidă și chei duplicate arată chenarul roșu și NU aruncă în VS | **NERULAT** |
| 5 | `KBotDataView` → `Columns` → 3 coloane de tipuri diferite, lățimi, `FormatString`, reordonare, ștergere, redeschidere | **NERULAT** |
| 6 | `KBotBusyBar` cu `Running = True` pictează fără să animeze, CPU-ul VS rămâne plat | **NERULAT** |

Nu le revendic ca «verificate». Sunt exact genul de lucruri pe care felia 0023 le-a raportat de
două ori ca livrate fără să fi fost vreodată pe ecran (lecția consemnată în `KBOT_STATUS.md`), și
regula aia se aplică și aici.

## 5. Pasul 1 — ce am putut proba, și ce nu

Rotunda propriu-zisă (butonul «…», dialogul de colecție, liniile scrise în `*.Designer.vb`) **nu a
fost probată**. Trei lucruri o susțin însă, două măsurate și unul citit din repo:

1. **Măsurat.** `StockCollectionEditor_IsResolvedForItems` și
   `StockCollectionEditor_IsResolvedForColumns` trec: `TypeDescriptor` întoarce editorul **STOCK**
   `System.ComponentModel.Design.CollectionEditor` pentru ambele proprietăți, **fără niciun atribut
   `<Editor>`**. Exact mecanismul pe care planul îl bănuia (înregistrare intrinsecă pentru
   `ICollection`) — deci varianta de rezervă din §3 (atributul explicit cu editorul stoc) **nu e
   necesară** și nu a fost adăugată. Dacă butonul lipsește totuși în VS, ăsta e primul lucru de
   încercat, și tot nu cere assembly de design-time.
2. **Măsurat.** Ambele proprietăți raportează `SerializationVisibility = Content`, adică
   serializatorul CodeDom va emite conținutul colecției, nu o atribuire de referință.
3. **Citit din repo.** `src/KBot.App/Views/DdfView.Designer.vb:341-349` conține deja un
   `KBotNavList` autorit de designer (`navSub.Orientation = …Horizontal`, `navSub.SelectedKey = Nothing`),
   deci designer-ul VS de pe mașina asta **instanțiază și serializează deja controlul**. Asta nu
   probează colecția, dar scoate din discuție ipoteza «controlul nu se poate încărca deloc în
   designer».

Ce NU probează nimic din cele de mai sus: că dialogul chiar se deschide, că `Add` chiar creează un
element editabil, și că `MainForm.Designer.vb` chiar recapătă elementele la redeschidere.

## 6. Pasul 7 — migrarea `MainForm.navViews`: **A RULAT** (cerut explicit de operator)

Prima variantă a feliei a livrat pașii 0–5 și **a oprit înaintea pasului 7**, fiindcă planul îl
condiționează de trecerea pașilor 1 și 6, iar pasul 6 cere Visual Studio. Operatorul a cerut
migrarea oricum, după ce riscul i-a fost spus. Consemnat ca atare: pasul 7 e livrat pe o rotundă
**neprobată în designer**, nu pe una verificată.

Cele opt intrări trăiesc acum în `MainForm.Designer.vb`, exact în forma pe care o emite designer-ul
(instanțe `Dim KBotNavItem1..8`, atribuiri de proprietăți, `navViews.Items.Add(...)`), plus perechea
`CType(navViews, ISupportInitialize).BeginInit()` / `.EndInit()` alături de cea a lui `split`:

| # | Cheie | Text | Align | Notă |
|---|---|---|---|---|
| 1 | `sumar` | `Sumar` | Near | mereu vizibil, fără flag `Are*` |
| 2 | `istoric` | `Istoric` | Near | |
| 3 | `rezervari` | `Rezervări` | Near | |
| 4 | `receptii` | `Recepții` | Near | |
| 5 | `plati` | `Plăți` | Near | |
| 6 | *(separator)* | — | **Far** | `IsSeparator = True`, fără cheie — `EndInit` îi dă `__sep_1` |
| 7 | `ddf` | `DDF` | **Far** | |
| 8 | `ord` | `ORD` | **Far** | |

`Align = Near` NU e emis (e `DefaultValue`), la fel `Enabled`/`Visible`/`Badge`; se emit doar
`Align = Far` și `IsSeparator = True`. Nu am adăugat un `AddRange` pe colecție tocmai ca forma
scrisă de mână să fie cea pe care o va regenera VS: `CollectionCodeDomSerializer` folosește
`AddRange` doar dacă există, altfel emite `Add` per element — și `Collection(Of T)` nu are `AddRange`.

Din `MainForm.vb` a dispărut blocul `AddItem`/`AddSeparator` (dacă rămânea, `AddItem` lovea
aruncarea pe cheie duplicată la prima rulare). **`navViews.SelectedKey = "sumar"` rămâne în cod** —
atribuirea aia e cea care ridică `SelectionChanged` și deci creează prima vedere; în designer ar fi
o valoare moartă. Cele trei intrări comentate (`indicatori`, `revizii`, `partener`) NU au fost
adăugate în designer; comentariul din cod le păstrează numele pentru ziua în care vederile lor
există.

**O corecție la plan:** §9.3 spune «keep `navViews.SelectedKey = "sumar"` in code, after
`ApplyViewGating(Nothing)` as today». În cod era, și a rămas, **înaintea** lui
`ApplyViewGating(Nothing)`. Ambele ordini funcționează (`IsViewEnabled` întoarce True și pentru
cheie goală, și pentru «sumar»), dar «as today» descria greșit codul, așa că am păstrat ordinea
reală în loc să introduc o schimbare de comportament pe care nimeni nu a cerut-o.

**Diacriticele sunt literale, verificat pe octeți**, nu doar pe ochi: `Rezervări` = `C4 83`,
`Recepții` / `Plăți` = `C8 9B`, zero apariții de `\u00` în fișier. Și, mai important, cele două
teste noi compară cu literalele românești la RULARE — dacă designer-ul ar rescrie vreodată fișierul
cu escape-uri, VB le-ar purta ca text simplu și testele ar pica.

**Cinci teste noi** (`tests/KBot.App.Tests/MainFormNavItemsTests.vb`) construiesc `MainForm` pe un
fir STA cu cele cinci dependențe pe `Nothing` (constructorul face doar `InitializeComponent` +
atribuiri de câmpuri) și fixează: cele opt intrări în ordine cu Align-ul corect; diacriticele
literale; faptul că separatorul a primit `__sep_1`, adică **`BeginInit`/`EndInit` chiar au rulat**
din `InitializeComponent`; faptul că toate cheile pe care le folosește `ApplyViewGating` se rezolvă
(`SetItemVisible` aruncă pe o cheie necunoscută, deci trecerea probează că poarta nu se poate rupe
la pornire); și faptul că re-adăugarea oricăreia din cod ar arunca — motivul pentru care blocul a
trebuit să dispară.

Asta e apărarea reală împotriva pericolului feliei: o regenerare de designer care pierde sau
reordonează intrări. `IsViewEnabled` aruncă `ArgumentException` pe o cheie necunoscută, deci o
intrare pierdută nu e o pagubă cosmetică.

**Notă de regresie deja acoperită:** `DdfView` conține și el un `KBotNavList` autorit în designer.
Când VS regenerează `DdfView.Designer.vb`, va emite acum și `BeginInit()`/`EndInit()` în jurul lui
`navSub`. Calea aia a fost gândită: `EndInit` pe o colecție goală validează fără să arunce, iar
`SelectedKey = Nothing` reținut se aplică prin `ClearSelection()`, adică no-op. `DdfView.vb`
continuă să-și adauge cele patru pagini în cod.

**Ce rămâne neverificat aici:** nimeni nu a deschis `MainForm` în designer după migrare, deci nu se
știe dacă VS **reîncarcă** cele opt elemente în dialogul de colecție și dacă le **rescrie** identic
la prima regenerare. Ce se știe: se compilează, se construiește headless, și starea de rulare e
identică cu cea de dinainte (fixată de cele cinci teste). Dacă VS refuză rotunda, calea de întoarcere
e blocul `AddItem` din istoricul git — nu e o fundătură.

---

## 7. Nefăcut / neverificat, explicit

- **Rotunda din designer, Toolbox-ul, marcajul roșu și grila de proprietăți** — vezi §4 și §5.
  NERULATE, nu «verificate». Asta include acum și `MainForm`: cele opt intrări sunt scrise în
  `MainForm.Designer.vb` în forma pe care o emite designer-ul, dar **nimeni nu a deschis formularul
  în designer după migrare** — dacă VS le reîncarcă în dialogul de colecție și le rescrie identic
  la prima regenerare rămâne întrebarea deschisă a feliei. Starea de RULARE e fixată de cinci teste,
  iar calea de întoarcere (blocul `AddItem`) e în istoricul git.
- **Fără manipulare directă pe suprafața de design** (decizia 2 a planului): nu poți da click pe un
  buton din bară în designer, nu îl poți trage ca să-l reordonezi și nu îl poți șterge cu Del. Ar
  cere un `ControlDesigner` propriu, iar pe `net8.0-windows` designer-ul rulează în alt proces, deci
  orice designer / `UITypeEditor` / `TypeConverter` propriu ar avea nevoie de un assembly de
  design-time separat, contra `Microsoft.WinForms.Designer.SDK`. În afara scopului, deliberat.
- **Fără editare de noduri pentru `AdvancedTreeControl`** — arborele din `MainForm` se umple din
  `GET /api/forexe/tree`, deci nodurile autorite în designer ar fi aruncate la încărcare.
- **Fără migrarea coloanelor vederilor existente** — `IstoricView`, `PlatiView`, `SumarView`,
  `RezervariView`, `ReceptiiView`, `DdfView` și formularele din DevHarness își construiesc în
  continuare coloanele în cod cu `AddColumn`, neatinse. Felia asta doar face autorarea din designer
  POSIBILĂ.
- **`ComboItems` și `Tag` nu sunt serializabile prin proiectare** (§1.2).
- **Jumătatea rămasă din limitarea 0013**: editarea proprietăților unei coloane după încărcare tot
  cere `AutoSizeColumns()` explicit. Doar add/remove/clear declanșează layout singure acum.
- `<DefaultEvent("SelectionChanged")>` pe `KBotNavList` numește un eveniment cu semnătură VB
  (`SelectionChanged(key As String)`), nu `EventHandler`. Atributul e inofensiv, dar dacă
  dublu-click-ul pe control în designer nu generează handler-ul, asta e cauza — **neverificat**.
