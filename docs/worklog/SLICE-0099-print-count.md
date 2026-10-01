# SLICE-0099 — PrintCount: how many times a document was printed (operator request, 01.10.2026)

The operator asked: "all tables with PDFs will get a new column `PrintCount` int default 0 which
will be incremented when a user sends to the printer the document."

Four things were decided by the operator before any code (asked, answered the same day):

1. **How K-BOT knows about a print:** it watches the Windows print queue, and also notices
   Adobe's own Print window. "Microsoft Print to PDF" counts as a print.
2. **Which tables:** `FX_DDF_PDF`, `FX_ORD_PDF`, `FX_NoteCAB_PDF`, `FX_NoteCAB_Recipisa`.
   The attachment tables (`FX_DDF_REV_ATT_IMG`, `FX_ORD_ATT_IMG`) are left alone.
3. **Unsigned DDF / ORD:** they have no PDF row (only signed PDFs are stored), so their prints
   are counted on the document row: `FX_DDF_REV.PrintCount`, `FX_ORD.PrintCount`.
4. **Slice number:** 0099.

Later the same day: "the print wiring must also be connected in the harness form for pdf signing
and loading, so I can test it." Done (see «The bench»).

No tests and no git, as always.

## What changed and why

### 1. The column (`sql/0099_print_count.sql`)
- `PrintCount int(11) NOT NULL DEFAULT 0` on six tables: the four PDF tables and the two
  document tables. `ADD COLUMN IF NOT EXISTS`, so the file can be run twice.
- Total prints of a DDF / ORD = the count on its PDF row + the count on its document row.
- A re-signed PDF replaces the bytes of the same row (the upsert in `pdf.py` does not name the
  column), so the count is **kept** across signatures.
- Every `INSERT` into these six tables in `PYTHON/routes` names its columns (checked), so the
  new column breaks none of them. The DDF and ORD editors `UPDATE` an existing document, they do
  not delete and re-insert it, so the count on the document row survives an edit.

### 2. The server (`PYTHON/routes/forexe/print_count.py`, new)
- Four routes, all `POST`, no body:
  - `/api/forexe/ddf/pdf/<idrev>/print`
  - `/api/forexe/ord/pdf/<idordp>/print`
  - `/api/forexe/nc/pdf/<idnc>/print`
  - `/api/forexe/note-cab/recipisa/<idrcp>/print`
- **The server decides which row takes the count**, in one transaction: the PDF row when there
  is one; otherwise, for a DDF / ORD, the document row. The client never chooses.
- A CAB note with no stored PDF has no row for the count (`FX_NoteCAB` was not given the
  column): the answer is 200 with `numarat = false`, and a log line.
- Answers: `{"numarat", "tinta", "print_count"}` (ASCII keys); 404 when the document does not
  exist; **409 naming the SQL file** when the column is not there yet. Nothing is written then.
- The table / key / parent names are imported from `pdf.py` (one description, not two).
- Registered in `routes/forexe/__init__.py`, after `pdf`.

### 3. Noticing a print (`KBot.Controls/Adobe`, three new files)
K-BOT has no print command. The operator prints from inside Adobe.

- **`PrintSpooler.vb`** — plain `winspool.drv` calls, no new package:
  - lists the printers of the computer (its own and the connected network ones);
  - lists the jobs waiting in each queue;
  - asks Windows to signal when a job is added to a local queue.
- **`AdobePrintWatcher.vb`** — one watch for the whole process:
  - each viewer registers the file it shows; one background thread reads the queues for all;
  - the thread exists only while a document is shown, and never runs on the UI thread;
  - a **new** job whose name carries the file's name is one print; each job is counted once;
  - jobs already in a queue when watching starts are not counted; jobs of another Windows user
    are not counted;
  - a file stays watched for 15 s after its viewer lets it go, so a late job is still counted
    against the right document;
  - the queues are read at once when Windows announces a job, every 250 ms for 60 s after
    Adobe's Print window was seen, otherwise every 2 s. A slow pass stretches both intervals.
