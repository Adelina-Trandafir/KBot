# SLICE-0020-01…05 — Istoricul vederii DDF

Reunire documentară D001-19. Corpurile etapelor păstrează contractele, deviațiile, rezultatele și restanțele la reperul lor; paragrafele sunt reașezate fără schimbarea conținutului. Paginile goale din 02 sunt completate în 03/04, stubul de generare în 05. Revizuirea din 24.07.2026 de la 03 înlocuiește fluxul inițial din 04: fișierul real merge în «Document», montat lazy; «Vizualizare» rămâne reconstrucția XFA. Amânarea semnării la 0021 este istorică (indexul global consemnează ulterior absorbția în 0078), fără declararea unor probe noi.

[0020-01](#slice-0020-01) · [0020-02](#slice-0020-02) · [0020-03](#slice-0020-03) · [0020-04](#slice-0020-04) · [0020-05](#slice-0020-05)

## SLICE-0020-01 — server + client: `GET /api/forexe/ddf` <a id="slice-0020-01"></a>

Pass 1 of slice 0020 (vederea DDF), per `PLAN_DdfView.md` §3–§4. Ships the endpoint, its host-only tests, the `KBot.Domain` POCOs and `GetDdfAsync` on the API client. **No UI yet** — `MainForm.CreateView("ddf")` still returns a `PlaceholderView`; that is pass 02.

> **Worklog naming.** The plan names a single `SLICE-0020-ddf-view.md`. `CODE_WORKFLOW.md` §3.2 requires multi-pass slices to use sub-numbered files (`SLICE-0007-01-…`), which is also what 0015 and 0017 did. Followed the standing rule, not the plan's filename.

### What changed and why

One endpoint, three arrays, one round trip: `{cod, antet[], revizii[], linii[]}`. The client filters locally (tree, grid, clsf combo) so a tree click never issues a request — decision 7.

- **`antet` is an array:** the composite MariaDB PK `(IDDF, CUAL)` and absent `CodAngajament` uniqueness (`000_DEMO.sql:161`) do not enforce the single header Access assumes. Count > 1 logs a warning; `DdfInfo.AntetDeLucru(iddfPreferat)` explicitly matches `AngajamentTreeInfo.IDDF`, otherwise picks the first.
- **`TotalRevizie = SUM(ValCur)`** in a correlated scalar subquery. Access labels one `SA.ValCur` as the total; `Show_Revizii` re-adds `"RC_" & IDREV` per line, displaying an arbitrary value, like the 0011-03 `aggRev` fan-out. `COALESCE(…, 0)` keeps revisions without section-A lines at 0, which the Access `INNER JOIN` drops.
- **`Clsf` needs none of the 0011-03 `IdClsfAcc` + `IdUnitate` treatment.** Confirmed from `routes/ddf/sync_acc_mdb.py:7-8` (`Access.IdClsfPY → MariaDB.IdClsf`) and the DDL constraint `FX_DDF_REV_SA_ibfk_4 FOREIGN KEY (IdClsf) REFERENCES Clasificatii (IDClsf)`: on this table `IdClsf` already holds the nomenclator **primary key**, the reverse of `FX_Indicatori`. Keying on a PK is unique by definition → no fan-out, so **no `IdUnitate` predicate**. The denormalised `FX_DDF_REV_SA.Clsf` is preferred, falling back to the nomenclator only when blank.
- **`ParametriiFund` is on `linii` from the start** (plan §8 bis asked for this explicitly): the grid never shows it (decision 4), but the pass-05 XML builder writes it into section A's `Cell4`. Adding it now avoids reopening the endpoint later.

#### Deviation from the plan: no join to the header

Plan §3 joins `FX_DDF_REV` to `FX_DDF`, but `INNER JOIN … ON IDDF` would multiply revisions by the composite-key `CUAL` rows. Instead, `revizii`/`linii` use `IN (SELECT …)`, guaranteeing one row per `IDREV`/`IdSecA` by query shape, without a mutable `GROUP BY`. This extends the 0011-03 lesson already applied to `TotalRevizie`.

`test_second_cual_row_does_not_fan_out_revisions` is the regression pin: it inserts a genuine second `CUAL` row and asserts `antet` grows to 2 while `revizii` stays at 3 and `linii` at 4.

#### Access defects reproduced *deliberately differently*

| Access | Here |
|---|---|
| `SA.ValCur AS TotalRevizie` (one arbitrary line) | real `SUM(ValCur)` per revision |
| month root value is the literal `0` | client will show the real sum (pass 02) |
| revision with no section-A line vanishes (`INNER JOIN`) | survives with total `0.0` |

### Files touched

**Server** `PYTHON/routes/forexe/ddf.py` — **new**. Three parameterised queries, `@require_session`, `ensure_ascii=False`, base from `g.session.db_name` (never a query parameter). `PYTHON/routes/forexe/__init__.py` — registers `ddf` on `forexe_bp`. `PYTHON/tests/test_forexe_ddf.py` — **new**, 17 host-only tests (skip cleanly off-host).

**Client** `src/KBot.Domain/DdfInfo.vb` — **new**: `DdfAntet`, `RevizieRow`, `LinieSaRow`, `DdfInfo`. Carries three derived members read out of the Access source, not guessed: `DdfAntet.FolderPdf` / `NormalizeazaNume` (the partener vs `GENERAL` convention, `\W+` → `_`), `RevizieRow.EtichetaRevizie` (§2.6: `Format(NumarRev,"@@@")` is a **text** format — right-align in 3 chars padded with **spaces**, so `PadLeft(3)`, never `D3`/`000`), and `AntetDeLucru`. `src/KBot.Api/UpsertAngajamenteRequest.vb` — wire DTOs `GetDdfResponse` / `GetDdfAntetRow` / `GetDdfRevizieRow` / `GetDdfLinieRow` (snake_case stops at the wire). `src/KBot.Api/IApiClient.vb`, `src/KBot.Api/ApiClient.vb` — `GetDdfAsync`. `tests/KBot.App.Tests/{Plati,Receptii,Rezervari,Sumar}ViewTests.vb` — the four `FakeApiClient` stubs gain `GetDdfAsync` (interface addition; they throw `NotSupportedException` as before). `tests/KBot.Api.Tests/ApiClientTests.vb` — 8 tests. `tests/KBot.Domain.Tests/DdfInfoTests.vb` — **new**, 13 tests on the derived members.

### Test results

- **.NET** — `dotnet build KBot.sln`: **0 errors**, 16 warnings, all pre-existing `NU1701` (iTextSharp / BouncyCastle are .NET Framework packages on `net8.0-windows`, documented in 0019). `dotnet test KBot.sln`: **334 passed, 0 failed** across 8 projects (Api 61, App 50, Controls 134, Theming 27, Xfa 39, Domain 17, Common 7, LocalStore 1).
- **Python (offline, dev station)** — `pytest tests/`: **75 passed, 14 skipped, 0 failed**. `tests/test_forexe_ddf.py` is among the skips: it is host-only by design (`main` needs `config.py`), and skipping is the correct offline outcome, **not** evidence it works.

### Anything left unverified or deferred

1. **No real-database run:** every DDF test was observed only skipping host-only execution. SQL follows the verified DDL/Access export, but no row passed through it; the standing caveat from 0008/0011/0014/0015/0017 remains.
2. **Duplicate-header count unknown (§3.5):** `test_report_duplicate_cod_angajament_count` only prints how many `CodAngajament` values have multiple `FX_DDF` rows; it never fails and has not run. Host execution must record the count to resolve §2.7. Meanwhile `antet` remains an array with client logging.
3. **`data_creare` / `data_def` are truncated to a date.** They are `datetime` in MariaDB; the view groups and displays by day. If a later pass needs the time component, `_iso` must change.
4. **`Program` ambiguity is untouched** (§2.9): `FX_DDF.Program` is carried on `antet`, but which of it and the session's program code the Access XML builder actually wins with is a pass-05 question. Nothing here depends on the answer.
5. **`Tip` and `Semnatura`** are carried and displayed nowhere, per §8 item 3. `Semnatura` becomes live only if pass 06 goes ahead.

---

## SLICE-0020-02 — client: `DdfView` <a id="slice-0020-02"></a>

Pass 2 of slice 0020 (vederea DDF), per `PLAN_DdfView.md` §5. The **fifth real view** — after Sumar (0011), Rezervări (0014), Recepții (0015), Plăți (0017) — replacing `MainForm.CreateView("ddf")` → `PlaceholderView`.

### What changed and why

A master/detail view: an `AdvancedTreeControl` of revisions on the left, and on the right a **horizontal** `KBotNavList` (decision 8) over three lazily-shown pages. Read-only. One `GetDdfAsync` per `SetContext` through `WithReauth(Of DdfInfo)` with the `_requestedCod` stale-guard; **every tree click and combo change filters data already in memory — no request** (decision 7).

- **Tree — two levels.** Month roots (`LA_{yyyy}_{M}`, grouped on year+month of `DataRev`, chronological, **expanded**) → revision leaves (`RC_{IDREV}`, caption `{NumarRev.PadLeft(3)} - {dd.MM.yyyy}`, value `TotalRevizie`). Leaf icon: `Incarcat` → up, else `Preluat` → down, else neutral (Access `REV_SUS`/`REV_JOS`/`REV_NOT`). Leaf tooltip = `Desc_Scurta`. Each node's `Tag` carries a `DdfNodeRows` (its section-A lines + whether it is a root + the leaf's `RevizieRow`, which passes 03/04 need for the PDF path).
- **Pages — visibility, not a `TabControl`.** `pnlValori` / `pnlPreview` / `pnlFisiere`, exactly one `Visible`, the same lazy pattern `MainForm` uses for views. Preview and Fișiere are empty shells with a Romanian empty state; passes 03 and 04 fill them.
- **Grid.** `Clsf`, `ElementFund` (the only `AutoHide` column), `Data reviziei` (**root level only**), `ValPrec`, `ValCur`, `ValTot`. Totals row on with **Sum on `ValCur` only** (decision 5). `Clasificatii.Denumire` is not shown (decision 4). Sorting: root → `Clsf, DataRev`; leaf → `Clsf`.
- **Clsf combo.** Rebuilt from the clicked node's distinct `Clsf`, sorted, with `< Arată toate clasificațiile >` first and selected — **unconditionally on every node click** (decision 6), never trying to preserve the previous selection. `_suppressComboEvent` stops the rebuild from re-filtering mid-flight.

