# SLICE-0016 — KBotDataView: coloane care se ascund automat (`Column.AutoHide`)

O proprietate nouă pe coloană care o lasă să **dispară** când grila n-ar încăpea altfel fără
scrollbar orizontal, plus interacțiunea cerută cu „coloana care se întinde” (`ColumnFillMode`).

## Ce s-a schimbat și de ce

### Proprietatea (per coloană)
`KBotDataColumn.AutoHide As Boolean` (implicit False). Coloanele marcate pot fi ascunse de
pasul de layout ca să se evite scrollbarul orizontal.

### Starea internă (nu se calcă peste intenția caller-ului)
Ascunderea automată e distinctă de `Visible` (show/hide explicit al caller-ului):
- `Friend AutoHidden` — setat DOAR de pas, recalculat de la zero la fiecare layout.
- `Friend IsEffectivelyVisible` = `Visible AndAlso Not AutoHidden`.
Cele două singure locuri care citeau vizibilitatea coloanei (`RecalcColumnLayout` și
`VisibleColumns`) folosesc acum `IsEffectivelyVisible`, deci totul din aval — pictare,
hit-testing, navigație, auto-size — moștenește ascunderea fără cod nou pe fiecare cale.

### Algoritmul (în `PerformAutoSize`, după măsurare, înainte de fill)
1. **Reset** `AutoHidden` pe toate coloanele — o grilă lărgită re-arată ce încape acum.
2. **Ascunde** coloanele hidable (dreapta întâi, „collapse from the right”) până când restul
   încap SAU nu mai e ce ascunde. Coloana-țintă a fill-ului e **protejată** (vezi mai jos).
3. **Umple / micșorează:**
   - dacă acum încape și fill-ul e First/Last, coloana care se întinde absoarbe golul lăsat de
     cele dispărute (branch-ul „leftover” din slice 0013 — deci golul nu rămâne gol);
   - dacă nu mai e ce ascunde și tot nu încape, **NU se micșorează** (spre deosebire de 0013
     pur): scrollbarul apare normal. Cu auto-hide activ, ascunderea e răspunsul ales, nu
     strângerea supraviețuitoarelor.

### „Coloana care se întinde nu poate fi una care dispare; dacă ambele, întinderea câștigă”
„Coloana care se întinde” = ținta `ColumnFillMode` (First → prima vizibilă, Last → ultima
vizibilă). `FillTargetColumn` o întoarce; `PerformAutoHide` o **sare** mereu. Deci o coloană
marcată ȘI `AutoHide` ȘI care e ținta fill-ului **nu dispare** — întinderea are prioritate.
`Proportional`/`None` n-au o singură țintă protejată (toate hidable-urile pot dispărea).

## Interpretare (confirmată cu codul, nu cu presupuneri)

Planul vorbea de „the property to stretch a column”. În cod NU există o proprietate per-coloană
de întindere — întinderea e `ColumnFillMode` (grid-wide, țintește pozițional prima/ultima
coloană, slice 0013). Am reutilizat-o ca „coloana care se întinde”, în loc să inventez un
mecanism paralel. Dacă intenția era un flag per-coloană `Stretch`, se schimbă doar desemnarea
țintei — restul algoritmului rămâne. **De confirmat cu operatorul dacă interpretarea diferă.**

## Fișiere atinse

- `src/KBot.Controls/KBotDataColumn.vb` — `AutoHide`, `AutoHidden`, `IsEffectivelyVisible`.
- `src/KBot.Controls/KBotDataView.AutoSize.vb` — auto-hide în `PerformAutoSize`,
  `PerformAutoHide`, `FillTargetColumn`, `AnyColumnCanAutoHide`, `ClearAutoHiddenState`,
  `DistributeOrShrink(suppressShrink)`.
- `src/KBot.Controls/KBotDataView.Layout.vb` + `.Input.vb` — cele două citiri de vizibilitate
  trec pe `IsEffectivelyVisible`.
- `tests/KBot.Controls.Tests/KBotDataViewAutoHideTests.vb` (nou, 9 teste).
- `src/KBot.DevHarness/Internal/DataViewHarnessForm.*` — checkbox «Ascunde coloane la nevoie».
- `docs/worklog/KBOT_STATUS.md`.

## Rezultate teste

- `dotnet build src/KBot.Controls` și `src/KBot.DevHarness` — **0 warnings / 0 errors**.
- `dotnet test KBot.sln` — **243 passed / 0 failed / 0 skipped** (Controls 120, +9 noi).
- Grila de test e pe dimensionare MANUALĂ (widths deterministe: 5 × 100px în 250px lățime).
  Acoperit: ascundere din dreapta până încape; `Visible` rămâne True (doar `AutoHidden` se
  setează); fără mai multe hidable => scrollbar; fill Last/First protejează expanderul ȘI umple
  golul (c4=150 / c0=150); expanderul care e singura hidable nu dispare (=> scrollbar);
  lărgirea readuce coloanele; navigația sare peste o coloană ascunsă automat; și — fără AutoHide
  — micșorarea slice-0013 rămâne neschimbată.

## Rămas neverificat / amânat

- **Verdictul vizual uman tot NU a fost dat** (harness-ul KBotDataView n-a rulat niciodată).
  Efectul se vede în DevHarness → Controls/UI → proba KBotDataView, bifând «Ascunde coloane la
  nevoie» și îngustând fereastra. Testele acoperă matematica ascunderii, nu pixelii.
- **Editare + ascundere concomitentă:** dacă fereastra se îngustează cât un editor e deschis
  peste o coloană care tocmai dispare, editorul plutitor rămâne (comitul pe resize nu e cablat).
  Caz rar, pre-existent ca gap (comit e cablat pe scroll/mutare, nu pe resize/auto-hide).
- **Coloane înghețate:** o coloană `Frozen` marcată `AutoHide` poate dispărea (flagul decide,
  nu statutul de înghețare). Neobișnuit, dar permis; nefiltrat intenționat în v1.
- **Ordinea de ascundere** e fixă (dreapta întâi). Un câmp de prioritate per-coloană ar putea
  urma dacă e nevoie.
