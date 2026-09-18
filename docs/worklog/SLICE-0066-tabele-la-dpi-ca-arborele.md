# SLICE-0066 — Toate tabelele din soluție sunt `KBotTableLayoutPanel`, cu măsurile și marginea calculate la DPI ca arborele și grila

**Data:** 17.09.2026
**Cerere (operator):**

> all the tablelayouts used throughout the entire solution must become custom tly with sizes
> and paddings computed per dpi (just like in advanced tree or dgv). no git. not tests. also
> make a harness form to test it out. change everywhere where it's used. this is a new slice.

Numărul feliei: 0066 = «Next free» din STATUS.

## 1. Ce era, de fapt, stricat

`KBotTableLayoutPanel` exista din felia 0062, dar modelul lui de scară NU era al arborelui:
ținea o fotografie a stilurilor **așa cum le lăsase autoscalarea WinForms** și o corecta doar
sub `Fixed100`/`Manual`; sub `Automatic` — modul în care rulează operatorul — rămâneau numerele
platformei. Iar numerele platformei sunt greșite pentru noi, și s-au MĂSURAT (sondă în afara
soluției, monitor la 150%): autoscalarea pe font înmulțește stilurile Absolute și `Padding`-ul
cu raportul a două înălțimi de font, **1,43 pe X și 1,67 pe Y**, nu cu 1,5 — și repetă
înmulțirea peste valoarea CURENTĂ la fiecare schimbare de font. Un rând autorat la 40 ieșea 67;
arborele de lângă el desena rândul de 22 la exact 33. Nu se aliniau niciodată.

## 2. Modelul nou — cel al lui `AdvancedTreeControl.Dpi.vb` / `KBotDataView.Dpi.vb`

Controlul e acum trei partiale în `KBot.Controls/Table/`: `KBotTableLayoutPanel.vb`
(comutatoare, culori, grila din temă), `.Dpi.vb` (modelul logic, declanșatoarele, API-ul de
rulare), `.Fit.vb` (regula surplusului din 0062 și `GetPreferredSize`).

- **Două valori pentru fiecare măsură.** AUTORATĂ = logică (px la 96 dpi): ce a scris
  designerul, ce întoarce `Padding`, ce citesc testele. LIVE = cea citită de motorul de
  așezare (`RowStyle`/`ColumnStyle`, `MyBase.Padding`) = autorată × scară, pixeli întregi.
  Scara e `AppScaling.FactorFor(Me)` — aceeași sursă unică ca arborele și grila: `DeviceDpi/96`
  × mărimea textului sub Automatic, 1 sub Fix 100%, numărul operatorului sub Manual.
- **Platforma e DECLANȘATOR, nu sursă.** `ScaleControl` face fotografia ÎNAINTE de apelul de
  bază (prima autoscalare e primul lucru care atinge stilurile după `InitializeComponent`,
  deci ce găsește e valoarea autorată — verificat cu sonda), lasă baza să înmulțească, apoi
  rescrie totul din sursa logică. Același drum la `OnHandleCreated`, `OnDpiChangedAfterParent`,
  `ApplyTheme`, `RefreshDpiMetrics` (difuzarea din `AppScaling.Broadcast`). Nimic nu se
  derivă vreodată dintr-o valoare live, deci două treceri nu se pot compune.
- **`Padding` e umbrit** (`Control.Padding` nu e virtual — verificat prin reflecție): getter-ul
  întoarce valoarea logică (serializatorul scrie ce a tastat operatorul), `MyBase.Padding`
  poartă valoarea de ecran, `PaddingPx` o arată. Cine ține tabelul ca `Control` (motorul de
  așezare, `ThemeFormFit`) citește în continuare valoarea de ecran.
- **Marginile copiilor rămân ale platformei** — sunt proprietățile copiilor, iar arborele nu
  se bagă în alte controale. Scris în doc, la «Where it stops».
- **La design time scara e 1** (C6), spre deosebire de arbore: suprafața VS ștampilează pixeli
  de ecran în stilurile unui tabel, deci scalate încă o dată s-ar vedea de două ori.
- **Sub `ScaleAbsoluteStyles = False`** tabelul rămâne la pixelii logici (se desface și
  scalarea platformei) — ce face `Fixed100` pentru toată aplicația, disponibil per tabel.

**API de rulare, tot în pixeli logici și idempotent:** `SetRowHeight`/`SetColumnWidth`
(scrie măsura autorată, face stilul Absolute, ridică o strângere), `SetRowCollapsed`/
`SetColumnCollapsed` + `IsRowCollapsed`/`IsColumnCollapsed` (0 păstrat peste orice trecere;
un rând Percent/AutoSize refuză cu `ArgumentException`, C3), `RefitToTheme()` (toată trecerea
la cerere), `ResetStyleBaseline()` rămâne ca portiță pentru cine scrie stilurile direct.
Stilurile adăugate la rulare (numărul lor s-a schimbat) se citesc din valoarea live,
DEscalate; cele neatinse își păstrează măsura logică.

## 3. Ce a arătat bancul, și cele trei reguli de măsurare care au ieșit din el

