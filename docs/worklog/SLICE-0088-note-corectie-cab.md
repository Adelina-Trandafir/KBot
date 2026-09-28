# SLICE 0088 — «Nota contabila corectie CAB» (F1135) for the ERRRRRRRRRR operations

**Date:** 28.09.2026. **Request (operator):** the «Operațiuni necorectate» FOREXE shows after the
login carry «ERRRRRRRRRR»: FOREXE could not attach those receipts to an angajament. The operator
attaches each one to an existing angajament + indicator; K-BOT makes the F1135 note PDF, stores
it in MariaDB and, on «Vrei să încarci NOTA DE CORECȚIE în CAB?», uploads it through
https://forexe.mfinante.gov.ro/transmitere-documente-electronice (`linkdoc` + «Trimite»).
Plus a «Note corecție» view on KbotForm (tree + grid + «Document» page: generate unless signed,
download when signed).

**Operator corrections during the slice (same day):**
1. no message box when new operations are found — the window opens straight away;
2. the operator fills EVERY row (angajament + indicator); a complete row gets a tick; «Salvează
   tot» is enabled only when all rows are ticked and saves ALL of them (one note);
3. «Ieșire» / closing saves nothing more — the rows are already in `FX_Operatiuni`;
4. menu entry «Operațiuni necorelate» reopens the window; hidden when there is none; when there
   are, the entry and the «Meniu» button carry «(!)»;
5. rows already in `FX_Operatiuni` are neither stored again nor shown again after a login.

## What changed

### Database — `sql/0088_fx_note_cab.sql` (the operator runs it)
- `FX_Operatiuni`: + `CodAngajament varchar(11)` (the «Angajament» column of the FOREXE table),
  `Suma` int → decimal(15,2) (0084 left it int), index on (ReferintaTrezor, NrDoc).
- `FX_NoteCAB` (header: NrNota unique per An, DataNota, DenumireEP, CifEP, Semnatura, Trimis…).
- `FX_NoteCAB_Corectii` (one row per corrected operation; UNIQUE (ReferintaTrezor, NrDoc); FK to
  FX_NoteCAB, FX_Angajamente, FX_Operatiuni).
- `FX_NoteCAB_PDF` (same shape as FX_ORD_PDF).
- `FX_PDF_SEMNATURI`: + `IDNC`, check becomes «exactly one of IDREV / IDORDP / IDNC».

### Server (PYTHON)
- `routes/forexe/operatiuni.py`: stores `angajament`; answers `noi` = IDFXP of the rows THIS call
  inserted; new `GET /api/forexe/operatiuni/necorelate` (ERR or NULL `CodAngajament`, not in
  `FX_NoteCAB_Corectii`; Clasificatii joined on its primary key).
- `routes/forexe/note_cab.py` (new): `GET …/note-cab/pregatire` (next number + angajamente with
  indicators and their SS programs), `POST …/note-cab` (header + ALL corrections, one
  transaction, F1135 rules re-checked), `GET …/note-cab?cod=` (notes with a correction on that
  angajament, with all their rows), `POST …/note-cab/<idnc>/trimitere`.
- `routes/forexe/pdf.py`: third family `_NC` (FX_NoteCAB_PDF, roles S1/S2),
  `GET|PUT /api/forexe/nc/pdf/<idnc>`, file name `NOTA_CAB_{nr}_{an}.PDF`.
- `routes/forexe/__init__.py`: registers `note_cab`.

### PDF — `KBot.Xfa`
- `Templates/F1135.pdf` (embedded resource): the FIRST revision (bytes 0..97098) of the sample
  «NOTA CAB 23.pdf» = the MF form as published (no data, no signature, Reader usage rights UR3).
  There is no public download address for F1135 (the DDF/ORD templates come from the MF guides
  page; this machine had no internet to check further).
- `CabNotePdf.Build`: fills the XFA datasets and attaches `f1135.xml` (described «text/xml»), in
  APPEND mode — what Adobe does on «VALIDARE SI GENERARE XML».
- `AdobeUtils.ClassifySigner` / `PdfSignatureInfo.RoleName(s)`: doc type «NC»,
  SignatureField1 → S1, SignatureField2 → S2.