#### Deviations from Access (deliberate, mirroring the pass-01 endpoint)

| Access | Here |
|---|---|
| month root value is the literal `0` (`"…~~~0"` to `AddTree_Root`) | the real sum of its leaves' `TotalRevizie` |
| `cRoot.foreColor = cNode.foreColor` — the root inherits **whichever leaf was processed last** | a root is red **only when its own total is negative** |
| a revision with no section-A line vanishes (`INNER JOIN`) | it stays, with total `0,00` |

Each has a test pinning it.

#### Two plan inaccuracies found by reading the real files

1. **§5 says the leaf icons go "through `FxIcons`". They cannot.** `FxIcons` loads **embedded image resources** keyed on an angajament's `Stare` — it has no palette-tinted GDI drawing. The pattern §5 actually describes ("GDI-drawn and palette-tinted, as in Rezervări, no binary resources") lives in `RezervariIcons` / `PlatiIcons` / `ReceptiiIcons`, **one icon class per view**. Followed the real convention: added `DdfIcons`.
2. **§8 item 5 — `AutoHide` on a non-rightmost column is fine; the column does not need moving.** `PerformAutoHide` (`KBotDataView.AutoSize.vb:176`) iterates right-to-left but `Continue For`s past any column without `AutoHide`, so a single auto-hiding column is found wherever it sits. The only protected column is the fill target, which under `ColumnFillMode.LastColumn` is `ValTot` — not `ElementFund`. **Open item 5 is closed.**