Bancul a fost desenat prin `DrawToBitmap` dintr-o gazdă de unică folosință (form cu
`Opacity = 0`, `Show`, `PerformClick` pe butoane), pe monitorul de 150% cu textul la 110%
(scara 1,65). Prima rulare a dat trei numere greșite, toate în FIT, nu în scară — toate cu
aceeași formă: rândul/coloana nu putea să VINĂ ÎNAPOI.

1. **Coloana gazdei de 300 ieșea 628.** `ThemeFormFit.ContentDemand` plimba arborele ca pe un
   container și îi aduna caseta de căutare, poziționată de el din propria mărime — ecoul
   celulei. Regulă nouă în `ThemeFormFit`: un control care se pictează singur și își ține
   copiii (`IThemedControl`, nu `IThemedContainer`, nu `ContainerControl` — vederile compozite
   rămân plimbate) **nu cere nimic**.
2. **Coloana de 180 rămânea 297 sub Fix 100%** (trebuia 198). `KBotTextField.GetPreferredSize`
   întorcea `Width` — adică celula. Acum `KBotTextField` și `KBotTextBox` întorc lățime 0 când
   andocarea le întinde (n-au lățime proprie), lățimea autorată altfel.
3. **Rândul butoanelor de 40 rămânea 66 sub Fix 100%** (trebuia 44). MĂSURAT în sondă: un
   `Button` andocat răspunde la `GetPreferredSize` cu propriile margini, pe ORICE `FlatStyle`,
   cu sau fără `AutoSize`. `ThemeFormFit` îl măsoară acum de mână — umplutură + un rând de
   text + cele două chenare, formula lui `ModernRenderer.FitHeightToPaddingAndText`; lățime 0
   când e întins.

După cele trei: la 1,65 coloanele 90/180/12/110 → 149/297/20/182, rândurile 32/32/40/24 →
53/53/66/40, marginea 8 → 13, coloana gazdei 300 → 495; sub Fix 100% (×1,10 din text)
99/198/13/121, 43/43/44/30, 9, 330 — unde 43 și 30 sunt 35 + surplusul câmpului de text și
26 + surplusul etichetei, exact regula din 0062; arborele cu `ItemHeight = 32` dă 53 = rândul 0
al tabelului sub Automatic. Strângere rând/coloană, `SetRowHeight(0, 60) → 99`,
`Padding = 24 → 40`, și înapoi: toate din sursa logică, nimic compus.

## 4. Peste tot

- **35 de `.Designer.vb`, ~80 de tabele**, în `KBot.App` (29 de fișiere), `KBot.Controls`
  (`KBotFilterPopup`, `KBotFilterConditionDialog`), `KBot.DevHarness` (`AdobeReaderHarnessForm`
  cu 32, `FormFitHarnessForm`, `FormFitProbeDialog`) și `KBot.Migrator` — înlocuire textuală
  `TableLayoutPanel` → `Global.KBot.Controls.KBotTableLayoutPanel` la declarații și
  instanțieri, cu sfârșiturile de linie păstrate. În `src/` nu mai există niciun
  `TableLayoutPanel` simplu în afară de `_reference/` (necompilat) și de tipurile parametrilor
  din `ThemeTableFit`.
- **Scriitorii de stiluri de la rulare, mutați pe API:** `OrdBeneficiariPage.AplicaVizibilitateaBifei`
  (banda bifei — `SetRowCollapsed(0, …)`, câmpul `_inaltimeaBifei` a dispărut),
  `KBotFilterConditionDialog.StrangeRandul` (`SetRowCollapsed` pe rând fix, `SetRowHeight(…, 0)`
  pe celelalte), `KBotFilterPopup` (fără `ThemeTableFit.Capture`; `SetRowCollapsed` pentru
  rândul condițiilor; `RefitToTheme()` în `AjusteazaInaltimea`), `FormFitHarnessForm`
  (strânge/desface banda prin `SetRowCollapsed`, fără `ResetStyleBaseline`).
- **`ThemeTableFit` a rămas în `KBot.Theming` fără niciun apelant în `src/`** — nu s-a șters
  în felia asta (are comentarii care îl citează în `KBotTextField`/`KBotTextBox`; testele
  Theming nu îl folosesc). De șters într-o felie de curățenie.

## 5. Bancul de probă

`KBot.DevHarness/Internal/TableLayoutHarnessForm` (+ `.Designer.vb`, toate controalele în
designer) și `Tests/TableLayoutHarnessTest` («KBotTableLayoutPanel — măsuri la DPI ca arborele
(0066)», Controls/UI, nedistructiv, fără conexiune). Pe ecran: butoanele de schemă, scara
K-BOT (automată / fix 100% / manual × N, prin `AppScaling.Configure`, ca formularul de opțiuni)
și cursorul de text; comutatoarele tabelului; strânge/desface rândul 3 și coloana 3,
`SetRowHeight(0, 60|32)`, `Padding → 24|8`; un tabel-gazdă (coloană fixă 300 pentru arbore —
tabel în tabel) cu tabelul-sondă în stânga și un `AdvancedTreeControl` cu `ItemHeight = 32`
(același număr ca primele două rânduri) în dreapta. Citirea scrie în jurnal și pe fața
tabelului: scara și modul, fiecare coloană/rând fix autorat → acum, marginea, coloana
gazdei, și rândul arborelui față de rândul 0 al tabelului («= ACELAȘI» / «≠ DIFERIT»). Schema,
modul de scalare și mărimea textului se pun înapoi la închidere.

