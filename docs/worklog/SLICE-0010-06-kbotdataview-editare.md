# SLICE-0010-06 — KBotDataView: editare in-place (ultima trecere)

Ultima trecere din slice-ul 0010 — motivul pentru care grila e NELEGATĂ de date. Peste celula
activă plutește UN SINGUR editor real (TextBox sau ComboBox, declarați în Designer încă din
0010-01), deci numărul de handle-uri rămâne constant indiferent de câte mii de rânduri sunt.

## Ce s-a schimbat și de ce

### Ciclul de editare
`BeginEdit` → (Enter / Tab / pierdere de focus / mutare de celulă / derulare) → `CommitEdit`,
sau Esc → `CancelEdit`. Cele trei sunt `Friend`, ca testele să le conducă headless (editorii
reali au nevoie de buclă de mesaje pentru focus și taste).

- **Editabilitate** (`CanEdit`, public): `Not ReadOnlyGrid AndAlso Not Column.ReadOnly AndAlso
  IsCellEnabled(...) AndAlso (tipul e Text sau Combo)`. Refolosește rezoluția de activare
  efectivă din 0010-04 — regula nu e reimplementată.
- **Intrare în editare**: F2, dublu-click. Editorul se poziționează pe dreptunghiul real al
  celulei (`CellRect`, care ține cont de banda înghețată, de ambele derulări și de antet), se
  tematizează, primește valoarea formatată și select-all.
- **Commit**: ridică `CellValidating`, care poate **respinge** (`Cancel = True` => editorul
  rămâne deschis și focalizat, valoarea din rând neatinsă) sau **corecta** valoarea (handler-ul
  rescrie `ProposedValue` — trim/normalizare/conversie de tip). Dacă trece: scrie în rând,
  marchează rândul editat, ascunde editorul, ridică `CellValueChanged`, apoi aplică navigația
  în așteptare (Enter => un rând mai jos, Tab => o coloană la dreapta).
- **Un singur editor viu**: `BeginEdit` pe altă celulă, mutarea celulei curente și derularea
  comit întâi. Dacă acel commit e respins, **mutarea nu are loc** — altfel editorul ar rămâne
  agățat peste o celulă care nu mai e a lui.

### Semantica `IsDirty` — CONTRACT SCHIMBAT față de 0010-01
Planul cere explicit: „murdar” = **editat de operator**, nu „scris programatic”. Prin urmare
`KBotDataRow.Item` **nu mai** ridică `IsDirty` (nici `KBotDataView.Item`) — scrierea prin API e
ÎNCĂRCARE de date. Steagul se ridică doar la: commit de editare, comutare de bifă, selectare de
opțiune. `GetDirtyRows()` întoarce deci exact rândurile atinse de operator, iar `ClearDirty()`
(nou, la nivel de grilă) + `KBotDataRow.MarkClean()` (per rând) resetează baseline-ul după
încărcare sau după trimiterea modificărilor la server.

**Două teste din 0010-01 au fost rescrise** ca să reflecte noul contract
(`Row_SetStoresWithoutMarkingDirty`, `Item_GetSetThroughControl_DoesNotDirtyTheRow`) — schimbare
deliberată, nu o „reparare” a testelor ca să treacă.

## Două bug-uri REALE prinse aici (aceeași capcană VB)

VB.NET e **case-insensitive**, deci un parametru ascunde o proprietate cu același nume, iar o
atribuire nekalificată nimerește parametrul, nu proprietatea. Compilatorul nu spune nimic.

1. `KBotCellValidatingEventArgs.New`: `ProposedValue = proposedValue` era un **no-op** =>
   commit-ul scria mereu `Nothing` în celulă. Prins de testele de editare.
2. `KBotDataColumn.New`: `HeaderText = If(headerText, String.Empty)` era un **no-op** =>
   **TOATE antetele de coloană erau `Nothing`** încă din 0010-01. Nicio pictare nu arunca, deci
   nici testele de virtualizare/tipuri nu-l puteau vedea — ar fi apărut ca un rând de antet gol
   la prima rulare a harness-ului.