`KBotNavList`'s real API was confirmed from the file, as §0 demanded: `Orientation As KBotNavOrientation` with `.Horizontal` (0018 added it).

### Files touched

`src/KBot.App/Views/DdfView.Designer.vb` — **new**. Every control declared here (house rule); children added in **reverse dock order** (Fill first, Top last) inside each card panel. `src/KBot.App/Views/DdfView.vb` — **new**. Also declares `DdfNodeRows` (the node payload POCO). `src/KBot.App/DdfIcons.vb` — **new**. GDI, palette-tinted, cached on (state, colour, size). `src/KBot.App/MainForm.vb` — `Case "ddf"` now returns `New DdfView(...)`. `tests/KBot.App.Tests/DdfViewTests.vb` — **new**, 19 headless STA tests.

### Test results

- **.NET** — `dotnet build KBot.sln`: **0 errors, 0 BC warnings** (only the pre-existing NU1701 iTextSharp/BouncyCastle ones). `dotnet test KBot.sln`: **355 passed, 0 failed** (App 69, Api 61, Controls 134, Theming 27, Xfa 39, Domain 17, Common 7, LocalStore 1).
- Python untouched this pass (still 75 passed / 14 skipped from 0020-01).

**A test-harness bug the tests caught:** `FindControl(Of ComboBox)` walks depth-first and returned `KBotDataView`'s own floating **editor** ComboBox instead of `cboClsf`, so two combo tests failed against the wrong control. Fixed by looking the filter up **by name** (`FindByName`, the helper `PlatiViewTests` already carries for this reason). Product code was not at fault.

### Anything left unverified or deferred

1. **NO VISUAL VERDICT.** Same standing caveat as every view since 0010: verified by compilation and headless tests only. The view has **never been seen on screen** — layout, the horizontal nav's measured widths, column widths, and the auto-hide behaviour under a real width are all unobserved. `AutoHide` is argued from reading `PerformAutoHide`, not from watching a column disappear.
2. **Never run against a real database** — inherits 0020-01's caveat; the view has only ever seen hand-built `DdfInfo` objects.
3. **`_antet` is loaded and held but not yet used.** It carries `CUAL` / `PartAng` / `NumePartener` for the PDF path; passes 03/04 consume it. Deliberate, not dead code.
4. **The multi-header warning path is untested.** When `Antet.Count > 1` the view logs via `GlobalErrorLog` and picks by `_preferredIddf`; no test exercises it because §3 item 5 has not yet told us whether that case exists in real data.
5. **`pnlPreview` / `pnlFisiere` are empty shells** with placeholder Romanian text. Their labels are worded as an empty state, but the Fișiere one still says the list "va fi disponibilă într-o etapă următoare" — replace that string in pass 04 rather than leaving it shipped.
6. **Sub-nav gating is not wired to anything.** All three pages are always reachable; nothing disables Vizualizare/Fișiere when there is no document. Passes 03/04 decide that.

