# SLICE DIR1-01 - the director's module: the main window narrowed to DDF / ORD / CAB notes (30.10.2026)

## What changed and why
Operator, 30.10.2026: the missing «director module» shows ONLY FOREXE (K-BOT), limited to DDF, ORD and CAB correction notes «momentan» (more may follow). DIR1 covers the director.

A director gets the SAME main window as everybody else (before: `DirectorForm`, slice 0081-06, only the list), narrowed in four places. The rules live in one class, `DirectorMode`, so a fourth document type is one line there. `DirectorForm` is reached from a ribbon button (DIR1-02).

- **`Director/DirectorMode.vb` (new)** - `ViewKeys` (`ddf`, `ord`, `notecab`), `RibbonKeys`, `RibbonKeyToSign`, `SigningRoles` (DIR1-03), `IsActive(session)` (reads the role live), `ReadOnlyMessage`.
- **`Program.vb`** - always `AvacontMain`; the `DirectorForm` branch is gone.
- **Shell (`KbotForm.Views.vb`, `KbotForm.vb`)** - hides Sumar, Istoric, Rezervari, Receptii, Plati, Extrase, E-Factura and Browser FOREXE. Keeps the open view if available on the selected node, else its first available view, else `PlaceholderView` (key `gol`, fixed «Alegeti din lista un angajament...»). `NavViews_SelectionChanged` handles cleared selection instead of failing in `ActivateView(Nothing)`. FOREXE footer hidden; no start tutorial.
- **Write commands** - `ExecutaComandaDdf`, `ExecutaComandaOrd` and `UploadCabNoteAsync` answer `DirectorMode.ReadOnlyMessage` and stop. Signing is not one of these commands (it runs inside `DdfView`), so it still works.
- **No FOREXE** - both `ForexeController.ConnectAsync` overloads refuse for a director (`RaporteazaEsec`), which covers every robot entry (tree download icons, send, upload).
- **Ribbon (`AvacontMain.vb`)** - `ApplyDirectorRibbon` hides every button and field except `RibbonKeys` and every tab left with none; it only hides, so it runs after each refresh (`SyncRibbon`, `ApplyAppTabs`, `UpdateFormGroup`, `UpdateViewGroup`). The window opens straight on the shell instead of the opening page.
- **Unit selector (`KbotForm.Units.vb`)** - lists only `Director` units for a director; selecting the other kind gives a message in either direction, preventing accountant/director shell changes mid-session.
- **`PlaceholderView`** - optional fixed message; the busy bar does not run for it.

## Files touched
NEW `src/KBot.App/Director/DirectorMode.vb`. `src/KBot.App/{Program.vb,AvacontMain.vb,KbotForm.vb,KbotForm.Views.vb,KbotForm.Units.vb,KbotForm.Ddf.vb,KbotForm.Ord.vb,KbotForm.CabNotes.vb}`, `src/KBot.App/Forexe/ForexeController.vb`, `src/KBot.App/Views/PlaceholderView.vb`. Help: `SLICE-0000-141-ajutor-modul-director.md`. No server change.

## Test results
Build 0 errors, 0 warnings. A director login was used by the operator on 30.10.2026 (narrowed window, list, signing): working. The unit selector with two director units was not tried.

## Unverified / deferred
- CLIENT-side restriction only: the server does not refuse a director token on write routes (the signing needs some of them), so a hand-made request could still write. A server-side role check per route is separate work.
- The tree's download icons and the Setari pages (including FOREXE and Documente) are still visible to a director; the download icons answer with the refusal message.
- A change of the session's role in the middle of the run (a re-login as another role) does not rebuild the ribbon or the navigation; `ModDirector` is read live, the narrowing is applied on the next refresh.
