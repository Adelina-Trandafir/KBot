# SLICE-0031-03 — `KBotChipBar`: bara de jetoane, fratele multi-select al lui `KBotNavList`

A treia trecere din felia 0031, după `docs/PLAN_LogViewer.md` §7. Un singur control nou, în
`src/KBot.Controls/ChipBar/`, conform regulii casei («fiecare control K-BOT are folderul lui»).

Ordinea trecerilor a fost schimbată de operator: 0031-03 (controlul) și 0031-04 (fereastra) s-au
făcut împreună, înaintea lui 0031-02 (jurnalele de server). Motivul e cererea explicită — «i want to
see the log form on press on optionbutton in the capbar» — iar fereastra nu se poate face fără
jetoane. **0031-02 rămâne nefăcută**, cu urmările scrise în `SLICE-0031-04`.

---

## Ce s-a schimbat și de ce

### 1. Trei fișiere, copiate ca decizii din `KBotNavList`

Planul cere explicit: «Copy `KBotNavList`'s decisions rather than reinventing them.» Așa s-a făcut —
`KBotNavList.vb`, `KBotNavItem.vb` și colecția lor au fost citite întâi, iar bara nouă e recognoscibil
același cod: același set `SetStyle`, aceeași `ThemeShapes.FillModern`, același chenar roșu de 2 px pe
cheile greșite în designer, aceeași suspendare a validării între `BeginInit` și `EndInit`, aceleași
cârlige `Debug*` pentru probe headless.

- **`KBotChip.vb`** — un jeton: `Key`, `Text`, `Checked`, `Count` (pastila), `Enabled`, `Visible`,
  `AccentOverride`. `Bounds` e `Friend` (stare derivată, nu se serializează). `ToString` întoarce
  `err — "Erori" [✓]`, ca dialogul de colecție să fie citibil.
- **`KBotChipCollection.vb`** — `Collection(Of KBotChip)` cu `Owner`; cele patru mutatoare
  invalidează așezarea și **loghează + re-aruncă** (sunt puncte de intrare, deci acoperirea
  tranzitivă din regula casei nu se aplică). Validarea cheilor **nu** stă aici: dialogul de colecție
  inserează un jeton gol în clipa în care se apasă «Add».
- **`KBotChipBar.vb`** — controlul: `Control` + `IThemedControl` + `ISupportInitialize`.

### 2. Culoarea vine de la APELANT, nu din control

`AccentOverride` există pentru un singur motiv concret din plan: jetonul ERORI trebuie să fie roșu și
cel de AVERTISMENTE chihlimbariu. Controlul **nu numește nicio culoare** — apelantul îi dă
`Palette.ErrorColor` / `Palette.WarningColor` și i-o **re-dă** la fiecare comutare de schemă
(`LogViewerForm.OnThemeChanged` face exact asta). `Color.Empty` = accentul schemei.

### 3. Rândurile se rup după lățime, dar bara nu-și scrie `Height`-ul

Jetoanele curg stânga→dreapta și trec pe rândul următor când nu mai încap. Înălțimea de care ar fi
nevoie se citește din `PreferredBarHeight`; **bara nu scrie `Height`**, fiindcă gazda o andochează —
aceeași regulă ca `HostOwnsWidth` de la arbore (0027-02), unde scrisul dimensiunii contra unui părinte
care andochează doar produce pâlpâire.

### 4. `MinimumRequiredChecked`: un refuz care se VEDE, nu o excepție

La 1, stingerea ultimului jeton bifat cu mouse-ul sau cu `Space` se refuză: jetonul clipește 140 ms
și rămâne bifat, **fără excepție și fără eveniment**. E un gest de operator, nu un apel de API — de
aceea `SetChecked` / `UncheckAll` **nu** sunt oprite de prag (codul care cere explicit o stare o
primește). Distincția e fixată cu două teste, în ambele sensuri.

Pragul contează fiindcă `LogFilter` tratează mulțimea goală ca «nimic», nu ca «toate»: fără el,
debifarea ultimului jeton ar goli grila fără nicio explicație.

### 5. Ce s-a memorat și ce NU

