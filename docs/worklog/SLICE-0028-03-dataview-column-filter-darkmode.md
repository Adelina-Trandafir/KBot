# SLICE-0028-03 — DataView: filtrul de coloană în stil Access + suprascrierea pe întuneric

Al treilea pas al feliei 0028 (`KBotDataView`). Două cereri ale operatorului:

> 1. A property called `ShowColumnFilter` as an icon with exposed props: Size, Icon and HoverColor.
>    It should replicate the Access Filtering system on the native DataSheets (different options for
>    Number, String and Dates). When pressed a custom popup menu should appear which will allow the
>    user to filter the dgv. Also, like in Access the first options should be for Sorting. Should use
>    the Custom Theming System.
> 2. The DGV should also use the theming system, but only for DarkMode. If DarkMode, it should ignore
>    the colors set by me manually in the designer and set the normal DarkMode colors for the DGV
>    (header, footer and cells). Also the scrollbars should use the Theming System.

Trei decizii au fost puse operatorului înainte de a scrie cod (răspunsurile lui în paranteze):
cine filtrează efectiv rândurile (**grila însăși**), cât de fidel e meniul față de Access
(**paritate completă**, cu submeniu de condiții) și cum se colorează barele de derulare
(**bare native + tema întunecată a Windows-ului**, nu un control desenat de noi).

---

## 1. Motorul de sortare și filtrare (pur)

Patru tipuri noi, fără nicio referință la pictare sau la vreun control — aici scrie ce înseamnă
„mai mic” și „conține”, o singură dată, pentru meniu, pentru dialog și pentru potrivire:

| Fișier | Rol |
|---|---|
| `KBotSortDirection.vb` | `None` / `Ascending` / `Descending`. `None` **nu** e „ascendent implicit”: e ordinea de încărcare, la care o vedere care primește rândurile deja ordonate de server trebuie să se poată întoarce. |
| `KBotFilterOperator.vb` | Vocabularul submeniurilor Access: Egal / Diferit / Conține / Începe cu / Se termină cu / Mai mic / Mai mare / Între / Este (ne)completat. |
| `KBotFilterEngine.vb` | Comparația tipizată, potrivirea condiției, oferta pe tip (`AllowedOperators`), textele de meniu. |
| `KBotColumnFilter.vb` | Filtrul unei coloane: lista de valori bifate **ȘI** condiția, aplicate împreună. |

Reguli care nu sunt evidente și de aceea sunt scrise în cod:

- **Două feluri de valoare, dinadins.** Condițiile de TEXT se potrivesc pe textul **afișat** (ca
  operatorul să caute ce vede), cele de mărime pe valoarea **brută**, citită în tipul coloanei.
  Altfel «mai mic decât 10» ar fi alfabetic, unde «9» e mai mare decât «10».
- **Un operand ilizibil face condiția INERTĂ, nu goală.** «Mai mic decât <text>» arată tot, nu
  nimic: o grilă golită arată ca un bug, un filtru care n-a înțeles ce i s-a scris nu.
- **Goalele întâi, iar necitibilele la urmă.** `Nothing`, `""` și spațiile sunt aceeași stare. Pe o
  coloană numerică, o valoare care se citește ca număr stă mereu înaintea uneia care nu se citește
  — a le declara egale ar face sortarea instabilă tocmai pe rândurile ciudate, adică pe cele
  căutate.
- **`SelectedValues = Nothing` ≠ mulțime goală.** Primul înseamnă «toate», al doilea «niciuna».
- **Coloanele logice n-au submeniu de condiții** (`AllowedOperators` întoarce gol): două căsuțe în
  listă spun deja tot ce se poate spune despre o bifă.

## 2. Harta vedere ↔ model (`KBotDataView.Filtering.vb`)

Aceasta e schimbarea cu adevărat invazivă, fiindcă până acum „indexul rândului” însemna un singur
lucru. Acum sunt două numerotări, și **regula e că nu se amestecă niciodată**:

