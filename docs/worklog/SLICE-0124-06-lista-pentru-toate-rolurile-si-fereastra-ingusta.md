# SLICE-0124-06 — The list of documents to sign for every role, and the narrowed window

**Date:** 30.10.2026. CRITICAL rule of the operator: [0124-03](SLICE-0124-03-server-flux-si-restrictii.md).

## What changed and why

- **Director / Contabil / Imputernicit** (the full K-BOT): the «Documente de semnat» window (`DirectorForm`, kept by name) reads the flow's list for every role; a director also reads the older list
  (A and B signed) and the two are joined (`DdfDeSemnatLista.Uneste`). `DirectorForm.AreListaRol` / `AreListaDeSemnat` say who has it; the ribbon button (hidden in the designer) is shown to the accountant
  and the proxy too (`AvacontMain.ApplyDirectorRibbon`); the tray's request (`KbotForm.Director.vb`) answers for these roles. The «commands refused» message is the director's only for a director.
- **Secretar / Administrator**: `Program` opens a window of their own, `DeSemnatRestransForm` (new, all controls in the Designer), instead of the main window. It lists the documents addressed to the role
  (unit signed in on); for the one picked it **generates the document on this PC with section A only** (`DdfPdfGenerator`, `DdfPdfMode.OnlyA`, through the DDF read that gives a restricted role only its addressed revisions), shows it in Adobe, accepts only the signature `A` (`AllowedNewRoles`), and after the upload releases Adobe, deletes the local files and reads the list again. The «pending uploads» retry and the main-window hooks are not started for it.
- **Tray (`KBot.Tray/TrayChecker.vb`)**: every role reads the flow's list (the session no longer moves to a director unit just to read it; the older director list is added when the session is on a
  director unit). `NoList` now only means the user has no unit.

## Files touched

`src/KBot.App/Director/DirectorForm.vb`, `DeSemnatRestransForm.vb` + `.Designer.vb` (new), `KbotForm.Director.vb`, `AvacontMain.vb`, `AvacontMain.Designer.vb` (tooltip), `Program.vb`; `src/KBot.Tray/TrayChecker.vb`; `src/KBot.Api/IDdfSendApi.vb`.

## Test results

`dotnet build` of `KBot.App` and `KBot.Tray` (scratch output folders): 0 errors. Nothing run on screen; neither Designer was opened in Visual Studio.

## Left unverified / deferred

- The restricted window was never opened: the Adobe hosting in a form that is the application's main window, `KBotShellForm` as a main window with `ShowInTaskbar` true, the template download and the generation for a restricted role, the signing session's upload (server check of `X-Semnatura = A`), the deletion of the local files.
- A tray click for a restricted role starts K-BOT but does not pre-pick the document (no hub in that window).
- The accountant's list opens the document in the full DDF view; the tray's «open this document» uses the same path as the director's.