---

## SLICE-0020-03 — client: two DDF preview surfaces <a id="slice-0020-03"></a>

Pass 3 of slice 0020 (vederea DDF), per `PLAN_DdfView.md` §6 and §11. **Both** preview implementations are built here; neither is dead code that "will be finished later". Selection is one compile-time constant in `DdfPreviewFactory` — deliberately **no runtime switch**.

### What changed and why

- **`IDdfPreview`** (contract): `Surface`, `ShowDocument(pdfPath, exists)`, `Clear()`, `GenerateRequested`. `DdfView` knows nothing about Adobe, XFA, or window handles.
- **`DdfPreviewMode` + `DdfPreviewFactory`**: `Private Const Mode = DdfPreviewMode.XfaXml` (default) and a `Select Case`. Change the constant, rebuild — §11 to the letter.
- **`XfaXmlPreview`** (default, option C): renders from the XFA XML — sibling `.xml` first (`Path.ChangeExtension(pdf, ".xml")`), else `PdfXmlExtractor.TryExtract` on the PDF (which already handles raw and `FlateDecode`). Three states: content (a header of antet fields, a `KBotDataView` of section-A lines, a note), the missing-document surface, and a message. Not pixel-identical to the printed PDF — the accepted trade (§6), the only option that needs no second process.
- **`DdfXfaModel` + `DdfXfaParser`** (pure, testable): parse the `form1` tree namespace-agnostically (by local-name), so it reads both the bare `form1` the builder emits and the `xdp:xdp/xfa:data/form1` embedded in a PDF. It skips the dummy first `Row1` exactly as the Access machete's JS does. **This is the one part of the pass that can be unit-tested.**
- **`ReaderHostPreview`** (backup, option A): `Process.Start` the PDF, poll `EnumWindows` for a top-level Adobe window (class contains `Acrobat`) whose title contains the file's base name, bounded by an 8 s timeout, then `SetParent` it into a host panel with `WS_CAPTION`/ `WS_THICKFRAME`/`WS_POPUP` cleared and `WS_CHILD` set, re-laid on resize. On dispose and on every document change it restores the original parent/style and closes the process it started, so no orphaned Reader is left. If the window is not found in time it falls back to a plain `Process.Start` and says so. `GetWindowLongPtr`/`SetWindowLongPtr` are dispatched 32/64-bit-safe.
- **`DdfView`** mounts `DdfPreviewFactory.Create()` into `pnlPreview`, subscribes `GenerateRequested`, cascades the theme to the surface, and `Clear()`s the preview on a root click / on context clear.

#### The DDF element set was derived from the module source, not the repo sample

**Plan discrepancy (§0/§8 bis/DoD):** `Surse/SURSA_XFA_WRITTER/XSD_XML/ddf_demo.xml` is byte-identical to `ord_demo.xml` (MD5 `9e5fe957…`), hence an ORD copy. The DDF elements were derived from `mdl_FX_DDF_PDF.GenereazaXML_PentruPython`, the source for pass 05: `SubformAntet` → `DenInstPb`/`cif`/`NrUnicInreg`/`SubtitluDF`/`DataRevizuirii`/`Revizuirea`; `SubformSectiuneaA` → `Subform123` + `Subform4/Table1/Row1`, with `Cell1`=ElementFund, `Cell3`=SS+clsf, `Cell5`=ValPrec, `Cell6`=ValCur. **Open thread:** no genuine Access DDF XML exists in the repo for the DoD comparison; capture from a live run is required.

### Files touched

`src/KBot.App/Views/Ddf/IDdfPreview.vb` — contract, `DdfPreviewMode`, `DdfPreviewFactory`. `src/KBot.App/Views/Ddf/DdfXfaModel.vb` — `DdfXfaModel`, `DdfXfaLinie`, `DdfXfaParser`. `src/KBot.App/Views/Ddf/XfaXmlPreview.vb` + `.Designer.vb` — the default surface. `src/KBot.App/Views/Ddf/ReaderHostPreview.vb` + `.Designer.vb` — the backup surface. `src/KBot.App/Views/DdfView.vb` — mounts the preview, `GenerateRequested`, theme cascade, `Clear()` on root/clear. `tests/KBot.App.Tests/DdfXfaParserTests.vb` — **new**, 9 pure-logic tests. `tests/KBot.App.Tests/XfaXmlPreviewTests.vb` — **new**, 7 headless STA state-machine tests (incl. the real render-from-sibling-`.xml` path via a temp file).