Planul cere pensule memorate, refăcute în `ApplyTheme`, eliberate în `Dispose`. S-au memorat doar
cele care nu depind de starea jetonului (`_borderPen`, `_badgeBrush`). Umplerea jetonului trece prin
`ThemeShapes.FillModern`, care ia o **culoare**, iar `TextRenderer` desenează tot cu o culoare, nu cu
o pensulă — acolo n-ar fi avut ce să se memoreze. Câmpuri memorate și nefolosite ar fi fost cod mort
care arată ca optimizare.

---

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Controls/ChipBar/KBotChip.vb` | **nou** — modelul jetonului |
| `src/KBot.Controls/ChipBar/KBotChipCollection.vb` | **nou** — colecția |
| `src/KBot.Controls/ChipBar/KBotChipBar.vb` | **nou** — controlul |
| `tests/KBot.Controls.Tests/KBotChipBarTests.vb` | **nou** — probele 12–18 din plan |

---

## Rezultatele probelor

Numere REALE, dintr-o rulare, nu din plan:

- `dotnet build KBot.sln -c Debug`: **0 erori**, 4 avertismente `MSB3825` **preexistente**
  (`DdfView.resx`, `PlatiView.resx`, `ReceptiiView.resx`, `RezervariView.resx` — fișiere neatinse de
  felia asta).
- `dotnet build KBot.sln -c Release`: **0 erori**, aceleași 4 avertismente. **Abatere de la
  `SLICE-0031-01`**, care raporta «Release 0/0»: avertismentele apar pe cele patru `.resx` de la
  alte felii, neatinse aici, deci nu pot fi cauzate de trecerea asta — cifra veche a fost cel mai
  probabil măsurată pe o construcție incrementală care sărise generarea de resurse. Se consemnează,
  nu se ascunde.
- `KBot.Controls.Tests`: **831** verzi, dintre care **17** sunt ale barei de jetoane (măsurate cu
  `--filter FullyQualifiedName~KBotChipBar`), deci **814 înainte**. `SLICE-0030` raporta 808 —
  diferența de 6 vine din lucru din afara acestei felii, aflat necomis în arbore la începutul ei;
  nu a fost atinsă și nu se revendică aici.

Probele 12–18 din plan, toate acoperite: colecția conduce așezarea și refuză `Nothing`; cheile vide /
duplicate / necunoscute aruncă pe fiecare setter; `CheckedKeys` / `CheckAll` / `UncheckAll`;
`CheckedChanged` o dată pe schimbare reală și niciodată pe una redundantă; pragul de 1; dus-întorsul
`BeginInit`/`EndInit`; tastatura (`Space` comută, săgețile sar jetoanele ascunse și dezactivate, fără
wrap).

---

## Ce a rămas NEVERIFICAT sau amânat

1. **Dus-întorsul prin designerul real din Visual Studio nu a fost făcut.** Butonul «…», dialogul de
   colecție, liniile scrise în `.Designer.vb`, chenarul roșu pictat pe suprafața de design — niciunul
   nu a fost văzut. Testul programatic `BeginInit`/`EndInit` e cea mai apropiată probă disponibilă și
   **nu e același lucru**; felia 0025 a lovit exact zidul ăsta.
2. **Bara nu a fost văzută pe ecran ca bară de jetoane goală/independentă.** A fost văzută doar în
   `LogViewerForm` (vezi 0031-04, unde operatorul a confirmat vizual fereastra).
3. **`ShouldSerialize`/`Reset` există doar pentru `KBotChip.AccentOverride`.** Controlul însuși nu
   își scrie `BackColor`/`Font` decât prin `ApplyTheme` → `BackColor`, ceea ce **este** o scriere, deci
   regula din `CLAUDE.md` («dacă un control își scrie singur `BackColor`, are nevoie de
   `ShouldSerializeBackColor`») s-ar putea aplica. Nu s-a făcut și nu s-a măsurat: proba cerută de
   casă (`TypeDescriptor.GetProperties(c)("BackColor").ShouldSerializeValue(c)` pe un control proaspăt
   pus în designer) **nu a fost rulată**. `KBotNavList` are exact aceeași scriere și aceeași lipsă,
   deci nu e o regresie nouă, dar rămâne un fir deschis pentru amândouă.
