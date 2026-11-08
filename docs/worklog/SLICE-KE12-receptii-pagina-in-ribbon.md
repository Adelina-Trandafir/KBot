# SLICE-KE11…15 — The reception window «Recepții <> EFACTURA» (removed in KE21)

The window and its ribbon button were REMOVED on 08.11.2026 ([KE21-02](SLICE-KE21-02-view-recepti-comun.md)): the same work is done by the shared view `KeReceptiiView`, in the form «Facturi primite» and in the shell's E-Factura view of an angajament. This file keeps only what the window did and the lessons that outlive it. [Tick boxes](SLICE-KE12-03-dgv-casete-de-bifat.md) and [proposals](SLICE-KE16-propuneri-legare-in-bloc.md) are separate.

## What the window did (operator requests 18-19.10.2026)

- **One window for one or several invoices** (KE11), later a stand-alone page of the ribbon (KE12) and an embedded variant in the E-Factura views (KE12-04). It linked invoices to a FOREXE reception: new, existing or modified. The work was: save the changed classifications, check every invoice on the server, write ONE reception in FOREXE with the rows of all invoices added up per indicator and ONE marker, link every invoice, report failures.
- **Tree of invoices** (KE12-02, KE13): suppliers as headings without a tick, invoices newest first, search band, header options menu. Every invoice is listed; one whose receptions cover its total is ticked and locked (grey box, dim text, tooltip with the receptions).
- **Grid of products** grouped by invoice: «Recepționată» (editable, proposed from the line value and what the invoice and the reception still have) and «Clasificație» (combo; a pick is copied to the lines below it; a combo in the invoice's header band sets all its lines). When the angajament has one single indicator every line takes it; a classification of another angajament moves the choice to the first angajament that has it.
- **«Angajamente»**: the supplier's angajamente (DDF of the supplier, links made, added by hand on the always-last empty row); one with a linked reception cannot be taken off the list; «Elimină asociere» removes only the operator's manual links.
- **«Date recepție»**: tabs Adaugă / Asociate / Existente and a grid of amounts per classification: Buget | Recepții | Disponibil | Valoare | Rămas (+ «Rest recepție existentă»), judged on the reception's day as the ordonanțare does (budget of that day; receptions STRICTLY before the day). A sum above Disponibil blocked the button.
- **A resolved invoice** (KE14): a press showed it read-only: its products, angajament, the reception it is linked to, and «Elimină asociere» under the receptions table.
- **KE15**: typed amounts are read in the operator's Windows format; the message about a reception without rest says on which classifications the rest is.

## What outlives the window

- Control features built for it: `KBotDataView` `GroupHeaderCombo`, `CommitOnSelect`, `ReadOnlyCell`, `KBotGroupLevel.HeaderBorder`; `TreeItem.CheckLocked`, `AdvancedTreeControl.RemoveItem`; ribbon `SeparatorAfter`.
- Server route `GET /api/efactura/ke/angajament/<cod>/disponibil?data=` (client `GetKeDisponibilAsync`), class `KeLegaFactura` (result per invoice).

## Traps to remember

- `KBotDateEditBox` showed only «04.0»: its text rectangle was set on WM_SIZE, before WinForms updated `ClientRectangle`; it is now also set on `OnClientSizeChanged`.
- Amounts must be parsed with `CurrentCulture`, the culture the grid paints and edits in; `ro-RO` reads «77.49» as 7749 for a user with a dot decimal.
- An angajament picked on the empty row kept the receptions of the previous one because the code was set before the data guard ran: the guard must also check that the cached data is that angajament's.
- «Recepții» in the amounts uses `DATE(Data)` strictly before the day: two receptions of the same day on one indicator are not added up by the check.
- A saved DDF revision does not change `FX_Rezervari`: the reserved figures change only after the revision is sent to FOREXE and read back.

Never run with real data; the old KE caveats stay (server not deployed by me, the two workflows not run on the real site).
