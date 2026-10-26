# SLICE-0000-116…118/122 — Ajutorul și tutorialul trimiterii la VERCON

116 documentează varianta modală, înlocuită de pagina din 117; 118 introduce tutorialul paginii, iar 122 schimbă bifarea la un singur furnizor și adaugă contul pentru toate liniile. Tutorialul din 118 cu bifarea lunilor este istoric, amendat de 122. Limitele despre jumătatea VERCON din 116 descriu numai starea acelui checkpoint, nu infirmă etapele EV04/EV05 ulterioare. Nici captura lipsă, nici tutorialul nerulat nu sunt declarate rezolvate.

- [0000-116](#slice-0000-116)
- [0000-117](#slice-0000-117)
- [0000-118](#slice-0000-118)
- [0000-122](#slice-0000-122)

## SLICE-0000-116 - Help for the sending of received invoices to Vercon (00EF-47, 21.10.2026) <a id="slice-0000-116"></a>

`contabil/efactura/index.md`:
- new section **«Trimiterea la Vercon»** (tag `00EF-45, 00EF-47, 0000-116`): who decides (the «Contabil»), how to send (button «Trimite la Vercon» in the footer of the list / «…» of a row), the window (all lines are sent, the account of Vercon on a line or once on the header of an invoice, «(fără cont)», the account is a proposal, the summary under the table, the note), what is refused (credit notes, a supplier not among the partners), the column «Vercon» and its words, «Schimbă conturile pentru Vercon…», «Retrage de la Vercon», send again after a return/withdrawal.
- the section «Facturile primite»: a bullet on the column «Vercon» and the button; its tag gains `00EF-47, 0000-116`.
- header: `screens:` gains `VerconTrimiteForm`; `keywords:` gain vercon, trimite la vercon, nir, administrator, cont vercon.

No capture added (the window and the column are new; a capture `efactura-vercon` can be taken when the operator has seen the screen). `Check-Help.ps1 -Coverage`: the new window is covered; the errors it prints about `KE12-03` / `KE12-04` / tutorials are older and not from this change; `0000-116` has its row in `state/KBOT_STATUS_0000-0009.md` and this worklog.

**What the help says before it is true:** the words «Preluată în NIR …» and «Returnată: motivul» of the column, and «Dacă un NIR făcut din ea este anulat în Vercon...», describe the VERCON half (slices EV04/EV05 of `docs/EV_PLAN.md`), which is NOT written yet: until then an invoice sent stays «Trimisă, așteaptă Administratorul». Revisit the section when EV04/EV05 land.

## SLICE-0000-117 - Help for the page «Trimite la Vercon» of the ribbon (00EF-48, 21.10.2026) <a id="slice-0000-117"></a>

`src/KBot.App/HelpContent/contabil/efactura/index.md`:
- section «Trimiterea la Vercon»: now starts from the ribbon button (tab «E-Factura»), says what the page shows (invoices on the left with the column «Vercon», lines of the ticked ones on the right), how an invoice that is still waiting is handled (tick it alone: «Salvează conturile» and «Retrage de la Vercon»). Its tag gains `00EF-48, 0000-117`.
- section «Facturile primite» (the list «Toate facturile»): the bullet about the column «Vercon» and the footer button is removed (they are gone from the list); the tag gains `00EF-48, 0000-117`.
- header `screens:`: `VerconForm` instead of `VerconTrimiteForm`.

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. `Check-Help.ps1`: nothing new about this change once the slice rows exist (older errors remain: `KE12-03`/`KE12-04`, tutorials). Nothing seen on screen.

### Unverified / deferred
- The wording describes a page nobody has seen run.
- No capture for the page (the capture list `capture:` is not touched).

## SLICE-0000-118 - Tutorial «Trimite facturile primite la Vercon» (21.10.2026) <a id="slice-0000-118"></a>

Operator request: the sending of received invoices to Vercon needs a tutorial. Follows 00EF-48 (the page) and 0000-117 (its help text).

### What changed and why
- **`HelpContent/tutorials/efactura-trimite-la-vercon.md`** (new; `starts: AvacontMain`, `host-key: efactura-vercon`): the ribbon tab «E-Factura» (signal `ribbon-tab:e-factura`), the button «Trimite la Vercon» (anchor `ribbon.efactura_vercon`, signal `ribbon:efactura_vercon`), then the page: the tree of invoices, the grouping by supplier (optional), the ticks (a month or a supplier ticks all its invoices), the lines grid, the Vercon account (grouped list, once on an invoice's header = all its lines, only a proposal), the note (optional), «Trimite la Vercon» (`wait: click`, `guard: yes`: the tutorial ends at the button, the operator presses it outside the tutorial), then two plain-message steps: withdrawing / changing the accounts of an invoice that still waits, and what the Administrator does in VERCON («NIR din e-Factura…»).
- **`KBot.EFactura/Forms/VerconForm.Tutorial.vb`** (new): the page implements `IKBotTutorialHost` for the key `efactura-vercon` (nothing to do on begin / end, no anchors), like `GrupeForm`, so the controls are found by name and the checker has no warning for it.

### Test results
`Check-Help.ps1`: no message about the file other than the slice id (fixed by this worklog and the status rows). No tutorial run on screen. No test written or run.

### Unverified / deferred
- **The tutorial was never run**: whether the runner finds the page `VerconForm` (a page of the main window, not a window of its own) by the control names `VerconForm.tree` / `gridLinii` / `txtNota` / `btnTrimite` the way it finds `AsociereForm` (a page too, in `legaturile-receptiilor`), whether `ribbon.efactura_vercon` shows the ring on the new button, and whether the signal `ribbon:efactura_vercon` arrives once the page is open.
- The bubble texts are not seen on screen (length, line breaks).
- The tutorial does not wait for ticks (the tree gives no tutorial signal for a tick): the steps «Gruparea» and «Bifează facturile» go on with «Înainte».

## SLICE-0000-122 - Help: «Trimiterea la Vercon», one account for all lines and one supplier at a time (23.10.2026) <a id="slice-0000-122"></a>

Follows slice 00EF-49 (see `SLICE-00EF-45-trimitere-la-vercon-serverul.md#slice-00EF-49`).

### What changed and why
- `contabil/efactura/index.md`, section «Trimiterea la Vercon»: ticking is for one supplier at a time (a month has no tick; a second supplier takes the first one's ticks off), the list «Cont pentru toate liniile» beside the note writes one account on every row of the grid, the note box is named «Notă».
- Tutorial `tutorials/efactura-trimite-la-vercon.md`: steps «Bifează facturile» and «Contul din Vercon» say the same; the step «Nota pentru Administrator» only carries the new slice tag.

### Files touched
`KBot.App/HelpContent/contabil/efactura/index.md`, `KBot.App/HelpContent/tutorials/efactura-trimite-la-vercon.md`.

### Test results
`tools\HelpCheck\Check-Help.ps1`: the new slice tags resolve; the remaining messages are older ones (other tutorials, KE12 tags).

### Unverified / deferred
- Nothing seen on screen; the tutorial step `target:` names (`VerconForm.gridLinii`, `VerconForm.txtNota`) are unchanged.
