# SLICE-0000-116 - Help for the sending of received invoices to Vercon (00EF-47, 21.10.2026)

`contabil/efactura/index.md`:
- new section **«Trimiterea la Vercon»** (tag `00EF-45, 00EF-47, 0000-116`): who decides (the «Contabil»), how to send (button «Trimite la Vercon» in the footer of the list / «…» of a row), the window (all lines are sent, the account of Vercon on a line or once on the header of an invoice, «(fără cont)», the account is a proposal, the summary under the table, the note), what is refused (credit notes, a supplier not among the partners), the column «Vercon» and its words, «Schimbă conturile pentru Vercon…», «Retrage de la Vercon», send again after a return/withdrawal.
- the section «Facturile primite»: a bullet on the column «Vercon» and the button; its tag gains `00EF-47, 0000-116`.
- header: `screens:` gains `VerconTrimiteForm`; `keywords:` gain vercon, trimite la vercon, nir, administrator, cont vercon.

No capture added (the window and the column are new; a capture `efactura-vercon` can be taken when the operator has seen the screen). `Check-Help.ps1 -Coverage`: the new window is covered; the errors it prints about `KE12-03` / `KE12-04` / tutorials are older and not from this change; `0000-116` has its row in `state/KBOT_STATUS_0000-0009.md` and this worklog.

**What the help says before it is true:** the words «Preluată în NIR …» and «Returnată: motivul» of the column, and «Dacă un NIR făcut din ea este anulat în Vercon...», describe the VERCON half (slices EV04/EV05 of `docs/EV_PLAN.md`), which is NOT written yet: until then an invoice sent stays «Trimisă, așteaptă Administratorul». Revisit the section when EV04/EV05 land.
