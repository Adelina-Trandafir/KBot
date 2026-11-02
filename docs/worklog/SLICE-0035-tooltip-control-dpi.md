# SLICE-0035 — `KBotToolTip` (control nou) + scalarea la DPI a arborelui și a grilei + etichete în română

**Data:** 2026-08-15 **Stare:** cod verde (`dotnet build KBot.sln` — 0 erori, 5 avertismente `MSB3825` **preexistente**). `KBot.Controls.Tests`: **831 verzi, 0 roșii** (suita EXISTENTĂ, rulată ca plasă de siguranță — ea a și prins greșeala descrisă în §2b). **Fără teste noi, fără commit** — cerute explicit așa. ⚠️ **Nicio suprafață n-a fost privită pe ecran.** Tot ce urmează e verificat de compilator, nu de ochi.

Reper ulterior: [0036](SLICE-0036-theme-options-scaling.md#slice-0036) centralizează factorul în AppScaling și oferă DpiUnaware ca opțiune explicită, cu costurile consemnate. Descrierea DeviceDpi / 96 de mai jos rămâne etapa inițială. [0040](SLICE-0040-tree-scaling-sweep.md) extinde folosirea măsurilor scalate în arbore.

---

## 1. De ce

Trei cereri ale operatorului, care s-au dovedit a fi una singură plus două:

1. **Tooltip propriu**, configurabil în designer: antet cu pictogramă/titlu, font/aliniere/fundal proprii, corp îmbogățit, subsol și separatoare (§3). Două controale ale aceluiași formular trebuie să poată arăta diferit; fără extinderea tooltipului intern al unui control.
2. **Aplicația scalează pe jumătate.** „În grilă, celulele, antetul și subsolul se fac mai mari, dar textul rămâne la fel." Și, mai revelator: *«la 100% arată prost, la 125% mai bine, la 150% și mai bine»*.
3. **Butoanele din antetele arborelui și ale grilei nu spun la ce folosesc**, iar formularele n-au etichete deloc.

---

## 2. Diagnosticul DPI — și de ce răspunsul la «facem aplicația surdă la scalare?» e NU

Un control desenat de noi are două feluri de măsuri:

- **literele** — un `Font` e în PUNCTE, deci la 150% același font se pictează cu 1,5× mai mulți pixeli. Se scalează **singur**, fără cod;
- **geometria** — `ItemHeight = 22`, `RowHeight = 28`, `HeaderHeight = 30`, `Indent`, `ExpanderSize`, mărimile de iconiță: erau în pixeli BRUȚI. Rămâneau 22, 28, 30 la orice scalare.

La 100%, rândul de 28 px cu litere ~12 px avea mult aer; la 150%, tot 28 px cu litere ~18 px părea mai bun. La 175% textul ar fi început să nu încapă. Lipsea scalarea geometriei, nu o alegere de aspect.

**HighDpiMode.DpiUnaware ar agrava:** Windows ar randa la 96 dpi și ar întinde bitmapul; proporțiile s-ar potrivi, dar textul ar deveni neclar peste 100%, inclusiv la 150% unde lucrează operatorul. Se păstrează PerMonitorV2 (Program.Main) și se corectează geometria.

### Leacul, care e cel al platformei

WinForms cheamă `Control.ScaleControl(factor, specified)` pe fiecare copil:

- la autoscalarea formularului (`AutoScaleMode.Font`, ce are fiecare formular K-BOT), și
- la fiecare schimbare de DPI, fiindcă aplicația e `PerMonitorV2`.

ScaleControl scalează Bounds/Padding în controalele obișnuite; noile AdvancedTreeControl.Dpi.vb/KBotDataView.Dpi.vb îl suprascriu și pentru măsurile proprii. Fără apeluri suplimentare din gazdă.

### Perechea logic / scalat — și de ce nu se putea altfel

Fiecare măsură are acum două valori:

| | valoarea | cine o vede |
|---|---|---|
| `_itemHeightLogic`, `_rowHeightLogic`, … | ce a scris operatorul, px la 96 dpi | proprietatea publică, `.Designer.vb` |
| `_itemHeight`, `_rowHeight`, … | aceeași măsură scalată | pictarea, hit-testul, așezarea |

Getterul scalat ar serializa 33 în loc de 22 logici, apoi ar scala din nou la redeschidere: capcana ShouldSerialize, valoare rezolvată confundată cu alegerea operatorului, aplicată numerelor.

**Design time:** KBotDesignTime.IsDesignTime oprește scalarea; suprafața VS lucrează la 96 dpi și arată valorile logice tastate, indiferent de scara monitorului. La «trebuie proiectat la 100%?» răspunsul este nu: se poate proiecta și la 150%.

### Ce s-a atins efectiv

- **Arbore:** `ItemHeight`, `HeaderHeight`, `FooterHeight`, `ExpanderSize`, `Indent`, `CheckBoxSize`, `LeftIconSize`, `RightIconSize`, `MinimumCollapsedWidth`. Cele ~40 de locuri care citeau PROPRIETATEA pentru așezare au fost mutate pe CÂMP (proprietatea a rămas logică); `RecalculateItemHeight` (înălțime automată din `Font.Height`) produce o măsură deja scalată și își completează perechea logică prin `UnscaleY`.
- **Grilă:** `RowHeight`, `HeaderHeight`, `FooterHeight`, plus înălțimile benzilor de grup (`KBotGroupLevel.HeaderHeight`/`FooterHeight`, scalate la folosire în `GroupBandHeight`).
- **Lățimile de coloană** — vezi §2b, făcute într-o a doua trecere (0035-01).

---

## 2b. Lățimile de coloană (trecerea 0035-01)

Prima livrare nu scalase lățimile: coloana de 170 px rămânea la fel, iar textul creștea cu 50% și ajungea tăiat cu elipsă.

### Prima încercare, greșită — și ce a arătat

Am ținut întâi lățimea SCALATĂ în `_width` și am făcut `Width` să întoarcă lățimea CERUTĂ (`_authoredWidth`). Compila, dar au căzut **29 de teste**. Ele aveau dreptate: `Width` însemna, de la felia 0013 încoace, «lățimea de acum» — cea pe care o lasă în urmă o trecere de auto-dimensionare, de umplere sau de strâmtare. Mutându-i înțelesul pe «lățimea cerută», o citire de după o trecere ar fi întors altceva decât se pictează, iar asta **nu e o schimbare de DPI, e o schimbare de API** — și una tăcută.

### Forma corectă: scalare LA FOLOSIRE, nu la stocare

Tot modelul de coloană rămâne în pixeli LOGICI — `Width`, `MinWidth`, `MaxWidth`, `HeaderLeftIconSize`, `HeaderRightIconSize`, `ColumnFilterIconSize`. Niciun înțeles nu se schimbă, niciun `.Designer.vb` nu se rescrie, cele 831 de teste trec neatinse.

Scalarea se face la citire, prin accesorii `Friend` noi: `WidthPx`, `EffectiveMinWidthPx`, `MaxWidthPx`, `HeaderLeftIconSizePx`, `HeaderRightIconSizePx`, `ColumnFilterIconSizePx`, `HeaderIconsWidthPx`. Grila citește `…Px` peste tot unde așază sau pictează (~40 de locuri în `.AutoSize`, `.Layout`, `.Painting`, `.Input`, `.HeaderIcons`, `.FilterIcon`, `.Editing`, `.Tooltip`, `.Grouping.Painting`, `.ButtonTips`).

Cele două scrieri care pornesc din pixeli de ECRAN se aduc înapoi în logic:

- `SetLayoutWidth(devicePx)` — trecerea de layout măsoară text real, deci lucrează în px de ecran; valoarea se stochează nescalată.
- **tragerea de margine** (`.Input`): reperul de pornire e `WidthPx`, iar rezultatul se scrie prin `UnscaleX`. Fără asta, o coloană trasă la 300 px pe ecran s-ar ține minte ca 300 *logici*, adică 450 pe ecran la următoarea așezare — ar sări de sub cursor.

### O sursă de scară, nu două

Scara devine **`DeviceDpi / 96`**; `ScaleControl`, `OnHandleCreated` și `OnDpiChangedAfterParent` sunt numai declanșatoare. `AutoScaleMode.Font` produce ~1,45 la 150%, față de 1,5 din `ScaleDpi`: două surse ar permite lățimi sub desen. Podeaua și pictarea folosesc acum aceeași valoare; nota `KBotDataView.HeaderIcons` este corectată pentru scalarea pictogramelor. Nu se mai acumulează rotunjiri la două apeluri `ScaleControl`; arborele este aliniat la fel.

`OnHandleCreated` contează separat: până la handle, `DeviceDpi` întoarce 96 chiar și pe un ecran la 150%, deci un control construit din cod (nu din designer, deci fără autoscalare) ar fi rămas la scara 1.

⚠️ **Rămâne de văzut:** drumul dus-întors device → logic → device poate pierde un pixel la împărțirea surplusului, deci coloana de umplere ar putea lăsa o dungă de 1 px la marginea din dreapta, la scalări ne-întregi. La 100% (scara 1) e exact — de aceea testele nu-l prind.

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

Stilul este valoare, nu componentă: KBotToolTip.Style implicit, SetStyleFor(ctrl,style) per control, uzual din Style.Clone(). Alternativ, mai multe componente pe formular. Ambele permit stiluri distincte fără modificarea tooltipului intern al vreunui control.

### Regulile casei, respectate

- `Color.Empty` / `Nothing` = «din schema activă», rezolvat la FIECARE afișare din `ThemeManager.Current` (deci comutarea de temă se vede fără cod în plus); orice culoare pusă explicit câștigă.
- Perechi `ShouldSerialize*`/`Reset*` pe tot ce nu poate purta `<DefaultValue>`: culori, fonturi, imagini, `Padding`, `Size`.
- Try/Catch cu `GlobalErrorLog.Write` la frontiere; `WndProc` lăsat pe plasa globală.
- Fereastra nu se instanțiază **niciodată** la design time.

### De ce nu se refolosește analizorul arborelui

AdvancedTreeControl.ParseRichText este Friend Shared, legat de structurile arborelui; tooltipul general nu trebuie să depindă de el. Vechiul TooltipPopup/tabel XML rămâne neatins, fără regresie cerută.

---

## 4. Etichete pe butoanele desenate (arbore + grilă)

Butoanele de antet sunt zone pictate, fără control de extins prin System.Windows.Forms.ToolTip. Urmărirea existentă a survolării apelează KBotToolTip.ShowAt(owner,content,screenPos)/HideNow().

**Arbore** (`AdvancedTreeControl.ButtonTips.vb`) — proprietăți multilinie (editor `MultilineStringEditor`): `HeaderSearchIconTooltip`, `HeaderRightIconTooltip`, `FooterLeftIconTooltip`, `FooterRightIconTooltip`, `CollapseButtonTooltip`, `ExpandButtonTooltip`, plus `ButtonTooltip` (componenta, îmbrăcabilă din grila de proprietăți). Butonul de strângere are **două** texte, fiindcă are două înțelesuri; un singur text ar minți jumătate din timp. Legături: `UpdateHeaderButtonHover`, `HandleFooterMouseMove`, `ClearButtonHover`, `HandleFooterMouseLeave`.

**Grilă** (`KBotDataView.ButtonTips.vb` + trei proprietăți noi pe `KBotDataColumn`): `KBotDataColumn.HeaderTooltip` / `HeaderRightIconTooltip` / `FilterIconTooltip`, plus, pe grilă, `FilterIconTooltip` (comună — filtrul face același lucru pe orice coloană), `CollapseButtonTooltip`, `ExpandButtonTooltip` și `ButtonTooltip`. Prioritatea la survolare: **filtru > pictogramă > titlu**, scrisă o singură dată în `RefreshHeaderTip`. Legături: `UpdateHeaderIconHover`, `ClearHeaderIconHover`, `HandleFooterMouseMove`/`Leave` din `.Collapse`.

O cheie de buton repetată nu reprogramează apariția — altfel fiecare pixel de mișcare peste același buton ar reporni întârzierea și eticheta n-ar ieși niciodată.

---

## 5. Etichete în română pe formulare și vederi

Componenta tips/KBotToolTip: declarată în .Designer.vb și inclusă în components pentru eliberare; SetToolTipHeader/SetToolTipText în InitializeComponent, în forma citită/scrisă de designerul VS.

Acoperite: `LoginForm`, `MainForm`, `StartupLauncherForm`, `InternalInfoForm`, `ForexeConsoleForm`, `ForexeFooterView`, `IstoricView`, `LogViewerForm`, `LogClearDialog`, `DdfDocumentPage`, `ReaderHostPreview`, `XfaXmlPreview` — adică **toate formularele/vederile care au butoane, combo-uri, casete de text sau bare de cipuri proprii**.

Arborii din `MainForm`, `DdfView`, `OrdView`, `PlatiView`, `ReceptiiView`, `RezervariView`, `IstoricView` au primit textele de buton (strângere/desfacere, reîncărcare, căutare), iar grila din `IstoricView` eticheta comună de filtrare.

**NEACOPERITE**, și de ce:
- vederile care n-au decât arbore + grilă (`SumarView`, `RezervariView`, `ReceptiiView`, `PlatiView`, `DdfValoriPage`, `OrdVizualizarePage`, `DdfFileBrowser`, `PlaceholderView`) n-au controale proprii de apăsat: butoanele lor sunt cele ale arborelui/grilei, deci se explică prin proprietățile de mai sus, pe coloană sau pe arbore;
- **`KBotNavList` n-are încă etichetă pe element.** Ar cere o proprietate `Tooltip` pe `KBotNavItem` și un fir de survolare în listă — aditiv, dar e o felie proprie, nu una de strecurat aici. Rândurile de navigare rămân, deocamdată, fără explicație la survolare;
- **`KBotCaptionBar`** (minimizează / maximizează / închide / temă) la fel: butoanele lui sunt desenate, iar textele lor sunt convenții de sistem. Se poate adăuga în aceeași felie viitoare.

---

## 6. Fișiere atinse

**Nou:** `src/KBot.Controls/ToolTip/KBotRichText.vb`, `KBotToolTipStyle.vb`, `KBotToolTipWindow.vb`, `KBotToolTip.vb`; `src/KBot.Controls/Tree/AdvancedTreeControl.Dpi.vb`, `.ButtonTips.vb`; `src/KBot.Controls/DataView/KBotDataView.Dpi.vb`, `.ButtonTips.vb`.

**Modificat:** `AdvancedTreeControl.{vb,Properties,ButtonHover,Footer,Painting,Overrides, NodeInspector,Keyboard,Search}.vb`; `KBotDataView.{vb,Layout,Grouping,Grouping.Painting,HeaderIcons, FilterIcon,Collapse,AutoSize,Input,Painting,Editing,Tooltip,Theming,Designer}.vb`; `KBotDataColumn.vb`; `KBotDataColumnCollection.vb`; cele 12 `.Designer.vb` din `KBot.App` de mai sus
+ arborii vederilor.

---

## 7. Deschis / de verificat pe ecran

1. **Nimic n-a fost văzut.** Prima privire trebuie să fie la 150% (scalarea operatorului) ȘI la 100%, pe aceeași fereastră.
2. **Dus-întorsul prin designerul Visual Studio** al celor 12 `.Designer.vb` — riscul standing 0025/0027. Apelurile `SetToolTip*` sunt scrise în forma pe care o produce designerul, dar asta n-a fost confirmat prin deschidere.
3. **Coloana de umplere poate lăsa 1 px la marginea din dreapta** la scalări ne-întregi (§2b).
4. **Mutarea între monitoare cu scalări diferite** n-a fost exercitată — nici `ScaleControl`, nici `OnDpiChangedAfterParent`.
5. Fontul etichetei, nesetat, e cel al controlului care a cerut-o. Pe controalele cu font propriu (arborele din `MainForm` e pe Consolas 10) eticheta va scrie tot cu Consolas — de văzut dacă e ce se dorește sau dacă trebuie un font explicit în stil.
6. `KBotNavList` / `KBotCaptionBar` fără etichete (§5).

Repere unice din registru: componenta oferă fiecărui control text/titlu/subsol pentru etichetă; separatorul se desenează numai între două secțiuni vizibile.
