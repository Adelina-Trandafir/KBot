# SLICE-0078-13 - «Replace existing file?» on the older Acrobat; F8 in the hosted window (operator request, 06.10.2026)

Scope: hosted-window engine only (old AND new Adobe share one code path). NOTHING on the ActiveX engines.

## What changed and why

- **Replace-existing-file confirm.** `adobe_preview.log` 05.10 20:57:44 (Acrobat 19.12): the «Save As» box with Yes/No and the text
  «…The file already exists. Replace existing file?» was classified `Other` and left alone, because `AdobeSaveDialogFilter.Classify`
  accepted a confirm only when its owner was the Save As we pressed. Save As then timed out and reappeared 44+ times.
  Now, while a Save As of ours is pending, a Yes/No box of the watched process whose text is the replace question
  (`IsReplaceQuestion`: «already exists» / «Replace existing» / Romanian equivalents) is a `ConfirmOverwrite` too; the owner test
  stays. Any other Yes/No question is still left alone. The answer is the existing `HandleConfirm` (WM_COMMAND IDYES).
  The ownership hypothesis of 0078-12 was therefore not needed to fix it, and is still unproven.
- **F8** (no Ctrl) is queued with Read Mode in `AdobeReaderHost.ArmReadMode`: order Ctrl+H, F8, Ctrl+2 (F8 before the zoom-to-width,
  so the width is taken as the right pane goes). `HideRightPaneEnabled` (default True) switches it off; the older-Adobe bench
  ticks it with the Ctrl+H box. Same conditions, retries and log lines as the other keys («F8 trimis documentului», manual hint on failure).
  ASSUMPTION: F8 toggles the right-hand toolbar in this Acrobat (operator's observation); on a document where that pane is already hidden it would show it.
- Controls 1.63 -> **1.64** (App not touched). Help: the toolbar the operator sees changes -> **help topic not yet updated** (see below).

## Files touched
`AdobeSaveDialogFilter.vb`, `AdobeReaderHost.vb`, `OldAdobeHostHarnessForm(.Designer).vb`, `KBot.Controls.vbproj`,
`tests/KBot.Controls.Tests/AdobeSaveDialogFilterTests.vb` (one new test).

## Test results
Builds of `KBot.App` and `KBot.Controls.Tests`: 0 errors, 0 warnings. Test written, not run (operator rule). Nothing run on screen.

## Unverified / deferred
- Nothing seen: that the confirm is now answered on Acrobat 19.12 and on the new Reader, and that F8 hides the right pane in both.
- If F8 acts differently after Ctrl+H (Read Mode), the order or a per-version choice may be needed.
- Help (0000-NN) for the hidden right pane: not done.
