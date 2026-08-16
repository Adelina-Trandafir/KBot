# SLICE-0035 — `KBotToolTip` (control nou) + scalarea la DPI a arborelui și a grilei + etichete în română

**Data:** 2026-08-15
**Stare:** cod verde (`dotnet build KBot.sln` — 0 erori, 5 avertismente `MSB3825` **preexistente**).
`KBot.Controls.Tests`: **831 verzi, 0 roșii** (suita EXISTENTĂ, rulată ca plasă de siguranță — ea a
și prins greșeala descrisă în §2b).
**Fără teste noi, fără commit** — cerute explicit așa.
⚠️ **Nicio suprafață n-a fost privită pe ecran.** Tot ce urmează e verificat de compilator, nu de ochi.

---

## 1. De ce

Trei cereri ale operatorului, care s-au dovedit a fi una singură plus două:

1. **O etichetă plutitoare proprie**, cu proprietăți vizibile în designer, cu antet (pictogramă +
   titlu, cu font, aliniere și fundal propriu), subsol, linie despărțitoare între secțiuni și corp
   cu text îmbogățit. Condiția tare: **două controale de pe același formular trebuie să poată avea
   etichete care arată diferit** — deci NU se extinde obiectul intern de tooltip al vreunui control.
2. **Aplicația scalează pe jumătate.** „În grilă, celulele, antetul și subsolul se fac mai mari, dar
   textul rămâne la fel." Și, mai revelator: *«la 100% arată prost, la 125% mai bine, la 150% și mai
   bine»*.
3. **Butoanele din antetele arborelui și ale grilei nu spun la ce folosesc**, iar formularele n-au
   etichete deloc.

---

## 2. Diagnosticul DPI — și de ce răspunsul la «facem aplicația surdă la scalare?» e NU

Un control desenat de noi are două feluri de măsuri:

- **literele** — un `Font` e în PUNCTE, deci la 150% același font se pictează cu 1,5× mai mulți
  pixeli. Se scalează **singur**, fără cod;
- **geometria** — `ItemHeight = 22`, `RowHeight = 28`, `HeaderHeight = 30`, `Indent`, `ExpanderSize`,
  mărimile de iconiță: erau în pixeli BRUȚI. Rămâneau 22, 28, 30 la orice scalare.

De aici, exact ce se vedea: la 100%, un rând de 28 px cu litere de ~12 px — mult aer degeaba; la
150%, aceleași 28 px cu litere de ~18 px — „arată mai bine", fiindcă textul a crescut până la măsura
rândului. La 175% ar fi început să nu mai încapă. Nu era o alegere de aspect: era **o măsură care
lipsea**.

**A face aplicația surdă la scalare (`HighDpiMode.DpiUnaware`) ar înrăutăți lucrurile:** Windows ar
randa totul la 96 dpi și ar întinde imaginea ca pe un bitmap. Proporțiile s-ar potrivi, dar TOT
textul ar fi neclar pe orice ecran peste 100% — inclusiv la 150%, unde operatorul lucrează. Se
păstrează `PerMonitorV2` (vezi `Program.Main`) și se repară măsurile.

### Leacul, care e cel al platformei

WinForms cheamă `Control.ScaleControl(factor, specified)` pe fiecare copil:

- la autoscalarea formularului (`AutoScaleMode.Font`, ce are fiecare formular K-BOT), și
- la fiecare schimbare de DPI, fiindcă aplicația e `PerMonitorV2`.

Un control obișnuit își scalează acolo `Bounds`/`Padding`. Fișierele noi
`AdvancedTreeControl.Dpi.vb` și `KBotDataView.Dpi.vb` suprascriu `ScaleControl` și scalează, în plus,
**măsurile noastre**. Nimic de chemat din gazdă, nimic de ținut minte.

### Perechea logic / scalat — și de ce nu se putea altfel

Fiecare măsură are acum două valori:

