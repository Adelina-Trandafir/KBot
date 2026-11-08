# SLICE-0086-05 — the `Setari` folder given an order <a id="slice-0086-05"></a>

Operator request, 05.11.2026: «hai sa facem si folderul setari». Continuation of
[0086-04](SLICE-0086-04-kbot-app-root-cleanup.md#slice-0086-04).

## What changed and why

`src\KBot.App\Setari\` had 49 files side by side: the window, its pages, three dialogs and the FOREXE page-rule editor.
The same rule as for `KBotForm\Views` was applied: what only the settings window uses stays under `Setari\`;
what another part of the program also uses goes to `Comune`.

| Folder | Units |
|--------|-------|
| `Setari\` | `SetariForm` (the window, opened by `KbotForm` and `Program`), `ISetariView` (the contract of one page) |
| `Setari\Views\` | the pages only the window shows: `SetariAccessView`, `SetariAplicatieView`, `SetariAutentificareView`, `SetariExtraseView`, `SetariFolder`, `SetariForexeView`, `SetariInfoView`, `SetariMultithreadView`, `SetariPaginaView`, `SetariSemnareDdfView`, `SetariTemaView` |
| `Setari\Views\Comune\` | `SetariIstoricView` (also hosted by `IstoricVersiuniPage`), `SetariJurnalView` (also hosted by `JurnalPage` and `LogViewer\LogViewerForm`) |
| `Setari\Dialogs\` | `AdobeGazduireForm`, `AdobeMesajeForm`, `ParolaAvansataForm` (opened only from `SetariAplicatieView`) |
| `Setari\PageRules\` | `RegulaPaginaForm`, `RegulaPaginaEditor`, `ElementPagina` (the FOREXE page-rule editor, opened only from `SetariPaginaView`) |

43 files moved, plain renames, no code touched. Who-uses-whom was read from a type-name graph (comments and strings
stripped); outside `Setari\` only the three units named above had users. The form resources keep their names
(checked in `obj`, both configurations). `KBot.App.vbproj` needed nothing (no `Setari` entries). Catalog: 21 `file` paths re-pointed.

## `Pages` dissolved (operator agreed, same day)

The folder made in 0086-04 held three pages with nothing in common, each used by one place only. Moved next to the one that opens it:
`IstoricVersiuniPage` and `JurnalPage` (opened only by `KbotForm.Pagini.vb`) to `KBotForm\Views\`; `AcasaPage` (only `AvacontMain`) to `Avacont`. 8 files, plain renames, no catalog entry pointed there. Build Debug and Release 0 / 0.

## Files touched

43 moved (+8 from `Pages`); edited `src/KBot.App/Config/mesaje_catalog.json`.

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj`, Debug AND Release: **0 warnings, 0 errors**. No test run (the test project does not compile, see 0086-01).

## Left unverified or deferred

- Visual Studio not looked at; the settings window was not opened.
- `AdobeMesajeForm` is also named by `HarnessTests\OldAdobeHostHarnessForm` (Debug test bench) — same project, so the move cost nothing.
- Older worklogs still name the old `Setari\` paths (history, not rewritten). Nothing committed (operator does git).
