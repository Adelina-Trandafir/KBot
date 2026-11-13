# SLICE-0000-178 — Help: the redesigned «Facturi si plati» page (slice AV02-04, 10.11.2026)

## What changed and why
The page «Facturi si plati» of the window «Operatii contabile» was redesigned after the operator's sketch (see `SLICE-AV02-04-facturi-plati.md`, section «Layout of the page»). The help section «Facturi si plati» in `contabil/nomenclatoare/operatii.md` was rewritten to match what the operator now sees: classification and partner on top with the code and the bank account read-only, «Documente evidentiere» / «Documente plata» lists on the left, the three-option bar over the invoice grids with the blank row of new invoices, and the payment grid whose first row is the payment being made (O.P. number, date, note, explanation, value) and whose other rows are the orders of the year. The tag `0000-178` was added next to `AV02-04` in the hidden slice tag of the topic. Later the same day (10.11.2026): the help also says that the note is mandatory on every new invoice (its own note per invoice), that rows are removed with the ✕ column (no «Sterge randul» / «Sterge plata» buttons exist any more) and that «Cod client» shows the partner's client code (several codes of one CodFiscal joined with «;»). Nothing about the inner working was put in the help (users only).

## Files touched
`src/KBot.App/HelpContent/contabil/nomenclatoare/operatii.md`; `docs/worklog/state/KBOT_STATUS_0000-0009.md` (row 0000-178). The watermark `help-version.txt` already holds 2026-11-10.

## Test results
None run (no tests by operator rule). `tools\HelpCheck\Check-Help.ps1` NOT run.

## Unverified / deferred (de citit de operator)
- The page was never seen on screen, so no capture exists or was re-shot for it.
- «Cod client» comes from the new column `Parteneri.CodClient` (DDL not run yet, so for now the field is empty); see `SLICE-AV02-04-facturi-plati.md`. The text about the «;» join describes a migration that has never been run.
