# SLICE 0072-01 — Setări: pagina «Jurnal», dialogul ferestrei găzduite Adobe, cursorul cu opriri, meniurile barei de titlu

**Data:** 20.09.2026
**Cererea operatorului:** «in the setari: 1. i added a new button - Jurnal. that needs to show the
journal form (but make it a view) … the elements in the DGV must be sorted descending. the mesaj
column must be hidden and that info only shown in the bottom txtDetaliu (which must be a custom
Textbox). 2. in aplicatie page: the extra settings for the pdf opening MUST NOT be on the view, but
when the motor previzualizare is set to Fereastra Gazduita - a new small popup form will open with
the extra settings. 3. in temaview, the slider for the text size must have a few snapping points (at
100%, 110% and 125%). in capbar custom control: the themeview button will ONLY show the slider (if
using the "font din tema") also with the snapping points and the existing themes. that's all. the
setari button will only show the Jurnal (if enabled from setari) and the Setari option. if jurnal is
not enabled, then the setari form will open directly without a popup menu. also, the two options will
have icons and a separator»

## Presupuneri declarate

- **Numărul feliei: 0072-01** — a doua trecere peste fereastra «Setări» (0072), nu o felie nouă;
  «next free» rămâne 0073.
- **`LogViewerForm` rămâne**, dar goală pe dinăuntru: bara de titlu + pagina nouă andocată. O
  folosesc în continuare launcher-ul de pornire («Jurnale», fără shell și fără login) și bancul de
  probă (`ILogViewerLauncher`); shell-ul nu o mai deschide. Testele ei existente merg mai departe
  prin cârligele `Debug*`, înaintate una la una către pagină.
- **«Sincronizare (server)» a plecat din meniul butonului de opțiuni** — operatorul a spus «only
  the Jurnal … and the Setari option». `KbotForm.SincronizeazaAsync` a rămas în cod, dar nu mai are
  nicio cale de apel din interfață; de hotărât de operator dacă primește alt loc sau se șterge.
- **«Font din temă» s-a mutat** din meniul butonului de temă în pagina «Temă», lângă cursor: meniul
  «will ONLY show the slider (if using the font din tema) … and the existing themes», iar
  comutatorul trebuie totuși să existe undeva, altfel cursorul n-ar mai putea fi readus.
- **«Opțiuni temă…» și «Stiluri…» au plecat din meniu** fără înlocuitor: prima e pagina «Temă»
  însăși (portul din 0072); a doua (`ThemeEditorForm`, excepțiile pe controale) rămâne în cod fără
  cale de apel — thread deschis.
- **Un buton «Opțiuni fereastră găzduită…» sub combo-ul motorului** (activ doar pe «Fereastră
  găzduită»): cererea spune că setările NU stau pe pagină și că dialogul se deschide când se
  alege motorul; fără buton operatorul n-ar mai fi putut ajunge la ele fără să comute motorul
  dus-întors.
- **Toleranța de lipire: 3 unități** (procente): 97–103 → 100, 107–113 → 110, 122–128 → 125.
  Doar la mouse; săgețile merg în pași de 5 și nimeresc singure punctele. Aceleași trei puncte în
  meniu (`KBotCaptionBar.TextScaleSnapPoints`) și în pagina «Temă».

## Ce s-a schimbat și de ce

### 1. `KBot.Controls` — cursorul cu puncte de oprire; meniul de temă redus

- `Popup/CustomPopupItem.vb`: `SliderSnapPoints As Integer()` (gol = liber), `SliderSnapTolerance = 3`,
  `SnapSliderValue(v)` (regula pură), fabrica `Slider(key, text, min, max, value, snapPoints)`
  (punctele din afara intervalului se aruncă, dublurile se strâng, lista iese sortată).
- `Popup/CustomPopup.Slider.vb`: `SliderValueAt` trece valoarea brută prin `SnapSliderValue`
  (mouse); `DrawSliderRow` desenează liniuțe fine în culoarea conturului la fiecare punct, SUB
  deget și sub partea parcursă; `SliderXForValue` e inversa lui `SliderValueAt`, ca liniuțele și
  degetul să stea pe același calcul.
