# SLICE-KE16…17 — Propuneri de legare în bloc și recepții parțiale

KE16 introduce propuneri 1:1, apoi corectează contextul furnizorului. KE17 permite sume parțiale și mai multe facturi pe aceeași recepție, cu reverificarea totalului în tranzacție; regula propunerii automate rămâne pentru valoarea întreagă și o singură clasificație. Limitarea selecțiilor furnizorului la memoria paginii este checkpointul KE16; [0121](SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md) și [0122](SLICE-0122-parteneri-fara-idpartener.md) documentează ulterior persistența și conversia codurilor, cu limitele lor.

- [KE16](#slice-KE16)
- [KE17](#slice-KE17)

## SLICE-KE16 - Page «Recepții»: bulk link of a supplier's invoices to receptions, as PROPOSALS (19.10.2026) <a id="slice-KE16"></a>

Operator (19.10.2026): in the tree of suppliers of the page «Recepții», on hover of a supplier, a button opens a modal window that links the unlinked invoices in bulk. It must PROPOSE, not do. The rule stays «Reception = Invoice»; when several receptions and invoices have the same value, a tie-break: the reception date may be equal to the invoice date up to +2 days (receptions are usually made within 2 days of receiving the invoice). In the modal NO selection per invoice line and NO partial reception: 1 = 1.

### What changed and why
- **Server** (`PYTHON/routes/efactura/primite_ke_propuneri.py`, routes in `primite_ke_routes.py`):
  - `POST /api/efactura/ke/propuneri {facturi:[IdPrimita]}` (reads only): for each invoice with NO link at all, the receptions of its angajamente (`_angajamente_candidate`: DDFs of the supplier + manual associations) that have rest on exactly ONE classification equal to the whole invoice total (same sign, `Receptie = Factura`); the invoice's lines must add up to the total and not carry two different classifications, and a classification already chosen on the lines must be the reception's (the accountant's choice is not overwritten). Several candidates: the one dated `invoice date .. +ZILE_DUPA_FACTURA (2) days`, if exactly one; none or several = no proposal, all candidates still listed. Two invoices on the same reception: only an invoice that is alone inside its window keeps it. Answer: per invoice the candidates (`Zile`, `InFereastra`), `propunere` (IDRR or null), `motiv`.
  - `POST /api/efactura/ke/propuneri/aplica {legaturi:[{IdPrimita, CodAngajament, IDRR}]}`: re-checks EVERY pair against the database (still no link, reception still has exactly the value), then writes all in one transaction (lines without a classification get the reception's; one `EF_PrimiteReceptii` row of type `existenta`, `Suma` = invoice total, `UN` = the user), or nothing (409 `NU_MAI_ESTE_VALIDA`). Audit `EF_KE_LEGA_MASA`. Role «Contabil» on both.
  - The date window is a constant on the server, not a setting.
- **Client**: contract `EFacturaKePropuneri*` (`KBot.Domain/EFacturaKe.vb`), `GetKePropuneriAsync` / `ApplyKePropuneriAsync` (`IEFacturaApi`, `ApiClient.EFactura.Ke.vb`). New modal `KePropuneriForm` (+ Designer, all controls declared): top grid = the invoices (tick boxes, the reception proposed, its date, remarks), bottom grid = the candidates of the current invoice (double click chooses one and ticks the row), «Leagă (n)» / «Renunță», a notice bar. A tick needs a chosen reception; a reception goes to one ticked invoice only (refused with a notice).
- **Tree** (`KeLegaForm`): a supplier that has invoices without a reception gets `RightIcon` (two chain links, `FacturaIcons.Link`) shown on hover only (per node `ShowRightIconOnHover`), `StyleSupplier` (also after `RefreshTree`); `Tree_RightIconClicked` opens the window, then reads the invoices again and says how many were linked.

### Files touched
`PYTHON/routes/efactura/primite_ke_propuneri.py` (new), `primite_ke_routes.py`; `src/KBot.Domain/EFacturaKe.vb`; `src/KBot.Api/IEFacturaApi.vb`, `ApiClient.EFactura.Ke.vb`; `src/KBot.EFactura/Forms/KePropuneriForm.vb` + `.Designer.vb` (new), `KeLegaForm.vb`, `Views/Vanzare/FacturaIcons.vb`; help `contabil/efactura/index.md` (0000-112).

### Test results
`dotnet build src\KBot.EFactura\KBot.EFactura.vbproj`: 0 warnings, 0 errors. Python: `py_compile` and `import` of the two modules OK (PYTHON\.venv). Nothing run against a database, nothing seen on screen, no test written (house rule).

### Unverified / deferred
- Server NOT deployed; the SQL of the new queries is reused from `primite_ke.py` (`_valori_receptii`, `_alocat_receptii`, `_SQL_INDICATORI`) but the new module was never run against data. The case that must come out: invoice 941 (81.54) -> reception 129 on `...20.01.30` in AAB2KPFMMMN.
- Not seen on screen: the hover button (position next to the amount, tooltip on the whole supplier row), the two grids, the tick boxes in a `ReadOnlyGrid`-less grid.
- Interpretation of the operator's date rule: reception date from the invoice date up to 2 days after (never before it). In the data seen (invoice 31.03, reception 17.04) the rule is not met; there it only matters when values repeat.
- A reception with rest on SEVERAL classifications is never proposed (the invoice would need its lines split).
- The window does not pick receptions of angajamente the supplier has no DDF / association for.

### Update (19.10.2026, operator's first look at the window)
Screenshot: supplier TRANSPORT CALATORI EXPRESS, 7 invoices of 208.00, every row «Nicio recepție cu același rest»; the table of angajamente of the page still showed AAB622THPNT of ANOTHER supplier (TESA MED SOLUTIONS). Operator's rule: **an angajament chosen in the table «Angajamente» belongs to the SUPPLIER, not to the invoice, whether an invoice or the supplier is pressed.**
- Cause of the leftover row (read from the code): `KeLegaForm._added` (the angajamente picked in the empty row) was ONE list for the whole page, so it followed every supplier. Now `_added` is per supplier (`SupplierKey`): `ContextKeys` = the suppliers of the ticked invoices, else the one pressed in the tree; added / removed / shown by those keys.
- Probable cause of «nicio recepție» (NOT seen in data): the server knew no angajament for that supplier (no DDF partner, nothing picked on its invoices), so there was nothing to compare. The reason now says so: «Furnizorul nu are niciun angajament: alegeți-l în tabelul «Angajamente» al paginii și deschideți din nou propunerile.»
- The window sends to the server the angajamente picked in the page for the supplier (`angajamente` in both routes; they count for every invoice of the supplier, validated against `FX_Angajamente`); the server also takes, for a supplier, the manual associations and links of ANY of its invoices (`_cods_furnizor`), not only of the invoice itself.
- Still memory only: what is picked in the table lives until the page is closed. A durable supplier <-> angajament link would be a write (e.g. a partner row of the DDF) and is not made: to decide with the operator.

## SLICE-KE17 - «Propuneri» window: angajament per invoice, written sum, grouped receptions with «Rămas» (20.10.2026) <a id="slice-KE17"></a>

Operator, after seeing the modal of KE16 (supplier APA NOVA PLOIESTI, 9 invoices, one proposal):
1. in the invoices grid, a choice of the angajament when the invoice has more than one;
2. a new editable column «Recepție» after «Valoare»: the value the accountant receives from that invoice. The reception must know how much was associated from it, so that the table below, after the row above changes, is updated, and a reception left at 0 disappears;
3. the receptions table in two groups: group 1 = the receptions with the same value, group 2 = the rest, not fully associated;
4. a column «Rămas» in the receptions table: the value of the reception minus the total associated.

This LIFTS the KE16 rule «no partial reception, 1 = 1» at the operator's request: the proposal (what comes ticked) is still the whole invoice on a whole reception, but a sum below the invoice is now allowed, and a reception may serve several invoices while its rest lasts.

### What changed and why
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

### Files touched
`PYTHON/routes/efactura/primite_ke_propuneri.py`, `primite_ke_routes.py` (docstring); `src/KBot.Domain/EFacturaKe.vb`; `src/KBot.Api/ApiClient.EFactura.Ke.vb`; `src/KBot.EFactura/Forms/KePropuneriForm.vb` + `.Designer.vb`; help `contabil/efactura/index.md`.

### Test results
`dotnet build src\KBot.EFactura\KBot.EFactura.vbproj`: 0 warnings, 0 errors. Python: `py_compile` and `import` of both modules OK (PYTHON\.venv). Nothing run against a database, nothing seen on screen, no test written (house rule). The server is NOT deployed (the operator deploys by hand); the client needs the new server: an old server answers with the old shape.

### Unverified / limits
- Not seen on screen: the new columns, the combo, the two groups (grouping on a hidden column, as `gridLinii` does), the look of «Angajament» when the invoice has only one, the Designer file in the VS designer.
- Whether the combo list set on selection change (`FillCandidates`) is in place before the editor opens is not known; `CellValidating` refuses anything that is not one of the invoice's own labels anyway.
- An invoice linked for LESS than its total now has a link, so it is no longer «neasociată» for this window: the rest is linked from the page «Recepții» (the help says so).
- A reception with rest on SEVERAL classifications is never proposed, but is listed (one row per classification) and may be picked by hand.
- Only «Rămas» was added to the receptions table, as asked, not the value of the reception.
