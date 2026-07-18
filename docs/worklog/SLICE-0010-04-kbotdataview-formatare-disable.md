# SLICE-0010-04 — KBotDataView: formatare condiționată + dezactivare pe trei niveluri

A patra trecere din slice-ul 0010. Plumbing-ul evenimentelor de formatare exista din 0010-02;
acum capătă **efect vizual** și, mai important, **contractul de activare efectivă** pe care se
va sprijini blocarea input-ului și a editării în 0010-05/06.

## Ce s-a schimbat și de ce

### Activarea EFECTIVĂ (coloană × rând × celulă)
Regula, identică la pictare și la interogare:

```
efectiv = Column.Enabled AndAlso <Row.Enabled după RowFormatting> AndAlso <cellEnabled după CellFormatting>
```

Handler-ul de celulă poate **coborî** pe False, dar **nu poate ridica** peste o coloană sau un
rând dezactivat (verificat prin test). Două metode publice expun rezoluția, ca input-ul din 05
să nu re-implementeze regula:
- `IsRowEnabled(rowIndex)` — `Row.Enabled` + eventualul veto din `RowFormatting`.
- `IsCellEnabled(colKey, rowIndex)` — regula completă de mai sus.

Ambele ridică evenimentele de formatare cu **instanțe de argumente SEPARATE**
(`_probeCellArgs`/`_probeRowArgs`), ca o interogare să nu calce peste argumentele refolosite
ale unei pictări în curs — capcana evidentă a refolosirii.

### Randarea „dezactivat”
- Textul dezactivat trece pe `DisabledTextColor` **indiferent** ce a cerut handler-ul: „inert”
  trebuie să se și VADĂ inert.
- Spălarea faintă (`Blend(rowBack, Surface, 0.4)`) se aplică **doar** când handler-ul n-a impus
  alt fundal — altfel am călca peste o regulă de formatare condiționată a caller-ului.
- Toate cele șase tipuri respectă starea: bifa/opțiunea se desenează cu marca ștearsă, chenarul
  butonului și eticheta lui trec pe culoarea dezactivată, umplerea barei de progres la fel,
  chevronul combo la fel.

### Reguli demonstrative în harness (acceptanța pasului)
- `RowFormatting`: `Stare = «Anulat»` => `e.Enabled = False` => tot rândul șters + inert.
- `CellFormatting`: valoare numerică negativă => text pe `Palette.ErrorColor` (roșul PALETEI,
  nu o culoare literală — regula «zero literale» se aplică și în bancul de test).
- Datele sintetice conțin acum și valori negative, ca regula să aibă ce colora.

## Fișiere atinse

- `src/KBot.Controls/KBotDataView.vb` — `IsRowEnabled`, `IsCellEnabled`, instanțele „probe”.
- `src/KBot.Controls/KBotDataView.Theming.vb` — `_cDisabledText`/`_cDisabledWash` + resurse GDI.
- `src/KBot.Controls/KBotDataView.Painting.vb` — aplicarea stării dezactivate în `DrawCell` și
  în cei cinci pictori de tip.
- `tests/KBot.Controls.Tests/KBotDataViewFormattingTests.vb` (nou, 10 teste).
- `src/KBot.DevHarness/Internal/DataViewHarnessForm.vb` — cele două reguli demonstrative.
- `docs/worklog/KBOT_STATUS.md`

## Rezultate teste

- `dotnet build KBot.sln -c Debug` — **0 warnings / 0 errors**.
- `dotnet test KBot.sln` — **123 passed / 0 failed / 0 skipped** (39 în `KBot.Controls.Tests`).
- Acoperire nouă: dezactivare pe coloană (afectează doar coloana), pe rând (doar rândul), pe
  celulă via eveniment (doar celula), veto de rând din `RowFormatting`, imposibilitatea de a
  ridica activarea dintr-un handler de celulă, coloană necunoscută => `ArgumentException`,
  numărul de ridicări ale evenimentelor la o pictare (2 rânduri × 2 coloane = 2 + 4), faptul că
  suprascrierea `e.Text` schimbă doar AFIȘAREA nu și valoarea din model, și că celulele
  dezactivate se pictează fără excepție.

## Rămas neverificat / amânat

- **Verdictul vizual uman tot NU a fost dat.** Culoarea efectiv pictată nu se poate citi
  headless — testele verifică CONTRACTUL care o determină, nu pixelii. Cum arată rândul gri
  «Anulat» și textul roșu pe negative **rămâne neconfirmat** până rulează cineva harness-ul
  (DevHarness → Controls/UI → «KBotDataView — virtualizare + temă»).
- Dezactivarea **blochează deocamdată doar randarea**; blocarea click-ului/tastelor/editării
  vine în 0010-05/06, care vor consuma `IsCellEnabled`.
- Hover/pressed pe `Button`, textul procentual pe `ProgressBar` — tot amânate.
- Urmează: 05 input+selecție, 06 editare.
