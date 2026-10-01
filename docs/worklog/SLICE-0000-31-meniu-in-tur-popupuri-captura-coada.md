# SLICE-0000-31 — MENIU opened by the tour, popups kept open during captures, help for the robot queue

Operator request, 01.10.2026 (follow-up to 0000-30).

## What changed and why

1. **MENIU in the tour.** New step key `reveal: menu` (only on `KbotForm.btnMeniu`; `HelpTour.Reveal`,
   `Check-Help.ps1`). `KbotForm.HelpReveal` (`KbotForm.HelpReveal.vb`, `IKBotHelpReveal`) opens `menuNou`,
   forces the rows «Jurnal activitate» and «(!) Operatiuni necorelate» visible (`MenuNou_Opening`), and the
   runner rings button + menu (`HelpReveal` now returns an area and an optional note;
   `KBotDropDownMenu.OpenBounds`). Ended with the step. Used in `tur-fereastra` and `tur-notecab`.
   **Design choice:** the bubble still takes the focus (so its keys work); the menu stays open because of
   the guard below, not because the bubble is non-activating.
2. **Popups kept open (`KBot.Theming/KBotPopupGuard.vb`).** `Hold/Release` (counted), `KeepOpen`,
   `CloseOnRelease`. Held from the capture bar's showing until the capture is taken or given up
   (`HelpCaptureForm.StartCapture` / `OnPromptFinished`), and by the tour's menu step. Checked where a popup
   closes on click-away / deactivation: `KBotDropDownMenu` (+ its message filter, keys pass through while
   held), `CustomPopup` (tree header-right menu, node right-click), `KBotFilterPopup` (grid column menu),
   tree `ColFilterPopup`, `KBotCalendarPopup`, `KBotComboBox` list. At release the popups that were kept
   close. The capture bar's hint text was updated.
3. **Robot queue help (0098, never covered).** New topic `contabil.forexe.coada` (+ capture tag
   `coada-robot`), new tour `tur-coada` (7 steps, runs only with the queue window open), new step «Coada
   robotului» in `tur-forexe` (the «Coadă N» button is revealed by the 0000-30 mechanism), rows in
   `contabil.fereastra` and `contabil.forexe`. Code: operator message in `RobotQueue.vb` («au fost removed»
   -> «au fost scoase»).
4. Docs: `HelpContent/README.md`, `docs/HELP_SYSTEM.md` (0000-30 text still accurate), `contabil.ajutor`.

## Files touched

Code: `KBot.Theming/KBotPopupGuard.vb` (new), `KBotHelp.vb`; `KBot.Controls`: `Menu/KBotDropDownMenu.vb`,
`.Input.vb`, `Popup/CustomPopup.vb`, `DataView/Filter/KBotFilterPopup.vb`, `Tree/AdvancedTreeControl.ColFilter.vb`,
`.HelpParts.vb`, `Calendar/KBotCalendarPopup.vb`, `Combo/KBotComboBox.DropDown.vb`, `NavList/KBotNavList.HelpParts.vb`;
`KBot.App`: `KbotForm.HelpReveal.vb` (new), `KbotForm.HelpCapture.vb`, `Help/HelpTour.vb`, `HelpTourRunner.vb`,
`HelpCaptureForm.vb`, `HelpCapturePromptForm.vb`, `Forexe/RobotQueue.vb`. Tools: `tools/HelpCheck/Check-Help.ps1`.

## Test results

Compile check (`dotnet build ... -p:OutDir=<scratch>`): 0 warnings, 0 errors. The normal build is blocked:
K-BOT (and Visual Studio) are running and lock the DLLs in `src\KBot.App\bin`; close them and rebuild.
Nothing run on screen.

## Left unverified or deferred

- **Nothing seen on screen:** the menu opening in the tour (ring around button + menu, bubble position), the
  popups staying open and closing at the end of a capture, `tur-coada` on a live queue.
- The tooltip popup of the tree and the «?» help popup are NOT held (tooltips / the help itself).
- A held menu ignores the keyboard (the keys go to the focused window).
- `tur-coada` ends with the queue: the window closes itself when the queue is empty.
- Capture to shoot: `coada-robot` (new).
