# SLICE-0010-03 — KBotDataView: restul tipurilor de coloană

A treia trecere din slice-ul 0010. Completează cele șase tipuri de coloană: peste **Text** și
**CheckBox** (livrate în 0010-02) se adaugă **Combo**, **OptionButton**, **Button** și
**ProgressBar**, plus semantica de date a grupurilor de opțiuni.

## Ce s-a schimbat și de ce

### Model
- `KBotDataColumn.OptionGroup As String` — grupul de exclusivitate pentru coloanele
  `OptionButton`. Grup vid => opțiune independentă.

### Semantica grupurilor de opțiuni (date, nu input)
`KBotDataView.SetOptionValue(colKey, rowIndex, value)`: bifarea (True) stinge celelalte
opțiuni **din același rând** care au **același `OptionGroup`**. Nu atinge alte rânduri, nu
atinge alte grupuri, iar opțiunile fără grup rămân independente. Cheie necunoscută sau coloană
de alt tip => `ArgumentException` (fără no-op tăcut). Cablarea pe click/Space vine în 0010-05.

### Pictare (stare de afișare)
- **Combo** — textul formatat + un chevron `TextDim` în dreapta; zona de text se micșorează
  cu lățimea chevronului. Editorul real (ComboBox flotant) apare abia la editare (0010-06).
- **OptionButton** — elipsă + punct central (geometria din `AdvancedTreeControl`, culorile din
  paletă: `Border` / `Accent` / `AccentText`).
- **Button** — față rotunjită + chenar + etichetă centrată (`ButtonBack`/`ButtonBorder`/
  `ButtonText`). Eticheta = textul celulei, iar dacă lipsește, `HeaderText`-ul coloanei (cazul
  uzual: o coloană «Detalii» cu același buton pe fiecare rând). Stările hover/pressed vin în
  0010-05, odată cu urmărirea mouse-ului.
- **ProgressBar** — șină + umplere proporțională (`Accent`), scalată pe `ProgressMin`/
  `ProgressMax`. Sub o lățime egală cu înălțimea, colțul rotunjit degenerează, deci se umple
  drept. Textul procentual (opțional în plan) NU s-a implementat.

Toate culorile vin din paletă — zero literale.

## Fișiere atinse

- `src/KBot.Controls/KBotDataColumn.vb` — `OptionGroup`.
- `src/KBot.Controls/KBotDataView.vb` — `SetOptionValue` + `ClearOptionSiblings`.
- `src/KBot.Controls/KBotDataView.Theming.vb` — culori + resurse GDI pentru cele patru tipuri noi.
- `src/KBot.Controls/KBotDataView.Painting.vb` — `DrawOptionCell`, `DrawButtonCell`,
  `DrawProgressCell`, `DrawComboCell`, `ProgressFraction`, `ToDouble` + dispatch-ul pe tip.
- `tests/KBot.Controls.Tests/KBotDataViewColumnTypeTests.vb` (nou, 9 teste).
- `src/KBot.DevHarness/Internal/DataViewHarnessForm.vb` — datele sintetice conțin acum câte o
  coloană din fiecare tip (acceptanța pasului).
- `docs/worklog/KBOT_STATUS.md`

## Rezultate teste

- `dotnet build KBot.sln -c Debug` — **0 warnings / 0 errors**.
- `dotnet test KBot.sln` — **113 passed / 0 failed / 0 skipped** (29 în `KBot.Controls.Tests`).
- Acoperire nouă: exclusivitatea `OptionGroup` (același grup / alt grup / alt rând / fără grup /
  coloană de tip greșit), scalarea + limitarea `ProgressFraction` (inclusiv interval degenerat
  `Min = Max`, care NU împarte la zero), și o pictare cu **toate cele șase tipuri** simultan,
  care confirmă că `OnPaint` chiar a desenat rândul (`DebugLastPaintedDataRows = 1`).

## Rămas neverificat / amânat

- **Verdictul vizual uman tot NU a fost dat.** Harness-ul (DevHarness → Controls/UI →
  «KBotDataView — virtualizare + temă») afișează acum toate cele șase tipuri; cum arată
  chevronul, butonul și bara de progres **rămâne neconfirmat până când cineva îl rulează**.
  Testele dovedesc doar că pictarea nu aruncă și că matematica e corectă.
- Hover/pressed pe celulele `Button` — în 0010-05 (are nevoie de urmărirea mouse-ului).
- Textul procentual pe `ProgressBar` — neimplementat (opțional în plan).
- `KBotDataColumn.Frozen` rămâne metadata neutilizată (autoritar e `FrozenColumnCount`).
- Urmează: 04 formatare+disable, 05 input+selecție, 06 editare.
