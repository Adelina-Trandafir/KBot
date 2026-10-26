# SLICE-0013/0016/0037 — Istoricul dimensionării și ascunderii coloanelor

Consolidare documentară D002-10. 0013 definește măsurarea/umplerea, 0016 introduce ascunderea în aceeași trecere, iar 0037 extinde desemnarea țintei de umplere la o cheie arbitrară. Corpurile păstrează algoritmii, formulele, probele și limitele fiecărei etape. Interpretarea pozițională inițială din 0016 este urmată de extensia SpecificColumn din 0037, fără un verdict vizual nou.

Registrul 0013 avea simultan Status «VISUALLY ACCEPTED in harness playground» și, în Notes, «Still unrun on screen — like all of 0010/0011». Corpul consemnează acceptarea playgroundului și cele două corecții descoperite la rulare; contradicția vechii note este păstrată aici, fără a anula sau extinde acceptarea. Nota 0016 repeta «Unrun on screen like all of 0010/0013»; proba proprie a 0016 rămâne fără verdict vizual. Planul 0013 era lipit în sesiune, potrivit registrului.

Regula limbii comentariilor din 0013 este istorică și nu înlocuiește instrucțiunile actuale.

[0013](#slice-0013) · [0016](#slice-0016) · [0037](#slice-0037)

<a id="slice-0013"></a>

## SLICE-0013 — KBotDataView: auto-dimensionarea coloanelor + moduri de umplere

Lățimile coloanelor veneau până acum exclusiv din ce pasa caller-ul la `AddColumn` (sau dintr-un drag manual). Slice-ul adaugă două comportamente grid-wide, aplicate ca O SINGURĂ trecere în `UpdateLayout`, înaintea offset-urilor și a barelor de derulare:

1. **`AutoSizeColumnsMode`** (`KBotAutoSizeMode`) — `None` sau `ToContent` (implicit). `ToContent` măsoară fiecare coloană vizibilă la conținutul ei (max între antet și celulele eșantionate) și o limitează la `[MinWidth, MaxWidth]`.
2. **`ColumnFillMode`** (`KBotFillMode`) — `None` (implicit), `FirstColumn`, `LastColumn`, `Proportional`. Aplicat DUPĂ auto-dimensionare, peste spațiul rămas (sau lipsă).

Cele patru cazuri din brief:

| Caz | Setări | Rezultat |
|---|---|---|
| 1 — fit la conținut | `ToContent` | fiecare coloană cât textul ei cel mai lat |
| 2 — cheltuie restul | `ToContent` + `First/Last/Proportional` | fără bandă goală în dreapta |
| 3 — doar manual | `None` + `None` | lățimile caller-ului rămân neatinse |
| 4 — depășire | orice + `None` => bară orizontală; orice + mod de umplere => coloanele se micșorează, bara nu apare |

### Regulă nouă de limbă

**Toate comentariile de cod adăugate/atinse în acest slice sunt în ENGLEZĂ** (marcate „English (slice 0013)”). Restul `KBot.Controls` rămâne comentat în română — NU s-a făcut conversie în masă; doar codul nou/atins urmează regula. Mesajele de eroare și textul UI rămân în română.

### Ce s-a schimbat și de ce

#### Model (`KBotDataColumn`)
- **`MaxWidth`** (implicit `Integer.MaxValue`, neplafonat). Ținut mereu ≥ `MinWidth`; coborârea lui re-limitează `Width`.
- **`Width`** re-limitat la `[MinWidth, MaxWidth]` la FIECARE scriere — invarianta trăiește în model, deci trecerea de auto-size/fill/shrink poate atribui liber și lasă setter-ul să impună marginile.
- **`UserSized`** (`Friend`) — ridicat de un drag de operator; `ToContent` sare peste astfel de coloane, dar fill/shrink li se aplică în continuare. Golit de `ResetColumnSizing`.
- Capcana VB de case-insensitivity: proprietățile noi nu au parametri omonimi în setter/ctor, deci nu era nevoie de `Me.`; există totuși regresie pentru `MaxWidth`/`UserSized` (round-trip + clamp).

#### Trecerea (`KBotDataView.AutoSize.vb`, nou)
- `PerformAutoSize()` rulează din `UpdateLayout`: măsoară (ToContent, sărind coloanele `UserSized`) apoi fill/shrink. Gardă de reintrare `_inAutoLayout` + oprire dacă `_updateDepth > 0` (se rulează o singură dată din `EndUpdate`). Dacă `None` + `None`, iese imediat (Caz 3 = zero atingeri).
- **Lățimea disponibilă** oglindește exact ce folosește `UpdateScrollBars` (`ClientSize.Width − bară verticală dacă e vizibilă`), deci un mod de umplere face totalul EGAL cu viewport-ul și bara orizontală nu apare. Vizibilitatea barei verticale depinde doar de numărul de rânduri și înălțimea corpului (niciodată de lățimi), deci se decide prima — fără dependență circulară.
- **Măsurare**: `TextRenderer.MeasureText` cu aceleași fonturi ca pictorul (antet semibold, corp ambient). Text/Combo/Button măsoară șirul FORMATAT (aplică `FormatString`); Combo adaugă zona de chevron; CheckBox/OptionButton folosesc cutia glifului + padding; ProgressBar păstrează lățimea caller-ului (antetul tot participă). Padding-ul e cel din pictură (6px corp, 8px antet, DPI-scalat).
- **Proporțional**: `extra(i) = leftover · width(i) / total`; restul întreg din diviziune merge INTEGRAL la ultima coloană (totaluri exacte, fără gol de 1–2px la dreapta). O coloană plafonată de `MaxWidth` își pasează surplusul celor neplafonate într-o SINGURĂ trecere suplimentară (nu buclă la convergență).
- **Micșorare** (depășire + mod de umplere): scade deficitul din coloanele încă peste `MinWidth`, proporțional cu lățimea curentă, cu podea la `MinWidth`. Bucla e mărginită (fiecare rundă fie converge, fie fixează încă o coloană la podea — cel mult `nrColoane` runde).

#### Integrare
- `UpdateLayout` cheamă `PerformAutoSize` înaintea `RecalcColumnLayout`/`UpdateScrollBars`.
- `ApplyTheme` și `OnFontChanged` cheamă `UpdateLayout` (schimbarea fontului schimbă măsurarea).
- Drag-ul de coloană (`OnMouseMove`) ridică `UserSized` pe coloana trasă.
- Metode publice: `AutoSizeColumns()` (forțează o trecere) și `ResetColumnSizing()` (golește `UserSized` pe toate + re-măsoară). Setter-ele celor trei proprietăți declanșează o trecere.

### Fișiere atinse

- `src/KBot.Controls/KBotAutoSizeMode.vb`, `KBotFillMode.vb` (noi — enum-uri)
- `src/KBot.Controls/KBotDataView.AutoSize.vb` (nou — trecerea)
- `src/KBot.Controls/KBotDataColumn.vb` — `MaxWidth`, `UserSized`, clamp `Width`/`MinWidth`
- `src/KBot.Controls/KBotDataView.Layout.vb` — `PerformAutoSize` în `UpdateLayout`
- `src/KBot.Controls/KBotDataView.Theming.vb` — `UpdateLayout` în `ApplyTheme`/`OnFontChanged`
- `src/KBot.Controls/KBotDataView.Input.vb` — `UserSized = True` la drag
- `tests/KBot.Controls.Tests/KBotDataViewAutoSizeTests.vb` (nou, 15 teste)
- `tests/KBot.Controls.Tests/KBotDataModelTests.vb` — 4 teste (`MaxWidth`/`UserSized`)

### Rezultate teste

- `dotnet build KBot.sln -c Debug` — **0 warnings / 0 errors**.
- `dotnet test KBot.sln` — **toate verzi** (93 în `KBot.Controls.Tests`, restul neschimbat).
- Poarta de virtualizare rămâne validă: numărul de rânduri pictate NU depinde de `RowCount` (auto-size trăiește în `UpdateLayout`, nu în calea de pictare, și eșantionează cel mult `AutoSizeSampleRows` rânduri).

### Compromisuri și limitări (de reținut)

- **Eșantionare (`AutoSizeSampleRows`, implicit 200; 0 = toate):** măsurarea a 5.000×20 celule ar fi ~100k măsurători și ar rupe promisiunea O(viewport). Se măsoară antetul + primele `AutoSizeSampleRows` rânduri; o valoare mai lată mai jos va fi tăiată cu «…». Compromis deliberat — cine vrea exactitate pe grile mici setează 0 (Sumar are ~10 rânduri, deci 0 e gratis acolo).
- **`CellFormatting` NU se ridică la măsurare** (ar fi scump și reintrant). Consecință: un handler care rescrie `Text` poate produce un șir mai lat decât lățimea măsurată, care va ellipsiza.
- **Fallback-ul onest la depășire:** dacă `sum(MinWidth) > available`, coloanele nu încap. Se micșorează tot la `MinWidth` și se arată TOTUȘI bara orizontală — textul care dispare complet e mai rău decât o bară pe care caller-ul n-a cerut-o.
- **Fără back-reference coloană→grilă:** `KBotDataColumn` e un model „prost”, fără pointer la control, deci o schimbare programatică de `Width`/`MinWidth`/`MaxWidth`/`Visible`/`FormatString` DUPĂ încărcare nu declanșează singură o trecere. `AddColumn` declanșează; drag-ul declanșează; în rest caller-ul cheamă `AutoSizeColumns()` (sau se prinde la următorul eveniment de layout — resize/temă/EndUpdate). În practică coloanele se configurează complet imediat după `AddColumn`, deci punctele de declanșare reale sunt acoperite.

### Verificare în harness (playground)

S-a adăugat proba `KBotDataView — proprietăți runtime (playground)` (Controls/UI): grilă + panou care comută LIVE fiecare proprietate runtime (auto-size / fill / eșantion / frozen / înălțimi / antet / alternante / read-only) + inspector de coloană (Visible/Enabled/ReadOnly/Width/Min/Max) + comutator de rânduri (12/200/5000, cu o valoare foarte lată la rândul 300 ca să se vadă efectul eșantionării). Trei fișiere noi în `KBot.DevHarness` (`Tests/KBotDataViewPlaygroundTest.vb`, `Internal/DataViewPlaygroundForm.{vb,Designer.vb}`).

**Bug găsit și reparat la rulare (preexistent din 0010-05):** `ReadOnlyGrid` NU se aplica la comutarea bifelor / opțiunilor — `ActivateCell` verifica doar `IsCellEnabled`, nu și read-only. Fix: o comutare de valoare (CheckBox/OptionButton) e blocată când `ReadOnlyGrid` SAU `Column.ReadOnly` e adevărat (același contract ca editarea text/combo din `CanEdit`); un **Button rămâne activ** (e acțiune pură, fără valoare/dirty). 4 teste noi în `KBotDataViewInputTests.vb` (97 în `KBot.Controls.Tests`). Butonul de comandă ERA deja clicabil (ridica `ButtonClick`) — lipsea doar feedback-ul în playground; s-a adăugat un handler care scrie în info + log.

**Al doilea bug de pictare (frozen column):** la derulare orizontală, conținutul benzii derulate «răzbătea» sub coloana înghețată — celulele înghețate se desenau PESTE cea derulată, dar doar TEXTUL (fără fundal opac), deci textul derulat rămas se vedea printre glife. Fix în `KBotDataView.Painting.vb`: banda înghețată (antet + fiecare rând) e re-pictată OPAC imediat înainte de celulele ei, garantând că coloana statică e mereu deasupra. 2 teste noi (`KBotDataViewFrozenColumnTests.vb`, 99 în total) — verifică headless că nicio culoare a benzii derulate nu apare în banda înghețată după `hScroll`.

### Rămas / în afara scopului

- Override per-coloană de `AutoSizeMode`, auto-dimensionarea rândurilor, word-wrap în celulă, reordonarea coloanelor, re-măsurarea la fiecare scroll — toate în afara scopului.
- **Verdict vizual: ACCEPTAT de operator** în proba playground (Controls/UI) — vezi «Verificare în harness».
- **Follow-up Sumar (nu în acest slice):** `SumarView` ar trebui să seteze `AutoSizeColumnsMode = ToContent` + `ColumnFillMode = LastColumn`/`Proportional` și să renunțe la lățimile hardcodate din `BuildColumns` (coloana Partener se scoate separat).

<a id="slice-0016"></a>

## SLICE-0016 — KBotDataView: coloane care se ascund automat (`Column.AutoHide`)

O proprietate nouă pe coloană care o lasă să **dispară** când grila n-ar încăpea altfel fără scrollbar orizontal, plus interacțiunea cerută cu „coloana care se întinde” (`ColumnFillMode`).

### Ce s-a schimbat și de ce

#### Proprietatea (per coloană)
`KBotDataColumn.AutoHide As Boolean` (implicit False). Coloanele marcate pot fi ascunse de pasul de layout ca să se evite scrollbarul orizontal.

#### Starea internă (nu se calcă peste intenția caller-ului)
Ascunderea automată e distinctă de `Visible` (show/hide explicit al caller-ului):
- `Friend AutoHidden` — setat DOAR de pas, recalculat de la zero la fiecare layout.
- `Friend IsEffectivelyVisible` = `Visible AndAlso Not AutoHidden`. Cele două singure locuri care citeau vizibilitatea coloanei (`RecalcColumnLayout` și `VisibleColumns`) folosesc acum `IsEffectivelyVisible`, deci totul din aval — pictare, hit-testing, navigație, auto-size — moștenește ascunderea fără cod nou pe fiecare cale.

#### Algoritmul (în `PerformAutoSize`, după măsurare, înainte de fill)
1. **Reset** `AutoHidden` pe toate coloanele — o grilă lărgită re-arată ce încape acum.
2. **Ascunde** coloanele hidable (dreapta întâi, „collapse from the right”) până când restul încap SAU nu mai e ce ascunde. Coloana-țintă a fill-ului e **protejată** (vezi mai jos).
3. **Umple / micșorează:**
   - dacă acum încape și fill-ul e First/Last, coloana care se întinde absoarbe golul lăsat de cele dispărute (branch-ul „leftover” din slice 0013 — deci golul nu rămâne gol);
   - dacă nu mai e ce ascunde și tot nu încape, **NU se micșorează** (spre deosebire de 0013 pur): scrollbarul apare normal. Cu auto-hide activ, ascunderea e răspunsul ales, nu strângerea supraviețuitoarelor.

#### „Coloana care se întinde nu poate fi una care dispare; dacă ambele, întinderea câștigă”
„Coloana care se întinde” = ținta `ColumnFillMode` (First → prima vizibilă, Last → ultima vizibilă). `FillTargetColumn` o întoarce; `PerformAutoHide` o **sare** mereu. Deci o coloană marcată ȘI `AutoHide` ȘI care e ținta fill-ului **nu dispare** — întinderea are prioritate. `Proportional`/`None` n-au o singură țintă protejată (toate hidable-urile pot dispărea).

### Interpretare (confirmată cu codul, nu cu presupuneri)

Planul vorbea de „the property to stretch a column”. În cod NU există o proprietate per-coloană de întindere — întinderea e `ColumnFillMode` (grid-wide, țintește pozițional prima/ultima coloană, slice 0013). Am reutilizat-o ca „coloana care se întinde”, în loc să inventez un mecanism paralel. Dacă intenția era un flag per-coloană `Stretch`, se schimbă doar desemnarea țintei — restul algoritmului rămâne. **De confirmat cu operatorul dacă interpretarea diferă.**

### Fișiere atinse

- `src/KBot.Controls/KBotDataColumn.vb` — `AutoHide`, `AutoHidden`, `IsEffectivelyVisible`.
- `src/KBot.Controls/KBotDataView.AutoSize.vb` — auto-hide în `PerformAutoSize`, `PerformAutoHide`, `FillTargetColumn`, `AnyColumnCanAutoHide`, `ClearAutoHiddenState`, `DistributeOrShrink(suppressShrink)`.
- `src/KBot.Controls/KBotDataView.Layout.vb` + `.Input.vb` — cele două citiri de vizibilitate trec pe `IsEffectivelyVisible`.
- `tests/KBot.Controls.Tests/KBotDataViewAutoHideTests.vb` (nou, 9 teste).
- `src/KBot.DevHarness/Internal/DataViewHarnessForm.*` — checkbox «Ascunde coloane la nevoie».
- `docs/worklog/KBOT_STATUS.md`.

### Rezultate teste

- `dotnet build src/KBot.Controls` și `src/KBot.DevHarness` — **0 warnings / 0 errors**.
- `dotnet test KBot.sln` — **243 passed / 0 failed / 0 skipped** (Controls 120, +9 noi).
- Grila de test e pe dimensionare MANUALĂ (widths deterministe: 5 × 100px în 250px lățime). Acoperit: ascundere din dreapta până încape; `Visible` rămâne True (doar `AutoHidden` se setează); fără mai multe hidable => scrollbar; fill Last/First protejează expanderul ȘI umple golul (c4=150 / c0=150); expanderul care e singura hidable nu dispare (=> scrollbar); lărgirea readuce coloanele; navigația sare peste o coloană ascunsă automat; și — fără AutoHide — micșorarea slice-0013 rămâne neschimbată.

### Rămas neverificat / amânat

- **Verdictul vizual uman tot NU a fost dat** (harness-ul KBotDataView n-a rulat niciodată). Efectul se vede în DevHarness → Controls/UI → proba KBotDataView, bifând «Ascunde coloane la nevoie» și îngustând fereastra. Testele acoperă matematica ascunderii, nu pixelii.
- **Editare + ascundere concomitentă:** dacă fereastra se îngustează cât un editor e deschis peste o coloană care tocmai dispare, editorul plutitor rămâne (comitul pe resize nu e cablat). Caz rar, pre-existent ca gap (comit e cablat pe scroll/mutare, nu pe resize/auto-hide).
- **Coloane înghețate:** o coloană `Frozen` marcată `AutoHide` poate dispărea (flagul decide, nu statutul de înghețare). Neobișnuit, dar permis; nefiltrat intenționat în v1.
- **Ordinea de ascundere** e fixă (dreapta întâi). Un câmp de prioritate per-coloană ar putea urma dacă e nevoie.

<a id="slice-0037"></a>

## SLICE-0037 — Grila: coloana care se întinde poate fi ORICARE, nu doar prima sau ultima

Cerere de operator:

> «pentru dgv: vreau să pot alege și altă coloană (în afară de prima sau ultima) pentru autofill»

### De ce lipsea

`ColumnFillMode` (felia 0013) numea ținta prin POZIȚIE: `FirstColumn` / `LastColumn`, plus `Proportional`, care nu numește pe nimeni fiindcă le mișcă pe toate. Într-o grilă cu un cod îngust la stânga și sume la dreapta, coloana care trebuie să crească e descrierea din MIJLOC — și nu exista niciun fel de a o cere. `Proportional` nu e răspunsul: ar lăți și coloanele de sume, care au deja exact lățimea cifrelor lor.

### Ce s-a schimbat

**`KBotFillMode.SpecificColumn` (=4)** și, lângă el, **`KBotDataView.FillColumnKey`** — cheia coloanei-țintă. Trei alegeri merită scrise, fiindcă niciuna nu e evidentă:

1. **Cheie, nu poziție.** Un index ar arăta către altceva după prima reordonare sau ascundere de coloană; `Key` e deja moneda cu care vorbesc gruparea (`KBotGroupLevel.ColumnKey`), filtrarea și sortarea. Aceeași monedă peste tot înseamnă că o coloană mutată nu mută întinderea.
2. **Cheie greșită = zgomot la `EndInit`, nu la fiecare pictare.** `ValidateFillColumn` stă lângă `ValidateGroupLevels`, se sare în designer (o excepție din `InitializeComponent` înseamnă un formular care nu se mai deschide) și reclamă atât cheia GOALĂ, cât și una necunoscută: un mod de umplere care n-are ce umple e o greșeală de model, nu o stare. O trecere de layout n-are voie să arunce pentru asta — s-ar loga la fiecare redimensionare a ferestrei.
3. **Coloana ascunsă ACUM nu e o greșeală, e o stare.** Ținta se caută printre coloanele vizibile; dacă lipsește, nu crește nimeni și spațiul rămâne gol, ca la `None`. **Nu** se cade pe ultima coloană — ar întinde taman coloana pe care operatorul a ocolit-o alegând alta.

Ținta trece prin `FillTargetColumn`, deci moștenește gratis și **protecția față de auto-ascundere** (felia 0016): coloana aleasă nu poate fi ascunsă ca să încapă restul, exact cum se poartă azi ținta `FirstColumn`/`LastColumn`. Întinderea bate ascunderea, ca și până acum.

Pe drum: setter-ul lui `ColumnFillMode` **validează acum enum-ul**. Înainte accepta tăcut orice `CType(99, KBotFillMode)` și grila se purta ca la `None` — exact no-op-ul tăcut pe care regula casei îl interzice. `AutoSizeColumnsMode` avea garda dintotdeauna; sora lui nu.

### Cum se folosește

```vb
grid.ColumnFillMode = KBotFillMode.SpecificColumn
grid.FillColumnKey = "Descriere"
```

Ambele sunt proprietăți de designer (categoria «K-BOT»), deci se pot autora în `.Designer.vb` — `FillColumnKey` poate fi scrisă și înainte de a comuta modul, fiindcă în celelalte moduri e metadata inertă.

### Ce NU s-a făcut

- **Fără verdict vizual** — modul nu a fost văzut pe o grilă adevărată; e verificat prin compilare.
- **Fără teste** (cerute explicit sărite la această trecere). Rămân de scris cel puțin: ținta din mijloc primește tot restul, ținta ascunsă nu întinde pe nimeni, cheia necunoscută aruncă la `EndInit`, ținta e protejată de auto-ascundere.
- **Nicio vedere existentă nu a fost trecută pe noul mod** — `SumarView`/`IstoricView`/`DdfView` rămân cum erau.

Reper de compilare din registrul 0037: KBot.Controls — 0 erori, 0 avertismente. Acesta nu este un verdict vizual.
