# SLICE-0124-06 — The list of documents to sign for every role, and the narrowed window

**Date:** 30.10.2026. CRITICAL rule of the operator: [0124-03](SLICE-0124-03-server-flux-si-restrictii.md).

## What changed and why

- **Director / Contabil / Imputernicit** (the full K-BOT): the «Documente de semnat» window (`DirectorForm`, kept by name) reads the flow's list for every role; a director also reads the older list
  (A and B signed) and the two are joined (`DdfDeSemnatLista.Uneste`). `DirectorForm.AreListaRol` / `AreListaDeSemnat` say who has it; the ribbon button (hidden in the designer) is shown to the accountant
  and the proxy too (`AvacontMain.ApplyDirectorRibbon`); the tray's request (`KbotForm.Director.vb`) answers for these roles. The «commands refused» message is the director's only for a director.
- **Secretar / Administrator**: `Program` replaces the main window with new `DeSemnatRestransForm` (controls in Designer), listing documents addressed to that role in the signed-in unit. Selection **generates locally, A only** (`DdfPdfGenerator`, `DdfPdfMode.OnlyA`, filtered DDF read), shows Adobe and accepts only `A` (`AllowedNewRoles`). After upload: releases Adobe, deletes local files, rereads list. No pending-upload retry or main-window hooks start.
- **Tray (`KBot.Tray/TrayChecker.vb`)**: every role reads the flow list without moving session to a director unit; older director list is added when already on one. `NoList` now means no user unit.

## Files touched

`src/KBot.App/Director/DirectorForm.vb`, `DeSemnatRestransForm.vb` + `.Designer.vb` (new), `KbotForm.Director.vb`, `AvacontMain.vb`, `AvacontMain.Designer.vb` (tooltip), `Program.vb`; `src/KBot.Tray/TrayChecker.vb`; `src/KBot.Api/IDdfSendApi.vb`.

## Test results

`dotnet build` of `KBot.App` and `KBot.Tray` (scratch output folders): 0 errors. Nothing run on screen; neither Designer was opened in Visual Studio.

## Left unverified / deferred

- The restricted window was never opened: the Adobe hosting in a form that is the application's main window, `KBotShellForm` as a main window with `ShowInTaskbar` true, the template download and the generation for a restricted role, the signing session's upload (server check of `X-Semnatura = A`), the deletion of the local files.
- A tray click for a restricted role starts K-BOT but does not pre-pick the document (no hub in that window).
- The accountant's list opens the document in the full DDF view; the tray's «open this document» uses the same path as the director's.
