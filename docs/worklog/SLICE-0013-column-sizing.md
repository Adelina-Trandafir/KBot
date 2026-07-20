# SLICE-0013 — KBotDataView: auto-dimensionarea coloanelor + moduri de umplere

Lățimile coloanelor veneau până acum exclusiv din ce pasa caller-ul la `AddColumn` (sau dintr-un
drag manual). Slice-ul adaugă două comportamente grid-wide, aplicate ca O SINGURĂ trecere în
`UpdateLayout`, înaintea offset-urilor și a barelor de derulare:

1. **`AutoSizeColumnsMode`** (`KBotAutoSizeMode`) — `None` sau `ToContent` (implicit). `ToContent`
   măsoară fiecare coloană vizibilă la conținutul ei (max între antet și celulele eșantionate) și
   o limitează la `[MinWidth, MaxWidth]`.
2. **`ColumnFillMode`** (`KBotFillMode`) — `None` (implicit), `FirstColumn`, `LastColumn`,
   `Proportional`. Aplicat DUPĂ auto-dimensionare, peste spațiul rămas (sau lipsă).

Cele patru cazuri din brief:

| Caz | Setări | Rezultat |
|---|---|---|
| 1 — fit la conținut | `ToContent` | fiecare coloană cât textul ei cel mai lat |
| 2 — cheltuie restul | `ToContent` + `First/Last/Proportional` | fără bandă goală în dreapta |
| 3 — doar manual | `None` + `None` | lățimile caller-ului rămân neatinse |
| 4 — depășire | orice + `None` => bară orizontală; orice + mod de umplere => coloanele se micșorează, bara nu apare |

## Regulă nouă de limbă

**Toate comentariile de cod adăugate/atinse în acest slice sunt în ENGLEZĂ** (marcate „English
(slice 0013)”). Restul `KBot.Controls` rămâne comentat în română — NU s-a făcut conversie în masă;
doar codul nou/atins urmează regula. Mesajele de eroare și textul UI rămân în română.

## Ce s-a schimbat și de ce

### Model (`KBotDataColumn`)
- **`MaxWidth`** (implicit `Integer.MaxValue`, neplafonat). Ținut mereu ≥ `MinWidth`; coborârea lui
  re-limitează `Width`.
- **`Width`** re-limitat la `[MinWidth, MaxWidth]` la FIECARE scriere — invarianta trăiește în model,
  deci trecerea de auto-size/fill/shrink poate atribui liber și lasă setter-ul să impună marginile.
- **`UserSized`** (`Friend`) — ridicat de un drag de operator; `ToContent` sare peste astfel de
  coloane, dar fill/shrink li se aplică în continuare. Golit de `ResetColumnSizing`.
- Capcana VB de case-insensitivity: proprietățile noi nu au parametri omonimi în setter/ctor, deci
  nu era nevoie de `Me.`; există totuși regresie pentru `MaxWidth`/`UserSized` (round-trip + clamp).

### Trecerea (`KBotDataView.AutoSize.vb`, nou)
- `PerformAutoSize()` rulează din `UpdateLayout`: măsoară (ToContent, sărind coloanele `UserSized`)
  apoi fill/shrink. Gardă de reintrare `_inAutoLayout` + oprire dacă `_updateDepth > 0` (se rulează o
  singură dată din `EndUpdate`). Dacă `None` + `None`, iese imediat (Caz 3 = zero atingeri).
- **Lățimea disponibilă** oglindește exact ce folosește `UpdateScrollBars`
  (`ClientSize.Width − bară verticală dacă e vizibilă`), deci un mod de umplere face totalul EGAL cu
  viewport-ul și bara orizontală nu apare. Vizibilitatea barei verticale depinde doar de numărul de
  rânduri și înălțimea corpului (niciodată de lățimi), deci se decide prima — fără dependență
  circulară.
- **Măsurare**: `TextRenderer.MeasureText` cu aceleași fonturi ca pictorul (antet semibold, corp
  ambient). Text/Combo/Button măsoară șirul FORMATAT (aplică `FormatString`); Combo adaugă zona de
  chevron; CheckBox/OptionButton folosesc cutia glifului + padding; ProgressBar păstrează lățimea
  caller-ului (antetul tot participă). Padding-ul e cel din pictură (6px corp, 8px antet, DPI-scalat).
- **Proporțional**: `extra(i) = leftover · width(i) / total`; restul întreg din diviziune merge INTEGRAL
  la ultima coloană (totaluri exacte, fără gol de 1–2px la dreapta). O coloană plafonată de `MaxWidth`
  își pasează surplusul celor neplafonate într-o SINGURĂ trecere suplimentară (nu buclă la convergență).
- **Micșorare** (depășire + mod de umplere): scade deficitul din coloanele încă peste `MinWidth`,
  proporțional cu lățimea curentă, cu podea la `MinWidth`. Bucla e mărginită (fiecare rundă fie
  converge, fie fixează încă o coloană la podea — cel mult `nrColoane` runde).

### Integrare
- `UpdateLayout` cheamă `PerformAutoSize` înaintea `RecalcColumnLayout`/`UpdateScrollBars`.
- `ApplyTheme` și `OnFontChanged` cheamă `UpdateLayout` (schimbarea fontului schimbă măsurarea).
- Drag-ul de coloană (`OnMouseMove`) ridică `UserSized` pe coloana trasă.
- Metode publice: `AutoSizeColumns()` (forțează o trecere) și `ResetColumnSizing()` (golește
  `UserSized` pe toate + re-măsoară). Setter-ele celor trei proprietăți declanșează o trecere.

