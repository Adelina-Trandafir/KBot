# SLICE-0072-03 — «Setări ▸ FOREXE» no longer crashes on open (operator report, 01.10.2026)

## What changed and why
`NullReferenceException` in `SetariForexeView.ChkMeniuPagina_CheckedChanged` when the page was created
(`$VB$Me._controller was Nothing`). The designer sets `chkMeniuPagina.Checked = True` / `CheckState` inside
`InitializeComponent()`; that raises `CheckedChanged` while the constructor is still running, before `_controller`
is assigned, and the page's own guard (`_suppress`) was still `False`.

Fix: the constructor holds `_suppress = True` around `InitializeComponent()`. Every other handler on the page
checks `_suppress` before it touches `_controller`, so this covers all of them, not only that check box.
The defect was already in the code (not part of slice 0100); it surfaced while debugging the fork.

## Files
`src/KBot.App/Setari/SetariForexeView.vb`.

## Test results
`dotnet build src\KBot.App`: no compile errors. The DLL copy step failed only because the app was running under
the debugger. Nothing run on screen.

## Unverified
The page was not opened after the fix.
