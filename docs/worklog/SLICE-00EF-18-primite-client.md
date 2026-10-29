# SLICE-00EF-18…22 — Facturile primite: vedere, fereastră și legare

18 introduce controlul comun și vederea DDF; 19 adaugă fereastra tuturor facturilor, iar 20 închide restanța selectorului manual. 21 restrânge fila la facturi existente prin două cereri; 22 înlocuiește acele cereri cu steagul ArePrimite la încărcarea arborelui. 29 înlocuiește căutarea separată și starea citită, 30 mută sincronizarea în ribbon, 31 scoate Atașamente/Observații și schimbă salvarea PDF, 32 introduce rezultatul sincronizării, 33 îl mută în control/subsol, 35 adaugă opțiunile, iar 36 adaugă XmlViz. [34/37](SLICE-00EF-34-customPopup-combo-casuta.md) păstrează popup-ul și recitirea de pe serverul K-BOT. „Fără mesaj la succes” și „fără recitirea listei” sunt checkpointuri înlocuite de etapele ulterioare. 40 scoate contorul și anul din etichetele arborelui, cu revenirea anului pentru liste multianuale ca alegere de implementare neconfirmată. Fiecare etapă păstrează probele, limitele și ajutorul ei; amplasările ulterioare din ribbon sunt în 0114/0119, nu reguli concurente cu meniul inițial.