### Test results

- **.NET** — `dotnet build KBot.sln`: **0 errors, 0 BC warnings** (only pre-existing NU1701). `dotnet test KBot.sln`: **371 passed, 0 failed** (App 85, Api 61, Controls 134, Theming 27, Xfa 39, Domain 17, Common 7, LocalStore 1).
- A test caught its own bug: `btn.PerformClick()` needs the button `CanSelect` (visible+enabled), so the test now switches the preview to the missing-document state first. Product code fine.

### Anything left unverified or deferred

1. **No visual verdict:** `ReaderHostPreview` window hosting (`EnumWindows`, `SetParent`, styles, resize, detach) has never run and cannot be exercised headless or without Adobe. Defensive implementation does not prove operation; this remains the least-verified part.
2. **No genuine DDF sample** as documented above: `XfaXmlPreview` used hand-built `form1` from the module source, not Access output. The structural DoD comparison remains open pending a live artifact.
3. **Cross-linking is pass 04.** Selecting a leaf does **not** yet call `ShowDocument` — that needs `KBotPaths.DdfPdfRoot` + `CUAL`/`PartAng` to compute the §2.5 path, which pass 04 adds. Pass 03 only `Clear()`s the preview on a root click. The preview is therefore inert until 04.
4. **`GenerateRequested` is stubbed.** `DdfView.OnGenerateRequested` logs a `NotImplementedException` to `GlobalErrorLog`; pass 05 replaces the body with the real `XfaWriter.Genereaza` call on a background thread.
5. **`ReaderHostPreview` keyboard/focus across the process boundary** is documented as not-native and deliberately not fixed (§6).
6. **`XfaXmlPreview` header/nota styling** is functional but unseen; the `tblHeader` runtime layout has no visual verdict.

### Revizuire operator (2026-07-24) — a fourth sub-nav page: «Document» (PDF real)

Two operator requests on the DDF view:

1. **Selecting a file in «Fișiere» no longer reverts to «Vizualizare».** That page is the XFA-XML *reconstruction*, not the chosen file — jumping there on a file click was confusing.
2. **A dedicated tab shows the actual PDF.** Added a fourth sub-nav page **«Document»** (order: Valori · Vizualizare · Document · Fișiere) hosting a **`ReaderHostPreview`** instance (`_pdfPreview`) — always the real-PDF surface, independent of the compile-time `DdfPreviewFactory` (which still drives «Vizualizare» = `XfaXmlPreview`). So both preview surfaces now coexist as separate tabs, neither dead code.

Flow: `OnFileActivated` routes the real PDF to «Document» and switches to it (not «Vizualizare»); a leaf click in the tree sets the «Document» target too. **Lazy by design** — `ShowPdfSurface` mounts the PDF (and possibly launches Adobe) **only when «Document» is the visible page**, guarded by `_pdfShownPath` so switching pages doesn't re-embed the same file. `SetPdfTarget`/`ClearPdfTarget` carry the current path + on-disk existence; generation forces a re-mount (existence changed). `ClearAll`, root-node clicks and the theme cascade all reach `_pdfPreview`.

Tests: `View_MountsFileBrowser_AndBothPreviews` (now also asserts a `ReaderHostPreview` is mounted) and `FileActivated_RoutesToDocumentPage_NotVizualizare` (was `…RoutesToPreview_AndSwitchesToVizualizarePage`). A missing file shows `ReaderHostPreview`'s "document lipsă" state without launching Adobe, so the STA tests stay headless-safe. Full .NET suite 436 green.

⚠️ **Still no visual/PDF verdict** — the `ReaderHostPreview` Win32 hosting path (`EnumWindows`/ `SetParent`/style-cleanup) remains the least-verified code in the whole feature and has never run; if Adobe is not the default `.pdf` handler it falls back to opening the file externally.

---

## SLICE-0020-04 — client: `KBotPaths` + DDF file browser + cross-linking <a id="slice-0020-04"></a>

Pass 4 of slice 0020 (vederea DDF), per `PLAN_DdfView.md` §7. Adds the local-path config, the `*.pdf` browser under it, and the tree-leaf → preview cross-link.

### What changed and why

