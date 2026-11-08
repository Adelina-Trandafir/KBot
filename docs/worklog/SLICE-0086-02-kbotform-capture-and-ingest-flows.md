# SLICE-0086-02 — KbotForm phase 1, steps 1-2: the picture flow and the ingest leave the form <a id="slice-0086-02"></a>

Operator go-ahead, 04.11.2026 («Da, pe pași»), after [0086-01](SLICE-0086-01-kbotform-folders-and-shell-state.md#slice-0086-01).
Phase 1 goes bottom-up because the flows call each other: captures → Ingest → Download → Parallel → ForexeWatch.
**This slice is the first two.** Download, Parallel, ForexeWatch and the rest are still partials of the form.

## What changed and why

Two flows became classes of their own in `KBotForm\Forexe\`, built once by the form (`_captures`, `_ingest`). The code was
moved by text transformation, not retyped; behaviour is meant to be identical.

| New file | Was | Contents |
|----------|-----|----------|
| `CaptureFlow.vb` (243 l) | block of `KbotForm.ForexeWatch.vb` | `TakeReceptionPicturesAsync`, `SendAsync`, `AskToDelete`, `SayNotSent`, `PictureLabel` |
| `IngestFlow.vb` (524 l) | whole `KbotForm.Ingest.vb` (deleted) | `IngestAsync` (was `DuLaIngestieAsync`), `SaveWithoutFormAsync`, `ChooseReceptiiToSkipAsync`, `RefreshReceptii(Async)`, `RefreshRezervari(Async)`, `OfferRefreshOfCutReceptiiAsync` |
| `ForexeNotices.vb` | `KbotForm.AratEsecul` | `RobotFailure(owner, controller, whatStarted)`; the form keeps `AratEsecul` as a one-line forwarder for its ~12 remaining callers |
| `IShellHost.vb` | — | what a flow still asks of the form: `WithReauth`, `SetBusy`, `LoadTreeAsync`, `ShowModalAsync`, `CodRef`, `AngajamentName`, `MarkDownloadWithoutQuestion`, `AssociationAllowsAsync` (8 members, all behaviours; data objects go to the constructors) |
| `KbotForm.Flows.vb` | — | `Implements IShellHost` (each member forwards to the method the form already had), the `_captures` / `_ingest` fields, `AratEsecul` |

Wiring: two lines in the `KbotForm` constructor (after `_capturiApi`); the callers in `Download`, `Parallel`, `ForexeWatch`,
`KeRobot`, `DdfSend`, `DdfSendMenu`, `Views` now call `_captures.SendAsync/SayNotSent/TakeReceptionPicturesAsync` and
`_ingest.IngestAsync/ChooseReceptiiToSkipAsync/RefreshReceptii/RefreshRezervari`. In the moved code: `Me` → the owner window,
`busyBar.Running = x` → `_host.SetBusy(x)` (14 calls kept, checked by count), `WithReauth` → `_host.WithReauth`; the
`GlobalErrorLog` source tags now name the new class (`IngestFlow.IngestAsync`, `CaptureFlow.SendAsync`, …).

**Message catalog.** The 13 entries that moved (`KbotForm.Ingest.*`, `KbotForm.ForexeWatch.IntreabaStergereaCapturilor`)
were re-keyed to the new file / member in `Config\mesaje_catalog.json` (id, file, function; JSON re-parsed). None of the 13
was edited, so no operator wording was lost. Line numbers in those entries are stale until the bench's «Actualizează».

Size: the `KbotForm*.vb` partials went from 10,180 to 9,521 lines (Designer included); the two flows account for the
difference (about 730 lines moved out).

## Files touched

New: `src/KBot.App/KBotForm/IShellHost.vb`, `KBotForm/Forexe/{CaptureFlow,IngestFlow,ForexeNotices,KbotForm.Flows}.vb`.
Deleted: `KBotForm/Forexe/KbotForm.Ingest.vb`. Edited: `KBotForm/KbotForm.vb` (constructor), `KBotForm/Forexe/KbotForm.{ForexeWatch,Download,Parallel,KeRobot}.vb`,
`KBotForm/Documents/KbotForm.{DdfSend,DdfSendMenu}.vb`, `KBotForm/Navigation/KbotForm.Views.vb`, `src/KBot.App/Config/mesaje_catalog.json`.

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`, Debug AND Release: **0 warnings, 0 errors**.
- No test could run (`tests\KBot.App.Tests` does not compile: `DdfEditGrileTests`, see 0086-01). Not run on screen.

## Left unverified or deferred

- **Nothing was run.** The download → ingest → association window → pictures road, the two partial refreshes and the
  «la ce sa renunt» question were never exercised after the move; the operator should try one download of a node, one
  receptii refresh and one reservation save.
- Caller order assumption: `_ingest` / `_captures` are built in the constructor before any view exists (Views take
  `AddressOf _ingest.RefreshReceptii` later). True by construction; not seen running.
- Still in the form, in the order planned: Download (393 l), Parallel (254), ForexeWatch (≈540 after this cut), then
  Ddf*/Ord/Tree/TreeOptions. `KbotForm.Console` and `Machete` are glue to the window and may stay.
- Help: nothing the operator sees changed. Nothing committed (operator does git).
