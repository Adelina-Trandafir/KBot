# SLICE-KE16…17 — Propuneri de legare în bloc și recepții parțiale

KE16 introduce propuneri 1:1, apoi corectează contextul furnizorului. KE17 permite sume parțiale și mai multe facturi pe aceeași recepție, cu reverificarea totalului în tranzacție; regula propunerii automate rămâne pentru valoarea întreagă și o singură clasificație. Limitarea selecțiilor furnizorului la memoria paginii este checkpointul KE16; [0121](SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md) și [0122](SLICE-0122-parteneri-fara-idpartener.md) documentează ulterior persistența și conversia codurilor, cu limitele lor.

- [KE16](#slice-KE16)
- [KE17](#slice-KE17)

## SLICE-KE16 - Page «Recepții»: bulk link of a supplier's invoices to receptions, as PROPOSALS (19.10.2026) <a id="slice-KE16"></a>

Operator (19.10.2026): in the tree of suppliers of the page «Recepții», on hover of a supplier, a button opens a modal window that links the unlinked invoices in bulk. It must PROPOSE, not do. The rule stays «Reception = Invoice»; when several receptions and invoices have the same value, a tie-break: the reception date may be equal to the invoice date up to +2 days (receptions are usually made within 2 days of receiving the invoice). In the modal NO selection per invoice line and NO partial reception: 1 = 1.

### What changed and why
- **Server** (`PYTHON/routes/efactura/primite_ke_propuneri.py`, routes in `primite_ke_routes.py`):
  - `POST /api/efactura/ke/propuneri {facturi:[IdPrimita]}`, readonly: invoices without ANY link; supplier DDF/manual-association angajamente from `_angajamente_candidate`; reception rest on exactly ONE classification equals the whole signed invoice total. Lines must sum to that total, have at most one classification, and any accountant-chosen classification must match the reception, without overwriting that choice. One candidate is proposed directly. Multiple candidates: propose only the unique one in invoice-date…+ZILE_DUPA_FACTURA (2); otherwise list all with no proposal. If invoices compete for a reception, only one alone in its date window keeps it. Per invoice: candidates (Zile/InFereastra), propunere (IDRR/null), motiv.
  - `POST /api/efactura/ke/propuneri/aplica {legaturi:[{IdPrimita, CodAngajament, IDRR}]}` rechecks each pair (still unlinked, reception still has the exact invoice value), then writes the whole batch atomically or returns 409 NU_MAI_ESTE_VALIDA. Unclassified lines get the reception classification; EF_PrimiteReceptii gets one existenta row, Suma = invoice total, UN = user. Audit EF_KE_LEGA_MASA; Contabil role on both routes.
  - The date window is a constant on the server, not a setting.
- **Client:** EFacturaKePropuneri* in KBot.Domain/EFacturaKe.vb; GetKePropuneriAsync/ApplyKePropuneriAsync in IEFacturaApi and ApiClient.EFactura.Ke.vb. New modal KePropuneriForm (+ Designer, all controls declared): top invoices grid (ticks, proposed reception/date/remarks), bottom candidates for current invoice (double-click chooses/ticks), Leagă (n), Renunță, notice bar. A tick requires a reception, reserved to one ticked invoice; conflicts give notice.
- **Tree, KeLegaForm:** StyleSupplier, including after RefreshTree, adds hover-only RightIcon for suppliers with unlinked invoices: FacturaIcons.Link (two chain links), per-node ShowRightIconOnHover. Tree_RightIconClicked opens the modal, rereads invoices and reports linked count.

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
- **Leftover-row cause, read in code:** page-wide `_added` followed every supplier. Now keyed by SupplierKey; ContextKeys takes suppliers of ticked invoices, otherwise the tree selection, for add/remove/display.
- **Probable, unverified in data:** server had no angajament for this supplier (no DDF partner/invoice selection), leaving no comparison. Message: «Furnizorul nu are niciun angajament: alegeți-l în tabelul «Angajamente» al paginii și deschideți din nou propunerile.»
- Both routes receive supplier-picked angajamente, validated against FX_Angajamente for all that supplier invoices. `_cods_furnizor` also reads manual associations/links of ANY of its invoices, not only the current one.
- **Memory only:** selections last until page close. Durable supplier ↔ angajament would require a write (e.g. DDF partner row), not implemented pending operator decision.

## SLICE-KE17 - «Propuneri» window: angajament per invoice, written sum, grouped receptions with «Rămas» (20.10.2026) <a id="slice-KE17"></a>

Operator, after seeing the modal of KE16 (supplier APA NOVA PLOIESTI, 9 invoices, one proposal):
1. in the invoices grid, a choice of the angajament when the invoice has more than one;
2. a new editable column «Recepție» after «Valoare»: the value the accountant receives from that invoice. The reception must know how much was associated from it, so that the table below, after the row above changes, is updated, and a reception left at 0 disappears;
3. the receptions table in two groups: group 1 = the receptions with the same value, group 2 = the rest, not fully associated;
4. a column «Rămas» in the receptions table: the value of the reception minus the total associated.

KE17 lifts KE16 no-partial/1:1 at the operator request: automatic proposals still pair whole values, but manual sums may be below invoice total and several invoices may share available reception rest.

### What changed and why
- **Server** (`primite_ke_propuneri.py`, answer and body shapes in the docstring of `primite_ke_routes.py`):
  - `propuneri` returns all remaining receptions of the invoices angajamente, per reception/classification: receptii {IDRR, CodAngajament, DataR, TipReceptie, Descriere, IdClsf, Clsf, Rest}; named angajamente list; per invoice: angajamente codes, angajament (proposal first, otherwise first with fitting reception), IdClsfLinii, propunere {IDRR, IdClsf, CodAngajament}/null, motiv. Automatic eligibility remains [KE16](#slice-KE16): whole value, ONE classification, 2-day window and invoice conflicts.
  - `aplica` receives {IdPrimita, CodAngajament, IDRR, IdClsf, Suma}. Rechecks: invoice unlinked; angajament is among the supplier candidates; sum has invoice sign and does not exceed its total; classification matches classified lines; reception has rest on it; combined pairs per reception/classification fit the rest. Atomic batch or 409 NU_MAI_ESTE_VALIDA, with unclassified lines filled and one existenta link/Suma. Still one pair per invoice.
- **Contract / client**: `EFacturaKePropunereReceptie` (now the rest of a reception on ONE classification), new `EFacturaKePropunereAngajament`, `EFacturaKePropunereAleasa`; `EFacturaKePropunereFactura` (Angajamente, Angajament, IdClsfLinii, Propunere object), `EFacturaKePropuneri` (Angajamente, Receptii), `EFacturaKeLegaturaMasa` (+ IdClsf, Suma). Wire classes and both calls in `ApiClient.EFactura.Ke.vb`.
- **Window** (`KePropuneriForm` + Designer):
  - invoices grid: new columns «Recepție» (editable number, default = the whole invoice when a reception is chosen, else 0) and «Angajament» (combo, committed on pick; a list only when the invoice has several, else read-only); «Recepția» (was «Recepția propusă») shows the reception chosen.
  - Suma must parse in operator format, have invoice sign and not exceed total; with reception chosen/tick on, not exceed that row available rest. Zero clears/unticks; changing angajament clears the old choice.
  - Before drawing candidates, subtract ticked allocations per reception row. Omit those without rest for the current invoice, retaining its own choice/allocated part. Ticking requires reception/sum/sufficient rest; Leagă rechecks sums before sending.
  - Hidden grup column groups receptions: «1 · Aceeași valoare ca suma de recepționat» (rest = written sum, otherwise invoice total), «2 · Neasociate integral». Rămas = rest − ticked allocations, including current row. Double-click picks reception/sought sum capped at available rest, with notice; shared receptions allowed while rest lasts.
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
