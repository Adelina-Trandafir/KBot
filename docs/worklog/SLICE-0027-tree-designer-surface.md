# SLICE-0027 — Istoricul AdvancedTreeControl și al gazdelor lui

Consolidare D001-14: baza, 02 și 03 sunt reunite cronologic. Corpul tehnic, probele, fișierele și restanțele fiecărei etape sunt păstrate. Regulile citate în istoric nu înlocuiesc AGENTS actual.

Acceptarea vizuală din 08.08 privește baza; 02 are verdict parțial și corecții ulterioare nereprobate, 03 are vederi nevăzute pe ecran. TreeFont/FontName/FontSize din baza istorică sunt eliminate ulterior de 02, Font fiind supraviețuitorul. Contractul gazdei pentru strângere, doar descris inițial, este implementat în corecția 02 și apoi în cele patru vederi la 03.

## SLICE-0027 — `AdvancedTreeControl`: banda de căutare, tematizare și suprafață de designer <a id="slice-0027-baza"></a>

Cerere de operator, în două runde, fără plan prealabil.

**Runda 1:**

> «read main form. even though i set in the tree to have searchbar visible it's not showing it.
> what i need: fix this situation. also, when visible, the designer should properly show it in the
> control (like it does for the header). also update the harness form for the treeview and build
> something like the playground for the dgv - with all properties changeable at run-time (just like
> the dgv)»

**Runda 2:**

> 1. «the colors for the searchbar, searchtextbox and header backcolor are not being applied when
>    set-up in the designer»
> 2. antetul are nevoie de: font cu toate proprietățile, textalign, fundal în degrade (din backcolor
>    spre alb pe temă luminoasă / spre negru pe temă întunecată), imagini pentru SearchIcon,
>    RightIcon și LeftIcon
> 3. banda de căutare are nevoie de: padding în jurul butonului de golire, imagine pe buton,
>    golire la ESC, font pentru etichetă ȘI pentru casetă (bold/italic pot dispărea)
> 4. «the tree itself must expose a collection of keys for nodes and also a collection of images
>    which can be imported through the designer IDE»

Arborele era ultimul control K-BOT rămas în afara convențiilor casei: netematizat, needitabil
serios din designer și cu o bandă de căutare care nu se aprindea niciodată din designer. Felia
închide toate trei.

---

### 1. De ce nu se vedea banda de căutare

`SearchShow = True` era **o proprietate moartă pe calea designerului**. Singurul cod care deschidea
banda trăia în `ResolveHeaderIcons` (`AdvancedTreeControl.Search.vb`), adică pe calea
constructorului XML FOREXE (`Tree.Builder`). Un formular autorit în designer nu cheamă niciodată
acea metodă, deci `tree.SearchShow = True` din `MainForm.Designer.vb` scria un câmp și atât.

Corectura:

- setterul `SearchShow` cheamă `ApplySearchShow()`, care deschide banda când NU există iconiță de
  toggle în antet (bandă permanentă) și o închide când proprietatea trece pe False;
- `ResolveHeaderIcons` și setterul `HeaderSearchIcon` trec prin aceeași metodă;
- s-a adăugat un `OnHandleCreated` care re-aplică proprietatea: ordinea în care designerul scrie
  proprietățile în `InitializeComponent` **nu e a controlului**, iar `SearchShow` poate ajunge
  înaintea fontului sau a iconițelor;
- `CloseSearchMode` s-a spart în două: garda «banda permanentă nu se închide» rămâne acolo,
  iar `ForceCloseSearchMode` face demontarea necondiționată (folosită de `SearchShow = False`).

### 2. Banda desenată la design-time

`DrawSearchBarPreview` desenează banda întreagă — eticheta cu fontul ei, caseta în
`SearchBoxBackColor`, placeholder-ul, butonul ✕ (glifă sau imagine) — urmând pas cu pas geometria
din `PositionSearchTextBox`, ca trecerea design-time → runtime să nu mute nimic.

În designer NU se creează controale copil reale: un `TextBox` viu pe suprafața de design fură
click-urile și apare ca element ne-selectabil. Detecția folosește `KBotDesignTime.IsDesignTime`
(ajutorul casei din felia 0025), nu `Control.DesignMode` — pe `net8.0-windows` designerul rulează
**în alt proces** (`DesignToolsServer.exe`), iar un control imbricat nici măcar nu e «sitat».

### 3. De ce culorile din designer nu se aplicau — două cauze, ambele structurale

**Cauza 1 (cea ascunsă).** `ThemeManager.Traverse` **recurge în copiii** oricărui control care nu e
`IThemedControl`. Copiii interni ai benzii de căutare cădeau pe regulile generice pe tip —
`TextBox → InputBackColor`, `Label → TextColor`/Transparent — deci `SearchBoxBackColor` era șters
la **fiecare** aplicare de temă. Exact capcana descrisă în comentariul lui `IThemedControl`, care
lovise deja `TextBox`-ul intern din `KBotTextField`.

**Cauza 2.** `MainForm.OnThemeChanged` și cele patru vederi împingeau explicit culorile paletei
peste cele din designer.

Corectura: `AdvancedTreeControl` **implementează acum `IThemedControl`** (partiala nouă
`.Theming`), deci traversarea se oprește la el, iar toate împingerile manuale au dispărut din
`MainForm` și din cele patru vederi. CLAUDE.md spunea că arborele e deliberat netematizat și că
retrofitul e amânat — felia asta îl face, pentru că era singurul mod de a onora cererea.

**Contractul de culoare, nou:** `Color.Empty` = «auto, din temă»; orice culoare setată câștigă,
definitiv. Fiecare culoare a primit `ShouldSerialize*`/`Reset*`.

Perechea `ShouldSerialize`/`Reset` contează mai mult decât pare: **fără ea, VS serializa în
`.Designer.vb` implicitul REZOLVAT**, iar acela devenea «alegere a operatorului». Așa ajunsese
paleta luminoasă înghețată în cinci fișiere de designer — 13–14 linii de culoare pe fișier, pe
care nimeni nu le alesese. Au fost scoase; singura păstrată e `SearchBoxBackColor` verde din
`MainForm`, care chiar e o alegere.

Câmpurile `_auto*` pornesc de la **exact** culorile hardcodate dinainte de tematizare, ca un host
NEtematizat (bancul de probă, calea FOREXE/VBA) să arate neschimbat.

#### 3.1 `BackColor`/`ForeColor` — gaura care mai rămăsese

Prima variantă a feliei a lăsat o portiță, semnalată de operator la recitire: suprafața și textul
nodurilor sunt `Control.BackColor`/`ForeColor`, iar acolo **nu ajunge contractul `_auto*`** —
controlul ținea doar steagurile `_backColorPinned`/`_foreColorPinned`, fără să suprascrie
`ShouldSerializeBackColor`.

De ce contează: `Control.ShouldSerializeBackColor` întoarce True de îndată ce proprietatea a fost
SCRISĂ vreodată, iar arborele o scrie singur de două ori — în constructor (albul implicit) și în
`ApplyTheme`. Visual Studio ar fi serializat deci un `tree.BackColor = <culoarea temei>` în
designerul formularului gazdă, **pe care nimeni nu l-a ales**; la următoarea încărcare linia ar fi
trecut prin setterul public, ar fi ridicat steagul de fixare, și arborele n-ar mai fi urmat
niciodată tema. Adică exact bug-ul pe care felia îl repară, reintrodus pe ușa din dos — și e chiar
mecanismul prin care `tree.BackColor = Color.White` ajunsese în toate cele cinci designere.