- **API-ul public vorbește în indici de MODEL** — `Item(cheie, index)`, `CurrentRowIndex`,
  `CellClick`, `EnsureVisible`, `IsRowEnabled` înseamnă azi exact ce însemnau înainte. Vederile
  existente (Sumar, Istoric, Plăți…) își țin propriii indici de rând; o filtrare care i-ar
  renumerota le-ar corupe în tăcere. Un test dedicat păzește tocmai asta.
- **Geometria lucrează în poziții de VEDERE** — `RowTop`, `FirstVisibleRow`, virtualizarea,
  hit-testul. `RowAtPoint` traduce o dată, la intrare; `RowTopForModel` traduce în sens invers
  pentru celule/editare/etichetă.

Alte două alegeri:

- **Reconstrucție LENEȘĂ** (`_viewDirty` + `EnsureView`), nu pe loc. O încărcare în masă între
  `BeginUpdate`/`EndUpdate` altfel ar lăsa harta în urma rândurilor, iar o pictare căzută la mijloc
  ar indexa în gol — genul de excepție care apare o dată la o mie de încărcări și nu se reproduce.
  De aceea `RecomputeFooter` s-a redenumit **`RecomputeDerived`** și marchează harta murdară
  ÎNAINTEA gărzii de `BeginUpdate`.
- **Sortare stabilă**: `List.Sort` nu e stabilă, deci comparatorul cade la egalitate pe indexul de
  model. Fără asta, două rânduri egale și-ar schimba locul la fiecare resortare.

Ce s-a mutat pe rândurile vizibile, și de ce:

| Loc | Înainte | Acum |
|---|---|---|
| Agregatele din subsol | toate rândurile | doar cele care trec de filtre — un total care nu iese la adunare pe ecran e, pentru cine citește pagina, o greșeală de calcul |
| Înălțimea de derulat | `_rows.Count` | `ViewCount()` |
| Eșantionul de auto-dimensionare | primele N rânduri din model | primele N **vizibile** |
| Dungile alternante | index de model | poziție de vedere (rămân alternante după filtrare) |
| Săgețile sus/jos | pas în model | pas în vedere („rândul desenat sub acesta”) |
| `KBotAggregate.Last` | ultimul din model | ultimul de pe ecran (sub o sortare nu mai e același) |

Selecția: dacă rândul curent e filtrat afară, se abandonează editarea deschisă și selecția cade
(`DropSelectionIfHidden`) — o selecție pe un rând invizibil ar muta săgețile și editarea în gol.

## 3. Butonul de filtrare — **pe coloană** (`KBotDataColumn` + `KBotDataView.FilterIcon.vb`)

Prima variantă a feliei avea steagul pe GRILĂ (`KBotDataView.ShowColumnFilter`) plus un comutator
de excludere pe coloană. **Operatorul a cerut invers**: se hotărăște COLOANĂ CU COLOANĂ, în
designer. Cele patru proprietăți stau acum pe `KBotDataColumn`, lângă celelalte pictograme de antet
ale coloanei — `ShowColumnFilter` (implicit False), `ColumnFilterIcon`, `ColumnFilterIconSize`
(16×16), `ColumnFilterHoverColor` — toate cu perechea `ShouldSerialize`/`Reset`, verificată în teste
**prin `TypeDescriptor`**, calea pe care o ia chiar Visual Studio. `Filterable` și
`FilterIconReserve` au DISPĂRUT: coloana știe acum totul singură, deci grila n-are ce să-i mai
împingă.

