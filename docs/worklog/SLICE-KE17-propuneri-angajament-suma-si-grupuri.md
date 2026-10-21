# SLICE-KE17 - «Propuneri» window: angajament per invoice, written sum, grouped receptions with «Rămas» (20.10.2026)

Operator, after seeing the modal of KE16 (supplier APA NOVA PLOIESTI, 9 invoices, one proposal):
1. in the invoices grid, a choice of the angajament when the invoice has more than one;
2. a new editable column «Recepție» after «Valoare»: the value the accountant receives from that invoice. The reception must know how much was associated from it, so that the table below, after the row above changes, is updated, and a reception left at 0 disappears;
3. the receptions table in two groups: group 1 = the receptions with the same value, group 2 = the rest, not fully associated;
4. a column «Rămas» in the receptions table: the value of the reception minus the total associated.

This LIFTS the KE16 rule «no partial reception, 1 = 1» at the operator's request: the proposal (what comes ticked) is still the whole invoice on a whole reception, but a sum below the invoice is now allowed, and a reception may serve several invoices while its rest lasts.

## What changed and why
- **Server** (`primite_ke_propuneri.py`, answer and body shapes in the docstring of `primite_ke_routes.py`):
  - `propuneri` now answers with ALL the receptions that still have rest on the angajamente of the invoices, one row per reception AND classification (`receptii`: IDRR, CodAngajament, DataR, TipReceptie, Descriere, IdClsf, Clsf, Rest), the list of angajamente with their names (`angajamente`), and per invoice: `angajamente` (the codes it can take), `angajament` (the one shown first: the proposal's, else the first with a reception that fits), `IdClsfLinii` (the classification its lines already have), `propunere` = `{IDRR, IdClsf, CodAngajament}` or null, `motiv`. The proposal rule itself is unchanged (whole value, rest on ONE classification, date window of 2 days, conflicts between invoices).
  - `aplica` takes `{IdPrimita, CodAngajament, IDRR, IdClsf, Suma}` per link. Re-checked against the database: invoice still without links, the angajament is one of the supplier's, `Suma` has the invoice's sign and is not above its total, the classification is the lines' (when they have one), the reception still has rest on that classification, and the sums of the pairs that take from the same reception + classification fit together in its rest. All written in one transaction (lines without classification get it; one `existenta` link row with `Suma`), or none (409 `NU_MAI_ESTE_VALIDA`). An invoice is still in one pair only.
- **Contract / client**: `EFacturaKePropunereReceptie` (now the rest of a reception on ONE classification), new `EFacturaKePropunereAngajament`, `EFacturaKePropunereAleasa`; `EFacturaKePropunereFactura` (Angajamente, Angajament, IdClsfLinii, Propunere object), `EFacturaKePropuneri` (Angajamente, Receptii), `EFacturaKeLegaturaMasa` (+ IdClsf, Suma). Wire classes and both calls in `ApiClient.EFactura.Ke.vb`.
- **Window** (`KePropuneriForm` + Designer):
  - invoices grid: new columns «Recepție» (editable number, default = the whole invoice when a reception is chosen, else 0) and «Angajament» (combo, committed on pick; a list only when the invoice has several, else read-only); «Recepția» (was «Recepția propusă») shows the reception chosen.
  - a sum is accepted when it is a number in the operator's own format, has the sign of the invoice, is not above the invoice, and (reception chosen + row ticked) not above what the reception still has for this row. 0 clears the choice and unticks the row. Another angajament clears the choice of the old one.
  - what the ticked rows take is subtracted per reception row before the table below is drawn; a reception with nothing left for the current invoice is not listed (the one this row already holds stays, since it has its own part). A tick needs a reception and a sum and is refused when the reception has no longer enough; «Leagă» checks the sums once more before sending.
  - receptions table: two groups (grid grouping on a hidden «grup» column): «1 · Aceeași valoare ca suma de recepționat» (rest equals the sum written, else the invoice total) and «2 · Neasociate integral»; new column «Rămas» = rest − what the ticked invoices take (the current row included). Double click picks a reception and takes the sum looked for, cut to what the reception has (a notice says so); several invoices may pick the same reception while it lasts.
  - only receptions of the chosen angajament, of the invoice's sign and (if the lines have a classification) of that classification are listed.
- **Help**: `contabil/efactura/index.md` paragraph «Legare în bloc, cu propuneri» rewritten (sub-slice 0000-115).

## Files touched
`PYTHON/routes/efactura/primite_ke_propuneri.py`, `primite_ke_routes.py` (docstring); `src/KBot.Domain/EFacturaKe.vb`; `src/KBot.Api/ApiClient.EFactura.Ke.vb`; `src/KBot.EFactura/Forms/KePropuneriForm.vb` + `.Designer.vb`; help `contabil/efactura/index.md`.

## Test results
`dotnet build src\KBot.EFactura\KBot.EFactura.vbproj`: 0 warnings, 0 errors. Python: `py_compile` and `import` of both modules OK (PYTHON\.venv). Nothing run against a database, nothing seen on screen, no test written (house rule). The server is NOT deployed (the operator deploys by hand); the client needs the new server: an old server answers with the old shape.

## Unverified / limits
- Not seen on screen: the new columns, the combo, the two groups (grouping on a hidden column, as `gridLinii` does), the look of «Angajament» when the invoice has only one, the Designer file in the VS designer.
- Whether the combo list set on selection change (`FillCandidates`) is in place before the editor opens is not known; `CellValidating` refuses anything that is not one of the invoice's own labels anyway.
- An invoice linked for LESS than its total now has a link, so it is no longer «neasociată» for this window: the rest is linked from the page «Recepții» (the help says so).
- A reception with rest on SEVERAL classifications is never proposed, but is listed (one row per classification) and may be picked by hand.
- Only «Rămas» was added to the receptions table, as asked, not the value of the reception.
