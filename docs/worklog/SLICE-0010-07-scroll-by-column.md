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
- **Un singur loc de aliniere**: `OnScrollValueChanged` (pentru bara orizontală) apelează
  `SnapHScrollToColumn`, cu gardă de reintrare `_snappingHScroll` (setarea `hScroll.Value`
  re-ridică `ValueChanged`). Acoperă TOATE căile care mișcă bara — tragere de thumb, click pe
  săgeți/șină, `Shift`+rotiță — fără cod separat pe fiecare.
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
- `KBot.Controls.Tests` — **106 passed / 0 failed** (7 noi pentru `ScrollByColumn`).
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
