# SLICE-0086-04 — `TempDeRezolvat` given its homes; the loose files of the `KBot.App` root into folders <a id="slice-0086-04"></a>

Operator request, 05.11.2026: «găsim fiecărui fișier din TempDeRezolvat un loc potrivit … și ștergem cele două moarte» and
«curățenie și în rădăcină». Continuation of [0086-03](SLICE-0086-03-kbotform-views-folder.md#slice-0086-03).

## What changed and why

**`TempDeRezolvat\` (19 files) is gone**, each file placed by what it is:

| Folder | Files |
|--------|-------|
| `Documents\Ddf\` | `DdfPdfGenerator`, `DdfPdfLocator`, `DdfSectionBInsert`, `DdfXfaModel`, `DdfXmlBuilder` |
| `Documents\Pdf\` | `PdfCache`, `PdfPrintTarget` |
| `Documents\Signing\` | `PdfSigningSession`, `PendingPdfUploads`, `SignatureRecords`, `SignedPdfFiles`, `SigningMessages`, `SigningRestore` |
| `Adobe\` | `AdobeUiPreference`, `AdobeAdvice` (from the root) |
| `LogViewer\` | `LogViewerForm` (+ Designer, resx), `LogViewerLauncher` (from the root) |
| **deleted** | `AntetHeaderText.vb` (named only in a comment), `Ddf\IDdfForexeUpload.vb` (named nowhere; also declared `DdfForexeUploadNeimplementat`, used nowhere). Both are tracked by git at `Views/…`, so they can be restored from history. |

**The root of `src\KBot.App`** now holds only `Program.vb`, `kbot.ico` and the project files:

| Folder | Files from the root |
|--------|---------------------|
| `Icons\` | `DdfIcons`, `FxIcons`, `PlatiIcons`, `ReceptiiIcons`, `RezervariIcons` |
| `Pages\` | `AcasaPage`, `IstoricVersiuniPage`, `JurnalPage` (pages hosted by the shells) |
| `Auth\` | `LoginForm` (next to `ForcedPasswordChangeForm`) |
| `Startup\` | `StartupLauncherForm`, `SingleInstance`, `TrayRequestHub`, `OneTimeActions`, `MessageErrorReporter` |
| `Avacont\` | `AvacontMain` (+ `.Sidebar`, `.Tutorial`, Designer, resx) — the shared Avacont shell, as the Avacont layout rule wants |
| `KBotForm\Views\` | `InternalInfoForm` (only `KbotForm` opens it) |

49 files moved. Plain renames, no code touched; the form resources keep their names (checked in `obj`, Release).
`KBot.App.vbproj`: the two `AvacontMain.*` nesting entries re-pathed (`Avacont\…`). Catalog: 9 more paths re-pointed.
**Left in place on purpose:** `kbot.ico` (the project's `ApplicationIcon` and the installer script
`tools\KBotInstaller\KBot.iss` both point at it) and `Program.vb` (`StartupObject`).

**Trap avoided:** `.gitignore` ignores any folder called `Logs/` (`[Ll]ogs/`), so a source folder with that name would have
been invisible to git. The log viewer lives in `LogViewer\`.

## Files touched

49 moved; 2 deleted; edited `src/KBot.App/KBot.App.vbproj`, `src/KBot.App/Config/mesaje_catalog.json`,
`src/KBot.App/KBotForm/KbotForm.vb` (remarks).

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj`, Debug AND Release: **0 warnings, 0 errors**. No test run (the test project does
not compile, see 0086-01).

## Left unverified or deferred

- Visual Studio not looked at.
- `KBot.App.vbproj.Backup.tmp` in the root is a leftover of Visual Studio; not touched (not mine).
- Other root folders were not touched: `DDF_EDIT`, `ORD_EDIT` (the editors, also opened by `AvacontMain`), `Director`, `Forexe`,
  `Setari`, `Update`, `Tutorial`, `Help`, `HelpContent`, `Chat`, `Config`, `Resources`, `HarnessTests`.
- Older worklogs still name the old paths (history, not rewritten). Nothing committed (operator does git).