Ambele sunt reparate cu `Me.` explicit (+ comentariu de avertizare) și au acum **teste de
regresie**. A treia instanță a aceleiași capcane a fost prinsă la compilare mai devreme, în
bucla `For Each item In col.ComboItems` (`item` se lega la proprietatea `Item` a clasei).

## Fișiere atinse

- `src/KBot.Controls/KBotDataView.Editing.vb` (nou — ciclul complet + `CellRect` + editorii)
- `src/KBot.Controls/Events/KBotCellValidatingEventArgs.vb` (nou; bug reparat)
- `src/KBot.Controls/KBotDataColumn.vb` — bug `HeaderText` reparat
- `src/KBot.Controls/KBotDataRow.vb` — `Item` nu mai murdărește; documentație de contract
- `src/KBot.Controls/KBotDataView.vb` — `ClearDirty`, `GetDirtyRows` redocumentat, `WireEditors`
- `src/KBot.Controls/KBotDataView.Input.vb` — F2, dublu-click => editare; auto-commit la mutare
- `src/KBot.Controls/KBotDataView.Layout.vb` — auto-commit la derulare
- `tests/KBot.Controls.Tests/KBotDataViewEditingTests.vb` (nou, 15 teste) + 2 teste rescrise
  + 1 test nou de regresie pe `HeaderText`
- `docs/worklog/KBOT_STATUS.md`

## Rezultate teste

- `dotnet build KBot.sln -c Debug` — **0 warnings / 0 errors**.
- `dotnet test KBot.sln` — **158 passed / 0 failed / 0 skipped** (74 în `KBot.Controls.Tests`).
- Acoperire nouă: `CanEdit` (doar Text/Combo; fals pe `ReadOnlyGrid`, coloană read-only sau
  dezactivată, rând dezactivat, celulă dezactivată din eveniment), commit (scrie + murdărește +
  ridică `CellValueChanged` cu old/new corecte), veto (editor deschis, valoare neatinsă, nimic
  murdar), coerciția valorii din handler, Esc (nimic scris, niciun eveniment), un singur editor
  viu, auto-commit la mutare, **veto care blochează mutarea selecției**, refuzul unei celule
  needitabile, commit/cancel fără editare deschisă, `ClearDirty`, și `BeginUpdate/EndUpdate`
  care lasă totul curat.

## Rămas neverificat / amânat (starea finală a slice-ului)

- **Verdictul vizual/interactiv uman NU a fost dat — pentru NICIUNA dintre cele șase treceri.**
  Nimeni n-a rulat încă harness-ul. Ce NU poate fi verificat headless: fluența derulării,
  traseul WinForms tastă→handler, drag-ul de redimensionare, poziționarea reală a editorului
  flotant peste celulă, focusul și select-all-ul, și cum arată efectiv culorile. Bug-ul cu
  antetele gol arată exact cât de necesară e proba vizuală: testele nu-l puteau prinde.
  **Rulează:** DevHarness (pornire Debug → «Nu») → Controls/UI → «KBotDataView — virtualizare
  + temă (5.000 × 20)».
- **Neimplementat din plan** (asumat, nu uitat): intrarea în editare prin simpla TASTARE a unui
  caracter (F2 și dublu-click funcționează); hover/pressed pe celulele `Button`; textul
  procentual pe `ProgressBar`.
- `KBotDataColumn.Frozen` rămâne metadata neutilizată — mecanismul autoritar e
  `FrozenColumnCount`. De curățat sau de cablat.
- În afara scopului v1 (conform planului): scrollbar-uri tematizate (rămân cele WinForms
  standard — singura gaură de tematizare), sortare/filtrare în antet, selecție multiplă,
  reordonare de coloane prin drag, orice fel de data binding.
- Urmează: slice-ul **Sumar**, care consumă grila în mod read-only (`ReadOnlyGrid = True`).
