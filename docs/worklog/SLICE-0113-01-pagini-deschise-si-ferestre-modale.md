# SLICE-0113-01 - Pages stay open; editors and answer-windows are modal windows again (16.10.2026)

Corrective of 0113 (operator, 16.10.2026): "toate machetele ca pagini, una cate una" was too much. Two kinds of macheta now.

## What changed and why

1. **Pages stay open.** `AvacontMain` keeps a list of open pages (`_pages`, most recently used last) instead of one `_page`.
   `ShowPage` no longer closes the page on screen: it puts it away (hidden, still open, what the operator typed stays) and
   shows the new one; a page that is open already comes back to the front. Pages: Extrase de cont, Clasificatii, Parteneri,
   Setari, Jurnal, Istoric versiuni, Ajutor, E-Factura Vanzare / Cumparare.
   - `ShowShell` / `ShowHome` no longer close the page on screen either (they hide it); both always return True now.
   - A page that closes while on screen is replaced by the page used before it (`Page_FormClosed`), else by the base with
     the caption it had. A page closed while put away changes nothing on screen.
   - `OnFormClosing` brings every open page to the front, most recently used first, and closes it, so each one's own
     question about unsaved changes is seen; one that refuses cancels the closing of the application.
   - `FindPage(Of T)` (used by `SetariForm.ShowFor`), per-page caption bars in `_pageBars`.
   - `Activate()` on an open Extrase / Clasificatii / Parteneri form (a no-op on a hidden hosted page) became
     `ShowMachetaPage(...)`.
2. **Modal windows are real windows.** DDF / ORD editors, `AsociereForm` (both openers), `CabNoteForm`
   (Operatiuni necorelate), `GraficRezervariForm`, `MessageCatalogForm` (Debug) and, by its own code (it awaited the
   close and read `Modificat`), `GrupeForm`: `ShowMachetaAsync` is replaced by `ShowModalAsync` (`AvacontMain.ShowModalAsync`,
   `KbotForm.ShowModalAsync`) = `ShowDialog(AvacontMain)`: their own caption bar and buttons (not hosted, so `PrepareHosted` /
   `HideCloseButtons` do not touch them), `StartPosition = CenterParent` on the main window (may cover the side panel),
   size cut down to the work area (`pnlHost`) minus a 16 px margin (`FitModal`: `MaximumSize`/`MinimumSize`/`Size`). They do
   not touch `_pages`: when one closes the pages are as they were. The history still records them (`_registry.Note`);
   `ShowPageAndWaitAsync` and the hosted-modal path are gone.
