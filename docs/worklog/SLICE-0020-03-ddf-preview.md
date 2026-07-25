# SLICE-0020-03 — client: two DDF preview surfaces

Pass 3 of slice 0020 (vederea DDF), per `PLAN_DdfView.md` §6 and §11. **Both** preview
implementations are built here; neither is dead code that "will be finished later". Selection is
one compile-time constant in `DdfPreviewFactory` — deliberately **no runtime switch**.

## What changed and why

- **`IDdfPreview`** (contract): `Surface`, `ShowDocument(pdfPath, exists)`, `Clear()`,
  `GenerateRequested`. `DdfView` knows nothing about Adobe, XFA, or window handles.
- **`DdfPreviewMode` + `DdfPreviewFactory`**: `Private Const Mode = DdfPreviewMode.XfaXml`
  (default) and a `Select Case`. Change the constant, rebuild — §11 to the letter.
- **`XfaXmlPreview`** (default, option C): renders from the XFA XML — sibling `.xml` first
  (`Path.ChangeExtension(pdf, ".xml")`), else `PdfXmlExtractor.TryExtract` on the PDF (which
  already handles raw and `FlateDecode`). Three states: content (a header of antet fields, a
  `KBotDataView` of section-A lines, a note), the missing-document surface, and a message.
  Not pixel-identical to the printed PDF — the accepted trade (§6), the only option that needs
  no second process.
- **`DdfXfaModel` + `DdfXfaParser`** (pure, testable): parse the `form1` tree namespace-agnostically
  (by local-name), so it reads both the bare `form1` the builder emits and the
  `xdp:xdp/xfa:data/form1` embedded in a PDF. It skips the dummy first `Row1` exactly as the
  Access machete's JS does. **This is the one part of the pass that can be unit-tested.**
- **`ReaderHostPreview`** (backup, option A): `Process.Start` the PDF, poll `EnumWindows` for a
  top-level Adobe window (class contains `Acrobat`) whose title contains the file's base name,
  bounded by an 8 s timeout, then `SetParent` it into a host panel with `WS_CAPTION`/
  `WS_THICKFRAME`/`WS_POPUP` cleared and `WS_CHILD` set, re-laid on resize. On dispose and on
  every document change it restores the original parent/style and closes the process it started,
  so no orphaned Reader is left. If the window is not found in time it falls back to a plain
  `Process.Start` and says so. `GetWindowLongPtr`/`SetWindowLongPtr` are dispatched 32/64-bit-safe.
- **`DdfView`** mounts `DdfPreviewFactory.Create()` into `pnlPreview`, subscribes
  `GenerateRequested`, cascades the theme to the surface, and `Clear()`s the preview on a root
  click / on context clear.

### The DDF element set was derived from the module source, not the repo sample

**Finding that contradicts the plan.** §0/§8 bis/DoD all assume a known-good DDF `.xml` exists
in the repo (`Surse/SURSA_XFA_WRITTER/XSD_XML/ddf_demo.xml`) to render/validate against.
**`ddf_demo.xml` and `ord_demo.xml` are byte-identical (same MD5 `9e5fe957…`)** — the file named
`ddf_demo.xml` is a copy of the ORD sample, not a real DDF. So the DDF element set
(`SubformAntet` → `DenInstPb`/`cif`/`NrUnicInreg`/`SubtitluDF`/`DataRevizuirii`/`Revizuirea`;
`SubformSectiuneaA` → `Subform123` + `Subform4/Table1/Row1` with `Cell1`=ElementFund,
`Cell3`=SS+clsf, `Cell5`=ValPrec, `Cell6`=ValCur) was read out of
`mdl_FX_DDF_PDF.GenereazaXML_PentruPython`, which is the authoritative source and what pass 05
will port. **This must be recorded as an open thread: there is no genuine DDF sample in the repo
to diff pass 05's output against** — DoD's "structurally matches a known-good Access-written
`.xml`" cannot be met from the repo as it stands; a real one has to be captured from a live run.

## Files touched

- `src/KBot.App/Views/Ddf/IDdfPreview.vb` — contract, `DdfPreviewMode`, `DdfPreviewFactory`.
- `src/KBot.App/Views/Ddf/DdfXfaModel.vb` — `DdfXfaModel`, `DdfXfaLinie`, `DdfXfaParser`.
- `src/KBot.App/Views/Ddf/XfaXmlPreview.vb` + `.Designer.vb` — the default surface.
- `src/KBot.App/Views/Ddf/ReaderHostPreview.vb` + `.Designer.vb` — the backup surface.
- `src/KBot.App/Views/DdfView.vb` — mounts the preview, `GenerateRequested`, theme cascade,
  `Clear()` on root/clear.