Corectura: `ShouldSerializeBackColor`/`ShouldSerializeForeColor` suprascrise ca să răspundă din
**steagul de fixare**, nu din punga de proprietăți a lui `Control`, plus `ResetBackColor`/
`ResetForeColor` care coboară steagul și reaplică valoarea din temă (câmpurile noi `_autoNodeBack`
/`_autoNodeFore`).

Verificat că restul controalelor tematizate NU au aceeași gaură: niciunul nu scrie
`BackColor`/`ForeColor` în `ApplyTheme` — se pictează singure din paletă în `OnPaint`. Arborele era
expus tocmai fiindcă moștenește o suprafață pictată real (`e.Graphics.Clear(Me.BackColor)`).

#### 3.2 Restul zgomotului din designere

Aceeași întrebare, pusă empiric: **ce scrie designerul într-un formular gazdă pentru un arbore pe
care nimeni nu l-a atins?** Numărând liniile `tree.*` din cele cinci fișiere, șapte proprietăți
apăreau în TOATE, adică erau implicituri serializate, nu alegeri:

| Proprietate | De ce se scria | Corectura |
|---|---|---|
| `TreeFont` | Un `Font` nu poate purta `<DefaultValue>` (atributul cere o constantă) | `ShouldSerializeTreeFont`/`ResetTreeFont` |
| `LeftIconSize`, `RightIconSize`, `HeaderIconSize` | `Size` fără `<DefaultValue>` | `ShouldSerialize*`/`Reset*` |
| `Font` | Constructorul îl atribuie («Segoe UI, 9») ⇒ `Control.ShouldSerializeFont` = True | `Font` suprascris cu steag de fixare + `ShouldSerializeFont`/`ResetFont`; ctor trecut pe `MyBase.Font` |
| `AutoScrollMinSize`, `AutoScrollPosition` | Sunt `Shadows`, deci **nu moștenesc** atributele de serializare ale bazei | `<Browsable(False)>` + `DesignerSerializationVisibility.Hidden` |

Cele cinci designere au fost curățate din nou (−4 linii fiecare; restul liniilor `tree.*` sunt acum
verificat alegeri reale — antetul stilizat din `MainForm`, dimensiunile de iconițe din vederi).

**Limită cunoscută, NU reparată aici — fontul nu se moștenește.** Steagul de fixare rezolvă doar
SERIALIZAREA. Constructorul atribuie în continuare «Segoe UI, 9» explicit, deci arborele nu
moștenește fontul ambiant, iar `ThemeManager.ApplyBaseFont` — care pune fontul schemei pe FORMULAR
și se bazează pe moștenire — nu ajunge la el. Ar fi fost o linie de șters din constructor, dar
`Me.Font` alimentează înălțimea benzii de căutare și `RecalculateItemHeight`, deci schimbarea se
VEDE: după ce felia a primit verdict vizual, n-am schimbat unilateral cum arată. `ResetFont()` redă
moștenirea pentru cine o vrea, iar testul
`Fontul_implicit_e_explicit_iar_ResetFont_reda_mostenirea` fixează comportamentul REAL, ca nimeni
să nu presupună mai mult. **De decis de operator.**

### 4. Antet

| Proprietate | Ce face |
|---|---|
| `HeaderFont` | Font întreg pentru caption; nesetat = `TreeFont` |
| `HeaderTextAlign` | `ContentAlignment` complet (toate cele nouă poziții), aplicat pe tot șirul de fragmente mini-html |
| `HeaderBackStyle` | `Solid` / `GradientVertical` / `GradientHorizontal` |
| `HeaderGradientEndColor` | Gol = **automat**: spre alb dacă baza e deschisă, spre negru dacă e închisă |
| `HeaderLeftIcon` / `HeaderRightIcon` / `HeaderSearchIcon` | Devenite `Browsable` + serializabile — se aleg din selectorul standard de imagini (designerul le pune în `.resx`) |

Capătul automat al degradeului se deduce din **luminanța** culorii de bază (BT.601), nu din temă:
așa arborele dă «spre alb pe temă luminoasă, spre negru pe temă întunecată» fără să știe ce temă
rulează. Cheile `*IconKey` supraviețuiesc și se rezolvă din `NodeImages`.

### 5. Banda de căutare

- `SearchClearButtonPadding` (`Padding`) — intră în lățimea rezervată butonului;
- `SearchClearButtonImage` — imagine din designer; nesetată = glifa «✕»;
- **ESC** golește caseta; pe o casetă deja goală închide banda, dar `CloseSearchMode` e no-op
  pentru banda permanentă, deci acolo ESC doar golește;
- `SearchBarLabelFont` și `SearchBarFont` — fonturi întregi, **în locul** lui
  `SearchBarLabelBold`/`SearchBarLabelItalic`, care s-au **șters** (cerere explicită).

`SearchBarFontName`/`SearchBarFontSize` NU s-au șters: au devenit accesori `Browsable(False)`
peste `SearchBarFont` — exact tiparul deja folosit în acest control de `FontName`/`FontSize` peste
`TreeFont` — ca aplicatorul XML (`Tree.Builder`/`TreeXmlAppliers`) și fișierele de designer
existente să compileze neatinse.

### 6. Colecțiile din designer

- **`NodeImages As ImageList`** — un `ImageList` obișnuit: se pune pe formular, se încarcă pozele
  prin editorul lui (care le scrie în `.resx`) și se leagă de arbore. Așa se «importă imagini prin
  designer» fără niciun editor propriu de tipuri. Arborele NU deține lista și n-o eliberează.
- **`Nodes As TreeNodeDefinitionCollection`** — definiții **plate** (`TreeNodeDefinition`: `Key`,
  `Caption`, `ParentKey`, `ImageKey`, `OpenImageKey`, `RightImageKey`, `Tooltip`, `Tag`,
  `Expanded`, `HasCheckBox`, `LazyNode`), materializate în `Items` de `RebuildFromDefinitions`.

Plat cu `ParentKey`, nu ierarhie de referințe, din **exact** motivul invocat de felia 0025 pentru
`KBotNavItem`: o colecție ierarhică ar cere un editor propriu, iar un editor propriu ar cere un
assembly de design-time compilat contra `Microsoft.WinForms.Designer.SDK`. Prețul nu se plătește
nici aici.

Reguli: ordinea din colecție = ordinea între frați; un părinte declarat **mai jos** funcționează
(treceri repetate); un `ParentKey` inexistent **urcă nodul la rădăcină**, ca greșeala să se vadă,
nu să dispară în tăcere. **Definițiile sunt sursa DOAR cât timp colecția e nevidă** — vederile
existente, care umplu arborele la rulare prin `AddItem`/XML, nu sunt niciodată golite de o
colecție de designer neatinsă.

### 7. Bancul de probă