**Interzis pe `Button` și `ProgressBar`.** Acelea nu poartă o valoare pe care s-o cauți — o celulă
buton arată o comandă, una de progres o fracțiune desenată; o listă de valori distincte peste ele ar
fi o listă de nimic, iar «sortează A → Z» n-ar avea ce ordona. Aprinderea **ARUNCĂ**, nu se stinge
tăcut: un buton care nu apare unde a fost cerut e chiar no-op-ul tăcut interzis de regula casei.
Perechea se apără din **amândouă** direcțiile (inclusiv la schimbarea tipului sub un filtru deja
aprins), cu aceeași amânare în `BeginInit`/designer ca perechea `ValueType × Aggregate` — designerul
emite proprietățile în ordinea LUI, iar o excepție în `InitializeComponent` ar închide formularul cu
totul — și cu verificarea finală în `ValidateSettled`, la `EndInit`. `FilterEnabled` (steag ȘI tip)
e ce citesc pictarea și hit-testul, ca în designer să nu se deseneze un buton pe care `EndInit`
apoi îl refuză.

- **Stă mereu în același loc** — capătul din dreapta al celulei de antet — iar `HeaderRightIcon` se
  mută la stânga lui. Niciunul dintre cele două nu se sacrifică la îngustare: amândouă se apasă.
- **Podeaua de lățime**: `HeaderIconsWidth` numără acum trei piese și spațiile dintre ele, direct
  din starea coloanei.
- **Fără imagine dată, se DESENEAZĂ**: o pâlnie trasată din culoarea temei, **plină și în accent
  cât timp coloana chiar e filtrată**, doar conturată altfel. Așa antetul spune dintr-o privire
  care coloane sunt filtrate — jumătate din rostul semnului — fără nicio resursă imagine.

`SumarView.Designer.vb` fusese deja autorat în designerul VS cu forma veche
(`grid.ShowColumnFilter = True` + `grid.ColumnFilterIcon = My.Resources.Resources.filter`); a fost
mutat pe cele șapte coloane ale vederii, păstrând exact comportamentul autorat.

## 4. Meniul (`DataView/Filter/`)

`KBotFilterPopup` (+ `.Painting`) e o FEREASTRĂ desenată de noi, ca `CustomPopup` și din același
motiv: un `ContextMenuStrip` cu un `CheckedListBox` în el ar rămâne două dreptunghiuri albe sub o
schemă întunecată. Cele patru etaje Access, în ordinea Access:

1. sortarea (crescător / descrescător), cu textul luat din tip — «A → Z» pe text, «de la mic la
   mare» pe numere, «de la vechi la nou» pe date — și cu un semn pe sensul activ;
2. «Șterge filtrul din «Coloană»», stins cât timp coloana n-are filtru;
3. submeniul de condiții («Filtre text / numerice / de dată»), care deschide
   `KBotFilterConditionDialog` (dialog modal, cu `.Designer.vb` propriu, conform regulii casei);
4. caseta de căutare + lista de valori bifate cu «(Selectează tot)», apoi OK / Anulează.

Alegeri deliberate:

- **Sortarea se aplică IMEDIAT și închide meniul; filtrul se predă abia la OK.** Sortarea e o
  comandă, nu o alegere de confirmat — exact ca în Access. Popup-ul lucrează pe o COPIE a
  filtrului (`Clone`), deci «Anulează» și Esc nu lasă nimic în urmă.
- **«Toate bifate» = nicio restricție** (`SelectedValues` rămâne `Nothing`), altfel filtrul ar
  rămâne activ pentru totdeauna și antetul ar arăta coloana ca filtrată degeaba.
- **Lista distinctă se calculează peste rândurile care trec de filtrele CELORLALTE coloane**, dar
  ignoră filtrul coloanei ÎNSEȘI: altfel, odată bifată o valoare, celelalte ar dispărea din listă
  și nimeni nu s-ar mai putea răzgândi.
- **Golul are eticheta lui**, «(Necompletate)», dar în model rămâne textul vid — o etichetă
  strecurată în model ar filtra la fel și o coloană care chiar conține textul acela.
- Singurul control-copil adevărat e caseta de căutare (`KBotTextField`, `IThemedControl`), fiindcă
  text tastat cere un control care știe să primească taste. Popup-ul e el însuși `IThemedControl`,
  deci traversarea temei nu-i calcă interiorul — regula care a mușcat deja de două ori.