- **`AdobePrintJobFilter.vb`** — the pure decisions (job name ↔ file, same user, Print-window
  title), kept apart so they can be unit-tested later.
- **`AdobeSaveTrap.vb`** (changed, four lines): the trap already looks at every Adobe dialog on
  each sweep. A dialog whose title is a Print window's is now reported to the watcher. The
  dialog itself is left alone, as before.

**About "watch Adobe's messages".** The operator also asked for a watcher on the Adobe window's
print messages and on the Print window's messages. Reading another program's window messages
needs code loaded inside that program; K-BOT never does that to Adobe (and Adobe's protected mode
would refuse it). What K-BOT can see from outside is the Print window appearing and going away.
That is what was wired. It cannot tell «Print» from «Cancel» — the queue does: a cancelled
dialog leaves no job.

### 4. The client call (`KBot.Api`)
- `IPrintCountApi` + `ApiClient.PrintCount.vb`: `RecordPrintAsync(kind, id)`.
- Kept out of `IApiClient`, like `ICabNotesApi`, so the test doubles of `IApiClient` still
  compile untouched.
- It is a write: while the FOREXE robot runs it waits at the server gate (slice 0098) and goes
  out when the robot is done.

### 5. The wiring (`KBot.App`)
- **`PdfPrintTarget`** (new): which server document the file on screen is (kind + id). It makes
  the call and writes the outcome to `adobe_preview.log`.
- The three views build it next to the signing session and put it in the page context:
  `DdfView` (IDREV), `OrdView` (IDORDP), `NoteCabView` (IDNC, and IDRCP for the receipt).
- The four pages hand it to the preview before the document is shown: `DdfDocumentPage`,
  `OrdDocumentPage`, `CabNoteDocumentPage`, `CabNoteReceiptPage`.
- **`ReaderHostPreview`** registers every document it shows with the watcher, and lets it go when
  the document changes or the pane is cleared. New event `DocumentPrinted`.
- A file picked from the DDF «Fișiere» list belongs to no revision: its print is logged, not
  counted (same rule as the signing session).
- **Nothing is shown to the operator.** A counter must not interrupt printing. Every decision
  is in `adobe_preview.log`; a failed call is also in the error log.

### 6. The bench (`PdfSigningHarnessForm`, Debug only)
- Every document opened on the signing bench is watched like on the real pages.
- The journal shows the «[Adobe] Tipărire…» lines live (Print window seen, job found, jobs not
  taken) and a «TIPĂRIRE detectată» line of the bench.
- With an id and a login — and not in «Doar simulează încărcarea» — the print is also sent to
  the server and counted on the **real** row of that id. The bench says so when the document is
  opened. Without them the print is only detected.

## Files touched
- **New:**
  - `sql/0099_print_count.sql`
  - `PYTHON/routes/forexe/print_count.py`
  - `src/KBot.Api/IPrintCountApi.vb`
  - `src/KBot.Api/ApiClient.PrintCount.vb`
  - `src/KBot.Controls/Adobe/PrintSpooler.vb`
  - `src/KBot.Controls/Adobe/AdobePrintWatcher.vb`
  - `src/KBot.Controls/Adobe/AdobePrintJobFilter.vb`
  - `src/KBot.App/Views/PdfPrintTarget.vb`
- **Changed:**
  - `PYTHON/routes/forexe/__init__.py`
  - `src/KBot.Controls/Adobe/AdobeSaveTrap.vb`
  - `src/KBot.App/Views/Ddf/ReaderHostPreview.vb`
  - `src/KBot.App/Views/Ddf/DdfPageContext.vb`, `Ddf/DdfDocumentPage.vb`
  - `src/KBot.App/Views/Ord/OrdPageContext.vb`, `Ord/OrdDocumentPage.vb`
  - `src/KBot.App/Views/NoteCab/ICabNotePage.vb`, `NoteCab/CabNoteDocumentPage.vb`,
    `NoteCab/CabNoteReceiptPage.vb`
  - `src/KBot.App/Views/DdfView.vb`, `OrdView.vb`, `NoteCabView.vb`
  - `src/KBot.App/HarnessTests/PdfSigningHarnessForm.vb`
  - `src/KBot.Controls/KBot.Controls.vbproj` (FileVersion 1.59.0.0),
    `src/KBot.Api/KBot.Api.vbproj` (FileVersion 1.0.17.0)

