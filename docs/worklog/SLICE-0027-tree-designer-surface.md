# SLICE-0027 — `AdvancedTreeControl`: banda de căutare, tematizare și suprafață de designer

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

## 1. De ce nu se vedea banda de căutare

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

## 2. Banda desenată la design-time

`DrawSearchBarPreview` desenează banda întreagă — eticheta cu fontul ei, caseta în
`SearchBoxBackColor`, placeholder-ul, butonul ✕ (glifă sau imagine) — urmând pas cu pas geometria
din `PositionSearchTextBox`, ca trecerea design-time → runtime să nu mute nimic.

În designer NU se creează controale copil reale: un `TextBox` viu pe suprafața de design fură
click-urile și apare ca element ne-selectabil. Detecția folosește `KBotDesignTime.IsDesignTime`
(ajutorul casei din felia 0025), nu `Control.DesignMode` — pe `net8.0-windows` designerul rulează
**în alt proces** (`DesignToolsServer.exe`), iar un control imbricat nici măcar nu e «sitat».

## 3. De ce culorile din designer nu se aplicau — două cauze, ambele structurale

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

### 3.1 `BackColor`/`ForeColor` — gaura care mai rămăsese

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

### 3.2 Restul zgomotului din designere

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

## 4. Antet

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

## 5. Banda de căutare

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

## 6. Colecțiile din designer

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

## 7. Bancul de probă

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

## 8. Fișiere atinse

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

## 9. Rezultatele testelor

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

## 10. Verificat pe ecran

Operator, 08.08.2026: **«everything had screen time and it checks out»** — banda de căutare în
`MainForm`, randarea ei în designerul Visual Studio, antetul (font / aliniere / degrade /
iconițe), banda de căutare (padding, imagine pe ✕, ESC, cele două fonturi), colecțiile `Nodes` și
`NodeImages`, și playground-ul din banc.

Prima felie de control K-BOT cu verdict vizual dat, după 0010+ (`KBotDataView`), 0018/0025
(`KBotNavList`) și 0026, care toate au rămas «code green, nevăzut».

## 11. Rămas nefăcut / de urmărit

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

## 12. Adaos 08.08.2026 — export de setări din playground

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
