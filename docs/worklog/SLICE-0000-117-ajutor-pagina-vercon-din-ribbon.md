# SLICE-0000-117 - Help for the page «Trimite la Vercon» of the ribbon (00EF-48, 21.10.2026)

`src/KBot.App/HelpContent/contabil/efactura/index.md`:
- section «Trimiterea la Vercon»: now starts from the ribbon button (tab «E-Factura»), says what the page shows (invoices on the left with the column «Vercon», lines of the ticked ones on the right), how an invoice that is still waiting is handled (tick it alone: «Salvează conturile» and «Retrage de la Vercon»). Its tag gains `00EF-48, 0000-117`.
- section «Facturile primite» (the list «Toate facturile»): the bullet about the column «Vercon» and the footer button is removed (they are gone from the list); the tag gains `00EF-48, 0000-117`.
- header `screens:`: `VerconForm` instead of `VerconTrimiteForm`.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. `Check-Help.ps1`: nothing new about this change once the slice rows exist (older errors remain: `KE12-03`/`KE12-04`, tutorials). Nothing seen on screen.

## Unverified / deferred
- The wording describes a page nobody has seen run.
- No capture for the page (the capture list `capture:` is not touched).
