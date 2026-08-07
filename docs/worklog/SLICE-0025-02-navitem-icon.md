# SLICE-0025-02 — pictogramă stânga pe elementele de navigație

Pasul 2 al feliei 0025 (vezi `SLICE-0025-designer-surfaces.md` pentru pasul 1 și pentru pasul 7).
Cerere de operator, în afara planului original: «elementele de navigație trebuie să aibă încă o
proprietate pentru o pictogramă în stânga; să fie ca orice pictogramă de buton».

---

## 1. Ce s-a schimbat

### `KBotNavItem.Image`

```vb
<Category("K-BOT")>
<Description("Pictograma desenată la stânga textului (ca Image-ul unui buton). Ignorată pe separatori.")>
Public Property Image As System.Drawing.Image
```

**Numele e `Image`, nu `Icon` sau `IconImage`, deliberat.** Cererea a fost «ca orice pictogramă de
buton», iar echivalentul exact e `Button.Image`: același nume, același tip, **același editor de
imagine din grila de proprietăți** (cel stoc — designer-ul depune imaginea în `.resx`-ul
formularului și emite `KBotNavItem1.Image = CType(resources.GetObject(…), Image)`). Cine caută
proprietatea o va căuta sub numele ăsta. `KBotCaptionBar` folosește `IconImage`, deci în casă
există acum două nume pentru aceeași idee — abaterea e conștientă și e consemnată aici; bara de
titlu nu se editează prin dialog de colecție, deci cele două nu se întâlnesc niciodată în aceeași
grilă.

Tipul e scris CALIFICAT (`System.Drawing.Image`). În VB, o proprietate `Image` din aceeași clasă
ascunde numele de tip `Image`, deci `Public Property Image As Image` nu compilează.

Trei decizii mici:
- **Fără `ImageAlign`.** Un rând de navigație e „pictogramă apoi text", prin construcție. O
  aliniere pe care nimeni nu a cerut-o e încă un lucru care se poate seta greșit.
- **Separatorii o ignoră**, exact ca pe `Key` și `Text`.
- **Elementul NU deține imaginea** și nu o eliberează niciodată: ea aparține apelantului sau
  resurselor formularului, ca `KBotCaptionBar.IconImage`. Test dedicat — o imagine partajată de
  două elemente nu are voie să moară odată cu primul.

### `KBotNavList.IconSize`

Latura (px logici, scalați la DPI) a pătratului în care se desenează pictograma; implicit **20**,
`DefaultValue`, deci designer-ul nu o serializează degeaba.

Se aplică TUTUROR elementelor, nu per element: într-o bară de navigație pictogramele trebuie să
aibă aceeași mărime, altfel textul nu se mai aliniază pe verticală. Sursa poate fi de orice
dimensiune (o probă de 64px se scalează în pătratul de 20), deci nu impune lățimea rândului.
`IconSize = 0` înseamnă «fără pictograme» — nu «pictograme de zero pixeli care lasă totuși o
gaură»: textul revine la padding-ul simplu.

### Geometrie și pictare

Trei funcții private, folosite **și** la măsurare **și** la pictare, ca cele două să nu se poată
despărți (dacă slotul crește, textul se mută cu el, automat):

| Funcție | Ce dă |
|---|---|
| `IconSide()` | latura nominală, scalată la DPI |
| `IconSlotWidth(it)` | cât mănâncă pictograma din lățime (0 fără imagine / pe separator / `IconSize = 0`) |
| `IconRect(it, r)` | pătratul efectiv, centrat vertical, **strâns** dacă rândul e mai scund decât latura nominală |

`ItemExtent` (orizontal) adaugă `IconSlotWidth`; pe vertical înălțimea rândului e fixă, deci
pictograma nu schimbă nimic. Textul începe acum de la `r.Left + padX + IconSlotWidth(it)` în loc de
`r.Left + padX`. Badge-ul rămâne neatins (e ancorat la dreapta).

**Pe un element dezactivat pictograma se estompează**, ca imaginea unui `Button` dezactivat:
`ImageAttributes` + `ColorMatrix` cu luminanța standard (0.299/0.587/0.114) și alfa 45%. Fără asta,
un buton stins ar avea text gri lângă o pictogramă în culori vii. S-a ales matricea în locul lui
`ControlPaint.DrawImageDisabled` fiindcă aceea nu scalează imaginea, iar noi desenăm mereu într-un
pătrat impus.