| | valoarea | cine o vede |
|---|---|---|
| `_itemHeightLogic`, `_rowHeightLogic`, … | ce a scris operatorul, px la 96 dpi | proprietatea publică, `.Designer.vb` |
| `_itemHeight`, `_rowHeight`, … | aceeași măsură scalată | pictarea, hit-testul, așezarea |

Dacă getter-ul ar întoarce valoarea scalată, designerul ar reciti 33 acolo unde s-a scris 22 și ar
îngheța 33 — iar la următoarea deschidere s-ar scala încă o dată. E **aceeași capcană** pe care o
descrie regula casei despre `ShouldSerialize` (valoarea rezolvată care se citește pentru totdeauna
ca alegere a operatorului), doar că pe numere în loc de culori.

**La design time nu se scalează nimic** (`KBotDesignTime.IsDesignTime`): suprafața Visual Studio
desenează la 96 dpi, deci acolo se vede chiar valoarea tastată.

**Răspunsul la «trebuie să proiectez la 100%?»: nu.** Designerul lucrează la 96 dpi indiferent de
scalarea ecranului fizic, iar valorile scrise acolo sunt logice prin definiție. Se poate proiecta
liniștit la 150%.

### Ce s-a atins efectiv

- **Arbore:** `ItemHeight`, `HeaderHeight`, `FooterHeight`, `ExpanderSize`, `Indent`, `CheckBoxSize`,
  `LeftIconSize`, `RightIconSize`, `MinimumCollapsedWidth`. Cele ~40 de locuri care citeau
  PROPRIETATEA pentru așezare au fost mutate pe CÂMP (proprietatea a rămas logică);
  `RecalculateItemHeight` (înălțime automată din `Font.Height`) produce o măsură deja scalată și își
  completează perechea logică prin `UnscaleY`.
- **Grilă:** `RowHeight`, `HeaderHeight`, `FooterHeight`, plus înălțimile benzilor de grup
  (`KBotGroupLevel.HeaderHeight`/`FooterHeight`, scalate la folosire în `GroupBandHeight`).
- **Lățimile de coloană** — vezi §2b, făcute într-o a doua trecere (0035-01).

---

## 2b. Lățimile de coloană (trecerea 0035-01)

Prima livrare le lăsase deoparte. Erau ultimul lucru nescalat din grilă, deci ultimul care mai
putea arăta urât la 150%: o coloană de 170 px rămânea 170 px, în timp ce textul din ea creștea cu
jumătate — se tăia cu elipsă exact acolo unde operatorul o făcuse destul de largă.

### Prima încercare, greșită — și ce a arătat

Am ținut întâi lățimea SCALATĂ în `_width` și am făcut `Width` să întoarcă lățimea CERUTĂ
(`_authoredWidth`). Compila, dar au căzut **29 de teste**. Ele aveau dreptate: `Width` însemna, de
la felia 0013 încoace, «lățimea de acum» — cea pe care o lasă în urmă o trecere de
auto-dimensionare, de umplere sau de strâmtare. Mutându-i înțelesul pe «lățimea cerută», o citire
de după o trecere ar fi întors altceva decât se pictează, iar asta **nu e o schimbare de DPI, e o
schimbare de API** — și una tăcută.

### Forma corectă: scalare LA FOLOSIRE, nu la stocare

Tot modelul de coloană rămâne în pixeli LOGICI — `Width`, `MinWidth`, `MaxWidth`,
`HeaderLeftIconSize`, `HeaderRightIconSize`, `ColumnFilterIconSize`. Niciun înțeles nu se schimbă,
niciun `.Designer.vb` nu se rescrie, cele 831 de teste trec neatinse.

Scalarea se face la citire, prin accesorii `Friend` noi: `WidthPx`, `EffectiveMinWidthPx`,
`MaxWidthPx`, `HeaderLeftIconSizePx`, `HeaderRightIconSizePx`, `ColumnFilterIconSizePx`,
`HeaderIconsWidthPx`. Grila citește `…Px` peste tot unde așază sau pictează (~40 de locuri în
`.AutoSize`, `.Layout`, `.Painting`, `.Input`, `.HeaderIcons`, `.FilterIcon`, `.Editing`,
`.Tooltip`, `.Grouping.Painting`, `.ButtonTips`).

