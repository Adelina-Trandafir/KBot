# SLICE-KE12-02 - Tree of the page «Recepții»: search, closed start, no supplier tick, newest first (18.10.2026)

## What changed and why
Operator corrections on the tree «Facturi de recepționat» of the page «Recepții» (KE12), from a screenshot:
- **Search button in the header** (the lens, as in the received-invoices tree: same `HeaderSearchIcon` and search-band settings); it finds a supplier by name or an invoice by number (the control's search over the captions; at least 3 characters).
- **The tree starts closed**: the suppliers are not expanded.
- **The supplier has NO tick** (it is only a heading); the ticks are on the invoices.
- **Invoices in descending order of date** under each supplier (newest first).
- **Invoices already covered are not listed**: an invoice whose links, over one or several receptions, add up to its whole total is left out (as before); now also an invoice whose total is 0 (nothing to receive;
  the screenshot showed a supplier at 0,00).
Tooltip of the tree and help (`contabil/efactura/index.md`, 0000-103) follow.

## Second part: the grid of products (same day)
- Each ticked invoice starts COLLAPSED (`CollapsedByDefault` on the group level of `gridLinii`).
- The invoice's header band has no coloured background (white; the scheme's blue is not used) and keeps only bold text and a frame a little more visible than the cell lines: new `KBotGroupLevel.HeaderBorder` (documented in `KBotDataView.md`), painted by `DrawGroupBand`.

## Third part: the reservation check and the columns of the amounts (same day; corrected twice)
- The grid of amounts has the columns of the section A of the fundamentation document: **Buget** | **Recepții** | **Disponibil** (= Buget - Recepții) | **Valoare** (= the sum of «Recepționată») | **Rămas** (= Disponibil - Valoare), then, only for an existing reception, «Rest recepție existentă». «Valoare factură» is gone.
- **The principle of the ordonanțare (routes/forexe/ddf_edit.py, budget_on_day.py)**, taken as it is: Buget = the budget of the classification ON THE DAY (`budget_on_day`: the version in force + its rectifications; no version that day = today's credit, `FX_Indicatori_Buget.CreditBugetar`,
  and the server says so); Recepții = `FX_Receptii` of the indicator with `DATE(Data)` STRICTLY BEFORE the day. The day is the date of the new reception, or the date of the chosen one (existing / modified).
- **New server route** `GET /api/efactura/ke/angajament/<cod>/disponibil?data=aaaa-ll-zz` (`primite_ke.disponibil`, `primite_ke_routes.ke_disponibil`; the forexe module is imported inside the function), client `IEFacturaApi.GetKeDisponibilAsync`, domain `EFacturaKeDisponibil` / `EFacturaKeDisponibilZi`.
  Server NOT deployed (like all KE), py_compile clean, nothing called. The page asks once per (angajament, day) and keeps the answer; a date change, a reception chosen or a mode change asks again (`KeLegaForm.RefreshDispoAsync`).
- **The check** (`AvailabilityProblem`, from `RefreshOk`, so it runs on every classification picked on a line or on an invoice header, on a typed amount, a new day, angajament or mode): on every indicator the sum of «Recepționată» of all the ticked invoices must be at most Disponibil.
  Otherwise an ERROR notice and the button stays off. Not for «Leagă de o recepție existentă»; a storno (negative sum) is not checked; while the answer has not come, or the budget is unknown, nothing is checked.
  This replaces my first version of the check, which used `AngajamentLegal - Receptii` of the indicator (wrong source for the columns the operator described).
- The reading of «strictly before»: a reception of the SAME day does not count in «Recepții» (as in the ordonanțare). Two receptions on one day on one indicator are therefore not added up by this check; to confirm.

## Fourth part: the list of the empty angajament row (same day)
- The list offered by the empty row of «Angajamente» no longer contains the angajamente already in the grid (the ones of the same invoices / supplier): `FillAngajamenteChoices` leaves them out and is called every time the rows are rebuilt. A code typed by hand that is already there just selects that row (nothing is added twice). Not seen on screen.