3. **Adobe is closed before anything covers it** (`IPutAway`, new in `KBot.Common/Machete`). `AvacontMain.PutAwayFront` is
   called by `PutFrontAway` (new page, page brought back, base switched) and by `ShowModal` (any modal window). It puts away
   what is on screen: the shell -> `KbotForm.PutAway` -> the active view; a page -> its `IPutAway`. Each implementation moves
   the **navigation bar's selection** to a tab without a PDF (the bar is never hidden) and lets the documents go
   (`ReleaseDocument` = `ReaderHostPreview.Clear`, which detaches the host and clears Adobe; the local-copy deletion stays off,
   `LocalCopyDeleteEnabled = False`):
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
   - The modal windows are **not in the history**: `MachetaType` got `InHistory` (default True; False for DDF / ORD editor,
     Asocieri, Operatiuni necorelate, Grafic, Grupe, Mesaje); `MachetaRegistry.Note` / `OpenAsync` skip those types;
     `ShowModal` no longer notes anything.
   - `MachetaEntry.IsOpen`; `MachetaHistory` keeps the OPEN entries first, in the order they were opened. An open entry never
     moves (asking for an open page again only refreshes its title); a new open entry goes after the last open one.
   - A page that closes goes to the END of the list as a closed entry (`MachetaHistory.MarkClosed`, from `Page_FormClosed`
     through `MachetaRegistry.NoteClosed`; `AvacontMain._pageRefs` remembers each page's reference). Opened again, it leaves the
     tail and takes its place after the last open one. A page re-asked under another reference (another year) closes the old line.
   - Capacity stays 10 lines: over the limit the CLOSED entries leave, the one closed longest ago first (`TrimClosed`); open ones
     are never dropped (more than 10 open are all kept). Entries loaded from a store start closed.
   - The list of angajamente (shell) is a macheta that is open while it is the base: `SwitchBase` notes it open / closed. The
     views of the shell (`view.*`) are NOT in the history (operator, 16.10.2026: only the pages and the list of angajamente stay):
     their types are registered with `InHistory = False` (`NoteViewOpened` and `OpenAsync` skip them); the reopen code of a
     view stays registered ("old paths stay").
   - `KBotSidePanelRow.Italic` (new, Line style) and `FillMachete`: closed lines in italics; tooltips «Deschisă…» / «Închisă…».

## Files touched
- NEW: `src/KBot.Common/Machete/IPutAway.vb`.
- Round 2: `KBot.Common/Machete/MachetaRef.vb` (`InHistory`, `IsOpen`), `MachetaRegistry.vb`, `MachetaHistory.vb`;
  `KBot.Controls/SidePanel/KBotSidePanelItems.vb` + `KBotSidePanel.vb` (`Italic`); `AvacontMain.Sidebar.vb`; `KbotForm.Machete.vb`
  (types flagged out of the history).
- `src/KBot.App/AvacontMain.vb` (pages list, `OpenNewPage`, `MakeFront`, `PutFrontAway`, `PutAwayFront`, `ShowModalAsync`,
  `ShowModal`, `FitModal`, `SwitchBase`, `Page_FormClosed`, `OnFormClosing`, caption), `KbotForm.Machete.vb`
  (`ShowModalAsync`, header), `KbotForm.Views.vb` (`PutAway`), `KbotForm.Ddf.vb`, `KbotForm.Ord.vb`, `KbotForm.Receptii.vb`,
  `KbotForm.Ingest.vb`, `KbotForm.CabNotes.vb`, `KbotForm.MessageCatalog.vb`, `Views/RezervariView.vb` (renamed call),
  `KbotForm.Grupe.vb` (modal), `KbotForm.Extrase.vb`, `KbotForm.Nomenclatoare.vb` (back to front), `Setari/SetariForm.vb`.
- `Views/DdfView.vb`, `Views/OrdView.vb`, `Views/NoteCabView.vb`, `Views/EFactura/EFacturaView.vb` (`IPutAway`).
- `src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `Forms/PrimiteForm.vb`, `FacturiForm.vb`, `FacturiForm.Pdf.vb` (`IPutAway`).
- Help: see `SLICE-0000-86-ajutor-pagini-ramase-deschise.md`.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 errors, 0 warnings (before the help edits; help edits do not touch code).
No tests written or run (project rule). **Nothing was run or seen on screen.**

## Unverified / deferred
- **Everything on screen**: switching between open pages, the caption, the ribbon buttons («Inchide», page buttons), the sizes
  and the centring of the modal windows, the cut-down size when the designer size is bigger than the work area (and the
  operator's text-size zoom, which can still enlarge a window past `MaximumSize`).
- **Adobe really closing** on `PutAway`: it rests on `ReaderHostPreview.Clear` detaching the host; whether the Adobe process
  of K-BOT ends, and how the shared Acrobat broker behaves when a modal editor opens right after, was not observed.
- A modal opened while another modal is open (the robot queue opens `AsociereForm` on its own): owner is the main window,
  not the first modal. Not tried.
- A running job in a put-away page (E-Factura sync, a download) keeps running while hidden; nothing tells the operator.
- **Year / unit change with pages open**: open pages are not refreshed or closed (e.g. Clasificatii holds the old year). Decision
  needed.
- Round 2 not seen on screen: the order of the stack, the italics, the trimming at 10. The shell line is open only while the shell is the base.
- Assumptions to confirm: `GrupeForm` is modal (its code awaited the close); Setari and Ajutor stay pages; closing a page
  brings back the page used before it (not the base).
- The side panel's «Machete deschise» is still the history (not "open now"); a history row of a modal reopens the angajament
  view, not the modal. The `DdfEditForm` / `OrdEditForm` / `AsociereForm` branch of `HideCloseButtons` is now unused (kept,
  "old paths stay").
- Tours / tutorials not re-walked (the `tur-ddf-editor` `btnRenunta` stale note of 0113 is now void: the button is back).
