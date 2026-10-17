# SLICE-0114-13 - KbotMain: no «Grupe» row when there is no custom group (operator item 6.3) (17.10.2026)

## What changed and why
`ShowTreeOptionsMenu` is async: it asks the server for the groups first (`ExistaGrupeAsync`; no unit, no API or a failed read = no row, logged) and adds «Grupe» only if at least one group exists (or one is active, so «Toate angajamentele» stays reachable). Groups are made with the ribbon button (0114-11).

## Files touched
`src/KBot.App/KbotForm.TreeOptions.vb`, `KbotForm.Grupe.vb`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: layout of the two ribbon groups side by side, the footer popup position, the nav order.
- The groups are read from the server every time the options menu opens (one extra call); the menu appears after it answers.