- `CaptionBar/KBotCaptionBar.ThemeButton.vb`: `ConstruiesteElementeleMeniului` = cursor (dacă
  `ShowTextScaleSlider` ȘI `ThemeManager.WritesFormFont`) + separator (doar când există ambele
  părți) + schemele alegibile. Au plecat: rândul «Font din temă» (bifa desenată, `BifaPentru`),
  «Opțiuni temă…», «Stiluri…», constantele lor, `DeschideOptiunileDeTema`,
  `DeschideEditorulDeStiluri`. **Proprietățile `ShowThemeOptions` și `ShowThemeEditor` nu mai
  există** — șterse, nu lăsate inerte (o proprietate care nu face nimic e no-op-ul tăcut interzis);
  cele 12 fișiere `.Designer.vb` care le scriau au fost măturate (11 în `KBot.App`, `MigratorForm`
  în `KBot.Migrator`). `TextScaleSnapPoints = {100, 110, 125}` e public, ca pagina «Temă» să
  citească aceleași valori.
- `CaptionBar/KBotCaptionBar.md`, `Popup/CustomPopup.md`: actualizate.

### 2. `KBot.App` — pagina «Jurnal» și `LogViewerForm` ca simplu înveliș

- `Setari/SetariJurnalView.vb` + `.Designer.vb` (noi): corpul fostului `LogViewerForm` ca
  `KBotThemedUserControl` care implementează `ISetariView` + `IThemedContainer`. Diferențe față de
  fereastră: **(a)** grila e ordonată **descrescător** pe marcajul corectat
  (`OrdoneazaCeleMaiNoiPrimele`, stabilă; intrările fără dată la coadă, în ordinea fișierului) —
  sortarea se face o singură dată, după încărcare, pe același drum pentru fișiere locale, server și
  cârligul de test; **(b)** coloana «Mesaj» **nu mai există** — mesajul se citește întreg în
  `txtDetaliu`, care e acum `KBotTextBox` (Consolas, ambele bare de derulare tematizate, fără
  împachetare); **(c)** clientul API e proprietatea `ApiClient`, nu un argument de constructor, ca
  designerul gazdei să poată construi pagina cu constructorul fără parametri; **(d)** prima
  `Activated()` construiește lista și încarcă «Toate fișierele», următoarele recitesc selecția
  (jurnalele au crescut cât timp era altă pagină pe ecran); **(e)** starea și ocuparea se ridică și
  către banda ferestrei «Setări» (`StatusChanged` / `BusyChanged`); **(f)** identificatorul
  `AratăGol` (cu diacritic, moștenit) a devenit `ArataGol`. Comentariile sunt în engleză; numele
  controalelor și ale metodelor private au rămas cele din fereastra veche (cod mutat, referit de
  teste și de cerere — `txtDetaliu`).
- `Views/LogViewerForm.vb` + `.Designer.vb`: rescrise ca înveliș — `pnlRoot` cu `capBar` sus și
  `jurnal As SetariJurnalView` andocat; `OnLoad` cheamă `jurnal.Activated()`; cele nouă cârlige
  `Debug*` înaintează către pagină (+ `DebugIntrareaRandului(index)`, nou).
- `Setari/SetariForm.vb`: constructor cu `apiClient As IApiClient` (DI îl rezolvă singur);
  `CreateView("jurnal")`; `ShowPage(key)` public (cheie goală → `ArgumentException`).
  `SetariForm.Designer.vb`: rândul «Jurnal» + separatorul, adăugate de operator (necomise), au
  rămas cum le-a pus.
- `KbotForm.vb`: meniul butonului de opțiuni = «Arată jurnal» (pictograma editorului de text) +
  separator + «Setări…» (pictograma de setări); cu `LogViewerEnabled` stins **nu se deschide niciun
  meniu**, clicul deschide direct fereastra «Setări». `ShowLog()` = `SetariForm.ShowFor(...).ShowPage("jurnal")`
  (aceeași fereastră nemodală, o singură instanță). Câmpul `_logViewer`, `OPT_SINCRONIZARE` și
  rândul lui au plecat; `MeniuOptiuni_ItemClicked` nu mai e `Async`.