- **`TreePlaygroundForm`** (nou, + Designer) — perechea playground-ului `KBotDataView`: panou
  stânga derulabil cu **toate** proprietățile comutabile la rulare, în cinci secțiuni (Antet /
  Căutare / Arbore / Tooltip / Date), butoane de temă, `ColorDialog`/`FontDialog` pentru culori și
  fonturi, buton «Reconstruiește din Nodes (designer)» care probează colecția de definiții +
  `NodeImages`, bandă de info live, verdict Pass/Fail. Controalele care n-au efect în starea
  curentă se dezactivează. Apare singur în harness (descoperire prin reflecție), la
  **Controls/UI → «AdvancedTreeControl — proprietăți runtime (playground)»**.
- **`TreeVisualForm`** — pornește acum cu antet + bandă de căutare permanentă, deci proba vizuală
  existentă ar fi prins regresia; loghează `SearchFinished`.

---

### 8. Fișiere atinse

**`src/KBot.Controls/Tree/`**

| Fișier | |
|---|---|
| `AdvancedTreeControl.Theming.vb` | **NOU** — `IThemedControl`, culorile `_auto*`, `ApplyTheme`, `RestyleSearchChildren`, `BackColor`/`ForeColor` suprascrise cu steag «fixat de operator», `AutoGradientEnd`/`Luminance`/`Blend` |
| `AdvancedTreeControl.DesignerNodes.vb` | **NOU** — `Nodes`, `NodeImages`, `NodeImage`, `RebuildFromDefinitions`, `MaterializeNode`, `ResolveHeaderIconsFromNodeImages` |
| `TreeNodeDefinition.vb` | **NOU** — `TreeNodeDefinition` + `TreeNodeDefinitionCollection` |
| `AdvancedTreeControl.Search.vb` | `ApplySearchShow`, `InDesigner`, `RecomputeSearchBarHeight`, `RefreshSearchBarMetrics`, `DrawSearchBarPreview`, `EnsureClearButton`, `ApplyClearButtonLook`, `RefreshClearButton`, `RefreshSearchBarLabel`, `ForceCloseSearchMode`, `ClearSearchText`, ESC, degrade + aliniere în `DrawHeader` |
| `AdvancedTreeControl.Properties.vb` | Culori pe contractul Empty=auto + `ShouldSerialize`/`Reset`; `HeaderFont`, `HeaderTextAlign`, `HeaderBackStyle`, `HeaderGradientEndColor`; iconițe `Browsable`; `SearchBarLabelFont`/`SearchBarFont`; `SearchClearButtonPadding`/`Image`/`Width`; ștergerea Bold/Italic |
| `AdvancedTreeControl.Overrides.vb` | `OnHandleCreated`; `OnFontChanged` re-dimensionează banda |
| `AdvancedTreeControl.vb` | `_nodeDefinitions.Owner = Me`; `MyBase.BackColor` în constructor |

**`src/KBot.App/`** — `MainForm.vb` + `Views/{Ddf,Plati,Receptii,Rezervari}View.vb`: împingerea
de culori pe arbore ștearsă. `MainForm.Designer.vb` + cele patru `*View.Designer.vb`: liniile de
culoare auto-serializate scoase (13–14 pe fișier).

**`src/KBot.DevHarness/`** — `Internal/TreePlaygroundForm.vb` + `.Designer.vb` (noi),
`Tests/AdvancedTreePlaygroundTest.vb` (nou), `Internal/TreeVisualForm.vb` + `.Designer.vb`.

**`tests/KBot.Controls.Tests/`** — `AdvancedTreeSearchBarTests.vb`,
`AdvancedTreeThemingTests.vb`, `AdvancedTreeDesignerNodesTests.vb`, `TestAssemblyInfo.vb` (toate noi).

**Versiuni:** `KBot.Controls` 1.7.0.0 → **1.9.0.0**; `KBot.DevHarness` 1.0.8.0 → **1.0.10.0**;
`KBot.App` 1.0.6.0 → **1.0.7.0**.

---

### 9. Rezultatele testelor

`dotnet build KBot.sln` — **succeeded, 0 warnings, 0 errors**.
`dotnet test KBot.sln` — **884 passed, 0 failed, 0 skipped** (arborele n-avea niciun test
înainte; felia adaugă 23).

Acoperire nouă:

- `AdvancedTreeSearchBarTests` (10) — `SearchShow` deschide/închide banda; iconița de antet
  comută regimul; handle-ul nu dublează banda; ✕ comutat după deschidere; eticheta apare/dispare;
  ESC golește; lățimea butonului include padding-ul și imaginea; **calea de design-time** (arbore
  montat sub un părinte cu `ISite.DesignMode = True`: zero controale copil + un paint care nu aruncă).
- `AdvancedTreeThemingTests` (9) — culoarea din designer supraviețuiește temei; cea lăsată goală o
  urmează; fără temă, «auto» dă culorile istorice; `ShouldSerialize` e False până la o alegere
  reală; `ThemeManager.Apply` nu mai repictează caseta de căutare; arborele chiar implementează
  `IThemedControl`; suprafața nodurilor urmează tema până o fixează cineva; degradeul automat merge
  spre alb/negru după luminanță; și **«tema nu se scurge în designerul gazdei»** — întrebarea se
  pune prin `TypeDescriptor.GetProperties(...).ShouldSerializeValue`, adică pe exact calea pe care
  o folosește Visual Studio ca să decidă dacă scrie linia. Testul a fost verificat că PICĂ dacă se
  scoate suprascrierea (redenumită temporar), ca să nu fie un test vid. Plus trei pe aceeași temă
  (§3.2): **un arbore neatins nu scrie NICIO linie** (nouă proprietăți verificate + cele două de
  derulare, ascunse), o alegere reală se serializează și se poate anula din «Reset», iar fontul
  implicit e explicit — `ResetFont()` e cel care redă moștenirea.
- `AdvancedTreeDesignerNodesTests` (6) — ierarhie după `ParentKey`; părinte declarat mai jos;
  `ParentKey` negăsit urcă la rădăcină; cheile de imagini se rezolvă din `NodeImages`; cheia de
  iconiță de antet se rezolvă în ambele ordini de scriere; **colecția goală nu atinge arborele
  umplut la rulare**.

**Trei defecte reale prinse de teste, nu de compilator:**

1. `Blend` arunca `OverflowException` — în VB, `Byte - Byte` rămâne `Byte`, deci orice amestec
   spre o culoare mai închisă (adică tot degradeul pe temă întunecată) exploda;
2. ordinea fraților ieșea inversată din `RebuildFromDefinitions` (parcurgere înapoi);
3. `SearchClearButtonPadding` nu ajungea niciodată la buton (setterul nu chema `ApplyClearButtonLook`).

Plus două găsite la recitire, pe aceeași temă (cine are voie să «fixeze» o culoare):
constructorul făcea `Me.BackColor = Color.White`, ceea ce marca suprafața drept «fixată de
operator» și ar fi ținut arborele alb pe tema întunecată (acum `MyBase.BackColor`); și lipsea
`ShouldSerializeBackColor`/`ForeColor`, deci Visual Studio ar fi putut îngheța o culoare din temă
în designerul gazdei (§3.1) — semnalat de operator, nu de teste, fiindcă niciun test nu întreba
încă ce serializează designerul. Acum întreabă.

