# SLICE-AV01-07 - «Plan de conturi»: message box, no exit button, build fix, CLOSED (operator, 25.10.2026)

## What changed and why
Last corrective pass on the «Plan de conturi» window; the operator then looked at it and said it «pare corect» -> **the window is closed** (no more work planned on it, see the end).

1. **Build fix.** `KbotForm.PlanCont.resx` (an empty Visual Studio template, untracked, next to `KbotForm.PlanCont.vb`) collided with `KbotForm.resx`
   (MSB3577, two outputs for `KBot.App.KbotForm.resources`). Deleted. A partial of `KbotForm` must never get its own `.resx`.
2. **Exit button gone.** `btnIesire` was removed from the designer by the operator (the window closes from its title bar / the shell); the leftover `BtnIesire_Click`
   and the `ApplySecondary(btnIesire, ...)` line in `OnThemeChanged` are deleted.
3. **Messages use the same system as `OrdEditForm`.** The bottom status strip (`tlySubsol` + `lblStare`) is replaced by one `KBotNotice` (`ntfMesaj`) on row 1 of `tlyMain`,
   ABOVE the card (the `tlyMain` row is `AutoSize`: hidden = no height). `SetStatus(text)` -> `ntfMesaj.Show` (Warning) / `Clear` when the text is empty;
   `SetStatus(text, kind)` for failures (`NoticeKind.Error`).
4. **Warnings and errors only (operator).** No help / progress / success text any more: removed «Cont nou: alegeti...», «Modificati contul ...», «N conturi. Tastati direct...»,
   «Se incarca / sterge / salveaza...», «Excelul a fost salvat...» and the account count «N conturi in ...» (nothing is shown at open). What stays: the
   missing-subunit warnings (`SetChartStatus`), «Introduceti o suma...» (grid validation), «Modificarile nesalvate au ramas...», and every caught API / load error (red).
   The busy state still shows the wait cursor.

## Files touched
`src/KBot.App/Avacont/Configurari/PlanCont/Views/{PlanContForm,PlanContForm.Designer,PlanContConturiView,PlanContSolduriView}.vb`;
`src/KBot.App/Avacont/Configurari/PlanCont/KbotForm.PlanCont.resx` (deleted); help `contabil/nomenclatoare/plan-conturi.md` (tag `AV01-07`, see 0000-125).

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run by me, nothing tested; the operator saw the window and approved it.

## Unverified / deferred
- `NoticeKind.Success` is no longer used by this window (kept by the control).
- The unused handler wiring of `SubunitateEditForm` (its own `lblStare`) is untouched: the form is not reachable (AV01-06).
- DDL `AV01_04` / `AV01_04b` and the server routes: whether the operator ran them is not recorded here; the window working on screen implies the server side answers, but this was not checked.

## Closing the «Plan de conturi» (AV01-04 + AV01-06 + AV01-07)
Closed by the operator on 25.10.2026 («planul de conturi pare a fi corect»). What remains open, NOT part of this window:
the interface for subunits (`docs/access-map/avacont/plan-interfata-subunitati.md`, not critical), a way to mark an account inactive (operator will decide), the AV role rights
for the accounts routes, `ParteneriSI` not migrated, the 4 `DefaClsfV` rows without `Tip`.
