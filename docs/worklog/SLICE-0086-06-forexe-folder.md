# SLICE-0086-06 — the `Forexe` folder sorted; `KBotForm\Forexe` renamed `KBotForm\Robot` <a id="slice-0086-06"></a>

Operator request, 05.11.2026: «și asta» (the `Forexe` folder of `KBot.App`), then agreed to: icons stay where they are, rename
`KBotForm\Forexe` to `KBotForm\Robot`, delete `IstoricIntervalForm`. Continuation of
[0086-05](SLICE-0086-05-setari-folder.md#slice-0086-05).

## What changed and why

There were two folders called `Forexe` (the root one and `KBotForm\Forexe`). The root one held 46 files of three kinds.
Who uses what was read from the type-name graph (comments and strings stripped, dead code not a user).

| Folder | Units | Why |
|--------|-------|-----|
| `Forexe\Robot\` | `ForexeController` (4 parts), `ForexeAnswerStore`, `ForexeRunDump`, `WorkflowResultStore`, `RobotQueue`, `ParallelDownloadBoard` | the robot side; `ForexeController` is used by `KbotForm` AND `AvacontMain` |
| `Forexe\Asociere\` | `AsociereForm`, `AsociereBenziForm`, `GraficeAsociereForm`, `CorectieValoareForm`, `PrelucrareCoordinator`, `AlegereUnitateForm` | the association family; `AsociereForm` is opened by `KbotForm` AND `AvacontMain` |
| `KBotForm\Views\` | `ForexeConsoleForm`, `ForexeFooterView`, `ForexeHistoryForm`, `RobotQueueForm`, `ActualizareMultiplaForm`, `SelectieReceptiiForm` | windows only `KbotForm` (or its `IngestFlow`) opens |
| `Forexe\Forexe_Icons\`, `Forexe\Receptii_Icons\` | (unchanged) | read by `My Project\Resources.resx` by path; left in place so the resource file is not touched |
| **deleted** | `IstoricIntervalForm` (+ Designer) | named by no live code; its 2 catalog entries removed; git keeps the files at `Forexe/` |

`KBotForm\Forexe\` became `KBotForm\Robot\` (12 files, names unchanged). 39 files moved, 2 deleted. Plain renames, no code touched
except comments (the folder map in `KbotForm.vb`, one line in `KbotForm.Flows.vb`). Catalog: 37 paths re-pointed to `KBotForm/Robot/`,
27 to the new homes, 2 entries removed. `KBot.App.vbproj` needed nothing.

## Files touched

39 moved, 2 deleted, 12 renamed with their folder; edited `src/KBot.App/Config/mesaje_catalog.json`,
`src/KBot.App/KBotForm/KbotForm.vb` (remarks), `src/KBot.App/KBotForm/Robot/KbotForm.Flows.vb` (comment).

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj`, Debug AND Release: **0 warnings, 0 errors**. No test run (the test project does not compile, see 0086-01).

## Left unverified or deferred

- Visual Studio not looked at; no window opened.
- `Forexe\` still has two names that are close to `KBotForm\Robot\` in meaning; not a problem, but the next reader should know the robot's controller lives in `Forexe\Robot\` and the form's FOREXE parts in `KBotForm\Robot\`.
- `KBot.Domain\PrelucrareRezultat.vb` has a comment naming the old path `KBot.App\Forexe\WorkflowResultStore.vb` (history of slice 0048-02, left as it is).
- Older worklogs still name the old paths. Nothing committed (operator does git).
