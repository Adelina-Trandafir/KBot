# SLICE-0000-118 - Tutorial «Trimite facturile primite la Vercon» (21.10.2026)

Operator request: the sending of received invoices to Vercon needs a tutorial. Follows 00EF-48 (the page) and 0000-117 (its help text).

## What changed and why
- **`HelpContent/tutorials/efactura-trimite-la-vercon.md`** (new; `starts: AvacontMain`, `host-key: efactura-vercon`): the ribbon tab «E-Factura» (signal `ribbon-tab:e-factura`), the button «Trimite la Vercon» (anchor `ribbon.efactura_vercon`, signal `ribbon:efactura_vercon`), then the page: the tree of invoices, the grouping by supplier (optional), the ticks (a month or a supplier ticks all its invoices), the lines grid, the Vercon account (grouped list, once on an invoice's header = all its lines, only a proposal), the note (optional), «Trimite la Vercon» (`wait: click`, `guard: yes`: the tutorial ends at the button, the operator presses it outside the tutorial), then two plain-message steps: withdrawing / changing the accounts of an invoice that still waits, and what the Administrator does in VERCON («NIR din e-Factura…»).
- **`KBot.EFactura/Forms/VerconForm.Tutorial.vb`** (new): the page implements `IKBotTutorialHost` for the key `efactura-vercon` (nothing to do on begin / end, no anchors), like `GrupeForm`, so the controls are found by name and the checker has no warning for it.

## Test results
`Check-Help.ps1`: no message about the file other than the slice id (fixed by this worklog and the status rows). No tutorial run on screen. No test written or run.

## Unverified / deferred
- **The tutorial was never run**: whether the runner finds the page `VerconForm` (a page of the main window, not a window of its own) by the control names `VerconForm.tree` / `gridLinii` / `txtNota` / `btnTrimite` the way it finds `AsociereForm` (a page too, in `legaturile-receptiilor`), whether `ribbon.efactura_vercon` shows the ring on the new button, and whether the signal `ribbon:efactura_vercon` arrives once the page is open.
- The bubble texts are not seen on screen (length, line breaks).
- The tutorial does not wait for ticks (the tree gives no tutorial signal for a tick): the steps «Gruparea» and «Bifează facturile» go on with «Înainte».