## 6. Fișiere atinse

- `src/KBot.Controls/Table/KBotTableLayoutPanel.vb` (rescris), `KBotTableLayoutPanel.Dpi.vb`
  (nou), `KBotTableLayoutPanel.Fit.vb` (nou), `KBotTableLayoutPanel.md` (nou);
  `CONTROLS.md`, `README.md` (rânduri de index).
- `src/KBot.Theming/ThemeFormFit.vb` — `ContentDemand`: regula controlului care se pictează
  singur + `ButtonDemand`.
- `src/KBot.Controls/TextField/KBotTextField.vb`, `KBotTextBox.vb` — `AuthoredWidthDemand`.
- 35 de `.Designer.vb` (lista la §4) + `OrdBeneficiariPage.vb`, `KBotFilterConditionDialog.vb`,
  `KBotFilterPopup.vb` (+ două comentarii în `.Designer.vb`), `FormFitHarnessForm.vb`
  (+ textul unui buton în `.Designer.vb`).
- `src/KBot.DevHarness/Internal/TableLayoutHarnessForm.vb` + `.Designer.vb`,
  `Tests/TableLayoutHarnessTest.vb` (noi).
- `tests/KBot.Controls.Tests/KBotTableLayoutPanelTests.vb` — rescris pe modelul nou: scară
  (`Manual`, `Fixed100`, text, Automatic exprimat prin `DpiScale`), rotunjire, autoscalarea
  platformei ca declanșator (form `AutoScaleMode.Font` cu ștampilă mai mică + schimbare de
  font), fit peste măsura scalată, buton andocat, API-ul (strângere, `SetRowHeight`,
  `Padding`, stiluri adăugate la rulare), `ResetStyleBaseline`, grila din temă, traversarea,
  serializarea (inclusiv `Padding` logic).
- `docs/worklog/KBOT_STATUS.md`, acest worklog.

## 7. Rezultate

- `dotnet build KBot.sln`: **0 erori, 0 avertismente** în afara celor 7 `MSB3825` PREEXISTENTE
  pe `.resx`.
- **NICIO SUITĂ N-A FOST RULATĂ și nimic nu s-a comis** (cerere explicită: «no git. not
  tests»). Testele noi/rescrise compilează; sunt de rulat: `dotnet test
  tests\KBot.Controls.Tests` (niciodată `KBot.sln`, vezi memoria despre DevHarness).
- **Văzut pe ecran** prin `DrawToBitmap`, șase stări, dintr-o gazdă de unică folosință în
  afara soluției; **niciun gest făcut cu mâna**, bancul nu a fost deschis din DevHarness.

## 8. Neverificat / amânat

- ⚠ **Cele ~80 de tabele migrate nu s-au văzut pe ecran** — s-a văzut doar bancul. Efectul
  vizibil așteptat pe formularele existente: rândurile/coloanele fixe și marginile tabelelor
  ies acum la `DeviceDpi/96` × text (1,5 la 150%) în loc de 1,43×1,67, adică puțin mai
  ÎNGUSTE pe Y și puțin mai LATE pe X decât ieri; și orice rând cu un buton andocat poate
  acum să REVINĂ la măsura autorată (ieri rămânea la cea mai mare atinsă). Un formular
  autorat strâns pe cifrele vechi se poate schimba cu câțiva pixeli — de văzut la prima
  deschidere, mai ales `LoginForm`, `AlegereUnitateForm`, `OrdEditForm`, `DdfEditForm`.
- ⚠ `ButtonDemand` cere cu câțiva pixeli MAI PUȚIN decât chenarul pe care WinForms îl pune în
  jurul textului unui buton (măsurat: 53 față de 38 pentru un buton Standard cu umplutură 16
  la 144 dpi): un rând autorat MAI STRÂMT decât textul + umplutura butonului taie chenarul, nu
  textul. Formula e cea a casei (`ModernRenderer`), deliberat.
- ⚠ Sub `Fixed100`/`Manual` tabelul se întoarce la pixelii logici, dar `Bounds`-urile și
  fonturile din jur rămân scalate de platformă — compromisul documentat al acelor moduri
  (`AppScaling`), vizibil intenționat în banc, nu o scăpare.
- ⚠ Marginile copiilor (`Margin`) rămân scalate de platformă (1,43×1,67), nu de noi.
- ⚠ `ThemeTableFit` fără apelant — de șters.
- ⚠ Comportamentul la design time (scara 1) e raționat, nu văzut: nimic nu s-a deschis în
  designerul VS.
- ⚠ Testele vechi de formă (`ThemeFormFitTests`) n-au fost atinse și nu folosesc butoane
  andocate sau `IThemedControl`, deci regulile noi din `ContentDemand` nu le ating pe hârtie
  — de confirmat la rulare.