Cele două scrieri care pornesc din pixeli de ECRAN se aduc înapoi în logic:

- `SetLayoutWidth(devicePx)` — trecerea de layout măsoară text real, deci lucrează în px de ecran;
  valoarea se stochează nescalată.
- **tragerea de margine** (`.Input`): reperul de pornire e `WidthPx`, iar rezultatul se scrie prin
  `UnscaleX`. Fără asta, o coloană trasă la 300 px pe ecran s-ar ține minte ca 300 *logici*, adică
  450 pe ecran la următoarea așezare — ar sări de sub cursor.

### O sursă de scară, nu două

Scara nu mai e factorul cumulat primit de la `ScaleControl`, ci **`DeviceDpi / 96`**;
`ScaleControl` (plus `OnHandleCreated` și `OnDpiChangedAfterParent`) rămâne doar declanșatorul.
Motivul e concret: `AutoScaleMode.Font` dă raportul înălțimilor de font (~1,45 la 150%, nu 1,5), în
timp ce toate constantele din pictură treceau deja prin `ScaleDpi`, care citește `DeviceDpi`. Cu
două surse, **podeaua de lățime și desenul ar fi ieșit din două formule** și coloana s-ar fi putut
strâmta cu câțiva pixeli sub ce se picta — chiar scăparea pe care o admitea, în scris, nota de DPI
din `KBotDataView.HeaderIcons`. Nota aceea e acum rescrisă: mărimile de pictogramă se scalează, iar
podeaua și desenul citesc același număr. Bonus: nu se mai acumulează rotunjiri dacă `ScaleControl`
e chemat de două ori. Arborele a fost aliniat la fel.

`OnHandleCreated` contează separat: până la handle, `DeviceDpi` întoarce 96 chiar și pe un ecran la
150%, deci un control construit din cod (nu din designer, deci fără autoscalare) ar fi rămas la
scara 1.

⚠️ **Rămâne de văzut:** drumul dus-întors device → logic → device poate pierde un pixel la
împărțirea surplusului, deci coloana de umplere ar putea lăsa o dungă de 1 px la marginea din
dreapta, la scalări ne-întregi. La 100% (scara 1) e exact — de aceea testele nu-l prind.

---

## 3. `KBotToolTip` — controlul nou (`src/KBot.Controls/ToolTip/`)

Patru fișiere, în folderul propriu al familiei, cum cere regula casei.

| Fișier | Ce e |
|---|---|
| `KBotRichText.vb` | motorul de text îmbogățit: `Parse` (`<b> <i> <u> <color=#…> <back=#…>`) → `Layout` (rupere explicită + la lățime, cu ruperea forțată a cuvintelor prea lungi) → `Draw`. **Pur**: intră text + font + culoare, ies segmente și dimensiuni; se poate măsura fără ecran |
| `KBotToolTipStyle.vb` | STILUL: `BackColor`, `ForeColor`, `BorderColor`, `BorderWidth`, `CornerRadius`, `Font`, `Padding`, `MaxWidth` + `Header`/`Footer` (`KBotToolTipBand`: `Visible`, `Text`, `Font`, `ForeColor`, `BackColor` — **transparent implicit**, `TextAlign`, `Icon`, `IconSize`, `IconGap`, `Padding`) + `Separator` (`KBotToolTipSeparator`: `Visible`, `ForeColor`, `Width`, `Inset`, `Margin`) |
| `KBotToolTipWindow.vb` | fereastra: `WS_EX_NOACTIVATE` + `ShowWithoutActivation`, `WS_EX_TOOLWINDOW`, `SetVisibleCore` ocolit prin `ShowWindow(SW_SHOWNOACTIVATE)`, `HTTRANSPARENT` pe `WM_NCHITTEST`. Așază pe secțiuni: antet, linie, corp, linie, subsol |
| `KBotToolTip.vb` | componenta `IExtenderProvider` + `KBotToolTipContent` |

