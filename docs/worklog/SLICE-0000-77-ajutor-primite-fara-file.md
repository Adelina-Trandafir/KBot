# SLICE-0000-77 - Help for 00EF-31: received invoices without «Atasamente» / «Observatii», save asks where (15.10.2026)

## What changed and why
`contabil/efactura/index.md` («Facturile primite» and «E-Factura Cumparare»): the lists of views no longer name «Atasamente» /
«Observatii»; one line about Adobe's save button in «Factura PDF» (asks whether and where). Tags `00EF-31, 0000-77`.
`help-version.txt` stays `2026-10-15`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`, `docs/worklog/KBOT_STATUS.md`.

## Test results / unverified
`Check-Help.ps1 -Coverage`: no new error for `efactura/index.md`. Text describes code that was not seen working.