### 3. `KBot.App` — dialogul ferestrei găzduite Adobe

- `Setari/AdobeGazduireForm.vb` + `.Designer.vb` (noi): `KBotThemedForm` cu bara de titlu, o
  explicație și cele patru setări (mod vizualizator, instanță nouă `/n`, eliberarea ferestrei,
  fereastra plutitoare), «Renunță» / «Salvează». Valorile se citesc la `Load`, nu în constructor;
  se scriu **doar pe «Salvează»**, în ambele magazine (`AdobeViewerSettings.Persist` cu motorul
  `WindowHost` + `AppSettings` clonat și salvat); `Rezumat` poartă o linie pentru banda ferestrei.
  `DetachItem` s-a mutat aici (era privat în pagină).
- `Setari/SetariAplicatieView.vb` + `.Designer.vb`: din secțiunea «Documente» au plecat cele patru
  rânduri (`cboAdobeMod`, `cboAdobeInst`, `cboAdobeDetach`, `chkAdobePopup` + etichetele); a rămas
  combo-ul motorului și, sub el, `btnAdobeGazduire` («Opțiuni fereastră găzduită…», activ doar pe
  `WindowHost`). Alegerea motorului salvează motorul (celelalte două valori din `kbot_paths.json`
  se recitesc din magazin, neatinse) și, dacă e «Fereastră găzduită», deschide dialogul modal
  (peste fereastra «Setări»). La încărcarea paginii (`_suppress`) dialogul NU se deschide.
  Tabelul a scăzut de la 6 la 3 rânduri; secțiunea «Foldere» s-a mutat în sus cu 122 px.

### 4. `KBot.App` — pagina «Temă»

- `Setari/SetariTemaView.vb` + `.Designer.vb`: `chkThemeFont` («Font din temă», lângă valoarea
  cursorului) legat la `ThemeManager.WritesFormFont` (setterul persistă și difuzează). Cursorul
  `TrackBar` se lipește pe `Scroll` (gestul), nu pe `ValueChanged` (valoarea din magazin trebuie să
  cadă exact unde spune magazinul): `Lipeste(v)` e regula pură, `_snapping` oprește reintrarea.

### 5. Versiuni

`KBot.App` 1.0.30 ▸ **1.0.31**, `KBot.Controls` 1.50 ▸ **1.51**, `KBot.Migrator` 1.9 ▸ **1.10**
(doar `MigratorForm.Designer.vb` măturat). `AssemblyVersion` neatins.

## Fișiere atinse

**Noi:** `src/KBot.App/Setari/SetariJurnalView.vb`, `SetariJurnalView.Designer.vb`,
`AdobeGazduireForm.vb`, `AdobeGazduireForm.Designer.vb`, `docs/worklog/SLICE-0072-01-setari-jurnal-si-meniuri.md`.

**Modificate:** `src/KBot.Controls/Popup/CustomPopupItem.vb`, `CustomPopup.Slider.vb`, `CustomPopup.md`,
`src/KBot.Controls/CaptionBar/KBotCaptionBar.vb`, `KBotCaptionBar.ThemeButton.vb`, `KBotCaptionBar.md`,
`src/KBot.Controls/KBot.Controls.vbproj`; `src/KBot.App/KbotForm.vb`, `KbotForm.Designer.vb` (pictograma
butonului de opțiuni — schimbarea operatorului), `Setari/SetariForm.vb`, `SetariForm.Designer.vb`,
`Setari/SetariAplicatieView.vb`, `SetariAplicatieView.Designer.vb`, `Setari/SetariTemaView.vb`,
`SetariTemaView.Designer.vb`, `Views/LogViewerForm.vb`, `LogViewerForm.Designer.vb`, `My Project/Resources.*`
(+ două PNG-uri noi în `Resources/`, aduse de operator), `KBot.App.vbproj`; măturarea
`ShowThemeEditor/ShowThemeOptions` în `DDF_EDIT/DdfEditForm`, `Forexe/AlegereUnitateForm`,
`Forexe/AsociereBenziForm`, `Forexe/AsociereForm`, `Forexe/GraficeAsociereForm`, `ORD_EDIT/OrdEditForm`,
`Views/GraficRezervariForm`, `Views/Ord/OrdTextForm`, `Views/Ord/OrdZiuaForm`, `LoginForm` (toate
`.Designer.vb`), `src/KBot.Migrator/MigratorForm.Designer.vb`, `KBot.Migrator.vbproj`;
`docs/SETARI_UTILIZATOR.md` §6; `docs/worklog/KBOT_STATUS.md`.

