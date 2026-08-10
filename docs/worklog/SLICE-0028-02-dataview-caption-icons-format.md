# SLICE-0028-02 — DataView: titlu de subsol, pictograme de coloană, `Format` în stil Access

Al doilea pas al feliei 0028 (`KBotDataView`). Trei cereri ale operatorului, toate pe grilă.

## Ce s-a schimbat și de ce

### 1. Titlul din subsol (`FooterCaption` + pictogramă)

Banda de subsol avea, în stânga, spațiu gol până la prima coloană agregată. Acolo intră acum un
text și o pictogramă, cu același vocabular ca subsolul arborelui (`FooterCaption`,
`FooterLeftIcon`, `FooterIconSize`), plus ce n-are arborele: pictograma se apasă
(`FooterLeftIconClicked`) și are culoare de hover (`FooterLeftIconHoverColor`, gol = din temă).

**Unde se oprește zona** e singura regulă care contează: la PRIMA coloană agregată, niciodată mai
departe. Un titlu care ar curge pe sub o sumă ar arăta ca eticheta acelei sume — adică ar spune
altceva decât spune. Fără nicio coloană agregată, zona e toată banda. Colțul butonului de
strângere rămâne al lui (zona pornește din `FooterContentRect`, care îl scade deja), deci cu
butonul pus pe stânga titlul se mută singur după el, nu se așază sub el.

Pictograma se desenează doar dacă încape ÎNTREAGĂ în zonă, și tot atunci se și apasă: ce nu se
vede nu se poate apăsa.

### 2. Pictograme pe fiecare coloană, în antet (`HeaderLeftIcon` / `HeaderRightIcon`)

Fiecare coloană poate purta o pictogramă la stânga titlului și una la dreapta lui, fiecare cu
mărimea ei (`HeaderLeftIconSize` / `HeaderRightIconSize`, implicit 16×16). Cea din DREAPTA e cea
care se apasă — `KBotDataView.HeaderRightIconClicked`, cu cheia coloanei ȘI dreptunghiul
pictogramei în `KBotColumnEventArgs` (cazul de folosire e un meniu de filtru/sortare, care trebuie
așezat SUB ea). Ea are și hover (`HeaderRightIconHoverColor`, gol = din temă); cea din stânga e un
semn, nu un buton, deci n-are nici eveniment, nici hover — o evidențiere sub ceva inert ar promite
o acțiune care nu există.

**Ordinea sacrificiului la îngustare** (cerută explicit): titlul se taie ÎNTÂI, cu elipsă, până la
lățime zero; abia apoi cade pictograma din STÂNGA; cea din dreapta nu cade niciodată.

**Podeaua de lățime.** `KBotDataColumn.EffectiveMinWidth` = `max(MinWidth, HeaderIconsWidth)`, unde
`HeaderIconsWidth` = `2×Pad + stânga + Gap + dreapta` (Pad 8, Gap 4, px logici). Ea limitează orice
scriere în `Width` (`ClampWidth`) și o folosesc toate trecerile de auto-dimensionare
(`MeasureColumnToContent`, `ShrinkToFit`). **Podeaua bate `MaxWidth`**: un plafon sub cât cer
pictogramele e o cerere imposibilă, iar dacă ar câștiga plafonul, coloana ar picta piese
suprapuse. Setarea unei pictograme re-limitează lățimea PE LOC, nu la următorul layout.

Așezarea celor trei piese stă într-o funcție pură, `KBotDataView.ComputeHeaderCellLayout`, pe care
o cheamă și pictarea, și hit-testul, și testele — o a doua formulă ar însemna o pictogramă
desenată unde nu se apasă.

### 3. `Format` — proprietatea numită, în stil Access

`KBotDataColumn.Format` (enum `KBotFormat`) aduce lista Access: General Number, Currency, Euro,
Fixed, Standard, Percent, Scientific, General/Long/Medium/Short Date, Long/Medium/Short Time,
Yes/No, True/False, On/Off. Traducerea în text stă într-un singur loc, `KBotColumnFormat`, citit de
AMÂNDOI consumatorii — pictarea celulelor și agregatele din subsol — ca un total să nu se scrie
altfel decât coloana de deasupra lui.

Două lucruri deliberate:

- **Formatul numit CITEȘTE valoarea**, nu doar o formatează: o coloană `ValueType.Text` care poartă
  numere (cazul DdfView/PlatiView) se scrie „Standard” corect. O valoare care nu se poate citi în
  tipul cerut cade pe calea obișnuită și tot se vede — nu se stinge într-o celulă goală.
- **`Format` și `FormatString` nu se folosesc împreună** — sunt două fețe ale aceluiași lucru.
  Amândouă setate ARUNCĂ (`ArgumentException`), verificat amânat în blocul de inițializare al
  designerului și așezat la `EndInit`, exact ca perechea `ValueType × Aggregate`. Alternativa —
  una câștigând tăcut — ar lăsa în formular o proprietate care arată ca o setare activă și nu face
  nimic.

Câte zecimale scrie un format vine din `DecimalPlaces` când e fixat, altfel 2 (implicitul Access),
deci cele două proprietăți nu se contrazic. `AggregateFormatString` rămâne deasupra tuturor.

