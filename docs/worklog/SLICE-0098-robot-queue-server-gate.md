# SLICE-0098 — Robot queue + server gate (operator request, 30.09.2026)

The operator asked for three things:

1. While the FOREXE robot is running, nothing goes to or comes from the MariaDB server, so
   nothing can break halfway.
2. A queue for the robot: several refresh clicks in a row are kept in memory and run in the
   exact order they were made. While an «Asociere» window waits for the operator, the robot
   waits too, so there is never more than one «Asociere» window.
3. A small queue window with basic commands: pause, and cancel queued tasks.

No tests and no git, as asked.

## What changed and why

### 1. The server gate (`KBot.Api/ServerGate.vb`, `ServerGateHandler.vb`)
- Every request on the shared `HttpClient` now goes through `ServerGateHandler`. This covers
  `ApiClient`, `AuthApi` and `UpdateApi`.
- `ForexeController.RunGatedAsync` closes the gate around the runner call **only**. That
  includes the workflow run (`RunJobAsync` / `RunAsync`, so connect, downloads, DDF send and
  opening an angajament), the statements walk, the CAB document upload and the receipt search.
- It does not close the gate around the whole operation. The server reads that decide how the
  run starts (the local history for REVERSE, the last statement date) happen just before the
  run, and the ingest happens after it.
- **Closed gate = requests wait, they are not refused.** A view that loads during a download
  simply shows its busy bar until the run ends.
- Closing the gate first waits for any request already on the wire to come back, answer body
  included. So the robot never starts while an answer is still arriving.
- **The timeout moved into the handler.** It is counted after the wait, and
  `HttpClient.Timeout` is now infinite. Otherwise a request held for a four-minute download
  would die at 100 s without ever leaving the PC.
  - When it fires, the handler throws the same error as `HttpClient` (a
    `TaskCanceledException` over a `TimeoutException`), so the retry in `ApiClient` still
    recognises it.
- **Some requests pass a closed gate** (`ServerGate.Bypass`, set on the request):
  - `process_excel`: a workflow step waits for it; no database.
  - `marcaj/rezerva`: the FOREXE page waits for the marker when it saves, which can happen
    inside a DDF send run.
  - The two update-channel GETs: no database, and the zip is streamed.
  - Holding the first two would hold the robot forever.

### 2. The robot queue (`KBot.App/Forexe/RobotQueue.vb`)
- **One task = the whole operation.** That means the questions, the download, the two-phase
  ingest (the «Asociere» window included), the tree reload and the pictures.
- Tasks run one at a time, in order, on the UI thread. So the next robot run cannot start
  while an «Asociere» window is open, and the robot never runs while the previous package is
  being written (rule 1).
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

- A second click on something already queued or running is refused, with a line on the
  console.
- **Before each task the queue waits for the robot to be idle.** An operation started outside
  the queue (the Conectare button, the «Browser FOREXE» view) finishes first instead of making
  the task fail as «busy».
- **A task that calls another queued flow runs it in place.** Otherwise it would wait behind
  itself forever. This is detected with an `AsyncLocal`.
- **A task that never ran throws `RobotTaskDroppedException`**, with a Romanian message. The
  click handlers swallow it, because the console already said it. The CAB note shows it in
  its receipt window.
- **`DuLaIngestieAsync`** marks the queue as waiting while the «Asociere» window is open, and
  the queue window shows it.
- **`KbotForm.LoadTreeAsync`** now reads the selected node **after** the tree request comes
  back.
  - Before, it read it before the request. With the gate, that request can wait for a whole
    robot run, and the reload would have put back the old node and undone a click made in
    the meantime.

### 3. The queue window (`RobotQueueForm`) and the footer button
- **`RobotQueueForm`** is modeless, owned by the shell and does not take focus when shown.
  - It shows the running task and what it waits for, then the waiting tasks in order, with
    the time each was queued.
  - Commands:
    - **Pauză / Continuă**: the running task finishes and nothing new starts.
    - **Scoate**: removes the selected waiting task.
    - **Golește coada**: removes all waiting tasks, after a confirmation.
    - **Oprește curenta**: the robot's own cancel, the same as «Anulează» in the console. It
      only acts while the robot is running.
- **When it opens:**
  - by itself, the moment a task has to wait behind another;
  - from the new footer button **«Coadă N»**, which is visible only while the queue has tasks
    and shows «‖» when paused.
  - If the operator closes it while tasks are waiting, it does not reopen by itself until the
    queue has been empty once.

## Files touched
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

## Test results
- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**.
- `dotnet build KBot.sln` still fails in `tests/KBot.App.Tests`. The errors (`KbotForm.New`
  without `capturiApi`, `ForexeAnswerStoreTests` line 43) are older than this slice and were
  not touched.
- No tests run and none written (operator's instruction). Not run, not seen on screen.

## Left unverified or deferred
- **Nothing was run.** Not the gate, not the queue order, not the window, not the footer
  button, at any DPI or theme.
- **Views wait during a download.** A node clicked while the robot runs shows its busy bar
  until the run ends, which can be minutes. That is the rule as asked. If it feels too
  strict, reads could be let through and only writes held.
- **Modal boxes of a running task.** They disable the queue window like every other window,
  so pause and cancel can't be pressed while an «Asociere» window is open. The window only
  shows that the queue is waiting.
  - If the window opens by itself *during* such a box, it is a new window and stays usable.
- **«Oprește curenta»** stops only the robot part. A task in its ingest phase finishes
  normally.
- **Direct robot callers outside the queue** (the Conectare button, the «Browser FOREXE» view
  opening a node) are not queued. The queue waits for them before each task, but one started
  in the few seconds between a task's start and its robot call still gets «Rulează deja o
  operație FOREXE».
- **The queue lives in memory only.** Closing K-BOT drops the waiting tasks, as asked.
- **`SincronizeazaAsync`** (unreachable since 20.09.2026) was left out of the queue.
- **Help not updated.** The working tree has uncommitted help edits (0000 work in progress), so
  the topics were not touched. The stale topics are named in the 0000 «Ajutor de actualizat»
  line:
  - `contabil.forexe.descarcare`
  - `contabil.fereastra` (footer button «Coadă»)
  - `contabil.vederi.receptii`
  - `contabil.ddf.index`
- `KBot.App` / `KBot.Api` FileVersion not bumped (done at `push-update.ps1`).