---

## 2. Fișiere atinse

- `src/KBot.Theming/KBotNavItem.vb` — proprietatea `Image`
- `src/KBot.Theming/KBotNavList.vb` — `IconSize`, cele trei funcții de geometrie, `ItemExtent`,
  pictarea, `DrawItemImage`, două cârlige `Friend` de test, `Imports System.Drawing.Imaging`
- `src/KBot.Theming/KBot.Theming.vbproj` — `FileVersion` 1.1.0.0 → **1.2.0.0**
  (`AssemblyVersion` stă: suprafața doar crește)
- `tests/KBot.Theming.Tests/KBotNavListTests.vb` — 8 teste noi
- `docs/worklog/SLICE-0025-02-navitem-icon.md` (acesta), `docs/worklog/KBOT_STATUS.md`

Niciun element din `MainForm` nu primește o pictogramă în pasul ăsta — s-a cerut PROPRIETATEA, nu
un set de pictograme. `MainForm.Designer.vb` e neatins, `KBot.App` nu primește bump.

---

## 3. Rezultate de teste

Publicat înainte (`artifacts\KBot_Debug_20260807_101720`), rulat pe mașina clientului, ieșirea în
`src\KBot.App\bin\Debug\net8.0-windows\win-x64\Logs\test_20260807_101750_823.log`.

**769 passed / 0 failed / 0 skipped** (761 înainte de pas; `KBot.Theming.Tests` 51 → **59**).
Build: **0 erori, 0 avertismente**.

Cele 8 teste noi: un element fără imagine lasă textul exact unde era (regresie pe rândurile
existente); o imagine primește un slot PĂTRAT, lipit de padding-ul din stânga, centrat vertical, și
împinge textul cu `slot + spațiu`; `IconSize` conduce și slotul și decalajul textului, iar 0 le
anulează pe amândouă; slotul se STRÂNGE într-un rând mai scund decât latura nominală și nu iese din
el; o imagine lățește un element ORIZONTAL dar nu unul vertical; separatorul își ignoră imaginea;
pictarea headless trece prin ambele ramuri (activ / dezactivat), ceea ce contează fiindcă o matrice
de culoare greșită ARUNCĂ, nu doar arată prost; imaginea supraviețuiește eliberării barei.

**Corecție adusă și testelor existente din pasul 1:** patru assert-uri comparau cu pixeli logici
literali (`20`, `12`, `nav.Height - 6`). Treceau aici fiindcă mașina e la 96 dpi, dar ar fi picat
pe un ecran la 150%. Toate trec acum prin `ThemeShapes.ScaleDpi` (vizibil testelor prin
`InternalsVisibleTo`), deci verifică relația, nu numărul.

---

## 4. Nefăcut / neverificat

- **Nimic nu a fost văzut pe ecran.** Cum arată o pictogramă de 20px lângă textul de navigație, dacă
  estomparea de 45% e destulă pe schema Dark, și dacă interpolarea bicubică e curată la scalarea
  unei surse mari — toate sunt judecăți vizuale, iar nimeni nu a rulat aplicația. Testele probează
  GEOMETRIA și faptul că pictarea nu aruncă; atât.
- **Editorul de imagine din grila de proprietăți nu a fost deschis** — ca tot restul feliei 0025,
  vezi §4 din worklog-ul pasului 1. În particular, rotunda unei imagini printr-un element de
  COLECȚIE (designer-ul trebuie să scrie în `.resx` și să emită
  `CType(resources.GetObject("KBotNavItem1.Image"), Image)`) e mecanismul standard, dar **nu a fost
  probat**; e prima proprietate din felie care are nevoie de fișierul de resurse al formularului.
- **`IconSize` e per bară, nu per element.** Dacă apare vreodată nevoia de pictograme de mărimi
  diferite în aceeași bară, asta e schimbarea de făcut — nu s-a construit din start.
- Nicio pictogramă reală nu a fost aleasă sau desenată pentru cele opt intrări din `MainForm`.
  `KBot.App` are deja generatoare GDI tematizabile (`FxIcons`, `DdfIcons`, `PlatiIcons`,
  `RezervariIcons`, `ReceptiiIcons`) — dacă intrările primesc pictograme, de acolo ar veni, prin
  cod, nu prin `.resx`.
