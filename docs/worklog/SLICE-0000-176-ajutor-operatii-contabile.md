# SLICE-0000-176 — help: the window «Operatii contabile» (slices AV02-03 / AV02-04)

**What changed and why.** New topic `contabil/nomenclatoare/operatii.md` (id `contabil.nomenclatoare.operatii`, screens `OperatiiSabloaneForm, OperatiiDiversePage, FacturiPlatiPage`) explains the ribbon button **Date › Operatii**, the page «Operatii diverse» (header, amount copied on the lines, what «Salveaza» posts) and the page «Facturi si plati» (partner and classification, new invoices, unpaid invoices of the current / previous year, payment template, the explanation of the order, the PROVISIONAL order number, «Sterge plata» and when it is refused). Listed in `nomenclatoare/index.md`. Tags `AV02-03, AV02-04, 0000-176`.

**Files touched.** `src/KBot.App/HelpContent/contabil/nomenclatoare/operatii.md` (new), `index.md`.

**Test results.** `tools\HelpCheck\Check-Help.ps1` is not re-run here (it already reported many errors that were there before; the ones on these files are of the kind «slice not in KBOT_STATUS.md»).

**Unverified / deferred.** The text describes code that is compiled but never run and never seen on screen. No capture was made.