**Teste (scrise, NERULATE — regula casei):** `tests/KBot.Controls.Tests/KBotCaptionBarThemeButtonTests.vb`
(trei teste ale uneltelor rescrise ca `Meniul_are_doar_cursorul_si_schemele`,
`Cursorul_de_marime_are_punctele_de_oprire`, `Fara_cursor_raman_doar_schemele`; teoria de designer și
serializarea explicită fără cele două proprietăți șterse), `tests/KBot.Controls.Tests/CustomPopupSliderTests.vb`
(+ `Valoarea_se_lipeste_de_punctele_de_oprire` ×9, `Fara_puncte_de_oprire_valoarea_ramane`,
`Punctele_de_oprire_se_curata_la_construire`, `Mouse_ul_se_lipeste_de_punctul_apropiat`,
`Sagetile_nu_se_lipesc`; `Meniul_de_tema_incepe_cu_cursorul_de_marime` fără rândul de font),
`tests/KBot.App.Tests/LogViewerFormTests.vb` (cinci coloane fără «mesaj», `txtDetaliu` e `KBotTextBox`,
+ `Randurile_SuntOrdonateDescrescator_CeleMaiNoiPrimele`).

## Rezultate

- `dotnet build` pe `src\KBot.Controls` (Debug), `src\KBot.App` (Debug + Release),
  `src\KBot.Migrator` (Release), `src\KBot.Forexe.Editor` (Release): **0 erori, 0 avertismente**.
- Proiectele de test **nu au fost construite și nu au fost rulate** (regula casei); testele noi și
  cele rescrise sunt scrise, nu dovedite.
- **Văzut prin `DrawToBitmap`, în afara monitorului** (un proiect de o clipă în scratchpad, șters
  după): `LogViewerForm` (deci pagina «Jurnal») pe Classic / Dark / Modern — cinci coloane, panoul
  de detaliu cu chenarul casei; `AdobeGazduireForm` pe cele trei scheme; `SetariTemaView` cu bifa
  «Font din temă» lângă cursor; `SetariAplicatieView` cu secțiunea «Documente» la trei rânduri și
  butonul sub combo; meniul de temă (`CustomPopup`) cu cursorul la 110 %: liniuțele de la 100 și
  125 se văd de o parte și de alta a degetului, cea de la 110 e sub el.

## Nerulat / neverificat / amânat

- **Nimic apăsat cu mâna**: tragerea cursorului (lipirea la mouse), deschiderea dialogului Adobe la
  alegerea motorului, meniul butonului de opțiuni cu / fără «Arată jurnal», comutarea paginii
  «Jurnal» din shell — toate numai prin cod și randări statice.
- **`SincronizeazaAsync` fără cale de apel** din interfață (vezi presupunerile). De hotărât.
- **`ThemeEditorForm` («Stiluri…») și `ThemeOptionsForm` fără cale de apel** din interfață;
  `ThemeOptionsForm` e dublată de pagina «Temă», `ThemeEditorForm` nu are înlocuitor.
- Pagina «Jurnal» în fereastra «Setări» (946 × 735 la 150 %) e mai strâmtă decât fostul
  `LogViewerForm` (1173 × 869): coloana fișierelor e fixată la 250 px, restul curge; de văzut pe
  ecran dacă rândul de filtre are loc la lățimea minimă a ferestrei (1000 px).
- Terminațiile de linie: câteva `.Designer.vb` erau deja cu terminații amestecate (salvări VS);
  git le normalizează la comitere.