**O regresie introdusă ȘI reparată în cursul feliei:** `KBot.Controls.Tests` n-avea
`DisableTestParallelization`. Testele construiesc controale WinForms reale, fiecare pe firul lui
STA; xUnit rulează implicit clasele în paralel; iar starea statică WinForms (dicționarul de
handle-uri din `NativeWindow`) nu e thread-safe — simptomul e un `IndexOutOfRangeException` din
`Dictionary.TryInsert`, aparent fără legătură cu testul care pică. În plus, un test de-al meu
chema `ThemeManager.SetScheme`, care difuzează schema peste `Application.OpenForms`, deci peste
formularele altor clase de test, de pe alt fir. Ambele reparate (`TestAssemblyInfo.vb` +
`ThemeManager.Apply` pe formularul propriu); `KBot.Theming.Tests` avea deja aceeași protecție,
din același motiv.

---

### 10. Verificat pe ecran

Operator, 08.08.2026: **«everything had screen time and it checks out»** — banda de căutare în
`MainForm`, randarea ei în designerul Visual Studio, antetul (font / aliniere / degrade /
iconițe), banda de căutare (padding, imagine pe ✕, ESC, cele două fonturi), colecțiile `Nodes` și
`NodeImages`, și playground-ul din banc.

Prima felie de control K-BOT cu verdict vizual dat, după 0010+ (`KBotDataView`), 0018/0025
(`KBotNavList`) și 0026, care toate au rămas «code green, nevăzut».

### 11. Rămas nefăcut / de urmărit

- **Un `Escape` pe arborele însuși** (nu în casetă) nu golește căutarea — ESC e legat pe
  `KeyDown`-ul casetei. Nimeni n-a cerut altceva; se notează ca să nu fie confundat cu un bug.
- **Cheile de noduri nu se validează** (nevide / unice). Deliberat, ca la
  `KBotNavItemCollection`: editorul de colecții inserează elementul în clipa în care apeși «Add»,
  cu mult înainte să fi tastat ceva. O cheie vidă primește un GUID la materializare.
- **Fontul arborelui nu moștenește fontul ambiant** (§3.2) — implicitul din constructor e o
  atribuire explicită, deci `ThemeManager.ApplyBaseFont` nu ajunge la el. Reparabil cu o linie
  ștearsă din constructor, dar se VEDE (înălțimea benzii de căutare, `RecalculateItemHeight`), așa
  că rămâne decizia operatorului.
- **`ComboBox` theming retrofit** rămâne amânat — CLAUDE.md îl lista împreună cu arborele; jumătatea
  «arbore» e făcută aici, jumătatea «ComboBox» nu.
- **Butoanele de sortare** din Istoric și din capul arborelui rămân `MsgBox`-uri (fir vechi, 0022).
- Nimic din felia asta n-a atins API-ul Python și nici nu s-a rulat pe o bază reală.

---

### 12. Adaos 08.08.2026 — export de setări din playground

Cerere de operator: playground-ul trebuie să scoată combinația probată pe ecran într-un format
aplicabil DIRECT pe un fișier de designer.

`TreeSettingsExporter` (`src\KBot.DevHarness\Internal\TreeSettingsExporter.vb`) traduce starea
runtime a arborelui în linii `Me.{TREE}.<Proprietate> = <literal VB>`, grupate pe categoriile din
grila de proprietăți, plus definițiile din `Nodes` și o secțiune de note pentru ce nu are literal
(imagini, `NodeImages`). Butonul «Exportă setările (linii de designer)» din panoul playground-ului
pune textul în clipboard ȘI îl scrie în `<AppDir>\Exports\tree-designer-<marcaj>.vb.txt`.

Selecția proprietăților trece prin `PropertyDescriptor.ShouldSerializeValue` — ADICĂ exact
mecanismul pe care designerul Visual Studio îl folosește el însuși. Consecința care contează:
o culoare lăsată «auto» (`Color.Empty`) NU pleacă în export, deși getterul ei întoarce culoarea
rezolvată din temă. Fără asta, exportul ar fi reintrodus chiar capcana pe care felia 0027 a
curățat-o — paleta de la momentul exportului înghețată într-un `.Designer.vb`. Din același motiv
`BackColor`/`ForeColor` sunt omise explicit: `ApplyTheme` le rescrie la rulare, deci la export
sunt mereu diferite de implicitul WinForms.

Etalon secundar: un `AdvancedTreeControl` proaspăt, pentru proprietățile fără `DefaultValue` și
fără `ShouldSerialize*` (`TreeFont`, `*IconSize`) — altfel s-ar fi scris mereu.

Acoperire: `tests\KBot.DevHarness.Tests\TreeSettingsExporterTests.vb` (8 teste) — arbore proaspăt
= export gol, culorile de temă rămân afară, enum/font/padding în forma designerului, imaginile ies
ca notă, nodurile cu doar membrii nevizi.

## SLICE-0027-02 — `AdvancedTreeControl`: un singur font, banda de subsol și strângerea <a id="slice-0027-02"></a>

Cerere de operator, fără plan prealabil:

> advancedtree:
> 0. has two properties: TreeFont and Font. Only one remains (Font). update everywhere.
> 1. a new section - footer. similar to header with:
>    a. height, backcolor, gradient
>    b. left icon
>    c. caption - lable with font, backcolor forecolor exposed
>    d. collapsable buttons (on/off) with:
>       I. size, picture, position (left,right). if left andalso lefticon, lefticon is ignored
>       II. minimum collapsed width (default 100px)
>       III. when collapsed, a swithc (show modern hover) which will do what the navlist control
>            does when it's collapsed and a button is hovered: present the node as if it was
>            extending from the collapsed tree without actually enlarging the tree control.
>       IV. exposed properties (like navlist) for the timers
> 2. in the playground form for the tree there are properties missing which need adding and also
>    there are old properties which need removing.

Sub-felie a lui 0027 (aceeași suprafață de control: proprietăți de designer + tematizare).

---

### 1. Un singur font

Arborele avea DOUĂ fonturi. `TreeFont` (implicit `Consolas, 9`) desena nodurile; `Font`
(implicit `Segoe UI, 9`, cu perechea `ShouldSerializeFont`/`ResetFont` adăugată în 0027) dădea
înălțimea rândului (`RecalculateItemHeight` citea `Me.Font.Height`), fontul benzii de căutare și
implicitul antetului. Două surse de adevăr pentru aceeași măsură — adică un arbore care putea
desena text mai mare decât rândul care-l ținea.

A rămas `Font`. Odată cu `TreeFont` au plecat și accesoriile ascunse `FontName`/`FontSize`, care
nu erau decât «mută TreeFont» scris pe bucăți.

Consecință de reținut: fontul IMPLICIT al nodurilor s-a mutat de la `Consolas, 9` la
`Segoe UI, 9`. Nicio vedere nu se schimbă la ecran — cele trei care chiar desenau noduri
(`Plati`/`Receptii`/`Rezervari`) își puneau explicit `FontName = "Segoe UI"` / `FontSize = 9`,
iar `DdfView.Designer.vb` avea `tree.TreeFont = New Font("Segoe UI", 9F)`. Toate patru au fost
ȘTERSE, nu traduse în `tree.Font = …`: valoarea e acum implicitul, iar scrisă explicit ar fi
FIXAT fontul (`_fontPinned`) și l-ar fi făcut surd la `ThemeManager.ApplyBaseFont`.

