# SLICE-00EF-48 - «Trimite la Vercon» as its own page on the E-Factura tab of the ribbon (EV03 redone, 21.10.2026)

Plan: [`docs/EV_PLAN.md`](../EV_PLAN.md), slice EV03. Operator's correction to 00EF-47: «un form de sine statator pus in ribbon la efactura» -- the sending is not a button and a modal window of the list «Toate facturile», it is a page of its own, opened from the ribbon. The first attempt put the button next to «Recepții» in the tab «Angajamente»; the operator said it belongs to **E-Factura** (Recepții is in Angajamente, this is not).

## What changed and why
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

## Files touched
`KBot.EFactura/Forms/VerconForm.vb` + `.Designer.vb` (new), `Forms/VerconTrimiteForm.vb` + `.Designer.vb` (deleted), `Views/Primite/PrimiteView.Vercon.vb` (deleted), `PrimiteListaPage.vb` + `.Designer.vb`, `PrimiteView.vb`, `PrimiteView.Lista.vb` (the 00EF-47 additions removed), `KBot.EFactura.vbproj`; `KBot.App/AvacontMain.vb`, `AvacontMain.Designer.vb`, `KbotForm.EFactura.vb`, `KbotForm.Machete.vb`; help; `docs/EV_PLAN.md`; status.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. No test written or run (operator's rule), nothing run on screen, no server called.

## Unverified / deferred
- **Nothing seen on screen.** The designer was written by hand and never opened in Visual Studio: the split between the two halves (left fixed at 600 logical px, right fills), the look of the labels in the card panels, the width of the columns, the button `Visible` toggling, the ribbon button's place and icon.
- The header band of an invoice with the combo, the folding of the gestiune groups and the typed search are as in 00EF-46/47: not seen.
- Ticking a supplier or a month reads the states of ALL its invoices one by one (one call each): a big supplier takes a moment (the line under the tree counts the ticked ones). The tree has no «tick all» of its own.
- The tree: expansion of the groups of the ticked invoices after a rebuild, the grey locked ticks, the tooltip, the options menu and the search band over the tree are written as in `KeLegaForm` / `PrimiteView` and not seen.
- A tick on a supplier that has a taken-over invoice: the control's propagation unticks that locked leaf when the supplier is unticked (only the picture; it is never sent).
- The server 00EF-45 must be deployed and its SQL run, or the column stays empty and every call answers an error; the invoices still list.
- The state indicator in the tree of the received invoices, the tree-menu filter and a button in the detail view of one invoice (EV03 in the plan) are still not built; the page is the only place to send from.
- The Administrator's side (VC06-07/08) is unchanged.
