# SLICE-0028-06 — Meniul de filtrare, autorat în DESIGNER (nu desenat în cod)

Cerere de operator:

> i want the filtering form to be configed in the designer, not in normal code. of course, except
> the dynamic "list" of values. that can also be configured in the designer, but the values in it
> should remain filled at runtime

---

## 1. De ce era desenat în cod, și de ce motivul acela a expirat

`KBotFilterPopup` s-a născut în 0028-03 ca **fereastră desenată integral de noi**: ~400 de linii de
pictură (`.Painting.vb`) plus tot atâtea de geometrie și hit-test în fișierul principal. Motivul era
scris acolo și era corect atunci: «un `ContextMenuStrip` cu un `CheckedListBox` în el ar rămâne două
dreptunghiuri albe sub o schemă întunecată».

Între timp motivul a dispărut, tot în felia 0028: `ThemeManager` are reguli pe tip pentru
`CheckedListBox`, `ListBox`, `CheckBox`, `Button` și `Panel` — inclusiv tema NATIVĂ a barelor de
derulare (`DarkMode_Explorer` / `Explorer`) — iar `KBotThemedForm` le aplică singur, la încărcare și
la fiecare comutare de schemă. Adică exact ce lipsea în 0028-03 există acum.

Deci meniul a fost **rescris ca formular obișnuit al casei**, cu regula casei aplicată la literă:
toate controalele în `KBotFilterPopup.Designer.vb`.

## 2. Ce s-a schimbat

| Înainte | Acum |
|---|---|
| `Inherits Form` + `Implements IThemedControl`, culorile luate manual într-un `ApplyTheme` propriu | `Inherits KBotThemedForm` — tema vine prin `ThemeManager.Apply`, ca la orice formular |
| rânduri de meniu desenate (`MenuRow`, `Bounds`, `HotNone/HotOk/HotCancel`), hit-test pe `OnMouseDown` | patru `Button`-e docate sus + `CheckBox` + `CheckedListBox` + `pnlButoane` cu OK/Anulează |
| lista de valori desenată rând cu rând, cu derulare proprie (`_listScroll`, `ClampScroll`, `DrawListScrollHint`) | `CheckedListBox` cu bara ei, tematizată nativ |
| «(Selectează tot)» desenat cu bifă în trei stări | `CheckBox` cu `ThreeState = True` |
| lățime și înălțime calculate din texte (`MeasureNaturalWidth`) | **lățimea și toate mărimile sunt ale designerului**; codul mai atinge doar ÎNĂLȚIMEA, ca lista să arate câte rânduri are (până la 10) |
| `KBotFilterPopup.Painting.vb` (408 linii) | **șters** |

Ce a rămas neapărat la rulare, și de ce:

- **conținutul listei** — valorile distincte ale unei coloane nu există la proiectare (chiar cererea
  operatorului); controlul, fontul, înălțimea rândului și locul lui sunt însă din designer;
- **textele care depind de tipul coloanei** — sortarea se numește «A → Z» pe text și «de la mic la
  mare» pe numere (`KBotFilterEngine.SortCaption`), iar «Șterge filtrul din «X»» poartă numele
  coloanei;
- **activarea lui «Șterge filtrul»** (stins fără filtru) și **existența butonului de condiții**
  (coloanele logice n-au submeniu — butonul se ASCUNDE, nu se stinge, ca să nu rămână un rând care
  nu duce nicăieri).

**Deciziile n-au fost atinse.** Filtrul se predă la OK dintr-o COPIE (`Clone`), sortarea se aplică
imediat și închide meniul, «tot bifat» ≠ filtru, «(Selectează tot)» atinge doar rândurile ARĂTATE,
golul se numește «(Necompletate)» dar rămâne text vid în model, iar `_suppressDeactivate` ține
meniul deschis cât timp submeniul de condiții e sus. Vechiul `KBotFilterPopupTests` a trecut
NESCHIMBAT — el fixează tocmai deciziile, care n-aveau voie să se miște fiindcă s-a schimbat felul
în care e desenat meniul.

## 3. Defect găsit pe drum: `KBotTextField` nu ridica niciodată `TextChanged`

