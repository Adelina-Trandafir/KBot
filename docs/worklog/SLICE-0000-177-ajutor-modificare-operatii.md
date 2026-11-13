# SLICE-0000-177 - Ajutor: «Modificare operații» și fila «Introducere date» (felia AV02-05)

## What changed and why

Slice AV02-05 added the page «Modificare operații» and a new ribbon tab «Introducere date» (the «Operații» button moved there from
Configurări › Date). Any visual change goes into the help with its slice number (operator rule 30.09.2026).

- New topic `src/KBot.App/HelpContent/contabil/nomenclatoare/modificare-operatii.md` (`contabil.nomenclatoare.modificare-operatii`, order 47,
  `screens: ModificareOperatiiForm`): the columns and the icons of «Tip», «Ord», «R»; filters on several columns and the letter keys; the
  remembered sort; the ribbon buttons (Reîncarcă, Ordonare implicită, Curăță filtrele, Excel listă / tot anul); the info panel; deleting an
  operation or a document and when it is refused.
- `nomenclatoare/operatii.md`: the button is now in «Introducere date › Introducere Date › Operații».
- `nomenclatoare/index.md`: the new topic in the list; the «Operații» path changed.
- `ribbon.md`: the tab «Introducere date» with its two groups in the table of tabs.
- Tags: `AV02-05, 0000-177`. `help-version.txt` = 2026-11-10; watermark moved.

## Files touched

`src/KBot.App/HelpContent/contabil/nomenclatoare/modificare-operatii.md` (new), `.../nomenclatoare/operatii.md`, `.../nomenclatoare/index.md`,
`.../contabil/ribbon.md`, `src/KBot.App/HelpContent/help-version.txt`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`.

## Test results

`tools\HelpCheck\Check-Help.ps1 -Coverage`: `ModificareOperatiiForm` is covered; no new error on these files (the checker's long list of
older errors is unchanged).

## Left unverified or deferred

- Text only: the page was never seen on screen; no capture tag added (none can be taken before the server files are published).
- No guided tour for the page.
- Other help topics that may still say «Date › Operații» were searched (`HelpContent`): only `operatii.md` and `nomenclatoare/index.md` did.