- `_suppressDeactivate` cât timp submeniul e sus: pierderea activării atunci nu înseamnă «s-a dat
  clic în altă parte», înseamnă «se uită la copil».

Grila expune și `ColumnFilterOpening` (anulabil) plus `ShowColumnFilterMenu(cheie)`, pentru o gazdă
care vrea să lege comanda de un buton propriu sau să oprească meniul (o vedere care filtrează pe
server).

## 5. Întunericul bate designerul + barele de derulare

`KBotDataView.Theming.vb`:

- Sub o schemă cu `IsDark`, `HeaderBackResolved` / `HeaderForeResolved` / `FooterBackResolved` /
  `FooterForeResolved` **ignoră culorile fixate în designer** și întorc culorile paletei
  (`DarkOverridesDesignerColors`). Regula obișnuită a casei e inversă — «cine a pus explicit o
  culoare câștigă» — și **rămâne inversă pe schemele luminoase**. Excepția e cerută de ce se
  întâmplă altfel: paleta de designer se autorează pe fundal deschis, iar o bandă lăsată albă peste
  un corp devenit aproape negru nu e „alegerea operatorului respectată”, e o grilă imposibil de
  citit. **Culorile nu se pierd**, doar se ignoră: ieșirea din schema întunecată le dă înapoi.
- Se aplică numai CULORILOR. **Un font fixat rămâne al operatorului în orice schemă** — un font nu
  devine ilizibil pe fundal închis.
- Celulele luau deja culorile din paletă în orice schemă (`_cRowBack` / `_cCellText`), deci acolo
  n-a fost nimic de schimbat.
- Barele de derulare: `SetWindowTheme` cu «DarkMode_Explorer» / «Explorer», prin
  `NativeMethods.ApplyWindowTheme` — același truc pe care ThemeManager îl folosește deja pentru
  liste și pentru `KBotComboBox`. Se re-aplică la `OnHandleCreated`, fiindcă apelul cere un handle.

## 6. Bug găsit pe drum: tema nu ajungea în NICIO vedere

Raportat de operator: «even though in the Harness Form the color scheme gets applied, in the
SumarView it doesn't». Nu e o problemă a lui SumarView, e una a motorului de teme, și lovea toate
cele șase vederi reale deodată.

`ThemeManager.Traverse` se OPREA la primul `IThemedControl`:

```vb
If themed IsNot Nothing Then
    themed.ApplyTheme(_current)
    Return          ' ← și copiii lui nu mai vedeau niciodată schema
End If
```

Oprirea avea un motiv bun — regulile GENERICE pe tip n-au voie să intre în interiorul unui control
auto-tematizat (ar repicta suprafața ca pe un `Panel` și ar strica `TextBox`-ul intern al lui
`KBotTextField`). Dar **toate cele șase vederi reale sunt ele însele `IThemedControl` ȘI țin un
`KBotDataView` înăuntru** (`Controls.Add(grid)`, direct pe vedere), deci grila din fiecare vedere nu
primea schema de la nimeni și rămânea pe culorile ei implicite. Pe banc, unde aceeași grilă stă
direct pe formular, se colora corect — de aceea simptomul arăta ca o problemă a vederii.

Comentariul din `SumarView.ApplyTheme` chiar spunea premisa pe dos («grila NU se atinge aici:
`ThemeManager.Traverse` ajunge la el»), ceea ce a ținut bug-ul ascuns.

**Reparat la nivelul potrivit**, nu în fiecare vedere: `ThemeManager.ApplyToNestedThemed` coboară
prin subarborele unui control auto-tematizat și PREDĂ schema doar altor `IThemedControl`, fără să
aplice vreo regulă pe tip. Contractul vechi rămâne întreg — `TextBox`-ul intern al lui
`KBotTextField` nu e `IThemedControl`, deci nu e atins. Un apel copiat în fiecare `ApplyTheme` de
vedere ar fi mers și el, dar s-ar fi uitat exact la a șaptea vedere.

