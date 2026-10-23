# SLICE-0123-02 - Pagina «VERCON Rapoarte» in K-BOT.app, dupa principiul ADECHIT (23.10.2026)

Operator: bring the VERCON reports into K-BOT.app on the principle of the ADECHIT page, with «that tree combo with the months», the **Subunitate** and **Gestiune** choices, and everything
**exactly as on the VERCON site**. The server keeps building the reports (same code, same PDF / Excel); the page only chooses the parameters, shows the PDF and saves the Excel.

## What changed and why

### Server (PYTHON) - NOT deployed, the operator publishes it
- **`routes/vercon/kbot_bridge.py`** (new) + one branch in `routes/vercon/__init__.py` (`guard`): the SAME routes accept a K-BOT bearer token, but only for GET `context`, `reports/catalog`,
  `reports/<name>/selection`, `reports/<name>/generate`, `reports/export/<token>/(pdf|xlsx)` and DELETE `reports/export/<token>`. The identity (`email` = the session's user name, `db_name`) goes into `g.portal`,
  so the VR rights (`drepturi.operations_of(..., 'VR')`), the context tree and the snapshot owner work as for a site user. Documents, nomenclatures, locks and every write stay closed to a bearer token.
- **`routes/vercon/report_routes.py`**: `DELETE /api/vercon/reports/export/<token>` (204) releases the saved report (the VERCON half of the release that ADECHIT got in 0123-01; `reporting/snapshots.py` already has `release`).

### K-BOT
- **KBot.Domain**: `Vercon.vb` (plain data: subunit, gestiune, selection, context, report spec, period, catalog, selection node / branch, report handle) and `VerconReportLogic.vb` - the rules the site keeps in its JS, ported
  without change: `VerconReportTree` (the catalog as a tree: Magazie > Produse / Intrari / Iesiri / Liste de alimente, the OI / MF branches, the group of every report for the tree reuse, labels in capitals),
  `VerconPeriods` (the period list: «Anul curent pana azi», «TOTI anii», per year «Tot anul» + the months up to the last with movement, newest first), `VerconSelectionLogic` (which rows are data rows / boxed branches,
  which ticked rows become which filter - `variant_ids`, `receipt_selection`, `outgoing_selection`, `mf_selection`..., «all ticked = no filter unless required», the one-choice errors).
- **KBot.Api**: `IVerconApi`, `ApiClient.Vercon.vb` (context, catalog, selection branch, generate, download pdf / xlsx, release). Sends `X-Vercon-Unit` (the session's unit); subunit / gestiune / year go in the query.
- **KBot.App/Views/Vercon/`VerconRapoarteForm`** (+ Designer, all controls declared there; the Designer was written from a helper script in the scratchpad, the output is plain VB), three columns:
  1. **Choices**: Subunitate, Gestiune (only when the subunit has several), An (not for OI / MF), «Selectează perioada» with the **tree combo** (`KBotComboBox` with grouped items: year = foldable header, rows «Tot anul» + months;
     editable with find-as-you-type; shows «Interval personalizat» when the two dates match no row), **Dată început / Dată sfârșit** (`KBotDatePicker`, bounded to the years of the data; one «La data» for the depreciation report),
     the option rows (shown per report: Grupează pe cont..., Afiseaza FARA miscari, Afișează Documente, Afișează Primitori, Grupează după Primitori), and the **report tree** (`AdvancedTreeControl`, first group open, a press on a group opens it and closes the other top groups).
  2. **Selectare date**: `AdvancedTreeControl` with a box per row, branches read on demand (`RequestLazyLoad`); ticking an unread branch reads it first; «Selectează TOATE» reads everything and ticks; the search (magnifier) reads the unread branches first;
     the reason a report cannot be asked for yet is written under the tree; **Export Excel** and **Listare** buttons.
  3. **The report**: PDF in the embedded Adobe surface (`ReaderHostPreview`, as ADECHIT) and the notice line.
  The reports of one group share the selection tree already read (as on the site); the MF / OI reports each read their own. A change of period keeps the ticks that are still valid. The report held on the server is released when
  replaced, when the context changes and when the page closes. `KbotForm.Vercon.vb`, registry entry `vercon_rapoarte`, ribbon button «Rapoarte» in the VERCON tab (before «Trimite la Vercon»), dispatch in `AvacontMain.vb`.
- **KBot.Controls** (1.67.0.0 -> 1.68.0.0), `AdvancedTreeControl`: `CompleteLazyLoad(item)` (removes the «Loading...» row, stops the spinner when no node waits - the lazy path never stopped its timer), event `SearchStarting(text)` + `RerunSearch()`
  (a host that loads on demand reads the rest before the search runs). Nothing changes for existing hosts.
- **Help** `contabil/vercon-rapoarte.md` (new, 0123-02), `contabil/ribbon.md` (the VERCON row). `Check-Help.ps1`: no error on these two files; the 22 errors it lists are older ones (tutorials, KE12 tags, a tour).

## Files touched
`PYTHON/routes/vercon/kbot_bridge.py` (new), `PYTHON/routes/vercon/__init__.py`, `PYTHON/routes/vercon/report_routes.py`; `src/KBot.Domain/Vercon.vb`, `VerconReportLogic.vb`; `src/KBot.Api/IVerconApi.vb`, `ApiClient.Vercon.vb`;
`src/KBot.App/Views/Vercon/VerconRapoarteForm.vb` + `.Designer.vb`, `KbotForm.Vercon.vb`, `KbotForm.Machete.vb`, `AvacontMain.vb`, `AvacontMain.Designer.vb`; `src/KBot.Controls/Tree/AdvancedTreeControl.API.vb`, `.Events.vb`, `.Search.vb`, `KBot.Controls.vbproj`;
help as above.

## Files to publish (server) - the operator does it
`routes/vercon/kbot_bridge.py` (new), `routes/vercon/__init__.py`, `routes/vercon/report_routes.py`, plus what 0123-01 / 0123-03 already listed (`reporting/snapshots.py`, `routes/adechit/*`). Then restart the service.
**Careful:** while this slice was written, other VERCON server files were being changed by someone else in the same working tree (`report_catalog.py`, `report_body.py`, `report_mf.py`, `report_mf_selection.py`, `report_service.py`, new `report_voucher.py`, the site's `reports.js` / `report-result.js`). They are not part of this slice. `report_routes.py` also carries a change that is not mine (`identity` now returns `gestiune_name`). Publish them together or not at all; they were not touched by me.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 warnings, 0 errors. `py_compile` of the three Python files: ok. No test written or run (operator's rule), the application was not started, nothing was called on the server (not deployed).

## Unverified / deferred
- **Nothing ran**: not the page on screen (layout of the three columns, DPI, theme, the grouped period list, the spinner of the lazy rows), not the bearer bridge against the server, not a single report. First real run will show the layout and the lazy tree.
- The bridge assumes `Session.username` is the e-mail of the VR roles (as for ADECHIT); if not, the first call answers 403.
- `AdvancedTreeControl`'s lazy path (`RequestLazyLoad`) was not used by any host before; it is used here for the first time. The ticking of a branch not read yet (read first, then ticked) is written around the control's own propagation and not exercised.
- The tree combo has all years open (the control has no way to start folded); the site opens only the latest year.
- Deliberate differences from the site: the subunit is taken by itself when there is only one (the site waits for a click); the preview has no «Schimbă perioada» button (the period is on the page, press Listare again); no column picker for Excel (all columns).
- The report tree and the selection logic copy the site's JS **as of 23.10.2026** (including the new `tree_group` of the OI reports that appeared meanwhile). If the site's `report-result.js` / `report-selection.js` change again, `VerconReportTree` / `VerconSelectionLogic` must follow.
- The OI reports are being reworked on the server by the other change (new selection modes); this page reads them through the generic selection endpoint, so it should follow, but it was not tried.
- Help: no capture taken for the new page / the VERCON tab.
