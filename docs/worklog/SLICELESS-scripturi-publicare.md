# SLICELESS - KBotAdmin «Scripturi» (02.11.2026)

Owner's tool only (KBotAdmin, never published). The `push-*.ps1` and `publish-*.ps1` scripts of the repository can be started from a page of KBotAdmin, with their switches as ticks.

## What it does
Page «Scripturi» (ribbon Publicare > Scripturi, key `scripturi`): two `KBotDataView` side by side, 50% each with an 8 px gap.
- **Left, the scripts**, grouped (`EnableGrouping`, hidden column `grup`) under «Push - trimit pe server» and «Publish - construiesc local»: columns «Script», «Ce face» and «Rulează». The button column has `ButtonMargin`, `ButtonPadding` and `CellPadding` all 0, so the button IS the cell.
- **Right, the switches of the chosen script**, one per row: a tick, «Switch» (as typed: `-Mandatory`, `-Audience Access`, `-Sign Yes`, ...) and «Ce face». Only the switches the script has are listed. Under the list a text box for `-Notes`, enabled only for the scripts that have it.
- The ticks and the notes are kept per script while the program runs (changing the chosen script and coming back keeps them). The TICKS are also saved at once in `admin_settings.json` (`ScriptSwitches`: script key -> ticked switches), so they come back at the next start and after a `publish-admin.ps1` (which keeps that file); the `-Notes` text is never kept (it belongs to one release). The pairs that are two values of one parameter clear each other (`ScriptCatalog.Exclusive`: Access / NonAccess, Sign Yes / No, Restart / NoRestart, Release / Debug). A switch left unticked is not passed, so the script asks as it does today (`-Audience`, `-Sign`, `-Bump`, release notes). `-Bump` and `-ToVersion` are not on the page on purpose: the script asks.
- Under the grids: the description and the exact call of the chosen script; under that, a log of what was started and how it ended (exit code of the script).

«Rulează» asks for a confirmation that repeats the call, then starts `powershell.exe -ExecutionPolicy Bypass -Command "& '<script>' <args>; ... Read-Host ..."` in its own VISIBLE window with the repository root as folder, so the scripts' questions, the sftp password prompt and the release-notes wait work as when typed by hand. The window closes by itself when the script ends well; when it fails it stays open until Enter, so the error can be read. Its exit code is that of the script. Nothing starts without the operator pressing the button.

Scripts: push-update, push-last-zip, rollback-update, tools\Push-Changed (PYTHON files); publish-release, publish-debug, publish-tray (four rows: Run, Publish, Start, Stop), publish-vrc-migrator, publish-avc-migrator (AVC.Migrator only, to `_AvcMigrator`, added 02.11.2026), publish-admin.

**publish-admin.ps1 is detached:** it deletes and rewrites `_Admin`, where KBotAdmin itself runs from. The page asks, then starts a PowerShell that first waits for this program's process to end (`Wait-Process -Id`) and runs the script after; the program closes (like the tray's «Închide», after the "an operation is still running" question). When the script ends well, the same window starts the published `_Admin\KBotAdmin.exe` again before it closes; on failure it stays open and nothing is restarted. No exit code is logged for that row (the program is gone).

The repository root is found by walking up from the exe to `KBot.sln` (as `AdminSettings.EffectiveLocalRoot` does); no new setting. `Logs\publicare.log` traces every start and end.

History of the page: the first version had one grid with a column of ticks per switch (empty where a script lacks it); the operator did not like it and asked for this two-grid layout. The control property `HideGlyph` (`KBotCellFormattingEventArgs`, paint-only: a check or option cell draws nothing, a disabled cell that sets it gets no grey wash) was added for that first version and is KEPT in KBot.Controls on the operator request, for future use; nothing uses it yet. Files: `KBotCellFormattingEventArgs.vb`, `KBotDataView.Painting.vb`, `KBotDataView.md`.

## Files touched
- `KBotAdmin/Scripts/ScriptCatalog.vb` (new): the script list, the switches and their words, the arguments, the PowerShell call.
- `KBotAdmin/Views/ScriptsView.vb` + `.Designer.vb` (new).
- `src/KBot.Controls/DataView/Events/KBotCellFormattingEventArgs.vb`, `KBotDataView.Painting.vb`, `KBotDataView.md`: `HideGlyph` (kept for future use, unused now).
- `KBotAdmin/MainForm.vb`, `MainForm.Designer.vb`: button «Scripturi» in a new group of the «Publicare» tab, the page, and the close-the-program hook for publish-admin.
- `KBotAdmin/README.md`.

## Verified / unverified
- Verified: `dotnet build KBotAdmin\KBotAdmin.vbproj` clean (0 warnings, 0 errors).
- **NOT run:** the page was never opened and no script was started from it. Nothing was pushed or published.
- To confirm on the first run: the grouping under Push / Publish shows in that order (sorted descending on the group text) and a click on a group band does not break the switch list; the button fills the whole cell; ticks survive a change of script; the PowerShell quoting of `-Notes` text with quotes or spaces; the exit code of a script that ends with `throw` (should be 1); the detached `publish-admin.ps1` really starts after KBotAdmin has closed; `Read-Host` works in the window the page opens (shown only after a failure); the restart of KBotAdmin after a good publish-admin.
- Not in the K-BOT help: KBotAdmin is the owner's tool and has no help topics.