Patru teste noi (`NestedThemedControlTests`) fixează ambele jumătăți: copilul și nepotul tematizat
PRIMESC schema, copilul simplu NU e atins, și fiecare control e aplicat exact o dată.

## Fișiere atinse

**Noi (`src/KBot.Controls/DataView/`)**: `KBotSortDirection.vb`, `KBotFilterOperator.vb`,
`KBotFilterEngine.vb`, `KBotColumnFilter.vb`, `KBotDataView.Filtering.vb`,
`KBotDataView.FilterIcon.vb`, `Filter/KBotFilterPopup.vb`, `Filter/KBotFilterPopup.Painting.vb`,
`Filter/KBotFilterConditionDialog.vb`, `Filter/KBotFilterConditionDialog.Designer.vb`.

**Modificate (`src/KBot.Controls/DataView/`)**: `KBotDataColumn.vb` (cele patru proprietăți de
filtrare, `FilterEnabled`, interdicția pe Button/ProgressBar din ambele direcții,
`HeaderIconsWidth` pe trei piese), `KBotDataView.vb`, `.HeaderIcons.vb` (slotul de filtru în
`ComputeHeaderCellLayout`), `.Layout.vb` (poziții de vedere, `RowTopForModel`, `EnsureVisible`),
`.Painting.vb` (`DrawRow` pe poziție de vedere), `.Input.vb` (`RowAtPoint`, `MoveRow`, hover/click
de filtru), `.Footer.vb` (`RecomputeDerived`, agregate pe `ViewRows`), `.AutoSize.vb`,
`.Editing.vb`, `.Tooltip.vb`, `.Theming.vb`.

**Motorul de teme**: `src/KBot.Theming/ThemeManager.vb` (`ApplyToNestedThemed`).

**Aplicația**: `src/KBot.App/Views/SumarView.vb` (comentariul care spunea premisa pe dos),
`src/KBot.App/Views/SumarView.Designer.vb` (filtrul mutat de pe grilă pe cele șapte coloane).

**Bancul de probă**: `DataViewPlaygroundForm(.Designer).vb` — `ShowColumnFilter` în inspectorul de
coloană (stins pentru Button/ProgressBar, ca bancul să arate regula, nu s-o descopere printr-o
excepție) și butonul `ClearAllFilters()`.

**Versiuni**: `KBot.Controls` 1.13.0.0 → **1.14.0.0**; `KBot.Theming` 1.5.0.0 → **1.6.0.0**;
`KBot.App` 1.0.12.0 → **1.0.13.0**; `KBot.DevHarness` 1.0.16.0 → **1.0.17.0**.

**Teste noi**: `KBotFilterEngineTests.vb`, `KBotDataViewFilteringTests.vb`,
`KBotDataViewFilterIconTests.vb`, `KBotFilterPopupTests.vb`,
`tests/KBot.Theming.Tests/NestedThemedControlTests.vb`. Modificate:
`KBotDataViewBandThemingTests.vb` (vezi mai jos) și `tests/KBot.App.Tests/DdfViewTests.vb`
(`RunSta` folosește acum `ExceptionDispatchInfo.Capture(...).Throw()` în loc de `Throw failure` —
altfel fiecare eșec din firul STA se raporta ca o excepție fără loc, ceea ce a costat un drum în
plus la diagnosticarea eșecurilor preexistente de mai jos).

## Rezultatele testelor

```
dotnet build KBot.sln          → 0 erori, 0 avertismente noi
                                 (1 avertisment MSB3825 pe DdfView.resx, preexistent)
KBot.Controls.Tests            → 685 / 685  (era 627 înainte de felie, +58)
KBot.DevHarness.Tests          → 170 / 170
KBot.Theming.Tests             →  65 /  65  (era 61, +4 pentru bug-ul de traversare)
KBot.Common.Tests / Xfa.Tests  →  14 / 14, 39 / 39
```