## Fișiere atinse

- `src/KBot.Controls/KBotAutoSizeMode.vb`, `KBotFillMode.vb` (noi — enum-uri)
- `src/KBot.Controls/KBotDataView.AutoSize.vb` (nou — trecerea)
- `src/KBot.Controls/KBotDataColumn.vb` — `MaxWidth`, `UserSized`, clamp `Width`/`MinWidth`
- `src/KBot.Controls/KBotDataView.Layout.vb` — `PerformAutoSize` în `UpdateLayout`
- `src/KBot.Controls/KBotDataView.Theming.vb` — `UpdateLayout` în `ApplyTheme`/`OnFontChanged`
- `src/KBot.Controls/KBotDataView.Input.vb` — `UserSized = True` la drag
- `tests/KBot.Controls.Tests/KBotDataViewAutoSizeTests.vb` (nou, 15 teste)
- `tests/KBot.Controls.Tests/KBotDataModelTests.vb` — 4 teste (`MaxWidth`/`UserSized`)

## Rezultate teste

- `dotnet build KBot.sln -c Debug` — **0 warnings / 0 errors**.
- `dotnet test KBot.sln` — **toate verzi** (93 în `KBot.Controls.Tests`, restul neschimbat).
- Poarta de virtualizare rămâne validă: numărul de rânduri pictate NU depinde de `RowCount`
  (auto-size trăiește în `UpdateLayout`, nu în calea de pictare, și eșantionează cel mult
  `AutoSizeSampleRows` rânduri).

## Compromisuri și limitări (de reținut)

- **Eșantionare (`AutoSizeSampleRows`, implicit 200; 0 = toate):** măsurarea a 5.000×20 celule ar
  fi ~100k măsurători și ar rupe promisiunea O(viewport). Se măsoară antetul + primele
  `AutoSizeSampleRows` rânduri; o valoare mai lată mai jos va fi tăiată cu «…». Compromis deliberat —
  cine vrea exactitate pe grile mici setează 0 (Sumar are ~10 rânduri, deci 0 e gratis acolo).
- **`CellFormatting` NU se ridică la măsurare** (ar fi scump și reintrant). Consecință: un handler
  care rescrie `Text` poate produce un șir mai lat decât lățimea măsurată, care va ellipsiza.
- **Fallback-ul onest la depășire:** dacă `sum(MinWidth) > available`, coloanele nu încap. Se
  micșorează tot la `MinWidth` și se arată TOTUȘI bara orizontală — textul care dispare complet e mai
  rău decât o bară pe care caller-ul n-a cerut-o.
- **Fără back-reference coloană→grilă:** `KBotDataColumn` e un model „prost”, fără pointer la control,
  deci o schimbare programatică de `Width`/`MinWidth`/`MaxWidth`/`Visible`/`FormatString` DUPĂ încărcare
  nu declanșează singură o trecere. `AddColumn` declanșează; drag-ul declanșează; în rest caller-ul
  cheamă `AutoSizeColumns()` (sau se prinde la următorul eveniment de layout — resize/temă/EndUpdate).
  În practică coloanele se configurează complet imediat după `AddColumn`, deci punctele de declanșare
  reale sunt acoperite.

## Verificare în harness (playground)

S-a adăugat proba `KBotDataView — proprietăți runtime (playground)` (Controls/UI): grilă + panou
care comută LIVE fiecare proprietate runtime (auto-size / fill / eșantion / frozen / înălțimi /
antet / alternante / read-only) + inspector de coloană (Visible/Enabled/ReadOnly/Width/Min/Max) +
comutator de rânduri (12/200/5000, cu o valoare foarte lată la rândul 300 ca să se vadă efectul
eșantionării). Trei fișiere noi în `KBot.DevHarness` (`Tests/KBotDataViewPlaygroundTest.vb`,
`Internal/DataViewPlaygroundForm.{vb,Designer.vb}`).

**Bug găsit și reparat la rulare (preexistent din 0010-05):** `ReadOnlyGrid` NU se aplica la
comutarea bifelor / opțiunilor — `ActivateCell` verifica doar `IsCellEnabled`, nu și read-only.
Fix: o comutare de valoare (CheckBox/OptionButton) e blocată când `ReadOnlyGrid` SAU
`Column.ReadOnly` e adevărat (același contract ca editarea text/combo din `CanEdit`); un **Button
rămâne activ** (e acțiune pură, fără valoare/dirty). 4 teste noi în `KBotDataViewInputTests.vb`
(97 în `KBot.Controls.Tests`). Butonul de comandă ERA deja clicabil (ridica `ButtonClick`) — lipsea
doar feedback-ul în playground; s-a adăugat un handler care scrie în info + log.

## Rămas / în afara scopului

- Override per-coloană de `AutoSizeMode`, auto-dimensionarea rândurilor, word-wrap în celulă,
  reordonarea coloanelor, re-măsurarea la fiecare scroll — toate în afara scopului.
- **Verdict vizual: ACCEPTAT de operator** în proba playground (Controls/UI) — vezi «Verificare în harness».
- **Follow-up Sumar (nu în acest slice):** `SumarView` ar trebui să seteze
  `AutoSizeColumnsMode = ToContent` + `ColumnFillMode = LastColumn`/`Proportional` și să renunțe la
  lățimile hardcodate din `BuildColumns` (coloana Partener se scoate separat).