- **`KBotPaths`** (`KBot.Common`): a JSON-backed config next to the executable (`<AppDir>\kbot_paths.json`), one property for now — `DdfPdfRoot`, default `C:\AVACONT\FOREXE\PDF\DDF\` (decision 13). Missing / empty / malformed → default + log, **never throws at startup**. `Load(dir)` is testable (bypasses the `Current` singleton). **This is not a revival of `AppConfig`.** `AppConfig` was retired because it held the server address, which stays hardcoded in `ApiOptions`; `KBotPaths` holds only local filesystem locations. The Config form that will edit it is a later slice, out of scope. `KBot.Common` `FileVersion` 1.0.0.0 → 1.1.0.0 (this project changed).
- **`DdfPdfLocator`** (pure, testable): the §2.5 path convention (`<root>\<partener|GENERAL>\DDF_NR_{CUAL}_REV_{NumarRev}_{Cod}.PDF`) and the enumeration. `Enumerate` walks the root recursively for `*.pdf` and keeps only names ending `_{CodAngajament}.PDF` — this picks up `GENERAL\` **and** every partener folder without hardcoding either, and excludes `.xml` siblings by construction (it only globs `*.pdf`). `PDF\ORD\` is never touched — it is outside `DdfPdfRoot`. Sorted by Modified descending. A missing root returns an empty list — no throw, no folder creation.
- **`DdfFileBrowser`** (`KBotDataView`): columns Folder / Nume fișier / CUAL / Rev. / Dimensiune / Modificat. A missing root shows a Romanian empty-state **naming the configured path**. Selecting a row raises `FileActivated(fullPath)`.
- **Cross-linking in `DdfView`** (§7): one `IDdfPreview` instance, **two entry points**.
  - Selecting a tree **leaf** computes the expected path via `DdfPdfLocator.ExpectedPath(KBotPaths.Current.DdfPdfRoot, _antet, NumarRev)` (this is where `_antet`'s `CUAL`/`PartAng`/`NumePartener`, loaded in pass 02, finally gets used) and hands it to the preview with `File.Exists` as the flag. A **root** click clears the preview.
  - Selecting a file **row** routes to the *same* preview and switches the horizontal sub-nav to the Vizualizare page — the single surface, reached from either page.
  - The browser reloads on each `SetContext`; both browser and preview clear on context clear.
- The pass-02 placeholder string on `pnlFisiere` ("va fi disponibilă într-o etapă următoare") is gone — the open item from that worklog is closed.

### Files touched

- `src/KBot.Common/KBotPaths.vb` — **new** (+ `KBotPathsDto`).
- `src/KBot.Common/KBot.Common.vbproj` — FileVersion bump.
- `src/KBot.App/Views/Ddf/DdfPdfLocator.vb` — **new** (+ `DdfPdfFile`).
- `src/KBot.App/Views/Ddf/DdfFileBrowser.vb` + `.Designer.vb` — **new**.
- `src/KBot.App/Views/DdfView.vb` — mounts the browser, `OnFileActivated`, `LinkPreviewLaFrunza`, loads the browser in `LoadAsync`, clears it in `ClearAll`, cascades the theme.
- `src/KBot.App/Views/DdfView.Designer.vb` — neutral empty-state string on `pnlFisiere`.
- `tests/KBot.Common.Tests/KBotPathsTests.vb` — **new**, 7 tests.
- `tests/KBot.App.Tests/DdfPdfLocatorTests.vb` — **new**, 10 tests.
- `tests/KBot.App.Tests/DdfFileBrowserTests.vb` — **new**, 5 headless STA tests.
- `tests/KBot.App.Tests/DdfViewTests.vb` — 2 cross-link tests added.

### Test results

- **.NET** — `dotnet build KBot.sln`: **0 errors, 0 BC warnings** (only pre-existing NU1701). `dotnet test KBot.sln`: **391 passed, 0 failed** (App 103, Api 61, Controls 134, Theming 27, Xfa 39, Domain 17, Common 14, LocalStore 1).
- A test caught a real WinForms subtlety: `Control.Visible` returns False when an ancestor is hidden, so the page-switch assertion only works after the view has loaded data (which makes `split` visible). Fixed the test, not the product.

### Anything left unverified or deferred

1. **NO VISUAL VERDICT.** Grid columns, the empty-state wording, the sub-nav page switch on file selection — all verified by headless tests and never seen on screen.
2. **`KBotPaths.Current` (the singleton) is not directly tested** — only `Load(dir)` is. `Current` just wraps `Load(AppContext.BaseDirectory)` behind a lock; testing it would need to write into the test host's own bin directory, which is not worth the fragility.
3. **`§8 item 5` (ElementFund auto-hide) stays closed** from 0020-02; nothing here reopened it.
4. **Cross-linking is wired but unproven against real files** — `ExpectedPath` and `Enumerate` are tested against temp trees, but no real `C:\AVACONT\FOREXE\PDF\DDF\` layout has been read, and the preview shown from a real generated PDF has never been seen (that also waits on pass 05).
5. **`GenerateRequested` is still stubbed** (pass 05).

---

## SLICE-0020-05 — DDF PDF generation (un-gated parts) <a id="slice-0020-05"></a>

Pass 5 of slice 0020 (vederea DDF), per `PLAN_DdfView.md` §8 bis. **Scope decision (operator):** build the parts that do not depend on the live machete download; leave step 05-00's live verification and the real-PDF verdict for a host run. **Pass 06 (signing) deferred to slice 0021** (operator decision).

### What changed and why

#### Step 05-00 (client side)
- `src/KBot.Xfa/Config.vb`: `BASE_URL` `http://adcredit.avatarsoft.ro:5008/api/` → `https://kbot.avatarsoft.ro/api/`. Corrected the stale comment (the machete endpoint does **not** live on a separate "old server" — `mfp_bp` is registered in the *same* Flask app, confirmed at `main.py:52`; the two hostnames are two front doors onto one app). `FileVersion` 1.1.0.0 → 1.2.0.0. The `X-API-KEY` **stays** — a deliberate deferral, logged as an open thread in STATUS (removing it means `@require_session` on the two `mfp` routes + a bearer token into `TemplateDownloader`, which also touches ORD).
- **Server side confirmed present (offline):** `routes/mfp.py:177` serves `/api/mfp/template_ddf` for **both GET and HEAD** under `@require_api_key`, from the server's own cache. So `TemplateDownloader`'s HEAD-then-GET is supported. **The live GET/HEAD over 443 (and whether nginx routes `/api/mfp/*` on that vhost) is NOT verified here — that is step 05-00's host run.**

