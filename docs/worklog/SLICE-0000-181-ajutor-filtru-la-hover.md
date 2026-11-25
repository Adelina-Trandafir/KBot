# SLICE-0000-181 - help: the filter funnel appears on hover in «Modificare operatii»

Feature slice DGV0-02 (24.11.2026): the funnel in the column headers of the list of operations is drawn only under the mouse, and stays on columns that have a filter set.

## What changed and why
- `HelpContent/contabil/nomenclatoare/modificare-operatii.md`, section «Cautare si ordonare» (and its opening paragraph about the filter button): says the funnel shows when the mouse is over the header, and stays on filtered columns. Tag: `AV02-05, 0000-177, DGV0-02, 0000-181`.
- `help-version.txt` 2026-11-10 -> 2026-11-24; watermark in `KBOT_STATUS_0000-0009.md` moved to 0000-181.

## Files touched
`src/KBot.App/HelpContent/contabil/nomenclatoare/modificare-operatii.md`, `src/KBot.App/HelpContent/help-version.txt`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`.

## Test results
`Check-Help.ps1` and the App build: see the DGV0-02 worklog / the closing message of the task.

## Unverified / deferred
- Capture re-shoot: no capture tag exists in this topic, so none is listed. Not seen on screen.

## Second pass (24.11.2026)
`modificare-operatii.md`: the section «Informatiile operatiei» removed (the info panel and its ribbon button are gone), the filter paragraph now says CH, CF, Ord and R have no filter button; `nomenclatoare/index.md`: the line of the page no longer mentions the info. Check-Help: no message about these topics; App build 0/0. Details: `SLICE-DGV0-02-pictograma-filtrare-la-hover.md` (second pass).
