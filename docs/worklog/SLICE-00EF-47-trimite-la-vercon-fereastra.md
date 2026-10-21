# SLICE-00EF-47 - «Trimite la Vercon»: the K-BOT window and the list (EV03, 21.10.2026)

> **Superseded in part by [SLICE-00EF-48](SLICE-00EF-48-pagina-trimite-la-vercon-in-ribbon.md) (same day, operator's correction):** the modal window `VerconTrimiteForm`, the column «Vercon», the footer button and the row-menu entries of the list «Toate facturile» (and `PrimiteView.Vercon.vb`) were REMOVED; the sending is a page of its own on the E-Factura tab of the ribbon (`VerconForm`). Domain, Api and the server calls described below are still what the page uses. Read the text below as history.

Plan: [`docs/EV_PLAN.md`](../EV_PLAN.md), slice EV03. Server half: 00EF-45; the combo with foldable groups: 00EF-46.

## What changed and why
The accountant sends received invoices to Vercon from K-BOT, choosing (if she wants) the Vercon account of each line.

- **Domain** (`KBot.Domain/EFacturaVercon.vb`): plain data for the routes of 00EF-45 (`EFacturaVerconStare` constants, the tree `EFacturaVerconGestiune`/`Cont`, the invoice state `EFacturaVerconStareFactura` with `Linii`, `Trimitere`, `Istoric`, the short line `EFacturaVerconSumar`, `EFacturaVerconContLinie` for what is sent).
- **Api** (`IEFacturaApi` + `ApiClient.EFactura.Vercon.vb`): `GetVerconConturiAsync`, `GetVerconStariAsync`, `GetVerconAsync`, `SendVerconAsync`, `SetVerconConturiAsync`, `WithdrawVerconAsync`. Wire names are the server's (case-sensitive).
- **Window `VerconTrimiteForm`** (KBot.EFactura/Forms, modal, designer + code, after `KePropuneriForm`): one or several invoices; `KBotDataView` GROUPED BY INVOICE (header with the sum), columns Nr / Denumire / Cantitate / Valoare cu TVA / **Cont Vercon**. The column is a combo whose items are `KBotComboGroupedItem` (00EF-46): «(fără cont)» first, then the accounts under their gestiune; `GroupHeaderCombo` writes the pick on all lines of the invoice (the solution of the receptions window, KE11). **All lines are sent** (no tick, decision D10). Under the table: where the lines go (per gestiune, plus «Fără cont») with the values; a note box for the Administrator (500 chars); notices for what cannot be sent (credit note, no partner, already sent, wrong role) and a warning for a possible duplicate NIR. «Renunță» left, «Trimite la Vercon (n)» right. On ONE invoice that was sent and still waits the same window changes the proposed accounts («Salvează conturile»). An invoice that fails stays in the grid with its error, the ones sent leave it.
- **List of all the invoices** (`PrimiteListaPage` + designer): new column **«Vercon»** (the state in words), new footer button **«Trimite la Vercon»** on the ticked rows, and in the «…» menu of a row: «Trimite la Vercon…» (never sent / returned / withdrawn), «Schimbă conturile pentru Vercon…» and «Retrage de la Vercon» (waiting), nothing when taken over.
- **`PrimiteView.Vercon.vb`**: reads the states with the list (`GetVerconStariAsync`, a failure or an old server leaves the column empty and is logged), opens the window, withdraws after a Yes/No, reads the states again.
- FileVersion: Api 1.0.28.0, Domain 1.2.19.0, EFactura 1.9.0.0. Help: 0000-116.

## Files touched
`KBot.Domain/EFacturaVercon.vb` (new), `KBot.Api/IEFacturaApi.vb`, `ApiClient.EFactura.Vercon.vb` (new), `KBot.EFactura/Forms/VerconTrimiteForm.vb` + `.Designer.vb` (new), `Views/Primite/PrimiteListaPage.vb` + `.Designer.vb`, `PrimiteView.Vercon.vb` (new), `PrimiteView.vb` (LoadAsync reads the states), `PrimiteView.Lista.vb` (FillList passes them); the three `.vbproj` versions; help `HelpContent/contabil/efactura/index.md`, `help-version.txt`.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. `Check-Help.ps1 -Coverage`: nothing about this change (older errors remain: `KE12-03`/`KE12-04`, tutorials). No test written or run (operator's rule). Nothing seen on screen, nothing called on a server.

## Unverified / deferred
- **Nothing seen on screen**: the window, the grouped account list inside the grid cell and on the header band of an invoice, the column «Vercon», the button and the row menu. The designers were written by hand; the VS designer was never opened.
- The server must have 00EF-45 deployed and its SQL run, or every call answers an error (the list still loads: the column stays empty).
- **Not in this slice (the plan's EV03 mentioned them):** the state indicator in the TREE of invoices, the filter of the tree menu («netrimise la Vercon»), and a button in the detail view of ONE invoice. The list ( root «Toate facturile» ) is the only place to send from. Say if they are wanted.
- The role is checked by the server; the footer button stays enabled for other roles and the window says why it cannot send.
- The VERCON half (list «Din e-Factura», return, take over, closing the loop) is EV04/EV05, not written: an invoice sent stays «Trimisă, așteaptă Administratorul».
- `KBotComboGroupedItem` as a cell value: after a pick the cell holds the item, the code reads it with `Convert.ToString` (its text); typed text must be a known account label (refused otherwise).
