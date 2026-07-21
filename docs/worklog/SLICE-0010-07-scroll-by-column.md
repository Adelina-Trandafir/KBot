# SLICE-0010-07 — KBotDataView: proprietatea ScrollByColumn

Adăugare cerută după închiderea slice-ului 0010: o proprietate `ScrollByColumn` care face
derularea ORIZONTALĂ să avanseze o coloană întreagă odată, în loc de pixel cu pixel.

## Ce s-a schimbat și de ce

- **`Public Property ScrollByColumn As Boolean`** (implicit False), în partiala `.Layout`. Când
  e True, valoarea barei orizontale se **aliniază la marginile coloanelor** din banda derulată.
  Nu atinge derularea verticală — aceea e deja „pe rând”, prin virtualizare.
- **Aliniere în DIRECȚIA mișcării** (`SnapHScrollToColumn` / `SnappedHValue`): la creștere se
  urcă la marginea următoare (`CeilToColumnStart`), la scădere se coboară la cea precedentă
  (`FloorToColumnStart`). Așa un pas mic — o săgeată, o rotiță — tot avansează o coloană
  întreagă, în loc să se lipească de aceeași margine (capcana unei alinieri „la cea mai
  apropiată”). Valoarea aliniată e limitată la maximul util al barei (`Maximum − LargeChange
  + 1`), deci ultimele coloane rămân accesibile chiar dacă capătul nu cade pe o margine.
- **Aliniere pe două căi, după cum se derulează:**
  - *Săgeți / șină / rotiță* — trec prin `ValueChanged` și se aliniază DIRECȚIONAL (ceil la
    creștere, floor la scădere), cu gardă de reintrare `_snappingHScroll`.
  - *Tragerea thumb-ului cu mouse-ul* — se ascultă evenimentul `ScrollBar.Scroll` (care spune
    CUM se derulează, spre deosebire de `ValueChanged`). Cât timp `ThumbTrack`, derularea e
    **liberă** (pixel cu pixel), altfel alinierea ar smuci thumb-ul înapoi la fiecare mișcare
    a mouse-ului („se refresh-uia oribil, de parcă nu se decidea ce coloană să arate”).
    Alinierea se face DOAR la `EndScroll` (eliberare), la marginea cea mai APROPIATĂ.
  - **Bug de comportament reparat:** prima versiune alinia și în timpul tragerii (tot prin
    `ValueChanged`) => thumb-ul sărea. Corecția e distincția track-vs-release prin `Scroll`.
- **Activarea aliniază pe loc**: setter-ul, când trece pe True, aliniază imediat poziția
  curentă (care putea fi la mijloc de coloană).
- **Re-aliniere după relayout**: `UpdateScrollBars` re-aliniază la final, fiindcă lățimile se
  pot schimba sub auto-size-ul din slice 0013.

## Fișiere atinse

- `src/KBot.Controls/KBotDataView.Layout.vb` — proprietatea + mecanica de aliniere.
- `tests/KBot.Controls.Tests/KBotDataViewScrollByColumnTests.vb` (nou, 7 teste).
- `src/KBot.DevHarness/Internal/DataViewHarnessForm.vb` + `.Designer.vb` — checkbox
  «Derulare pe coloană» care comută proprietatea, ca proba vizuală să o poată exercita.
- `docs/worklog/KBOT_STATUS.md`

## Rezultate teste

- `dotnet build src/KBot.Controls` și `src/KBot.DevHarness` — **0 warnings / 0 errors**.
- `KBot.Controls.Tests` — **111 passed / 0 failed** (12 pentru `ScrollByColumn`: 7 direcționale
  + 5 pentru tragerea thumb-ului — liberă în timpul tragerii, „nearest” la eliberare, inertă
  când proprietatea e off, iar săgețile rămân direcționale după eliberare). Tragerea se
  simulează prin `BeginHorizontalThumbDrag`/`EndHorizontalThumbDrag` (Friend) — evenimentul
  `Scroll` nu se poate ridica headless.
- Grila de test e fixată pe dimensionare MANUALĂ (`AutoSizeColumnsMode = None`,
  `ColumnFillMode = None`) ca marginile de coloană să fie deterministe (5 × 100px => marginile
  0/100/200/300/400, maximul util 300). Acoperit: implicit = pixel; creștere => marginea
  următoare; scădere => marginea precedentă; un pas mic tot avansează o coloană; alinierea nu
  depășește maximul util; activarea aliniază offset-ul curent; o poziție deja pe margine nu se
  mișcă.

## Notă de mediu

Build-ul întregii soluții a raportat `MSB3026` (file-lock) la `KBot.App`/`KBot.App.Tests`
fiindcă Visual Studio ținea aplicația deschisă — contenție de COPIERE, nu eroare de cod.
Proiectele atinse (`KBot.Controls`, `KBot.DevHarness`) compilează curat izolat. `KBot.App` NU a
fost atins (proprietatea e pur aditivă pe control).

## Rămas neverificat

- La fel ca tot slice-ul 0010: **verdictul vizual uman NU a fost dat.** Cum se simte alinierea
  la derulare (thumb/săgeți/rotiță) se validează rulând harness-ul (DevHarness → Controls/UI →
  «KBotDataView — virtualizare + temă») și bifând «Derulare pe coloană». Testele acoperă
  matematica alinierii, nu senzația.
