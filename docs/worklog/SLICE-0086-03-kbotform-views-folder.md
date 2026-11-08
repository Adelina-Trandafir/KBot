# SLICE-0086-03 — The views of KbotForm move to `KBotForm\Views\` <a id="slice-0086-03"></a>

Operator request, 05.11.2026: «un folder Views in KbotForm si sa pui acolo toate views din acest form (sunt destul de multe)».
Slice number: continuation of 0086 (the operator gave none).

## What changed and why

The 12 views the navigation creates (`KbotForm.CreateView`) and their files moved from `src\KBot.App\Views\` to
`src\KBot.App\KBotForm\Views\`:

- `SumarView`, `PlaceholderView`, `IstoricView` (+ `IstoricFilter`), `RezervariView` (+ `.Tutorial`), `ReceptiiView`,
  `PlatiView`, `ExtraseView`, `DdfView`, `OrdView`, `NoteCabView`, `BrowserView`, each with its `.Designer.vb` / `.resx`;
- `EFactura\` (`EFacturaView` + `Vanzare\FacturaPdfViewer`), whole;
- the contracts they share: `IAngajamentView`, `IViewRibbon`, `IReleasesDocument`.

39 files. Plain rename, no git, no code touched. VB does not put the folder in the resource name, so the `.resx` of the views
keep their names (checked in `obj`). `Config\mesaje_catalog.json`: the `file` of the 20 message entries of the moved files
re-pointed (JSON re-parsed). `KbotForm.vb` class remarks list the new folder.

**Second pass (same day).** The first pass moved only the 12 views and left the windows and editor pages the views open
(`ExtraseForm`, `GrupeForm`, `LogViewerForm`, `SumarPartnersForm`, the signing / PDF helpers, and the folders `Ddf`, `Ord`,
`NoteCab`, `Nomenclatoare`, `Extrase`, `Print`, `Adechit`, `Vercon`) in `Views\`. The operator asked why: for them everything
in `Views\` is a view of the form. Those 122 files moved too, sub-folders kept, no name clashes. 61 more catalog paths
re-pointed. Nothing outside the form depended on the old path (project file, tests and scripts checked).

**Third pass (same day) — only what the form uses stays.** The operator pointed out that not everything in `Views\` belongs to
the form («le separ — asta e ideea de bază») and asked for every moved file to be checked. Method: a graph of the type names
of every `.vb` in `src\` (comments and strings stripped), walked from the `KbotForm` files, through the windows and pages it
opens (not through `Program`, `AvacontMain`, the start window, the login window, nor the harness). Of the 83 units under
`KBotForm\Views\`, **80 are reached by `KbotForm`**; **3 are not and went back to `src\KBot.App\Views\`** (the root folder
exists again): `LogViewerForm` (+ Designer, resx; created by `Program` and `LogViewerLauncher`), `AntetHeaderText` (named only
in a comment, used by nobody) and `Ddf\IDdfForexeUpload` (named nowhere). Build **0 / 0**.

**Fourth pass (same day) — only what is UNIQUE to the form stays.** The operator: «lasa in kbotform DOAR ce e unic pe el, nu si
cele partajate». Rule: a unit stays in `KBotForm\Views\` only if every LIVE file that uses it is `KbotForm` itself or
something only `KbotForm` reaches; a file used by anything else (directly, or because a shared unit uses it) goes back to
`Views\` at the root. Method: type-name graph of `src\KBot.App\**\*.vb` — top-level types only (nested ones with generic
names such as `Rezultat` had produced false links), comments and strings stripped, `HarnessTests\` left out (a Debug bench is
not a user), dead code not counted as a user, then shrunk to a fixed point.

Result of the 80 units that were in `KBotForm\Views\`: **41 unique, 39 shared** (70 files moved back; catalog: 38 paths).
- **Shared directly (code checked):** `DdfView`, `DdfComanda` (`Director\DirectorForm`); `DdfPdfGenerator`, `DdfXmlBuilder`
  (its `DdfPdfMode`), `PdfSigningSession`, `ReaderHostPreview`, `PendingPdfUploads`, `SigningMessages`
  (`Director\DeSemnatRestransForm`, `Program`); `DdfFisierPreview`, `DdfPartnersView` (`DDF_EDIT`, which `AvacontMain`
  also opens); `Ord\OrdTextForm` (`ORD_EDIT`, also used by `PlanContForm` / `AvacontMain`); `ExtraseForm`,
  `Nomenclatoare\ParteneriForm`, `IViewRibbon` (`AvacontMain`, `PlanContForm`); `LogClearDialog` (`Setari`);
  `AdobeUiPreference` (`Setari`).
- **Shared only through those** (they ride with the shared unit that needs them): the rest of `Ddf\`, `IAngajamentView`,
  `IReleasesDocument`, `PdfCache`, `PdfPrintTarget`, `SigningRestore`, `SignedPdfFiles`, `SignatureRecords`, `Print\`,
  `Extrase\ExtrasePanel` / `ExtraseLayout`.
- **Unique:** `SumarView`, `IstoricView` (+ `IstoricFilter`), `RezervariView`, `ReceptiiView`, `PlatiView`, `ExtraseView`,
  `OrdView`, `NoteCabView`, `BrowserView`, `PlaceholderView`, `EFactura\`, `Ord\` (editor pages), `NoteCab\`,
  `Nomenclatoare\` (except `ParteneriForm`), `Adechit\`, `Vercon\`, `GrupeForm` / `GrupeUi`, `SumarPartnersForm`,
  `GraficRezervariForm`, `RezervariReparaForm`.
- Counts: `KBotForm\Views\` 86 files, `Views\` (root) 75 files = the 161 moved.

**Fifth pass (same day) — the root `Views\` cleaned.** The operator: shared ones in `Views\Comune`, what has no business in
`Views\` out into a «temp de rezolvat». The 75 files at the root were sorted by their real type (`Inherits` / declarations):
- **`Views\Comune\` (56 files):** the screens and their contracts — `DdfView`, `ExtraseForm`, `LogClearDialog`, `IAngajamentView`,
  `IReleasesDocument`, `IViewRibbon`; `Ddf\` (the seven pages, the two previews, `DdfComanda`, `DdfPageContext`, `IDdfPage`,
  `IDdfPreview`); `Extrase\` (`ExtrasePanel`, `ExtraseLayout`); `Nomenclatoare\ParteneriForm`; `Ord\OrdTextForm`; `Print\`.
- **`src\KBot.App\TempDeRezolvat\` (19 files, same relative layout):** not screens — `Ddf\` (`AdobeUiPreference`, `DdfPdfGenerator`,
  `DdfPdfLocator`, `DdfSectionBInsert`, `DdfXfaModel`, `DdfXmlBuilder`, `IDdfForexeUpload`), the signing / PDF helpers
  (`PdfCache`, `PdfPrintTarget`, `PdfSigningSession`, `PendingPdfUploads`, `SignatureRecords`, `SignedPdfFiles`,
  `SigningMessages`, `SigningRestore`), `LogViewerForm` (a window of the application, created by `Program`) and the dead
  `AntetHeaderText`. **To be given a real home** (services, a PDF / signing folder, deletion of what nobody uses).
- `Views\` now holds only `Comune\`. 38 catalog paths re-pointed; build **0 / 0**.
- Judgement calls, easy to flip: `DdfComanda`, `DdfPageContext` and `ExtraseLayout` went to `Comune` (they exist only to serve a
  screen); `LogClearDialog` stays in `Comune` (a dialog shared with `Setari`).
- **Noted, not touched:** `Forexe\IstoricIntervalForm` is the only other user of `IstoricView` and nothing opens it (dead
  code); `IstoricView` was treated as unique for that reason.

## Files touched

161 moved files (39 + 122) under `src/KBot.App/KBotForm/Views/`; edited `src/KBot.App/KBotForm/KbotForm.vb` (remarks),
`src/KBot.App/Config/mesaje_catalog.json`.

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: **0 warnings, 0 errors**. No test run (the test project does not compile, see 0086-01). The project file
needed no change (no entry pointed at `Views\`).

## Left unverified or deferred

- Visual Studio not looked at: the new folder, and whether the views' designers open from it.
- Older worklogs still name `src\KBot.App\Views\...` for these files (history, not rewritten).
- Nothing committed (operator does git).
