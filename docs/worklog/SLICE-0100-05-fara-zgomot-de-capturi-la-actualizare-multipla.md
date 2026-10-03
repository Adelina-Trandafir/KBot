# SLICE-0100-05 - no «nicio captură» noise after «Actualizare multiplă» (operator request, 03.10.2026)

Trigger: the operator's log of 03.10.2026 16:14 showed two warnings per angajament, e.g.
`[MainForm.TrimiteCapturileAsync] «AAB2MA25TCB»: nicio captură de rezervare de trimis — nu există niciuna în «C:\KBOT\Capturi\AAB2MA25TCB». Pe disc: «C:\KBOT\Capturi» e gol.`
(and the same for «receptie»), for every angajament that downloaded fine. Operator: «i want the noise gone».

## What changed and why

- The pictures of the FOREXE page are taken only by the operator flows (reception saved, DDF sent): the parallel robot
  (`ForexeController.Parallel.vb`) has no capture code. So after each successful parallel download
  `ActualizeazaMaiMulteCuCoadaAsync` asked `TrimiteCapturileAsync` to send two kinds of pictures that were never taken,
  and it warned about it.
- `src/KBot.App/KbotForm.Parallel.vb`: the two direct `TrimiteCapturileAsync` calls became a loop that calls it only when
  `_controller.CapturileDe(cod, fel).Count > 0`. Pictures LEFT ON DISK by an earlier session (kept with «Nu») are still
  sent exactly as before; an empty-handed run says nothing. `TrimiteCapturileAsync` itself is unchanged: the other callers
  (reception / DDF flows) keep their warning, where a missing picture is news.

## Files touched

`src/KBot.App/KbotForm.Parallel.vb`.

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run (operator rule).

## Left unverified / deferred

- `CapturileDe` also stamps the marker number on waiting pictures as a side effect (idempotent; `TrimiteCapturileAsync`
  calls it again). Read from code, not run.
- `KBot.App` FileVersion not bumped here: the project was already bumped in the same uncommitted work (1.1.1.5 -> 1.1.1.7).
- No help / NOUTATI change (operator order).
