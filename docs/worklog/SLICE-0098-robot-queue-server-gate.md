# SLICE-0098 — Coada robotului și poarta cererilor

Istoric din 30.09.2026. Etapa 02 înlocuiește blocarea tuturor cererilor cu blocarea scrierilor după metoda HTTP și schimbă deschiderea automată a ferestrei la mai mult de o acțiune. Bancul simulat a fost construit, dar nu a fost rulat. Declarațiile inițiale de mai jos sunt checkpointuri, nu o a doua regulă actuală.

- [0098 — arhitectura inițială](#slice-0098)
- [0098-02 — citirile, bancul și fereastra](#slice-0098-02)

<a id="slice-0098"></a>

## SLICE-0098 — Robot queue + server gate (operator request, 30.09.2026)

The operator asked for three things:

1. While the FOREXE robot is running, nothing goes to or comes from the MariaDB server, so nothing can break halfway.
2. A queue for the robot: several refresh clicks in a row are kept in memory and run in the exact order they were made. While an «Asociere» window waits for the operator, the robot waits too, so there is never more than one «Asociere» window.
3. A small queue window with basic commands: pause, and cancel queued tasks.

No tests and no git, as asked.

### What changed and why

#### 1. The server gate (`KBot.Api/ServerGate.vb`, `ServerGateHandler.vb`)
- Every request on the shared `HttpClient` now goes through `ServerGateHandler`. This covers `ApiClient`, `AuthApi` and `UpdateApi`.
- `ForexeController.RunGatedAsync` closes the gate around the runner call **only**. That includes the workflow run (`RunJobAsync` / `RunAsync`, so connect, downloads, DDF send and opening an angajament), the statements walk, the CAB document upload and the receipt search.
- Pre-run server reads (REVERSE local history, last statement date) and post-run ingest remain outside the gate.
- **Closed gate = requests wait, they are not refused.** A view that loads during a download simply shows its busy bar until the run ends.
- Closing the gate first waits for any request already on the wire to come back, answer body included. So the robot never starts while an answer is still arriving.
- **The handler counts timeout after the gate wait; `HttpClient.Timeout` is infinite.** Otherwise a four-minute download could exhaust the 100 s timeout before a waiting request left the PC.
  - When it fires, the handler throws the same error as `HttpClient` (a `TaskCanceledException` over a `TimeoutException`), so the retry in `ApiClient` still recognises it.
- **Some requests pass a closed gate** (`ServerGate.Bypass`, set on the request):
  - `process_excel`: a workflow step waits for it; no database.
  - `marcaj/rezerva`: the FOREXE page waits for the marker when it saves, which can happen inside a DDF send run.
  - The two update-channel GETs: no database, and the zip is streamed.
  - Holding the first two would hold the robot forever.

#### 2. The robot queue (`KBot.App/Forexe/RobotQueue.vb`)
- **One task = the whole operation.** That means the questions, the download, the two-phase ingest (the «Asociere» window included), the tree reload and the pictures.
- Tasks run sequentially on the UI thread; the next run waits for the previous association window and package write (rule 1).
- **Entry points routed through the queue:**

  | Entry point | Duplicate guard key |
  |---|---|
  | Node right icon | `nod|cod` |
  | List footer icon | `lista` |
  | Recepții refresh | `receptii|cod` |
  | Rezervări refresh | `rezervari|cod` |
  | Statements (footer left icon, Extrase window) | `extrase` |
  | DDF «Trimite / Reia trimiterea» | none |
  | Rezervări menu «Definitivează / Derulează» | none |
  | CAB note upload and receipt search | none |
  | Operation captured in the FOREXE page | none; goes **in front** of the waiting tasks, because its pictures are of the page as the operator left it |

- A second click on something already queued or running is refused, with a line on the console.
- **Each task first waits for robot idle**, allowing an outside operation («Conectare» / «Browser FOREXE») to finish instead of failing as busy.
- **Nested queued flows run in place**, detected with `AsyncLocal`, avoiding a task waiting behind itself.
- **A task that never ran throws `RobotTaskDroppedException`**, with a Romanian message. The click handlers swallow it, because the console already said it. The CAB note shows it in its receipt window.
- **`DuLaIngestieAsync`** marks the queue as waiting while the «Asociere» window is open, and the queue window shows it.
- **`KbotForm.LoadTreeAsync`** now reads the selected node **after** the tree request comes back.
  - Previously selection was read before the request; a gate wait could then restore the old node over a newer click.

#### 3. The queue window (`RobotQueueForm`) and the footer button
- **`RobotQueueForm`** is modeless, owned by the shell and does not take focus when shown.
  - It shows the running task and what it waits for, then the waiting tasks in order, with the time each was queued.
  - Commands:
    - **Pauză / Continuă**: the running task finishes and nothing new starts.
    - **Scoate**: removes the selected waiting task.
    - **Golește coada**: removes all waiting tasks, after a confirmation.
    - **Oprește curenta**: the robot's own cancel, the same as «Anulează» in the console. It only acts while the robot is running.
- **When it opens:**
  - by itself, the moment a task has to wait behind another;
  - from the new footer button **«Coadă N»**, which is visible only while the queue has tasks and shows «‖» when paused.
  - If the operator closes it while tasks are waiting, it does not reopen by itself until the queue has been empty once.

### Files touched
- **New:**
  - `src/KBot.Api/ServerGate.vb`
  - `src/KBot.Api/ServerGateHandler.vb`
  - `src/KBot.App/Forexe/RobotQueue.vb`
  - `src/KBot.App/Forexe/RobotQueueForm.vb`
  - `src/KBot.App/Forexe/RobotQueueForm.Designer.vb`
  - `src/KBot.App/KbotForm.RobotQueue.vb`
- **Changed:**
  - `src/KBot.Api/ApiClient.vb` (Excel bypass)
  - `src/KBot.Api/ApiClient.Marcaj.vb` (bypass)
  - `src/KBot.Api/UpdateApi.vb` (bypass)
  - `src/KBot.App/Program.vb` (gate + handler, infinite `HttpClient.Timeout`)
  - `src/KBot.App/Forexe/ForexeController.vb` (gate parameter, `RunGatedAsync`, `WaitUntilIdleAsync`)
  - `src/KBot.App/Forexe/ForexeController.DocumentUpload.vb`
  - `src/KBot.App/Forexe/ForexeFooterView.vb` and `.Designer.vb` (`btnCoada`, `BindQueue`, `QueueRequested`)
  - `src/KBot.App/KbotForm.vb`
  - `src/KBot.App/KbotForm.Download.vb`
  - `src/KBot.App/KbotForm.Ingest.vb`
  - `src/KBot.App/KbotForm.Extrase.vb`
  - `src/KBot.App/KbotForm.Ddf.vb`
  - `src/KBot.App/KbotForm.CabNotes.vb`
  - `src/KBot.App/KbotForm.ForexeWatch.vb`
  - `src/KBot.App/KbotForm.Tree.vb`

### Test results
- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**.
- `dotnet build KBot.sln` still fails in `tests/KBot.App.Tests`. The errors (`KbotForm.New` without `capturiApi`, `ForexeAnswerStoreTests` line 43) are older than this slice and were not touched.
- No tests run and none written (operator's instruction). Not run, not seen on screen.

### Left unverified or deferred
- **Nothing was run.** Not the gate, not the queue order, not the window, not the footer button, at any DPI or theme.
- **Views wait during a download.** A node clicked while the robot runs shows its busy bar until the run ends, which can be minutes. That is the rule as asked. If it feels too strict, reads could be let through and only writes held.
- **Modal boxes of a running task.** They disable the queue window like every other window, so pause and cancel can't be pressed while an «Asociere» window is open. The window only shows that the queue is waiting.
  - If the window opens by itself *during* such a box, it is a new window and stays usable.
- **«Oprește curenta»** stops only the robot part. A task in its ingest phase finishes normally.
- **Direct robot callers outside the queue** (the Conectare button, the «Browser FOREXE» view opening a node) are not queued. The queue waits for them before each task, but one started in the few seconds between a task's start and its robot call still gets «Rulează deja o operație FOREXE».
- **The queue lives in memory only.** Closing K-BOT drops the waiting tasks, as asked.
- **`SincronizeazaAsync`** (unreachable since 20.09.2026) was left out of the queue.
- **Help not updated.** The working tree has uncommitted help edits (0000 work in progress), so the topics were not touched. The stale topics are named in the 0000 «Ajutor de actualizat» line:
  - `contabil.forexe.descarcare`
  - `contabil.fereastra` (footer button «Coadă»)
  - `contabil.vederi.receptii`
  - `contabil.ddf.index`
- `KBot.App` / `KBot.Api` FileVersion not bumped (done at `push-update.ps1`).

---

<a id="slice-0098-02"></a>

## SLICE-0098-02 — Reads pass the gate + robot queue bench (operator request, 30.09.2026)

The operator gave two answers after 0098:

1. "Let reads through and hold only writes."
2. "Add a harness for testing the queue system. It doesn't have access to the real FOREXE, so use a mock system with FOREXE-like answers."

No tests and no git, as asked.

### What changed and why

#### 1. Only writes wait at the gate
- `ServerGate.IsRead(method)` treats GET, HEAD and OPTIONS as reads. `ServerGateHandler` lets them straight through: no wait, not counted, body not buffered.
- Everything else waits while the robot runs, as before.
- Closing the gate now waits only for **writes** already on the wire.
- **The method is the only thing the gate can see.** So a POST that only reads (the ingest proposal, which the server rolls back) is still held. Holding a read is harmless; letting a write through is not.
- **Effect:** clicking another angajament during a download loads its view at once. Only its writes wait. This removes the "views wait for minutes" note from 0098.
- The comment in `KbotForm.LoadTreeAsync` was updated. Reading the selection after the request is still right, because a reload after a queued refresh can take a while.

#### 2. The bench (DevHarness → category «FOREXE»)
The entry is «Coada robotului + poarta serverului (FOREXE și server simulate)». It is not destructive and needs no login or connection.

- **The real parts are wired together:**
  - the real `RobotQueue`;
  - the real `ServerGate` and `ServerGateHandler`;
  - a real `ForexeController`, so the gate closes exactly where it does in the app;
  - the real `RobotQueueForm`.
- **Two simulated parts:**
  - **`FakeForexeRunner`** implements `IForexeRunner` with no browser. Each run:
    - takes a set number of seconds, in ten steps, with progress and status lines;
    - honours «Oprește curenta»;
    - can be told to fail;
    - halfway through, sends a request marked Bypass, like the real Excel step does;
    - returns an answer shaped like «Prelucrare Completa»: the flat variables plus `TabelIstoric` (Timp / Utilizator / Descriere / Observatii), `TabelIndicatori_results` and `ListaReceptii_results`, with random rows.
  - **`FakeServerHandler`** is the last step of the bench's own `HttpClient`. It answers with JSON after a set delay and writes to the journal when each request **arrives** and when it is **answered**.
- **The bench uses its own gate, client, controller and queue.** It cannot hold or release the app's real requests.
- **One bench task has the same shape as `DescarcaNodulAsync`:**
  1. GET (the receptions question);
  2. the controller's `DownloadNodeAsync`, whose history read is a GET made before the run;
  3. POST the proposal;
  4. the «Asociere» window, simulated with a message box, with the queue note set while it is open;
  5. POST the save.
- **Scenario buttons:**
  - refresh the selected nodes, in order;
  - all six nodes in a row;
  - the same node twice (the second must be refused);
  - an operation from the FOREXE page, which goes in front of the queue;
  - a robot run started outside the queue, with `DeschideAngajamentAsync` (the queue must wait for it);
  - a manual GET (must pass at once) and a manual POST (must wait while the robot runs);
  - open the queue window;
  - clear the journal.
- **Settings:** robot run length (s), server delay (ms), open the simulated «Asociere», make the robot fail.
- **Live labels:** whether the gate is open or closed, and whether the robot is busy.
- **The journal** has timestamps and is copied into the harness run log.
- **Verdict buttons:** «Merge corect» and «Nu merge».

### Files touched
- **New:**
  - `src/KBot.App/HarnessTests/FakeForexeRunner.vb`
  - `src/KBot.App/HarnessTests/FakeServerHandler.vb`
  - `src/KBot.App/HarnessTests/RobotQueueHarnessForm.vb`
  - `src/KBot.App/HarnessTests/RobotQueueHarnessForm.Designer.vb`
  - `src/KBot.App/HarnessTests/RobotQueueHarnessTest.vb`
  - All Debug only (`#If DEBUG`).
- **Changed:**
  - `src/KBot.Api/ServerGate.vb`
  - `src/KBot.Api/ServerGateHandler.vb`
  - `src/KBot.App/KbotForm.Tree.vb` (comment only)

### Test results
- `dotnet build src/KBot.App/KBot.App.vbproj`, Debug and Release: **0 warnings, 0 errors**.
- The bench was **not run** (operator rule: no UI runs unless asked). Nothing was seen on screen.

### Left unverified or deferred
- **The bench itself was never opened.** Layout, theme and every scenario are unchecked.
- **The simulated «Asociere» is a `KBotMessage` box.** Like every operator message, it is also written to `mesaje_operator.log`, marked «[SIMULARE]».
- **The bench writes the controller's usual files to disk** for each simulated run, under the codes PROBA-*: the answer in «Rezultate_Forexe», the package in «WorkflowResults», and the run log.
- **The bench's controller stays subscribed to `ThemeManager.ThemeChanged`** after the bench closes. The controller has no unsubscribe method, so it is kept alive until the harness exits. This is harmless.

### Follow-up (same day): the queue window opens by itself only above one action

The operator said: "the queue form should appear only when MORE THAN one action is being performed through forexe".

- **The fault.** The shell opened the window whenever the waiting list was not empty.
  - One click passes through the waiting list for a moment before the queue picks it up.
  - It stays there longer while a robot operation started outside the queue finishes.
  - So a single action could open the window.
- **The fix.** New `RobotQueue.ActionCount(robotBusy)`: the waiting tasks plus one for the running task. When no queued task runs, a robot operation started outside the queue (`ForexeController.IsBusy`) counts as that one.
  - `KbotForm.RobotQueue_Changed` opens the window only when the count is above 1.
  - The bench follows the same rule. It no longer opens the window when it starts.
  - The footer button «Coadă N» still opens it at any time.
- **A build break found on the way.** Visual Studio had created two blank template `.resx` files, `KbotForm.RobotQueue.resx` and `KbotForm.AsociereGuard.resx`. That happens when a partial file of `KbotForm` is opened in the designer.
  - Both map to the same resource name as the real `KbotForm.resx`, so the build failed with MSB3577.
  - Both held only the template's sample entries. They were **moved** out of the repo (not deleted) to the session scratch folder.
  - `Forexe/RobotQueueForm.resx` (also new, created by the designer) was left in place.
- **Build:** Debug and Release, 0 warnings, 0 errors. Not run.
- **Files:** `src/KBot.App/Forexe/RobotQueue.vb`, `src/KBot.App/KbotForm.RobotQueue.vb`, `src/KBot.App/HarnessTests/RobotQueueHarnessForm.vb`.