## Fifth part: the classifications of ALL the angajamente (same day)
- The list of classifications in the grid of products holds the indicators of EVERY angajament of the «Angajamente» grid, not only the one in use: the page reads the others in the background (`BeginLoadOtherAngajamente`, `KnownData`, kept in the same cache) and rebuilds the list (`ApplyIndicators`). The same classification in two angajamente is one entry.
- A classification picked (on a line or on an invoice header) that is NOT an indicator of the angajament in use moves the use to the first angajament that has it (`EnsureAngajamentFor`; the row of the grid follows, the receptions and the amounts are read for it). The other lines are judged against the new angajament at the button (the existing message: the classification is not an indicator of the angajament). One reception is on one angajament, so lines whose classifications belong to different angajamente cannot go together.
- Not seen on screen; the first list shows the current angajament's classifications until the others have been read.

## Sixth part: the supplier shows its angajamente; the navbar of the reception (same day, KE12-05)
- The angajamente belong to the supplier, not to the invoice: pressing a supplier node (or an invoice row) in the tree, while NO invoice is ticked, reads the state of the supplier's newest invoice (`Tree_NodeMouseDown`, the supplier node's `Tag` is that invoice id, `_browsed`) and fills «Angajamente» from it (`FillAngajamente` takes the ticked invoices, else the browsed one). Once something is ticked the ticks decide; a reload forgets it. One angajament = it becomes the current one and its receptions are read.
- The combo of the mode is replaced by a horizontal `KBotNavList` (`navMod`): «Recepție nouă» | «Recepții existente». Under «Recepții existente» a small combo «Acțiune» (`cmbMod`) keeps the two things the page did with an existing reception: «Leagă facturile de recepție» (only links; receptions with rest 0 still hidden) and «Modifică recepția (valorile cresc)». The mode is computed by `ChangeMode` from both. Operator said only «Recepție nouă» / «Recepții existente»: the link-or-modify choice is mine to keep (nothing may be written in FOREXE by guessing); to confirm.
- Fix: an angajament picked on the empty row of «Angajamente» kept the receptions and indicators of the previous one: `BeginAddAngajament` set `_angCod` first, so `UseAngajamentAsync` saw «same code, data present» and returned. The guard now also requires that `_data` is the cached data of that very angajament.
- Single indicator (KE12-06): when the angajament pressed in «Angajamente» has exactly ONE indicator, every line of the products grid takes its classification (`ApplySingleIndicator(True)` from `UseAngajamentAsync`; pressing another angajament changes them again). Invoices ticked later fill only their lines without a classification (`ApplySingleIndicator(False)` from `ApplyChecked`). An angajament with several indicators leaves the lines as they are. Build 0/0, not seen on screen.
- After a reception (KE12-07) the tree is no longer rebuilt: `ReloadAsync(True)` reads the list and `PruneTree` removes only the invoices now covered (and a supplier left empty), unticks the rest and refreshes their numbers and the supplier totals; expansion, selection and scroll stay. New `AdvancedTreeControl.RemoveItem`. The caches of the budget (`_dispoCache`) are now also cleared on a reload (they would have kept the receptions of before). Not seen on screen.
- Not seen on screen.

## Files touched
`KBot.EFactura/Forms/KeLegaForm.vb`, `KeLegaForm.Designer.vb`; server `PYTHON/routes/efactura/primite_ke.py`, `primite_ke_routes.py`; `KBot.Api/IEFacturaApi.vb`, `ApiClient.EFactura.Ke.vb`; `KBot.Domain/EFacturaKe.vb`; help `HelpContent/contabil/efactura/index.md`.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Whether the control's search finds the invoices of a supplier when the name is typed (it does in the received-invoices tree, which uses the same control and defaults); not seen on this tree.
- «Covered» uses the `Receptii` the list carries (sum of `Suma` per invoice, in absolute value, tolerance 0.01); an invoice with receptions still waiting for the download counts with their sums.
