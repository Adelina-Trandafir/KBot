# SLICE-0000-76 - Help for 0114-01 / 00EF-30: sync from the ribbon (15.10.2026)

## What changed and why
- `contabil/efactura/index.md`: the intro (order «Cumparare» / «Vanzare»; the sync button only with the «Cumparare» page), the section
  «Sincronizarea facturilor primite» (no window any more; the list of days, the box for days 1-60, the bottom line) and the section of the
  «Facturi primite» page (no button «Sincronizeaza cu ANAF»; the bar of views; no tooltips on the tables).
- `contabil/ribbon.md`: the E-Factura row (order, the fields, the conditional button).
- Tags `0114-01, 00EF-30, 0000-76`; `help-version.txt` stays `2026-10-15`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, `src/KBot.App/HelpContent/contabil/ribbon.md`,
`docs/worklog/state/KBOT_STATUS_0000-0009.md`, `docs/worklog/KBOT_STATUS.md`.

## Test results
`Check-Help.ps1 -Coverage`: see the worklog of the task; build of KBot.App: 0 warnings, 0 errors. The help was not opened on screen.

## Unverified / deferred
- Captures to redo: `ribbon-efactura` (the tab changed: order, button hidden by default) and `efactura-cumparare` (already marked).
