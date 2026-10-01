# SLICE-0000-23 — Tururi cu vârf și pe butoane, fereastra de ajutor (istoric, mărime text, deasupra), capturi estompate

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

## What changed and why

### 1. Callout (`HelpTourBubble`)
- Bula are acum un triunghi pe latura dinspre țintă, cu vârful pe marginea exterioară a inelului
  (`HelpTourFrame.Outset`). Fereastra crește cu adâncimea triunghiului pe latura aceea, `Padding`
  îi dă corpului înapoi fâșia, iar `Region` = dreptunghiul corpului ∪ triunghiul. Culoarea de accent
  care desenează conturul de 1 px umple și triunghiul.
- Așezare: dreapta, stânga, dedesubt, deasupra — prima latură unde încape TOATĂ bula; o țintă prea
  mare pentru orice latură (o vedere întreagă) primește bula înăuntru, cu vârful în sus spre
  marginea ei. Vârful alunecă pe latură până la colțuri când bula e împinsă de marginea ecranului.
- Designer: `BorderlessShadow = False` (o fereastră cu `Region` nu mai primește umbra DWM),
  `AutoFitToTheme = False` (bula își face singură mărimea, `FitToText`).

### 2. Părțile controalelor (`IKBotHelpParts`)
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

### 3. Conținutul tururilor (17 tururi, ~60 de pași noi)
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

### 4. Fereastra de ajutor
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

### 5–6. Capturi estompate (`HelpCaptureRedaction`)
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

## Files touched

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

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj` → **0 avertismente, 0 erori** (trage după el Controls, Theming, Common).
- `dotnet build KBot.sln` → proiectele din `src\` curate; **12 erori în `tests\KBot.App.Tests`**
  (`MainFormNavItemsTests`, `MainFormPoartaDdfTests`: lipsește argumentul `capturiApi` al
  constructorului `KbotForm`; `ForexeAnswerStoreTests`: `JobRequest` indexat) — preexistente, fără
  legătură cu felia; neatinse (regula: fără cod de test).
- `Check-Help.ps1 -Coverage` → **No errors.**; acoperire: doar `RobotQueueForm` (0098, deja semnalat).
- Niciun test rulat, nimic pornit pe ecran (regula operatorului).

## Left unverified or deferred

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

### De citit de operator
- **«Fișiere» în Fundamentare**: elementul e ascuns în designer (`KBotNavItem3.Visible = False`,
  commit «temp»), dar `contabil.ddf` încă spune că vederea are trei pagini, inclusiv «Fișiere». Turul
  nu-l mai arată; subiectul NU a fost schimbat — e ascuns temporar sau definitiv?

### Capturi de refăcut
- `ajutor-fereastra-cautare` — bara ferestrei de ajutor are acum «Istoric ▾», «A−», «A+».