`ValidateAggregatePair` s-a redenumit `ValidateSettled` — verifică acum trei perechi așezate, nu una.

## Fișiere atinse

Noi:

- `src/KBot.Controls/DataView/KBotFormat.vb` — enum-ul formatelor numite.
- `src/KBot.Controls/DataView/KBotColumnFormat.vb` — traducerea lor în text (singurul loc).
- `src/KBot.Controls/DataView/KBotDataView.HeaderIcons.vb` — așezare, hover, hit-test, eveniment.
- `src/KBot.Controls/DataView/KBotDataView.FooterCaption.vb` — zona, pictograma, evenimentul.
- `src/KBot.Controls/DataView/Events/KBotColumnEventArgs.vb` — cheie + dreptunghiul pictogramei.
- `tests/KBot.Controls.Tests/KBotDataViewHeaderIconsTests.vb` (14 teste)
- `tests/KBot.Controls.Tests/KBotDataViewColumnFormatTests.vb` (14 teste)
- `tests/KBot.Controls.Tests/KBotDataViewFooterCaptionTests.vb` (14 teste)

Modificate:

- `KBotDataColumn.vb` — pictogramele + mărimile + culoarea de hover, `ClampWidth`,
  `HeaderIconsWidth`/`EffectiveMinWidth`, `Format`, `FormatString` devenit proprietate întreagă,
  `ValidateSettled`.
- `KBotDataView.vb` — `OnColumnFormatChanged`, apelurile `ValidateSettled`.
- `KBotDataView.Painting.vb` — `DrawHeaderCell` prin `DrawHeaderIcons`, titlul de subsol,
  `FormatValue` trece prin formatul numit; `ToBool` devenit `Friend`.
- `KBotDataView.Footer.vb` — agregatele trec prin formatul numit; `TryNumeric`/`TryDate` `Friend`.
- `KBotDataView.AutoSize.vb` — măsurarea adună pictogramele; podeaua e `EffectiveMinWidth`.
- `KBotDataView.Input.vb` — apăsarea/hover-ul pictogramelor de antet, cursorul de mână.
- `KBotDataView.Collapse.vb` — banda de subsol trimite apăsarea/hover-ul mai departe la pictogramă.

## Rezultatele testelor

- `dotnet build KBot.sln` — succes, 0 erori. Singurul warning e cel preexistent, `MSB3825`
  (BinaryFormatter în `DdfView.resx`), nelegat de felia asta.
- `KBot.Controls.Tests` — **627 trecute, 0 căzute** (42 noi).
- `KBot.Theming.Tests` 61, `KBot.Common.Tests` 14, `KBot.Xfa.Tests` 39, `KBot.DevHarness.Tests`
  170 — toate verzi.
- **Căzute, dar PREEXISTENTE în arborele de lucru, nelegate de felia asta:** 1 în
  `KBot.Api.Tests` (`GetDdf_FormatsRevisionLabel_…` — `KBot.Api` nici măcar nu referă
  `KBot.Controls`) și 8 în `KBot.App.Tests` (`MainFormNavItemsTests.Designer_WroteLiteralDiacritics_…`
  plus 7 `DdfViewTests`, care caută prin reflecție un `tree_NodeMouseUp` ce nu mai există ca metodă
  în `DdfView.vb` — a rămas doar în eticheta de log de la linia 650). Nu au fost atinse aici.

## Rămas neverificat / amânat

- **Nimic nu s-a văzut pe ecran.** Ca la 0028: totul e verificat prin compilare + teste headless
  (`DrawToBitmap`). `TreePlaygroundForm`/`DataViewHarnessForm` NU au fost extinse cu piesele noi —
  harness-ul nu arată încă nici titlul de subsol, nici pictogramele de coloană, nici lista de
  formate. Ăsta e pasul următor dacă se vrea verdict vizual.
- **DPI.** Mărimile pictogramelor sunt în pixeli, ne-scalate (ca la arbore: ce se vede în designer
  e ce se desenează), pe când spațiile se scalează. Pe un ecran la 150% podeaua rămâne cu câțiva
  pixeli sub necesarul real al pictării — se plătește din text, care oricum se taie primul,
  niciodată din pictograme, deci degradarea e cea documentată, nu una care suprapune desene.
  Neverificat pe un ecran real la DPI ridicat.
- **`ShouldSerialize`/`Reset` neverificate empiric în designerul Visual Studio.** Perechile există
  pentru toate proprietățile noi de tip `Image`/`Size`/`Color` (regula casei: `Size` nu poate purta
  `<DefaultValue>`), dar verificarea cerută de CLAUDE.md — o coloană proaspăt creată să producă
  ZERO linii de proprietăți, probat prin `TypeDescriptor.GetProperties(c)(name).ShouldSerializeValue(c)`
  — n-a fost rulată pentru ele.
- **`Format` nu e folosit încă de nicio vedere.** `SumarView`/`DdfView`/`PlatiView` etc. rămân pe
  `FormatString`; nimic nu s-a migrat, ca să nu se amestece o schimbare de contract cu o
  rescriere de vederi.