#### Defect prins pe drum: `ResetFont` nu reseta nimic

Asertul nou din `O_alegere_reala_se_serializeaza` (`ResetFont()` ⇒ `ShouldSerializeFont() = False`)
a picat pe cod NESCHIMBAT de mine. Cauza: `ResetFont` stingea steagul ÎNAINTE de `MyBase.ResetFont()`,
iar `Control.ResetFont` scrie `Font = Nothing` prin setterul VIRTUAL — al nostru — care îl aprindea
la loc. Reset-ul rămânea fără efect asupra serializării, adică exact capcana pe care 0027 o repara.
Ordinea e acum «resetează baza, apoi stinge steagul», la fel în `ResetBackColor`/`ResetForeColor`.

---

### 2. Banda de subsol

Fișier nou `AdvancedTreeControl.Footer.vb` (desen + geometrie + strângere + nodul plutitor) și
`TreeNodeFlyout.vb` (fereastra). Proprietățile stau în `.Properties`, categoria
**«K-BOT Arbore - Subsol»**, după tiparul antetului: `Color.Empty` = «din temă», pereche
`ShouldSerialize*`/`Reset*` pentru fiecare culoare/font/`Size`.

| Cerut | Proprietăți |
|---|---|
| a. înălțime, fundal, degrade | `FooterVisible`, `FooterHeight` (28), `FooterBackColor`, `FooterForeColor`, `FooterBackStyle`, `FooterGradientEndColor` |
| b. iconiță stânga (+ dreapta, §7) | `FooterLeftIcon`/`FooterRightIcon` + cheile lor (din `NodeImages`), `FooterIconSize`, evenimentul `FooterRightIconClicked` |
| c. caption cu font/culori | `FooterCaption`, `FooterCaptionFont`, `FooterCaptionForeColor`, `FooterCaptionBackColor`, `FooterTextAlign` |
| d. buton de strângere | `FooterCollapseButton`, `FooterCollapseButtonSize` (16), `FooterCollapseButtonPosition`, `FooterCollapseExpandedImage`, `FooterCollapseCollapsedImage` |
| d.II | `MinimumCollapsedWidth` (100) |
| d.III | `CollapsedFlyout` (True) |
| d.IV | `FlyoutDelay` (250), `FlyoutSlideDuration` (120) — aceleași nume ca la `KBotNavList` |
| stare + cârlige | `Collapsed` (nese­rializat), `ToggleCollapse()`, evenimentul `CollapsedChanged`, `FooterCollapseButtonRect` |

Reguli implementate cuvânt cu cuvânt din cerere:

- **butonul în stânga ignoră `FooterLeftIcon`** — nu se înghesuie amândouă la aceeași margine;
- fără pictograme pe buton se desenează unghiul (`‹` strâns / `›` desfășurat) din `FooterForeColor`,
  deci butonul e folosibil din prima;
- `FooterCaptionBackColor` gol înseamnă **fără plajă proprie** (se vede banda), nu «din temă» —
  singura culoare a arborelui cu sensul ăsta, și e comentat ca atare.

`AlignStartX`/`AlignStartY` au fost scoase în comun din `.Header` (erau `HeaderTextStartX/Y`), ca
`ContentAlignment` să aibă o singură interpretare pentru amândouă benzile.

#### Rezervarea de spațiu

Subsolul iese din zona de noduri, nu peste ea. Atinse: clip-ul și bucla din `OnPaint`,
`RefreshScrollVisibility` / `UpdateVScrollMaximum` (bara de derulare se oprește DEASUPRA benzii —
altfel săgeata ei de jos ar cădea peste buton), `OnMouseWheel`, `EnsureNodeVisible` și
`HitTestItem` (banda nu e zonă de noduri: acolo nu se selectează și nu se survolează nimic).

#### Strângerea

`Me.Width = MinimumCollapsedWidth`, cu lățimea desfășurată reținută la fiecare `OnResize` care NU
e al nostru (`_applyingCollapseExtent`) — exact mecanismul din `KBotNavList`, din același motiv:
altfel prima strângere ar deveni noua «lățime desfășurată» și arborele n-ar mai reveni. Setterul
`Collapsed` ARUNCĂ pe strângere fără buton (regula casei: niciun no-op tăcut); `ToggleCollapse`
nu aruncă — e apăsarea unui buton care oricum nu se desenează. Stingerea butonului cât arborele e
strâns îl desface, ca să nu rămână îngust pe veci.

Contractul cu gazda e cel de la `KBotNavList`: un arbore ancorat primește lățimea de la layout-ul
formularului, iar gazda își mută splitter-ul ascultând `CollapsedChanged`.

#### Nodul plutitor (d.III)

`TreeNodeFlyout` e sora lui `KBotNavFlyout`, cu aceleași trucuri: `WS_EX_NOACTIVATE` +
`ShowWithoutActivation`, `WS_EX_TOOLWINDOW`, `HTTRANSPARENT` pe `WM_NCHITTEST` (fereastra acoperă
chiar rândul survolat — fără asta hover-ul s-ar pierde, eticheta s-ar ascunde, hover-ul ar reveni…
la infinit) și colțuri tăiate din REGIUNE, nu doar din desen.

Ce o face să pară nodul care se desface, nu o etichetă lipită: pleacă exact din dreptunghiul
rândului strâns, crește DOAR spre dreapta, iar iconița și textul se desenează la ACELAȘI X ca în
arbore (`NodeTextStartX` / `NodeIconRect` repetă formula din `DrawContent`). Culorile vin din
aceleași reguli ca `DrawSelection` (selectat → `SelectedBackColor` + `SelectedBorderColor`, altfel
`HoverBackColor` + `LineColor`).

Decizia «pentru cine, cât de desfășurat, unde» stă în funcții pure, calculabile fără ecran —
`CollapsedFlyoutTargetAt`, `FlyoutClientBounds`, plus cârligele `DebugFlyout*`. Lățimea completă se
măsoară O DATĂ, la fixarea țintei; fără handle (banc de probă) măsurarea cade pe `TextRenderer`,
care nu cere fereastră, iar diferența GDI↔GDI+ e de câțiva px ÎN PLUS, adică joc, nu text tăiat.
Eticheta se retrage la derulare, la redimensionare, la desfășurare, la schimbarea temei și când
cursorul intră în bandă.

Caption-ul e desenat cu ACELEAȘI bucăți îmbogățite ca rândul (`ParseRichText`); separatorul `~~~`
devine spațiu, fiindcă eticheta e exact atât de lată cât îi trebuie și n-are zonă dreaptă rezervată.

#### Un defect de hit-test prins de teste

Prima variantă ținea dreptunghiul butonului într-un câmp scris de `DrawFooter`, deci apăsarea
depindea de o repictare anterioară — headless nu se apăsa niciodată. Acum și desenul, și hit-testul,
și testele citesc aceeași funcție pură (`ComputeFooterButtonRect`).

---

### 3. Playground (`TreePlaygroundForm`)

**Scos:** `lblTreeFont`/`numTreeFont` (proprietatea nu mai există).

