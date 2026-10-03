# SLICE-0100-04 - robot click covered by FOREXE's fixed bar; Reverse workflow timeouts (operator request, 03.10.2026)

Trigger: the operator's log of 03.10.2026 16:14, «Actualizare multiplă»: 1 of 3 angajamente failed with
`Timeout 5000ms exceeded ... <footer id="footer" class="navbar navbar-fixed-bottom"> intercepts pointer events`
while clicking `table.table-striped... tbody tr:nth-child(5) .glyphicon-eye-open` (row 5 of the indicators).

## What changed and why

- `src/KBot.Forexe/Workflows/adlop - Prelucrare Completa Reverse.wfl` (the workflow «Actualizare multiplă» and the reverse
  download run): every `timeout="5"` is now `timeout="15"`, the value the normal `adlop - Prelucrare Completa.wfl` already
  used for the same clicks. Two stay at 5 on purpose: the `IfExists` probes for the optional «Înapoi» button and the
  «Afișează informații complete» button (the normal workflow keeps them at 5 too; raising them would only slow the
  «button is not there» case by 10 s each). The two `timeout="1"` probes are untouched.
- `src/KBot.Forexe/Executor/Actions/WorkflowExecutor.Actions.Click.vb`: when the standard click fails with Playwright's
  «intercepts pointer events» (checked by `IsCoveredClick`) and the step is not `Force`, the element is scrolled to the
  MIDDLE of the window (`scrollIntoView({block:'center'})`) and clicked once more, with a console warning. The second
  failure, if any, is the one reported (same exception path as before). Applies to every workflow, not only Reverse.
- FileVersion: `KBot.Forexe` 1.0.19 -> 1.0.20.

## Files touched

`src/KBot.Forexe/Workflows/adlop - Prelucrare Completa Reverse.wfl`,
`src/KBot.Forexe/Executor/Actions/WorkflowExecutor.Actions.Click.vb`, `src/KBot.Forexe/KBot.Forexe.vbproj`.

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen (operator rule: no
tests / renders unless asked).

## Left unverified / deferred

- That the footer, not slowness, was the cause is a reading of the log: the real page was not seen. Playwright normally
  changes its scroll alignment between its own retries; why it did not within 5 s here is not known. The longer timeout
  and the centred retry are both cheap guards, neither is proved on that page.
- The other workflows with 5 s on the same eye click (`Creare\adlop - Definitivare Angajament.wfl`,
  `Definitivare_Derulare Angajament.wfl`) were NOT changed (outside the request); they get the centred retry.
- No help / NOUTATI change (operator order; nothing the operator sees apart from a console line).