### Cum se satisface „stiluri diferite pe același formular"

Stilul e o **valoare**, nu o componentă. `KBotToolTip` are un stil implicit (`Style`), iar orice
control poate primi **propriul** stil prin `SetStyleFor(ctrl, style)` — de obicei pornind de la
`Style.Clone()`. Se pot pune, la fel de bine, mai multe componente pe formular. Ambele căi merg;
niciuna nu cere modificarea obiectului intern de tooltip al vreunui control.

### Regulile casei, respectate

- `Color.Empty` / `Nothing` = «din schema activă», rezolvat la FIECARE afișare din
  `ThemeManager.Current` (deci comutarea de temă se vede fără cod în plus); orice culoare pusă
  explicit câștigă.
- Perechi `ShouldSerialize*`/`Reset*` pe tot ce nu poate purta `<DefaultValue>`: culori, fonturi,
  imagini, `Padding`, `Size`.
- Try/Catch cu `GlobalErrorLog.Write` la frontiere; `WndProc` lăsat pe plasa globală.
- Fereastra nu se instanțiază **niciodată** la design time.

### De ce nu se refolosește analizorul arborelui

`AdvancedTreeControl.ParseRichText` e `Friend Shared` pe control și legat de structurile lui
interne. O etichetă generală, folosibilă de orice formular, n-are voie să depindă de un control
anume. Tooltip-ul vechi al arborelui (`TooltipPopup`, cu modul lui de tabel XML) **rămâne neatins** —
nu s-a cerut regresie acolo.

---

## 4. Etichete pe butoanele desenate (arbore + grilă)

Butoanele din antete nu sunt controale, sunt zone pictate: `System.Windows.Forms.ToolTip` n-are ce
extinde. De aceea `KBotToolTip` are `ShowAt(owner, content, screenPos)` / `HideNow()`, chemate din
funcțiile care urmăreau deja survolarea.

**Arbore** (`AdvancedTreeControl.ButtonTips.vb`) — proprietăți multilinie (editor `MultilineStringEditor`):
`HeaderSearchIconTooltip`, `HeaderRightIconTooltip`, `FooterLeftIconTooltip`,
`FooterRightIconTooltip`, `CollapseButtonTooltip`, `ExpandButtonTooltip`, plus `ButtonTooltip`
(componenta, îmbrăcabilă din grila de proprietăți). Butonul de strângere are **două** texte, fiindcă
are două înțelesuri; un singur text ar minți jumătate din timp.
Legături: `UpdateHeaderButtonHover`, `HandleFooterMouseMove`, `ClearButtonHover`,
`HandleFooterMouseLeave`.

**Grilă** (`KBotDataView.ButtonTips.vb` + trei proprietăți noi pe `KBotDataColumn`):
`KBotDataColumn.HeaderTooltip` / `HeaderRightIconTooltip` / `FilterIconTooltip`, plus, pe grilă,
`FilterIconTooltip` (comună — filtrul face același lucru pe orice coloană), `CollapseButtonTooltip`,
`ExpandButtonTooltip` și `ButtonTooltip`. Prioritatea la survolare: **filtru > pictogramă > titlu**,
scrisă o singură dată în `RefreshHeaderTip`.
Legături: `UpdateHeaderIconHover`, `ClearHeaderIconHover`, `HandleFooterMouseMove`/`Leave` din
`.Collapse`.

O cheie de buton repetată nu reprogramează apariția — altfel fiecare pixel de mișcare peste același
buton ar reporni întârzierea și eticheta n-ar ieși niciodată.

---

## 5. Etichete în română pe formulare și vederi

O componentă `tips` (`KBotToolTip`) declarată în `.Designer.vb`, adăugată în `components` (deci
eliberată odată cu formularul), cu apelurile `SetToolTipHeader` / `SetToolTipText` scrise tot în
`InitializeComponent` — forma pe care o scrie și o reciteste designerul Visual Studio.