**Adăugat:** `Font (nodurile)…` (o singură intrare de font), `LeftIconSize`, `RightIconSize`,
`HeaderIconSize`, secțiunea **Culori** (`BackColor`, `ForeColor`, `HoverBackColor`,
`SelectedBackColor`, `SelectedBorderColor`, `LineColor`, `BorderColor`), `TooltipBackColor`,
`TooltipForeColor`, `SearchBarLabelForeColor`, `SearchMode`, plus secțiunile **Subsol** și
**Subsol: strângere** cu toate proprietățile de mai sus și un buton `ToggleCollapse()`.

Arborele NU mai e docat `Fill`, ci `Left` într-un `pnlTreeHost` docat `Fill`: butonul de strângere
scrie `Width`, iar un control docat `Fill` n-are lățime proprie — altfel proba n-ar fi arătat nimic.
Banda de info arată acum și starea (`STRÂNS la 100px` / `desfășurat (420px)`).

Activările dependente urmăresc realitatea: subsolul ascuns stinge tot ce ține de el; butonul în
stânga stinge alegerea iconiței de stânga; cronometrele se sting fără nodul plutitor.

---

### Fișiere atinse

**Control (nou):** `src/KBot.Controls/Tree/AdvancedTreeControl.Footer.vb`,
`src/KBot.Controls/Tree/TreeNodeFlyout.vb`

**Control (modificat):** `AdvancedTreeControl.Properties.vb` (TreeFont/FontName/FontSize scoase;
regiunea de subsol), `.Painting.vb`, `.Header.vb`, `.Overrides.vb`, `.Keyboard.vb`,
`.DesignerNodes.vb`, `.Theming.vb` (`_autoFooter*`; ordinea din `Reset*`), `AdvancedTreeControl.vb`

**App:** `Views/PlatiView.vb`, `Views/ReceptiiView.vb`, `Views/RezervariView.vb`,
`Views/DdfView.vb`, `Views/DdfView.Designer.vb`

**Harness:** `Internal/TreePlaygroundForm.vb`, `Internal/TreePlaygroundForm.Designer.vb`,
`Internal/TreeSettingsExporter.vb` (doar comentariu — exportul e condus de metadate, deci a prins
proprietățile de subsol fără nicio modificare)

**Teste (nou):** `tests/KBot.Controls.Tests/AdvancedTreeFooterTests.vb` (15 teste)
**Teste (modificat):** `tests/KBot.Controls.Tests/AdvancedTreeThemingTests.vb`

**Versiuni:** `KBot.Controls` 1.9.0.0 → 1.10.0.0, `KBot.DevHarness` 1.0.11.0 → 1.0.12.0,
`KBot.App` 1.0.7.0 → 1.0.8.0

---

### Rezultate teste

`dotnet build KBot.sln` — 0 erori, 1 avertisment (`MSB3825` pe `DdfView.resx`, BinaryFormatter —
PREEXISTENT, neatins de felia asta).

`dotnet test KBot.sln`:

| Proiect | Rezultat |
|---|---|
| KBot.Controls.Tests | **426 / 426** (411 înainte + 15 noi) |
| KBot.DevHarness.Tests | 170 / 170 |
| KBot.Theming.Tests | 27 / 27 |
| KBot.Common.Tests | 14 / 14 |
| KBot.Xfa.Tests | 39 / 39 |
| KBot.LocalStore.Tests | 1 / 1 |
| KBot.Api.Tests | 67 / 68 — **1 pic PREEXISTENT** |
| KBot.App.Tests | 147 / 149 — **2 picuri PREEXISTENTE** |
| KBot.Domain.Tests | 14 / 17 — **3 picuri PREEXISTENTE** |

Cele 6 picuri sunt familia `EtichetaRevizie` (`DdfInfoTests` ×3, `ApiClientTests` ×1,
`DdfViewTests` ×2). **Verificate ca preexistente prin `git stash` + rulare pe arborele curat** —
aceleași 6, aceeași cauză, cu felia asta scoasă cu totul. Nu le-am atins: sunt în `KBot.Domain` /
`KBot.Api`, unde felia n-a modificat nicio linie.

---

### 4. Corectură după prima probă pe ecran: strângerea în `MainForm`

Raport de operator:

> «something is wrong with the mainform: it collapses the tree then imediately it expands it again
> so i never get to see it collapsed. in the playground is working»

Diagnostic — și playground-ul era indiciul, nu contra-exemplul. În `MainForm` arborele e
`Dock = Fill` în `pnlTree`, `pnlTree` e `Dock = Fill` în `split.Panel1`: lățimea lui o dă
`split.SplitterDistance`, nu el. `ApplyCollapseExtent` scria `Me.Width = 100`, iar layout-ul
formularului i-o dădea înapoi la următoarea trecere — de unde pâlpâirea. În playground arborele e
docat `Left` într-o gazdă `Fill` (schimbare făcută tocmai ca proba să arate ceva), deci acolo
`Width` chiar e al lui și totul funcționa.

Contractul era deja scris în worklog («gazda ascultă `CollapsedChanged` și mută splitter-ul»), dar
**nimeni nu-l implementa, iar controlul se bătea cu layout-ul în loc să tacă**. Ambele jumătăți au
fost reparate:

- **Control** — `HostOwnsWidth` (orice `Dock`, sau ancorare pe amândouă laturile) și
  `ExpandedWidth` (ultima lățime desfășurată, ca gazda să știe unde să se întoarcă). Când lățimea
  nu e a noastră, `ApplyCollapseExtent` NU mai scrie nimic: starea se schimbă, evenimentul pleacă,
  atât. O scriere acolo n-ar fi decât o pâlpâire.
- **`MainForm`** — `tree_CollapsedChanged` mută `split.SplitterDistance` la
  `MinimumCollapsedWidth + split.Panel1.Padding.Left` (padding-ul e CITIT, nu presupus).
  `Panel1MinSize` era 240, adică peste ținta de 111, deci păzea și comanda, nu doar tragerea:
  se coboară cât ține starea strânsă și se pune la loc la desfacere. Cât e strâns,
  `IsSplitterFixed = True` — tras, splitter-ul n-ar duce decât la o lățime care nu e nici starea
  strânsă, nici cea desfășurată. `ClampSplitter` ține distanța în intervalul acceptat de
  `SplitContainer`, fiindcă altfel o fereastră îngustă ar transforma apăsarea butonului în
  `InvalidOperationException`.

Două teste noi (428 în total): un arbore docat își schimbă starea și ridică evenimentul **fără**
să-și atingă `Width`, iar `HostOwnsWidth` urmărește `Dock`-ul și ancorarea pe ambele laturi.

---

### 5. A doua corectură de pe ecran: iconița din dreapta nu mai ține locul degeaba

Raport de operator:

> «showrighticononhover SHOULDN'T reserve the space by default. if it's disabled, then DON'T
> RESERVE THE SPACE! when hovered and the space is NOT reserved, the text of the node will become
> narrower to fit the icon»

`DrawContent` retrăgea marginea dreaptă a textului ori de câte ori nodul avea `RightIcon`, **fără
să se uite dacă iconița e sau nu pe ecran**. Comentariul de lângă îi spunea «Varianta A»: text care
nu sare la hover. Prețul, însă, era o fâșie goală pe FIECARE rând, pentru o iconiță pe care n-o
vede nimeni până la survolare — adică exact ce anulează rostul lui `ShowRightIconOnHover`.

