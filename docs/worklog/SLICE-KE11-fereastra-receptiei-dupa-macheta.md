# SLICE-KE11 - The reception window rebuilt on the operator's mock-up (18.10.2026)

> Since KE12 (same day) this window is a PAGE of the ribbon, not a modal window: see `SLICE-KE12-receptii-pagina-in-ribbon.md` (`PrimiteView.LinkManyAsync` and the entries from the view / list are gone).

## What changed and why
Operator request (18.10.2026, a screenshot of the old window, a crop of its lower half and a hand-drawn mock-up «asa TREBUIE sa arate»), told to belong to the KE slice.
`KeLegaForm` (the window of the three kinds of link: existing / new / modified reception) is now ONE window for ONE OR SEVERAL invoices, laid out as the mock-up:

- **Top left: tree of invoices** (`AdvancedTreeControl`, `CheckBoxes`): suppliers (by tax code, name order) with their invoices, a tick each; a supplier tick ticks all of its invoices.
  The invoices already covered by a reception (`|sum of its links| >= |total|`) are not listed, except the ones the window was opened for. Ticking reads the invoice's state (`GetKeAsync`) when it is not known.
- **Top right: «Produse Factura nr / data»** (the title follows the ticks: one invoice, «Produse facturi bifate (n)», none): the lines of the ticked invoices in ONE `KBotDataView` grouped by
  invoice (group header = «nr / data» and the sums, `ShowHeaderAggregates`). Columns: «Număr / Dată factură», «Denumire», «Valoare cu TVA», **«Recepționată»** (editable; proposed = the line's
  value, cut to what the invoice still has on that classification and, for an existing reception, to what that reception still has; typed amounts are kept and taken out first; must have the
  line's sign and not exceed its value), «Clasificație» (combo from the indicators of the chosen angajament). A classification chosen on a line is copied to the lines BELOW it, of the same invoice,
  that have none.
- **Bottom left: «Angajamente»**: grid of the angajamente the ticked invoices can go to (the server's candidates: DDF of the supplier, links already made) + those added here; the CURRENT row is the angajament of
  the reception; the existing rows are inert (cannot be changed); an empty row is ALWAYS last: a combo with every angajament the server lists (typing narrows it), free text = code, else the server's search
  (one hit adds it; none / many = a notice). Per-row button «…» -> «Elimină asociere» (see assumptions).
- **Bottom right: «Date recepție …»** (title: «nouă» for a new reception; else «Date recepție <descriere> (<data>)» of the chosen one), a row split 35% / 65%: on the left Tip / Data / Descriere (new
  reception) or the list of the angajament's receptions (existing / modified), on the right the grid of amounts by classification: «Clasificație», «Valoare factură», «Valoare recepție»
  (= sum of «Recepționată») and, for an existing reception only, «Rest recepție existentă». Read-only. Date (latest invoice date) and description («plata factura n/d» / «plata facturi ...») follow the ticks until the operator writes his own.
- Footer: «Renunță» and the action button; **«Ca la prima linie» and «Alt angajament…» are gone** (the first by request; the second replaced by the empty row). Window wider (1320x840, min 1100x720).

`PrimiteView`: `LinkAsync` (the three buttons of the invoice view) and the list's «Recepție nouă» both open the window through ONE routine, `LinkManyAsync` (file `PrimiteView.ReceptieMultipla.vb`, rewritten):
window -> save the changed classifications -> server's check of every invoice -> (new / modified) ONE reception in FOREXE with the rows of all invoices added up per indicator and ONE marker -> every invoice gets its link.
The KE10 sequence of one window per invoice (`FixeazaReceptiaNoua`) is gone. For a MODIFIED reception with several invoices the value written is its value now plus all the amounts. New class `KeLegaFactura` (the result per invoice).
`NewReceptionAsync`, `ChangeReceptionAsync`, `WriteLinkAsync`, `RowsOf`, `InvoiceLabel` removed (nothing else used them). The server and the contract are unchanged.

## Corrections after the first run (18.10.2026, the operator's annotated screenshot)
- Tree: the ticks were missing (every node needs `HasCheckBox`, `CheckBoxes` alone shows none) and the header was white: it now has the look of the received-invoices tree
  (Control colour, gradient, Calibri); the first invoice asked for is selected and revealed. The «4.869,36» on the right was the TICKED invoice (the one the window was opened on);
  the highlighted tree row was another one: with the ticks shown the two can no longer be mixed up.
- Classification choices show ONLY the classification code (no name).
- A classification picked with the mouse is written at once and copied to the lines below (new `KBotDataColumn.CommitOnSelect` + `KBotComboBox.ItemPicked`), it no longer waits for the cursor to leave the row.
- The existing rows of «Angajamente» are no longer greyed: `KBotCellFormattingEventArgs.ReadOnlyCell` (new) makes a click open no editor and nothing else. Documented in `KBotDataView.md`.
- Build after the corrections: 0 warnings, 0 errors; still nothing seen on screen.

## Second correction round (18.10.2026)
- No tooltips on the four grids of the window (`tips` text removed from gridLinii, gridAng, gridReceptii, gridAloc). The grid's own overflow label (cut cell text) is a different thing and stays.
- **Combo in the header band of an invoice** (the mock-up's «daca e posibil», first called not possible): new, general feature of `KBotDataView`: `KBotDataColumn.GroupHeaderCombo`, event
  `GroupHeaderValuePicked` (`KBotGroupHeaderValueEventArgs`), file `KBotDataView.GroupEditor.vb`, floating `groupCombo` in the grid's Designer; documented in `KBotDataView.md`.
  The header cell shows the classification all the lines of the invoice share (empty when they differ); a press opens the list (not the fold of the group); picking writes it on ALL the lines of the invoice
  (`KeLegaForm.GridLinii_GroupHeaderValuePicked`). Help text updated (0000-101). Build: 0 warnings, 0 errors; NOT seen on screen: the placement of the combo over the header cell, the list opening, behaviour with the grid scrolled
  (a layout pass closes the combo when it has no focus).

## Third correction round (18.10.2026)
- «Leagă de recepție»: the receptions whose rest is 0 on every classification are not listed (they can take no invoice). Only this mode: «Modifică recepția» still lists them (a reception covered in full can be raised).
- «Descriere» is a multi-line box (`KBotTextBox`, scroll bars when needed) filling the cell under Tip / Data; a line break, typed or pasted, becomes a space before the text goes to FOREXE (one line there). Help text updated (0000-101). Build: 0 warnings, 0 errors; not seen on screen.

## Decisions taken without the operator (to confirm)
1. **«Elimină asociere»**: an angajament added in the window just leaves the list; one that comes from the server asks a question and removes the operator's MANUAL links of the ticked invoices with a DDF of that
   angajament (`GetPrimitaAsync` -> `LegaturiManuale` -> `UnlinkPrimitaAsync`). An association that comes from the supplier being a partner of the DDF, or from links with receptions, is not removed
   (a message says where to do it). Adding an angajament on the empty row does NOT write an association (there is no server route for «invoice <-> angajament» besides the DDF link): it only chooses the angajament for this reception.
2. The reception list of the existing / modified modes sits in the 35% cell where the new-reception fields are (the mock-up has only the fields).
3. «Valoare factură» = the value of the lines on that classification (not the rest after earlier links); the rest shows in the proposed amounts.
4. The fill-down of a classification stays inside the same invoice.
5. Columns «Indicator» and «Rest factură» of the old grid are gone (the mock-up has three columns); a classification that is not an indicator of the angajament is marked in the grid and the button refuses it, as before.

## Files touched
`KBot.EFactura/Forms/KeLegaForm.Designer.vb` (rewritten), `KeLegaForm.vb` (rewritten), `KeLegaFactura.vb` (new), `Views/Primite/PrimiteView.ReceptieMultipla.vb` (rewritten), `PrimiteView.Receptii.vb`;
help `HelpContent/contabil/efactura/index.md` (0000-101).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen, no test written or run (operator rule).
`Check-Help.ps1`: the only complaint about this topic was the missing worklog for 0000-101 (written); the tutorial warnings are old.

## Unverified / deferred
- NOTHING seen on screen: the proportions (27/73, 24/76, 35/65, 52/48), the grouped grid with editable cells, the always-last empty row with its inert neighbours (they show the grey «inert» look), the hidden button of the empty row,
  the receptions list in 35% of the width, the tree check propagation, the DPI behaviour. The designer was never opened in Visual Studio.
- ~~Combo on the group row~~ done in the second correction round (below).
- Existing reception with several invoices: each invoice is checked by the server alone; the sum over the invoices is checked in the window (`ReceptionFitProblem`), the server's `leaga` checks again per invoice.
- The window reads at most the newest 300 angajamente for the empty row (the server's cap); anything else is found by the server's search on the typed text.
- All the old KE caveats stay: SQL not run, server not deployed, the two workflows not run on the real site.
