# SLICE-0086-01 — KbotForm: partials into folders + one shared state object <a id="slice-0086-01"></a>

Operator request, 04.11.2026: «kbotform a devenit un monstru» — split it up, and «le-as vrea cumva puse si in
foldere, NU sa imi lasi totul in radacina». Phases agreed: (1) the workflows leave the form, (2) the shared
state becomes one object. Tests allowed, «dar nu exagera». **Done here: the folders and phase 2. Phase 1 (the
workflows into classes of their own) is NOT done** — see «Left unverified or deferred».

## What changed and why

**Folders.** The 41 `KbotForm*.vb` files (and `KbotForm.resx`) left the root of `src\KBot.App` for `KBotForm\`:

| Folder | Files |
|--------|-------|
| `KBotForm\` | `KbotForm.vb`, `.Designer.vb`, `.resx`, Periods, Units, Subunit, Chrome, Host, Pagini, MessageCatalog (these 7 stay nested under `KbotForm.vb`) |
| `KBotForm\Navigation\` | Views, Tree, TreeOptions, Grupe, Nomenclatoare |
| `KBotForm\Forexe\` | Console, Browser, ForexeWatch, Download, Ingest, Parallel, RobotQueue, KeRobot |
| `KBotForm\Documents\` | Ddf, DdfDelete, DdfSend, DdfSendMenu, DdfSendHelpers, Ord, Receptii, Machete |
| `KBotForm\Operations\` | CabNotes, UncorrectedOperations, AsociereGuard, Extrase |
| `KBotForm\Integrations\` | EFactura, Vercon, Adechit, Director |
| `KBotForm\Help\` | HelpCapture, HelpReveal, Tutorial |

Plain `rename`, no git. File names unchanged. `KBot.App.vbproj`: the 35 old `DependentUpon` blocks removed, 7 new ones
for the partials left beside the main file (a file in another folder cannot nest under it in Solution Explorer —
the folder is the grouping, as already done for `Avacont\...\KbotForm.*.vb`). VB does not put the folder in the
resource name: the form resource is still `KBot.App.KbotForm.resources`.

**Message catalog.** `Config\mesaje_catalog.json`: the `file` field of the 126 `KbotForm.*` entries rewritten to
the new paths (text replace, JSON re-parsed). The run-time lookup (`MessageOverrides.Apply`) goes by the file
**name without extension** + member, so the edited messages kept working through the move anyway.

**Phase 2 — `KBotForm\ShellState.vb`.** Ten loose fields of the form became one holder, `_state`:
`Periods`, `Views`, `ActiveView`, `CurrentInfo`, `TreeInfos`, `TreeOrder`, `IncludeHidden`, `TreeRows`,
`DescarcateInSesiune`, `DescarcareFaraIntrebare`. 133 references in 20 files rewritten (`_currentInfo` →
`_state.CurrentInfo`, …); declarations removed from `KbotForm.vb`. Not moved on purpose: `_cts` (documented trap,
born and killed in one flow), `_grupaActiva`, per-flow flags. The map in the `KbotForm` class remarks now lists the
folders.

## Files touched

New: `src/KBot.App/KBotForm/ShellState.vb`. Moved: the 41 `KbotForm*.vb` + `KbotForm.resx` into `src/KBot.App/KBotForm/...`.
Edited: the 20 partials that read the moved fields, `KBotForm/KbotForm.vb`, `src/KBot.App/KBot.App.vbproj`,
`src/KBot.App/Config/mesaje_catalog.json`, `tests/KBot.App.Tests/MainFormPoartaDdfTests.vb` (reads
`_state.CurrentInfo` instead of the field `_currentInfo`).

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: **0 warnings, 0 errors** after the move and after the state object.
- **The tests could not run.** `tests\KBot.App.Tests` does not compile: 10 × BC30456 in `DdfEditGrileTests.vb`
  (`btnAdauga` / `btnSterge` are not members of `DdfEditSectiuneaAPage`). The file is untouched by this slice
  (git shows no change), so the break predates it. The two form tests (`MainFormPoartaDdfTests`,
  `MainFormNavItemsTests`) were therefore not run; the edit to the first is unproven.

## Left unverified or deferred

- **Phase 1 not started.** Measured before touching anything: the workflows call each other in a chain —
  `ForexeWatch` → `Ingest` → captures (`TrimiteCapturileAsync`), `Download` → `Ingest`, `Parallel` → `Download`,
  `KeRobot` → `Ingest` — so they cannot leave one by one in any order; they go bottom-up (captures, Ingest, Download,
  Parallel, Watch) behind a small host interface (`WithReauth`, ApiClient, Session, Controller, RobotQueue, busyBar,
  `LoadTreeAsync`, `ShowModalAsync`, owner window). Each step changes async UI flows nobody can run here.
- Catalog trap for that phase: an edited message is found by file name + member. A class extracted into a new file
  loses its edits silently. Today one edited entry sits on `KbotForm`: `KbotForm.Ord.GenereazaInLotAsync#3`.
- Solution Explorer not looked at in Visual Studio; the application not run (build only).
- No help change (nothing the operator sees changed).
- Nothing committed (operator does git).