Acoperite: `LoginForm`, `MainForm`, `StartupLauncherForm`, `InternalInfoForm`, `ForexeConsoleForm`,
`ForexeFooterView`, `IstoricView`, `LogViewerForm`, `LogClearDialog`, `DdfDocumentPage`,
`ReaderHostPreview`, `XfaXmlPreview` — adică **toate formularele/vederile care au butoane, combo-uri,
casete de text sau bare de cipuri proprii**.

Arborii din `MainForm`, `DdfView`, `OrdView`, `PlatiView`, `ReceptiiView`, `RezervariView`,
`IstoricView` au primit textele de buton (strângere/desfacere, reîncărcare, căutare), iar grila din
`IstoricView` eticheta comună de filtrare.

**NEACOPERITE**, și de ce:
- vederile care n-au decât arbore + grilă (`SumarView`, `RezervariView`, `ReceptiiView`, `PlatiView`,
  `DdfValoriPage`, `OrdVizualizarePage`, `DdfFileBrowser`, `PlaceholderView`) n-au controale
  proprii de apăsat: butoanele lor sunt cele ale arborelui/grilei, deci se explică prin proprietățile
  de mai sus, pe coloană sau pe arbore;
- **`KBotNavList` n-are încă etichetă pe element.** Ar cere o proprietate `Tooltip` pe `KBotNavItem`
  și un fir de survolare în listă — aditiv, dar e o felie proprie, nu una de strecurat aici. Rândurile
  de navigare rămân, deocamdată, fără explicație la survolare;
- **`KBotCaptionBar`** (minimizează / maximizează / închide / temă) la fel: butoanele lui sunt
  desenate, iar textele lor sunt convenții de sistem. Se poate adăuga în aceeași felie viitoare.

---

## 6. Fișiere atinse

**Nou:**
`src/KBot.Controls/ToolTip/KBotRichText.vb`, `KBotToolTipStyle.vb`, `KBotToolTipWindow.vb`,
`KBotToolTip.vb`; `src/KBot.Controls/Tree/AdvancedTreeControl.Dpi.vb`, `.ButtonTips.vb`;
`src/KBot.Controls/DataView/KBotDataView.Dpi.vb`, `.ButtonTips.vb`.

**Modificat:** `AdvancedTreeControl.{vb,Properties,ButtonHover,Footer,Painting,Overrides,
NodeInspector,Keyboard,Search}.vb`; `KBotDataView.{vb,Layout,Grouping,Grouping.Painting,HeaderIcons,
FilterIcon,Collapse,AutoSize,Input,Painting,Editing,Tooltip,Theming,Designer}.vb`;
`KBotDataColumn.vb`; `KBotDataColumnCollection.vb`; cele 12 `.Designer.vb` din `KBot.App` de mai sus
+ arborii vederilor.

---

## 7. Deschis / de verificat pe ecran

1. **Nimic n-a fost văzut.** Prima privire trebuie să fie la 150% (scalarea operatorului) ȘI la 100%,
   pe aceeași fereastră.
2. **Dus-întorsul prin designerul Visual Studio** al celor 12 `.Designer.vb` — riscul standing
   0025/0027. Apelurile `SetToolTip*` sunt scrise în forma pe care o produce designerul, dar asta
   n-a fost confirmat prin deschidere.
3. **Coloana de umplere poate lăsa 1 px la marginea din dreapta** la scalări ne-întregi (§2b).
4. **Mutarea între monitoare cu scalări diferite** n-a fost exercitată — nici `ScaleControl`, nici
   `OnDpiChangedAfterParent`.
5. Fontul etichetei, nesetat, e cel al controlului care a cerut-o. Pe controalele cu font propriu
   (arborele din `MainForm` e pe Consolas 10) eticheta va scrie tot cu Consolas — de văzut dacă e ce
   se dorește sau dacă trebuie un font explicit în stil.
6. `KBotNavList` / `KBotCaptionBar` fără etichete (§5).
