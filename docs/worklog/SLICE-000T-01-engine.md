# SLICE-000T-01 — interactive tutorials: the engine

Operator request (04.10.2026): game-style tutorials that point at the next thing, wait for the user to do
it (never do it for them), cross windows (main tree -> Rezervari tab -> «+» -> DdfEditForm -> its tabs ->
Salveaza), have optional steps with «Sari peste» and a reason, ask «Vrei sa iesi din tutorial?» when the
user does anything else, and can be authored visually. Plan: `~/.claude/plans/i-need-a-tutorial-humble-pretzel.md`.
This sub-slice is the engine only (000T-02..05 follow).

## What changed and why

- `src/KBot.Theming/KBotTutorial.vb` — `IKBotTutorialHost` (`TutorialSupports`, `TutorialBegin`,
  `TutorialEnd`, `TutorialAnchor`, event `TutorialSignal`) and `KBotTutorialRequest` (flow id + the flow's
  `host-key`). A window that knows a tutorial state; the runner tells it to start with a key, so one
  window can serve several tutorials. Nothing implements it yet (000T-02).
- `src/KBot.App/Tutorial/TutorialFlow.vb` — file model and parser: header (`id`, `title`, `part`,
  `keywords`, `starts`, `host-key`), step keys (`target`, `part`, `anchor`, `wait`, `when`, `optional`,
  `merge`, `why`, `dim`, `allow`). Unknown key = `ArgumentException`. An optional step without `why` is
  refused. `AcceptedFrom(index)` = the look-ahead rule (the step itself; after an optional or merged step
  the next one too, up to and including the first mandatory one).
  **Added to the plan:** the `merge: yes` key (step 3 «select the reservation» is completed by the «+» click
  of step 4: they are one mouse-down) and `when: checked:Type.control` takes a full target.
- `src/KBot.App/Tutorial/TutorialRunner.vb` — one running tutorial: per step it resolves the target
  (`HelpTourRunner.FindTarget`, parts through `IKBotHelpParts`, anchors through the host), skips a step whose
  `when:` does not hold, arms what it waits for (tree `NodeMouseUp`, tree `RightIconClicked`, `Click`,
  `TextChanged` / `SelectedIndexChanged` / `CheckedChanged`) or polls state (nav tab selected, check box
  ticked, window opened / closed, `TutorialSignal`), ring + dim + bubble per host window created AFTER the
  window is shown, a 150 ms timer that follows a moved window, hides everything while another program or a
  non-K-BOT dialog is in front, and the exit question (`KBotMessage.Show`) on a click on the veil, a key typed
  in the dimmed window outside the allowed places (`IMessageFilter`), «Mă opresc», or the host window closing.
  «Da» ends it, «Nu» stays on the same step.
- `src/KBot.App/Tutorial/TutorialDim.vb` — the veil: borderless, black 45 %, `Region` with holes, no focus, owned
  by the host window; a click on it raises `Clicked`.
- `src/KBot.App/Help/HelpTourBubble.vb` — `ShowTutorial(...)`: the same callout bubble, no «Inapoi», right button
  «Sari peste» / «Inainte» / hidden, «Inchide» reads «Ma opresc»; arrow/Enter keys honour the hidden button.
  (No designer change.)
- `src/KBot.App/Help/HelpLibrary.vb` — `Tutorials`, `FindTutorial`, loads `<root>\tutorials\*.md`, the topic loader
  skips that folder. `HelpTour.CleanText` is now `Friend` (shared with the tutorial parser).
- `src/KBot.App/Help/HelpService.vb` — `StartTutorial(id)`.
- `tools/HelpCheck/Check-Help.ps1` — skips `tutorials\` in the topic scan, accepts slice id `000T` / `000T-NN`,
  validates tutorial files (header keys, targets, parts, wait / when kinds, `why` on optional steps, a warning
  when an `opens:` window does not implement `IKBotTutorialHost`), reports the tutorial count.
- `tests/KBot.App.Tests/TutorialFlowTests.vb` — parser + look-ahead tests (pure, no windows).

## Files touched

`src/KBot.Theming/KBotTutorial.vb` (new), `src/KBot.App/Tutorial/{TutorialFlow,TutorialRunner,TutorialDim}.vb` (new),
`src/KBot.App/Help/{HelpTourBubble,HelpTour,HelpLibrary,HelpService}.vb`, `tools/HelpCheck/Check-Help.ps1`,
`tests/KBot.App.Tests/TutorialFlowTests.vb` (new), `docs/worklog/KBOT_STATUS.md`,
`docs/worklog/state/KBOT_STATUS_0000-0009.md`.

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `tests\KBot.App.Tests` does **not compile**, from errors that were there before this work
  (`ForexeAnswerStoreTests.vb(43)`, `MainFormPoartaDdfTests.vb`, `MainFormNavItemsTests.vb`: `KbotForm`'s constructor
  now needs `capturiApi`). I did not touch those tests. `TutorialFlowTests` was run instead from a throw-away
  project (compiles only that file against `KBot.App`): **17 passed, 0 failed**.
- `Check-Help.ps1 -Coverage`: «No errors.»; the coverage list is NOT «(none)»: six windows without a topic
  (`HelpCaptureViewForm`, `IstoricIntervalForm`, `PrintListPage`, `SetariAccessView`, `SetariIstoricView`,
  `UpdateOfferForm`), none from this slice.
- Nothing was run on screen.

## Left unverified or deferred

- **Nothing seen on screen.** The per-window presenter inside the modal `DdfEditForm`, the veil against drop-down
  lists and the OS file dialog, the ring / veil following a moved window, the key policing — all written from
  reading code only.
- No window implements `IKBotTutorialHost` yet, no tutorial file exists, no search / «?» entry starts one: all of
  that is 000T-02. `HelpService.StartTutorial` has no caller yet.
- A `part:` that is hidden by state is NOT revealed for the step (tours do that through `IKBotHelpReveal`); the
  tutorial falls back to the whole control.
- Step 10 of the operator's flow (the «Element fundamentare» cell) may be impossible in the «+» flow: the grid is
  read-only when its rows come from reservations (`DdfDraft.DinRezervari`).
- Out of this slice: the «Parteneri» tab must follow the «Partener asociat» box (corrective of 0094-02).
- Help (`0000-NN`) and NOUTATI.md are not written: no operator-visible behaviour exists until 000T-02 wires a start.
- Weekly usage was 86 % at this point (stop line 95 %).