## Test results
- `dotnet build src/KBot.App/KBot.App.vbproj`, Debug and Release: **0 warnings, 0 errors**.
- `print_count.py` and `__init__.py` parse (syntax only, with the venv's Python).
- **Nothing was run.** No print was made, no route was called, the SQL was not applied.
- No test code was written. No test project was built.

## Help
No help change: nothing the operator sees was added or changed. The bench is Debug only.

## Left unverified or deferred
- **The name Adobe gives a print job is assumed** to be the file name. If it is something else
  (for example the document's title), no print is counted. The log then shows the job with «nu
  poartă numele unui document deschis în K-BOT» — that line is the thing to read first.
- **The title of Adobe's Print window is assumed** («Print», «Imprimare», «Tipărire»…) and its
  window class is assumed to be the standard dialog one. If either is wrong, only the 250 ms
  reading is lost; counting still works through the queue. The existing line «Dialog Adobe alt
  dialog Adobe (lăsat în pace) … titlu=«…»» shows the real title.
- **«Microsoft Print to PDF» and the Save As trap — a risk, not measured.** That printer asks
  where to save its output with a standard file window. If that window belongs to the Adobe
  process, the trap of slice 0078 will treat it as Adobe's «Save As»: write the document's own
  path into it and press Save. The trap was **not changed** here (it guards the signing). On the
  bench this shows as «Dialog «Salvare ca» prins» right after pressing Print.
- **Network printers:** Windows announces new jobs only for the queues of this computer. A job
  on a connected network printer is found by the periodic reading (2 s, or 250 ms after the
  Print window). A job that lives less than that on such a printer, with the Print window not
  recognised, is missed.
- **Session expired:** the call goes straight to the client, with no re-login. A print made after
  the session expired is logged as not counted.
- **A CAB note with no stored PDF** is not counted (no row to hold it).
- **Prints made outside K-BOT** (the same file opened in the operator's own Adobe) are counted
  only if K-BOT shows that file at the same moment — the job has the same name.
- **`PrintCount` is not shown anywhere** in K-BOT, and no list route returns it. Not asked.
- **Deploy order:** the SQL on every unit database and on `AVACONT_SURSA` first (in
  `AVACONT_SURSA` the dump of 22.09.2026 has no `FX_NoteCAB_Recipisa`: run `sql/0088_04` there
  first), then `print_count.py` + `__init__.py` on the VPS, then the client.
- **`sql/AVACONT_SURSA.sql`** (the hand-written, drifted copy) was not updated.
- **`MariaDB_Schema/`** will not show the column until the operator refreshes the dump.

---

# Part 2 — the print list (ORD and DDF), same slice, 01.10.2026

Operator request (kept as «still slice 0099»): when a node of the ORD / DDF tree that is NOT a leaf
(a month, or the «Toate…» root) is selected, the right side shows a grid with one row per revision /
ordonantare instead of the document pages: a tick column whose header icon opens a popup
(select / deselect all; select / deselect only the unlisted), then the signatures, when it was
signed, «Listat» (a tick, true when printed at least once) and the number of prints. The footer has
**«Generează și imprimă»** (the ticked documents are made and sent to the printer without being
opened) and **«Salvează local»** (the ticked documents copied to a folder the operator picks).
Ticking «Listat» on a row whose count is 0 asks once whether to mark it as printed; nothing is said
afterwards except on error.

## What changed and why

- **Server — the list routes carry the count.** `GET /api/forexe/ddf` and `GET /api/forexe/ord` return
  `print_count` per revision / ordonantare = the count on the PDF row + the count on the document row
  (the same total rule as part 1). New `routes/forexe/print_count_column.py` probes the columns once
  per database (a True answer is remembered) and gives the SQL fragment, which is the literal `0`
  where `sql/0099_print_count.sql` has not run — the lists keep working before the DDL. The new value
  is the LAST column of the DDF revision SELECT and sits before the optional `CalePDF` of the ORD one,
  so the unpacking stays positional and fixed.
- **Client data.** `GetDdfRevizieRow` / `GetOrdHeaderRow` get `print_count`; `RevizieRow` /
  `OrdHeaderRow` get `PrintCount`; `ApiClient` maps it. «Semnat la» is the existing `PdfDataModif`
  (last write of the signed PDF), shown only when a signed PDF exists; the signatures are the existing
  `Semnatura` roles.
- **`PrintListPage` (new, `KBot.App/Views/Print/`)** — one UserControl for both views, declared in its
  `.Designer.vb`; it knows nothing about DDF or ORD, only `PrintListItem` rows (label, signatures,
  signed-at, count, a delegate that puts the PDF on disk, a callback that tells the view's own row the
  new count). The tick column's header icon opens a `CustomPopup` with the two choices («only unlisted»
  leaves the listed rows as they were). «Listat» is locked (CellFormatting) once the count is above 0.
  Marking by hand and printing both go through `IPrintCountApi.RecordPrintAsync` (part 1); the page
  raises the shown total by one itself, because the server answers with the count of the ROW it raised
  (the PDF row or the document row), not the document's total.
- **Printing without opening (`PdfPrinter`).** One printer choice (`PrintDialog`, remembered for the
  run) per click; each document is sent with the shell's `printto` verb (hidden window), waiting for
  the handler to finish (2 min limit). Copies asked in the dialog are done by repeating the sending;
  the count goes up by 1 per document. A document that printed but could not be counted is reported
  as such. Failures are collected and said ONCE at the end; success is silent (status line only).
- **Getting the PDF (`DdfView` / `OrdView.ObtainPrintPdfAsync`).** Signed → the local cache checked
  against the server (`PdfCache`, same as a click on the leaf; an error is thrown with its Romanian
  text). Unsigned → generated into `TempPdf` exactly like «Generează» — DDF through the existing
  `DdfPdfGenerator` (the generation data is read ONCE per batch, `_genData`), ORD through the new
  `OrdPdfGenerator` (the steps of `OrdView.OnGenerateRequested`, which was left as it is).
- **The views.** `printList` sits in `split.Panel2` next to `navSub` / `pnlPages` (both Designers).
  `PushToActivePage` decides the surface: root node → list (pages get the empty context FIRST while
  still visible, so a document open in Adobe is cleared, then they are hidden); leaf → pages shown
  again BEFORE they get the context. New fields `_nodeRevizii` / `_nodeOrdonantari` (what the node
  covers) set wherever the node changes.
- **Help.** `contabil.vederi.ord` and `contabil.ddf` get the «Lista de tipărire» section, tagged 0099
  (same slice, no new 0000-NN number: the operator said this stays slice 0099; the help edit is part
  of this task). `PrintListPage` is deliberately NOT in a `screens:` line: F1 inside it walks up to
  `OrdView` / `DdfView`, which give the right topic for each (the checker lists it under
  «without a topic» — informational).

## Files touched (part 2)
- **New:** `PYTHON/routes/forexe/print_count_column.py`; `src/KBot.App/Views/Print/PrintListItem.vb`,
  `PdfPrinter.vb`, `PrintListPage.vb`, `PrintListPage.Designer.vb`; `src/KBot.App/Views/Ord/OrdPdfGenerator.vb`.
- **Changed:** `PYTHON/routes/forexe/ddf.py`, `ord.py`; `src/KBot.Api/UpsertAngajamenteRequest.vb`,
  `ApiClient.vb`; `src/KBot.Domain/DdfInfo.vb`, `OrdInfo.vb` (+ `KBot.Domain.vbproj` FileVersion 1.2.9.0);
  `src/KBot.App/Views/DdfView.vb`, `DdfView.Designer.vb`, `OrdView.vb`, `OrdView.Designer.vb`;
  `src/KBot.App/HelpContent/contabil/vederi/ord.md`, `contabil/ddf/index.md`.

## Test results (part 2)
- `dotnet build src/KBot.App/KBot.App.vbproj`, Debug and Release: **0 warnings, 0 errors**.
- `ddf.py`, `ord.py`, `print_count_column.py` compile (`py_compile`, venv Python).
- `Check-Help.ps1 -Coverage`: no error about the new text; the one error it prints
  (`contabil\fereastra.md: slice '0077-3' …`) was already there and is not from this work.
- **Nothing was run**: no window opened, no print made, no route called, the SQL not applied. No test
  code written.

## Left unverified or deferred (part 2)
- **Nothing seen on screen.** The new Designer was written by hand and never opened in Visual Studio;
  layout, the header icon, the disabled «Listat» look, the popup position are unchecked.
- **`printto`** needs a PDF program that registers it (Adobe Reader does). Which printer / how the
  hidden window behaves, whether Adobe closes by itself, was not measured. «Microsoft Print to PDF»
  asks for a file name: if that window belongs to the Adobe process the Save-As trap of slice 0078 may
  react to it (part 1 risk, not changed).
- **The count is raised when the PDF program finishes**, not when paper comes out; a job stuck in the
  queue is counted.
- **The 401 net:** the print call and the generation of a signed document go straight to the client
  (no re-login), except the DDF generation data which uses the view's net. A session that expired
  during a long batch gives per-document errors.
- **A signed DDF revision sent to FOREXE and signed on A only** is printed as stored on the server
  (the section-B insert of 0078-06 is a screen-time step, not done here).
- **«Semnat la»** = the last write of the signed PDF (`DataModif`), so a re-signature moves it; there
  is no per-signature date in the data.
- The two ORD document-page code paths that generate (`OrdView.OnGenerateRequested` and
  `OrdPdfGenerator`) are duplicates of the same steps; moving the first onto the second was left alone
  to avoid touching a working path.
- `NoteCabView` has no non-leaf node list (not asked).

## Part 2, addendum — captures and tour steps (01.10.2026)

Operator: put the print list in the helper of the DDF and ORD views, with captures of the view and of the documents.

- **Tour steps** (`tours/tur-ord.md`, `tours/tur-ddf.md`), four each, before «Paginile…»: «Lista de tipărire» (target `OrdView.printList` / `DdfView.printList`), «Lista › Bifele» (`PrintListPage.grila`, part `header`), «Lista › Generează și imprimă» (`PrintListPage.btnImprima`), «Lista › Salvează local» (`PrintListPage.btnSalveaza`). The first step says what makes the list appear (a month or the «Toate…» root, no leaf chosen). Tagged 0099.
- **New capture tags (6 + 2 = 8 ids), all to be shot by the operator — no picture exists yet, the topics show «Imagine lipsă» until then:**
  - `ord-lista-tiparire`, `ord-lista-meniu` (the header popup held open), `ord-document` (Document page of a signed ordonanțare) — in `contabil.vederi.ord`;
  - `ddf-lista-tiparire`, `ddf-lista-meniu`, `ddf-document` (Document PDF page of a signed revision) — in `contabil.ddf`.
  - For the two list shots the angajament should have a mix of signed / printed and unsigned / unprinted rows, so the columns say something.
- `Check-Help.ps1 -Coverage`: 64 capture tags, tour targets and parts accepted; only the old `0077-3` error remains.
- **Unverified:** the steps target `PrintListPage.*`, a type used by both views; the runner should pick the instance that is on screen. Not run. The «Lista › …» steps on a leaf selection fall under the «hidden by state» rule; not seen.
