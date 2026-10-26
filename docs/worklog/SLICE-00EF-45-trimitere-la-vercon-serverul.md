# SLICE-00EF-45/47…49 — Trimiterea facturilor primite către VERCON

45 definește serverul EV01; [46](SLICE-00EF-46-combo-cu-grupe-pliabile.md) păstrează separat controlul EV02. 47 introduce varianta modală EV03, înlocuită în 48 de pagina proprie în ribbon. 49 adaugă contul comun tuturor liniilor și limitează bifarea la un singur furnizor: lunile nu mai au căsuță. Descrierile vechi ale ferestrei și bifării pe luni sunt checkpointuri istorice. Etapele păstrează separat limitele, versiunile și probele de atunci; EV04/EV05 sunt în worklogurile VERCON, nu sunt declarate executate aici.

- [00EF-45](#slice-00EF-45)
- [00EF-47](#slice-00EF-47)
- [00EF-48](#slice-00EF-48)
- [00EF-49](#slice-00EF-49)

## SLICE-00EF-45 - A received invoice SENT to VERCON: the server half (EV01, 21.10.2026) <a id="slice-00EF-45"></a>

Plan: [`docs/EV_PLAN.md`](../EV_PLAN.md) (decisions D1-D12 of the operator). This is EV01 of the plan: the data model and the K-BOT routes. The tree control (EV02), the K-BOT window (EV03) and the VERCON half (EV04, EV05) are later slices of the same family.

### What changed and why
The accountant decides in K-BOT which received invoices go to VERCON, so the Administrator can take them over in «Adaugare NIR». Nothing is copied: the invoice stays in `EF_Primite` / `EF_PrimiteLinii`; a *sending* is a row that says "this invoice, with these proposed accounts, is ready to be taken over".

- **Three new tables** (`sql/EV_01_primite_vercon.sql`, to run by hand on `AVACONT_SURSA` and then sync):
  - `EF_PrimiteVercon`: the sending (`Stare` TRIMISA / PRELUATA / RETURNATA / RETRASA, who and when, note, return reason, who took it over). At most one ACTIVE sending (TRIMISA / PRELUATA) per invoice, checked in the transaction under a lock on the invoice row (MariaDB has no partial unique index).
  - `EF_PrimiteVerconLinii`: ALL the lines of the invoice (decision D10) with the account proposed by the accountant (`IdMasina`, NULL = the Administrator chooses; D9: a proposal).
  - `EF_PrimiteVerconNir`: the NIRs created from the sending (0..n; EV05 writes it). No foreign key to `VC_NIR` on purpose: the migrator empties the VC tables.
- **Server** (`PYTHON/routes/efactura/primite_vercon.py`, `primite_vercon_routes.py`, registered in `__init__.py`):
  - `GET /api/efactura/vercon/conturi`: the tree gestiuni -> accounts for the whole unit database, MF and OI included (D8). The rule "which accounts a gestiune accepts" is VERCON's own (`routes.vercon.receipt_products.warehouse`, imported lazily so the two packages are not tied at start-up).
  - `GET /api/efactura/vercon/stari`: one short line per invoice that was ever sent (for the tree of received invoices).
  - `GET /api/efactura/primite/<id>/vercon`: lines with values WITH VAT (the receptions' rule `primite_ke.valori_cu_tva`), the active sending, the history, the matched partner, possible duplicates (same supplier + number + date already in `VC_NIR`) and `poate_trimite` / `motiv`.
  - `POST` (send), `PUT .../conturi` (change the proposed accounts while it waits), `DELETE .../<idTrimitere>` (take back while it waits). Writes are for the role **Contabil** only, checked on the server. Audit: `EF_VC_TRIMITE`, `EF_VC_CONTURI`, `EF_VC_RETRAGE`.
  - **Effective state (D5)**: a PRELUATA sending whose NIRs are all cancelled or gone counts as TRIMISA again - decided when reading, so it does not depend on every way a NIR can be cancelled.
- **Refusals** (`NETRIMISIBILA`, 409): credit notes (D3), an invoice without lines, an invoice already sent, a supplier not found in `Parteneri` (the NIR needs a partner; the accountant can associate it first). A duplicate only WARNS.

### Files touched
`sql/EV_01_primite_vercon.sql` (new), `PYTHON/routes/efactura/primite_vercon.py` (new), `primite_vercon_routes.py` (new), `__init__.py` (docs + import), `docs/EV_PLAN.md` (written earlier).

### Test results
`py_compile` of the three Python files: ok. `import routes.efactura.primite_vercon`: ok. No test written or run (operator's rule), the SQL not run, nothing called.

### Unverified / deferred
- **SQL not run.** The three tables must exist (run on `AVACONT_SURSA`, then the schema sync) and `KE_01_primite_receptii.sql` must have been run (the VAT rate of a line).
- **Server not deployed** (the operator does it by hand; nothing was sent). New client + old server would fail on the new routes.
- The partner is found by the normalised CUI against `Parteneri.CodFiscal` with the expression of `primite.py` (`_PARTNER_CUI`); not run on real data. `Parteneri` is not filtered by `IdUnitate` here.
- `VC_Gestiuni.Active`, `VC_Subunitati.Active`, `VC_Masini.Activ`, `VC_NIR.Anulat/DataFc/NrFact/IdPartener` are used as the dump of 22.09.2026 shows them (read in the file, not on the live server).
- The VERCON side (list, return with a reason, take over, close the loop with `EF_PrimiteVerconNir`) is NOT in this slice (EV04, EV05); until then a sending stays TRIMISA.
- The case "one of several NIRs of a sending is cancelled" is decided in EV05 (proposal in the plan, section 4).
- No help yet: nothing the operator sees has changed (the window is EV03).

## SLICE-00EF-47 - «Trimite la Vercon»: the K-BOT window and the list (EV03, 21.10.2026) <a id="slice-00EF-47"></a>

> **Superseded in part by [SLICE-00EF-48](#slice-00EF-48) (same day, operator's correction):** the modal window `VerconTrimiteForm`, the column «Vercon», the footer button and the row-menu entries of the list «Toate facturile» (and `PrimiteView.Vercon.vb`) were REMOVED; the sending is a page of its own on the E-Factura tab of the ribbon (`VerconForm`). Domain, Api and the server calls described below are still what the page uses. Read the text below as history.

Plan: [`docs/EV_PLAN.md`](../EV_PLAN.md), slice EV03. Server half: 00EF-45; the combo with foldable groups: 00EF-46.

### What changed and why
The accountant sends received invoices to Vercon from K-BOT, choosing (if she wants) the Vercon account of each line.

- **Domain** (`KBot.Domain/EFacturaVercon.vb`): plain data for the routes of 00EF-45 (`EFacturaVerconStare` constants, the tree `EFacturaVerconGestiune`/`Cont`, the invoice state `EFacturaVerconStareFactura` with `Linii`, `Trimitere`, `Istoric`, the short line `EFacturaVerconSumar`, `EFacturaVerconContLinie` for what is sent).
- **Api** (`IEFacturaApi` + `ApiClient.EFactura.Vercon.vb`): `GetVerconConturiAsync`, `GetVerconStariAsync`, `GetVerconAsync`, `SendVerconAsync`, `SetVerconConturiAsync`, `WithdrawVerconAsync`. Wire names are the server's (case-sensitive).
- **Window `VerconTrimiteForm`** (KBot.EFactura/Forms, modal, designer + code, after `KePropuneriForm`): one or several invoices; `KBotDataView` GROUPED BY INVOICE (header with the sum), columns Nr / Denumire / Cantitate / Valoare cu TVA / **Cont Vercon**. The column is a combo whose items are `KBotComboGroupedItem` (00EF-46): «(fără cont)» first, then the accounts under their gestiune; `GroupHeaderCombo` writes the pick on all lines of the invoice (the solution of the receptions window, KE11). **All lines are sent** (no tick, decision D10). Under the table: where the lines go (per gestiune, plus «Fără cont») with the values; a note box for the Administrator (500 chars); notices for what cannot be sent (credit note, no partner, already sent, wrong role) and a warning for a possible duplicate NIR. «Renunță» left, «Trimite la Vercon (n)» right. On ONE invoice that was sent and still waits the same window changes the proposed accounts («Salvează conturile»). An invoice that fails stays in the grid with its error, the ones sent leave it.
- **List of all the invoices** (`PrimiteListaPage` + designer): new column **«Vercon»** (the state in words), new footer button **«Trimite la Vercon»** on the ticked rows, and in the «…» menu of a row: «Trimite la Vercon…» (never sent / returned / withdrawn), «Schimbă conturile pentru Vercon…» and «Retrage de la Vercon» (waiting), nothing when taken over.
- **`PrimiteView.Vercon.vb`**: reads the states with the list (`GetVerconStariAsync`, a failure or an old server leaves the column empty and is logged), opens the window, withdraws after a Yes/No, reads the states again.
- FileVersion: Api 1.0.28.0, Domain 1.2.19.0, EFactura 1.9.0.0. Help: 0000-116.

### Files touched
`KBot.Domain/EFacturaVercon.vb` (new), `KBot.Api/IEFacturaApi.vb`, `ApiClient.EFactura.Vercon.vb` (new), `KBot.EFactura/Forms/VerconTrimiteForm.vb` + `.Designer.vb` (new), `Views/Primite/PrimiteListaPage.vb` + `.Designer.vb`, `PrimiteView.Vercon.vb` (new), `PrimiteView.vb` (LoadAsync reads the states), `PrimiteView.Lista.vb` (FillList passes them); the three `.vbproj` versions; help `HelpContent/contabil/efactura/index.md`, `help-version.txt`.

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. `Check-Help.ps1 -Coverage`: nothing about this change (older errors remain: `KE12-03`/`KE12-04`, tutorials). No test written or run (operator's rule). Nothing seen on screen, nothing called on a server.

### Unverified / deferred
- **Nothing seen on screen**: the window, the grouped account list inside the grid cell and on the header band of an invoice, the column «Vercon», the button and the row menu. The designers were written by hand; the VS designer was never opened.
- The server must have 00EF-45 deployed and its SQL run, or every call answers an error (the list still loads: the column stays empty).
- **Not in this slice (the plan's EV03 mentioned them):** the state indicator in the TREE of invoices, the filter of the tree menu («netrimise la Vercon»), and a button in the detail view of ONE invoice. The list ( root «Toate facturile» ) is the only place to send from. Say if they are wanted.
- The role is checked by the server; the footer button stays enabled for other roles and the window says why it cannot send.
- The VERCON half (list «Din e-Factura», return, take over, closing the loop) is EV04/EV05, not written: an invoice sent stays «Trimisă, așteaptă Administratorul».
- `KBotComboGroupedItem` as a cell value: after a pick the cell holds the item, the code reads it with `Convert.ToString` (its text); typed text must be a known account label (refused otherwise).

## SLICE-00EF-48 - «Trimite la Vercon» as its own page on the E-Factura tab of the ribbon (EV03 redone, 21.10.2026) <a id="slice-00EF-48"></a>

Plan: [`docs/EV_PLAN.md`](../EV_PLAN.md), slice EV03. Operator's correction to 00EF-47: «un form de sine statator pus in ribbon la efactura» -- the sending is not a button and a modal window of the list «Toate facturile», it is a page of its own, opened from the ribbon. The first attempt put the button next to «Recepții» in the tab «Angajamente»; the operator said it belongs to **E-Factura** (Recepții is in Angajamente, this is not).

### What changed and why
- **Page `VerconForm`** (`KBot.EFactura/Forms`, a `KBotShellForm` like `PrimiteForm` / `KeLegaForm`, hand-written designer, controls all in the designer file). Opens from the ribbon and stays open until closed.
  - **Left:** an `AdvancedTreeControl` (the operator: «TREBUIE sa fie tree, la fel ca peste tot, cu optiune de grupare dupa furnizor») with the received invoices of the year chosen in K-BOT (credit notes and invoices with no value are not listed: the server refuses to send them): month > supplier > invoice, or supplier > invoice (the options button of the header switches, like the macheta «Cumpărare»). A tick on every row; **a tick on a month or a supplier ticks all its invoices** (the control's own propagation), and the grid on the right then shows one group per invoice. The leaf reads «data · număr · trimisă / preluată / returnată» and the tooltip says the full state (NIR numbers, the reason of a return); an invoice taken over in a NIR is ticked and locked (grey). Footer: the total; under the tree «n din N facturi bifate». The first version had a grid with ticks here; the operator rejected it.
  - **Text size (operator, +2 pt):** the labels, the note and the line «Unde merg liniile» are pinned to Calibri 11 (the base is Calibri 9); the tree keeps the font of the other trees.
  - **Right:** the lines of the ticked invoices, grouped by invoice (the grid of the old window), column **Cont Vercon** = combo with the grouped items of 00EF-46 (a gestiune's accounts under its header; an account picked on the header of an invoice goes on all its lines), the note for the Administrator, the line «Unde merg liniile», and the buttons. ALL lines are sent; the account is only a proposal.
  - **Ticking** an invoice reads its state from the server (`GetVerconAsync`) and refills the lines (`SyncTicksAsync`, with a sequence number so a newer tick wins). The accounts picked so far are kept while the grid is refilled (`_choices`).
  - **One ticked invoice that was sent and still waits** opens in the mode that changes its accounts: the button reads «Salvează conturile», «Retrage de la Vercon» appears, the note shows the sending's note read-only. An invoice already taken over or one that cannot be sent is named in a notice and left out of the lines.
  - After a send / change / withdraw the invoices written are unticked, their state read again, the column refreshed. A failed invoice is named in the notice and keeps its accounts (the server writes one invoice all or none).
- **Ribbon:** new button «Trimite la Vercon» (key `efactura_vercon`, image `export`) in the group «E-Factura» of the tab «E-Factura», after «Vânzare» (`AvacontMain.Designer.vb`); `AvacontMain.Ribbon_ItemClicked` routes the key like `efactura_cumparare`; `KbotForm.EFactura.vb` has `MachetaEFacturaVercon`, `_verconForm`, `DeschideEFacturaVercon` (asking again brings the open page forward) and the case in `RunEFacturaCommand`; `KbotForm.Machete.vb` registers the type «E-Factura Trimite la Vercon» (history / «Machete deschise»).
- **Taken out of the list «Toate facturile»** (`PrimiteListaPage` + designer, `PrimiteView.vb`, `PrimiteView.Lista.vb`): the column «Vercon», the button «Trimite la Vercon» with its gap panel, the events, the row-menu entries and the tooltip words. `PrimiteView.Vercon.vb` and `VerconTrimiteForm` (+ designer) deleted. The list is as it was before 00EF-47.
- Unchanged and still used: `KBot.Domain/EFacturaVercon.vb`, `IEFacturaApi` + `ApiClient.EFactura.Vercon.vb` (00EF-47), the combo with foldable groups (00EF-46), the server (00EF-45) and the VERCON half (VC06-07/08). KBot.EFactura `FileVersion` 1.9.0.0 -> 1.10.0.0.
- **Help** 0000-117 (`contabil/efactura/index.md`).

### Files touched
`KBot.EFactura/Forms/VerconForm.vb` + `.Designer.vb` (new), `Forms/VerconTrimiteForm.vb` + `.Designer.vb` (deleted), `Views/Primite/PrimiteView.Vercon.vb` (deleted), `PrimiteListaPage.vb` + `.Designer.vb`, `PrimiteView.vb`, `PrimiteView.Lista.vb` (the 00EF-47 additions removed), `KBot.EFactura.vbproj`; `KBot.App/AvacontMain.vb`, `AvacontMain.Designer.vb`, `KbotForm.EFactura.vb`, `KbotForm.Machete.vb`; help; `docs/EV_PLAN.md`; status.

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. No test written or run (operator's rule), nothing run on screen, no server called.

### Unverified / deferred
- **Nothing seen on screen.** The designer was written by hand and never opened in Visual Studio: the split between the two halves (left fixed at 600 logical px, right fills), the look of the labels in the card panels, the width of the columns, the button `Visible` toggling, the ribbon button's place and icon.
- The header band of an invoice with the combo, the folding of the gestiune groups and the typed search are as in 00EF-46/47: not seen.
- Ticking a supplier or a month reads the states of ALL its invoices one by one (one call each): a big supplier takes a moment (the line under the tree counts the ticked ones). The tree has no «tick all» of its own.
- The tree: expansion of the groups of the ticked invoices after a rebuild, the grey locked ticks, the tooltip, the options menu and the search band over the tree are written as in `KeLegaForm` / `PrimiteView` and not seen.
- A tick on a supplier that has a taken-over invoice: the control's propagation unticks that locked leaf when the supplier is unticked (only the picture; it is never sent).
- The server 00EF-45 must be deployed and its SQL run, or the column stays empty and every call answers an error; the invoices still list.
- The state indicator in the tree of the received invoices, the tree-menu filter and a button in the detail view of one invoice (EV03 in the plan) are still not built; the page is the only place to send from.
- The Administrator's side (VC06-07/08) is unchanged.

## SLICE-00EF-49 - «Trimite la Vercon»: one account for all the lines, one supplier at a time (23.10.2026) <a id="slice-00EF-49"></a>

Operator's request: next to the note box, a combo that shows the gestiuni (as in the grid) and puts the SAME account on every row of the grid, however many invoices are ticked; and NOT several suppliers ticked at once.

### What changed and why
- **Combo `cmbCont`** (KBotComboBox, not editable, placeholder «Cont pentru toate liniile») on the note row of `VerconForm`, right of `txtNota` (a small gap panel `pnlGapNota` between). Same items as the column «Cont Vercon» (`_items`: «(fără cont)», then the accounts under their gestiune), filled in `BuildChoices`. Picking an item writes it on ALL rows of the grid (`CmbCont_SelectedIndexChanged`), then the summary line is redone. Lines added later (another invoice ticked) that have no choice yet take the account held in the combo (`ComboAccount`); a line the operator already chose keeps its choice. The combo is disabled while busy / without the Contabil role.
- To make room, the label `lblNota` reads «Notă» (was «Notă pentru Administrator»; the tooltip of the box still says it is for the Administrator).
- **One supplier at a time:** `KeepOneSupplier` runs on every tick; if ticks of two suppliers exist, the newly ticked supplier stays and the others are unticked (tree's own propagation), and a notice says so (`_supplierNote`, shown by `FillGrid`). The same supplier under several months is fine (the key is the tax code). **Months no longer have a tick** (`HasCheckBox` off): a month would tick several suppliers. In the grouping by supplier nothing else changes.
- Text of the page's intro line updated. KBot.EFactura FileVersion 1.10.0.0 -> 1.11.0.0.
- **Help** 0000-122 (`contabil/efactura/index.md`, tutorial `efactura-trimite-la-vercon.md`).

### Files touched
`KBot.EFactura/Forms/VerconForm.vb`, `VerconForm.Designer.vb`, `KBot.EFactura.vbproj`; help (2 files); status.

### Test results
`dotnet build src\KBot.EFactura\KBot.EFactura.vbproj`: 0 warnings, 0 errors. No test written or run (operator's rule); nothing run on screen.

### Unverified / deferred
- **Nothing seen on screen:** the width of the note box next to the combo at the minimum window size (about 150 px of text), the placeholder, the folding groups in this combo, and a tree month with no tick.
- A tree node `HasCheckBox = False` at the top level of a tree whose children have ticks: the control's propagation to a parent without a box was not read in detail.
