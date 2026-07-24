# SLICE-0020-04 — client: `KBotPaths` + DDF file browser + cross-linking

Pass 4 of slice 0020 (vederea DDF), per `PLAN_DdfView.md` §7. Adds the local-path config, the
`*.pdf` browser under it, and the tree-leaf → preview cross-link.

## What changed and why

- **`KBotPaths`** (`KBot.Common`): a JSON-backed config next to the executable
  (`<AppDir>\kbot_paths.json`), one property for now — `DdfPdfRoot`, default
  `C:\AVACONT\FOREXE\PDF\DDF\` (decision 13). Missing / empty / malformed → default + log,
  **never throws at startup**. `Load(dir)` is testable (bypasses the `Current` singleton).
  **This is not a revival of `AppConfig`.** `AppConfig` was retired because it held the server
  address, which stays hardcoded in `ApiOptions`; `KBotPaths` holds only local filesystem
  locations. The Config form that will edit it is a later slice, out of scope.
  `KBot.Common` `FileVersion` 1.0.0.0 → 1.1.0.0 (this project changed).
- **`DdfPdfLocator`** (pure, testable): the §2.5 path convention
  (`<root>\<partener|GENERAL>\DDF_NR_{CUAL}_REV_{NumarRev}_{Cod}.PDF`) and the enumeration.
  `Enumerate` walks the root recursively for `*.pdf` and keeps only names ending
  `_{CodAngajament}.PDF` — this picks up `GENERAL\` **and** every partener folder without
  hardcoding either, and excludes `.xml` siblings by construction (it only globs `*.pdf`).
  `PDF\ORD\` is never touched — it is outside `DdfPdfRoot`. Sorted by Modified descending.
  A missing root returns an empty list — no throw, no folder creation.
- **`DdfFileBrowser`** (`KBotDataView`): columns Folder / Nume fișier / CUAL / Rev. / Dimensiune /
  Modificat. A missing root shows a Romanian empty-state **naming the configured path**.
  Selecting a row raises `FileActivated(fullPath)`.
- **Cross-linking in `DdfView`** (§7): one `IDdfPreview` instance, **two entry points**.
  - Selecting a tree **leaf** computes the expected path via
    `DdfPdfLocator.ExpectedPath(KBotPaths.Current.DdfPdfRoot, _antet, NumarRev)` (this is where
    `_antet`'s `CUAL`/`PartAng`/`NumePartener`, loaded in pass 02, finally gets used) and hands
    it to the preview with `File.Exists` as the flag. A **root** click clears the preview.
  - Selecting a file **row** routes to the *same* preview and switches the horizontal sub-nav
    to the Vizualizare page — the single surface, reached from either page.
  - The browser reloads on each `SetContext`; both browser and preview clear on context clear.
- The pass-02 placeholder string on `pnlFisiere` ("va fi disponibilă într-o etapă următoare")
  is gone — the open item from that worklog is closed.

## Files touched

- `src/KBot.Common/KBotPaths.vb` — **new** (+ `KBotPathsDto`).
- `src/KBot.Common/KBot.Common.vbproj` — FileVersion bump.
- `src/KBot.App/Views/Ddf/DdfPdfLocator.vb` — **new** (+ `DdfPdfFile`).
- `src/KBot.App/Views/Ddf/DdfFileBrowser.vb` + `.Designer.vb` — **new**.
- `src/KBot.App/Views/DdfView.vb` — mounts the browser, `OnFileActivated`, `LinkPreviewLaFrunza`,
  loads the browser in `LoadAsync`, clears it in `ClearAll`, cascades the theme.
- `src/KBot.App/Views/DdfView.Designer.vb` — neutral empty-state string on `pnlFisiere`.
- `tests/KBot.Common.Tests/KBotPathsTests.vb` — **new**, 7 tests.
- `tests/KBot.App.Tests/DdfPdfLocatorTests.vb` — **new**, 10 tests.
- `tests/KBot.App.Tests/DdfFileBrowserTests.vb` — **new**, 5 headless STA tests.
- `tests/KBot.App.Tests/DdfViewTests.vb` — 2 cross-link tests added.

## Test results

- **.NET** — `dotnet build KBot.sln`: **0 errors, 0 BC warnings** (only pre-existing NU1701).
  `dotnet test KBot.sln`: **391 passed, 0 failed** (App 103, Api 61, Controls 134, Theming 27,
  Xfa 39, Domain 17, Common 14, LocalStore 1).
- A test caught a real WinForms subtlety: `Control.Visible` returns False when an ancestor is
  hidden, so the page-switch assertion only works after the view has loaded data (which makes
  `split` visible). Fixed the test, not the product.

## Anything left unverified or deferred

1. **NO VISUAL VERDICT.** Grid columns, the empty-state wording, the sub-nav page switch on file
   selection — all verified by headless tests and never seen on screen.
2. **`KBotPaths.Current` (the singleton) is not directly tested** — only `Load(dir)` is. `Current`
   just wraps `Load(AppContext.BaseDirectory)` behind a lock; testing it would need to write into
   the test host's own bin directory, which is not worth the fragility.
3. **`§8 item 5` (ElementFund auto-hide) stays closed** from 0020-02; nothing here reopened it.
4. **Cross-linking is wired but unproven against real files** — `ExpectedPath` and `Enumerate` are
   tested against temp trees, but no real `C:\AVACONT\FOREXE\PDF\DDF\` layout has been read, and
   the preview shown from a real generated PDF has never been seen (that also waits on pass 05).
5. **`GenerateRequested` is still stubbed** (pass 05).