#### Endpoint extension (one route, `GET /api/forexe/ddf`)
- `linii` gains `ss` — `COALESCE(NULLIF(SA.SS,''), Clasificatii.SS by IDClsf)`. The XML builder needs it for `Cell3` (form1) and `codSSI` (NOTAFD). **Finding:** MariaDB `Clasificatii` has **no `CodSSI`** column (the VBA reads Access `Clasificatii.CodSSI`) — it has the generated `SS` (Sector+Sursa), which is the analog. One `ss` field serves both the form1 and NOTAFD needs.
- `sectiuneb` (FX_DDF_REV_SB) and `atasamente` (FX_DDF_REV_ATT) arrays, **opt-in behind a single `?pentru_generare=1` flag**. Both are generation-only (§2.8: the view must not carry data it never shows). **Deviation from the plan**, which proposed a separate `?atasamente=1` for attachments only — one flag is cleaner and avoids a half-loaded generation payload; documented in the route.

#### The XML builder — `DdfXmlBuilder` (pure, the crown jewel of the pass)
A faithful port of the three module functions, generating **per revision** (as the Access `tmpFX_*` tables hold one revision):
- `BuildFormXml` ← `GenereazaXML_PentruPython`: `form1` → `SubformAntet`, `SubformSectiuneaA` (`Subform123` + `Subform4/Table1` with the **dummy first `Row1`** + real rows `Cell1..6`, `Subform5`), `SubformSectiuneaB/Table3` (dummy row + rows `Cell1..6,8,9` — **`Cell7` skipped**).
- `BuildNotafdXml` ← `GenereazaXML_NOTAFD`: the namespaced `NOTAFD` with `sectiuneaA` (`rowT_ang_pl_val`) and `sectiuneaB` (`rowT_ang_ctrl_ang`), sums **computed** (anterior+influență).
- `InsertAttachments` ← `InsereazaAtasamente`: the `<Attachments>` node with `NOTAFD.xml` (base64) + the already-base64 `FX_DDF_REV_ATT` rows.

Fidelity details ported and pinned by tests: `ToXmlNum` (comma→dot, no forced decimals), `AdaugaSubnod`'s empty-element omission, the dummy-row skip, `Int()` = **floor** (not toward zero), the `Left(…,N)` field widths, and §2.9 (program from the session global, not `FX_DDF.Program`).

**Two deliberate deviations from a literal VBA port, both documented in the code:**
1. **NOTAFD elements are namespace-qualified.** The VBA used `createElement` (no namespace) for children, producing elements in *no* namespace — **invalid** against `notafd_v0.xsd`, which is `elementFormDefault="qualified"`. The port qualifies every element so the output validates. (Confirmed against the XSD, not guessed.)
2. **NOTAFD base64 is UTF-8, not ANSI.** The VBA's `EncodeBase64` used `StrConv(…, vbFromUnicode)` = system-ACP bytes. Without a real DDF `.xml` to compare (the repo's `ddf_demo.xml` is actually ORD — open thread), the correct codepage can't be decided; UTF-8 is lossless for Romanian text whereas ANSI truncates out-of-codepage characters. **Flagged as an unverified fidelity risk.**

#### The generation flow — `DdfView.OnGenerateRequested`
`Async Sub` (UI boundary: log + swallow): fetch `GetDdfAsync(cod, pentruGenerare:=True)` → filter to the selected revision's rows → `DdfXmlBuilder.BuildComplete` → write the sibling `.xml` under the partener/`GENERAL` folder (created on demand) → `Await Task.Run(XfaWriter.Genereaza(...))` on a background thread → refresh browser + preview from disk. **No write-back to the DB** (§2.4 — the four columns don't exist; existence is the disk scan). Guarded against double-invocation. `DdfView` now takes an optional `SessionContext` (for the unit globals); `MainForm` passes `_session`.