### Domain — `KBot.Domain/CabCorrectionNotes.vb`
- `CabCorrectionNote` (header + `Corrections`), `CabNoteCorrection`, `CabNoteRow`, the
  preparation POCOs, and `CabCorrectionNoteRules`: account symbol («02A» + «650401200103» →
  «24A650401200103»), control sum (CIF + yyyyMMdd + rows, each digit +2 mod 10), the F1135 field
  rules, `FormDataXml` (datasets) and `ExportXml` (f1135.xml). Row order per operation: storno
  (ERRRRRRRRRR, reference + date, negative) then correction (angajament + indicator, positive).
- `UncorrectedOperation`: + `IdFxp`, `Commitment`, `CommitmentIndicator` (the page's
  «Angajament» / «Indicator ang» columns — the 0084 script already returned them).

### API — `KBot.Api`
- `ICabNotesApi` + `ApiClient.CabNotes.vb`; `IUncorrectedOperationsApi` gets
  `GetUncorrelatedOperationsAsync` and `UncorrectedOperationsSaveResult.NewIds`. Kept off
  `IApiClient` so the test doubles are untouched.

### FOREXE robot — `KBot.Forexe`
- `WorkflowExecutor.DocumentUpload.vb`: goes to «Transmitere documente electronice», finds
  `input[type=file][name=linkdoc]` in ANY frame (the form lives in the `…/WAS6DUS/` iframe),
  sets the file, clicks `input[type=submit][value='Trimite']`, waits for the POST navigation +
  load, returns the frame's text (FOREXE's answer), goes back to the previous page.
- `IForexeDocumentUpload` (global, implemented by `ForexeRunner`) — not on `IForexeRunner`, so
  the test fakes are untouched. `ForexeController.TrimiteDocumentAsync`.

### App — `KBot.App`
- `KbotForm.UncorrectedOperations.vb`: no message box; saves silently (skipped rows → operator
  log; only a failed save is shown), then opens the window on the rows the save just INSERTED.