Regula acum, într-un singur loc (`RightIconGutter`):

| iconiță | nesurvolat | survolat |
|---|---|---|
| permanentă | ia locul | ia locul |
| hover-only, `ReserveRightIconSpace = False` (implicit) | **nu ia nimic** | ia locul |
| hover-only, `ReserveRightIconSpace = True` | ia locul | ia locul |

`ReserveRightIconSpace` era deja `False` implicit, dar guverna doar banda de coloane; acum e
comutatorul care spune «ține locul fix» și pentru text. Descrierea din grila de proprietăți s-a
rescris ca atare.

**Banda de coloane rămâne DELIBERAT ne-condiționată de hover** (`ReservedRightIconWidth`): o
geometrie pe tot controlul care se re-așază la fiecare trecere a cursorului ar fi de nefolosit.
Doar textul de nod se îngustează. Cele două funcții stau una lângă alta, cu motivul scris între ele.

Prins alături, în zona de clic a iconiței (`OnMouseDown`): marginea era scrisă `6`, adică
implicitul lui `RightIconRightPadding` — deci zona de clic se despărțea de cea desenată de îndată
ce cineva atingea proprietatea. Acum se citește proprietatea.

În playground, `RightIconRightPadding` era activat doar când spațiul era rezervat; padding-ul
POZIȚIONEAZĂ iconița, deci contează oricând e desenată. Gating scos.

Șase teste noi (434 în total), în `AdvancedTreeRightIconTests`: tabelul de mai sus, nodul fără
iconiță, steagul per-nod care ridică hover-only doar pentru nodul lui, și banda de coloane care NU
se re-așază la hover. Survolarea se pune din test prin `DebugSetHoveredItem` — o regulă care nu se
poate proba decât cu un cursor real e o regulă neprobată.

---

### 6. A treia corectură de pe ecran: nodul selectat nu mai iese

Raport de operator:

> «when the tree is collapsed and there is a selected node, that node MUST NOT DO THE FLYOUT
> thing! Only for the selected row. should be done through an option: flyoutselectednode = boolean
> (exposed in the designer)»

Nodul selectat e singurul pe care operatorul îl are deja în minte — l-a ales el. O etichetă care
iese peste el nu-i spune nimic nou, dar acoperă vederea de alături exact în locul spre care se
uită, și o face de fiecare dată când cursorul trece pe deasupra.

`FlyoutSelectedNode` (Boolean, categoria «K-BOT Arbore - Subsol», **implicit False**) guvernează
asta. Suprimarea e pe UN rând — cel selectat — nu o stingere a etichetei: vecinii lui ies în
continuare, cum a și cerut («Only for the selected row»).

Trei căi, ca eticheta să nu rămână agățată:

- `CollapsedFlyoutTargetAt` sare nodul scutit, deci nu mai iese deloc;
- `EnsureCollapsedFlyoutStillAllowed` retrage eticheta DEJA ieșită când rândul de sub ea devine
  selectat — cazul obișnuit, «survolez, iese, dau clic»; chemată din `OnMouseDown`, din setterul
  `SelectedNode` (selecție din cod / din shell) și din setterul proprietății, ca stingerea
  opțiunii cât eticheta e afară s-o retragă pe loc;
- `RenderCollapsedFlyout` re-verifică la fiecare cadru, fiindcă selecția se poate muta din
  tastatură chiar cât desfășurarea e în curs.

Patru teste noi (438 în total): nodul selectat tace iar vecinul lui iese, `FlyoutSelectedNode = True`
o readuce, selectarea rândului survolat retrage eticheta deja ieșită, iar stingerea opțiunii la fel.
În playground, `chkFlyoutSelected`, activat doar când nodul plutitor e pornit.

---

### 7. Iconița din dreapta subsolului

Cerere de operator:

> «i also need a righticon in the footer similar to the right icon in the header. if collapse
> button is on the right, the right button is not displayed. the same if it is on the left»

`FooterRightIcon` + `FooterRightIconKey`, perechea celor din stânga, cu evenimentul
`FooterRightIconClicked(e As MouseEventArgs)` — sora lui `HeaderRightIconClicked`, aceeași
semnătură, ca gazda să nu aibă de învățat un al doilea tipar.

**Regula de capăt e acum SIMETRICĂ și scrisă o singură dată** (`ButonPeLatura`): latura pe care
stă butonul de strângere îi aparține, iar iconița de acolo nu se mai desenează. Până acum exista
doar jumătatea din stânga, cu regula împrăștiată în `DrawFooter`; s-a mutat în
`ShowFooterLeftIcon` / `ShowFooterRightIcon`, care spun ce e pe ecran ACUM și sunt publice (le
citește și playground-ul ca să-și stingă comutatoarele fără să reproducă regula).

Dreptunghiul iconiței vine din `ComputeFooterRightIconRect`, funcție pură folosită și de desen și
de hit-test — a doua oară în felia asta când se aplică lecția butonului: o geometrie publicată de
desen ar face apăsarea să depindă de o repictare anterioară. `FooterRightIconRect` o expune, ca
`FooterCollapseButtonRect`.

Iconița suprimată de buton nu e doar invizibilă, e și **inapăsabilă**: altfel ar fi rămas o zonă de
clic fantomă exact peste buton. Are test.

Patru teste noi (442 în total): tabelul de suprimare (buton dreapta / buton stânga / fără buton),
iconița stă în bandă și ridică evenimentul, zona de clic dispare odată cu ea, iar cele patru
proprietăți de iconițe nu se scriu în designer pe un arbore neatins. `HandleFooterMouseDown`
primește acum și `MouseEventArgs`, ca evenimentul să plece cu poziția clicului, exact ca la antet.

În playground: `chkFooterRightIcon`, comutatoarele de iconițe se sting după latura butonului, iar
`FooterRightIconClicked` se scrie în jurnal — proba că iconița chiar e apăsabilă.

---

### Rămas neverificat / amânat

- **Verdict vizual PARȚIAL.** Operatorul a rulat playground-ul și `MainForm` și a raportat
  strângerea ca fiind ruptă în shell (vezi §4, reparat) și funcțională în playground. Deci
  strângerea ARE timp de ecran; restul benzii — degradeul, unghiul desenat, alinierea caption-ului,
  culorile proprii ale etichetei — nu a fost confirmat bucată cu bucată.
- **Nimic din §4–§7 nu a fost revăzut pe ecran.** Mutarea splitter-ului,
  `IsSplitterFixed` cât e strâns, întoarcerea la distanța dinainte și îngustarea textului la hover
  sunt verzi la teste, dar nu re-probate nici în `MainForm`, nici în playground.
- **`RezervariView` cere explicit `ReserveRightIconSpace = True`** (`RezervariView.Designer.vb`),
  deci acolo textul rămâne cu locul rezervat permanent — neschimbat față de înainte. `PlatiView` și
  `ReceptiiView` îl au pe `False`, iar `MainForm` nu-l setează deloc: toate trei trec pe
  comportamentul nou. Dacă vreuna dintre ele arăta bine tocmai fiindcă locul era ținut, se va
  vedea abia la rulare.