- `tests/KBot.App.Tests/DdfXfaParserTests.vb` — **new**, 9 pure-logic tests.
- `tests/KBot.App.Tests/XfaXmlPreviewTests.vb` — **new**, 7 headless STA state-machine tests
  (incl. the real render-from-sibling-`.xml` path via a temp file).

## Test results

- **.NET** — `dotnet build KBot.sln`: **0 errors, 0 BC warnings** (only pre-existing NU1701).
  `dotnet test KBot.sln`: **371 passed, 0 failed** (App 85, Api 61, Controls 134, Theming 27,
  Xfa 39, Domain 17, Common 7, LocalStore 1).
- A test caught its own bug: `btn.PerformClick()` needs the button `CanSelect` (visible+enabled),
  so the test now switches the preview to the missing-document state first. Product code fine.

## Anything left unverified or deferred

1. **NO VISUAL VERDICT — and for `ReaderHostPreview`, none is possible here.** The Win32
   window-hosting path (`EnumWindows` match, `SetParent`, style-stripping, resize, detach) has
   **never run** and cannot be exercised headless or without Adobe installed. It is written
   defensively against every failure the plan lists, but "written carefully" is not "seen
   working". This is the single least-verified piece of the whole slice.
2. **No genuine DDF sample in the repo** (see above). `XfaXmlPreview` was tested against a
   `form1` XML built by hand from the module source, not against a real Access-written file.
   The DoD item "structurally matches a known-good Access `.xml`" is **unmeetable from the repo**
   — logged as an open thread; needs a captured live artifact.
3. **Cross-linking is pass 04.** Selecting a leaf does **not** yet call `ShowDocument` — that
   needs `KBotPaths.DdfPdfRoot` + `CUAL`/`PartAng` to compute the §2.5 path, which pass 04 adds.
   Pass 03 only `Clear()`s the preview on a root click. The preview is therefore inert until 04.
4. **`GenerateRequested` is stubbed.** `DdfView.OnGenerateRequested` logs a `NotImplementedException`
   to `GlobalErrorLog`; pass 05 replaces the body with the real `XfaWriter.Genereaza` call on a
   background thread.
5. **`ReaderHostPreview` keyboard/focus across the process boundary** is documented as
   not-native and deliberately not fixed (§6).
6. **`XfaXmlPreview` header/nota styling** is functional but unseen; the `tblHeader` runtime
   layout has no visual verdict.

## Revizuire operator (2026-07-24) — a fourth sub-nav page: «Document» (PDF real)

Two operator requests on the DDF view:

1. **Selecting a file in «Fișiere» no longer reverts to «Vizualizare».** That page is the XFA-XML
   *reconstruction*, not the chosen file — jumping there on a file click was confusing.
2. **A dedicated tab shows the actual PDF.** Added a fourth sub-nav page **«Document»** (order:
   Valori · Vizualizare · Document · Fișiere) hosting a **`ReaderHostPreview`** instance
   (`_pdfPreview`) — always the real-PDF surface, independent of the compile-time
   `DdfPreviewFactory` (which still drives «Vizualizare» = `XfaXmlPreview`). So both preview
   surfaces now coexist as separate tabs, neither dead code.

Flow: `OnFileActivated` routes the real PDF to «Document» and switches to it (not «Vizualizare»);
a leaf click in the tree sets the «Document» target too. **Lazy by design** — `ShowPdfSurface`
mounts the PDF (and possibly launches Adobe) **only when «Document» is the visible page**, guarded
by `_pdfShownPath` so switching pages doesn't re-embed the same file. `SetPdfTarget`/`ClearPdfTarget`
carry the current path + on-disk existence; generation forces a re-mount (existence changed).
`ClearAll`, root-node clicks and the theme cascade all reach `_pdfPreview`.

Tests: `View_MountsFileBrowser_AndBothPreviews` (now also asserts a `ReaderHostPreview` is mounted)
and `FileActivated_RoutesToDocumentPage_NotVizualizare` (was `…RoutesToPreview_AndSwitchesToVizualizarePage`).
A missing file shows `ReaderHostPreview`'s "document lipsă" state without launching Adobe, so the
STA tests stay headless-safe. Full .NET suite 436 green.

⚠️ **Still no visual/PDF verdict** — the `ReaderHostPreview` Win32 hosting path (`EnumWindows`/
`SetParent`/style-cleanup) remains the least-verified code in the whole feature and has never run;
if Adobe is not the default `.pdf` handler it falls back to opening the file externally.
