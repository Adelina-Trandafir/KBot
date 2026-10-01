# SLICE-0777-02 — Underlined downloads, auto-all receptii, queue window closes itself (operator request, 01.10.2026)

## What changed and why
1. **Tree rows underlined for what was downloaded / refreshed in this run.** `KbotForm._descarcateInSesiune`
   gets the code when `DuLaIngestieAsync` has saved the package (node, Recepții and Rezervări refresh all
   pass there), before the tree reload; `PopulateTree` sets `TreeItem.Underline`. Memory only: empty at every start.
   New `TreeItem.Underline` flag, painted next to Bold/Italic (`AdvancedTreeControl.Painting.vb`). Caption only.
2. **No receptii selection window when several refreshes run together.** A click on a node (or a Recepții
   refresh) while another FOREXE action runs or waits marks that code, and the node / receptii tasks already
   waiting, in `_descarcareFaraIntrebare`; `AlegeReceptiileDeSaritAsync` then returns an empty skip list
   (everything is downloaded) and says so on the console. A lone click clears the mark and asks as before.
3. **The queue window closes when the queue is empty** (`RobotQueue_Changed` → `CloseRobotQueueIfIdle`,
   re-checked in a posted message so the pump can start the next task first).

## Files touched
`KBot.Controls/Tree/AdvancedTreeControl.TreeItem.vb`, `.Painting.vb`; `KBot.App/KbotForm.vb`, `.Tree.vb`,
`.Ingest.vb`, `.Download.vb`, `.RobotQueue.vb`; help `contabil/fereastra.md` (tag 0777-02).

## Test results
`dotnet build KBot.App` clean, 0 warnings. No tests, nothing run on screen (operator rule).

## Unverified / deferred
Nothing seen on screen. A node whose selection window was already shown when the next click arrives is not
changed. A mark left by a task dropped from the queue stays until the next click on that code.
