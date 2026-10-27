# SLICE-0025 — suprafețe editabile din designer (`KBotNavList`, `KBotDataView`, `KBotCaptionBar`, `KBotBusyBar`)

Regrupare documentară D001-08 (25.10.2026): suprafețele Designer și cele șase continuări NavList sunt reunite mai jos. „Pasul 7” din plan (migrarea MainForm) este distinct de subfelia 0025-07 (flyout); nu se renumerotează. Contractele inițiale (inclusiv ciclul în trei stări și lipsa proprietății de mărime) sunt istorice și au corecțiile ulterioare explicite. Rezultatele headless nu certifică designerul sau experiența vizuală.

Repere: [0025 de bază](#slice-0025-baza), [0025-02](#slice-0025-02), [0025-05](#slice-0025-05), [0025-06](#slice-0025-06), [0025-07](#slice-0025-07), [0025-08](#slice-0025-08), [0025-09](#slice-0025-09). Căile Markdown vechi din listele istorice de fișiere rămân proveniență; navigarea curentă folosește aceste ancore.

<a id="slice-0025-baza"></a>

Plan: `PLAN_DesignerSurfaces.md` (lipit în sesiune). Planul presupunea felia **0024**; `KBOT_STATUS.md` spune «Next free slice number: **0025**» (0024 e luată de profilurile Adobe de pe fila «Document»), deci felia și worklog-ul poartă **0025**, cum cere §0 al planului.

## 0. Reconcilieri față de plan (nu ocolite în tăcere — CODE_WORKFLOW §4)

| # | Ce spune planul | Ce am găsit în repo | Ce am făcut |
|---|---|---|---|
| 1 | «Slice 0024» | 0024 = profiluri Adobe (`SLICE-0024-01/02/03`), next free = 0025 | Felie **0025**, fișierul redenumit corespunzător |
| 2 | «`KBotNavList` currently has **no** xUnit tests at all (open item from slice 0018)» | `tests/KBot.Theming.Tests/KBotNavListSelectionTests.vb` EXISTĂ (4 teste, adăugate de commit-ul `5f94f44` pentru bug-ul `SelectedKey = Nothing` din DdfView) | Firul 0018 rămâne **parțial** deschis: existau teste pentru `SelectedKey`, dar NU pentru separatori / aliniere / vizibilitate / orientare. Felia asta le adaugă (20 de teste noi), deci firul se poate închide acum |
| 3 | «Existing counts before this slice: `KBot.Controls.Tests` 134, `KBot.App.Tests` 136, solution 436 — confirm these numbers against `KBOT_STATUS.md`» | Confirmat contra STATUS + rulare reală: **Controls 240, App 143, soluție 715** (cifrele planului sunt de pe vremea feliei 0022; 0023/0024 au adăugat ~280 de teste) | Folosite cifrele reale |
| 4 | Helper-ul de design-time e descris ca `Friend Shared Function IsDesignTime` | `Friend` nu traversează assembly-uri, iar `KBot.Controls` (alt assembly) are nevoie de el | `Public NotInheritable Class KBotDesignTime` cu `<EditorBrowsable(Advanced)>` — scos din lista comună IntelliSense, dar vizibil pentru `KBot.Controls`. Fără `InternalsVisibleTo` suplimentar |
| 5 | Pasul 1 cere proba rotundei **în Visual Studio**, altfel «stop and report» | Nu pot deschide designer-ul VS din acest mediu | Am construit **proba programatică cea mai apropiată** (vezi §5) și am mers mai departe pe pașii 2–5. **Pasul 6 (verificările manuale) tot NU a rulat.** Pasul 7 s-a oprit inițial din acest motiv (regula planului), apoi **a fost cerut explicit de operator și livrat** — vezi §6 |
| 6 | §9.3: «keep `navViews.SelectedKey = "sumar"` in code, **after** `ApplyViewGating(Nothing)` **as today**» | În cod era ÎNAINTEA lui `ApplyViewGating(Nothing)`, nu după | Păstrată ordinea REALĂ. Ambele funcționează; «as today» descria greșit codul, iar o schimbare de ordine pe care nimeni nu a cerut-o nu merită riscul |

## 1. Ce s-a schimbat și de ce, per control

### 1.1 `KBotNavList` (+ `KBotNavItem`, `KBotNavItemCollection`) — `KBot.Theming`

Modelul de element a ieșit din control. Clasa privată imbricată `NavItem` a fost **înlocuită** cu `KBotNavItem` public, cu constructor fără parametri (cerința dialogului de colecție), toate cele șapte proprietăți atribuite `Category`/`Description`/`DefaultValue`, și `Bounds` lăsat **`Friend`** ca designer-ul să nu-l vadă și să nu-l serializeze (e stare derivată, recalculată la fiecare layout).

`ToString()` e motivul pentru care **un singur tip** de element e suportabil: lista din stânga dialogului de colecție arată `ddf — "DDF" (Far)` pentru un buton și `──────── separator (Far) ────────` pentru un separator, deci diferența se vede dintr-o privire. Un element fără cheie apare ca `<fără cheie> — …`. (Decizia 1 din plan: două tipuri într-o colecție ordonată ar cere un editor de colecție PROPRIU, care cere un assembly de design-time — în afara scopului.)

`KBotNavItemCollection : Collection(Of KBotNavItem)` cu referință `Friend` la bară; fiecare `InsertItem`/`SetItem`/`RemoveItem`/`ClearItems` cheamă baza și apoi `Owner?.InvalidateLayout()`. `Nothing` e respins. **Cheile NU se validează aici**: dialogul inserează elementul în clipa în care apeși «Add», înainte să fi tastat ceva în el.

Pe control:
- `<ToolboxItem(True)>` + `<DefaultProperty("Items")>` + `<DefaultEvent("SelectionChanged")>`;
- `Implements ISupportInitialize`;
- `Items` public, `<DesignerSerializationVisibility(Content)>`;
- `AddItem`/`AddSeparator`/`SetBadge`/`SetItemEnabled`/`SetItemVisible` — **semnături și comportament de rulare NESCHIMBATE**, inclusiv fiecare `ArgumentException`; scriu prin `Items`;
- `FindIndex` **sare peste separatori** ȘI respinge cheia vidă. A doua parte nu e cosmetică: `OnKeyDown` cheamă `FindIndex(_selectedKey)` cu `Nothing` când nu e nimic selectat, iar `String.Equals(Nothing, Nothing)` = True ar fi potrivit un element autorit în designer cu cheia încă goală și ar fi selectat elementul greșit;
- cheile separatorilor: `NextSeparatorKey()` incrementează `_sepSeq` până nimerește o cheie **nefolosită de niciun element** (inclusiv de un separator căruia i s-a tastat manual `__sep_1` în designer), și e folosită atât de `AddSeparator` cât și de `EndInit`;
- `SelectedKey` între `BeginInit` și `EndInit`: **se reține fără validare** și getter-ul întoarce valoarea reținută. Designer-ul nu are nicio obligație să emită `Items` înaintea lui `SelectedKey`, iar setter-ul de rulare ar arunca pe o cheie care apare o linie mai jos. În afara inițializării, comportamentul e exact cel de azi;
- `EndInit`: (1) dă chei separatorilor fără cheie, (2) validează butoanele (cheie vidă / duplicată → `ArgumentException` care numește indexul și cheia, în română cu diacritice reale), (3) aplică selecția reținută pe drumul normal. **În designer, (2) și (3) se sar** — dar valoarea reținută se scrie totuși în `_selectedKey`, ca să se re-serializeze corect (dacă am ignora-o, designer-ul ar pierde-o la următoarea regenerare; asta e o mică abatere de la plan, în plus, nu în minus);
- `OnPaint`, doar design-time: un buton cu cheie vidă sau duplicată primește un **chenar roșu de 2px** în locul ramei normale. La rulare calea nu se execută niciodată.

### 1.2 `KBotDataColumn` + `KBotDataColumnCollection` + `KBotDataView` — `KBot.Controls`

`KBotDataColumn` primește constructor fără parametri (cel de patru argumente rămâne **identic**, inclusiv aruncarea pe cheie vidă) și un `Friend Property Owner As KBotDataView`, setat de colecție la inserare și golit la scoatere.

`Key` și `ColumnType` trec din `ReadOnly` în citire/scriere, dar **aruncă `InvalidOperationException` când coloana aparține unei grile cu rânduri**: valorile celulelor sunt ținute pe cheia coloanei în dicționarul lui `KBotDataRow`, deci o redenumire peste date ar orfelina fiecare celulă stocată și coloana ar picta gol — adică pierdere de date deghizată în no-op. `Key` cheamă apoi `Owner.OnColumnKeyChanged` (reconstruiește indexul și mută selecția curentă dacă stătea pe cheia veche); `ColumnType` abandonează editarea deschisă (`CancelEdit`), fiindcă ea aparține tipului VECHI. Cât `Owner` e `Nothing` — situația designer-ului — ambele sunt libere.

Toată suprafața publică a coloanei e atribuită `Category("K-BOT")`/`Description`/`DefaultValue`. `IsEffectivelyVisible`, `ComboItems` și `Tag` sunt `Browsable(False)` + `DesignerSerializationVisibility(Hidden)`: primul e stare derivată, ultimele două **nu pot face rotunda** prin `InitializeComponent` (o `IList(Of Object)` și un `Object` arbitrar), iar o sursă combo pe jumătate serializată e mai rea decât niciuna. `UserSized`/`AutoHidden` erau deja `Friend`, deci nu se serializează.

`KBotDataColumnCollection` face pe `KBotDataView`: `RebuildColumnIndex()` + `RecomputeTotals()` + `LayoutChanged()` la fiecare adăugare/înlocuire/ștergere/golire (prin `OnColumnsChanged`). **Asta închide jumătate din limitarea consemnată la felia 0013** («fără back-reference coloană→grilă»): add și remove declanșează layout singure. Editarea PROPRIETĂȚILOR unei coloane după încărcare se comportă ca azi și tot are nevoie de `AutoSizeColumns()` explicit. Cheile duplicate se **sar în tăcere la reconstrucția indexului** — validarea e la `AddColumn` și la `EndInit`, din același motiv ca la navlist.

`KBotDataView`: `Implements ISupportInitialize` (`EndInit` validează cheile, se sare în designer), `Columns` trece de la `IReadOnlyList(Of KBotDataColumn)` la `KBotDataColumnCollection` (atribuit `Content`), `AddColumn` își păstrează semnătura, cele două aruncări și valoarea întoarsă dar nu mai duplică `_columnIndex(key) = col` / `RecomputeTotals()` / `LayoutChanged()` (vin de la colecție). `Column(key)` neatins. Proprietățile de grilă au primit `Category`/`Description`/`DefaultValue`; `Rows`, `RowCount`, `Item`, `CurrentRowIndex`, `CurrentColumnKey`, `CurrentRow`, `IsEditing` sunt `Browsable(False)` + `Hidden` — sunt DATE de rulare, iar un designer care le-ar scrie în `InitializeComponent` ar îngheța în formular datele de test ale cuiva.

**Un defect de rotundă prins pe drum, care nu e în plan:** `TotalsRowHeight` NU putea primi `<DefaultValue(0)>`. Getter-ul lui întoarce `HeaderHeight` cât timp banda e pe «urmărește antetul», deci designer-ul ar fi serializat numărul REZOLVAT (30) și ar fi fixat banda pentru totdeauna — o rotundă care schimbă tăcut înțelesul. Rezolvat cu `ShouldSerializeTotalsRowHeight` / `ResetTotalsRowHeight`, pe care designer-ul le respectă înaintea oricărui `DefaultValue`. Test dedicat.

### 1.3 `KBotCaptionBar` / `KBotBusyBar`

Ambele trec pe `<ToolboxItem(True)>` și primesc `Category`/`Description`/`DefaultValue` pe `IconImage`, `ShowMinimize`, `ShowMaximize`, respectiv `Running`. `Text` (moștenit) rămâne neatins.

`KBotBusyBar.Running`: când `KBotDesignTime.IsDesignTime` e True, valoarea se reține, se cere repictare, și **cronometrul nu se atinge**. Un timer la 15 ms care ticăie în procesul designer-ului repictează suprafața la infinit și arde CPU în `devenv`/`DesignToolsServer`. `Dispose` rămâne corect pe calea asta (opreșterea unui timer nepornit e no-op) — verificat prin citire.

**`IconImage` NU se setează în designer-ul lui `MainForm`/`LoginForm`.** `MainForm.vb:163` face `capBar.IconImage = My.Resources.kbot_64` în cod și acea atribuire câștigă la rulare; setarea și în designer ar copia imaginea a doua oară în `.resx`-ul formularului, degeaba.

### 1.4 Helper-ul comun de design-time

`KBotDesignTime.IsDesignTime(c)` întoarce True dacă: `LicenseManager.UsageMode = Designtime`; sau `c.Site.DesignMode`; sau un părinte urcând lanțul e sitat în design mode; sau procesul curent e unul dintre `devenv` / `DesignToolsServer` / `Microsoft.VisualStudio.DesignTools.DesignToolsServer` / `XDesProc` / `Blend`. `Control.DesignMode` singur nu ajunge: e False în constructor și False pentru un control imbricat în alt control.

**Toate** apelurile `GlobalErrorLog.Write` din pictarea / mouse-ul / tastatura celor patru controale sunt acum sărite la design time (11 locuri). `GlobalErrorLog` scrie în `AppContext.BaseDirectory\Logs\harness_errors.log`, adică **lângă `devenv.exe`** când rulează în designer — zgomot în cel mai bun caz, sursă de excepții în cel mai rău.

**Abatere deliberată de la regula casei, consemnată:** `IsDesignTime` și `DetectDesignerProcess` prind excepția și întorc `False` **fără să logheze**. Sunt singurele două locuri din felie care fac asta, și motivul e că logarea e exact lucrul pe care predicatul îl suprimă; comentariul din cod o spune la fața locului.

## 2. Fișiere atinse

**Noi**
- `src/KBot.Theming/KBotDesignTime.vb`
- `src/KBot.Theming/KBotNavItem.vb` (`KBotNavItem` + `KBotNavItemCollection`)
- `src/KBot.Controls/KBotDataColumnCollection.vb`
- `tests/KBot.Theming.Tests/KBotNavListTests.vb`
- `tests/KBot.Controls.Tests/KBotDataColumnDesignerTests.vb`
- `docs/worklog/SLICE-0025-designer-surfaces.md` (acesta)

**Modificate**
- `src/KBot.Theming/KBotNavList.vb` — model extern, `Items`, `ISupportInitialize`, `FindIndex`, chei de separator, marcaj roșu, cârlige `Friend` pentru teste
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

**`AssemblyVersion` rămâne 1.0.0.0 în ambele.** Pentru `KBot.Theming` e evident (suprafața doar crește). Pentru `KBot.Controls` **NU** e evident și merită spus pe față: `Columns` **și-a schimbat tipul declarat** din `IReadOnlyList(Of KBotDataColumn)` în `KBotDataColumnCollection`, ceea ce e o rupere la compilare pentru orice cod care îl ținea ca `IReadOnlyList`. Toată soluția se recompilează împreună și niciun apelant nu îl ținea așa (verificat prin grep la pasul 0: cele 11 locuri folosesc doar `.Count`, indexatorul și `For Each`, toate oferite de `Collection(Of T)`), deci `AssemblyVersion` stă pe loc — dar contractul manifestului cere ca decizia să fie scrisă, nu subînțeleasă.

`KBot.App` a primit bump (1.0.5.0 → 1.0.6.0) fiindcă pasul 7 a rulat.

## 3. Rezultate de teste

Publicat înainte de rulare (`.\publish-debug.ps1` → `artifacts\KBot_Debug_20260806_155847`), rulat pe mașina clientului, ieșirea scrisă în `src\KBot.App\bin\Debug\net8.0-windows\win-x64\Logs\test_20260806_155928_226.log`.

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

**761 passed / 0 failed / 0 skipped.** `dotnet build KBot.sln`: **0 erori, 0 avertismente** (de orice fel — cele 16 NU1701 au fost reduse la tăcere în felia 0024-03).

Cifrele «înainte» sunt derivate: singurele fișiere de test adăugate sunt cele trei de mai sus, cu 20, 21 și 5 `<Fact>` și zero `<Theory>` (numărate). Cifrele din planul feliei (Controls 134, App 136, soluție 436) erau de pe vremea feliei 0022 și au fost corectate contra rulării reale.

A doua rulare (după pasul 7) e logată în `src\KBot.App\bin\Debug\net8.0-windows\win-x64\Logs\test_20260807_093959_095.log`, cu publicare înainte (`artifacts\KBot_Debug_20260806_161919`).

Ce acoperă cele 41 de teste noi:
- `Items` add/remove/clear schimbă ce întoarce `IndexAt` (deci invalidarea de layout chiar are loc);
- ordinea Near/Far se păstrează prin colecție, cu separator între grupuri, verificată **geometric** (grupul Far ancorat la `Height − margine`);
- `EndInit` aruncă pe cheie duplicată și pe cheie vidă pe un ne-separator;
- `EndInit` dă `__sep_N` separatorilor fără cheie, nu se ciocnește cu `AddSeparator` și lasă în pace o cheie de separator scrisă de mână;
- `SelectedKey` setat între `BeginInit`/`EndInit` nu aruncă pe o cheie necunoscută și se aplică la `EndInit`, ridicând `SelectionChanged` **o singură dată** (și zero ori cât timp inițializează);
- `AddItem`/`SetItemVisible`/`SetItemEnabled`/`SetBadge` aruncă în continuare pe cheie vidă, duplicată și necunoscută; cheia internă de separator nu e accesibilă prin nicio căutare;
- un element ascuns primește `Rectangle.Empty`, e sărit de `IndexAt` și de navigarea cu tastatura (**acoperirea care lipsea din 0018**);
- constructorul fără parametri produce o coloană utilizabilă; `Key`/`ColumnType` aruncă peste rânduri și reușesc fără; scoaterea din grilă golește `Owner` și ridică garda;
- `Columns.Add` se poartă identic cu `AddColumn` (index, layout, totaluri), remove/clear reconstruiesc indexul; `AddColumn` e neschimbat bit cu bit (aceleași aruncări, aceeași valoare întoarsă, o singură intrare în `Columns`);
- `EndInit` aruncă pe chei de coloană duplicate/vide;
- rândurile și starea de selecție nu se serializează niciodată; `ComboItems`/`Tag`/`IsEffectivelyVisible` nici atât; `TotalsRowHeight` nu se serializează cât urmărește antetul.

## 4. Pasul 6 — verificările manuale în Visual Studio: **NU AU RULAT**

Niciuna dintre cele șase verificări din §8 al planului nu a fost făcută. Nu am acces la designer-ul Visual Studio din mediul în care s-a scris felia; ele cer un om în fața ecranului.

| # | Verificare | Stare |
|---|---|---|
| 1 | Cele patru controale apar în Toolbox | **NERULAT** |
| 2 | Fiecare se poate arunca pe un formular de probă fără să arunce | **NERULAT** |
| 3 | `navViews` → `Items` → adaugă 3 butoane + 1 separator, `Align = Far` pe două, `Badge = 3` pe unul, reordonează, șterge unul, redeschide | **NERULAT** |
| 4 | Cheie vidă și chei duplicate arată chenarul roșu și NU aruncă în VS | **NERULAT** |
| 5 | `KBotDataView` → `Columns` → 3 coloane de tipuri diferite, lățimi, `FormatString`, reordonare, ștergere, redeschidere | **NERULAT** |
| 6 | `KBotBusyBar` cu `Running = True` pictează fără să animeze, CPU-ul VS rămâne plat | **NERULAT** |

Nu le revendic ca «verificate». Sunt exact genul de lucruri pe care felia 0023 le-a raportat de două ori ca livrate fără să fi fost vreodată pe ecran (lecția consemnată în `KBOT_STATUS.md`), și regula aia se aplică și aici.

## 5. Pasul 1 — ce am putut proba, și ce nu

Rotunda propriu-zisă (butonul «…», dialogul de colecție, liniile scrise în `*.Designer.vb`) **nu a fost probată**. Trei lucruri o susțin însă, două măsurate și unul citit din repo:

1. **Măsurat.** `StockCollectionEditor_IsResolvedForItems` și `StockCollectionEditor_IsResolvedForColumns` trec: `TypeDescriptor` întoarce editorul **STOCK** `System.ComponentModel.Design.CollectionEditor` pentru ambele proprietăți, **fără niciun atribut `<Editor>`**. Exact mecanismul pe care planul îl bănuia (înregistrare intrinsecă pentru `ICollection`) — deci varianta de rezervă din §3 (atributul explicit cu editorul stoc) **nu e necesară** și nu a fost adăugată. Dacă butonul lipsește totuși în VS, ăsta e primul lucru de încercat, și tot nu cere assembly de design-time.
2. **Măsurat.** Ambele proprietăți raportează `SerializationVisibility = Content`, adică serializatorul CodeDom va emite conținutul colecției, nu o atribuire de referință.
3. **Citit din repo.** `src/KBot.App/Views/DdfView.Designer.vb:341-349` conține deja un `KBotNavList` autorit de designer (`navSub.Orientation = …Horizontal`, `navSub.SelectedKey = Nothing`), deci designer-ul VS de pe mașina asta **instanțiază și serializează deja controlul**. Asta nu probează colecția, dar scoate din discuție ipoteza «controlul nu se poate încărca deloc în designer».

Ce NU probează nimic din cele de mai sus: că dialogul chiar se deschide, că `Add` chiar creează un element editabil, și că `MainForm.Designer.vb` chiar recapătă elementele la redeschidere.

## 6. Pasul 7 — migrarea `MainForm.navViews`: **A RULAT** (cerut explicit de operator)

Prima variantă a feliei a livrat pașii 0–5 și **a oprit înaintea pasului 7**, fiindcă planul îl condiționează de trecerea pașilor 1 și 6, iar pasul 6 cere Visual Studio. Operatorul a cerut migrarea oricum, după ce riscul i-a fost spus. Consemnat ca atare: pasul 7 e livrat pe o rotundă **neprobată în designer**, nu pe una verificată.

Cele opt intrări trăiesc acum în `MainForm.Designer.vb`, exact în forma pe care o emite designer-ul (instanțe `Dim KBotNavItem1..8`, atribuiri de proprietăți, `navViews.Items.Add(...)`), plus perechea `CType(navViews, ISupportInitialize).BeginInit()` / `.EndInit()` alături de cea a lui `split`:

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

`Align = Near` NU e emis (e `DefaultValue`), la fel `Enabled`/`Visible`/`Badge`; se emit doar `Align = Far` și `IsSeparator = True`. Nu am adăugat un `AddRange` pe colecție tocmai ca forma scrisă de mână să fie cea pe care o va regenera VS: `CollectionCodeDomSerializer` folosește `AddRange` doar dacă există, altfel emite `Add` per element — și `Collection(Of T)` nu are `AddRange`.

Din `MainForm.vb` a dispărut blocul `AddItem`/`AddSeparator` (dacă rămânea, `AddItem` lovea aruncarea pe cheie duplicată la prima rulare). **`navViews.SelectedKey = "sumar"` rămâne în cod** — atribuirea aia e cea care ridică `SelectionChanged` și deci creează prima vedere; în designer ar fi o valoare moartă. Cele trei intrări comentate (`indicatori`, `revizii`, `partener`) NU au fost adăugate în designer; comentariul din cod le păstrează numele pentru ziua în care vederile lor există.

**O corecție la plan:** §9.3 spune «keep `navViews.SelectedKey = "sumar"` in code, after `ApplyViewGating(Nothing)` as today». În cod era, și a rămas, **înaintea** lui `ApplyViewGating(Nothing)`. Ambele ordini funcționează (`IsViewEnabled` întoarce True și pentru cheie goală, și pentru «sumar»), dar «as today» descria greșit codul, așa că am păstrat ordinea reală în loc să introduc o schimbare de comportament pe care nimeni nu a cerut-o.

**Diacriticele sunt literale, verificat pe octeți**, nu doar pe ochi: `Rezervări` = `C4 83`, `Recepții` / `Plăți` = `C8 9B`, zero apariții de `\u00` în fișier. Și, mai important, cele două teste noi compară cu literalele românești la RULARE — dacă designer-ul ar rescrie vreodată fișierul cu escape-uri, VB le-ar purta ca text simplu și testele ar pica.

**Cinci teste noi** (`tests/KBot.App.Tests/MainFormNavItemsTests.vb`) construiesc `MainForm` pe un fir STA cu cele cinci dependențe pe `Nothing` (constructorul face doar `InitializeComponent` + atribuiri de câmpuri) și fixează: cele opt intrări în ordine cu Align-ul corect; diacriticele literale; faptul că separatorul a primit `__sep_1`, adică **`BeginInit`/`EndInit` chiar au rulat** din `InitializeComponent`; faptul că toate cheile pe care le folosește `ApplyViewGating` se rezolvă (`SetItemVisible` aruncă pe o cheie necunoscută, deci trecerea probează că poarta nu se poate rupe la pornire); și faptul că re-adăugarea oricăreia din cod ar arunca — motivul pentru care blocul a trebuit să dispară.

Asta e apărarea reală împotriva pericolului feliei: o regenerare de designer care pierde sau reordonează intrări. `IsViewEnabled` aruncă `ArgumentException` pe o cheie necunoscută, deci o intrare pierdută nu e o pagubă cosmetică.

**Notă de regresie deja acoperită:** `DdfView` conține și el un `KBotNavList` autorit în designer. Când VS regenerează `DdfView.Designer.vb`, va emite acum și `BeginInit()`/`EndInit()` în jurul lui `navSub`. Calea aia a fost gândită: `EndInit` pe o colecție goală validează fără să arunce, iar `SelectedKey = Nothing` reținut se aplică prin `ClearSelection()`, adică no-op. `DdfView.vb` continuă să-și adauge cele patru pagini în cod.

**Ce rămâne neverificat aici:** nimeni nu a deschis `MainForm` în designer după migrare, deci nu se știe dacă VS **reîncarcă** cele opt elemente în dialogul de colecție și dacă le **rescrie** identic la prima regenerare. Ce se știe: se compilează, se construiește headless, și starea de rulare e identică cu cea de dinainte (fixată de cele cinci teste). Dacă VS refuză rotunda, calea de întoarcere e blocul `AddItem` din istoricul git — nu e o fundătură.

## 7. Nefăcut / neverificat, explicit

- **Rotunda din designer, Toolbox-ul, marcajul roșu și grila de proprietăți** — vezi §4 și §5. NERULATE, nu «verificate». Asta include acum și `MainForm`: cele opt intrări sunt scrise în `MainForm.Designer.vb` în forma pe care o emite designer-ul, dar **nimeni nu a deschis formularul în designer după migrare** — dacă VS le reîncarcă în dialogul de colecție și le rescrie identic la prima regenerare rămâne întrebarea deschisă a feliei. Starea de RULARE e fixată de cinci teste, iar calea de întoarcere (blocul `AddItem`) e în istoricul git.
- **Fără manipulare directă pe suprafața de design** (decizia 2 a planului): nu poți da click pe un buton din bară în designer, nu îl poți trage ca să-l reordonezi și nu îl poți șterge cu Del. Ar cere un `ControlDesigner` propriu, iar pe `net8.0-windows` designer-ul rulează în alt proces, deci orice designer / `UITypeEditor` / `TypeConverter` propriu ar avea nevoie de un assembly de design-time separat, contra `Microsoft.WinForms.Designer.SDK`. În afara scopului, deliberat.
- **Fără editare de noduri pentru `AdvancedTreeControl`** — arborele din `MainForm` se umple din `GET /api/forexe/tree`, deci nodurile autorite în designer ar fi aruncate la încărcare.
- **Fără migrarea coloanelor vederilor existente** — `IstoricView`, `PlatiView`, `SumarView`, `RezervariView`, `ReceptiiView`, `DdfView` și formularele din DevHarness își construiesc în continuare coloanele în cod cu `AddColumn`, neatinse. Felia asta doar face autorarea din designer POSIBILĂ.
- **`ComboItems` și `Tag` nu sunt serializabile prin proiectare** (§1.2).
- **Jumătatea rămasă din limitarea 0013**: editarea proprietăților unei coloane după încărcare tot cere `AutoSizeColumns()` explicit. Doar add/remove/clear declanșează layout singure acum.
- `<DefaultEvent("SelectionChanged")>` pe `KBotNavList` numește un eveniment cu semnătură VB (`SelectionChanged(key As String)`), nu `EventHandler`. Atributul e inofensiv, dar dacă dublu-click-ul pe control în designer nu generează handler-ul, asta e cauza — **neverificat**.

## SLICE-0025-02 — pictogramă stânga pe elementele de navigație <a id="slice-0025-02"></a>

Pasul 2 al feliei 0025 (vezi [0025-baza](#slice-0025-baza) pentru pasul 1 și pentru pasul 7). Cerere de operator, în afara planului original: «elementele de navigație trebuie să aibă încă o proprietate pentru o pictogramă în stânga; să fie ca orice pictogramă de buton».

### 1. Ce s-a schimbat

#### `KBotNavItem.Image`

```vb
<Category("K-BOT")>
<Description("Pictograma desenată la stânga textului (ca Image-ul unui buton). Ignorată pe separatori.")>
Public Property Image As System.Drawing.Image
```

**Numele e `Image`, nu `Icon` sau `IconImage`, deliberat.** Cererea a fost «ca orice pictogramă de buton», iar echivalentul exact e `Button.Image`: același nume, același tip, **același editor de imagine din grila de proprietăți** (cel stoc — designer-ul depune imaginea în `.resx`-ul formularului și emite `KBotNavItem1.Image = CType(resources.GetObject(…), Image)`). Cine caută proprietatea o va căuta sub numele ăsta. `KBotCaptionBar` folosește `IconImage`, deci în casă există acum două nume pentru aceeași idee — abaterea e conștientă și e consemnată aici; bara de titlu nu se editează prin dialog de colecție, deci cele două nu se întâlnesc niciodată în aceeași grilă.

Tipul e scris CALIFICAT (`System.Drawing.Image`). În VB, o proprietate `Image` din aceeași clasă ascunde numele de tip `Image`, deci `Public Property Image As Image` nu compilează.

Trei decizii mici:
- **Fără `ImageAlign`.** Un rând de navigație e „pictogramă apoi text", prin construcție. O aliniere pe care nimeni nu a cerut-o e încă un lucru care se poate seta greșit.
- **Separatorii o ignoră**, exact ca pe `Key` și `Text`.
- **Elementul NU deține imaginea** și nu o eliberează niciodată: ea aparține apelantului sau resurselor formularului, ca `KBotCaptionBar.IconImage`. Test dedicat — o imagine partajată de două elemente nu are voie să moară odată cu primul.

#### `KBotNavList.IconSize`

Latura (px logici, scalați la DPI) a pătratului în care se desenează pictograma; implicit **20**, `DefaultValue`, deci designer-ul nu o serializează degeaba.

Se aplică TUTUROR elementelor, nu per element: într-o bară de navigație pictogramele trebuie să aibă aceeași mărime, altfel textul nu se mai aliniază pe verticală. Sursa poate fi de orice dimensiune (o probă de 64px se scalează în pătratul de 20), deci nu impune lățimea rândului. `IconSize = 0` înseamnă «fără pictograme» — nu «pictograme de zero pixeli care lasă totuși o gaură»: textul revine la padding-ul simplu.

#### Geometrie și pictare

Trei funcții private, folosite **și** la măsurare **și** la pictare, ca cele două să nu se poată despărți (dacă slotul crește, textul se mută cu el, automat):

| Funcție | Ce dă |
|---|---|
| `IconSide()` | latura nominală, scalată la DPI |
| `IconSlotWidth(it)` | cât mănâncă pictograma din lățime (0 fără imagine / pe separator / `IconSize = 0`) |
| `IconRect(it, r)` | pătratul efectiv, centrat vertical, **strâns** dacă rândul e mai scund decât latura nominală |

`ItemExtent` (orizontal) adaugă `IconSlotWidth`; pe vertical înălțimea rândului e fixă, deci pictograma nu schimbă nimic. Textul începe acum de la `r.Left + padX + IconSlotWidth(it)` în loc de `r.Left + padX`. Badge-ul rămâne neatins (e ancorat la dreapta).

**Pe un element dezactivat pictograma se estompează**, ca imaginea unui `Button` dezactivat: `ImageAttributes` + `ColorMatrix` cu luminanța standard (0.299/0.587/0.114) și alfa 45%. Fără asta, un buton stins ar avea text gri lângă o pictogramă în culori vii. S-a ales matricea în locul lui `ControlPaint.DrawImageDisabled` fiindcă aceea nu scalează imaginea, iar noi desenăm mereu într-un pătrat impus.

### 2. Fișiere atinse

- `src/KBot.Theming/KBotNavItem.vb` — proprietatea `Image`
- `src/KBot.Theming/KBotNavList.vb` — `IconSize`, cele trei funcții de geometrie, `ItemExtent`, pictarea, `DrawItemImage`, două cârlige `Friend` de test, `Imports System.Drawing.Imaging`
- `src/KBot.Theming/KBot.Theming.vbproj` — `FileVersion` 1.1.0.0 → **1.2.0.0** (`AssemblyVersion` stă: suprafața doar crește)
- `tests/KBot.Theming.Tests/KBotNavListTests.vb` — 8 teste noi
- `docs/worklog/SLICE-0025-02-navitem-icon.md` (acesta), `docs/worklog/KBOT_STATUS.md`

Niciun element din `MainForm` nu primește o pictogramă în pasul ăsta — s-a cerut PROPRIETATEA, nu un set de pictograme. `MainForm.Designer.vb` e neatins, `KBot.App` nu primește bump.

### 3. Rezultate de teste

Publicat înainte (`artifacts\KBot_Debug_20260807_101720`), rulat pe mașina clientului, ieșirea în `src\KBot.App\bin\Debug\net8.0-windows\win-x64\Logs\test_20260807_101750_823.log`.

**769 passed / 0 failed / 0 skipped** (761 înainte de pas; `KBot.Theming.Tests` 51 → **59**). Build: **0 erori, 0 avertismente**.

Cele 8 teste noi: un element fără imagine lasă textul exact unde era (regresie pe rândurile existente); o imagine primește un slot PĂTRAT, lipit de padding-ul din stânga, centrat vertical, și împinge textul cu `slot + spațiu`; `IconSize` conduce și slotul și decalajul textului, iar 0 le anulează pe amândouă; slotul se STRÂNGE într-un rând mai scund decât latura nominală și nu iese din el; o imagine lățește un element ORIZONTAL dar nu unul vertical; separatorul își ignoră imaginea; pictarea headless trece prin ambele ramuri (activ / dezactivat), ceea ce contează fiindcă o matrice de culoare greșită ARUNCĂ, nu doar arată prost; imaginea supraviețuiește eliberării barei.

**Corecție adusă și testelor existente din pasul 1:** patru assert-uri comparau cu pixeli logici literali (`20`, `12`, `nav.Height - 6`). Treceau aici fiindcă mașina e la 96 dpi, dar ar fi picat pe un ecran la 150%. Toate trec acum prin `ThemeShapes.ScaleDpi` (vizibil testelor prin `InternalsVisibleTo`), deci verifică relația, nu numărul.

### 4. Nefăcut / neverificat

- **Nimic nu a fost văzut pe ecran.** Cum arată o pictogramă de 20px lângă textul de navigație, dacă estomparea de 45% e destulă pe schema Dark, și dacă interpolarea bicubică e curată la scalarea unei surse mari — toate sunt judecăți vizuale, iar nimeni nu a rulat aplicația. Testele probează GEOMETRIA și faptul că pictarea nu aruncă; atât.
- **Editorul de imagine din grila de proprietăți nu a fost deschis** — ca tot restul feliei 0025, vezi §4 din worklog-ul pasului 1. În particular, rotunda unei imagini printr-un element de COLECȚIE (designer-ul trebuie să scrie în `.resx` și să emită `CType(resources.GetObject("KBotNavItem1.Image"), Image)`) e mecanismul standard, dar **nu a fost probat**; e prima proprietate din felie care are nevoie de fișierul de resurse al formularului.
- **`IconSize` e per bară, nu per element.** Dacă apare vreodată nevoia de pictograme de mărimi diferite în aceeași bară, asta e schimbarea de făcut — nu s-a construit din start.
- Nicio pictogramă reală nu a fost aleasă sau desenată pentru cele opt intrări din `MainForm`. `KBot.App` are deja generatoare GDI tematizabile (`FxIcons`, `DdfIcons`, `PlatiIcons`, `RezervariIcons`, `ReceptiiIcons`) — dacă intrările primesc pictograme, de acolo ar veni, prin cod, nu prin `.resx`.

## SLICE-0025-05 — `KBotNavList`: bară colapsabilă + `ItemPadding` <a id="slice-0025-05"></a>

Pasul 5 al feliei 0025 (vezi [0025-baza](#slice-0025-baza) pentru pasul 1 și pasul 7, [0025-02](#slice-0025-02) pentru pictogramă). Cerere de operator, în afara planului:

> «navlist-ul să aibă o proprietate vizibilă în designer "Colapsable". Când e true, o pictogramă
> mică apare într-unul din colțuri (proprietatea `CollapseCorner {TopLeft, TopRight, BottomLeft,
> BottomRight}`). Strâns are două opțiuni: "Complete" și "Icons". La "Complete" lățimea barei (sau
> înălțimea, dacă e orizontală) devine puțin mai mare decât butonul de strângere. La "Icons", doar
> pe verticală, se redimensionează și bara și butoanele la lățimea pictogramei plus niște padding;
> dacă niciun buton nu are pictogramă, opțiunea nu e disponibilă. Merg din același buton: 1 =
> icons, 2 = complet, 3 = mărimea inițială. În plus, navlist-ul trebuie să expună o proprietate de
> padding, ca să pot controla aerul din jurul butoanelor.»

### 1. Ce s-a schimbat și de ce

#### 1.1 `ItemPadding` — aerul din jurul butoanelor

```vb
<Category("K-BOT")>
<DefaultValue(GetType(System.Windows.Forms.Padding), "6, 6, 6, 6")>
Public Property ItemPadding As System.Windows.Forms.Padding
```

Înainte, `RecalcLayout` folosea o **margine fixă** `ThemeShapes.ScaleDpi(Me, 6)` pe toate cele patru laturi. Acum cele patru laturi vin din proprietate; implicit tot 6, deci o bară care nu atinge proprietatea arată **exact** ca înainte (fixat de un test dedicat, nu doar afirmat).

Pe verticală `Left`/`Right` strâng coloana butoanelor (deci și lățimea lor, cât sunt pe „umple bara"), iar `Top`/`Bottom` depărtează primul buton de sus și ultimul buton `Far` de jos. Pe orizontală rolurile se inversează. Valorile negative se aduc la 0, ca la `IconSize` / `ItemWidth` — un setter de dimensiune care aruncă ar rupe `InitializeComponent` la o valoare greșită din designer.

**De ce `ItemPadding` și nu `Padding`.** `Control.Padding` există deja pe orice control și e **scalat automat de WinForms** la autoscalarea formularului (`Control.ScaleControl` ajustează `Padding` și `Margin`). Bara asta își scalează singură fiecare măsură logică prin `ThemeShapes.ScaleDpi` — înălțimea rândului (36), padding-ul intern al textului (12), latura separatorului (11), `IconSize`, `ItemWidth`. Dacă aerul din jurul butoanelor ar sta pe `Control.Padding`, aceeași valoare ar fi ajustată de **două** ori pe un ecran la 150%. Așa că:

- `ItemPadding` = px **logici**, scalat de noi, consecvent cu tot restul barei;
- `Control.Padding` moștenit e **ascuns din grilă** (`Browsable(False)` + `DesignerSerializationVisibility.Hidden`, prin `Shadows`), fiindcă pe un layout owner-drawn nu face nimic. O proprietate care nu face nimic, lăsată în grila de proprietăți exact lângă una care face, este prima pe care ai încerca-o.

#### 1.2 Strângerea barei

Trei proprietăți + un eveniment + o metodă:

| Membru | Rol |
|---|---|
| `Collapsible As Boolean` (implicit `False`) | desenează butonul mic din colț și rezervă banda lui |
| `CollapseCorner As KBotNavCorner` (implicit `TopRight`) | `TopLeft` / `TopRight` / `BottomLeft` / `BottomRight` |
| `CollapseState As KBotNavCollapseState` | `Expanded` / `Icons` / `Complete`; **nu se serializează** |
| `IconsCollapseAvailable As Boolean` (ReadOnly) | dacă „Icons" are sens ACUM |
| `ToggleCollapse()` | trece la starea următoare, ca un click pe buton |
| `CollapseStateChanged(state)` | ridicat la fiecare trecere reală |

**Numele.** Cererea zicea `"Colapsable"`; proprietatea se numește `Collapsible` — ortografia engleză corectă, consecventă cu `CollapseCorner` / `CollapseState`, care au venit în engleză din aceeași cerere. Restul API-ului barei e tot în engleză (`Items`, `Orientation`, `IconSize`, `ItemWidth`); descrierile din grilă rămân în română.

**Ciclul** e cel cerut: `Expanded → Icons → Complete → Expanded`. Butonul **sare** starea `Icons` când nu e disponibilă (bară orizontală, `IconSize = 0`, sau niciun buton **vizibil** cu pictogramă) — un buton care aruncă în fața operatorului fiindcă nicio intrare n-are pictogramă ar fi o pedeapsă pentru o alegere de autorare. Setter-ul `CollapseState`, în schimb, **aruncă** `InvalidOperationException` pe aceeași stare: e API, iar regula casei e „fără no-op-uri tăcute".

**Dimensiunile** (px logici, scalați):

| Stare | Latura pe axa care se strânge |
|---|---|
| `Complete` | `18 + 2*6 = 30` — «puțin mai mult decât butonul» |
| `Icons` | `ItemPadding.Left + ItemPadding.Right + IconSize + 2*8` (implicit `6+6+20+16 = 48`), niciodată sub `Complete` |
| `Expanded` | ultima mărime avută **desfășurată** |

Lățimea butoanelor în `Icons` = `IconSize + 2*8` (implicit 36) și bate **și** `AutoSize` pe element **și** `ItemWidth` pe bară: bara s-a strâns la lățimea unei pictograme, nicio cerere de lățime nu mai are ce cere.

**„Mărimea inițială" e a operatorului, nu a constructorului.** `OnSizeChanged` reține mărimea curentă ca `_expandedExtent` cât timp starea e `Expanded`, deci dacă operatorul lățește bara la 240 și apoi o strânge, al treilea click îi dă înapoi 240, nu 170. Steagul `_applyingCollapseExtent` oprește propriile noastre redimensionări să fie confundate cu ale lui — fără el, prima strângere ar deveni noua „mărime inițială" și bara nu s-ar mai putea desfășura niciodată.

**Banda rezervată.** Butonul din colț își ia o bandă de `18 + 2*6` la capătul axei principale dinspre colțul ales (sus/jos pe verticală, stânga/dreapta pe orizontală), ca să nu stea peste primul sau ultimul buton. Se pictează ultimul, peste tot, fiindcă pe o bară foarte îngustă un buton lat tot i-ar putea intra pe dedesubt.

**`Complete` nu lasă niciun slot.** `RecalcLayout` iese devreme, toate `Bounds` rămân `Rectangle.Empty` — deci nici pictarea, nici `IndexAt`, nici hover-ul nu mai ating vreun buton. Un buton pictat „tăiat" într-o bară de 30px ar fi fost gunoi vizual, iar unul apăsabil, invizibil, ar fi fost mai rău.

**Pictarea în `Icons`:** pictograma se **centrează** (nu mai are text lângă care să stea), textul dispare, iar pastila badge-ului devine un **punct** în colțul din dreapta-sus — informația «are ceva de arătat» nu are voie să dispară complet. Un buton **fără** pictogramă primește **inițiala** textului, centrată: starea se poate cere de îndată ce UN singur buton are pictogramă, iar restul trebuie totuși să se distingă între ele.

**Butonul** e un unghi („chevron") desenat cu `GraphicsPath`-ul obișnuit, în `TextDimColor` (accent la hover, cu fundal `ButtonHoverColor`). Arată spre începutul axei cât mai sunt trepte de strâns și înapoi din `Complete`. Zero culori hardcodate.

**În designer NU se strânge:** click-ul pe buton e ignorat la design time — ar redimensiona controlul și ar murdări formularul cuiva. Butonul se **desenează** totuși, ca să se vadă unde cade colțul ales.

**Schimbarea axei cât bara e strânsă** (caz marginal, dar tratat): se desfășoară întâi pe axa veche — altfel lățimea strânsă ar rămâne agățată de o axă care nu mai e cea care se strânge —, apoi mărimea curentă de pe axa nouă devine „mărimea inițială", `Icons` se retrogradează la `Complete`, și se restrânge.

### 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Theming/KBotNavList.vb` | enum-urile `KBotNavCorner` / `KBotNavCollapseState`; `ItemPadding`, `Padding` ascuns, `Collapsible`, `CollapseCorner`, `CollapseState`, `IconsCollapseAvailable`, `ToggleCollapse`, evenimentul `CollapseStateChanged`; geometria strângerii; `RecalcLayout` / `IconRect` / `CrossWidthFor` / `OnPaint` / `OnMouseMove` / `OnMouseClick` / `OnMouseLeave` / `OnSizeChanged` / setter-ul `Orientation`; două cârlige `Friend` noi pentru teste |
| `src/KBot.Theming/KBot.Theming.vbproj` | `FileVersion` 1.2.0.0 → **1.3.0.0** (`AssemblyVersion` stă pe loc — nu se rupe nimic la compilare) |
| `tests/KBot.Theming.Tests/KBotNavListCollapseTests.vb` | **nou**, 25 de teste |

Nimic altceva nu s-a atins. `MainForm.navViews` **nu** a primit `Collapsible = True`: s-a cerut capacitatea, nu activarea ei. Cu `Collapsible = False` (implicit) bara se comportă bit cu bit ca înainte — o linie în designer o pornește când se decide.

### 3. Rezultate teste

```
dotnet build KBot.sln     → Build succeeded. 0 Warning(s), 0 Error(s)
dotnet test  KBot.sln     → 812 passed / 0 failed / 0 skipped
                            (KBot.Theming.Tests 76 → 101)
```

Cele 25 de teste noi acoperă: implicitul `ItemPadding = 6` **este** vechea margine fixă; cele patru laturi pe verticală și inversarea lor pe orizontală; clamparea negativelor; `Padding` moștenit ascuns din grilă (prin `TypeDescriptor`, nu prin reflexie brută); butonul în fiecare din cele patru colțuri; banda rezervată la capătul corect, pe ambele axe; ciclul complet prin **click real** (`OnMouseClick`, nu prin setter); saltul peste `Icons` fără pictograme; lățimile din `Icons` (bară + butoane) și centrarea pictogramei; `Icons` peste `AutoSize`/`ItemWidth`; `Complete` fără sloturi și fără lovire cu mouse-ul; întoarcerea la mărimea pe care o pusese operatorul; bara orizontală care își strânge înălțimea, nu lățimea; cele trei stări imposibile care aruncă; pictograma unui buton **ascuns** care NU deblochează `Icons`; retrogradarea la schimbarea axei; `Collapsible = False` care desfășoară imediat; evenimentul, o dată pe treaptă și niciodată pe un no-op; și pictarea headless a fiecărei combinații stare × colț.

### 4. Neverificat / amânat

1. **Niciun verdict vizual.** Ca la tot ce s-a livrat din felia 0010 încoace: nimeni nu a văzut bara strângându-se pe ecran. Chevron-ul, fundalul de hover, pictograma centrată, punctul în locul pastilei și inițiala pe butoanele fără pictogramă sunt probate doar prin `DrawToBitmap` headless — adică «nu aruncă», nu «arată bine». `PaintingEveryCollapsedState_DoesNotThrow` execută fiecare ramură de pictare, atât.
2. **Nedeschis în Visual Studio.** Cele patru proprietăți noi n-au fost văzute în grila de proprietăți; în particular, editorul standard de `Padding` pe `ItemPadding` (cel care desface Left/Top/Right/Bottom) și felul în care designer-ul serializează `ItemPadding` sunt neverificate. Firul deschis (b) al feliei 0025 rămâne deschis.
3. **`Dock` presupus, nu impus.** Strângerea schimbă `Width` (respectiv `Height`). Asta merge cu `Dock.Left`/`Right` (cum e `MainForm.navViews`), `Dock.Top`/`Bottom` sau ancorare. Cu `Dock.Fill` containerul rescrie imediat dimensiunea și bara nu se va strânge — nu e o gardă în cod (n-am inventat o excepție pentru o configurație pe care nimeni n-a cerut-o), e o limitare consemnată aici.
4. **Fără tooltip în `Icons`.** Cu textul ascuns, un tooltip cu numele vederii ar fi util. Nu s-a cerut, nu s-a adăugat.
5. **Fără navigare la tastatură pe butonul din colț.** Se acționează doar cu mouse-ul (sau din cod, prin `ToggleCollapse`). `Tab` intră în bară ca înainte, iar săgețile schimbă selecția — inclusiv cât bara e `Complete`, unde nu se vede nimic. Selecția rămâne o stare validă a controlului, deci n-am blocat-o; dacă deranjează, e o linie în `OnKeyDown`.

## SLICE-0025-06 — `KBotNavList`: pictograme pe butonul de strângere + unghiul care se întoarce <a id="slice-0025-06"></a>

Pasul 6 al feliei 0025 (vezi [0025-baza](#slice-0025-baza) pentru pașii 1 și 7, [0025-02](#slice-0025-02) pentru pictograma elementelor, [0025-05](#slice-0025-05) pentru butonul de strângere însuși). A treia cerere de operator în afara planului:

> «in kbotnavlist, the expander button must have an exposed property to allow for puting a
> expanded icon and an collapsed icon in the designer (and in code). if none is supplied use the
> current `DrawCollapseButton`, but with the mention that when collapsed it should show the other
> arrow `>` instead of the same `<`»

### 1. Ce s-a schimbat și de ce

#### 1.1 Cele două pictograme

```vb
<Category("K-BOT")>
Public Property CollapseExpandedImage As Image     ' desfășurată => butonul strânge
<Category("K-BOT")>
Public Property CollapseCollapsedImage As Image    ' strânsă     => butonul desfășoară
```

Tipul e `Image`, nu `Icon`/`Bitmap`, și numele conține `Image`, exact ca `KBotNavItem.Image` de la pasul 02 — asta e ce face grila de proprietăți să deschidă **editorul de imagine stoc** și designer-ul să depună imaginea în `.resx`-ul formularului. Ambele merg și din cod, evident (setter public, nicio validare de stare).

**Sunt INDEPENDENTE.** Cine dă doar una primește unghiul desenat pe cealaltă stare; cererea a fost «să pot pune o pictogramă», nu «un set complet sau nimic». Rezolvarea se face pe starea curentă, într-un singur loc (`CollapseButtonImage`), folosit și la pictare și de cârligul de test — aceeași regulă ca la geometria pictogramei de la 0025-02: o singură funcție, ca cele două să nu se despartă.

**O singură pictogramă pentru amândouă stările strânse** (`Icons` și `Complete`): din amândouă butonul face același lucru — desfășoară. Trei proprietăți ar fi cerut operatorului să aleagă o imagine pentru o distincție pe care butonul n-o face.

**Bara NU deține imaginea** și nu o eliberează niciodată (`Dispose` rămâne neatins) — ca `KBotNavItem.Image` și `KBotCaptionBar.IconImage`. E a apelantului sau a resurselor formularului; o pictogramă partajată de două bare n-are voie să moară odată cu prima. Test dedicat.

`ShouldSerialize*` / `Reset*` **private** pentru amândouă, ca la `KBotNavItem.Image` (0025-03): un tip referință n-are `<DefaultValue>` utilizabil în VB, iar fără ele designer-ul ar fi scris `navViews.CollapseExpandedImage = Nothing` în fiecare formular care nu s-a atins de proprietate. `TypeDescriptor` le găsește după nume inclusiv nepublice.

Imaginea se desenează scalată în pătratul butonului (18 px logici, minus 2 px de aer), prin `DrawItemImage` — funcția care exista deja pentru pictogramele elementelor. Fundalul de hover rămâne desenat și sub pictogramă: el spune «se poate apăsa», nu «uite o săgeată», deci n-are de ce să depindă de felul glifei.

#### 1.2 Unghiul care se întoarce (defect, nu preferință)

Înainte:

```vb
Dim forward As Boolean = (_collapseState = KBotNavCollapseState.Complete)
```

Adică unghiul se întorcea **doar** din `Complete`. Pe `Icons` rămânea identic cu cel din starea desfășurată (`<` pe o bară verticală), deci o bară strânsă la pictograme arăta ca una desfășurată și butonul părea că nu mai face nimic. Acum:

```vb
Private Function ChevronPointsToExpand() As Boolean
    Return _collapseState <> KBotNavCollapseState.Expanded
End Function
```

Asta contează cu atât mai mult de când operatorul a **scurtat ciclul** (vezi §2): `Complete` nu se mai atinge din buton cât timp `Icons` e disponibil, deci `<` era literalmente singurul unghi pe care apuca să-l vadă cineva pe o bară cu pictograme.

### 2. Ciclul scurtat de operator (schimbare găsită în arbore, nu făcută acum)

`CycleCollapse` avea, necomis, ramura `Icons → Complete` comentată de operator:

```vb
'eliminat de mine (project manager). nu imi place cum arata fara nicio iconita!
'Case KBotNavCollapseState.Icons
'    [next] = KBotNavCollapseState.Complete
```

Deci butonul face acum `Expanded ↔ Icons` cât timp `Icons` e disponibil, și `Expanded ↔ Complete` când nu e (o bară fără nicio pictogramă). Starea `Complete` rămâne în API (setter-ul `CollapseState`) și rămâne acoperită de teste, dar butonul nu mai duce pe nimeni acolo dintr-o bară cu pictograme.

**Două teste din 0025-05 rămăseseră roșii pe schimbarea asta** (`ClickingTheButton_CyclesIcons…` și `CollapseStateChanged_FiresOncePerStep…`, ambele fixau ciclul în trei trepte). Erau roșii ÎNAINTE de pasul 06 — sunt aliniate acum la ciclul care e în cod, cu motivul scris în test. Comportamentul de rulare **nu** a fost schimbat înapoi: decizia e a operatorului.

### 3. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Controls/NavList/KBotNavList.vb` | `CollapseExpandedImage` / `CollapseCollapsedImage` + `ShouldSerialize`/`Reset`, `CollapseButtonImage()`, `ChevronPointsToExpand()`, ramura de pictogramă din `DrawCollapseButton`, două cârlige `Friend` de test |
| `src/KBot.Controls/KBot.Controls.vbproj` | `FileVersion` 1.4.0.0 → **1.5.0.0** (`AssemblyVersion` stă pe loc: doar adăugiri) |
| `tests/KBot.Controls.Tests/KBotNavListCollapseTests.vb` | 7 teste noi; 2 teste existente aliniate la ciclul scurtat (§2) |

Nimic în `MainForm`/`DdfView`: s-a cerut **proprietatea**, nu un set de pictograme. Bara `MainForm.navViews` n-are nici măcar `Collapsible = True` (de la 0025-05).

### 4. Rezultate

- Build `KBot.sln`: **0 erori / 0 avertismente**.
- `dotnet test KBot.sln`: **819 passed / 0 failed / 0 skipped** (812 înainte; `KBot.Controls.Tests` 335 → 342).
- Teste noi: implicitul `Nothing` (⇒ unghi), alegerea pictogramei după starea curentă, aceeași pictogramă pe `Icons` și pe `Complete`, independența celor două (jumătate de set merge), întoarcerea unghiului pe **orice** stare strânsă, `ShouldSerialize`/`Reset` prin `PropertyDescriptor`, faptul că bara nu eliberează imaginile, și pictare fără excepție în toate cele trei stări cu pictograme puse.

### 5. Ce rămâne neverificat / amânat

- **Nimic din pasul ăsta nu s-a văzut pe ecran.** Cum arată o pictogramă de 14 px efectivi în pătratul de 18, dacă `DrawItemImage` (stretch, fără păstrarea proporției) e destul pentru pictograme ne-pătrate, și dacă unghiul întors se citește corect din `Icons` — toate probate doar prin `DrawToBitmap` headless («nu aruncă»), ca tot restul feliei. Firele (a)–(j) din 0025 rămân deschise.
- **Editorul de imagine din grilă n-a fost deschis** pentru cele două proprietăți. E a doua rotundă de IMAGINE din felie care are nevoie de `.resx`-ul formularului (prima e `KBotNavItem.Image`, tot neprobată — firul (f)) — dar aici e o proprietate directă pe CONTROL, nu printr-un element de colecție, deci e calea simplă a celor două, nu cea grea.
- **Nicio proporție păstrată și nicio dimensiune reglabilă** pentru pictograma butonului: se întinde în pătratul de 18 px logici, ca pictogramele elementelor. Dacă operatorul vrea alt raport sau alt pătrat, e o proprietate în plus (`CollapseButtonSize`), nu s-a cerut.
- **Fără pictogramă separată pentru hover / apăsat** și fără estompare pe o bară dezactivată — nu s-au cerut; butonul din colț n-are nici azi stare de „apăsat".

## SLICE-0025-07 — `KBotNavList`: eticheta plutitoare a barei strânse <a id="slice-0025-07"></a>

Pasul 7 al feliei 0025 (nu confunda cu «PASUL 7» din planul original = migrarea `MainForm.navViews`; numerotarea `-0x` a sub-feliilor merge separat de pașii planului). Vezi [0025-05](#slice-0025-05) pentru strângere și [0025-06](#slice-0025-06) pentru butonul din colț. A patra cerere de operator în afara planului:

> «i removed the complete collapse. only icons or expanded. what i need is when is collapsed and i
> hover any navbar button (except for the sep) to have a sort of tooltip with the entire button.
> but it mustn't be a real tooltip (yellow and ugly), but some kind of floating control which takes
> on all the props of the hovered button and displays it on a small timer from left to right (if
> the navbar is vertical - not yet sure for the horizontal)»

### 1. Ce s-a schimbat și de ce

#### 1.1 Nu e un `ToolTip` — e butonul care se desfășoară

Fișier nou: `src/KBot.Controls/NavList/KBotNavFlyout.vb` (fereastra + `KBotNavFlyoutStyle`).

Cheia întregii felii e **de unde pleacă** eticheta. Nu se așază lângă buton (aia ar fi tot un tooltip, doar mai frumos): pleacă **exact din dreptunghiul butonului strâns** și crește doar spre dreapta, iar pictograma se desenează **centrată în prima bandă**, adică fix peste locul unde bara o pictează deja. Ce se vede e un buton care se **desfășoară**, cu pictograma nemișcată și textul ieșind de sub ea — nu două pictograme una lângă alta. Asta rezolvă și ambiguitatea din cerere («the entire button» vs. «doar ce s-a tăiat»): se desenează butonul ÎNTREG, dar suprapus peste el însuși, deci nimic nu apare de două ori.

La progres `0` eticheta e **nedeosebită** de buton (test dedicat); la `1` are lățimea completă.

Preia toate proprietățile butonului de sub cursor: pictograma (estompată la fel, prin aceeași `KBotNavList.DrawItemImage`, devenită `Friend`), textul, pastila de badge, fundalul de selecție (`_selectedFill`) sau de hover, culoarea de accent + fontul semibold pe cel selectat, textul estompat pe cel dezactivat, conturul din `BorderColor` și raza de colț a schemei. Regulile de culoare **NU** sunt rescrise în fereastră: bara le calculează (`BuildFlyoutStyle`) exact ca în `OnPaint` și i le predă gata făcute în `KBotNavFlyoutStyle`, ca logica de temă să nu ajungă în două locuri.

#### 1.2 O fereastră care nu se bagă în seamă

`WS_EX_NOACTIVATE` + `ShowWithoutActivation` (nu fură focusul), `WS_EX_TOOLWINDOW` (fără buton de bară de activități), `AutoScaleMode.None` (bara dă px deja scalați; o a doua ajustare ar dezalinia eticheta de butonul din care trebuie să pară că iese), `Region` tăiată din calea rotunjită (altfel în colțuri s-ar vedea dreptunghiul ferestrei peste vederea de dedesubt).

**`HTTRANSPARENT` pe `WM_NCHITTEST` — nu e cosmetică.** Eticheta acoperă chiar butonul peste care stă cursorul. Fără click-through, apariția ei ar fi însemnat «mouse-ul a părăsit bara» ⇒ eticheta se ascunde ⇒ mouse-ul e iar pe bară ⇒ eticheta apare… la infinit. Cu `HTTRANSPARENT` mouse-ul trece prin ea la bară, hover-ul se păstrează, și click-ul pe pictogramă ajunge tot la buton.

#### 1.3 Cronometrul

Două proprietăți, amândouă în grilă:

| Proprietate | Implicit | Ce face |
|---|---|---|
| `CollapsedFlyout` | **True** | poarta capabilității |
| `FlyoutDelay` | 250 ms | cât stă cursorul înainte să iasă eticheta (0 = imediat) |
| `FlyoutSlideDuration` | 120 ms | cât durează desfășurarea (0 = fără animație) |

`CollapsedFlyout` e **True implicit**, spre deosebire de `Collapsible` (care e False). Nu e o inconsecvență: poarta rămâne `Collapsible` — dacă bara nu se strânge, eticheta nu există. Dar o bară STRÂNSĂ fără etichete e nefolosibilă (nu mai scrie nimic pe butoane), deci implicitul care respectă intenția e «pornit». Negativele se aduc la 0, ca la `IconSize`/`ItemWidth`/`ItemPadding`.

Pasul animației e fix, 15 ms; progresul avansează `15 / FlyoutSlideDuration` pe tic și se oprește exact la `1.0`.

#### 1.4 Cine primește etichetă

Cere: bară strânsă la `Icons`, `CollapsedFlyout = True`, și sub cursor un **buton vizibil cu text**. Săriți: separatorii (cerut explicit), butoanele ascunse, butoanele **fără text** (eticheta n-ar dezvălui nimic peste pictograma deja vizibilă), și butonul de strângere din colț (are prioritate).

**Butoanele DEZACTIVATE primesc etichetă**, spre deosebire de hover-ul obișnuit care le sare: tocmai pe o bară strânsă e cel mai greu de ghicit ce e butonul stins de sub cursor. Se desenează stins, cu culorile lui.

Eticheta se retrage la: părăsirea barei, trecerea pe alt buton (repornește de acolo), trecerea pe butonul din colț, **orice invalidare de layout** (add/remove, vizibilitate, `ItemPadding`, orientare, resize, strângere/desfășurare — ține minte un index și un dreptunghi din layout-ul vechi, deci se retrage, nu se recalculează), și `CollapsedFlyout = False`. Mișcarea ÎN INTERIORUL aceluiași buton nu repornește nimic (altfel temporizarea s-ar reseta la fiecare pixel și eticheta n-ar ieși niciodată). Se reîmprospătează, fără să se retragă, la schimbarea selecției (click pe butonul de sub etichetă) și la schimbarea temei.

#### 1.5 Bara orizontală — întrebarea se închide singură

Operatorul a lăsat-o deschisă («not yet sure for the horizontal»). Nu e nevoie de o decizie: `IconsCollapseAvailable` întoarce False pe orizontală, iar în `Complete` `RecalcLayout` iese din start și **niciun** element nu primește slot. Deci pe o bară orizontală **nu există stare strânsă cu butoane de survolat** — eticheta nu se poate declanșa, prin construcție, nu prin gardă.

### 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Controls/NavList/KBotNavFlyout.vb` | **NOU** — fereastra + `KBotNavFlyoutStyle` |
| `src/KBot.Controls/NavList/KBotNavList.vb` | `CollapsedFlyout`/`FlyoutDelay`/`FlyoutSlideDuration`, geometria (`FlyoutFullWidth`/`FlyoutClientBounds`), decizia (`FlyoutTargetAt`), mașina de stări + cele două cronometre, cârlige în `OnMouseMove`/`OnMouseLeave`/`InvalidateLayout`/`SelectIndex`/`ApplyTheme`/`Dispose`, `DrawItemImage` `Private`→`Friend`, 10 cârlige `Friend` de test |
| `src/KBot.Controls/KBot.Controls.vbproj` | `FileVersion` 1.5.0.0 → **1.6.0.0** |
| `src/KBot.Controls/README.md` | tabelul de foldere: `NavList/` are acum și fereastra |
| `tests/KBot.Controls.Tests/KBotNavListFlyoutTests.vb` | **NOU** — 21 de teste |

Nimic în `MainForm`/`DdfView`: s-a cerut capacitatea. `MainForm.navViews` n-are nici azi `Collapsible = True`, deci nu se schimbă nimic în aplicație.

### 3. Rezultate

- Build `KBot.sln`: **0 erori / 0 avertismente**.
- `dotnet test KBot.sln`: **843 passed / 0 failed / 0 skipped** (819 înainte; `KBot.Controls.Tests` 342 → 366 — 21 de teste noi de etichetă + 3 din felia trecută recontorizate pe fișier nou).
- Ce fixează testele: implicitele; bara desfășurată nu scoate etichetă; separator / buton ascuns / buton fără text nu primesc; butonul DEZACTIVAT primește; la progres 0 eticheta e **exact** butonul; crește doar spre dreapta, păstrând `Left`/`Top`/`Height`; lățimea completă = bandă + text + aer (+ pastilă); progresul e limitat la [0,1]; temporizare → desfășurare → oprire fix la 1.0; `FlyoutDelay = 0` și `FlyoutSlideDuration = 0`; negativele se aduc la 0; mișcarea în același buton nu repornește, pe altul repornește; ieșirea de pe bară, butonul din colț, mutarea sloturilor și desfășurarea retrag eticheta; fereastra se pictează fără excepție pe butonul selectat-cu-pastilă și pe cel stins; bara nu eliberează pictograma primită.

### 4. Ce rămâne neverificat / amânat

- **Nimic nu s-a văzut pe ecran** — al patrulea pas la rând în situația asta. Fereastra nu se arată headless (`RenderFlyout` se retrage când bara n-are handle și n-are formular părinte), deci ce e probat e DECIZIA și GEOMETRIA, nu pixelii. Nedovedite: că fereastra apare unde crede bara, că `HTTRANSPARENT` chiar lasă mouse-ul să treacă la butonul de dedesubt (dacă NU, simptomul e o etichetă care pâlpâie la infinit — vezi §1.2), că `WS_EX_NOACTIVATE` chiar nu fură focusul, cum arată desfășurarea de 120 ms, și dacă `Region` taie colțurile curat sau zimțat.
- **Fără clamp la marginea ecranului.** Eticheta crește spre dreapta din poziția butonului; dacă bara strânsă ar fi lipită de marginea DREAPTĂ a ecranului, eticheta ar ieși în afară. Nu s-a pus gardă: o bară de navigație strânsă e ancorată la stânga formularului prin construcție (`MainForm.navViews` e `Dock.Left`). Limitare consemnată, nu rezolvată.
- **Fereastra nu urmărește formularul.** Dacă formularul se mută sau se redimensionează cât timp eticheta e afară, ea rămâne în coordonatele de ecran vechi până la următoarea mișcare de mouse. În practică nu se poate muta un formular fără să miști mouse-ul de pe bară, dar o mutare programatică ar arăta-o.
- **Fără tastatură.** Navigarea cu Sus/Jos pe o bară strânsă NU scoate etichetă (eticheta e legată de hover, nu de selecție). N-a fost cerut, dar e golul evident dacă bara strânsă trebuie folosită fără mouse.
- **Fără întârziere la retragere.** Eticheta dispare instantaneu la ieșirea de pe buton; nu se desfășoară înapoi. Animația e într-un singur sens, cum s-a cerut («from left to right»).
- Firele (a)–(l) ale feliilor 0025 și 0026 rămân toate deschise.

## SLICE-0025-08 — `KBotNavList`: rază de colț per bară + gradient „modern", din motorul de teme <a id="slice-0025-08"></a>

Pasul 8 al feliei 0025. Continuă direct [0025-07](#slice-0025-07) (eticheta plutitoare). Cerere de operator:

> «the appearing button must have rounded corners (also the buttons in the navlist) similar to the
> button. that property should be exposed in the designer per navlist (not per button) and also it
> should have a modern gradient. also it should use the theming system»

### 1. Ce s-a schimbat și de ce

#### 1.1 Gradientul stă în MOTOR, nu în control

Cererea „also it should use the theming system" s-a citit ca o cerință de arhitectură, nu doar de culori: umplerea nouă e o funcție PUBLICĂ în `KBot.Theming`, nu cod de pictură ascuns în bară.

```vb
' src/KBot.Theming/ThemeShapes.vb
Public Function Lighten(c As Color, amount As Double) As Color
Public Function Darken(c As Color, amount As Double) As Color
Public Sub FillModern(g, path, bounds, baseColor As Color, strength As Integer)
```

`FillModern` **nu introduce nicio culoare nouă**: primește culoarea de bază din paletă (fundalul de selecție derivat din accent, `ButtonHoverColor`, …) și îi calculează cele două capete. Deci gradientul se schimbă odată cu schema, exact ca restul barei, și merge la fel pe schemele deschise și pe cele întunecate — nimic nu e o constantă.

Două decizii de aspect, amândouă cu motiv:

- **Capetele nu sunt simetrice** — partea de jos se închide cu 3/4 din cât se deschide partea de sus. Un gradient simetric arată a buton Windows XP; deschis-sus/abia-închis-jos citește ca lumină căzând de sus, care e tot ce înseamnă „modern" aici.
- **`strength = 0` ⇒ umplere PLATĂ**, pe drumul `SolidBrush`, fără alocare de gradient — deci «bit cu bit ca înainte» rămâne disponibil și e ce se picta până acum.

Zona gradientului se umflă cu un pixel sus și jos: `LinearGradientBrush` pictează primul/ultimul rând cu culoarea capătului opus (artefact vechi de rotunjire), iar așa rândurile stricate cad în afara căii.

**E singura bucată din familia asta de felii care se poate proba HEADLESS pe bune**: un bitmap spune dacă sus e mai deschis decât jos. Patru teste fac exact asta, inclusiv pe o culoare de schemă întunecată.

#### 1.2 Forma, dată per BARĂ

```vb
<DefaultValue(-1)>  Public Property ItemCornerRadius As Integer    ' -1 = raza schemei active
<DefaultValue(14)>  Public Property ItemGradient As Integer        ' 0..100; 0 = plat
```

**Per bară, nu per element** — cerut explicit, și e și singura variantă corectă: într-o navigație butoanele trebuie să aibă aceeași formă, altfel coloana arată ruptă (același raționament ca la `IconSize`, care e tot pe bară). Există un test care cade dacă cineva adaugă vreodată o rază pe `KBotNavItem`, ca decizia să se reia conștient, nu din inerție.

**`-1` = raza schemei active** (`ThemeStyleOptions.CornerRadius`: 0 pe Classic și Dark, 8 pe Modern). Ăsta e implicitul, deci: (a) o bară care nu atinge proprietatea arată exact ca înainte, și (b) urmează tema când operatorul schimbă schema. O valoare de la 0 în sus o înlocuiește — `0` înseamnă „colțuri drepte chiar și pe o schemă rotunjită", nu „nesetat". De aici și `-1` ca sentinel în loc de `0`: pe o proprietate de rază, `0` e o valoare reală.

Sub `-1` se limitează la `-1`, iar gradientul la `[0,100]`: un setter de dimensiune care aruncă ar rupe `InitializeComponent` la o valoare greșită din designer (aceeași regulă ca la `IconSize` / `ItemWidth` / `ItemPadding`).

Raza efectivă se rezolvă într-o SINGURĂ funcție (`ItemRadius()`), folosită de butoane, de butonul din colț ȘI de etichetă — altfel cele trei ar putea să se rotunjească diferit. La fel gradientul: aceeași `ThemeShapes.FillModern` în toate trei.

#### 1.3 Eticheta primește exact aceeași formă

`KBotNavFlyoutStyle` capătă `GradientStrength`, iar `Radius` vine acum din `ItemRadius()`. Eticheta trebuie să fie **butonul care se desfășoară** (vezi 0025-07): o rază sau un gradient care nu se potrivesc ar strica iluzia mai rău decât lipsa lor. Fixat de test. `Region`-ul ferestrei folosea deja `_style.Radius`, deci colțurile ferestrei urmează automat.

### 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Theming/ThemeShapes.vb` | **NOU:** `Lighten`, `Darken`, `FillModern` |
| `src/KBot.Theming/KBot.Theming.vbproj` | `FileVersion` 1.3.0.0 → **1.4.0.0** |
| `src/KBot.Controls/NavList/KBotNavList.vb` | `ItemCornerRadius`, `ItemGradient`, `ItemRadius()`; umplerea butoanelor și a butonului din colț trece prin `FillModern`; `BuildFlyoutStyle` duce raza + gradientul; 2 cârlige `Friend` de test |
| `src/KBot.Controls/NavList/KBotNavFlyout.vb` | `KBotNavFlyoutStyle.GradientStrength`; umplerea prin `FillModern` |
| `src/KBot.Controls/KBot.Controls.vbproj` | `FileVersion` 1.6.0.0 → **1.7.0.0** |
| `tests/KBot.Controls.Tests/KBotNavListShapeTests.vb` | **NOU** — 11 teste |

`AssemblyVersion` stă pe loc peste tot: totul e aditiv.

### 3. Rezultate

- Build `KBot.sln`: **0 erori / 0 avertismente**.
- `dotnet test KBot.sln`: **854 passed / 0 failed / 0 skipped** (843 înainte; `KBot.Controls.Tests` 366 → 377).
- Ce fixează testele: implicitele (`-1` / `14`); `-1` chiar ia raza schemei (Classic 0, Modern 8); o rază explicită bate schema în AMBELE sensuri (rotunjit peste o schemă pătrată și drept peste una rotunjită) și `-1` o redă schemei; valorile din afara intervalului se limitează, nu aruncă; proprietățile sunt pe BARĂ și lipsesc de pe element; eticheta primește exact raza și gradientul barei; `FillModern(strength := 0)` e identic cu umplerea plată **pe pixeli**; gradientul chiar deschide sus și închide jos, **asimetric**, inclusiv pe o culoare de schemă întunecată; și un dreptunghi degenerat (1 px înălțime, lățime 0) nu aruncă.

### 4. Ce rămâne neverificat / amânat

- **Tot nimic pe ecran.** Testele de pixeli probează că `FillModern` face ce spune, NU că 14 e intensitatea potrivită, că 8 px de rază arată bine pe un rând de 36, sau că gradientul se citește la fel pe Dark și pe Modern. Alegerea implicitelor e o judecată, nu o măsurătoare.
- **`Region`-ul ferestrei taie colțurile rotunjite drept, fără antialiasing** — pe o rază mare colțurile etichetei pot ieși zimțate. Nu se poate vedea headless; alternativa (fereastră stratificată cu alfa) e mult mai scumpă și nu s-a cerut.
- **Restul schemelor nu au fost atinse.** `FillModern` e disponibilă acum pentru orice control, dar o folosește DOAR `KBotNavList` (+ eticheta). `KBotDataView`, `KBotCaptionBar`, `KBotBusyBar`, `KBotNotice`, `KBotTextField` și butoanele din `ModernRenderer` rămân pe umplere plată — s-a cerut bara de navigație, iar o trecere generală la gradient e o decizie de aspect pe toată aplicația, nu un efect colateral al acestei felii.
- **`ThemeStyleOptions` NU a primit un flag de gradient.** Intensitatea stă pe control, nu pe schemă. Dacă gradientul se generalizează (punctul de mai sus), locul lui firesc e o proprietate nouă în `ThemeStyleOptions`, serializată în JSON ca restul — moment în care implicitul `ItemGradient` ar trebui să devină un sentinel «ia din schemă», ca `ItemCornerRadius` azi.
- Firele (a)–(p) ale feliilor 0025 / 0026 rămân toate deschise.

## SLICE-0025-09 — `KBotNavList`: latura butonului de strângere, expusă <a id="slice-0025-09"></a>

Pasul 9 al feliei 0025. Închide firul **(l)** deschis la 0025-06 («fără `CollapseButtonSize`»). Cerere de operator:

> «i also need an exposed property for the size of the expander button for navlist»

### 1. Ce s-a schimbat și de ce

```vb
<Category("K-BOT")>
<DefaultValue(18)>
Public Property CollapseButtonSize As Integer      ' px logici; 0 = fără buton
```

Implicit **18** — exact valoarea fixă care stătea până acum în `CollapseButtonSide()`, deci o bară care nu atinge proprietatea arată identic (fixat de test).

#### 1.1 Nu e doar desen — e layout

Cele trei locuri care depindeau de constanta 18 se mută automat pe proprietate, fiindcă toate treceau deja prin `CollapseButtonSide()`:

| Ce | Formulă | Efect al schimbării |
|---|---|---|
| Pătratul butonului | `latură` | se desenează mai mare/mic |
| Banda rezervată pe axa principală | `latură + 2*6` | primul (sau ultimul) element se mută |
| Lățimea în `Complete` | `latură + 2*6` | bara complet strânsă e mai lată |
| Lățimea în `Icons` | `Max(Complete, pad + pictogramă)` | urmează, dacă butonul o depășește |

Din cauza ultimelor două, setter-ul **reaplică dimensiunea pe loc** dacă bara e STRÂNSĂ chiar atunci (`ApplyCollapseExtent`) — altfel o bară pe `Complete` ar fi rămas la lățimea butonului vechi, adică proprietatea ar fi părut că nu face nimic exact în starea în care contează cel mai mult. Test dedicat, inclusiv că desfășurarea readuce **mărimea inițială a operatorului**, nu una derivată din buton.

#### 1.2 Ce e DESENAT în buton se scalează cu el

Unghiul era deja proporțional (brațul = `latură/5`, grosimea liniei = `latură/10`), dar aerul din jurul pictogramei autorate (0025-06) era **fix, 2 px scalați la DPI**. La o latură de 44 pictograma ar fi stat lipită de margini; la una de 8 n-ar mai fi rămas nimic din ea. Devine `latură \ 9`, cu minim 1 — ceea ce dă exact 2 la latura implicită de 18, deci nimic nu se schimbă la valoarea din oficiu, iar cele două glife se scalează acum la fel.

#### 1.3 `0` = fără buton

Aceeași convenție ca `IconSize = 0` («fără pictograme», nu «pictograme de zero pixeli care lasă o gaură»): la 0 butonul nu se desenează, `CollapseButtonRect()` întoarce `Rectangle.Empty` — deci nu poate fi nici survolat, nici apăsat — și, important, **nu mai rezervă bandă** (`CollapseBandExtent` întoarce 0), altfel ar fi rămas o gaură de 12 px în capătul barei pentru un buton inexistent.

E o stare validă, nu o greșeală: o aplicație care strânge bara din propriul ei buton de bară de unelte, prin `ToggleCollapse()` / `CollapseState`, nu vrea și unghiul din colț. **Dar e și o capcană, scrisă ca atare în documentația proprietății:** cu `Collapsible = True` și latura 0, operatorul nu mai poate strânge sau desfășura bara cu mouse-ul — strângerea rămâne exclusiv din cod. Testul verifică ambele jumătăți: click în colț nu mai face nimic, `ToggleCollapse()` da.

Negativele se aduc la 0, ca la fiecare măsură a barei (un setter de dimensiune care aruncă ar rupe `InitializeComponent` la o valoare greșită din designer).

### 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Controls/NavList/KBotNavList.vb` | `CollapseButtonSize`; `CollapseButtonSide()` citește proprietatea; `CollapseButtonRect()` și `CollapseBandExtent()` tratează latura 0; aerul pictogramei devine proporțional |
| `tests/KBot.Controls.Tests/KBotNavListCollapseTests.vb` | 6 teste noi în secțiunea butonului din colț |

`KBot.Controls` `FileVersion` era **deja** pe 1.8.0.0 în arborele de lucru (HEAD e pe 1.7.0.0) — bump-ul necomis acoperă și felia asta, nu s-a mai adăugat unul. `AssemblyVersion` stă pe loc: schimbarea e aditivă.

### 3. Rezultate

- Build `KBot.sln`: **0 erori / 0 avertismente**.
- `dotnet test KBot.sln`: **868 passed / 0 failed / 0 skipped**, `KBot.Controls.Tests` la 391. Cifra pe soluție NU e comparabilă cu cele 854 de la 0025-08: între timp operatorul a comis feliile anterioare și a adăugat WIP propriu pe `AdvancedTreeControl` (inclusiv `AdvancedTreeSearchBarTests.vb`, netrecut prin mine). Contribuția feliei ăsteia, măsurată pe filtrul `KBotNavList`: **116 → 122**.
- Ce fixează testele: implicitul 18 = vechea valoare fixă; butonul ȘI banda se redimensionează împreună, iar primul element coboară cu ele; lățimea din `Complete` urmează **pe loc** o schimbare făcută cât bara e strânsă, iar desfășurarea readuce mărimea operatorului; `0` = niciun dreptunghi, nicio bandă, click-ul în colț inert, dar `ToggleCollapse()` funcțional; negativele se limitează la 0; pictarea nu aruncă la 0 / 6 / 18 / 44, cu pictogramă și fără.

### 4. Ce rămâne neverificat / amânat

- **Tot nimic pe ecran** (a cincea felie la rând). Nu se știe cum arată un buton de 30 față de unul de 18 lângă rândurile de 36 px, nici dacă unghiul rămâne lizibil la laturi mici.
- **Marginea din jurul butonului (6 px logici) NU e expusă.** S-a cerut latura; marginea intră în bandă și în lățimea strânsă la fel de mult, deci dacă se dorește control complet asupra colțului e o proprietate în plus (`CollapseButtonMargin`), nu o ajustare a acesteia.
- **Nicio gardă pentru un buton mai lat decât bara.** Pe o bară DESFĂȘURATĂ îngustă, o latură mare împinge dreptunghiul la coordonate negative (butonul iese pe stânga). În stările strânse nu se poate întâmpla — acolo lățimea barei se calculează DIN latura butonului. Limitare consemnată, aceeași filozofie ca la `ItemWidth`: valoarea e a operatorului.
- Firele (a)–(k) și (m)–(t) ale feliilor 0025 / 0026 rămân deschise; **(l) se închide pe jumătate** — `CollapseButtonSize` există acum; păstrarea proporției pictogramei și o glifă separată de hover/apăsat tot nu.

## Notă de trasabilitate din registrul 0025

La checkpointul registrului, (j) **pașii 03 (`ItemWidth`) și 04 (`AutoSize` pe element + măsurarea cu fontul semibold) există în cod și în teste, dar NU au worklog** — sunt descriși doar în comentariile din `KBotNavList.vb`/`KBotNavItem.vb`; gaura e din feliile anterioare, nu din 05. Această constatare este păstrată ca istoric, fără reconstruirea unor workloguri 03/04 sau inventarea unor probe.