[00EF-18](#slice-00EF-18) · [00EF-19](#slice-00EF-19) · [00EF-20](#slice-00EF-20) · [00EF-21](#slice-00EF-21) · [00EF-22](#slice-00EF-22) · [00EF-29](#slice-00EF-29) · [00EF-30](#slice-00EF-30) · [00EF-31](#slice-00EF-31) · [00EF-32](#slice-00EF-32) · [00EF-33](#slice-00EF-33) · [00EF-35](#slice-00EF-35) · [00EF-36](#slice-00EF-36) · [00EF-40](#slice-00EF-40)

## SLICE-00EF-18 -- Received e-invoices, client half (domain, API, view in the shell, sync dialog) <a id="slice-00EF-18"></a>

Decisions of the operator are in `SLICE-00EF-17-primite-server.md`. Server: 00EF-17.

### Done
- `KBot.Domain/EFacturaPrimite.vb` (POCOs), `IEFacturaApi` + `ApiClient.EFactura.Primite.vb` (sync, list, detail, mark read, xml, zip, pdf, embedded file, link / unlink). Wire names are the server's.
- `KBot.EFactura/Views/Primite/PrimiteView` (+ designer): tree (root «Toate facturile» > months > invoices; orange dot = never opened), horizontal bar Linii / Factura PDF / Atasamente / Mesaje (the last two only when the invoice has some), right click «Salveaza ca ZIP / XML» and, with a DDF, «Leaga de acest DDF» / «Scoate legatura». Pages: `PrimiteLiniiPage` (lines + VAT per rate), `PrimiteAtasamentePage` (double click saves a file), `PrimiteMesajePage` (XML notes + messages); the PDF page is `VanzarePdfPage` (the embedded classic PDF if the XML carries one, else ANAF's drawing). Made for reuse: the separate screen (00EF-19) puts the same control in a window.
- `KBot.App/Views/EFactura/EFacturaView` (+ designer): the shell view «E-Factura» (nav item, Far side, shown with `AreDDF`); finds the DDF of the angajament (`AntetDeLucru(0).Iddf`; more than one header -> the first, logged) and calls `PrimiteView.LoadAsync(iddf)`.
- `KBot.EFactura/Forms/SincronizarePrimiteForm`: period 7/15/30/45/60 days, repeats the sync call (20 per call) with a progress line until nothing is left or a call brings nothing; no figures box on success; problems in a notice. Opened from the title-bar menu of `FacturiForm` («Sincronizeaza facturi primite»).
- Help 0000-61 (`contabil/efactura/index.md`: «Facturile primite», «Sincronizarea facturilor primite»).

### Verified / not
- Build of `KBot.Api`, `KBot.EFactura`, `KBot.App`: 0 errors, 0 warnings. NOTHING ran: not on screen, not in the VS designer, not against the server (server not deployed). No tests written (not asked).
- Assumptions to check on first run: the DDF view's «rev 0» is just «a DDF exists» (operator); `AdvancedTreeControl.NodeMouseUp` raises for the right button too (the invoice window relies on it); `KBotComboBox.Items.AddRange` in the dialog's designer.
- Server prerequisites: deploy 00EF-17 files, run `sql/00EF_17_primite_tva.sql` (the asocieri DDL is already run) + schema sync.

### Not done (next)
- 00EF-19: the separate window with all received invoices (same control, no DDF), opened from the menu.
- Manual link only from the DDF view's «all» list; a picker of DDFs from the separate screen is not built.
- Message to the supplier (`TrimiteMesajFactura`): later (operator).

## SLICE-00EF-19 -- Received e-invoices, the separate window <a id="slice-00EF-19"></a>

Continues 00EF-17 (server) and 00EF-18 (client). Operator decision: a separate screen with all the received invoices.

### Done
- `KBot.EFactura/Forms/PrimiteForm` (+ designer): `KBotShellForm` with the same `PrimiteView` as the DDF view, without a DDF: every received invoice of the year chosen in K-BOT (`SessionContext.An`), months in the tree, views on the right. Bottom bar: Iesire (left), Reimprospateaza and Sincronizeaza cu ANAF (right; the latter opens `SincronizarePrimiteForm` and reloads when something was added).
- `PrimiteView`: search box `txtCauta` above the tree (visible only without a DDF): narrows the tree on supplier, tax code or number, client side.
- Opened from the title-bar menu of `FacturiForm` («Facturi primite»), one at a time (asking again brings it forward). `FacturiForm` now keeps the PDF viewer factory it was given.
- Help 0000-62 (`contabil/efactura/index.md`, «Fereastra Facturi primite»).

### Verified / not
- Build of `KBot.EFactura` and `KBot.App`: 0 errors. Nothing ran, nothing seen on screen, not opened in the VS designer.

### Not done
- Linking an invoice to a DDF from this window (needs a DDF picker); the link is made from the DDF view («Arata toate facturile primite»).
- Year choice inside the window (it follows K-BOT's year, as the issued-invoice window does).
- Message to the supplier (`TrimiteMesajFactura`): later.

## SLICE-00EF-20 -- DDF picker for the manual link of a received invoice <a id="slice-00EF-20"></a>

Operator request, 09.10.2026 («hai sa facem si selectorul»). Closes the open item of 00EF-19.

### Done
- Server: `primite.lista_ddf` + `GET /api/efactura/primite-ddf?q=` (the newest 300 `FX_DDF`: id, angajament, object, partner, tax code; `q` narrows). `py_compile` + the SQL shape checked with a fake cursor; NOT run against MariaDB.
- Client: `EFacturaDdfAlegere`, `IEFacturaApi.GetDdfAlegereAsync`, `ApiClient.EFactura.Primite`.
- `KBot.EFactura/Forms/AlegeDdfForm` (+ designer): search box, grid of fundamentari (double click picks), «Leaga» (right) / «Renunta» (left).
- `PrimiteView` without a DDF (the window of all the invoices): the invoice menu gets «Leaga de un DDF…» and, once the invoice has been shown (clicked), «Scoate legatura cu <angajament>» for every link the operator made. After a change the list and the invoice are read again.
- Help 0000-63.

### Verified / not
- Build of `KBot.Api`, `KBot.EFactura`, `KBot.App`: 0 errors. Nothing ran or was seen on screen. The server must be redeployed (new route).
- Choice made: the picker lists the newest 300; typing asks the SERVER (350 ms after the last key, a newer key cancels), so older fundamentari are found too.

## SLICE-00EF-21 -- the tab «E-Factura» only for angajamente with received invoices <a id="slice-00EF-21"></a>

Operator request, 09.10.2026.

### Done
- `KbotForm.Views.vb`: `ApplyViewGating` shows the tab only when the DDF exists (`AreDDF`) AND the last check for THIS angajament found invoices; it starts `VerificaFacturiPrimite` on every gating.
- `KbotForm.EFactura.vb`: `VerificaFacturiPrimite` (async Sub, logs and swallows): reads the DDF of the angajament, then `GET /primite?iddf=` (linked invoices, automatic or manual); shows or hides the tab; a newer question cancels the older answer; if the operator is on the tab and it goes, back to «Sumar».
- Cost: two requests per change of angajament with a DDF (the DDF read is the one the DDF view does too). A failure leaves the tab as it was and is logged.
- Help 0000-64.

### Verified / not
- Build of `KBot.App`: 0 errors. Nothing ran. The tab is hidden until the first answer arrives (a short moment after a selection); an angajament changed while on the tab drops to «Sumar» first, as with any view that closes.

## SLICE-00EF-22 -- the tab «E-Factura» from a tree flag (no extra requests) <a id="slice-00EF-22"></a>

Replaces the client-side check of 00EF-21 (two requests per selection: the DDF read and the list).

### Done
- `PYTHON/routes/forexe/tree.py`: new column `ArePrimite` = EXISTS(partner of the angajament's DDF whose normalised tax code is in `EF_Primite.CuiNormalizat`) OR EXISTS(row of `EF_PrimiteAsocieri` for the DDF). Same normalisation as `routes/efactura/primite.py`. `0` when `EF_Primite` / `EF_PrimiteAsocieri` / `FX_DDF_Parteneri` are not all there (checked in information_schema, like the notes tables).
- Client: `AngajamentTreeInfo.ArePrimite`, wire `ArePrimite`; `ApplyViewGating` shows the tab when `AreDDF AndAlso ArePrimite`; `VerificaFacturiPrimite` and its fields removed.
- Help 0000-65.

### Verified / not
- `py_compile`; the generated SQL read (5 bind markers as before, both shapes). NOT run on MariaDB (REGEXP_REPLACE in the tree query, join cost). Build of `KBot.App`: 0 errors. Nothing seen on screen. Server must be redeployed.
- Like `AreDDF` the flag is filled when the tree is loaded: new invoices or a new link show the tab at the next refresh of the tree.

## SLICE-00EF-29 - E-Factura received invoices: tree by supplier, search in the header, «Continut» view reworked (15.10.2026) <a id="slice-00EF-29"></a>

Slice number: next free after 00EF-28 (checked with grep: 00EF-29 was not used anywhere). The help for it is recorded as **0000-75** in this same file (sub-slice of 0000, as the rules ask).

### What changed and why
Operator request of 15.10.2026, eight points, all in the received-invoices control `PrimiteView` (used by the «E-Factura» view of the DDF and by the window «Facturi primite»):

1. **Tree-header search replaces the separate box.** Removed `txtCauta`, its handler, `_filter` and `ShownItems`. `HeaderSearchIcon` uses the standard band («... tastează minim 3 caractere ...»): 3 characters, Esc clears, matching node captions (supplier, date, number).
2. **Right header button for filter options**: `HeaderRightIcon` (filter picture) + `Tree_HeaderRightIconClicked`. Nothing to put in it yet, so the menu has ONE disabled line («Opțiunile de filtrare vor fi adăugate aici.») instead of a silent no-op.
3. **Grouping**: Toate facturile > MONTH > SUPPLIER > invoice. Supplier key = `CuiNormalizat` (the tax code, which the server's list SQL already returns, `EF_Primite.CuiNormalizat`); an invoice without a code falls back to the name written on it. The name shown is the one of the supplier's newest invoice. Suppliers in name order, invoices newest first. Leaf caption: `dd.MM.yyyy · [NC ]number ~~~ total`. Supplier row has a tooltip (count and total).
4. **Text no longer cut:** removed fixed `LeftTextWidth = 100`, which truncated «Toate facturile (3 ...». `RightTextWidth = 96` reserves the whole right-aligned value (2 decimals, `~~~`, no columns); left text uses the rest.
5. **Opening an invoice** keeps the tree and position. Operator decision, same day: read/new state disappears; the dot colour «will mean something else». Removed `MarkReadAsync`, `_markedRead` and tooltip «Noua»; the server no longer marks read (`MarkPrimitaCititaAsync` remains unused in the API, `EF_Mesaje.Nou` stays 1). `DotOf` returns NEUTRAL `FacturaIcons.StareNeutra` (theme dim colour); the DDF view retains the grey ring for an unlinked invoice.
6. **Taxable-base grid removed**: `gridCote` (and its columns, footer «TOTAL TVA») gone from `PrimiteLiniiPage`, and its filling from `PrimiteView.FillLines`. The model (`EFacturaPrimitaDetaliu.Cote`) and the classic PDF keep the VAT per rate.
7. **Lines grid = 60% of the height**: `PrimiteLiniiPage` is now a `KBotTableLayoutPanel` (`tlyPagina`), rows 40% / 60%.
8. **Supplier and notes on top** (the 40%): `lblFurnizor` (`KBotHtmlLabel`: name in bold, cod fiscal, adresa, scadenta, referinta for a credit note, accounts) and, under it, `rtbNote` (`KBotRichTextEditor`, read-only, header/footer hidden) with the notes of the XML joined. All text is HTML-encoded. «Observatii» view stays as it was.

Pictures: `src/KBot.EFactura/Resources/search.png` (copy of the main window's magnifier) and `filter.png` (the filter picture of KBot.App), added to `Resources.resx` and `Resources.Designer.vb` by hand.

Help (0000-75): `contabil/efactura/index.md`, sections «Facturile primite» and «E-Factura Cumparare: «Facturi primite»», tags `00EF-29, 0000-75`; `help-version.txt` = `2026-10-15`.

### Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `src/KBot.EFactura/Views/Vanzare/FacturaIcons.vb`, `PrimiteView.Designer.vb`, `PrimiteLiniiPage.vb`, `PrimiteLiniiPage.Designer.vb`, `src/KBot.EFactura/My Project/Resources.resx`, `Resources.Designer.vb`, `src/KBot.EFactura/Resources/search.png`, `filter.png`, `src/KBot.App/HelpContent/contabil/efactura/index.md`, `src/KBot.App/HelpContent/help-version.txt`, `docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`.

### Test results
- `dotnet build src/KBot.EFactura` and `src/KBot.App`: 0 warnings, 0 errors.
- `Check-Help.ps1 -Coverage`: nothing wrong in `efactura/index.md` once the slice `0000-75` is in the status; the remaining errors are old ones (tours / tutorials: `tur-setari`, `efactura-factura-noua`, `ordonantare-din-plata`).
- No tests added or run (operator rule). The window was NOT opened: nothing here was seen on screen.

### Unverified / deferred
- **Not seen on screen**: the 40 / 60 split, the height of `lblFurnizor` (fixed 96 px logical, a long address wraps and may be clipped), the width of the reserved value (96) at 150% scale, the search band inside the tree of this control.
- The filter menu is empty on purpose; the filter options themselves are not defined yet.
- The dot has no meaning yet (neutral). What its colour will say is a decision of the operator; `StateDot` / `DotOf` are the only places to change. The server's «new» flag is untouched and no longer read by this screen.
- Grouping is done in the client over the list the server returns (up to 2000 rows, `MAX_LIST`); no SQL was changed. If «SQL grouping» meant a server-side `GROUP BY`, say so.
- The «Observatii» view repeats the notes now shown above the lines; kept, not asked to remove.

### Captures to redo
- `efactura-cumparare` (tag in `contabil/efactura/index.md` has `redo:` set).

### Follow-up (15.10.2026, same day)
- Supplier block font 12 pt Calibri (was the default, too small); `lblFurnizor` 120 px logical, `rtbNote` the rest.
- **Display-only note cleanup:** `PrimiteObservatiiPage.CleanNote` / `CleanNotes` splits CR/LF, trims lines, drops empty lines/notes and joins notes with one newline. Used above the lines and in «Observatii» rows/full text; visibility counts only nonempty notes. Stored XML remains unchanged.
- Build KBot.App: 0 warnings, 0 errors; not seen on screen.
- Second look (same day): supplier block 11 pt (was 12), notes text 10 pt (`rtbNote.EditorFont`, the editor's default was 11), the two side by side in `tlySus` (supplier 42% | 6 px gap | notes 58%, replaces `pnlFurnizor`); the lines grid font 10 pt (was the ambient Calibri 9; row height 28 px unchanged). Build KBot.App: 0 warnings, 0 errors; not seen on screen.

## SLICE-00EF-30 - Received invoices: sync from the ribbon, no dialog; view polish (15.10.2026) <a id="slice-00EF-30"></a>

E-Factura side of the request; the ribbon side (fields, order, conditional visibility) is [0114-01](SLICE-0114-ribbon.md#slice-0114-01).

### What changed and why
1. **Tooltips off the lines grid**: `PrimiteLiniiPage` no longer sets tooltips on the grid (nor on the supplier block / notes box, whose «Furnizorul» tip covered the notes).
2. **Bar of views as high as the header of the tree**: `navDetaliu` 40 -> 30 (= `tree.HeaderHeight`).
3. **Gap bar / tree = gap grid / tree**: the bar sits in `pnlNav` with a 3 px side padding, which is what the grids have (`Padding(3)` of the pages), so the bar starts where the grids do.
4. **«Sincronizeaza cu ANAF» button of the page removed** (`PrimiteForm`: control, handler, designer). The old dialog `SincronizarePrimiteForm` is no longer opened by anything: also the item «Sincronizeaza facturi primite» of the old title-bar menu of `FacturiForm` is gone (the class files stay, unused).
5. **Sync without a dialog**: `PrimiteForm.SyncFromAnafAsync(days)` (same batch loop as the dialog: 20 per call until nothing is left or a batch added nothing); progress and the result on the bottom line of the page (`lblStare`); the list is read again when something was added; unreadable messages are told in one warning box; no box on success. `KbotForm.SyncPrimite(days)` calls it (the old `OpenSincronizarePrimite` and the `ef_primite_sync` case of `RunEFacturaCommand` are gone).
6. **Days**: `AvacontMain.ReadSyncDays`: the text box when filled (1-60, anything else = a warning and no sync), else the combo (7, 15, 30, 45, 60; 7 by default).

### Files touched
`src/KBot.EFactura/Forms/PrimiteForm.vb`, `PrimiteForm.Designer.vb`, `src/KBot.EFactura/FacturiForm.Designer.vb`, `FacturiForm.Furnizor.vb`, `src/KBot.EFactura/Views/Primite/PrimiteView.Designer.vb`, `PrimiteLiniiPage.Designer.vb`, `src/KBot.App/KbotForm.EFactura.vb`, `AvacontMain.vb`; help: `contabil/efactura/index.md`, `contabil/ribbon.md` (0000-76).

### Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. No tests run. Nothing run or seen on screen; no real sync against ANAF.

### Unverified / deferred
- Nothing was run: the sync from the ribbon, the progress line and the 30 px bar are not seen.
- While a sync runs the ribbon button can be pressed again: `_busy` ignores the second press without any message.
- If the operator leaves the page during a sync the loop goes on (it stops by itself when the form is disposed after the next call).
- The page of the received invoices must be the one on screen for the button to exist; the DDF view has no sync button.

## SLICE-00EF-31 - Received invoices: no «Atasamente» / «Observatii» views; the operator chooses where a PDF is saved (15.10.2026) <a id="slice-00EF-31"></a>

### What changed and why
1. **Removed «Atasamente» / «Observatii» views** from shared `PrimiteView`: nav items, `pgAtasamente` / `pgObservatii`, attachment double-click save and filling. Files remain in «Factura PDF» popup (the unsupported-file notice no longer points to the removed view); notes stay above «Continut» lines (00EF-29). `PrimiteAtasamentePage` / `PrimiteObservatiiPage` remain: `PrimiteObservatiiPage.CleanNotes` still serves the notes box.
2. **Adobe Save lets the operator choose a path.** New `AdobeSaveTrap.ChooseSavePath` (`Func(Of String, String)`), when set, parks Save As off screen and posts the chooser callback outside the sweep, passing the Adobe-proposed filename. Full path sets `_chosenPath` for the existing save flow; the operator chooser handles overwrite and the trap accepts Adobe confirmation as before. `Nothing` cancels the Adobe dialog without raising an event. Forwarded by `AdobeReaderHost` / `AcroPdfViewer`; `ReaderHostPreview.UserChoosesSave` uses `SaveFileDialog` («Salveaza documentul», PDF, overwrite prompt). Received-invoice hosts `PrimiteForm` / `EFacturaView` pass `FacturaPdfViewer(True)`; issued invoices and DDF retain save-to-document behaviour.
3. The ribbon fields overlapped (the KBot combo forces its own height): see [0114-01](SLICE-0114-ribbon.md#slice-0114-01), follow-up.

### Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `PrimiteView.Designer.vb`, `src/KBot.Controls/Adobe/AdobeSaveTrap.vb`, `AdobeReaderHost.vb`, `AcroPdfViewer.SaveTrap.vb`, `src/KBot.App/Views/Ddf/ReaderHostPreview.vb`, `src/KBot.App/Views/EFactura/Vanzare/FacturaPdfViewer.vb`, `EFacturaView.vb`, `src/KBot.App/KbotForm.EFactura.vb`.

### Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. No tests run. **The save trap was not exercised against a real Adobe**.

### Unverified / deferred
- Everything about the Save As flow is untested: that the chooser appears, that the file is written where chosen, that Cancel leaves Adobe usable, that the second Save works. Look at `adobe_preview.log` (lines «operatorul alege unde se salveaza», «Salvare anulata de operator»).
- A Save As of a DIFFERENT viewer that shares the Acrobat broker (the DDF) is still arbitrated by the existing rules.
- The temp PDF itself is not changed by the save any more when a path is chosen (Adobe now shows the chosen file).
- `Check-Help.ps1`: the tutorials that target `FacturiForm.navDetaliu` (issued invoices) are old errors, untouched.

## SLICE-00EF-32 - Received invoices: the end of a sync tells what was brought (15.10.2026) <a id="slice-00EF-32"></a>

### What changed and why
At the end of `PrimiteForm.SyncFromAnafAsync`:
- invoices were brought -> a window `SincronizareNoiForm` (own designer; caption bar, theme, one «OK») with a table of the NEW invoices (date, number, supplier, tax code, total, with a total in the footer), newest first; the same facts go to `mesaje_operator.log` through `OperatorLog` (the window is not a `KBotMessage`, so it logs by hand, as the house rule says);
- nothing brought and no unreadable message -> the info message «Nu există facturi noi pe serverul ANAF.» (`KBotMessage`, Information);
- brought but not visible in the list (another year than the one chosen in K-BOT, or the list could not be read first) -> an info message with the count.
«New» means absent from the pre-sync list, read first if needed via new `PrimiteView.Items` / `ListLoaded`; no server change. `KBotMessageBox` has no table, hence the separate small dialog.

### Files touched
`src/KBot.EFactura/Forms/SincronizareNoiForm.vb` and `.Designer.vb` (new), `PrimiteForm.vb`, `src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `contabil/efactura/index.md` (0000-78).

### Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. Nothing run, nothing seen; no real sync.

### Unverified / deferred
- The look of the window and the footer total; no real sync against ANAF.
- The comparison is by the list of the chosen year: invoices of another year are only counted in the message.
- If someone else's sync added invoices between the baseline and the end, they appear as new too.

### Follow-up (15.10.2026): «no messages» is INFO, never a warning
ANAF answers an empty period as an error text («Nu exista mesaje in ultimele N zile pentru cif=...»), which the server turned into `EfEroare` -> the client showed it as a warning and wrote it in the error log. Now:
- **Server** (`routes/efactura/anaf_api.py`, `lista_mesaje`): an `eroare` that starts with «nu exista ... mesaje» returns an empty list (no failure). `py_compile` ok; **not deployed**, not run against ANAF.
- **Client** (`PrimiteView.SyncFromAnafAsync`): an `ApiException` whose text says the same is shown as the info message «Nu există facturi noi pe serverul ANAF.» (Information, no entry in the error log) -- works before the server is redeployed.
- The footer menu's second caption is now only «Între 1 și 60 de zile».

## SLICE-00EF-33 - Received invoices: «Actualizare…» in the footer of the tree; no «Reimprospateaza» (15.10.2026) <a id="slice-00EF-33"></a>

### What changed and why
1. **«Reimprospateaza» of the window is gone** (`PrimiteForm`: button, handler, designer). Operator: the re-read of the database is not wanted.
2. **Footer of the tree** (`PrimiteView`, so in the «Cumparare» page AND in the DDF view «E-Factura»): caption «Actualizare…», the refresh picture on the right (`refresh.png`, new resource), tooltip. A click on the caption or on the picture opens a popup ABOVE the footer.
3. **The popup is the standard `CustomPopup`**, with two NEW kinds of row (slice 00EF-34, see its worklog): a combo and a text box. (A first version with a window of its own, `KBotComboTextPopup`, was replaced the same day at the operator's request.)
4. **New tree event `AdvancedTreeControl.FooterCaptionClicked`**: the rest of the footer band (outside the collapse button and the two icons) used to be swallowed; now it raises this, so the caption can be a button.
5. **The update moved into the control**: `PrimiteView.SyncFromAnafAsync(days)` (the loop, the progress in the footer caption and in the new event `SyncStatus`, the result window, one at a time) and `PrimiteForm.SyncFromAnafAsync` only forwards (the ribbon button of 00EF-30 still calls it). The text box: empty or outside 1-60 = no update and a warning. In the DDF view the list is read again with its DDF.
6. The ribbon button and its two fields (0114-01) are KEPT: nothing was said to remove them.

### Files touched
`src/KBot.Controls/Tree/AdvancedTreeControl.Events.vb`, `AdvancedTreeControl.Footer.vb`, `src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `PrimiteView.Designer.vb`, `src/KBot.EFactura/Forms/PrimiteForm.vb`, `PrimiteForm.Designer.vb`, `SincronizareNoiForm.vb` (a comment), `src/KBot.EFactura/My Project/Resources.resx`, `Resources.Designer.vb`, `Resources/refresh.png`; help `contabil/efactura/index.md` (0000-79).

### Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. Nothing run, nothing seen on screen; no real update from ANAF.

### Unverified / deferred
- The popup: its place above the footer, the focus in the text box, that picking a row of the list closes it and starts the update (the list's own drop-down window is no-activate, so it should not close the popup by deactivation; untested), Enter in the box.
- The caption alignment of the footer (right, as in the Recepții tree) next to the refresh picture.
- Two entry points exist now (ribbon and footer); say if the ribbon ones should go.

### Follow-up (17.10.2026): the footer text goes with its icon
Operator: when the footer icon (right or left) is hidden, its text must hide too; otherwise the text stays with no button.
- `AdvancedTreeControl.Footer.vb`: new `FooterCaptionShown()`. Caption empty = hidden; no visible footer icon (`ShowFooterLeftIcon` / `ShowFooterRightIcon` both False: the collapse button took their side, OR the host set the icon to Nothing, e.g. `ExtrasePanel.ShowDownloadIcon = False`) = hidden. (First version kept the text when no icon was set at all; the operator showed the Extrase screen where it stayed: fixed same day.) `DrawFooter` draws the caption only if `FooterCaptionShown()`, and `HandleFooterMouseDown` no longer raises `FooterCaptionClicked` for a hidden caption (no invisible button).
- `AdvancedTreeControl.md`: one line in the Footer section.
- Build: `dotnet build src/KBot.App` : 0 warnings, 0 errors. Not seen on screen.
- Consequence: a tree footer can no longer show a caption alone; no tree in the app does.

## SLICE-00EF-35 - Received invoices: the options menu of the tree header (15.10.2026) <a id="slice-00EF-35"></a>

### What changed and why
The button on the right of the tree header (it had one disabled line) opens the real menu:
1. **«Afisare facturi fara operatii in AVACONT»** (checked while in force): only the invoices with `NrOperatii = 0`. Server: `primite.lista` adds `NrOperatii` = the rows of `EF_PrimiteOperatii` of each invoice (the table is filled by the migrator; when the table does not exist yet every invoice has 0 and the list still works -- the check is on `information_schema`). Client: `EFacturaPrimita.NrOperatii`.
2. **«Afisare facturi noi»**, offered only when this session brought invoices (`PrimiteView._newIds`, filled by `ShowSyncResult`): without a sync of this session nothing says which invoices are new.
3. separator.
4. **Grouping**: «Grupare dupa furnizor» (tree = supplier roots, their invoices under them; no «Toate facturile» root) or «Grupare dupa luna» (what existed: Toate facturile > month > supplier > invoice). Only the one NOT in force is offered.
5. In supplier grouping only: a separator and **«Ordonare crescatoare facturi»** / **«Ordonare descrescatoare facturi»** (by invoice date, the one in force checked). Under a month the invoices stay newest first.
The options are of the session (not stored). The tree is built again with the shown invoice kept selected.

### Files touched
`PYTHON/routes/efactura/primite.py` (py_compile ok, **not deployed**), `src/KBot.Domain/EFacturaPrimite.vb`, `src/KBot.Api/ApiClient.EFactura.Primite.vb`, `src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `PrimiteView.Designer.vb` (tooltip), `contabil/efactura/index.md` (0000-80).

### Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. Nothing run, nothing seen; the list query was not run against a database.

### Unverified / deferred
- Until the server is redeployed the list has no `NrOperatii`: every invoice counts as 0, so the first filter shows everything.
- Whether EF_PrimiteOperatii has rows for the unit (the migrator fills it; the DDL run is not confirmed in the status).
- The options are not kept between sessions (nothing was asked); the search of the header and the options work together.

## SLICE-00EF-36 - Received invoices: the view «XML Factura» with the new control XmlViz (15.10.2026) <a id="slice-00EF-36"></a>

Slice number: the operator wrote «SLICE-01EF» for the work that imports from the editor; there is no 01EF slice anywhere, so it is read as the E-Factura slice (00EF), next free number 00EF-36 (to be corrected if 01EF was meant). The file name keeps «in-editor» from the first version.

### What changed and why
**First version (same day, REVERTED at the operator's word «NU e bine»)**: the FOREXE workflow editor was imported into `KBot.Forexe.Editor` and opened in a window of its own with a button of the bar. Taken out again: the ten imported files, `wfl_help.md`, the project / README edits and the launcher are gone; `src/KBot.Forexe.Editor` is as in HEAD (an empty project). **Now**: a new control, written from the ideas of that editor (the map, the colouring), made for the e-invoice:
1. **`XmlViz`** (`KBot.Controls/Xml/XmlViz.vb` + designer): left a MAP (`AdvancedTreeControl`), right the text (a read-only `RichTextBox`). No help panel, no validation, no editing, no autocomplete, no window of its own.
   - **Map**: one node per element, named WITHOUT prefix (`cbc:IssueDate` -> «IssueDate»; the prefix only says value / aggregate, which the expander and the bold name already say); a value element shows its text on the right (`~~~`, cut at 60 characters). A click selects the element in the text.
   - **Text**: written by the control from the parsed XML (two spaces per level, one element per line) as RTF, so the position of every element is known and the colours cost one assignment. Colours are the theme palette's (punctuation, element, attribute name, attribute value, text, comment) and follow a theme change. A value longer than 400 characters (the base64 of an attachment) is cut with the count of what was left out. A text that is not well-formed is shown as it came, with the reason.
   - **Romanian names (follow-up, same day)**: the map shows the names in Romanian (`XmlVizLabels`, ~180 UBL / CIUS-RO names, looked up by parent + name first: `ID` = «Număr factură» under the root, «IBAN» under `PayeeFinancialAccount`, «Nr. linie» under a line; `Party` = «Date furnizor» / «Date client» by its parent); an element not in the list keeps its name without prefix; the file's own name (with prefix) is in the tooltip of the node. The text on the right is untouched. Not used: the XSD (it has no Romanian). The list was written from the UBL 2.1 / CIUS-RO element names by hand: **to be read by someone who knows the business terms**, a wrong word is a one-line fix in `XmlVizLabels.vb`.
2. **The view**: item `xml` «XML Factura» (`Align = Far`) in `navDetaliu` of `PrimiteView`, visible while an invoice is shown; page `pgXml` (`XmlViz`) in `pnlPages`; the XML (`GetPrimitaXmlAsync`, UTF-8) is read when the view is shown, once per invoice (`RefreshXml`, with a stale-answer guard).

### Files touched
`src/KBot.Controls/Xml/XmlViz.vb`, `XmlViz.Designer.vb` (new), `src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `PrimiteView.Designer.vb`, `contabil/efactura/index.md` (0000-81). Nothing else remains of the first version.

### Test results
`dotnet build src/KBot.App` (builds Controls and EFactura): 0 warnings, 0 errors. Nothing run, nothing seen on screen; no test.

### Unverified / deferred
- Nothing seen: the colours in the three themes, the map on a real invoice (hundreds of nodes), `RightTextWidth` of the map for the values, the click-to-select (it relies on the RichTextBox counting a paragraph break as ONE character).
- The XSD was not needed: the rule «prefix = name without it» is enough. With the XSD the map could show the Romanian / standard labels of the business terms (BT-xx); not done, not asked.
- `XmlViz.md` / `CONTROLS.md` line: not written. No search box in the map, no way to resize the map / text split.
- The edit of an attachment's base64 is NOT shown (cut); «Salvează ca XML» of the tree menu still gives the exact file.

### Follow-up (15.10.2026): the map starts expanded at the root only
`XmlViz`: `pExpanded` is true only for the root element (depth 0); every other node starts collapsed.

## SLICE-00EF-40 - Received invoices tree: no counter on the root, no year on the months (17.10.2026) <a id="slice-00EF-40"></a>

### What changed and why
Operator, from a screenshot of the tree «Facturi primite, pe luni»: (1) the root says «Toate facturile» without the count («Toate facturile (10)» before); (2) the month rows say «Septembrie», not «Septembrie 2026».

`PrimiteView.BuildMonthTree`: the root caption no longer carries `({k_shown.Count})`; `MonthLabel` takes a flag and leaves the year out. My addition: the year comes back on every month when the list holds invoices of MORE than one year (the view of an angajament is not limited to one year), so two Septembers can never look the same. With one year (the usual case, and the one in the screenshot) the months are bare names. Not changed: the supplier-grouped tree, the totals on the right, the node keys (they still use year*100+month).

### Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `src/KBot.EFactura/KBot.EFactura.vbproj` (FileVersion 1.3.0.0 -> 1.4.0.0). Help: SLICE-0000-96.

### Test results
`dotnet build src/KBot.EFactura`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

### Unverified / deferred
The multi-year behaviour is my guard, not something the operator asked for; if the months must never show a year, delete the `k_severalYears` flag.