- Nodul plutitor n-a fost probat cu o fereastră reală: `TreeNodeFlyout.OnPaint`, decupajul de
  colțuri și trecerea mouse-ului prin fereastră (`HTTRANSPARENT`) sunt copiate din `KBotNavFlyout`
  (care ARE verdict vizual din 0025-07), dar nu confirmate aici.
- Cât e strâns, `MainForm` fixează splitter-ul (`IsSplitterFixed`). E o decizie de GAZDĂ, nu a
  controlului — dacă operatorul o vrea altfel (splitter liber și în starea strânsă), se schimbă
  într-un singur loc, în `tree_CollapsedChanged`.
- `FooterLeftIconKey` se rezolvă pe ambele căi (`ResolveHeaderIcons` din XML/FOREXE și
  `ResolveHeaderIconsFromNodeImages` din designer), dar niciuna n-a fost rulată cu o cheie de
  subsol reală.
- Secțiunea **«K-BOT Arbore - Coloane»** (`TreeListView`/`DynamicColumns`/`ColumnsLevel`) rămâne
  în continuare NEACOPERITĂ de playground: coloanele se definesc din XML (`SetTreeListView`), nu
  din grila de proprietăți, deci comutatoarele n-ar avea ce arăta fără un constructor de coloane
  în bancul de probă. Fir deschis, nu scăpare.

## SLICE-0027-03 — the tree's designer values in the four views (Rezervări / Recepții / Plăți / DDF) <a id="slice-0027-03"></a>

Sub-slice of 0027 (`AdvancedTreeControl`). Operator request: «in all the views with
advancedtree, implement the designer values (like in mainform) and remove any code that is
setting the tree properties».

### What changed and why

**1. The look of the tree is now authored in the designer, in all four views.** Until now the
four views dropped a bare `AdvancedTreeControl` (dock, item height, icon sizes, right-text
width) and the header/footer bands simply did not exist there — they only existed in
`MainForm`, where 0027/0027-02 authored them. Each view's tree now carries the same band set
as `MainForm.tree`, with its own caption:

| view | `HeaderCaption` |
|---|---|
| `RezervariView` | `« REZERVĂRI»` |
| `ReceptiiView` | `« RECEPȚII»` |
| `PlatiView` | `« PLĂȚI»` |
| `DdfView` | `« REVIZII DDF»` |

Common to all four: header visible (40px, `Tahoma 9 Bold`, `GradientHorizontal` toward
`CornflowerBlue`, `folder_open` as the left icon, black caption — the same deliberate choice
`MainForm` makes), footer visible (40px) carrying ONLY the collapse button (left side,
`expand_24`/`collapse_24`), `MinimumCollapsedWidth = 120`, `RootExpander = True`.

**No end icons in the footer, and no header right icon.** `MainForm` has them because it has
handlers behind them (refresh / settings). A view has nothing to hang there yet, and an icon
that does nothing when clicked is exactly the silent no-op the house rules forbid.

**No `Font` line.** `DdfView.Designer.vb` carried `tree.Font = New Font("Segoe UI", 9F, …)` —
the tree's own default, written explicitly, which is the trap 0027-02 documented: it sets
`_fontPinned` and makes the tree deaf to `ThemeManager.ApplyBaseFont`. Deleted, not
translated. The other three never had it (0027-02 removed them there).

**2. `ConfigureTree()` is gone from all four.** It only ever set `RootExpander = True`, which
is now a designer line. `DdfView`'s copy was already commented out; the dead block was
removed rather than left as a comment.

**3. The collapse button needed its host half.** The tree is `Dock = Fill` inside
`split.Panel1` in every one of these views, so `HostOwnsWidth` is True and the control does
not write `Width` at all — it flips state and raises `CollapsedChanged`, and the HOST moves
its splitter (the 09.08.2026 correction in 0027-02). Without the handler the button would
have been a visible dead control in four views. Each view therefore gained
`tree_CollapsedChanged` + `ClampSplitter`, the same shape as `MainForm`: remember the
splitter distance, drop `Panel1MinSize` for the duration (a min-size guards the DRAG, and
collapsing is a command, not a drag), fix the splitter while collapsed, clamp the distance so
a narrow view cannot turn a button press into `InvalidOperationException`.

**4. `DdfView`'s two sample designer nodes were removed.** `TreeNodeDefinition1/2` («Ianuarie
~~~ 1.234.567,00», «01.01.2025 ~~~ 1.234.567,00») were left over from designer-surface
experiments. `RebuildFromDefinitions` materializes them at RUNTIME too, and `DdfView`'s
constructor only calls `ShowEmpty` (which does not clear the tree — `ClearAll` does), so the
running app would have shown two fake revisions until the first selection. The
`tree_image_list` binding (`NodeImages`) is kept — it is the designer's icon source, not a
node source.

### Files touched

- `src/KBot.App/Views/RezervariView.Designer.vb` — full tree band block
- `src/KBot.App/Views/ReceptiiView.Designer.vb` — idem
- `src/KBot.App/Views/PlatiView.Designer.vb` — idem
- `src/KBot.App/Views/DdfView.Designer.vb` — band block reworked (caption fixed from the
  copy-pasted «REZERVĂRI»), `Font` line dropped, the two sample node definitions removed
- `src/KBot.App/Views/RezervariView.vb`, `ReceptiiView.vb`, `PlatiView.vb`, `DdfView.vb` —
  `ConfigureTree()` removed, `tree_CollapsedChanged` + `ClampSplitter` + the two
  splitter-state fields added
- `src/KBot.App/KBot.App.vbproj` — `FileVersion` 1.0.9.0 → 1.0.10.0

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj` — **build succeeded, 0 errors**, 1 warning:
  the pre-existing `MSB3825` (BinaryFormatter) on `DdfView.resx`, from the operator's
  `tree_image_list` image stream. Not introduced here and not touched here.
- `dotnet test tests\KBot.App.Tests` — **148 passed / 1 failed / 149 total**. The failure is
  `DdfViewTests.Leaf_CaptionPadsRevisionNumberWithSpaces`, and it is **pre-existing on this
  branch**: `DdfInfo.EtichetaRevizie` returns `$"{data}"` with the `$"{numar} - {data}"` form
  commented out in the source (`src/KBot.Domain/DdfInfo.vb`, unmodified by this sub-slice —
  it is the same `EtichetaRevizie` family of failures 0027-02 recorded as pre-existing).

### Left unverified / deferred

- **Never seen on screen.** Not one of the four views has been rendered — this is the same
  standing gap as for the views themselves. The collapse behaviour is verified only by
  analogy with `MainForm`, where it WAS seen working after the 0027-02 correction.
- The four views' `split` designers do not set `Panel1MinSize` (so the default 25 already
  clears the 120px target); the min-size dance is carried anyway so a later designer change
  cannot silently veto the collapse. Untested against a non-default `Panel1MinSize` in a view.
- `MinimumCollapsedWidth = 120` and `HeaderCaption` wording are judgement calls, not operator
  choices — the captions in particular («REVIZII DDF») are open to a rename.
- `OrdonantareView` is deliberately out of scope: it is still a `PlaceholderView`.
