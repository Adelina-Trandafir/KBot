# SLICE-0098-02 — Reads pass the gate + robot queue bench (operator request, 30.09.2026)

The operator gave two answers after 0098:

1. "Let reads through and hold only writes."
2. "Add a harness for testing the queue system. It doesn't have access to the real FOREXE,
   so use a mock system with FOREXE-like answers."

No tests and no git, as asked.

## What changed and why

### 1. Only writes wait at the gate
- `ServerGate.IsRead(method)` treats GET, HEAD and OPTIONS as reads.
  `ServerGateHandler` lets them straight through: no wait, not counted, body not buffered.
- Everything else waits while the robot runs, as before.
- Closing the gate now waits only for **writes** already on the wire.
- **The method is the only thing the gate can see.** So a POST that only reads (the ingest
  proposal, which the server rolls back) is still held. Holding a read is harmless; letting a
  write through is not.
- **Effect:** clicking another angajament during a download loads its view at once. Only its
  writes wait. This removes the "views wait for minutes" note from 0098.
- The comment in `KbotForm.LoadTreeAsync` was updated. Reading the selection after the request
  is still right, because a reload after a queued refresh can take a while.

### 2. The bench (DevHarness → category «FOREXE»)
The entry is «Coada robotului + poarta serverului (FOREXE și server simulate)». It is not
destructive and needs no login or connection.

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
    - returns an answer shaped like «Prelucrare Completa»: the flat variables plus
      `TabelIstoric` (Timp / Utilizator / Descriere / Observatii), `TabelIndicatori_results`
      and `ListaReceptii_results`, with random rows.
  - **`FakeServerHandler`** is the last step of the bench's own `HttpClient`. It answers with
    JSON after a set delay and writes to the journal when each request **arrives** and when it
    is **answered**.
- **The bench uses its own gate, client, controller and queue.** It cannot hold or release the
  app's real requests.
- **One bench task has the same shape as `DescarcaNodulAsync`:**
  1. GET (the receptions question);
  2. the controller's `DownloadNodeAsync`, whose history read is a GET made before the run;
  3. POST the proposal;
  4. the «Asociere» window, simulated with a message box, with the queue note set while it
     is open;
  5. POST the save.
- **Scenario buttons:**
  - refresh the selected nodes, in order;
  - all six nodes in a row;
  - the same node twice (the second must be refused);
  - an operation from the FOREXE page, which goes in front of the queue;
  - a robot run started outside the queue, with `DeschideAngajamentAsync` (the queue must wait
    for it);
  - a manual GET (must pass at once) and a manual POST (must wait while the robot runs);
  - open the queue window;
  - clear the journal.
- **Settings:** robot run length (s), server delay (ms), open the simulated «Asociere», make
  the robot fail.
- **Live labels:** whether the gate is open or closed, and whether the robot is busy.
- **The journal** has timestamps and is copied into the harness run log.
- **Verdict buttons:** «Merge corect» and «Nu merge».

## Files touched
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

## Test results
- `dotnet build src/KBot.App/KBot.App.vbproj`, Debug and Release: **0 warnings, 0 errors**.
- The bench was **not run** (operator rule: no UI runs unless asked). Nothing was seen on screen.

## Left unverified or deferred
- **The bench itself was never opened.** Layout, theme and every scenario are unchecked.
- **The simulated «Asociere» is a `KBotMessage` box.** Like every operator message, it is also
  written to `mesaje_operator.log`, marked «[SIMULARE]».
- **The bench writes the controller's usual files to disk** for each simulated run, under the
  codes PROBA-*: the answer in «Rezultate_Forexe», the package in «WorkflowResults», and the run
  log.
- **The bench's controller stays subscribed to `ThemeManager.ThemeChanged`** after the bench
  closes. The controller has no unsubscribe method, so it is kept alive until the harness exits.
  This is harmless.

## Follow-up (same day): the queue window opens by itself only above one action

The operator said: "the queue form should appear only when MORE THAN one action is being
performed through forexe".

- **The fault.** The shell opened the window whenever the waiting list was not empty.
  - One click passes through the waiting list for a moment before the queue picks it up.
  - It stays there longer while a robot operation started outside the queue finishes.
  - So a single action could open the window.
- **The fix.** New `RobotQueue.ActionCount(robotBusy)`: the waiting tasks plus one for the
  running task. When no queued task runs, a robot operation started outside the queue
  (`ForexeController.IsBusy`) counts as that one.
  - `KbotForm.RobotQueue_Changed` opens the window only when the count is above 1.
  - The bench follows the same rule. It no longer opens the window when it starts.
  - The footer button «Coadă N» still opens it at any time.
- **A build break found on the way.** Visual Studio had created two blank template `.resx`
  files, `KbotForm.RobotQueue.resx` and `KbotForm.AsociereGuard.resx`. That happens when a
  partial file of `KbotForm` is opened in the designer.
  - Both map to the same resource name as the real `KbotForm.resx`, so the build failed with
    MSB3577.
  - Both held only the template's sample entries. They were **moved** out of the repo (not
    deleted) to the session scratch folder.
  - `Forexe/RobotQueueForm.resx` (also new, created by the designer) was left in place.
- **Build:** Debug and Release, 0 warnings, 0 errors. Not run.
- **Files:** `src/KBot.App/Forexe/RobotQueue.vb`, `src/KBot.App/KbotForm.RobotQueue.vb`,
  `src/KBot.App/HarnessTests/RobotQueueHarnessForm.vb`.