Meniul se probează headless prin porțile `Debug*` (convenția casei): construcția + așezarea
(`DebugMeasure`) prind orice cădere în `Recalc`, iar `BuildFilter` face verificabilă regula «tot
bifat ≠ filtru» fără să deschidă vreo fereastră.

### Eșecuri PREEXISTENTE, neatinse de felie

`KBot.Domain.Tests` (3), `KBot.Api.Tests` (1) și `KBot.App.Tests` (8) au eșecuri care **nu vin de
aici**. Verificat, nu presupus:

- Cele din `Domain`/`Api` (`EtichetaRevizie*`) eșuează identic pe arborele CURAT, la HEAD, cu felia
  scoasă cu `git stash`.
- Din cele 8 din `App.Tests`, 2 eșuează la HEAD, iar restul de 6 vin din rescrierea **în lucru** a
  lui `DdfView` din arborele de lucru: testele ajung la handler prin reflecție
  (`GetMethod("tree_NodeMouseUp")`), iar declarația aceea nu mai există în `DdfView.vb` (e la HEAD
  linia 622, în arborele de lucru a rămas doar șirul din `GlobalErrorLog.Write`) — `GetMethod`
  întoarce `Nothing` și `m.Invoke` dă `NullReferenceException`.

**Un test existent a fost REscris, nu reparat**:
`KBotDataViewBandThemingTests.AColourSetByTheOperator_SurvivesAThemeSwitch` verifica exact regula
pe care cererea 2 o răstoarnă — folosea schema **Dark** ca să demonstreze că o culoare de designer
supraviețuiește. Acum se numește `…SurvivesASwitchBetweenLIGHTSchemes` și folosește Modern, iar
regula nouă are trei teste proprii (suprascriere sub întuneric, culorile date înapoi la ieșire,
fontul lăsat în pace).

## Rămase neverificate / amânate

- **NIMIC DIN FELIA ASTA N-A FOST VĂZUT PE ECRAN.** Meniul, pâlnia, bifele, dialogul de condiție și
  barele întunecate sunt verde la compilare și la teste headless, atât. Bancul de probă
  (`DataViewPlaygroundForm`, bifa «ShowColumnFilter») e locul unde se verifică — este singurul pas
  care rămâne, și e cel care a găsit bug-uri în fiecare felie de control de până acum.
- **Barele de derulare urmează doar perechea întuneric/lumină.** `SetWindowTheme` aduce griul
  Windows-ului, nu culorile paletei: sub o schemă colorată barele rămân cele de sistem, iar accentul
  nu ajunge niciodată pe ele. Varianta completă e un `KBotScrollBar` owner-drawn (folositor și
  arborelui, și popup-ului) — nefăcut aici, la alegerea operatorului.
- **O schemă personalizată cu `IsDark = True` ȘI `PreserveDesignerColors = True`** ar readuce
  culorile de designer după `ApplyTheme`, prin `DesignerBaseline.Restore` din `ThemeManager`.
  Niciuna dintre cele patru scheme compilate nu combină cele două steaguri (Colorful e singura cu
  `PreserveDesignerColors`, și e luminoasă), dar editorul de teme le poate combina.
- **Filtrarea nu s-a rulat peste date reale.** Toate testele merg pe rânduri construite în memorie;
  nicio vedere (Sumar / Istoric / Plăți / DDF) n-a fost încă trecută pe `ShowColumnFilter`.
- **Sortarea e pe O SINGURĂ coloană**, ca în foaia de date Access. Sortarea pe mai multe coloane
  n-a fost cerută și nu e implementată.
- Butoanele de sortare din Istoric și din arbore (`MsgBox` substituent) rămân neatinse — sunt un
  fir deschis mai vechi, nu al acestei felii.

## Fir deschis creat de felie

Vederile existente pot aprinde acum `ShowColumnFilter = True` fără altă modificare (grila
filtrează singură, iar indicii de rând nu-și schimbă înțelesul). Care dintre ele o face, și dacă
subsolul lor trebuie să arate «filtrat din N rânduri», e o decizie de operator — nu s-a luat aici.