- `KbotForm.CabNotes.vb`: `ShowUncorrelatedAsync` (login: new ids only; menu: all),
  `RefreshUncorrelatedMarkAsync` (at load and after the window), `UploadCabNoteAsync` (the
  question, the upload, `…/trimitere`, FOREXE's answer; warns when the PDF is unsigned).
- `Views/NoteCab/CabNoteForm` (designer + code): grid with a tick column; the bottom part edits
  the selected row's draft; «Salvează tot» only when every row is complete → one note, its PDF
  (stored unsigned, `X-Semnatura: -`), then the upload question. «Ieșire» saves nothing.
- `Views/NoteCabView` + `NoteCab/CabNoteVizualizarePage`, `CabNoteDocumentPage`, `ICabNotePage`,
  `CabNoteFiles`: month → note tree; «Document»: signed → download (PdfCache, sha-checked),
  unsigned → generate; a `PdfSigningSession` (`PdfDocKind.Nc`) uploads a signature at once.
  Right click on a note: «Încarcă în CAB», «Generează din nou documentul» (unsigned only).
- `PendingPdfUploads` / `PdfSigningSession`: family-agnostic (`DocType`, `UploadToServerAsync`).
- `KbotForm.Designer.vb`: nav entry «Note corecție» (`notecab`, shown for any angajament),
  menu entry `operatiuni_necorelate` (hidden by default). `KbotForm.Views.vb`: factory + gating.

Versions: Domain 1.2.7, Api 1.0.13, Xfa 1.4.0, Forexe 1.0.17. KBot.App left to `push-update.ps1`.

## Files touched
`sql/0088_fx_note_cab.sql` (new); `PYTHON/routes/forexe/{note_cab.py (new), operatiuni.py, pdf.py,
__init__.py}`; `src/KBot.Domain/{CabCorrectionNotes.vb (new), UncorrectedOperations.vb,
KBot.Domain.vbproj}`; `src/KBot.Api/{ICabNotesApi.vb (new), ApiClient.CabNotes.vb (new),
IUncorrectedOperationsApi.vb, ApiClient.UncorrectedOperations.vb, KBot.Api.vbproj}`;
`src/KBot.Xfa/{CabNotePdf.vb (new), Templates/F1135.pdf (new), AdobeUtils.vb, PdfSignatures.vb,
KBot.Xfa.vbproj}`; `src/KBot.Forexe/{IForexeDocumentUpload.vb (new),
Executor/WorkflowExecutor.DocumentUpload.vb (new), ForexeRunner.vb, KBot.Forexe.vbproj}`;
`src/KBot.App/{KbotForm.CabNotes.vb (new), KbotForm.UncorrectedOperations.vb, KbotForm.vb,
KbotForm.Views.vb, KbotForm.Designer.vb, KbotForm.Nomenclatoare.vb,
Forexe/ForexeController.DocumentUpload.vb (new), Views/NoteCabView{,.Designer}.vb (new),
Views/NoteCab/* (new), Views/PendingPdfUploads.vb, Views/PdfSigningSession.vb}`.

## Test results
- `dotnet build` of KBot.Domain, KBot.Xfa, KBot.Api, KBot.Forexe, KBot.App: **0 errors, 0 warnings**.
- `py_compile` of note_cab.py, operatiuni.py, pdf.py, __init__.py: green.
- No tests written or run (operator's rule). One early scratch run (before the operator stopped
  it) generated note 23 from the sample's data: its `f1135.xml` came out byte-identical to the
  one inside «NOTA CAB 23.pdf» and the control sum 61647952 matched; that was the one-note/one-
  operation version, the multi-operation shape was not run.

## 0088-02 (same day) — signed note refused: `CK_FX_PDF_SEMNATURI_DOC` failed (014_SCSV)
- Cause: on 014_SCSV `FX_PDF_SEMNATURI` had the `IDNC` column but still the 0079 check
  («IDREV or IDORDP»), so every signature row with IDNC broke the check; the log shares the PDF's
  transaction, so the PDF was refused too, and the pending upload failed again at every start.
- `sql/0088_02_fx_pdf_semnaturi_check.sql` (new): shows the check, then puts IDNC + the
  three-way check + the FK in place, one statement at a time.
- `pdf.py`: `_sign_log_accepts` — signature rows are written only when the table has the family's
  column AND the check names it; otherwise the PDF is saved and the rows are skipped (logged).

## 0088-03 (same day) — FX_NoteCAB left in the first draft shape
- The AVACONT_SURSA dump (28.09 14:18) shows `FX_NoteCAB` as in the FIRST draft of sql/0088 (the
  operation inside the note, UQ on ReferintaTrezor+NrDoc) and no `FX_NoteCAB_Corectii`: the
  final script's `CREATE TABLE IF NOT EXISTS` skipped the existing table. `note_cab.py` needs the
  final shape.
- `sql/0088_03_fx_notecab_split.sql` (new): step 1 says whether a database is in the draft shape;
  if so, creates `FX_NoteCAB_Corectii`, copies every stored note into it as one correction, then
  strips the operation columns, keys and checks off `FX_NoteCAB`.
- Rest of the dump matches the final DDL: FX_Operatiuni (CodAngajament, decimal Suma, index),
  FX_NoteCAB_PDF, FX_PDF_SEMNATURI (IDNC, three-way check, FK) — on AVACONT_SURSA 0088-02 is not needed.

## 0088-04 (same day) — pending copies question, upload answer window, FOREXE receipt
Operator: (a) «Documentul pentru care s-a trimis PDF-ul nu există» came back at every start;
(b) the answer to an upload goes in its own window with «Verifică recipisa»; (c) the receipt is
found in FOREXE's SNM inbox by the upload's index, downloaded and kept on the server, attached to
the note's PDF; (d) a «Recipisă» tab next to «Document»: the receipt itself when there is one (no
button), otherwise «Validează documentul», which opens the window of (b).
- `PendingPdfUploads.RetryAllAsync` → `PendingRetryResult` (lines + entries left);
  `SigningMessages.AskDeletePending`: when copies are left after the retry at start, one Yes/No
  question — Yes deletes them from `PdfDeIncarcat\`.
- `sql/0088_04_fx_notecab_recipisa.sql` (new): `FX_NoteCAB.IndexInregistrare`;
  `FX_NoteCAB_Recipisa` (IDPDF → FX_NoteCAB_PDF, UNIQUE IndexInregistrare, NumarInregistrare,
  IdMesaj, DescriereMesaj, DataMesaj, NumeFisier, Dimensiune, Sha256, Continut, UN).
- `note_cab.py`: «trimitere» takes `index`; GET note-cab returns `index_inregistrare` + the latest
  `recipisa`; `POST /api/forexe/note-cab/<idnc>/recipisa` (base64 JSON; 409 without a PDF);
  `GET /api/forexe/note-cab/recipisa/<idrcp>` (the file).
- Domain: `CabNoteReceipt`; note `RegistrationIndex` / `Receipt`;
  `CabCorrectionNoteRules.RegistrationIndexFrom` / `RegistrationNumberFrom` / `IsRegistrationIndex`.
- Api: `ICabNotesApi.MarkCabNoteSentAsync(+index)`, `SaveCabNoteReceiptAsync`, `DownloadCabNoteReceiptAsync`.
- Forexe: `ForexeSNM.CautaRecipisaAsync` — opens `viewAll.do?nomCategoryCode=1`, reads the grid's
  JSON (`loadAll.do?nomCategoryCode=1`, newest first, ≤ 5 pages of 200), matches
  «INTERNT-{index}-» in the message (or a field equal to the index), downloads
  `downloadFile.do?id=…&fileName=…&nomCategoryCode=1`, goes back to the previous page.
  `IForexeDocumentUpload.FindReceiptAsync` + `ForexeReceipt`; `ForexeController.CautaRecipisaAsync`.
- App: `CabNoteReceiptForm` (designer + code): headline (success / error colour), FOREXE's answer,
  editable index, «Verifică recipisa» (hidden once a receipt exists), «Deschide recipisa», «Închide».
  `KbotForm.UploadCabNoteAsync` shows it instead of message boxes; `OpenReceiptWindow`,
  `VerifyReceiptAsync` (robot → local copy `...\PDF\NC\Recipise\RECIPISA_{index}.pdf` → server).
  `CabNoteReceiptPage` + nav entry «Recipisă»; `NoteCabView.EnsureReceiptAsync` / `ReceiptSaved`;
  `ReaderHostPreview.SetMissingTexts` / `ShowNotice`.
- Build: Domain, Api, Forexe, App 0/0; py_compile note_cab.py green. Nothing run.
- Unverified: FOREXE's answer to «Trimite» was never seen — the index is read from
  «INTERNT-…» or «index … digits» in it, else the operator types it. The JSON fields of category 1
  are assumed to be the bank statements' (`mesajTab.id / numeFisier / descriere / dataCreare`).
  The receipt is assumed to be a PDF (shown in Adobe).

## 0088-05 (same day) — one note per number
Operator: two ERR rows with different numbers were saved under one number (the number was one field
for the whole window). Now: the number is per row; choosing an angajament already chosen on another
row brings that row's number, a new angajament gets the next number free on the server AND in the
window; one number = one note = one PDF. A number given to two angajamente, or already used on the
server this year, stops the save and NOTHING is stored.
- `note_cab.py`: `pregatire` also returns `numere_folosite`; new `POST /api/forexe/note-cab/lot`
  (all notes in ONE transaction; refuses a repeated number, a note on two angajamente, an operation
  in two notes, a number already used, an operation already corrected); `_verifica_in_baza` /
  `_insereaza_nota` shared with the single-note route.
- Domain `CabNotePreparation.UsedNumbers`; Api `SaveCabNotesAsync` (sets each IdNc).
- `CabNoteForm`: `_numberTexts` per row, `NumberFor`, `RowNumber`, `RowProblems`, `BuildNotes`;
  save → one call for all → one PDF per note → the CAB question per note. `SavedNotes` replaces
  `SavedNote`. Tooltips updated.
- Build App 0/0; py_compile green. Nothing run.

## Left unverified / deferred
- ⚠ **Run `sql/0088_fx_note_cab.sql` first** on every unit database + AVACONT_SURSA, then upload
  `note_cab.py`, `operatiuni.py`, `pdf.py`, `__init__.py`. The new `operatiuni.py` writes
  `CodAngajament` — without the DDL every login save fails (500).
- Nothing run against the server, FOREXE or Adobe; the window and the view never seen on screen.
- The generated PDF was never opened in Adobe: that Reader still allows signing after
  iTextSharp's append (UR3) is assumed from the DDF/ORD generator, which works the same way.
- The upload page's `iframe` content was not in the saved HTML; `linkdoc` / «Trimite» come from
  the operator's message. FOREXE's answer text after «Trimite» is unknown — it is shown and
  stored as the frame shows it.
- Account-symbol prefix: only sector 02 → «24» is known (from the sample note); any other sector
  leaves the field for the operator to type.
- Sample note: «Data operatiune initiala» 23.09 while the page's «Dată plată» for that operation
  was 24/09 — K-BOT proposes «Dată plată», editable.
- «Încasare» → «Suma credit» is from the sample; other kinds are put in «Suma debit» (unverified).
- The operator wrote «CodAngajament and NumarDocument» for the bottom part; implemented as
  angajament + indicator (the note number is one field for the whole note). To confirm.
- Only page 1 of the FOREXE table is read (0084 limit, unchanged).
- Signing is not forced before the upload: the question warns when the PDF is unsigned.