Primul `Handles txtCauta.TextChanged` scris în noul formular n-a rulat niciodată. Cauza:
`KBotTextField.Text` e delegat la `TextBox`-ul intern, deci textul se schimba fără ca evenimentul
CADRULUI să se ridice vreodată — până acum toți consumatorii se legau la `InnerTextBox.TextChanged`
și nimeni n-a observat. Pentru un formular autorat în designer asta e o capcană curată: legi
evenimentul în designer și nu se întâmplă nimic, tăcut.

Reparat în control: `_inner.TextChanged` → `OnTextChanged(EventArgs.Empty)` + `Invalidate()`
(placeholder-ul oricum se redesena la fel).

## 4. Ce poate face operatorul acum

`KBotFilterPopup.vb` se deschide în designerul Visual Studio (a plecat `<DesignerCategory("Code")>`)
și se schimbă acolo fonturile, înălțimile rândurilor, lățimea meniului, ordinea butoanelor,
`ItemHeight`-ul listei — fără să atingă cod. Chenarul de 1px al meniului nu e pictat: e `Padding`-ul
formularului peste fundalul lui, iar culoarea vine din temă (`OnThemeChanged`), deci **nu există
nicio culoare scrisă în designer sau în cod**.

## Fișiere atinse

| Fișier | Ce s-a schimbat |
|---|---|
| `src/KBot.Controls/DataView/Filter/KBotFilterPopup.Designer.vb` | **nou** — toate controalele, cu ordinea de andocare inversă celei vizuale |
| `src/KBot.Controls/DataView/Filter/KBotFilterPopup.vb` | rescris: doar comportament (630 → ~470 linii, fără geometrie și fără pictură) |
| `src/KBot.Controls/DataView/Filter/KBotFilterPopup.Painting.vb` | **șters** (408 linii) |
| `src/KBot.Controls/TextField/KBotTextField.vb` | `TextChanged` al cadrului se ridică la schimbarea textului intern |
| `tests/KBot.Controls.Tests/KBotFilterPopupDesignerTests.vb` | **nou**, 9 teste care trec prin CONTROALE, nu prin porțile `Debug*` |

**Versiuni**: `KBot.Controls` 1.16.0.0 → **1.17.0.0**.

## Rezultatele testelor

```
dotnet build KBot.sln    → 0 erori
KBot.Controls.Tests      → 708 / 708   (699 înainte, +9)
KBot.DevHarness.Tests    → 170 / 170
KBot.Theming.Tests       →  65 /  65
```

Cele 9 teste noi: lista e declarată în designer și umplută la rulare, căutarea reumple CONTROLUL,
o bifă stinsă în `CheckedListBox` ajunge în filtru (prin `ItemCheck`, drumul mouse-ului), un filtru
existent aprinde bifele potrivite, «Șterge filtrul» e stins fără filtru, «(Selectează tot)» arată
starea mixtă și atinge doar rândurile arătate, coloana logică n-are rândul de condiții (verificat
prin CONSECINȚĂ — ce urmează urcă — fiindcă `Visible` pe un formular nearătat răspunde despre lanțul
de părinți, nu despre buton), iar titlurile de sortare vin din tipul valorii.

## Rămase neverificate / amânate

- **NEVĂZUT PE ECRAN**, ca tot restul feliei 0028. Meniul e verde la compilare și în teste headless;
  locul unde se probează rămâne bancul (`DataViewPlaygroundForm`, bifa `ShowColumnFilter` pe o
  coloană) — plus, de acum, chiar suprafața de proiectare din Visual Studio.
- **Culorile pe schema întunecată n-au fost văzute.** Regulile pe tip există și se aplică, dar
  bifele desenate de sistem într-un `CheckedListBox` sub o paletă închisă sunt exact genul de lucru
  care se judecă privind, nu citind.
- **Navigarea cu tastatura s-a schimbat**: nu mai există rândul „survolat” desenat de noi, ci
  ordinea de TAB a controalelor (Esc și Enter au rămas la fel, prin `KeyPreview`). E comportamentul
  standard Windows, dar e ALT comportament față de 0028-03.
- Meniul de condiții (`CustomPopup`) și dialogul de operanzi au rămas neatinse.
