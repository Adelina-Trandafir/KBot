# SLICE-0000-23…34 — Istoric tururi, capturi și fereastră de ajutor

Șapte etape tematice sunt reunite integral, fără a inventa etapele dintre ele. Ordinea explică diferențele: 24 schimbă bula din 23; 27 înlocuiește lista tururilor din toate ferestrele cu numai fereastra al cărei «?» este apăsat; 30 arată temporar elementele ascunse, iar 31 rezolvă deschiderea meniului și completează ajutorul cozii; 32 schimbă documentul, tipărirea/exportul și ipoteza înghețării; 34 adaugă capturile de refăcut și textul pentru descărcări multiple. Limitările și refuzurile de categorii de estompare din 23 sunt istorice și nu înlocuiesc instrucțiunile actuale ale proiectului pentru capturi. Cauzele citite din captură/cod nu devin cauze măsurate prin reunire. Întreținerea curentă este în [HELP_SYSTEM](../HELP_SYSTEM.md).

[0000-23](#slice-0000-23) · [0000-24](#slice-0000-24) · [0000-27](#slice-0000-27) · [0000-30](#slice-0000-30) · [0000-31](#slice-0000-31) · [0000-32](#slice-0000-32) · [0000-34](#slice-0000-34)

<a id="slice-0000-23"></a>

## SLICE-0000-23 — Tururi cu vârf și pe butoane, fereastra de ajutor (istoric, mărime text, deasupra), capturi estompate

Operator, 30.09.2026 (felia 0000, ajutorul):

1. turul ghidat folosește o bulă de tip «callout», cu vârful pe obiectul descris;
2. la fiecare arbore, tabel, bară de titlu și bară de vederi, turul trece prin FIECARE element al
   controlului (ex. lista de angajamente: întâi lista, apoi butonul de reîmprospătare — aprins
   pentru exemplu —, apoi fiecare buton din cap și din subsol);
3. fereastra de ajutor are istoric pe sesiune (înapoi / înainte) și o opțiune de mărime a textului;
4. fereastra de ajutor se deschide mereu deasupra și se închide singură când se deschide o
   fereastră modală, ca să nu devină inaccesibilă;
5. (mesaj ulterior) unealta de capturi detectează singură datele sensibile și le estompează:
   utilizatorul, numele unității, orice număr care arată ca un cont (începe cu RO), CNP (13 cifre
   la rând). «Poți extinde, dar nu fără să întrebi.»
6. (mesaj ulterior) estomparea NU ia toată bara de titlu sau tot meniul — doar textul.

### What changed and why

#### 1. Callout (`HelpTourBubble`)
- Bula are acum un triunghi pe latura dinspre țintă, cu vârful pe marginea exterioară a inelului
  (`HelpTourFrame.Outset`). Fereastra crește cu adâncimea triunghiului pe latura aceea, `Padding`
  îi dă corpului înapoi fâșia, iar `Region` = dreptunghiul corpului ∪ triunghiul. Culoarea de accent
  care desenează conturul de 1 px umple și triunghiul.
- Așezare: dreapta, stânga, dedesubt, deasupra — prima latură unde încape TOATĂ bula; o țintă prea
  mare pentru orice latură (o vedere întreagă) primește bula înăuntru, cu vârful în sus spre
  marginea ei. Vârful alunecă pe latură până la colțuri când bula e împinsă de marginea ecranului.
- Designer: `BorderlessShadow = False` (o fereastră cu `Region` nu mai primește umbra DWM),
  `AutoFitToTheme = False` (bula își face singură mărimea, `FitToText`).

#### 2. Părțile controalelor (`IKBotHelpParts`)
- Interfață nouă în `KBot.Theming\KBotHelp.vb`: `HelpPartBounds(part)` (dreptunghi client; Empty =
  partea există dar nu e pe ecran acum; nume necunoscut = `ArgumentException`) și
  `SetHelpPartDemo(part, show)`.
- Implementări (`*.HelpParts.vb`):
  - `AdvancedTreeControl`: `header`, `header.search`, `header.right`, `columns`, `node.icon`,
    `footer`, `footer.left`, `footer.right`, `footer.collapse`. `node.icon` cu demo: iconița de la
    capătul rândului (cea care la lista de angajamente apare doar sub mouse) se aprinde pe rândul
    selectat, altfel pe primul rând de pe ecran care o are (`_helpDemoItem`, citit de
    `DrawRightIcon` și `RightIconGutter`; golit la `Clear`).
  - `KBotDataView`: `header`, `header.filter`, `rows`, `footer`, `footer.left`, `footer.right`,
    `footer.collapse`.
  - `KBotCaptionBar`: `icon`, `title`, `unit`, `options`, `theme`, `help`, `minimize`, `maximize`,
    `close` (+ `IconRect()` / `TitleLeft()` scoase din `OnPaint`, o singură formulă).
  - `KBotNavList`: `item:<Key>`, `collapse`.
- Tururi: cheie nouă de pas `part:` (`HelpTour.Parse`). `HelpTourRunner`: cere demo-ul, inelul și
  vârful merg pe parte; demo-ul se stinge la schimbarea pasului și la sfârșit; **o parte care nu e
  pe ecran se sare** în sensul mersului (vederea n-are butonul, lista e goală, pagina e ascunsă) —
  la capăt, turul se încheie; o parte greșită în fișier = jurnal + tot controlul + notă.
- `Check-Help.ps1`: verifică fiecare `part:` după tipul controlului (citit din declarația lui) și
  fiecare `item:` după cheile din designer.

#### 3. Conținutul tururilor (17 tururi, ~60 de pași noi)
Convenția: un pas pentru controlul întreg, apoi câte unul pe parte, titlurile «Lista › Lupa»,
«Tabel › TOTALURI»... Textele sunt luate din tooltipurile din designer și din subiectele existente
(verificate în trecerile 0000-13…0000-15):
- `tur-fereastra`: lista (lupa, rotița, reîmprospătarea unui angajament — aprinsă pentru exemplu,
  extrase, actualizează), fiecare vedere din bară (10) + butonul de strângere, bara de titlu
  (unitatea, setări, temă, «?», minimizează, mărește, închide).
- `tur-rezervari`, `tur-receptii`, `tur-plati`, `tur-istoric`, `tur-sumar`, `tur-ddf`, `tur-ord`,
  `tur-notecab`, `tur-extrase`, `tur-clasificatii`, `tur-parteneri`: butoanele arborelui și ale
  tabelului (lupa, capul/subsolul, «+», strângerea, pâlnia, TOTALURI) și paginile (`navSub`).
- `tur-asocieri`: «Grafic» / «Distribuție»; `tur-setari`, `tur-avansat`: fiecare pagină arătată
  pe rândul ei din listă (+ paginile noi în tur: «Informații», «Extrase», filele «Aplicație»).
- `tur-forexe`: «Descărcarea unui angajament» arată acum chiar iconița rândului.
- În trecere: `contabil.vederi.rezervari` primește «Reanalizează rezervările» (meniul din stânga,
  jos) — închide rândul din «Ajutor de actualizat».

#### 4. Fereastra de ajutor
- **Istoric pe sesiune**: `HelpHistory` (Înapoi / Înainte / pagina curentă / paginile văzute, max
  25), ținut de `HelpService` — supraviețuiește închiderii ferestrei; nimic pe disc. Buton nou
  «Istoric ▾» (`KBotDropDownMenu`): paginile, cea mai nouă sus, cea de pe ecran marcată «pe ecran».
- **Mărimea textului**: «A−» / «A+»; `AppSettings.HelpTextPercent` (80, 90, 100, 115, 130, 150, 175,
  200 %), peste mărimea textului K-BOT; salvată. Pagina de pe ecran se schimbă pe loc (stilul
  `body`), deci derularea rămâne.
- **Mereu deasupra**: `TopMost = True`. **Se închide singură** când modalul altei ferestre o
  dezactivează: `WM_ENABLE(false)` → verificare după 150 ms (dialogul nu există încă la
  `WM_ENABLE`); rămâne deschisă dacă modalul e al ei (lanțul de proprietari:
  «Salvează manualul», unealta de capturi) sau dacă e minimizată. `HelpService.EnsureWindow` vede
  acum și dezactivarea făcută de Windows (`IsWindowEnabled`), nu doar `Control.Enabled`.
- `HelpService.StepAside()`: fereastra se minimizează cât rulează un tur (ca înainte) și acum și
  cât se face o captură (altfel, fiind deasupra, ar intra în poză).

#### 5–6. Capturi estompate (`HelpCaptureRedaction`)
- Pe ecranul înghețat, ÎNAINTE ca operatorul să aleagă dreptunghiul (vede exact ce se salvează),
  se estompează TEXTUL (nu bara, nu meniul, nu rândul): utilizatorul conectat (e-mailul), numele
  unității, numele celorlalte unități din lista barei de titlu, orice «RO» urmat de cifre (IBAN, cod
  fiscal cu RO; spații simple permise), 13 cifre la rând (CNP). Eticheta de sus spune câte locuri
  au fost estompate.
- De unde: `Text` pentru controalele obișnuite (etichetă: textul măsurat după aliniere; căsuță:
  rândul scris; buton / combo: interiorul), elementele unui `ListBox`, și `IKBotCaptureRedaction`
  (interfață nouă, Theming) pentru controalele care își desenează textul: arbore (textul rândului
  și banda celulelor, textul capului), grilă (cutia de conținut a celulei), bara de titlu (doar
  numele unității din selector și partea titlului de după «K-BOT — »), `CustomPopup` și
  `KBotMenuWindow` (doar textul rândului).
- **Categorii adăugate cu acordul operatorului** (întrebat la sfârșitul feliei, 30.09.2026):
  **codul fiscal fără «RO»** (2-10 cifre) — doar într-un câmp sau o coloană care chiar e cod fiscal
  (numele / capul coloanei conține «cod fiscal», «CUI» sau «CIF»; interfața primește de aceea și
  contextul: cheia + capul coloanei, numele controlului); **adresele de e-mail**; **numerele de
  telefon** românești (07.., 02.., 03.., cu +40 / 0040). Refuzate: numele partenerilor, numele
  persoanelor de pe documente.
- Estomparea: zona micșorată la o șesime și întinsă înapoi, de două ori — nu se mai poate citi
  nicio literă.
- Nedescrisă în ajutor (unealta de capturi e a operatorului, regula din HELP_SYSTEM §5).

### Files touched

- `src/KBot.Theming/KBotHelp.vb` (`IKBotHelpParts`, `IKBotCaptureRedaction`), `KBot.Theming.vbproj` (FileVersion 1.13.2.0)
- `src/KBot.Controls/Tree/AdvancedTreeControl.HelpParts.vb` (nou), `.API.vb`, `.Painting.vb`, `.Properties.vb`
- `src/KBot.Controls/DataView/KBotDataView.HelpParts.vb` (nou)
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.HelpParts.vb` (nou), `KBotCaptionBar.vb`, `KBotCaptionBar.UnitSelector.vb`
- `src/KBot.Controls/NavList/KBotNavList.HelpParts.vb` (nou)
- `src/KBot.Controls/Popup/CustomPopup.CaptureRedaction.vb` (nou), `src/KBot.Controls/Menu/KBotMenuWindow.vb`
- `src/KBot.Controls/KBot.Controls.vbproj` (FileVersion 1.58.0.0)
- `src/KBot.Common/AppSettings.vb` (`HelpTextPercent`), `KBot.Common.vbproj` (FileVersion 1.5.8.0)
- `src/KBot.App/Help/`: `HelpTour.vb`, `HelpTourRunner.vb`, `HelpTourFrame.vb`, `HelpTourBubble.vb`, `HelpTourBubble.Designer.vb`,
  `HelpForm.vb`, `HelpForm.Designer.vb`, `HelpService.vb`, `HelpHtml.vb`, `HelpCaptureForm.vb`, `HelpCaptureOverlay.vb`,
  noi: `HelpHistory.vb`, `HelpWindowNative.vb`, `HelpCaptureRedaction.vb`
- `src/KBot.App/HelpContent/`: `README.md`, `contabil/ajutor.md`, `contabil/vederi/rezervari.md`, toate cele 17 `tours/*.md`
- `tools/HelpCheck/Check-Help.ps1`, `docs/HELP_SYSTEM.md`
- KBot.App FileVersion nebumped (se face la `push-update.ps1`).

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj` → **0 avertismente, 0 erori** (trage după el Controls, Theming, Common).
- `dotnet build KBot.sln` → proiectele din `src\` curate; **12 erori în `tests\KBot.App.Tests`**
  (`MainFormNavItemsTests`, `MainFormPoartaDdfTests`: lipsește argumentul `capturiApi` al
  constructorului `KbotForm`; `ForexeAnswerStoreTests`: `JobRequest` indexat) — preexistente, fără
  legătură cu felia; neatinse (regula: fără cod de test).
- `Check-Help.ps1 -Coverage` → **No errors.**; acoperire: doar `RobotQueueForm` (0098, deja semnalat).
- Niciun test rulat, nimic pornit pe ecran (regula operatorului).

### Left unverified or deferred

- **Nimic văzut pe ecran**: forma bulei (colțurile rotunjite DWM cu `Region`, marginile în trepte
  ale triunghiului), vârful la marginea ecranului, demo-ul iconiței de reîmprospătare, săriturile
  de pași, «Istoric ▾», «A−/A+», închiderea la un modal străin (pragul de 150 ms), estomparea.
- **Lupa din arborii vederilor**: numai lista de angajamente are `HeaderSearchIcon` setat în
  designer; la celelalte arbori pasul «Lupa» se sare singur dacă iconița nu există. De văzut.
- **Estomparea nu vede**: documentul din Adobe, pagina FOREXE, paginile din fereastra de ajutor
  (WebBrowser) și pozele încărcate cu «Încarcă». Eticheta la survolare (tooltip) a unui rând nu e
  citită. Numele scurte (sub 3 litere) nu se caută.
- **Categorii**: numele partenerilor și numele persoanelor de pe documente — propuse, NEalese de
  operator, deci neimplementate. Orice altă categorie, doar după întrebare.
- Codul fiscal fără «RO» se recunoaște doar după numele coloanei / câmpului: o coloană de cod
  fiscal cu alt nume (ex. «Cod») nu e prinsă.

#### De citit de operator
- **«Fișiere» în Fundamentare**: elementul e ascuns în designer (`KBotNavItem3.Visible = False`,
  commit «temp»), dar `contabil.ddf` încă spune că vederea are trei pagini, inclusiv «Fișiere». Turul
  nu-l mai arată; subiectul NU a fost schimbat — e ascuns temporar sau definitiv?

#### Capturi de refăcut
- `ajutor-fereastra-cautare` — bara ferestrei de ajutor are acum «Istoric ▾», «A−», «A+».

---

<a id="slice-0000-24"></a>

## SLICE-0000-24 — tour bubble without a caption bar, section titles in the «?» menu

Operator request, 30.09.2026, with two screenshots:

1. The guided-tour bubble should have **no caption bar at all**, a **slightly larger font**, and
   the **hole on its left side** must go.
2. In the «?» menu, **«Tururi ghidate»** sat too high (glued to the row above it) and did not read
   as the title of the tours listed under it.

### What changed and why

#### Tour bubble (`HelpTourBubble`)

- **Caption bar removed** (`capBar` gone from the designer). The tour's name, which the bar used to
  show, now leads the step line: «Fereastra principală  ·  Pasul 4 din 32». Closing stays on
  «Închide», Esc and Alt+F4 (all raise `CloseRequested`).
- **Larger text**: step line 9.75 pt, title 12.5 pt semibold (was 11), body 10.5 pt, note 10 pt,
  buttons 10 pt (explicit fonts in the designer). Width 400 → 440 logical, button columns
  100/100/100, button row 40 → 44, inner padding 14/8/14/10 → 16/12/16/12. `FitToText` no longer
  counts the bar and uses the 44 px row.
- **The hole on the left**: the callout window is the body plus a strip for the triangle; its
  Region cuts the strip down to the triangle. On Windows 11 DWM still rounds the WHOLE window
  rectangle and draws its grey 1 px border around it (the theme asks for rounded corners on every
  borderless form), so the transparent strip beside the triangle was framed and read as a hole.
  This is read from the screenshot (a rounded grey outline around the full rectangle, triangle
  included) — not measured on the machine. New `HelpWindowNative.PlainFrame(hWnd)` sets
  `DWMWA_WINDOW_CORNER_PREFERENCE = DWMWCP_DONOTROUND` and `DWMWA_BORDER_COLOR = DWMWA_COLOR_NONE`
  (Windows 10 does not know them: the calls return an HRESULT and change nothing). The bubble calls
  it after every placement, in `OnShown`, and after each theme change (`BeginInvoke`, because
  `ThemeManager.Apply` rounds borderless forms after `OnThemeChanged`).

#### «?» menu headers (`KBotHelpList`, shared with the search results of the help window)

- A header row now has air above it (16 px logical; 4 px for the first row, right under the
  search box) and 6 px below; its title is drawn in the **accent colour**, bold, standing on a thin
  rule (button-border colour) across the list. Before: dim grey, `VerticalCenter Or Bottom` in a
  row only 12 px taller than the text, so it looked glued to the row above.

### Files touched

- `src/KBot.App/Help/HelpTourBubble.Designer.vb` — capBar removed, fonts, sizes
- `src/KBot.App/Help/HelpTourBubble.vb` — tour name in `lblPas`, `FitToText`, `PlainFrame` calls, comments
- `src/KBot.App/Help/HelpWindowNative.vb` — `PlainFrame` (DWM corner + border)
- `src/KBot.Controls/Popup/KBotHelpList.vb` — header air, accent title, rule
- `src/KBot.App/HelpContent/contabil/ajutor.md` — «Meniul «?»» (titled parts) and «Tururile
  ghidate» (tour name + step at the top of the bubble), tags `0000-24`

KBot.Controls FileVersion was already bumped in the uncommitted work (1.57 → 1.58); not bumped again.

### Test results

- No tests (operator: no tests).
- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `Check-Help.ps1 -Coverage`: **No errors.** Coverage lists only `RobotQueueForm` (pre-existing,
  already in the 0000 Open threads under 0098).

### Left unverified or deferred

- **Not seen on screen.** The DWM diagnosis of the «hole» comes from the screenshot only; if the
  outline is still there on the operator's PC, the next suspect is the theme re-rounding after a
  path not covered here.
- The bubble still inherits `KBotShellForm` (8 px resize band on its edges); left as it was.
- Captures to re-shoot: `ajutor-meniu` (the headers look different now).
- Pre-existing, not from this slice: `RobotQueueForm` has no help topic (`-Coverage`).

---

<a id="slice-0000-27"></a>

## SLICE-0000-27 — «?» popup tours: only the window whose «?» was pressed (and its views)

Operator request, 01.10.2026: the guided tours listed in the «?» popup of a caption bar must show only what belongs to the window whose «?» was pressed; its views may share tours, any other window (with or without a parent) is not part of it.

### What changed and why

- `HelpPopupTours.Collect` takes the root window and looks at that window ONLY (the form and the views /
  controls inside it). No other window counts, owned by it or not. Before, it took every open form of
  the application. First pass (owned windows included) was rejected by the operator: only views are
  separate objects that may share tours; another window, with or without a parent, is not part of
  this window's «?». The per-window folders in the popup can no longer occur.
- `HelpService.ShowHelpMenu` passes `origin.FindForm()` (the form holding the pressed «?») and
  `PopupHomeRows` hands it on. A child control (a view) was already covered: a tour belongs to a
  window when its screen is visible inside it.
- Help: section «Meniul «?»» of `contabil/ajutor.md` (tag `0000-27`). `HELP_SYSTEM.md` row updated.

### Files touched

- `src/KBot.App/Help/HelpPopupTours.vb`, `src/KBot.App/Help/HelpService.vb`
- `src/KBot.App/HelpContent/contabil/ajutor.md`, `docs/HELP_SYSTEM.md`
- `docs/worklog/state/KBOT_STATUS_0000-0009.md`

### Test results

- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**.
- `tools/HelpCheck/Check-Help.ps1 -Coverage`: «No errors.» (coverage lists only RobotQueueForm, SetariIstoricView, UpdateOfferForm, unchanged).
- Nothing seen on screen.

### Unverified / deferred

- Not seen on screen.

---

<a id="slice-0000-30"></a>

## SLICE-0000-30 — guided tours show what the app hides, and say how to get it

Operator request, 01.10.2026: «all elements hidden by various facts (switches, not being connected to FOREXE...) in the guided tour must become visible when the step talks about them and must mention how they can get visible; also this must happen in the help».

### What changed and why

Before: a tour step about a part that was not on screen (a nav button the angajament has no data for, «Browser FOREXE» while not connected, a hidden FOREXE-band button, the «+» of a row, the footer menu icon of Rezervari) was skipped or got the note «Partea ... nu e pe ecran acum». The operator never saw it and the text said «gri» for things the app really HIDES (`KbotForm.Views.ApplyViewGating` uses `SetItemVisible`).

Engine (`IKBotHelpReveal`, new, `KBot.Theming/KBotHelp.vb`):
- `HelpTourRunner.ShowStep`: (1) a control named by `target: View.control` that is not visible but sits
  under a visible parent is shown for the step (`FindHiddenTarget`, put back by `EndDemo`) -- covers
  `SumarView.btnPartners`, `ForexeFooterView.lblCert / btnIstoric / btnBrowser / btnExtinde`;
  (2) a `part:` that is missing is asked of `IKBotHelpReveal` (target and every control above it,
  `TryRevealPart`), then measured again. When anything was shown the bubble adds the note «Îl vezi acum
  doar pentru tur: în mod obișnuit K-BOT îl ascunde ...».
- `KBotNavList` (`.HelpParts.vb`): a hidden item (`part: item:<Key>`) is shown, put back by `HelpRevealEnd`.
- `AdvancedTreeControl` (`.HelpParts.vb`): `node.icon` (picture on the selected leaf / first leaf) and
  `footer.left` from two new hidden properties `HelpDemoRightIcon`, `HelpDemoFooterLeftIcon`, which the
  VIEW fills: `RezervariView` (the «+» and `_iconitaMeniu`), `PlatiView` (the «+»).

Help (all tagged `0000-30`): every step / section about something conditional now says what makes it
appear for real.
- Tours: `tur-fereastra` (MENIU rows, intro of «Vederile» -- the old «gri» was wrong --, one condition per
  view step), `tur-forexe` (certificate label, Istoric, Browser, console buttons), `tur-sumar`
  (Asociaza parteneri), `tur-ddf`, `tur-rezervari`, `tur-plati`, `tur-ord` (the «+»), `tur-ddf-editor`
  (Sectiunea B), `tur-notecab`.
- Topics: `contabil.fereastra` (section «Vederile»: a table, view -> when it appears),
  `contabil.notecab`, `contabil.ddf.rezervare`, `contabil.forexe` (band table), `contabil.vederi.sumar`,
  `contabil.ddf.editor`, `contabil.ajutor` (tours: what hidden things do).
- Maintainer docs: `HelpContent/README.md` (Guided tours), `docs/HELP_SYSTEM.md` (tours + writing rule).

### Files touched

- Code: `src/KBot.Theming/KBotHelp.vb`, `src/KBot.App/Help/HelpTourRunner.vb`,
  `src/KBot.Controls/NavList/KBotNavList.HelpParts.vb`,
  `src/KBot.Controls/Tree/AdvancedTreeControl.HelpParts.vb`, `src/KBot.App/Views/RezervariView.vb`,
  `src/KBot.App/Views/PlatiView.vb`.
- Help: files above under `src/KBot.App/HelpContent/`, `help-version.txt`.

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors.
- `Check-Help.ps1 -Coverage`: see the STATUS row. Nothing was run on screen (operator rule).

### Left unverified or deferred

- **Not seen on screen.** The reveal needs one look by the operator: tour «Fereastra principală» on an
  angajament with few views (the hidden view buttons appear one by one, with the note, and disappear
  after their step); tour «Legătura cu FOREXE» while not connected; «Drumul unui document de
  fundamentare» on an angajament with a «+» left.
- **MENIU rows are NOT opened by the tour** («Jurnal activitate», «(!) Operațiuni necorelate»): the
  popup menu closes when its host window is deactivated (`KBotDropDownMenu.HostForm_Deactivate`) and the
  bubble takes the focus. The step text says when each row appears. Needs a decision if wanted
  (e.g. a bubble that does not activate).
- **A view hidden as a whole** (no data for the angajament) cannot be shown by a tour: the step keeps its
  note («selectează un angajament care o are»).
- **Finding for the operator (not changed):** `ForexeFooterView.btnIstoric` is hidden in the constructor
  and no code ever shows it -- the button is invisible in the real app, yet the help (`contabil.forexe`,
  `consola`) and `tur-forexe` described it as part of the band. The help now says it is hidden and no
  setting shows it; decide whether to show it or drop it from the help.
- `btnCoada` («Coadă N», slice 0098) is shown only while the robot queue has work; its help is part of the
  0098 backlog, not covered here.
- Capture ids to re-shoot: none.

---

<a id="slice-0000-31"></a>

## SLICE-0000-31 — MENIU opened by the tour, popups kept open during captures, help for the robot queue

Operator request, 01.10.2026 (follow-up to 0000-30).

### What changed and why

1. **MENIU in the tour.** New step key `reveal: menu` (only on `KbotForm.btnMeniu`; `HelpTour.Reveal`,
   `Check-Help.ps1`). `KbotForm.HelpReveal` (`KbotForm.HelpReveal.vb`, `IKBotHelpReveal`) opens `menuNou`,
   forces the rows «Jurnal activitate» and «(!) Operatiuni necorelate» visible (`MenuNou_Opening`), and the
   runner rings button + menu (`HelpReveal` now returns an area and an optional note;
   `KBotDropDownMenu.OpenBounds`). Ended with the step. Used in `tur-fereastra` and `tur-notecab`.
   **Design choice:** the bubble still takes the focus (so its keys work); the menu stays open because of
   the guard below, not because the bubble is non-activating.
2. **Popups kept open (`KBot.Theming/KBotPopupGuard.vb`).** `Hold/Release` (counted), `KeepOpen`,
   `CloseOnRelease`. Held from the capture bar's showing until the capture is taken or given up
   (`HelpCaptureForm.StartCapture` / `OnPromptFinished`), and by the tour's menu step. Checked where a popup
   closes on click-away / deactivation: `KBotDropDownMenu` (+ its message filter, keys pass through while
   held), `CustomPopup` (tree header-right menu, node right-click), `KBotFilterPopup` (grid column menu),
   tree `ColFilterPopup`, `KBotCalendarPopup`, `KBotComboBox` list. At release the popups that were kept
   close. The capture bar's hint text was updated.
3. **Robot queue help (0098, never covered).** New topic `contabil.forexe.coada` (+ capture tag
   `coada-robot`), new tour `tur-coada` (7 steps, runs only with the queue window open), new step «Coada
   robotului» in `tur-forexe` (the «Coadă N» button is revealed by the 0000-30 mechanism), rows in
   `contabil.fereastra` and `contabil.forexe`. Code: operator message in `RobotQueue.vb` («au fost removed»
   -> «au fost scoase»).
4. Docs: `HelpContent/README.md`, `docs/HELP_SYSTEM.md` (0000-30 text still accurate), `contabil.ajutor`.

### Files touched

Code: `KBot.Theming/KBotPopupGuard.vb` (new), `KBotHelp.vb`; `KBot.Controls`: `Menu/KBotDropDownMenu.vb`, `.Input.vb`, `Popup/CustomPopup.vb`, `DataView/Filter/KBotFilterPopup.vb`, `Tree/AdvancedTreeControl.ColFilter.vb`, `.HelpParts.vb`, `Calendar/KBotCalendarPopup.vb`, `Combo/KBotComboBox.DropDown.vb`, `NavList/KBotNavList.HelpParts.vb`; `KBot.App`: `KbotForm.HelpReveal.vb` (new), `KbotForm.HelpCapture.vb`, `Help/HelpTour.vb`, `HelpTourRunner.vb`, `HelpCaptureForm.vb`, `HelpCapturePromptForm.vb`, `Forexe/RobotQueue.vb`. Tools: `tools/HelpCheck/Check-Help.ps1`.

### Test results

Compile check (`dotnet build ... -p:OutDir=<scratch>`): 0 warnings, 0 errors. The normal build is blocked: K-BOT (and Visual Studio) are running and lock the DLLs in `src\KBot.App\bin`; close them and rebuild. Nothing run on screen.

### Left unverified or deferred

- **Nothing seen on screen:** the menu opening in the tour (ring around button + menu, bubble position), the
  popups staying open and closing at the end of a capture, `tur-coada` on a live queue.
- The tooltip popup of the tree and the «?» help popup are NOT held (tooltips / the help itself).
- A held menu ignores the keyboard (the keys go to the focused window).
- `tur-coada` ends with the queue: the window closes itself when the queue is empty.
- Capture to shoot: `coada-robot` (new).

---

<a id="slice-0000-32"></a>

## SLICE-0000-32 — Help window: one continuous page, print, export by scope, and the freeze on «Exportă»

Operator request, 01.10.2026. Reported: pressing «Exportă» in the help window froze K-BOT (Visual Studio: «The debugger timed out trying to pause process ... broken state»). Asked for: (1) scrolling in the help window goes through the whole contents, and the tree selects the topic reached; (2) the new `btnPrint` (added by the operator in `tlyBara`) prints, from a menu: everything (with «(!)» and a confirmation that recommends against it) / the current topic / everything of the parent topic; (3) the same menu on export. During the task the operator dropped the temporary PDF: «if we can print straight from the help window, let's do that, as long as it keeps the formatting and images».

### What changed and why

1. **The freeze on «Exportă» (hypothesis from the code, NOT proven by a log or a dump).**
   Since 0000-23 the help window closes itself when a dialog disables it and, 150 ms later, no enabled
   window owned by it is found (`TmrModal_Tick`). The export's «Save as» is the help window's own
   dialog, but a file dialog can take longer than 150 ms to appear (first use, more so under a
   debugger). In that case the check found nothing, took the dialog for another window's and closed the
   help window -- the owner of a dialog that was just being opened, while every other K-BOT window was
   still disabled for it. That leaves a modal loop with no window: K-BOT frozen. The 0000-01 open
   thread already said the export had never been pressed on screen, and 0000-23 added the check
   afterwards, so the path was never exercised.
   Fix, two layers (`HelpForm.vb`, `HelpWindowNative.vb`):
   - every dialog the help window opens itself (messages, «Save as», print) runs inside
     `Using OwnDialog()`; no check starts or closes anything meanwhile, and for 2 s after it returns;
   - a check that finds no dialog on screen at all yet (`HelpWindowNative.AnyEnabledWindow`) looks
     again 150 ms later instead of closing. Another window's dialog still closes the help window, as
     before, once that dialog is on screen.
2. **One page, the whole help.** `HelpHtml.Book` builds ONE document: the start page, each visible
   part's heading, every topic in contents order, each in its own `<div class='sec'>` with an id
   (`home`, `p-<part>`, `t-<topic id>`; a `## ` section is `t-<id>--<section>`). `HelpForm.Render`
   now scrolls to a section instead of loading a page; the document is built once per opening (and
   again on a theme change, a new picture, a topic reload). `HelpHtml.TopicPage` / `HomePage` are gone.
   - **The tree follows the scroll**: `tmrScroll` (250 ms, in the designer) reads how far the page is
     scrolled and, only when that changed, asks the page what is at the top of the view
     (`GetElementFromPoint`) and selects that topic's node. A timer rather than the page's scroll
     event: nothing has to be wired into each loaded document. On a read failure the timer stops (one
     log line, not four a second).
   - A topic reached by scrolling becomes `HelpHistory.Current` (so «Înapoi» returns there after a
     jump) but gets no «Istoric» line until it is left by a jump.
   - Part nodes of the tree now scroll to the part's heading (`p:<part>`); they used to open the start
     page. A click on the already selected node goes back to the start of its topic.
   - Text size in place: what was at the top of the view is scrolled back there after the change.
   - A topic this login may not read, or a missing one, still shows the message page (it replaces the
     document; the next page change loads the book again).
3. **«Imprimă»** (`btnPrint`, now always visible; it had inherited the old «Capturi...» button's
   capture-mode visibility through the rename). Menu `mnuPrint` -> `HelpScope` rows:
   «Tot ajutorul», «Subiectul curent: ...», «... cu subiectele de sub el (n)» (only when the topic has
   children), «Tot capitolul «...» (n subiecte)» (the parent with everything under it; only when
   there is a parent). Printing is straight from the window: the book carries `@media print` rules
   (black on white, fixed 10.5 pt, no start page / tour links / «În acest capitol»), the sections
   outside the scope get the class `noprint`, then `WebBrowser.ShowPrintDialog()` (Windows' print
   dialog, the operator chooses the printer). No PDF, no temporary file.
   «Tot ajutorul» and any scope over `HelpScope.LargeTopicCount` (15) topics carry «(!)» and are asked
   about first (default «Nu», the text recommends printing only what is needed).
4. **«Exportă»** (`btnManual`): the same menu (`mnuExport`), no «(!)» (nothing goes on paper).
   `HelpService.ExportManual` -> `HelpService.Export(owner, scope)`: everything = the manual as before
   (`HelpHtml.Manual`, cover + contents, `ManualParts`); less = `HelpHtml.Excerpt` (just those topics;
   a link to a topic outside the file becomes plain text). File name `Manual_KBOT_<date>.html` or
   `Ajutor_KBOT_<topic id>_<date>.html`.
5. **The «Capturi...» button of the help window is gone** (the operator replaced it with `btnPrint`
   in the designer). The capture list still opens from the main window's capture menu
   (`KbotForm.HelpCapture.vb`).
6. Help text: `contabil.ajutor` -- «Fereastra de ajutor» rewritten (continuous page, the buttons as
   they are now: icons, «Imprimă»), new section «Tipărirea și exportul»; new keywords. The start page
   says the whole help is on one page.

Design choices (decided without asking, say if wrong):
- the extra row «... cu subiectele de sub el»: on a chapter page (e.g. «Documentul de fundamentare»)
  «the parent» is «Despre K-BOT», i.e. the whole Part 1, so without this row a chapter could not be
  printed alone;
- «(!)» + confirmation also on a parent scope over 15 topics (that same «Despre K-BOT» case = 39);
- export keeps HTML (opens in the browser, «Save as PDF» there), as before.

### Files touched

- `src/KBot.App/Help/HelpForm.vb` (rewritten in place), `HelpForm.Designer.vb` (`mnuPrint`,
  `mnuExport`, `tmrScroll`; `btnPrint.Visible = False` removed; tooltips of `btnPrint`, `btnManual`)
- `src/KBot.App/Help/HelpHtml.vb` (`Book`, `Excerpt`, anchors, screen + paper CSS; `TopicPage`,
  `HomePage` removed)
- `src/KBot.App/Help/HelpScope.vb` (new)
- `src/KBot.App/Help/HelpService.vb` (`Export`), `HelpLibrary.vb` (`Branch`),
  `HelpWindowNative.vb` (`AnyEnabledWindow`)
- `src/KBot.App/HelpContent/contabil/ajutor.md`
- `docs/HELP_SYSTEM.md` (§1, engine table)
- `docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`, this file

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `Check-Help.ps1 -Coverage`: see «left unverified» (two findings, neither from this slice).
- No tests written, run or built (operator's rule). **Nothing was run or seen on screen.**

### Capturi de refăcut

- `ajutor-fereastra-cautare` (the bar now has icons, «Imprimă» and «Exportă»; the page is continuous).

### Left unverified or deferred

- **The cause of the freeze is read from the code, not measured.** If «Exportă» still freezes after
  this build, the hypothesis is wrong and a dump / the Call Stack of the main thread at the freeze is
  needed.
- **Never seen on screen**, all of it: the continuous page (about 2.8 MB of pictures embedded in one
  document -- loading time on a slow PC unknown), the tree following the scroll, the print dialog, the
  print rules (whether the left-out sections really stay out depends on the `class` attribute being
  honoured by the page's rendering mode; both attribute names are written).
- **Windows' print adds its own header and footer** (page title, page number, «about:blank», date):
  they come from the operator's Internet Explorer page setup, K-BOT does not change that setting.
- **Print dialog timing**: `ShowPrintDialog` is assumed to return after the dialog closes; the 2 s
  grace after an own dialog covers the other case.
- `Check-Help.ps1` reports, both older than this slice and not touched: slice tag `0077-3` in
  `contabil\fereastra.md` (no such slice in the index), and three windows without a topic
  (`HelpCaptureViewForm`, `SetariIstoricView`, `UpdateOfferForm`).
- De citit de operator: the new section «Tipărirea și exportul» and the confirmation text.
- Not committed (operator: no git).

---

<a id="slice-0000-34"></a>

## SLICE-0000-34 — Help for slice 0100 + stale captures marked in red (operator request, 01.10.2026)

Help work for feature slice **0100** (multi-thread downloads, branch `SLICE-0100-Multithreading`). Recorded as 0000-34 because all help work is slice 0000. The sections carry `<!-- slice: 0100 -->` as the rules ask.

### What changed and why

**Help text.**
- New topic `contabil.forexe.descarcare-multipla` («Mai multe descărcări deodată»): how to switch it on (advanced
  options), the five options with their defaults, the «Actualizează angajamente» window and its «Actualizat» column,
  how a download runs (N at once, the rest in a FIFO line, one failure does not stop the others, nothing saved until
  all ended, saved one after another, one tab left open), the on-connect update, the new-angajamente question,
  «Actualizează implicit toate recepțiile».
- `contabil.setari`: the «Generale» tab grouped (new section «cum e grupată fila»); `avansat` (step 3 names the new
  group); `contabil.forexe.descarcare` (picker skipped option + pointer), `.lista` (the new question), `.coada`
  (one task in the queue).

**Capture mechanism (new).** A capture tag may carry `redo: yyyy-MM-dd HH:mm` + `why: <slice>: <what changed>`. `HelpCapture.RedoSince/RedoWhy/NeedsRedo`; the capture list (`HelpCaptureForm`) paints a row RED (theme `ErrorColor`) while its saved picture is older than the `redo:` moment, writes «· de refăcut» in the state column, shows «DE REFĂCUT — why» above the preparation text and counts them in the summary line. Retaking or loading the picture clears it by itself. `Check-Help.ps1` knows the two keys. Documented in `HelpContent/README.md` and `docs/HELP_SYSTEM.md`.

### Captures to redo / new
- Red now (existing pictures, tagged `redo: 2026-10-01 22:48`): `setari`, `avansat-activare` (the «Aplicație» page was regrouped).
- New, missing (shown «lipsă»): `setari-descarcari-multiple`, `arbore-meniu-actualizare`, `actualizare-multipla`.

### Files touched
`src/KBot.App/Help/HelpCapture.vb`, `HelpCaptureForm.vb`; `tools/HelpCheck/Check-Help.ps1`;
`src/KBot.App/HelpContent/` `contabil/forexe/descarcare-multipla.md` (new), `contabil/setari.md`, `avansat/index.md`,
`contabil/forexe/descarcare.md`, `lista.md`, `coada.md`, `README.md`; `docs/HELP_SYSTEM.md`; status files.

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. `Check-Help.ps1 -Coverage`: the only errors left
are the slice tags `0100` (valid only once the fork with slice 0100 is merged — `KBOT_STATUS.md` on this branch has
no 0100 row) and the older `0077-3` tag in `contabil\fereastra.md`. Nothing run on screen.

### Unverified / deferred
- The red rows were not seen (no screen run). The `0100` tags validate only after the fork is merged into `master`.
- The tree's «Actualizat: …» tooltip line is not in the help (no capture shows a tooltip of the list).
