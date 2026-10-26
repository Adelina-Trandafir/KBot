# SLICE-0113 / 0114-20 — Pagini, ferestre modale și maximizare

Istoric 13–17.10.2026. 0113-01 înlocuiește regula inițială „toate machetele ca pagini, una câte una” cu pagini păstrate deschise și editoare modale; runda 2 scoate modalele din istoric. 0114-20 păstrează cele trei runde ale corecției de maximizare și baza comună KBotModalForm. Probele operatorului în KBOT_TEST preced implementările corective nevăzute pe ecran.

- [0113 — arhitectura inițială](#slice-0113)
- [0113-01 — pagini, Adobe și stivă](#slice-0113-01)
- [0114-20 — cele trei runde modale](#slice-0114-20)

## SLICE-0113 - Fereastra principala AvacontMain si machetele ca pagini (13.10.2026) <a id="slice-0113"></a>

Construit si probat in copia `KBOT_TEST`, adus in `KBOT\src` la 13.10.2026 (~17:15); istoria rundelor e in `KBOT_TEST\MERGE_NOTES.md` (sectiunile 1, 2, 3b, 3c, 3d, 3i). Felie inregistrata dupa unire, ca lucru deja facut. Feliile surori ale aceleiasi arhitecturi: 0114 (ribbon), 0115 (Acasa + panoul lateral), 0116 (registrul de machete), 0117 (actiuni recente), 0118 (Jurnal / Istoric versiuni ca machete), 0119 (E-Factura pe ribbon), 0120 (randul de jos din editorul DDF).

### Ce s-a schimbat si de ce
1. **Fereastra principala noua, `AvacontMain`** (`KBotShellForm`): sus bara de titlu si bara de ocupat (aceleasi instante ale lui `KbotForm`, mutate la rulare prin `KbotForm.TakeHeader`, ca `Handles capBar...` sa mearga in continuare), apoi ribbonul (0114), in stanga panoul lateral (0115), restul = zona de pagini `pnlHost`. `Program.vb` porneste `AvacontMain` in loc de `KbotForm`.
2. **Lista angajamentelor (`KbotForm`) e gazduita**: `PrepareForHosting` (fara chenar, `TopLevel = False`, `Dock = Fill`, `AutoFitToTheme = False`, randul MENIU `pnlHeader` ascuns). Se incarca in spate la pornire (`OnShown`: `_kbot.Show()` sub pagina Acasa), deci selectoarele, arborele, banda FOREXE pornesc ca inainte. `KbotForm.OnLoad` maximizeaza doar cand nu e gazduit.
3. **Decizie: toate machetele se deschid ca pagini, una cate una** (`AvacontMain.ShowPage`, `ShowPageAndWaitAsync`, `ShowMachetaAsync`). Extrase, Grupe, Setari, Clasificatii, Parteneri, editoarele DDF / ORD, legaturile receptiilor (`AsociereForm`), operatiunile necorelate (`CabNoteForm`), E-Factura Vanzare / Cumparare, graficul rezervarilor, asocierea partenerilor din Sumar, catalogul de mesaje (Debug). Apelurile `ShowDialog` au devenit `Await ShowMachetaAsync(...)`: codul de dupa inchidere ruleaza exact ca inainte (citeste `SAuSalvatModificari`). Fara fereastra principala (bancul Debug) totul ramane dialog, ca inainte. **Raman ferestre separate / dialoguri:** casetele mici (alegeri, confirmari, file pickers, `OrdZiuaForm`, `CabNoteReceiptForm`, dialogurile din editoare, «Conturi Unitate», «Date Unitate», tokenul ANAF, sincronizarea facturilor primite), ferestrele de monitorizare (consola FOREXE, istoric FOREXE, coada robotului, informatii interne), `LoginForm`, `DirectorForm`, fereastra de ajutor deschisa cu F1.
4. **Titlul paginii** trece in bara de titlu a ferestrei principale (bara de titlu proprie a paginii se ascunde, `HidePageTitleBar`); la inchiderea paginii revine titlul de dinainte.
5. **Machetele si-au pierdut butoanele proprii de inchidere** (`HideCloseButtons`: `btnInchide`, `btnIesire`, `btnClose`; plus `btnRenunta` la `DdfEditForm` / `OrdEditForm` / `AsociereForm`). Se inchid din butonul «Inchide» al ribbonului (0114); **Esc** inchide in continuare (ascultat pe forma cand butonul de anulare a fost ascuns). «Renunta» al unui nomenclator sau al unei facturi ramane (anuleaza o editare, nu inchide). Inchiderea pastreaza intrebarile fiecarei machete despre modificarile nesalvate (`FormClosing` al ei); o macheta care refuza inchiderea opreste si deschiderea celei noi.
6. **Pornire marita o singura data** (`AppSettings.FullScreenOffered`): la prima pornire dupa schimbare, `StartMaximized` se pune pe True si fereastra porneste marita; apoi bifa e a operatorului.
7. **Adobe / Office gazduite**: `FindForm()` -> `TopLevelForm()` (`KBot.Controls/Adobe/AdobeTopForm.vb`, 13 locuri in 6 fisiere Adobe + `OfficeDocumentHost.vb`): fereastra de sus, nu primul `Form` din lant. Remediaza Ctrl+H, activarea si inchiderea cand lista angajamentelor e gazduita.
8. `HelpService.ShowHelpMenu`: proprietarul popup-ului «?» = fereastra de sus (`TopLevelControl`), fiindca o pagina gazduita nu e fereastra de sus.

### Fisiere
- NOU: `src/KBot.App/AvacontMain.vb` + `.Designer.vb` (partea de pagini; ribbonul si panoul sunt in 0114 / 0115), `src/KBot.App/KbotForm.Host.vb` (`HostForm`, `TakeHeader`, `PrepareForHosting`), `src/KBot.Controls/Adobe/AdobeTopForm.vb`.
- Modificate: `KBot.App/Program.vb`, `KbotForm.vb`, `KbotForm.Chrome.vb`, `KbotForm.HelpCapture.vb`, `KbotForm.Extrase.vb`, `KbotForm.Grupe.vb`, `KbotForm.Ddf.vb`, `KbotForm.Ord.vb`, `KbotForm.Receptii.vb`, `KbotForm.Ingest.vb`, `KbotForm.MessageCatalog.vb`, `Views/SumarView.vb`, `Views/RezervariView.vb`, `Setari/SetariForm.vb` (`ShowFor`: pagina cand e gazduit), `Help/HelpService.vb` (proprietarul popup-ului); `KBot.Common/AppSettings.vb` (`FullScreenOffered` + DTO); `KBot.Controls/Adobe/AcroPdfViewer.vb`, `AcroPdfViewer.CheckLog.vb`, `.LightWatch.vb`, `.ReadMode.vb`, `.SaveTrap.vb`, `.Watch.vb`, `AdobeReaderHost.vb`, `KBot.Controls/Office/OfficeDocumentHost.vb`.

### Teste / verificare
- Dupa unire (MERGE_NOTES, «UNIT in KBOT»): `dotnet build KBot.sln` 0 erori, 0 avertismente. `dotnet test`: 99 teste picate, aceleasi ca pe codul de dinainte (mediu / cai), deci niciunul nou. Rulate de operator, nu in aceasta sesiune.
- Probat pe ecran de operator in `KBOT_TEST`; in `KBOT` nimic rulat in aceasta sesiune.

### Neverificat / amanat
- **Turul initial de ajutor nu mai porneste**: `PornesteTurulInitial` verifica `Visible`, iar shell-ul e ascuns la pornire (Acasa e pe ecran); turul e si ancorat pe `btnMeniu`, ascuns. De reparat separat (Open threads 0113).
- `menuNou` / `btnMeniu` raman in `KbotForm`, doar ascunse: ribbonul le copiaza starea si ruleaza aceleasi handlere. Sterse doar la cererea operatorului.
- FileVersion pentru `KBot.App`, `KBot.Common`, `KBot.Controls` nebumpate (de facut separat).
- `KbotForm` gazduit nu mai creste fereastra cand tema cere fonturi mai mari (`AutoFitToTheme = False`).
- Ajutorul: 0000-69 (machete ca pagini, fara butoane de inchidere).

## SLICE-0113-01 - Pages stay open; editors and answer-windows are modal windows again (16.10.2026) <a id="slice-0113-01"></a>

Corrective of 0113 (operator, 16.10.2026): "toate machetele ca pagini, una cate una" was too much. Two kinds of macheta now.

### What changed and why

1. **Pages stay open.** `AvacontMain` keeps a list of open pages (`_pages`, most recently used last) instead of one `_page`. `ShowPage` no longer closes the page on screen: it puts it away (hidden, still open, what the operator typed stays) and shows the new one; a page that is open already comes back to the front. Pages: Extrase de cont, Clasificatii, Parteneri, Setari, Jurnal, Istoric versiuni, Ajutor, E-Factura Vanzare / Cumparare.
   - `ShowShell` / `ShowHome` no longer close the page on screen either (they hide it); both always return True now.
   - A page that closes while on screen is replaced by the page used before it (`Page_FormClosed`), else by the base with the caption it had. A page closed while put away changes nothing on screen.
   - `OnFormClosing` brings every open page to the front, most recently used first, and closes it, so each one's own question about unsaved changes is seen; one that refuses cancels the closing of the application.
   - `FindPage(Of T)` (used by `SetariForm.ShowFor`), per-page caption bars in `_pageBars`.
   - `Activate()` on an open Extrase / Clasificatii / Parteneri form (a no-op on a hidden hosted page) became `ShowMachetaPage(...)`.
2. **Modal windows are real windows.** DDF / ORD editors, `AsociereForm` (both openers), `CabNoteForm` (Operatiuni necorelate), `GraficRezervariForm`, `MessageCatalogForm` (Debug) and, by its own code (it awaited the close and read `Modificat`), `GrupeForm`: `ShowMachetaAsync` is replaced by `ShowModalAsync` (`AvacontMain.ShowModalAsync`, `KbotForm.ShowModalAsync`) = `ShowDialog(AvacontMain)`: their own caption bar and buttons (not hosted, so `PrepareHosted` / `HideCloseButtons` do not touch them), `StartPosition = CenterParent` on the main window (may cover the side panel), size cut down to the work area (`pnlHost`) minus a 16 px margin (`FitModal`: `MaximumSize`/`MinimumSize`/`Size`). They do not touch `_pages`: when one closes the pages are as they were. The history still records them (`_registry.Note`); `ShowPageAndWaitAsync` and the hosted-modal path are gone.
3. **Adobe is closed before anything covers it** (`IPutAway`, new in `KBot.Common/Machete`). `AvacontMain.PutAwayFront` is called by `PutFrontAway` (new page, page brought back, base switched) and by `ShowModal` (any modal window). It puts away what is on screen: the shell -> `KbotForm.PutAway` -> the active view; a page -> its `IPutAway`. Each implementation moves the **navigation bar's selection** to a tab without a PDF (the bar is never hidden) and lets the documents go (`ReleaseDocument` = `ReaderHostPreview.Clear`, which detaches the host and clears Adobe; the local-copy deletion stays off, `LocalCopyDeleteEnabled = False`):
   | Holder | Moves to | Releases |
   |---|---|---|
   | `DdfView` | `previzualizare` (only when `document` is selected and it is not the print list) | all pages |
   | `OrdView` | `vizualizare` (same rule) | all pages |
   | `NoteCabView` | `vizualizare` (from `document` / `recipisa`) | all pages |
   | `EFacturaView` -> `PrimiteView` | `linii` (from `pdf`) | `pgPdf` |
   | `PrimiteForm` -> `PrimiteView` | same | same |
   | `FacturiForm` | `generale` (from `pdf` / `anaf` / `eroare`) | `pgPdf`, `pgAnaf`, `pgEroare` |
   A failure is logged and does not stop the navigation (`PutAwayFront` catches; the implementations log and rethrow).
   Coming back, the operator is on that tab and picks the PDF tab: the page remounts (`Stamp` / `_shownPath` were reset).

4. **Round 2 (operator, 16.10.2026): «Machete deschise» is a stack.**
   - The modal windows are **not in the history**: `MachetaType` got `InHistory` (default True; False for DDF / ORD editor, Asocieri, Operatiuni necorelate, Grafic, Grupe, Mesaje); `MachetaRegistry.Note` / `OpenAsync` skip those types; `ShowModal` no longer notes anything.
   - `MachetaEntry.IsOpen`; `MachetaHistory` keeps the OPEN entries first, in the order they were opened. An open entry never moves (asking for an open page again only refreshes its title); a new open entry goes after the last open one.
   - A page that closes goes to the END of the list as a closed entry (`MachetaHistory.MarkClosed`, from `Page_FormClosed` through `MachetaRegistry.NoteClosed`; `AvacontMain._pageRefs` remembers each page's reference). Opened again, it leaves the tail and takes its place after the last open one. A page re-asked under another reference (another year) closes the old line.
   - Capacity stays 10 lines: over the limit the CLOSED entries leave, the one closed longest ago first (`TrimClosed`); open ones are never dropped (more than 10 open are all kept). Entries loaded from a store start closed.
   - The list of angajamente (shell) is a macheta that is open while it is the base: `SwitchBase` notes it open / closed. The views of the shell (`view.*`) are NOT in the history (operator, 16.10.2026: only the pages and the list of angajamente stay): their types are registered with `InHistory = False` (`NoteViewOpened` and `OpenAsync` skip them); the reopen code of a view stays registered ("old paths stay").
   - `KBotSidePanelRow.Italic` (new, Line style) and `FillMachete`: closed lines in italics; tooltips «Deschisă…» / «Închisă…».

### Files touched
- NEW: `src/KBot.Common/Machete/IPutAway.vb`.
- Round 2: `KBot.Common/Machete/MachetaRef.vb` (`InHistory`, `IsOpen`), `MachetaRegistry.vb`, `MachetaHistory.vb`; `KBot.Controls/SidePanel/KBotSidePanelItems.vb` + `KBotSidePanel.vb` (`Italic`); `AvacontMain.Sidebar.vb`; `KbotForm.Machete.vb` (types flagged out of the history).
- `src/KBot.App/AvacontMain.vb` (pages list, `OpenNewPage`, `MakeFront`, `PutFrontAway`, `PutAwayFront`, `ShowModalAsync`, `ShowModal`, `FitModal`, `SwitchBase`, `Page_FormClosed`, `OnFormClosing`, caption), `KbotForm.Machete.vb` (`ShowModalAsync`, header), `KbotForm.Views.vb` (`PutAway`), `KbotForm.Ddf.vb`, `KbotForm.Ord.vb`, `KbotForm.Receptii.vb`, `KbotForm.Ingest.vb`, `KbotForm.CabNotes.vb`, `KbotForm.MessageCatalog.vb`, `Views/RezervariView.vb` (renamed call), `KbotForm.Grupe.vb` (modal), `KbotForm.Extrase.vb`, `KbotForm.Nomenclatoare.vb` (back to front), `Setari/SetariForm.vb`.
- `Views/DdfView.vb`, `Views/OrdView.vb`, `Views/NoteCabView.vb`, `Views/EFactura/EFacturaView.vb` (`IPutAway`).
- `src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `Forms/PrimiteForm.vb`, `FacturiForm.vb`, `FacturiForm.Pdf.vb` (`IPutAway`).
- Help: see [0000-86](SLICE-0000-69-ajutor-machete-ca-pagini.md#slice-0000-86).

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 errors, 0 warnings (before the help edits; help edits do not touch code). No tests written or run (project rule). **Nothing was run or seen on screen.**

### Unverified / deferred
- **Everything on screen**: switching between open pages, the caption, the ribbon buttons («Inchide», page buttons), the sizes and the centring of the modal windows, the cut-down size when the designer size is bigger than the work area (and the operator's text-size zoom, which can still enlarge a window past `MaximumSize`).
- **Adobe really closing** on `PutAway`: it rests on `ReaderHostPreview.Clear` detaching the host; whether the Adobe process of K-BOT ends, and how the shared Acrobat broker behaves when a modal editor opens right after, was not observed.
- A modal opened while another modal is open (the robot queue opens `AsociereForm` on its own): owner is the main window, not the first modal. Not tried.
- A running job in a put-away page (E-Factura sync, a download) keeps running while hidden; nothing tells the operator.
- **Year / unit change with pages open**: open pages are not refreshed or closed (e.g. Clasificatii holds the old year). Decision needed.
- Round 2 not seen on screen: the order of the stack, the italics, the trimming at 10. The shell line is open only while the shell is the base.
- Assumptions to confirm: `GrupeForm` is modal (its code awaited the close); Setari and Ajutor stay pages; closing a page brings back the page used before it (not the base).
- The side panel's «Machete deschise» is still the history (not "open now"); a history row of a modal reopens the angajament view, not the modal. The `DdfEditForm` / `OrdEditForm` / `AsociereForm` branch of `HideCloseButtons` is now unused (kept, "old paths stay").
- Tours / tutorials not re-walked (the `tur-ddf-editor` `btnRenunta` stale note of 0113 is now void: the button is back).

## SLICE-0114-20 - Modal windows maximise inside the area of the pages, not at the screen corner (operator report) (17.10.2026) <a id="slice-0114-20"></a>

### What changed and why
A modal window (e.g. «Legaturile receptiilor») opens with `MaximumSize` = the opening size (area of the pages minus a margin). Maximised, Windows put it at the monitor's work-area corner and cut the size to that `MaximumSize`: right size, wrong place. `AvacontMain.FitModal` now sets `MaximumSize` to the whole area of the pages (`pnlHost`, under the ribbon, beside the side panel; a smaller `MaximumSize` of the form itself still wins) and `MaximizedBounds` to that rectangle relative to the monitor (`KBotThemedForm.SetMaximizedBounds`, new public method, because `Form.MaximizedBounds` is Protected). The opening size is unchanged (margin all round).

### Files touched
`src/KBot.App/AvacontMain.vb` (`FitModal`), `src/KBot.Theming/KBotThemedForm.vb` (`SetMaximizedBounds`, `HasMaximizedBounds`), `src/KBot.Theming/Interop/NativeMethods.vb` (`ApplyMinMaxInfo`), `src/KBot.Theming/KBotModalForm.vb` (new) and the Designer base of the seven modal forms.

Second round (operator: «nu pare ca s-a schimbat NIMIC», DDF editor): the editors (DDF, ORD, chart, groups...) are `KBotShellForm`, whose `WM_GETMINMAXINFO` handler replaced the maximised place with the monitor work area after WinForms had written `MaximizedBounds`. `ApplyMinMaxInfo` now leaves the message alone when the form has maximised bounds. The first round only fixed the windows based on `KBotThemedForm` (e.g. «Legaturile receptiilor»). 

### Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

### Unverified / deferred
- Not seen on screen: the maximised position on the main monitor and on a second monitor (`ptMaxPosition` is taken as relative to the monitor's corner, as `NativeMethods.ApplyMinMaxInfo` already does).
- A modal window dragged to another monitor keeps the bounds of the monitor of the main window.
- The operator wrote «fara right-panel»; read as the area of the pages (`pnlHost`), which already leaves out the side panel.
- A modal window can now also be dragged larger than the opening size, up to the area of the pages.

### Third round: one type for every modal window (operator: «TOATE machetele popup modal TREBUIE sa fie acelasi TIP»)
New `KBot.Theming/KBotModalForm` (inherits `KBotShellForm`): the base of every window opened through `AvacontMain.ShowModalAsync`. Moved onto it: `DdfEditForm`, `OrdEditForm`, `GrupeForm`, `GraficRezervariForm` (were `KBotShellForm`) and `AsociereForm`, `CabNoteForm`, `DevHarness.MessageCatalogForm` (were `KBotThemedForm`, so they had no resize band and no maximise handler). `ShowModalAsync` / `ShowModal` / `FitModal` now take a `KBotModalForm`, so a modal window of another type no longer compiles. Whole solution builds: 0 warnings, 0 errors. Not seen on screen: the resize band now present on «Legaturile receptiilor» and the CAB notes window.

Trasabilitate ajutor: 0113 → 0000-69; 0113-01 → 0000-86; 0114-20 → 0000-92.
