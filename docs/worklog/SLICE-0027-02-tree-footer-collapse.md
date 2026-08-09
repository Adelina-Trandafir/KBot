# SLICE-0027-02 — `AdvancedTreeControl`: un singur font, banda de subsol și strângerea

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

## 1. Un singur font

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

### Defect prins pe drum: `ResetFont` nu reseta nimic

Asertul nou din `O_alegere_reala_se_serializeaza` (`ResetFont()` ⇒ `ShouldSerializeFont() = False`)
a picat pe cod NESCHIMBAT de mine. Cauza: `ResetFont` stingea steagul ÎNAINTE de `MyBase.ResetFont()`,
iar `Control.ResetFont` scrie `Font = Nothing` prin setterul VIRTUAL — al nostru — care îl aprindea
la loc. Reset-ul rămânea fără efect asupra serializării, adică exact capcana pe care 0027 o repara.
Ordinea e acum «resetează baza, apoi stinge steagul», la fel în `ResetBackColor`/`ResetForeColor`.

---

## 2. Banda de subsol

Fișier nou `AdvancedTreeControl.Footer.vb` (desen + geometrie + strângere + nodul plutitor) și
`TreeNodeFlyout.vb` (fereastra). Proprietățile stau în `.Properties`, categoria
**«K-BOT Arbore - Subsol»**, după tiparul antetului: `Color.Empty` = «din temă», pereche
`ShouldSerialize*`/`Reset*` pentru fiecare culoare/font/`Size`.

| Cerut | Proprietăți |
|---|---|
| a. înălțime, fundal, degrade | `FooterVisible`, `FooterHeight` (28), `FooterBackColor`, `FooterForeColor`, `FooterBackStyle`, `FooterGradientEndColor` |
| b. iconiță stânga | `FooterLeftIcon`, `FooterLeftIconKey` (din `NodeImages`), `FooterIconSize` |
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

### Rezervarea de spațiu

Subsolul iese din zona de noduri, nu peste ea. Atinse: clip-ul și bucla din `OnPaint`,
`RefreshScrollVisibility` / `UpdateVScrollMaximum` (bara de derulare se oprește DEASUPRA benzii —
altfel săgeata ei de jos ar cădea peste buton), `OnMouseWheel`, `EnsureNodeVisible` și
`HitTestItem` (banda nu e zonă de noduri: acolo nu se selectează și nu se survolează nimic).

### Strângerea

`Me.Width = MinimumCollapsedWidth`, cu lățimea desfășurată reținută la fiecare `OnResize` care NU
e al nostru (`_applyingCollapseExtent`) — exact mecanismul din `KBotNavList`, din același motiv:
altfel prima strângere ar deveni noua «lățime desfășurată» și arborele n-ar mai reveni. Setterul
`Collapsed` ARUNCĂ pe strângere fără buton (regula casei: niciun no-op tăcut); `ToggleCollapse`
nu aruncă — e apăsarea unui buton care oricum nu se desenează. Stingerea butonului cât arborele e
strâns îl desface, ca să nu rămână îngust pe veci.

Contractul cu gazda e cel de la `KBotNavList`: un arbore ancorat primește lățimea de la layout-ul
formularului, iar gazda își mută splitter-ul ascultând `CollapsedChanged`.

### Nodul plutitor (d.III)

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

### Un defect de hit-test prins de teste

Prima variantă ținea dreptunghiul butonului într-un câmp scris de `DrawFooter`, deci apăsarea
depindea de o repictare anterioară — headless nu se apăsa niciodată. Acum și desenul, și hit-testul,
și testele citesc aceeași funcție pură (`ComputeFooterButtonRect`).

---

## 3. Playground (`TreePlaygroundForm`)

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

## Fișiere atinse

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

## Rezultate teste

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

## 4. Corectură după prima probă pe ecran: strângerea în `MainForm`

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

## 5. A doua corectură de pe ecran: iconița din dreapta nu mai ține locul degeaba

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

## Rămas neverificat / amânat

- **Verdict vizual PARȚIAL.** Operatorul a rulat playground-ul și `MainForm` și a raportat
  strângerea ca fiind ruptă în shell (vezi §4, reparat) și funcțională în playground. Deci
  strângerea ARE timp de ecran; restul benzii — degradeul, unghiul desenat, alinierea caption-ului,
  culorile proprii ale etichetei — nu a fost confirmat bucată cu bucată.
- **Corecturile din §4 și §5 nu au fost revăzute pe ecran.** Mutarea splitter-ului,
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