### Files touched

- `src/KBot.Xfa/Config.vb`, `src/KBot.Xfa/KBot.Xfa.vbproj` (FileVersion).
- `PYTHON/routes/forexe/ddf.py` — `ss` on linii, `sectiuneb`/`atasamente` opt-in, `_truthy`.
- `PYTHON/tests/test_forexe_ddf.py` — updated key-sets + 5 new tests (ss, flag on/off, sectiuneb, atasamente).
- `src/KBot.Domain/DdfInfo.vb` — `LinieSaRow.SS`, `SectiuneBRow`, `AtasamentRow`, `DdfInfo` lists.
- `src/KBot.Api/UpsertAngajamenteRequest.vb`, `IApiClient.vb`, `ApiClient.vb` — wire DTOs + `pentruGenerare` flag + mapping.
- `src/KBot.App/Views/Ddf/DdfXmlBuilder.vb` — **new**.
- `src/KBot.App/Views/DdfView.vb` — generation flow; `src/KBot.App/MainForm.vb` — passes `_session`.
- Test fakes in the four other `*ViewTests` + `DdfViewTests` updated for the new signature.
- `tests/KBot.App.Tests/DdfXmlBuilderTests.vb` — **new**, 20 structural tests.
- `tests/KBot.Api.Tests/ApiClientTests.vb` — 2 generation-flag tests.

### Test results

- **.NET** — `dotnet build KBot.sln`: **0 errors, 0 BC warnings** (only pre-existing NU1701). `dotnet test KBot.sln`: **415 passed, 0 failed** (App 120, Api 63, Controls 134, Theming 27, Xfa 39, Domain 17, Common 14, LocalStore 1).
- **Python** — `pytest tests/`: **75 passed, 14 skipped, 0 failed** (the DDF tests skip off-host).

### Anything left unverified or deferred

1. **Step 05-00 live verification NOT done** — `GET`/`HEAD` on `https://kbot.avatarsoft.ro/api/mfp/template_ddf` with the `X-API-KEY`, over 443. The route and HEAD support exist in the Flask app (offline-confirmed), but whether nginx routes `/api/mfp/*` on the 443 vhost and a template sits in the server cache is unverified. **Run this on the host before trusting generation** — if the machete can't be fetched, `XfaWriter.Genereaza` fails at `TemplateDownloader`.
2. **No real PDF has been produced or opened.** `XfaWriter.Genereaza` is wired and called on a background thread, but it needs the machete + iTextSharp against a real template; the "Adobe Reader opens the generated PDF correctly" verdict (0019 open items a/c, and this slice's DoD) is **still open**.
3. **No structural diff against a known-good Access `.xml`** — impossible from the repo (the DDF sample is really ORD; open thread). The builder is pinned by 20 structural tests against the *module source*, not against a real artifact. Capture a real Access-written DDF `.xml` on the host and diff it.
4. **NOTAFD base64 ANSI-vs-UTF-8** (see above) — unverified fidelity risk.
5. **The generation UI has no busy bar.** The plan wanted "the busy bar up"; `DdfView` has no handle to the shell's busy bar, so it only guards against re-entry (`_generating`). A visible progress indication is a follow-up.
6. **Pass 06 (signing) deferred to slice 0021** (operator decision). `Semnatura` stays carried but unused; the write-back route is not built.

### Historical signing follow-up recorded on 24.07.2026

The retired migration summary made the deferred 0021 contract explicit: call XfaWriter.GenereazaSiSemneaza on a background thread, add write-back through UPDATE FX_DDF_REV.Semnatura, and never sign while ReaderHostPreview still holds a window. Semnatura was carried but unused at that checkpoint. This preserves the original deferral; it does not assert the present implementation or authorize running SQL.

## Repere unice din registrul 0020

MD5-ul complet consemnat pentru cele două mostre identice DDF/ORD: `9e5fe957751aca9630a71222dd974dda`. Acesta este un reper istoric preluat, nu o comparație executată acum asupra exporturilor. Focusul numea DDF «sixth real view», în timp ce jurnalul 02 spune «fifth real view»; ambele formulări istorice rămân delimitate, fără recenzarea ordinii implementărilor. Același focus rezuma «0 warnings», dar jurnalele precizează NU1701 preexistente și zero BC: probele detaliate de etapă prevalează. Numărul 17 de teste host-only din focus este cel inițial, anterior celor cinci adăugate în 05. Eliminarea `X-API-KEY` de pe cele două rute mfp, prin `@require_session` și bearer în `TemplateDownloader`, este o amânare deliberată («decide when, not whether»), cu impact și asupra ORD.
