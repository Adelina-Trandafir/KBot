# SLICE-KE11…15 — De la fereastra recepției la pagina și vederile încorporate

Istoricul păstrează fiecare rundă de corecții și limitele ei. KE11 unește facturile într-o fereastră; KE12 o transformă în pagină, iar KE12-04 o încorporează în vederi. KE12-02 consemnează și KE12-05/06/07: navigarea, clasificația automată a unicului indicator și păstrarea arborelui. KE13 înlocuiește eliminarea facturilor acoperite cu afișarea lor blocată; KE14 mută desfacerea asocierii din clic dreapta sub tabelul recepțiilor și adaugă vederea doar citire. KE15 separă formatul Windows al interfeței de formatul robotului și confirmă din fișa angajamentului unde se afla restul. Notele inițiale „combo pe grup nefăcut”, „fără cale de desfacere” și „cauză doar din cod” sunt depășite de actualizările explicite din corp. [Casetele de bifat](SLICE-KE12-03-dgv-casete-de-bifat.md) și [propunerile](SLICE-KE16-propuneri-legare-in-bloc.md) rămân separat.

- [KE11](#slice-KE11)
- [KE12](#slice-KE12)
- [KE12-02](#slice-KE12-02)
- [KE12-04](#slice-KE12-04)
- [KE13](#slice-KE13)
- [KE14](#slice-KE14)
- [KE15](#slice-KE15)

## SLICE-KE11 - The reception window rebuilt on the operator's mock-up (18.10.2026) <a id="slice-KE11"></a>

> Since KE12 (same day) this window is a PAGE of the ribbon, not a modal window: see `#slice-KE12` (`PrimiteView.LinkManyAsync` and the entries from the view / list are gone).

### What changed and why
Operator request (18.10.2026, a screenshot of the old window, a crop of its lower half and a hand-drawn mock-up «asa TREBUIE sa arate»), told to belong to the KE slice. `KeLegaForm` (the window of the three kinds of link: existing / new / modified reception) is now ONE window for ONE OR SEVERAL invoices, laid out as the mock-up:

- **Top left: tree of invoices** (`AdvancedTreeControl`, `CheckBoxes`): suppliers (by tax code, name order) with their invoices, a tick each; a supplier tick ticks all of its invoices. The invoices already covered by a reception (`|sum of its links| >= |total|`) are not listed, except the ones the window was opened for. Ticking reads the invoice's state (`GetKeAsync`) when it is not known.
- **Top right: «Produse Factura nr / data»** (the title follows the ticks: one invoice, «Produse facturi bifate (n)», none): the lines of the ticked invoices in ONE `KBotDataView` grouped by invoice (group header = «nr / data» and the sums, `ShowHeaderAggregates`). Columns: «Număr / Dată factură», «Denumire», «Valoare cu TVA», **«Recepționată»** (editable; proposed = the line's value, cut to what the invoice still has on that classification and, for an existing reception, to what that reception still has; typed amounts are kept and taken out first; must have the line's sign and not exceed its value), «Clasificație» (combo from the indicators of the chosen angajament). A classification chosen on a line is copied to the lines BELOW it, of the same invoice, that have none.
- **Bottom left: «Angajamente»**: grid of the angajamente the ticked invoices can go to (the server's candidates: DDF of the supplier, links already made) + those added here; the CURRENT row is the angajament of the reception; the existing rows are inert (cannot be changed); an empty row is ALWAYS last: a combo with every angajament the server lists (typing narrows it), free text = code, else the server's search (one hit adds it; none / many = a notice). Per-row button «…» -> «Elimină asociere» (see assumptions).
- **Bottom right: «Date recepție …»** (title: «nouă» for a new reception; else «Date recepție <descriere> (<data>)» of the chosen one), a row split 35% / 65%: on the left Tip / Data / Descriere (new reception) or the list of the angajament's receptions (existing / modified), on the right the grid of amounts by classification: «Clasificație», «Valoare factură», «Valoare recepție» (= sum of «Recepționată») and, for an existing reception only, «Rest recepție existentă». Read-only. Date (latest invoice date) and description («plata factura n/d» / «plata facturi ...») follow the ticks until the operator writes his own.
- Footer: «Renunță» and the action button; **«Ca la prima linie» and «Alt angajament…» are gone** (the first by request; the second replaced by the empty row). Window wider (1320x840, min 1100x720).

`PrimiteView`: `LinkAsync` (the three buttons of the invoice view) and the list's «Recepție nouă» both open the window through ONE routine, `LinkManyAsync` (file `PrimiteView.ReceptieMultipla.vb`, rewritten): window -> save the changed classifications -> server's check of every invoice -> (new / modified) ONE reception in FOREXE with the rows of all invoices added up per indicator and ONE marker -> every invoice gets its link. The KE10 sequence of one window per invoice (`FixeazaReceptiaNoua`) is gone. For a MODIFIED reception with several invoices the value written is its value now plus all the amounts. New class `KeLegaFactura` (the result per invoice). `NewReceptionAsync`, `ChangeReceptionAsync`, `WriteLinkAsync`, `RowsOf`, `InvoiceLabel` removed (nothing else used them). The server and the contract are unchanged.

### Corrections after the first run (18.10.2026, the operator's annotated screenshot)
- Tree: the ticks were missing (every node needs `HasCheckBox`, `CheckBoxes` alone shows none) and the header was white: it now has the look of the received-invoices tree (Control colour, gradient, Calibri); the first invoice asked for is selected and revealed. The «4.869,36» on the right was the TICKED invoice (the one the window was opened on); the highlighted tree row was another one: with the ticks shown the two can no longer be mixed up.
- Classification choices show ONLY the classification code (no name).
- A classification picked with the mouse is written at once and copied to the lines below (new `KBotDataColumn.CommitOnSelect` + `KBotComboBox.ItemPicked`), it no longer waits for the cursor to leave the row.
- The existing rows of «Angajamente» are no longer greyed: `KBotCellFormattingEventArgs.ReadOnlyCell` (new) makes a click open no editor and nothing else. Documented in `KBotDataView.md`.
- Build after the corrections: 0 warnings, 0 errors; still nothing seen on screen.

### Second correction round (18.10.2026)
- No tooltips on the four grids of the window (`tips` text removed from gridLinii, gridAng, gridReceptii, gridAloc). The grid's own overflow label (cut cell text) is a different thing and stays.
- **Combo in the header band of an invoice** (the mock-up's «daca e posibil», first called not possible): new, general feature of `KBotDataView`: `KBotDataColumn.GroupHeaderCombo`, event `GroupHeaderValuePicked` (`KBotGroupHeaderValueEventArgs`), file `KBotDataView.GroupEditor.vb`, floating `groupCombo` in the grid's Designer; documented in `KBotDataView.md`. The header cell shows the classification all the lines of the invoice share (empty when they differ); a press opens the list (not the fold of the group); picking writes it on ALL the lines of the invoice (`KeLegaForm.GridLinii_GroupHeaderValuePicked`). Help text updated (0000-101). Build: 0 warnings, 0 errors; NOT seen on screen: the placement of the combo over the header cell, the list opening, behaviour with the grid scrolled (a layout pass closes the combo when it has no focus).

### Third correction round (18.10.2026)
- «Leagă de recepție»: the receptions whose rest is 0 on every classification are not listed (they can take no invoice). Only this mode: «Modifică recepția» still lists them (a reception covered in full can be raised).
- «Descriere» is a multi-line box (`KBotTextBox`, scroll bars when needed) filling the cell under Tip / Data; a line break, typed or pasted, becomes a space before the text goes to FOREXE (one line there). Help text updated (0000-101). Build: 0 warnings, 0 errors; not seen on screen.

### Decisions taken without the operator (to confirm)
1. **«Elimină asociere»**: an angajament added in the window just leaves the list; one that comes from the server asks a question and removes the operator's MANUAL links of the ticked invoices with a DDF of that angajament (`GetPrimitaAsync` -> `LegaturiManuale` -> `UnlinkPrimitaAsync`). An association that comes from the supplier being a partner of the DDF, or from links with receptions, is not removed (a message says where to do it). Adding an angajament on the empty row does NOT write an association (there is no server route for «invoice <-> angajament» besides the DDF link): it only chooses the angajament for this reception.
2. The reception list of the existing / modified modes sits in the 35% cell where the new-reception fields are (the mock-up has only the fields).
3. «Valoare factură» = the value of the lines on that classification (not the rest after earlier links); the rest shows in the proposed amounts.
4. The fill-down of a classification stays inside the same invoice.
5. Columns «Indicator» and «Rest factură» of the old grid are gone (the mock-up has three columns); a classification that is not an indicator of the angajament is marked in the grid and the button refuses it, as before.

### Files touched
`KBot.EFactura/Forms/KeLegaForm.Designer.vb` (rewritten), `KeLegaForm.vb` (rewritten), `KeLegaFactura.vb` (new), `Views/Primite/PrimiteView.ReceptieMultipla.vb` (rewritten), `PrimiteView.Receptii.vb`; help `HelpContent/contabil/efactura/index.md` (0000-101).

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen, no test written or run (operator rule). `Check-Help.ps1`: the only complaint about this topic was the missing worklog for 0000-101 (written); the tutorial warnings are old.

### Unverified / deferred
- NOTHING seen on screen: the proportions (27/73, 24/76, 35/65, 52/48), the grouped grid with editable cells, the always-last empty row with its inert neighbours (they show the grey «inert» look), the hidden button of the empty row, the receptions list in 35% of the width, the tree check propagation, the DPI behaviour. The designer was never opened in Visual Studio.
- ~~Combo on the group row~~ done in the second correction round (below).
- Existing reception with several invoices: each invoice is checked by the server alone; the sum over the invoices is checked in the window (`ReceptionFitProblem`), the server's `leaga` checks again per invoice.
- The window reads at most the newest 300 angajamente for the empty row (the server's cap); anything else is found by the server's search on the typed text.
- All the old KE caveats stay: SQL not run, server not deployed, the two workflows not run on the real site.

## SLICE-KE12 - The reception window becomes a page of the ribbon (18.10.2026) <a id="slice-KE12"></a>

### What changed and why
Operator request: the window built in KE11 (opened by «Leagă» / «Recepție nouă» of the «Recepții» view) becomes a stand-alone PAGE, not a modal window; it goes in the ribbon, tab E-Factura, after «Cumpărare», with a separator after it; the footer buttons of the «Recepții» view go away, in E-Factura and in K-BOT alike (the same `PrimiteView` hosts both), for now.

- **`KeLegaForm` is a page.** Base class `KBotShellForm` (like `PrimiteForm`), borderless, hosted by `AvacontMain.ShowPage`. New constructor `New(api, gate, unitName, year)`; no modal parts left (`DialogResult`, «Renunță», the results `Facturi` / `Cod` / `Receptie` ...). It reads the year's invoices itself (`GetPrimiteAsync`), `ReloadAsync` forgets states and reads again.
- **Mode chosen in the page**: a combo «Recepția:» above the data of the reception (new / link to an existing / modify an existing); `CmbMod_SelectedIndexChanged` re-applies the mode.
- **The work moved in**: what `PrimiteView.LinkManyAsync` did (save classifications, the server's check of every invoice, the robot for new / modified, ONE marker, the links, the failure messages) is now `KeLegaForm.CommitAsync` (+ `ReserveMarkerAsync`, `MarkerRegex`, `SumRows`, `KeMarker`). While it runs the page is disabled (`SetBusy`); afterwards the page reads the invoices again (the covered ones leave the tree) and says how it went in the notice. `KeLegaFactura` got `Verificare`.
- **Ribbon**: button «Recepții» (key `efactura_receptii`, icon `Fatcow_Farm_Fresh_Check_boxes_32`) after «Cumpărare»; new property `KBotRibbonButton.SeparatorAfter` (a thin line after a Large button of a group when another button follows; `KBotRibbon.PlaceGroup` / `PaintGroup`), set on the new button, so the separator sits between «Recepții» and «Vânzare». Machete type `efactura_receptii` registered (`KbotForm.Machete.vb`), opener `KbotForm.DeschideEFacturaReceptii`, `AvacontMain.Ribbon_ItemClicked`.
- **«Recepții» view**: `PrimiteReceptiiPage` has no footer: the four buttons (Leagă, Nouă, Modifică, **Scoate legătură**) and their events are gone, so removing a link is not possible from anywhere for now. `PrimiteView.Receptii.vb`: `PgReceptii_LinkRequested`, `PgReceptii_RemoveRequested`, `LinkAsync`, the marker code removed; `PrimiteView.ReceptieMultipla.vb` deleted (its work is in the page).
- **The list's «Recepție nouă» button** (KE10, footer of the list «Toate facturile») is removed too: it opened the modal window that no longer exists (decision of mine, to confirm; the page replaces it).

### Decisions taken without the operator (to confirm)
1. «Scoate legătura» went with the other footer buttons (the operator said «butoanele din footer»): no way to remove a link until it is given a place.
2. The separator is a new ribbon feature inside a group (the ribbon only had group-edge lines and pinned-button lines); it comes after «Recepții», so «Vânzare» follows it.
3. The mode combo's three texts: «Recepție nouă», «Leagă de o recepție existentă», «Modifică o recepție existentă».
4. The year of the invoices is the year of K-BOT (as «Cumpărare»); invoices of other years are not listed.

### Files touched
`KBot.EFactura/Forms/KeLegaForm.vb`, `KeLegaForm.Designer.vb`, `KeLegaFactura.vb`; `Views/Primite/PrimiteReceptiiPage.vb` + `.Designer.vb`, `PrimiteView.Receptii.vb`, `PrimiteListaPage.vb` + `.Designer.vb`, `PrimiteView.ReceptieMultipla.vb` (deleted); `KBot.Controls/Ribbon/KBotRibbonItems.vb`, `KBotRibbon.vb`; `KBot.App/AvacontMain.Designer.vb`, `AvacontMain.vb`, `KbotForm.EFactura.vb`, `KbotForm.Machete.vb`; help `HelpContent/contabil/efactura/index.md` (0000-102).

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen, no test written or run.

### Unverified / deferred
- Nothing seen on screen: the page as a hosted page (its caption bar is hidden by the host), the mode row, the ribbon button and its separator, the view without footer.
- The page on a unit without received invoices, the role «Contabil» check (the button stays off), the robot missing (a box at the click) — written, never run.
- The page closes the combo of a group header when a layout pass runs and the combo has no focus (KE11).
- All the old KE caveats stay: SQL not run, server not deployed, the two workflows not run on the real site.

## SLICE-KE12-02 - Tree of the page «Recepții»: search, closed start, no supplier tick, newest first (18.10.2026) <a id="slice-KE12-02"></a>

### What changed and why
Operator corrections on the tree «Facturi de recepționat» of the page «Recepții» (KE12), from a screenshot:
- **Search button in the header** (the lens, as in the received-invoices tree: same `HeaderSearchIcon` and search-band settings); it finds a supplier by name or an invoice by number (the control's search over the captions; at least 3 characters).
- **The tree starts closed**: the suppliers are not expanded.
- **The supplier has NO tick** (it is only a heading); the ticks are on the invoices.
- **Invoices in descending order of date** under each supplier (newest first).
- **Invoices already covered are not listed**: an invoice whose links, over one or several receptions, add up to its whole total is left out (as before); now also an invoice whose total is 0 (nothing to receive; the screenshot showed a supplier at 0,00).
Tooltip of the tree and help (`contabil/efactura/index.md`, 0000-103) follow.

### Second part: the grid of products (same day)
- Each ticked invoice starts COLLAPSED (`CollapsedByDefault` on the group level of `gridLinii`).
- The invoice's header band has no coloured background (white; the scheme's blue is not used) and keeps only bold text and a frame a little more visible than the cell lines: new `KBotGroupLevel.HeaderBorder` (documented in `KBotDataView.md`), painted by `DrawGroupBand`.

### Third part: the reservation check and the columns of the amounts (same day; corrected twice)
- The grid of amounts has the columns of the section A of the fundamentation document: **Buget** | **Recepții** | **Disponibil** (= Buget - Recepții) | **Valoare** (= the sum of «Recepționată») | **Rămas** (= Disponibil - Valoare), then, only for an existing reception, «Rest recepție existentă». «Valoare factură» is gone.
- **The principle of the ordonanțare (routes/forexe/ddf_edit.py, budget_on_day.py)**, taken as it is: Buget = the budget of the classification ON THE DAY (`budget_on_day`: the version in force + its rectifications; no version that day = today's credit, `FX_Indicatori_Buget.CreditBugetar`, and the server says so); Recepții = `FX_Receptii` of the indicator with `DATE(Data)` STRICTLY BEFORE the day. The day is the date of the new reception, or the date of the chosen one (existing / modified).
- **New server route** `GET /api/efactura/ke/angajament/<cod>/disponibil?data=aaaa-ll-zz` (`primite_ke.disponibil`, `primite_ke_routes.ke_disponibil`; the forexe module is imported inside the function), client `IEFacturaApi.GetKeDisponibilAsync`, domain `EFacturaKeDisponibil` / `EFacturaKeDisponibilZi`. Server NOT deployed (like all KE), py_compile clean, nothing called. The page asks once per (angajament, day) and keeps the answer; a date change, a reception chosen or a mode change asks again (`KeLegaForm.RefreshDispoAsync`).
- **The check** (`AvailabilityProblem`, from `RefreshOk`, so it runs on every classification picked on a line or on an invoice header, on a typed amount, a new day, angajament or mode): on every indicator the sum of «Recepționată» of all the ticked invoices must be at most Disponibil. Otherwise an ERROR notice and the button stays off. Not for «Leagă de o recepție existentă»; a storno (negative sum) is not checked; while the answer has not come, or the budget is unknown, nothing is checked. This replaces my first version of the check, which used `AngajamentLegal - Receptii` of the indicator (wrong source for the columns the operator described).
- The reading of «strictly before»: a reception of the SAME day does not count in «Recepții» (as in the ordonanțare). Two receptions on one day on one indicator are therefore not added up by this check; to confirm.

### Fourth part: the list of the empty angajament row (same day)
- The list offered by the empty row of «Angajamente» no longer contains the angajamente already in the grid (the ones of the same invoices / supplier): `FillAngajamenteChoices` leaves them out and is called every time the rows are rebuilt. A code typed by hand that is already there just selects that row (nothing is added twice). Not seen on screen.

### Fifth part: the classifications of ALL the angajamente (same day)
- The list of classifications in the grid of products holds the indicators of EVERY angajament of the «Angajamente» grid, not only the one in use: the page reads the others in the background (`BeginLoadOtherAngajamente`, `KnownData`, kept in the same cache) and rebuilds the list (`ApplyIndicators`). The same classification in two angajamente is one entry.
- A classification picked (on a line or on an invoice header) that is NOT an indicator of the angajament in use moves the use to the first angajament that has it (`EnsureAngajamentFor`; the row of the grid follows, the receptions and the amounts are read for it). The other lines are judged against the new angajament at the button (the existing message: the classification is not an indicator of the angajament). One reception is on one angajament, so lines whose classifications belong to different angajamente cannot go together.
- Not seen on screen; the first list shows the current angajament's classifications until the others have been read.

### Sixth part: the supplier shows its angajamente; the navbar of the reception (same day, KE12-05)
- The angajamente belong to the supplier, not to the invoice: pressing a supplier node (or an invoice row) in the tree, while NO invoice is ticked, reads the state of the supplier's newest invoice (`Tree_NodeMouseDown`, the supplier node's `Tag` is that invoice id, `_browsed`) and fills «Angajamente» from it (`FillAngajamente` takes the ticked invoices, else the browsed one). Once something is ticked the ticks decide; a reload forgets it. One angajament = it becomes the current one and its receptions are read.
- The combo of the mode is replaced by a horizontal `KBotNavList` (`navMod`): «Recepție nouă» | «Recepții existente». Under «Recepții existente» a small combo «Acțiune» (`cmbMod`) keeps the two things the page did with an existing reception: «Leagă facturile de recepție» (only links; receptions with rest 0 still hidden) and «Modifică recepția (valorile cresc)». The mode is computed by `ChangeMode` from both. Operator said only «Recepție nouă» / «Recepții existente»: the link-or-modify choice is mine to keep (nothing may be written in FOREXE by guessing); to confirm.
- Fix: an angajament picked on the empty row of «Angajamente» kept the receptions and indicators of the previous one: `BeginAddAngajament` set `_angCod` first, so `UseAngajamentAsync` saw «same code, data present» and returned. The guard now also requires that `_data` is the cached data of that very angajament.
- Single indicator (KE12-06): when the angajament pressed in «Angajamente» has exactly ONE indicator, every line of the products grid takes its classification (`ApplySingleIndicator(True)` from `UseAngajamentAsync`; pressing another angajament changes them again). Invoices ticked later fill only their lines without a classification (`ApplySingleIndicator(False)` from `ApplyChecked`). An angajament with several indicators leaves the lines as they are. Build 0/0, not seen on screen.
- After a reception (KE12-07) the tree is no longer rebuilt: `ReloadAsync(True)` reads the list and `PruneTree` removes only the invoices now covered (and a supplier left empty), unticks the rest and refreshes their numbers and the supplier totals; expansion, selection and scroll stay. New `AdvancedTreeControl.RemoveItem`. The caches of the budget (`_dispoCache`) are now also cleared on a reload (they would have kept the receptions of before). Not seen on screen.
- Not seen on screen.

### Files touched
`KBot.EFactura/Forms/KeLegaForm.vb`, `KeLegaForm.Designer.vb`; server `PYTHON/routes/efactura/primite_ke.py`, `primite_ke_routes.py`; `KBot.Api/IEFacturaApi.vb`, `ApiClient.EFactura.Ke.vb`; `KBot.Domain/EFacturaKe.vb`; help `HelpContent/contabil/efactura/index.md`.

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

### Unverified / deferred
- Whether the control's search finds the invoices of a supplier when the name is typed (it does in the received-invoices tree, which uses the same control and defaults); not seen on this tree.
- «Covered» uses the `Receptii` the list carries (sum of `Suma` per invoice, in absolute value, tolerance 0.01); an invoice with receptions still waiting for the download counts with their sums.

## SLICE-KE12-04 - The reception form inside the views of the received invoices (18.10.2026) <a id="slice-KE12-04"></a>

Operator (18.10.2026): in the views (not in the form) -- instead of the grid that shows the associated receptions, show a variant of the form «Recepții»: without the choice of the invoice when it is opened from «Facturi Cumpărare» (the invoice is the chosen one), and without the choice of the angajament too when it is opened from the angajamente of K-BOT's main window (the invoice and the angajament are given).

### What changed and why
- **KeLegaForm** gets an embedded variant: `Embed()` (no frame, no header, docked by the host, never closed), `ShowInvoice(invoice, state, angajament)` (works on THAT invoice, already ticked; the tree is not shown, nothing is read for a list; everything kept about a former invoice is dropped; `angajament` not empty = the view of an angajament: the table of angajamente is not shown and that angajament is the one in use), `ClearInvoice()`, event `Committed`. The invoice stays in the tree even when it is covered (it is the one on screen). After a reception the form does not read anything itself: it raises `Committed` and waits to be given the invoice again. The window of the ribbon works as before (the new members do nothing there).
- **PrimiteReceptiiPage**: the table of links (`gridLegaturi`) is removed; under the summary line there is a panel (`pnlForma`) that hosts the form (`Embed`). The summary (total, allocated, rest, state) and the notes stay.
- **PrimiteView**: `AngajamentCode` (set by `EFacturaView.SetContext` for the view of an angajament, cleared with `ClearAll`); `ShowKeForm` (makes the form on the first invoice, feeds it in `ApplyKe`), `KeForm_Committed` (the list and the invoice are read again, which gives the invoice back to the form), `ClearInvoice` when the invoice goes away or its links cannot be read.

### Second part (same day): corrections after the first look
- Embedded variant: the titles «Date recepție …» and «Angajamente» are hidden, so the title band over the lower zones is gone; the rows of «Angajamente» show the description only (no code).
- The navbar of the reception: «Recepție nouă» on the left (icon `add_fact`), «Recepții existente» on the right (icon `table`) -- also in the page of the ribbon.
- The empty note label of `PrimiteReceptiiPage` (Bottom, 36 px) left a blank band under the form: it is shown only when it has text.
- `KBotDateEditBox`: the field of the date showed only a few characters (text clipped at about «04.0»). Cause (read from the code, not seen): the formatting rectangle is set on WM_SIZE, which arrives BEFORE WinForms has updated `ClientRectangle`, so after a resize (the embedded form is resized after its first layout) the rectangle kept the old narrow width. It is now also set on `OnClientSizeChanged`. Not seen on screen.

### Files touched
`KBot.EFactura/Forms/KeLegaForm.vb`, `Views/Primite/PrimiteReceptiiPage.vb`, `PrimiteReceptiiPage.Designer.vb`, `PrimiteView.vb`, `PrimiteView.Receptii.vb`; `KBot.App/Views/EFactura/EFacturaView.vb`; help `contabil/efactura/index.md` (0000-105).

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen, no test written.

### Unverified / deferred
- Not seen: whether the form fits the area of the views (about 757 x 450 px: the products and the data/amounts bands share it), the hidden columns (tree, angajamente) in the table layouts, the tick boxes in that size.
- A child form inside a view (TopLevel False, as AvacontMain does for pages) was assumed to take the theme and to be disposed with the view.
- The list of the links already made (angajament, classification, reception, sum) is no longer shown anywhere in the view; «Recepții existente» lists the receptions of the angajament and the summary line gives the allocated / rest. To say if the operator wants the links back.
- In the view of an angajament the invoice may be linked to receptions of OTHER angajamente (its old links): they are not shown any more.

## SLICE-KE13 - The tree of the page «Recepții»: every invoice, covered ones locked, un-associate, options icon (18.10.2026) <a id="slice-KE13"></a>

Operator (18.10.2026), on the page «Recepții» and the view «Recepții» of an invoice:
1. text seems to exist under the tree;
2. the tree gets, as in K-BOT's main tree, a new button at the right of its header -- the options one -- with a popup holding «Arată facturile asociate», which shows ONLY the invoices that are associated with a reception;
3. the page shows ALL the invoices by itself, not only the ones that are not associated (same in the «Recepții» view of the E-Factura view);
4. the ones already associated can no longer be ticked (they are ticked by themselves); the whole association system has no point for them; they can be un-associated, nothing more. Suggestions asked on how they should look.

### What changed and why
- **Text under the tree (1)** -- read from the code, not seen: `AdvancedTreeControl.OnPaint` clips the rows to the area between header and footer, but the tree of this page had no footer, so the last row was cut in half by the bottom border and looked like text under the tree. The tree now has a footer (as the main tree has one): `TOTAL <sum of the listed invoices>`, which also takes the place of the cut row. The same sum is refreshed with every read.
- **Options icon (2)**: `tree.HeaderRightIcon` (new resource `settings`, the same picture as the main tree's) in `KeLegaForm`. A press opens a popup with ONE row, «Arată facturile asociate» (ticked while on). On: the tree lists only the invoices that have at least one link with a reception (a partly covered one counts); the header says «FACTURI ASOCIATE CU RECEPȚII» (else «FACTURI PRIMITE»). A second press turns it off. The tree is built again from the invoices already read (the suppliers that were open stay open); nothing is ticked afterwards.
- **All the invoices (3)**: `BuildTree` no longer leaves out the covered invoices (only the ones with value 0 stay out). After a reception `RefreshTree` (was `PruneTree`) keeps every row, refreshes the numbers and locks the ones that became covered. The embedded variant (the view of an invoice) already listed the invoice on screen whether covered or not (`ShowInvoice`, KE12-04) and has no tree on screen, so nothing changes there; it has no lock either (the host ticks its one invoice).
- **Covered invoices (4)**: an invoice whose receptions cover its whole total is ticked and LOCKED -- new `TreeItem.CheckLocked` in `KBot.Controls` (the box is drawn grey, a press on it does nothing, the row still selects); the row is written in the theme's dim text colour; the tooltip (`TreeItem.Tooltip`) says «Recepționată integral» / «Recepționată parțial: x din y» and lists date · amount of each reception, then «Clic dreapta: «Desface asocierea»». Locked rows are not in `CheckedIds`, so they never reach the lines grid. **Un-associate**: right click on any invoice that has links -> popup «Desface asocierea cu recepția» -> confirmation -> every link of the invoice is removed in K-BOT with the existing `RemoveKeLegaturaAsync` (the receptions in FOREXE are not touched; only the role «Contabil», checked on the server too) -> the tree is read again. A partly covered invoice stays tickable (it still has a rest) and can also be un-associated.

### Suggestions for the look of the associated invoices (the operator asked)
Built now: dim text + grey locked tick + tooltip + right-click «Desface asocierea». Other ways, not built: a small «recepționată» icon at the right of the row; the reception date written after the number; a thin «Recepționate» sub-heading under each supplier; a real «Desface» button on the row (the tree has no row buttons today); asking nothing and un-associating from the options popup for the ticked-and-locked rows.

### Files touched
`src/KBot.EFactura/Forms/KeLegaForm.vb`, `KeLegaForm.Designer.vb`; `src/KBot.EFactura/My Project/Resources.resx`, `Resources.Designer.vb`, `Resources/settings.png` (copy of the main app's picture); `src/KBot.Controls/Tree/AdvancedTreeControl.TreeItem.vb`, `.Overrides.vb`, `.Painting.vb`; help `contabil/efactura/index.md` (0000-108).

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen, no test written (house rule).

### Unverified / deferred
- Not seen: the half-row explanation (1) is from the paint code; the grey tick and dim text; the footer text fitting the width; the popup under the icon; the right click (assumes `NodeMouseUp` carries `e.Button = Right`, as the main window's tree does).
- `ThemeManager.Current.Palette.TextDimColor` is read when a row is built: a theme change in mid-session shows the old colour until the next read.
- The un-association was not run against the live server (`DELETE .../ke/legaturi/{id}` exists since KE01).
- In the embedded view of an invoice a covered invoice can still be linked again (no lock there, as before).

## SLICE-KE14 - A resolved invoice in the page «Recepții»; linked angajamente locked (19.10.2026) <a id="slice-KE14"></a>

Operator (19.10.2026), page «Recepții»:
1. an angajament that has receptions associated can no longer be taken off the list of angajamente (its three-dots button hidden on those rows);
2. a single press on an already resolved invoice: on the right no «Recepție nouă» part, only the associated receptions; the products part lists the invoice's products (as when an unassociated invoice is ticked) but nothing can be changed;
3. also on that press (resolved ones only): «Recepții existente» holds ONLY the reception the invoice is associated with; under the table of receptions, for such receptions, a button «Elimină asociere» (first read as the page footer, moved under the table on 19.10.2026 at the operator's word); the right-click of the tree for associated invoices goes away.

### What changed and why
- **Linked angajamente (1)**: `AngItem.Linked` = a link (`EFacturaKeStare.Legaturi`) of one of the invoices that feed the grid has that angajament. The button is hidden for it (`GridAng_CellFormatting`) and refused (`GridAng_ButtonClick`). It is read from the links of the ticked invoices (or of the pressed supplier's invoice when nothing is ticked), not from `Sursa`, which a DDF with the same code can hide.
- **View of a resolved invoice (2, 3)**: a press on a locked (covered) invoice calls `BeginView` -> `EnterViewing`. The page then shows, for that invoice only and read-only (`ReadOnlyGrid` on `gridLinii` and `gridAng`: no cell opens, the tick boxes are inert): the products (`FillViewedLines`, the amount column holds the line value); the angajament(s) of its links (`FillViewedAngajamente`, no empty row, no button); the receptions it is linked to, one row each (`FillViewedReceptii`; the «Total» column becomes «Valoare factură» = what the invoice takes of that reception, «Rest» is hidden); the amounts by classification (`FillViewedAlocari`; first laid out with the classification and the value only, then -- fix of 19.10.2026, the first version left the budget columns empty -- Buget / Recepții / Disponibil / Rămas are filled as in the form of a reception, from `GetKeDisponibilAsync` asked once per angajament and reception day: `BeginViewedDispo`). The tabs, the action combo and the form of a new reception are hidden. The button `btnElimina`, docked at the bottom of the receptions panel (the footer bar is hidden meanwhile), reads «Elimină asociere» (enabled for the role «Contabil») and runs `BeginUnlink` (confirmation, `RemoveKeLegaturaAsync` for each link, the tree read again). The titles read «Produse Factura …» / «Recepția asociată». `LeaveViewing` brings the page back (tabs, form, lines of the ticks): on a press on any other row, on a tick, and after the tree is read again. Nothing of the new-reception pipeline (defaults, budget, `RefreshOk` notices) runs while viewing.
- **Right-click menu removed** from the tree (`Tree_NodeMouseUp`); the tooltips say a press shows the reception and the products.
- With ticked invoices already on the page a press on a resolved one does nothing but say to untick them first (their typed lines would be lost) -- my choice, say if you prefer otherwise.

### Files touched
`src/KBot.EFactura/Forms/KeLegaForm.vb`, `KeLegaForm.Designer.vb` (tooltip text); help `contabil/efactura/index.md` (0000-109).

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen, no test written (house rule).

### Unverified / deferred
- Not seen on screen: the hidden button on linked rows, the read-only grids (the group header of `gridLinii` and the check boxes assumed inert under `ReadOnlyGrid`, read from `KBotDataView.CheckBoxes.vb` / `Editing.vb`), the footer button, the swap of the column header.
- A link whose reception is not downloaded yet shows the date «în așteptare».
- The classification column of the products shows the label the server sends with the line.

## SLICE-KE15 - Page «Recepții»: amounts typed in the operator's regional format; the rest message says where the rest is (19.10.2026) <a id="slice-KE15"></a>

Operator (19.10.2026), page «Recepții», two reports:
1. «Leagă» stays disabled with «Recepția aleasă nu are destul rest pe 65.02.04.01.20.01.03: rest 0,00, de alocat 81,54», while the table of receptions shows «Rest nealocat» 81,54 for the chosen reception.
2. The user's Windows has «.» as decimal symbol. Typing `77.49` in «Recepționată» gives «Suma recepționată a liniei nu poate depăși valoarea ei (77,49) …»; ticking / unticking the row leaves «Recepționată» at 0.

### What changed and why
- **Typed amounts (2)**: `ReadNumber` parsed the typed text with `DdfSendInputs.CulturaSite` (ro-RO: «.» is the thousands separator), so `77.49` was read as 7749 and refused as above the line value. The grid paints and edits in `CurrentCulture` (`KBotColumnFormat`, `KBotDataView.Painting`), so the text is now read with `CurrentCulture` too. A comma user still types «77,49», a dot user «77.49».
- **Numbers in the notices**: new `Fmt(Decimal)` (`N2`, `CurrentCulture`) used by the three notices of the window (line above its value, reception without enough rest, availability), so a notice shows the sum as the cells do (81.54, not 81,54, for a dot user). Dates and the other `_ro` uses stay.
- **Rest message (1)**: `ReceptionFitProblem` now adds, when the chosen reception has rest on OTHER classifications, which ones and how much («Recepția are rest pe: X (81.54). Alegeți clasificația liniilor în acord cu ea sau o altă recepție.»). Reason (from the code read, not from the data): the list's «Rest nealocat» is the SUM over all the classifications of the reception (`FillReceptii`), the message and «Rest recepție existentă» are PER classification of the lines. 81.54 in the list with 0.00 on `65.02.04.01.20.01.03` can only mean the rest is on another classification of the reception; the old message did not say so.
- The tick/untick returning 0 is the same cause: with an existing reception, `RecomputeDefaults` cuts the default to what the reception has on that classification (0), as designed since KE11. It is NOT changed.

### Update (19.10.2026, after the operator sent `FX_FisaAngajament` of AAB2KPFMMMN)
Verified from the data: reception 129 (13.02.2026) has 54,493.93 on `...20.01.03` fully allocated (invoice 3584) and 9,359.07 on `...20.01.30` with 9,277.53 allocated (invoice 70) -> rest 81.54 on `.30`. The penalty invoice 941 (81.54) belongs to `.30` (its Access operation and order are on `.30`) but its lines had no classification and the operator had them on `.03`. So the old message was right, the screen did not show it. Nothing was wrong in `primite_ke.py`.
- `FillAlocari` now adds, with an existing reception chosen, one row for every classification where the reception has rest but no line stands («... (recepția are rest aici; nicio linie nu e pe ea)», only the «Rest recepție existentă» cell filled), under the rows of the lines. The operator sees the 81.54 next to the line's 0.00.
- **The real hole (operator's screenshot with `.03`, «Recepționată» 0,00, «Leagă» grey, no message)**: with an existing reception whose rest on the lines' classification is 0, `RecomputeDefaults` cuts every sum to 0; `RefreshOk` ran `ReceptionFitProblem` only when some sum was above 0 (`k_ok` needs `k_any`), so NOTHING was said (the message of the first screenshot only showed while old sums were still there; the bottom bar kept the older «Sunt facturi bifate…» text). Now, when no sum is left (`Not k_any`), an existing reception is chosen and an invoice is ticked, `ReceptionFitProblem(True)` checks with the value of the lines (less what the invoice already has linked) and shows the same warning, naming where the reception has rest.
- Not done (offered): default the classification of the lines from the reception's single classification with rest, or from `EF_PrimiteOperatii` (Access).

### Files touched
`src/KBot.EFactura/Forms/KeLegaForm.vb` (`ReadNumber`, `Fmt`, `GridLinii_CellValidating`, `ReceptionFitProblem`, `AvailabilityProblem`); help `contabil/efactura/index.md` (0000-111).

### Test results
`dotnet build src\KBot.EFactura\KBot.EFactura.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen, no test written (house rule).

### Unverified / deferred
- **Initial limit, superseded by the data update of 19.10 above:** Report 1 was read from the code only: the data of that angajament (the reception's classifications) was not looked at. If the reception really has rest 81.54 on `65.02.04.01.20.01.03`, the fault is in `_SQL_VALORI_RECEPTII` / `_SQL_ALOCAT_RECEPTII` (`primite_ke.py`) instead; ask for `FX_FisaAngajament(db, cod)` (KE01-02) of that angajament.
- A dot user who types a comma («77,49») gets 7749 (comma = thousands in his format): the window follows his Windows setting.
- Not seen on screen.
